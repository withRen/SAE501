using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

// [SAE501] Constructeur de la scène multijoueur 1v1 : "Alvéole 17" (Assets/Scenes/Arena1v1_Bunker.unity).
// Construit la scène par script à partir du modèle exporté de Blender (Assets/Arena1v1/arena_1v1_bunker.fbx).
// Crée les matériaux URP, assigne les colliders, l'éclairage, les spawns 1v1, les armes et le XR Rig.
public static class Arena1v1Builder
{
    public const string FbxPath = "Assets/Arena1v1/arena_1v1_bunker.fbx";
    public const string MatDir = "Assets/Arena1v1/Materials";
    public const string ScenePath = "Assets/Scenes/Arena1v1_Bunker.unity";
    const string XriDir = "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/";

    public static readonly Vector3 SpawnP1 = new Vector3(-13.0f, 0.1f, 0.0f);
    public static readonly Quaternion SpawnRotP1 = Quaternion.Euler(0f, 90f, 0f);

    public static readonly Vector3 SpawnP2 = new Vector3(13.0f, 0.1f, 0.0f);
    public static readonly Quaternion SpawnRotP2 = Quaternion.Euler(0f, -90f, 0f);

    [MenuItem("SAE501/Build Arena 1v1 Scene")]
    public static void Build()
    {
        EnsureMaterials();
        ConfigureFbxImporter();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Configuration de l'ambiance et du rendu URP (bunker souterrain hermétique)
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.09f, 0.12f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.06f, 0.07f, 0.09f);
        RenderSettings.fogDensity = 0.012f;

        // Racine de la carte
        var rootMap = new GameObject("Arena1v1_Map");

        var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (!fbxAsset)
        {
            Debug.LogError($"[Arena1v1Builder] Modèle introuvable à {FbxPath}");
            return;
        }

