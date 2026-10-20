using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

// Version PC (clavier + souris) : activée UNIQUEMENT quand on lance la scène MainScene_PC (DesktopBoot).
// Tant que DesktopMode.On est faux (jeu au casque, build Quest), rien ne change : aucun composant n'est ajouté.
// Une fois activé, chaque scène chargée (jeu, rejouer, retour au menu) reçoit un DesktopPlayer sur son XR Origin.
public static class DesktopMode
{
    public static bool On { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        On = false;
        SceneManager.sceneLoaded -= OnLoaded;
    }

    public static void Enable()
    {
        if (On) return;
        On = true;
        SceneManager.sceneLoaded += OnLoaded;
        StopXR();
    }

    static void OnLoaded(Scene s, LoadSceneMode m)
    {
        foreach (var origin in Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None))
            if (origin.gameObject.scene == s && !origin.GetComponent<DesktopPlayer>())
                origin.gameObject.AddComponent<DesktopPlayer>();
    }

    // casque branché (Link) pendant la version PC : on coupe l'XR pour que la souris pilote la caméra
    static void StopXR()
    {
        var settings = UnityEngine.XR.Management.XRGeneralSettings.Instance;
        var manager = settings ? settings.Manager : null;
        if (manager == null || manager.activeLoader == null) return;
        manager.StopSubsystems();
        manager.DeinitializeLoader();
    }
}
