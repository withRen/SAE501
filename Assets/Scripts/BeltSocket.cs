using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Holster : n'accepte qu'un type d'objet
public class BeltSocket : XRSocketInteractor
{
    public BeltItemType accepts;

    bool Match(object o)
    {
        var c = o as Component;
        var item = c ? c.GetComponent<BeltItem>() : null;
        return item && item.type == accepts;
    }

    public override bool CanHover(IXRHoverInteractable interactable) => base.CanHover(interactable) && Match(interactable);
    public override bool CanSelect(IXRSelectInteractable interactable) => base.CanSelect(interactable) && Match(interactable);
}
