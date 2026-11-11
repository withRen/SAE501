using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Garde-fou : recrée le dossier Temp du projet s'il disparaît pendant que l'éditeur tourne.
/// Sans lui, Unity affiche « Opening file failed: …/Temp/UnityTempFile-… : No such file or directory ».
/// Cause constatée le 2026-11-11 : l'option « Quit on close » de l'utilitaire Vorssaint fermait
/// les processus Unity sans fenêtre (workers d'import), qui suppriment Temp en quittant.
/// Vérifie toutes les 3 secondes, ne supprime rien.
/// </summary>
[InitializeOnLoad]
static class TempDirGuard
{
    const double IntervalSeconds = 3.0;
    static double _next;

    static TempDirGuard()
    {
        EditorApplication.update += Check;
    }

    static void Check()
    {
        double now = EditorApplication.timeSinceStartup;
        if (now < _next) return;
        _next = now + IntervalSeconds;

        string temp = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Temp");
        if (Directory.Exists(temp)) return;
        try
        {
            Directory.CreateDirectory(temp);
            Debug.LogWarning("[TempDirGuard] Dossier Temp absent : recréé (" + temp + ").");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[TempDirGuard] Impossible de recréer Temp : " + e.Message);
        }
    }
}
