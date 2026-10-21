using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// Cinématique de fin (victoire en mode Évasion) : une vidéo rendue dans Blender (Tools/blender/cinematique_fin.py),
// jouée sur un grand écran dans le noir, face au joueur. GameOverUI la lance après le fondu au noir, avant le bilan.
// Confort au casque : l'écran est fixe dans le monde (posé devant la tête au lancement), la tête bouge librement,
// aucune caméra n'est déplacée à la place du joueur. La caméra ne dessine plus que l'écran (calque Layer) : décor, mains
// et zombies disparaissent, il reste un écran de cinéma dans le noir.
// Une gâchette passe la vidéo (après skipAfter secondes). Sans vidéo dans Resources, rien n'est joué.
public static class EscapeVideo
{
    public const string ClipPath = "Cinematique/CinematiqueFin";   // Assets/Resources/Cinematique/CinematiqueFin.mp4
    public const int Layer = 30;            // calque réservé à l'écran (31 : incrustations de vidéosurveillance)
    const float Distance = 3.2f;            // m devant la tête
    const float Width = 4.2f;               // m : ≈ 66° de large, comme une place au milieu d'une salle de cinéma
    const float SkipAfter = 2f;

    public static bool Available => Resources.Load<VideoClip>(ClipPath) != null;

    public static IEnumerator Play(Transform head)
    {
        var clip = Resources.Load<VideoClip>(ClipPath);
        var cam = head ? head.GetComponent<Camera>() : Camera.main;
        if (clip == null || cam == null) yield break;

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
        var vp = screen.AddComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.clip = clip;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.targetTexture = rt;
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.SetTargetAudioSource(0, audio);
        vp.isLooping = false;
        vp.Prepare();
        while (!vp.isPrepared) yield return null;

        // on ne voit plus que l'écran
        int mask = cam.cullingMask;
        var clear = cam.clearFlags; var bg = cam.backgroundColor;
        cam.cullingMask = 1 << Layer;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        vp.Play();
        float len = (float)clip.length;
        for (float t = 0f; t < len + 0.5f; t += Time.unscaledDeltaTime)
        {
            mat.SetColor("_BaseColor", Color.white * Mathf.Clamp01(t / 0.5f));   // l'écran s'allume doucement
            if (t > SkipAfter && VRInput.AnyTrigger()) break;
            if (t > 1f && !vp.isPlaying) break;
            yield return null;
        }
        vp.Stop();

        cam.cullingMask = mask;
        cam.clearFlags = clear; cam.backgroundColor = bg;
        Object.Destroy(screen);
        rt.Release();
    }
}
