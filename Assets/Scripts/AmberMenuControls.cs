using System;
using System.Collections.Generic;
using UnityEngine;

// Page COMMANDES des menus ambre (menu d'accueil et menu pause) : schéma des manettes Quest 3 ou du clavier et de la souris,
// avec des traits de rappel vers l'action de chaque bouton. Les textes suivent les réglages en cours
// (téléportation ou déplacement continu, prise en un clic ou en maintenant, rotation par à-coups ou fluide) :
// ils sont relus à chaque ouverture de la page (RefreshControls).
// Dessin des manettes : Resources/Commandes/manette_gauche.png et manette_droite.png (Tools/make_controller_images.py).
public abstract partial class AmberMenu
{
    // points visés sur les images des manettes (u, v depuis le bas à gauche), donnés par Tools/make_controller_images.py
    static readonly Vector2 PtTrigger = new Vector2(0.508f, 0.924f), PtGrip = new Vector2(0.375f, 0.297f),
                            PtStick = new Vector2(0.383f, 0.698f), PtLow = new Vector2(0.629f, 0.609f),
                            PtHigh = new Vector2(0.688f, 0.745f), PtSmall = new Vector2(0.488f, 0.542f);
    const float PadH = 0.5f, PadW = PadH * 512f / 768f, PadX = 0.165f, PadY = 0.075f;   // taille et place des manettes
    const float LabelX = 0.37f;                                                        // colonne des libellés (± depuis le centre)
    const float Tc = 0.0046f;                                                          // taille des textes du schéma

    readonly List<(TextMesh text, Func<string> get)> controlTexts = new List<(TextMesh, Func<string>)>();
    Transform padPage, keysPage;

    protected Transform BuildControls(Action onBack)
    {
        var p = Panel("Page_Commandes", "BUNKER-OS  //  COMMANDES", "");
        float y = 0.49f;
        Choice(p, ref y, "Commandes", new[] { "MANETTES", "CLAVIER + SOURIS" }, () => keysPage && keysPage.gameObject.activeSelf ? 1 : 0, ShowControlsTab);

        padPage = new GameObject("Manettes").transform;
        padPage.SetParent(p, false);
        BuildPads(padPage);
        keysPage = new GameObject("Clavier").transform;
        keysPage.SetParent(p, false);
        BuildKeys(keysPage);
        ShowControlsTab(DesktopMode.On ? 1 : 0);

        var back = MakeButton(p, new Vector2(-HW + 0.17f, -0.53f), new Vector2(0.34f, 0.09f), "◂  RETOUR", 0.0095f, onBack);
        back.textColor = A;
        return p;
    }

    void ShowControlsTab(int i)
    {
        padPage.gameObject.SetActive(i == 0);
        keysPage.gameObject.SetActive(i == 1);
    }

    /// <summary>Relit les réglages (déplacement, prise, rotation) et met les textes de la page à jour.</summary>
    protected void RefreshControls()
    {
        foreach (var (text, get) in controlTexts) text.text = get();
    }

