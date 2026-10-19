using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// [#118] Niveaux de détail (LOD) des gros objets du décor, construits à chaque Build VR Scene (RoomBuilder).
// La carte Blender plafonne chaque objet à 6 000 triangles (export_map.py) : ~140 objets sont à ce plafond et pèsent
// l'essentiel des ~800 000 triangles. Pour chacun, on calcule une version simplifiée (regroupement des sommets sur une grille,
// « vertex clustering ») et on pose un LODGroup :
//   LOD0 (maillage complet) tant que l'objet occupe plus de lod0ScreenHeight de la hauteur de l'écran,
//   LOD1 (simplifié) ensuite, puis rien en dessous de cullScreenHeight (objet minuscule, au fond du brouillard).
// Les maillages simplifiés sont enregistrés dans Assets/Generated/LOD : mis à jour sur place à chaque construction
// (même fichier, même GUID : pas de changement dans git si la carte n'a pas changé), fichiers devenus inutiles supprimés.
// LOD1 ne reçoit pas de lightmap (UV abîmés par la simplification) : il prend la lumière des sondes (RoomBuilder.BuildLightProbes).
public static class LodBuilder
{
    const string Dir = "Assets/Generated/LOD";
    public const int MinTriangles = 2000;          // en dessous : objet déjà léger, pas de LOD
    public const int GridCells = 20;               // cases de la grille sur la plus grande dimension de l'objet
    public const float MaxKeptRatio = 0.6f;        // LOD1 gardé seulement s'il retire au moins 40 % des triangles
    public const float Lod0ScreenHeight = 0.1f;        // ~8 m pour un objet de 1,8 m (champ du Quest 3)
    public const float CullScreenHeight = 0.012f;

    public static void Build(Transform mapRoot)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Generated", "LOD");
        var used = new HashSet<string>();

        int count = 0; long before = 0, after = 0;
        var done = new Dictionary<Mesh, Mesh>();       // un même maillage partagé n'est simplifié qu'une fois
        foreach (var mf in mapRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            var mesh = mf.sharedMesh;
            var r = mf.GetComponent<MeshRenderer>();
            if (!mesh || !r || !mesh.isReadable || mf.GetComponentInParent<LODGroup>() || mf.GetComponentInParent<XRBaseInteractable>()) continue;
            int tris = (int)(mesh.GetIndexCount(0) / 3);
            for (int s = 1; s < mesh.subMeshCount; s++) tris += (int)(mesh.GetIndexCount(s) / 3);
            if (tris < MinTriangles) continue;

            if (!done.TryGetValue(mesh, out var low))
            {
                low = Simplify(mesh, GridCells);
                if (low != null && low.triangles.Length / 3 > tris * MaxKeptRatio) low = null;
                if (low != null) low = Save(low, $"{Dir}/{Safe(mesh.name)}_LOD1.asset", used);
                done[mesh] = low;
            }
            if (low == null) continue;

            var child = new GameObject(mf.name + "_LOD1");
            child.transform.SetParent(mf.transform, false);
            child.AddComponent<MeshFilter>().sharedMesh = low;
            var lr = child.AddComponent<MeshRenderer>();
            lr.sharedMaterials = r.sharedMaterials;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveGI = ReceiveGI.LightProbes;
            GameObjectUtility.SetStaticEditorFlags(child, GameObjectUtility.GetStaticEditorFlags(mf.gameObject) | StaticEditorFlags.ContributeGI);

            var g = mf.gameObject.AddComponent<LODGroup>();
            g.SetLODs(new[] { new LOD(Lod0ScreenHeight, new Renderer[] { r }), new LOD(CullScreenHeight, new Renderer[] { lr }) });
            g.fadeMode = LODFadeMode.None;
            g.RecalculateBounds();
            count++; before += tris; after += low.triangles.Length / 3;
        }
        foreach (var guid in AssetDatabase.FindAssets("t:Mesh", new[] { Dir }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!used.Contains(path)) AssetDatabase.DeleteAsset(path);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[LodBuilder] {count} objets avec LOD : {before} triangles en LOD0, {after} en LOD1 ({(before > 0 ? 100f * after / before : 0f):F0} %)");
    }

