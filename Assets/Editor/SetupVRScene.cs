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

        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) Object.DestroyImmediate(l.gameObject);

        var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XriDir + "Prefabs/XR Origin (XR Rig).prefab");
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(XriDir + "XRI Default Input Actions.inputactions");

        AddHand(rig, "Left Controller", "l", "XRI Left Interaction", actions);
        AddHand(rig, "Right Controller", "r", "XRI Right Interaction", actions);

        // même mode de suivi que dans "My project"
        rig.GetComponent<Unity.XR.CoreUtils.XROrigin>().RequestedTrackingOriginMode = Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Device;

        RoomBuilder.Populate(rig);

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

        var anim = hand.GetComponent<AnimateHandOnInput>() ?? hand.AddComponent<AnimateHandOnInput>();
        anim.handAnimator = hand.GetComponentInChildren<Animator>();
        anim.triggerValue = new InputActionProperty(Ref(actions, map, "Activate Value"));
        anim.gripValue = new InputActionProperty(Ref(actions, map, "Select Value"));
    }

    static InputActionReference Ref(InputActionAsset asset, string map, string action)
    {
        var path = AssetDatabase.GetAssetPath(asset);
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<InputActionReference>()
            .First(r => r.action != null && r.action.actionMap.name == map && r.action.name == action);
    }
}
