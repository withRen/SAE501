using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Éclairage précalculé de la carte (lightmaps) : rebonds de lumière et ombres douces, comme le rendu Cycles de Blender,
// pour un coût quasi nul au casque. Les lampes fixes sont « Baked » (RoomBuilder.BuildMapLights) ; les objets qui bougent
// prennent la lumière des sondes (RoomBuilder.BuildLightProbes).
// Étapes : SAE501 > Build VR Scene, puis SAE501 > Cuire l'éclairage (quelques minutes, la scène est enregistrée à la fin).
public static class LightBaker
{
    const string SettingsPath = "Assets/Settings/Eclairage_MainScene.lighting";

    public static void ApplySettings()
    {
        var ls = AssetDatabase.LoadAssetAtPath<LightingSettings>(SettingsPath);
        if (ls == null) { ls = new LightingSettings { name = "Eclairage_MainScene" }; AssetDatabase.CreateAsset(ls, SettingsPath); }
        ls.bakedGI = true;
        ls.realtimeGI = false;
        ls.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
        ls.lightmapResolution = 6f;                 // texels par mètre : assez pour les ombres douces, léger pour le Quest
        ls.lightmapMaxSize = 2048;
        ls.lightmapPadding = 2;
        ls.lightmapCompression = LightmapCompression.NormalQuality;
        ls.directionalityMode = LightmapsMode.NonDirectional;     // le moins coûteux au casque
        ls.directSampleCount = 32;
        ls.indirectSampleCount = 256;
        ls.environmentSampleCount = 32;
        ls.maxBounces = 3;
        ls.ao = true; ls.aoMaxDistance = 0.6f; ls.aoExponentIndirect = 1f; ls.aoExponentDirect = 0f;   // coins et contacts assombris
        ls.mixedBakeMode = MixedLightingMode.IndirectOnly;
        EditorUtility.SetDirty(ls);
        Lightmapping.lightingSettings = ls;
        // ciel noir comme dans la scène Blender (World_Black) : aucune lumière d'ambiance
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.reflectionIntensity = 0f;
    }

    [MenuItem("SAE501/Cuire l'éclairage (lightmaps)")]
    public static void Bake()
    {
        ApplySettings();
        var t0 = EditorApplication.timeSinceStartup;
        Lightmapping.bakeCompleted -= Done;
        Lightmapping.bakeCompleted += Done;
        if (!Lightmapping.BakeAsync()) Debug.LogError("[LightBaker] le calcul n'a pas pu démarrer");
        else Debug.Log("[LightBaker] calcul de l'éclairage lancé…");

        void Done()
        {
            Lightmapping.bakeCompleted -= Done;
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[LightBaker] éclairage cuit en {EditorApplication.timeSinceStartup - t0:F0} s : {LightmapSettings.lightmaps.Length} lightmaps, scène enregistrée");
        }
    }
}
