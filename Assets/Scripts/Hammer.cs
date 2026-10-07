using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Hammer : MonoBehaviour
{
    public static bool Held;
    XRGrabInteractable grab;

    void Awake() => grab = GetComponent<XRGrabInteractable>();
    void Update() => Held = BeltItem.InHand(grab);
    void OnDisable() => Held = false;
}
