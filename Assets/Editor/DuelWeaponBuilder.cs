using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Armes principales du duel 1v1 : fusil d'assaut AK74M (pack Cold War Weapons) et sniper.
// Menu SAE501 → Arène 1v1 → Construire AK74 et sniper : crée Assets/Resources/Weapons/AK74.prefab et Sniper.prefab (chargés par DuelLoadout).
// Ce sont des « Pistol » réglés autrement (cadence, rafale, dégâts, son) : la version PC (DesktopPlayer), le holster, la visée et le tir
// fonctionnent donc comme pour le pistolet. Le point d'attache est la poignée-pistolet, à l'origine du prefab ; l'axe Z est le canon, Y le haut.
// AK74 : modèle du pack (matériau recréé en URP). Sniper : MODÈLE PROVISOIRE fait de formes simples, à remplacer par un vrai modèle
// (déposer un prefab « Sniper » dans Resources/Weapons garde les réglages à refaire sur lui : composant Pistol, Rigidbody, grab).
public static class DuelWeaponBuilder
{
    const string Out = "Assets/Resources/Weapons";
    const string AkPrefab = "Assets/Cold War Weapons/AK74M/Prefabs/ak74m.prefab";
    const string TexDir = "Assets/Cold War Weapons/AK74M/Materials/Textures/";

    [MenuItem("SAE501/Arène 1v1/Construire AK74 et sniper")]
    public static void Build()
    {
        System.IO.Directory.CreateDirectory(Out);
        BuildAk();
        BuildSniper();
        AssetDatabase.SaveAssets();
        Debug.Log("[Armes] AK74 et sniper construits dans " + Out);
    }

    // ---------- AK74 ----------
    static void BuildAk()
    {
        var src = AssetDatabase.LoadAssetAtPath<GameObject>(AkPrefab);
        if (!src) { Debug.LogError("[Armes] " + AkPrefab + " introuvable"); return; }
        var root = new GameObject("AK74");
        var rotor = new GameObject("Modele").transform;
        rotor.SetParent(root.transform, false);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(src, rotor);
        PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        var mat = AkMaterial();
        foreach (var r in model.GetComponentsInChildren<Renderer>(true))
        {
            r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // orientation : le canon est à l'opposé de la crosse, le haut à l'opposé du chargeur (mesuré sur les pièces du modèle)
        Transform Part(string n) => model.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == n);
        var b = Bounds(model);
        var stock = Part("stock"); var mag = Part("mag"); var handle = Part("handle");
        Vector3 c = b.center;
        int L = b.size.x >= b.size.y && b.size.x >= b.size.z ? 0 : (b.size.y >= b.size.z ? 1 : 2);
        var fwd = Vector3.zero; fwd[L] = Mathf.Sign(c[L] - (stock ? Center(stock)[L] : b.min[L]));
        // le chargeur est le côté « bas » : l'axe, hors canon, où il s'écarte le plus du centre
        var md = (mag ? Center(mag) : c - Vector3.up) - c; md[L] = 0f;
        int U = Mathf.Abs(md.x) >= Mathf.Abs(md.y) && Mathf.Abs(md.x) >= Mathf.Abs(md.z) ? 0 : (Mathf.Abs(md.y) >= Mathf.Abs(md.z) ? 1 : 2);
        var up = Vector3.zero; up[U] = -Mathf.Sign(md[U]);
        Debug.Log("[Armes] AK74 : taille " + b.size + " axe long " + L + " canon vers " + fwd + " haut " + up);
        rotor.localRotation = Quaternion.Inverse(Quaternion.LookRotation(fwd, up));

        // poignée-pistolet à l'origine : la pièce « handle » du modèle est la poignée d'armement, pas la crosse ; la poignée est sous la détente,
        // un peu en arrière (6,5 cm) et plus bas (5 cm), mesuré sur le modèle
        var trigger = Part("trigger");
        var grip = trigger ? Center(trigger) - fwd * 0.065f - up * 0.05f : (handle ? Center(handle) : c);
        rotor.localPosition = -(rotor.localRotation * grip);
        b = BoundsIn(root.transform, model);

        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(root.transform, false);
        muzzle.localPosition = new Vector3(0f, 0.10f, b.max.z);          // hauteur du canon au-dessus de la poignée, mesurée sur le modèle
        var ejection = new GameObject("Ejection").transform;
        ejection.SetParent(root.transform, false);
        ejection.localPosition = new Vector3(0.035f, 0.09f, 0.12f);       // fenêtre d'éjection, côté droit de la boîte de culasse
        ejection.localRotation = Quaternion.Euler(0f, 90f, 0f);

        var pi = Finish(root, b, muzzle, ejection, model.GetComponentInChildren<Animator>());
        pi.ammo = pi.maxAmmo = 25; pi.reloadTime = 2.2f;
        pi.automatic = true; pi.fireInterval = 0.11f;
        pi.pvpHeadDamage = 100f; pi.pvpBodyDamage = 25f;          // CS : 4 balles au corps, 1 à la tête
        pi.shotPitch = 0.78f; pi.shotVolume = 1f;
        pi.fireState = "AK74_Fire"; pi.fireStartNormalized = 0f; pi.casingDelay = 0.02f;
        pi.smokeLocalPosition = new Vector3(0f, 0.10f, 0.10f);
        Save(root, "AK74");
    }

