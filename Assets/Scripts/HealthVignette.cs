using UnityEngine;

// Vignettage rouge qui s'épaissit quand les PV baissent (style Call of Duty).
// À mettre sur n'importe quel objet de la scène. Assigner "head" = Main Camera du XR Origin.
public class HealthVignette : MonoBehaviour
{
    public Transform head;
    public Color color = new Color(0.65f, 0f, 0f, 1f);

    [Tooltip("Intensité de base selon les PV perdus : 0 PV perdu -> 0, 1 -> .., 3 PV perdus (1 PV restant) -> ..")]
    public float[] intensityByHpLost = { 0f, 0.3f, 0.55f, 0.85f };

    public float smooth = 4f;       // vitesse de transition
    public float hitFlash = 0.45f;  // pic rouge supplémentaire à chaque coup
    public float flashDecay = 2.5f;

    Renderer rend;
    float shown, flash;
    int hp, max;

    void Start()
    {
        if (!head) head = Camera.main.transform;
        float d = Camera.main.nearClipPlane + 0.05f;
        baseSize = 3.2f * d;
        rend = HeadOverlay.Create(head, "HealthVignette", HeadOverlay.RadialTexture(), new Color(color.r, color.g, color.b, 0f), d, 4000);

        var h = PlayerHealth.Instance;
        if (!h) { Debug.LogWarning("HealthVignette : aucun PlayerHealth dans la scène."); enabled = false; return; }
        hp = h.Health; max = h.maxHealth;
        h.Changed += (c, m) => { hp = c; max = m; };
        h.Damaged += _ => flash = hitFlash;
    }

    float baseSize;

    void Update()
    {
        int lost = Mathf.Clamp(max - hp, 0, intensityByHpLost.Length - 1);
        float target = intensityByHpLost[lost];

        // pulsation "cœur" quand il reste 1 PV
        if (hp == 1) target *= 0.9f + 0.1f * Mathf.Sin(Time.time * 6f);

        shown = Mathf.Lerp(shown, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        flash = Mathf.MoveTowards(flash, 0f, flashDecay * Time.deltaTime);

        float k = Mathf.Clamp01(shown + flash);

        // plus k est grand, plus le quad rétrécit => le rouge avance vers le centre
        rend.transform.localScale = Vector3.one * baseSize * Mathf.Lerp(1.5f, 0.8f, k);
        rend.material.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(k * 1.2f));
        rend.enabled = k > 0.005f;
    }
}
