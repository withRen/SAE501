using UnityEngine;

// « VAGUE N » au milieu de la vue au début de chaque vague, calé sur le son de début de vague (Resources/Audio/Vague_Debut, 4 s) :
// fondu d'apparition 1 s, 1,5 s bien visible, fondu de disparition 1,5 s (le chuchotement monte, tient, puis s'éteint).
// Texte collé à la tête (VR et PC, même caméra), dessiné par-dessus le décor ; rien ne bouge la caméra.
// Créé par WaveManager.Start, appelé par WaveManager.StartLevel.
public class WaveTitle : MonoBehaviour
{
    public float fadeIn = 1f, hold = 1.5f, fadeOut = 1.5f;     // total 4 s, la durée du son
    public float distance = 1.2f;                              // devant les yeux (m)
    public Color color = new Color(0.95f, 0.12f, 0.08f);
    [Range(0f, 1f)] public float volume = 1f;

    TextMesh text;
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
        text = gameObject.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 120; text.characterSize = 0.0075f * distance;
        text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center;
        text.fontStyle = FontStyle.Bold;
        var mat = new Material(text.font.material);
        mat.SetFloat("unity_GUIZTestMode", (float)UnityEngine.Rendering.CompareFunction.Always);   // jamais caché par un mur proche
        mat.renderQueue = 4000;
        var r = GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.enabled = false;

        clip = Resources.Load<AudioClip>("Audio/Vague_Debut");
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false; src.spatialBlend = 0f;
    }

    public void Show(int wave)
    {
        text.text = "VAGUE " + wave;
        startedAt = Time.time;
        if (clip) src.PlayOneShot(clip, volume);
    }

    void Update()
    {
        float t = Time.time - startedAt;
        float a = t < fadeIn ? t / fadeIn : t < fadeIn + hold ? 1f : 1f - (t - fadeIn - hold) / fadeOut;
        a = Mathf.Clamp01(a);
        var r = GetComponent<MeshRenderer>();
        r.enabled = a > 0f;
        if (a > 0f) text.color = new Color(color.r, color.g, color.b, Mathf.SmoothStep(0f, 1f, a));
    }
}
