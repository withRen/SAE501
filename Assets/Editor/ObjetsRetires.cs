using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Objets de la carte retirés par Rahim (journal 174).
// La scène est générée par script : chaque « Build VR Scene » recrée tout ce qui vient du FBX, donc un objet supprimé à la main
// dans la scène revient à la reconstruction suivante. Ce fichier est la liste durable : un nom d'objet par ligne
// (Assets/Editor/objets_retires.txt, versionné), supprimé à chaque construction par RoomBuilder, juste après les retraits codés en dur.
//
// Menu SAE501 > Scène > Retirer la sélection (durable) : supprime les objets sélectionnés dans la scène ET ajoute leurs noms au fichier.
// Menu SAE501 > Scène > Ouvrir la liste des objets retirés : pour relire ou enlever une ligne (l'objet reviendra à la reconstruction suivante).
public static class ObjetsRetires
{
    public const string Fichier = "Assets/Editor/objets_retires.txt";

    public static HashSet<string> Lire()
    {
        var set = new HashSet<string>();
        if (!File.Exists(Fichier)) return set;
        foreach (var l in File.ReadAllLines(Fichier))
        {
            var n = l.Trim();
            if (n.Length > 0 && !n.StartsWith("#")) set.Add(n);
        }
        return set;
    }

    /// <summary>Supprime de la carte tous les objets de la liste (et leurs enfants). Rend le nombre d'objets retirés.</summary>
    public static int Appliquer(Transform root)
    {
        var noms = Lire();
        if (noms.Count == 0) return 0;
        int n = 0;
        foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(x => x != root && noms.Contains(x.name)).ToList())
            if (t) { Object.DestroyImmediate(t.gameObject); n++; }
        Debug.Log($"[ObjetsRetires] {n} objets retirés d'après {Fichier} ({noms.Count} noms)");
        return n;
    }

    [MenuItem("SAE501/Scène/Retirer la sélection (durable)")]
    static void RetirerSelection()
    {
        var sel = Selection.gameObjects.Where(g => g && g.scene.IsValid()).ToList();
        if (sel.Count == 0) { Debug.LogWarning("[ObjetsRetires] rien de sélectionné dans la scène"); return; }
        var noms = Lire();
        var ajout = new List<string>();
        foreach (var g in sel)
        {
            // un enfant « _LOD1 » suit son parent : on note le parent
            var n = g.name.EndsWith("_LOD1") && g.transform.parent ? g.transform.parent.name : g.name;
            if (noms.Add(n)) ajout.Add(n);
        }
        if (ajout.Count > 0)
        {
            if (!File.Exists(Fichier)) File.WriteAllText(Fichier, "# Objets de la carte retirés (un nom par ligne). Lu par RoomBuilder via ObjetsRetires.cs à chaque Build VR Scene.\n");
            File.AppendAllLines(Fichier, ajout);
            AssetDatabase.ImportAsset(Fichier);
        }
        foreach (var g in sel) if (g) Undo.DestroyObjectImmediate(g.name.EndsWith("_LOD1") && g.transform.parent ? g.transform.parent.gameObject : g);
        Debug.Log($"[ObjetsRetires] {sel.Count} objets supprimés, {ajout.Count} noms ajoutés à {Fichier}");
    }

    [MenuItem("SAE501/Scène/Ouvrir la liste des objets retirés")]
    static void Ouvrir()
    {
        if (!File.Exists(Fichier)) File.WriteAllText(Fichier, "# Objets de la carte retirés (un nom par ligne). Lu par RoomBuilder via ObjetsRetires.cs à chaque Build VR Scene.\n");
        AssetDatabase.ImportAsset(Fichier);
        AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(Fichier));
    }
}
