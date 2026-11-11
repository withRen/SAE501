using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// [SAE501] Corrections de level design de l'arène « Alvéole 17 » (Arena1v1_Bunker), relevées en revue (page wiki « Arenes-du-duel ») :
//   1. escaliers des passerelles : la dernière marche finissait 0,40 m SOUS la passerelle et sous sa dalle (marche > stepOffset 0,35 :
//      on ne montait qu'en sautant, impossible au casque sans saut) → 11 marches régulières de 0,217 m, la dernière contre la passerelle ;
//   2. caisses du centre : caisse (0,95 m, escaladable avec le saut de 0,75 m) + caisse empilée (1,45 m) collées au mur central (2,2 m)
//      = escalier vers le haut du mur, qui domine toute l'arène → caisse du bas portée à 1,20 m (non escaladable), la pile suit ;
//   3. gravats : petits blocs (≤ 0,5 m) avec collider convexe = marchepieds pour monter sur les caisses et accrochages du joueur → sans collider ;
//   4. socles « Pickup » vides (armes, munitions, spécial) : plus rien n'y apparaît (le kit est donné en main par DuelLoadout), ils trompaient
//      le joueur et servaient de marches → retirés ;
//   5. flaque « Puddle_Leak_NW » : maillage vide et matériau « Lit » par défaut (non URP du projet) → remplacée par le miroir de la flaque SE
//      (arène symétrique par rotation de 180°) ;
//   6. flaques : reflet du ciel par défaut et reflet spéculaire = aplats blanc-bleu → réflexions coupées (comme les flaques de MainScene) ;
//   7. lumière principale : ombres douces « de soleil » dans un hall fermé sous dalle (et coût d'ombres sur Quest) → sans ombres ;
//   8. ligne de vue d'un spawn à l'autre (26 m) par la brèche du mur central (1,25 m, sous les yeux à 1,6 m) : au sniper, mort à l'apparition
//      → un écran de sas en béton (2,4 m, bande jaune) à 2,4 m devant chaque spawn, on en sort par la gauche ou la droite.
// Les corrections sont idempotentes : menu SAE501 → Arène 1v1 → Corriger le level design, ou à chaque Build Arena 1v1 Scene.
public static class Arena1v1Fixes
{
    const float CatwalkTop = 2.6f;
    const int Steps = 11;

