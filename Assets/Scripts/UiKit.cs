using UnityEngine;
using UnityEngine.Rendering;

// Petites briques d'interface « terminal de bunker » construites en code (pas d'asset) :
// panneaux, cadres, coins en équerre, lignes de balayage, textes. Utilisé par StartScreen, EndScreen et ScoreBoard.
// Tout est en mètres, dans le plan XY local d'un objet racine (le joueur regarde vers +Z de cette racine).
public static class UiKit
{
    public static readonly Color Amber = new Color(1f, 0.75f, 0.27f);
    public static readonly Color Green = new Color(0.35f, 1f, 0.55f);
    public static readonly Color Red = new Color(1f, 0.27f, 0.24f);
    public static readonly Color White = new Color(0.88f, 0.92f, 0.9f);
    public static readonly Color Dim = new Color(0.47f, 0.59f, 0.51f);
    public static readonly Color PanelGreen = new Color(0.024f, 0.055f, 0.04f, 0.88f);
    public static readonly Color PanelRed = new Color(0.05f, 0.024f, 0.024f, 0.9f);

    // ordre d'affichage pour les écrans collés à la tête (au-dessus du fondu au noir, 4100)
    public const int QPanel = 4250, QLine = 4260, QText = 4300;

    static Texture2D scan;

    /// <summary>Racine d'un écran collé à la tête, à `distance` m devant les yeux.</summary>
    public static Transform HeadRoot(Transform head, string name, float distance, float scale)
    {
        var t = new GameObject(name).transform;
        t.SetParent(head, false);
        t.localPosition = new Vector3(0f, 0f, distance);
        t.localScale = Vector3.one * scale;
        return t;
    }

    /// <summary>Vrai : les écrans collés à la tête passent devant les murs (sinon un mur proche les cacherait).</summary>
    public static void SetOnTop(bool on) =>
        Shader.SetGlobalFloat("unity_GUIZTestMode", on ? (float)CompareFunction.Always : (float)CompareFunction.LessEqual);

    public static Material Mat(Color c, int queue = -1, Texture tex = null)
    {
        var m = new Material(Shader.Find("UI/Default"));
        m.color = c;
        if (tex) m.mainTexture = tex;
        if (queue >= 0) m.renderQueue = queue;
        return m;
    }

    public static Renderer Quad(Transform parent, string name, Vector2 pos, Vector2 size, Color c, int queue = -1, float z = 0f, Texture tex = null)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Quad);
        g.name = name;
        Object.Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = new Vector3(pos.x, pos.y, z);
        g.transform.localScale = new Vector3(size.x, size.y, 1f);
        var r = g.GetComponent<Renderer>();
        r.material = Mat(c, queue, tex);
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows = false;
        return r;
    }

    /// <summary>Contour fin d'un rectangle.</summary>
    public static void Frame(Transform p, Vector2 c, Vector2 s, float t, Color col, int queue = -1, float z = 0f)
    {
        Quad(p, "Cadre", c + new Vector2(0f, s.y / 2f), new Vector2(s.x, t), col, queue, z);
        Quad(p, "Cadre", c - new Vector2(0f, s.y / 2f), new Vector2(s.x, t), col, queue, z);
        Quad(p, "Cadre", c + new Vector2(s.x / 2f, 0f), new Vector2(t, s.y), col, queue, z);
        Quad(p, "Cadre", c - new Vector2(s.x / 2f, 0f), new Vector2(t, s.y), col, queue, z);
    }

    /// <summary>Coins en équerre autour d'un rectangle.</summary>
    public static void Brackets(Transform p, Vector2 c, Vector2 s, float len, float t, Color col, int queue = -1)
    {
        for (int sx = -1; sx <= 1; sx += 2)
        for (int sy = -1; sy <= 1; sy += 2)
        {
            var corner = c + new Vector2(sx * s.x / 2f, sy * s.y / 2f);
            Quad(p, "Coin", corner + new Vector2(-sx * len / 2f, 0f), new Vector2(len, t), col, queue);
            Quad(p, "Coin", corner + new Vector2(0f, -sy * len / 2f), new Vector2(t, len), col, queue);
        }
    }

    /// <summary>Lignes horizontales sombres (effet vieil écran) par-dessus un panneau.</summary>
    public static void Scanlines(Transform p, Vector2 c, Vector2 s, int queue = -1)
    {
        if (!scan)
        {
            scan = new Texture2D(2, 256, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < 256; y++)
            {
                var px = new Color(0f, 0f, 0f, y % 2 == 0 ? 0.22f : 0f);
                scan.SetPixel(0, y, px); scan.SetPixel(1, y, px);
            }
            scan.Apply();
        }
        Quad(p, "Balayage", c, s, Color.white, queue, 0f, scan);
    }

    public static TextMesh Text(Transform parent, string name, Vector2 pos, float charSize, Color c, string text = "",
                                TextAnchor anchor = TextAnchor.MiddleCenter, bool bold = false, int queue = -1)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        var tm = g.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64;
        tm.characterSize = charSize;
        tm.anchor = anchor;
        tm.alignment = anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.UpperLeft ? TextAlignment.Left
                     : anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight ? TextAlignment.Right : TextAlignment.Center;
        tm.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        tm.color = c;
        tm.text = text;
        var mr = g.GetComponent<MeshRenderer>();
        mr.sharedMaterial = tm.font.material;
        if (queue >= 0) mr.material.renderQueue = queue;
        return tm;
    }

    /// <summary>Bouton encadré (texte + fond + cadre). Renvoie les éléments à faire clignoter.</summary>
    public static Renderer[] Button(Transform p, Vector2 c, Vector2 s, string label, float charSize, Color col, out TextMesh text)
    {
        var bg = Quad(p, "Bouton_Fond", c, s, new Color(col.r, col.g, col.b, 0.16f), QPanel + 2);
        Frame(p, c, s, 0.004f, col, QLine);
        text = Text(p, "Bouton_Texte", c, charSize, col, label, TextAnchor.MiddleCenter, true, QText);
        return new[] { bg };
    }

    /// <summary>Ajoute un espace entre les lettres (TextMesh n'a pas d'espacement de caractères).</summary>
    public static string Spaced(string s) => string.Join(" ", s.ToCharArray()).Replace("   ", "     ");

    public static void SetAlpha(TextMesh t, float a) { var c = t.color; c.a = a; t.color = c; }
    public static void SetAlpha(Renderer r, float a) { var c = r.material.color; c.a = a; r.material.color = c; }
}
