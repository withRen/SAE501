using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// NavMesh des zombies : agent fin (rayon 0,2 m) pour passer entre les objets posés dans les couloirs de la carte Blender.
// Les déchets mous (sacs poubelle, cartons, papiers) ne bloquent pas : les zombies les piétinent.
public static class NavBuilder
{
    public const float AgentRadius = 0.2f;
    static readonly string[] SoftClutter = { "Trashbag", "Cardboard", "Papers", "Debris" };

    // règle l'agent "Humanoïde" du projet (ProjectSettings/NavMeshAreas)
    static void SetupAgentType()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset");
        if (assets == null || assets.Length == 0) return;
        var so = new SerializedObject(assets[0]);
        var settings = so.FindProperty("m_Settings");
        if (settings == null || settings.arraySize == 0) return;
        var s0 = settings.GetArrayElementAtIndex(0);
        s0.FindPropertyRelative("agentRadius").floatValue = AgentRadius;
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
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Include))
            if (SoftClutter.Any(k => c.name.Contains(k))) Ignore(c.gameObject);

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
