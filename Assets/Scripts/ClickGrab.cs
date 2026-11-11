using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Prise par clic : un appui sur le bouton latéral (grip) attrape l'objet, un 2e appui le lâche.
// Plus besoin de garder le bouton enfoncé. À mettre sur les interacteurs des mains (SetupVRScene).
// Option des PARAMÈTRES « Prise : MAINTENIR » (GameSettings.HoldToGrab, au casque) : on tient tant que le grip est enfoncé.
// La version PC reste en prise par clic (DesktopPlayer prend et lâche lui-même les objets).
// Les boutons/écrans (XRSimpleInteractable) ne restent pas « tenus » : on les relâche aussitôt, sauf les leviers
// (générateur, disjoncteur : Lever) et les poignées de porte, qu'on tient pour les déplacer.
[RequireComponent(typeof(XRBaseInputInteractor))]
public class ClickGrab : MonoBehaviour
{
    XRBaseInputInteractor interactor;
    IXRSelectInteractable pending;

    void Awake()
    {
        interactor = GetComponent<XRBaseInputInteractor>();
        ApplyMode();
        interactor.selectEntered.AddListener(OnSelect);
    }

    /// <summary>Clic (Toggle) ou maintien (StateChange) selon les paramètres. Appelé aussi par GameSettings.Apply.</summary>
    public void ApplyMode()
    {
        if (!interactor) interactor = GetComponent<XRBaseInputInteractor>();     // manette encore éteinte : Awake pas encore passé
        interactor.selectActionTrigger = GameSettings.HoldToGrab && !DesktopMode.On
            ? XRBaseInputInteractor.InputTriggerType.StateChange
            : XRBaseInputInteractor.InputTriggerType.Toggle;
    }

    void OnDestroy() => interactor.selectEntered.RemoveListener(OnSelect);

    void OnSelect(SelectEnterEventArgs a)
    {
        if (a.interactableObject is XRGrabInteractable) return;
        if (a.interactableObject.transform.GetComponentInParent<Lever>()) return;       // levier (générateur, disjoncteur) : tenu tant qu'on l'abaisse
        if (a.interactableObject.transform.GetComponentInParent<HingedDoor>()) return;   // poignée de porte : tenue tant qu'on tire
        pending = a.interactableObject;                // relâché à la fin de l'image (pas pendant l'événement)
    }

    void LateUpdate()
    {
        if (pending == null) return;
        if (interactor.IsSelecting(pending)) interactor.interactionManager.SelectExit(interactor, pending);
        pending = null;
    }
}