    static Material AkMaterial()
    {
        var path = Out + "/AK74_URP.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        Texture T(string n) => AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + n);
        m.SetTexture("_BaseMap", T("ak74m_AlbedoSmoothness.png"));
        m.SetColor("_BaseColor", Color.white);
        m.SetTexture("_BumpMap", T("ak74m_Normal.png")); m.EnableKeyword("_NORMALMAP");
        m.SetTexture("_MetallicGlossMap", T("ak74m_MetallicSmoothness.png")); m.EnableKeyword("_METALLICSPECGLOSSMAP");
        m.SetTexture("_OcclusionMap", T("ak74m_AO.png")); m.EnableKeyword("_OCCLUSIONMAP");
        m.SetFloat("_Smoothness", 0.9f);
        EditorUtility.SetDirty(m);
        return m;
    }

    // ---------- sniper : AWM (pack VCR01, matériaux URP du pack) ----------
    const string AwmPrefab = "Assets/VCR01/VCR_AWM/Black/URP/AWMOptimizedBlackURP.prefab";

    static void BuildSniper()
    {
        var src = AssetDatabase.LoadAssetAtPath<GameObject>(AwmPrefab);
        if (!src) { BuildSniperPlaceholder(); return; }
        var root = new GameObject("Sniper");
        var rotor = new GameObject("Modele").transform;
        rotor.SetParent(root.transform, false);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(src, rotor);
        PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        // le silencieux et la cartouche de la chambre sont retirés : le fusil fait alors environ 2 m dans le pack, on le ramène à 1,3 m
        foreach (var n in new[] { "Surpressor", "Bullet" })
        {
            var t = model.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == n);
            if (t) Object.DestroyImmediate(t.gameObject);
        }
        foreach (var r in model.GetComponentsInChildren<Renderer>(true)) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // dans le pack : canon vers -Z, haut vers +Y ; la crosse est à l'arrière (z ≈ -0,1), la poignée sous la boîte de culasse
        var b = Bounds(model);
        float s = 1.3f / b.size.z;
        rotor.localScale = Vector3.one * s;
        rotor.localRotation = Quaternion.Euler(0f, 180f, 0f);
        var grip = new Vector3(b.center.x, b.min.y + 0.10f * b.size.y, b.max.z - 0.25f * b.size.z);   // poignée-pistolet : sous la culasse, en arrière
        rotor.localPosition = -(rotor.localRotation * (grip * s));
        b = BoundsIn(root.transform, model);
        Debug.Log("[Armes] AWM : taille " + b.size + " échelle " + s);

        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(root.transform, false);
        muzzle.localPosition = new Vector3(0f, 0.09f, b.max.z);
        var ejection = new GameObject("Ejection").transform;
        ejection.SetParent(root.transform, false);
        ejection.localPosition = new Vector3(0.03f, 0.09f, 0.1f);
        ejection.localRotation = Quaternion.Euler(0f, 90f, 0f);

