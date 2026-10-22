using System;
using UnityEngine;
using UnityEngine.UI;

// Panneau du tutoriel posé DANS la carte (Canvas uGUI en espace monde) : il apparaît à côté de l'objet de l'étape
// (planches, fenêtre, disjoncteur, écrans, cible…), tourné vers le joueur, relié à l'objet par un trait.
// Pour les objets de la ceinture (qui bougent avec le joueur), il est posé devant lui au début de l'étape.
public class TutorialBoard : MonoBehaviour
{
    static readonly Color Amber = new Color(1f, 0.75f, 0.27f);
    static readonly Color Ink = new Color(0.03f, 0.04f, 0.035f, 0.94f);
    static readonly Color TextCol = new Color(0.92f, 0.95f, 0.93f);
    static readonly Color Dim = new Color(0.55f, 0.65f, 0.58f);
    static readonly Color Ok = new Color(0.35f, 1f, 0.55f);
    const float Scale = 0.0013f;          // 1 px du canvas = 1,3 mm
    static Font font;
    static Material lineMat;

    Transform head;
    Text title, body, footer;
    Image band;
    LineRenderer line;
    bool followHead;
    float spawn;

    public static TutorialBoard Create(Transform head, string step, string label, string text, Vector3? anchor, Action onSkip = null)
    {
        if (!font) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go = new GameObject("Tutoriel_Panneau_" + label);
        var b = go.AddComponent<TutorialBoard>();
        b.head = head;
        b.Build(step, label, text);
        b.BuildCross(onSkip);
        b.Place(anchor);
        return b;
    }

    void Build(string step, string label, string text)
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;
        var rt = (RectTransform)transform;
        rt.sizeDelta = new Vector2(900, 470);
        transform.localScale = Vector3.one * Scale;           // 1,17 × 0,61 m

        Img("Fond", new Vector2(0, 0), new Vector2(900, 470), Ink);
        band = Img("Bande", new Vector2(0, 205), new Vector2(900, 60), Amber);
        Img("Cadre_Bas", new Vector2(0, -233), new Vector2(900, 4), Amber);
        title = Txt("Titre", new Vector2(-420, 205), new Vector2(840, 60), 34, FontStyle.Bold, new Color(0.05f, 0.04f, 0.02f), TextAnchor.MiddleLeft);
        title.text = step + "   ·   " + label;
        body = Txt("Texte", new Vector2(0, 10), new Vector2(840, 300), 40, FontStyle.Normal, TextCol, TextAnchor.MiddleCenter);
        body.text = text;
        footer = Txt("Aide", new Vector2(0, -200), new Vector2(860, 40), 24, FontStyle.Normal, Dim, TextAnchor.MiddleCenter);
        footer.text = "Passer le tutoriel : visez la croix et appuyez sur la gâchette (main vide), ou A + X pendant 2 s";

