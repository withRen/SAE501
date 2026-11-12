using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// [#171] Vieux PC de décor sur le bureau vide du poste de surveillance (Bunker_Surveillance_Desk_West), demandé par Rahim le 12/11.
// Modèle fait pour le projet : Tools/blender/vieux_pc.py -> Assets/Decor/VieuxPC/Vieux_PC.fbx (écran cathodique, clavier, tour).
// L'écran affiche un terminal vert (Ecran_Terminal.png, émissif) et une petite lumière temps réel en sort (sans ombres, portée
// 1,4 m), qui vacille doucement (FlickerLamp). Ce n'est pas un des écrans de caméras retirés au journal 191 : aucune image de caméra.
// Appelé par RoomBuilder (après RaccordsJardin) et par le menu pour la scène ouverte, sans reconstruction.
public static class VieuxPC
{
    const string Dir = "Assets/Decor/VieuxPC";
    const string Nom = "Deco_Vieux_PC";

    [MenuItem("SAE501/Scène/Vieux PC sur le bureau")]
    static void Menu()
    {
        var root = GameObject.Find("Carte (Blender)");
        if (!root) { Debug.LogWarning("[VieuxPC] « Carte (Blender) » introuvable : ouvrir MainScene"); return; }
        if (Placer(root.transform)) { EditorSceneManager.MarkSceneDirty(root.scene); EditorSceneManager.SaveScene(root.scene); }
    }

    public static bool Placer(Transform carte)
    {
        var desk = carte.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r => r.name == "Bunker_Surveillance_Desk_West");
        if (!desk) { Debug.LogWarning("[VieuxPC] bureau Bunker_Surveillance_Desk_West introuvable"); return false; }
        var fbx = $"{Dir}/Vieux_PC.fbx";
        if (AssetImporter.GetAtPath(fbx) is ModelImporter mi && (!mi.generateSecondaryUV || mi.importAnimation || mi.materialImportMode != ModelImporterMaterialImportMode.None))
        {
            mi.generateSecondaryUV = true; mi.importAnimation = false; mi.importCameras = false; mi.importLights = false;
            mi.materialImportMode = ModelImporterMaterialImportMode.None;
            mi.SaveAndReimport();
        }
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
        var mf0 = model ? model.GetComponentInChildren<MeshFilter>() : null;
        if (!mf0) { Debug.LogWarning($"[VieuxPC] {fbx} introuvable : lancer Tools/blender/vieux_pc.py"); return false; }
        var mesh = mf0.sharedMesh;

        // ordre des emplacements du FBX : Coque, Sombre, Ecran, Led, Touche (voir le script Blender)
        var slots = new[] { "Coque", "Sombre", "Ecran", "Led", "Touche" };
        var mats = slots.Select(Materiau).ToArray();

        var old = carte.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == Nom);
        if (old) Object.DestroyImmediate(old.gameObject);

        var b = desk.bounds;
        var go = new GameObject(Nom);
        go.transform.SetParent(desk.transform.parent, false);
        // sur le plateau, un peu à droite du centre (la lanterne et la douille sont à gauche), écran vers les tabourets (+z)
        go.transform.SetPositionAndRotation(new Vector3(b.center.x + 0.2f, b.max.y, b.center.z - 0.07f), Quaternion.Euler(0f, -6f, 0f));
        go.layer = desk.gameObject.layer;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterials = mats;
        r.shadowCastingMode = ShadowCastingMode.On;
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        var col = go.AddComponent<BoxCollider>();
        col.center = mesh.bounds.center; col.size = mesh.bounds.size;

        // lumière de l'écran : verte, douce, sans ombres, portée courte (coût faible sur Quest)
        var lg = new GameObject("Lumiere_Ecran");
        lg.transform.SetParent(go.transform, false);
        lg.transform.localPosition = new Vector3(0f, 0.42f, 0.35f);       // au-dessus de l'axe de l'écran : pas de reflet en plein milieu
        var l = lg.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.45f, 1f, 0.6f);
        l.intensity = 0.35f;
        l.range = 1.4f;
        l.shadows = LightShadows.None;
        l.lightmapBakeType = LightmapBakeType.Realtime;
        l.renderMode = LightRenderMode.ForcePixel;
        var fl = go.AddComponent<FlickerLamp>();
        fl.lamp = l; fl.bulb = null; fl.baseIntensity = l.intensity;     // pas de bulb : FlickerLamp teinterait l'écran en orange
        fl.hum = 0.04f; fl.burstMinGap = 12f; fl.burstMaxGap = 30f; fl.dipDepth = 0.6f;

        Debug.Log($"[VieuxPC] vieux PC posé sur {desk.name} en {go.transform.position}");
        return true;
    }

    static Material Materiau(string slot)
    {
        switch (slot)
        {
            case "Coque": return AssetDatabase.LoadAssetAtPath<Material>("Assets/Room/Materials/SinkEnamel.mat");   // plastique beige jauni
            case "Sombre": return AssetDatabase.LoadAssetAtPath<Material>("Assets/Room/Materials/RadioBlack.mat");
            case "Touche": return Copie("VieuxPC_Touches", "Assets/Room/Materials/SinkEnamel.mat", m => m.SetColor("_BaseColor", new Color(0.78f, 0.74f, 0.64f)));
            case "Led": return Emissif("VieuxPC_Led", null, new Color(0.3f, 1f, 0.35f) * 2f);
            default: return Emissif("VieuxPC_Ecran", AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/Ecran_Terminal.png"), Color.white * 1.6f);
        }
    }

    static Material Copie(string nom, string src, System.Action<Material> regle)
    {
        var path = $"{Dir}/{nom}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m)
        {
            var s = AssetDatabase.LoadAssetAtPath<Material>(src);
            m = s ? new Material(s) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }
        regle(m); EditorUtility.SetDirty(m);
        return m;
    }

    static Material Emissif(string nom, Texture2D tex, Color emission)
    {
        return Copie(nom, null, m =>
        {
            m.shader = Shader.Find("Universal Render Pipeline/Lit");
            m.SetColor("_BaseColor", tex ? Color.white * 0.15f : new Color(0.05f, 0.2f, 0.05f));
            if (tex) { m.SetTexture("_BaseMap", tex); m.SetTextureScale("_BaseMap", new Vector2(-1f, 1f)); m.SetTextureOffset("_BaseMap", new Vector2(1f, 0f)); }   // UV du FBX inversées en x
            m.SetFloat("_Smoothness", 0.6f);                  // verre bombé : reflet doux
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission);
            if (tex) m.SetTexture("_EmissionMap", tex);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        });
    }
}
