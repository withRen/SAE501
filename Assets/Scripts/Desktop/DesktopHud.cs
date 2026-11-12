using UnityEngine;
using UnityEngine.UI;

// Affichage de la version PC (par-dessus l'image, uGUI en Screen Space Overlay ; jamais créé au casque) :
//   - réticule au centre : 4 petits traits + un point ; il s'ouvre et passe en ambre quand on vise quelque chose d'utilisable ;
//   - aide sous le réticule : touche + action (« E  Prendre le pistolet », « E  Poser la planche », « CLIC  Frapper ») ;
//   - en bas à droite : munitions du pistolet en main (et chargeur à la ceinture), charge de la lampe ;
//   - messages courts (« Aucun chargeur à la ceinture ») ; « Cliquez pour reprendre » quand la souris n'est pas capturée ;
//   - F1 : liste des touches (rappel discret « F1 · commandes » en haut à gauche).
// Caché en pause et à la fin de la partie.
public class DesktopHud : MonoBehaviour
{
    static readonly Color Amber = new Color(1f, 0.75f, 0.27f);
    static readonly Color White = new Color(1f, 1f, 1f, 0.9f);
    static readonly Color Ink = new Color(0.04f, 0.035f, 0.03f, 0.78f);
    static DesktopHud instance;

    DesktopPlayer player;
    Font font;
    RectTransform cross, dot;
    readonly Image[] ticks = new Image[4];
    Image dotImg, keyBox;
    Text keyText, hintText, ammoText, ammoSub, messageText, clickText, helpTip, helpText;
    GameObject hintRoot, ammoRoot, helpPanel;
    float gap = 7f, messageUntil;
    bool help;
    CanvasGroup group;

    public static void Create(DesktopPlayer p)
    {
        if (instance) Destroy(instance.gameObject);
        var go = new GameObject("PC_Hud");
        instance = go.AddComponent<DesktopHud>();
        instance.player = p;
        instance.Build();
    }

    /// <summary>Message court au-dessus de l'aide (disparaît tout seul).</summary>
    public static void Message(string text, float seconds = 1.8f)
    {
        if (!instance) return;
        instance.messageText.text = text;
        instance.messageUntil = Time.unscaledTime + seconds;
    }

    void Build()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f;
        group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false; group.interactable = false;
        var root = (RectTransform)transform;

