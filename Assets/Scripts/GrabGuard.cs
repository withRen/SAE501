using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Règles de prise pour tous les objets attrapables (ajouté par RoomBuilder.MakeGrab) :
//   1. Portée : une main ne peut attraper un objet que s'il est à moins de `maxReach` de sa paume (pas de prise de loin).
//   2. Pas de vol : un objet tenu ne peut pas être arraché par l'autre main, ni aspiré par un holster de la ceinture
//      quand la main passe à côté (on le range en le LÂCHANT près du holster).
//   (La « prise ferme » du pistolet a été retirée : la prise par clic, ClickGrab, garde déjà l'objet en main.)
[RequireComponent(typeof(XRGrabInteractable))]
public class GrabGuard : MonoBehaviour, IXRSelectFilter
{
    public float maxReach = 0.2f;

    XRGrabInteractable grab;

    public bool canProcess => isActiveAndEnabled;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectFilters.Add(this);
    }

    void OnDestroy()
    {
        if (!grab) return;
        grab.selectFilters.Remove(this);
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        if (interactable.interactorsSelecting.Contains(interactor)) return true;          // celui qui tient déjà l'objet le garde

        bool interactorIsSocket = interactor is XRSocketInteractor;
        if (interactable.isSelected)
        {
            bool heldByHand = !(interactable.firstInteractorSelecting is XRSocketInteractor);
            if (heldByHand && (!interactorIsSocket || interactor is BeltSocket)) return false;   // ni l'autre main, ni un holster
        }
        if (interactorIsSocket) return true;                              // holster / puits de chargeur : règles d'XRI

        // une main : l'objet doit être à portée de la paume
        var palm = interactor.GetAttachTransform(interactable).position;
        return DistanceTo(palm) <= maxReach;
    }

    float DistanceTo(Vector3 p)
    {
        float best = float.MaxValue;
        var cols = grab.colliders;
        if (cols != null && cols.Count > 0)
        {
            foreach (var c in cols) if (c && c.enabled) best = Mathf.Min(best, Vector3.Distance(p, Closest(c, p)));
        }
        else
        {
            foreach (var c in GetComponentsInChildren<Collider>()) if (c.enabled && !c.isTrigger) best = Mathf.Min(best, Vector3.Distance(p, Closest(c, p)));
        }
        return best == float.MaxValue ? Vector3.Distance(p, transform.position) : best;
    }

    // ClosestPoint n'accepte pas les MeshCollider non convexes : on prend alors la boîte englobante
    static Vector3 Closest(Collider c, Vector3 p) =>
        c is MeshCollider mc && !mc.convex ? c.bounds.ClosestPoint(p) : c.ClosestPoint(p);
}
