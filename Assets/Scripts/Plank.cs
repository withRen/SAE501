using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Cycle d'une planche :
//   Free    : planche normale qu'on attrape (une seule à la fois). Celles du râtelier restent accrochées tant qu'on ne les prend pas.
//   Snapped : tenue contre une ouverture, elle se cale toute seule sur l'emplacement.
//   Placed  : lâchée sur l'emplacement, elle y reste (pas besoin de la tenir). Sans clou : un coup de zombie la fait tomber.
//             On peut encore la reprendre tant qu'aucun clou n'est enfoncé.
//   Nailed  : au moins un clou enfoncé au marteau : elle ne se reprend plus, un zombie doit frapper 3 fois pour l'arracher.
public class Plank : MonoBehaviour
{
    public enum State { Free, Snapped, Placed, Nailed }

    public static Plank Held;
    public static readonly List<Plank> All = new List<Plank>();

    public Nail[] nails;
    public float unsnapRadius = 0.4f;      // si la main s'éloigne de plus de 40 cm de l'endroit où elle a calé la planche, celle-ci se décale
    public float snapDistance = 0.3f;      // tenue en main : calée dès que la planche passe à 30 cm d'un emplacement libre (sur toute sa longueur)
    public float releaseSnapDistance = 0.5f;   // lâchée à moins de 50 cm d'un emplacement libre : posée dessus, quelle que soit son orientation
    public float halfLength = 0.85f;       // demi-longueur de la planche (axe x local)
    public int hitsWhenNailed = 3;         // coups de zombie pour arracher une planche clouée (sans clou : 1 seul)
    public bool stored;                    // posée sur le râtelier : immobile jusqu'à ce qu'on la prenne
    public BarricadeSlot startSlot;        // si renseigné : la planche est déjà clouée sur cet emplacement au début de la partie

    public State state { get; private set; }
    public bool InSlot => state == State.Placed || state == State.Nailed;     // barricade en place (pas tenue en main)
    public bool CanBeNailed => state != State.Free;                           // clous accessibles au marteau
    public int Hits { get; private set; }

