using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Trous du toit du bunker (journal 150).
// Dans la carte Blender, les deux trous du plafond (au-dessus des lits et de l'établi, là où tombent les zombies du toit et
// la caisse de la radio) sont des carrés nets découpés dans la dalle, avec 9 lianes en boîtes sombres qui pendent de
// chacun (on les prenait pour des tiges). Ici, sans toucher au FBX :
//   1. les lianes qui pendent des trous sont retirées (copie du maillage Apoc_Vines_And_Moss ; les taches au sol restent) ;
//   2. un bord de béton cassé remplit les coins du carré : l'ouverture devient irrégulière (bord ébréché, enduit tombé
//      sous la dalle, tranche de béton plus sombre) ; ses faces du dessus et du dessous sont dans le plan de la dalle,
//      avec les mêmes UV que le plafond, donc sans raccord visible ;
//   3. une auréole noircie (suie, saleté, coulures) sous le plafond autour du trou ;
//   4. quelques gravats au sol, sous le trou (pas sur les meubles).
// Coût au casque : ~800 triangles de bord et ~330 d'auréole par trou, un matériau opaque de plus et un transparent, pas de collision
// (le NavMesh, les zombies et la caisse ne voient que les collisions : rien ne change pour eux).
// Appelé par RoomBuilder après la création des collisions de la carte.
public static class TrousToit
{
    const string Dir = "Assets/Generated/TrousToit";
    const float Bottom = 2.697f, Top = 2.797f;          // dalle du plafond (FBX)
    const float UvScale = -0.4f;                        // UV du plafond : u = -0,4 x, v = -0,4 z (2,5 m par répétition)

    // trous découpés dans Bunker_Ceiling (x min, x max, z min, z max) ; centre où la caisse de la radio passe (null : aucun)
    static readonly (string name, float x0, float x1, float z0, float z1, Vector2? drop, int seed)[] Holes =
    {
        ("Lits", 3.3f, 4.5f, 7.3f, 8.5f, null, 11),
        ("Etabli", -1.8f, -0.5f, 11.6f, 12.8f, new Vector2(-1.05f, 12.15f), 23),   // RoomBuilder.SupplyDrop
    };
    const float CrateClear = 0.36f;                     // demi-diagonale de la caisse (0,5 x 0,36 m) + marge

    public static void Apply(Transform root)
    {
        var ceiling = root.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.name == "Bunker_Ceiling");
        if (!ceiling) { Debug.LogWarning("[TrousToit] Bunker_Ceiling introuvable : trous du toit inchangés"); return; }
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Generated", "TrousToit");

