using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu d'accueil (scène « Menu »), style « terminal de bunker » ambre :
//   SOLO (lance le jeu), MULTIJOUEUR (grisé « BIENTÔT »), PARAMÈTRES (son, confort VR, difficulté), QUITTER.
// Le panneau flotte devant le joueur, un peu à droite ; il est construit au lancement (UiKit). Pointeur : MenuPointer.
public class MainMenu : MonoBehaviour
{
    public string gameScene = "MainScene";
    public Transform head;
    public Vector3 position = new Vector3(0.75f, 1.45f, 2.1f);   // devant le joueur, un peu à droite
    public string version = "v0.9";

    Transform mainPage, settingsPage;
    Renderer fade;
    bool leaving;
    static readonly Color A = UiKit.Amber;
    static readonly Color Ink = new Color(0.06f, 0.04f, 0.015f);
    static readonly Color PanelCol = new Color(0.045f, 0.033f, 0.02f, 0.84f);
    static readonly Vector2 PanelSize = new Vector2(1.04f, 1.28f);

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        // le panneau regarde le joueur (son axe +Z s'éloigne de lui)
        transform.position = position;
        var flat = position; flat.y = 0f;
        transform.rotation = Quaternion.LookRotation(flat.sqrMagnitude > 0.01f ? flat : Vector3.forward);

        mainPage = BuildMain();
        settingsPage = BuildSettings();
        settingsPage.gameObject.SetActive(false);
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

        BigButton(p, 0.078f, "SOLO", () => StartCoroutine(Play()));
        var multi = BigButton(p, -0.082f, "MULTIJOUEUR", null);
        multi.interactable = false;
        UiKit.Text(multi.transform, "Bientot", new Vector2(0.39f, 0f), 0.0068f, A, "BIENTÔT", TextAnchor.MiddleRight, true);
        UiKit.Frame(multi.transform, new Vector2(0.39f - 0.065f, 0f), new Vector2(0.15f, 0.05f), 0.003f, A, -1, 0.004f);
        BigButton(p, -0.242f, "PARAMÈTRES", () => ShowSettings(true));
        BigButton(p, -0.402f, "QUITTER", Quit);

