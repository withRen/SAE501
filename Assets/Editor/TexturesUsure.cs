using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Textures PBR « usure » des objets restés en aplat de couleur (poste de surveillance, radios, caisses, fûts, papiers…).
// Les textures (1024, raccordables) sont générées par Tools/textures/generer_usure.py à partir de textures CC0 de Poly Haven
// (rust_coarse_01, dirty_concrete, stone_01) : albedo par matériau (couleur d'origine + éclats, rouille, rayures, saleté),
// normal et métal/lissé partagés par famille (peinture, acier, plastique, papier).
// Appelé à la fin de l'import de la carte (RoomImport) et de la reconstruction de la scène (RoomBuilder),
// qui recréent ces matériaux en couleur unie : sans cet appel, les textures seraient perdues.
// Quest 3 : 1024 px max, ASTC 6x6 sur Android, mipmaps (pas de scintillement au loin).
public static class TexturesUsure
{
    public const string Dir = "Assets/Room/Textures/Usure";

    // matériau -> (albedo, famille)
    static readonly Dictionary<string, (string albedo, string famille)> Table = new()
    {
        // cadres des écrans CCTV, meuble, magnétoscope à LEDs et clavier, caméras (le portail du jardin a son matériau : PortailFer)
        { "Assets/Room/Materials/MetalDark.mat", ("MetalSombre", "Peinture") },
        { "Assets/Generated/Dark.mat", ("MetalSombre", "Peinture") },            // 5e écran, radio d'annonces
        { "Assets/Room/Materials/Mat_CCTV_CameraBody.mat", ("BoitierCamera", "Peinture") },
        // radios kaki
        { "Assets/Room/Materials/RadioBody.mat", ("RadioKaki", "Peinture") },
        { "Assets/Generated/Radio.mat", ("RadioKaki", "Peinture") },
        // caisses, fûts, portes
        { "Assets/Room/Materials/DoorMetal.mat", ("MetalBrun", "Peinture") },
        { "Assets/Room/Materials/DoorMetal.001.mat", ("MetalBrun", "Peinture") },
        { "Assets/Room/Materials/GeneratorPaint.mat", ("VertMoteur", "Peinture") },
        { "Assets/Room/Materials/FuelRed.mat", ("RougeCarburant", "Peinture") },
        { "Assets/Room/Materials/LampShade.mat", ("AbatJour", "Peinture") },
        { "Assets/Room/Materials/LampShade.001.mat", ("AbatJour", "Peinture") },
        { "Assets/Room/Materials/FridgeEnamel.mat", ("Email", "Peinture") },
        { "Assets/Room/Materials/SinkEnamel.mat", ("Email", "Peinture") },
        { "Assets/Room/Materials/Countertop.mat", ("PlanTravail", "Peinture") },
        // acier nu
        { "Assets/Generated/Metal.mat", ("Acier", "Acier") },                    // pieds, poignées, étagères générés
        { "Assets/Room/Materials/HammerSteel.mat", ("AcierSombre", "Acier") },
        { "Assets/Room/Materials/Chrome.mat", ("Chrome", "Acier") },
        { "Assets/Room/Materials/Chrome.001.mat", ("Chrome", "Acier") },
        // plastique et caoutchouc noirs
        { "Assets/Room/Materials/RadioBlack.mat", ("Plastique", "Plastique") },
        { "Assets/Room/Materials/Rubber.mat", ("Plastique", "Plastique") },
        { "Assets/Room/Materials/Rubber.001.mat", ("Plastique", "Plastique") },
        // papiers (cartes, feuilles au sol)
        { "Assets/Room/Materials/Paper.mat", ("Papier", "Papier") },
        { "Assets/Room/Materials/Paper.001.mat", ("Papier", "Papier") },
    };

    [MenuItem("SAE501/Salle/Appliquer les textures d'usure (PBR)")]
    public static void Appliquer()
    {
        if (!AssetDatabase.IsValidFolder(Dir)) { Debug.LogWarning($"[TexturesUsure] {Dir} absent : lancer Tools/textures/generer_usure.py"); return; }
        RéglerImports();
        int n = 0;
        foreach (var kv in Table)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(kv.Key);
            if (m == null) continue;
            var (albedo, famille) = kv.Value;
            var a = Tex($"Usure_{albedo}_Albedo.png");
            var nrm = Tex($"Usure_{famille}_Normal.png");
            var ms = Tex($"Usure_{famille}_MS.png") ?? Tex("Usure_Plastique_MS.png");    // papier : lissé du plastique (métal 0)
            if (a == null || nrm == null) continue;
            m.SetTexture("_BaseMap", a);
            m.SetColor("_BaseColor", Color.white);        // la couleur est dans l'albedo
            m.SetTexture("_BumpMap", nrm);
            m.SetFloat("_BumpScale", 1f);
            m.EnableKeyword("_NORMALMAP");
            m.SetTexture("_MetallicGlossMap", ms);
            m.SetFloat("_Smoothness", 1f);                // multiplie l'alpha de la carte métal/lissé
            m.SetFloat("_SmoothnessTextureChannel", 0f);  // lissé dans l'alpha de la carte métal
            m.EnableKeyword("_METALLICSPECGLOSSMAP");
            EditorUtility.SetDirty(m);
            n++;
        }

        // pierres du jardin : le modèle stone_01 avait sa normale mais pas sa couleur (albedo tiré de textures.zip)
        var pierre = AssetDatabase.LoadAssetAtPath<Material>("Assets/Room/Materials/stone_01.mat");
        var pa = Tex("Usure_Pierre_Albedo.png");
        if (pierre != null && pa != null)
        {
            pierre.SetTexture("_BaseMap", pa);
            pierre.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(pierre);
            n++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[TexturesUsure] {n} matériaux texturés");
    }

    static Texture2D Tex(string file) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/{file}");

    static void RéglerImports()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Dir }))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            var imp = (TextureImporter)AssetImporter.GetAtPath(p);
            bool normal = p.EndsWith("_Normal.png");
            bool masque = p.EndsWith("_MS.png");
            bool change = false;
            void Set<T>(T actuel, T voulu, System.Action<T> f) { if (!EqualityComparer<T>.Default.Equals(actuel, voulu)) { f(voulu); change = true; } }
            Set(imp.textureType, normal ? TextureImporterType.NormalMap : TextureImporterType.Default, v => imp.textureType = v);
            Set(imp.sRGBTexture, !normal && !masque, v => imp.sRGBTexture = v);
            Set(imp.maxTextureSize, 1024, v => imp.maxTextureSize = v);
            Set(imp.mipmapEnabled, true, v => imp.mipmapEnabled = v);
            Set(imp.wrapMode, TextureWrapMode.Repeat, v => imp.wrapMode = v);
            Set(imp.anisoLevel, 2, v => imp.anisoLevel = v);
            var android = imp.GetPlatformTextureSettings("Android");
            if (!android.overridden || android.format != TextureImporterFormat.ASTC_6x6 || android.maxTextureSize != 1024)
            {
                android.overridden = true;
                android.maxTextureSize = 1024;
                android.format = TextureImporterFormat.ASTC_6x6;
                imp.SetPlatformTextureSettings(android);
                change = true;
            }
            if (change) imp.SaveAndReimport();
        }
    }
}