    [MenuItem("SAE501/Arène 1v1/Corriger le level design (sans reconstruire)")]
    public static void ApplyToScene()
    {
        var scene = EditorSceneManager.OpenScene(Arena1v1Builder.ScenePath);
        Apply();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    public static void Apply()
    {
        var geo = GameObject.Find("Bunker_Geometry");
        if (!geo) { Debug.LogError("[Arène 1v1] Bunker_Geometry introuvable"); return; }
        var all = geo.GetComponentsInChildren<Transform>(true);

        int stairs = FixStairs(all, "Stairs_West_North_Step", +1f) + FixStairs(all, "Stairs_East_South_Step", -1f);
        int crates = FixCrates(all);

        int rubble = 0;
        foreach (var t in all.Where(t => t.name.StartsWith("Rubble_")))
            foreach (var c in t.GetComponents<Collider>()) { Object.DestroyImmediate(c); rubble++; }

        int pads = 0;
        foreach (var t in all.Where(t => t.name.StartsWith("Pickup_") && t.gameObject.activeSelf)) { t.gameObject.SetActive(false); pads++; }

        FixPuddles(all);

        SpawnShields();

        int low = LowPolyProps(all);
        RebuildProps(all);
        int cols = SimplePropColliders(all);
        int merged = MergeMap();
        Debug.Log($"[Arène 1v1] optimisation : {low} cylindres refaits en low-poly, {cols} MeshCollider de props remplacés, {merged} matériaux fusionnés");

        var sun = GameObject.Find("Light_Main_BunkerFill");
        if (sun) sun.GetComponent<Light>().shadows = LightShadows.None;

        Debug.Log($"[Arène 1v1] level design corrigé : {stairs} marches, {crates} caisses, {rubble} colliders de gravats retirés, {pads} socles vides masqués, flaques et ombres");
    }

    // marches posées du bas (|x| grand) vers le haut, la dernière collée au bout de la passerelle (|x| = 6)
    static int FixStairs(Transform[] all, string prefix, float side)
    {
        var steps = all.Where(t => t.name.StartsWith(prefix) && t.GetComponent<Renderer>()).OrderByDescending(t => Mathf.Abs(t.GetComponent<Renderer>().bounds.center.x)).ToList();
        if (steps.Count == 0) return 0;            // décor déjà fusionné : marches déjà posées
        if (steps.Count != Steps) { Debug.LogWarning($"[Arène 1v1] {prefix} : {steps.Count} marches (attendu {Steps})"); if (steps.Count == 0) return 0; }
        float rise = CatwalkTop / (steps.Count + 1);
        for (int k = 0; k < steps.Count; k++)
        {
            var r = steps[k].GetComponent<Renderer>();
            var b = r.bounds;
            var target = new Vector3(side * (6.2f + 0.35f * (steps.Count - 1 - k)), (k + 1) * rise - b.extents.y, b.center.z);
            steps[k].position += target - b.center;
        }
        return steps.Count;
    }

    // caisse du bas à 1,20 m (au-dessus des 1,05 m atteints avec le saut), la caisse empilée reste posée dessus
    static int FixCrates(Transform[] all)
    {
        int n = 0;
        foreach (var side in new[] { "West", "East" })
        {
            var mid = all.FirstOrDefault(t => t.name.StartsWith("Crate_" + side + "_Mid"));
            var stack = all.FirstOrDefault(t => t.name.StartsWith("Crate_Stack_" + side));
            if (!mid) continue;
            var r = mid.GetComponent<Renderer>();
            if (!r) continue;                      // décor déjà fusionné : corrections déjà faites
            var b = r.bounds;
            float top = b.max.y;
            if (top < 1.19f)
            {
                float k = 1.2f / Mathf.Max(0.01f, top);
                // mise à l'échelle verticale autour du sol (le pivot du nœud Blender n'est pas forcément au sol)
                float floorY = b.min.y;
                var p = mid.position; mid.position = new Vector3(p.x, floorY + (p.y - floorY) * k, p.z);
                // axe local vertical (les nœuds Blender sont tournés de 270° sur X : c'est leur Z)
                var up = mid.InverseTransformDirection(Vector3.up);
                int axis = Mathf.Abs(up.x) > Mathf.Abs(up.y) ? (Mathf.Abs(up.x) > Mathf.Abs(up.z) ? 0 : 2) : (Mathf.Abs(up.y) > Mathf.Abs(up.z) ? 1 : 2);
                var s = mid.localScale; s[axis] *= k; mid.localScale = s;
                if (stack) stack.position += Vector3.up * (1.2f - top);
                n++;
            }
        }
        Physics.SyncTransforms();
        return n;
    }

    // ---------------------------------------------------------------- optimisation Quest
    // 9. barils et tuyaux du modèle Blender : cylindres de 32 faces (124 triangles) aux couvercles en longs triangles fins depuis un seul sommet
    //    → cylindre partagé de 12 faces (48 triangles, couvercles en éventail depuis le centre), même taille, cerclages en normal map ;
    // 10. props (caisses, barils, hérissons) en MeshCollider convexe → BoxCollider / CapsuleCollider ;
    // 11. ~200 objets dessinés un par un → fusion par matériau (≈ 10 appels de dessin), les colliders restent sur les objets d'origine.
    static int LowPolyProps(Transform[] all)
    {
        int n = 0;
        foreach (var t in all.Where(t => t.name.StartsWith("Pipe_")))
        {
            var mf = t.GetComponent<MeshFilter>();
            if (!mf || !mf.sharedMesh || mf.sharedMesh.name.StartsWith("Cylindre")) continue;
            var b = mf.sharedMesh.bounds;                      // repère local du maillage
            var e = b.extents;
            int axis = 0; float best = -1f;
            for (int i = 0; i < 3; i++)
            {
                float other = (e[(i + 1) % 3] + e[(i + 2) % 3]) / 2f;
                float d = Mathf.Abs(e[i] - other);
                if (d > best) { best = d; axis = i; }
            }
            float r = (e[(axis + 1) % 3] + e[(axis + 2) % 3]) / 2f, h = e[axis] * 2f;
            int seg = r < 0.15f ? 8 : 12;
            mf.sharedMesh = AxisCylinder(r, h, seg, axis, b.center);
            n++;
        }
        return n;
    }

    // cylindre d'axe local donné, centré sur c (maillage partagé par dimensions)
    static readonly System.Collections.Generic.Dictionary<string, Mesh> axisCyl = new System.Collections.Generic.Dictionary<string, Mesh>();
    static Mesh AxisCylinder(float r, float h, int seg, int axis, Vector3 c)
    {
        string k = $"{r:F2}_{h:F2}_{seg}_{axis}_{c.x:F2}_{c.y:F2}_{c.z:F2}";
        if (axisCyl.TryGetValue(k, out var cached) && cached) return cached;
        var src = ArenaKit.SharedCylinder(r, h, seg);
        var dir = Vector3.zero; dir[axis] = 1f;
        var rot = Quaternion.FromToRotation(Vector3.up, dir);
        var v = src.vertices; var nn = src.normals;
        for (int i = 0; i < v.Length; i++) { v[i] = rot * (v[i] - Vector3.up * h / 2f) + c; nn[i] = rot * nn[i]; }
        var m = new Mesh { name = "Cylindre_" + k };
        m.vertices = v; m.normals = nn; m.uv = src.uv; m.triangles = src.triangles;
        m.RecalculateTangents(); m.RecalculateBounds();
        return axisCyl[k] = m;
    }

    // ---------------------------------------------------------------- props refaits (retour de Rahim, journal 184)
    // 12. barils du modèle Blender : posés en partie les uns dans les autres et contre le pilier, un baril couché à travers un autre
    //     → barils masqués, remplacés par des barils propres (16 faces lisses, 64 triangles) posés au sol sans se toucher :
    //     2 debout + 1 couché par groupe (NO, SE), 1 debout dans chaque coin des spawns ;
    // 13. barrières cassées (Broken_Barrier, ~1 700 triangles chacune) : plaques qui se croisent, faces vues de l'intérieur, texture étirée
    //     → mur de béton fermé en 3 tronçons de hauteur décroissante (cassure), même emprise, UV en mètres, 36 triangles par tronçon ;
    //     couvert de 2,2 m conservé au plus haut ; collider = les 3 boîtes ;
    // 14. gravats et hérissons tchèques : reposés au sol (ils flottaient, jusqu'à 15 cm pour les hérissons).
    static void RebuildProps(Transform[] all)
    {
        var map = GameObject.Find("Arena1v1_Map");
        if (!map || map.transform.Find("Props_Refaits")) return;
        var root = new GameObject("Props_Refaits").transform;
        root.SetParent(map.transform, false);
        var baril = ArenaKit.Gen("Arene_Baril_Rouille", "Baril", new Color(0.55f, 0.28f, 0.16f), 0.4f, 0.2f, 1f);
        var barilSombre = ArenaKit.Gen("Arene_Baril_Sombre", "Baril", new Color(0.3f, 0.32f, 0.36f), 0.4f, 0.2f, 1f);
        var beton = AssetDatabase.LoadAssetAtPath<Material>(Arena1v1Builder.MatDir + "/Mat_Concrete_Wall.mat");

        foreach (var t in all.Where(t => t.name.StartsWith("Barrel"))) t.gameObject.SetActive(false);
        const float R = 0.29f, Hb = 0.9f;
        foreach (float s in new[] { 1f, -1f })          // s = 1 : groupe NO (x > 0, z < 0), s = -1 : son miroir SE
        {
            ArenaKit.Cylinder(root, "Baril", new Vector3(s * 3.6f, 0f, s * -6.9f), R, Hb, baril, true, 16);
            ArenaKit.Cylinder(root, "Baril", new Vector3(s * 4.25f, 0f, s * -6.6f), R, Hb, baril, true, 16);
            ArenaKit.LyingCylinder(root, "Baril_Couche", new Vector3(s * 3.6f, R, s * -5.95f), R, Hb, 0f, baril);
            ArenaKit.Cylinder(root, "Baril_Coin_Spawn", new Vector3(s * 13.7f, 0f, s * -6.0f), R, Hb, barilSombre, true, 16);
        }

        foreach (var t in all.Where(t => t.name.StartsWith("Broken_Barrier") && t.GetComponent<MeshFilter>()))
        {
            var mf = t.GetComponent<MeshFilter>();
            var v = mf.sharedMesh.vertices.Select(p => t.TransformPoint(p)).ToArray();
            // axe principal dans le plan du sol (analyse en composantes principales sur x, z)
            float mx = v.Average(p => p.x), mz = v.Average(p => p.z);
            float sxx = v.Sum(p => (p.x - mx) * (p.x - mx)), szz = v.Sum(p => (p.z - mz) * (p.z - mz)), sxz = v.Sum(p => (p.x - mx) * (p.z - mz));
            float ang = 0.5f * Mathf.Atan2(2f * sxz, sxx - szz);
            var d = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
            var n = new Vector3(-d.z, 0f, d.x);
            float a0 = v.Min(p => Vector3.Dot(p, d)), a1 = v.Max(p => Vector3.Dot(p, d));
            float b0 = v.Min(p => Vector3.Dot(p, n)), b1 = v.Max(p => Vector3.Dot(p, n));
            float H = v.Max(p => p.y);
            float L = a1 - a0, T = Mathf.Clamp(b1 - b0, 0.35f, 0.5f);
            var c = d * (a0 + a1) / 2f + n * (b0 + b1) / 2f;
            float yaw = Mathf.Atan2(-d.z, d.x) * Mathf.Rad2Deg;
            // 3 tronçons : 55 % de la longueur à pleine hauteur, puis 25 % à 80 %, puis 20 % à 60 % (cassure)
            float[] part = { 0.55f, 0.25f, 0.20f }, haut = { 1f, 0.8f, 0.6f };      // 2,2 / 1,76 / 1,32 m : le plus bas dépasse déjà les ~1,05 m escaladables, on ne monte sur aucun
            float start = -L / 2f;
            for (int i = 0; i < 3; i++)
            {
                float len = L * part[i], h = H * haut[i];
                var center = c + d * (start + len / 2f) + Vector3.up * h / 2f;
                ArenaKit.Box(root, t.name + "_Bloc" + i, center, new Vector3(len, h, T), beton, yaw);
                start += len;
            }
            t.gameObject.SetActive(false);
        }

        // gravats et hérissons : reposés sur le sol (base à y = 0)
        foreach (var t in all.Where(t => (t.name.StartsWith("Rubble_") || t.name.StartsWith("Czech_Hedgehog")) && t.GetComponent<Renderer>()))
        {
            var b = t.GetComponent<Renderer>().bounds;
            if (Mathf.Abs(b.min.y) > 0.01f) t.position -= Vector3.up * b.min.y;
        }
    }

    static int SimplePropColliders(Transform[] all)
    {
        int n = 0;
        foreach (var mc in all.Select(t => t.GetComponent<MeshCollider>()).Where(c => c && c.convex))
        {
            var go = mc.gameObject;
            var mf = go.GetComponent<MeshFilter>();
            var mesh = mf && mf.sharedMesh ? mf.sharedMesh : mc.sharedMesh;
            Object.DestroyImmediate(mc);
            if (!mesh || go.name.StartsWith("Rubble_")) continue;     // gravats : décor sans collider (correction 3)
            var b = mesh.bounds;
            var box = go.AddComponent<BoxCollider>();
            box.center = b.center; box.size = b.size;
            n++;
        }
        return n;
    }

    // fusion par matériau du décor (modèle Blender, écrans de sas) ; une seule fois (idempotent)
    static int MergeMap()
    {
        var map = GameObject.Find("Arena1v1_Map");
        if (!map || map.transform.Find("Rendu_Fusionne")) return 0;
        var geo = map.transform.Find("Bunker_Geometry");
        bool Roof(MeshRenderer r) => r.name.StartsWith("Ceiling") || r.name.StartsWith("HVAC") || r.name.StartsWith("Fixture");
        // plafond à part (vue en coupe des captures, et le plafond n'est jamais vu en même temps que tout le sol)
        int n = ArenaKit.MergeByMaterial(map.transform, "Arena1v1_Bunker_Plafond", r => r.transform.IsChildOf(geo) && Roof(r));
        map.transform.Find("Rendu_Fusionne").name = "Plafond_Fusionne";
        n += ArenaKit.MergeByMaterial(map.transform, "Arena1v1_Bunker",
            r => (r.transform.IsChildOf(geo) || r.transform.parent.name == "Ecrans_Sas" || r.transform.parent.name == "Props_Refaits") && !Roof(r));
        return n;
    }

    // écrans de sas : refaits à chaque passage (idempotent)
    static void SpawnShields()
    {
        var map = GameObject.Find("Arena1v1_Map");
        if (!map) return;
        if (map.transform.Find("Ecrans_Sas")) return;      // déjà posés (et peut-être fusionnés au rendu : ne pas doubler)
        var root = new GameObject("Ecrans_Sas").transform;
        root.SetParent(map.transform, false);
        var wall = AssetDatabase.LoadAssetAtPath<Material>(Arena1v1Builder.MatDir + "/Mat_Concrete_Wall.mat");
        var hazard = ArenaKit.Gen("Arene_Bandes_Danger", "Danger", Color.white, 0.3f, 0f, 1f);      // bandes jaunes et noires (Tools/textures/generer_arenes.py)
        // bandes des portes blindées : même texture (Mat_Hazard_Yellow n'avait qu'une couleur unie)
        var portes = AssetDatabase.LoadAssetAtPath<Material>(Arena1v1Builder.MatDir + "/Mat_Hazard_Yellow.mat");
        if (portes && hazard)
        {
            portes.SetTexture("_BaseMap", hazard.GetTexture("_BaseMap"));
            portes.SetTextureScale("_BaseMap", hazard.GetTextureScale("_BaseMap"));
            portes.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(portes);
        }
        foreach (float side in new[] { -1f, 1f })
        {
            string n = side < 0 ? "J1" : "J2";
            ArenaKit.Box(root, "Ecran_Sas_" + n, new Vector3(side * 10.6f, 1.2f, 0f), new Vector3(0.5f, 2.4f, 2.6f), wall);
            ArenaKit.Box(root, "Ecran_Sas_" + n + "_Bande", new Vector3(side * 10.6f, 2.0f, 0f), new Vector3(0.52f, 0.3f, 2.62f), hazard, 0f, false);
        }
    }

    static void FixPuddles(Transform[] all)
    {
        var broken = all.FirstOrDefault(t => t.name.StartsWith("Puddle_Leak_NW") && !t.name.EndsWith("_Miroir"));
        var se = all.FirstOrDefault(t => t.name.StartsWith("Puddle_Leak_SE"));
        if (broken && broken.gameObject.activeSelf)
        {
            var mf = broken.GetComponent<MeshFilter>();
            if (!mf || !mf.sharedMesh || mf.sharedMesh.vertexCount == 0) broken.gameObject.SetActive(false);
        }
        if (se && !all.Any(t => t.name == "Puddle_Leak_NW_Miroir"))
        {
            var copy = Object.Instantiate(se.gameObject, se.parent);
            copy.name = "Puddle_Leak_NW_Miroir";
            // rotation de 180° autour de l'axe vertical passant par le centre de l'arène
            copy.transform.position = new Vector3(-se.position.x, se.position.y, -se.position.z);
            copy.transform.rotation = Quaternion.Euler(0f, 180f, 0f) * se.rotation;
        }
        var mat = AssetDatabase.LoadAssetAtPath<Material>(Arena1v1Builder.MatDir + "/Mat_Water_Puddle.mat");
        if (mat)
        {
            mat.SetFloat("_EnvironmentReflections", 0f);
            mat.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            mat.SetFloat("_SpecularHighlights", 0f);
            mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            mat.SetFloat("_Smoothness", 0.7f);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
        }
    }
}
