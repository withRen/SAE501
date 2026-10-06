using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MonitorOverlayClose : MonoBehaviour
{
    public MonitorStation station;

    void Awake() => GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => station.Hide());
}
