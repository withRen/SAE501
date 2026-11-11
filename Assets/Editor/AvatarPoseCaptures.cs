using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Captures de la tenue d'arme du personnage vu par l'autre joueur (PlayerAvatar.HoldGun), sans mode Play ni réseau :
// le personnage est posé dans Alvéole 17 (ouverte en plus de la scène courante, puis refermée) avec la pose qu'envoie
// un joueur en version PC (arme « vue FPS », DesktopPlayer.ItemPose), pour le pistolet, l'AK74 et le sniper,
// visée droite, vers le haut et vers le bas. Menu SAE501 → Arène 1v1 → Captures tenue d'arme.
public static class AvatarPoseCaptures
{
    public static string OutDir = "docs/images/avatar/";
    public static string ParamsFile;       // réglage sans recompiler : lignes « rifleHold.grip=0.1,-0.3,0.2 »
    public static bool Before;             // vrai : ancienne tenue (mains suivies) pour comparer

    [MenuItem("SAE501/Arène 1v1/Captures tenue d'arme")]
    public static void Run() => Run("", new[] { 0f, -30f, 30f });

    public static void Run(string only, float[] pitches)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Arena1v1_Bunker.unity", OpenSceneMode.Additive);
        try
        {
            var pistol = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Pistol>(true)).FirstOrDefault(p => p.name != "AK74" && p.name != "Sniper");
            var guns = new[] { ("Pistolet", pistol ? pistol.gameObject : null), ("AK74", Resources.Load<GameObject>("Weapons/AK74")), ("Sniper", Resources.Load<GameObject>("Weapons/Sniper")) };
            foreach (var (name, src) in guns)
                foreach (var pitch in pitches)
                    if (src && name.Contains(only)) Shot(scene, src, pitch, name + (Before ? "_avant" : "") + (pitch == 0f ? "" : pitch < 0f ? "_haut" : "_bas"));
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    static void Shot(UnityEngine.SceneManagement.Scene scene, GameObject src, float pitch, string file)
    {
        var root = new GameObject("Capture_Tenue");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
        var feet = new Vector3(13f, 0.1f, 0f); float yaw = -90f;
        Transform Node(string n) { var t = new GameObject(n).transform; t.SetParent(root.transform); return t; }
        var head = Node("Tete"); var left = Node("Main_G"); var right = Node("Main_D");
        head.SetPositionAndRotation(feet + Vector3.up * 1.65f, Quaternion.Euler(pitch, yaw, 0f));

        // version PC : arme « vue FPS » (DesktopPlayer.ItemPose), main gauche au repos sous le champ de vision
        var pistol = src.GetComponent<Pistol>(); var grab = src.GetComponent<XRGrabInteractable>();
        bool longGun = src.name == "AK74" || src.name == "Sniper";
        var gpos = head.TransformPoint(longGun ? new Vector3(0.14f, -0.25f, 0.2f) : new Vector3(0.15f, -0.16f, 0.36f));
        var muzzle = pistol && pistol.muzzle ? Quaternion.Inverse(src.transform.rotation) * pistol.muzzle.rotation : Quaternion.identity;
        var grot = head.rotation * Quaternion.Inverse(muzzle);
        var grip = grab && grab.attachTransform ? src.transform.InverseTransformPoint(grab.attachTransform.position) : Vector3.zero;
        var attachRot = grab && grab.attachTransform ? Quaternion.Inverse(src.transform.rotation) * grab.attachTransform.rotation : Quaternion.identity;
        right.SetPositionAndRotation(gpos + grot * Vector3.Scale(grip, src.transform.lossyScale), grot * attachRot);
        left.SetPositionAndRotation(head.TransformPoint(new Vector3(-0.25f, -0.55f, 0.05f)), head.rotation);

        // copie visuelle de l'arme dans la main droite (comme NetPlayer.VisualCopy)
        var prop = new GameObject("Tenu_" + src.name).transform;
        prop.SetParent(right, false); prop.localScale = src.transform.lossyScale;
        foreach (var mf in src.GetComponentsInChildren<MeshFilter>(false))
        {
            var mr = mf.GetComponent<MeshRenderer>(); if (!mr || !mr.enabled || !mf.sharedMesh) continue;
            var g = new GameObject(mf.name); g.transform.SetParent(prop, false);
            g.transform.localPosition = src.transform.InverseTransformPoint(mf.transform.position);
            g.transform.localRotation = Quaternion.Inverse(src.transform.rotation) * mf.transform.rotation;
            var ls = src.transform.lossyScale; var ms = mf.transform.lossyScale;
            g.transform.localScale = new Vector3(ms.x / ls.x, ms.y / ls.y, ms.z / ls.z);
            g.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh; g.AddComponent<MeshRenderer>().sharedMaterials = mr.sharedMaterials;
        }
        prop.SetPositionAndRotation(gpos, grot);
        var propPos = prop.localPosition; var propRot = prop.localRotation;

        var skin = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("Skins/Survivalist_2"), scene);
        skin.transform.SetParent(root.transform);
        skin.transform.SetPositionAndRotation(feet, Quaternion.Euler(0f, yaw, 0f));
        var av = skin.GetComponent<PlayerAvatar>(); var anim = skin.GetComponent<Animator>();
        av.Init();
        if (!string.IsNullOrEmpty(ParamsFile) && System.IO.File.Exists(ParamsFile))
            foreach (var line in System.IO.File.ReadAllLines(ParamsFile)) SetParam(av, line);
        av.headTarget = head; av.leftTarget = left; av.rightTarget = right; av.floorY = feet.y; av.rightHolding = true;
        for (int i = 0; i < 60; i++)
        {
            anim.Update(0.02f);
            prop.SetLocalPositionAndRotation(propPos, propRot);      // NetPlayer.ShowProp, chaque image
            av.HoldGun(Before ? null : prop, grip, muzzle);
            av.Solve(0.02f);
        }

