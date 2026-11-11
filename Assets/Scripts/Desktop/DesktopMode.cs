using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

// Version PC (clavier + souris) du jeu, dans les MÊMES scènes que la version casque (Menu, MainScene).
// Le casque reste le mode par défaut : la version PC ne s'active que si aucun casque n'est actif.
//   - Quest (Android) : toujours la version casque, rien ne change.
//   - PC : casque branché et suivi (OpenXR démarré, affichage XR en marche) → version casque ; sinon → version PC.
//   - On peut forcer le choix : argument de lancement « -pc » ou « -vr » ; dans l'éditeur, menu SAE501 > Mode de jeu.
// Une fois la version PC choisie, chaque scène chargée reçoit un DesktopPlayer sur son XR Origin (jeu) ou dans le menu
// d'accueil. Tant que DesktopMode.On est faux, aucun composant n'est ajouté : le jeu au casque est inchangé.
public static class DesktopMode
{
    public enum Choice { Auto, VR, PC }

    public static bool On { get; private set; }

    /// <summary>Clé EditorPrefs du choix de l'éditeur (SAE501 > Mode de jeu).</summary>
    public const string EditorPrefKey = "sae501.playMode";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        On = false;
        SceneManager.sceneLoaded -= OnLoaded;
    }

    // après l'initialisation d'OpenXR (au lancement), avant le chargement de la 1re scène
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Detect()
    {
        if (Application.platform == RuntimePlatform.Android) return;      // Quest : toujours le casque
        var choice = Forced();
        bool pc = choice == Choice.PC || (choice == Choice.Auto && !HeadsetActive());
        if (pc) Enable();
        Debug.Log("SAE501: mode " + (pc ? "PC (clavier + souris)" : "casque") + (choice == Choice.Auto ? " (détection automatique)" : " (forcé)"));
    }

    // 1re scène (lancement du jeu, mode Play de l'éditeur) : sceneLoaded n'est pas toujours appelé pour elle
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AttachFirstScene()
    {
        if (On) OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    /// <summary>Choix imposé : argument -pc / -vr, sinon réglage de l'éditeur.</summary>
    public static Choice Forced()
    {
        foreach (var a in System.Environment.GetCommandLineArgs())
        {
            if (string.Equals(a, "-pc", System.StringComparison.OrdinalIgnoreCase)) return Choice.PC;
            if (string.Equals(a, "-vr", System.StringComparison.OrdinalIgnoreCase)) return Choice.VR;
        }
#if UNITY_EDITOR
        return (Choice)UnityEditor.EditorPrefs.GetInt(EditorPrefKey, (int)Choice.Auto);
#else
        return Choice.Auto;
#endif
    }

    /// <summary>Vrai si un casque est vraiment là : chargeur OpenXR actif et affichage XR en marche.</summary>
    public static bool HeadsetActive()
    {
        var settings = UnityEngine.XR.Management.XRGeneralSettings.Instance;
        var manager = settings ? settings.Manager : null;
        if (manager == null || manager.activeLoader == null) return false;
        var display = manager.activeLoader.GetLoadedSubsystem<UnityEngine.XR.XRDisplaySubsystem>();
        return display != null && display.running;
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

    // casque branché mais version PC forcée (-pc) : on coupe l'XR pour que la souris pilote la caméra
    static void StopXR()
    {
        var settings = UnityEngine.XR.Management.XRGeneralSettings.Instance;
        var manager = settings ? settings.Manager : null;
        if (manager == null || manager.activeLoader == null) return;
        manager.StopSubsystems();
        manager.DeinitializeLoader();
    }

    /// <summary>Texte selon la version : casque ou PC (aides du tutoriel, des menus et des écrans).</summary>
    public static string Text(string vr, string pc) => On ? pc : vr;
}
