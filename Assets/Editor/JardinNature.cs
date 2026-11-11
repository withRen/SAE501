using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Jardin et forêt autour de la maison (demande de Rahim : « les arbres sont horribles », « la texture du sol du jardin est horrible »).
//
// 1) Sol. Les deux terrains de la carte (Garden_Terrain_Vallons, Exterior_World_Terrain) avaient une seule image cuite dans Blender
//    (un aplat vert flou de 1024 px étalé sur 20 m et 98 m) et des normales recalculées sur des sommets séparés (facettes claires
//    sous la lune). Ici, sur une copie du maillage (le FBX n'est pas touché) :
//      - UV refaites par projection du dessus, en mètres (une répétition de texture tous les TileJardin / TileForet mètres) ;
//      - normales lissées (moyenne des sommets au même endroit) : plus de facettes ; tangentes recalculées pour la carte normale ;
//      - matériaux Poly Haven 1k (CC0, Rob Tuytel) : forrest_ground_01 (herbe négligée, brindilles) dans le jardin,
//        forest_leaves_02 (mousse, feuilles mortes) dans la forêt. Textures dans Assets/Nature/Sol, ASTC 6x6 sur Quest.
//    L'UV de lightmap (UV2) ne change pas.
//
// 2) Arbres. Les 8 arbres du jardin (Garden_RealTree_*) avaient été simplifiés à l'export de la carte (6 000 triangles par objet,
//    « collapse » global) : troncs en pointes noires étirées. La forêt autour (Exterior_Forest_Trunks / _Canopies) était le
//    blockout Blender : 36 sapins en cônes et arbres « sucette » sans texture. Les deux utilisent maintenant l'arbre refait par
//    Tools/blender/arbre_quest.py depuis l'original Poly Haven island_tree_02 (Assets/Nature/Arbres/Arbre_Quest.fbx) :
//      LOD0 ~19 000 triangles (tronc simplifié à part, une feuille = un quad), LOD1 ~6 700, puis un « imposteur » :
//      trois panneaux croisés (12 triangles) avec une image de l'arbre rendue ici une fois
//      (Assets/Generated/Nature/Arbre_Imposteur.png, refaite par le menu SAE501 > Jardin > Refaire l'imposteur des arbres),
//      puis rien dans le brouillard. Pas d'ombre portée, lumière des sondes.
//    Jardin : mêmes places, tailles et capsules qu'avant. Forêt : un arbre à la place de chaque tronc du blockout, tourné et mis à
//    l'échelle au hasard (graine fixe : même forêt à chaque construction), capsule sur le tronc.
// 3) Le terrain extérieur, qui ressortait à travers le jardin, est descendu sous lui.
// Appelé par RoomBuilder après MakeMapColliders et SinkTerrainUnderFloors, avant le NavMesh et les LOD (LodBuilder saute les objets qui ont déjà un LODGroup).
public static class JardinNature
{
    const string SolDir = "Assets/Nature/Sol";
    const string Dir = "Assets/Generated/Nature";
    const string ImpostorPath = Dir + "/Arbre_Imposteur.png";
    const float TileJardin = 2.5f;                       // m par répétition de la texture du jardin
    const float TileForet = 3.5f;

    // seuils de hauteur à l'écran (part de la hauteur de l'image) ; le brouillard (exp², 0,06) cache presque tout après 25 m
    const float Lod0Screen = 0.55f;      // forêt : LOD0 seulement tout près (on ne s'en approche qu'en fuyant par le portail)
    const float Lod1Screen = 0.22f;
    const float Lod0Jardin = 0.35f;      // jardin : on passe à côté des arbres
    const float Lod1Jardin = 0.12f;
    const float CullScreen = 0.01f;

