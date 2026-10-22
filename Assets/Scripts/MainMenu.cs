using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu d'accueil (scène « Menu »), style « terminal de bunker » ambre :
//   SOLO (choix du mode : ÉVASION = 4 niveaux et le générateur, SURVIE = vagues infinies), MULTIJOUEUR (grisé « BIENTÔT »), PARAMÈTRES (son, confort VR, difficulté),
//   COMMANDES (schéma des manettes, AmberMenuControls), QUITTER.
// Le panneau flotte devant le joueur, un peu à droite ; il est construit au lancement (UiKit). Pointeur : MenuPointer.
public class MainMenu : AmberMenu
{
    public string gameScene = "MainScene";
    public Transform head;
    public Vector3 position = new Vector3(0.45f, 1.4f, 2.0f);    // devant le joueur, un peu à droite
    public float scale = 1.1f;                                   // taille du panneau (1 = 1,52 m de large)
    public string version = "v0.9";

    Transform mainPage, modesPage, settingsPage, controlsPage;
    Renderer fade;
    bool leaving;

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        // le panneau regarde le joueur (son axe +Z s'éloigne de lui)
        transform.position = position;
        var flat = position; flat.y = 0f;
        transform.rotation = Quaternion.LookRotation(flat.sqrMagnitude > 0.01f ? flat : Vector3.forward);
        transform.localScale = Vector3.one * scale;

        mainPage = BuildMain();
        modesPage = BuildModes();
        modesPage.gameObject.SetActive(false);
        settingsPage = BuildSettings(() => ShowSettings(false));
        settingsPage.gameObject.SetActive(false);
        controlsPage = BuildControls(() => ShowControls(false));
        controlsPage.gameObject.SetActive(false);
        // textes toujours devant les fonds (sinon le titre de la bande du haut était caché par la bande)
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            if (r.GetComponent<TextMesh>()) r.material.renderQueue = 3100;
        if (!GetComponent<MenuPointer>()) gameObject.AddComponent<MenuPointer>().head = head;

        if (head)
        {
            fade = HeadOverlay.Create(head, "MenuFondu", null, new Color(0, 0, 0, 1), Camera.main.nearClipPlane + 0.03f, 4100);
            fade.transform.localScale = Vector3.one * 6f;
            StartCoroutine(Fade(1f, 0f, 1.2f));
        }
    }

    // ---------- page principale ----------
    Transform BuildMain()
    {
        var p = Panel("Page_Menu", "BUNKER-OS  //  MENU", version);
        UiKit.Text(p, "Titre_Haut", new Vector2(0f, 0.47f), 0.0113f, A, UiKit.Spaced("THE LAST"), TextAnchor.MiddleCenter, true);
        UiKit.Text(p, "Titre", new Vector2(0f, 0.348f), 0.0285f, A, "ESCAPOR", TextAnchor.MiddleCenter, true);
        UiKit.Text(p, "Accroche", new Vector2(0f, 0.22f), 0.0075f, UiKit.Dim, "SURVIVRE  ·  CLOUER  ·  S'ÉCHAPPER");

        // 5 boutons, 0,135 m d'écart (0,16 m quand il n'y en avait que 4)
        BigButton(p, 0.09f, "SOLO", () => ShowModes(true));
        var multi = BigButton(p, -0.045f, "MULTIJOUEUR", null);
        multi.interactable = false;
        UiKit.Text(multi.transform, "Bientot", new Vector2(0.565f, 0f), 0.0075f, A, "BIENTÔT", TextAnchor.MiddleRight, true);
        UiKit.Frame(multi.transform, new Vector2(0.56f - 0.09f, 0f), new Vector2(0.22f, 0.055f), 0.003f, A, -1, 0.004f);
        BigButton(p, -0.18f, "PARAMÈTRES", () => ShowSettings(true));
        BigButton(p, -0.315f, "COMMANDES", () => ShowControls(true));
        BigButton(p, -0.45f, "QUITTER", Quit);

        UiKit.Text(p, "Aide", new Vector2(0f, -0.578f), 0.0068f, UiKit.Dim, "Visez un bouton  ·  gâchette pour valider");
        return p;
    }

    void ShowSettings(bool on)
    {
        mainPage.gameObject.SetActive(!on);
        settingsPage.gameObject.SetActive(on);
    }

    void ShowControls(bool on)
    {
        if (on) RefreshControls();
        mainPage.gameObject.SetActive(!on);
        controlsPage.gameObject.SetActive(on);
    }

    // ---------- page SOLO : choix du mode ----------
    Transform BuildModes()
    {
        var p = Panel("Page_Modes", "BUNKER-OS  //  SOLO", version);
        UiKit.Text(p, "Titre_Haut", new Vector2(0f, 0.43f), 0.0113f, A, UiKit.Spaced("SOLO"), TextAnchor.MiddleCenter, true);
        UiKit.Text(p, "Titre", new Vector2(0f, 0.31f), 0.02f, A, "MODE DE JEU", TextAnchor.MiddleCenter, true);

        BigButton(p, 0.12f, "ÉVASION", () => StartCoroutine(Play(false)));
        UiKit.Text(p, "Desc_Evasion", new Vector2(0f, 0f), 0.0068f, UiKit.Dim,
                   "4 niveaux de difficulté croissante\nObjectif : activer le générateur", TextAnchor.MiddleCenter);

        BigButton(p, -0.16f, "VAGUES INFINIES", () => StartCoroutine(Play(true)));
        UiKit.Text(p, "Desc_Survie", new Vector2(0f, -0.28f), 0.0068f, UiKit.Dim,
                   "Des vagues sans fin, de plus en plus dures\nObjectif : survivre le plus longtemps", TextAnchor.MiddleCenter);
        if (EndlessRecord.HasRecord)
            UiKit.Text(p, "Record", new Vector2(0f, -0.38f), 0.0075f, A, "RECORD : " + EndlessRecord.Describe().ToUpper(), TextAnchor.MiddleCenter, true);

        var back = MakeButton(p, new Vector2(-HW + 0.17f, -0.53f), new Vector2(0.34f, 0.09f), "◂  RETOUR", 0.0095f, () => ShowModes(false));
        back.textColor = A;
        return p;
    }

    void ShowModes(bool on)
    {
        mainPage.gameObject.SetActive(!on);
        modesPage.gameObject.SetActive(on);
    }

    // ---------- actions ----------
    IEnumerator Play(bool endless)
    {
        if (leaving) yield break;
        leaving = true;
        WaveManager.Endless = endless;        // lu par WaveManager dans la scène de jeu (reste valable pour « rejouer »)
        PlayerPrefs.Save();
        yield return Fade(0f, 1f, 0.8f);
        SceneManager.LoadScene(gameScene);
    }

    void Quit()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator Fade(float from, float to, float time)
    {
        if (!fade) yield break;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            fade.material.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t / time));
            yield return null;
        }
        fade.material.color = new Color(0, 0, 0, to);
    }
}
