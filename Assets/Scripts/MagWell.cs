using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Emplacement du chargeur sur le pistolet.
// Rééquilibrage : recharger ne jette plus les balles restantes. Si l'arme n'était pas vide, le chargeur inséré ressort
// sous l'arme avec le reste (ex. 5 balles dans l'arme + chargeur de 12 : l'arme a 12, le chargeur ressort avec 5).
// Rechargement facilité : en plus du socket XRI (qui ratait le chargeur quand la main passait vite, le chargeur
// traversait alors l'arme sans rien faire), le puits regarde à chaque image les chargeurs tenus en main. Si l'un d'eux
// passe à moins de `grabRadius` du puits pendant que le pistolet est en main (même entre deux images, trajet compris),
// il est inséré tout de suite, quelle que soit son orientation.
public class MagWell : XRSocketInteractor
{
    public Pistol pistol;
    public float ejectBlockTime = 1.5f;      // le chargeur éjecté ne peut pas être repris tout de suite par l'emplacement
    [Tooltip("Distance (m) entre le chargeur tenu et le puits sous la crosse qui suffit pour recharger.")]
    public float grabRadius = 0.09f;
    Magazine ejected, inserting;
    float ejectedUntil;
    XRGrabInteractable pistolGrab;
    readonly System.Collections.Generic.Dictionary<Magazine, Vector3> last = new System.Collections.Generic.Dictionary<Magazine, Vector3>();

    static Magazine MagOf(object o)
    {
        var c = o as Component;
        return c ? c.GetComponent<Magazine>() : null;
    }

    protected override void Awake()
    {
        base.Awake();
        selectEntered.AddListener(OnIn);
    }

    protected override void Start()
    {
        base.Start();
        if (pistol) pistolGrab = pistol.GetComponent<XRGrabInteractable>();
    }

    void LateUpdate()
    {
        // arme pleine : rien à faire (sinon le chargeur ressortirait aussitôt)
        if (!pistol || !BeltItem.InHand(pistolGrab) || hasSelection || inserting || pistol.ammo >= pistol.maxAmmo) { last.Clear(); return; }
        var well = transform.position;
        Magazine hit = null;
        foreach (var m in Magazine.All)
        {
            if (!m) continue;
            var g = m.GetComponent<XRGrabInteractable>();
            if (!BeltItem.InHand(g) || !Allowed(m)) continue;
            var c = Center(m);
            var prev = last.TryGetValue(m, out var p) ? p : c;
            last[m] = c;
            if (hit == null && DistanceToSegment(well, prev, c) <= grabRadius) hit = m;
        }
        if (hit == null) return;
        last.Remove(hit);
        StartCoroutine(Insert(hit));
    }

    // centre du chargeur : milieu de ses colliders (son origine est en bas du chargeur)
    static Vector3 Center(Magazine m)
    {
        var col = m.GetComponent<Collider>();
        return col ? col.bounds.center : m.transform.position;
    }

    static float DistanceToSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        float t = ab.sqrMagnitude < 1e-8f ? 0f : Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector3.Distance(p, a + ab * t);
    }

    void OnIn(SelectEnterEventArgs a)
    {
        var mag = MagOf(a.interactableObject);
        if (mag) StartCoroutine(Insert(mag));
    }

    IEnumerator Insert(Magazine mag)
    {
        inserting = mag;
        yield return null;
        inserting = null;
        if (!mag) yield break;
        int left = pistol.Load(mag.rounds);
        if (left <= 0) { mag.Consume(); yield break; }
        mag.rounds = left;
        ejected = mag; ejectedUntil = Time.time + ejectBlockTime;
        var at = (attachTransform ? attachTransform : transform).position;
        mag.Eject(at + Vector3.down * 0.15f, Vector3.down * 0.5f);
    }

    bool Allowed(object o)
    {
        var m = MagOf(o);
        return m && m != inserting && !(m == ejected && Time.time < ejectedUntil);
    }

    public override bool CanHover(IXRHoverInteractable interactable) => base.CanHover(interactable) && Allowed(interactable);
    public override bool CanSelect(IXRSelectInteractable interactable) => base.CanSelect(interactable) && Allowed(interactable);
}