        var pi = Finish(root, b, muzzle, ejection, null);
        pi.ammo = pi.maxAmmo = 10; pi.reloadTime = 3f;
        pi.automatic = false; pi.fireInterval = 1.4f;
        pi.pvpHeadDamage = 100f; pi.pvpBodyDamage = 100f; pi.pvpLegDamage = 50f;          // AWM : 1 coup (2 aux jambes)
        pi.shotPitch = 0.55f; pi.shotVolume = 1f;
        pi.casingDelay = 0.5f;
        pi.smokeLocalPosition = new Vector3(0f, 0.09f, 0.1f);
        Save(root, "Sniper");
    }

    // ---------- sniper de secours (formes simples, si le pack AWM est absent) ----------
    static void BuildSniperPlaceholder()
    {
        var root = new GameObject("Sniper");
        var steel = SolidMat("Sniper_Acier", new Color(0.09f, 0.09f, 0.1f), 0.55f, 0.8f);
        var wood = SolidMat("Sniper_Crosse", new Color(0.3f, 0.32f, 0.2f), 0.25f, 0f);
        var glass = SolidMat("Sniper_Lentille", new Color(0.05f, 0.12f, 0.2f), 0.9f, 0f);
        Shape(root, "Canon", PrimitiveType.Cylinder, new Vector3(0f, 0.045f, 0.52f), new Vector3(0.026f, 0.43f, 0.026f), Quaternion.Euler(90f, 0f, 0f), steel);
        Shape(root, "Bouche", PrimitiveType.Cylinder, new Vector3(0f, 0.045f, 0.97f), new Vector3(0.036f, 0.03f, 0.036f), Quaternion.Euler(90f, 0f, 0f), steel);
        Shape(root, "Boitier", PrimitiveType.Cube, new Vector3(0f, 0.04f, 0.1f), new Vector3(0.05f, 0.075f, 0.42f), Quaternion.identity, steel);
        Shape(root, "Garde_main", PrimitiveType.Cube, new Vector3(0f, 0.032f, 0.45f), new Vector3(0.048f, 0.06f, 0.36f), Quaternion.identity, wood);
        Shape(root, "Crosse", PrimitiveType.Cube, new Vector3(0f, 0.0f, -0.3f), new Vector3(0.045f, 0.13f, 0.34f), Quaternion.Euler(-6f, 0f, 0f), wood);
        Shape(root, "Joue", PrimitiveType.Cube, new Vector3(0f, 0.065f, -0.22f), new Vector3(0.04f, 0.04f, 0.2f), Quaternion.identity, wood);
        Shape(root, "Poignee", PrimitiveType.Cube, new Vector3(0f, -0.06f, -0.06f), new Vector3(0.038f, 0.12f, 0.055f), Quaternion.Euler(18f, 0f, 0f), wood);
        Shape(root, "Chargeur", PrimitiveType.Cube, new Vector3(0f, -0.045f, 0.14f), new Vector3(0.035f, 0.09f, 0.09f), Quaternion.identity, steel);
        Shape(root, "Culasse", PrimitiveType.Cylinder, new Vector3(0.055f, 0.055f, 0.02f), new Vector3(0.012f, 0.04f, 0.012f), Quaternion.Euler(0f, 0f, 90f), steel);
        Shape(root, "Lunette", PrimitiveType.Cylinder, new Vector3(0f, 0.115f, 0.12f), new Vector3(0.042f, 0.17f, 0.042f), Quaternion.Euler(90f, 0f, 0f), steel);
        Shape(root, "Lunette_Avant", PrimitiveType.Cylinder, new Vector3(0f, 0.115f, 0.3f), new Vector3(0.055f, 0.035f, 0.055f), Quaternion.Euler(90f, 0f, 0f), steel);
        Shape(root, "Lunette_Arriere", PrimitiveType.Cylinder, new Vector3(0f, 0.115f, -0.04f), new Vector3(0.05f, 0.03f, 0.05f), Quaternion.Euler(90f, 0f, 0f), steel);
        Shape(root, "Lentille", PrimitiveType.Cylinder, new Vector3(0f, 0.115f, 0.318f), new Vector3(0.045f, 0.004f, 0.045f), Quaternion.Euler(90f, 0f, 0f), glass);
        Shape(root, "Support_1", PrimitiveType.Cube, new Vector3(0f, 0.083f, 0.05f), new Vector3(0.02f, 0.035f, 0.03f), Quaternion.identity, steel);
        Shape(root, "Support_2", PrimitiveType.Cube, new Vector3(0f, 0.083f, 0.22f), new Vector3(0.02f, 0.035f, 0.03f), Quaternion.identity, steel);

        var b = BoundsIn(root.transform, root);
        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(root.transform, false);
        muzzle.localPosition = new Vector3(0f, 0.045f, b.max.z);
        var ejection = new GameObject("Ejection").transform;
        ejection.SetParent(root.transform, false);
        ejection.localPosition = new Vector3(0.03f, 0.05f, 0.05f);
        ejection.localRotation = Quaternion.Euler(0f, 90f, 0f);

        var pi = Finish(root, b, muzzle, ejection, null);
        pi.ammo = pi.maxAmmo = 10; pi.reloadTime = 3f;
        pi.automatic = false; pi.fireInterval = 1.4f;
        pi.pvpHeadDamage = 100f; pi.pvpBodyDamage = 100f; pi.pvpLegDamage = 50f;          // AWM : 1 coup (2 aux jambes)
        pi.shotPitch = 0.55f; pi.shotVolume = 1f;
        pi.casingDelay = 0.5f;                         // la douille sort quand on actionne la culasse
        pi.smokeLocalPosition = new Vector3(0f, 0.05f, 0.1f);
        Save(root, "Sniper");
    }

    // ---------- commun ----------
    // Pistol + prise (grab, règles de MainScene, holster) : le même câblage que RoomBuilder.MakePistol
    static Pistol Finish(GameObject root, Bounds b, Transform muzzle, Transform ejection, Animator animator)
    {
        var attach = new GameObject("Attach").transform;
        attach.SetParent(root.transform, false);
        attach.localPosition = Vector3.zero;

        var bc = root.AddComponent<BoxCollider>();
        bc.center = b.center; bc.size = b.size;
        var rb = root.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        var grab = root.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grab.attachTransform = attach;
        root.AddComponent<IgnorePlayer>();
        root.AddComponent<GrabGuard>();
        root.AddComponent<ItemSafety>();
        root.AddComponent<BeltItem>().type = BeltItemType.Pistol;      // l'arme principale se range aussi au holster (touches 1 à 4 en version PC)

        var pi = root.AddComponent<Pistol>();
        pi.muzzle = muzzle; pi.ejection = ejection; pi.animator = animator;
        pi.casingPrefab = GunBuilder.CasingPrefab();
        pi.muzzleFlashPrefab = GunBuilder.MuzzleFlashPrefab();
        pi.smokeMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Generated/GunSmoke.mat");
        pi.shotClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Gun/Pistolet_Tir.mp3");
        pi.reloadClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Gun/Pistolet_Rechargement.mp3");

        var fl = new GameObject("Flash");
        fl.transform.SetParent(muzzle, false);
        var l = fl.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(1f, 0.8f, 0.4f); l.range = 5f; l.intensity = 10f; l.shadows = LightShadows.None; l.enabled = false;
        pi.flash = l;
        return pi;
    }

    static void Save(GameObject root, string name)
    {
        PrefabUtility.SaveAsPrefabAsset(root, Out + "/" + name + ".prefab");
        Object.DestroyImmediate(root);
    }

    static Material SolidMat(string name, Color c, float smooth, float metal)
    {
        var path = Out + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", smooth); m.SetFloat("_Metallic", metal);
        EditorUtility.SetDirty(m);
        return m;
    }

    static void Shape(GameObject root, string name, PrimitiveType type, Vector3 pos, Vector3 scale, Quaternion rot, Material m)
    {
        var g = GameObject.CreatePrimitive(type);
        g.name = name;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.SetParent(root.transform, false);
        g.transform.localPosition = pos; g.transform.localRotation = rot; g.transform.localScale = scale;
        var r = g.GetComponent<Renderer>(); r.sharedMaterial = m; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    static Vector3 Center(Transform t)
    {
        var r = t.GetComponent<Renderer>();
        return r ? r.bounds.center : t.position;
    }

    static Bounds Bounds(GameObject g)
    {
        var rs = g.GetComponentsInChildren<Renderer>(true);
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    // boîte englobante dans le repère de `space` (les rendus sont tournés : on encadre les 8 coins de chaque boîte)
    static Bounds BoundsIn(Transform space, GameObject g)
    {
        bool first = true; var res = new Bounds();
        foreach (var r in g.GetComponentsInChildren<Renderer>(true))
        {
            var wb = r.bounds;
            for (int i = 0; i < 8; i++)
            {
                var corner = wb.center + Vector3.Scale(wb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var p = space.InverseTransformPoint(corner);
                if (first) { res = new Bounds(p, Vector3.zero); first = false; } else res.Encapsulate(p);
            }
        }
        return res;
    }
}
