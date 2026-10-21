using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// Cinématique de fin rendue dans Blender (Tools/blender/cinematique_fin.py) : Blender ne sait pas reprendre les animations
// humanoïdes de Unity sur un autre squelette (Mixamo -> squelette UE des zombies et du personnage). On laisse Unity faire ce
// recalage (avatar Humanoid) : pour chaque couple modèle + clip, on échantillonne le clip image par image et on écrit la position
// et la rotation de chaque os, dans le repère du monde Unity, plus la pose de repos du modèle. Blender rejoue ces poses os par os.
// Écrit aussi les textures des matériaux (couleur, normales) pour que Blender habille les modèles comme dans le jeu.
// Menu : SAE501 > Cinématique > Exporter les poses pour Blender (fichiers dans Tools/blender/cinematique/).
public static class CinematicPoseExport
{
    public const string OutDir = "Tools/blender/cinematique";
    const int Fps = 30;

    // id, prefab du modèle, clips (fichier, nom court)
    static readonly (string id, string model, (string file, string name)[] clips)[] Jobs =
    {
        ("survivant", "Assets/Protection_suite/Prefab/Mesh_protective suit.prefab", new[]
        {
            ("Assets/Zombie/Animations/Zombie@Z_Run.FBX", "course"),
            ("Assets/Animations/Zombie3/Zombie Running.fbx", "sprint"),
        }),
        ("zombie", "Assets/NewPunch/ShirtlessZombieFree/Prefabs/ShirtlessZombie_FREE_URP.prefab", new[]
        {
            ("Assets/Animations/Zombie/Zombie Walk.fbx", "marche"),
            ("Assets/Animations/Zombie/Zombie Attack.fbx", "attaque"),
            ("Assets/Animations/Zombie2/Zombie Punching.fbx", "poings"),
            ("Assets/Animations/Zombie3/Zombie Running.fbx", "course"),
        }),
        ("cogneur", "Assets/ZombieMale_AAB/Prefabs/URP/ZombieMale_AAB_URP.prefab", new[]
        {
            ("Assets/Animations/Zombie2/Walking.fbx", "marche"),
            ("Assets/Animations/Zombie2/Zombie Punching.fbx", "poings"),
            ("Assets/Animations/Zombie/Zombie Attack.fbx", "attaque"),
            ("Assets/Animations/Zombie3/Zombie Running.fbx", "course"),
        }),
    };

    [MenuItem("SAE501/Cinématique/Exporter les poses pour Blender")]
    public static void ExportAll()
    {
        Directory.CreateDirectory(OutDir);
        foreach (var job in Jobs)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(job.model);
            if (prefab == null) { Debug.LogWarning("[Cinématique] modèle absent : " + job.model); continue; }
            WriteMaterials(job.id, job.model, prefab);
            foreach (var (file, name) in job.clips)
            {
                var clip = AssetDatabase.LoadAllAssetsAtPath(file).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview"));
                if (clip == null) { Debug.LogWarning("[Cinématique] clip absent : " + file); continue; }
                ExportClip(prefab, clip, Path.Combine(OutDir, $"{job.id}_{name}.json"));
            }
        }
        Debug.Log("[Cinématique] poses écrites dans " + OutDir);
    }

    static void ExportClip(GameObject prefab, AnimationClip clip, string path)
    {
        var go = Object.Instantiate(prefab);
        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var anim = go.GetComponentInChildren<Animator>();
        if (anim) anim.applyRootMotion = true;
        var smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
        var bones = go.GetComponentsInChildren<Transform>().Where(t => t != go.transform && t.GetComponent<Renderer>() == null).ToArray();

        var sb = new StringBuilder();
        sb.Append("{\"clip\":\"").Append(clip.name).Append("\",\"fps\":").Append(Fps)
          .Append(",\"length\":").Append(F(clip.length)).Append(",\"bones\":[")
          .Append(string.Join(",", bones.Select(b => "\"" + b.name + "\""))).Append("],\"rest\":");
        AppendPose(sb, bones);

        int n = Mathf.Max(2, Mathf.RoundToInt(clip.length * Fps) + 1);
        sb.Append(",\"frames\":[");
        AnimationMode.StartAnimationMode();
        try
        {
            for (int i = 0; i < n; i++)
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(go, clip, Mathf.Min(clip.length, i / (float)Fps));
                AnimationMode.EndSampling();
                if (i > 0) sb.Append(',');
                AppendPose(sb, bones);
            }
        }
        finally { AnimationMode.StopAnimationMode(); }
        sb.Append("]}");
        File.WriteAllText(path, sb.ToString());
        Object.DestroyImmediate(go);
        Debug.Log($"[Cinématique] {path} : {bones.Length} os, {n} images");
    }

    // pour chaque os : position (x, y, z) et rotation (x, y, z, w) dans le monde Unity
    static void AppendPose(StringBuilder sb, Transform[] bones)
    {
        sb.Append('[');
        for (int i = 0; i < bones.Length; i++)
        {
            var p = bones[i].position; var q = bones[i].rotation;
            if (i > 0) sb.Append(',');
            sb.Append('[').Append(F(p.x)).Append(',').Append(F(p.y)).Append(',').Append(F(p.z)).Append(',')
              .Append(F(q.x)).Append(',').Append(F(q.y)).Append(',').Append(F(q.z)).Append(',').Append(F(q.w)).Append(']');
        }
        sb.Append(']');
    }

    static void WriteMaterials(string id, string modelPath, GameObject prefab)
    {
        var lines = new List<string>();
        var fbx = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(r => r.sharedMesh).Where(m => m)
                        .Select(m => AssetDatabase.GetAssetPath(m)).FirstOrDefault();
        foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
        {
            if (!r.gameObject.activeSelf || !r.enabled) continue;
            foreach (var m in r.sharedMaterials.Where(m => m))
            {
                string Tex(string prop) => m.HasProperty(prop) && m.GetTexture(prop) ? Path.GetFullPath(AssetDatabase.GetAssetPath(m.GetTexture(prop))) : "";
                var c = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;
                var baseTex = Tex("_BaseMap"); if (baseTex == "") baseTex = Tex("_MainTex");
                lines.Add($"{{\"renderer\":\"{r.name}\",\"material\":\"{m.name}\",\"base\":\"{baseTex}\",\"normal\":\"{Tex("_BumpMap")}\",\"color\":[{F(c.r)},{F(c.g)},{F(c.b)}]}}");
            }
        }
        File.WriteAllText(Path.Combine(OutDir, id + "_modele.json"),
            $"{{\"fbx\":\"{(fbx != null ? Path.GetFullPath(fbx) : "")}\",\"materials\":[{string.Join(",", lines)}]}}");
    }

    static string F(float v) => v.ToString("0.#####", CultureInfo.InvariantCulture);
}
