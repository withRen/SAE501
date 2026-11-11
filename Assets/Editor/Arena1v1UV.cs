using UnityEditor;
using UnityEngine;

// [SAE501] UV de l'arène 1v1 : le modèle Blender (arena_1v1_bunker.fbx) n'a pas d'UV sur la moitié de ses maillages
// (contreforts, poutres, portes, gravats) et les autres ont des UV 0-1 par face. Sans UV à l'échelle, une texture de béton
// est soit absente soit étirée. À l'import, on recalcule l'UV0 de chaque maillage par projection cubique en mètres :
// chaque sommet prend les deux axes perpendiculaires à sa normale dominante, divisés par TailleTuile.
// L'UV2 (lightmap) est générée par Unity à part (generateSecondaryUV) et n'est pas touchée.
public class Arena1v1UV : AssetPostprocessor
{
    public const float TailleTuile = 2f;   // une répétition de texture tous les 2 m

    void OnPostprocessModel(GameObject root)
    {
        if (assetPath != Arena1v1Builder.FbxPath) return;
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            if (mf.sharedMesh) Projeter(mf.sharedMesh, mf.transform);
    }

    static void Projeter(Mesh m, Transform t)
    {
        var v = m.vertices;
        if (v.Length == 0) return;   // flaque dégénérée (tous ses polygones écartés par l'import)
        var n = m.normals;
        if (n == null || n.Length != v.Length) { m.RecalculateNormals(); n = m.normals; }
        var uv = new Vector2[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            // en mètres dans le repère de la scène (l'échelle des nœuds Blender est prise en compte)
            Vector3 p = t.TransformPoint(v[i]);
            Vector3 a = t.TransformDirection(n[i]);
            float ax = Mathf.Abs(a.x), ay = Mathf.Abs(a.y), az = Mathf.Abs(a.z);
            Vector2 q = ay >= ax && ay >= az ? new Vector2(p.x, p.z)
                      : ax >= az ? new Vector2(p.z, p.y)
                      : new Vector2(p.x, p.y);
            uv[i] = q / TailleTuile;
        }
        m.uv = uv;
        m.RecalculateTangents();   // nécessaires à la normal map
    }
}
