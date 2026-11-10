using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Fin de partie, dans le même style que le menu d'accueil et le menu pause (AmberMenu, « terminal de bunker » ambre).
// À la mort (ou à la victoire, appelé par WaveManager) : la salle s'assombrit (fondu noir collé à la tête), puis un panneau
// s'affiche devant le joueur avec le titre, le bilan de la partie (GameStats) et des boutons visés au rayon de la manette
// (MenuPointer), validés à la gâchette — comme dans le menu pause :
//   mort au niveau 2 ou plus (campagne) : REPRENDRE · NIVEAU n  /  RECOMMENCER  /  MENU
//   sinon (niveau 1, mode infini, victoire) : REJOUER  /  MENU
// [#121] La reprise passe par WaveManager.ResumeLevel. Pendant l'écran de fin le temps est figé (comme la pause).
// L'ancien TextMesh « gameOverText » n'est plus utilisé (il est masqué) ; plus de relance automatique : le joueur choisit.
public class GameOverUI : AmberMenu
{
    public static GameOverUI Instance { get; private set; }

    public Transform head;
    public TextMesh gameOverText;       // ancien texte de fin : masqué (gardé pour ne pas casser la scène)
    public float fadeDuration = 1.2f;
    [Range(0f, 1f)] public float dimAlpha = 0.9f;   // à quel point la salle est assombrie derrière le panneau (1 = noir total)
    public float victoryDelay = 4f;     // victoire : le joueur voit la salle se rallumer avant l'écran de fin
    public string menuScene = "Menu";
    public float distance = 1.3f;       // m devant les yeux
    public float scale = 0.8f;          // 1 = panneau de 1,52 m de large
    public string message = "VOUS ÊTES MORT\nLe signal n'a pas été envoyé...";
    public string victoryMessage = "VOUS AVEZ ÉCHAPPÉ\nLe générateur est en marche.";
    public Color deathColor = new Color(0.8f, 0.1f, 0.1f);
    public Color victoryColor = new Color(1f, 0.8f, 0.35f);
    public Color summaryColor = new Color(0.85f, 0.85f, 0.8f);

    Transform panelRoot, page;
    Renderer black;
    TextMesh header, title, subtitle, summary;
    MenuButton[] buttons;
    Action[] actions = new Action[3];
    bool running, shown, leaving;

    protected override Transform PanelParent => panelRoot;

    void Awake() => Instance = this;

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        if (gameOverText) gameOverText.gameObject.SetActive(false);

        // fondu qui assombrit la salle (le panneau est dessiné par-dessus)
        black = HeadOverlay.Create(head, "GameOverFade", null, new Color(0, 0, 0, 0), Camera.main.nearClipPlane + 0.03f, 4100);
        black.transform.localScale = Vector3.one * 6f;
        black.enabled = false;

        // le panneau a sa propre racine : on ne déplace pas l'objet qui porte ce script (il peut porter d'autres composants)
        panelRoot = new GameObject("GameOver_Panneau").transform;
        page = BuildPage();
        panelRoot.gameObject.AddComponent<MenuPointer>().head = head;     // rayons des manettes, seulement quand le panneau est affiché
        foreach (var r in panelRoot.GetComponentsInChildren<Renderer>(true))
            r.material.renderQueue = r.GetComponent<TextMesh>() ? 4600 : 4500;   // au-dessus du fondu (4100) et de tout le reste
        panelRoot.gameObject.SetActive(false);

