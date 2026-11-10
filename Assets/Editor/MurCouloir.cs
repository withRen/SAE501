using System.Linq;
using UnityEditor;
using UnityEngine;

// Mur manquant du couloir (demande de Rahim, journal 173) : dans le couloir qui longe la cuisine, la paroi ouest (x = 13 m) a un trou de
// 2,3 m entre le bout du mur de la salle du générateur (z = 0,65) et le mur de la cuisine (z = 2,95) : on voyait dans le renfoncement
// derrière. Un mur est ajouté, même épaisseur (15 cm), même hauteur (2,7 m), même matériau que Wall_Corridor_West, avec ses UV en
// continuité (u = -0,667 x (z - 3,5), v = 0,667 x y : 1,5 m par répétition). Collision à part (boîte). Pas d'UV de lightmap :
// il prend la lumière des sondes. Appelé par RoomBuilder avant le NavMesh.
public static class MurCouloir
{
    const float X0 = 12.90f, X1 = 13.05f, Z0 = 0.65f, Z1 = 2.95f, H = 2.7f;

    public static void Apply(Transform root)
    {
        var west = root.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.name == "Wall_Corridor_West");
        if (!west) { Debug.LogWarning("[MurCouloir] Wall_Corridor_West introuvable : mur non ajouté"); return; }
        if (root.GetComponentsInChildren<Transform>(true).Any(t => t.name == "Wall_Corridor_West_Ajout")) return;

        var go = new GameObject("Wall_Corridor_West_Ajout");
        go.transform.SetParent(west.transform.parent, false);
        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = Build();
        go.AddComponent<MeshRenderer>().sharedMaterial = west.sharedMaterial;
        var bc = go.AddComponent<BoxCollider>();
        bc.center = new Vector3((X0 + X1) * 0.5f, H * 0.5f, (Z0 + Z1) * 0.5f);
        bc.size = new Vector3(X1 - X0, H, Z1 - Z0);
        var r = go.GetComponent<MeshRenderer>();
        r.receiveGI = ReceiveGI.LightProbes;
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.NavigationStatic);
        Debug.Log($"[MurCouloir] mur ajouté : x {X0}..{X1}, z {Z0}..{Z1}");
    }

    static Mesh Build()
    {
        var v = new System.Collections.Generic.List<Vector3>(); var n = new System.Collections.Generic.List<Vector3>();
        var uv = new System.Collections.Generic.List<Vector2>(); var t = new System.Collections.Generic.List<int>();
        Vector2 UV(float z, float y) => new Vector2(-0.667f * (z - 3.5f), 0.667f * y);
        // face ouest (x = X0, normale -x : côté couloir) et face est (x = X1, normale +x)
        void Side(float x, Vector3 nor, bool flip)
        {
            int o = v.Count;
            foreach (var (z, y) in new[] { (Z0, 0f), (Z1, 0f), (Z1, H), (Z0, H) }) { v.Add(new Vector3(x, y, z)); n.Add(nor); uv.Add(UV(z, y)); }
            if (flip) t.AddRange(new[] { o, o + 2, o + 1, o, o + 3, o + 2 }); else t.AddRange(new[] { o, o + 1, o + 2, o, o + 2, o + 3 });
        }
        Side(X0, Vector3.left, false);
        Side(X1, Vector3.right, true);
        // chants : haut et deux bouts (visibles si on regarde de près)
        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 nor)
        {
            int o = v.Count;
            foreach (var p in new[] { a, b, c, d }) { v.Add(p); n.Add(nor); uv.Add(UV(p.z, p.y)); }
            t.AddRange(new[] { o, o + 1, o + 2, o, o + 2, o + 3 });
        }
        Quad(new Vector3(X0, H, Z0), new Vector3(X0, H, Z1), new Vector3(X1, H, Z1), new Vector3(X1, H, Z0), Vector3.up);
        Quad(new Vector3(X1, 0, Z0), new Vector3(X1, H, Z0), new Vector3(X0, H, Z0), new Vector3(X0, 0, Z0), Vector3.back);
        Quad(new Vector3(X0, 0, Z1), new Vector3(X0, H, Z1), new Vector3(X1, H, Z1), new Vector3(X1, 0, Z1), Vector3.forward);
        var m = new Mesh { name = "Mur_Couloir_Ajout" };
        m.SetVertices(v); m.SetNormals(n); m.SetUVs(0, uv); m.SetTriangles(t, 0);
        m.RecalculateBounds();
        m.RecalculateTangents();
        const string dir = "Assets/Generated/TrousToit";   // dossier des maillages du décor réparé (existe après TrousToit.Apply)
        if (AssetDatabase.IsValidFolder(dir))
        {
            var path = dir + "/Mur_Couloir_Ajout.asset";
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (old == null) { AssetDatabase.CreateAsset(m, path); return m; }
            EditorUtility.CopySerialized(m, old); EditorUtility.SetDirty(old); Object.DestroyImmediate(m);
            return old;
        }
        return m;
    }
}
