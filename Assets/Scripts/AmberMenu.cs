using System;
using System.Collections.Generic;
using UnityEngine;

// Base commune des menus « terminal de bunker » ambre (menu d'accueil MainMenu, menu pause PauseMenu) :
// panneau, gros boutons, page PARAMÈTRES (son, confort VR ou souris et affichage en version PC, difficulté), page COMMANDES (AmberMenuControls.cs)
// et briques (curseurs, choix).
// Tout est construit en code (UiKit) ; le pointeur est MenuPointer.
public abstract partial class AmberMenu : MonoBehaviour
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
        // version PC : deux lignes de plus (VSync, limite FPS) ; lignes un peu plus serrées pour garder RETOUR en bas
        if (DesktopMode.On) { rowStep = 0.082f; sectionStep = 0.078f; }
        try { return BuildSettingsPage(onBack); }
        finally { rowStep = 0.095f; sectionStep = 0.085f; }
    }

    float rowStep = 0.095f, sectionStep = 0.085f;

    Transform BuildSettingsPage(Action onBack)
    {
        var p = Panel("Page_Parametres", "BUNKER-OS  //  PARAMÈTRES", "");
        float y = 0.49f;
        Section(p, ref y, "SON");
        Slider(p, ref y, "Musique", () => GameSettings.Music, v => GameSettings.Music = v);
        Slider(p, ref y, "Effets", () => GameSettings.Effects, v => GameSettings.Effects = v);
        Slider(p, ref y, "Zombies", () => GameSettings.Zombies, v => GameSettings.Zombies = v);
        if (DesktopMode.On)
        {
            // version PC : les réglages du casque n'ont pas de sens ; souris et champ de vision à la place
            Section(p, ref y, "SOURIS ET AFFICHAGE");
            FloatInput(p, ref y, "Sensibilité", () => GameSettings.MouseSensitivity, v => GameSettings.MouseSensitivity = v, 0.05f, 10f);
            Choice(p, ref y, "Souris inversée", new[] { "NON", "OUI" }, () => GameSettings.InvertMouseY ? 1 : 0, i => GameSettings.InvertMouseY = i == 1);
            var fovs = GameSettings.FieldOfViews;
            Choice(p, ref y, "Champ de vision", System.Array.ConvertAll(fovs, f => f + "°"),
                   () => Mathf.Max(0, System.Array.IndexOf(fovs, GameSettings.FieldOfView)), i => GameSettings.FieldOfView = fovs[i]);
            // VSync (fréquence de l'écran) ou limite d'images par seconde ; la limite est grisée tant que le VSync est actif
            // (Unity ignore alors Application.targetFrameRate)
            List<MenuButton> fpsButtons = null;
            Action greyFps = () => { if (fpsButtons != null) foreach (var b in fpsButtons) b.interactable = !GameSettings.VSync; };
            Choice(p, ref y, "VSync", new[] { "NON", "OUI" }, () => GameSettings.VSync ? 1 : 0, i => { GameSettings.VSync = i == 1; greyFps(); });
            var caps = GameSettings.FrameRateCaps;
            fpsButtons = Choice(p, ref y, "Limite FPS", System.Array.ConvertAll(caps, c => c <= 0 ? "SANS" : c.ToString()),
                   () => Mathf.Max(0, System.Array.IndexOf(caps, GameSettings.FrameRateCap)), i => GameSettings.FrameRateCap = caps[i]);
            greyFps();
        }
        else
        {
            Section(p, ref y, "CONFORT VR");
            Choice(p, ref y, "Déplacement", new[] { "CONTINU", "TÉLÉPORTATION" }, () => GameSettings.Teleport ? 1 : 0, i => GameSettings.Teleport = i == 1);
            Choice(p, ref y, "Prise (grip)", new[] { "UN CLIC", "MAINTENIR" }, () => GameSettings.HoldToGrab ? 1 : 0, i => GameSettings.HoldToGrab = i == 1);
            Choice(p, ref y, "Rotation", new[] { "PAR À-COUPS", "FLUIDE" }, () => GameSettings.SmoothTurn ? 1 : 0, i => GameSettings.SmoothTurn = i == 1);
            Choice(p, ref y, "Bords sombres", new[] { "OUI", "NON" }, () => GameSettings.ComfortVignette ? 0 : 1, i => GameSettings.ComfortVignette = i == 0);
            // RECENTRER est descendu à côté de RETOUR : la ligne « Déplacement » a pris sa place
            var re = MakeButton(p, new Vector2(HW - 0.17f, -0.53f), new Vector2(0.34f, 0.09f), "RECENTRER", 0.0095f, GameSettings.Recenter);
            re.textColor = A;
        }
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
        // fond dessiné avant tout le reste de la page (même file, il recouvrait les éléments éloignés du centre, triés plus loin que lui)
        UiKit.Quad(p, "Fond_Page", Vector2.zero, PanelSize, PanelCol, 2990, 0.012f);
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
        y -= sectionStep;
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
        y -= rowStep;
    }

    protected List<MenuButton> Choice(Transform p, ref float y, string label, string[] options, Func<int> get, Action<int> set)
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
        y -= rowStep;
        return buttons;
    }

    // champ numérique éditable au clavier (clic gauche pour saisir directement un float, Entrée ou clic extérieur pour valider)
    protected void FloatInput(Transform p, ref float y, string label, Func<float> get, Action<float> set, float min, float max)
    {
        UiKit.Text(p, "Libelle", new Vector2(-HW, y), 0.0105f, UiKit.White, label, TextAnchor.MiddleLeft);
        const float w = 0.24f;
        var b = MakeButton(p, new Vector2(HW - w / 2f, y), new Vector2(w, 0.072f), "", 0.0088f, null);
        var field = b.gameObject.AddComponent<MenuFloatField>();
        field.Init(b, get, set, min, max);
        y -= rowStep;
    }
}

