using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Cycle d'une planche :
//   Free    : planche normale qu'on attrape (une seule à la fois)
//   Snapped : tenue contre une ouverture, elle se cale toute seule ; il faut la TENIR. Clous visibles avec le marteau.
//             Lâchée sans clou enfoncé -> elle tombe. Lâchée avec au moins 1 clou -> Nailed.
//   Nailed  : 1 clou enfoncé : elle PEND par ce clou (sauf si on la tient avec l'autre main). 2 clous -> Fixed.
//   Fixed   : solidement clouée.
public class Plank : MonoBehaviour
{
    public enum State { Free, Snapped, Nailed, Fixed }

    public static Plank Held;
    public static readonly List<Plank> All = new List<Plank>();

    public XRSimpleInteractable holdZone;
    public Nail[] nails;
    public float unsnapRadius = 0.35f;     // si la main s'éloigne de plus de 35 cm de l'ouverture, la planche se décale
    public BarricadeSlot startSlot;        // si renseigné : la planche est déjà clouée sur cet emplacement au début de la partie

    public State state { get; private set; }
    public bool Placed => state != State.Free;

    // vrai si la planche est maintenue en place par une main (la main qui la tient, ou l'autre main via la zone de maintien)
    public bool IsHeldByHand =>
        (state == State.Snapped && grab.isSelected)
        || (holdZone && holdZone.gameObject.activeSelf && holdZone.isSelected);

