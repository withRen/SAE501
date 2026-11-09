using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Faces superposées de la carte Blender (journal 148).
// Dans le fichier, plusieurs murs entrent les uns dans les autres : les murs du couloir droit traversent le mur est du bunker,
// les deux planchers se recouvrent sous la porte, les murs d'angle se recouvrent sur la face extérieure, etc. Leurs faces
// tombent exactement dans le même plan que la face visible de l'autre mur : deux surfaces à la même profondeur, avec des
// textures différentes, se battent à l'écran (z-fighting : taches et bandes qui clignotent quand on bouge la tête).
// Correction : un triangle aligné sur les axes, entièrement recouvert par une face d'un AUTRE objet dans le même plan et
// tourné du même côté, est retiré (il ne se voyait de toute façon qu'à travers l'autre face). Si deux faces identiques se
// recouvrent, une seule est retirée. Les maillages modifiés sont des copies (Assets/Generated/Recouvrements), le FBX n'est
// pas touché ; appelé par RoomBuilder avant la création des collisions.
public static class MursSuperposes
{
    const string Dir = "Assets/Generated/Recouvrements";
    const float PlaneTol = 0.002f;       // 2 mm : même plan
    const float EdgeTol = 0.002f;        // 2 mm : tolérance sur les bords du recouvrement

    // seulement la structure fixe de la maison (murs, sols, plafonds, plinthes, corniches, lambris, encadrements) : un objet
    // qu'on peut déplacer ou prendre ne doit ni perdre une face, ni en cacher une (il laisserait un trou en bougeant)
    static readonly string[] Structure = { "Wall", "Floor", "Ceiling", "Trim", "Cornice", "Wainscot", "Frame", "Backsplash" };
    static readonly string[] Skip = { "_LOD", "Exterior", "Environment", "Apoc_" };

    class Tri { public MeshFilter mf; public int sub, index; public int axis, sign; public float d; public Vector2 a, b, c; public bool removed; }

    public static int Apply(Transform root)
    {
        var filters = root.GetComponentsInChildren<MeshFilter>(true)
            .Where(f => f.sharedMesh && f.sharedMesh.isReadable && f.GetComponent<MeshRenderer>() && Structure.Any(s => f.name.Contains(s)) && !Skip.Any(s => f.name.Contains(s)))
            .OrderBy(f => f.name, System.StringComparer.Ordinal).ToList();

        // triangles alignés sur les axes, rangés par plan (axe, sens, position arrondie à 2 mm)
        var all = new List<Tri>();
        var byPlane = new Dictionary<(int, int, int), List<Tri>>();
        foreach (var mf in filters)
        {
            var m = mf.sharedMesh; var v = m.vertices; var M = mf.transform.localToWorldMatrix;
            for (int s = 0; s < m.subMeshCount; s++)
            {
                var t = m.GetTriangles(s);
                for (int i = 0; i < t.Length; i += 3)
                {
                    Vector3 p0 = M.MultiplyPoint3x4(v[t[i]]), p1 = M.MultiplyPoint3x4(v[t[i + 1]]), p2 = M.MultiplyPoint3x4(v[t[i + 2]]);
                    var n = Vector3.Cross(p1 - p0, p2 - p0);
                    if (n.sqrMagnitude < 1e-10f) continue;
                    n.Normalize();
                    int ax = -1;
                    for (int k = 0; k < 3; k++) if (Mathf.Abs(n[k]) > 0.999f) ax = k;
                    if (ax < 0) continue;
                    int u = (ax + 1) % 3, w = (ax + 2) % 3;
                    var tri = new Tri { mf = mf, sub = s, index = i, axis = ax, sign = n[ax] > 0 ? 1 : -1, d = (p0[ax] + p1[ax] + p2[ax]) / 3f,
                        a = new Vector2(p0[u], p0[w]), b = new Vector2(p1[u], p1[w]), c = new Vector2(p2[u], p2[w]) };
                    all.Add(tri);
                    var key = (ax, tri.sign, Mathf.RoundToInt(tri.d / PlaneTol));
                    if (!byPlane.TryGetValue(key, out var list)) byPlane[key] = list = new List<Tri>();
                    list.Add(tri);
                }
            }
        }

        // un triangle est recouvert si ses trois sommets (ramenés de 2 mm vers son centre) sont dans des triangles d'un autre objet
        foreach (var t in all)
        {
            var others = new List<Tri>();
            int k0 = Mathf.RoundToInt(t.d / PlaneTol);
            for (int dk = -1; dk <= 1; dk++)
                if (byPlane.TryGetValue((t.axis, t.sign, k0 + dk), out var l))
                    others.AddRange(l.Where(o => o.mf != t.mf && !o.removed && Mathf.Abs(o.d - t.d) <= PlaneTol));
            if (others.Count == 0) continue;
            var center = (t.a + t.b + t.c) / 3f;
            bool covered = true;
            foreach (var p in new[] { t.a, t.b, t.c })
            {
                var q = p + (center - p).normalized * Mathf.Min(EdgeTol, (center - p).magnitude);
                if (!others.Any(o => Inside(q, o.a, o.b, o.c))) { covered = false; break; }
            }
            if (covered) t.removed = true;
        }

        // copies des maillages concernés, sans les triangles retirés
        var removedBy = all.Where(t => t.removed).GroupBy(t => t.mf).ToList();
        if (removedBy.Count > 0 && !AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Generated", "Recouvrements");
        int total = 0;
        foreach (var g in removedBy)
        {
            var mf = g.Key; var src = mf.sharedMesh;
            var gone = new HashSet<(int, int)>(g.Select(t => (t.sub, t.index)));
            var mesh = Object.Instantiate(src);
            mesh.name = mf.name + "_SansRecouvrement";
            for (int s = 0; s < src.subMeshCount; s++)
            {
                var tris = src.GetTriangles(s);
                var keep = new List<int>(tris.Length);
                for (int i = 0; i < tris.Length; i += 3)
                    if (!gone.Contains((s, i))) { keep.Add(tris[i]); keep.Add(tris[i + 1]); keep.Add(tris[i + 2]); }
                mesh.SetTriangles(keep, s);
            }
            var path = Dir + "/" + mf.name + ".asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            var mc = mf.GetComponent<MeshCollider>();
            if (mc && mc.sharedMesh == src) mc.sharedMesh = mesh;
            mf.sharedMesh = mesh;
            total += gone.Count;
        }
        Debug.Log($"[RoomBuilder] faces superposées retirées : {total} triangles sur {removedBy.Count} objets ("
                  + string.Join(", ", removedBy.Select(g => g.Key.name + " " + g.Count())) + ")");
        return total;
    }

    static bool Inside(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross(p, a, b), d2 = Cross(p, b, c), d3 = Cross(p, c, a);
        const float e = 1e-6f;
        bool neg = d1 < -e || d2 < -e || d3 < -e, pos = d1 > e || d2 > e || d3 > e;
        return !(neg && pos);
    }

    static float Cross(Vector2 p, Vector2 a, Vector2 b) => (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
}
