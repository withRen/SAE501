using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// [SAE501] Briques communes des arènes 1v1 construites par code (ArenaMapsBuilder : Quai 9 ; écran de sas d'Alvéole 17).
//   - matériaux URP Lit réutilisant les textures du projet (Poly Haven 1k déjà importées), teinte pour l'ambiance de nuit ;
//   - boîtes et cylindres à UV en MÈTRES (une texture n'est jamais étirée, quelle que soit la taille de la boîte), collider exact
//     (BoxCollider, ou MeshCollider convexe pour un cylindre : tout est statique) ;
//   - fusion des maillages statiques par matériau (un appel de dessin par matériau sur Quest), les colliders restent séparés ;
//   - scène : ambiance, apparitions, pistolets du duel, XR Rig et systèmes du joueur repris d'Arena1v1Builder.
public static class ArenaKit
{
    public const string Root = "Assets/Arenas";
    public const float UvMeters = 2f;               // une répétition de texture tous les 2 m (même règle qu'Arena1v1UV)

    // ------------------------------------------------------------------ matériaux

    /// <summary>Matériau URP Lit (créé ou mis à jour). tex : chemin sans suffixe ("Assets/Room/Textures/grey_plaster") ; tile : taille d'une répétition en m.</summary>
    public static Material Mat(string name, string albedo, string normal, Color tint, float smooth, float metal, float tile = 2f)
    {
        Folder(Root + "/Materials");
        string path = $"{Root}/Materials/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name }; AssetDatabase.CreateAsset(m, path); }
        if (albedo != null) Import(albedo, false);
        if (normal != null) Import(normal, true);
        var a = albedo != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(albedo) : null;
        var n = normal != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(normal) : null;
        if (albedo != null && !a) Debug.LogWarning("[ArenaKit] texture introuvable : " + albedo);
        m.SetTexture("_BaseMap", a);
        m.SetColor("_BaseColor", tint);
        m.SetTextureScale("_BaseMap", Vector2.one * (UvMeters / tile));
        if (n)
        {
            m.SetTexture("_BumpMap", n);
            m.SetFloat("_BumpScale", 0.8f);
            m.EnableKeyword("_NORMALMAP");
        }
        else { m.SetTexture("_BumpMap", null); m.DisableKeyword("_NORMALMAP"); }
        m.SetFloat("_Smoothness", smooth);
        m.SetFloat("_Metallic", metal);
        // pas de reflet du ciel par défaut (aplats clairs la nuit), comme les vitres et flaques de MainScene
        m.SetFloat("_EnvironmentReflections", 0f);
        m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        m.SetColor("_EmissionColor", Color.black);
        m.DisableKeyword("_EMISSION");
        m.enableInstancing = true;
        EditorUtility.SetDirty(m);
        return m;
    }

    public const string TexDir = Root + "/Textures";      // textures générées (Tools/textures/generer_arenes.py)

    /// <summary>Réglages d'import Quest des textures des arènes : 1024 px, mipmaps, répétition, ASTC 6x6 sur Android ; normal map en type NormalMap.
    /// Ne touche qu'aux textures de Assets/Arenas (les textures partagées du projet gardent leurs réglages).</summary>
    static void Import(string path, bool normal)
    {
        if (!path.StartsWith(Root)) return;
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (!imp) return;
        var android = imp.GetPlatformTextureSettings("Android");
        var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        if (imp.textureType == type && imp.maxTextureSize == 1024 && android.overridden && android.format == TextureImporterFormat.ASTC_6x6) return;
        imp.textureType = type;
        imp.sRGBTexture = !normal;
        imp.maxTextureSize = 1024;
        imp.mipmapEnabled = true;
        imp.wrapMode = TextureWrapMode.Repeat;
        imp.anisoLevel = 2;
        android.overridden = true; android.maxTextureSize = 1024; android.format = TextureImporterFormat.ASTC_6x6;
        imp.SetPlatformTextureSettings(android);
        imp.SaveAndReimport();
    }

