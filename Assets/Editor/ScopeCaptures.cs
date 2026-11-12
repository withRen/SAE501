using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Captures de la lunette du sniper (SniperScope), sans mode Play ni casque, dans les deux arènes du duel :
//   - casque : la vue de l'œil droit (champ de 90°, comme un œil du Quest 3) l'œil à 7,5 cm de l'oculaire, dans l'axe puis décalé
//     de 1,5 cm (ombre de la pupille), et la lunette au repos vue de côté (verre sombre, caméra éteinte) ;
//   - PC : la vue normale et la vue zoomée au clic droit (champ de vision ÷ grossissement) avec l'image de la lunette plein écran
//     de DesktopHud posée par-dessus (même texture, composée ici à la main : l'affichage uGUI n'est pas rendu hors mode Play).
// Une cible (personnage du duel) est posée au point d'apparition de l'adversaire. Menu SAE501 → Arène 1v1 → Captures lunette.
public static class ScopeCaptures
{
    public static string OutDir = "docs/images/lunette/";

    // ligne de commande : pose la lunette sur le prefab puis fait les captures
    public static void Batch() { DuelWeaponBuilder.AddScope(); AssetDatabase.Refresh(); Run(); }

    [MenuItem("SAE501/Arène 1v1/Captures lunette")]
    public static void Run()
    {
        foreach (var map in ArenaMaps.All) Map(map);
        Debug.Log("[Lunette] captures dans " + OutDir);
    }