        if (!lineMat) { lineMat = new Material(Shader.Find("Universal Render Pipeline/Unlit")); lineMat.SetColor("_BaseColor", Amber); }
        var lg = new GameObject("Trait");
        line = lg.AddComponent<LineRenderer>();
        line.sharedMaterial = lineMat;
        line.widthMultiplier = 0.008f;
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        spawn = Time.time;
    }

    // croix « passer le tutoriel » à droite de la bande du titre (TutorialSkipCross : visée + gâchette)
    void BuildCross(Action onSkip)
    {
        var g = new GameObject("Croix", typeof(RectTransform));
        g.transform.SetParent(transform, false);
        var r = (RectTransform)g.transform; r.anchoredPosition = new Vector2(415, 205); r.sizeDelta = new Vector2(60, 60);
        var bg = Img("Croix_Fond", new Vector2(415, 205), new Vector2(54, 54), new Color(0.05f, 0.04f, 0.02f, 0.85f));
        var bars = new Image[2];
        for (int i = 0; i < 2; i++)
        {
            bars[i] = Img("Croix_Trait", new Vector2(415, 205), new Vector2(40, 7), Amber);
            bars[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, i == 0 ? 45f : -45f);
        }
        var cross = g.AddComponent<TutorialSkipCross>();
        cross.half = 40f;                     // zone visée un peu plus grande que la croix (10 cm)
        cross.onClick = onSkip;
        cross.onHover = h =>
        {
            bg.color = h ? new Color(0.85f, 0.2f, 0.15f, 0.95f) : new Color(0.05f, 0.04f, 0.02f, 0.85f);
            foreach (var b in bars) b.color = h ? Color.white : Amber;
        };
    }

    Image Img(string n, Vector2 pos, Vector2 size, Color c)
    {
        var g = new GameObject(n, typeof(RectTransform));
        g.transform.SetParent(transform, false);
        var r = (RectTransform)g.transform; r.anchoredPosition = pos; r.sizeDelta = size;
        var i = g.AddComponent<Image>(); i.color = c; i.raycastTarget = false;
        return i;
    }

    Text Txt(string n, Vector2 pos, Vector2 size, int fs, FontStyle st, Color c, TextAnchor a)
    {
        var g = new GameObject(n, typeof(RectTransform));
        g.transform.SetParent(transform, false);
        var r = (RectTransform)g.transform; r.anchoredPosition = pos + (a == TextAnchor.MiddleLeft ? new Vector2(size.x / 2f, 0) : Vector2.zero); r.sizeDelta = size;
        var t = g.AddComponent<Text>(); t.font = font; t.fontSize = fs; t.fontStyle = st; t.color = c; t.alignment = a;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.raycastTarget = false;
        return t;
    }

    // panneau à côté de l'objet, vers le joueur et un peu au-dessus ; objet porté par le joueur (ceinture) : devant lui
    void Place(Vector3? anchor)
    {
        var hp = head ? head.position : Vector3.up * 1.6f;
        var hf = head ? Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized : Vector3.forward;
        if (hf.sqrMagnitude < 0.01f) hf = Vector3.forward;
        Vector3 pos;
        if (!anchor.HasValue || Flat(anchor.Value - hp) < 0.7f)
        {
            followHead = !anchor.HasValue;
            pos = hp + hf * 1.2f + Vector3.down * 0.15f;
        }
        else
        {
            var a = anchor.Value;
            var toPlayer = Vector3.ProjectOnPlane(hp - a, Vector3.up);
            toPlayer = toPlayer.sqrMagnitude > 0.01f ? toPlayer.normalized : -hf;
            pos = a + toPlayer * 0.5f + Vector3.up * 0.65f;
            pos.y = Mathf.Clamp(pos.y, 1.15f, 2.05f);
        }
        transform.position = pos;
        Face();
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }

    void Face()
    {
        if (!head) return;
        var d = Vector3.ProjectOnPlane(transform.position - head.position, Vector3.up);
        if (d.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(d.normalized);
    }

    public void SetText(string text) { if (body.text != text) body.text = text; }

    // trait du bas du panneau vers l'objet (mis à jour : l'objet peut bouger)
    public void Point(Vector3? anchor)
    {
        bool on = anchor.HasValue && Vector3.Distance(anchor.Value, transform.position) > 0.35f;
        line.enabled = on;
        if (on)
        {
            line.SetPosition(0, transform.position + Vector3.down * 235f * Scale);
            line.SetPosition(1, anchor.Value);
        }
    }

    public void Success()
    {
        band.color = Ok;
        body.color = Ok;
        body.text = "Bien joué !";
        line.enabled = false;
        Destroy(gameObject, 1.2f);
    }

    void LateUpdate()
    {
        if (followHead && head)
        {
            var hf = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            var goal = head.position + hf * 1.2f + Vector3.down * 0.15f;
            transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-3f * Time.deltaTime));
        }
        Face();
        // apparition : petit zoom
        float k = Mathf.Clamp01((Time.time - spawn) / 0.25f);
        transform.localScale = Vector3.one * Scale * Mathf.Lerp(0.6f, 1f, 1f - (1f - k) * (1f - k));
    }

    void OnDestroy() { if (line) Destroy(line.gameObject); }
}
