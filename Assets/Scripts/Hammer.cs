using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Hammer : MonoBehaviour
{
    public static bool Held;
    public static XRNode Hand = XRNode.RightHand;     // [#112] main qui tient le marteau : c'est elle qui vibre à chaque coup
    XRGrabInteractable grab;

    void Awake() => grab = GetComponent<XRGrabInteractable>();

    void Update()
    {
        Held = BeltItem.InHand(grab);
        if (Held && grab.firstInteractorSelecting is Component c) Hand = VRInput.HandOf(c);
    }

    void OnDisable() => Held = false;
}
