using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Emplacement du chargeur sur le pistolet.
// Rééquilibrage : recharger ne jette plus les balles restantes. Si l'arme n'était pas vide, le chargeur inséré ressort
// sous l'arme avec le reste (ex. 5 balles dans l'arme + chargeur de 12 : l'arme a 12, le chargeur ressort avec 5).
public class MagWell : XRSocketInteractor
{
    public Pistol pistol;
    public float ejectBlockTime = 1.5f;      // le chargeur éjecté ne peut pas être repris tout de suite par l'emplacement
    Magazine ejected;
    float ejectedUntil;

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

    void OnIn(SelectEnterEventArgs a)
    {
        var mag = MagOf(a.interactableObject);
        if (mag) StartCoroutine(Insert(mag));
    }

    IEnumerator Insert(Magazine mag)
    {
        yield return null;
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
        return m && !(m == ejected && Time.time < ejectedUntil);
    }

    public override bool CanHover(IXRHoverInteractable interactable) => base.CanHover(interactable) && Allowed(interactable);
    public override bool CanSelect(IXRSelectInteractable interactable) => base.CanSelect(interactable) && Allowed(interactable);
}