// Champ de saisie décimale (float) pour un bouton de menu BUNKER-OS :
// cliquer active la saisie au clavier (chiffres, '.' ou ',', Retour arrière, Entrée ou clic ailleurs pour valider).
public class MenuFloatField : MonoBehaviour
{
    MenuButton button;
    Func<float> getter;
    Action<float> setter;
    float minVal, maxVal;
    bool editing, freshEdit;
    string buffer = "";
    int editStartFrame;

    public void Init(MenuButton b, Func<float> get, Action<float> set, float min, float max)
    {
        button = b;
        getter = get;
        setter = set;
        minVal = min;
        maxVal = max;
        button.onClick = ToggleEdit;
        RefreshLabel();
    }

    static string Format(float v) => v.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

    void RefreshLabel()
    {
        if (!button || !button.label || getter == null) return;
        if (!editing)
            button.label.text = Format(getter());
        else
        {
            bool caret = Mathf.Repeat(Time.unscaledTime, 0.9f) < 0.45f;
            button.label.text = buffer + (caret ? "|" : " ");
        }
    }

    void ToggleEdit()
    {
        if (editing) Commit();
        else StartEdit();
    }

    void StartEdit()
    {
        editing = true;
        freshEdit = true;
        editStartFrame = Time.frameCount;
        buffer = Format(getter());
        if (button) button.selected = true;
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            kb.onTextInput -= OnTextInput;
            kb.onTextInput += OnTextInput;
        }
        RefreshLabel();
    }

    void StopListening()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null) kb.onTextInput -= OnTextInput;
    }

    void OnDisable()
    {
        if (editing) Commit();
        else StopListening();
    }

    void OnDestroy() => StopListening();

    void OnTextInput(char c)
    {
        if (!editing) return;
        if (c >= '0' && c <= '9')
        {
            if (freshEdit) { buffer = ""; freshEdit = false; }
            if (buffer.Length < 6) buffer += c;
        }
        else if (c == '.' || c == ',')
        {
            if (freshEdit) { buffer = "0."; freshEdit = false; }
            else if (!buffer.Contains(".") && !buffer.Contains(",") && buffer.Length < 5)
                buffer += buffer.Length == 0 ? "0." : ".";
        }
    }

    void Update()
    {
        if (!editing) return;

        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            if (kb.backspaceKey.wasPressedThisFrame)
            {
                if (freshEdit) { buffer = ""; freshEdit = false; }
                else if (buffer.Length > 0) buffer = buffer.Substring(0, buffer.Length - 1);
            }
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)
            {
                Commit();
                return;
            }
        }

        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && Time.frameCount > editStartFrame)
        {
            Commit();
            return;
        }

        RefreshLabel();
    }

    void Commit()
    {
        if (!editing) return;
        editing = false;
        StopListening();
        if (button) button.selected = false;

        string norm = buffer.Replace(',', '.').Trim();
        if (float.TryParse(norm, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v))
        {
            v = Mathf.Clamp(v, minVal, maxVal);
            setter?.Invoke(v);
            PlayerPrefs.Save();
        }
        RefreshLabel();
    }
}
