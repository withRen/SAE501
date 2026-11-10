using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Arme de corps à corps (pied de biche du bunker, hache du stockage) : on la prend en main et on frappe les zombies.
// Un coup compte si l'extrémité de l'arme (striker) touche une zone du zombie (ZombieHitbox) assez vite.
// Dégâts en part des PV de base du zombie, selon la zone (comme le pistolet) ; un même zombie ne peut être touché
// qu'une fois par coup (délai entre deux coups).
// [SONS, PR #151] Le souffle du geste est le vrai son « axe_whoosh » : un seul par GRAND geste (extrémité plus rapide que
// whooshMinSpeed), réarmé quand le geste est fini. Sans le fichier : l'ancien souffle procédural, au seuil d'avant.
public class MeleeWeapon : MonoBehaviour
{
    [Header("Dégâts (part des PV de base du zombie)")]
    public float headDamage = 0.5f;
    public float torsoDamage = 0.25f;
    public float legsDamage = 0.15f;

    [Header("Coup")]
    public Transform striker;              // extrémité qui frappe (tête de la hache, crochet du pied de biche)
    public Vector3 strikerSize = new Vector3(0.08f, 0.12f, 0.08f);
    public float minSpeed = 1.6f;          // m/s : en dessous, ce n'est pas un coup (on pose l'arme contre le zombie)
    public float cooldown = 0.45f;         // délai avant de pouvoir retoucher le même zombie

    [Header("Son du geste (axe_whoosh) [SONS]")]
    public AudioClip whooshSound;          // sinon Assets/Resources/Sounds/axe_whoosh
    public float whooshMinSpeed = 2.8f;    // m/s de l'extrémité : en dessous, geste lent ou petit coup : pas de souffle
    public float whooshCooldown = 0.35f;
    [Range(0f, 1f)] public float whooshVolume = 0.9f;

    XRGrabInteractable grab;
    AudioSource src, whooshSrc;            // source à part pour le souffle : le pitch de l'impact ne doit pas le changer
    bool whooshArmed = true;
    static AudioClip swingClip, axeClip, crowbarClip;
    Vector3 lastPos, swingDir;
    float speed, lastSwingAt = -10f;
    bool isAxe;
    readonly Dictionary<Zombie, float> lastHit = new Dictionary<Zombie, float>();
    readonly Collider[] hits = new Collider[16];

    public float Speed => speed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => swingClip = axeClip = crowbarClip = null;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = DesktopMode.On ? 0.25f : 0.65f;
        src.minDistance = 0.8f;
        src.maxDistance = 14f;
        src.priority = 36;
        whooshSrc = gameObject.AddComponent<AudioSource>();
        whooshSrc.playOnAwake = false;
        whooshSrc.minDistance = 0.8f; whooshSrc.maxDistance = 14f; whooshSrc.priority = 36;
        isAxe = headDamage >= 0.9f || name.IndexOf("Hache", System.StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Axe", System.StringComparison.OrdinalIgnoreCase) >= 0;
        if (!swingClip) { swingClip = ProcAudio.MeleeSwing(); axeClip = ProcAudio.AxeImpact(); crowbarClip = ProcAudio.CrowbarImpact(); }
    }

    void Start() { if (striker) lastPos = striker.position; }

    void Update()
    {
        if (!striker) return;
        if (PauseMenu.Paused) { lastPos = striker.position; speed = 0f; return; }   // pause : temps figé, pas de coup
        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        var p = striker.position;
        speed = Mathf.Max((p - lastPos).magnitude / dt, speed * 0.8f);   // vitesse lissée : reste « chaude » le temps du contact
        if ((p - lastPos).sqrMagnitude > 1e-8f) swingDir = p - lastPos;
        lastPos = p;
        bool held = BeltItem.InHand(grab);
        if (held) Whoosh();
        if (!held || speed < minSpeed) return;
        Strike();
    }

    // souffle de l'arme : axe_whoosh, un seul par grand geste (plus fort et plus aigu si le geste est rapide)
    void Whoosh()
    {
        var clip = AudioSlices.Clip(whooshSound, "axe_whoosh");
        if (!clip)
        {
            // ancien souffle procédural
            if (speed < minSpeed * 1.1f || Time.time - lastSwingAt < 0.24f) return;
            lastSwingAt = Time.time;
            src.spatialBlend = DesktopMode.On ? 0.25f : 0.65f;
            src.pitch = isAxe ? Random.Range(0.88f, 0.96f) : Random.Range(1.06f, 1.16f);
            src.PlayOneShot(swingClip, isAxe ? 0.60f : 0.52f);
            Haptic(0.18f, 0.04f);
            return;
        }
        if (speed < whooshMinSpeed * 0.5f) whooshArmed = true;
        if (!whooshArmed || speed < whooshMinSpeed || Time.time - lastSwingAt < whooshCooldown) return;
        whooshArmed = false;
        lastSwingAt = Time.time;
        float power = Mathf.InverseLerp(whooshMinSpeed, whooshMinSpeed * 2.2f, speed);
        whooshSrc.spatialBlend = DesktopMode.On ? 0.25f : 0.65f;
        whooshSrc.pitch = Random.Range(0.95f, 1.05f) + 0.12f * power + (isAxe ? -0.05f : 0.05f);
        whooshSrc.PlayOneShot(clip, Mathf.Lerp(0.6f, 1f, power) * whooshVolume);
        Haptic(0.18f, 0.04f);
    }

    // cherche les zones de zombie touchées par l'extrémité de l'arme ; public pour être testé sans casque
    public int Strike()
    {
        int n = Physics.OverlapBoxNonAlloc(striker.position, strikerSize * 0.5f, hits, striker.rotation, ~0, QueryTriggerInteraction.Collide);
        int done = 0;
        for (int i = 0; i < n; i++)
        {
            var hb = hits[i].GetComponent<ZombieHitbox>();
            if (hb == null || hb.zombie == null || hb.zombie.Dead) continue;
            if (lastHit.TryGetValue(hb.zombie, out var t) && Time.time - t < cooldown) continue;
            lastHit[hb.zombie] = Time.time;
            float dmg = hb.zone == HitZone.Head ? headDamage : hb.zone == HitZone.Torso ? torsoDamage : legsDamage;
            hb.zombie.HitFeedback(hits[i].ClosestPoint(striker.position), swingDir, hb.zone == HitZone.Head);   // [#113]
            hb.zombie.OnMeleeHit(hb.zone, dmg);
            src.spatialBlend = 1f;
            src.pitch = isAxe ? Random.Range(0.94f, 1.06f) : Random.Range(0.96f, 1.04f);
            src.PlayOneShot(isAxe ? axeClip : crowbarClip, 0.82f);
            Haptic(0.8f, 0.1f);
            done++;
        }
        return done;
    }

    void Haptic(float amplitude, float duration)
    {
        var who = grab.firstInteractorSelecting as Component;
        if (who == null) return;
        bool left = false;
        for (var c = who.transform; c != null; c = c.parent)
            if (c.name.IndexOf("left", System.StringComparison.OrdinalIgnoreCase) >= 0) { left = true; break; }
            else if (c.name.IndexOf("right", System.StringComparison.OrdinalIgnoreCase) >= 0) break;
        var dev = InputDevices.GetDeviceAtXRNode(left ? XRNode.LeftHand : XRNode.RightHand);
        if (dev.isValid) dev.SendHapticImpulse(0, amplitude, duration);
    }
}