    // écrit le maillage dans le fichier existant s'il y en a un (garde son GUID), sinon le crée
    static Mesh Save(Mesh m, string path, HashSet<string> used)
    {
        var stem = path.Substring(0, path.Length - "_LOD1.asset".Length);
        for (int i = 2; used.Contains(path); i++) path = stem + "_" + i + "_LOD1.asset";     // deux maillages du même nom
        used.Add(path);
        var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (old == null) { AssetDatabase.CreateAsset(m, path); return m; }
        EditorUtility.CopySerialized(m, old);
        old.name = Path.GetFileNameWithoutExtension(path);
        EditorUtility.SetDirty(old);
        Object.DestroyImmediate(m);
        return old;
    }

    static string Safe(string n)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) n = n.Replace(c, '_');
        return n.Replace(' ', '_');
    }

    // Regroupement des sommets : chaque sommet est rangé dans une case de la grille (et selon le sens de sa normale,
    // pour ne pas fusionner les deux faces d'une planche) ; chaque case garde un seul sommet, les triangles qui deviennent
    // plats disparaissent. Les sous-maillages (matériaux) sont conservés.
    public static Mesh Simplify(Mesh src, int cells)
    {
        var v = src.vertices; var n = src.normals; var uv = src.uv; var tg = src.tangents; var col = src.colors;
        bool hasN = n.Length == v.Length, hasUV = uv.Length == v.Length, hasT = tg.Length == v.Length, hasC = col.Length == v.Length;
        var b = src.bounds;
        float cell = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)) / cells;
        if (cell <= 1e-5f) return null;

        var map = new int[v.Length];
        var key2new = new Dictionary<long, int>();
        var nv = new List<Vector3>(); var nn = new List<Vector3>(); var nuv = new List<Vector2>(); var nt = new List<Vector4>(); var nc = new List<Color>();
        for (int i = 0; i < v.Length; i++)
        {
            var p = v[i] - b.min;
            long x = Mathf.FloorToInt(p.x / cell), y = Mathf.FloorToInt(p.y / cell), z = Mathf.FloorToInt(p.z / cell);
            long oct = 0;
            if (hasN) oct = (n[i].x >= 0 ? 1 : 0) | (n[i].y >= 0 ? 2 : 0) | (n[i].z >= 0 ? 4 : 0);
            long key = (((x * 1024 + y) * 1024 + z) << 3) | oct;
            if (!key2new.TryGetValue(key, out var id))
            {
                id = nv.Count; key2new[key] = id;
                nv.Add(v[i]);
                if (hasN) nn.Add(n[i]);
                if (hasUV) nuv.Add(uv[i]);
                if (hasT) nt.Add(tg[i]);
                if (hasC) nc.Add(col[i]);
            }
            map[i] = id;
        }

        var m = new Mesh { name = src.name + "_LOD1" };
        m.indexFormat = nv.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        m.SetVertices(nv);
        if (hasN) m.SetNormals(nn);
        if (hasUV) m.SetUVs(0, nuv);
        if (hasT) m.SetTangents(nt);
        if (hasC) m.SetColors(nc);
        m.subMeshCount = src.subMeshCount;
        int total = 0;
        for (int s = 0; s < src.subMeshCount; s++)
        {
            var t = src.GetTriangles(s);
            var outT = new List<int>(t.Length);
            var seen = new HashSet<(int, int, int)>();
            for (int i = 0; i + 2 < t.Length; i += 3)
            {
                int a = map[t[i]], c = map[t[i + 1]], d = map[t[i + 2]];
                if (a == c || c == d || a == d) continue;                    // triangle devenu plat
                // même triangle en double (même sommets, même sens) : un seul
                int lo = Mathf.Min(a, Mathf.Min(c, d));
                var k = lo == a ? (a, c, d) : lo == c ? (c, d, a) : (d, a, c);
                if (!seen.Add(k)) continue;
                outT.Add(a); outT.Add(c); outT.Add(d);
            }
            m.SetTriangles(outT, s, false);
            total += outT.Count;
        }
        if (total == 0) return null;
        m.RecalculateBounds();
        return m;
    }
}
