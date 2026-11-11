using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Modèles détaillés du poste de surveillance (Tools/blender/poste_surveillance.py -> Assets/Decor/Surveillance) :
// remplacent les boîtes de la carte (étagère à écrans + enregistreur + clavier, radio de secours) et du 5e écran.
// Les modèles sont exportés à la place exacte des boîtes d'origine (repère de l'objet remplacé) : seul le maillage change.
// Matériaux : ceux de la carte, texturés par TexturesUsure (peinture usée, plastique, acier, chrome, caoutchouc).
public static class PosteSurveillance
{
    public const string Dir = "Assets/Decor/Surveillance";

    static readonly Dictionary<string, string> Communs = new()
    {
        { "Plastique", "Assets/Room/Materials/RadioBlack.mat" },
        { "Acier", "Assets/Room/Materials/HammerSteel.mat" },
        { "Chrome", "Assets/Room/Materials/Chrome.mat" },
        { "Caoutchouc", "Assets/Room/Materials/Rubber.mat" },
        { "Touche", "Assets/Room/Materials/SinkEnamel.mat" },      // touches du clavier : plastique beige jauni (émail usé)
    };

    // modèle -> matériau de la peinture
    static string Peinture(string modele) => modele switch
    {
        "Poste_Radio" => "Assets/Room/Materials/RadioBody.mat",
        "Poste_Moniteur" => "Assets/Generated/Dark.mat",
        _ => "Assets/Room/Materials/MetalDark.mat",
    };

    public static (Mesh mesh, Material[] mats) Modele(string nom)
    {
        string p = $"{Dir}/{nom}.fbx";
        if (AssetImporter.GetAtPath(p) is ModelImporter mi && (!mi.generateSecondaryUV || mi.importAnimation))
        {
            mi.generateSecondaryUV = true;      // UV de lightmap (éclairage précalculé)
            mi.importAnimation = false;
            mi.importCameras = false;
            mi.importLights = false;
            mi.SaveAndReimport();
        }
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
        var mf = go ? go.GetComponentInChildren<MeshFilter>() : null;
        if (mf == null) return (null, null);
        var noms = mf.GetComponent<MeshRenderer>().sharedMaterials.Select(m => m ? m.name : "").ToArray();
        var mats = noms.Select(n => AssetDatabase.LoadAssetAtPath<Material>(n == "Peint" ? Peinture(nom) : Communs.TryGetValue(n, out var c) ? c : Peinture(nom))).ToArray();
        return (mf.sharedMesh, mats);
    }

    // carte : étagère + enregistreur + clavier (un seul objet d'origine) et radio. Appelé avant la création des collisions de la carte.
    public static void RemplacerCarte(Transform carte)
    {
        int n = 0;
        var riser = Trouver(carte, "Bunker_CCTV_Riser_And_DVR");
        if (riser && Remplacer(riser.gameObject, "Poste_Meuble"))
        {
            n++;
            foreach (var nom in new[] { "Poste_Clavier", "Poste_Magnetoscope" })
            {
                var t = riser.Find(nom);
                var g = t ? t.gameObject : new GameObject(nom);
                g.transform.SetParent(riser, false);
                g.layer = riser.gameObject.layer;
                GameObjectUtility.SetStaticEditorFlags(g, GameObjectUtility.GetStaticEditorFlags(riser.gameObject));
                if (!g.GetComponent<MeshFilter>()) g.AddComponent<MeshFilter>();
                if (!g.GetComponent<MeshRenderer>()) g.AddComponent<MeshRenderer>();
                if (Remplacer(g, nom)) n++;
            }
        }
        var radio = Trouver(carte, "Bunker_Emergency_Radio");
        if (radio && Remplacer(radio.gameObject, "Poste_Radio")) n++;
        Debug.Log($"[PosteSurveillance] {n} modèles détaillés en place");
    }

    // 5e écran (RoomBuilder.BuildExtraScreen) : boîtier + pied en un seul modèle. Renvoie false si le modèle manque.
    public static bool Moniteur(Transform mon)
    {
        var (mesh, mats) = Modele("Poste_Moniteur");
        if (mesh == null) return false;
        var g = new GameObject("Boitier");
        g.transform.SetParent(mon, false);
        g.AddComponent<MeshFilter>().sharedMesh = mesh;
        g.AddComponent<MeshRenderer>().sharedMaterials = mats;
        return true;
    }

    static bool Remplacer(GameObject g, string nom)
    {
        var (mesh, mats) = Modele(nom);
        if (mesh == null) { Debug.LogWarning($"[PosteSurveillance] {Dir}/{nom}.fbx introuvable : boîte d'origine gardée"); return false; }
        g.GetComponent<MeshFilter>().sharedMesh = mesh;
        g.GetComponent<MeshRenderer>().sharedMaterials = mats;
        return true;
    }

    static Transform Trouver(Transform t, string nom)
    {
        if (t.name == nom) return t;
        foreach (Transform c in t) { var r = Trouver(c, nom); if (r) return r; }
        return null;
    }
}
