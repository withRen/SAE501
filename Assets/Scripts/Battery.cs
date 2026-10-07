using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Battery : MonoBehaviour
{
    public void Consume()
    {
        var g = GetComponent<XRGrabInteractable>();
        if (g && g.interactionManager) g.interactionManager.CancelInteractableSelection((IXRSelectInteractable)g);
        Destroy(gameObject);
    }
}