        var mapInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, rootMap.transform);
        mapInstance.name = "Bunker_Geometry";
        mapInstance.transform.localPosition = Vector3.zero;
        mapInstance.transform.localRotation = Quaternion.identity;

        // Ajout des colliders sur toute la géométrie
        AddColliders(mapInstance);

        // Éclairage URP temps réel
        var rootLights = new GameObject("Arena_Lighting");
        rootLights.transform.SetParent(rootMap.transform, false);
        BuildLights(rootLights.transform);

        // Points de Spawn repères
        var spawnsRoot = new GameObject("Spawns");
        spawnsRoot.transform.SetParent(rootMap.transform, false);
        CreateSpawnMarker(spawnsRoot.transform, "Spawn_Player_1", SpawnP1, SpawnRotP1, new Color(1f, 0.75f, 0.25f));
        CreateSpawnMarker(spawnsRoot.transform, "Spawn_Player_2", SpawnP2, SpawnRotP2, new Color(0.3f, 0.75f, 1f));

        // Armes & munitions au sol
        var itemsRoot = new GameObject("Weapons_Pickups");
        itemsRoot.transform.SetParent(rootMap.transform, false);
        SpawnWeapon(itemsRoot.transform, new Vector3(-12.0f, 0.8f, 0.0f), "Pistol_West");
        SpawnWeapon(itemsRoot.transform, new Vector3(12.0f, 0.8f, 0.0f), "Pistol_East");
        // (les anciens pistolets « Special_Center » et « Sniper_Nord / Sud » sont retirés : le kit d'armes se choisit dans le lobby, voir DuelLoadout)

        // Configuration XR Origin / Joueur VR
        var rig = BuildXRRig(SpawnP1, SpawnRotP1);

        // comme en jeu normal : vignette de dégâts, musique, menu pause, ceinture avec holster du pistolet
        AddMainSceneSystems(rig);
        // (le gestionnaire de duel NetDuel n'est plus dans la scène : c'est un prefab réseau créé par l'hôte, voir NetDuel.EnsureSpawned)

        // corrections de level design (escaliers, caisses, gravats, socles vides, flaques, ombres) : Arena1v1Fixes
        Arena1v1Fixes.Apply();
        ArenaNavMesh.Bake(scene);                                   // [hotfix #155] NavMesh pour la téléportation

        // Sauvegarde de la scène
        EditorSceneManager.SaveScene(scene, ScenePath);

        // Ajout dans les scènes de build Unity
        UpdateBuildSettings(ScenePath);

        Debug.Log($"[Arena1v1Builder] Scène 1v1 construite avec succès : {ScenePath}");
    }

    static void EnsureMaterials()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Arena1v1")) AssetDatabase.CreateFolder("Assets", "Arena1v1");
        if (!AssetDatabase.IsValidFolder(MatDir)) AssetDatabase.CreateFolder("Assets/Arena1v1", "Materials");

        MakeLitMat("Mat_Concrete_Floor", new Color(0.24f, 0.25f, 0.27f), 0.25f, 0.05f);
        MakeLitMat("Mat_Concrete_Wall", new Color(0.32f, 0.33f, 0.35f), 0.20f, 0.05f);
        MakeLitMat("Mat_Steel_Dark", new Color(0.12f, 0.13f, 0.15f), 0.65f, 0.85f);
        MakeLitMat("Mat_Rust_Cover", new Color(0.38f, 0.22f, 0.14f), 0.20f, 0.40f);
        MakeLitMat("Mat_Hazard_Yellow", new Color(0.85f, 0.65f, 0.08f), 0.35f, 0.10f);
        MakeLitMat("Mat_Water_Puddle", new Color(0.08f, 0.09f, 0.11f), 0.95f, 0.10f);

        MakeEmissiveMat("Mat_Lamp_Emissive", new Color(0.85f, 0.92f, 1.0f), new Color(1.5f, 1.8f, 2.2f) * 2.5f);
        MakeEmissiveMat("Mat_Warning_Emissive", new Color(1.0f, 0.45f, 0.05f), new Color(2.2f, 1.0f, 0.1f) * 3.0f);

        AppliquerTextures();
        AssetDatabase.SaveAssets();
    }

    // Textures PBR CC0 de Poly Haven (1k, raccordables) dans Assets/Arena1v1/Textures. Les UV à l'échelle viennent
    // de Arena1v1UV (projection cubique à l'import du FBX). La teinte assombrit la texture pour garder l'ambiance du bunker.
    const string TexDir = "Assets/Arena1v1/Textures";

    [MenuItem("SAE501/Arène 1v1/Appliquer les textures")]
    public static void AppliquerTextures()
    {
        RéglerImportsTextures();
        Texturer("Mat_Concrete_Floor", "dirty_concrete", null, new Color(0.62f, 0.62f, 0.64f), 0.25f, 0.05f);
        Texturer("Mat_Concrete_Wall", "cracked_concrete_wall", null, new Color(0.70f, 0.71f, 0.74f), 0.15f, 0.05f);
        Texturer("Mat_Rust_Cover", "rust_coarse_01", null, new Color(0.85f, 0.85f, 0.85f), 0.20f, 0.40f);
        // acier sombre et peinture jaune : albedo et relief de la salle principale (Tools/textures/generer_usure.py)
        Texturer("Mat_Steel_Dark", null, "AcierSombre|Acier", new Color(0.55f, 0.57f, 0.62f), 0.55f, 0.85f);
        Texturer("Mat_Hazard_Yellow", null, "|Peinture", new Color(0.85f, 0.65f, 0.08f), 0.35f, 0.10f);
        AssetDatabase.SaveAssets();
    }

    // poly : préfixe Poly Haven (_diff_1k / _nor_gl_1k) ; usure : "albedo|famille" dans TexturesUsure.Dir
    static void Texturer(string mat, string poly, string usure, Color teinte, float smoothness, float metallic)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>($"{MatDir}/{mat}.mat");
        if (!m) return;
        Texture2D albedo = null, normal = null;
        if (poly != null)
        {
            albedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexDir}/{poly}_diff_1k.jpg");
            normal = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexDir}/{poly}_nor_gl_1k.jpg");
        }
        else
        {
            var parts = usure.Split('|');
            if (parts[0].Length > 0) albedo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesUsure.Dir}/Usure_{parts[0]}_Albedo.png");
            normal = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesUsure.Dir}/Usure_{parts[1]}_Normal.png");
        }
        if (albedo) m.SetTexture("_BaseMap", albedo);
        m.SetColor("_BaseColor", teinte);
        if (normal)
        {
            m.SetTexture("_BumpMap", normal);
            m.SetFloat("_BumpScale", 1f);
            m.EnableKeyword("_NORMALMAP");
        }
        m.SetFloat("_Smoothness", smoothness);
        m.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(m);
    }

    // Quest 3 : 1024 px, ASTC 6x6 sur Android, mipmaps, répétition
    static void RéglerImportsTextures()
    {
        if (!AssetDatabase.IsValidFolder(TexDir)) return;
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { TexDir }))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            var imp = (TextureImporter)AssetImporter.GetAtPath(p);
            bool normal = p.Contains("_nor_gl");
            var android = imp.GetPlatformTextureSettings("Android");
            if (imp.textureType == (normal ? TextureImporterType.NormalMap : TextureImporterType.Default)
                && imp.maxTextureSize == 1024 && imp.wrapMode == TextureWrapMode.Repeat
                && android.overridden && android.format == TextureImporterFormat.ASTC_6x6) continue;
            imp.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            imp.sRGBTexture = !normal;
            imp.maxTextureSize = 1024;
            imp.mipmapEnabled = true;
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.anisoLevel = 2;
            android.overridden = true;
            android.maxTextureSize = 1024;
            android.format = TextureImporterFormat.ASTC_6x6;
            imp.SetPlatformTextureSettings(android);
            imp.SaveAndReimport();
        }
    }

    static Material MakeLitMat(string name, Color color, float smoothness, float metallic)
    {
        string path = $"{MatDir}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material MakeEmissiveMat(string name, Color baseColor, Color emissionColor)
    {
        string path = $"{MatDir}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", baseColor);
        mat.SetFloat("_Smoothness", 0.5f);
        mat.SetFloat("_Metallic", 0.1f);
        mat.SetColor("_EmissionColor", emissionColor);
        mat.EnableKeyword("_EMISSION");
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void ConfigureFbxImporter()
    {
        var mi = (ModelImporter)AssetImporter.GetAtPath(FbxPath);
        if (!mi) return;

        mi.globalScale = 1.0f;
        mi.useFileScale = true;
        mi.importCameras = false;
        mi.importLights = false;
        mi.isReadable = true;
        mi.generateSecondaryUV = true;

        var matNames = new[]
        {
            "Mat_Concrete_Floor", "Mat_Concrete_Wall", "Mat_Steel_Dark", "Mat_Rust_Cover",
            "Mat_Hazard_Yellow", "Mat_Lamp_Emissive", "Mat_Warning_Emissive", "Mat_Water_Puddle"
        };

        foreach (var mname in matNames)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MatDir}/{mname}.mat");
            if (mat)
                mi.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), mname), mat);
        }

        mi.SaveAndReimport();
    }

    static void AddColliders(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in renderers)
        {
            var mf = mr.GetComponent<MeshFilter>();
            if (!mf || !mf.sharedMesh) continue;

            string n = mr.name.ToLower();
            // Ignorer les tubes de néons et câbles fins pour les collisions physiques
            if (n.Contains("fixture") || n.Contains("tube") || n.Contains("cable") || n.Contains("rebar") || n.Contains("puddle"))
                continue;

            var mc = mr.gameObject.GetComponent<MeshCollider>();
            if (!mc) mc = mr.gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;

            // Débris, caisses et barils : convex = true pour une interaction dynamique propre
            if (n.Contains("rubble") || n.Contains("casing") || n.Contains("crate") || n.Contains("barrel") || n.Contains("hedgehog"))
            {
                mc.convex = true;
            }
            else
            {
                mc.convex = false;
            }

            mr.gameObject.isStatic = true;
        }
    }

    static void BuildLights(Transform parent)
    {
        // 1. Lumière Principale (Main Light URP) : Directional Light zénithale douce
        // Ne compte PAS dans la limite des Additional Lights (8 par objet sous URP Forward),
        // évite tout grésillement / flickering ou permutation de lumière d'une frame à l'autre.
        var dirGo = new GameObject("Light_Main_BunkerFill");
        dirGo.transform.SetParent(parent, false);
        dirGo.transform.rotation = Quaternion.Euler(70f, -25f, 0f);
        var dirLight = dirGo.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(0.82f, 0.88f, 1.0f);
        dirLight.intensity = 1.3f;
        dirLight.shadows = LightShadows.Soft;

        // 2. Spots localisés d'ambiance et de gameplay (Total = 5 lumières d'accentuation < 8 max)
        // Aucune ombre sur les Additional Lights (LightShadows.None) pour garantir 72 FPS stables
        // et supprimer tout scintillement ou conflit de shadow maps sur Meta Quest 3.

        // Balise ambrée au sas d'apparition Ouest (J1)
        CreateSpotLight(parent, "Spot_Spawn_West", new Vector3(-13.0f, 4.6f, 0.0f), Vector3.down, new Color(1.0f, 0.55f, 0.15f), 3.5f, 65f, 8.5f);

        // Balise ambrée au sas d'apparition Est (J2)
        CreateSpotLight(parent, "Spot_Spawn_East", new Vector3(13.0f, 4.6f, 0.0f), Vector3.down, new Color(1.0f, 0.55f, 0.15f), 3.5f, 65f, 8.5f);

        // Néon de la passerelle Nord (surplomb et tir)
        CreateSpotLight(parent, "Spot_Catwalk_North", new Vector3(0.0f, 5.2f, 10.5f), Vector3.down, new Color(0.85f, 0.92f, 1.0f), 3.2f, 75f, 9.5f);

        // Néon de la passerelle Sud (surplomb et tir)
        CreateSpotLight(parent, "Spot_Catwalk_South", new Vector3(0.0f, 5.2f, -10.5f), Vector3.down, new Color(0.85f, 0.92f, 1.0f), 3.2f, 75f, 9.5f);

        // Alarme d'ambiance sur la brèche centrale contestée
        CreateSpotLight(parent, "Spot_Center_Contested", new Vector3(0.0f, 4.8f, 0.0f), Vector3.down, new Color(1.0f, 0.45f, 0.15f), 2.8f, 65f, 8.0f);
    }

    internal static void CreateSpotLight(Transform parent, string name, Vector3 pos, Vector3 dir, Color color, float intensity, float angle, float range)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.LookRotation(dir);
        var l = go.AddComponent<Light>();
        l.type = LightType.Spot;
        l.color = color;
        l.intensity = intensity;
        l.spotAngle = angle;
        l.innerSpotAngle = angle * 0.45f;
        l.range = range;
        l.shadows = LightShadows.None; // Garanti sans conflit de shadow maps URP
    }

    internal static void CreateSpawnMarker(Transform parent, string name, Vector3 pos, Quaternion rot, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(pos, rot);

        // Cylindre visuel semi-transparent au sol comme repère dans l'éditeur
        var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "Visual_Marker";
        marker.transform.SetParent(go.transform, false);
        marker.transform.localScale = new Vector3(1.2f, 0.02f, 1.2f);
        Object.DestroyImmediate(marker.GetComponent<Collider>());
        var r = marker.GetComponent<Renderer>();
        var m = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = color };
        r.sharedMaterial = m;
    }

    internal static void SpawnWeapon(Transform parent, Vector3 pos, string name)
    {
        // le même pistolet qu'en jeu normal (animation et son du tir, douille, flash, fumée, compteur de balles) ; secours : version simple ci-dessous
        if (GunBuilder.Available) { RoomBuilder.CreatePistol(parent, pos, name); return; }
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        // Modèle du pistolet M1911 si disponible
        if (GunBuilder.Available)
        {
            var gun = GunBuilder.InstantiateGun(go.transform);
            var bc = go.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, 0.035f, 0.07f);
            bc.size = new Vector3(0.04f, 0.14f, 0.24f);

            var rb = go.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var grab = go.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            var attach = new GameObject("Attach").transform;
            attach.SetParent(go.transform, false);
            attach.localPosition = new Vector3(0f, 0.02f, -0.015f);
            grab.attachTransform = attach;

            AddGrabRules(go);
            var pi = go.AddComponent<Pistol>();
            pi.muzzle = gun.barrel;
            pi.animator = gun.animator;
            pi.slider = gun.slider;
            pi.ejection = gun.ejection;
        }
    }

    // Ce que MainScene donne au joueur et que l'arène n'avait pas : vignette de dégâts (HealthVignette), écran de fin, musique, menu pause
    // (RoomBuilder.AddPlayerSystems) et la ceinture avec un holster pour le pistolet (le lâcher près de la hanche le range, comme en jeu normal).
    // Déplacement, accroupi, saut, prise par clic, tir et mains viennent déjà du même rig et de SetupVRScene.UseDirectInteractor.
    internal static void AddMainSceneSystems(GameObject rig)
    {
        RoomBuilder.AddPlayerSystems(rig);
        if (Object.FindFirstObjectByType<BeltRig>(FindObjectsInactive.Include)) return;
        var head = rig.GetComponentInChildren<Camera>(true);
        var belt = new GameObject("Belt");
        belt.transform.SetPositionAndRotation(rig.transform.position + new Vector3(0f, 0.95f, 0f), Quaternion.identity);
        var br = belt.AddComponent<BeltRig>();
        br.head = head.transform;
        br.rigRoot = rig.transform;
        var holster = new GameObject("Holster_Pistol");
        holster.transform.SetParent(belt.transform, false);
        holster.transform.localPosition = new Vector3(0.22f, 0f, 0.02f);
        var sc = holster.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.16f;
        var socket = holster.AddComponent<BeltSocket>();
        socket.accepts = BeltItemType.Pistol;
        socket.showInteractableHoverMeshes = false;
    }

    // mêmes règles de prise que les objets de MainScene (RoomBuilder.MakeGrab) : portée limitée, pas de vol, jamais sous le sol
    static void AddGrabRules(GameObject go)
    {
        var grab = go.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab) grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;
        if (!go.GetComponent<IgnorePlayer>()) go.AddComponent<IgnorePlayer>();
        if (!go.GetComponent<GrabGuard>()) go.AddComponent<GrabGuard>();
        if (!go.GetComponent<ItemSafety>()) go.AddComponent<ItemSafety>();
        if (!go.GetComponent<BeltItem>()) go.AddComponent<BeltItem>().type = BeltItemType.Pistol;      // le holster de la ceinture l'accepte
    }

    /// <summary>
    /// Répare la scène DÉJÀ construite sans la reconstruire (éclairage et matériaux intacts) : mains avec interacteur direct
    /// (prise et tir en version PC et au casque) et règles de prise sur les pistolets. Menu SAE501 → Arène 1v1 → Réparer mains et armes.
    /// </summary>
    [MenuItem("SAE501/Arène 1v1/Réparer mains et armes")]
    public static void RepairHandsAndWeapons()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath);
        int hands = 0, guns = 0;
        foreach (var name in new[] { "Left Controller", "Right Controller" })
        {
            var controller = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name == name);
            if (!controller) continue;
            if (controller.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>(true)) continue;
            SetupVRScene.UseDirectInteractor(controller, name.StartsWith("Left") ? -1f : 1f);
            hands++;
        }
        // les pistolets déjà posés sont refaits avec le pistolet complet de MainScene (même nom, même place)
        if (GunBuilder.Available)
            foreach (var old in Object.FindObjectsByType<Pistol>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (old.GetComponent<InspectHud>()) continue;           // déjà le pistolet complet
                var parent = old.transform.parent; var pos = old.transform.position; var n = old.name;
                Object.DestroyImmediate(old.gameObject);
                RoomBuilder.CreatePistol(parent, pos, n);
            }
        foreach (var p in Object.FindObjectsByType<Pistol>(FindObjectsInactive.Include, FindObjectsSortMode.None)) { AddGrabRules(p.gameObject); guns++; }
        var rigOrigin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>(FindObjectsInactive.Include);
        if (rigOrigin) AddMainSceneSystems(rigOrigin.gameObject);
        // pistolets en trop (appelés « Sniper » mais identiques au pistolet) : le kit d'armes se choisit maintenant dans le lobby
        foreach (var extra in Object.FindObjectsByType<Pistol>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (extra.name == "Special_Center" || extra.name.StartsWith("Sniper_")) { Object.DestroyImmediate(extra.gameObject); guns--; }
        foreach (var old in Object.FindObjectsByType<NetDuel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!old.GetComponent<Unity.Netcode.NetworkObject>()) Object.DestroyImmediate(old.gameObject);      // ancien gestionnaire de scène, sans objet réseau
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[Arène 1v1] mains réparées : " + hands + ", pistolets : " + guns);
    }

    internal static GameObject BuildXRRig(Vector3 spawnPos, Quaternion spawnRot)
    {
        var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XriDir + "Prefabs/XR Origin (XR Rig).prefab");
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(XriDir + "XRI Default Input Actions.inputactions");

        rig.transform.SetPositionAndRotation(spawnPos, spawnRot);

        foreach (var cim in rig.GetComponentsInChildren<ControllerInputActionManager>(true))
            cim.smoothTurnEnabled = true;

        AddHand(rig, "Left Controller", "l", "XRI Left Interaction", actions);
        AddHand(rig, "Right Controller", "r", "XRI Right Interaction", actions);

        foreach (var t in rig.GetComponentsInChildren<Transform>(true))
            if (t.name == "Grab Move" || t.name == "Climb" || t.name == "Climb Teleport" || t.name == "Teleportation")
                t.gameObject.SetActive(false);

        foreach (var jump in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump.JumpProvider>(true))
        {
            jump.gameObject.SetActive(true);
            jump.jumpHeight = 0.75f;
            jump.variableHeightJump = false;
            jump.unlimitedInAirJumps = false;
            jump.inAirJumpCount = 0;
        }

        var cc = rig.GetComponent<CharacterController>();
        if (cc)
        {
            cc.radius = 0.2f;
            cc.skinWidth = 0.02f;
            cc.stepOffset = 0.35f; // franchissement facile des marches de 20 cm
            cc.minMoveDistance = 0f;
        }

        rig.AddComponent<PlayerFallGuard>();
        rig.AddComponent<Crouch>();
        rig.AddComponent<PlayerHealth>();

        rig.GetComponent<Unity.XR.CoreUtils.XROrigin>().RequestedTrackingOriginMode = Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Device;

        var head = rig.GetComponentInChildren<Camera>(true);
        if (head)
        {
            head.farClipPlane = 60f;
            var ucd = head.GetComponent<UniversalAdditionalCameraData>() ?? head.gameObject.AddComponent<UniversalAdditionalCameraData>();
            ucd.renderPostProcessing = false;
            ucd.dithering = false;
        }

        return rig;
    }

    static void AddHand(GameObject rig, string controllerName, string side, string map, InputActionAsset actions)
    {
        var controller = rig.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == controllerName);
        if (!controller) return;

        foreach (var r in controller.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        // même main qu'en jeu normal (SetupVRScene) : interacteur direct dans la paume, prise par clic. Sans lui (Near-Far des Starter Assets),
        // la version PC ne trouvait aucune main et on ne pouvait ni prendre ni tirer.
        SetupVRScene.UseDirectInteractor(controller, side == "l" ? -1f : 1f);

        var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Animated Hands/Prefabs/{(side == "l" ? "Left" : "Right")} Hand Model.prefab");
        if (modelPrefab)
        {
            var hand = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, controller);
            hand.name = (side == "l" ? "Left" : "Right") + " Hand Model";
            hand.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            hand.transform.localRotation = Quaternion.Euler(0f, 0f, side == "l" ? 90f : -90f);

            var anim = hand.GetComponent<AnimateHandOnInput>() ?? hand.AddComponent<AnimateHandOnInput>();
            anim.handAnimator = hand.GetComponentInChildren<Animator>();
            if (actions)
            {
                anim.triggerValue = new InputActionProperty(actions.FindActionMap(map).FindAction("Activate Value"));
                anim.gripValue = new InputActionProperty(actions.FindActionMap(map).FindAction("Select Value"));
            }
        }
    }

    internal static void UpdateBuildSettings(string newScenePath)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == newScenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(newScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
