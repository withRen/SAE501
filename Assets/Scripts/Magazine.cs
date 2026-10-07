using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Magazine : MonoBehaviour
{
    public int rounds = 12;

    public void Consume()
    {
        var g = GetComponent<XRGrabInteractable>();
        if (g && g.interactionManager) g.interactionManager.CancelInteractableSelection((IXRSelectInteractable)g);
        Destroy(gameObject);
    }
}
