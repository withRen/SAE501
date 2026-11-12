using System.Collections.Generic;
using UnityEngine;

// [Multijoueur, hotfix #155] Retours visuels et sonores des éliminations, propres à CE casque (rien n'est créé sur les autres) :
//   - tireur : croix blanche à chaque balle qui touche un joueur, croix ROUGE + son de kill + « ÉLIMINÉ » quand il le tue ;
//   - victime : écran qui vire au rouge, « TU ES MORT » et qui l'a tué ; en version PC seulement, la vue tombe au sol
//     (au casque, pas de mouvement de caméra imposé : règle de confort de la SAÉ) ;
//   - tout le monde : une ligne dans le fil des éliminations (« J1 a éliminé J2 »), en haut de la vue, 4 s.
// Tout est accroché à la caméra du joueur (Camera.main) : quads et textes à 1 m environ, visibles au casque comme sur PC.
// Appelé par NetPlayer (DiedRpc, touche) ; la chute du personnage vu par les autres est dans PlayerAvatar.SetFallen.
[DefaultExecutionOrder(1000)]     // après DesktopPlayer : la chute de la vue PC s'ajoute à la pose qu'il vient de donner
public class NetKillFx : MonoBehaviour
{
    static NetKillFx instance;

    Transform cam;
    Renderer markA, markB;
    float markStart = -10f, markLife;
    Color markColor;
    TextMesh banner, sub, feed, status;
    float bannerUntil;
    readonly List<(string line, float until)> feedLines = new List<(string, float)>();
    Renderer redScreen;
    float deathStart = -1f;
    AudioSource audioSrc;
    static AudioClip killClip, hitClip;

    public static bool DeathScreenOn => instance && instance.deathStart >= 0f;

    static NetKillFx Get()
    {
        var c = Camera.main;
        if (!c) return null;
        if (instance && instance.cam == c.transform) return instance;
        if (instance) Destroy(instance.gameObject);
        var go = new GameObject("FX_Eliminations");
        instance = go.AddComponent<NetKillFx>();
        instance.Build(c.transform);
        return instance;
    }

    void OnDestroy() { if (instance == this) instance = null; }

    // ---------- appels ----------

    /// <summary>Balle du joueur local sur un autre joueur (pas encore mort).</summary>
    public static void Hit()
    {
        var f = Get(); if (!f) return;
        if (!hitClip) hitClip = Tone("hitmarker", 0.06f, 1900f, 0f);
        f.Mark(Color.white, 0.25f, 0.6f, hitClip);
    }

    /// <summary>Le joueur local vient d'éliminer quelqu'un.</summary>
    public static void Kill(string victim)
    {
        var f = Get(); if (!f) return;
        if (!killClip) killClip = Tone("kill", 0.32f, 880f, 1320f);
        f.Mark(new Color(1f, 0.15f, 0.12f), 0.9f, 1f, killClip);
        f.Banner("ÉLIMINÉ", victim, new Color(1f, 0.22f, 0.18f), 1.8f);
        VRInput.HapticBoth(0.7f, 0.12f);
    }

    /// <summary>Ligne du fil des éliminations, chez tout le monde.</summary>
    public static void Feed(string line)
    {
        var f = Get(); if (!f) return;
        f.feedLines.Add((line, Time.unscaledTime + 4f));
        if (f.feedLines.Count > 3) f.feedLines.RemoveAt(0);
        f.RefreshFeed();
    }

    /// <summary>Le joueur local est mort : écran rouge, « TU ES MORT » (sous-titre : qui l'a tué ou ce qui se passe ensuite).</summary>
    public static void Death(string detail)
    {
        var f = Get(); if (!f) return;
        f.deathStart = Time.unscaledTime;
        f.Banner("TU ES MORT", detail, new Color(1f, 0.25f, 0.2f), 999f);
    }

    /// <summary>[hotfix #161] Ligne d'état persistante en haut de la vue (vide = masquée) : compteur du portail en coop Évasion.</summary>
    public static void Status(string text)
    {
        var f = string.IsNullOrEmpty(text) ? instance : Get();
        if (!f) return;
        f.status.text = text ?? "";
    }

    /// <summary>Retour à la vie (manche suivante, fin de vague) : on efface l'écran de mort.</summary>
    public static void Alive()
    {
        if (!instance) return;
        instance.deathStart = -1f;
        instance.bannerUntil = 0f;
    }

    // ---------- construction ----------

    void Build(Transform c)
    {
        cam = c;
        transform.SetParent(c, false);
        var tex = Texture2D.whiteTexture;
        markA = HeadOverlay.Create(c, "Croix_A", tex, Color.clear, 1f, 4010);
        markB = HeadOverlay.Create(c, "Croix_B", tex, Color.clear, 1f, 4010);
        foreach (var m in new[] { markA, markB }) { m.transform.SetParent(transform, false); m.transform.localPosition = new Vector3(0f, 0f, 1f); m.enabled = false; }
        markA.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        markB.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);

        redScreen = HeadOverlay.Create(c, "Ecran_Mort", HeadOverlay.RadialTexture(128, 0.05f), Color.clear, 0.35f, 4000);
        redScreen.transform.SetParent(transform, false);
        redScreen.transform.localPosition = new Vector3(0f, 0f, 0.35f);
        redScreen.transform.localScale = new Vector3(0.9f, 0.7f, 1f);    // bords rouges visibles au casque (≈ 100°) comme sur PC (70°)
        redScreen.enabled = false;

