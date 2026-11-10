using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Personnages des joueurs : pack « Survivalist character » de Slayver (Assets/Survivalist), 4 tenues.
// Menu SAE501 → Build Survivalist Skins :
//   - textures : 1024 au plus sur Quest, compression ASTC 6x6 (le pack est en 2048 / 4096) ;
//   - matériaux : URP/Lit simple (couleur + relief) dans Assets/Survivalist/Materials Quest, à la place des matériaux Standard du pack ;
//   - animation : contrôleur Survivalist.controller (attente / marche / course selon « Speed », clips humanoïdes du pack) ;
//   - prefabs : Assets/Resources/Skins/Survivalist_1..4 (Animator + PlayerAvatar), chargés par PlayerSkin.
// Le pack reste tel quel : on peut relancer le menu après une mise à jour du pack.
public static class SurvivalistBuilder
{
    const string Pack = "Assets/Survivalist";
    const string MatDir = Pack + "/Materials Quest";
    const string OutDir = "Assets/Resources/Skins";
    const string ControllerPath = Pack + "/Survivalist.controller";
    const string AnimDir = Pack + "/StarterAssets/ThirdPersonController/Character/Animations";

    [MenuItem("SAE501/Build Survivalist Skins")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(Pack)) { Debug.LogError("[Skins] " + Pack + " introuvable"); return; }
        Textures();
        var controller = Controller();
        Directory.CreateDirectory(MatDir);
        Directory.CreateDirectory(OutDir);
        AssetDatabase.Refresh();

        for (int i = 1; i <= PlayerSkin.Count; i++)
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(Pack + "/Prefab/Survivalist (" + i + ").prefab");
            if (!src) { Debug.LogError("[Skins] prefab Survivalist (" + i + ") introuvable"); continue; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(src);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.name = "Survivalist_" + i;
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            int tris = 0;
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                r.sharedMaterials = r.sharedMaterials.Select(QuestMaterial).ToArray();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                if (r is SkinnedMeshRenderer s)
                {
                    s.updateWhenOffscreen = true;           // les os sont déplacés par script : les limites d'origine ne valent plus
                    s.quality = SkinQuality.Bone2;
                    if (s.sharedMesh && r.gameObject.activeInHierarchy) tris += s.sharedMesh.triangles.Length / 3;
                }
            }
            // rien que le visuel : ni collisions ni scripts du pack
            foreach (var c in go.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            foreach (var m in go.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(m);

            var anim = go.GetComponent<Animator>();
            if (!anim) anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            anim.applyRootMotion = false;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            go.AddComponent<PlayerAvatar>();

            bool human = anim.avatar && anim.avatar.isHuman;
            PrefabUtility.SaveAsPrefabAsset(go, OutDir + "/Survivalist_" + i + ".prefab");
            Object.DestroyImmediate(go);
            Debug.Log("[Skins] Survivalist_" + i + " : " + tris + " triangles, humanoïde " + human);
        }
        AssetDatabase.SaveAssets();
    }

    // matériau Standard du pack → URP/Lit léger (couleur + relief), créé une fois
    static readonly Dictionary<Material, Material> cache = new Dictionary<Material, Material>();
    static Material QuestMaterial(Material src)
    {
        if (!src) return src;
        if (cache.TryGetValue(src, out var done)) return done;
        var path = MatDir + "/" + src.name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        m.shader = Shader.Find("Universal Render Pipeline/Lit");
        var baseTex = src.HasProperty("_MainTex") ? src.GetTexture("_MainTex") : null;
        var bump = src.HasProperty("_BumpMap") ? src.GetTexture("_BumpMap") : null;
        m.SetTexture("_BaseMap", baseTex);
        m.SetColor("_BaseColor", src.HasProperty("_Color") ? src.GetColor("_Color") : Color.white);
        m.SetTexture("_BumpMap", bump);
        if (bump) m.EnableKeyword("_NORMALMAP"); else m.DisableKeyword("_NORMALMAP");
        m.SetFloat("_Metallic", 0f);
        m.SetFloat("_Smoothness", 0.15f);
        m.enableInstancing = true;
        EditorUtility.SetDirty(m);
        cache[src] = m;
        return m;
    }

