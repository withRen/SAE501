using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// Construit le prefab du zombie.
// Provisoire : silhouette humanoïde en boîtes (greybox) avec animation procédurale.
// Avec le modèle réel (asset de zombie) : menu SAE501 > Zombie > Utiliser un modèle (voir UseModel).
public static class ZombieBuilder
{
    public const string PrefabPath = "Assets/Prefabs/Zombie.prefab";
    public const string SpawnPointsPath = "Assets/Prefabs/ZombieSpawnPoints.prefab";

    // Prefab à part : les 3 points de départ des chemins NavMesh des zombies (carte Blender, repère Unity = (-x, z, -y) de Blender).
    //   bout nord du couloir gauche, bout nord du couloir droit, extérieur au sud du bunker (devant les deux fenêtres).
    [MenuItem("SAE501/Zombie/Reconstruire le prefab des points de départ")]
    public static GameObject BuildSpawnPoints()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        var root = new GameObject("ZombieSpawnPoints");
        var sp = root.AddComponent<ZombieSpawnPoints>();
        Transform P(string n, Vector3 pos, Vector3 look)
        {
            var t = new GameObject(n).transform;
            t.SetParent(root.transform, false);
            t.position = pos;
            t.rotation = Quaternion.LookRotation(look);     // les zombies regardent vers le bunker
            return t;
        }
        sp.leftCorridorEnd = P("Spawn_Couloir_Gauche", new Vector3(12.0f, 0f, -3.7f), Vector3.forward);    // Blender (-12, 3,7)
        sp.rightCorridorEnd = P("Spawn_Couloir_Droit", new Vector3(-7.1f, 0f, -7.0f), Vector3.forward);    // Blender (7,1, 7)
        sp.exterior = P("Spawn_Exterieur_Sud", new Vector3(1.85f, 0f, 17.6f), Vector3.back);               // Blender (-1,85, -17,6)
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, SpawnPointsPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        return prefab;
    }


    static Material Mat(string name, Color c)
    {
        string p = $"Assets/Generated/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(p);
        if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, p); }
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Smoothness", 0.15f);
        EditorUtility.SetDirty(m);
        return m;
    }

    static GameObject Cube(string name, Transform parent, Vector3 pos, Vector3 scale, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }

    const string ModelPrefab = "Assets/NewPunch/ShirtlessZombieFree/Prefabs/ShirtlessZombie_FREE_URP.prefab";

    // Zombie avec le modèle « Shirtless Zombie FREE » (rig humanoïde).
    // Animations : clips Mixamo si présents (ZombieMixamo), sinon os animés par ZombieBoneAnim.
    [MenuItem("SAE501/Zombie/Reconstruire le prefab (modèle Shirtless Zombie)")]
    public static GameObject BuildShirtless()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPrefab);
        if (model == null) return BuildPlaceholder();
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

        var root = new GameObject("Zombie");
        var visual = new GameObject("Visual").transform;
        visual.SetParent(root.transform, false);
        var m = (GameObject)PrefabUtility.InstantiatePrefab(model, visual);
        PrefabUtility.UnpackPrefabInstance(m, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        m.name = "ShirtlessZombie";
        m.transform.localPosition = Vector3.zero; m.transform.localRotation = Quaternion.identity;
        foreach (var e in m.GetComponentsInChildren<FreeZombie_EyesGlow>(true)) Object.DestroyImmediate(e);   // script HDRP du pack, inutile
        var anim = m.GetComponent<Animator>();
        // vraies animations Mixamo si elles sont dans le projet (voir ZombieMixamo), sinon os pilotés par script
        var mixamo = anim ? ZombieMixamo.EnsureController() : null;
        if (mixamo)
        {
            anim.enabled = true;
            anim.runtimeAnimatorController = mixamo;
            anim.applyRootMotion = false;                  // c'est le NavMeshAgent qui déplace le zombie
            anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }
        else if (anim) anim.enabled = false;               // aucun clip : on pilote les os par script

        Transform B(string n) { foreach (var t in m.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t; Debug.LogError("os introuvable : " + n); return null; }
        var head = B("head");

        var z = root.AddComponent<Zombie>();
        z.ApplyLevel(1);

        // ---- hitbox sur les os (suivent l'animation) : tête 100 % / torse + bras 33 % / jambes 20 % ----
        void Hit(string name, Transform bone, HitZone zone, System.Func<GameObject, Collider> make)
        {
            var g = new GameObject(name);
            g.transform.SetParent(bone, true);
            var c = make(g);
            var h = g.AddComponent<ZombieHitbox>(); h.zombie = z; h.zone = zone;
        }
        float Sc(Transform t) => Mathf.Max(0.0001f, t.lossyScale.x);
        // sphère au point monde `p`, rayon monde `r`
        void Sphere(string n, Transform bone, HitZone zone, Vector3 p, float r) =>
            Hit(n, bone, zone, g => { g.transform.position = p; g.transform.rotation = root.transform.rotation; var c = g.AddComponent<SphereCollider>(); c.radius = r / Sc(g.transform); return c; });
        // boîte au point monde `p`, taille monde `size` (axes du zombie)
        void Box(string n, Transform bone, HitZone zone, Vector3 p, Vector3 size) =>
            Hit(n, bone, zone, g => { g.transform.position = p; g.transform.rotation = root.transform.rotation; var c = g.AddComponent<BoxCollider>(); c.size = size / Sc(g.transform); return c; });
        // capsule entre deux os (monde)
        void Capsule(string n, Transform bone, HitZone zone, Vector3 a, Vector3 b, float r) =>
            Hit(n, bone, zone, g =>
            {
                g.transform.position = (a + b) / 2f;
                g.transform.rotation = Quaternion.FromToRotation(Vector3.up, (b - a).normalized);
                var c = g.AddComponent<CapsuleCollider>();
                c.direction = 1; c.radius = r / Sc(g.transform); c.height = (b - a).magnitude / Sc(g.transform) + 2f * c.radius;
                return c;
            });

        Sphere("Hitbox_Tete", head, HitZone.Head, head.position + Vector3.up * 0.08f, 0.14f);
        var pelvis = B("pelvis"); var sp2 = B("spine_02"); var neck = B("neck_01");
        Box("Hitbox_Torse", sp2, HitZone.Torso, (pelvis.position + neck.position) / 2f + Vector3.up * 0.02f, new Vector3(0.42f, (neck.position.y - pelvis.position.y) + 0.06f, 0.26f));
        foreach (var side in new[] { "l", "r" })
        {
            Capsule("Hitbox_Bras_" + side, B("upperarm_" + side), HitZone.Torso, B("upperarm_" + side).position, B("hand_" + side).position, 0.06f);
            Capsule("Hitbox_Cuisse_" + side, B("thigh_" + side), HitZone.Legs, B("thigh_" + side).position, B("calf_" + side).position, 0.1f);
            Capsule("Hitbox_Mollet_" + side, B("calf_" + side), HitZone.Legs, B("calf_" + side).position, B("foot_" + side).position + Vector3.up * 0.02f, 0.075f);
        }

        var agent = root.GetComponent<NavMeshAgent>() ?? root.AddComponent<NavMeshAgent>();
        agent.radius = NavBuilder.AgentRadius; agent.height = 1.8f; agent.speed = 0.9f; agent.acceleration = 4f; agent.angularSpeed = 240f; agent.stoppingDistance = 0.1f;
        var rb = root.AddComponent<Rigidbody>(); rb.isKinematic = true;

        root.AddComponent<ZombieVoice>().variants = RoomBuilder.ZombieVoices();     // cris de zombie (3D)
        var bridge = root.AddComponent<ZombieAnimationBridge>();
        root.AddComponent<ZombieWindowClimb>().visual = visual;     // [#74] enjambe les fenêtres au lieu de traverser le mur
        if (mixamo)
        {
            bridge.animator = anim;                         // Speed / Attack / Die -> Animator
            z.attackHitDelay = ZombieMixamo.AttackHitDelay;  // dégâts et planche arrachée au moment où la main frappe dans le clip
        }
        else
        {
            var bone = root.AddComponent<ZombieBoneAnim>();
            bone.visual = visual;
            bone.spine = B("spine_01"); bone.neck = neck; bone.head = head;
            bone.thighL = B("thigh_l"); bone.thighR = B("thigh_r"); bone.calfL = B("calf_l"); bone.calfR = B("calf_r");
            bone.armL = B("upperarm_l"); bone.armR = B("upperarm_r"); bone.foreL = B("lowerarm_l"); bone.foreR = B("lowerarm_r");
            bridge.boneAnim = bone;
        }

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    [MenuItem("SAE501/Zombie/Reconstruire le prefab provisoire")]
    public static GameObject BuildPlaceholder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        var skin = Mat("Zombie_Peau", new Color(0.45f, 0.55f, 0.40f));
        var cloth = Mat("Zombie_Vetements", new Color(0.22f, 0.20f, 0.18f));

        var root = new GameObject("Zombie");
        var visual = new GameObject("Visual").transform;
        visual.SetParent(root.transform, false);

        Cube("Torse", visual, new Vector3(0, 1.05f, 0), new Vector3(0.5f, 0.72f, 0.28f), cloth);
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Tete";
        head.transform.SetParent(visual, false);
        head.transform.localPosition = new Vector3(0, 1.6f, 0.02f);
        head.transform.localScale = Vector3.one * 0.3f;
        head.GetComponent<Renderer>().sharedMaterial = skin;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        Transform Limb(string name, Vector3 pivotPos, Vector3 size, Material m)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(visual, false);
            pivot.localPosition = pivotPos;
            Cube("Membre", pivot, new Vector3(0, -size.y / 2f, 0), size, m);
            return pivot;
        }
        var armL = Limb("BrasG", new Vector3(-0.33f, 1.38f, 0), new Vector3(0.12f, 0.62f, 0.12f), skin);
        var armR = Limb("BrasD", new Vector3(0.33f, 1.38f, 0), new Vector3(0.12f, 0.62f, 0.12f), skin);
        var legL = Limb("JambeG", new Vector3(-0.14f, 0.72f, 0), new Vector3(0.17f, 0.72f, 0.17f), cloth);
        var legR = Limb("JambeD", new Vector3(0.14f, 0.72f, 0), new Vector3(0.17f, 0.72f, 0.17f), cloth);

        // zones touchables (placeholder ; avec le vrai modèle : une zone par os, voir docs/ZOMBIES.md)
        //   Tête   : sphère autour de la tête         -> 100 % (one-shot)
        //   Torse  : tronc + bras (hanches à épaules)  ->  33 % (3 tirs au niveau 1)
        //   Jambes : des pieds aux hanches             ->  20 % (5 tirs au niveau 1)
        var z = root.AddComponent<Zombie>();
        z.ApplyLevel(1);
        void Zone(string name, HitZone zone, Collider c)
        {
            var h = c.gameObject.AddComponent<ZombieHitbox>();
            h.zombie = z; h.zone = zone;
            c.gameObject.name = name;
        }
        var hg = new GameObject("Hitbox_Tete");
        hg.transform.SetParent(root.transform, false);
        var sc = hg.AddComponent<SphereCollider>();
        sc.center = new Vector3(0, 1.6f, 0.02f); sc.radius = 0.2f;
        Zone("Hitbox_Tete", HitZone.Head, sc);

        var tg = new GameObject("Hitbox_Torse");
        tg.transform.SetParent(root.transform, false);
        var tc = tg.AddComponent<BoxCollider>();
        tc.center = new Vector3(0, 1.08f, 0); tc.size = new Vector3(0.78f, 0.74f, 0.32f);   // épaules et bras compris
        Zone("Hitbox_Torse", HitZone.Torso, tc);

        var lg = new GameObject("Hitbox_Jambes");
        lg.transform.SetParent(root.transform, false);
        var lc = lg.AddComponent<BoxCollider>();
        lc.center = new Vector3(0, 0.35f, 0); lc.size = new Vector3(0.45f, 0.70f, 0.28f);
        Zone("Hitbox_Jambes", HitZone.Legs, lc);

        var agent = root.GetComponent<NavMeshAgent>() ?? root.AddComponent<NavMeshAgent>();
        agent.radius = NavBuilder.AgentRadius; agent.height = 1.8f; agent.speed = 0.9f; agent.acceleration = 4f; agent.angularSpeed = 240f; agent.stoppingDistance = 0.1f;

        var rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        var proc = root.AddComponent<ZombieProcAnim>();
        proc.visual = visual; proc.armL = armL; proc.armR = armR; proc.legL = legL; proc.legR = legR;
        root.AddComponent<ZombieVoice>().variants = RoomBuilder.ZombieVoices();     // cris de zombie (3D)
        var bridge = root.AddComponent<ZombieAnimationBridge>();
        root.AddComponent<ZombieWindowClimb>().visual = visual;     // [#74] enjambe les fenêtres au lieu de traverser le mur
        bridge.procedural = proc;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    // reconstruit à chaque génération de scène : modèle Shirtless Zombie s'il est importé, sinon silhouette en boîtes
    public static GameObject EnsurePrefab() => BuildShirtless();
}
