using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// [#167] Feuillage dans la cabane du jardin.
// Deux arbres du fichier Blender (Garden_RealTree_07 et son LOD1, Garden_RealTree_08) ont des branches qui traversent les murs et
// la fenêtre de la cabane : de l'intérieur, les feuilles passent à travers le mur, le cadre de la fenêtre et l'étagère
// (textures « qui passent les unes dans les autres »). Correction : tout triangle de végétation dont un sommet est dans le
// volume de la cabane (murs compris, + marge) est retiré. Les maillages modifiés sont des copies (Assets/Generated/Feuillage),
// le FBX n'est pas touché ; appelé par RoomBuilder après la cabane (FixGardenCabin), et par le menu pour la scène ouverte.
public static class FeuillageCabane
{
    const string Dir = "Assets/Generated/Feuillage";
    const float Margin = 0.05f;          // 5 cm autour des murs : une feuille qui effleure le mur côté jardin disparaît aussi
    static readonly string[] Vegetation = { "Tree", "Plant", "Fern", "Bush", "Hedge", "Vine", "Ivy", "Leaf", "Leaves" };

    [MenuItem("SAE501/Scène/Feuillage hors de la cabane")]
    static void Menu()
    {
        var root = GameObject.Find("Carte (Blender)");
        int n = Apply(root ? root.transform : null);
        if (n > 0) { EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); EditorSceneManager.SaveOpenScenes(); }
        Debug.Log("[FeuillageCabane] " + n + " triangles de feuillage retirés de la cabane");
    }

    public static int Apply(Transform root)
    {
        if (!root) return 0;
        var walls = root.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.name == "Garden_Cabin_Walls");
        if (!walls) return 0;
        var box = walls.bounds; box.Expand(Margin * 2f);

        int removed = 0;
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!mf.sharedMesh || mf.name.StartsWith("Garden_Cabin")) continue;
            var path = PathOf(mf.transform);
            if (!Vegetation.Any(v => path.Contains(v)) || path.Contains("Imposteur")) continue;
            var r = mf.GetComponent<MeshRenderer>();
            if (!r || !r.bounds.Intersects(box)) continue;
            if (!mf.sharedMesh.isReadable) { Debug.LogWarning("[FeuillageCabane] " + path + " : maillage non lisible, ignoré"); continue; }
            removed += Trim(mf, box);
        }
        return removed;
    }

    static int Trim(MeshFilter mf, Bounds box)
    {
        var src = mf.sharedMesh; var v = src.vertices; var M = mf.transform.localToWorldMatrix;
        var inside = new bool[v.Length]; bool any = false;
        for (int i = 0; i < v.Length; i++) { inside[i] = box.Contains(M.MultiplyPoint3x4(v[i])); any |= inside[i]; }
        if (!any) return 0;

        var mesh = Object.Instantiate(src);
        mesh.name = src.name.Replace("(Clone)", "") + "_SansCabane";
        int removed = 0;
        for (int s = 0; s < mesh.subMeshCount; s++)
        {
            var t = mesh.GetTriangles(s); var keep = new List<int>(t.Length);
            for (int i = 0; i < t.Length; i += 3)
            {
                if (inside[t[i]] || inside[t[i + 1]] || inside[t[i + 2]]) { removed++; continue; }
                keep.Add(t[i]); keep.Add(t[i + 1]); keep.Add(t[i + 2]);
            }
            mesh.SetTriangles(keep, s, false);
        }
        mesh.RecalculateBounds();
        Directory.CreateDirectory(Dir);
        var file = Dir + "/" + PathOf(mf.transform).Replace("/", "_") + ".asset";
        AssetDatabase.DeleteAsset(file);
        AssetDatabase.CreateAsset(mesh, file);
        mf.sharedMesh = mesh;
        var mc = mf.GetComponent<MeshCollider>();
        if (mc && mc.sharedMesh == src) mc.sharedMesh = mesh;
        Debug.Log("[FeuillageCabane] " + PathOf(mf.transform) + " : " + removed + " triangles retirés");
        return removed;
    }

    static string PathOf(Transform t)
    {
        var s = t.name;
        for (var p = t.parent; p && p.name != "Carte (Blender)"; p = p.parent) s = p.name + "/" + s;
        return s;
    }
}
