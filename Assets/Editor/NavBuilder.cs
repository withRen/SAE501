using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// NavMesh des zombies : agent plus fin que celui par défaut (rayon 0,3 m) pour passer les portes de 1 m.
public static class NavBuilder
{
    // règle l'agent "Humanoïde" du projet (ProjectSettings/NavMeshAreas)
    static void SetupAgentType()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset");
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        var settings = so.FindProperty("m_Settings");
        if (settings == null || settings.arraySize == 0) return;
        var s0 = settings.GetArrayElementAtIndex(0);
        s0.FindPropertyRelative("agentRadius").floatValue = 0.3f;
        s0.FindPropertyRelative("agentHeight").floatValue = 1.8f;
        s0.FindPropertyRelative("agentClimb").floatValue = 0.3f;
        s0.FindPropertyRelative("agentSlope").floatValue = 45f;
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    public static void Ignore(GameObject go)
    {
        var m = go.GetComponent<NavMeshModifier>() ?? go.AddComponent<NavMeshModifier>();
        m.overrideArea = false;
        m.ignoreFromBuild = true;
        m.applyToChildren = true;
    }

    public static void Bake(Transform parent, params GameObject[] ignored)
    {
        SetupAgentType();
        foreach (var g in ignored) if (g) Ignore(g);

        var go = new GameObject("NavMesh");
        go.transform.SetParent(parent, false);
        var surface = go.AddComponent<NavMeshSurface>();
        surface.agentTypeID = 0;
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.layerMask = ~0;
        surface.BuildNavMesh();

        if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");
        const string path = "Assets/Generated/NavMesh_MainScene.asset";
        if (surface.navMeshData != null)
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(surface.navMeshData, path);
            surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            EditorUtility.SetDirty(surface);
        }
        var tri = NavMesh.CalculateTriangulation();
        Debug.Log($"[NavBuilder] NavMesh construit : {tri.vertices.Length} sommets, {tri.indices.Length / 3} triangles");
    }
}