    static void Textures()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Pack + "/Texture" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!imp) continue;
            var cur = imp.GetPlatformTextureSettings("Android");
            if (cur.overridden && cur.maxTextureSize == 1024 && cur.format == TextureImporterFormat.ASTC_6x6 && imp.maxTextureSize <= 2048) continue;
            imp.maxTextureSize = Mathf.Min(imp.maxTextureSize, 2048);
            imp.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android", overridden = true, maxTextureSize = 1024, format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = 50,
            });
            imp.SaveAndReimport();
        }
    }

    // attente / marche / course selon la vitesse réelle du joueur (m/s), boucle
    static AnimatorController Controller()
    {
        var idle = Clip("Stand--Idle"); var walk = Clip("Locomotion--Walk_N"); var run = Clip("Locomotion--Run_N");
        AssetDatabase.DeleteAsset(ControllerPath);
        var c = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        c.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var state = c.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
        tree.blendType = BlendTreeType.Simple1D;
        tree.blendParameter = "Speed";
        tree.useAutomaticThresholds = false;
        tree.AddChild(idle, 0f);
        tree.AddChild(walk, 1.6f);
        tree.AddChild(run, 4.5f);
        c.layers[0].stateMachine.defaultState = state;
        EditorUtility.SetDirty(c);
        return c;
    }

    static AnimationClip Clip(string file)
    {
        var path = AnimDir + "/" + file + ".anim.fbx";
        var imp = AssetImporter.GetAtPath(path) as ModelImporter;
        if (imp)
        {
            // boucle (le pack ne la coche pas sur tous les clips)
            var clips = imp.clipAnimations.Length > 0 ? imp.clipAnimations : imp.defaultClipAnimations;
            bool changed = false;
            foreach (var cl in clips) if (!cl.loopTime) { cl.loopTime = true; changed = true; }
            if (changed) { imp.clipAnimations = clips; imp.SaveAndReimport(); }
        }
        var clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));
        if (!clip) Debug.LogError("[Skins] clip introuvable : " + path);
        return clip;
    }

    // ---------- vérification sans casque : une image par tenue, en pose « pistolet tenu à deux mains » ----------
    public static void Preview()
    {
        Build();
        var dir = System.Environment.GetEnvironmentVariable("SKIN_PREVIEW_DIR");
        if (string.IsNullOrEmpty(dir)) dir = "Temp";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);
        var cam = Camera.main;
        cam.backgroundColor = new Color(0.16f, 0.17f, 0.19f); cam.clearFlags = CameraClearFlags.SolidColor; cam.fieldOfView = 40f;
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.55f);
        for (int i = 0; i < PlayerSkin.Count; i++)
        {
            var root = new GameObject("Joueur").transform;
            var a = PlayerSkin.Create(i, root);
            if (!a) { Debug.LogError("[Skins] prefab " + (i + 1) + " absent"); continue; }
            a.Init();
            var anim = a.GetComponent<Animator>();
            anim.Rebind(); anim.Update(0f); anim.Update(0.3f);
            a.headTarget = Target(root, new Vector3(0f, 1.66f, 0f), Quaternion.Euler(5f, 0f, 0f));
            a.rightTarget = Target(root, new Vector3(0.10f, 1.42f, 0.42f), Quaternion.Euler(-3f, -4f, 0f));
            a.leftTarget = Target(root, new Vector3(0.02f, 1.38f, 0.40f), Quaternion.Euler(0f, 25f, 20f));
            a.rightHolding = a.leftHolding = true;
            a.floorY = 0f;
            for (int k = 0; k < 40; k++) { anim.Update(0.02f); a.Solve(0.02f); }
            Shot(cam, new Vector3(1.6f, 1.5f, 2.3f), new Vector3(0f, 1.05f, 0.1f), dir + "/skin_" + (i + 1) + "_face.png");
            Shot(cam, new Vector3(-2.4f, 1.5f, 0.6f), new Vector3(0f, 1.2f, 0.2f), dir + "/skin_" + (i + 1) + "_cote.png");
            if (i == 0)
            {
                // accroupi, une main vers le bas (ramasser), l'autre le long du corps
                a.headTarget.localPosition = new Vector3(0f, 1.05f, 0.1f); a.headTarget.localRotation = Quaternion.Euler(35f, 0f, 0f);
                a.rightTarget.localPosition = new Vector3(0.2f, 0.35f, 0.45f); a.rightTarget.localRotation = Quaternion.Euler(60f, 0f, 0f);
                a.leftTarget.localPosition = new Vector3(-0.28f, 0.75f, 0.1f); a.leftTarget.localRotation = Quaternion.Euler(70f, 0f, 0f);
                a.leftHolding = false;
                for (int k = 0; k < 40; k++) { anim.Update(0.02f); a.Solve(0.02f); }
                Shot(cam, new Vector3(2.0f, 1.2f, 1.6f), new Vector3(0f, 0.7f, 0.1f), dir + "/skin_1_accroupi.png");
            }
            Object.DestroyImmediate(root.gameObject);
        }
        Debug.Log("[Skins] aperçus écrits dans " + dir);
    }

    static Transform Target(Transform parent, Vector3 p, Quaternion r)
    {
        var t = new GameObject("Cible").transform;
        t.SetParent(parent, false);
        t.localPosition = p; t.localRotation = r;
        return t;
    }

    static void Shot(Camera cam, Vector3 from, Vector3 lookAt, string path)
    {
        cam.transform.position = from;
        cam.transform.LookAt(lookAt);
        var rt = new RenderTexture(900, 1100, 24);
        cam.targetTexture = rt; cam.Render(); cam.Render(); cam.targetTexture = null;      // 1er rendu : textures pas encore chargées
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tex.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
    }
}