    /// <summary>Matériau d'une texture générée (Assets/Arenas/Textures/&lt;nom&gt;_Albedo/_Normal.png).</summary>
    public static Material Gen(string name, string tex, Color tint, float smooth, float metal, float tile)
        => Mat(name, $"{TexDir}/{tex}_Albedo.png", $"{TexDir}/{tex}_Normal.png", tint, smooth, metal, tile);

    public static Material Emissive(string name, Color baseColor, Color emission)
    {
        var m = Mat(name, null, null, baseColor, 0.4f, 0f);
        m.SetColor("_EmissionColor", emission);
        m.EnableKeyword("_EMISSION");
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        EditorUtility.SetDirty(m);
        return m;
    }

    /// <summary>Tissu opaque, double face (rideaux) : cache la vue, ne protège pas (aucun collider).</summary>
    public static Material Cloth(string name, string albedo, Color tint)
    {
        var m = Mat(name, albedo, null, tint, 0.1f, 0f, 1.5f);
        m.SetFloat("_Cull", (float)CullMode.Off);
        m.doubleSidedGI = true;
        EditorUtility.SetDirty(m);
        return m;
    }

    // ------------------------------------------------------------------ géométrie

    /// <summary>Boîte de taille size (m), centrée en center, tournée de yaw degrés autour de la verticale ; UV en mètres. collider : BoxCollider exact.</summary>
    public static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material mat, float yaw = 0f, bool collider = true)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(center, Quaternion.Euler(0f, yaw, 0f));
        go.AddComponent<MeshFilter>().sharedMesh = BoxMesh(size);
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        if (collider) go.AddComponent<BoxCollider>().size = size;
        go.isStatic = true;
        return go;
    }

    /// <summary>Boîte posée au sol : base à y = baseY, coins (x0,z0)-(x1,z1).</summary>
    public static GameObject Slab(Transform parent, string name, float x0, float z0, float x1, float z1, float baseY, float height, Material mat, bool collider = true)
    {
        float ax = Mathf.Min(x0, x1), bx = Mathf.Max(x0, x1), az = Mathf.Min(z0, z1), bz = Mathf.Max(z0, z1);
        return Box(parent, name, new Vector3((ax + bx) / 2f, baseY + height / 2f, (az + bz) / 2f), new Vector3(bx - ax, height, bz - az), mat, 0f, collider);
    }

    /// <summary>Cylindre vertical low-poly (12 faces = 48 triangles, un seul anneau, couvercles en éventail depuis le centre ; cerclages et détails
    /// en normal map) posé au sol. Maillage partagé entre tous les cylindres de mêmes dimensions. Collider simple : boîte (barils, rayon ≥ 0,2 m)
    /// ou capsule (poteaux, mâts), jamais de MeshCollider.</summary>
    public static GameObject Cylinder(Transform parent, string name, Vector3 basePos, float radius, float height, Material mat, bool collider = true, int seg = 12)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = basePos;
        var mesh = SharedCylinder(radius, height, seg);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        if (collider) SimpleCylinderCollider(go, radius, height, Vector3.zero, 1);
        go.isStatic = true;
        return go;
    }

    /// <summary>Cylindre couché (axe horizontal, tourné de yaw) : barils renversés, tuyaux.</summary>
    public static GameObject LyingCylinder(Transform parent, string name, Vector3 center, float radius, float length, float yaw, Material mat, bool collider = true)
    {
        var go = Cylinder(parent, name, Vector3.zero, radius, length, mat, collider);
        // le maillage est centré sur sa base : on le recentre puis on le couche
        go.transform.SetPositionAndRotation(center, Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(0f, 0f, 90f) * Quaternion.identity);
        go.transform.position = center + go.transform.rotation * new Vector3(0f, -length / 2f, 0f);
        return go;
    }

    static Mesh BoxMesh(Vector3 s)
    {
        var h = s / 2f;
        var v = new List<Vector3>(); var n = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
        void Face(Vector3 normal, Vector3 u, Vector3 w, float su, float sw)
        {
            // u, w : demi-axes de la face ; su, sw : longueurs en mètres
            var c = Vector3.Scale(normal, h);
            int i = v.Count;
            v.Add(c - u - w); v.Add(c + u - w); v.Add(c + u + w); v.Add(c - u + w);
            for (int k = 0; k < 4; k++) n.Add(normal);
            uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(su / UvMeters, 0)); uv.Add(new Vector2(su / UvMeters, sw / UvMeters)); uv.Add(new Vector2(0, sw / UvMeters));
            // sens des triangles : face visible du côté de la normale
            // (i, i+1, i+2) a pour normale géométrique u × w (règle de Unity : produit vectoriel des côtés = face avant)
            if (Vector3.Dot(Vector3.Cross(u, w), normal) > 0) { t.Add(i); t.Add(i + 1); t.Add(i + 2); t.Add(i); t.Add(i + 2); t.Add(i + 3); }
            else { t.Add(i); t.Add(i + 2); t.Add(i + 1); t.Add(i); t.Add(i + 3); t.Add(i + 2); }
        }
        Face(Vector3.up, new Vector3(h.x, 0, 0), new Vector3(0, 0, h.z), s.x, s.z);
        Face(Vector3.down, new Vector3(h.x, 0, 0), new Vector3(0, 0, h.z), s.x, s.z);
        Face(Vector3.right, new Vector3(0, 0, h.z), new Vector3(0, h.y, 0), s.z, s.y);
        Face(Vector3.left, new Vector3(0, 0, h.z), new Vector3(0, h.y, 0), s.z, s.y);
        Face(Vector3.forward, new Vector3(h.x, 0, 0), new Vector3(0, h.y, 0), s.x, s.y);
        Face(Vector3.back, new Vector3(h.x, 0, 0), new Vector3(0, h.y, 0), s.x, s.y);
        var m = new Mesh { name = "Boite" };
        m.SetVertices(v); m.SetNormals(n); m.SetUVs(0, uv); m.SetTriangles(t, 0);
        m.RecalculateTangents(); m.RecalculateBounds();
        return m;
    }

    static readonly Dictionary<string, Mesh> cylinders = new Dictionary<string, Mesh>();

    /// <summary>Maillage de cylindre partagé (un seul par rayon / hauteur / nombre de faces).</summary>
    public static Mesh SharedCylinder(float r, float hgt, int seg = 12)
    {
        string k = $"{r:F3}_{hgt:F3}_{seg}";
        if (!cylinders.TryGetValue(k, out var m) || !m) cylinders[k] = m = CylinderMesh(r, hgt, seg);
        return m;
    }

    /// <summary>Collider d'un cylindre d'axe local <paramref name="axis"/> (0 x, 1 y, 2 z) dont la base est en <paramref name="baseLocal"/> :
    /// boîte si le rayon ≥ 0,2 m (on tient debout dessus, pas de roulis), sinon capsule.</summary>
    public static Collider SimpleCylinderCollider(GameObject go, float r, float h, Vector3 baseLocal, int axis)
    {
        var dir = Vector3.zero; dir[axis] = 1f;
        var center = baseLocal + dir * h / 2f;
        if (r >= 0.2f)
        {
            var b = go.AddComponent<BoxCollider>();
            var size = Vector3.one * 2f * r; size[axis] = h;
            b.center = center; b.size = size;
            return b;
        }
        var c = go.AddComponent<CapsuleCollider>();
        c.direction = axis; c.radius = r; c.height = h; c.center = center;
        return c;
    }

    static Mesh CylinderMesh(float r, float hgt, int seg)
    {
        var v = new List<Vector3>(); var n = new List<Vector3>(); var uv = new List<Vector2>(); var t = new List<int>();
        float perim = 2f * Mathf.PI * r;
        for (int i = 0; i <= seg; i++)
        {
            float a = i * Mathf.PI * 2f / seg;
            var d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
            v.Add(d * r); n.Add(d); uv.Add(new Vector2(perim * i / seg / UvMeters, 0));
            v.Add(d * r + Vector3.up * hgt); n.Add(d); uv.Add(new Vector2(perim * i / seg / UvMeters, hgt / UvMeters));
        }
        for (int i = 0; i < seg; i++)
        {
            int a = i * 2;
            t.Add(a); t.Add(a + 1); t.Add(a + 2); t.Add(a + 2); t.Add(a + 1); t.Add(a + 3);
        }
        foreach (var top in new[] { true, false })
        {
            int c = v.Count;
            float y = top ? hgt : 0f;
            v.Add(new Vector3(0, y, 0)); n.Add(top ? Vector3.up : Vector3.down); uv.Add(Vector2.zero);
            for (int i = 0; i <= seg; i++)
            {
                float a = i * Mathf.PI * 2f / seg;
                var p = new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r);
                v.Add(p); n.Add(top ? Vector3.up : Vector3.down); uv.Add(new Vector2(p.x, p.z) / UvMeters);
            }
            for (int i = 0; i < seg; i++)
            {
                if (top) { t.Add(c); t.Add(c + i + 2); t.Add(c + i + 1); }
                else { t.Add(c); t.Add(c + i + 1); t.Add(c + i + 2); }
            }
        }
        var m = new Mesh { name = "Cylindre" };
        m.SetVertices(v); m.SetNormals(n); m.SetUVs(0, uv); m.SetTriangles(t, 0);
        m.RecalculateTangents(); m.RecalculateBounds();
        return m;
    }

    // ------------------------------------------------------------------ fusion par matériau

    /// <summary>Fusionne les maillages statiques sous root par matériau (un objet « Rendu_&lt;mat&gt; » chacun, maillage enregistré dans
    /// Assets/Arenas/&lt;map&gt;/Meshes). Les objets d'origine gardent leurs colliders, sans rendu.</summary>
    public static int MergeByMaterial(Transform root, string mapFolder, System.Func<MeshRenderer, bool> include = null)
    {
        string dir = $"{Root}/{mapFolder}/Meshes";
        Folder(dir);
        var groups = new Dictionary<Material, List<CombineInstance>>();
        var done = new List<MeshRenderer>();
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>(false))
        {
            var mf = r.GetComponent<MeshFilter>();
            if (!mf || !mf.sharedMesh || !r.sharedMaterial) continue;
            if (include != null ? !include(r) : !r.gameObject.isStatic) continue;
            // un objet à plusieurs matériaux (néon : boîtier + tube) : chaque sous-maillage va dans le groupe de son matériau
            var mats = r.sharedMaterials;
            for (int i = 0; i < Mathf.Min(mats.Length, mf.sharedMesh.subMeshCount); i++)
            {
                if (!mats[i]) continue;
                if (!groups.TryGetValue(mats[i], out var list)) groups[mats[i]] = list = new List<CombineInstance>();
                list.Add(new CombineInstance { mesh = mf.sharedMesh, subMeshIndex = i, transform = r.transform.localToWorldMatrix });
            }
            done.Add(r);
        }
        var merged = new GameObject("Rendu_Fusionne").transform;
        merged.SetParent(root, false);
        foreach (var kv in groups)
        {
            // format d'index choisi AVANT la fusion (le changer après vide les triangles)
            int verts = kv.Value.Sum(c => c.mesh.vertexCount);
            var mesh = new Mesh { name = mapFolder + "_" + kv.Key.name, indexFormat = verts < 65000 ? IndexFormat.UInt16 : IndexFormat.UInt32 };
            mesh.CombineMeshes(kv.Value.ToArray(), true, true);
            mesh.RecalculateBounds();
            string path = $"{dir}/{mesh.name}.asset";
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (old) { old.Clear(); EditorUtility.CopySerialized(mesh, old); mesh = old; }
            else AssetDatabase.CreateAsset(mesh, path);
            var go = new GameObject("Rendu_" + kv.Key.name);
            go.transform.SetParent(merged, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = kv.Key;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            go.isStatic = true;
        }
        foreach (var r in done)
        {
            var mf = r.GetComponent<MeshFilter>();
            Object.DestroyImmediate(r);
            if (mf) Object.DestroyImmediate(mf);
        }
        AssetDatabase.SaveAssets();
        return groups.Count;
    }

    // ------------------------------------------------------------------ éclairage

    public static Light Point(Transform parent, string name, Vector3 pos, Color color, float intensity, float range)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color; l.intensity = intensity; l.range = range;
        l.shadows = LightShadows.None;
        return l;
    }

    public static Light Spot(Transform parent, string name, Vector3 pos, Vector3 target, Color color, float intensity, float angle, float range)
    {
        Arena1v1Builder.CreateSpotLight(parent, name, pos, (target - pos).normalized, color, intensity, angle, range);
        return parent.Find(name).GetComponent<Light>();
    }

    // ------------------------------------------------------------------ scène

    /// <summary>Nouvelle scène vide avec l'ambiance de nuit (lumière ambiante plate, brouillard).</summary>
    public static UnityEngine.SceneManagement.Scene NewScene(Color ambient, Color fog, float fogDensity)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambient;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fog;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.reflectionIntensity = 0f;
        return scene;
    }

    /// <summary>Apparitions, pistolets du duel (Pistol_West pour J1, Pistol_East pour J2 : DuelLoadout), XR Rig et systèmes du joueur ; sauvegarde
    /// et ajout aux scènes du build.</summary>
    public static void FinishScene(UnityEngine.SceneManagement.Scene scene, Transform root, ArenaMaps.Map map)
    {
        var spawns = new GameObject("Spawns").transform;
        spawns.SetParent(root, false);
        Arena1v1Builder.CreateSpawnMarker(spawns, "Spawn_Player_1", map.spawn1, Quaternion.Euler(0f, map.yaw1, 0f), new Color(1f, 0.75f, 0.25f));
        Arena1v1Builder.CreateSpawnMarker(spawns, "Spawn_Player_2", map.spawn2, Quaternion.Euler(0f, map.yaw2, 0f), new Color(0.3f, 0.75f, 1f));

        var items = new GameObject("Weapons_Pickups").transform;
        items.SetParent(root, false);
        Arena1v1Builder.SpawnWeapon(items, map.spawn1 + Quaternion.Euler(0f, map.yaw1, 0f) * new Vector3(0f, 0.8f, 1f), "Pistol_West");
        Arena1v1Builder.SpawnWeapon(items, map.spawn2 + Quaternion.Euler(0f, map.yaw2, 0f) * new Vector3(0f, 0.8f, 1f), "Pistol_East");

        var rig = Arena1v1Builder.BuildXRRig(map.spawn1, Quaternion.Euler(0f, map.yaw1, 0f));
        Arena1v1Builder.AddMainSceneSystems(rig);

        ArenaNavMesh.Bake(scene);                                   // [hotfix #155] NavMesh pour la téléportation
        string path = "Assets/Scenes/" + map.scene + ".unity";
        EditorSceneManager.SaveScene(scene, path);
        Arena1v1Builder.UpdateBuildSettings(path);
        Debug.Log("[ArenaKit] scène construite : " + path);
    }

    public static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        Folder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    /// <summary>Miroir par rotation de 180° autour du centre (0,0) : la règle d'équité des cartes (chaque joueur a la moitié de l'autre).</summary>
    public static Vector3 Mirror(Vector3 p) => new Vector3(-p.x, p.y, -p.z);
}
