using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Construit la scène du menu d'accueil : SAE501 > Build Menu Scene  (Assets/Scenes/Menu.unity, 1re scène du jeu).
// Le joueur est debout sur un quai de gare en ruine : l'image Assets/Menu/MenuBackground.png est posée sur un grand écran
// courbé au loin (horizon) ; autour de lui : sol mouillé, pluie, brouillard qui dérive, néon vert qui grésille, éclairs.
// Le panneau du menu (MainMenu) et le pointeur (MenuPointer) sont construits au lancement.
public static class MenuBuilder
{
    const string XriDir = "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/";
    public const string ScenePath = "Assets/Scenes/Menu.unity";
    const string ImagePath = "Assets/Menu/MenuBackground.png";
    const string Dir = "Assets/Generated/Menu";

    [MenuItem("SAE501/Build Menu Scene")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Generated", "Menu");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- joueur ----------
        var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XriDir + "Prefabs/XR Origin (XR Rig).prefab");
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
        rig.GetComponent<Unity.XR.CoreUtils.XROrigin>().RequestedTrackingOriginMode = Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Device;
        foreach (var nf in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>(true))
            nf.gameObject.SetActive(false);                      // les rayons du menu sont ceux de MenuPointer
        var cam = rig.GetComponentInChildren<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.01f, 0.012f, 0.014f);
        new GameObject("XR Interaction Manager").AddComponent<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();

        // ---------- ambiance ----------
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.055f, 0.06f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.03f, 0.035f, 0.04f);
        RenderSettings.fogDensity = 0.03f;

        // ---------- décor lointain : l'image sur un écran courbé ----------
        var tex = PrepareImage();
        float radius = 14f, arc = 110f;
        float width = radius * arc * Mathf.Deg2Rad;
        float height = tex ? width * tex.height / tex.width : width * 0.5625f;
        float top = 1.2f + 0.55f * height;                           // l'horizon de l'image (~55 % depuis le haut) un peu sous les yeux
        var backdropGo = new GameObject("Decor_Ville");
        backdropGo.AddComponent<MeshFilter>().sharedMesh = CurvedMesh(radius, arc, top - height, top);
        var backdrop = backdropGo.AddComponent<MeshRenderer>();
        backdrop.sharedMaterial = Mat("Decor_Ville", "Universal Render Pipeline/Unlit", new Color(0.85f, 0.85f, 0.85f), tex);
        backdrop.shadowCastingMode = ShadowCastingMode.Off;

        // ---------- sol mouillé ----------
        var floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floor.name = "Quai_Mouille";
        Object.DestroyImmediate(floor.GetComponent<Collider>());
        floor.transform.SetPositionAndRotation(new Vector3(0f, 0f, 2f), Quaternion.Euler(90f, 0f, 0f));
        floor.transform.localScale = new Vector3(12f, 10f, 1f);
        var fm = Mat("Quai_Mouille", "Universal Render Pipeline/Lit", new Color(0.05f, 0.05f, 0.055f), null);
        fm.SetFloat("_Smoothness", 0.9f); fm.SetFloat("_Metallic", 0f);
        floor.GetComponent<Renderer>().sharedMaterial = fm;

        // ---------- lumières ----------
        var moon = new GameObject("Lune").AddComponent<Light>();
        moon.type = LightType.Directional; moon.color = new Color(0.55f, 0.6f, 0.7f); moon.intensity = 0.25f; moon.shadows = LightShadows.None;
        moon.transform.rotation = Quaternion.Euler(35f, -20f, 0f);
        var flash = new GameObject("Eclair").AddComponent<Light>();
        flash.type = LightType.Directional; flash.color = new Color(0.75f, 0.8f, 1f); flash.intensity = 0f; flash.shadows = LightShadows.None;
        flash.transform.rotation = Quaternion.Euler(60f, 10f, 0f);
        var neon = new GameObject("Neon_Vert").AddComponent<Light>();
        neon.type = LightType.Point; neon.color = new Color(0.3f, 1f, 0.45f); neon.range = 5f; neon.intensity = 2f; neon.shadows = LightShadows.None;
        neon.transform.position = new Vector3(-2.2f, 2.5f, 1.8f);

        // panneau lumineux près du néon
        var sign = new GameObject("Panneau_HorsService");
        sign.transform.SetPositionAndRotation(new Vector3(-2.2f, 2.35f, 2.0f), Quaternion.LookRotation(new Vector3(-2.2f, 0f, 2.0f)));
        var signBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(signBack.GetComponent<Collider>());
        signBack.transform.SetParent(sign.transform, false);
        signBack.transform.localPosition = new Vector3(0f, 0f, 0.04f);
        signBack.transform.localScale = new Vector3(1.3f, 0.3f, 0.06f);
        signBack.GetComponent<Renderer>().sharedMaterial = Mat("Panneau", "Universal Render Pipeline/Lit", new Color(0.04f, 0.04f, 0.045f), null);
        var st = new GameObject("Texte").AddComponent<TextMesh>();
        st.transform.SetParent(sign.transform, false);
        st.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        st.fontSize = 64; st.characterSize = 0.012f; st.anchor = TextAnchor.MiddleCenter; st.fontStyle = FontStyle.Bold;
        st.color = new Color(0.35f, 1f, 0.5f); st.text = "HORS SERVICE";
        st.GetComponent<MeshRenderer>().sharedMaterial = st.font.material;

        // ---------- pluie ----------
        var rain = new GameObject("Pluie").AddComponent<ParticleSystem>();
        rain.transform.SetPositionAndRotation(new Vector3(0f, 7f, 2f), Quaternion.Euler(90f, 0f, 0f));   // émet vers le bas
        var main = rain.main;
        main.startLifetime = 0.9f; main.startSpeed = 10f; main.startSize = 0.012f; main.maxParticles = 2500;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.7f, 0.75f, 0.8f, 0.35f);
        var em = rain.emission; em.rateOverTime = 1100f;
        var sh = rain.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(14f, 12f, 0.1f);
        var pr = rain.GetComponent<ParticleSystemRenderer>();
        pr.renderMode = ParticleSystemRenderMode.Stretch; pr.velocityScale = 0.045f; pr.lengthScale = 1f;
        pr.sharedMaterial = Mat("Pluie", "Universal Render Pipeline/Particles/Unlit", new Color(0.75f, 0.8f, 0.85f, 0.35f), null);
        pr.shadowCastingMode = ShadowCastingMode.Off;

        // ---------- brouillard qui dérive ----------
        var mistTex = MistTexture();
        var mistMat = Mat("Brouillard", "Sprites/Default", new Color(0.6f, 0.65f, 0.7f, 0.22f), mistTex);
        var mists = new List<Transform>();
        float[] dist = { 5f, 7.5f, 10f };
        for (int i = 0; i < dist.Length; i++)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = "Brouillard_" + i;
            Object.DestroyImmediate(q.GetComponent<Collider>());
            q.transform.SetPositionAndRotation(new Vector3(0f, 0.7f + 0.4f * i, dist[i]), Quaternion.identity);
            q.transform.localScale = new Vector3(16f, 2.5f + i, 1f);
            q.GetComponent<Renderer>().sharedMaterial = mistMat;
            mists.Add(q.transform);
        }

        // ---------- menu et ambiance animée ----------
        new GameObject("Menu").AddComponent<MainMenu>().head = cam.transform;
        var amb = new GameObject("Ambiance").AddComponent<MenuAmbience>();
        amb.backdrop = backdrop; amb.flashLight = flash; amb.neon = neon; amb.mist = mists.ToArray();

        EditorSceneManager.SaveScene(scene, ScenePath);
        // le menu est la 1re scène du jeu ; les autres scènes déjà listées gardent leur ordre
        var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("SAE501 : scène du menu construite (" + ScenePath + "), 1re scène du jeu.");
    }

    static Texture2D PrepareImage()
    {
        var ti = AssetImporter.GetAtPath(ImagePath) as TextureImporter;
        if (ti == null) { Debug.LogWarning("Image du menu introuvable : " + ImagePath); return null; }
        ti.textureType = TextureImporterType.Default;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.maxTextureSize = 2048;
        ti.mipmapEnabled = true;
        var android = ti.GetPlatformTextureSettings("Android");
        android.overridden = true; android.maxTextureSize = 2048; android.format = TextureImporterFormat.ASTC_6x6;
        ti.SetPlatformTextureSettings(android);
        ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(ImagePath);
    }

    static Material Mat(string name, string shader, Color c, Texture tex)
    {
        string p = $"{Dir}/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(p);
        if (m == null) { m = new Material(Shader.Find(shader)); AssetDatabase.CreateAsset(m, p); }
        else m.shader = Shader.Find(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        m.color = c;
        if (tex)
        {
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
            m.mainTexture = tex;
        }
        if (shader.Contains("Particles"))
        {
            m.SetFloat("_Surface", 1f);                                // transparent
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    // écran courbé (portion de cylindre) vu de l'intérieur, centré sur l'axe +Z
    static Mesh CurvedMesh(float radius, float arcDeg, float bottom, float top)
    {
        string p = $"{Dir}/Decor_Courbe.asset";
        const int seg = 40;
        var v = new Vector3[(seg + 1) * 2]; var uv = new Vector2[v.Length]; var tri = new int[seg * 6];
        for (int i = 0; i <= seg; i++)
        {
            float a = (-arcDeg / 2f + arcDeg * i / seg) * Mathf.Deg2Rad;
            var dir = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius;
            v[i * 2] = dir + Vector3.up * bottom; v[i * 2 + 1] = dir + Vector3.up * top;
            uv[i * 2] = new Vector2(i / (float)seg, 0f); uv[i * 2 + 1] = new Vector2(i / (float)seg, 1f);
            if (i < seg)
            {
                int b = i * 2, t = i * 6;
                tri[t] = b; tri[t + 1] = b + 1; tri[t + 2] = b + 2;
                tri[t + 3] = b + 1; tri[t + 4] = b + 3; tri[t + 5] = b + 2;
            }
        }
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(p);
        if (mesh == null) { mesh = new Mesh { name = "Decor_Courbe" }; AssetDatabase.CreateAsset(mesh, p); }
        mesh.Clear();
        mesh.vertices = v; mesh.uv = uv; mesh.triangles = tri;
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        return mesh;
    }

    // texture de brouillard douce (bruit de Perlin, bords transparents)
    static Texture2D MistTexture()
    {
        string p = $"{Dir}/Brouillard.asset";
        var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        if (t != null) return t;
        t = new Texture2D(256, 64, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 256; x++)
        {
            float n = Mathf.PerlinNoise(x * 0.025f, y * 0.06f) * 0.7f + Mathf.PerlinNoise(x * 0.07f + 13f, y * 0.15f) * 0.3f;
            float edge = Mathf.Sin(Mathf.PI * y / 63f) * Mathf.Clamp01(Mathf.Min(x, 255 - x) / 40f);
            t.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((n - 0.25f) * 1.6f) * edge));
        }
        t.Apply();
        AssetDatabase.CreateAsset(t, p);
        return t;
    }
}