        // réticule
        cross = Rect("Reticule", root, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        for (int i = 0; i < 4; i++)
        {
            ticks[i] = Img("Trait", cross, Vector2.zero, i < 2 ? new Vector2(2f, 9f) : new Vector2(9f, 2f), White);
            Shadow(ticks[i].gameObject);
        }
        dotImg = Img("Point", cross, Vector2.zero, new Vector2(3f, 3f), White);
        Shadow(dotImg.gameObject);

        // aide : [E] action
        hintRoot = new GameObject("Aide", typeof(RectTransform));
        var hr = (RectTransform)hintRoot.transform;
        hr.SetParent(root, false);
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0.5f);
        hr.anchoredPosition = new Vector2(0f, -64f);
        keyBox = Img("Touche", hr, Vector2.zero, new Vector2(34f, 30f), Amber);
        keyText = Txt("Lettre", keyBox.rectTransform, Vector2.zero, new Vector2(80f, 30f), 18, FontStyle.Bold, new Color(0.08f, 0.06f, 0.03f), TextAnchor.MiddleCenter);
        hintText = Txt("Action", hr, Vector2.zero, new Vector2(600f, 30f), 21, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
        Shadow(hintText.gameObject);

        messageText = Txt("Message", root, new Vector2(0f, -112f), new Vector2(900f, 30f), 19, FontStyle.Normal, new Color(1f, 0.85f, 0.6f), TextAnchor.MiddleCenter);
        Shadow(messageText.gameObject);
        clickText = Txt("Cliquer", root, new Vector2(0f, 60f), new Vector2(900f, 40f), 24, FontStyle.Bold, Amber, TextAnchor.MiddleCenter);
        clickText.text = "Cliquez pour reprendre le contrôle";
        Shadow(clickText.gameObject);

        // munitions / lampe (bas droite)
        ammoRoot = new GameObject("Munitions", typeof(RectTransform));
        var ar = (RectTransform)ammoRoot.transform;
        ar.SetParent(root, false);
        ar.anchorMin = ar.anchorMax = ar.pivot = new Vector2(1f, 0f);
        ar.anchoredPosition = new Vector2(-48f, 40f);
        ar.sizeDelta = new Vector2(360f, 110f);
        var bg = Img("Fond", ar, new Vector2(-180f, 55f), new Vector2(360f, 110f), new Color(0f, 0f, 0f, 0.35f));
        bg.rectTransform.anchorMin = bg.rectTransform.anchorMax = new Vector2(1f, 0f);
        Img("Bord", ar, new Vector2(-358f, 55f), new Vector2(3f, 110f), Amber).rectTransform.anchorMin = new Vector2(1f, 0f);
        ammoText = Txt("Balles", ar, new Vector2(-180f, 66f), new Vector2(330f, 60f), 46, FontStyle.Bold, Color.white, TextAnchor.MiddleRight);
        ammoText.rectTransform.anchorMin = ammoText.rectTransform.anchorMax = new Vector2(1f, 0f);
        ammoSub = Txt("Detail", ar, new Vector2(-180f, 22f), new Vector2(330f, 30f), 17, FontStyle.Normal, new Color(1f, 1f, 1f, 0.7f), TextAnchor.MiddleRight);
        ammoSub.rectTransform.anchorMin = ammoSub.rectTransform.anchorMax = new Vector2(1f, 0f);
        Shadow(ammoText.gameObject);

        // aide F1
        helpTip = Txt("F1", root, Vector2.zero, new Vector2(400f, 26f), 16, FontStyle.Normal, new Color(1f, 1f, 1f, 0.55f), TextAnchor.UpperLeft);
        var tr = helpTip.rectTransform; tr.anchorMin = tr.anchorMax = tr.pivot = new Vector2(0f, 1f); tr.anchoredPosition = new Vector2(24f, -20f);
        helpTip.text = "F1  ·  commandes";
        Shadow(helpTip.gameObject);
        helpPanel = new GameObject("Commandes", typeof(RectTransform));
        var hp = (RectTransform)helpPanel.transform;
        hp.SetParent(root, false);
        hp.anchorMin = hp.anchorMax = hp.pivot = new Vector2(0f, 1f);
        hp.anchoredPosition = new Vector2(20f, -50f);
        hp.sizeDelta = new Vector2(470f, 520f);
        var pbg = Img("Fond", hp, new Vector2(235f, -260f), new Vector2(470f, 520f), Ink);
        pbg.rectTransform.anchorMin = pbg.rectTransform.anchorMax = new Vector2(0f, 1f);
        var band = Img("Bande", hp, new Vector2(235f, -18f), new Vector2(470f, 36f), Amber);
        band.rectTransform.anchorMin = band.rectTransform.anchorMax = new Vector2(0f, 1f);
        var title = Txt("Titre", hp, new Vector2(235f, -18f), new Vector2(440f, 36f), 18, FontStyle.Bold, new Color(0.08f, 0.06f, 0.03f), TextAnchor.MiddleLeft);
        title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0f, 1f);
        title.text = "COMMANDES  ·  CLAVIER + SOURIS";
        helpText = Txt("Liste", hp, new Vector2(235f, -275f), new Vector2(430f, 470f), 17, FontStyle.Normal, Color.white, TextAnchor.UpperLeft);
        helpText.rectTransform.anchorMin = helpText.rectTransform.anchorMax = new Vector2(0f, 1f);
        helpText.lineSpacing = 1.12f;
        helpText.supportRichText = true;
        helpText.text = HelpText;
        helpPanel.SetActive(false);
    }

    /// <summary>Liste des touches de la version PC (aide F1, page COMMANDES).</summary>
    public const string HelpText =
        "<b>ZQSD</b>   se déplacer\n" +
        "<b>Souris</b>   regarder, viser (réticule)\n" +
        "<b>Espace</b>   sauter      <b>Ctrl</b> (ou C)   s'accroupir\n" +
        "<b>E</b>   prendre, poser une planche, utiliser\n" +
        "      (levier : <b>maintenir E</b>)\n" +
        "<b>Clic gauche</b>   tirer, frapper, clouer\n" +
        "<b>Clic droit</b>   viser au pistolet\n" +
        "<b>R</b>   recharger (chargeur de la ceinture)\n" +
        "<b>1</b> pistolet   <b>2</b> marteau   <b>3</b> lampe   <b>4</b> mêlée\n" +
        "<b>Molette</b>   objet suivant   <b>G</b>   lâcher\n" +
        "<b>F</b>   allumer la lampe   <b>V</b>   inspecter (maintenir)\n" +
        "<b>B</b> (maintenir)   lancer la vague\n" +
        "<b>Échap</b>   pause (souris libre)\n" +
        "<b>T</b>   revoir le tutoriel   <b>F1</b>   cette aide";

    void Update()
    {
        if (!player) { Destroy(gameObject); return; }
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.f1Key.wasPressedThisFrame) help = !help;

        bool hidden = PauseMenu.Paused || DesktopPlayer.GameOver || !player.isActiveAndEnabled;
        group.alpha = Mathf.MoveTowards(group.alpha, hidden ? 0f : 1f, Time.unscaledDeltaTime * 6f);

        // réticule
        bool hov = player.Hovering;
        bool aim = player.Aiming;
        float goal = aim ? 4f : hov ? 11f : 7f;
        gap = Mathf.Lerp(gap, goal, 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime));
        var col = hov ? Amber : White;
        ticks[0].rectTransform.anchoredPosition = new Vector2(0f, gap + 4.5f);
        ticks[1].rectTransform.anchoredPosition = new Vector2(0f, -gap - 4.5f);
        ticks[2].rectTransform.anchoredPosition = new Vector2(-gap - 4.5f, 0f);
        ticks[3].rectTransform.anchoredPosition = new Vector2(gap + 4.5f, 0f);
        foreach (var t in ticks) t.color = col;
        dotImg.color = col;
        cross.gameObject.SetActive(player.MouseCaptured);

        // aide sous le réticule
        string key = null, text = null;
        if (!string.IsNullOrEmpty(player.HoverText)) { key = "E"; text = player.HoverText; }
        else
        {
            switch (player.RightKind)
            {
                case DesktopPlayer.Kind.Hammer: key = "CLIC"; text = "Frapper (clouer)"; break;
                case DesktopPlayer.Kind.Melee: key = "CLIC"; text = "Frapper"; break;
                case DesktopPlayer.Kind.Battery: key = "R"; text = "Recharger la lampe"; break;
                case DesktopPlayer.Kind.Plank: key = "E"; text = "Visez une fenêtre pour poser la planche"; break;
                case DesktopPlayer.Kind.Pistol:
                    var p = player.HeldPistol;
                    if (p && p.ammo <= 0) { key = "R"; text = "Recharger"; }
                    break;
            }
        }
        hintRoot.SetActive(key != null && player.MouseCaptured);
        if (key != null)
        {
            keyText.text = key;
            float kw = Mathf.Max(30f, 14f + key.Length * 13f);
            hintText.text = text;
            float tw = hintText.preferredWidth;
            float total = kw + 10f + tw;
            keyBox.rectTransform.sizeDelta = new Vector2(kw, 30f);
            keyBox.rectTransform.anchoredPosition = new Vector2(-total / 2f + kw / 2f, 0f);
            hintText.rectTransform.sizeDelta = new Vector2(tw + 4f, 30f);
            hintText.rectTransform.anchoredPosition = new Vector2(-total / 2f + kw + 10f + tw / 2f, 0f);
        }

        // munitions / lampe
        var pistol = player.HeldPistol;
        var lamp = player.HeldLamp;
        ammoRoot.SetActive(pistol || lamp);
        if (pistol)
        {
            ammoText.text = pistol.ammo.ToString("00") + " <size=26>/ " + pistol.maxAmmo + "</size>";
            ammoText.supportRichText = true;
            ammoText.color = pistol.ammo <= 0 ? (Mathf.Repeat(Time.time, 0.6f) < 0.3f ? new Color(1f, 0.25f, 0.2f) : new Color(1f, 0.25f, 0.2f, 0.4f))
                           : pistol.ammo <= 3 ? new Color(1f, 0.6f, 0.1f) : Color.white;
            int mags = 0;
            foreach (var s in FindObjectsByType<BeltSocket>(FindObjectsSortMode.None))
                if (s.accepts == BeltItemType.Magazine && s.hasSelection) mags++;
            ammoSub.text = (lamp ? "LAMPE " + Mathf.RoundToInt(lamp.level * 100f) + " %   ·   " : "") + (mags > 0 ? "CHARGEUR À LA CEINTURE" : "AUCUN CHARGEUR");
        }
        else if (lamp)
        {
            ammoText.text = Mathf.RoundToInt(lamp.level * 100f) + " <size=26>%</size>";
            ammoText.supportRichText = true;
            ammoText.color = lamp.level < 0.1f ? new Color(1f, 0.35f, 0.25f) : Color.white;
            ammoSub.text = "LAMPE  ·  F : allumer";
        }

        messageText.gameObject.SetActive(Time.unscaledTime < messageUntil);
        clickText.gameObject.SetActive(!player.MouseCaptured && !hidden);
        helpPanel.SetActive(help && !hidden);
        helpTip.gameObject.SetActive(!help && !hidden);
    }

    // ───────────────────────── briques uGUI ─────────────────────────
    static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size, Vector2 anchor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static Image Img(string name, Transform parent, Vector2 pos, Vector2 size, Color c)
    {
        var rt = Rect(name, parent, pos, size, new Vector2(0.5f, 0.5f));
        var img = rt.gameObject.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        return img;
    }

    Text Txt(string name, Transform parent, Vector2 pos, Vector2 size, int fs, FontStyle st, Color c, TextAnchor a)
    {
        var rt = Rect(name, parent, pos, size, new Vector2(0.5f, 0.5f));
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font; t.fontSize = fs; t.fontStyle = st; t.color = c; t.alignment = a;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static void Shadow(GameObject go)
    {
        var s = go.AddComponent<UnityEngine.UI.Shadow>();
        s.effectColor = new Color(0f, 0f, 0f, 0.75f);
        s.effectDistance = new Vector2(1.5f, -1.5f);
    }
}
