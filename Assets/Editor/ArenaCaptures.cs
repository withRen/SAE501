using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// [SAE501] Captures des cartes du duel 1v1 (ArenaMaps) : vue d'ensemble en coupe (plafond masqué), plan vu de dessus, vue du joueur 1
// au départ → docs/images/arenes/<scène>_<vue>.png. La vue d'ensemble sert aussi d'aperçu dans le lobby (Resources/ArenaPreviews/<scène>.png,
// 640 × 360). Menu SAE501 → Arènes 1v1 → Captures. Rendu par une caméra temporaire (aucune modification des scènes).
public static class ArenaCaptures
{
    const string DocDir = "docs/images/arenes";
    const string PreviewDir = "Assets/Resources/ArenaPreviews";

    struct View { public string name; public Vector3 pos; public Vector3 euler; public bool ortho; public float size; public bool cut; }

    static View[] Views(string scene)
    {
        switch (scene)
        {
            case "Arena1v1_Quai":
                return new[]
                {
                    new View { name = "ensemble", pos = new Vector3(-4f, 21f, -27f), euler = new Vector3(40f, 8f, 0f), size = 60f },
                    new View { name = "plan", pos = new Vector3(0f, 40f, 0f), euler = new Vector3(90f, 0f, 0f), ortho = true, size = 15f },
                    new View { name = "joueur", pos = new Vector3(-16.5f, 1.6f, 4.2f), euler = new Vector3(4f, 105f, 0f), size = 80f },
                };
            default:
                return new[]
                {
                    new View { name = "ensemble", pos = new Vector3(0f, 24f, -23f), euler = new Vector3(48f, 0f, 0f), size = 60f, cut = true },
                    new View { name = "plan", pos = new Vector3(0f, 30f, 0f), euler = new Vector3(90f, 0f, 0f), ortho = true, size = 13.6f, cut = true },
                    new View { name = "joueur", pos = new Vector3(-12.2f, 1.7f, 3.2f), euler = new Vector3(5f, 100f, 0f), size = 80f },
                };
        }
    }

    [MenuItem("SAE501/Arènes 1v1/Captures (docs et aperçus du lobby)")]
    public static void CaptureAll()
    {
        var previous = EditorSceneManager.GetActiveScene().path;
        foreach (var m in ArenaMaps.All) Capture(m.scene);
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
    }

