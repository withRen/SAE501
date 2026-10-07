using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// [6.5] Écran de fin, style « terminal de bunker » : défaite (rouge) ou victoire (vert).
// Bande de titre, gros titre, sous-titre, tableau du bilan (score, vague, zombies abattus, temps), bouton « rejouer ».
//   - Défaite : GameOverUI fait le fondu au noir ; son message devient le sous-titre et son texte est masqué.
//   - Victoire : fondu sombre.
//   - Rejouer : gâchette d'une des deux manettes (ou relance automatique après `autoRestartAfter` s).
public class EndScreen : MonoBehaviour
{
    public static EndScreen Instance { get; private set; }

    public Transform head;
    public float showDelay = 2.5f;          // après la mort / la victoire
    public float inputDelay = 2f;           // la gâchette n'est prise en compte qu'après ce délai
    public float autoRestartAfter = 45f;
    public string victorySubtitle = "Le générateur est en marche. Le signal est parti.";

    bool shown, ended, triggerWasDown = true;
    float shownAt;
    Renderer buttonBg;
    TextMesh buttonText;

    void Awake() => Instance = this;
    void OnDestroy() { if (Instance == this) Instance = null; UiKit.SetOnTop(false); }

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        if (PlayerHealth.Instance) PlayerHealth.Instance.Died += OnDied;
    }

    void OnDied()
    {
        if (ended) return;
        ended = true;
        if (ScoreManager.Instance) ScoreManager.Instance.Stop();
        StartCoroutine(Show(false));
    }

    void Update()
    {
        var wm = WaveManager.Instance;
        if (!ended && wm && wm.Victory) { ended = true; StartCoroutine(Show(true)); }

        if (!shown) return;
        float a = 0.55f + 0.45f * Mathf.Sin(Time.time * 3.5f);     // le bouton clignote doucement
        UiKit.SetAlpha(buttonText, a);
        UiKit.SetAlpha(buttonBg, 0.1f + 0.12f * a);

        bool down = VRInput.AnyTrigger();
        bool pressed = down && !triggerWasDown;
        triggerWasDown = down;
        if (Time.time - shownAt < inputDelay) return;
        if (pressed || Time.time - shownAt > autoRestartAfter) Restart();
    }

    IEnumerator Show(bool victory)
    {
        yield return new WaitForSeconds(showDelay);

        string subtitle = victorySubtitle;
        if (victory)
        {
            var black = HeadOverlay.Create(head, "VictoireFondu", null, new Color(0, 0, 0, 0), Camera.main.nearClipPlane + 0.03f, 4100);
            black.transform.localScale = Vector3.one * 6f;
            for (float t = 0; t < 1f; t += Time.deltaTime) { black.material.color = new Color(0, 0, 0, 0.85f * t); yield return null; }
        }
        else
        {
            // le texte de mort de l'équipe devient le sous-titre du panneau
            var go = FindFirstObjectByType<GameOverUI>();
            if (go)
            {
                var lines = go.message.Split('\n');
                subtitle = lines.Length > 1 ? lines[1] : "";
                if (go.gameOverText) go.gameOverText.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        var sm = ScoreManager.Instance;
        var wm = WaveManager.Instance;
        int total = wm ? wm.levels.Length : 4;
        int lvl = Mathf.Max(1, WaveManager.Level);
        int secs = sm ? Mathf.FloorToInt(sm.Duration) : 0;
        var rows = new[]
        {
            ("SCORE", (sm ? sm.Score : 0).ToString()),
            (victory ? "VAGUES SURVÉCUES" : "VAGUE ATTEINTE", lvl + " / " + total),
            ("ZOMBIES ABATTUS", (sm ? sm.Kills : 0) + "  (" + (sm ? sm.Headshots : 0) + " tête)"),
            (victory ? "TEMPS D'ÉVACUATION" : "TEMPS DE SURVIE", (secs / 60).ToString("00") + ":" + (secs % 60).ToString("00")),
        };

        Build(victory ? UiKit.Green : UiKit.Red,
              victory ? "BUNKER-OS  //  ÉVACUATION" : "BUNKER-OS  //  SIGNAL PERDU",
              victory ? "VOUS AVEZ ÉCHAPPÉ" : "VOUS ÊTES MORT",
              subtitle, rows, victory ? UiKit.PanelGreen : UiKit.PanelRed);
        UiKit.SetOnTop(true);
        shown = true;
        shownAt = Time.time;
        triggerWasDown = true;   // il faut relâcher puis appuyer
    }

    void Build(Color acc, string header, string title, string subtitle, (string, string)[] rows, Color panelCol)
    {
        var root = UiKit.HeadRoot(head, "EcranFin", 1.2f, 0.8f);
        var size = new Vector2(1.07f, 0.84f);
        UiKit.Quad(root, "Panneau", Vector2.zero, size, panelCol, UiKit.QPanel);
        UiKit.Scanlines(root, Vector2.zero, size, UiKit.QPanel + 1);
        UiKit.Frame(root, Vector2.zero, size, 0.003f, new Color(acc.r, acc.g, acc.b, 0.4f), UiKit.QLine);
        UiKit.Brackets(root, Vector2.zero, size + new Vector2(0.02f, 0.02f), 0.05f, 0.006f, acc, UiKit.QLine);

        var ink = new Color(0.02f, 0.05f, 0.03f);
        UiKit.Quad(root, "Bande", new Vector2(0f, 0.391f), new Vector2(size.x, 0.059f), new Color(acc.r, acc.g, acc.b, 0.9f), UiKit.QLine);
        UiKit.Text(root, "Bande_G", new Vector2(-0.51f, 0.391f), 0.0062f, ink, header, TextAnchor.MiddleLeft, true, UiKit.QText);
        UiKit.Text(root, "Bande_D", new Vector2(0.51f, 0.391f), 0.0062f, ink, "FIN DE TRANSMISSION", TextAnchor.MiddleRight, true, UiKit.QText);

        UiKit.Text(root, "Titre", new Vector2(0f, 0.262f), 0.0155f, acc, title, TextAnchor.MiddleCenter, true, UiKit.QText);
        UiKit.Text(root, "SousTitre", new Vector2(0f, 0.175f), 0.0068f, UiKit.Dim, subtitle, TextAnchor.MiddleCenter, false, UiKit.QText);
        UiKit.Quad(root, "Separateur", new Vector2(0f, 0.12f), new Vector2(0.907f, 0.003f), new Color(acc.r, acc.g, acc.b, 0.5f), UiKit.QLine);

        for (int i = 0; i < rows.Length; i++)
        {
            float y = 0.065f - i * 0.077f;
            UiKit.Text(root, "Intitule", new Vector2(-0.413f, y), 0.0072f, UiKit.Dim, rows[i].Item1, TextAnchor.MiddleLeft, false, UiKit.QText);
            UiKit.Text(root, "Valeur", new Vector2(0.413f, y), i == 0 ? 0.0088f : 0.0078f, i == 0 ? acc : UiKit.White, rows[i].Item2, TextAnchor.MiddleRight, true, UiKit.QText);
            UiKit.Quad(root, "Ligne", new Vector2(0f, y - 0.04f), new Vector2(0.827f, 0.0015f), new Color(0.3f, 0.3f, 0.3f, 0.55f), UiKit.QLine);
        }

        buttonBg = UiKit.Button(root, new Vector2(0f, -0.347f), new Vector2(0.68f, 0.075f), "GÂCHETTE  POUR  REJOUER", 0.0068f, UiKit.Green, out buttonText)[0];
    }

    void Restart()
    {
        shown = false;
        UiKit.SetOnTop(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    [ContextMenu("Test : écran de victoire")] void TestWin() { if (!ended) { ended = true; StartCoroutine(Show(true)); } }
    [ContextMenu("Test : écran de défaite")] void TestLose() => OnDied();
}
