using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Magazine : MonoBehaviour
{
    public int rounds = 12;

    // chargeurs présents (pour MagWell : rechargement par proximité, sans parcourir la scène à chaque image)
    public static readonly System.Collections.Generic.List<Magazine> All = new System.Collections.Generic.List<Magazine>();
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

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
