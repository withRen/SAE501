using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Version PC (clavier + souris) : petite scène de lancement MainScene_PC qui active DesktopMode puis charge MainScene.
// MainScene et le rig VR ne sont pas modifiés ; la scène PC n'est pas ajoutée au build Quest.
public static class DesktopSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/MainScene_PC.unity";

    [MenuItem("SAE501/Build PC Scene")]
    public static void Build()
    {
        var active = EditorSceneManager.GetActiveScene().path;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = new GameObject("Camera").AddComponent<Camera>();      // une image noire le temps du chargement
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        new GameObject("DesktopBoot").AddComponent<DesktopBoot>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("SAE501: PC scene built (" + ScenePath + ")");
        if (!string.IsNullOrEmpty(active) && active != ScenePath) EditorSceneManager.OpenScene(active);
    }

    // ouvre la scène PC et lance le mode Play (la scène du jeu, dans les Build Settings, est chargée par DesktopBoot)
    [MenuItem("SAE501/Play PC Version")]
    public static void Play()
    {
        if (!System.IO.File.Exists(ScenePath)) Build();
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath);
        EditorApplication.isPlaying = true;
    }
}
