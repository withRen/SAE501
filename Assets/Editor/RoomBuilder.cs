using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Construit la scène de jeu à partir de la carte Blender (Assets/Room, exportée par Tools/blender/export_map.py)
// + tout le gameplay (planches, marteau, écrans de surveillance, ceinture, portes de niveau, générateur, zombies).
// Repère : Blender (x, y, z haut) -> Unity (-x, z, -y). Les positions viennent des objets et repères du fichier.
public static class RoomBuilder
{
    const string Dir = "Assets/Generated";

    static Material lockerM, darkM, metalM, plankM, ghostM, pistolM, headM, nailM, batteryM, magM, lightM, blockM;
    static Transform env, items, bunkerRoot, mapRoot;
    static Mesh plankMesh;
    static Material[] plankMats;
    static GameObject roomRoot;

    // planches de jeu : même taille que les planches clouées sur les fenêtres du fichier (1,84 m x 28 cm)
    public const float PlankLength = 1.84f, PlankWidth = 0.28f;

    // départ du joueur : au milieu du bunker, face aux deux fenêtres sud (là où arrivent les premiers zombies)
    static readonly Vector3 Spawn = B(-3.6f, -9.4f, 0f);
    static readonly Quaternion SpawnRot = Quaternion.LookRotation(Vector3.forward);

    // Blender -> Unity
    static Vector3 B(float x, float y, float z) => new Vector3(-x, z, -y);
    static Vector3 B(float[] v) => new Vector3(-v[0], v[2], -v[1]);

    [System.Serializable] class LightInfo { public string name, type; public float energy; public float[] color, loc, dir; public float spot_size, spot_blend, radius; public bool shadow; }
    [System.Serializable] class CamInfo { public string name; public float[] loc, dir, up; public float fov; }
    [System.Serializable] class MapMeta { public LightInfo[] lights; public CamInfo[] cameras; public float exposure; }
    static MapMeta meta;

    public static void Populate(GameObject rig)
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets", "Generated");
        // références gardées d'une reconstruction à l'autre : après un réimport de la carte elles pointeraient vers d'anciens matériaux (planches roses)
        plankMesh = null; plankMats = null; hammerGrab = null;
        MakeMaterials();
        EnsureFxMaterials();
        blockM = Mat("Blockout", new Color(0.05f, 0.05f, 0.055f));

        bunkerRoot = new GameObject("Bunker").transform;
        mapRoot = new GameObject("Map").transform;
        env = new GameObject("Environment").transform;
        env.SetParent(bunkerRoot, false);
        items = new GameObject("Items").transform;
        items.SetParent(bunkerRoot, false);

