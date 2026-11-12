using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// [#170] Raccords du jardin : trois défauts visuels relevés par Rahim le 12/11, corrigés ici pour qu'ils ne reviennent pas
// à la prochaine reconstruction (appelé par RoomBuilder après FeuillageCabane) et applicables à la scène ouverte par le menu.
//  1. Arbres : depuis le passage au jacaranda (journal 175, FBX remplacé sans reconstruire), l'ordre des sous-maillages est
//     feuilles, rameaux, tronc, mais les arbres de la scène gardaient l'ordre de l'olivier (feuilles, tronc, rameaux) : le tronc
//     recevait la texture des rameaux, faite pour des brindilles, d'où une écorce « énorme ». Les matériaux sont remis dans l'ordre.
//  2. Portail du jardin : les boules de haie de chaque côté avalaient les piliers en brique et dépassaient dans le passage.
//     Chaque boule qui touche un pilier est reculée (sans déformation) pour s'arrêter au milieu du pilier.
//  3. Cabane : l'étagère entrait de 7 cm dans le mur est, à côté de la fenêtre (le mur coupait son montant). Elle est ramenée
//     contre le lambris ; le buffet voisin suit pour ne pas la chevaucher.
// Les maillages modifiés sont des copies (Assets/Generated/Raccords), le FBX n'est pas touché.
public static class RaccordsJardin
{
    const string Dir = "Assets/Generated/Raccords";

    [MenuItem("SAE501/Scène/Raccords du jardin (arbres, portail, étagère)")]
    static void Menu()
    {
        var root = GameObject.Find("Carte (Blender)");
        if (!root) { Debug.LogWarning("[RaccordsJardin] « Carte (Blender) » introuvable : ouvrir MainScene"); return; }
        if (Apply(root.transform)) { EditorSceneManager.MarkSceneDirty(root.scene); EditorSceneManager.SaveScene(root.scene); }
    }

    public static bool Apply(Transform root)
    {
        if (!root) return false;
        int a = Arbres(root), h = Haies(root), e = Etagere(root);
        Debug.Log($"[RaccordsJardin] {a} arbres remis en ordre, {h} boules de haie reculées des piliers, étagère/buffet déplacés : {e}");
        return a + h + e > 0;
    }

