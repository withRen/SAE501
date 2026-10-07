using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class SetupVRScene
{
    const string XriDir = "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/";

    [MenuItem("SAE501/Build VR Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) Object.DestroyImmediate(cam.gameObject);

<<<<<<< HEAD
<<<<<<< HEAD
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(2, 1, 2);
=======
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) Object.DestroyImmediate(l.gameObject);
>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4
=======
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) Object.DestroyImmediate(l.gameObject);
>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4

        var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XriDir + "Prefabs/XR Origin (XR Rig).prefab");
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(XriDir + "XRI Default Input Actions.inputactions");

        AddHand(rig, "Left Controller", "l", "XRI Left Interaction", actions);
        AddHand(rig, "Right Controller", "r", "XRI Right Interaction", actions);

<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4
        // même mode de suivi que dans "My project"
        rig.GetComponent<Unity.XR.CoreUtils.XROrigin>().RequestedTrackingOriginMode = Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Device;

        RoomBuilder.Populate(rig);

<<<<<<< HEAD
>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4
=======
>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4
        var path = "Assets/Scenes/MainScene.unity";
        EditorSceneManager.SaveScene(scene, path);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
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

<<<<<<< HEAD
<<<<<<< HEAD
=======
        UseDirectInteractor(controller);

>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4
=======
        UseDirectInteractor(controller);

>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4
        var anim = hand.GetComponent<AnimateHandOnInput>() ?? hand.AddComponent<AnimateHandOnInput>();
        anim.handAnimator = hand.GetComponentInChildren<Animator>();
        anim.triggerValue = new InputActionProperty(Ref(actions, map, "Activate Value"));
        anim.gripValue = new InputActionProperty(Ref(actions, map, "Select Value"));
    }

<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4
    // Même configuration que "My project" : on attrape uniquement à portée de main.
    // - le Near-Far Interactor des Starter Assets (rayon qui attrape de loin, objet tenu à distance, joystick qui
    //   approche/tourne l'objet tenu) est désactivé ;
    // - à la place, un XRDirectInteractor : sphère de 15 cm autour de la main, point d'accroche dans la paume
    //   (valeurs de My project : sphère centrée en (0 ; -0,05 ; 0,05), accroche en (0 ; -0,07 ; 0,04)).
    // Les boutons sont repris du Near-Far Interactor (grip = attraper, gâchette = utiliser).
    static void UseDirectInteractor(Transform controller)
    {
        var nearFar = controller.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor>(true);

        var go = new GameObject("Direct Interactor");
        go.transform.SetParent(controller, false);
        var sc = go.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.15f;
        sc.center = new Vector3(0f, -0.05f, 0.05f);
        var direct = go.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
        var attach = new GameObject("Attach").transform;
        attach.SetParent(go.transform, false);
        attach.localPosition = new Vector3(0f, -0.07f, 0.04f);
        direct.attachTransform = attach;
        direct.improveAccuracyWithSphereCollider = true;
        direct.physicsTriggerInteraction = QueryTriggerInteraction.Ignore;
        direct.keepSelectedTargetValid = true;

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
            nearFar.gameObject.SetActive(false);
        }
    }

<<<<<<< HEAD
>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4
=======
>>>>>>> 6ff67b85a7cb6a20d032da0311c78e65fe42e8f4
    static InputActionReference Ref(InputActionAsset asset, string map, string action)
    {
        var path = AssetDatabase.GetAssetPath(asset);
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<InputActionReference>()
            .First(r => r.action != null && r.action.actionMap.name == map && r.action.name == action);
    }
}