        // ambiance sombre
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.0008f, 0.0008f, 0.0012f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.06f, 0.075f, 0.1f);       // bleu nuit : le décor au loin se fond dans la brume au bas du ciel (SkySphere)
        RenderSettings.fogDensity = 0.06f;                   // léger : la profondeur reste lisible (rendu proche de Cycles)

        var cam = rig.GetComponentInChildren<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        rig.transform.SetPositionAndRotation(Spawn, SpawnRot);

        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(RoomImport.FbxPath);
        if (fbx == null) { Debug.LogError("Carte introuvable : exporte le .blend (Tools/blender/export_map.py) puis SAE501 > Salle > 1 - Importer la carte"); return; }
        meta = JsonUtility.FromJson<MapMeta>(File.ReadAllText(RoomImport.MetaPath));

        roomRoot = Object.Instantiate(fbx);
        roomRoot.name = "Carte (Blender)";
        roomRoot.transform.SetParent(mapRoot, false);

        var slots = CollectSlots(roomRoot.transform);
        var planks = CollectPlanks(roomRoot.transform);
        // retirés : planches clouées sur les fenêtres et sur la porte condamnée du couloir (les ouvertures démarrent sans barricade : c'est au joueur de clouer),
        // planches au sol du bunker (remplacées par la pile de réserve),
        // porte ouverte du bunker (elle gênait la barricade), marteaux de la cuisine et du stockage (un seul vrai marteau, dans le bunker)
        RemoveNodes(roomRoot.transform, n => n.StartsWith("Bunker_WindowPlank_") || n.StartsWith("Bunker_WinNE_FloorPlank_") || n == "Corridor_Door" || n == "Hammer" || n == "Stockage_Hammer" || n == "Door_Boarded_Planks");
        var hammerNode = Find(roomRoot.transform, "Bunker_Hammer_Workbench");
        var crowbarNode = Find(roomRoot.transform, "Bunker_Crowbar_Workbench");
        var axeNode = Find(roomRoot.transform, "Stockage_PH_Axe");
        var bunkerAxeNode = Find(roomRoot.transform, "Bunker_Axe_Stove");      // hache posée au sol, sous le bord de l'établi du bunker
        var genCrowbarNode = Find(roomRoot.transform, "GenRoom_Crowbar");       // carte finale : établi de la salle du générateur

        BuildMapLights();
        MakeMapColliders(roomRoot.transform);
        BuildExterior();
        BuildPlanks(planks);
        BuildHammer(hammerNode);
        // armes de corps à corps : dégâts en part des PV de base du zombie (tête, torse, jambes)
        BuildMelee(crowbarNode, "PiedDeBiche", 0.5f, 0.25f, 0.15f);
        BuildMelee(axeNode, "Hache", 1f, 0.5f, 1f / 3f);
        BuildMelee(bunkerAxeNode, "Hache_Bunker", 1f, 0.5f, 1f / 3f);
        BuildMelee(genCrowbarNode, "PiedDeBiche_Generateur", 0.5f, 0.25f, 0.15f);
        BuildMonitors(rig, cam, BuildCameras());
        BuildBreaker();
        BuildBatteries();
        BuildMagazines();
        BuildSupplyRadio();
        BuildBelt(rig, cam);
        BuildBunkerOpenings(slots);
        BuildGatesAndGenerator();
        BuildExteriorDecor();
        BuildZombieSetup(rig);
        BuildMusic();

        new GameObject("Menu_Pause").AddComponent<PauseMenu>();          // bouton menu de la manette gauche
        new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
        NavBuilder.Bake(mapRoot, rig, items.gameObject, GameObject.Find("MonitorStation"), GameObject.Find("Belt"), gatesRoot);
        LodBuilder.Build(roomRoot.transform);          // [#118] après le NavMesh : les LOD1 n'y entrent pas
        BuildLightProbes();
        UnbatchInteractables();
        LightBaker.ApplySettings();
    }

    // sondes de lumière : les objets qui bougent (zombies, armes, mains) prennent l'éclairage précalculé de l'endroit où ils sont
    // Objets de la carte devenus interactifs (écrans CCTV, disjoncteur…) : pas de regroupement statique. Sinon, en jeu, leur
    // MeshFilter renvoie le maillage fusionné de toute la carte et le contour (InteractableOutline) l'entoure en entier
    // (grande forme jaune devant les écrans). L'éclairage précalculé (ContributeGI) est conservé.
    static void UnbatchInteractables()
    {
        foreach (var it in Object.FindObjectsByType<XRBaseInteractable>(FindObjectsInactive.Include))
            foreach (var r in it.GetComponentsInChildren<MeshRenderer>(true))
            {
                var f = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
                GameObjectUtility.SetStaticEditorFlags(r.gameObject, f & ~StaticEditorFlags.BatchingStatic);
            }
    }

    // [#119] Un groupe de sondes par zone (MAP.md), plus serré dans les couloirs et les petites salles où les zombies passent
    // près des lampes, sur trois hauteurs (jambes, torse, tête) ; dehors, une grille large. Avant : un seul groupe tous les 2 m,
    // trop lâche pour les couloirs de 1,4 m (zombies noirs ou trop clairs selon la sonde la plus proche).
    // Zones en coordonnées Blender : (x min, x max, y min, y max, pas en m).
    static readonly (string name, float x0, float x1, float y0, float y1, float step)[] ProbeZones =
    {
        ("Bunker", -7.5f, 3.8f, -14.1f, -6.1f, 1.5f),
        ("Couloir_Gauche", -14.5f, -7.35f, -11.2f, 4.2f, 1f),
        ("Cuisine", -22f, -16.65f, -8f, -1.65f, 1.25f),
        ("Generateur", -19.7f, -14.5f, 0.35f, 4.65f, 1f),
        ("Couloir_Droit", 3.65f, 10.5f, -11.2f, 7.6f, 1f),
        ("Stockage", 10.5f, 16.8f, 3.85f, 9.15f, 1.25f),
    };
    static readonly float[] ProbeHeightsInside = { 0.4f, 1.2f, 1.9f };
    static readonly float[] ProbeHeightsOutside = { 0.5f, 1.7f };

    static void BuildLightProbes()
    {
        var root = new GameObject("Sondes_Lumiere").transform;
        root.SetParent(mapRoot, false);
        var zones = new List<Bounds>();
        int total = 0;
        foreach (var z in ProbeZones)
        {
            var a = B(z.x0, z.y0, 0f); var c = B(z.x1, z.y1, 0f);
            var zb = new Bounds((a + c) / 2f, new Vector3(Mathf.Abs(a.x - c.x), 10f, Mathf.Abs(a.z - c.z)));
            zones.Add(zb);
            total += ProbeGroup(root, "Sondes_" + z.name, zb, z.step, ProbeHeightsInside, null);
        }
        // extérieur et reste de la carte : grille de 3 m, hors des zones déjà couvertes
        var b = new Bounds(roomRoot.transform.position, Vector3.zero);
        foreach (var r in roomRoot.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
        total += ProbeGroup(root, "Sondes_Exterieur", b, 3f, ProbeHeightsOutside, zones);
        Debug.Log($"[RoomBuilder] {total} sondes de lumière en {ProbeZones.Length + 1} groupes");
    }

    static int ProbeGroup(Transform parent, string name, Bounds b, float step, float[] heights, List<Bounds> exclude)
    {
        var pts = new List<Vector3>();
        for (float x = b.min.x + step / 2f; x <= b.max.x; x += step)
            for (float z = b.min.z + step / 2f; z <= b.max.z; z += step)
            {
                if (exclude != null && exclude.Any(e => e.Contains(new Vector3(x, e.center.y, z)))) continue;
                // sol sous le point (pièces : sous le plafond ; dehors : terrain)
                if (!Physics.Raycast(new Vector3(x, 2.4f, z), Vector3.down, out var hit, 3f, ~0, QueryTriggerInteraction.Ignore)) continue;
                foreach (var h in heights)
                {
                    var p = hit.point + Vector3.up * h;
                    if (!Physics.CheckSphere(p, 0.15f, ~0, QueryTriggerInteraction.Ignore)) pts.Add(p);
                }
            }
        if (pts.Count == 0) return 0;
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<LightProbeGroup>().probePositions = pts.ToArray();
        return pts.Count;
    }

    // ---------- lecture de la carte ----------
    struct Pose { public string name; public Vector3 pos; public Quaternion rot; }

    static void RemoveNodes(Transform root, System.Func<string, bool> match)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(x => x != root && match(x.name)).ToList())
            if (t) Object.DestroyImmediate(t.gameObject);
    }

    // emplacements de planches du fichier : seuls ceux du bunker servent (les zombies n'attaquent que le bunker)
    static List<Pose> CollectSlots(Transform root)
    {
        var res = new List<Pose>();
        foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(x => x.name.Contains("PlankSlot_")).ToList())
        {
            if (t.name.StartsWith("Bunker_PlankSlot_")) res.Add(new Pose { name = t.name, pos = t.position, rot = t.rotation });
            Object.DestroyImmediate(t.gameObject);
        }
        return res;
    }

    // planches posées dans la carte (cuisine, stockage) : elles deviennent des planches de jeu, au même endroit et dans le même sens
    static List<Pose> CollectPlanks(Transform root)
    {
        var res = new List<Pose>();
        foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(x => System.Text.RegularExpressions.Regex.IsMatch(x.name, @"^(Stockage_)?Plank_\d+$")).OrderBy(x => x.name).ToList())
        {
            var mf = t.GetComponent<MeshFilter>();
            var b = mf.sharedMesh.bounds;
            // axes du maillage, du plus long au plus fin -> longueur (X), largeur (Y), épaisseur (Z) de la planche de jeu
            var axes = new[] { 0, 1, 2 }.OrderByDescending(i => b.size[i]).ToArray();
            Vector3 Ax(int i) { var v = Vector3.zero; v[i] = 1f; return t.TransformDirection(v).normalized; }
            var width = Ax(axes[1]); var thick = Ax(axes[2]);
            if (Vector3.Dot(thick, Vector3.up) < 0f) thick = -thick;
            res.Add(new Pose { name = t.name, pos = t.TransformPoint(b.center), rot = Quaternion.LookRotation(thick, width) });
            if (plankMesh == null && t.name == "Plank_01") { plankMesh = mf.sharedMesh; plankMats = t.GetComponent<Renderer>().sharedMaterials; }
            Object.DestroyImmediate(t.gameObject);
        }
        return res;
    }

    // ---------- collisions ----------
    // sans collision : décor plat ou suspendu, et tout ce qui boucherait un passage (cadres, plantes grimpantes, tuyaux sur toute la longueur des couloirs)
    static readonly string[] NoCollider = { "Debris", "Papers", "Rug", "Runner", "Cornice", "Wainscot", "Backsplash", "PictureFrame", "Handle", "Frame",
        "WindowSash", "WindowGlass", "Chair_Overturned", "Brick", "Apoc_", "Puddle", "OilStain", "Cables", "CableConduits", "Pendant", "Sconce",
        "CCTV_Monitor", "CCTV_Bezel", "LEDs", "TallyLED", "CameraMesh", "Mount", "Bulb", "Door_Boarded_Planks",
        "Canopy", "Canopies", "OilSpills", "CeilingVines", "Cabin_Roof" };   // jardin : feuillage des arbres, toit de la cabane ; salle du générateur : flaques, lianes
    // structure : collider exact (murs percés de portes et de fenêtres, sols en L)
    // jardin (carte finale) : terrain vallonné, allée, haies, rochers, piliers du portail, troncs ; tuyaux de la salle du générateur
    // (une boîte englobante bloquerait tout : la haie fait le tour du jardin, les tuyaux longent toute la salle)
    static readonly string[] Structural = { "Wall", "Floor", "Ceiling", "Trim", "Stub", "Terrain", "StonePath", "Hedges", "Rocks", "Pillars", "Trunk", "IndustrialPipes", "EscapeRoad" };

    static void MakeMapColliders(Transform root)
    {
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            // statique : regroupement des maillages + éclairage précalculé (lightmaps, SAE501 > Cuire l'éclairage)
            GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            var n = r.name;
            if (NoCollider.Any(k => n.Contains(k))) continue;
            if (Structural.Any(k => n.Contains(k))) r.gameObject.AddComponent<MeshCollider>().sharedMesh = r.GetComponent<MeshFilter>().sharedMesh;
            else r.gameObject.AddComponent<BoxCollider>();
        }
    }

    // ---------- lumières du fichier ----------
    // niveau général de toutes les lumières de la carte (1 = valeurs d'origine) : baissé pour une ambiance plus sombre
    const float LightLevel = 0.62f;     // calé sur le rendu Cycles de la carte (même vue, luminosité moyenne mesurée)
    static readonly List<Light> generatorLights = new List<Light>();
    static readonly List<float> generatorLightLevels = new List<float>();
    // luminosité du ciel (sphère d'Alban) vu par les fenêtres et le trou du toit
    const float SkyLevel = 0.3f;
    static void BuildMapLights()
    {
        generatorLights.Clear(); generatorLightLevels.Clear();
        var root = new GameObject("Lumieres").transform;
        root.SetParent(env, false);
        foreach (var li in meta.lights)
        {
            var g = new GameObject(li.name);
            g.transform.SetParent(root, false);
            g.transform.position = B(li.loc);
            var l = g.AddComponent<Light>();
            // énergie vue dans Blender : la carte est réglée avec une exposition (Color Management), 2^exposition
            float energy = li.energy * Mathf.Pow(2f, meta.exposure);
            l.color = new Color(li.color[0], li.color[1], li.color[2]);
            // Blender rend avec AgX, qui désature les lumières chaudes : sans ça tout sortait orange vif (mesuré : rouge/vert 4,9 contre 2,2)
            Color.RGBToHSV(l.color, out var hue, out var sat, out var val);
            l.color = Color.HSVToRGB(hue, sat * 0.55f, val);
            l.shadows = LightShadows.None;
            if (li.type == "SPOT")
            {
                l.type = LightType.Spot;
                g.transform.rotation = Quaternion.LookRotation(B(li.dir));
                l.spotAngle = Mathf.Clamp(li.spot_size * Mathf.Rad2Deg, 10f, 170f);
                l.innerSpotAngle = l.spotAngle * (1f - Mathf.Clamp01(li.spot_blend));
                l.intensity = energy / 24f;            // même conversion que les ampoules : dans Blender un spot de N W éclaire comme une ampoule de N W (dans son cône)
                l.range = Mathf.Max(9f, li.loc[2] * 2f);   // lune haute (jardin : 14 m) : portée large, sinon l'atténuation de fin de portée l'éteint avant le sol
            }
            else
            {
                l.type = LightType.Point;
                l.intensity = energy / 24f;            // 18 W (lampe à huile, exposition 0) -> 0,75
                l.range = Mathf.Clamp(1.1f * Mathf.Sqrt(energy), 2.5f, 6.5f);
            }
            l.intensity *= LightLevel;                 // ambiance générale (voir LightLevel)
            // éclairage précalculé (rebonds, ombres douces comme dans Cycles) pour les lampes fixes ; restent en temps réel
            // celles qui changent en jeu : lampes qui vacillent (établi, poêle, lampes à huile) et lumières du générateur
            bool live = li.name == "Light_Bunker_Workbench" || li.name.Contains("BarrelStove") || li.name.Contains("OilLamp")
                     || li.name == "Light_GeneratorBulb" || li.name.StartsWith("Light_GeneratorRoom");
            l.lightmapBakeType = live ? LightmapBakeType.Realtime : LightmapBakeType.Baked;
            if (!live) { l.shadows = LightShadows.Soft; l.shapeRadius = Mathf.Max(0.05f, li.radius); }
            // lampe à huile de l'établi : la seule avec des ombres (planches, chaises), et qui vacille
            if (li.name == "Light_Bunker_Workbench")
            {
                l.shadows = LightShadows.Soft; l.shadowStrength = 0.95f; l.shadowBias = 0.03f; l.shadowNormalBias = 0.4f;
                var f = g.AddComponent<FlickerLamp>(); f.lamp = l; f.baseIntensity = l.intensity; f.hum = 0.08f; f.humSpeed = 2.4f; f.dipDepth = 0.75f; f.burstMinGap = 5f; f.burstMaxGap = 14f;
            }
            else if (li.name.Contains("BarrelStove") || li.name.Contains("OilLamp"))
            {
                var f = g.AddComponent<FlickerLamp>(); f.lamp = l; f.baseIntensity = l.intensity; f.hum = 0.12f; f.humSpeed = 5f; f.dipDepth = 0.85f;
            }
            // générateur à l'arrêt : les lumières de sa salle sont presque éteintes (le Generator les rallume)
            if (li.name == "Light_GeneratorBulb") l.intensity = 0.08f;
            else if (li.name.StartsWith("Light_GeneratorRoom")) { generatorLights.Add(l); generatorLightLevels.Add(l.intensity); l.intensity *= 0.08f; }
        }
    }

    // ---------- extérieur : bande de terrain au sud du bunker, d'où les zombies attaquent les deux fenêtres ----------
    static void BuildExterior()
    {
        var ext = new GameObject("Exterieur_Sud").transform;
        ext.SetParent(mapRoot, false);
        // bunker : x Blender -7,5..3,8 / mur sud y -14 ; le terrain va de y -14 à -19
        var c = B(-1.85f, -16.6f, -0.05f);
        if (Find(roomRoot.transform, "Exterior_World_Terrain") != null) return;   // la carte a son propre terrain extérieur : pas de sol ajouté
        var sol = Box("Sol", ext, c, new Vector3(13f, 0.1f, 5.2f), blockM);
        GameObjectUtility.SetStaticEditorFlags(sol, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
    }

    // ---------- décor d'Alban (branche features_exterieur) : sphère de ciel (panorama de forêt de nuit) ----------
    // Le prefab est extrait de sa scène (la forêt en relief n'est pas reprise). La sphère passe sur un shader sans brouillard
    // (sinon notre brouillard dense la rendait noire), rétrécie à 50 m et collée à la tête du joueur (SkyFollow) pour rester
    // sous la distance d'affichage de la caméra (60 m). Les cadres, plaques et voyants des portes de niveau sont rattachés
    // au battant de chaque porte (ils bougent avec elle quand elle s'ouvre).
    const string ExteriorPrefab = "Assets/Environment/Exterieur_Alban.prefab";
    const string SkyMatPath = "Assets/Environment/Materials/M_NightForest_SkySphere_SansBrouillard.mat";
    static void BuildExteriorDecor()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExteriorPrefab);
        if (prefab == null) return;
        var ext = (GameObject)PrefabUtility.InstantiatePrefab(prefab, mapRoot);
        PrefabUtility.UnpackPrefabInstance(ext, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        var sphere = ext.transform.Find("Environment_Skysphere");
        if (sphere != null)
        {
            var r = sphere.GetComponent<Renderer>();
            var src = r.sharedMaterial;
            var m = AssetDatabase.LoadAssetAtPath<Material>(SkyMatPath);
            if (m == null) { m = new Material(Shader.Find("SAE501/SkySphere")); AssetDatabase.CreateAsset(m, SkyMatPath); }
            m.shader = Shader.Find("SAE501/SkySphere");
            m.SetTexture("_BaseMap", src.GetTexture("_BaseMap"));
            m.SetTextureScale("_BaseMap", src.GetTextureScale("_BaseMap"));
            m.SetTextureOffset("_BaseMap", src.GetTextureOffset("_BaseMap"));
            m.SetColor("_BaseColor", src.GetColor("_BaseColor") * SkyLevel);   // ciel assombri
            EditorUtility.SetDirty(m);
            r.sharedMaterial = m;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            GameObjectUtility.SetStaticEditorFlags(sphere.gameObject, 0);   // elle bouge avec la tête
            sphere.localScale = Vector3.one * 50f;
            sphere.gameObject.AddComponent<SkyFollow>();
        }

        var portes = ext.transform.Find("Portes");
        if (portes != null && gatesRoot != null)
        {
            foreach (Transform deco in portes)
            {
                var leaf = gatesRoot.transform.Find(deco.name + "/Charniere/Battant") ?? gatesRoot.transform.Find(deco.name + "/Battant");
                if (leaf == null) continue;
                for (int i = deco.childCount - 1; i >= 0; i--) deco.GetChild(i).SetParent(leaf, true);
            }
            Object.DestroyImmediate(portes.gameObject);
        }
    }

    // ---------- disjoncteur des caméras de surveillance : boîtier électrique du mur est du bunker ----------
    // Au départ les caméras sont coupées (écrans noirs) ; on actionne le disjoncteur (grip) pour les allumer (Breaker, CctvPower).
    static void BuildBreaker()
    {
        var box = Find(roomRoot.transform, "Bunker_PowerBox_East");
        if (box == null) { Debug.LogWarning("Boîtier électrique introuvable : Bunker_PowerBox_East"); return; }
        var r = box.GetComponent<Renderer>();
        var b = r.bounds;
        // face avant : côté le plus fin du boîtier, tournée vers l'intérieur du bunker
        int thin = b.size.x <= b.size.z ? 0 : 2;
        var toCenter = B(-1.85f, -10.1f, 0f) - b.center;
        var front = Vector3.zero; front[thin] = Mathf.Sign(toCenter[thin]);
        // [#111] levier physique : pivot sur la face avant, poignée vers le joueur ; on l'abaisse pour enclencher (Lever, Breaker)
        var lever = MakeLever("Levier_Disjoncteur", box, b.center + front * (b.extents[thin] + 0.02f) + Vector3.down * 0.02f, front, 0.16f, false);
        var led = Box("Voyant", box, Vector3.zero, Vector3.one * 0.025f, lightM, false);
        led.transform.SetParent(null, true);
        led.transform.position = b.center + front * (b.extents[thin] + 0.015f) + Vector3.up * 0.15f;
        led.transform.localScale = Vector3.one * 0.025f;
        led.transform.SetParent(box, true);
        var br = box.gameObject.AddComponent<Breaker>();
        br.lever = lever; br.led = led.GetComponent<Renderer>();
    }

    // [#111] Levier physique : pivot (Lever, tourne autour de son x local) + tige + boule qu'on attrape (XRSimpleInteractable).
    // front = côté du joueur : au repos la tige part vers lui ; levée de 50° (haut), abaissée de 50° (bas).
    static Lever MakeLever(string name, Transform parent, Vector3 pivotPos, Vector3 front, float length, bool spring)
    {
        var right = Vector3.Cross(Vector3.up, front).normalized;
        var pivot = new GameObject(name).transform;
        pivot.SetParent(parent, true);
        pivot.SetPositionAndRotation(pivotPos, Quaternion.LookRotation(front, Vector3.up));
        Box("Axe", pivot, Vector3.zero, new Vector3(0.06f, 0.025f, 0.025f), metalM, false);
        Box("Tige", pivot, new Vector3(0f, 0f, length / 2f), new Vector3(0.018f, 0.018f, length), metalM, false);
        var knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        knob.name = "Boule";
        knob.transform.SetParent(pivot, false);
        knob.transform.localPosition = new Vector3(0f, 0f, length);
        knob.transform.localScale = Vector3.one * 0.05f;
        knob.GetComponent<Renderer>().sharedMaterial = Mat(name + "_Boule", new Color(0.6f, 0.1f, 0.1f), 0.4f);
        var sc = knob.GetComponent<SphereCollider>();
        sc.radius = 1.6f;                                   // zone de prise : 16 cm de diamètre autour de la boule de 5 cm
        var lv = pivot.gameObject.AddComponent<Lever>();
        lv.handle = knob.AddComponent<XRSimpleInteractable>();
        lv.springBack = spring;
        return lv;
    }

    // ---------- ouvertures du bunker (zombies) : 2 portes, 2 fenêtres, 3 planches chacune ----------
    static void AddSlots(Transform holder, string name, IList<Vector3> positions, Vector3 inward)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            var s = new GameObject($"Slot_{name}_{i + 1}");
            s.transform.SetParent(holder, false);
            s.transform.SetPositionAndRotation(positions[i], Quaternion.LookRotation(inward, Vector3.up));
            var ghost = Box("Ghost", s.transform, Vector3.zero, new Vector3(PlankLength + 0.04f, PlankWidth + 0.03f, 0.04f), ghostM, false);
            var gr = ghost.GetComponent<Renderer>();
            gr.enabled = false;   // visible uniquement quand une planche est en main
            s.AddComponent<BarricadeSlot>().ghost = gr;
        }
    }

    static void BuildBunkerOpenings(List<Pose> fileSlots)
    {
        var holder = new GameObject("BarricadeSlots").transform;
        holder.SetParent(mapRoot, false);
        float[] doorY = { 0.55f, 1.10f, 1.65f };

        // portes (pas d'emplacements dans le fichier) : ouest vers le couloir gauche, est vers le couloir droit, planches contre la face intérieure du mur
        var west = B(-7.28f, -10.1f, 0f); var east = B(3.58f, -10.1f, 0f);
        AddSlots(holder, "Ouest", doorY.Select(y => new Vector3(west.x, y, west.z)).ToList(), Vector3.left);
        AddSlots(holder, "Est", doorY.Select(y => new Vector3(east.x, y, east.z)).ToList(), Vector3.right);
        // fenêtres sud : emplacements du fichier (bas, milieu, haut)
        foreach (var w in new[] { "SW", "SE" })
            AddSlots(holder, w, fileSlots.Where(s => s.name.Contains("_" + w + "_")).OrderBy(s => s.pos.y).Select(s => s.pos).ToList(), Vector3.back);

        MakeOpening(holder, "Ouest", B(-7.43f, -10.1f, 1.0f), B(-8.4f, -10.1f, 0f), B(-6.3f, -10.1f, 0f), false);
        MakeOpening(holder, "Est", B(3.72f, -10.1f, 1.0f), B(4.6f, -10.1f, 0f), B(2.6f, -10.1f, 0f), false);
        MakeOpening(holder, "SW", B(-4.4f, -13.92f, 1.5f), B(-4.4f, -14.9f, 0f), B(-4.4f, -12.6f, 0f), true);
        MakeOpening(holder, "SE", B(0.7f, -13.92f, 1.5f), B(0.7f, -14.9f, 0f), B(0.95f, -12.45f, 0f), true);
        // trous du toit du bunker (carte finale, repérés par rayons : ~1,2 x 1,2 m) : les zombies du toit s'y laissent tomber.
        // Pas de planches : on ne peut pas les barricader. Approche sur le toit (y = 2,8 m), arrivée au sol dessous.
        MakeOpening(holder, "Toit_Lits", new Vector3(3.9f, 2.8f, 7.9f), new Vector3(3.9f, 2.8f, 6.85f), new Vector3(3.9f, 0f, 7.9f), true);
        MakeOpening(holder, "Toit_Etabli", new Vector3(-1.1f, 2.8f, 12.3f), new Vector3(-1.1f, 2.8f, 13.35f), new Vector3(-1.0f, 0f, 12.0f), true);
    }

    // window = vrai : le zombie passe par la fenêtre grâce à un lien NavMesh (il n'y a pas de sol continu sous l'allège)
    static Vector3 Ground(Vector3 p)
    {
        Physics.SyncTransforms();
        return Physics.Raycast(p + Vector3.up * 1.2f, Vector3.down, out var h, 3f, ~0, QueryTriggerInteraction.Ignore) ? h.point : p;
    }

    static void MakeOpening(Transform holder, string name, Vector3 center, Vector3 approach, Vector3 inside, bool window)
    {
        // carte avec terrain en relief : les points d'approche et d'arrivée sont posés sur le sol réel (sinon enterrés ou en l'air)
        approach = Ground(approach); inside = Ground(inside);
        var go = new GameObject("Ouverture_" + name);
        go.transform.SetParent(holder, false);
        go.transform.position = center;
        var o = go.AddComponent<Opening>();
        o.openingName = name;
        o.zombiesCanUse = true;
        o.slots = holder.Cast<Transform>().Where(c => c.name.StartsWith("Slot_" + name + "_")).Select(c => c.GetComponent<BarricadeSlot>()).ToArray();
        var a = new GameObject("Approche").transform; a.SetParent(go.transform, false); a.position = approach; o.approachPoint = a;
        var i = new GameObject("Interieur").transform; i.SetParent(go.transform, false); i.position = inside; o.insidePoint = i;
        if (window)
        {
            var link = go.AddComponent<NavMeshLink>();
            link.startPoint = go.transform.InverseTransformPoint(approach);
            link.endPoint = go.transform.InverseTransformPoint(inside);
            link.width = 1.0f;
            link.bidirectional = false;
            link.agentTypeID = 0;
            link.area = Zombie.PassageArea;   // réservé aux zombies qui entrent (voir Zombie.WalkMask)
        }
        if (name.StartsWith("Toit_"))
        {
            o.fromAbove = true;                  // [#104] le zombie prévient avant de tomber
            o.dust = MakeDust(go.transform, new Vector3(center.x, 2.62f, center.z));
        }
    }

    // [#104] poussière qui tombe d'un trou du toit, sous le plafond du bunker (jouée par Opening.WarnFromAbove)
    static Material dustM;
    static ParticleSystem MakeDust(Transform parent, Vector3 pos)
    {
        if (!dustM)
        {
            dustM = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            dustM.SetFloat("_Surface", 1f); dustM.SetFloat("_Blend", 0f);
            dustM.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); dustM.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); dustM.SetFloat("_ZWrite", 0f);
            dustM.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            dustM.SetOverrideTag("RenderType", "Transparent");
            dustM.renderQueue = (int)RenderQueue.Transparent;
            var tex = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
            if (tex) dustM.SetTexture("_BaseMap", tex);
            dustM.SetColor("_BaseColor", Color.white);
            AssetDatabase.CreateAsset(dustM, $"{Dir}/Poussiere_Toit.mat");
        }
        var g = new GameObject("Poussiere");
        g.transform.SetParent(parent, false);
        g.transform.SetPositionAndRotation(pos, Quaternion.identity);
        var ps = g.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false; main.loop = false; main.duration = 3.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.15f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.85f, 0.8f, 0.7f, 0.8f), new Color(0.6f, 0.55f, 0.48f, 0.95f));
        main.gravityModifier = 0.25f;
        main.maxParticles = 400;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission;
        em.rateOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 60f, 1f, 160f));   // de plus en plus fort jusqu'à la chute
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });
        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale = new Vector3(1.0f, 0.02f, 1.0f);                // le trou fait ~1,2 x 1,2 m
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;
        var r = g.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = dustM;
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows = false;
        return ps;
    }

    // ---------- planches ----------
    static void BuildPlanks(List<Pose> planks)
    {
        // planches de la cuisine et du stockage, là où elles sont dans la carte
        foreach (var p in planks) MakePlank(p.pos, p.rot, plankMesh, plankMats).name = "Planche_" + p.name;
        // réserve du bunker : râtelier sur le mur sud, entre les deux fenêtres (face au joueur, à hauteur de main).
        // 3 rangées de 2 planches, à plat contre le mur ; elles restent accrochées jusqu'à ce qu'on les prenne.
        var rack = new GameObject("Ratelier_Planches").transform;
        rack.SetParent(bunkerRoot, false);
        var faceRoom = Quaternion.LookRotation(Vector3.back, Vector3.up);       // comme sur une fenêtre : longueur le long du mur
        float[] rows = { 1.0f, 1.32f, 1.64f };
        for (int r = 0; r < rows.Length; r++)
        {
            for (int d = 0; d < 2; d++)
            {
                var pl = MakePlank(B(-1.85f, -13.835f + 0.035f * d, rows[r]), faceRoom, plankMesh, plankMats);
                pl.name = $"Planche_Ratelier_{r + 1}_{d + 1}";
                pl.GetComponent<Plank>().stored = true;
            }
            // deux supports métalliques sous chaque rangée
            foreach (float dx in new[] { -0.7f, 0.7f })
                Box("Support", rack, B(-1.85f + dx, -13.82f, rows[r] - PlankWidth / 2f - 0.015f), new Vector3(0.05f, 0.03f, 0.1f), metalM, false);
        }
    }

    // ---------- caméras de surveillance : celles du fichier (couloir gauche, couloir droit, cuisine, stockage) ----------
    // [#103] + une caméra ajoutée ici sur l'extérieur sud (zone d'arrivée du niveau 1 : les deux fenêtres et le point d'apparition)
    public const string SouthCamName = "CCTV_Camera_05_Exterieur_Sud";
    static readonly Vector3 SouthCamPos = new Vector3(7.3f, 2.6f, 15.6f);           // au-delà de l'angle sud-ouest du bunker (Blender (-7,3, -15,6)), à 1,5 m du mur
    static readonly Vector3 SouthCamLook = new Vector3(0.5f, 0.4f, 15.6f);          // le long du mur sud : les deux fenêtres à gauche, le point d'apparition (1,85, 17,6) au centre

    static Camera[] BuildCameras()
    {
        var list = new List<Camera>();
        var infos = meta.cameras.OrderBy(x => x.name).Select(c => (c.name, pos: B(c.loc), rot: Quaternion.LookRotation(B(c.dir), B(c.up)), c.fov)).ToList();
        infos.Add((SouthCamName, SouthCamPos, Quaternion.LookRotation(SouthCamLook - SouthCamPos, Vector3.up), 90f * Mathf.Deg2Rad));
        foreach (var c in infos)
        {
            var g = new GameObject(c.name);
            g.transform.SetParent(mapRoot, false);
            g.transform.SetPositionAndRotation(c.pos, c.rot);
            var cam = g.AddComponent<Camera>();
            // angle horizontal de Blender (90°) -> angle vertical Unity pour une image 16:9
            cam.fieldOfView = 2f * Mathf.Atan(Mathf.Tan(c.fov / 2f) * 9f / 16f) * Mathf.Rad2Deg;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 25f;
            var ucd = g.AddComponent<UniversalAdditionalCameraData>();
            ucd.renderShadows = false;
            ucd.renderPostProcessing = false;
            g.AddComponent<CctvOverlay>().label = CctvOverlay.LabelFor(c.name);   // [7.2] « ● REC », nom de la caméra, heure
            list.Add(cam);
        }
        return list.ToArray();
    }

    static void BuildBatteries()
    {
        MakeBattery(B(1.45f, -7.0f, 0.81f));      // sur le bureau de commandement
        MakeBattery(B(-2.6f, -11.75f, 0.92f));    // sur l'établi
        MakeBattery(B(-5.5f, -8.6f, 0.04f));      // au sol, près des lits
        MakeBattery(B(-20.5f, -2.1f, 0.92f));     // dans la cuisine (niveau 2)
    }

    // son d'effet : Vorbis ; loadType = DecompressOnLoad (court, sans latence) ou CompressedInMemory (plus long, moins de mémoire)
    static AudioClip SfxClip(string path, AudioClipLoadType loadType, bool mono)
    {
        if (AssetImporter.GetAtPath(path) is AudioImporter ai)
        {
            var st = ai.defaultSampleSettings;
            if (st.loadType != loadType || st.compressionFormat != AudioCompressionFormat.Vorbis || ai.forceToMono != mono)
            {
                st.loadType = loadType;
                st.compressionFormat = AudioCompressionFormat.Vorbis;
                st.quality = 0.7f;
                ai.defaultSampleSettings = st;
                ai.forceToMono = mono;
                ai.SaveAndReimport();
            }
        }
        var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (!c) Debug.LogWarning("Son introuvable : " + path);
        return c;
    }

    public static AudioClip[] ZombieVoices() => new[] {
        SfxClip("Assets/Audio/Zombie/Zombie_Voix_1.mp3", AudioClipLoadType.CompressedInMemory, true),
        SfxClip("Assets/Audio/Zombie/Zombie_Voix_2.mp3", AudioClipLoadType.CompressedInMemory, true) };

    // sons courts du pistolet : décompressés au chargement (pas de latence au tir), Vorbis pour rester léger sur le Quest
    static AudioClip GunClip(string path)
    {
        if (AssetImporter.GetAtPath(path) is AudioImporter ai)
        {
            var st = ai.defaultSampleSettings;
            if (st.loadType != AudioClipLoadType.DecompressOnLoad || st.compressionFormat != AudioCompressionFormat.Vorbis)
            {
                st.loadType = AudioClipLoadType.DecompressOnLoad;
                st.compressionFormat = AudioCompressionFormat.Vorbis;
                st.quality = 0.7f;
                ai.defaultSampleSettings = st;
                ai.forceToMono = true;          // le son vient de l'arme : spatialisé en 3D, une seule voix suffit
                ai.SaveAndReimport();
            }
        }
        var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (!c) Debug.LogWarning("Son introuvable : " + path);
        return c;
    }

    // ---------- musique de fond : les deux pistes du jeu, en alternance ----------
    static void BuildMusic()
    {
        string[] paths = { "Assets/Audio/Music/Musique_Principale_1.mp3", "Assets/Audio/Music/Musique_Principale_2.mp3" };
        var clips = new List<AudioClip>();
        foreach (var p in paths)
        {
            // musique longue : lue en streaming (pas chargée entière en mémoire sur le Quest), compressée en Vorbis
            if (AssetImporter.GetAtPath(p) is AudioImporter ai)
            {
                var st = ai.defaultSampleSettings;
                if (st.loadType != AudioClipLoadType.Streaming || st.compressionFormat != AudioCompressionFormat.Vorbis)
                {
                    st.loadType = AudioClipLoadType.Streaming;
                    st.compressionFormat = AudioCompressionFormat.Vorbis;
                    st.quality = 0.6f;
                    ai.defaultSampleSettings = st;
                    ai.SaveAndReimport();
                }
            }
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
            if (c) clips.Add(c); else Debug.LogWarning("Musique introuvable : " + p);
        }
        var go = new GameObject("Musique");
        go.transform.SetParent(bunkerRoot, false);
        go.AddComponent<MusicPlayer>().tracks = clips.ToArray();
    }

    // ---------- portes de niveau et générateur ----------
    static GameObject gatesRoot;

    static void BuildGatesAndGenerator()
    {
        gatesRoot = new GameObject("Portes_Niveaux");
        gatesRoot.transform.SetParent(mapRoot, false);
        // pos = centre du passage (au sol) ; viewer = côté d'où le joueur arrive (le panneau y est lisible)
        void Gate(string name, string zone, string contents, int level, Vector3 pos, float width, Vector3 viewer)
        {
            var root = new GameObject("Porte_" + name).transform;
            root.SetParent(gatesRoot.transform, false);
            // charnière sur un bord du passage (le passage s'étend sur Z), battant en enfant ; la porte s'ouvre vers le joueur (viewer)
            var hinge = new GameObject("Charniere").transform;
            hinge.SetParent(root, false);
            hinge.position = pos - Vector3.forward * (width / 2f);
            var leaf = Box("Battant", hinge, Vector3.zero, new Vector3(0.12f, 2.1f, width), metalM);
            leaf.transform.position = pos + Vector3.up * 1.05f;
            var obstacle = leaf.AddComponent<UnityEngine.AI.NavMeshObstacle>();      // fermée : les zombies ne passent pas
            obstacle.shape = UnityEngine.AI.NavMeshObstacleShape.Box; obstacle.size = Vector3.one; obstacle.carving = true;
            // poignée côté joueur, près du bord libre (enfant de la charnière : elle tourne avec la porte)
            var handle = Box("Poignee", hinge, Vector3.zero, new Vector3(0.04f, 0.2f, 0.04f), darkM, false);
            handle.transform.position = pos + Vector3.up * 1.0f + Vector3.forward * (width / 2f - 0.15f) + viewer * 0.1f;
            var hbc = handle.AddComponent<BoxCollider>(); hbc.size = new Vector3(4f, 1.5f, 4f);   // zone de prise plus large que la poignée (16 x 30 cm)
            var hi = handle.AddComponent<XRSimpleInteractable>();
            var door = hinge.gameObject.AddComponent<HingedDoor>();
            door.leaf = leaf.transform; door.handle = hi; door.openSign = viewer.x > 0f ? 1f : -1f;
            var tm = MakeLabel("Panneau", root, pos + Vector3.up * 1.5f + viewer * 0.09f, Quaternion.LookRotation(-viewer), 0.012f, new Color(1f, 0.2f, 0.15f));
            var g = root.gameObject.AddComponent<ZoneGate>();
            g.unlockLevel = level; g.zoneName = zone; g.contents = contents; g.door = leaf.transform; g.doorCollider = leaf.GetComponent<Collider>(); g.sign = tm; g.hinged = door;
        }
        // [#106] contenu de chaque zone (panneau « VERROUILLÉ » et annonce du niveau) : ce que le joueur gagne à y aller
        Gate("Couloir_Gauche", "Couloir gauche et cuisine", "Planches, chargeurs, batterie", 2, B(-7.43f, -10.1f, 0f), 1.4f, Vector3.left);
        Gate("Couloir_Droit", "Couloir droit", "Passage vers le stockage", 2, B(3.72f, -10.1f, 0f), 1.8f, Vector3.right);
        Gate("Stockage", "Stockage", "Planches, chargeurs, hache", 3, B(10.58f, 6.5f, 0f), 1.85f, Vector3.right);
        Gate("Generateur", "Générateur", "Le générateur : votre sortie", 4, B(-14.58f, 2.5f, 0f), 1.25f, Vector3.left);
        // portes ouest / est du bunker : barricadables (planches de l'ouverture) et enfoncées par les zombies avant les planches
        foreach (var (gate, op) in new[] { ("Porte_Couloir_Gauche", "Ouverture_Ouest"), ("Porte_Couloir_Droit", "Ouverture_Est") })
        {
            var door = gatesRoot.transform.Find(gate)?.GetComponentInChildren<HingedDoor>();
            var opening = GameObject.Find(op)?.GetComponent<Opening>();
            if (door && opening) { door.opening = opening; opening.door = door; }
        }

        // le générateur du fichier : [#111] un levier à ressort à la place de son petit interrupteur (l'abaisser et le tenir 5 s)
        var genNode = Find(roomRoot.transform, "Generator");
        var sw = Find(roomRoot.transform, "Generator_Switch");
        var gen = new GameObject("Generateur_Objectif").transform;
        gen.SetParent(mapRoot, false);
        gen.position = sw.position;
        Box("Platine", gen, new Vector3(0f, 0.32f, 0.02f), new Vector3(0.16f, 0.24f, 0.03f), darkM, false);
        var genLever = MakeLever("Levier_Generateur", gen, sw.position + new Vector3(0f, 0.35f, 0.06f), Vector3.forward, 0.28f, true);
        // panneau d'état au-dessus de l'interrupteur, lisible depuis l'entrée de la salle (côté sud du générateur)
        var label = MakeLabel("Etat", gen, sw.position + new Vector3(0f, 0.9f, 0.12f), Quaternion.LookRotation(Vector3.back), 0.01f, new Color(1f, 0.85f, 0.4f));
        label.text = "HORS SERVICE";
        var lamp = GameObject.Find("Light_GeneratorBulb");
        var g2 = gen.gameObject.AddComponent<Generator>();
        g2.lever = genLever; g2.buttonRenderer = genLever.handle.GetComponent<Renderer>(); g2.label = label; g2.lamp = lamp ? lamp.GetComponent<Light>() : null;
        g2.roomLights = generatorLights.ToArray(); g2.roomLightLevels = generatorLightLevels.ToArray();
        if (genNode) GameObjectUtility.SetStaticEditorFlags(genNode.gameObject, StaticEditorFlags.BatchingStatic);
    }

    // Pupitre de surveillance du fichier : les 4 écrans (CCTV_Screen_01..04) affichent les 4 caméras ; sélectionner un écran = plein écran.
    static void BuildMonitors(GameObject rig, Camera head, Camera[] feeds)
    {
        var stationGo = new GameObject("MonitorStation");
        stationGo.transform.SetParent(bunkerRoot, false);
        var station = stationGo.AddComponent<MonitorStation>();
        station.feeds = new Material[feeds.Length];
        var rr = stationGo.AddComponent<CctvRoundRobin>();
        rr.cameras = feeds;
        rr.screens = new Renderer[feeds.Length];

        for (int i = 0; i < feeds.Length; i++)
        {
            var rt = new RenderTexture(320, 180, 16) { name = $"Feed_{i}" };
            AssetDatabase.CreateAsset(rt, $"{Dir}/Feed_{i}.renderTexture");
            feeds[i].targetTexture = rt;
            feeds[i].clearFlags = CameraClearFlags.SolidColor;
            feeds[i].backgroundColor = Color.black;
            feeds[i].depth = -10;

            var sm = Mat($"Screen_{i}", Color.white, 0f, "SAE501/CCTV");          // [7.2] effet caméra de surveillance (vert, grain, lignes)
            sm.SetFloat("_Gain", 5f);                                             // vision nocturne : la carte est très sombre
            sm.SetTexture("_BaseMap", rt);
            station.feeds[i] = sm;

            // écran du fichier dont le nom finit comme la caméra (ex. CCTV_Camera_01_Corridor_Left -> CCTV_Screen_01_Corridor_Left)
            var suffix = feeds[i].name.Replace("CCTV_Camera_", "");
            var scr = Find(roomRoot.transform, "CCTV_Screen_" + suffix);
            if (!scr && feeds[i].name == SouthCamName) scr = BuildExtraScreen(stationGo.transform, "CCTV_Screen_" + suffix);   // [#103] pas d'écran dans le fichier
            if (!scr) { Debug.LogWarning("Écran introuvable pour " + feeds[i].name); continue; }
            scr.GetComponent<Renderer>().sharedMaterial = sm;
            rr.screens[i] = scr.GetComponent<Renderer>();
            var col = scr.GetComponent<Collider>();
            if (col) Object.DestroyImmediate(col);
            var bc = scr.gameObject.AddComponent<BoxCollider>();
            bc.size = Vector3.Max(bc.size, new Vector3(0.02f, 0.02f, 0.02f)) + new Vector3(0.02f, 0.02f, 0.02f);
            scr.gameObject.AddComponent<XRSimpleInteractable>();
            var ms = scr.gameObject.AddComponent<MonitorScreen>();
            ms.station = station;
            ms.index = i;
        }

        // overlay plein écran, accroché à la caméra du joueur
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = "MonitorOverlay";
        q.transform.SetParent(head.transform, false);
        q.transform.localPosition = new Vector3(0, 0, 0.5f);        // à portée de main pour le toucher et le fermer
        q.transform.localScale = new Vector3(1.07f, 0.6f, 1f);     // même taille apparente qu'avant (1,6 x 0,9 à 0,75 m)
        Object.DestroyImmediate(q.GetComponent<MeshCollider>());
        var obc = q.AddComponent<BoxCollider>();
        obc.size = new Vector3(1f, 1f, 0.05f);
        q.AddComponent<XRSimpleInteractable>();
        q.AddComponent<MonitorOverlayClose>().station = station;
        station.overlay = q.transform;
        station.overlayRenderer = q.GetComponent<Renderer>();
        q.SetActive(false);
        rr.overlay = q;
    }

    // [#103] 5e écran du pupitre (caméra extérieure sud) : moniteur posé sur le bureau de surveillance, à droite de l'écran 01,
    // même hauteur et même orientation que lui (le joueur assis sur le tabouret le voit avec les autres).
    static Transform BuildExtraScreen(Transform parent, string name)
    {
        var refScr = Find(roomRoot.transform, "CCTV_Screen_01_Corridor_Left");
        var face = Quaternion.LookRotation(Vector3.back);                                      // face visible du Quad vers +z (le tabouret)
        var pos = refScr ? refScr.GetComponent<Renderer>().bounds.center + Vector3.right * 0.52f : new Vector3(1.07f, 1.17f, 7.27f);
        var mon = new GameObject("CCTV_Monitor_05").transform;
        mon.SetParent(parent, false);
        mon.SetPositionAndRotation(pos, face);
        Box("Boitier", mon, new Vector3(0f, 0f, 0.035f), new Vector3(0.47f, 0.36f, 0.06f), darkM, false);
        Box("Pied", mon, new Vector3(0f, -(pos.y - 0.79f) / 2f - 0.09f, 0.05f), new Vector3(0.05f, pos.y - 0.79f - 0.18f, 0.05f), metalM, false);
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = name;
        q.transform.SetParent(mon, false);
        q.transform.localPosition = Vector3.zero;
        q.transform.localScale = new Vector3(0.42f, 0.2363f, 1f);                            // 16:9
        Object.DestroyImmediate(q.GetComponent<MeshCollider>());
        return q.transform;
    }

    // ---------- matériaux ----------
    static Material Mat(string name, Color c, float smooth = 0.1f, string shader = "Universal Render Pipeline/Lit")
    {
        var m = new Material(Shader.Find(shader));
        m.SetColor("_BaseColor", c);
        if (shader.EndsWith("Lit")) { m.SetFloat("_Smoothness", smooth); m.SetFloat("_Metallic", 0f); }
        AssetDatabase.CreateAsset(m, $"{Dir}/{name}.mat");
        return m;
    }

    // [#113] [#116] [#117] matériaux des effets (Fx, ZombieEyes), dans Resources pour être chargés en jeu et inclus dans le build :
    //   Fx_Particules : particules en transparence (impacts, éclats de bois) ; Fx_Lueur : additif, pour les yeux des zombies.
    static void EnsureFxMaterials()
    {
        const string texPath = "Assets/Resources/Fx_Point.png";
        if (!File.Exists(texPath))
        {
            const int n = 32;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f - 0.5f, n / 2f - 0.5f)) / (n / 2f);
                    float a = Mathf.Clamp01(1f - d); a *= a;
                    t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            File.WriteAllBytes(texPath, t.EncodeToPNG());
            Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(texPath);
            var imp = (TextureImporter)AssetImporter.GetAtPath(texPath);
            imp.alphaIsTransparency = true; imp.mipmapEnabled = false; imp.wrapMode = TextureWrapMode.Clamp;
            imp.SaveAndReimport();
        }
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        FxMat("Assets/Resources/Fx_Particules.mat", tex, false);
        FxMat("Assets/Resources/Fx_Lueur.mat", tex, true);
    }

    static void FxMat(string path, Texture2D tex, bool additive)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")); AssetDatabase.CreateAsset(m, path); }
        m.SetTexture("_BaseMap", tex);
        m.SetColor("_BaseColor", Color.white);
        m.SetFloat("_Surface", 1f);                         // transparent
        m.SetFloat("_Blend", additive ? 2f : 0f);           // 2 = additif, 0 = alpha
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_Cull", 0f);                            // visible des deux côtés
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(m);
    }

    static void MakeMaterials()
    {
        darkM = Mat("Dark", new Color(0.10f, 0.10f, 0.11f), 0.3f);
        metalM = Mat("Metal", new Color(0.45f, 0.46f, 0.48f), 0.4f);
        plankM = Mat("Plank", new Color(0.55f, 0.52f, 0.48f));
        pistolM = Mat("Pistol", new Color(0.12f, 0.12f, 0.13f), 0.5f);
        headM = Mat("HammerHead", new Color(0.35f, 0.36f, 0.38f), 0.5f);
        nailM = Mat("Nail", new Color(0.75f, 0.75f, 0.78f), 0.6f);
        lockerM = Mat("Locker", new Color(0.16f, 0.20f, 0.18f), 0.35f);
        batteryM = Mat("Battery", new Color(0.25f, 0.45f, 0.30f));
        magM = Mat("Magazine", new Color(0.22f, 0.22f, 0.24f), 0.4f);
        lightM = Mat("FlashlightBar", new Color(0.9f, 0.8f, 0.3f), 0.1f, "Universal Render Pipeline/Unlit");

        ghostM = Mat("Ghost", new Color(0.2f, 1f, 0.4f, 0.3f), 0f, "Universal Render Pipeline/Unlit");
        ghostM.SetFloat("_Surface", 1f);
        ghostM.SetFloat("_Blend", 0f);
        ghostM.SetFloat("_SrcBlend", 5f);
        ghostM.SetFloat("_DstBlend", 10f);
        ghostM.SetFloat("_ZWrite", 0f);
        ghostM.SetOverrideTag("RenderType", "Transparent");
        ghostM.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        ghostM.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(ghostM);
    }

    // ---------- helpers ----------
    static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material m, bool collider = true, Vector3? euler = null)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = size;
        if (euler.HasValue) g.transform.localEulerAngles = euler.Value;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (!collider) Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }

    static GameObject Prim(PrimitiveType t, string name, Transform parent, Vector3 pos, Vector3 scale, Material m, Vector3? euler = null)
    {
        var g = GameObject.CreatePrimitive(t);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        if (euler.HasValue) g.transform.localEulerAngles = euler.Value;
        g.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }

    static XRGrabInteractable MakeGrab(GameObject g, Transform attach = null)
    {
        // détection continue : les petits objets (lampe, piles, chargeur) lâchés de haut traversaient le sol et tombaient sans fin
        var rb = g.GetComponent<Rigidbody>();
        if (rb) rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        var grab = g.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        if (attach) grab.attachTransform = attach;
        g.AddComponent<IgnorePlayer>();
        g.AddComponent<GrabGuard>();          // portée limitée (pas de prise de loin), pas de vol par l'autre main ou un holster
        g.AddComponent<ItemSafety>();         // jamais sous le sol : remis sur le sol au lâcher, rattrapé s'il tombe de la carte
        return grab;
    }

    // ---------- salle importée ----------
    static Transform Find(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    static XRGrabInteractable hammerGrab;

    static void BuildHammer(Transform node)
    {
        var mesh = node.GetComponent<MeshFilter>().sharedMesh;
        var mats = node.GetComponent<Renderer>().sharedMaterials;
        var pos = node.position;
        Object.DestroyImmediate(node.gameObject);

        // l'axe le plus long du maillage devient l'axe Y du marteau ; le bout le plus large est la tête
        var v = mesh.vertices; var b = mesh.bounds;
        int ax = b.size.x >= b.size.y && b.size.x >= b.size.z ? 0 : (b.size.y >= b.size.z ? 1 : 2);
        float len = b.size[ax], lo = b.min[ax], hi = b.max[ax];
        Vector3 SliceCenter(bool high, out Vector3 ext)
        {
            Vector3 mn = Vector3.one * 1e9f, mx = -Vector3.one * 1e9f, sum = Vector3.zero; int n = 0;
            foreach (var p in v)
            {
                bool inSlice = high ? p[ax] > hi - 0.22f * len : p[ax] < lo + 0.22f * len;
                if (!inSlice) continue;
                mn = Vector3.Min(mn, p); mx = Vector3.Max(mx, p); sum += p; n++;
            }
            ext = mx - mn;
            return sum / Mathf.Max(1, n);
        }
        var cHi = SliceCenter(true, out var eHi); var cLo = SliceCenter(false, out var eLo);
        float crossHi = eHi.x * eHi.y * eHi.z / Mathf.Max(1e-6f, eHi[ax]), crossLo = eLo.x * eLo.y * eLo.z / Mathf.Max(1e-6f, eLo[ax]);
        bool headHigh = crossHi >= crossLo;
        Vector3 headC = headHigh ? cHi : cLo, headExt = headHigh ? eHi : eLo;
        Vector3 axisDir = Vector3.zero; axisDir[ax] = headHigh ? 1f : -1f;
        var q = Quaternion.FromToRotation(axisDir, Vector3.up);

        var h = new GameObject("Hammer");
        h.transform.SetParent(items, false);
        h.transform.position = pos + new Vector3(0, 0.08f, 0);
        var vis = new GameObject("Visual");
        vis.transform.SetParent(h.transform, false);
        vis.transform.localRotation = q;
        vis.transform.localPosition = -(q * b.center);
        vis.AddComponent<MeshFilter>().sharedMesh = mesh;
        vis.AddComponent<MeshRenderer>().sharedMaterials = mats;

        var hc = q * headC + vis.transform.localPosition;
        var hs = new Vector3(Mathf.Abs((q * headExt).x), Mathf.Abs((q * headExt).y), Mathf.Abs((q * headExt).z));
        var head = new GameObject("Head");
        head.transform.SetParent(h.transform, false);
        head.transform.localPosition = hc;
        var hbc = head.AddComponent<BoxCollider>();
        hbc.size = hs + Vector3.one * 0.02f;
        head.AddComponent<HammerHead>();

        var handle = new GameObject("HandleCollider");
        handle.transform.SetParent(h.transform, false);
        var hb = handle.AddComponent<BoxCollider>();
        hb.size = new Vector3(0.03f, len, 0.03f);

        var attach = new GameObject("Attach").transform;
        attach.SetParent(h.transform, false);
        attach.localPosition = new Vector3(0, -len * 0.18f, 0);

        h.AddComponent<Rigidbody>();
        hammerGrab = MakeGrab(h, attach);
        h.AddComponent<BeltItem>().type = BeltItemType.Hammer;
        h.AddComponent<Hammer>();
    }

    // Arme de corps à corps à partir d'un objet de la carte (pied de biche, hache) : même analyse que le marteau,
    // l'axe le plus long devient Y, le bout le plus large (crochet, fer de la hache) est l'extrémité qui frappe.
    static void BuildMelee(Transform node, string name, float headDmg, float torsoDmg, float legsDmg)
    {
        if (node == null) { Debug.LogWarning("Arme introuvable dans la carte : " + name); return; }
        var mesh = node.GetComponent<MeshFilter>().sharedMesh;
        var mats = node.GetComponent<Renderer>().sharedMaterials;
        var rot = node.rotation;
        var pos = node.TransformPoint(mesh.bounds.center);       // centre du maillage dans la carte
        Object.DestroyImmediate(node.gameObject);

        var v = mesh.vertices; var b = mesh.bounds;
        int ax = b.size.x >= b.size.y && b.size.x >= b.size.z ? 0 : (b.size.y >= b.size.z ? 1 : 2);
        float len = b.size[ax], lo = b.min[ax], hi = b.max[ax];
        Vector3 Slice(bool high, out Vector3 ext)
        {
            Vector3 mn = Vector3.one * 1e9f, mx = -Vector3.one * 1e9f, sum = Vector3.zero; int n = 0;
            foreach (var p in v)
            {
                if (high ? p[ax] <= hi - 0.25f * len : p[ax] >= lo + 0.25f * len) continue;
                mn = Vector3.Min(mn, p); mx = Vector3.Max(mx, p); sum += p; n++;
            }
            ext = mx - mn;
            return sum / Mathf.Max(1, n);
        }
        var cHi = Slice(true, out var eHi); var cLo = Slice(false, out var eLo);
        float crossHi = eHi.x * eHi.y * eHi.z / Mathf.Max(1e-6f, eHi[ax]), crossLo = eLo.x * eLo.y * eLo.z / Mathf.Max(1e-6f, eLo[ax]);
        bool headHigh = crossHi >= crossLo;
        Vector3 headC = headHigh ? cHi : cLo, headExt = headHigh ? eHi : eLo;
        Vector3 axisDir = Vector3.zero; axisDir[ax] = headHigh ? 1f : -1f;
        var q = Quaternion.FromToRotation(axisDir, Vector3.up);

        var w = new GameObject(name);
        w.transform.SetParent(items, false);
        // posée comme dans la carte (le maillage garde son orientation), un peu au-dessus de la surface
        w.transform.SetPositionAndRotation(pos + Vector3.up * 0.03f, rot * Quaternion.Inverse(q));
        var vis = new GameObject("Visual");
        vis.transform.SetParent(w.transform, false);
        vis.transform.localRotation = q;
        vis.transform.localPosition = -(q * b.center);
        vis.AddComponent<MeshFilter>().sharedMesh = mesh;
        vis.AddComponent<MeshRenderer>().sharedMaterials = mats;

        var hc = q * headC + vis.transform.localPosition;
        var qe = q * headExt;
        var hs = new Vector3(Mathf.Abs(qe.x), Mathf.Abs(qe.y), Mathf.Abs(qe.z));
        var striker = new GameObject("Extremite");
        striker.transform.SetParent(w.transform, false);
        striker.transform.localPosition = hc;
        striker.AddComponent<BoxCollider>().size = hs + Vector3.one * 0.01f;

        var handle = new GameObject("Manche");
        handle.transform.SetParent(w.transform, false);
        handle.AddComponent<BoxCollider>().size = new Vector3(0.035f, len, 0.035f);

        var attach = new GameObject("Attach").transform;
        attach.SetParent(w.transform, false);
        attach.localPosition = new Vector3(0f, -len * 0.3f, 0f);       // prise vers le bout du manche

        var rb = w.AddComponent<Rigidbody>();
        rb.mass = 1.2f;
        MakeGrab(w, attach);
        var mw = w.AddComponent<MeleeWeapon>();
        mw.striker = striker.transform;
        mw.strikerSize = hs + new Vector3(0.06f, 0.06f, 0.06f);         // zone de frappe un peu plus large que le fer
        mw.headDamage = headDmg; mw.torsoDamage = torsoDmg; mw.legsDamage = legsDmg;
    }

    // chargeurs pleins posés dans la carte (coordonnées Blender : x, y, hauteur de la surface)
    // chacun n'apparaît qu'avec une probabilité MagazineChance, tirée au sort à chaque partie (RandomSpawn) et ajustée par la difficulté.
    // Rééquilibrage : les chargeurs du bunker sont garantis (avant : 1 chance sur 4 de n'en trouver aucun au niveau 1).
    const float MagazineChance = 0.5f;
    static void BuildMagazines()
    {
        var spots = new (float x, float y, float z, float yaw, string where)[]
        {
            (-1.05f, -7.4f, 0.79f, 20f,  "bunker : pupitre des caméras, à gauche des écrans"),
            (-1.85f, -12.45f, 0.9f, 75f, "bunker : établi (table)"),
            (-18.4f, -2.1f, 0.9f, 10f,   "cuisine : plan de travail"),
            (-20.7f, -2.15f, 0.9f, 100f, "cuisine : plan de travail"),
            (-17.65f, -0.25f, 0.9f, 30f, "générateur : établi (à gauche du coffre à outils)"),
            (-16.35f, 0.0f, 0.9f, 120f,  "générateur : établi (à droite)"),
            (-22.6f, -0.1f, 0.6f, 60f,   "générateur : caisse"),
            (12.9f, 6.35f, 0.9f, 15f,    "stockage : établi"),
            (14.2f, 6.9f, 0.9f, 140f,    "stockage : établi"),
            (15.87f, 8.15f, 1.005f, 70f, "stockage : caisse sur la palette"),
            (13.4f, 4.55f, 0.565f, 35f,  "stockage : caisse près des casiers"),
            (11.85f, 7.45f, 0.465f, 95f, "stockage : caisse militaire"),
        };
        foreach (var s in spots)
        {
            var g = MakeMagazine(B(s.x, s.y, s.z + 0.015f));
            g.transform.rotation = Quaternion.Euler(0f, s.yaw, 90f);   // couché sur le flanc
            if (s.where.StartsWith("bunker")) continue;               // garanti
            var rs = g.gameObject.AddComponent<RandomSpawn>();
            rs.chance = MagazineChance; rs.scaledByDifficulty = true;
        }
    }

    static GameObject MakePlank(Vector3 pos, Quaternion rot, Mesh mesh, Material[] mats)
    {
        var p = new GameObject("Plank");
        p.transform.SetParent(items, false);
        p.transform.SetPositionAndRotation(pos, rot);
        var bc = p.AddComponent<BoxCollider>();
        bc.size = new Vector3(PlankLength, PlankWidth, 0.03f);
        // le maillage du fichier est à plat (fin sur Y) : on le tourne pour que la face soit sur +Z
        var vis = new GameObject("Visual");
        vis.transform.SetParent(p.transform, false);
        vis.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // étirée à la taille des planches de jeu (comme celles clouées sur les fenêtres du bunker), centrée
        var mb = mesh.bounds;
        var sc0 = new Vector3(PlankLength / Mathf.Max(0.01f, mb.size.x), 1f, PlankWidth / Mathf.Max(0.01f, mb.size.z));
        vis.transform.localScale = sc0;
        vis.transform.localPosition = -(vis.transform.localRotation * Vector3.Scale(mb.center, sc0));
        vis.AddComponent<MeshFilter>().sharedMesh = mesh;
        vis.AddComponent<MeshRenderer>().sharedMaterials = mats;
        var rb = p.AddComponent<Rigidbody>();
        rb.mass = 2f;
        var plankGrab = MakeGrab(p);
        plankGrab.useDynamicAttach = true;      // prise là où la main la touche (pas de saut au centre de la planche)
        // la planche n'utilise que son propre collider (pas ceux des clous)
        plankGrab.colliders.Clear();
        plankGrab.colliders.Add(bc);

        // clous aux deux extrémités, face vers la pièce (+Z local)
        var nails = new Nail[2];
        float[] nx = { -(PlankLength / 2f - 0.1f), PlankLength / 2f - 0.1f };
        for (int i = 0; i < 2; i++)
        {
            var n = new GameObject($"Nail_{i}");
            n.transform.SetParent(p.transform, false);
            n.transform.localPosition = new Vector3(nx[i], 0, 0);
            var sc = n.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.05f;
            sc.center = new Vector3(0, 0, 0.03f);
            var nv = Prim(PrimitiveType.Cylinder, "Visual", n.transform, new Vector3(0, 0, 0.05f), new Vector3(0.016f, 0.04f, 0.016f), nailM, new Vector3(90, 0, 0));
            var nail = n.AddComponent<Nail>();
            nail.visual = nv;
            nail.col = sc;
            nv.SetActive(false);   // les clous n'apparaissent que marteau en main
            sc.enabled = false;
            nails[i] = nail;
        }


        var pl = p.AddComponent<Plank>();
        pl.nails = nails;
        return p;
    }

    static void MakeBattery(Vector3 pos)
    {
        var b = new GameObject("Battery");
        b.transform.SetParent(items, false);
        b.transform.position = pos;
        var bc = b.AddComponent<BoxCollider>();
        bc.size = new Vector3(0.03f, 0.03f, 0.06f);
        Box("Visual", b.transform, Vector3.zero, new Vector3(0.03f, 0.03f, 0.06f), batteryM, false);
        Box("Tip", b.transform, new Vector3(0, 0, 0.035f), new Vector3(0.012f, 0.012f, 0.01f), metalM, false);
        b.AddComponent<Rigidbody>();
        MakeGrab(b).useDynamicAttach = true;
        b.AddComponent<Battery>();
    }

    // UI diégétique de l'équipe : vignettage rouge selon les PV, écran noir + texte à la mort, puis relance de la scène
    static void BuildHealthUI(GameObject rig)
    {
        var head = rig.GetComponentInChildren<Camera>().transform;
        var ui = new GameObject("UI_Sante");
        ui.transform.SetParent(rig.transform, false);
        var hv = ui.AddComponent<HealthVignette>();
        hv.head = head;
        const string vp = "Assets/Generated/DamageVignette.mat";
        var vm = AssetDatabase.LoadAssetAtPath<Material>(vp);
        if (vm == null) { vm = new Material(Shader.Find("SAE501/DamageVignette")); AssetDatabase.CreateAsset(vm, vp); }
        hv.material = vm;

        var tg = new GameObject("TexteGameOver");
        tg.transform.SetParent(ui.transform, false);
        var tm = tg.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 80;
        tm.characterSize = 0.02f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.8f, 0.1f, 0.1f);
        tg.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        var go = ui.AddComponent<GameOverUI>();
        go.head = head;
        go.gameOverText = tm;
    }

    static TextMesh MakeLabel(string name, Transform parent, Vector3 pos, Quaternion rot, float charSize, Color c)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.position = pos; g.transform.rotation = rot;
        var tm = g.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64; tm.characterSize = charSize; tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.color = c;
        g.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        return tm;
    }

    // [#102] écran fixe des annonces de vague : mur sud du bunker, au-dessus du râtelier à planches, entre les deux fenêtres.
    // Face au point de départ du joueur ; les textes sont créés par WaveManager.
    static Transform BuildAnnouncementScreen()
    {
        var root = new GameObject("Ecran_Annonces").transform;
        root.SetParent(bunkerRoot, false);
        root.SetPositionAndRotation(WaveManager.DefaultScreenPosition, Quaternion.LookRotation(Vector3.forward));
        Box("Cadre", root, new Vector3(0f, 0f, 0.015f), new Vector3(2.04f, 0.79f, 0.03f), darkM, false);
        Box("Fond", root, new Vector3(0f, 0f, -0.002f), new Vector3(1.96f, 0.71f, 0.01f), Mat("Ecran_Annonces", new Color(0.015f, 0.04f, 0.03f), 0f, "Universal Render Pipeline/Unlit"), false);
        return root;
    }

    static void BuildZombieSetup(GameObject rig)
    {
        rig.AddComponent<PlayerHealth>();
        BuildHealthUI(rig);
        var sp = new GameObject("ZombieSpawner");
        sp.transform.SetParent(mapRoot, false);
        var z = sp.AddComponent<ZombieSpawner>();
        z.zombiePrefab = ZombieBuilder.EnsurePrefab();
        var pts = (GameObject)PrefabUtility.InstantiatePrefab(ZombieBuilder.BuildSpawnPoints(), mapRoot);
        z.points = pts.GetComponent<ZombieSpawnPoints>();
        z.spawnOnStart = false;               // c'est le WaveManager qui fait apparaître les zombies
        var wm = sp.AddComponent<WaveManager>();
        wm.spawner = z;
        wm.screen = BuildAnnouncementScreen();
        wm.startClip = SfxClip("Assets/Audio/Game/Lancement_Partie.mp3", AudioClipLoadType.CompressedInMemory, false);
    }

    // ---------- ceinture + kit ----------
    static BeltSocket MakeSocket(Transform belt, string name, Vector3 localPos, BeltItemType type, XRGrabInteractable start)
    {
        var g = new GameObject(name);
        g.transform.SetParent(belt, false);
        g.transform.localPosition = localPos;
        var sc = g.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.09f;
        var s = g.AddComponent<BeltSocket>();
        s.accepts = type;
        s.showInteractableHoverMeshes = false;
        s.startingSelectedInteractable = start;
        return s;
    }

    static void BuildBelt(GameObject rig, Camera head)
    {
        var belt = new GameObject("Belt");
        belt.transform.SetParent(bunkerRoot, false);
        belt.transform.SetPositionAndRotation(Spawn + new Vector3(0, 0.95f, 0), Quaternion.identity);
        var br = belt.AddComponent<BeltRig>();
        br.head = head.transform;
        br.rigRoot = rig.transform;

        Vector3 B = belt.transform.position;

        var pistol = MakePistol(B + new Vector3(0.22f, 0f, 0.02f));
        var light = MakeFlashlight(B + new Vector3(0.2f, 0f, -0.12f));
        var mag = MakeMagazine(B + new Vector3(-0.1f, 0f, 0.18f));

        MakeSocket(belt.transform, "Holster_Pistol", new Vector3(0.22f, 0f, 0.02f), BeltItemType.Pistol, pistol);
        MakeSocket(belt.transform, "Holster_Hammer", new Vector3(-0.24f, 0f, -0.08f), BeltItemType.Hammer, hammerGrab);
        MakeSocket(belt.transform, "Holster_Flashlight", new Vector3(0.2f, 0f, -0.12f), BeltItemType.Flashlight, light);
        MakeSocket(belt.transform, "Holster_Magazine", new Vector3(-0.1f, 0f, 0.18f), BeltItemType.Magazine, mag);
    }

    static Material SmokeMat()
    {
        const string path = "Assets/Generated/GunSmoke.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(m, path); }
        return m;
    }

    static XRGrabInteractable MakePistol(Vector3 pos)
    {
        var p = new GameObject("Pistol");
        p.transform.SetParent(items, false);
        p.transform.position = pos;

        // modèle M1911 du pack Modern Guns (sinon : boîtes grises)
        GunBuilder.Gun gun = GunBuilder.Available ? GunBuilder.InstantiateGun(p.transform) : null;
        Vector3 attachPos, magWellPos, hudPos, smokePos, muzzlePos;
        if (gun != null)
        {
            var bc = p.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, 0.035f, 0.07f); bc.size = new Vector3(0.03f, 0.13f, 0.23f);
            attachPos = new Vector3(0f, 0.02f, -0.015f);        // le creux de la poignée
            magWellPos = new Vector3(0f, -0.05f, -0.01f);       // sous la crosse
            hudPos = new Vector3(0f, 0.122f, 0.0f);             // dessus de la culasse, vers l'arrière
            smokePos = new Vector3(0f, 0.1f, -0.02f);           // arrière de la culasse
            muzzlePos = gun.barrel.localPosition;
        }
        else
        {
            Box("Slide", p.transform, new Vector3(0, 0.01f, 0.02f), new Vector3(0.03f, 0.035f, 0.17f), pistolM, true);
            Box("Grip", p.transform, new Vector3(0, -0.045f, -0.04f), new Vector3(0.028f, 0.09f, 0.04f), pistolM, true, new Vector3(-10, 0, 0));
            attachPos = new Vector3(0, -0.04f, -0.045f);
            magWellPos = new Vector3(0, -0.11f, -0.05f);
            hudPos = new Vector3(0, 0.03f, -0.03f);
            smokePos = new Vector3(0f, 0.02f, -0.07f);
            muzzlePos = new Vector3(0, 0.01f, 0.11f);
        }

        var attach = new GameObject("Attach").transform;
        attach.SetParent(p.transform, false);
        attach.localPosition = attachPos;

        p.AddComponent<Rigidbody>();
        var grab = MakeGrab(p, attach);
        p.AddComponent<BeltItem>().type = BeltItemType.Pistol;
        var pi = p.AddComponent<Pistol>();
        pi.smokeMaterial = SmokeMat();
        pi.shotClip = GunClip("Assets/Audio/Gun/Pistolet_Tir.mp3");
        pi.reloadClip = GunClip("Assets/Audio/Gun/Pistolet_Rechargement.mp3");
        // pi.dryFireClip : on garde le clic généré par code (ProcAudio.DryFire), ou on y met un fichier plus tard
        pi.smokeLocalPosition = smokePos;
        if (gun != null)
        {
            pi.muzzle = gun.barrel;
            pi.animator = gun.animator; pi.slider = gun.slider; pi.ejection = gun.ejection;
            pi.casingPrefab = GunBuilder.CasingPrefab();
            pi.muzzleFlashPrefab = GunBuilder.MuzzleFlashPrefab();
            gun.animator.GetComponent<GunAnimationEvents>().pistol = pi;
        }
        else
        {
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(p.transform, false);
            muzzle.localPosition = muzzlePos;
            pi.muzzle = muzzle;
        }

        var fl = new GameObject("Flash");
        fl.transform.SetParent(pi.muzzle, false);
        var l = fl.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.8f, 0.4f);
        l.range = 4f;
        l.intensity = 8f;
        l.shadows = LightShadows.None;
        l.enabled = false;   // allumé 0,04 s par tir
        pi.flash = l;

        var tg = new GameObject("AmmoHUD");
        tg.transform.SetParent(p.transform, false);
        tg.transform.localPosition = hudPos;
        tg.transform.localEulerAngles = new Vector3(90, 0, 0);
        var tm = tg.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64;
        tm.characterSize = 0.0035f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = new Color(1f, 0.85f, 0.3f);
        tg.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        pi.hud = tm;

        // emplacement du chargeur sous la crosse
        var well = new GameObject("MagWell");
        well.transform.SetParent(p.transform, false);
        well.transform.localPosition = magWellPos;
        var sc = well.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.05f;
        var mw = well.AddComponent<MagWell>();
        mw.pistol = pi;
        mw.showInteractableHoverMeshes = false;
        return grab;
    }

    // ---------- radio de ravitaillement (SupplyRadio) ----------
    // Une radio fixée au mur du bunker : bouton rouge = commander des chargeurs. Après une attente (selon la difficulté),
    // une caisse tombe par le trou du toit au-dessus de l'établi (Toit_Etabli, voir BuildBunkerOpenings) avec les chargeurs dedans.
    // La caisse et le chargeur sont des modèles cachés, copiés à chaque livraison.
    static readonly Vector3 SupplyDrop = new Vector3(-1.05f, 4.0f, 12.15f);     // au-dessus du trou du toit côté établi

    static void BuildSupplyRadio()
    {
        var radioM = Mat("Radio", new Color(0.22f, 0.26f, 0.16f), 0.25f);        // vert olive militaire
        var buttonM = Mat("Radio_Bouton", new Color(0.8f, 0.1f, 0.08f), 0.5f);
        var screenM = Mat("Radio_Ecran", new Color(0.01f, 0.02f, 0.015f), 0.8f);

        // place : premier mur touché par un rayon à hauteur de poitrine (murs est et ouest du bunker, loin des fenêtres)
        var tries = new[]
        {
            (B(1.5f, -8.0f, 1.25f), new Vector3(-1f, 0f, 0f)),
            (B(-5.5f, -8.0f, 1.25f), new Vector3(1f, 0f, 0f)),
            (B(1.5f, -12.8f, 1.25f), new Vector3(-1f, 0f, 0f)),
            (B(-5.5f, -12.8f, 1.25f), new Vector3(1f, 0f, 0f)),
        };
        Vector3 pos = B(-1.5f, -12.2f, 1.0f), normal = Vector3.back;           // secours : au-dessus de l'établi
        Physics.SyncTransforms();
        foreach (var (o, d) in tries)
            if (Physics.Raycast(o, d, out var hit, 4f) && hit.collider.name.Contains("Wall") && hit.distance > 0.3f)
            {
                normal = hit.normal; normal.y = 0f; normal.Normalize();
                pos = hit.point;
                break;
            }

        var root = new GameObject("Radio_Ravitaillement").transform;
        root.SetParent(bunkerRoot, false);
        root.SetPositionAndRotation(pos, Quaternion.LookRotation(normal));       // +Z = vers la pièce
        Box("Etagere", root, new Vector3(0f, -0.12f, 0.12f), new Vector3(0.42f, 0.03f, 0.24f), metalM);
        Box("Corps", root, new Vector3(0f, 0f, 0.1f), new Vector3(0.36f, 0.2f, 0.14f), radioM);
        Box("Antenne", root, new Vector3(0.14f, 0.22f, 0.1f), new Vector3(0.012f, 0.25f, 0.012f), darkM, false);
        Box("Ecran", root, new Vector3(-0.06f, 0.03f, 0.172f), new Vector3(0.19f, 0.11f, 0.005f), screenM, false);
        var display = MakeLabel("Affichage", root, root.TransformPoint(new Vector3(-0.06f, 0.03f, 0.176f)), Quaternion.LookRotation(-normal), 0.0032f, new Color(1f, 0.75f, 0.27f));
        display.text = "RAVITAILLEMENT";
        var btn = Box("Bouton", root, new Vector3(0.11f, 0.02f, 0.185f), new Vector3(0.06f, 0.06f, 0.03f), buttonM);
        ((BoxCollider)btn.GetComponent<Collider>()).size = new Vector3(1.8f, 1.8f, 2.5f);   // zone à toucher plus grande que le bouton
        var inter = btn.AddComponent<XRSimpleInteractable>();
        var tag = MakeLabel("Etiquette", root, root.TransformPoint(new Vector3(0.11f, -0.045f, 0.176f)), Quaternion.LookRotation(-normal), 0.0022f, new Color(0.85f, 0.85f, 0.8f));
        tag.text = "APPEL";

        // modèle de caisse (bac ouvert, les chargeurs tombent dedans)
        var crate = new GameObject("Modele_Caisse");
        crate.transform.SetParent(root, false);
        Box("Fond", crate.transform, Vector3.zero, new Vector3(0.5f, 0.03f, 0.36f), radioM);
        Box("Bord_A", crate.transform, new Vector3(0f, 0.11f, 0.165f), new Vector3(0.5f, 0.22f, 0.03f), radioM);
        Box("Bord_B", crate.transform, new Vector3(0f, 0.11f, -0.165f), new Vector3(0.5f, 0.22f, 0.03f), radioM);
        Box("Bord_C", crate.transform, new Vector3(0.235f, 0.11f, 0f), new Vector3(0.03f, 0.22f, 0.33f), radioM);
        Box("Bord_D", crate.transform, new Vector3(-0.235f, 0.11f, 0f), new Vector3(0.03f, 0.22f, 0.33f), radioM);
        var crb = crate.AddComponent<Rigidbody>();
        crb.mass = 20f;
        crb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        crate.AddComponent<IgnorePlayer>();                                    // ne pousse pas le joueur s'il est dessous
        crate.AddComponent<SupplyCrate>();
        crate.SetActive(false);

        // modèle de chargeur
        var mag = MakeMagazine(root.position).gameObject;
        mag.name = "Modele_Chargeur";
        mag.transform.SetParent(root, true);
        mag.SetActive(false);

        var drop = new GameObject("Point_Largage").transform;
        drop.SetParent(root, true);
        drop.position = SupplyDrop;

        var radio = root.gameObject.AddComponent<SupplyRadio>();
        radio.button = inter; radio.buttonRenderer = btn.GetComponent<Renderer>(); radio.display = display;
        radio.dropFrom = drop; radio.crateTemplate = crate; radio.magazineTemplate = mag;
    }

    static XRGrabInteractable MakeMagazine(Vector3 pos)
    {
        var m = new GameObject("Magazine");
        m.transform.SetParent(items, false);
        m.transform.position = pos;
        var bc = m.AddComponent<BoxCollider>();
        Transform attach = null;
        if (GunBuilder.Available)
        {
            // chargeur du pack avec ses 2 cartouches ; son origine est au bas du chargeur : on centre la prise et le collider
            GunBuilder.InstantiateMagazine(m.transform);
            bc.center = new Vector3(0f, 0.05f, 0.005f); bc.size = new Vector3(0.022f, 0.105f, 0.05f);
            attach = new GameObject("Attach").transform;
            attach.SetParent(m.transform, false);
            attach.localPosition = new Vector3(0f, 0.05f, 0.005f);
        }
        else
        {
            bc.size = new Vector3(0.03f, 0.08f, 0.05f);
            Box("Visual", m.transform, Vector3.zero, new Vector3(0.03f, 0.08f, 0.05f), magM, false);
        }
        m.AddComponent<Rigidbody>();
        var grab = MakeGrab(m, attach);
        m.AddComponent<BeltItem>().type = BeltItemType.Magazine;
        m.AddComponent<Magazine>().rounds = 12;
        return grab;
    }

    static XRGrabInteractable MakeFlashlight(Vector3 pos)
    {
        var f = new GameObject("Flashlight");
        f.transform.SetParent(items, false);
        f.transform.position = pos;
        var body = Prim(PrimitiveType.Cylinder, "Body", f.transform, Vector3.zero, new Vector3(0.04f, 0.08f, 0.04f), darkM, new Vector3(90, 0, 0));
        Prim(PrimitiveType.Cylinder, "Head", f.transform, new Vector3(0, 0, 0.09f), new Vector3(0.06f, 0.02f, 0.06f), metalM, new Vector3(90, 0, 0));
        var bc = f.AddComponent<BoxCollider>();
        bc.size = new Vector3(0.05f, 0.05f, 0.2f);
        bc.center = new Vector3(0, 0, 0.02f);

        var bar = Box("BatteryBar", f.transform, new Vector3(0, 0.026f, 0), new Vector3(0.01f, 0.004f, 0.1f), lightM, false);

        var port = new GameObject("BatteryPort");
        port.transform.SetParent(f.transform, false);
        port.transform.localPosition = new Vector3(0, 0, -0.1f);
        var sc = port.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.07f;

        var lg = new GameObject("Spot");
        lg.transform.SetParent(f.transform, false);
        lg.transform.localPosition = new Vector3(0, 0, 0.1f);
        var l = lg.AddComponent<Light>();
        l.type = LightType.Spot;
        l.range = 15f;
        l.spotAngle = 50f;
        l.intensity = 12f;
        l.color = new Color(1f, 0.97f, 0.88f);
        l.shadows = LightShadows.None;
        l.innerSpotAngle = 10f;                         // bord du faisceau très progressif (avant : 22°, transition marquée)
        l.renderMode = LightRenderMode.ForcePixel;      // toujours calculée par pixel
        l.enabled = false;

        // prise fixe : le corps de la lampe dans la paume, le faisceau dans l'axe de la main
        var fAttach = new GameObject("Attach").transform;
        fAttach.SetParent(f.transform, false);
        fAttach.localPosition = new Vector3(0f, 0f, -0.02f);

        f.AddComponent<Rigidbody>();
        var grab = MakeGrab(f, fAttach);
        f.AddComponent<BeltItem>().type = BeltItemType.Flashlight;
        var fl = f.AddComponent<Flashlight>();
        fl.spot = l;
        fl.bar = bar.transform;
        return grab;
    }
}
