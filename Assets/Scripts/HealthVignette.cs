using UnityEngine;

// Vignettage rouge façon Call of Duty : rouge sang sur les bords de la vue, de plus en plus envahissant quand les PV baissent,
// flash à chaque coup reçu, pulsation « cœur qui bat » quand il reste très peu de PV, et qui s'estompe quand les PV reviennent.
// Dessiné par le shader SAE501/DamageVignette (plein écran, compatible casque).
// À mettre sur n'importe quel objet de la scène. Assigner "head" = Main Camera du XR Origin et "material".
public class HealthVignette : MonoBehaviour
{
    public Transform head;
    public Material material;                       // matériau utilisant SAE501/DamageVignette
    public Color color = new Color(0.55f, 0f, 0f, 1f);

    [Tooltip("Intensité de base selon la part de vie perdue (de 0 % à 100 %, répartie régulièrement ; valeurs intermédiaires interpolées).")]
    public float[] intensityByHpLost = { 0f, 0.5f, 0.75f, 0.95f };

    public float smooth = 5f;          // vitesse de transition
    public float hitFlash = 0.6f;      // pic supplémentaire à chaque coup reçu
    public float flashDecay = 1.4f;
    public float heartbeatHz = 1.3f;   // rythme de la pulsation quand il reste très peu de PV

    Renderer rend;
    Material mat;
    float shown, flash;
    float hp, max;

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;

        // quad collé à la tête : sa position n'a pas d'importance (le shader le met en plein écran) mais il doit rester dans le champ de vision
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = "HealthVignette";
        Destroy(q.GetComponent<Collider>());
        q.transform.SetParent(head, false);
        q.transform.localPosition = new Vector3(0f, 0f, 1f);
        q.transform.localScale = Vector3.one * 4f;
        rend = q.GetComponent<Renderer>();
        mat = new Material(material);
        rend.sharedMaterial = mat;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
        rend.enabled = false;

        var h = PlayerHealth.Instance;
        if (!h) { Debug.LogWarning("HealthVignette : aucun PlayerHealth dans la scène."); enabled = false; return; }
        hp = h.Health; max = h.maxHealth;
        h.Changed += (c, m) => { hp = c; max = m; };
        h.Damaged += _ => flash = hitFlash;
    }

    void Update()
    {
        if (rend == null) return;
        float lostFraction = max > 0f ? Mathf.Clamp01(1f - hp / max) : 0f;
        float pos = lostFraction * (intensityByHpLost.Length - 1);
        int i0 = Mathf.Clamp(Mathf.FloorToInt(pos), 0, intensityByHpLost.Length - 1);
        int i1 = Mathf.Min(i0 + 1, intensityByHpLost.Length - 1);
        float target = Mathf.Lerp(intensityByHpLost[i0], intensityByHpLost[i1], pos - i0);

        // « cœur qui bat » : double battement quand il reste le quart des PV ou moins
        if (hp > 0f && hp <= max * 0.25f)
        {
            float t = Mathf.Repeat(Time.time * heartbeatHz, 1f);
            float beat = Mathf.Exp(-14f * t) + 0.6f * Mathf.Exp(-14f * Mathf.Abs(t - 0.3f));
            target = Mathf.Clamp01(target * (0.8f + 0.35f * beat));
        }

        shown = Mathf.Lerp(shown, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        flash = Mathf.MoveTowards(flash, 0f, flashDecay * Time.deltaTime);
        float k = Mathf.Clamp01(shown + flash);

        mat.SetColor("_Color", color);
        mat.SetFloat("_Intensity", k);
        mat.SetFloat("_Inner", Mathf.Lerp(0.78f, 0.3f, k));    // le rouge avance vers le centre quand c'est grave
        mat.SetFloat("_Width", Mathf.Lerp(0.55f, 0.4f, k));
        rend.enabled = k > 0.004f;
    }
}
