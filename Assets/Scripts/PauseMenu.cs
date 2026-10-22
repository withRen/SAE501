using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu pause de la partie : bouton menu (☰) de la manette gauche (Échap dans l'éditeur) pour l'ouvrir ou le fermer.
// Même style que le menu d'accueil (AmberMenu) : REPRENDRE, PARAMÈTRES (son, confort VR, difficulté), COMMANDES (schéma des manettes),
// MENU (retour à l'accueil).
// Pendant la pause : temps figé (zombies, vagues, physique), son en pause, tir / coups / clous bloqués (voir Paused).
// Le panneau apparaît devant le joueur, dans la direction où il regarde ; on vise avec le rayon de la manette, gâchette pour valider.
public class PauseMenu : AmberMenu
{
    public static bool Paused { get; private set; }

    public Transform head;
    public string menuScene = "Menu";
    public float distance = 1.3f;          // m devant les yeux
    public float scale = 0.8f;             // 1 = panneau de 1,52 m de large

    Transform panel, mainPage, settingsPage, controlsPage;
    bool wasDown, leaving;

    protected override Transform PanelParent => panel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Paused = false;

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        panel = new GameObject("Panneau").transform;
        panel.SetParent(transform, false);
        mainPage = BuildMain();
        settingsPage = BuildSettings(() => ShowSettings(false));
        controlsPage = BuildControls(() => ShowControls(false));
        panel.gameObject.AddComponent<MenuPointer>().head = head;   // rayons des manettes, seulement quand le panneau est affiché
        // fond plus opaque derrière les pages : le décor et les textes du jeu ne se lisent plus à travers
        UiKit.Quad(panel, "Fond_Opaque", Vector2.zero, PanelSize, new Color(0f, 0f, 0f, 0.97f), -1, 0.02f);
        // ordre d'affichage : au-dessus de tout ce qui est en jeu (textes d'état, écrans collés à la tête : 4300 au plus)
        foreach (var r in panel.GetComponentsInChildren<Renderer>(true))
            r.material.renderQueue = r.GetComponent<TextMesh>() ? 4600 : r.name == "Fond_Opaque" ? 4490 : r.name == "Fond_Page" ? 4495 : 4500;   // fonds dessinés en premier
        panel.gameObject.SetActive(false);
    }

    Transform BuildMain()
    {
        var p = Panel("Page_Pause", "BUNKER-OS  //  PAUSE", "");
        UiKit.Text(p, "Titre_Haut", new Vector2(0f, 0.43f), 0.0113f, A, UiKit.Spaced("PARTIE"), TextAnchor.MiddleCenter, true);
        UiKit.Text(p, "Titre", new Vector2(0f, 0.3f), 0.0285f, A, "PAUSE", TextAnchor.MiddleCenter, true);
        BigButton(p, 0.09f, "REPRENDRE", Resume);
        BigButton(p, -0.07f, "PARAMÈTRES", () => ShowSettings(true));
        BigButton(p, -0.23f, "COMMANDES", () => ShowControls(true));
        BigButton(p, -0.39f, "MENU", ToMenu);
        UiKit.Text(p, "Aide", new Vector2(0f, -0.53f), 0.0068f, UiKit.Dim, "Visez un bouton  ·  gâchette pour valider  ·  bouton menu : reprendre");
        return p;
    }

    void ShowSettings(bool on)
    {
        mainPage.gameObject.SetActive(!on);
        settingsPage.gameObject.SetActive(on);
        controlsPage.gameObject.SetActive(false);
    }

    void ShowControls(bool on)
    {
        if (on) RefreshControls();
        mainPage.gameObject.SetActive(!on);
        settingsPage.gameObject.SetActive(false);
        controlsPage.gameObject.SetActive(on);
    }

    void Update()
    {
        bool down = VRInput.LeftMenu();
        if (down && !wasDown) { if (Paused) Resume(); else Pause(); }
        wasDown = down;
    }

    public void Pause()
    {
        if (Paused || leaving || !panel) return;
        if (PlayerHealth.Instance && PlayerHealth.Instance.IsDead) return;   // écran de fin en cours
        if (!head && Camera.main) head = Camera.main.transform;
        // devant le joueur, à hauteur des yeux, tourné vers lui (seulement le cap : pas penché si on regarde en bas)
        var fwd = head ? head.forward : Vector3.forward; fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        fwd.Normalize();
        transform.SetPositionAndRotation(head.position + fwd * distance - Vector3.up * 0.1f, Quaternion.LookRotation(fwd));
        transform.localScale = Vector3.one * scale;
        ShowSettings(false);
        panel.gameObject.SetActive(true);
        Physics.SyncTransforms();        // temps figé : la physique ne met plus à jour les colliders du panneau toute seule
        UiKit.SetOnTop(true);            // le panneau passe devant le décor et les zombies (sinon un meuble ou un zombie le cache)
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Paused = true;
    }

    public void Resume()
    {
        if (!Paused) return;
        PlayerPrefs.Save();
        panel.gameObject.SetActive(false);
        UiKit.SetOnTop(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Paused = false;
    }

    void ToMenu()
    {
        if (leaving) return;
        leaving = true;
        Resume();
        SceneManager.LoadScene(menuScene);
    }

    void OnDestroy()
    {
        if (!Paused) return;
        UiKit.SetOnTop(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Paused = false;
    }
}