    // ---------- manettes ----------
    void BuildPads(Transform p)
    {
        var tex = new[] { Resources.Load<Texture2D>("Commandes/manette_gauche"), Resources.Load<Texture2D>("Commandes/manette_droite") };
        for (int side = 0; side < 2; side++)
        {
            var c = new Vector2(side == 0 ? -PadX : PadX, PadY);
            if (tex[side]) UiKit.Quad(p, side == 0 ? "Manette_Gauche" : "Manette_Droite", c, new Vector2(PadW, PadH), A, -1, 0.004f, tex[side]);
            UiKit.Text(p, "Main", c + new Vector2(0f, -PadH / 2f - 0.025f), Tc, UiKit.Dim, side == 0 ? "MAIN GAUCHE" : "MAIN DROITE", TextAnchor.MiddleCenter, true);
        }

        // main gauche (libellés à gauche) ; les points des images sont ceux de la manette droite, en miroir.
        // Le trait du joystick part sous le centre du capuchon et file à l'horizontale, entre les deux boutons.
        var stick = PtStick - new Vector2(0f, 0.022f);
        Callout(p, false, PtTrigger, 0.35f, "GÂCHETTE", () => "Utiliser l'objet tenu\nValider dans les menus");
        Callout(p, false, stick, StickY, "JOYSTICK", () => GameSettings.Teleport
            ? "Avant : se téléporter\nCôtés : tourner\nEnfoncer : s'accroupir"
            : "Se déplacer\nEnfoncer : s'accroupir");
        Callout(p, false, PtLow, 0.05f, "X", () => "Maintenir X + A (2 s) :\npasser le tutoriel");
        Callout(p, false, PtSmall, -0.06f, "MENU", () => "Pause");
        Callout(p, false, PtGrip, -0.17f, "GRIP", GripText);

        // main droite (libellés à droite)
        Callout(p, true, PtTrigger, 0.35f, "GÂCHETTE", () => "Tirer · utiliser l'objet tenu\nValider dans les menus");
        Callout(p, true, PtHigh, 0.26f, "B", () => "Maintenir : vague suivante");
        Callout(p, true, stick, StickY, "JOYSTICK", () => (GameSettings.SmoothTurn ? "Côtés : tourner (fluide)" : "Côtés : tourner (à-coups)")
            + (GameSettings.Teleport ? "\nAvant : se téléporter" : ""));
        Callout(p, true, PtLow, 0.05f, "A", () => "Sauter");
        Callout(p, true, PtGrip, -0.09f, "GRIP", GripText);

        UiKit.Text(p, "Ceinture", new Vector2(0f, -0.33f), Tc, UiKit.Dim,
                   "Ceinture : pistolet à droite, chargeur à gauche. Marteau sur l'établi.\n"
                   + "Recharger : glisser le chargeur sous la poignée du pistolet.");
    }

    static float StickY => PadY + (PtStick.y - 0.022f - 0.5f) * PadH;

    static string GripText() => GameSettings.HoldToGrab
        ? "Prendre : maintenir enfoncé\nLevier, porte, écran : idem"
        : "Prendre / lâcher : un appui\nLevier, porte, écran : idem";

    // trait de rappel d'un point de la manette (gauche ou droite) jusqu'à son libellé, à la hauteur labelY
    void Callout(Transform p, bool right, Vector2 uv, float labelY, string button, Func<string> action)
    {
        if (!right) uv.x = 1f - uv.x;
        var c = new Vector2(right ? PadX : -PadX, PadY);
        var from = c + new Vector2((uv.x - 0.5f) * PadW, (uv.y - 0.5f) * PadH);
        float sx = right ? 1f : -1f;
        var elbow = new Vector2(sx * (LabelX - 0.03f), labelY);
        var col = new Color(A.r, A.g, A.b, 0.75f);
        UiKit.Quad(p, "Point", from, new Vector2(0.012f, 0.012f), A, -1, 0f);
        Line(p, from, elbow, 0.0025f, col);
        Line(p, elbow, new Vector2(sx * (LabelX - 0.008f), labelY), 0.0025f, col);
        Label(p, new Vector2(sx * LabelX, labelY), right ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight, button, action);
    }

    void Label(Transform p, Vector2 pos, TextAnchor anchor, string title, Func<string> action)
    {
        var tm = UiKit.Text(p, "Libelle_" + title, pos, Tc, UiKit.White, "", anchor);
        Func<string> get = () => "<b><color=#FFBF45>" + title + "</color></b>\n" + action();
        tm.text = get();
        controlTexts.Add((tm, get));
    }

    // trait quelconque (quad tourné) entre deux points du plan du panneau
    static void Line(Transform p, Vector2 a, Vector2 b, float thickness, Color col)
    {
        var d = b - a;
        var r = UiKit.Quad(p, "Trait", (a + b) / 2f, new Vector2(d.magnitude + thickness, thickness), col, -1, 0.002f);
        r.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
    }

