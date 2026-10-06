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

    ParticleSystem smoke;

    [Header("HUD munitions")]
    public Color hudNormal = Color.white;
    public Color hudLow = new Color(1f, 0.6f, 0.1f);
    public Color hudEmpty = new Color(1f, 0.15f, 0.1f);
    public int lowThreshold = 3;

    MeshRenderer hudRenderer;

    XRGrabInteractable grab;
    AudioSource src;
    static AudioClip shot, click;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.activated.AddListener(OnActivate);
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        if (!shot) { shot = ProcAudio.Shot(); click = ProcAudio.Click(); }
        if (flash) flash.enabled = false;
        BuildSmoke();
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
        if (!BeltItem.InHand(grab)) return;
        if (ammo <= 0) { src.PlayOneShot(click); Haptic(a.interactorObject, emptyHapticAmplitude, 0.03f); return; }

        ammo--;
        UpdateHud();
        src.PlayOneShot(shot);
        StartCoroutine(Flash());
        if (smoke) smoke.Emit(14);
        Haptic(a.interactorObject, hapticAmplitude, hapticDuration);

        if (Physics.Raycast(muzzle.position, muzzle.forward, out var hit, range, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.rigidbody) hit.rigidbody.AddForceAtPosition(muzzle.forward * 6f, hit.point, ForceMode.Impulse);
            hit.collider.SendMessage("OnShot", hit, SendMessageOptions.DontRequireReceiver);
        }
    }

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

    IEnumerator Flash()
    {
        if (!flash) yield break;
        flash.enabled = true;
        yield return new WaitForSeconds(0.04f);
        flash.enabled = false;
    }

    public void Load(int rounds)
    {
        ammo = Mathf.Min(rounds, maxAmmo);
        src.PlayOneShot(click);
        UpdateHud();
    }

    void UpdateHud()
    {
        if (!hud) return;
        hud.text = ammo.ToString("00") + "/" + maxAmmo.ToString("00");
        hud.color = ammo <= 0 ? hudEmpty : (ammo <= lowThreshold ? hudLow : hudNormal);
    }
}