    XRGrabInteractable grab;
    Rigidbody rb;
    BarricadeSlot slot;
    float angle, vel;
    Vector3 pivot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Held = null; All.Clear(); }

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        All.Add(this);
        grab.selectEntered.AddListener(_ => OnGrab());
        grab.selectExited.AddListener(_ => OnRelease());
        foreach (var n in nails) { n.plank = this; n.Refresh(false); }
        holdZone.gameObject.SetActive(false);
    }

    void Start()
    {
        if (startSlot != null) PlaceFixed(startSlot);
    }

    void OnDestroy() => All.Remove(this);

    // ---------- [4.4] dégradation : un zombie frappe la planche ----------
    // Planche clouée de 2 clous : 1er coup = elle tremble ; 2e coup = un clou saute, elle pend par l'autre ; 3e coup = elle tombe.
    // Planche tenue par 1 seul clou : 2 coups suffisent. Si le joueur recloue le clou sauté avant, les dégâts repartent de zéro.
    int damage;
    bool popped;                           // un clou a sauté (planche réparable en le reclouant)
    public int Damage => damage;

    /// <summary>Un coup de zombie. Renvoie vrai si la planche est tombée.</summary>
    public bool Hit(Vector3 inward)
    {
        if (state != State.Nailed && state != State.Fixed) return false;
        int driven = DrivenCount();
        if (popped && driven >= 2) { damage = 0; popped = false; }      // réparée par le joueur entre deux coups
        damage++;
        vel += (Random.value < 0.5f ? -1f : 1f) * (25f + 15f * damage);  // elle tremble (ressort de LateUpdate)

        if (damage >= driven + 1)
        {
            BarricadeFx.Play(BarricadeFx.Break, transform.position);
            damage = 0; popped = false;
            Break(inward);
            return true;
        }
        if (state == State.Fixed && damage >= 2)
        {
            // un clou saute : la planche pend par l'autre clou
            foreach (var n in nails) if (n.Driven) { n.Pop(); break; }
            state = State.Nailed;
            popped = true;
            BarricadeFx.Play(BarricadeFx.Pop, transform.position);
        }
        else BarricadeFx.Play(BarricadeFx.Crack, transform.position);
        return false;
    }

    // planche déjà clouée (état de départ d'une barricade)
    public void PlaceFixed(BarricadeSlot s)
    {
        slot = s;
        s.Occupied = true;
        s.Plank = this;
        foreach (var n in nails) n.SetDriven();
        state = State.Fixed;
        grab.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.SetPositionAndRotation(s.transform.position, s.transform.rotation);
        holdZone.gameObject.SetActive(true);
        angle = vel = 0f;
        pivot = s.transform.position;
    }

    // arrachée par un zombie : la planche se détache de l'ouverture et tombe (on peut la ramasser et la reposer)
    public void Break(Vector3 pushDirection)
    {
        if (state != State.Nailed && state != State.Fixed) return;
        if (slot) { slot.Occupied = false; slot.Plank = null; }
        slot = null;
        state = State.Free;
        foreach (var n in nails) n.ResetHits();
        holdZone.gameObject.SetActive(false);
        grab.enabled = true;
        rb.isKinematic = false;
        rb.AddForce(pushDirection.normalized * 1.2f + Vector3.up * 0.8f, ForceMode.Impulse);   // elle tombe vers l'intérieur sans voler à travers la pièce
        rb.AddTorque(Random.onUnitSphere * 0.6f, ForceMode.Impulse);
    }

    // une seule planche attrapable à la fois
    void OnGrab()
    {
        Held = this;
        foreach (var p in All) if (p != this && p.state == State.Free) p.grab.enabled = false;
    }

    void OnRelease()
    {
        if (Held == this) Held = null;
        foreach (var p in All) if (p.state == State.Free) p.grab.enabled = true;
    }

    int DrivenCount()
    {
        int c = 0;
        foreach (var n in nails) if (n.Driven) c++;
        return c;
    }

    void Update()
    {
        switch (state)
        {
            case State.Free:
                if (Held == this)
                    foreach (var s in BarricadeSlot.All)
                        if (s.Accepts(transform)) { Snap(s); break; }
                break;

            case State.Snapped:
                if (!grab.isSelected)
                {
                    // lâchée : sans clou elle tombe, avec au moins un clou elle reste accrochée
                    if (DrivenCount() >= 1) EnterNailed(); else Unsnap();
                }
                else
                {
                    var att = grab.firstInteractorSelecting.GetAttachTransform(grab);
                    if (Vector3.Distance(att.position, slot.transform.position) > unsnapRadius) Unsnap();
                }
                break;

            case State.Nailed:
                if (DrivenCount() >= 2) state = State.Fixed;
                break;
        }

        bool show = Hammer.Held && (state == State.Snapped || state == State.Nailed);
        foreach (var n in nails) n.Refresh(show);
    }

    void Snap(BarricadeSlot s)
    {
        slot = s;
        s.Occupied = true;
        s.Plank = this;
        state = State.Snapped;
        grab.trackPosition = false;     // la planche ne suit plus la main : elle reste calée sur l'ouverture
        grab.trackRotation = false;
        angle = vel = 0f;
        pivot = s.transform.position;
    }

    void Unsnap()
    {
        if (slot) { slot.Occupied = false; slot.Plank = null; }
        slot = null;
        state = State.Free;
        grab.trackPosition = true;
        grab.trackRotation = true;
        foreach (var n in nails) n.ResetHits();
        if (!grab.isSelected) { rb.isKinematic = false; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }   // elle tombe
    }

    void EnterNailed()
    {
        state = State.Nailed;
        grab.trackPosition = true;
        grab.trackRotation = true;
        grab.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        holdZone.gameObject.SetActive(true);
        foreach (var p in All) if (p.state == State.Free) p.grab.enabled = true;
    }

    void LateUpdate()
    {
        if (state == State.Snapped)
        {
            transform.SetPositionAndRotation(slot.transform.position, slot.transform.rotation);
            return;
        }
        if (state != State.Nailed && state != State.Fixed) return;

        Vector3 slotPos = slot.transform.position;
        Quaternion slotRot = slot.transform.rotation;
        Vector3 axis = slotRot * Vector3.forward;

        int driven = DrivenCount();
        Nail dn = null;
        foreach (var n in nails) if (n.Driven) dn = n;

        float target = 0f;
        if (driven == 1)
        {
            pivot = slotPos + slotRot * new Vector3(dn.transform.localPosition.x, 0f, 0f);
            // un seul clou : la planche pend par ce clou (sauf si on la tient)
            if (!IsHeldByHand) target = Vector3.SignedAngle(slotPos - pivot, Vector3.down, axis);
        }

        float dt = Time.deltaTime;
        vel += (-40f * (angle - target) - 2.5f * vel) * dt;
        angle += vel * dt;

        var rot = Quaternion.AngleAxis(angle, axis);
        transform.SetPositionAndRotation(pivot + rot * (slotPos - pivot), rot * slotRot);
    }
}
