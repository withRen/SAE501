using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportationActivator : MonoBehaviour
{
    public XRRayInteractor teleportInteractor;
    public InputActionProperty teleportActivatorAction;

    private Coroutine deactivateCoroutine;

    private void Start()
    {
        if (teleportInteractor != null)
            teleportInteractor.gameObject.SetActive(false);

        if (teleportActivatorAction.action != null)
        {
            teleportActivatorAction.action.Enable();
            teleportActivatorAction.action.performed += Action_performed;
            teleportActivatorAction.action.canceled += Action_canceled;
        }
    }

    private void OnDestroy()
    {
        if (teleportActivatorAction.action != null)
        {
            teleportActivatorAction.action.performed -= Action_performed;
            teleportActivatorAction.action.canceled -= Action_canceled;
        }
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }

        if (teleportInteractor != null && !teleportInteractor.gameObject.activeSelf)
            teleportInteractor.gameObject.SetActive(true);
    }

    private void Action_canceled(InputAction.CallbackContext obj)
    {
        RequestDeactivation();
    }

    private void Update()
    {
        if (teleportActivatorAction.action != null && teleportActivatorAction.action.WasReleasedThisFrame())
        {
            RequestDeactivation();
        }
    }

    private void RequestDeactivation()
    {
        if (teleportInteractor != null && teleportInteractor.gameObject.activeSelf)
        {
            if (deactivateCoroutine != null)
                StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = StartCoroutine(DeactivateTeleportRay());
        }
    }

    private IEnumerator DeactivateTeleportRay()
    {
        // Wait until end of frame to let XR Interaction Manager process OnSelectExited teleport event
        yield return new WaitForEndOfFrame();
        if (teleportInteractor != null)
            teleportInteractor.gameObject.SetActive(false);
        deactivateCoroutine = null;
    }
}
