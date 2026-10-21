using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Fin de partie : écran noir progressif, titre, puis relance de la scène.
// [#122] Sous le titre, le bilan de la partie (GameStats) : zombies éliminés, % de tirs à la tête, planches posées,
// niveau atteint, temps total. Affiché à la mort ET à la victoire (générateur activé, appelé par WaveManager).
// On peut relancer plus tôt avec une gâchette (après minReadTime), sinon la scène se recharge au bout de restartDelay.
// [#121] Mort au niveau 2 ou plus : la gâchette (ou l'attente) reprend au début du niveau atteint ; maintenir B recommence au niveau 1.
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    public Transform head;
    public TextMesh gameOverText;       // optionnel : TextMesh avec ta police (sinon écran noir seul)
    public float fadeDuration = 1.2f;
    public float restartDelay = 15f;    // temps avant rechargement de la scène (le bilan se lit)
    public float minReadTime = 3f;      // la gâchette ne relance pas avant (évite de passer le bilan en tirant)
    public float victoryDelay = 4f;     // victoire : le joueur voit la salle se rallumer avant l'écran de fin
    public float restartHold = 1f;      // [#121] maintenir B ce temps : recommencer au niveau 1 au lieu de reprendre
    public string message = "VOUS ÊTES MORT\nLe signal n'a pas été envoyé...";
    public string victoryMessage = "VOUS AVEZ ÉCHAPPÉ\nLe générateur est en marche.";
    public Color deathColor = new Color(0.8f, 0.1f, 0.1f);
    public Color victoryColor = new Color(1f, 0.8f, 0.35f);
    public Color summaryColor = new Color(0.85f, 0.85f, 0.8f);

    Renderer black;
    TextMesh summary;
    bool running;

    void Awake() => Instance = this;

    void Start()
    {
        if (!head) head = Camera.main.transform;

        black = HeadOverlay.Create(head, "GameOverFade", null, new Color(0, 0, 0, 0), Camera.main.nearClipPlane + 0.03f, 4100);
        black.transform.localScale = Vector3.one * 6f;
        black.enabled = false;

        if (gameOverText)
        {
            gameOverText.transform.SetParent(head, false);
            gameOverText.transform.localPosition = new Vector3(0f, 0.42f, 1.5f);
            gameOverText.transform.localRotation = Quaternion.identity;
            gameOverText.text = message;
            var mr = gameOverText.GetComponent<MeshRenderer>();
            mr.material.renderQueue = 4200;   // dessiné au-dessus du noir
            mr.enabled = false;

            // bilan : même police, plus petit, sous le titre
            var go = new GameObject("TexteBilan");
            go.transform.SetParent(head, false);
            go.transform.localPosition = new Vector3(0f, -0.22f, 1.5f);
            summary = go.AddComponent<TextMesh>();
            summary.font = gameOverText.font;
            summary.fontSize = 64;
            summary.characterSize = 0.012f;
            summary.lineSpacing = 1.15f;
            summary.anchor = TextAnchor.MiddleCenter;
            summary.alignment = TextAlignment.Center;
            summary.color = summaryColor;
            var smr = go.GetComponent<MeshRenderer>();
            smr.sharedMaterial = gameOverText.font.material;
            smr.material.renderQueue = 4200;
            smr.enabled = false;
        }

        var h = PlayerHealth.Instance;
        if (h) h.Died += OnDied;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (PlayerHealth.Instance) PlayerHealth.Instance.Died -= OnDied;
    }

    // mode survie : pas de reprise au niveau atteint (on repart de la vague 1, le record compte)
    void OnDied() { if (!running) StartCoroutine(Run(message, deathColor, 0f, !WaveManager.Endless && WaveManager.Level >= 2 ? WaveManager.Level : 0)); }

    public void ShowVictory() { if (!running) StartCoroutine(Run(victoryMessage, victoryColor, victoryDelay, 0)); }

    // resumeLevel : niveau proposé à la reprise (0 : pas de reprise, on recommence au niveau 1)
    IEnumerator Run(string title, Color titleColor, float delay, int resumeLevel)
    {
        running = true;
        if (delay > 0f) yield return new WaitForSeconds(delay);
        black.enabled = true;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            black.material.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }
        black.material.color = Color.black;

        if (gameOverText)
        {
            gameOverText.text = title;
            gameOverText.color = titleColor;
            gameOverText.GetComponent<MeshRenderer>().enabled = true;
        }
        if (summary)
        {
            summary.text = "— BILAN —\n" + GameStats.Summary(WaveManager.Level, ZombieLevels.MaxLevel)
                         + (resumeLevel > 0 ? "\n\nGâchette : reprendre au niveau " + resumeLevel + "\nMaintenir B : recommencer au niveau 1" : "\n\nGâchette : rejouer");
            summary.GetComponent<MeshRenderer>().enabled = true;
        }

        // relance : gâchette (après minReadTime) ou au bout de restartDelay
        WaveManager.ResumeLevel = resumeLevel;           // par défaut (gâchette ou attente) : reprise si elle est proposée
        float held = 0f;
        for (float t = 0; t < restartDelay; t += Time.unscaledDeltaTime)
        {
            if (t >= minReadTime && VRInput.AnyTrigger()) break;
            if (resumeLevel > 0 && t >= minReadTime && VRInput.RightB())
            {
                held += Time.unscaledDeltaTime;
                if (held >= restartHold) { WaveManager.ResumeLevel = 0; break; }
            }
            else held = 0f;
            yield return null;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
