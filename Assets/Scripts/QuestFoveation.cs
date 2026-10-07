using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// Rendu fovéal fixe sur le Quest : la périphérie de l'image est calculée en plus basse résolution
// (invisible dans le casque, gros gain sur le GPU). Activé au lancement, sans rien dans la scène.
public static class QuestFoveation
{
    public const float Level = 1f;     // 0 = aucun, 1 = maximum

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Enable()
    {
        var displays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displays);
        foreach (var d in displays)
        {
            d.foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;   // suivi du regard s'il existe (Quest Pro), sinon fixe
            d.foveatedRenderingLevel = Level;
        }
    }
}
