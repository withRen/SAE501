using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Magazine : MonoBehaviour
{
    public int rounds = 12;

    // Rééquilibrage : le chargeur sort de l'arme avec les balles qui restaient (Pistol.Load) au lieu de disparaître
    public void Eject(Vector3 position, Vector3 velocity)
    {
        var g = GetComponent<XRGrabInteractable>();
        if (g && g.interactionManager && g.isSelected) g.interactionManager.CancelInteractableSelection((IXRSelectInteractable)g);
        transform.position = position;
        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.isKinematic = false; rb.linearVelocity = velocity; }
    }

    public void Consume()
    {
        var g = GetComponent<XRGrabInteractable>();
        if (g && g.interactionManager) g.interactionManager.CancelInteractableSelection((IXRSelectInteractable)g);
        Destroy(gameObject);
    }
}
