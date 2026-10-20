using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pistol : MonoBehaviour
{
    public int ammo = 12;
    public int maxAmmo = 12;
    public Transform muzzle;
    public Light flash;
    public TextMesh hud;
    public float range = 60f;

    [Header("Tir : fumée et haptique")]
    [Tooltip("Point d'où sort la fumée (arrière de l'arme). Créé automatiquement si vide.")]
    public Transform smokePoint;
    public Material smokeMaterial;
    public Vector3 smokeLocalPosition = new Vector3(0f, 0.02f, -0.07f);
    [Range(0f, 1f)] public float hapticAmplitude = 0.7f;
    public float hapticDuration = 0.08f;
    public float emptyHapticAmplitude = 0.15f;
    [Tooltip("[#113] Tir à la tête : vibration forte et plus longue, pour sentir le coup mortel.")]
    [Range(0f, 1f)] public float headshotHapticAmplitude = 1f;
    public float headshotHapticDuration = 0.2f;

    ParticleSystem smoke;

    [Header("Modèle Modern Guns : M1911 (animation, douille, flash)")]
    public Animator animator;                       // animation « Fire » du pack (glissière, chien, détente)
    public Transform slider;                        // glissière : reste en arrière quand le chargeur est vide
    public Transform ejection;                      // sortie des douilles (Ejection_Location)
    public GameObject casingPrefab;                 // douille du pack
    public GameObject muzzleFlashPrefab;            // flash du pack
    [Range(0f, 1f)] public float fireStartNormalized = 0.42f;   // l'animation du pack tire à 0,117 s ; on saute le début pour tirer sans retard
    public float casingDelay = 0.085f;              // la douille sort à 0,183 s dans le clip ; on part à 0,42 × 0,233 s = 0,098 s : 0,085 s plus tard
    public Vector3 slideBackOffset = new Vector3(0f, 0f, -0.039f);

    Vector3 sliderRest;
    float lastShot = -10f;
    Collider[] ownColliders;

    [Header("HUD munitions")]
    public Color hudNormal = Color.white;
    public Color hudLow = new Color(1f, 0.6f, 0.1f);
    public Color hudEmpty = new Color(1f, 0.15f, 0.1f);
    public int lowThreshold = 3;

    MeshRenderer hudRenderer;

    XRGrabInteractable grab;
    AudioSource src;
    static AudioClip shot, click;

    [Header("Sons (si vides : sons générés par code)")]
    public AudioClip shotClip;          // tir
    public AudioClip reloadClip;        // chargeur inséré
    public AudioClip dryFireClip;       // clic quand le chargeur est vide

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.activated.AddListener(OnActivate);
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        if (!shot) { shot = ProcAudio.Shot(); click = ProcAudio.DryFire(); }
        if (flash) flash.enabled = false;
        BuildSmoke();
        if (slider) sliderRest = slider.localPosition;
        ownColliders = GetComponents<Collider>();
        if (hud) hudRenderer = hud.GetComponent<MeshRenderer>();
        UpdateHud();
    }

    // Le compteur n'est visible que quand le pistolet est en main (pas dans le holster).
    void Update()
    {
        if (!hud) return;
        bool held = BeltItem.InHand(grab);
        if (hudRenderer) hudRenderer.enabled = held;
        if (!held) return;

        // chargeur vide : clignote en rouge
        if (ammo <= 0) hud.color = Mathf.Repeat(Time.time, 0.6f) < 0.3f ? hudEmpty : hudEmpty * 0.35f;
    }

    void OnActivate(ActivateEventArgs a)
    {
        if (!BeltItem.InHand(grab) || PauseMenu.Paused) return;
        Fire(a.interactorObject);
    }

    // un tir complet (son, flash, fumée, animation du pack, douille, haptique, rayon) ; public pour pouvoir être testé sans casque
    public void Fire(IXRActivateInteractor interactor = null)
    {
        if (ammo <= 0) { src.PlayOneShot(dryFireClip ? dryFireClip : click); Haptic(interactor, emptyHapticAmplitude, 0.03f); return; }

        ammo--;
        GameStats.Shot();
        UpdateHud();
        src.PlayOneShot(shotClip ? shotClip : shot);
        StartCoroutine(Flash());
        if (smoke) smoke.Emit(14);
        lastShot = Time.time;
        if (animator) animator.Play("Fire", 0, fireStartNormalized);
        // Unity ne livre pas les événements d'animation situés après un départ décalé : la douille est donc déclenchée par un minuteur
        CancelInvoke(nameof(EjectCasing));
        Invoke(nameof(EjectCasing), casingDelay);
        if (muzzleFlashPrefab && muzzle)
        {
            var f = Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation, muzzle);
            Destroy(f, 1f);
        }
        Haptic(interactor, hapticAmplitude, hapticDuration);

        if (AimedAtOwnHead()) { if (PlayerHealth.Instance) PlayerHealth.Instance.Kill(); return; }

        if (Physics.Raycast(muzzle.position, muzzle.forward, out var hit, range, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.rigidbody) hit.rigidbody.AddForceAtPosition(muzzle.forward * 6f, hit.point, ForceMode.Impulse);
            var hb = hit.collider.GetComponent<ZombieHitbox>();
            bool head = hb && hb.zone == HitZone.Head && hb.zombie && !hb.zombie.Dead;
            hit.collider.SendMessage("OnShot", hit, SendMessageOptions.DontRequireReceiver);
            // [#113] tête touchée : un deuxième retour, fort, juste après celui du recul de l'arme
            if (head) StartCoroutine(HeadshotHaptic(interactor));
        }
    }

    // canon collé contre la tête du joueur et pointé vers elle (la tête n'a pas de collider : test de distance et d'angle)
    [Header("Tir dans la tête")]
    public float headRadius = 0.35f;        // distance max canon -> centre de la tête
    public float headAngle = 35f;           // angle max entre le canon et la direction de la tête

    public bool AimedAtOwnHead()
    {
        var cam = Camera.main;
        if (!cam || !muzzle) return false;
        var toHead = cam.transform.position - muzzle.position;
        float d = toHead.magnitude;
        if (d > headRadius) return false;
        return d < 0.06f || Vector3.Angle(muzzle.forward, toHead) < headAngle;
    }

    // éjecte une douille vers la droite et le haut
    public void EjectCasing()
    {
        if (!casingPrefab || !ejection) return;
        var c = Instantiate(casingPrefab, ejection.position, ejection.rotation);
        var rb = c.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.mass = 0.01f;
            rb.linearVelocity = ejection.right * Random.Range(1.0f, 1.6f) + ejection.up * Random.Range(0.8f, 1.2f) - ejection.forward * 0.3f + Random.insideUnitSphere * 0.15f;
            rb.angularVelocity = Random.insideUnitSphere * 20f;
        }
        foreach (var cc in c.GetComponentsInChildren<Collider>())
            foreach (var own in ownColliders) if (own) Physics.IgnoreCollision(cc, own);
        Destroy(c, 2.5f);
    }

    // glissière bloquée en arrière quand le chargeur est vide (le clip du pack n'a pas cette pose)
    void LateUpdate()
    {
        if (!slider) return;
        if (ammo <= 0 && Time.time - lastShot > 0.3f) slider.localPosition = sliderRest + slideBackOffset;
        else if (ammo > 0 && Time.time - lastShot > 0.3f && (slider.localPosition - sliderRest).sqrMagnitude > 1e-8f && !AnimatorBusy()) slider.localPosition = sliderRest;
    }

    bool AnimatorBusy() => animator && animator.GetCurrentAnimatorStateInfo(0).IsName("Fire");

    // Petite bouffée de fumée grise qui sort de l'arrière de l'arme, vers l'arrière et le haut.
    void BuildSmoke()
    {
        if (!smokePoint)
        {
            var go = new GameObject("SmokePoint");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = smokeLocalPosition;
            go.transform.localRotation = Quaternion.Euler(-25f, 180f, 0f);   // regarde vers l'arrière, un peu vers le haut
            smokePoint = go.transform;
        }

        smoke = smokePoint.gameObject.AddComponent<ParticleSystem>();
        smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = smoke.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.045f);
        main.startColor = new Color(0.75f, 0.75f, 0.75f, 0.5f);
        main.gravityModifier = -0.04f;                       // la fumée monte doucement
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 60;

        var em = smoke.emission; em.enabled = false;         // on émet à la main à chaque tir
        var sh = smoke.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 14f; sh.radius = 0.004f;

        var col = smoke.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.55f, 0.1f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var size = smoke.sizeOverLifetime; size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 2.4f)));

        var r = smoke.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        if (smokeMaterial)
        {
            r.sharedMaterial = smokeMaterial;
            var m = r.material;                              // instance : on lui donne la texture ronde et douce
            m.mainTexture = SoftDot();
            r.material = m;
        }
    }

    static Texture2D softDot;
    static Texture2D SoftDot()
    {
        if (softDot) return softDot;
        const int n = 32;
        softDot = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f - 0.5f, n / 2f - 0.5f)) / (n / 2f);
                softDot.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(1f - d) * Mathf.Clamp01(1f - d)));
            }
        softDot.Apply();
        return softDot;
    }

    // Vibration de la manette qui tient l'arme (main identifiée par le nom de l'interacteur).
    static void Haptic(IXRActivateInteractor interactor, float amplitude, float duration)
    {
        if (interactor == null) return;
        var t = (interactor as Component)?.transform;
        bool left = false;
        for (var c = t; c != null; c = c.parent)
            if (c.name.IndexOf("left", System.StringComparison.OrdinalIgnoreCase) >= 0) { left = true; break; }
            else if (c.name.IndexOf("right", System.StringComparison.OrdinalIgnoreCase) >= 0) break;
        var dev = InputDevices.GetDeviceAtXRNode(left ? XRNode.LeftHand : XRNode.RightHand);
        if (dev.isValid) dev.SendHapticImpulse(0, amplitude, duration);
    }

    IEnumerator HeadshotHaptic(IXRActivateInteractor interactor)
    {
        yield return new WaitForSeconds(hapticDuration);
        Haptic(interactor, headshotHapticAmplitude, headshotHapticDuration);
    }

    IEnumerator Flash()
    {
        if (!flash) yield break;
        flash.enabled = true;
        yield return new WaitForSeconds(0.04f);
        flash.enabled = false;
    }

    // Recharge avec un chargeur de `rounds` balles. Rééquilibrage : les balles restantes ne sont plus perdues.
    // Comme en vrai, l'ancien chargeur sort avec ce qu'il lui restait : le pistolet prend ce qu'il peut (12 au plus)
    // et le reste est rendu (MagWell remet ces balles dans le chargeur qu'il éjecte). Renvoie ce reste (0 : chargeur vide, consommé).
    public int Load(int rounds)
    {
        int total = ammo + Mathf.Max(0, rounds);
        int loaded = Mathf.Min(total, maxAmmo);
        GameStats.Loaded(Mathf.Max(0, loaded - ammo));
        ammo = loaded;
        src.PlayOneShot(reloadClip ? reloadClip : click);
        UpdateHud();
        return total - loaded;
    }

    void UpdateHud()
    {
        if (!hud) return;
        hud.text = ammo.ToString("00") + "/" + maxAmmo.ToString("00");
        hud.color = ammo <= 0 ? hudEmpty : (ammo <= lowThreshold ? hudLow : hudNormal);
    }
}
