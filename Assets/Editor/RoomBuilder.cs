using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Construit la salle "bunker" low poly en boxes grises + tout le gameplay (écrans, planches, ceinture).
public static class RoomBuilder
{
    const string Dir = "Assets/Generated";

    static Material wallM, floorM, ceilM, darkM, metalM, plankM, doorM, ghostM, bulbM, pistolM, headM, nailM, batteryM, magM, lightM;
    static Transform env, items;

    public static void Populate(GameObject rig)
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets", "Generated");
        MakeMaterials();

        env = new GameObject("Environment").transform;
        items = new GameObject("Items").transform;

        // ambiance sombre
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.035f, 0.035f, 0.04f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.025f);
        RenderSettings.fogDensity = 0.03f;

        var cam = rig.GetComponentInChildren<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        BuildRoom();
        BuildLamp();
        var corridorCams = BuildCorridors();
        BuildMonitors(rig, cam, corridorCams);
        BuildWorkbench();
        BuildDoors();
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
        wallM = Mat("Wall", new Color(0.30f, 0.30f, 0.32f));
        floorM = Mat("Floor", new Color(0.20f, 0.20f, 0.21f));
        ceilM = Mat("Ceiling", new Color(0.24f, 0.24f, 0.25f));
        darkM = Mat("Dark", new Color(0.10f, 0.10f, 0.11f), 0.3f);
        metalM = Mat("Metal", new Color(0.45f, 0.46f, 0.48f), 0.4f);
        plankM = Mat("Plank", new Color(0.55f, 0.52f, 0.48f));
        doorM = Mat("Door", new Color(0.18f, 0.18f, 0.20f), 0.2f);
        pistolM = Mat("Pistol", new Color(0.12f, 0.12f, 0.13f), 0.5f);
        headM = Mat("HammerHead", new Color(0.35f, 0.36f, 0.38f), 0.5f);
        nailM = Mat("Nail", new Color(0.75f, 0.75f, 0.78f), 0.6f);
        batteryM = Mat("Battery", new Color(0.25f, 0.45f, 0.30f));
        magM = Mat("Magazine", new Color(0.22f, 0.22f, 0.24f), 0.4f);
        lightM = Mat("FlashlightBar", new Color(0.9f, 0.8f, 0.3f), 0.1f, "Universal Render Pipeline/Unlit");

        bulbM = Mat("Bulb", new Color(1f, 0.85f, 0.6f));
        bulbM.EnableKeyword("_EMISSION");
        bulbM.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        bulbM.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.5f) * 2.5f);

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

    // ---------- salle ----------
    static void BuildRoom()
    {
        Box("Floor", env, new Vector3(0, -0.1f, 0), new Vector3(6.4f, 0.2f, 6.4f), floorM);
        Box("Ceiling", env, new Vector3(0, 3.1f, 0), new Vector3(6.4f, 0.2f, 6.4f), ceilM);
        Box("Wall_Back", env, new Vector3(0, 1.5f, -3.1f), new Vector3(6.4f, 3f, 0.2f), wallM);
        Box("Wall_Front", env, new Vector3(0, 1.5f, 3.1f), new Vector3(6.4f, 3f, 0.2f), wallM);

        // murs gauche / droit avec ouverture de porte (z -0.5..0.5, h 2.1)
        foreach (float sx in new[] { -1f, 1f })
        {
            string n = sx < 0 ? "Left" : "Right";
            float x = 3.1f * sx;
            Box($"Wall_{n}_A", env, new Vector3(x, 1.5f, -1.85f), new Vector3(0.2f, 3f, 2.7f), wallM);
            Box($"Wall_{n}_B", env, new Vector3(x, 1.5f, 1.85f), new Vector3(0.2f, 3f, 2.7f), wallM);
            Box($"Wall_{n}_Lintel", env, new Vector3(x, 2.55f, 0f), new Vector3(0.2f, 0.9f, 1f), wallM);
        }

        // quelques caisses pour casser le volume
        Box("Crate_1", env, new Vector3(2.5f, 0.35f, 2.4f), new Vector3(0.7f, 0.7f, 0.7f), metalM);
        Box("Crate_2", env, new Vector3(-2.5f, 0.25f, 2.5f), new Vector3(0.5f, 0.5f, 0.5f), metalM);
    }

    static void BuildLamp()
    {
        var g = new GameObject("CeilingLamp");
        g.transform.SetParent(env, false);
        Prim(PrimitiveType.Cylinder, "Cord", g.transform, new Vector3(0, 2.85f, 0), new Vector3(0.01f, 0.15f, 0.01f), darkM);
        Prim(PrimitiveType.Cylinder, "Shade", g.transform, new Vector3(0, 2.7f, 0), new Vector3(0.3f, 0.04f, 0.3f), metalM);
        var bulb = Prim(PrimitiveType.Sphere, "Bulb", g.transform, new Vector3(0, 2.62f, 0), Vector3.one * 0.09f, bulbM);

        var lg = new GameObject("LampLight");
        lg.transform.SetParent(g.transform, false);
        lg.transform.localPosition = new Vector3(0, 2.55f, 0);
        var l = lg.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.78f, 0.5f);
        l.range = 9f;
        l.intensity = 6f;
        l.shadows = LightShadows.None;

        var f = g.AddComponent<FlickerLamp>();
        f.lamp = l;
        f.bulb = bulb.GetComponent<Renderer>();
        f.baseIntensity = 6f;
    }

    // couloirs derrière les portes (pour avoir quelque chose à filmer) -> retourne 2 caméras
    static Camera[] BuildCorridors()
    {
        var cams = new Camera[3];
        int i = 0;
        foreach (float sx in new[] { -1f, 1f })
        {
            string n = sx < 0 ? "Left" : "Right";
            var c = new GameObject($"Corridor_{n}").transform;
            c.SetParent(env, false);
            float cx = 5.1f * sx;
            Box("Floor", c, new Vector3(cx, -0.05f, 0), new Vector3(3.8f, 0.1f, 3.2f), floorM);
            Box("Ceiling", c, new Vector3(cx, 2.65f, 0), new Vector3(3.8f, 0.1f, 3.2f), ceilM);
            Box("Far", c, new Vector3(7.05f * sx, 1.3f, 0), new Vector3(0.1f, 2.6f, 3.2f), wallM);
            Box("Side_A", c, new Vector3(cx, 1.3f, -1.55f), new Vector3(3.8f, 2.6f, 0.1f), wallM);
            Box("Side_B", c, new Vector3(cx, 1.3f, 1.55f), new Vector3(3.8f, 2.6f, 0.1f), wallM);
            Box("Pillar", c, new Vector3(5.8f * sx, 1.3f, -0.9f), new Vector3(0.4f, 2.6f, 0.4f), metalM);

            var lg = new GameObject("CorridorLight");
            lg.transform.SetParent(c, false);
            lg.transform.position = new Vector3(5.7f * sx, 2.2f, 0);
            var l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = sx < 0 ? new Color(0.5f, 0.6f, 1f) : new Color(1f, 0.5f, 0.45f);
            l.range = 2.8f;
            l.intensity = 2f;
            l.shadows = LightShadows.None;

            var cg = new GameObject($"CCTV_{n}Door");
            cg.transform.SetParent(env, false);
            cg.transform.position = new Vector3(6.7f * sx, 2.3f, 0);
            cg.transform.LookAt(new Vector3(3.1f * sx, 1.0f, 0));
            cams[i++] = cg.AddComponent<Camera>();
        }

        var rg = new GameObject("CCTV_Room");
        rg.transform.SetParent(env, false);
        rg.transform.position = new Vector3(2.8f, 2.8f, -2.8f);
        rg.transform.LookAt(new Vector3(0, 1.0f, 0));
        cams[2] = rg.AddComponent<Camera>();
        return cams;
    }

    // ---------- table + écrans ----------
    static void BuildMonitors(GameObject rig, Camera head, Camera[] feeds)
    {
        var table = new GameObject("MonitorTable").transform;
        table.SetParent(env, false);
        Box("Top", table, new Vector3(0, 0.72f, 1.0f), new Vector3(2.0f, 0.06f, 0.6f), metalM);
        foreach (float lx in new[] { -0.9f, 0.9f })
            foreach (float lz in new[] { 0.75f, 1.25f })
                Box("Leg", table, new Vector3(lx, 0.35f, lz), new Vector3(0.06f, 0.7f, 0.06f), darkM);

        var stationGo = new GameObject("MonitorStation");
        var station = stationGo.AddComponent<MonitorStation>();
        station.feeds = new Material[feeds.Length];

        float[] xs = { -0.55f, 0f, 0.55f };
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
            m.transform.position = new Vector3(xs[i], 0.98f, 1.12f);
            m.transform.rotation = Quaternion.Euler(10f, 15f * xs[i] / 0.55f, 0f);
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

        // petite batterie sur la table
        MakeBattery(new Vector3(0.9f, 0.78f, 1.0f));
    }

    // ---------- établi + planches + piles ----------
    static void BuildWorkbench()
    {
        var wb = new GameObject("Workbench").transform;
        wb.SetParent(env, false);
        Box("Top", wb, new Vector3(0, 0.9f, -2.6f), new Vector3(2.8f, 0.08f, 0.7f), metalM);
        Box("Shelf", wb, new Vector3(0, 0.35f, -2.6f), new Vector3(2.6f, 0.05f, 0.6f), metalM);
        foreach (float lx in new[] { -1.3f, 1.3f })
            foreach (float lz in new[] { -2.9f, -2.3f })
                Box("Leg", wb, new Vector3(lx, 0.45f, lz), new Vector3(0.07f, 0.9f, 0.07f), darkM);

        for (int pile = 0; pile < 2; pile++)
            for (int i = 0; i < 3; i++)
                MakePlank(new Vector3(pile == 0 ? -0.65f : 0.65f, 0.97f + i * 0.055f, -2.55f), Quaternion.identity);

        MakeBattery(new Vector3(0f, 0.97f, -2.4f));
        MakeBattery(new Vector3(2.5f, 0.05f, -2.5f));
    }

    static GameObject MakePlank(Vector3 pos, Quaternion rot)
    {
        var p = new GameObject("Plank");
        p.transform.SetParent(items, false);
        p.transform.SetPositionAndRotation(pos, rot);
        var bc = p.AddComponent<BoxCollider>();
        bc.size = new Vector3(1.2f, 0.14f, 0.04f);
        Box("Visual", p.transform, Vector3.zero, new Vector3(1.2f, 0.14f, 0.04f), plankM, false);
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
            sc.center = new Vector3(0, 0, 0.04f);
            var vis = Prim(PrimitiveType.Cylinder, "Visual", n.transform, new Vector3(0, 0, 0.06f), new Vector3(0.016f, 0.04f, 0.016f), nailM, new Vector3(90, 0, 0));
            var nail = n.AddComponent<Nail>();
            nail.visual = vis;
            nail.col = sc;
            vis.SetActive(false);   // les clous n'apparaissent que marteau en main
            sc.enabled = false;
            nails[i] = nail;
        }

        // zone pour tenir la planche avec l'autre main
        var hz = new GameObject("HoldZone");
        hz.transform.SetParent(p.transform, false);
        var hc = hz.AddComponent<BoxCollider>();
        hc.size = new Vector3(1.3f, 0.26f, 0.16f);
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

    // ---------- portes + emplacements de planches ----------
    static void BuildDoors()
    {
        foreach (float sx in new[] { -1f, 1f })
        {
            string n = sx < 0 ? "Left" : "Right";
            var d = new GameObject($"Door_{n}").transform;
            d.SetParent(env, false);
            Box("Leaf", d, new Vector3(3.1f * sx, 1.05f, 0f), new Vector3(0.08f, 2.1f, 1f), doorM);
            Box("Handle", d, new Vector3(3.0f * sx, 1.0f, 0.35f), new Vector3(0.05f, 0.05f, 0.14f), metalM, false);

            Vector3 normal = new Vector3(-sx, 0, 0);   // vers l'intérieur de la pièce
            for (int i = 0; i < 3; i++)
            {
                var s = new GameObject($"Slot_{n}_{i}");
                s.transform.SetParent(d, false);
                s.transform.position = new Vector3(2.95f * sx, 0.55f + 0.5f * i, 0f);
                s.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
                var ghost = Box("Ghost", s.transform, Vector3.zero, new Vector3(1.24f, 0.18f, 0.06f), ghostM, false);
                var gr = ghost.GetComponent<Renderer>();
                gr.enabled = false;   // visible uniquement quand une planche est en main
                s.AddComponent<BarricadeSlot>().ghost = gr;
            }
        }
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
        belt.transform.SetPositionAndRotation(new Vector3(0, 0.95f, 0), Quaternion.identity);
        var br = belt.AddComponent<BeltRig>();
        br.head = head.transform;
        br.rigRoot = rig.transform;

        Vector3 B = belt.transform.position;

        var pistol = MakePistol(B + new Vector3(0.22f, 0f, 0.02f));
        var hammer = MakeHammer(B + new Vector3(-0.24f, 0f, -0.08f));
        var light = MakeFlashlight(B + new Vector3(0.2f, 0f, -0.12f));
        var mag = MakeMagazine(B + new Vector3(-0.1f, 0f, 0.18f));

        MakeSocket(belt.transform, "Holster_Pistol", new Vector3(0.22f, 0f, 0.02f), BeltItemType.Pistol, pistol);
        MakeSocket(belt.transform, "Holster_Hammer", new Vector3(-0.24f, 0f, -0.08f), BeltItemType.Hammer, hammer);
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

    static XRGrabInteractable MakeHammer(Vector3 pos)
    {
        var h = new GameObject("Hammer");
        h.transform.SetParent(items, false);
        h.transform.position = pos;
        var handle = Box("Handle", h.transform, new Vector3(0, 0.05f, 0), new Vector3(0.03f, 0.3f, 0.03f), plankM, true);
        var head = Box("Head", h.transform, new Vector3(0, 0.2f, 0.01f), new Vector3(0.05f, 0.05f, 0.14f), headM, true);
        head.AddComponent<HammerHead>();
        h.AddComponent<Rigidbody>();
        var grab = MakeGrab(h);
        h.AddComponent<BeltItem>().type = BeltItemType.Hammer;
        h.AddComponent<Hammer>();
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
