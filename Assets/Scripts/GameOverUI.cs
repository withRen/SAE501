using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Écran noir progressif à la mort, texte "GAME OVER", puis relance de la scène.
public class GameOverUI : MonoBehaviour
{
    public Transform head;
    public TextMesh gameOverText;       // optionnel : TextMesh avec ta police (sinon écran noir seul)
    public float fadeDuration = 1.2f;
    public float restartDelay = 6f;     // temps avant rechargement de la scène
    public bool autoRestart = true;     // faux quand l'écran de fin (EndScreen, 6.5) propose de rejouer
    public string message = "VOUS ÊTES MORT\nLe signal n'a pas été envoyé...";

    Renderer black;

    void Start()
    {
        if (!head) head = Camera.main.transform;

        black = HeadOverlay.Create(head, "GameOverFade", null, new Color(0, 0, 0, 0), Camera.main.nearClipPlane + 0.03f, 4100);
        black.transform.localScale = Vector3.one * 6f;
        black.enabled = false;

        if (gameOverText)
        {
            gameOverText.transform.SetParent(head, false);
            gameOverText.transform.localPosition = new Vector3(0f, 0f, 1.5f);
            gameOverText.transform.localRotation = Quaternion.identity;
            gameOverText.text = message;
            var mr = gameOverText.GetComponent<MeshRenderer>();
            mr.material.renderQueue = 4200;   // dessiné au-dessus du noir
            mr.enabled = false;
        }

        var h = PlayerHealth.Instance;
        if (h) h.Died += OnDied;
    }

    void OnDestroy()
    {
        if (PlayerHealth.Instance) PlayerHealth.Instance.Died -= OnDied;
    }

    void OnDied() => StartCoroutine(Run());

    IEnumerator Run()
    {
        black.enabled = true;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            black.material.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }
        black.material.color = Color.black;

        if (gameOverText) gameOverText.GetComponent<MeshRenderer>().enabled = true;

        if (!autoRestart) yield break;
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