    XRGrabInteractable grab;
    Rigidbody rb;
    BarricadeSlot slot;
    Vector3 snapHand;                      // position de la main au moment où la planche s'est calée
    float push, pushVel;                   // recul de la planche quand un zombie frappe (ressort)
    AudioSource src;
    static AudioClip thud;

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
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        if (!thud) thud = ProcAudio.Hit();
    }

    void Start()
    {
        if (startSlot != null) PlaceFixed(startSlot);
        else if (stored) rb.isKinematic = true;
    }

    void OnDestroy() => All.Remove(this);

    int DrivenCount()
    {
        int c = 0;
        foreach (var n in nails) if (n.Driven) c++;
        return c;
    }

    // planche déjà clouée (état de départ d'une barricade)
    public void PlaceFixed(BarricadeSlot s)
    {
        TakeSlot(s);
        foreach (var n in nails) n.SetDriven();
        Settle();
    }

    // un coup de zombie : sans clou elle tombe tout de suite, clouée il faut hitsWhenNailed coups. Renvoie vrai si elle est tombée.
    public bool TakeHit(Vector3 pushDirection)
    {
        if (!InSlot) return true;
        Hits++;
        src.pitch = Random.Range(0.55f, 0.7f);
        src.PlayOneShot(thud, 0.9f);
        pushVel += 0.9f;                                   // la planche encaisse : petit recul vers l'intérieur
        if (state == State.Placed || Hits >= hitsWhenNailed) { Break(pushDirection); return true; }
        return false;
    }

    // arrachée par un zombie : la planche se détache de l'ouverture et tombe (on peut la ramasser et la reposer)
    public void Break(Vector3 pushDirection)
    {
        if (!InSlot) return;
        ReleaseSlot();
        state = State.Free;
        Hits = 0;
        stored = false;
        push = pushVel = 0f;
        foreach (var n in nails) n.ResetHits();
        grab.enabled = true;
        grab.trackPosition = grab.trackRotation = true;
        rb.isKinematic = false;
        rb.AddForce(pushDirection.normalized * 3f + Vector3.up * 1.5f, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * 2f, ForceMode.Impulse);
    }

    // une seule planche attrapable à la fois
    void OnGrab()
    {
        Held = this;
        foreach (var p in All) if (p != this && p.state == State.Free) p.grab.enabled = false;
        if (state == State.Placed) { state = State.Snapped; grab.trackPosition = grab.trackRotation = false; }   // reprise sur l'emplacement
    }

    void OnRelease()
    {
        if (Held == this) Held = null;
        foreach (var p in All) if (p.state == State.Free) p.grab.enabled = true;
        if (state == State.Free)
        {
            // lâchée près d'une ouverture libre : posée proprement sur l'emplacement le plus proche (pas besoin de viser juste)
            var near = BarricadeSlot.Best(transform, halfLength, releaseSnapDistance, 0f);
            if (near) { Snap(near); Settle(); return; }
            stored = false; rb.isKinematic = false;                                // sortie du râtelier : elle tombe normalement
        }
    }

    void Update()
    {
        switch (state)
        {
            case State.Free:
                if (Held == this)
                {
                    var s = BarricadeSlot.Best(transform, halfLength, snapDistance, 0.5f);
                    if (s) Snap(s);
                }
                break;

            case State.Snapped:
                if (!grab.isSelected) Settle();            // lâchée sur l'emplacement : elle reste
                else
                {
                    // on compare la main à sa position au moment du calage (et non au centre de l'emplacement :
                    // en tenant la planche par un bout, la main est déjà à 50 cm du centre et la planche se décrochait aussitôt)
                    var att = grab.firstInteractorSelecting.GetAttachTransform(grab);
                    if (Vector3.Distance(att.position, snapHand) > unsnapRadius) Unsnap();
                }
                break;

            case State.Placed:
                if (DrivenCount() >= 1) { state = State.Nailed; grab.enabled = false; }
                break;
        }

        bool show = Hammer.Held && CanBeNailed;
        foreach (var n in nails) n.Refresh(show);
    }

    void TakeSlot(BarricadeSlot s)
    {
        slot = s;
        s.Occupied = true;
        s.Plank = this;
        if (!rb.isKinematic) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }   // (planche du râtelier : déjà immobile)
        rb.isKinematic = true;
    }

    void ReleaseSlot()
    {
        if (slot) { slot.Occupied = false; slot.Plank = null; }
        slot = null;
    }

    void Snap(BarricadeSlot s)
    {
        slot = s;
        snapHand = grab.isSelected ? grab.firstInteractorSelecting.GetAttachTransform(grab).position : transform.position;
        s.Occupied = true;
        s.Plank = this;
        state = State.Snapped;
        grab.trackPosition = false;     // la planche ne suit plus la main : elle reste calée sur l'ouverture
        grab.trackRotation = false;
    }

    // posée sur l'emplacement, sans main : immobile ; clouée si un clou est déjà enfoncé
    void Settle()
    {
        if (!slot) return;
        TakeSlot(slot);
        grab.trackPosition = grab.trackRotation = true;
        Hits = 0;
        stored = false;
        if (DrivenCount() >= 1) { state = State.Nailed; grab.enabled = false; }
        else { state = State.Placed; grab.enabled = true; }
        foreach (var p in All) if (p.state == State.Free) p.grab.enabled = true;
    }

    void Unsnap()
    {
        ReleaseSlot();
        state = State.Free;
        grab.trackPosition = true;
        grab.trackRotation = true;
        foreach (var n in nails) n.ResetHits();
        if (!grab.isSelected) { rb.isKinematic = false; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }   // elle tombe
    }

    void LateUpdate()
    {
        if (state == State.Free || !slot) return;
        // recul quand un zombie frappe, puis retour en place (ressort amorti)
        float dt = Time.deltaTime;
        pushVel += (-120f * push - 9f * pushVel) * dt;
        push += pushVel * dt;
        var t = slot.transform;
        transform.SetPositionAndRotation(t.position + t.forward * push, t.rotation);
    }
}
