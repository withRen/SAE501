using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Écran de fin de partie (terminal BUNKER-OS en World Space, construit en code via UiKit) :
//   - Défaite (PlayerHealth.Died) : thème Rouge/Ambre, « BUNKER-OS // ALERTE CRITIQUE », « VOUS ÊTES MORT »
//   - Victoire (portail du jardin franchi : WaveManager.PlayerEscaped -> ShowVictory) : thème Vert/Or, « BUNKER-OS // EXTRACTION »,
//     « VICTOIRE », « Vous avez réussi à vous enfuir. ». Avant le bilan, après un fondu au noir complet, la cinématique de fin (EscapeVideo : le survivant
//     s'enfuit par le portail, les zombies secouent la grille) est jouée sur un écran dans le noir ; le bilan apparaît
//     PAR-DESSUS la vidéo à la moitié (ou dès que le joueur la passe), la vidéo continue derrière (journal 177).
// Sous le titre, l'encadré « BILAN DE MISSION » affiche en 2 colonnes les compteurs de GameStats (zombies éliminés,
// précision des tirs à la tête, planches posées, niveau ou vague atteinte, temps et éventuel nouveau record en Survie).
// Le joueur choisit explicitement la suite via les boutons pointés au rayon (MenuPointer) ou à la souris (DesktopMode) :
//   - Mort au niveau >= 2 en mode Évasion : REPRENDRE AU NIVEAU X / RECOMMENCER (NIVEAU 1) / MENU PRINCIPAL
//   - Autres cas (Niveau 1, Survie, Victoire) : REJOUER (ou NOUVELLE PARTIE) / MENU PRINCIPAL
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }
    public static bool PanelOpen { get; private set; }

    static readonly Vector2 PanelSize = new Vector2(1.84f, 1.44f);   // journal 178 : 1,52 -> 1,84 m (le titre débordait)
    static readonly Color Ink = new Color(0.06f, 0.04f, 0.015f);
    const float StatX = 0.78f;

    public Transform head;
    public TextMesh gameOverText;       // conservé pour la scène existante (RoomBuilder.BuildHealthUI) : désactivé au profit du panneau World Space
    public float fadeDuration = 1.2f;
    public float restartDelay = 15f;    // conservé pour la compatibilité de sérialisation (plus de rechargement automatique par timeout)
    public float minReadTime = 1f;      // verrou de sécurité après le fondu : les boutons sont grisés pendant 1 s pour éviter un tir réflexe
    public float victoryDelay = 4f;     // conservé pour la sérialisation ; victoire : escapeDelay (le joueur vient de franchir le portail)
    public float escapeDelay = 1.2f;    // victoire : quelques pas sur la route avant le fondu et la cinématique
    public float restartHold = 1f;      // conservé pour la compatibilité de sérialisation
    public string message = "VOUS ÊTES MORT\nLe signal n'a pas été envoyé...";
    public string victoryMessage = "VICTOIRE\nVous avez réussi à vous enfuir.";
    public Color deathColor = new Color(0.8f, 0.1f, 0.1f);
    public Color victoryColor = new Color(1f, 0.8f, 0.35f);
    public Color summaryColor = new Color(0.85f, 0.85f, 0.8f);
    public string menuScene = "Menu";
    public float distance = 1.3f;       // distance devant les yeux (m)
    public float scale = 0.8f;          // échelle du panneau (1 = 1,52 m x 1,24 m)

    Renderer black;
    Transform panel;
    readonly List<MenuButton> buttons = new List<MenuButton>();
    bool running, leaving;
    int pendingResumeLevel;

    public Transform PanelTransform => panel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
        PanelOpen = false;
    }

    void Awake()
    {
        Instance = this;
        PanelOpen = false;
    }

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;

        if (head)
        {
            var cam = head.GetComponent<Camera>() ? head.GetComponent<Camera>() : Camera.main;
            float near = cam ? cam.nearClipPlane + 0.03f : 0.06f;
            black = HeadOverlay.Create(head, "GameOverFade", null, new Color(0f, 0f, 0f, 0f), near, 4100);
            black.transform.localScale = Vector3.one * 6f;
            black.enabled = false;
        }

        // L'ancien TextMesh collé à la caméra est désactivé : tout passe désormais par le terminal World Space Panneau_Fin
        if (gameOverText)
        {
            var mr = gameOverText.GetComponent<MeshRenderer>();
            if (mr) mr.enabled = false;
            gameOverText.gameObject.SetActive(false);
        }

        var h = PlayerHealth.Instance;
        if (h) h.Died += OnDied;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (PlayerHealth.Instance) PlayerHealth.Instance.Died -= OnDied;
        if (PanelOpen)
        {
            UiKit.SetOnTop(false);
            PanelOpen = false;
        }
    }

    // Mode survie : pas de reprise au niveau atteint (on repart de la vague 1, le record compte)
    void OnDied()
    {
        if (NetSession.Active) return;      // [Multijoueur] la partie continue tant qu'un joueur est debout (NetGame appelle ShowTeamDefeat)
        if (!running)
            StartCoroutine(Run(false, 0f, !WaveManager.Endless && WaveManager.Level >= 2 ? WaveManager.Level : 0));
    }

    /// <summary>[Multijoueur] tous les joueurs sont tombés : bilan d'équipe pour tout le monde.</summary>
    public void ShowTeamDefeat()
    {
        if (!running)
            StartCoroutine(Run(false, 0f, 0));
    }

    public void ShowVictory()
    {
        if (!running)
            StartCoroutine(Run(true, escapeDelay, 0));
    }

    IEnumerator Run(bool victory, float delay, int resumeLevel)
    {
        running = true;
        pendingResumeLevel = resumeLevel;

        if (delay > 0f) yield return new WaitForSeconds(delay);

        // Si le menu pause était ouvert au moment du déclenchement, on le ferme proprement
        if (PauseMenu.Paused)
            FindAnyObjectByType<PauseMenu>()?.Resume();

        if (!head && Camera.main) head = Camera.main.transform;

        // Victoire : fondu au noir complet, cinématique de fin (si la vidéo est là), puis le bilan sur fond assombri
        if (victory && black && EscapeVideo.Available)
        {
            black.enabled = true;
            float dur = Mathf.Max(0.01f, fadeDuration);
            for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
            {
                black.material.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t / dur));
                yield return null;
            }
            black.material.color = Color.black;
            // la vidéo continue ; le bilan apparaît par-dessus à la moitié (ou tout de suite si le joueur passe)
            bool menu = false, done = false;
            StartCoroutine(PlayVideo(() => menu = true, () => done = true));
            while (!menu && !done) yield return null;
            overVideo = menu && EscapeVideo.Playing;
        }

        // Fondu d'assombrissement jusqu'à 78 % d'opacité pour isoler le terminal tout en gardant la perception de l'espace
        // (pas par-dessus la vidéo : l'écran de cinéma reste visible autour du panneau)
        const float targetFadeAlpha = 0.78f;
        if (black && !overVideo)
        {
            float a0 = black.enabled ? black.material.color.a : 0f;     // après la cinématique : on part du noir complet
            black.enabled = true;
            float dur = Mathf.Max(0.01f, fadeDuration);
            for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
            {
                black.material.color = new Color(0f, 0f, 0f, Mathf.Lerp(a0, targetFadeAlpha, t / dur));
                yield return null;
            }
            black.material.color = new Color(0f, 0f, 0f, targetFadeAlpha);
        }

        BuildPanel(victory, resumeLevel);
        PlacePanelInFrontOfHead();
        if (overVideo) StartCoroutine(ShowOverVideo());

        // Verrou de sécurité anti-missclick : boutons grisés pendant 1 s (absorbe les tirs résiduels du combat)
        SetButtonsInteractable(false);
        panel.gameObject.SetActive(true);
        Physics.SyncTransforms();
        UiKit.SetOnTop(true);
        PanelOpen = true;

        float lockTime = Mathf.Clamp(minReadTime, 0.2f, 1.0f);
        for (float t = 0f; t < lockTime; t += Time.unscaledDeltaTime)
            yield return null;

        if (!leaving)
            SetButtonsInteractable(true);
    }

    bool overVideo;

    IEnumerator PlayVideo(Action onMenu, Action onDone)
    {
        yield return EscapeVideo.Play(head, onMenu);
        onDone();
    }

    // Par-dessus la vidéo : la caméra ne dessine que le calque de l'écran de cinéma ; le panneau et les rayons de pointage
    // y passent aussi (les rayons sont créés par MenuPointer.Start, d'où la 2e passe une image plus tard). Fond un peu
    // transparent : on devine la vidéo derrière le bilan. Le panneau grandit en 0,3 s.
    IEnumerator ShowOverVideo()
    {
        var bg = panel.Find("Fond_Opaque");
        if (bg && bg.GetComponent<Renderer>()) bg.GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 0.82f);
        Vector3 full = panel.localScale;
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (var t in panel.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = EscapeVideo.Layer;
            if (pass == 0) yield return null;
        }
        for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
        {
            if (!panel) yield break;
            panel.localScale = full * Mathf.Lerp(0.85f, 1f, 1f - Mathf.Pow(1f - t / 0.3f, 2f));
            yield return null;
        }
        if (panel) panel.localScale = full;
    }

    void PlacePanelInFrontOfHead()
    {
        if (!panel) return;
        if (!head && Camera.main) head = Camera.main.transform;
        var originPos = head ? head.position : Vector3.zero;
        var fwd = head ? head.forward : Vector3.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        fwd.Normalize();

        panel.SetPositionAndRotation(originPos + fwd * distance - Vector3.up * 0.10f, Quaternion.LookRotation(fwd));
        panel.localScale = Vector3.one * scale;
    }

    void BuildPanel(bool victory, int resumeLevel)
    {
        if (panel) Destroy(panel.gameObject);
        buttons.Clear();

        panel = new GameObject("Panneau_Fin").transform;
        panel.SetParent(transform, false);

        bool endless = WaveManager.Endless;
        Color frameCol = victory ? UiKit.Green : UiKit.Red;
        Color pageCol = victory ? UiKit.PanelGreen : UiKit.PanelRed;
        Color surTitleCol = victory ? UiKit.Green : UiKit.Amber;
        Color mainTitleCol = victory ? victoryColor : UiKit.Red;
        Color sectionCol = victory ? UiKit.Green : UiKit.Amber;

        // Fonds et cadre externe du terminal
        UiKit.Quad(panel, "Fond_Opaque", Vector2.zero, PanelSize, new Color(0f, 0f, 0f, 0.97f), 4490, 0.020f);
        UiKit.Quad(panel, "Fond_Page", Vector2.zero, PanelSize, pageCol, 4495, 0.012f);
        UiKit.Frame(panel, Vector2.zero, PanelSize, 0.004f, new Color(frameCol.r, frameCol.g, frameCol.b, 0.40f), 4500, 0.006f);
        UiKit.Brackets(panel, Vector2.zero, PanelSize + new Vector2(0.03f, 0.03f), 0.07f, 0.008f, frameCol, 4510);
        UiKit.Scanlines(panel, Vector2.zero, PanelSize, 4520);

        // Zone 1 — Bandeau système supérieur (y = +0.678)
        UiKit.Quad(panel, "Bande_Haut", new Vector2(0f, 0.678f), new Vector2(PanelSize.x, 0.084f),
                   new Color(frameCol.r, frameCol.g, frameCol.b, 0.92f), 4500, 0.006f);
        UiKit.Text(panel, "Bande_G", new Vector2(-PanelSize.x / 2f + 0.03f, 0.678f), 0.0085f, Ink,
                   victory ? "BUNKER-OS  //  EXTRACTION" : "BUNKER-OS  //  ALERTE CRITIQUE",
                   TextAnchor.MiddleLeft, true, 4600);
        UiKit.Text(panel, "Bande_D", new Vector2(PanelSize.x / 2f - 0.03f, 0.678f), 0.0085f, Ink,
                   endless ? "MODE SURVIE" : "MODE ÉVASION",
                   TextAnchor.MiddleRight, true, 4600);

        // Zone 2 — Bloc Titre & Statut (espacement vertical élargi : +0.575 / +0.448 / +0.335)
        // victoire : pas de sur-titre, « VICTOIRE » un peu plus haut (journal 178)
        if (!victory)
            UiKit.Text(panel, "Sur_Titre", new Vector2(0f, 0.575f), 0.0095f, surTitleCol,
                       UiKit.Spaced("SYSTÈME DÉFAILLANT"), TextAnchor.MiddleCenter, true, 4600);
        UiKit.Text(panel, "Titre_Principal", new Vector2(0f, victory ? 0.50f : 0.448f), victory ? 0.0275f : 0.0235f, mainTitleCol,
                   victory ? "VICTOIRE" : NetSession.Active ? "L'ÉQUIPE EST TOMBÉE" : "VOUS ÊTES MORT",
                   TextAnchor.MiddleCenter, true, 4600);
        string subTitle = victory ? "Vous avez réussi à vous enfuir."
                        : endless ? "Vous avez succombé à la horde."
                                  : "Le signal n'a pas été envoyé...";
        UiKit.Text(panel, "Sous_Titre", new Vector2(0f, 0.335f), 0.0085f, UiKit.White,
                   subTitle, TextAnchor.MiddleCenter, false, 4600);

        // Zone 3 — Encadré BILAN DE MISSION (centre y = +0.025, taille 1.36m x 0.480m : de -0.215 à +0.265)
        UiKit.Quad(panel, "Fond_Bilan", new Vector2(0f, 0.025f), new Vector2(PanelSize.x - 0.16f, 0.480f),
                   new Color(0f, 0f, 0f, 0.35f), 4500, 0.009f);
        UiKit.Frame(panel, new Vector2(0f, 0.025f), new Vector2(PanelSize.x - 0.16f, 0.480f), 0.003f,
                    new Color(frameCol.r, frameCol.g, frameCol.b, 0.45f), 4505, 0.004f);
        UiKit.Text(panel, "Titre_Bilan", new Vector2(-StatX, 0.222f), 0.0075f, sectionCol,
                   "BILAN DE MISSION", TextAnchor.MiddleLeft, true, 4600);
        UiKit.Quad(panel, "Trait_Bilan", new Vector2(0f, 0.186f), new Vector2(PanelSize.x - 0.24f, 0.002f),
                   new Color(frameCol.r, frameCol.g, frameCol.b, 0.40f), 4505, 0.004f);

        PopulateStats(endless);

        // Zone 4 — Barre d'actions (MenuButton)
        bool hasResume = !victory && !endless && resumeLevel >= 2;
        if (NetSession.Active)
        {
            // [Multijoueur] pas de « rejouer » : on repasse par le lobby (l'hôte qui quitte ferme la partie de tous)
            MakeActionButton(new Vector2(0f, -0.380f), new Vector2(1.50f, 0.110f), 0.0113f, "QUITTER LA PARTIE", ReturnToMainMenu);
        }
        else if (hasResume)
        {
            MakeActionButton(new Vector2(0f, -0.290f), new Vector2(1.50f, 0.094f), 0.0105f,
                             "REPRENDRE AU NIVEAU " + resumeLevel, ResumeAtLevel);
            MakeActionButton(new Vector2(0f, -0.410f), new Vector2(1.50f, 0.094f), 0.0105f,
                             "RECOMMENCER (NIVEAU 1)", RestartFromBeginning);
            MakeActionButton(new Vector2(0f, -0.530f), new Vector2(1.50f, 0.094f), 0.0105f,
                             "MENU PRINCIPAL", ReturnToMainMenu);
        }
        else
        {
            string primaryLabel = victory ? "NOUVELLE PARTIE" : "REJOUER";
            MakeActionButton(new Vector2(0f, -0.320f), new Vector2(1.50f, 0.110f), 0.0113f,
                             primaryLabel, RestartFromBeginning);
            MakeActionButton(new Vector2(0f, -0.470f), new Vector2(1.50f, 0.110f), 0.0113f,
                             "MENU PRINCIPAL", ReturnToMainMenu);
        }

        // Zone 5 — Pied de page (uniquement en VR : touche de validation)
        if (!DesktopMode.On)
            UiKit.Text(panel, "Aide_Bas", new Vector2(0f, -0.650f), 0.0068f, UiKit.Dim,
                       "Gâchette : valider", TextAnchor.MiddleCenter, false, 4600);

        // Force le ZTest Always sur chaque matériau du panneau (UI/Default déclare unity_GUIZTestMode en propriété locale)
        foreach (var r in panel.GetComponentsInChildren<Renderer>(true))
            r.material.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);

        var mp = panel.gameObject.AddComponent<MenuPointer>();
        mp.head = head;
        mp.color = UiKit.Amber;
    }

    void PopulateStats(bool endless)
    {
        if (NetSession.Active && NetGame.Instance) { PopulateTeamStats(); return; }
        int level = Mathf.Max(1, WaveManager.Level);
        if (endless)
            EndlessRecord.Submit(level, GameStats.Time);

        string killsVal = GameStats.Kills + (GameStats.MeleeKills > 0 ? "  (dont " + GameStats.MeleeKills + " à la mêlée)" : "");
        string hsVal = Mathf.RoundToInt(GameStats.HeadShotPercent) + " %  (" + GameStats.HeadShots + " sur " + GameStats.Shots + " tirs)";
        string planksVal = GameStats.PlanksPlaced.ToString();

        StatRow(1, 0.136f, "Zombies éliminés", killsVal, UiKit.White);
        StatRow(2, 0.058f, "Tirs à la tête", hsVal, UiKit.White);
        StatRow(3, -0.020f, "Planches posées", planksVal, UiKit.White);

        if (endless)
        {
            string waveVal = level + (!EndlessRecord.NewRecord && EndlessRecord.BestWave > 0 ? "  (record : vague " + EndlessRecord.BestWave + ")" : "");
            bool isRecord = EndlessRecord.NewRecord;
            string timeVal = GameStats.FormatTime(GameStats.Time) + (isRecord ? "  ·  NOUVEAU RECORD !" : "");
            StatRow(4, -0.098f, "Vague atteinte", waveVal, UiKit.White);
            StatRow(5, -0.176f, "Temps de survie", timeVal, isRecord ? UiKit.Amber : UiKit.White);
        }
        else
        {
            string lvlVal = level + " / " + ZombieLevels.MaxLevel;
            string timeVal = GameStats.FormatTime(GameStats.Time);
            StatRow(4, -0.098f, "Niveau atteint", lvlVal, UiKit.White);
            StatRow(5, -0.176f, "Temps total", timeVal, UiKit.White);
        }
    }

    // [Multijoueur] score d'équipe, vague, temps, puis une ligne par joueur (du meilleur score au moins bon)
    void PopulateTeamStats()
    {
        var g = NetGame.Instance;
        int level = Mathf.Max(1, WaveManager.Level);
        StatRow(1, 0.140f, "Score d'équipe", g.TeamScore.ToString("N0"), UiKit.Amber);
        StatRow(2, 0.080f, "Vague atteinte · temps", level + "  ·  " + GameStats.FormatTime(GameStats.Time), UiKit.White);
        float y = 0.010f;
        int i = 3;
        foreach (var sc in g.SortedScores())
        {
            bool me = sc.clientId == NetSession.LocalId;
            StatRow(i++, y, "Joueur " + sc.number + (me ? " (vous)" : ""),
                    sc.score.ToString("N0") + " pts  ·  " + sc.kills + " zombies  ·  " + sc.heads + " têtes  ·  " + sc.planks + " planches",
                    me ? UiKit.Amber : UiKit.White);
            y -= 0.058f;
            if (i > 6) break;
        }
    }

    void StatRow(int index, float y, string label, string value, Color valueColor)
    {
        UiKit.Text(panel, "Stat_G_" + index, new Vector2(-StatX, y), 0.0082f, UiKit.Dim,
                   label, TextAnchor.MiddleLeft, false, 4600);
        UiKit.Text(panel, "Stat_D_" + index, new Vector2(StatX, y), 0.0085f, valueColor,
                   value, TextAnchor.MiddleRight, true, 4600);
    }

    MenuButton MakeActionButton(Vector2 pos, Vector2 size, float charSize, string text, Action click)
    {
        Color accent = UiKit.Amber;
        var g = new GameObject("Bouton_" + text);
        g.transform.SetParent(panel, false);
        g.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        var col = g.AddComponent<BoxCollider>();
        col.size = new Vector3(size.x, size.y, 0.03f);
        var b = g.AddComponent<MenuButton>();
        b.accent = accent;
        b.textColor = UiKit.White;
        b.background = UiKit.Quad(g.transform, "Fond", Vector2.zero, size, new Color(accent.r, accent.g, accent.b, 0.10f), 4500, 0.008f);
        UiKit.Frame(g.transform, Vector2.zero, size, 0.004f, new Color(accent.r, accent.g, accent.b, 0.60f), 4505, 0.004f);
        b.label = UiKit.Text(g.transform, "Texte", new Vector2(-size.x / 2f + 0.052f, 0f), charSize, UiKit.White, text, TextAnchor.MiddleLeft, true, 4600);
        b.arrow = UiKit.Text(g.transform, "Fleche", new Vector2(-size.x / 2f - 0.04f, 0f), 0.009f, accent, "▶", TextAnchor.MiddleCenter, true, 4600);
        b.arrow.gameObject.SetActive(false);
        b.onClick = click;
        buttons.Add(b);
        return b;
    }

    void SetButtonsInteractable(bool on)
    {
        foreach (var b in buttons)
            if (b) b.interactable = on;
    }

    void ResumeAtLevel()
    {
        if (leaving) return;
        leaving = true;
        SetButtonsInteractable(false);
        WaveManager.ResumeLevel = pendingResumeLevel;
        ClosePanelState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void RestartFromBeginning()
    {
        if (leaving) return;
        leaving = true;
        SetButtonsInteractable(false);
        WaveManager.ResumeLevel = 0;
        ClosePanelState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ReturnToMainMenu()
    {
        if (leaving) return;
        leaving = true;
        SetButtonsInteractable(false);
        WaveManager.ResumeLevel = 0;
        ClosePanelState();
        if (NetSession.Active) { NetSession.Leave(); NetSession.LastError = null; }
        if (Application.CanStreamedLevelBeLoaded(menuScene))
            SceneManager.LoadScene(menuScene);
        else
        {
            Debug.LogWarning("[GameOverUI] Scène '" + menuScene + "' absente des Build Settings : rechargement de la scène active.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    static void ClosePanelState()
    {
        UiKit.SetOnTop(false);
        PanelOpen = false;
    }
}
