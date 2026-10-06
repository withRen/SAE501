using System.IO;
using UnityEditor;
using UnityEngine;

// Build de développement installé sur le Quest (pour lire les logs avec adb logcat)
public static class QuestBuild
{
    [MenuItem("SAE501/Build/Quest Development (install + lancer)")]
    public static void DevBuildAndRun()
    {
        Directory.CreateDirectory("Builds");
        var opts = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainScene.unity" },
            locationPathName = "Builds/SAE501-dev.apk",
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.Development | BuildOptions.AutoRunPlayer
        };
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("[QuestBuild] résultat : " + report.summary.result);
    }

    // Build de livraison (sans lancer sur le casque) : Builds/SAE501.apk
    [MenuItem("SAE501/Build/Quest Release (APK seulement)")]
    public static void ReleaseApk()
    {
        Directory.CreateDirectory("Builds");
        var opts = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainScene.unity" },
            locationPathName = "Builds/SAE501.apk",
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("[QuestBuild] release : " + report.summary.result + " " + report.summary.totalSize);
    }
}
