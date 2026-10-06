using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Pipeline de rendu allégé pour Meta Quest (copie de Mobile_RPAsset : rendu Forward, sans cascades ni ombres douces).
public static class QuestRender
{
    const string Src = "Assets/Settings/Mobile_RPAsset.asset";
    const string Dst = "Assets/Settings/Quest_RPAsset.asset";

    [MenuItem("SAE501/Build/Configurer le rendu Quest")]
    public static void Setup()
    {
        if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Dst) == null)
            AssetDatabase.CopyAsset(Src, Dst);
        var a = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Dst);

        a.renderScale = 0.9f;
        a.msaaSampleCount = 4;
        a.supportsHDR = false;
        a.supportsCameraDepthTexture = false;
        a.supportsCameraOpaqueTexture = false;
        a.shadowDistance = 8f;
        a.shadowCascadeCount = 1;

        var so = new SerializedObject(a);
        void B(string n, bool v) { var p = so.FindProperty(n); if (p != null) p.boolValue = v; }
        void I(string n, int v) { var p = so.FindProperty(n); if (p != null) p.intValue = v; }
        B("m_MainLightShadowsSupported", false);       // pas de lumière directionnelle
        B("m_AdditionalLightShadowsSupported", true);  // ombre de la lampe à huile
        B("m_SoftShadowsSupported", false);
        B("m_EnableLODCrossFade", false);
        I("m_AdditionalLightsShadowmapResolution", 1024);
        I("m_AdditionalLightsShadowResolutionTierLow", 256);
        I("m_AdditionalLightsShadowResolutionTierMedium", 512);
        I("m_AdditionalLightsShadowResolutionTierHigh", 1024);
        I("m_AdditionalLightsPerObjectLimit", 4);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(a);

        GraphicsSettings.defaultRenderPipeline = a;
        int current = QualitySettings.GetQualityLevel();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = a;
            QualitySettings.vSyncCount = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }
        QualitySettings.SetQualityLevel(current, false);

        // static batching (Android) : moins de draw calls pour les murs, le sol et le mobilier
        var ps = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var list = ps.FindProperty("m_BuildTargetBatching");
        if (list != null)
        {
            int idx = -1;
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("m_BuildTarget").stringValue == "Android") idx = i;
            if (idx < 0) { list.arraySize++; idx = list.arraySize - 1; list.GetArrayElementAtIndex(idx).FindPropertyRelative("m_BuildTarget").stringValue = "Android"; }
            list.GetArrayElementAtIndex(idx).FindPropertyRelative("m_StaticBatching").boolValue = true;
            list.GetArrayElementAtIndex(idx).FindPropertyRelative("m_DynamicBatching").boolValue = false;
            ps.ApplyModifiedPropertiesWithoutUndo();
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[QuestRender] pipeline Quest appliqué : " + a.name);
    }
}
