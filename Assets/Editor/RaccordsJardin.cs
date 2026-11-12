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
//  4. Tonneaux : l'export de la carte (6 000 triangles par objet) laisse le haut de chaque douelle en petit bloc à arêtes
//     vives : le rebord ressemblait à « plein de carrés ». Normales lissées (sommets confondus, angle < 80°) ; forme inchangée.
//  5. Cabane : faces confondues du cadre de fenêtre/porte et du lambris avec le mur (scintillement en dents de scie) ;
//     boules de haie qui entraient dans le mur du fond (reculées dans l'épaisseur du mur, voir Haies).
//  6. Façade de la cuisine (côté jardin) : boules de haie au-dessus du bac sud reculées, cœur de la haie prolongé jusqu'au coin
//     du mur (plus de jour entre la haie et la cuisine), coupes de l'export rentrées dans le mur de la salle du générateur.
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
        int a = Arbres(root), h = Haies(root), e = Etagere(root), t = Tonneaux(root), f = FacesConfondues(root);
        Debug.Log($"[RaccordsJardin] {a} arbres remis en ordre, {h} boules de haie déplacées (piliers, cabane), étagère/buffet déplacés : {e}, {t} tonneaux lissés, {f} maillages de la cabane resserrés");
        return a + h + e + t + f > 0;
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

        // source : le maillage d'origine du FBX de la carte (et pas la copie déjà rognée par FeuillageCabane, dont les trous
        // réapparaîtraient en déplaçant les boules) ; le rognage de l'intérieur de la cabane est refait ici, après les déplacements
        var mesh = FbxMesh(pillarsMf.sharedMesh, hedge.name) ?? hedge.sharedMesh;
        var current = hedge.sharedMesh;
        var v = mesh.vertices; var M = hedge.transform.localToWorldMatrix; var Mi = hedge.transform.worldToLocalMatrix;
        var wallsR = root.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.name == "Garden_Cabin_Walls");
        var cabin = wallsR ? wallsR.bounds : new Bounds();
        var rends = root.GetComponentsInChildren<MeshRenderer>(true);
        // bacs à fleurs du jardin (la haie ne doit pas pousser dedans) ; mur en brique de la salle du générateur ; mur de la cuisine
        var planters = rends.Where(r => r.name.StartsWith("Garden_Planter_") && !r.name.Contains("_LOD")).Select(r => r.bounds).ToList();
        var genWall = rends.FirstOrDefault(r => r.name == "Wall_GeneratorRoom_West");
        var kitchenWall = rends.FirstOrDefault(r => r.name == "Wall_North");
        int moved = 0;
        foreach (var part in Parts(mesh, hedge.transform))
        {
            if (part.box.size.magnitude > 4.5f) continue;                       // seulement les boules, pas le cœur de la haie
            var d = Vector3.zero;
            foreach (var p in pillars)
            {
                if (!part.box.Intersects(p)) continue;
                float dir = Mathf.Sign(gate[axis] - p.center[axis]);            // côté du passage
                float edge = dir > 0 ? part.box.max[axis] : part.box.min[axis];
                float shift = p.center[axis] - edge;                             // la boule s'arrête au milieu du pilier
                if (dir * shift < -0.005f) d[axis] = shift;
                break;
            }
            // boule qui entre dans un mur de la cabane : reculée pour s'arrêter 2 cm dans l'épaisseur du mur (ni vide, ni feuilles dedans)
            if (wallsR && part.box.Intersects(cabin) && !cabin.Contains(new Vector3(part.box.center.x, cabin.center.y, part.box.center.z)))
            {
                const float inset = 0.02f;
                var cands = new[]
                {
                    new Vector3(cabin.min.x + inset - part.box.max.x, 0, 0), new Vector3(cabin.max.x - inset - part.box.min.x, 0, 0),
                    new Vector3(0, 0, cabin.min.z + inset - part.box.max.z), new Vector3(0, 0, cabin.max.z - inset - part.box.min.z),
                };
                // sens : celui qui éloigne la boule de la cabane, le plus court
                var best = cands.Where(c => Vector3.Dot(c, part.box.center - cabin.center) > 0f).OrderBy(c => c.magnitude).FirstOrDefault();
                if (best.magnitude > 0.005f) d += best;
            }
            // boule au-dessus d'un bac : reculée à l'horizontale (sens le plus court) pour laisser 5 cm autour du bac
            foreach (var pl in planters)
            {
                var b = part.box; b.center += d;
                if (b.max.x <= pl.min.x - 0.05f || b.min.x >= pl.max.x + 0.05f || b.max.z <= pl.min.z - 0.05f || b.min.z >= pl.max.z + 0.05f) continue;
                var cands = new[]
                {
                    new Vector3(pl.min.x - 0.05f - b.max.x, 0, 0), new Vector3(pl.max.x + 0.05f - b.min.x, 0, 0),
                    new Vector3(0, 0, pl.min.z - 0.05f - b.max.z), new Vector3(0, 0, pl.max.z + 0.05f - b.min.z),
                };
                d += cands.Where(c => Vector3.Dot(c, b.center - pl.center) > 0f).OrderBy(c => c.magnitude).FirstOrDefault();
            }
            // boule coupée net par l'export à 10 cm du mur en brique de la salle du générateur : la coupe est rentrée dans le mur
            if (genWall)
            {
                var w = genWall.bounds; var b = part.box;
                if (b.min.x > w.max.x && b.min.x < w.max.x + 0.2f && b.min.z < w.max.z + 1.2f && b.max.z > w.min.z)
                    d.x += (w.max.x - 0.02f) - b.min.x;
            }
            if (d == Vector3.zero) continue;
            foreach (int i in part.verts) v[i] = Mi.MultiplyPoint3x4(M.MultiplyPoint3x4(v[i]) + d);
            moved++;
        }

        // coin de la cuisine : le cœur de la haie (bloc d'un mètre de large) s'arrêtait 20 cm avant le coin du mur, et les boules
        // arrondies laissaient un jour vers l'extérieur. Le bout du cœur est prolongé jusque derrière le coin et collé au mur.
        if (kitchenWall)
        {
            var w = kitchenWall.bounds;
            foreach (var part in Parts(mesh, hedge.transform))
            {
                var b = part.box;
                if (b.size.magnitude <= 4.5f || b.min.z < w.max.z - 0.3f || b.min.z > w.max.z + 0.6f || b.min.x > w.max.x + 0.5f || b.max.x < w.max.x) continue;
                foreach (int i in part.verts)
                {
                    var p = M.MultiplyPoint3x4(v[i]);
                    if (p.z < b.min.z + 0.05f) p.z = w.max.z - 0.1f;             // bout du cœur : 10 cm derrière le coin, dans l'épaisseur du mur
                    if (p.x < b.min.x + 0.05f) p.x = w.max.x - 0.05f;             // flanc côté mur : collé au mur
                    v[i] = Mi.MultiplyPoint3x4(p);
                }
                moved++;
            }
        }

        // rognage : tout triangle dont un sommet est dans le volume intérieur de la cabane (murs de 13 cm exclus)
        var inner = cabin; inner.Expand(new Vector3(-0.2f, 0f, -0.2f));
        var copy = Object.Instantiate(mesh);
        copy.name = "Garden_HighHedges_Boundary";
        copy.vertices = v;
        if (wallsR)
            for (int s = 0; s < copy.subMeshCount; s++)
            {
                var t = copy.GetTriangles(s); var keep = new List<int>(t.Length);
                for (int i = 0; i < t.Length; i += 3)
                {
                    bool In(int k) => inner.Contains(M.MultiplyPoint3x4(v[t[k]]));
                    if (In(i) || In(i + 1) || In(i + 2)) continue;
                    keep.Add(t[i]); keep.Add(t[i + 1]); keep.Add(t[i + 2]);
                }
                copy.SetTriangles(keep, s, false);
            }
        copy.RecalculateBounds();
        var file = Dir + "/Garden_HighHedges_Boundary.asset";
        if (AssetDatabase.GetAssetPath(current) == file && current.vertexCount == copy.vertexCount
            && current.triangles.Length == copy.triangles.Length && current.vertices.SequenceEqual(copy.vertices))
        { Object.DestroyImmediate(copy); return 0; }                             // déjà à jour
        Directory.CreateDirectory(Dir);
        AssetDatabase.DeleteAsset(file);
        AssetDatabase.CreateAsset(copy, file);
        var mc = hedge.GetComponent<MeshCollider>();
        if (mc) mc.sharedMesh = copy;
        hedge.sharedMesh = copy;
        return Mathf.Max(moved, 1);
    }

    // maillage d'origine (même nom) dans le FBX d'où vient « reference »
    static Mesh FbxMesh(Mesh reference, string name)
    {
        var path = AssetDatabase.GetAssetPath(reference);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) return null;
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().FirstOrDefault(m => m.name == name);
    }

    // ---------- 5. cabane : faces confondues (scintillement) ----------
    // Le cadre de la fenêtre et de la porte (Garden_Cabin_WindowSash) a ses faces intérieures exactement sur les tableaux,
    // l'appui et le linteau du mur, et le lambris (Garden_Cabin_Wainscot) a ses bords sur ceux du mur à la porte : deux faces
    // au même endroit scintillent en dents de scie. Chaque morceau de ces maillages est resserré de quelques millimètres vers
    // son centre : les faces intérieures du cadre passent devant le mur (bois visible), les bords du lambris passent derrière.
    static int FacesConfondues(Transform root)
    {
        int n = 0;
        foreach (var (name, mm) in new[] { ("Garden_Cabin_WindowSash", 0.004f), ("Garden_Cabin_Wainscot", 0.003f) })
        {
            var mf = root.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f => f.name == name);
            if (!mf || !mf.sharedMesh) continue;
            var file = Dir + "/" + name + ".asset";
            if (AssetDatabase.GetAssetPath(mf.sharedMesh) == file) continue;          // déjà fait
            var src = mf.sharedMesh;
            if (!src.isReadable) continue;
            var copy = Object.Instantiate(src); copy.name = name;
            var v = copy.vertices;
            foreach (var part in Parts(src, mf.transform))
            {
                var c = mf.transform.InverseTransformPoint(part.box.center);
                foreach (int i in part.verts)
                {
                    var p = v[i];
                    for (int a = 0; a < 3; a++)
                    {
                        float off = p[a] - c[a];
                        if (Mathf.Abs(off) > 0.01f) p[a] -= Mathf.Sign(off) * mm;
                    }
                    v[i] = p;
                }
            }
            copy.vertices = v; copy.RecalculateBounds();
            Directory.CreateDirectory(Dir);
            AssetDatabase.DeleteAsset(file);
            AssetDatabase.CreateAsset(copy, file);
            var mc = mf.GetComponent<MeshCollider>();
            if (mc && mc.sharedMesh == src) mc.sharedMesh = copy;
            mf.sharedMesh = copy;
            n++;
        }
        return n;
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

    // ---------- 4. tonneaux : normales lissées ----------
    static int Tonneaux(Transform root)
    {
        int n = 0;
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            var src = mf.sharedMesh;
            if (!src || !mf.name.Contains("Barrel") || !src.isReadable) continue;
            var file = Dir + "/" + mf.name + "_Lisse.asset";
            if (AssetDatabase.GetAssetPath(src) == file) continue;                 // déjà fait
            var m = Object.Instantiate(src); m.name = mf.name + "_Lisse";
            var v = m.vertices; var nr = m.normals;
            var groups = new Dictionary<Vector3Int, List<int>>();
            for (int i = 0; i < v.Length; i++)
            {
                var k = Vector3Int.RoundToInt(v[i] * 500f);                     // 2 mm
                if (!groups.TryGetValue(k, out var l)) groups[k] = l = new List<int>();
                l.Add(i);
            }
            var outN = (Vector3[])nr.Clone(); float cos = Mathf.Cos(80f * Mathf.Deg2Rad);
            foreach (var g in groups.Values)
                foreach (int i in g)
                {
                    var sum = Vector3.zero;
                    foreach (int j in g) if (Vector3.Dot(nr[i], nr[j]) > cos) sum += nr[j];
                    outN[i] = sum.normalized;
                }
            m.normals = outN;
            m.RecalculateTangents();
            Directory.CreateDirectory(Dir);
            AssetDatabase.DeleteAsset(file);
            AssetDatabase.CreateAsset(m, file);
            mf.sharedMesh = m;
            n++;
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
