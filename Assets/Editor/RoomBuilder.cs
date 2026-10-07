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
    [System.Serializable] class MapMeta { public LightInfo[] lights; public CamInfo[] cameras; }
    static MapMeta meta;

    public static void Populate(GameObject rig)
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets", "Generated");
        MakeMaterials();
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
        RenderSettings.fogColor = new Color(0.003f, 0.003f, 0.004f);
        RenderSettings.fogDensity = 0.12f;

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

        BuildMapLights();
        MakeMapColliders(roomRoot.transform);
        BuildExterior();
        BuildPlanks(planks);
        BuildHammer(hammerNode);
        BuildMonitors(rig, cam, BuildCameras());
        BuildBatteries();
        BuildBelt(rig, cam);
        BuildBunkerOpenings(slots);
        BuildGatesAndGenerator();
        BuildExteriorDecor(cam);
        BuildZombieSetup(rig);
        BuildMusic();

        new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
        NavBuilder.Bake(mapRoot, rig, items.gameObject, GameObject.Find("MonitorStation"), GameObject.Find("Belt"), gatesRoot);
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
        "WindowSash", "WindowGlass", "Chair_Overturned", "Brick", "Apoc_", "Puddle", "OilStain", "Cables", "CableConduits", "Ceiling", "Pendant", "Sconce",
        "CCTV_Monitor", "CCTV_Bezel", "LEDs", "TallyLED", "CameraMesh", "Mount", "Bulb", "Door_Boarded_Planks" };
    // structure : collider exact (murs percés de portes et de fenêtres, sols en L)
    static readonly string[] Structural = { "Wall", "Floor", "Trim", "Stub" };

    static void MakeMapColliders(Transform root)
    {
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.BatchingStatic);
            var n = r.name;
            if (NoCollider.Any(k => n.Contains(k))) continue;
            if (Structural.Any(k => n.Contains(k))) r.gameObject.AddComponent<MeshCollider>().sharedMesh = r.GetComponent<MeshFilter>().sharedMesh;
            else r.gameObject.AddComponent<BoxCollider>();
        }
    }

    // ---------- lumières du fichier ----------
    static void BuildMapLights()
    {
        var root = new GameObject("Lumieres").transform;
        root.SetParent(env, false);
        foreach (var li in meta.lights)
        {
            var g = new GameObject(li.name);
            g.transform.SetParent(root, false);
            g.transform.position = B(li.loc);
            var l = g.AddComponent<Light>();
            l.color = new Color(li.color[0], li.color[1], li.color[2]);
            l.shadows = LightShadows.None;
            if (li.type == "SPOT")
            {
                l.type = LightType.Spot;
                g.transform.rotation = Quaternion.LookRotation(B(li.dir));
                l.spotAngle = Mathf.Clamp(li.spot_size * Mathf.Rad2Deg, 10f, 170f);
                l.innerSpotAngle = l.spotAngle * (1f - Mathf.Clamp01(li.spot_blend));
                l.intensity = li.energy / 275f;        // 220 W (lune) -> 0,8 comme l'ancienne salle
                l.range = 9f;
            }
            else
            {
                l.type = LightType.Point;
                l.intensity = li.energy / 24f;         // 18 W (lampe à huile) -> 0,75
                l.range = Mathf.Clamp(1.1f * Mathf.Sqrt(li.energy), 2.5f, 6.5f);
            }
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
            // générateur à l'arrêt : l'ampoule de sa salle est presque éteinte (le Generator la rallume)
            if (li.name == "Light_GeneratorBulb") l.intensity = 0.08f;
        }
    }

    // ---------- extérieur : bande de terrain au sud du bunker, d'où les zombies attaquent les deux fenêtres ----------
    static void BuildExterior()
    {
        var ext = new GameObject("Exterieur_Sud").transform;
        ext.SetParent(mapRoot, false);
        // bunker : x Blender -7,5..3,8 / mur sud y -14 ; le terrain va de y -14 à -19
        var c = B(-1.85f, -16.6f, -0.05f);
        Box("Sol", ext, c, new Vector3(13f, 0.1f, 5.2f), blockM);
    }

    // ---------- décor extérieur d'Alban (branche features_exterieur) : terrain en relief, montagnes, pins, ciel, lune ----------
    // Le prefab est extrait de sa scène ; les cadres, plaques et voyants des portes de niveau sont rattachés au battant
    // de chaque porte (ils bougent avec elle quand elle s'ouvre).
    const string ExteriorPrefab = "Assets/Environment/Exterieur_Alban.prefab";
    // Le ciel : la sphère d'Alban (240 m) disparaissait dans le brouillard, on utilise à la place son matériau skybox
    // (panorama de forêt de nuit), que le brouillard ne voile pas. Brouillard allégé (0,12 -> 0,03) pour voir le relief.
    static void BuildExteriorDecor(Camera cam)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExteriorPrefab);
        if (prefab == null) return;
        var ext = (GameObject)PrefabUtility.InstantiatePrefab(prefab, mapRoot);
        PrefabUtility.UnpackPrefabInstance(ext, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        var sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/Environment/Materials/M_NightForest_Skybox.mat");
        var sphere = ext.transform.Find("Environment_Skysphere");
        if (sky != null)
        {
            RenderSettings.skybox = sky;
            cam.clearFlags = CameraClearFlags.Skybox;
            if (sphere != null) Object.DestroyImmediate(sphere.gameObject);
        }
        RenderSettings.fogDensity = 0.03f;
        var portes = ext.transform.Find("Portes");
        if (portes != null && gatesRoot != null)
        {
            foreach (Transform deco in portes)
            {
                var leaf = gatesRoot.transform.Find(deco.name + "/Battant");
                if (leaf == null) continue;
                for (int i = deco.childCount - 1; i >= 0; i--) deco.GetChild(i).SetParent(leaf, true);
            }
            Object.DestroyImmediate(portes.gameObject);
        }
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
    }

    // window = vrai : le zombie passe par la fenêtre grâce à un lien NavMesh (il n'y a pas de sol continu sous l'allège)
    static void MakeOpening(Transform holder, string name, Vector3 center, Vector3 approach, Vector3 inside, bool window)
    {
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
        }
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
    static Camera[] BuildCameras()
    {
        var list = new List<Camera>();
        foreach (var c in meta.cameras.OrderBy(x => x.name))
        {
            var g = new GameObject(c.name);
            g.transform.SetParent(mapRoot, false);
            g.transform.SetPositionAndRotation(B(c.loc), Quaternion.LookRotation(B(c.dir), B(c.up)));
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
        void Gate(string name, string zone, int level, Vector3 pos, float width, Vector3 viewer)
        {
            var root = new GameObject("Porte_" + name).transform;
            root.SetParent(gatesRoot.transform, false);
            var leaf = Box("Battant", root, pos + Vector3.up * 1.05f, new Vector3(0.12f, 2.1f, width), metalM);
            var tm = MakeLabel("Panneau", root, pos + Vector3.up * 1.5f + viewer * 0.09f, Quaternion.LookRotation(-viewer), 0.012f, new Color(1f, 0.2f, 0.15f));
            var g = root.gameObject.AddComponent<ZoneGate>();
            g.unlockLevel = level; g.zoneName = zone; g.door = leaf.transform; g.doorCollider = leaf.GetComponent<Collider>(); g.sign = tm;
        }
        Gate("Couloir_Gauche", "Couloir gauche et cuisine", 2, B(-7.43f, -10.1f, 0f), 1.4f, Vector3.left);
        Gate("Couloir_Droit", "Couloir droit", 2, B(3.72f, -10.1f, 0f), 1.8f, Vector3.right);
        Gate("Stockage", "Stockage", 3, B(10.58f, 6.5f, 0f), 1.85f, Vector3.right);
        Gate("Generateur", "Générateur", 4, B(-14.58f, 2.5f, 0f), 1.25f, Vector3.left);

        // le générateur du fichier : on rend son interrupteur actionnable (le maintenir 5 s)
        var genNode = Find(roomRoot.transform, "Generator");
        var sw = Find(roomRoot.transform, "Generator_Switch");
        var gen = new GameObject("Generateur_Objectif").transform;
        gen.SetParent(mapRoot, false);
        gen.position = sw.position;
        var btnGo = new GameObject("Interrupteur");
        btnGo.transform.SetParent(gen, false);
        var bc = btnGo.AddComponent<BoxCollider>();
        bc.size = new Vector3(0.3f, 0.3f, 0.3f);              // zone de sélection plus grande que le petit levier
        var btn = btnGo.AddComponent<XRSimpleInteractable>();
        // panneau d'état au-dessus de l'interrupteur, lisible depuis l'entrée de la salle (côté sud du générateur)
        var label = MakeLabel("Etat", gen, sw.position + new Vector3(0f, 0.9f, 0.12f), Quaternion.LookRotation(Vector3.back), 0.01f, new Color(1f, 0.85f, 0.4f));
        label.text = "HORS SERVICE";
        var lamp = GameObject.Find("Light_GeneratorBulb");
        var g2 = gen.gameObject.AddComponent<Generator>();
        g2.button = btn; g2.buttonRenderer = sw.GetComponent<Renderer>(); g2.label = label; g2.lamp = lamp ? lamp.GetComponent<Light>() : null;
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
            sm.SetTexture("_BaseMap", rt);
            station.feeds[i] = sm;

            // écran du fichier dont le nom finit comme la caméra (ex. CCTV_Camera_01_Corridor_Left -> CCTV_Screen_01_Corridor_Left)
            var suffix = feeds[i].name.Replace("CCTV_Camera_", "");
            var scr = Find(roomRoot.transform, "CCTV_Screen_" + suffix);
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

    // ---------- matériaux ----------
    static Material Mat(string name, Color c, float smooth = 0.1f, string shader = "Universal Render Pipeline/Lit")
    {
        var m = new Material(Shader.Find(shader));
        m.SetColor("_BaseColor", c);
        if (shader.EndsWith("Lit")) { m.SetFloat("_Smoothness", smooth); m.SetFloat("_Metallic", 0f); }
        AssetDatabase.CreateAsset(m, $"{Dir}/{name}.mat");
        return m;
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