        UiKit.Text(p, "Aide", new Vector2(0f, -0.578f), 0.0068f, UiKit.Dim, "Visez un bouton  ·  gâchette pour valider");
        return p;
    }

    // ---------- page paramètres ----------
    Transform BuildSettings()
    {
        var p = Panel("Page_Parametres", "BUNKER-OS  //  PARAMÈTRES", "");
        float y = 0.49f;
        Section(p, ref y, "SON");
        Slider(p, ref y, "Musique", () => GameSettings.Music, v => GameSettings.Music = v);
        Slider(p, ref y, "Effets", () => GameSettings.Effects, v => GameSettings.Effects = v);
        Section(p, ref y, "CONFORT VR");
        Choice(p, ref y, "Rotation", new[] { "PAR À-COUPS", "FLUIDE" }, () => GameSettings.SmoothTurn ? 1 : 0, i => GameSettings.SmoothTurn = i == 1);
        Choice(p, ref y, "Bords sombres", new[] { "OUI", "NON" }, () => GameSettings.ComfortVignette ? 0 : 1, i => GameSettings.ComfortVignette = i == 0);
        Choice(p, ref y, "Vue", new[] { "RECENTRER" }, () => -1, i => GameSettings.Recenter());
        Section(p, ref y, "JEU");
        Choice(p, ref y, "Difficulté", new[] { "FACILE", "NORMAL", "DIFFICILE" }, () => (int)GameSettings.Difficulty, i => GameSettings.Difficulty = (GameSettings.Level)i);

        var back = MakeButton(p, new Vector2(-0.33f, -0.53f), new Vector2(0.28f, 0.085f), "◂  RETOUR", 0.0085f, () => { PlayerPrefs.Save(); ShowSettings(false); });
        back.textColor = A;
        return p;
    }

    void ShowSettings(bool on)
    {
        mainPage.gameObject.SetActive(!on);
        settingsPage.gameObject.SetActive(on);
    }

    // ---------- briques ----------
    Transform Panel(string name, string header, string headerRight)
    {
        var p = new GameObject(name).transform;
        p.SetParent(transform, false);
        UiKit.Quad(p, "Fond", Vector2.zero, PanelSize, PanelCol, -1, 0.012f);
        UiKit.Frame(p, Vector2.zero, PanelSize, 0.004f, new Color(A.r, A.g, A.b, 0.4f), -1, 0.006f);
        UiKit.Brackets(p, Vector2.zero, PanelSize + new Vector2(0.03f, 0.03f), 0.07f, 0.008f, A);
        UiKit.Quad(p, "Bande", new Vector2(0f, 0.596f), new Vector2(PanelSize.x, 0.088f), new Color(A.r, A.g, A.b, 0.92f), -1, 0.006f);
        UiKit.Text(p, "Bande_G", new Vector2(-0.49f, 0.596f), 0.0085f, Ink, header, TextAnchor.MiddleLeft, true);
        if (!string.IsNullOrEmpty(headerRight))
            UiKit.Text(p, "Bande_D", new Vector2(0.49f, 0.596f), 0.0085f, Ink, headerRight, TextAnchor.MiddleRight, true);
        return p;
    }

    MenuButton BigButton(Transform p, float y, string text, Action click)
    {
        var b = MakeButton(p, new Vector2(0f, y), new Vector2(0.84f, 0.124f), text, 0.0113f, click, TextAnchor.MiddleLeft);
        b.arrow = UiKit.Text(b.transform, "Fleche", new Vector2(-0.46f, 0f), 0.009f, A, "▶", TextAnchor.MiddleCenter, true);
        b.arrow.gameObject.SetActive(false);
        return b;
    }

    MenuButton MakeButton(Transform p, Vector2 pos, Vector2 size, string text, float charSize, Action click, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        var g = new GameObject("Bouton_" + text);
        g.transform.SetParent(p, false);
        g.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        var col = g.AddComponent<BoxCollider>();
        col.size = new Vector3(size.x, size.y, 0.03f);
        var b = g.AddComponent<MenuButton>();
        b.background = UiKit.Quad(g.transform, "Fond", Vector2.zero, size, new Color(A.r, A.g, A.b, 0.1f), -1, 0.008f);
        UiKit.Frame(g.transform, Vector2.zero, size, 0.004f, new Color(A.r, A.g, A.b, 0.6f), -1, 0.004f);
        var lp = anchor == TextAnchor.MiddleLeft ? new Vector2(-size.x / 2f + 0.052f, 0f) : Vector2.zero;
        b.label = UiKit.Text(g.transform, "Texte", lp, charSize, UiKit.White, text, anchor, true);
        b.onClick = click;
        return b;
    }

    void Section(Transform p, ref float y, string title)
    {
        UiKit.Text(p, "Section", new Vector2(-0.46f, y), 0.0075f, A, title, TextAnchor.MiddleLeft, true);
        UiKit.Quad(p, "Trait", new Vector2(0f, y - 0.035f), new Vector2(0.92f, 0.002f), new Color(A.r, A.g, A.b, 0.4f), -1, 0.006f);
        y -= 0.085f;
    }

    // curseur en 10 segments (cliquer un segment règle la valeur)
    void Slider(Transform p, ref float y, string label, Func<float> get, Action<float> set)
    {
        UiKit.Text(p, "Libelle", new Vector2(-0.46f, y), 0.0088f, UiKit.White, label, TextAnchor.MiddleLeft);
        var segs = new List<MenuButton>();
        Action refresh = () => { int n = Mathf.RoundToInt(get() * 10f); for (int i = 0; i < segs.Count; i++) segs[i].selected = i < n; };
        for (int i = 0; i < 10; i++)
        {
            int k = i;
            var s = MakeButton(p, new Vector2(0.02f + i * 0.044f, y), new Vector2(0.036f, 0.05f), "", 0.005f, null);
            s.onClick = () => { set((k + 1) / 10f); refresh(); };
            segs.Add(s);
        }
        refresh();
        y -= 0.095f;
    }

    void Choice(Transform p, ref float y, string label, string[] options, Func<int> get, Action<int> set)
    {
        UiKit.Text(p, "Libelle", new Vector2(-0.46f, y), 0.0088f, UiKit.White, label, TextAnchor.MiddleLeft);
        var buttons = new List<MenuButton>();
        Action refresh = () => { int sel = get(); for (int i = 0; i < buttons.Count; i++) buttons[i].selected = i == sel; };
        float right = 0.46f;
        for (int i = options.Length - 1; i >= 0; i--)
        {
            float w = 0.04f + options[i].Length * 0.0175f;
            int k = i;
            var b = MakeButton(p, new Vector2(right - w / 2f, y), new Vector2(w, 0.06f), options[i], 0.0065f, null);
            b.onClick = () => { set(k); refresh(); };
            buttons.Insert(0, b);
            right -= w + 0.015f;
        }
        refresh();
        y -= 0.095f;
    }

    // ---------- actions ----------
    IEnumerator Play()
    {
        if (leaving) yield break;
        leaving = true;
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
