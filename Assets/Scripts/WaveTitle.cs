using UnityEngine;

// « VAGUE N » au milieu de la vue au début de chaque vague, calé sur le son de début de vague (Resources/Audio/Vague_Debut, 4 s) :
// fondu d'apparition 1 s, 1,5 s bien visible, fondu de disparition 1,5 s (le chuchotement monte, tient, puis s'éteint).
// Lettres de sang : police Nosifer (Typomondo, SIL Open Font License : usage libre, y compris commercial ; Resources/Fonts),
// rouge sang avec une ombre noire derrière pour rester lisible sur un décor sombre ou clair.
// Texte collé à la tête (VR et PC, même caméra), dessiné par-dessus le décor ; rien ne bouge la caméra.
// Créé par WaveManager.Start, appelé par WaveManager.StartLevel.
public class WaveTitle : MonoBehaviour
{
    public float fadeIn = 1f, hold = 1.5f, fadeOut = 1.5f;     // total 4 s, la durée du son
    public float distance = 1.2f;                              // devant les yeux (m)
    public Color color = new Color(0.62f, 0.02f, 0.02f);       // rouge sang
    public Color shadow = new Color(0f, 0f, 0f, 0.85f);
    [Range(0f, 1f)] public float volume = 1f;

    TextMesh text, back;
    AudioSource src;
    AudioClip clip;
    float startedAt = -99f;

    public static WaveTitle Create(Transform head)
    {
        if (!head) return null;
        var go = new GameObject("TitreVague");
        go.transform.SetParent(head, false);
        return go.AddComponent<WaveTitle>();
    }

    void Awake()
    {
        transform.localPosition = new Vector3(0f, 0f, distance);
        transform.localRotation = Quaternion.identity;
        var font = Resources.Load<Font>("Fonts/Nosifer-Regular");
        if (!font) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text = MakeText(gameObject, font, 4001);
        // ombre : même texte, un peu décalé vers le bas et derrière, dessiné avant
        var b = new GameObject("Ombre");
        b.transform.SetParent(transform, false);
        b.transform.localPosition = new Vector3(0.004f, -0.006f, 0.002f) * distance;
        back = MakeText(b, font, 4000);

        clip = Resources.Load<AudioClip>("Audio/Vague_Debut");
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false; src.spatialBlend = 0f;
    }

    TextMesh MakeText(GameObject go, Font font, int queue)
    {
        var t = go.AddComponent<TextMesh>();
        t.font = font;
        t.fontSize = 120; t.characterSize = 0.0068f * distance;
        t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center;
        var mat = new Material(font.material);
        mat.SetFloat("unity_GUIZTestMode", (float)UnityEngine.Rendering.CompareFunction.Always);   // jamais caché par un mur proche
        mat.renderQueue = queue;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.enabled = false;
        return t;
    }

    public void Show(int wave)
    {
        text.text = back.text = "VAGUE " + wave;
        startedAt = Time.time;
        if (clip) src.PlayOneShot(clip, volume);
    }

    void Update()
    {
        float t = Time.time - startedAt;
        float a = t < fadeIn ? t / fadeIn : t < fadeIn + hold ? 1f : 1f - (t - fadeIn - hold) / fadeOut;
        a = Mathf.Clamp01(a);
        a = Mathf.SmoothStep(0f, 1f, a);
        text.GetComponent<MeshRenderer>().enabled = back.GetComponent<MeshRenderer>().enabled = a > 0f;
        if (a > 0f)
        {
            text.color = new Color(color.r, color.g, color.b, a);
            back.color = new Color(shadow.r, shadow.g, shadow.b, shadow.a * a);
        }
    }
}
