using UnityEngine;
using UnityEngine.SceneManagement;

// Seul objet de la scène MainScene_PC : active la version PC puis charge la vraie scène du jeu (MainScene, inchangée).
public class DesktopBoot : MonoBehaviour
{
    public string scene = "MainScene";

    void Awake()
    {
        DesktopMode.Enable();
        SceneManager.LoadScene(scene);
    }
}