    // ---------- 1. ordre des matériaux des arbres ----------
    static int Arbres(Transform root)
    {
        int n = 0;
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true).Concat(Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)).Distinct())
        {
            var m = r.sharedMaterials;
            if (m.Length != 3 || m.Any(x => !x)) continue;
            var leaves = m.FirstOrDefault(x => x.name == "island_tree_02_leaves");
            var branches = m.FirstOrDefault(x => x.name == "island_tree_02_branches");
            var trunk = m.FirstOrDefault(x => x.name == "island_tree_02");
            if (!leaves || !branches || !trunk || (m[0] == leaves && m[1] == branches && m[2] == trunk)) continue;
            r.sharedMaterials = new[] { leaves, branches, trunk };      // ordre des sous-maillages de Arbre_Quest.fbx
            n++;
        }
        return n;
    }

    // ---------- 2. haies du portail ----------
    static int Haies(Transform root)
    {
        var all = root.GetComponentsInChildren<MeshFilter>(true);
        var pillarsMf = all.FirstOrDefault(f => f.name == "Garden_Gate_Pillars");
        var hedge = all.FirstOrDefault(f => f.name == "Garden_HighHedges_Boundary");
        if (!pillarsMf || !hedge || !hedge.sharedMesh || !hedge.sharedMesh.isReadable || !pillarsMf.sharedMesh.isReadable) return 0;

        var pillars = Parts(pillarsMf.sharedMesh, pillarsMf.transform).Select(p => p.box).Where(b => b.size.y > 2f).ToList();
        if (pillars.Count < 2) return 0;
        var gate = new Vector3(pillars.Average(b => b.center.x), 0f, pillars.Average(b => b.center.z));
        // axe du passage : celui où les piliers sont le plus écartés
        var span = pillars[1].center - pillars[0].center;
        int axis = Mathf.Abs(span.x) > Mathf.Abs(span.z) ? 0 : 2;

        var mesh = hedge.sharedMesh;
        var v = mesh.vertices; var M = hedge.transform.localToWorldMatrix; var Mi = hedge.transform.worldToLocalMatrix;
        int moved = 0; bool changed = false;
        foreach (var part in Parts(mesh, hedge.transform))
        {
            if (part.box.size.magnitude > 4.5f) continue;                       // seulement les boules, pas le cœur de la haie
            foreach (var p in pillars)
            {
                if (!part.box.Intersects(p)) continue;
                float dir = Mathf.Sign(gate[axis] - p.center[axis]);            // côté du passage
                float edge = dir > 0 ? part.box.max[axis] : part.box.min[axis];
                float limit = p.center[axis];
                float shift = limit - edge;                                      // < 0 si dir > 0 et la boule dépasse vers le passage
                if (dir * shift >= -0.005f) continue;
                var d = Vector3.zero; d[axis] = shift;
                foreach (int i in part.verts) v[i] = Mi.MultiplyPoint3x4(M.MultiplyPoint3x4(v[i]) + d);
                moved++; changed = true;
                break;
            }
        }
        if (!changed) return 0;

        var copy = Object.Instantiate(mesh);
        copy.name = "Garden_HighHedges_Boundary";
        copy.vertices = v;
        copy.RecalculateBounds();
        Directory.CreateDirectory(Dir);
        var file = Dir + "/Garden_HighHedges_Boundary.asset";
        AssetDatabase.DeleteAsset(file);
        AssetDatabase.CreateAsset(copy, file);
        var mc = hedge.GetComponent<MeshCollider>();
        if (mc && mc.sharedMesh == mesh) mc.sharedMesh = copy;
        hedge.sharedMesh = copy;
        return moved;
    }

    // ---------- 3. étagère de la cabane ----------
    static int Etagere(Transform root)
    {
        var shelf = Find(root, "Garden_Cabin_Bookshelf");
        if (!shelf) return 0;
        var sb = Bounds(shelf);
        // face intérieure du mur est (collider du mur), moins l'épaisseur du lambris (3 cm) et 1 cm de jeu
        Physics.SyncTransforms();
        var from = new Vector3(sb.center.x, 1.5f, sb.center.z);
        var hit = Physics.RaycastAll(from, Vector3.right, 3f, ~0, QueryTriggerInteraction.Ignore)
                         .Where(h => h.collider.name == "Garden_Cabin_Walls").OrderBy(h => h.distance).FirstOrDefault();
        if (!hit.collider) return 0;
        float limit = hit.point.x - 0.04f;
        float dx = limit - sb.max.x;
        if (dx >= -0.005f) return 0;
        Undo.RecordObject(shelf, "Raccords jardin");
        shelf.position += new Vector3(dx, 0f, 0f);
        int n = 1;
        var side = Find(root, "Garden_Cabin_Sideboard");
        if (side)
        {
            var b = Bounds(side); float over = b.max.x - (sb.min.x + dx - 0.01f);
            if (over > 0f && b.max.z > sb.min.z && b.min.z < sb.max.z) { Undo.RecordObject(side, "Raccords jardin"); side.position -= new Vector3(over, 0f, 0f); n++; }
        }
        return n;
    }

    static Transform Find(Transform root, string name) =>
        root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);

    static Bounds Bounds(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>();
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    // morceaux connexes d'un maillage (sommets soudés par position) : boîte monde + indices de sommets
    static List<(Bounds box, List<int> verts)> Parts(Mesh m, Transform t)
    {
        var v = m.vertices; var tri = m.triangles;
        var key = new Dictionary<Vector3Int, int>(); var id = new int[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            var k = Vector3Int.RoundToInt(v[i] * 1000f);
            if (!key.TryGetValue(k, out var j)) { j = key.Count; key[k] = j; }
            id[i] = j;
        }
        var par = Enumerable.Range(0, key.Count).ToArray();
        int F(int a) { while (par[a] != a) { par[a] = par[par[a]]; a = par[a]; } return a; }
        for (int i = 0; i < tri.Length; i += 3)
        {
            int a = F(id[tri[i]]);
            par[F(id[tri[i + 1]])] = a;
            par[F(id[tri[i + 2]])] = F(a);
        }
        var map = new Dictionary<int, int>(); var res = new List<(Bounds, List<int>)>();
        for (int i = 0; i < v.Length; i++)
        {
            int r = F(id[i]); var w = t.TransformPoint(v[i]);
            if (!map.TryGetValue(r, out var c)) { c = res.Count; map[r] = c; res.Add((new Bounds(w, Vector3.zero), new List<int>())); }
            var e = res[c]; e.Item1.Encapsulate(w); e.Item2.Add(i); res[c] = e;
        }
        return res;
    }
}
