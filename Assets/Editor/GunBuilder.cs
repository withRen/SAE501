using UnityEditor;
using UnityEngine;

// Pistolet et chargeur du pack « Modern Guns : Handgun » (M1911), préparés pour la VR.
// - matériaux URP du pack (les prefabs d'origine sont en Standard, donc roses sous URP)
// - SimpleShoot retiré (entrée souris) ; les événements d'animation sont relayés par GunAnimationEvents
// - chargeur « plein » du pack (avec ses 2 cartouches visibles), sans Rigidbody ni collider
public static class GunBuilder
{
    const string Root = "Assets/Nokobot/Modern Guns - Handgun/";
    const string GunPrefab = Root + "_Prefabs/Handgun Black/M1911 Handgun_Black (Shooting).prefab";
    const string MagPrefab = Root + "_Prefabs/Handgun Black/M1911 Magazine_Black_Full.prefab";
    const string CasingPrefabPath = Root + "_Prefabs/45ACP Bullet_Casing.prefab";
    const string FlashPrefabPath = Root + "Effects/MuzzleFlash.prefab";
    const string UrpDir = Root + "URP/Materials _URP/";
    const string OutDir = "Assets/Prefabs/Gun";

    public static bool Available => AssetDatabase.LoadAssetAtPath<GameObject>(GunPrefab) != null
                                    && AssetDatabase.LoadAssetAtPath<Material>(UrpDir + "M1911 Handgun_Black _URP.mat") != null;

    public class Gun
    {
        public GameObject model;
        public Animator animator;
        public Transform barrel, slider, ejection, magLocation;
    }

    // Les matériaux URP du pack pour le chargeur et la balle n'ont AUCUNE texture (ils s'affichent blancs) : on les refait ici,
    // avec les textures du pack, dans Assets/Generated (le pack n'est pas modifié).
    static Material Textured(string packName)
    {
        string tex = Root + "Textures/";
        string key, albedo, normal = null, metal = null; float metallic = 0f, smooth = 0.6f;
        if (packName == "M1911 Magazine_Black") { key = "Gun_Chargeur"; albedo = "M1911_Magazine_Albedo_Black.png"; normal = "M1911_Magazine_Normal.png"; metal = "M1911_Magazine_MetallicSmoothness.png"; }
        else if (packName == "45ACP_Bullet_Silver") { key = "Gun_Balle"; albedo = "45ACP_Bullet_Albedo_Silver.png"; normal = "45ACP_Bullet_Normal.png"; metallic = 0.9f; smooth = 0.75f; }
        else return null;
        string path = "Assets/Generated/" + key + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(tex + albedo));
        m.SetColor("_BaseColor", Color.white);
        if (normal != null) { m.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(tex + normal)); m.EnableKeyword("_NORMALMAP"); }
        if (metal != null) { m.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(tex + metal)); m.EnableKeyword("_METALLICSPECGLOSSMAP"); }
        else { m.SetFloat("_Metallic", metallic); m.SetFloat("_Smoothness", smooth); }
        EditorUtility.SetDirty(m);
        return m;
    }

    static void SwapMaterials(GameObject go)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                var urp = Textured(mats[i].name) ?? AssetDatabase.LoadAssetAtPath<Material>(UrpDir + mats[i].name + " _URP.mat");
                if (urp != null) mats[i] = urp;
            }
            r.sharedMaterials = mats;
        }
    }

    static Transform Find(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        Debug.LogError("GunBuilder : introuvable " + name);
        return null;
    }

    public static Gun InstantiateGun(Transform parent)
    {
        var src = AssetDatabase.LoadAssetAtPath<GameObject>(GunPrefab);
        var g = (GameObject)PrefabUtility.InstantiatePrefab(src, parent);
        PrefabUtility.UnpackPrefabInstance(g, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        g.name = "Modele_M1911";
        g.transform.localPosition = Vector3.zero; g.transform.localRotation = Quaternion.identity;
        foreach (var ss in g.GetComponentsInChildren<SimpleShoot>(true)) Object.DestroyImmediate(ss);
        SwapMaterials(g);
        var gun = new Gun { model = g, animator = g.GetComponentInChildren<Animator>() };
        gun.barrel = Find(g.transform, "Barrel_Location");
        gun.slider = Find(g.transform, "Slider");
        gun.ejection = Find(g.transform, "Ejection_Location");
        gun.magLocation = Find(g.transform, "Magazine_Location");
        gun.animator.gameObject.AddComponent<GunAnimationEvents>();
        return gun;
    }

    public static GameObject InstantiateMagazine(Transform parent)
    {
        var src = AssetDatabase.LoadAssetAtPath<GameObject>(MagPrefab);
        var m = (GameObject)PrefabUtility.InstantiatePrefab(src, parent);
        PrefabUtility.UnpackPrefabInstance(m, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        m.name = "Visuel_Chargeur";
        m.transform.localPosition = Vector3.zero; m.transform.localRotation = Quaternion.identity;
        foreach (var c in m.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
        foreach (var rb in m.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(rb);
        SwapMaterials(m);
        return m;
    }

    // douille du pack avec le matériau URP (copie dans Assets/Prefabs/Gun)
    public static GameObject CasingPrefab()
    {
        EnsureDir();
        var src = AssetDatabase.LoadAssetAtPath<GameObject>(CasingPrefabPath);
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(src);
        PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        SwapMaterials(inst);
        var prefab = PrefabUtility.SaveAsPrefabAsset(inst, OutDir + "/Douille_45ACP.prefab");
        Object.DestroyImmediate(inst);
        return prefab;
    }

    // flash du pack : ses matériaux (shaders Standard/Legacy) sont remplacés par un matériau URP additif avec la même planche de sprites
    public static GameObject MuzzleFlashPrefab()
    {
        EnsureDir();
        const string matPath = "Assets/Generated/MuzzleFlash_URP.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(mat, matPath);
        }
        mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Effects/MuzzleFlash_SpriteSheet.png");
        mat.SetFloat("_Surface", 1f); mat.SetFloat("_Blend", 2f);
        mat.SetFloat("_SrcBlend", 5f); mat.SetFloat("_DstBlend", 1f); mat.SetFloat("_ZWrite", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        EditorUtility.SetDirty(mat);

        var src = AssetDatabase.LoadAssetAtPath<GameObject>(FlashPrefabPath);
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(src);
        PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        foreach (var r in inst.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            r.sharedMaterial = mat;
            r.trailMaterial = null;
        }
        var prefab = PrefabUtility.SaveAsPrefabAsset(inst, OutDir + "/Flash_Bouche.prefab");
        Object.DestroyImmediate(inst);
        return prefab;
    }

    static void EnsureDir()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(OutDir)) AssetDatabase.CreateFolder("Assets/Prefabs", "Gun");
    }
}
