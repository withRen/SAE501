using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MonitorScreen : MonoBehaviour
{
    public MonitorStation station;
    public int index;

    void Awake() => GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => { if (Breaker.On) station.Show(index); });   // caméras coupées : pas de plein écran
}
