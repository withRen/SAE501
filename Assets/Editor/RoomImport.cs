using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Importe la salle "LastBoardStanding_Kitchen.blend" (exportée en FBX par Blender) avec des matériaux URP.
public static class RoomImport
{
    const string Dir = "Assets/Room";
    const string TexDir = Dir + "/Textures";
    const string MatDir = Dir + "/Materials";
    public const string FbxPath = Dir + "/LastBoardStanding_Room.fbx";

    [System.Serializable] class MatInfo
    {
        public float[] base_color; public float rough, metal, alpha; public string blend;
        public float[] tiling; public string base_tex, normal_tex, emission_tex; public float emission_strength; public bool alpha_linked;
    }

    [MenuItem("SAE501/Salle/1 - Importer la salle (textures + matériaux)")]
    public static void Import()
    {
        var json = File.ReadAllText(Dir + "/materials.json");
        var mats = MiniJson.ParseDict<MatInfo>(json);

        // 1) textures : cartes normales en mode Normal Map
        var normals = new HashSet<string>();
        foreach (var kv in mats) if (!string.IsNullOrEmpty(kv.Value.normal_tex)) normals.Add(kv.Value.normal_tex);
        AssetDatabase.StartAssetEditing();
        foreach (var f in Directory.GetFiles(TexDir, "*.png"))
        {
            var p = f.Replace("\\", "/");
            var ti = (TextureImporter)AssetImporter.GetAtPath(p);
            if (ti == null) continue;
            bool isNormal = normals.Contains(Path.GetFileName(p));
            ti.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            ti.maxTextureSize = 1024;
            ti.mipmapEnabled = true;
            ti.alphaIsTransparency = !isNormal;
            // Quest : compression ASTC (4 à 8 fois moins de mémoire et de bande passante qu'une texture non compressée)
            ti.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android", overridden = true, maxTextureSize = 1024,
                format = isNormal ? TextureImporterFormat.ASTC_5x5 : TextureImporterFormat.ASTC_6x6,
                textureCompression = TextureImporterCompression.Compressed
            });
            ti.SaveAndReimport();
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();

        // 2) matériaux URP
        if (!AssetDatabase.IsValidFolder(MatDir)) AssetDatabase.CreateFolder(Dir, "Materials");
        var made = new Dictionary<string, Material>();
        foreach (var kv in mats)
        {
            var i = kv.Value;
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = kv.Key };
            var bc = i.base_color;
            Texture2D bt = Tex(i.base_tex);
            m.SetColor("_BaseColor", bt ? Color.white : new Color(bc[0], bc[1], bc[2], 1f));
            if (bt)
            {
                m.SetTexture("_BaseMap", bt);
                m.SetTextureScale("_BaseMap", new Vector2(i.tiling[0], i.tiling[1]));
            }
            var nt = Tex(i.normal_tex);
            if (nt) { m.SetTexture("_BumpMap", nt); m.SetTextureScale("_BumpMap", new Vector2(i.tiling[0], i.tiling[1])); m.EnableKeyword("_NORMALMAP"); }
            m.SetFloat("_Metallic", Mathf.Clamp01(i.metal));
            m.SetFloat("_Smoothness", Mathf.Clamp01(1f - i.rough));

            bool transparent = i.alpha_linked || i.blend == "BLEND" || i.alpha < 0.99f;
            if (transparent)
            {
                m.SetFloat("_Surface", 1f);
                m.SetFloat("_Blend", 0f);
                m.SetFloat("_SrcBlend", 5f);
                m.SetFloat("_DstBlend", 10f);
                m.SetFloat("_ZWrite", 0f);
                m.SetFloat("_Cull", 0f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)RenderQueue.Transparent;
                if (!i.alpha_linked) m.SetColor("_BaseColor", new Color(bc[0], bc[1], bc[2], i.alpha));
            }
            var et = Tex(i.emission_tex);
            if (et && i.emission_strength > 0f)
            {
                m.SetTexture("_EmissionMap", et);
                m.SetColor("_EmissionColor", Color.white * i.emission_strength);
                m.EnableKeyword("_EMISSION");
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            AssetDatabase.CreateAsset(m, $"{MatDir}/{kv.Key}.mat");
            made[kv.Key] = m;
        }
        AssetDatabase.SaveAssets();

        // 3) le FBX utilise ces matériaux
        var mi = (ModelImporter)AssetImporter.GetAtPath(FbxPath);
        mi.globalScale = 1f;
        mi.useFileScale = true;
        mi.importCameras = false;
        mi.importLights = false;
        mi.isReadable = true;       // le builder lit les maillages (marteau)
        foreach (var kv in made)
            mi.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), kv.Key), kv.Value);
        mi.SaveAndReimport();
        Debug.Log($"[RoomImport] {made.Count} matériaux, FBX réimporté");
    }

    static Texture2D Tex(string file) => string.IsNullOrEmpty(file) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexDir}/{file}");
}

// JSON {nom: objet} -> dictionnaire (JsonUtility ne gère pas les dictionnaires)
public static class MiniJson
{
    public static Dictionary<string, T> ParseDict<T>(string json)
    {
        var res = new Dictionary<string, T>();
        var s = new Scanner(json);
        s.Expect('{');
        while (true)
        {
            s.Skip();
            if (s.Peek() == '}') break;
            var key = s.ReadString();
            s.Skip(); s.Expect(':'); s.Skip();
            res[key] = JsonUtility.FromJson<T>(s.ReadObject());
            s.Skip();
            if (s.Peek() == ',') s.Next();
        }
        return res;
    }

    class Scanner
    {
        readonly string t; int p;
        public Scanner(string text) { t = text; }
        public char Peek() => t[p];
        public void Next() => p++;
        public void Skip() { while (char.IsWhiteSpace(t[p])) p++; }
        public void Expect(char c) { Skip(); if (t[p] != c) throw new System.Exception("JSON: attendu " + c + " à " + p); p++; }
        public string ReadString() { Expect('"'); int s = p; while (t[p] != '"') p++; var r = t.Substring(s, p - s); p++; return r; }
        public string ReadObject()
        {
            int s = p, depth = 0; bool inStr = false;
            for (; p < t.Length; p++)
            {
                char c = t[p];
                if (c == '"' && t[p - 1] != '\\') inStr = !inStr;
                if (inStr) continue;
                if (c == '{') depth++;
                else if (c == '}') { depth--; if (depth == 0) { p++; break; } }
            }
            return t.Substring(s, p - s);
        }
    }
}
