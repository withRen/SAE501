using UnityEditor;
using UnityEngine;

// Méthode du cours (Dispositifs interactifs) : Meta > Tools > Project Setup Tool > Fix All / Apply All
public static class MetaSetup
{
    [MenuItem("SAE501/Meta/Rapport Project Setup (Android)")]
    public static void Report()
    {
        // écrit un fichier de rapport à la racine du projet
        OVRProjectSetupCLI.GenerateProjectSetupReport();
        Debug.Log("[MetaSetup] rapport demandé (fichier à la racine du projet)");
    }

    [MenuItem("SAE501/Meta/Fix All (Android)")]
    public static void FixAll()
    {
        _ = OVRProjectSetup.FixAllAsync(BuildTargetGroup.Android);
        Debug.Log("[MetaSetup] Fix All lancé");
    }
}
