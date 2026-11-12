using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// [hotfix #155] NavMesh des arènes du duel 1v1 (Alvéole 17, Quai 9), pour la téléportation au casque (NavTeleportArea) :
// la zone de téléportation n'accepte qu'un point du NavMesh relié aux pieds du joueur par un chemin court. Sans NavMesh,
// aucune téléportation n'était possible dans les arènes.
//   - agent « Humanoïde » du projet (rayon 0,2 m, marche 0,3 m, pente 45°), réglé par NavBuilder pour MainScene ;
//   - seulement les sols qu'on peut atteindre à pied : le dessus des conteneurs, des murs ou des caisses peut recevoir des
//     morceaux de NavMesh, mais sans chemin depuis le joueur ils sont refusés (NavTeleportArea.Reachable) ;
//   - les objets qu'on prend (armes) et le joueur sont ignorés.
// Menu : SAE501 → Arènes 1v1 → NavMesh de téléportation (rebake ciblé des deux scènes, sans les reconstruire).
// Aussi appelé par les constructeurs des arènes (Arena1v1Builder, ArenaKit.FinishScene) avant l'enregistrement de la scène.
public static class ArenaNavMesh
{
    [MenuItem("SAE501/Arènes 1v1/NavMesh de téléportation")]
    public static void BakeAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var setup = EditorSceneManager.GetSceneManagerSetup();
        foreach (var map in ArenaMaps.All)
        {
            string path = "Assets/Scenes/" + map.scene + ".unity";
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Bake(scene);
            EditorSceneManager.SaveScene(scene);
        }
        if (setup != null && setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
    }

    /// <summary>Construit le NavMesh de la scène (scène seule chargée) et l'enregistre dans Assets/Generated.</summary>
    public static void Bake(Scene scene)
    {
        foreach (var old in Object.FindObjectsByType<NavMeshSurface>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (old.gameObject.scene == scene && old.name == "NavMesh_Teleportation") Object.DestroyImmediate(old.gameObject);

        // armes et objets saisissables, joueur : hors du NavMesh
        foreach (var rb in Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (rb.gameObject.scene == scene) NavBuilder.Ignore(rb.gameObject);
        foreach (var o in Object.FindObjectsByType<Unity.XR.CoreUtils.XROrigin>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (o.gameObject.scene == scene) NavBuilder.Ignore(o.gameObject);

        var go = new GameObject("NavMesh_Teleportation");
        SceneManager.MoveGameObjectToScene(go, scene);
        var surface = go.AddComponent<NavMeshSurface>();
        surface.agentTypeID = 0;
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.layerMask = ~0;
        surface.BuildNavMesh();

        if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");
        string asset = "Assets/Generated/NavMesh_" + scene.name + ".asset";
        if (surface.navMeshData != null)
        {
            AssetDatabase.DeleteAsset(asset);
            AssetDatabase.CreateAsset(surface.navMeshData, asset);
            surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(asset);
            EditorUtility.SetDirty(surface);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        var tri = NavMesh.CalculateTriangulation();
        Debug.Log($"[ArenaNavMesh] {scene.name} : {tri.indices.Length / 3} triangles");
    }
}
