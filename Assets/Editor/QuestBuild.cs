using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
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

    // Version publiée (release GitHub) : à augmenter à chaque nouvelle release
    public const string Version = "0.9.0";
    public const int VersionCode = 2;

    const string IconPath = "Assets/Branding/AppIcon_1024.png";
    const string AdaptiveBackPath = "Assets/Branding/AppIcon_Adaptive_Fond.png";
    const string AdaptiveFrontPath = "Assets/Branding/AppIcon_Adaptive_Avant.png";

    // Build de livraison (sans lancer sur le casque) : Builds/TheLastEscapor-v<version>.apk
    // Toutes les scènes des Build Settings (Menu puis MainScene), nom, version et icônes appliqués avant.
    [MenuItem("SAE501/Build/Quest Release (APK seulement)")]
    public static void ReleaseApk()
    {
        ApplyIdentity();
        Directory.CreateDirectory("Builds");
        var opts = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = "Builds/TheLastEscapor-v" + Version + ".apk",
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("[QuestBuild] release : " + report.summary.result + " " + report.summary.totalSize);
        if (Application.isBatchMode)
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    // Nom de l'appli, version et icônes (icône par défaut + icônes Android adaptative, ronde et classique)
    [MenuItem("SAE501/Build/Appliquer nom, version et icônes")]
    public static void ApplyIdentity()
    {
        PlayerSettings.productName = "The Last Escapor";
        PlayerSettings.bundleVersion = Version;
        PlayerSettings.Android.bundleVersionCode = VersionCode;

        var icon = LoadIcon(IconPath);
        var back = LoadIcon(AdaptiveBackPath);
        var front = LoadIcon(AdaptiveFrontPath);
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);

        var android = NamedBuildTarget.Android;
        foreach (var kind in PlayerSettings.GetSupportedIconKinds(android))
        {
            var icons = PlayerSettings.GetPlatformIcons(android, kind);
            foreach (var i in icons)
            {
                if (kind == AndroidPlatformIconKind.Adaptive) i.SetTextures(back, front);
                else i.SetTexture(icon);
            }
            PlayerSettings.SetPlatformIcons(android, kind, icons);
        }
        // Le profil de build actif (Meta Quest) a ses propres Player Settings : c'est lui qu'il faut enregistrer
        var profile = UnityEditor.Build.Profile.BuildProfile.GetActiveBuildProfile();
        Debug.Log("[QuestBuild] profil actif : " + (profile ? AssetDatabase.GetAssetPath(profile) : "aucun"));
        if (profile) EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        Debug.Log("[QuestBuild] identité : The Last Escapor " + Version + " (" + VersionCode + ")");
    }

    static Texture2D LoadIcon(string path)
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp.textureCompression != TextureImporterCompression.Uncompressed || imp.mipmapEnabled)
        {
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}
