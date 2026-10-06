using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Construit la salle "bunker" low poly en boxes grises + tout le gameplay (écrans, planches, ceinture).
public static class RoomBuilder
{
    const string Dir = "Assets/Generated";

    static Material darkM, metalM, plankM, ghostM, pistolM, headM, nailM, batteryM, magM, lightM;
    static Transform env, items;

    // Position de départ du joueur (dans la salle importée de Blender, repère Unity)
    static readonly Vector3 Spawn = new Vector3(0.8f, 0f, 1.35f);
    static GameObject roomRoot;

    public static void Populate(GameObject rig)
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets", "Generated");
        MakeMaterials();

        env = new GameObject("Environment").transform;
        items = new GameObject("Items").transform;

        // ambiance sombre
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.02f, 0.02f, 0.025f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.025f);
        RenderSettings.fogDensity = 0.03f;

        rig.transform.position = Spawn;
        var cam = rig.GetComponentInChildren<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(RoomImport.FbxPath);
        if (fbx == null) { Debug.LogError("Salle introuvable : lance d'abord SAE501 > Salle > 1 - Importer la salle"); return; }
        roomRoot = Object.Instantiate(fbx);
        roomRoot.name = "Room (Blender)";
        roomRoot.transform.SetParent(env, false);

        var slots = CollectSlots(roomRoot.transform);
        var planks = CollectPlanks(roomRoot.transform);
        var hammerNode = Find(roomRoot.transform, "Hammer");
        var pendant = Find(roomRoot.transform, "CeilingPendant");

        BuildLights(pendant);
        MakeRoomColliders();
        BuildSlots(slots);
        BuildPlanks(planks);
        BuildHammer(hammerNode);
        var feeds = BuildCameras();
        BuildMonitors(rig, cam, feeds);
        BuildBatteries();
        BuildBelt(rig, cam);

        new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
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

    struct Pose { public string name; public Vector3 pos; public Quaternion rot; public Mesh mesh; public Material[] mats; }

    static List<Pose> CollectSlots(Transform root)
    {
        var res = new List<Pose>();
        foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(x => x.name.StartsWith("PlankSlot_")).ToList())
        {
            res.Add(new Pose { name = t.name, pos = t.position, rot = t.rotation });
            Object.DestroyImmediate(t.gameObject);
        }
        return res;
    }

    static List<Pose> CollectPlanks(Transform root)
    {
        var res = new List<Pose>();
        foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(x => x.name.StartsWith("Plank_")).OrderBy(x => x.name).ToList())
        {
            var mf = t.GetComponent<MeshFilter>();
            res.Add(new Pose { name = t.name, pos = t.position, rot = t.rotation, mesh = mf.sharedMesh, mats = t.GetComponent<Renderer>().sharedMaterials });
            Object.DestroyImmediate(t.gameObject);
        }
        return res;
    }

    static readonly string[] NoCollider = { "Floor_Debris", "Floor_Papers", "Rug", "Cornice", "Wainscot", "Backsplash", "PictureFrame", "CrankRadio_Handle" };

    static void MakeRoomColliders()
    {
        foreach (var r in roomRoot.GetComponentsInChildren<MeshRenderer>(true))
        {
            GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.BatchingStatic);
            if (NoCollider.Any(n => r.name.StartsWith(n))) continue;
            r.gameObject.AddComponent<BoxCollider>();
        }
    }

    static void BuildLights(Transform pendant)
    {
        // lampe suspendue qui grésille (au centre, au-dessus de l'établi)
        var lg = new GameObject("LampLight");
        lg.transform.SetParent(env, false);
        lg.transform.position = new Vector3(0, 1.85f, 0);
        var l = lg.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.78f, 0.5f);
        l.range = 7f;
        l.intensity = 2.2f;
        l.shadows = LightShadows.None;

        var bulbMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Room/Materials/BulbOff.mat");
        bulbMat.EnableKeyword("_EMISSION");
        bulbMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        bulbMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.5f) * 2.5f);
        EditorUtility.SetDirty(bulbMat);

        var f = lg.AddComponent<FlickerLamp>();
        f.lamp = l;
        f.bulb = pendant.GetComponent<Renderer>();
        f.bulbMaterialIndex = 2;          // 3e matériau du pendentif = ampoule (BulbOff)
        f.baseIntensity = 2.2f;

        // lampe à huile sur l'établi
        var oil = Find(roomRoot.transform, "OilLamp");
        var og = new GameObject("OilLampLight");
        og.transform.SetParent(env, false);
        og.transform.position = oil.position + new Vector3(0, 0.18f, 0);
        var ol = og.AddComponent<Light>();
        ol.type = LightType.Point;
        ol.color = new Color(1f, 0.52f, 0.18f);
        ol.range = 3.5f;
        ol.intensity = 0.8f;
        ol.shadows = LightShadows.None;
        var of = og.AddComponent<FlickerLamp>();
        of.lamp = ol;
        of.baseIntensity = 0.8f;
        of.burstMinGap = 4f;
        of.burstMaxGap = 12f;

        // clair de lune par les deux fenêtres (est : x = -3, ouest : x = +3)
        foreach (float sx in new[] { -1f, 1f })
        {
            var mg = new GameObject(sx < 0 ? "Moon_East" : "Moon_West");
            mg.transform.SetParent(env, false);
            mg.transform.position = new Vector3(4.9f * sx, 2.5f, sx < 0 ? -0.3f : 0f);
            mg.transform.LookAt(new Vector3(2.0f * sx, 0.6f, sx < 0 ? -0.3f : 0f));
            var ml = mg.AddComponent<Light>();
            ml.type = LightType.Spot;
            ml.color = new Color(0.42f, 0.56f, 1f);
            ml.spotAngle = 70f;
            ml.range = 9f;
            ml.intensity = 9f;
            ml.shadows = LightShadows.None;
        }
    }

    static void BuildSlots(List<Pose> slots)
    {
        // 4 ouvertures x 3 emplacements : position et orientation viennent du fichier Blender
        var holder = new GameObject("BarricadeSlots").transform;
        holder.SetParent(env, false);
        foreach (var p in slots)
        {
            var s = new GameObject(p.name.Replace("PlankSlot_", "Slot_"));
            s.transform.SetParent(holder, false);
            s.transform.SetPositionAndRotation(p.pos, p.rot);
            var ghost = Box("Ghost", s.transform, Vector3.zero, new Vector3(1.24f, 0.18f, 0.04f), ghostM, false);
            var gr = ghost.GetComponent<Renderer>();
            gr.enabled = false;   // visible uniquement quand une planche est en main
            s.AddComponent<BarricadeSlot>().ghost = gr;
        }
    }

    static void BuildPlanks(List<Pose> planks)
    {
        // les 5 premières sont empilées sur l'établi (comme dans le fichier) ; les 3 autres sont posées au sol, près de la porte sud
        // Le maillage du fichier est à plat : le "dessus" de la planche (axe Z du composant) doit regarder vers le haut.
        var flat = Quaternion.Euler(-90, 0, 0);
        for (int i = 0; i < planks.Count; i++)
        {
            var p = planks[i];
            Vector3 pos; Quaternion rot;
            if (i < 5)
            {
                pos = new Vector3(p.pos.x, 0.916f + 0.032f * i, p.pos.z);
                rot = Quaternion.Euler(0, p.rot.eulerAngles.y, 0) * flat;
            }
            else
            {
                pos = new Vector3(-1.7f, 0.02f + 0.032f * (i - 5), 1.35f);
                rot = flat;
            }
            MakePlank(pos, rot, p.mesh, p.mats);
        }
    }

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
    static XRGrabInteractable hammerGrab;

    // 3 caméras de surveillance : couloir sud, et deux coins de la salle
    static Camera[] BuildCameras()
    {
        var cams = new Camera[3];
        string[] names = { "CCTV_Corridor", "CCTV_Room_SW", "CCTV_Room_NE" };
        Vector3[] pos = { new Vector3(-0.8f, 2.35f, 2.45f), new Vector3(2.8f, 2.5f, 2.3f), new Vector3(-2.8f, 2.5f, -2.3f) };
        Vector3[] look = { new Vector3(-0.8f, 1.0f, 4.6f), new Vector3(-1.0f, 1.0f, -1.0f), new Vector3(1.0f, 1.0f, 1.0f) };
        for (int i = 0; i < 3; i++)
        {
            var g = new GameObject(names[i]);
            g.transform.SetParent(env, false);
            g.transform.position = pos[i];
            g.transform.LookAt(look[i]);
            cams[i] = g.AddComponent<Camera>();
        }
        // un peu de lumière froide dans le couloir pour que la caméra voie quelque chose
        var lg = new GameObject("CorridorLight");
        lg.transform.SetParent(env, false);
        lg.transform.position = new Vector3(-0.8f, 2.2f, 3.8f);
        var l = lg.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.5f, 0.6f, 1f);
        l.range = 3f;
        l.intensity = 2f;
        l.shadows = LightShadows.None;
        return cams;
    }

    // ---------- pupitre + écrans ----------
    static void BuildMonitors(GameObject rig, Camera head, Camera[] feeds)
    {
        var tableMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Room/Materials/TableWood.mat") ?? metalM;
        var table = new GameObject("MonitorDesk").transform;
        table.SetParent(env, false);
        Box("Top", table, new Vector3(1.3f, 0.72f, 2.15f), new Vector3(1.8f, 0.06f, 0.6f), tableMat);
        foreach (float lx in new[] { 0.45f, 2.15f })
            foreach (float lz in new[] { 1.9f, 2.4f })
                Box("Leg", table, new Vector3(lx, 0.35f, lz), new Vector3(0.06f, 0.7f, 0.06f), darkM);

        var stationGo = new GameObject("MonitorStation");
        var station = stationGo.AddComponent<MonitorStation>();
        station.feeds = new Material[feeds.Length];

        float[] xs = { 0.78f, 1.3f, 1.82f };
        for (int i = 0; i < feeds.Length; i++)
        {
            var rt = new RenderTexture(320, 180, 16) { name = $"Feed_{i}" };
            AssetDatabase.CreateAsset(rt, $"{Dir}/Feed_{i}.renderTexture");
            feeds[i].targetTexture = rt;
            feeds[i].fieldOfView = 70f;
            feeds[i].clearFlags = CameraClearFlags.SolidColor;
            feeds[i].backgroundColor = Color.black;
            feeds[i].depth = -10;

            var sm = Mat($"Screen_{i}", Color.white, 0f, "Universal Render Pipeline/Unlit");
            sm.SetTexture("_BaseMap", rt);
            station.feeds[i] = sm;

            var m = new GameObject($"Monitor_{i}");
            m.transform.SetParent(stationGo.transform, false);
            m.transform.position = new Vector3(xs[i], 0.98f, 2.22f);
            // écrans tournés vers le joueur (placé au nord du pupitre) et un peu inclinés
            m.transform.rotation = Quaternion.Euler(10f, -15f * (xs[i] - 1.3f) / 0.52f, 0f);
            var col = m.AddComponent<BoxCollider>();
            col.size = new Vector3(0.5f, 0.32f, 0.08f);
            Box("Body", m.transform, Vector3.zero, new Vector3(0.5f, 0.32f, 0.04f), darkM, false);
            Box("Stand", m.transform, new Vector3(0, -0.2f, 0.02f), new Vector3(0.06f, 0.1f, 0.04f), darkM, false);
            var scr = GameObject.CreatePrimitive(PrimitiveType.Quad);
            scr.name = "Screen";
            scr.transform.SetParent(m.transform, false);
            scr.transform.localPosition = new Vector3(0, 0, -0.0205f);
            scr.transform.localScale = new Vector3(0.46f, 0.28f, 1f);
            Object.DestroyImmediate(scr.GetComponent<Collider>());
            scr.GetComponent<Renderer>().sharedMaterial = sm;

            m.AddComponent<XRSimpleInteractable>();
            var ms = m.AddComponent<MonitorScreen>();
            ms.station = station;
            ms.index = i;
        }

        // overlay plein écran, accroché à la caméra du joueur
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = "MonitorOverlay";
        q.transform.SetParent(head.transform, false);
        q.transform.localPosition = new Vector3(0, 0, 0.75f);
        q.transform.localScale = new Vector3(1.6f, 0.9f, 1f);
        Object.DestroyImmediate(q.GetComponent<MeshCollider>());
        var bc = q.AddComponent<BoxCollider>();
        bc.size = new Vector3(1f, 1f, 0.05f);
        q.AddComponent<XRSimpleInteractable>();
        q.AddComponent<MonitorOverlayClose>().station = station;
        station.overlay = q.transform;
        station.overlayRenderer = q.GetComponent<Renderer>();
        q.SetActive(false);
    }

    static void BuildBatteries()
    {
        MakeBattery(new Vector3(2.05f, 0.77f, 2.15f));    // sur le pupitre
        MakeBattery(new Vector3(0.0f, 0.92f, 0.15f));     // sur l'établi
        MakeBattery(new Vector3(-2.7f, 0.92f, 0.8f));     // sur le plan de travail
        MakeBattery(new Vector3(-2.2f, 0.04f, -2.2f));    // au sol, dans un coin
    }

    static GameObject MakePlank(Vector3 pos, Quaternion rot, Mesh mesh, Material[] mats)
    {
        var p = new GameObject("Plank");
        p.transform.SetParent(items, false);
        p.transform.SetPositionAndRotation(pos, rot);
        var bc = p.AddComponent<BoxCollider>();
        bc.size = new Vector3(1.2f, 0.15f, 0.03f);
        // le maillage du fichier est à plat (fin sur Y) : on le tourne pour que la face soit sur +Z
        var vis = new GameObject("Visual");
        vis.transform.SetParent(p.transform, false);
        vis.transform.localRotation = Quaternion.Euler(90, 0, 0);
        vis.AddComponent<MeshFilter>().sharedMesh = mesh;
        vis.AddComponent<MeshRenderer>().sharedMaterials = mats;
        var rb = p.AddComponent<Rigidbody>();
        rb.mass = 2f;
        MakeGrab(p);

        // clous aux deux extrémités, face vers la pièce (+Z local)
        var nails = new Nail[2];
        float[] nx = { -0.52f, 0.52f };
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

        // zone pour tenir la planche avec l'autre main
        var hz = new GameObject("HoldZone");
        hz.transform.SetParent(p.transform, false);
        var hcol = hz.AddComponent<BoxCollider>();
        hcol.size = new Vector3(1.3f, 0.26f, 0.16f);
        var hold = hz.AddComponent<XRSimpleInteractable>();

        var pl = p.AddComponent<Plank>();
        pl.nails = nails;
        pl.holdZone = hold;
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
        MakeGrab(b);
        b.AddComponent<Battery>();
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

    static XRGrabInteractable MakePistol(Vector3 pos)
    {
        var p = new GameObject("Pistol");
        p.transform.SetParent(items, false);
        p.transform.position = pos;
        Box("Slide", p.transform, new Vector3(0, 0.01f, 0.02f), new Vector3(0.03f, 0.035f, 0.17f), pistolM, true);
        Box("Grip", p.transform, new Vector3(0, -0.045f, -0.04f), new Vector3(0.028f, 0.09f, 0.04f), pistolM, true, new Vector3(-10, 0, 0));

        var attach = new GameObject("Attach").transform;
        attach.SetParent(p.transform, false);
        attach.localPosition = new Vector3(0, -0.04f, -0.045f);

        p.AddComponent<Rigidbody>();
        var grab = MakeGrab(p, attach);
        p.AddComponent<BeltItem>().type = BeltItemType.Pistol;
        var pi = p.AddComponent<Pistol>();

        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(p.transform, false);
        muzzle.localPosition = new Vector3(0, 0.01f, 0.11f);
        pi.muzzle = muzzle;

        var fl = new GameObject("Flash");
        fl.transform.SetParent(muzzle, false);
        var l = fl.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.8f, 0.4f);
        l.range = 4f;
        l.intensity = 8f;
        l.shadows = LightShadows.None;
        pi.flash = l;

        var tg = new GameObject("AmmoHUD");
        tg.transform.SetParent(p.transform, false);
        tg.transform.localPosition = new Vector3(0, 0.03f, -0.03f);
        tg.transform.localEulerAngles = new Vector3(90, 0, 0);
        var tm = tg.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64;
        tm.characterSize = 0.004f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = new Color(1f, 0.85f, 0.3f);
        tg.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        pi.hud = tm;

        // emplacement du chargeur sous la crosse
        var well = new GameObject("MagWell");
        well.transform.SetParent(p.transform, false);
        well.transform.localPosition = new Vector3(0, -0.11f, -0.05f);
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
        bc.size = new Vector3(0.03f, 0.08f, 0.05f);
        Box("Visual", m.transform, Vector3.zero, new Vector3(0.03f, 0.08f, 0.05f), magM, false);
        m.AddComponent<Rigidbody>();
        var grab = MakeGrab(m);
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

        f.AddComponent<Rigidbody>();
        var grab = MakeGrab(f);
        f.AddComponent<BeltItem>().type = BeltItemType.Flashlight;
        var fl = f.AddComponent<Flashlight>();
        fl.spot = l;
        fl.bar = bar.transform;
        return grab;
    }
}
