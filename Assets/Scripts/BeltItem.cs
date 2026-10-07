using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public enum BeltItemType { Pistol, Hammer, Flashlight, Magazine }

public class BeltItem : MonoBehaviour
{
    public BeltItemType type;

    // vrai si l'objet est tenu par une main (et pas rangé dans un holster)
    public static bool InHand(XRGrabInteractable g) =>
        g != null && g.isSelected && !(g.firstInteractorSelecting is XRSocketInteractor);
}