    // ---------- clavier et souris (version PC : DesktopPlayer) ----------
    void BuildKeys(Transform p)
    {
        // déplacement : Z Q S D en T inversé (touches physiques W A S D)
        const float k = 0.07f, gap = 0.008f;
        var zq = new Vector2(-0.6f, 0.33f);                // bord gauche de Z et S
        Key(p, zq + new Vector2(0f, (k + gap) / 2f), "Z");
        Key(p, zq + new Vector2(-k - gap, -(k + gap) / 2f), "Q");
        Key(p, zq + new Vector2(0f, -(k + gap) / 2f), "S");
        Key(p, zq + new Vector2(k + gap, -(k + gap) / 2f), "D");
        Label(p, new Vector2(-0.43f, 0.33f), TextAnchor.MiddleLeft, "ZQSD", () => "Se déplacer\n(W A S D en QWERTY)");

        float y = 0.17f;
        Row(p, -0.68f, ref y, new[] { "ESPACE" }, "ESPACE", "Sauter · valider (menus)");
        Row(p, -0.68f, ref y, new[] { "C" }, "C", "S'accroupir");
        Row(p, -0.68f, ref y, new[] { "ÉCHAP" }, "ÉCHAP", "Pause");
        Row(p, -0.68f, ref y, new[] { "B" }, "B OU ENTRÉE", "Maintenir : vague suivante");

        y = 0.37f;
        Row(p, -0.1f, ref y, new[] { "E" }, "E", "Prendre / lâcher (main droite)");
        Row(p, -0.1f, ref y, new[] { "F" }, "F", "Utiliser l'objet de la main\ngauche (lampe)");
        Row(p, -0.1f, ref y, new[] { "R" }, "R", "Recharger (chargeur, pile)");
        Row(p, -0.1f, ref y, new[] { "1", "2", "3" }, "1  2  3", "Pistolet, marteau\n(à portée), lampe");
        Row(p, -0.1f, ref y, new[] { "N" }, "N", "Tutoriel : passer une étape");
        Row(p, -0.1f, ref y, new[] { "F1" }, "F1", "Aide à l'écran");

        // souris : corps, deux boutons (G et D), libellés dessous
        var m = new Vector2(0.57f, 0.3f);
        var body = new Vector2(0.13f, 0.2f);
        var fill = new Color(A.r, A.g, A.b, 0.25f);
        UiKit.Frame(p, m, body, 0.005f, A, -1, 0.004f);
        UiKit.Quad(p, "Souris_Milieu", m + new Vector2(0f, body.y / 4f), new Vector2(0.005f, body.y / 2f), A, -1, 0.004f);
        UiKit.Quad(p, "Souris_Boutons", m, new Vector2(body.x, 0.005f), A, -1, 0.004f);
        for (int i = -1; i <= 1; i += 2)
        {
            var c = m + new Vector2(i * body.x / 4f, body.y / 4f);
            UiKit.Quad(p, "Souris_Bouton", c, new Vector2(body.x / 2f, body.y / 2f), fill, -1, 0.005f);
            UiKit.Text(p, "Souris_Lettre", c, 0.0062f, UiKit.White, i < 0 ? "G" : "D", TextAnchor.MiddleCenter, true);
        }
        Label(p, new Vector2(0.43f, 0.12f), TextAnchor.MiddleLeft, "CLIC GAUCHE (G)", () => "Tirer, frapper, appuyer\nPrendre (main vide)");
        Label(p, new Vector2(0.43f, 0f), TextAnchor.MiddleLeft, "CLIC DROIT (D)", () => "Prendre / lâcher\n(main gauche)");
        Label(p, new Vector2(0.43f, -0.12f), TextAnchor.MiddleLeft, "SOURIS", () => "Regarder, viser\n(réticule au centre)");

        UiKit.Text(p, "Astuce", new Vector2(0f, -0.33f), Tc, UiKit.Dim,
                   "Levier, porte : E pour l'attraper, puis regarder ou reculer.\n"
                   + "Tutoriel : maintenir Maj + Espace pour le passer.  Cliquez dans la fenêtre pour capturer la souris.");
    }

    void Row(Transform p, float x, ref float y, string[] keys, string title, string action)
    {
        float kx = x;
        foreach (var key in keys) kx += Key(p, new Vector2(kx, y), key) + 0.01f;
        Label(p, new Vector2(Mathf.Max(kx + 0.01f, x + 0.1f), y), TextAnchor.MiddleLeft, title, () => action);
        y -= 0.1f;
    }

    // touche dessinée (cadre + lettre), posée par son bord gauche en x ; renvoie sa largeur
    float Key(Transform p, Vector2 leftCenter, string key)
    {
        float w = Mathf.Max(0.07f, 0.035f + key.Length * 0.024f);
        var c = leftCenter + new Vector2(w / 2f, 0f);
        UiKit.Quad(p, "Touche_Fond", c, new Vector2(w, 0.07f), new Color(A.r, A.g, A.b, 0.12f), -1, 0.005f);
        UiKit.Frame(p, c, new Vector2(w, 0.07f), 0.004f, A, -1, 0.004f);
        UiKit.Text(p, "Touche_" + key, c, 0.0058f, UiKit.White, key, TextAnchor.MiddleCenter, true);
        return w;
    }
}
