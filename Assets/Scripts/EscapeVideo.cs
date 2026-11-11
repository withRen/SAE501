using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// Cinématique de fin (victoire en mode Évasion, portail du jardin franchi) : une vidéo rendue dans Blender
// (Tools/blender/cinematique_fin.py, branche feature/cinematiques) — le survivant s'enfuit par le portail et le claque,
// les zombies secouent la grille. Sa bande-son est refaite à partir des sons du jeu et d'un son de grille CC0
// (Tools/blender/cinematique_son.py). GameOverUI la lance après le fondu au noir, avant le bilan.
// Confort au casque : l'écran est fixe dans le monde (posé devant la tête au lancement), la tête bouge librement,
// aucune caméra n'est déplacée à la place du joueur. La caméra ne dessine plus que l'écran (calque Layer) : décor, mains
// et zombies disparaissent, il reste un écran de cinéma dans le noir.
// Version PC (DesktopMode) : même écran devant la caméra ; le HUD en surimpression (Canvas Screen Space - Overlay) est masqué
// pendant la vidéo. La musique du jeu est coupée en fondu (MusicPlayer.Silenced) puis revient pour le bilan.
// Une gâchette (ou clic gauche / espace au PC) passe la vidéo après SkipAfter secondes. Sans vidéo dans Resources, rien n'est joué.
// [Journal 177] Bilan par-dessus la vidéo : GameOverUI passe onMenu ; à la moitié de la vidéo (ou si le joueur passe), le bilan
// apparaît devant l'écran de cinéma, la vidéo continue derrière puis se fige juste avant son fondu au noir.
public static class EscapeVideo
{
    public const string ClipPath = "Cinematique/CinematiqueFin";   // Assets/Resources/Cinematique/CinematiqueFin.mp4
    public const int Layer = 30;            // calque réservé à l'écran (31 : incrustations de vidéosurveillance)
    const float Distance = 3.2f;            // m devant la tête
    const float Width = 4.2f;               // m : ≈ 66° de large, comme une place au milieu d'une salle de cinéma
    const float SkipAfter = 2f;

    public static bool Available => Resources.Load<VideoClip>(ClipPath) != null;
    public static bool Playing { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Playing = false;

    public const float MenuAt = 0.5f;       // fraction de la vidéo où le bilan apparaît par-dessus (plan des zombies contre la grille)
    const float HoldBeforeEnd = 1.8f;       // avec le bilan : image figée avant le fondu au noir final de la vidéo (s avant la fin)

    /// <summary>Joue la vidéo. Avec onMenu : appelé à MenuAt (ou quand le joueur passe) ; la vidéo continue derrière le bilan,
    /// puis reste figée sur une image avant son fondu final (son coupé en fondu), et l'écran reste jusqu'au changement de scène.
    /// Sans onMenu : la vidéo va au bout (ou est passée), puis tout est remis en place.</summary>
    public static IEnumerator Play(Transform head, System.Action onMenu = null)
    {
        var clip = Resources.Load<VideoClip>(ClipPath);
        if (!head && Camera.main) head = Camera.main.transform;
        var cam = head ? head.GetComponent<Camera>() : null;
        if (!cam) cam = Camera.main;
        if (clip == null || cam == null || head == null) yield break;

        // écran : posé devant la tête, à hauteur des yeux, tourné vers le joueur (seulement le cap : pas penché)
        var fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.ProjectOnPlane(head.up, Vector3.up);
        fwd.Normalize();
        var screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "EcranCinematique";
        Object.Destroy(screen.GetComponent<Collider>());
        screen.layer = Layer;
        screen.transform.SetPositionAndRotation(head.position + fwd * Distance, Quaternion.LookRotation(fwd));
        float aspect = clip.height > 0 ? clip.width / (float)clip.height : 16f / 9f;
        screen.transform.localScale = new Vector3(Width, Width / aspect, 1f);

        var rt = new RenderTexture((int)clip.width, (int)clip.height, 0);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetTexture("_BaseMap", rt);
        mat.SetColor("_BaseColor", Color.black);
        var mr = screen.GetComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var audio = screen.AddComponent<AudioSource>();
        audio.spatialBlend = 0f;            // son « de salle » : pas spatialisé
        audio.priority = 0;
        var vp = screen.AddComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.clip = clip;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.targetTexture = rt;
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.SetTargetAudioSource(0, audio);
        vp.isLooping = false;
        vp.Prepare();
        Playing = true;
        MusicPlayer.Silenced = true;
        for (float t = 0f; !vp.isPrepared && t < 5f; t += Time.unscaledDeltaTime) yield return null;

        // on ne voit plus que l'écran (et plus le HUD du PC)
        int mask = cam.cullingMask;
        var clear = cam.clearFlags; var bg = cam.backgroundColor;
        cam.cullingMask = 1 << Layer;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        var overlays = new System.Collections.Generic.List<Canvas>();
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (c.enabled && c.renderMode == RenderMode.ScreenSpaceOverlay) { c.enabled = false; overlays.Add(c); }

        if (vp.isPrepared)
        {
            vp.Play();
            float len = (float)clip.length;
            bool menu = false;
            for (float t = 0f; t < len + 0.5f; t += Time.unscaledDeltaTime)
            {
                mat.SetColor("_BaseColor", Color.white * Mathf.Clamp01(t / 0.5f));   // l'écran s'allume doucement
                bool skip = t > SkipAfter && VRInput.AnyTrigger();
                if (onMenu != null && !menu && (t >= len * MenuAt || skip))
                {
                    menu = true;              // le bilan par-dessus ; la gâchette sert maintenant à ses boutons
                    onMenu();
                    continue;
                }
                if (menu)
                {
                    float vt = (float)vp.time, hold = len - HoldBeforeEnd;
                    audio.volume = Mathf.Clamp01((hold - vt) / 1.5f);              // son coupé en fondu avant l'image figée
                    if (vt >= hold || (t > 1f && !vp.isPlaying)) { vp.Pause(); audio.volume = 0f; break; }
                }
                else if (skip) break;
                if (t > 1f && !vp.isPlaying) break;
                yield return null;
            }
            if (menu)
            {
                // reste à l'écran derrière le bilan jusqu'au rechargement de la scène (boutons du bilan)
                while (screen) yield return null;
                yield break;
            }
            vp.Stop();
        }

        cam.cullingMask = mask;
        cam.clearFlags = clear; cam.backgroundColor = bg;
        foreach (var c in overlays) if (c) c.enabled = true;
        Object.Destroy(screen);
        rt.Release();
        Playing = false;
        MusicPlayer.Silenced = false;
    }
}
