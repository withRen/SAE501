using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Playlists de musique tirées du dossier Assets/Audio/Music : une playlist = tous les fichiers dont le nom commence par
// son préfixe (Musique_Principale_1, _2, _3...), triés par numéro. Pour ajouter une musique au jeu, il suffit de déposer
// Musique_Principale_3.mp3 dans le dossier puis SAE501 > Mettre à jour les musiques : pas besoin de reconstruire la scène
// (ce qui effacerait l'éclairage cuit).
public static class MusicLibrary
{
    public const string Folder = "Assets/Audio/Music";
    public const string Game = "Musique_Principale";
    public const string Menu = "Musique_Menu";

    public static AudioClip[] Load(string prefix)
    {
        var paths = AssetDatabase.FindAssets("t:AudioClip", new[] { Folder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => Path.GetFileNameWithoutExtension(p).StartsWith(prefix))
            .OrderBy(Number).ThenBy(p => p)
            .ToList();
        var clips = new List<AudioClip>();
        foreach (var p in paths)
        {
            // musique longue : lue en streaming (pas chargée entière en mémoire sur le Quest), compressée en Vorbis
            if (AssetImporter.GetAtPath(p) is AudioImporter ai)
            {
                var st = ai.defaultSampleSettings;
                if (st.loadType != AudioClipLoadType.Streaming || st.compressionFormat != AudioCompressionFormat.Vorbis)
                {
                    st.loadType = AudioClipLoadType.Streaming;
                    st.compressionFormat = AudioCompressionFormat.Vorbis;
                    st.quality = 0.6f;
                    ai.defaultSampleSettings = st;
                    ai.SaveAndReimport();
                }
            }
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
            if (c) clips.Add(c);
        }
        if (clips.Count == 0) Debug.LogWarning($"Aucune musique « {prefix}* » dans {Folder}");
        return clips.ToArray();
    }

    // numéro en fin de nom (Musique_Principale_10 après _2) ; sans numéro : en premier
    static int Number(string path)
    {
        var m = Regex.Match(Path.GetFileNameWithoutExtension(path), @"(\d+)$");
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }

    public static MusicPlayer Create(string name, string prefix, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent) go.transform.SetParent(parent, false);
        var mp = go.AddComponent<MusicPlayer>();
        mp.playlist = prefix;
        mp.tracks = Load(prefix);
        return mp;
    }

    // remet à jour les lecteurs de la scène ouverte avec le contenu actuel du dossier
    [MenuItem("SAE501/Mettre à jour les musiques")]
    public static void Refresh()
    {
        foreach (var mp in Object.FindObjectsByType<MusicPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (string.IsNullOrEmpty(mp.playlist)) mp.playlist = mp.name == "Musique_Menu" ? Menu : Game;
            Undo.RecordObject(mp, "Mettre à jour les musiques");
            mp.tracks = Load(mp.playlist);
            EditorUtility.SetDirty(mp);
            EditorSceneManager.MarkSceneDirty(mp.gameObject.scene);
            Debug.Log($"{mp.name} : {mp.tracks.Length} piste(s) — " + string.Join(", ", mp.tracks.Select(t => t.name)));
        }
    }
}
