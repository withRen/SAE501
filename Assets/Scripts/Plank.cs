using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Plank : MonoBehaviour
{
    public static Plank Held;
    public static readonly List<Plank> All = new List<Plank>();

    public XRSimpleInteractable holdZone;
    public Nail[] nails;

    public bool Placed { get; private set; }
    public bool IsHeldByHand => holdZone && holdZone.isSelected;

    XRGrabInteractable grab;
    Rigidbody rb;
    BarricadeSlot slot;
    bool locked;
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

    void OnDestroy() => All.Remove(this);

    // une seule planche attrapable à la fois
    void OnGrab()
    {
        Held = this;
        foreach (var p in All) if (p != this && !p.Placed) p.grab.enabled = false;
    }

    void OnRelease()
    {
        if (Held == this) Held = null;
        foreach (var p in All) if (!p.Placed) p.grab.enabled = true;
    }

    void Update()
    {
        if (!Placed)
        {
            if (Held != this) return;
            foreach (var s in BarricadeSlot.All)
                if (s.Accepts(transform)) { Place(s); break; }
            return;
        }
        foreach (var n in nails) n.Refresh(Hammer.Held);
    }

    void Place(BarricadeSlot s)
    {
        slot = s;
        s.Occupied = true;
        Placed = true;
        pivot = s.transform.position;
        if (grab.interactionManager) grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);
        StartCoroutine(Lock());
    }

    IEnumerator Lock()
    {
        yield return null;
        yield return null;
        if (grab.isSelected && grab.interactionManager) grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);
        grab.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.SetPositionAndRotation(slot.transform.position, slot.transform.rotation);
        holdZone.gameObject.SetActive(true);
        locked = true;
    }

    void LateUpdate()
    {
        if (!locked) return;

        Vector3 slotPos = slot.transform.position;
        Quaternion slotRot = slot.transform.rotation;
        Vector3 axis = slotRot * Vector3.forward;

        int driven = 0;
        Nail dn = null;
        foreach (var n in nails) if (n.Driven) { driven++; dn = n; }

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
