using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Arme de corps à corps (pied de biche du bunker, hache du stockage) : on la prend en main et on frappe les zombies.
// Un coup compte si l'extrémité de l'arme (striker) touche une zone du zombie (ZombieHitbox) assez vite.
// Dégâts en part des PV de base du zombie, selon la zone (comme le pistolet) ; un même zombie ne peut être touché
// qu'une fois par coup (délai entre deux coups).
// [SONS] Un « whoosh » (axe_whoosh) se déclenche à chaque GRAND geste de l'arme en main (extrémité plus rapide que whooshMinSpeed) :
// pas pour un petit mouvement. Il se réarme quand le geste est fini.
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

    [Header("Son du geste (axe_whoosh)")]
    public AudioClip whooshSound;          // sinon Assets/Resources/Sounds/axe_whoosh
    public float whooshMinSpeed = 2.8f;    // m/s de l'extrémité : en dessous, geste lent ou petit coup : pas de souffle
    public float whooshCooldown = 0.35f;
    [Range(0f, 1f)] public float whooshVolume = 0.9f;

    XRGrabInteractable grab;
    AudioSource src, whooshSrc;
    static AudioClip thud;
    bool whooshArmed = true;
    float nextWhoosh;
    Vector3 lastPos, swingDir;
    float speed;
    readonly Dictionary<Zombie, float> lastHit = new Dictionary<Zombie, float>();
    readonly Collider[] hits = new Collider[16];

    public float Speed => speed;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        whooshSrc = gameObject.AddComponent<AudioSource>();     // source à part : le pitch du « thud » ne doit pas changer le souffle
        whooshSrc.spatialBlend = 1f;
        whooshSrc.playOnAwake = false;
        if (!thud) thud = ProcAudio.Hit();
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

    // [SONS] souffle de l'arme : un seul par grand geste (armé quand le geste précédent est fini)
    void Whoosh()
    {
        if (speed < whooshMinSpeed * 0.5f) whooshArmed = true;
        if (!whooshArmed || speed < whooshMinSpeed || Time.time < nextWhoosh) return;
        var clip = AudioSlices.Clip(whooshSound, "axe_whoosh");
        if (!clip) return;
        whooshArmed = false;
        nextWhoosh = Time.time + whooshCooldown;
        float power = Mathf.InverseLerp(whooshMinSpeed, whooshMinSpeed * 2.2f, speed);   // geste plus rapide : plus fort et plus aigu
        whooshSrc.pitch = Random.Range(0.95f, 1.05f) + 0.12f * power;
        whooshSrc.PlayOneShot(clip, Mathf.Lerp(0.6f, 1f, power) * whooshVolume);
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
            src.pitch = Random.Range(0.6f, 0.8f);
            src.PlayOneShot(thud, 1f);
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
