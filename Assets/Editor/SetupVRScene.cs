using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public static class SetupVRScene
{
    const string XriDir = "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/";

    [MenuItem("SAE501/Build VR Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) Object.DestroyImmediate(cam.gameObject);

        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) Object.DestroyImmediate(l.gameObject);

        var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XriDir + "Prefabs/XR Origin (XR Rig).prefab");
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(XriDir + "XRI Default Input Actions.inputactions");

        foreach (var cim in rig.GetComponentsInChildren<ControllerInputActionManager>(true)) cim.smoothTurnEnabled = SmoothTurn;
        AddHand(rig, "Left Controller", "l", "XRI Left Interaction", actions);
        AddHand(rig, "Right Controller", "r", "XRI Right Interaction", actions);

        // déplacement : seulement marcher (joystick gauche) et tourner (joystick droit). Les autres modes des Starter Assets
        // (saut sur A, escalade, « grab move » qui déplace le joueur quand on serre le grip dans le vide, téléportation sans zone)
        // ne servent pas et faisaient passer le joueur à travers le sol ou par-dessus les obstacles.
        foreach (var t in rig.GetComponentsInChildren<Transform>(true))
            if (t.name == "Grab Move" || t.name == "Jump" || t.name == "Climb" || t.name == "Climb Teleport" || t.name == "Teleportation")
                t.gameObject.SetActive(false);
        var cc = rig.GetComponent<CharacterController>();
        if (cc)
        {
            cc.radius = 0.2f;          // 0,1 m avec 8 cm de marge : collisions instables contre les murs fins et les caisses
            cc.skinWidth = 0.02f;
            cc.stepOffset = 0.3f;      // marche franchissable (0,5 m : on montait sur les caisses et les tonneaux)
            cc.minMoveDistance = 0f;
        }
        rig.AddComponent<PlayerFallGuard>();                       // jamais sous la carte

        // même mode de suivi que dans "My project"
        rig.GetComponent<Unity.XR.CoreUtils.XROrigin>().RequestedTrackingOriginMode = Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Device;

        RoomBuilder.Populate(rig);

        // la carte fait moins de 50 m de diagonale : inutile de calculer l'affichage jusqu'à 1 km
        var head = rig.GetComponentInChildren<Camera>(true);
        if (head)
        {
            head.farClipPlane = 60f;
            // pas de post-traitement ni de tramage : le bruit du tramage se voyait (grésillement) dans une scène aussi sombre
            var ucd = head.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>()
                      ?? head.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            ucd.renderPostProcessing = false;
            ucd.dithering = false;
        }

        var path = "Assets/Scenes/MainScene.unity";
        EditorSceneManager.SaveScene(scene, path);
        // le menu d'accueil (SAE501 > Build Menu Scene) reste la 1re scène s'il existe, puis le jeu
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        if (System.IO.File.Exists(MenuBuilder.ScenePath)) scenes.Add(new EditorBuildSettingsScene(MenuBuilder.ScenePath, true));
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("SAE501: scene built");
    }

    static void AddHand(GameObject rig, string controllerName, string side, string map, InputActionAsset actions)
    {
        var controller = rig.GetComponentsInChildren<Transform>(true).First(t => t.name == controllerName);
        // hide the default controller model
        foreach (var r in controller.GetComponentsInChildren<Renderer>(true)) r.enabled = false;

        var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            $"Assets/Animated Hands/Prefabs/{(side == "l" ? "Left" : "Right")} Hand Model.prefab");
        var hand = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, controller);
        hand.name = (side == "l" ? "Left" : "Right") + " Hand Model";
        // même placement que dans "My project" : 10 cm sous la manette, tourné de ±90° sur Z
        hand.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        hand.transform.localRotation = Quaternion.Euler(0f, 0f, side == "l" ? 90f : -90f);
        hand.transform.localScale = Vector3.one;

        UseDirectInteractor(controller, side == "l" ? -1f : 1f);

        var anim = hand.GetComponent<AnimateHandOnInput>() ?? hand.AddComponent<AnimateHandOnInput>();
        anim.handAnimator = hand.GetComponentInChildren<Animator>();
        anim.triggerValue = new InputActionProperty(Ref(actions, map, "Activate Value"));
        anim.gripValue = new InputActionProperty(Ref(actions, map, "Select Value"));
    }

    // Rotation fluide (demande de l'équipe) au lieu des crans du Starter Assets.
    // Remarque : le document de la SAÉ recommande la rotation par crans pour le confort -> remettre false pour revenir aux crans.
    const bool SmoothTurn = true;

    // Même configuration que "My project" : on attrape uniquement à portée de main.
    // - le Near-Far Interactor des Starter Assets (rayon qui attrape de loin, objet tenu à distance, joystick qui
    //   approche/tourne l'objet tenu) est désactivé ;
    // - à la place, un XRDirectInteractor : sphère de 15 cm autour de la main, point d'accroche dans la paume
    //   (valeurs de My project : sphère centrée en (0 ; -0,05 ; 0,05), accroche en (0 ; -0,07 ; 0,04)).
    // Les boutons sont repris du Near-Far Interactor (grip = attraper, gâchette = utiliser) ; prise par clic (ClickGrab).
    static void UseDirectInteractor(Transform controller, float sideSign)
    {
        var nearFar = controller.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>(true);

        var go = new GameObject("Direct Interactor");
        go.transform.SetParent(controller, false);
        var sc = go.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.15f;
        sc.center = new Vector3(0.02f * sideSign, -0.12f, 0.03f);   // autour de la main (le modèle est 10 cm sous la manette)
        var direct = go.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
        var attach = new GameObject("Attach").transform;
        attach.SetParent(go.transform, false);
        // point d'accroche AU CENTRE DU POING FERMÉ du modèle de main (mesuré : jointures de l'index à -0,106 m, du majeur à -0,13 m,
        // côté pouce/jointures à ±0,05 m). Avec la valeur de My project (-0,07 m), la crosse flottait au-dessus du poing :
        // notre modèle de main est placé plus bas sous la manette.
        attach.localPosition = new Vector3(0.015f * sideSign, -0.135f, 0.025f);
        direct.attachTransform = attach;
        direct.improveAccuracyWithSphereCollider = true;
        direct.physicsTriggerInteraction = QueryTriggerInteraction.Ignore;
        direct.keepSelectedTargetValid = true;
        go.AddComponent<ClickGrab>();                               // un clic pour prendre, un clic pour lâcher

        if (nearFar != null)
        {
            var src = new SerializedObject(nearFar);
            var dst = new SerializedObject(direct);
            foreach (var prop in new[] { "m_SelectInput", "m_ActivateInput" })
            {
                var sp = src.FindProperty(prop);
                if (sp != null) dst.CopyFromSerializedProperty(sp);
            }
            dst.ApplyModifiedPropertiesWithoutUndo();
            // Le ControllerInputActionManager des Starter Assets RÉACTIVE le Near-Far à la fin d'un geste de téléportation
            // (joystick poussé puis relâché) : on retrouvait alors un rayon qui attrape de loin, en plus de la main
            // (2 objets dans la même main). On coupe sa référence et on supprime le Near-Far pour de bon.
            foreach (var cim in controller.root.GetComponentsInChildren<ControllerInputActionManager>(true))
            {
                var so = new SerializedObject(cim);
                var nf = so.FindProperty("m_NearFarInteractor");
                if (nf != null && nf.objectReferenceValue == nearFar) { nf.objectReferenceValue = null; so.ApplyModifiedPropertiesWithoutUndo(); }
            }
            Object.DestroyImmediate(nearFar.gameObject);
        }
    }

    static InputActionReference Ref(InputActionAsset asset, string map, string action)
    {
        var path = AssetDatabase.GetAssetPath(asset);
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<InputActionReference>()
            .First(r => r.action != null && r.action.actionMap.name == map && r.action.name == action);
    }
}
