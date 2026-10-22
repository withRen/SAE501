using UnityEngine;

// Vignettage rouge façon Call of Duty : rouge sang sur les bords de la vue, de plus en plus envahissant quand les PV baissent,
// flash à chaque coup reçu, pulsation « cœur qui bat » quand il reste très peu de PV, et qui s'estompe quand les PV reviennent.
// Dessiné par le shader SAE501/DamageVignette (plein écran, compatible casque).
// Retour de coup renforcé (confortable en VR : rien ne bouge la caméra) :
//   - flash rouge proportionnel aux dégâts ;
//   - arc rouge autour du centre de la vue du côté du zombie (haut = devant, bas = derrière), qui suit les rotations de la tête
//     et s'efface en ~2 s ; le bord de l'écran de ce côté-là rougit aussi (vision périphérique) ;
//   - son : choc sourd dans le corps + griffure jouée depuis la direction du zombie ; battement de cœur sous 25 % des PV.
// (La vibration des manettes, plus forte du côté du zombie, est dans PlayerHealth.)
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

    [Header("Indicateur de direction")]
    public float indicatorHold = 0.6f;     // durée à pleine intensité après le coup
    public float indicatorFade = 1.6f;     // puis disparition progressive

    [Header("Sons")]
    [Range(0f, 1f)] public float hurtVolume = 0.9f;
    [Range(0f, 1f)] public float swipeVolume = 0.7f;
    [Range(0f, 1f)] public float heartbeatVolume = 0.55f;

    Renderer rend;
    Material mat;
    float shown, flash;
    float hp, max;

    // coups récents (4 au plus, comme dans le shader) : position de l'attaquant et heure du coup
    readonly Vector3[] hitFrom = new Vector3[4];
    readonly float[] hitTime = { -99f, -99f, -99f, -99f };
    readonly Vector4[] hits = new Vector4[4];
    AudioSource hurtSrc, swipeSrc;
    AudioClip hurtClip, swipeClip, heartClip;
    float lastBeatPhase = 1f;

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
        h.Damaged += a => flash = Mathf.Max(flash, hitFlash * Mathf.Lerp(0.8f, 1.5f, Mathf.Clamp01(a / (max * 0.35f))));
        h.Damaged += _ => { if (hurtSrc) hurtSrc.PlayOneShot(hurtClip, hurtVolume); };
        h.DamagedFrom += (_, from) => OnHitFrom(from);

        hurtClip = ProcAudio.PlayerHurt(); swipeClip = ProcAudio.ClawSwipe(); heartClip = ProcAudio.Heartbeat(0.3f / heartbeatHz);
        hurtSrc = head.gameObject.AddComponent<AudioSource>();
        hurtSrc.playOnAwake = false; hurtSrc.spatialBlend = 0f;
        var sw = new GameObject("SonGriffure");                  // déplacé du côté du zombie à chaque coup
        sw.transform.SetParent(head, false);
        swipeSrc = sw.AddComponent<AudioSource>();
        swipeSrc.playOnAwake = false; swipeSrc.spatialBlend = 1f; swipeSrc.minDistance = 1f; swipeSrc.maxDistance = 10f;
    }

    void OnHitFrom(Vector3 from)
    {
        // même zombie (moins d'un mètre d'écart) : on ravive son arc ; sinon on remplace le plus ancien
        int slot = 0;
        for (int i = 0; i < hitFrom.Length; i++)
        {
            if (Time.time - hitTime[i] < indicatorHold + indicatorFade && (hitFrom[i] - from).sqrMagnitude < 1f) { slot = i; break; }
            if (hitTime[i] < hitTime[slot]) slot = i;
        }
        hitFrom[slot] = from; hitTime[slot] = Time.time;

        if (swipeSrc)
        {
            var d = Vector3.ProjectOnPlane(from - head.position, Vector3.up);
            swipeSrc.transform.position = head.position + (d.sqrMagnitude > 1e-4f ? d.normalized : head.forward) * 0.7f;
            swipeSrc.pitch = Random.Range(0.9f, 1.15f);
            swipeSrc.PlayOneShot(swipeClip, swipeVolume);
        }
    }

    // direction de l'attaquant dans le repère de la tête, à plat (x = droite, y = devant) : l'arc ne dépend que du cap du regard
    Vector2 FlatDirection(Vector3 from)
    {
        var local = Quaternion.Inverse(Quaternion.Euler(0f, head.eulerAngles.y, 0f)) * (from - head.position);
        var d = new Vector2(local.x, local.z);
        return d.sqrMagnitude > 1e-4f ? d.normalized : Vector2.up;
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
            if (t < lastBeatPhase && hurtSrc) hurtSrc.PlayOneShot(heartClip, heartbeatVolume);     // début d'un battement
            lastBeatPhase = t;
        }
        else lastBeatPhase = 1f;

        shown = Mathf.Lerp(shown, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        flash = Mathf.MoveTowards(flash, 0f, flashDecay * Time.deltaTime);
        float k = Mathf.Clamp01(shown + flash);

        mat.SetColor("_Color", color);
        mat.SetFloat("_Intensity", k);
        mat.SetFloat("_Inner", Mathf.Lerp(0.78f, 0.3f, k));    // le rouge avance vers le centre quand c'est grave
        mat.SetFloat("_Width", Mathf.Lerp(0.55f, 0.4f, k));

        bool anyHit = false;
        for (int i = 0; i < hits.Length; i++)
        {
            float age = Time.time - hitTime[i];
            float w = hp > 0f ? 1f - Mathf.Clamp01((age - indicatorHold) / indicatorFade) : 0f;
            var d = w > 0f ? FlatDirection(hitFrom[i]) : Vector2.up;
            hits[i] = new Vector4(d.x, d.y, w, 0f);
            anyHit |= w > 0f;
        }
        mat.SetVectorArray("_Hits", hits);
        rend.enabled = k > 0.004f || anyHit;
    }
}
