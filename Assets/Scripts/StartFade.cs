using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Fondu depuis le noir au début de chaque partie (casque et version PC, aussi après « rejouer ») :
// l'écran reste noir un court instant (le temps que la scène se pose), puis s'éclaircit.
// Rien à placer dans la scène : il s'installe tout seul dans toute scène qui contient un WaveManager.
public class StartFade : MonoBehaviour
{
    public float hold = 0.3f;           // s de noir complet
    public float duration = 1.4f;       // s pour revenir à l'image

    Renderer black;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Install()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.sceneLoaded += OnLoaded;
    }

    static void OnLoaded(Scene s, LoadSceneMode m)
    {
        if (m != LoadSceneMode.Single || !Object.FindFirstObjectByType<WaveManager>()) return;
        new GameObject("FonduDebut").AddComponent<StartFade>();
    }

    IEnumerator Start()
    {
        var cam = Camera.main;
        if (!cam) { Destroy(gameObject); yield break; }
        // même voile que le fondu du menu et de l'écran de fin (HeadOverlay, ordre 4100) : les textes collés à la tête restent devant
        black = HeadOverlay.Create(cam.transform, "FonduDebut_Voile", null, Color.black, cam.nearClipPlane + 0.03f, 4100);
        black.transform.localScale = Vector3.one * 6f;
        // temps réel : le fondu continue même si le jeu est mis en pause tout de suite
        for (float t = 0f; t < hold; t += Time.unscaledDeltaTime) yield return null;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float a = 1f - Mathf.SmoothStep(0f, 1f, t / duration);
            black.material.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }
        Destroy(black.gameObject);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (black) Destroy(black.gameObject);
    }
}
