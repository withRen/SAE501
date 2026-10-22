using System;
using System.Collections.Generic;
using UnityEngine;

// Page COMMANDES des menus ambre (menu d'accueil et menu pause) : schéma des manettes Quest 3,
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
    Transform padPage;

    protected Transform BuildControls(Action onBack)
    {
        var p = Panel("Page_Commandes", "BUNKER-OS  //  COMMANDES", "");
        padPage = new GameObject("Manettes").transform;
        padPage.SetParent(p, false);
        BuildPads(padPage);

        var back = MakeButton(p, new Vector2(-HW + 0.17f, -0.53f), new Vector2(0.34f, 0.09f), "◂  RETOUR", 0.0095f, onBack);
        back.textColor = A;
        return p;
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
                   + "Recharger : glisser le chargeur sous la poignée du pistolet.\n"
                   + "Balles, pile : objet en main près du visage, canon / faisceau vers la gauche.");
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
}
