using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Portail de sortie du jardin (Garden_ExitGate_Left / _Right, journal 152).
// Dans la carte, les deux battants partagent le matériau MetalDark (noir uni, celui des écrans) et leurs UV sont
// incohérentes (barreaux écrasés ou étirés). Ici, sans toucher au FBX :
//   - UV refaites par projection sur les faces (en mètres) : u = largeur (1 m par répétition), v = hauteur du battant
//     (0 au pied, 1 en haut) ; la texture sait donc où est le sol : rouille et terre au pied, coulures vers le bas ;
//   - matériau propre au portail (Portail_Fer) : fer forgé peint en noir, peinture écaillée, rouille, coulures, terre et
//     mousse au pied. Textures 1024 générées par Tools/textures/generer_portail.py (ASTC 6x6 sur Quest, comme les
//     textures d'usure). Même nombre de triangles, un matériau de plus.
// Appelé par RoomBuilder (avant les LOD). Retire aussi le batching statique des battants et du cadenas (ils bougent en jeu : EscapeGate).
public static class PortailFer
{
    const string Dir = "Assets/Generated/Portail";
    static readonly string[] Leaves = { "Garden_ExitGate_Left", "Garden_ExitGate_Right" };
    const float Height = 2.9f;                         // hauteur d'un battant (m)

    public static void Apply(Transform root)
    {
        var a = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesUsure.Dir}/Portail_Fer_Albedo.png");
        var n = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesUsure.Dir}/Portail_Fer_Normal.png");
        var ms = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturesUsure.Dir}/Portail_Fer_MS.png");
        if (!a || !n || !ms) { Debug.LogWarning("[PortailFer] textures absentes : lancer Tools/textures/generer_portail.py"); return; }
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/Generated", "Portail");

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Portail_Fer" };
        mat.SetTexture("_BaseMap", a);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetTexture("_BumpMap", n);
        mat.SetFloat("_BumpScale", 1f);
        mat.EnableKeyword("_NORMALMAP");
        mat.SetTexture("_MetallicGlossMap", ms);
        mat.SetFloat("_Smoothness", 1f);
        mat.SetFloat("_SmoothnessTextureChannel", 0f);
        mat.EnableKeyword("_METALLICSPECGLOSSMAP");
        var mp = $"{Dir}/Portail_Fer.mat";
        AssetDatabase.DeleteAsset(mp);
        AssetDatabase.CreateAsset(mat, mp);

        int done = 0;
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true).Where(f => Leaves.Contains(f.name)))
        {
            var src = mf.sharedMesh;
            if (!src || !src.isReadable) continue;
            var mesh = Reproject(src, mf.transform);
            var p = $"{Dir}/{mf.name}.asset";
            AssetDatabase.DeleteAsset(p);
            AssetDatabase.CreateAsset(mesh, p);
            mf.sharedMesh = mesh;
            var mc = mf.GetComponent<MeshCollider>();
            if (mc && mc.sharedMesh == src) mc.sharedMesh = mesh;
            var r = mf.GetComponent<MeshRenderer>();
            r.sharedMaterials = Enumerable.Repeat(mat, src.subMeshCount).ToArray();
            done++;
        }
        Debug.Log($"[PortailFer] {done} battants : UV refaites, matériau Portail_Fer");
        Movable(root);
    }

    // [Évasion] le portail s'ouvre en jeu (EscapeGate) : battants et cadenas ne doivent pas être fusionnés par le batching statique
    static readonly string[] Moving = { "Garden_ExitGate_Left", "Garden_ExitGate_Right", "Garden_ExitGate_Padlock" };
    public static void Movable(Transform root)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true).Where(t => Moving.Contains(t.name)))
        {
            var f = GameObjectUtility.GetStaticEditorFlags(t.gameObject);
            GameObjectUtility.SetStaticEditorFlags(t.gameObject, f & ~StaticEditorFlags.BatchingStatic);
        }
    }

    // une copie par sommet de triangle (le maillage est petit : ~150 triangles) ; projection selon la normale du triangle
    static Mesh Reproject(Mesh src, Transform t)
    {
        var v = src.vertices; var uv2 = src.uv2;
        var M = t.localToWorldMatrix;
        // repère du battant : le pied est le point le plus bas, la largeur suit l'axe horizontal le plus long
        var w = v.Select(p => M.MultiplyPoint3x4(p)).ToArray();
        float yMin = w.Min(p => p.y);
        var verts = new List<Vector3>(); var uvs = new List<Vector2>(); var lm = new List<Vector2>();
        var subs = new List<int[]>();
        for (int s = 0; s < src.subMeshCount; s++)
        {
            var tri = src.GetTriangles(s);
            var outT = new int[tri.Length];
            for (int i = 0; i < tri.Length; i += 3)
            {
                Vector3 p0 = w[tri[i]], p1 = w[tri[i + 1]], p2 = w[tri[i + 2]];
                var nrm = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                for (int k = 0; k < 3; k++)
                {
                    var p = w[tri[i + k]];
                    float hgt = (p.y - yMin) / Height;
                    Vector2 uv;
                    if (Mathf.Abs(nrm.y) > 0.7f) uv = new Vector2(p.x + p.z, hgt);                 // dessus / dessous d'une traverse
                    else if (Mathf.Abs(nrm.x) > Mathf.Abs(nrm.z)) uv = new Vector2(p.z, hgt);       // face tournée vers ±x
                    else uv = new Vector2(p.x, hgt);                                                // face tournée vers ±z
                    outT[i + k] = verts.Count;
                    verts.Add(v[tri[i + k]]);
                    uvs.Add(uv);
                    if (uv2.Length > 0) lm.Add(uv2[tri[i + k]]);
                }
            }
            subs.Add(outT);
        }
        var m = new Mesh { name = src.name + "_Fer" };
        m.SetVertices(verts);
        m.SetUVs(0, uvs);
        m.subMeshCount = subs.Count;
        for (int s = 0; s < subs.Count; s++) m.SetTriangles(subs[s], s);
        m.RecalculateNormals();
        m.RecalculateTangents();
        m.RecalculateBounds();
        if (lm.Count == verts.Count) m.SetUVs(1, lm); else Unwrapping.GenerateSecondaryUVSet(m);
        return m;
    }
}