        var h = PlayerHealth.Instance;
        if (h) h.Died += OnDied;
    }

    Transform BuildPage()
    {
        var p = Panel("Page_FinDePartie", "BUNKER-OS  //  FIN DE PARTIE", "");
        header = p.Find("Bande_G").GetComponent<TextMesh>();
        title = UiKit.Text(p, "Titre", new Vector2(0f, 0.43f), 0.0185f, deathColor, "", TextAnchor.MiddleCenter, true);
        subtitle = UiKit.Text(p, "Sous_Titre", new Vector2(0f, 0.335f), 0.0085f, UiKit.Dim, "");
        UiKit.Quad(p, "Trait", new Vector2(0f, 0.285f), new Vector2(1.3f, 0.002f), new Color(A.r, A.g, A.b, 0.4f), -1, 0.006f);
        UiKit.Text(p, "Bilan_Titre", new Vector2(0f, 0.25f), 0.0075f, A, UiKit.Spaced("BILAN"), TextAnchor.MiddleCenter, true);
        summary = UiKit.Text(p, "Bilan", new Vector2(0f, 0.06f), 0.008f, summaryColor, "", TextAnchor.MiddleCenter);

        buttons = new MenuButton[3];
        for (int i = 0; i < 3; i++)
        {
            int k = i;
            buttons[i] = BigButton(p, -0.17f - 0.135f * i, "", () => { if (shown && !leaving) actions[k]?.Invoke(); });
        }
        UiKit.Text(p, "Aide", new Vector2(0f, -0.575f), 0.0068f, UiKit.Dim, "Visez un bouton  ·  gâchette pour valider");
        return p;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (PlayerHealth.Instance) PlayerHealth.Instance.Died -= OnDied;
        if (shown) Unfreeze();
    }

    void OnDied()
    {
        // campagne, niveau 2 ou plus : on propose de reprendre ; au niveau 1 et en mode infini : rejouer
        int resume = !WaveManager.Infinite && WaveManager.Level >= 2 ? WaveManager.Level : 0;
        if (!running) StartCoroutine(Run(message, deathColor, 0f, resume));
    }

    public void ShowVictory() { if (!running) StartCoroutine(Run(victoryMessage, victoryColor, victoryDelay, 0)); }

    // resumeLevel : niveau proposé à la reprise (0 : pas de reprise)
    IEnumerator Run(string msg, Color titleColor, float delay, int resumeLevel)
    {
        running = true;
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // 1. la salle s'assombrit
        black.enabled = true;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            SetDim(Mathf.Lerp(0f, dimAlpha, t / fadeDuration));
            yield return null;
        }
        SetDim(dimAlpha);

        // 2. le panneau
        Split(msg, out string t1, out string t2);
        header.text = msg == victoryMessage ? "BUNKER-OS  //  ÉVACUATION" : "BUNKER-OS  //  SIGNAL PERDU";
        title.text = t1; title.color = titleColor;
        subtitle.text = t2;
        summary.text = GameStats.Summary(WaveManager.Level, ZombieLevels.MaxLevel);

        if (resumeLevel > 0)
        {
            SetButton(0, "REPRENDRE · NIVEAU " + resumeLevel, () => Restart(resumeLevel));
            SetButton(1, "RECOMMENCER", () => Restart(0));
            SetButton(2, "MENU", ToMenu);
        }
        else
        {
            SetButton(0, "REJOUER", () => Restart(0));
            SetButton(1, "MENU", ToMenu);
            SetButton(2, null, null);
        }

        // devant le joueur, à hauteur des yeux, tourné vers lui (seulement le cap)
        var fwd = head ? head.forward : Vector3.forward; fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        fwd.Normalize();
        Vector3 pos = head ? head.position : Vector3.zero;
        panelRoot.SetPositionAndRotation(pos + fwd * distance - Vector3.up * 0.1f, Quaternion.LookRotation(fwd));
        panelRoot.localScale = Vector3.one * scale;
        panelRoot.gameObject.SetActive(true);
        Physics.SyncTransforms();            // temps figé : les colliders des boutons doivent suivre le panneau
        UiKit.SetOnTop(true);                // le panneau passe devant le décor et les zombies
        Time.timeScale = 0f;
        AudioListener.pause = true;
        shown = true;
    }

    void SetButton(int i, string text, Action act)
    {
        buttons[i].gameObject.SetActive(text != null);
        if (text != null) buttons[i].label.text = text;
        actions[i] = act;
    }

    void SetDim(float a) { if (black) black.material.color = new Color(0f, 0f, 0f, a); }

    // « TITRE\nsous-titre » : première ligne = titre, le reste = sous-titre
    static void Split(string m, out string t, out string s)
    {
        int i = m.IndexOf('\n');
        if (i < 0) { t = m; s = ""; return; }
        t = m.Substring(0, i);
        s = m.Substring(i + 1).Replace("\n", " ");
    }

    // ---------- actions ----------
    void Restart(int resumeLevel)
    {
        if (leaving) return;
        leaving = true;
        WaveManager.ResumeLevel = resumeLevel;        // 0 : recommencer au niveau 1
        Unfreeze();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ToMenu()
    {
        if (leaving) return;
        leaving = true;
        WaveManager.ResumeLevel = 0;
        Unfreeze();
        SceneManager.LoadScene(menuScene);
    }

    void Unfreeze()
    {
        shown = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        UiKit.SetOnTop(false);
    }
}
