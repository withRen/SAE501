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

    // Prefab à part : les 3 points de départ des chemins NavMesh des zombies (repère de la carte, final).
    //   bout du couloir gauche, bout du couloir droit, fenêtre Nord (cour au nord du bunker).
    [MenuItem("SAE501/Zombie/Reconstruire le prefab des points de départ")]
    public static GameObject BuildSpawnPoints()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        var root = new GameObject("ZombieSpawnPoints");
        var sp = root.AddComponent<ZombieSpawnPoints>();
        Transform P(string n, Vector3 pos)
        {
            var t = new GameObject(n).transform;
            t.SetParent(root.transform, false);
            t.position = pos;
            t.rotation = Quaternion.LookRotation(Vector3.back);     // les zombies regardent vers le bunker
            return t;
        }
        sp.leftCorridorEnd = P("Spawn_Couloir_Gauche", new Vector3(-4.7f, 0f, 13.3f));
        sp.rightCorridorEnd = P("Spawn_Couloir_Droit", new Vector3(4.7f, 0f, 13.3f));
        sp.northWindow = P("Spawn_Fenetre_Nord", new Vector3(-0.3f, 0f, 7.0f));
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
        agent.radius = 0.3f; agent.height = 1.8f; agent.speed = 0.9f; agent.acceleration = 4f; agent.angularSpeed = 240f; agent.stoppingDistance = 0.1f;

        var rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        var proc = root.AddComponent<ZombieProcAnim>();
        proc.visual = visual; proc.armL = armL; proc.armR = armR; proc.legL = legL; proc.legR = legR;
        var bridge = root.AddComponent<ZombieAnimationBridge>();
        bridge.procedural = proc;

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    public static GameObject EnsurePrefab() => AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) ?? BuildPlaceholder();
}
