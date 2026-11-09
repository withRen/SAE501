using UnityEditor;
using UnityEditor.SceneManagement;

// Menu SAE501 > Mode de jeu : choix de la version lancée par le mode Play de l'éditeur (DesktopMode).
//   - Automatique (défaut) : casque branché et suivi → version casque ; sinon → version PC (clavier + souris).
//   - Casque : toujours la version casque (comme avant : clavier de secours de l'éditeur, Espace = gâchette).
//   - PC : toujours la version PC, même avec un casque branché (OpenXR est alors coupé).
// SAE501 > Jouer en version PC : ouvre la scène du menu d'accueil et lance le mode Play en version PC.
// Dans un build Windows / macOS : argument de lancement « -pc » ou « -vr » (sinon détection automatique).
public static class DesktopModeMenu
{
    const string Root = "SAE501/Mode de jeu/";

    [MenuItem(Root + "Automatique (casque s'il est branché)", false, 300)] static void Auto() => Set(DesktopMode.Choice.Auto);
    [MenuItem(Root + "Casque (VR)", false, 301)] static void Vr() => Set(DesktopMode.Choice.VR);
    [MenuItem(Root + "PC (clavier + souris)", false, 302)] static void Pc() => Set(DesktopMode.Choice.PC);

    [MenuItem(Root + "Automatique (casque s'il est branché)", true)] static bool AutoCheck() => Check(DesktopMode.Choice.Auto);
    [MenuItem(Root + "Casque (VR)", true)] static bool VrCheck() => Check(DesktopMode.Choice.VR);
    [MenuItem(Root + "PC (clavier + souris)", true)] static bool PcCheck() => Check(DesktopMode.Choice.PC);

    static void Set(DesktopMode.Choice c) => EditorPrefs.SetInt(DesktopMode.EditorPrefKey, (int)c);

    static bool Check(DesktopMode.Choice c)
    {
        Menu.SetChecked(Root + "Automatique (casque s'il est branché)", Current == DesktopMode.Choice.Auto);
        Menu.SetChecked(Root + "Casque (VR)", Current == DesktopMode.Choice.VR);
        Menu.SetChecked(Root + "PC (clavier + souris)", Current == DesktopMode.Choice.PC);
        return !EditorApplication.isPlaying;
    }

    static DesktopMode.Choice Current => (DesktopMode.Choice)EditorPrefs.GetInt(DesktopMode.EditorPrefKey, (int)DesktopMode.Choice.Auto);

    [MenuItem("SAE501/Jouer en version PC", false, 310)]
    public static void PlayPc()
    {
        if (EditorApplication.isPlaying) return;
        Set(DesktopMode.Choice.PC);
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (System.IO.File.Exists(MenuBuilder.ScenePath)) EditorSceneManager.OpenScene(MenuBuilder.ScenePath);
        EditorApplication.isPlaying = true;
    }
}