        banner = Text("Titre", new Vector3(0f, -0.06f, 1.1f), 0.006f, 80);
        sub = Text("Sous_titre", new Vector3(0f, -0.15f, 1.1f), 0.0045f, 64);
        feed = Text("Fil", new Vector3(0f, 0.27f, 1.2f), 0.0042f, 60);
        feed.color = new Color(1f, 1f, 1f, 0.9f);
        status = Text("Etat", new Vector3(0f, 0.19f, 1.2f), 0.0042f, 60);
        status.color = new Color(0.55f, 1f, 0.6f);

        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.spatialBlend = 0f; audioSrc.playOnAwake = false; audioSrc.ignoreListenerPause = true;
    }

    TextMesh Text(string n, Vector3 pos, float size, int fontSize)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        var t = go.AddComponent<TextMesh>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var mr = go.GetComponent<MeshRenderer>();
        var mat = new Material(t.font.material) { renderQueue = 4020 };
        mat.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);   // toujours devant le décor
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        t.fontSize = fontSize; t.characterSize = size; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center;
        t.text = "";
        return t;
    }

    void Mark(Color c, float life, float scale, AudioClip clip)
    {
        markColor = c; markLife = life; markStart = Time.unscaledTime;
        markA.transform.localScale = markB.transform.localScale = new Vector3(0.006f, 0.07f, 1f) * scale;
        if (clip) audioSrc.PlayOneShot(clip, Mathf.Clamp01(GameSettings.Effects + 0.2f));
    }

    void Banner(string title, string detail, Color c, float seconds)
    {
        banner.text = title; banner.color = c;
        sub.text = detail ?? ""; sub.color = new Color(1f, 1f, 1f, 0.9f);
        bannerUntil = Time.unscaledTime + seconds;
    }

    void RefreshFeed()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var l in feedLines) sb.AppendLine(l.line);
        feed.text = sb.ToString().TrimEnd();
    }

    // ---------- animation ----------

    void LateUpdate()
    {
        if (!cam) { Destroy(gameObject); return; }
        float now = Time.unscaledTime;

        // croix : apparaît grande puis se resserre, s'efface
        float mt = (now - markStart) / Mathf.Max(0.01f, markLife);
        bool markOn = mt < 1f;
        markA.enabled = markB.enabled = markOn;
        if (markOn)
        {
            float a = 1f - mt * mt;
            float grow = 1f + 0.6f * Mathf.Exp(-mt * 10f);
            var col = new Color(markColor.r, markColor.g, markColor.b, a);
            markA.material.color = markB.material.color = col;
            markA.transform.localPosition = markB.transform.localPosition = new Vector3(0f, 0f, 1f / grow);
        }

        // titre
        bool bannerOn = now < bannerUntil;
        banner.gameObject.SetActive(bannerOn); sub.gameObject.SetActive(bannerOn);
        if (bannerOn && bannerUntil - now < 0.4f)
        {
            float a = (bannerUntil - now) / 0.4f;
            banner.color = new Color(banner.color.r, banner.color.g, banner.color.b, a);
            sub.color = new Color(1f, 1f, 1f, 0.9f * a);
        }

        // fil des éliminations
        if (feedLines.Count > 0 && feedLines[0].until < now) { feedLines.RemoveAt(0); RefreshFeed(); }

        // écran de mort : rouge sombre qui monte en 0,6 s, bords plus sombres (texture radiale)
        bool dead = deathStart >= 0f && !NetSpectator.Active;       // spectateur : l'écran rouge laisse voir l'image du coéquipier
        redScreen.enabled = dead;
        if (NetSpectator.Active) { banner.gameObject.SetActive(false); sub.gameObject.SetActive(false); }
        if (dead)
        {
            float k = Mathf.Clamp01((now - deathStart) / 0.6f);
            float pulse = 1f + 0.08f * Mathf.Sin((now - deathStart) * 3f);
            redScreen.material.color = new Color(0.55f, 0.02f, 0.02f, Mathf.Lerp(0f, 0.85f, k) * pulse);
            // version PC : la vue tombe sur le côté (pas au casque : aucun mouvement de caméra imposé)
            if (DesktopMode.On && !NetSpectator.Active)
            {
                float e = 1f - Mathf.Pow(1f - Mathf.Clamp01((now - deathStart) / 0.7f), 3f);
                cam.localPosition += Vector3.down * (1.1f * e);
                cam.localRotation *= Quaternion.Euler(-12f * e, 0f, 28f * e);
            }
        }
    }

    // son généré : bip court (touche) ou deux notes montantes (kill)
    static AudioClip Tone(string name, float dur, float f1, float f2)
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(dur * rate);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float f = f2 > 0f && t > dur * 0.4f ? f2 : f1;
            float env = Mathf.Exp(-t * (f2 > 0f ? 9f : 60f)) * Mathf.Clamp01(t * 400f);
            data[i] = (Mathf.Sin(t * f * 6.2832f) * 0.6f + Mathf.Sin(t * f * 2f * 6.2832f) * 0.15f) * env * 0.7f;
        }
        var c = AudioClip.Create(name, n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }
}