        var cam = new GameObject("Cam").AddComponent<Camera>(); cam.transform.SetParent(root.transform); cam.nearClipPlane = 0.05f;
        var lt = new GameObject("Lumiere").AddComponent<Light>(); lt.transform.SetParent(root.transform);
        lt.type = LightType.Directional; lt.intensity = 1.2f; lt.transform.rotation = Quaternion.Euler(40f, 120f, 0f);
        var chest = feet + Vector3.up * 1.25f;
        // face de trois quarts, profil droit, profil gauche (main sur le garde-main), gros plan
        var views = new[] { new Vector3(-2.2f, 0.15f, 1.3f), new Vector3(-0.2f, 0.2f, 2.6f), new Vector3(-0.2f, 0.2f, -2.6f), new Vector3(-1.1f, 0.3f, -0.8f) };
        const int W = 1000, H = 900;
        var rt = new RenderTexture(W, H, 24); cam.targetTexture = rt; cam.aspect = (float)W / H;
        var tex = new Texture2D(W * views.Length, H, TextureFormat.RGB24, false);
        for (int v = 0; v < views.Length; v++)
        {
            cam.transform.position = chest + views[v];
            cam.fieldOfView = v == 3 ? 48f : 38f;
            cam.transform.LookAt(chest + new Vector3(-0.2f, v == 3 ? 0.2f : 0f, 0f));
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, W, H), v * W, 0);
        }
        tex.Apply(); RenderTexture.active = null; cam.targetTexture = null;
        System.IO.Directory.CreateDirectory(OutDir);
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(OutDir, file + ".png"), tex.EncodeToPNG());
        Object.DestroyImmediate(rt); Object.DestroyImmediate(tex); Object.DestroyImmediate(root);
        Debug.Log("[Tenue d'arme] " + file + " : poignée → bouche " + (pistol && pistol.muzzle ? src.transform.InverseTransformPoint(pistol.muzzle.position) - grip : Vector3.zero));
    }

    // « rifleHold.grip=0.1,-0.3,0.2 » ou « spineShare=0.5 »
    static void SetParam(PlayerAvatar av, string line)
    {
        var kv = line.Split('='); if (kv.Length != 2) return;
        object target = av; System.Reflection.FieldInfo fi = null;
        var path = kv[0].Trim().Split('.');
        for (int i = 0; i < path.Length; i++)
        {
            fi = target.GetType().GetField(path[i]); if (fi == null) return;
            if (i < path.Length - 1) target = fi.GetValue(target);
        }
        var n = kv[1].Split(',').Select(x => float.Parse(x.Trim(), System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        fi.SetValue(target, fi.FieldType == typeof(Vector3) ? (object)new Vector3(n[0], n[1], n[2]) : n[0]);
    }
}