    /// <summary>Lobby, mode duel, carte <paramref name="map"/> choisie (vu par l'hôte) : construit le menu hors mode Play dans la scène Menu,
    /// dans une scène vide temporaire jamais enregistrée → docs/images/arenes/Lobby_selecteur_carte.png.</summary>
    public static void CaptureLobby(int map)
    {
        // scène vide temporaire (jamais enregistrée) : le menu y est construit seul, sur fond sombre
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var menu = new GameObject("Menu").AddComponent<MainMenu>();
        menu.EditorPreviewDuelLobby(map);
        // états visuels des boutons (choisi, éteint) : normalement posés par leur Update en jeu
        var update = typeof(MenuButton).GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        foreach (var b in menu.GetComponentsInChildren<MenuButton>(true)) update.Invoke(b, null);
        foreach (var r in menu.GetComponentsInChildren<Renderer>(true))
            if (r.GetComponent<TextMesh>()) r.sharedMaterial.renderQueue = 3100;
        var t = menu.transform;
        var target = t.TransformPoint(new Vector3(0.42f, 0.02f, 0f));
        var pos = target - t.forward * 2.25f + Vector3.up * 0.05f;
        var v = new View { name = "lobby", pos = pos, euler = Quaternion.LookRotation(target - pos).eulerAngles, size = 62f };
        var tex = Render(v, 1920, 1080);
        Directory.CreateDirectory(DocDir);
        File.WriteAllBytes($"{DocDir}/Lobby_selecteur_carte.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    public static void Capture(string scene)
    {
        string path = "Assets/Scenes/" + scene + ".unity";
        if (!File.Exists(path)) { Debug.LogWarning("[Captures] scène absente : " + path); return; }
        EditorSceneManager.OpenScene(path);
        Directory.CreateDirectory(DocDir);
        foreach (var v in Views(scene))
        {
            var tex = Render(v, 1920, 1080);
            File.WriteAllBytes($"{DocDir}/{scene}_{v.name}.png", tex.EncodeToPNG());
            if (v.name == "ensemble")
            {
                ArenaKit.Folder(PreviewDir);
                var small = Render(v, 640, 360);
                string p = $"{PreviewDir}/{scene}.png";
                File.WriteAllBytes(p, small.EncodeToPNG());
                Object.DestroyImmediate(small);
                AssetDatabase.ImportAsset(p);
                var imp = (TextureImporter)AssetImporter.GetAtPath(p);
                imp.textureType = TextureImporterType.Default;
                imp.mipmapEnabled = true;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.maxTextureSize = 1024;
                imp.alphaSource = TextureImporterAlphaSource.None;
                var android = imp.GetPlatformTextureSettings("Android");
                android.overridden = true; android.maxTextureSize = 1024; android.format = TextureImporterFormat.ASTC_6x6;
                imp.SetPlatformTextureSettings(android);
                imp.SaveAndReimport();
            }
            Object.DestroyImmediate(tex);
        }
        Debug.Log("[Captures] " + scene + " : " + DocDir);
    }

    static Texture2D Render(View v, int w, int h)
    {
        var go = new GameObject("__Capture");
        go.transform.SetPositionAndRotation(v.pos, Quaternion.Euler(v.euler));
        var cam = go.AddComponent<Camera>();
        cam.orthographic = v.ortho;
        if (v.ortho) cam.orthographicSize = v.size; else cam.fieldOfView = v.size;
        cam.nearClipPlane = 0.05f; cam.farClipPlane = 200f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.03f, 0.05f);
        var ucd = go.AddComponent<UniversalAdditionalCameraData>();
        ucd.renderPostProcessing = false;

        // vue en coupe : plafond, poutres et néons masqués le temps du rendu
        var hidden = new System.Collections.Generic.List<Renderer>();
        if (v.cut)
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                string n = r.name;
                bool roof = n.StartsWith("Ceiling") || n.StartsWith("HVAC") || n.StartsWith("Fixture") || n.Contains("Plafond") || n.Contains("Neon") || n == "Ciel" || (r.transform.parent && r.transform.parent.name == "Plafond_Fusionne");
                if (roof && r.enabled) { r.enabled = false; hidden.Add(r); }
            }
        // le ciel suit la tête du joueur en jeu : on le centre sur la caméra de capture
        var sky = GameObject.Find("Ciel");
        Vector3 skyPos = sky ? sky.transform.position : Vector3.zero;
        if (sky) sky.transform.position = v.pos;

        // vues de dessus et d'ensemble d'une carte fermée sans lune : lumière d'appoint le temps du rendu, pour la lisibilité du plan
        // (la vue « joueur » garde l'éclairage réel du jeu)
        Light fill = null;
        if ((v.name == "ensemble" || v.name == "plan") && !System.Array.Exists(Object.FindObjectsByType<Light>(FindObjectsSortMode.None), l => l.type == LightType.Directional && l.isActiveAndEnabled))
        {
            fill = new GameObject("__Appoint").AddComponent<Light>();
            fill.type = LightType.Directional; fill.intensity = 0.9f; fill.color = new Color(0.8f, 0.88f, 1f);
            fill.transform.rotation = Quaternion.Euler(62f, 30f, 0f);
        }

        var rt = new RenderTexture(w, h, 24) { antiAliasing = 4 };
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;

        foreach (var r in hidden) r.enabled = true;
        if (fill) Object.DestroyImmediate(fill.gameObject);
        if (sky) sky.transform.position = skyPos;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(go);
        return tex;
    }
}
