using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Porte sur charnière (les 4 portes de niveau) : sur l'objet charnière, le battant en enfant.
// - verrouillée (ZoneGate, niveau pas encore atteint) : la poignée ne répond pas ;
// - déverrouillée : on attrape la poignée (grip, prise par clic) et on TIRE : la porte suit la main autour de la charnière ;
//   on la lâche (grip à nouveau, ou main trop loin) et elle reste où elle est ;
// - barricadable (portes ouest / est du bunker) : tant qu'une planche est posée sur son ouverture, elle ne s'ouvre plus ;
// - un zombie devant la porte fermée la frappe : au bout de hitsToBreak coups elle cède (Zombie, Opening.door).
// Le battant découpe le NavMesh (NavMeshObstacle) : fermée, les zombies ne passent pas à travers.
public class HingedDoor : MonoBehaviour
{
    public Transform leaf;                 // battant
    public XRSimpleInteractable handle;    // poignée, côté où le joueur arrive
    public Opening opening;                // emplacements de planches de cette porte (portes du bunker), sinon vide
    public float openSign = 1f;            // sens d'ouverture (vers le côté de la poignée)
    public float maxAngle = 100f;
    public int hitsToBreak = 3;
    public bool locked = true;

    public float Angle { get; private set; }
    public bool Closed => Angle < 20f;

    Quaternion rest;
    Vector3 closedDir;                     // de la charnière vers le bout du battant, porte fermée (à plat)
    float forced = -1f;                    // ouverture imposée par un zombie (animée)
    int hits;
    AudioSource src;
    static AudioClip thud, creak;

    void Awake()
    {
        rest = transform.localRotation;
        var tip = leaf.TransformPoint(Vector3.zero) - transform.position; tip.y = 0f;
        closedDir = tip.normalized;
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        if (!thud) { thud = ProcAudio.Hit(); creak = ProcAudio.Click(); }
    }

    bool Barricaded => opening != null && opening.IsBarricaded;

    void Update()
    {
        if (handle) handle.enabled = !locked;
        if (handle && handle.isSelected && !locked && forced < 0f)
        {
            var hand = handle.firstInteractorSelecting.GetAttachTransform(handle).position;
            // main trop loin de la poignée : on lâche
            if (Vector3.Distance(hand, handle.transform.position) > 0.6f) { handle.interactionManager.SelectExit(handle.firstInteractorSelecting, handle); return; }
            var v = hand - transform.position; v.y = 0f;
            if (v.sqrMagnitude > 0.01f)
            {
                float a = Vector3.SignedAngle(closedDir, v.normalized, Vector3.up) * openSign;
                float max = Barricaded ? 0f : maxAngle;          // planches posées : bloquée
                Angle = Mathf.Clamp(a, 0f, max);
            }
        }
        else if (forced >= 0f) Angle = Mathf.MoveTowards(Angle, forced, 140f * Time.deltaTime);
        transform.localRotation = rest * Quaternion.Euler(0f, Angle * openSign, 0f);
    }

    // coup de zombie sur la porte fermée
    public void ZombieHit()
    {
        hits++;
        src.pitch = Random.Range(0.5f, 0.65f);
        src.PlayOneShot(thud, 1f);
        if (hits >= hitsToBreak) ForceOpen();
        else Angle = Mathf.Min(Angle + 4f, 8f);              // elle bouge un peu sous le choc
    }

    // la porte cède (zombie) : grande ouverte, ou entrouverte tant que des planches la bloquent
    public void ForceOpen()
    {
        locked = false;
        forced = Barricaded ? 18f : maxAngle;
        src.pitch = 0.6f;
        src.PlayOneShot(creak, 0.8f);
    }

    void LateUpdate()
    {
        // entrouverte par un zombie puis planches arrachées : elle finit de s'ouvrir
        if (forced >= 0f && forced < maxAngle && !Barricaded) forced = maxAngle;
    }
}