    public static void Apply(Transform root)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Generated", "Nature");
        Sol(root);
        Arbres(root);
        AssetDatabase.SaveAssets();
    }

    // Vitres et flaques (transparentes, lisses à 0,6 ou plus) : sans sonde de réflexion, la vue Scène de l'éditeur leur fait refléter son ciel
    // par défaut bleu clair : fenêtres et flaques deviennent des aplats bleus qui cachent le décor. En jeu (fond noir, réflexions à 0) rien ne
    // change ; ici on coupe aussi les réflexions de l'environnement sur ces matériaux pour que l'éditeur montre la même chose.
    public static void NoSkyReflections(Transform root)
    {
        var done = new HashSet<Material>();
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            foreach (var m in r.sharedMaterials)
            {
                if (!m || !done.Add(m) || !m.HasProperty("_Surface") || m.GetFloat("_Surface") < 0.5f) continue;     // transparents seulement
                if (!m.HasProperty("_Smoothness") || m.GetFloat("_Smoothness") < 0.6f) continue;
                m.SetFloat("_EnvironmentReflections", 0f);
                m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
                // un matériau transparent prémultiplié ajoute son reflet spéculaire sans l'atténuer par l'alpha : la lune bleue
                // y fait des aplats bleu clair (flaques, vitres). On coupe aussi les reflets de la lumière principale.
                m.SetFloat("_SpecularHighlights", 0f);
                m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                EditorUtility.SetDirty(m);
            }
        Debug.Log($"[JardinNature] {done.Count} matériaux examinés : réflexions du ciel et reflets de la lune coupés sur les vitres et flaques");
    }

    // ---------------------------------------------------------------- sol
    static void Sol(Transform root)
    {
        var jardin = SolMaterial("Sol_Jardin", "forrest_ground_01_diff_1k", "forrest_ground_01_nor_gl_1k", 0.12f, 1f);
        var foret = SolMaterial("Sol_Foret", "forest_leaves_02_diffuse_1k", "forest_leaves_02_nor_gl_1k", 0.1f, 1f);
        int n = 0;
        if (jardin && Terrain(root, "Garden_Terrain_Vallons", jardin, TileJardin)) n++;
        if (foret && Terrain(root, "Exterior_World_Terrain", foret, TileForet, Find(root, "Garden_Terrain_Vallons"))) n++;
        Debug.Log($"[JardinNature] sol : {n} terrains avec textures Poly Haven, UV en mètres, normales lissées");
    }

    static Material SolMaterial(string name, string albedo, string normal, float smooth, float bump)
    {
        var a = Tex($"{SolDir}/{albedo}.jpg", false);
        var nm = Tex($"{SolDir}/{normal}.jpg", true);
        if (!a) { Debug.LogWarning($"[JardinNature] texture absente : {SolDir}/{albedo}.jpg"); return null; }
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        m.SetTexture("_BaseMap", a);
        m.SetColor("_BaseColor", new Color(0.85f, 0.85f, 0.85f));     // un peu assombri : nuit, et les images Poly Haven sont claires
        if (nm) { m.SetTexture("_BumpMap", nm); m.SetFloat("_BumpScale", bump); m.EnableKeyword("_NORMALMAP"); }
        m.SetFloat("_Smoothness", smooth);
        m.SetFloat("_Metallic", 0f);
        m.SetFloat("_EnvironmentReflections", 0f);       // sol mat : pas de reflets du ciel (un calcul de moins par pixel)
        m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        return SaveMat(m, $"{Dir}/{name}.mat");
    }

    static bool Terrain(Transform root, string name, Material mat, float tile, Transform above = null)
    {
        var t = Find(root, name);
        if (t == null) return false;
        var mf = t.GetComponent<MeshFilter>();
        if (!mf || !mf.sharedMesh || !mf.sharedMesh.isReadable) return false;
        var src = mf.sharedMesh;
        var mesh = Object.Instantiate(src);
        mesh.name = "Sol_" + name;
        var v = mesh.vertices;
        if (above) SinkUnder(t, v, above);
        mesh.vertices = v;
        mesh.RecalculateBounds();
        var uv = new Vector2[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            var w = t.TransformPoint(v[i]);
            uv[i] = new Vector2(w.x / tile, w.z / tile);
        }
        mesh.uv = uv;
        mesh.normals = SmoothNormals(mesh);
        mesh.RecalculateTangents();
        mesh = SaveMesh(mesh, $"{Dir}/Sol_{name}.asset");
        mf.sharedMesh = mesh;
        var mc = t.GetComponent<MeshCollider>();
        if (mc) mc.sharedMesh = mesh;
        var r = t.GetComponent<MeshRenderer>();
        r.sharedMaterials = Enumerable.Repeat(mat, mesh.subMeshCount).ToArray();
        return true;
    }

    // le terrain extérieur passe sous tout le jardin et ressort à travers lui par plaques (noires dans l'ancienne texture, bleu ciel
    // par reflet) : ses sommets sous le terrain du jardin, ou à moins de 40 cm de son bord, sont descendus 15 cm sous lui
    const float SinkDepth = 0.15f;
    static void SinkUnder(Transform t, Vector3[] v, Transform above)
    {
        var col = above.GetComponent<MeshCollider>();
        bool temp = col == null;
        if (temp) { col = above.gameObject.AddComponent<MeshCollider>(); col.sharedMesh = above.GetComponent<MeshFilter>().sharedMesh; }
        Physics.SyncTransforms();
        var offsets = new[] { Vector3.zero, new Vector3(0.4f, 0, 0), new Vector3(-0.4f, 0, 0), new Vector3(0, 0, 0.4f), new Vector3(0, 0, -0.4f) };
        float topY = col.bounds.max.y + 2f;
        int n = 0;
        for (int i = 0; i < v.Length; i++)
        {
            var w = t.TransformPoint(v[i]);
            float ground = float.NegativeInfinity;
            foreach (var o in offsets)
                if (col.Raycast(new Ray(new Vector3(w.x + o.x, topY, w.z + o.z), Vector3.down), out var h, topY - col.bounds.min.y + 3f))
                    ground = Mathf.Max(ground, h.point.y);
            if (float.IsNegativeInfinity(ground) || w.y <= ground - SinkDepth) continue;
            w.y = ground - SinkDepth;
            v[i] = t.InverseTransformPoint(w);
            n++;
        }
        if (temp) Object.DestroyImmediate(col);
        Debug.Log($"[JardinNature] terrain extérieur : {n} sommets descendus sous le jardin");
    }

    // normales calculées sur les faces puis moyennées entre sommets au même endroit (le maillage a des coutures d'UV : sommets doublés)
    static Vector3[] SmoothNormals(Mesh m)
    {
        var v = m.vertices;
        var acc = new Dictionary<Vector3Int, Vector3>();
        Vector3Int K(Vector3 p) => Vector3Int.RoundToInt(p * 1000f);
        for (int s = 0; s < m.subMeshCount; s++)
        {
            var t = m.GetTriangles(s);
            for (int i = 0; i < t.Length; i += 3)
            {
                var a = v[t[i]]; var b = v[t[i + 1]]; var c = v[t[i + 2]];
                var fn = Vector3.Cross(b - a, c - a);          // longueur = 2 x aire : les grandes faces pèsent plus
                foreach (var k in new[] { K(a), K(b), K(c) })
                    acc[k] = acc.TryGetValue(k, out var o) ? o + fn : fn;
            }
        }
        var res = new Vector3[v.Length];
        var old = m.normals;
        for (int i = 0; i < v.Length; i++)
        {
            var n = acc.TryGetValue(K(v[i]), out var o) ? o : Vector3.up;
            if (old.Length == v.Length && Vector3.Dot(n, old[i]) < 0f) n = -n;    // garde le sens d'origine (faces vers le ciel)
            res[i] = n.sqrMagnitude > 1e-12f ? n.normalized : Vector3.up;
        }
        return res;
    }

    // ---------------------------------------------------------------- arbres
    const string TreeFbx = "Assets/Nature/Arbres/Arbre_Quest.fbx";

    // maillages LOD0 / LOD1 de l'arbre refait (Tools/blender/arbre_quest.py) et matériaux de la carte dans l'ordre de ses sous-maillages
    static bool LoadTree(Transform root, out Mesh lod0, out Mesh lod1, out Material[] mats)
    {
        lod0 = lod1 = null; mats = null;
        var imp = AssetImporter.GetAtPath(TreeFbx) as ModelImporter;
        if (imp == null) { Debug.LogWarning($"[JardinNature] {TreeFbx} absent : lancer Tools/blender/arbre_quest.py"); return false; }
        if (!imp.isReadable || imp.generateSecondaryUV || imp.importCameras || imp.importLights || imp.importAnimation)
        {
            imp.isReadable = true; imp.generateSecondaryUV = false; imp.importCameras = false; imp.importLights = false; imp.importAnimation = false;
            imp.SaveAndReimport();
        }
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(TreeFbx))
            if (o is Mesh m) { if (m.name.StartsWith("LOD0")) lod0 = m; else if (m.name.StartsWith("LOD1")) lod1 = m; }
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreeFbx);
        var pr = prefab ? prefab.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.name.StartsWith("LOD0")) : null;
        if (!lod0 || !lod1 || pr == null) { Debug.LogWarning("[JardinNature] maillages LOD0 / LOD1 introuvables dans " + TreeFbx); return false; }
        // matériaux déjà posés sur un arbre du jardin (island_tree_02*, recréés à chaque import de la carte), dans l'ordre des
        // sous-maillages (voir plus bas). Leurs textures sont remplacées par celles du jacaranda (Retexture).
        // ordre des sous-maillages de l'FBX (Tools/blender/arbre_jacaranda.py) : feuilles, rameaux, tronc. Les matériaux sont pris PAR NOM,
        // sur un arbre du jardin s'il y en a un, sinon dans leurs fichiers.
        var byName = new Dictionary<string, Material>();
        if (root != null)
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.name.StartsWith("Garden_RealTree_") && !r.name.Contains("Gnarled")))
                foreach (var m in r.sharedMaterials) if (m && m.name.StartsWith("island_tree_02")) byName[m.name] = m;
        Material Get(string n) => byName.TryGetValue(n, out var m) ? m : AssetDatabase.LoadAssetAtPath<Material>($"Assets/Room/Materials/{n}.mat");
        mats = new[] { Get("island_tree_02_leaves"), Get("island_tree_02_branches"), Get("island_tree_02") };
        if (mats.Any(m => !m)) { Debug.LogWarning("[JardinNature] matériaux de l'arbre introuvables : arbres non remplacés"); mats = null; return false; }
        Retexture(mats);
        return true;
    }

    // textures du jacaranda (Poly Haven, CC0, 1k) sur les trois matériaux de l'arbre : tronc, feuilles (découpe alpha, double face), rameaux
    const string JacDir = "Assets/Nature/Arbres/Jacaranda";
    static void Retexture(Material[] m)
    {
        var td = Tex($"{JacDir}/jacaranda_tree_trunk_diff_1k.jpg", false); var tn = Tex($"{JacDir}/jacaranda_tree_trunk_nor_gl_1k.jpg", true);
        var bd = Tex($"{JacDir}/jacaranda_tree_branches_diff_1k.jpg", false); var bn = Tex($"{JacDir}/jacaranda_tree_branches_nor_gl_1k.jpg", true);
        var ld = Tex($"{JacDir}/jacaranda_tree_leaves_diff_1k.png", false); var ln = Tex($"{JacDir}/jacaranda_tree_leaves_nor_gl_1k.jpg", true);
        if (!td || !bd || !ld) { Debug.LogWarning($"[JardinNature] textures du jacaranda absentes dans {JacDir}"); return; }
        var li = (TextureImporter)AssetImporter.GetAtPath($"{JacDir}/jacaranda_tree_leaves_diff_1k.png");
        if (!li.alphaIsTransparency || !li.mipMapsPreserveCoverage)
        {
            li.alphaIsTransparency = true; li.mipMapsPreserveCoverage = true; li.alphaTestReferenceValue = 0.5f; li.SaveAndReimport();
        }
        void Set(Material mat, Texture2D d, Texture2D n, float smooth)
        {
            mat.SetTexture("_BaseMap", d); mat.SetTextureScale("_BaseMap", Vector2.one); mat.SetTextureOffset("_BaseMap", Vector2.zero);
            mat.SetColor("_BaseColor", Color.white);
            if (n) { mat.SetTexture("_BumpMap", n); mat.SetTextureScale("_BumpMap", Vector2.one); mat.SetFloat("_BumpScale", 1f); mat.EnableKeyword("_NORMALMAP"); }
            mat.SetFloat("_Smoothness", smooth);
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_SpecularHighlights", 0f); mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            mat.SetFloat("_EnvironmentReflections", 0f); mat.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            EditorUtility.SetDirty(mat);
        }
        Set(m[0], ld, ln, 0.1f); Set(m[1], bd, bn, 0.12f); Set(m[2], td, tn, 0.12f);     // feuilles, rameaux, tronc
        m[0].SetFloat("_AlphaClip", 1f); m[0].SetFloat("_Cutoff", 0.5f); m[0].SetFloat("_Cull", 0f); m[0].EnableKeyword("_ALPHATEST_ON");
        m[0].SetOverrideTag("RenderType", "TransparentCutout"); m[0].renderQueue = (int)RenderQueue.AlphaTest;
        AssetDatabase.SaveAssets();
    }

    /// <summary>Met à jour l'arbre dans la scène ouverte SANS la reconstruire : textures, imposteur et panneaux (le maillage vient du FBX, mis à jour tout seul).</summary>
    [MenuItem("SAE501/Jardin/Mettre à jour l'arbre sans reconstruire")]
    public static void RafraichirArbres()
    {
        var map = GameObject.Find("Carte (Blender)");      // absente si une autre scène est ouverte : les matériaux sont alors pris dans leurs fichiers
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Generated", "Nature");
        if (!LoadTree(map ? map.transform : null, out var lod0, out _, out var mats)) return;
        AssetDatabase.DeleteAsset(ImpostorPath);
        Impostor(lod0, mats);
        AssetDatabase.SaveAssets();
        Debug.Log("[JardinNature] arbre mis à jour (textures, imposteur) sans reconstruire la scène");
    }

    static (Material mat, Mesh cards) Impostor(Mesh lod0, Material[] mats)
    {
        var impTex = AssetDatabase.LoadAssetAtPath<Texture2D>(ImpostorPath);
        if (!impTex) impTex = RenderImpostor(lod0, mats);
        if (!impTex) return (null, null);
        var impMat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Arbre_Imposteur" };
        impMat.SetTexture("_BaseMap", impTex);
        impMat.SetColor("_BaseColor", Color.white);
        impMat.SetFloat("_AlphaClip", 1f);
        impMat.SetFloat("_Cutoff", 0.5f);
        impMat.EnableKeyword("_ALPHATEST_ON");
        impMat.SetFloat("_Cull", 0f);
        impMat.SetFloat("_Smoothness", 0f);
        impMat.SetFloat("_SpecularHighlights", 0f);
        impMat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        impMat.SetFloat("_EnvironmentReflections", 0f);
        impMat.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        impMat.SetOverrideTag("RenderType", "TransparentCutout");
        impMat.renderQueue = (int)RenderQueue.AlphaTest;
        impMat.enableInstancing = true;
        impMat = SaveMat(impMat, $"{Dir}/Arbre_Imposteur.mat");
        var cards = SaveMesh(Cards(FrameSize(lod0.bounds), lod0.bounds.min.y), $"{Dir}/Arbre_Imposteur.asset");
        return (impMat, cards);
    }

    // LODGroup d'un arbre : r0 (déjà sur l'objet ou créé) = LOD0, puis LOD1, puis imposteur, puis rien
    static void TreeLods(GameObject go, MeshRenderer r0, Mesh lod1, Material[] mats, Material impMat, Mesh cards, float lod0Screen, float lod1Screen)
    {
        var lods = new List<LOD> { new LOD(lod0Screen, new Renderer[] { r0 }) };
        var r1 = Part(go.transform, "LOD1", lod1, mats);
        if (cards != null)
        {
            lods.Add(new LOD(lod1Screen, new Renderer[] { r1 }));
            lods.Add(new LOD(CullScreen, new Renderer[] { Part(go.transform, "Imposteur", cards, new[] { impMat }) }));
        }
        else lods.Add(new LOD(CullScreen, new Renderer[] { r1 }));
        var g = go.AddComponent<LODGroup>();
        g.SetLODs(lods.ToArray());
        g.fadeMode = LODFadeMode.None;
        g.RecalculateBounds();
    }

    static void Arbres(Transform root)
    {
        if (!LoadTree(root, out var lod0, out var lod1, out var mats)) return;
        var (impMat, cards) = Impostor(lod0, mats);

        // arbres du jardin (Garden_RealTree_*, sauf le tronc couché) : même place, même taille, nouveau maillage et LOD
        int g = 0;
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true)
                     .Where(r => r.name.StartsWith("Garden_RealTree_") && !r.name.Contains("Gnarled") && !r.name.Contains("_LOD")).ToList())
        {
            r.GetComponent<MeshFilter>().sharedMesh = lod0;
            r.sharedMaterials = mats;
            r.receiveGI = ReceiveGI.LightProbes;           // pas d'UV de lightmap sur le nouvel arbre : lumière des sondes
            r.shadowCastingMode = ShadowCastingMode.Off;
            GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.BatchingStatic);
            TreeLods(r.gameObject, r, lod1, mats, impMat, cards, Lod0Jardin, Lod1Jardin);
            g++;
        }

        // forêt : les cônes du blockout remplacés
        int n = 0;
        var trunks = Find(root, "Exterior_Forest_Trunks");
        var canopies = Find(root, "Exterior_Forest_Canopies");
        var tm = trunks ? trunks.GetComponent<MeshFilter>().sharedMesh : null;
        if (tm && tm.isReadable)
        {
            var trunkParts = Parts(tm, trunks);
            var canopyParts = canopies ? Parts(canopies.GetComponent<MeshFilter>().sharedMesh, canopies) : new List<Bounds>();
            var parent = new GameObject("Foret_Arbres").transform;
            parent.SetParent(trunks.parent, false);
            float meshH = lod0.bounds.max.y;                  // pivot au pied du tronc
            var rnd = new System.Random(501);
            foreach (var tp in trunkParts)
            {
                var foot = new Vector3(tp.center.x, tp.min.y - 0.1f, tp.center.z);
                float top = tp.max.y + 2f;
                foreach (var c in canopyParts)
                    if (new Vector2(c.center.x - foot.x, c.center.z - foot.z).magnitude < 1.6f) top = Mathf.Max(top, c.max.y);
                float s = Mathf.Clamp((top - foot.y) / meshH, 1.1f, 2.4f) * (0.9f + 0.2f * (float)rnd.NextDouble());

                var go = new GameObject($"Foret_Arbre_{n:00}");
                go.transform.SetParent(parent, false);
                go.transform.SetPositionAndRotation(foot, Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f));
                go.transform.localScale = Vector3.one * s;
                TreeLods(go, Part(go.transform, "LOD0", lod0, mats), lod1, mats, impMat, cards, Lod0Screen, Lod1Screen);

                // seul le tronc bloque : capsule de 30 cm de rayon (le joueur et les zombies passent entre les arbres)
                var cap = go.AddComponent<CapsuleCollider>();
                cap.direction = 1;
                cap.radius = 0.3f / s; cap.height = 3f / s;
                cap.center = new Vector3(0f, cap.height * 0.5f, 0f);
                n++;
            }
            Object.DestroyImmediate(trunks.gameObject);
            if (canopies) Object.DestroyImmediate(canopies.gameObject);
        }
        Debug.Log($"[JardinNature] arbres : {g} au jardin, {n} en forêt (LOD0 {Tris(lod0)} / LOD1 {Tris(lod1)} / imposteur {(cards ? Tris(cards) : 0)} triangles)");
    }

    static MeshRenderer Part(Transform parent, string name, Mesh mesh, Material[] mats)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterials = mats.Length >= mesh.subMeshCount ? mats.Take(mesh.subMeshCount).ToArray() : Enumerable.Repeat(mats[0], mesh.subMeshCount).ToArray();
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveGI = ReceiveGI.LightProbes;
        r.lightProbeUsage = LightProbeUsage.BlendProbes;
        r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
        return r;
    }

    // morceaux connexes d'un maillage (sommets soudés par position), boîtes en coordonnées monde
    static List<Bounds> Parts(Mesh m, Transform t)
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
            par[F(id[tri[i + 2]])] = a;
        }
        var res = new Dictionary<int, Bounds>();
        for (int i = 0; i < v.Length; i++)
        {
            int r = F(id[i]);
            var w = t.TransformPoint(v[i]);
            if (res.TryGetValue(r, out var b)) { b.Encapsulate(w); res[r] = b; }
            else res[r] = new Bounds(w, Vector3.zero);
        }
        return res.Values.ToList();
    }

    // trois panneaux croisés à 60°, de la taille de l'arbre ; normales tirées vers le haut (lumière douce des deux côtés)
    // demi-côté du cadre carré de l'imposteur (l'image et les panneaux ont exactement ce cadre)
    static float FrameSize(Bounds b) => Mathf.Max((b.max.y - b.min.y) * 0.5f, Mathf.Max(b.extents.x, b.extents.z) * 1.05f);

    static Mesh Cards(float half, float y0)
    {
        float y1 = y0 + 2f * half;
        var vs = new List<Vector3>(); var ns = new List<Vector3>(); var uvs = new List<Vector2>(); var ts = new List<int>();
        for (int k = 0; k < 3; k++)
        {
            var q = Quaternion.Euler(0f, k * 60f, 0f);
            var right = q * Vector3.right; var fwd = q * Vector3.forward;
            var n = (Vector3.up * 0.8f + fwd * 0.2f).normalized;
            int o = vs.Count;
            vs.Add(-right * half + Vector3.up * y0); vs.Add(right * half + Vector3.up * y0);
            vs.Add(right * half + Vector3.up * y1); vs.Add(-right * half + Vector3.up * y1);
            for (int i = 0; i < 4; i++) ns.Add(n);
            uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0)); uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));
            ts.AddRange(new[] { o, o + 2, o + 1, o, o + 3, o + 2 });
        }
        var m = new Mesh { name = "Arbre_Imposteur" };
        m.SetVertices(vs); m.SetNormals(ns); m.SetUVs(0, uvs); m.SetTriangles(ts, 0);
        m.RecalculateTangents();
        m.RecalculateBounds();
        return m;
    }

    // ---------------------------------------------------------------- imposteur
    [MenuItem("SAE501/Jardin/Refaire l'imposteur des arbres")]
    static void RebuildImpostor()
    {
        var map = GameObject.Find("Carte (Blender)");
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Generated", "Nature");
        if (LoadTree(map ? map.transform : null, out var lod0, out _, out var mats)) RenderImpostor(lod0, mats);
    }

    // l'arbre vu de côté, en orthographique, avec des matériaux sans éclairage (couleur seule) ; le fond magenta devient transparent
    static Texture2D RenderImpostor(Mesh mesh, Material[] mats)
    {
        const int W = 512, H = 512;
        var b = mesh.bounds;
        var unlit = mats.Select(src =>
        {
            var u = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (src && src.HasProperty("_BaseMap")) u.SetTexture("_BaseMap", src.GetTexture("_BaseMap"));
            u.SetColor("_BaseColor", Color.white);
            bool clip = src && src.IsKeywordEnabled("_ALPHATEST_ON");
            if (clip) { u.SetFloat("_AlphaClip", 1f); u.SetFloat("_Cutoff", 0.5f); u.EnableKeyword("_ALPHATEST_ON"); }
            u.SetFloat("_Cull", 0f);
            return u;
        }).ToArray();

        var far = new Vector3(0f, -2000f, 0f);
        var obj = new GameObject("TMP_Imposteur") { hideFlags = HideFlags.HideAndDontSave };
        obj.transform.position = far;
        obj.AddComponent<MeshFilter>().sharedMesh = mesh;
        obj.AddComponent<MeshRenderer>().sharedMaterials = unlit;
        var camGo = new GameObject("TMP_ImposteurCam") { hideFlags = HideFlags.HideAndDontSave };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = FrameSize(b);
        cam.aspect = (float)W / H;
        // cadrage : bas de l'image = pied de l'arbre (b.min.y), comme les panneaux
        float midY = b.min.y + cam.orthographicSize;
        camGo.transform.position = far + new Vector3(0f, midY, -50f);
        camGo.transform.rotation = Quaternion.identity;
        cam.nearClipPlane = 1f; cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(1f, 0f, 1f, 1f);
        cam.allowHDR = false; cam.allowMSAA = false;
        var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
        cam.targetTexture = rt;

        bool fog = RenderSettings.fog;
        RenderSettings.fog = false;
        Texture2D tex;
        try
        {
            cam.Render();
            RenderTexture.active = rt;
            tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
        }
        finally
        {
            RenderSettings.fog = fog;
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(camGo);
            Object.DestroyImmediate(obj);
            foreach (var u in unlit) Object.DestroyImmediate(u);
        }

        // fond -> transparent ; couleur des pixels transparents = couleur moyenne de l'arbre (pas de liseré sombre dans les mipmaps)
        var px = tex.GetPixels();
        var avg = Color.black; int cnt = 0;
        for (int i = 0; i < px.Length; i++)
        {
            var c = px[i];
            bool bg = c.r > 0.25f && c.b > 0.25f && c.g < 0.6f * Mathf.Min(c.r, c.b);   // magenta et ses mélanges avec le vert des feuilles (bords lissés)
            if (bg) px[i] = Color.clear;
            else { px[i] = new Color(c.r, c.g, c.b, 1f); avg += c; cnt++; }
        }
        if (cnt > 0) avg /= cnt;
        for (int i = 0; i < px.Length; i++) if (px[i].a == 0f) px[i] = new Color(avg.r, avg.g, avg.b, 0f);
        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(ImpostorPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(ImpostorPath);
        var imp = (TextureImporter)AssetImporter.GetAtPath(ImpostorPath);
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = true;
        imp.mipMapsPreserveCoverage = true;           // les feuilles ne disparaissent pas au loin (petites mipmaps)
        imp.alphaTestReferenceValue = 0.5f;
        imp.wrapMode = TextureWrapMode.Clamp;
        imp.maxTextureSize = 512;
        var android = imp.GetPlatformTextureSettings("Android");
        android.overridden = true; android.maxTextureSize = 512; android.format = TextureImporterFormat.ASTC_6x6;
        imp.SetPlatformTextureSettings(android);
        imp.SaveAndReimport();
        Debug.Log($"[JardinNature] imposteur des arbres rendu ({cnt} pixels d'arbre) : {ImpostorPath}");
        return AssetDatabase.LoadAssetAtPath<Texture2D>(ImpostorPath);
    }

    // ---------------------------------------------------------------- outils
    static Texture2D Tex(string path, bool normal)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return null;
        bool change = false;
        var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        if (imp.textureType != type) { imp.textureType = type; change = true; }
        if (imp.maxTextureSize != 1024) { imp.maxTextureSize = 1024; change = true; }
        if (!imp.mipmapEnabled) { imp.mipmapEnabled = true; change = true; }
        if (imp.wrapMode != TextureWrapMode.Repeat) { imp.wrapMode = TextureWrapMode.Repeat; change = true; }
        if (imp.filterMode != FilterMode.Trilinear) { imp.filterMode = FilterMode.Trilinear; change = true; }
        if (imp.anisoLevel != 4) { imp.anisoLevel = 4; change = true; }   // sol vu en biais : net sans scintiller
        var android = imp.GetPlatformTextureSettings("Android");
        if (!android.overridden || android.format != TextureImporterFormat.ASTC_6x6 || android.maxTextureSize != 1024)
        {
            android.overridden = true; android.maxTextureSize = 1024; android.format = TextureImporterFormat.ASTC_6x6;
            imp.SetPlatformTextureSettings(android);
            change = true;
        }
        if (change) imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // écrit dans le fichier existant s'il y en a un (même GUID : pas de changement dans git si rien n'a changé)
    static Mesh SaveMesh(Mesh m, string path)
    {
        var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (old == null) { AssetDatabase.CreateAsset(m, path); return m; }
        EditorUtility.CopySerialized(m, old);
        old.name = Path.GetFileNameWithoutExtension(path);
        EditorUtility.SetDirty(old);
        Object.DestroyImmediate(m);
        return old;
    }

    static Material SaveMat(Material m, string path)
    {
        var old = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (old == null) { AssetDatabase.CreateAsset(m, path); return m; }
        old.shader = m.shader;
        old.CopyPropertiesFromMaterial(m);
        old.shaderKeywords = m.shaderKeywords;
        old.renderQueue = m.renderQueue;
        old.SetOverrideTag("RenderType", m.GetTag("RenderType", false));
        old.enableInstancing = m.enableInstancing;
        EditorUtility.SetDirty(old);
        Object.DestroyImmediate(m);
        return old;
    }

    static int Tris(Mesh m) { int t = 0; for (int s = 0; s < m.subMeshCount; s++) t += (int)m.GetIndexCount(s) / 3; return t; }

    static Transform Find(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }
}
