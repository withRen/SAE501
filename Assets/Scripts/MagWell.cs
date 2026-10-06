using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Emplacement du chargeur sur le pistolet
public class MagWell : XRSocketInteractor
{
    public Pistol pistol;

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
        pistol.Load(mag.rounds);
        mag.Consume();
    }

    public override bool CanHover(IXRHoverInteractable interactable) => base.CanHover(interactable) && MagOf(interactable);
    public override bool CanSelect(IXRSelectInteractable interactable) => base.CanSelect(interactable) && MagOf(interactable);
}
