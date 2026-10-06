using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// Réglages de performance appliqués au lancement sur casque.
public class QuestPerformance : MonoBehaviour
{
    [Range(0f, 1f)] public float foveation = 1f;   // rendu fovéal : réduit la résolution en périphérie

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 72;
        Time.fixedDeltaTime = 1f / 72f;
        Invoke(nameof(ApplyFoveation), 0.5f);      // laisse le temps au sous-système XR de démarrer
    }

    void ApplyFoveation()
    {
        var displays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displays);
        foreach (var d in displays)
        {
            try { d.foveatedRenderingLevel = foveation; }
            catch (System.Exception e) { Debug.LogWarning("Foveation indisponible : " + e.Message); }
        }
    }
}