        RemoveHangingVines(root);
        foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(t => t.name.EndsWith("_CeilingVines")).ToList()) Object.DestroyImmediate(t.gameObject);   // lianes du plafond de la salle du générateur

        // Rahim (journal 173) : trous bouchés. Un bouchon de la même épaisseur que la dalle et du même matériau remplit chaque carré ;
        // plus de bord cassé, d'auréole ni de gravats (le code qui les fabriquait reste plus bas, inutilisé, pour un retour en arrière).
        var plaster = ceiling.sharedMaterial;
        var holder = new GameObject("Bouchons_Toit").transform;
        holder.SetParent(root, false);
        foreach (var h in Holes)
        {
            var plug = new GameObject("Bouchon_" + h.name);
            plug.transform.SetParent(holder, false);
            plug.AddComponent<MeshFilter>().sharedMesh = Save(PlugMesh(h), "Bouchon_" + h.name);
            var mr = plug.AddComponent<MeshRenderer>();
            mr.sharedMaterial = plaster;
            mr.receiveGI = ReceiveGI.LightProbes;
            GameObjectUtility.SetStaticEditorFlags(plug, StaticEditorFlags.BatchingStatic);
        }
        Debug.Log($"[TrousToit] {Holes.Length} trous du toit bouchés");
    }

    // dalle pleine : face du dessous (vers le bas) et du dessus (vers le haut), UV du plafond ; 1 cm de recouvrement sur chaque bord
    static Mesh PlugMesh((string name, float x0, float x1, float z0, float z1, Vector2? drop, int seed) h)
    {
        const float e = 0.01f;
        float x0 = h.x0 - e, x1 = h.x1 + e, z0 = h.z0 - e, z1 = h.z1 + e;
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var n = new List<Vector3>(); var t = new List<int>();
        void Face(float y, Vector3 normal, bool flip)
        {
            int o = v.Count;
            foreach (var (x, z) in new[] { (x0, z0), (x1, z0), (x1, z1), (x0, z1) })
            { v.Add(new Vector3(x, y, z)); uv.Add(new Vector2(UvScale * x, UvScale * z)); n.Add(normal); }
            if (flip) t.AddRange(new[] { o, o + 1, o + 2, o, o + 2, o + 3 }); else t.AddRange(new[] { o, o + 2, o + 1, o, o + 3, o + 2 });
        }
        Face(Bottom, Vector3.down, true);
        Face(Top, Vector3.up, false);
        var m = new Mesh { name = "Bouchon_" + h.name };
        m.SetVertices(v); m.SetUVs(0, uv); m.SetNormals(n); m.SetTriangles(t, 0);
        m.RecalculateBounds();
        return m;
    }

    // ---------- code des anciens trous (inutilisé) ----------
    static void ApplyAncienTrou(Transform root, MeshRenderer ceiling)
    {
        var plaster = ceiling.sharedMaterial;
        var broken = BrokenMaterial(plaster);
        var soot = SootMaterial();
        var holder = new GameObject("Trous_Toit").transform;
        holder.SetParent(root, false);
        foreach (var h in Holes)
        {
            var rnd = new System.Random(h.seed);
            var ring = Ring(h, rnd);
            var rim = new GameObject("Bord_Casse_" + h.name);
            rim.transform.SetParent(holder, false);
            rim.AddComponent<MeshFilter>().sharedMesh = Save(RimMesh(ring), "Bord_" + h.name);
            var mr = rim.AddComponent<MeshRenderer>();
            mr.sharedMaterials = new[] { plaster, broken };
            GameObjectUtility.SetStaticEditorFlags(rim, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);

            var halo = new GameObject("Aureole_" + h.name);
            halo.transform.SetParent(holder, false);
            halo.AddComponent<MeshFilter>().sharedMesh = Save(SootMesh(ring, rnd), "Aureole_" + h.name);
            var sr = halo.AddComponent<MeshRenderer>();
            sr.sharedMaterial = soot;
            sr.shadowCastingMode = ShadowCastingMode.Off;
            sr.receiveShadows = false;
            sr.lightProbeUsage = LightProbeUsage.Off;
            GameObjectUtility.SetStaticEditorFlags(halo, StaticEditorFlags.BatchingStatic);

            Rubble(holder, h, broken, rnd);
        }
    }

    // ---------- 1. lianes ----------
    static void RemoveHangingVines(Transform root)
    {
        var mf = root.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f => f.name == "Apoc_Vines_And_Moss");
        if (!mf || !mf.sharedMesh || !mf.sharedMesh.isReadable) return;
        var src = mf.sharedMesh; var v = src.vertices; var M = mf.transform.localToWorldMatrix;
        var mesh = Object.Instantiate(src);
        mesh.name = "Apoc_Vines_And_Moss_SansTiges";
        int gone = 0;
        for (int s = 0; s < src.subMeshCount; s++)
        {
            var t = src.GetTriangles(s);
            var keep = new List<int>(t.Length);
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 a = M.MultiplyPoint3x4(v[t[i]]), b = M.MultiplyPoint3x4(v[t[i + 1]]), c = M.MultiplyPoint3x4(v[t[i + 2]]);
                var m = (a + b + c) / 3f;
                // sous le trou (carré + 25 cm) et en l'air : liane qui pend ; les taches de mousse au sol (y < 5 cm) restent
                // journal 173 : partout, plus seulement sous les trous (Rahim : « les éléments qui tombent du toit », des tiges sombres pendaient aussi dans le couloir)
                bool hanging = m.y > 1.0f;
                if (hanging) { gone++; continue; }
                keep.Add(t[i]); keep.Add(t[i + 1]); keep.Add(t[i + 2]);
            }
            mesh.SetTriangles(keep, s);
        }
        if (gone == 0) return;
        mf.sharedMesh = Save(mesh, "Lianes_SansTiges");
        var mc = mf.GetComponent<MeshCollider>();
        if (mc && mc.sharedMesh == src) mc.sharedMesh = mf.sharedMesh;
        Debug.Log($"[TrousToit] lianes qui pendaient des trous du toit retirées : {gone} triangles");
    }

    // ---------- 2. bord cassé ----------
    // Un point du contour par pas le long du carré (coins compris). Pour chacun, sur le rayon qui part du centre du trou :
    // bord de la dalle (o), enduit tombé sous la dalle (b2 : début de l'écaillage, b : lèvre du dessous, remontée), milieu
    // de la tranche (m, en saillie ou en retrait) et arête du dessus (t).
    struct P { public Vector3 o, b2, b, m, t; public Vector2 dir; public float rOut; }

    static List<P> Ring((string name, float x0, float x1, float z0, float z1, Vector2? drop, int seed) h, System.Random rnd)
    {
        var c = new Vector2((h.x0 + h.x1) * 0.5f, (h.z0 + h.z1) * 0.5f);
        // contour du carré, sens trigonométrique vu du dessus (x, z), ~6 cm par segment
        var corners = new[] { new Vector2(h.x1, h.z0), new Vector2(h.x1, h.z1), new Vector2(h.x0, h.z1), new Vector2(h.x0, h.z0) };
        var outline = new List<Vector2>();
        for (int k = 0; k < 4; k++)
        {
            Vector2 a = corners[k], b = corners[(k + 1) % 4];
            int n = Mathf.Max(4, Mathf.RoundToInt((b - a).magnitude / 0.06f));
            for (int i = 0; i < n; i++) outline.Add(Vector2.Lerp(a, b, i / (float)n));
        }
        float ph1 = R(rnd, 0f, 6.28f), ph2 = R(rnd, 0f, 6.28f), ph3 = R(rnd, 0f, 6.28f);
        float half = Mathf.Min(h.x1 - h.x0, h.z1 - h.z0) * 0.5f;
        var ring = new List<P>();
        float prevJ = 0f;
        foreach (var o in outline)
        {
            var d = o - c; float rOut = d.magnitude; var dir = d / rOut;
            float th = Mathf.Atan2(dir.y, dir.x);
            // rayon de l'ouverture : grandes bosses (2 et 3 par tour), éclats (5 par tour) et dents au hasard
            float j = Mathf.Lerp(prevJ, R(rnd, -0.05f, 0.05f), 0.6f); prevJ = j;
            float r = half * 0.95f + 0.06f * Mathf.Sin(2f * th + ph1) + 0.04f * Mathf.Sin(3f * th + ph2) + 0.025f * Mathf.Sin(5f * th + ph3) + j;
            if (rnd.NextDouble() < 0.12) r -= R(rnd, 0.03f, 0.08f);          // dent de béton qui avance dans le trou
            r = Mathf.Clamp(r, half * 0.75f, rOut - 0.04f);
            if (h.drop.HasValue)                                                // la caisse de la radio doit passer sans toucher
                while (r < rOut - 0.04f && Vector2.Distance(c + dir * r, h.drop.Value) < CrateClear) r += 0.01f;
            float rb = Mathf.Min(r + R(rnd, 0.03f, 0.09f), rOut - 0.02f);      // lèvre du dessous (l'enduit est tombé)
            float rb2 = Mathf.Min(rb + R(rnd, 0.03f, 0.10f), rOut - 0.008f);   // fin de l'écaillage
            float rt = Mathf.Min(r + R(rnd, 0.0f, 0.06f), rOut - 0.015f);      // arête du dessus
            Vector3 W(float rad, float y) => new Vector3(c.x + dir.x * rad, y, c.y + dir.y * rad);
            ring.Add(new P
            {
                dir = dir, rOut = rOut,
                o = W(rOut, 0f),
                b2 = W(rb2, Bottom),
                b = W(rb, Bottom + R(rnd, 0.012f, 0.035f)),
                m = W(r + R(rnd, -0.015f, 0.02f), (Bottom + Top) * 0.5f + R(rnd, -0.02f, 0.02f)),
                t = W(rt, Top - R(rnd, 0.0f, 0.02f)),
            });
        }
        return ring;
    }

    static Mesh RimMesh(List<P> ring)
    {
        var verts = new List<Vector3>(); var uvs = new List<Vector2>();
        var plaster = new List<int>(); var broken = new List<int>();
        int n = ring.Count;
        // dessous et dessus de la dalle : quads entre le bord du carré et l'arête, UV du plafond
        void Quad(List<int> tris, Vector3 a, Vector3 b, Vector3 c2, Vector3 d, bool up)
        {
            int i = verts.Count;
            foreach (var p in new[] { a, b, c2, d }) { verts.Add(p); uvs.Add(new Vector2(UvScale * p.x, UvScale * p.z)); }
            if (up) tris.AddRange(new[] { i, i + 2, i + 1, i, i + 3, i + 2 });
            else tris.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        }
        // tranche : UV le long du contour (x) et de la hauteur, pour que la texture de béton ne s'étire pas
        float run = 0f;
        for (int i = 0; i < n; i++)
        {
            var p = ring[i]; var q = ring[(i + 1) % n];
            Vector3 Ot(P x) => new Vector3(x.o.x, Top, x.o.z);
            Vector3 Ob(P x) => new Vector3(x.o.x, Bottom, x.o.z);
            Quad(plaster, Ot(p), Ot(q), q.t, p.t, true);         // dessus (vu du toit)
            Quad(plaster, Ob(p), Ob(q), q.b2, p.b2, false);      // dessous (vu du bunker)
            Quad(broken, p.b2, q.b2, q.b, p.b, false);           // enduit écaillé sous la dalle
            float seg = Vector3.Distance(p.m, q.m);
            int k = verts.Count;
            foreach (var (pt, u, vv) in new[] { (p.b, run, 0f), (q.b, run + seg, 0f), (q.m, run + seg, 0.05f), (p.m, run, 0.05f), (q.t, run + seg, 0.1f), (p.t, run, 0.1f) })
            { verts.Add(pt); uvs.Add(new Vector2(u * 2.5f, vv * 2.5f)); }
            // face tournée vers le centre du trou
            broken.AddRange(new[] { k, k + 1, k + 2, k, k + 2, k + 3, k + 3, k + 2, k + 4, k + 3, k + 4, k + 5 });
            run += seg;
        }
        var mesh = new Mesh { name = "Bord_Casse" };
        mesh.SetVertices(verts); mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(plaster, 0); mesh.SetTriangles(broken, 1);
        mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
        Unwrapping.GenerateSecondaryUVSet(mesh);          // lightmap
        return mesh;
    }

    // ---------- 3. auréole noircie ----------
    // Anneau 4 mm sous le plafond : opaque près de l'enduit écaillé, transparent à 15-45 cm du trou, avec des coulures.
    static Mesh SootMesh(List<P> ring, System.Random rnd)
    {
        var verts = new List<Vector3>(); var cols = new List<Color>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        const float y = Bottom - 0.004f;
        int n = ring.Count;
        float ph = R(rnd, 0f, 6.28f);
        foreach (var p in ring)
        {
            float th = Mathf.Atan2(p.dir.y, p.dir.x);
            float spread = 0.18f + 0.1f * Mathf.Sin(3f * th + ph) + R(rnd, 0f, 0.12f);
            if (rnd.NextDouble() < 0.1) spread += R(rnd, 0.1f, 0.25f);     // coulure
            Vector3 At(float rad) => new Vector3(p.o.x + p.dir.x * (rad - p.rOut), y, p.o.z + p.dir.y * (rad - p.rOut));
            float rIn = Vector2.Distance(new Vector2(p.b2.x, p.b2.z), new Vector2(p.o.x, p.o.z));
            var a = At(p.rOut - rIn); var b = At(p.rOut); var c = At(p.rOut + spread);
            foreach (var (pt, al) in new[] { (a, 0.85f), (b, 0.55f), (c, 0f) })
            { verts.Add(pt); cols.Add(new Color(1f, 1f, 1f, al)); uvs.Add(new Vector2(pt.x, pt.z) * 1.3f); }
        }
        for (int i = 0; i < n; i++)
        {
            int a = i * 3, b = ((i + 1) % n) * 3;
            for (int k = 0; k < 2; k++)    // vu d'en dessous : sens horaire vu du dessus
                tris.AddRange(new[] { a + k, b + k + 1, b + k, a + k, a + k + 1, b + k + 1 });
        }
        var mesh = new Mesh { name = "Aureole" };
        mesh.SetVertices(verts); mesh.SetColors(cols); mesh.SetUVs(0, uvs); mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }

    // ---------- 4. gravats ----------
    static void Rubble(Transform holder, (string name, float x0, float x1, float z0, float z1, Vector2? drop, int seed) h, Material mat, System.Random rnd)
    {
        var c = new Vector3((h.x0 + h.x1) * 0.5f, 2f, (h.z0 + h.z1) * 0.5f);
        var meshes = Enumerable.Range(0, 3).Select(i => Save(Chunk(rnd), $"Gravat_{h.name}_{i}")).ToArray();
        Physics.SyncTransforms();
        int placed = 0;
        for (int tries = 0; tries < 40 && placed < 9; tries++)
        {
            var off = Random2(rnd) * 0.7f;
            var from = c + new Vector3(off.x, 0f, off.y);
            if (!Physics.Raycast(from, Vector3.down, out var hit, 2.5f, ~0, QueryTriggerInteraction.Ignore) || hit.point.y > 0.12f) continue;   // meuble : pas de gravats dessus
            var g = new GameObject("Gravat_" + h.name + "_" + placed);
            g.transform.SetParent(holder, false);
            float s = R(rnd, 0.04f, 0.12f) * (placed < 2 ? 1.5f : 1f);
            g.transform.SetPositionAndRotation(hit.point + Vector3.up * s * 0.15f, Quaternion.Euler(R(rnd, -20f, 20f), R(rnd, 0f, 360f), R(rnd, -20f, 20f)));
            g.transform.localScale = new Vector3(s, s * R(rnd, 0.45f, 0.8f), s * R(rnd, 0.7f, 1.2f));
            g.AddComponent<MeshFilter>().sharedMesh = meshes[placed % meshes.Length];
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.receiveGI = ReceiveGI.LightProbes;            // petit objet : lumière des sondes (comme les petits décors de la carte)
            GameObjectUtility.SetStaticEditorFlags(g, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            placed++;
        }
    }

    // éclat de béton : octaèdre déformé (8 faces), facettes à plat
    static Mesh Chunk(System.Random rnd)
    {
        var p = new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back }
            .Select(v => v * R(rnd, 0.35f, 0.6f) + new Vector3(R(rnd, -0.1f, 0.1f), R(rnd, -0.1f, 0.1f), R(rnd, -0.1f, 0.1f))).ToArray();
        int[,] f = { { 2, 4, 0 }, { 2, 1, 4 }, { 2, 5, 1 }, { 2, 0, 5 }, { 3, 0, 4 }, { 3, 4, 1 }, { 3, 1, 5 }, { 3, 5, 0 } };
        var v2 = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
        for (int i = 0; i < 8; i++)
            for (int k = 0; k < 3; k++) { var q = p[f[i, k]]; v2.Add(q); uv.Add(new Vector2(q.x + q.y, q.z + q.y)); t.Add(v2.Count - 1); }
        var m = new Mesh { name = "Gravat" };
        m.SetVertices(v2); m.SetUVs(0, uv); m.SetTriangles(t, 0);
        m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }

    // ---------- matériaux ----------
    static Material BrokenMaterial(Material plaster)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Beton_Casse" };
        if (plaster)
        {
            if (plaster.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", plaster.GetTexture("_BaseMap"));
            if (plaster.HasProperty("_BumpMap") && plaster.GetTexture("_BumpMap"))
            {
                m.SetTexture("_BumpMap", plaster.GetTexture("_BumpMap"));
                m.SetFloat("_BumpScale", 1.6f);
                m.EnableKeyword("_NORMALMAP");
            }
        }
        m.SetColor("_BaseColor", new Color(0.36f, 0.34f, 0.31f));   // béton nu, sale, plus sombre que l'enduit
        m.SetFloat("_Smoothness", 0.08f);
        m.SetFloat("_Metallic", 0f);
        return SaveMat(m);
    }

    static Material SootMaterial()
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = "Aureole_Suie" };
        m.SetFloat("_Surface", 1f); m.SetFloat("_Blend", 0f);
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)RenderQueue.Transparent - 10;           // avant la poussière et les autres transparents
        m.SetTexture("_BaseMap", SootTexture());
        m.SetColor("_BaseColor", new Color(0.05f, 0.04f, 0.035f, 0.9f));
        return SaveMat(m);
    }

    // taches de suie : bruit de valeur sur 4 octaves, répétable (128 px)
    static Texture2D SootTexture()
    {
        const int N = 128;
        var rnd = new System.Random(7);
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, true) { name = "Suie", wrapMode = TextureWrapMode.Repeat };
        var px = new Color32[N * N];
        float[][] grids = Enumerable.Range(0, 4).Select(o => Enumerable.Range(0, (4 << o) * (4 << o)).Select(_ => (float)rnd.NextDouble()).ToArray()).ToArray();
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float v = 0f, amp = 0.5f;
                for (int o = 0; o < 4; o++)
                {
                    int g = 4 << o; float fx = x * g / (float)N, fy = y * g / (float)N;
                    int x0 = (int)fx, y0 = (int)fy; float tx = Mathf.SmoothStep(0, 1, fx - x0), ty = Mathf.SmoothStep(0, 1, fy - y0);
                    float G(int i, int j) => grids[o][((j % g) * g) + (i % g)];
                    v += amp * Mathf.Lerp(Mathf.Lerp(G(x0, y0), G(x0 + 1, y0), tx), Mathf.Lerp(G(x0, y0 + 1), G(x0 + 1, y0 + 1), tx), ty);
                    amp *= 0.5f;
                }
                byte a = (byte)(Mathf.Clamp01((v - 0.25f) * 1.8f) * 255f);
                px[y * N + x] = new Color32(255, 255, 255, a);
            }
        tex.SetPixels32(px); tex.Apply(true);
        var path = Dir + "/Suie.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(tex, path);
        return tex;
    }

    static Material SaveMat(Material m)
    {
        var path = $"{Dir}/{m.name}.mat";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static Mesh Save(Mesh m, string file)
    {
        var path = $"{Dir}/{file}.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static float R(System.Random r, float a, float b) => a + (float)r.NextDouble() * (b - a);
    static Vector2 Random2(System.Random r) { float a = R(r, 0f, 6.2832f), d = Mathf.Sqrt((float)r.NextDouble()); return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * d; }
}