    static void Map(ArenaMaps.Map map)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + map.scene + ".unity", OpenSceneMode.Additive);
        var root = new GameObject("Capture_Lunette");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
        try
        {
            string tag = map.scene == "Arena1v1_Quai" ? "quai9" : "alveole17";
            // tireur : tête à l'apparition du joueur 1 ; cible : un personnage debout dans la plus longue ligne de vue dégagée
            // (le centre des deux cartes est fermé par un obstacle)
            var headPos = map.spawn1 + Vector3.up * 1.6f;
            Physics.SyncTransforms();
            ShaderUtil.allowAsyncCompilation = false;          // sinon les premiers rendus montrent des matériaux gris (shaders en cours de compilation)
            Vector3 feet = map.spawn2; float best = 0f;
            float Free(float a)
            {
                var dir = Quaternion.Euler(0f, map.yaw1 + a, 0f) * Vector3.forward;
                return Physics.Raycast(headPos, dir, out var hit, 60f, ~0, QueryTriggerInteraction.Ignore) ? hit.distance : 60f;
            }
            for (float a = -70f; a <= 70f; a += 2f)
            {
                var dir = Quaternion.Euler(0f, map.yaw1 + a, 0f) * Vector3.forward;
                // ligne dégagée aussi un peu à gauche et à droite : la cible n'est pas collée au bord d'un obstacle
                float d = Mathf.Min(Free(a), Mathf.Min(Free(a - 3f), Free(a + 3f)));
                d = Mathf.Min(d - 1f, 30f);
                var p = headPos + dir * d;
                if (d > best && Physics.Raycast(p, Vector3.down, out var floor, 3f, ~0, QueryTriggerInteraction.Ignore)) { best = d; feet = floor.point; }
            }
            Debug.Log($"[Lunette] {map.scene} : cible à {best:F1} m");
            var skinSrc = Resources.Load<GameObject>("Skins/Survivalist_2");
            if (skinSrc)
            {
                var skin = (GameObject)PrefabUtility.InstantiatePrefab(skinSrc, scene);
                skin.transform.SetParent(root.transform);
                var back = headPos - feet; back.y = 0f;
                skin.transform.SetPositionAndRotation(feet, Quaternion.LookRotation(back, Vector3.up));
                var anim = skin.GetComponent<Animator>();
                if (anim) for (int i = 0; i < 10; i++) anim.Update(0.05f);
            }
            var target = feet + Vector3.up * 1.45f;

            var headRot = Quaternion.LookRotation(target - headPos, Vector3.up);
            var right = headRot * Vector3.right;
            var eye = headPos + right * 0.032f;

            var gun = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("Weapons/Sniper"), scene);
            gun.transform.SetParent(root.transform);
            var scope = gun.GetComponent<SniperScope>();
            if (!scope) { Debug.LogError("[Lunette] SniperScope absent du prefab (menu SAE501 → Arène 1v1 → Ajouter la lunette au sniper)"); return; }
            scope.EditorSetup();
            // l'oculaire 7,5 cm devant l'œil droit, l'axe de la lunette sur la ligne œil → cible
            var aim = Quaternion.LookRotation(target - eye, Vector3.up);
            gun.transform.rotation = aim;
            var ep = gun.transform.TransformPoint(scope.eyepiece);
            gun.transform.position += eye + (aim * Vector3.forward) * scope.eyeRelief - ep;

            var cam = new GameObject("Oeil").AddComponent<Camera>();
            cam.transform.SetParent(root.transform);
            cam.nearClipPlane = 0.01f; cam.farClipPlane = 300f;
            cam.clearFlags = CameraClearFlags.Skybox;

            // 1. casque, œil dans l'axe
            var eyeOff = Vector3.zero;
            bool on = scope.EyeAtScope(headPos, headRot, ref eyeOff);
            scope.ForceScoped(true, eyeOff);
            Shot(cam, eye, aim, 90f, 1024, 1024, "casque_lunette_" + tag, null);
            Shot(cam, eye, aim, 36f, 1024, 1024, "casque_lunette_gros_plan_" + tag, null);
            Debug.Log($"[Lunette] {tag} : activée {on}, œil {eyeOff}, champ de la caméra de lunette {ScopeFov(scope):F2}°");

            // 2. casque, tête décalée de 1,5 cm vers la droite : ombre de la pupille
            var headPos2 = headPos + right * 0.015f;
            eyeOff = Vector3.zero;
            on = scope.EyeAtScope(headPos2, headRot, ref eyeOff);
            scope.ForceScoped(true, eyeOff);
            Shot(cam, eye + right * 0.015f, aim, 90f, 1024, 1024, "casque_oeil_decale_" + tag, null);
            Debug.Log($"[Lunette] {tag} : œil décalé de 1,5 cm, activée {on}, œil {eyeOff}");

            // 3. tête à 30 cm : la lunette ne s'active pas (verre sombre), vue de trois quarts arrière
            eyeOff = Vector3.zero;
            on = scope.EyeAtScope(headPos - (aim * Vector3.forward) * 0.3f, headRot, ref eyeOff);
            scope.ForceScoped(false, Vector3.zero);
            var side = gun.transform.TransformPoint(scope.eyepiece + new Vector3(0.16f, 0.07f, -0.22f));
            Shot(cam, side, Quaternion.LookRotation(gun.transform.TransformPoint(scope.eyepiece + new Vector3(0f, -0.02f, 0.12f)) - side, Vector3.up), 50f, 1024, 768, "casque_lunette_repos_" + tag, null);
            Debug.Log($"[Lunette] {tag} : tête à 30 cm, activée {on} (attendu : False)");

            // 4. PC : vue normale (70°) puis zoom au clic droit avec la lunette plein écran ; l'arme est cachée pendant le zoom
            scope.SetPcHidden(true);
            float baseFov = 70f;
            float zoom = 2f * Mathf.Atan(Mathf.Tan(baseFov * 0.5f * Mathf.Deg2Rad) / scope.magnification) * Mathf.Rad2Deg;
            Shot(cam, headPos, headRot, baseFov, 1920, 1080, "pc_vue_normale_" + tag, null);
            Shot(cam, headPos, headRot, zoom, 1920, 1080, "pc_lunette_" + tag, DesktopHud.ScopeTexture(1024, true));
            Debug.Log($"[Lunette] {tag} : PC, champ {baseFov}° → {zoom:F1}° (×{scope.magnification})");
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }

    static float ScopeFov(SniperScope s)
    {
        var c = s.GetComponentInChildren<Camera>(true);
        return c ? c.fieldOfView : 0f;
    }

    static void Shot(Camera cam, Vector3 pos, Quaternion rot, float fov, int w, int h, string file, Texture2D overlay)
    {
        cam.transform.SetPositionAndRotation(pos, rot);
        cam.fieldOfView = fov;
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt; cam.aspect = (float)w / h;
        cam.Render(); cam.Render();          // le 1er rendu peut encore montrer des matériaux gris (shaders en cours de préparation)
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        RenderTexture.active = null; cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        if (overlay)
        {
            // comme DesktopHud : texture carrée étirée sur l'écran (uvRect), bord noir prolongé sur les côtés
            float aspect = (float)w / h;
            var px = tex.GetPixels();
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float u = 0.5f - aspect * 0.5f + aspect * (x + 0.5f) / w, v = (y + 0.5f) / h;
                    var o = overlay.GetPixelBilinear(Mathf.Clamp01(u), v);
                    int i = y * w + x;
                    px[i] = Color.Lerp(px[i], new Color(o.r, o.g, o.b, 1f), o.a);
                }
            tex.SetPixels(px);
        }
        tex.Apply();
        Write(tex, file);
    }

    static void Write(Texture2D tex, string file)
    {
        System.IO.Directory.CreateDirectory(OutDir);
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(OutDir, file + ".jpg"), tex.EncodeToJPG(88));
        Object.DestroyImmediate(tex);
    }
}
