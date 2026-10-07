using System;
using System.Collections.Generic;
using UnityEngine;

// Base commune des menus « terminal de bunker » ambre (menu d'accueil MainMenu, menu pause PauseMenu) :
// panneau, gros boutons, page PARAMÈTRES (son, confort VR, difficulté) et briques (curseurs, choix).
// Tout est construit en code (UiKit) ; le pointeur est MenuPointer.
public abstract class AmberMenu : MonoBehaviour
{
    protected static readonly Color A = UiKit.Amber;
    protected static readonly Color Ink = new Color(0.06f, 0.04f, 0.015f);
    protected static readonly Color PanelCol = new Color(0.045f, 0.033f, 0.02f, 0.84f);
    protected static readonly Vector2 PanelSize = new Vector2(1.52f, 1.28f);
    protected const float HW = 0.68f;                                      // demi-largeur utile du contenu

    // objet sous lequel les pages sont construites (le menu pause les range sous un panneau qu'il affiche/masque)
    protected virtual Transform PanelParent => transform;

    // ---------- page paramètres ----------
    // onBack : action du bouton RETOUR
    protected Transform BuildSettings(Action onBack)
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

        var back = MakeButton(p, new Vector2(-HW + 0.17f, -0.53f), new Vector2(0.34f, 0.09f), "◂  RETOUR", 0.0095f, () => { PlayerPrefs.Save(); onBack(); });
        back.textColor = A;
        return p;
    }

    // ---------- briques ----------
    protected Transform Panel(string name, string header, string headerRight)
    {
        var p = new GameObject(name).transform;
        p.SetParent(PanelParent, false);
        UiKit.Quad(p, "Fond", Vector2.zero, PanelSize, PanelCol, -1, 0.012f);
        UiKit.Frame(p, Vector2.zero, PanelSize, 0.004f, new Color(A.r, A.g, A.b, 0.4f), -1, 0.006f);
        UiKit.Brackets(p, Vector2.zero, PanelSize + new Vector2(0.03f, 0.03f), 0.07f, 0.008f, A);
        UiKit.Quad(p, "Bande", new Vector2(0f, 0.596f), new Vector2(PanelSize.x, 0.088f), new Color(A.r, A.g, A.b, 0.92f), -1, 0.006f);
        UiKit.Text(p, "Bande_G", new Vector2(-PanelSize.x / 2f + 0.03f, 0.596f), 0.0085f, Ink, header, TextAnchor.MiddleLeft, true);
        if (!string.IsNullOrEmpty(headerRight))
            UiKit.Text(p, "Bande_D", new Vector2(PanelSize.x / 2f - 0.03f, 0.596f), 0.0085f, Ink, headerRight, TextAnchor.MiddleRight, true);
        return p;
    }

    protected MenuButton BigButton(Transform p, float y, string text, Action click)
    {
        var b = MakeButton(p, new Vector2(0f, y), new Vector2(1.2f, 0.124f), text, 0.0113f, click, TextAnchor.MiddleLeft);
        b.arrow = UiKit.Text(b.transform, "Fleche", new Vector2(-0.64f, 0f), 0.009f, A, "▶", TextAnchor.MiddleCenter, true);
        b.arrow.gameObject.SetActive(false);
        return b;
    }

    protected MenuButton MakeButton(Transform p, Vector2 pos, Vector2 size, string text, float charSize, Action click, TextAnchor anchor = TextAnchor.MiddleCenter)
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

    protected void Section(Transform p, ref float y, string title)
    {
        UiKit.Text(p, "Section", new Vector2(-HW, y), 0.0075f, A, title, TextAnchor.MiddleLeft, true);
        UiKit.Quad(p, "Trait", new Vector2(0f, y - 0.035f), new Vector2(2f * HW, 0.002f), new Color(A.r, A.g, A.b, 0.4f), -1, 0.006f);
        y -= 0.085f;
    }

    // curseur en 10 segments (cliquer un segment règle la valeur)
    protected void Slider(Transform p, ref float y, string label, Func<float> get, Action<float> set)
    {
        UiKit.Text(p, "Libelle", new Vector2(-HW, y), 0.0105f, UiKit.White, label, TextAnchor.MiddleLeft);
        var segs = new List<MenuButton>();
        Action refresh = () => { int n = Mathf.RoundToInt(get() * 10f); for (int i = 0; i < segs.Count; i++) segs[i].selected = i < n; };
        for (int i = 0; i < 10; i++)
        {
            int k = i;
            var s = MakeButton(p, new Vector2(HW - 0.03f - (9 - i) * 0.065f, y), new Vector2(0.055f, 0.065f), "", 0.005f, null);
            s.onClick = () => { set((k + 1) / 10f); refresh(); };
            segs.Add(s);
        }
        refresh();
        y -= 0.095f;
    }

    protected void Choice(Transform p, ref float y, string label, string[] options, Func<int> get, Action<int> set)
    {
        UiKit.Text(p, "Libelle", new Vector2(-HW, y), 0.0105f, UiKit.White, label, TextAnchor.MiddleLeft);
        var buttons = new List<MenuButton>();
        Action refresh = () => { int sel = get(); for (int i = 0; i < buttons.Count; i++) buttons[i].selected = i == sel; };
        float right = HW;
        for (int i = options.Length - 1; i >= 0; i--)
        {
            float w = 0.07f + options[i].Length * 0.027f;
            int k = i;
            var b = MakeButton(p, new Vector2(right - w / 2f, y), new Vector2(w, 0.072f), options[i], 0.0078f, null);
            b.onClick = () => { set(k); refresh(); };
            buttons.Insert(0, b);
            right -= w + 0.02f;
        }
        refresh();
        y -= 0.095f;
    }
}
