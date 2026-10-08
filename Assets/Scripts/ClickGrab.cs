using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Prise par clic : un appui sur le bouton latéral (grip) attrape l'objet, un 2e appui le lâche.
// Plus besoin de garder le bouton enfoncé. À mettre sur les interacteurs des mains (SetupVRScene).
// Les boutons/écrans (XRSimpleInteractable) ne restent pas « tenus » : on les relâche aussitôt, sauf l'interrupteur
// du générateur, qui reste actionné tant que la main est dessus (voir Generator).
[RequireComponent(typeof(XRBaseInputInteractor))]
public class ClickGrab : MonoBehaviour
{
    XRBaseInputInteractor interactor;
    IXRSelectInteractable pending;

    void Awake()
    {
        interactor = GetComponent<XRBaseInputInteractor>();
        interactor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.Toggle;
        interactor.selectEntered.AddListener(OnSelect);
    }

    void OnDestroy() => interactor.selectEntered.RemoveListener(OnSelect);

    void OnSelect(SelectEnterEventArgs a)
    {
        if (a.interactableObject is XRGrabInteractable) return;
        if (a.interactableObject.transform.GetComponentInParent<Generator>()) return;
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
