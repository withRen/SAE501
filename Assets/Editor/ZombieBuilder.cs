using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// Construit le prefab du zombie.
// Provisoire : silhouette humanoïde en boîtes (greybox) avec animation procédurale.
// Avec le modèle réel (asset de zombie) : menu SAE501 > Zombie > Utiliser un modèle (voir UseModel).
public static class ZombieBuilder
{
    public const string PrefabPath = "Assets/Prefabs/Zombie.prefab";

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

        // zones touchables : tête = mort, corps = ralenti
        var z = root.AddComponent<Zombie>();
        var body = new GameObject("Corps");
        body.transform.SetParent(root.transform, false);
        var cc = body.AddComponent<CapsuleCollider>();
        cc.center = new Vector3(0, 0.78f, 0); cc.height = 1.5f; cc.radius = 0.3f;
        var bh = body.AddComponent<ZombieHitbox>(); bh.zombie = z; bh.head = false;
        var hg = new GameObject("TeteTouchable");
        hg.transform.SetParent(root.transform, false);
        var sc = hg.AddComponent<SphereCollider>();
        sc.center = new Vector3(0, 1.6f, 0.02f); sc.radius = 0.2f;
        var hh = hg.AddComponent<ZombieHitbox>(); hh.zombie = z; hh.head = true;

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
