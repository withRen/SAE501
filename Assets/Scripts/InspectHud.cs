using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Affichage à l'inspection : on tient l'objet en main, on le tourne de côté (canon ou faisceau vers la gauche) près du visage
// pour lire ce qu'il reste. Le reste du temps, rien ne s'affiche.
//   - Pistolet : « 8 / 12 » suivi d'une icône de balle (blanc, orange quand il en reste peu, rouge et clignotant à vide).
//   - Lampe : icône de pile à 4 barres ; la barre en cours clignote doucement, de plus en plus vite quand elle va s'éteindre.
// Le petit panneau flotte au-dessus de l'objet et se tourne vers les yeux.
[RequireComponent(typeof(XRGrabInteractable))]
public class InspectHud : MonoBehaviour
{
    public enum Kind { Ammo, Battery }
    public Kind kind;
    [Tooltip("Point de l'objet au-dessus duquel le panneau apparaît (repère local).")]
    public Vector3 anchor;
    public float above = 0.045f;

    [Header("Geste d'inspection")]
    [Tooltip("Distance maximale entre l'objet et les yeux.")]
    public float maxDistance = 0.6f;
    [Tooltip("L'objet doit être dans ce cône devant le regard (degrés).")]
    public float maxViewAngle = 50f;
    [Tooltip("Écart maximal (vu de dessus, dans le repère de la tête) entre l'axe de l'objet et la gauche du regard.")]
    public float maxLeftAngle = 60f;
    [Tooltip("Une fois affiché, les seuils sont élargis de cette part (pas de clignotement à la limite).")]
    public float hysteresis = 0.25f;
    [Tooltip("Le panneau reste encore ce temps quand le geste se perd (secondes).")]
    public float linger = 0.35f;
    public float fadeSpeed = 8f;
    [Tooltip("Test dans l'éditeur : affiche le panneau sans tenir l'objet.")]
    public bool forceShow;

    XRGrabInteractable grab;
    Pistol pistol;
    Flashlight lamp;
    Transform panel, head;
    float alpha, lastSeen = -10f;
    bool showing;

    TextMesh text;
    SpriteRenderer bullet;
    SpriteRenderer[] frame, bars;

    static Sprite white, bulletSprite;
    static readonly Color Amber = new Color(1f, 0.85f, 0.45f);

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        pistol = GetComponent<Pistol>();
        lamp = GetComponent<Flashlight>();
        Build();
        SetAlpha(0f);
    }

    // ───────────────────────── geste ─────────────────────────
    // canon (ou verre de la lampe) tourné vers la gauche du regard : on voit le flanc de l'objet devant soi.
    // La direction est lue dans le repère de la tête et vue de dessus : l'objet peut être relevé ou baissé (jusqu'à ~60°),
    // seule compte sa direction gauche / avant. widen (0 = seuils normaux) élargit tous les seuils.
    public static bool Inspecting(Transform obj, Transform head, float maxDistance, float maxViewAngle, float maxLeftAngle, float widen = 0f)
    {
        float k = 1f + widen;
        var to = obj.position - head.position;
        if (to.magnitude > maxDistance * k) return false;
        if (Vector3.Angle(head.forward, to) > maxViewAngle * k) return false;
        var d = head.InverseTransformDirection(obj.forward);
        if (Mathf.Abs(d.y) > 0.87f) return false;                         // pointé vers le haut ou le bas
        return Vector2.Angle(new Vector2(d.x, d.z), Vector2.left) <= maxLeftAngle * k;
    }

    void LateUpdate()
    {
        if (!head) { var cam = Camera.main; if (cam) head = cam.transform; else return; }
        bool held = BeltItem.InHand(grab) && !PauseMenu.Paused;
        bool gesture = held && Inspecting(transform, head, maxDistance, maxViewAngle, maxLeftAngle, showing ? hysteresis : 0f);
        if (gesture) lastSeen = Time.time;
        showing = held && Time.time - lastSeen <= linger;
        bool show = forceShow || showing;
        alpha = Mathf.MoveTowards(alpha, show ? 1f : 0f, fadeSpeed * Time.deltaTime);
        panel.gameObject.SetActive(alpha > 0.001f);
        if (alpha <= 0.001f) return;

        // au-dessus de l'objet (verticale du monde), tourné vers les yeux
        var p = transform.TransformPoint(anchor) + Vector3.up * above;
        panel.SetPositionAndRotation(p, Quaternion.LookRotation(p - head.position, head.up));
        if (kind == Kind.Ammo) UpdateAmmo(); else UpdateBattery();
    }

    // ───────────────────────── pistolet ─────────────────────────
    void UpdateAmmo()
    {
        if (!pistol) return;
        int ammo = pistol.ammo;
        text.text = ammo + " / " + pistol.maxAmmo;
        var c = ammo <= 0 ? pistol.hudEmpty : ammo <= pistol.lowThreshold ? pistol.hudLow : pistol.hudNormal;
        if (ammo <= 0 && Mathf.Repeat(Time.time, 0.6f) >= 0.3f) c *= 0.4f;     // vide : clignote
        c.a = alpha;
        text.color = c;
        bullet.color = c;
    }

    // ───────────────────────── lampe ─────────────────────────
    void UpdateBattery()
    {
        if (!lamp) return;
        float level = Mathf.Clamp01(lamp.level);
        int lit = Mathf.CeilToInt(level * 4f - 1e-4f);                   // barres encore là (0 à 4)
        float part = lit > 0 ? level * 4f - (lit - 1) : 0f;               // reste de la barre en cours (1 = pleine, 0 = vide)
        var on = level <= 0.25f ? new Color(1f, 0.35f, 0.2f) : Amber;     // dernière barre : rouge orangé
        for (int i = 0; i < 4; i++)
        {
            Color c;
            if (i < lit - 1) c = on;
            else if (i == lit - 1)
            {
                // clignotement doux (0,5 Hz) quand la barre est pleine, jusqu'à 4 Hz quand elle va s'éteindre
                float hz = Mathf.Lerp(4f, 0.5f, part);
                float k = 0.5f + 0.5f * Mathf.Cos(Time.time * hz * Mathf.PI * 2f);
                c = Color.Lerp(on * 0.3f, on, k);
            }
            else c = new Color(1f, 1f, 1f, 0.08f);                        // barre vide : à peine visible
            c.a = Mathf.Min(c.a, 1f) * alpha;
            bars[i].color = c;
        }
        var fc = lit == 0 ? new Color(1f, 0.2f, 0.15f) : Amber;
        fc.a = alpha;
        foreach (var f in frame) f.color = fc;
    }

    // ───────────────────────── construction ─────────────────────────
    void Build()
    {
        panel = new GameObject("Inspection").transform;
        panel.SetParent(transform, false);
        if (kind == Kind.Ammo)
        {
            var t = new GameObject("Texte");
            t.transform.SetParent(panel, false);
            t.transform.localPosition = new Vector3(-0.006f, 0f, 0f);
            text = t.AddComponent<TextMesh>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 64; text.characterSize = 0.0024f;
            text.anchor = TextAnchor.MiddleRight; text.alignment = TextAlignment.Right;
            text.fontStyle = FontStyle.Bold;
            var mr = t.GetComponent<MeshRenderer>();
            mr.sharedMaterial = text.font.material;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            bullet = Sprite("Balle", BulletSprite(), new Vector3(0.002f, 0f, 0f), new Vector2(0.0055f, 0.018f));
        }
        else
        {
            // pile couchée : contour (4 traits + téton à droite), 4 barres à l'intérieur, de gauche (fond) à droite (+)
            const float w = 0.042f, h = 0.018f, t = 0.0016f;
            frame = new[]
            {
                Sprite("Haut", WhiteSprite(), new Vector3(0f, h / 2f, 0f), new Vector2(w, t)),
                Sprite("Bas", WhiteSprite(), new Vector3(0f, -h / 2f, 0f), new Vector2(w, t)),
                Sprite("Gauche", WhiteSprite(), new Vector3(-w / 2f, 0f, 0f), new Vector2(t, h + t)),
                Sprite("Droite", WhiteSprite(), new Vector3(w / 2f, 0f, 0f), new Vector2(t, h + t)),
                Sprite("Teton", WhiteSprite(), new Vector3(w / 2f + 0.0022f, 0f, 0f), new Vector2(0.003f, h * 0.45f)),
            };
            bars = new SpriteRenderer[4];
            float inner = w - 2f * t - 0.004f, gap = 0.0016f, bw = (inner - 3f * gap) / 4f;
            for (int i = 0; i < 4; i++)
            {
                float x = -inner / 2f + bw / 2f + i * (bw + gap);
                bars[i] = Sprite("Barre_" + (i + 1), WhiteSprite(), new Vector3(x, 0f, 0f), new Vector2(bw, h - 2f * t - 0.003f));
            }
        }
    }

    SpriteRenderer Sprite(string name, Sprite s, Vector3 pos, Vector2 size)
    {
        var g = new GameObject(name);
        g.transform.SetParent(panel, false);
        g.transform.localPosition = pos;
        // le sprite fait 1 unité de côté (ppu = largeur) ; l'échelle lui donne sa taille en mètres
        g.transform.localScale = new Vector3(size.x / s.bounds.size.x, size.y / s.bounds.size.y, 1f);
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return sr;
    }

    void SetAlpha(float a)
    {
        alpha = a;
        if (panel) panel.gameObject.SetActive(a > 0.001f);
    }

    static Sprite WhiteSprite()
    {
        if (white) return white;
        var t = new Texture2D(4, 4, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var px = new Color32[16];
        for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
        t.SetPixels32(px); t.Apply();
        return white = UnityEngine.Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    // balle vue de profil, pointe vers le haut : ogive arrondie, étui, gorge et culot
    static Sprite BulletSprite()
    {
        if (bulletSprite) return bulletSprite;
        const int W = 32, H = 96;
        var t = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        var px = new Color32[W * H];
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            float cx = Mathf.Abs(x + 0.5f - W / 2f);
            float half;                                                    // demi-largeur de la silhouette à cette hauteur
            if (y < 6) half = 13f;                                         // culot
            else if (y < 9) half = 10f;                                    // gorge
            else if (y < 58) half = 12f;                                   // étui
            else { float u = (y - 58f) / (H - 1 - 58f); half = 12f * Mathf.Sqrt(Mathf.Max(0f, 1f - u * u)); }   // ogive
            float a = Mathf.Clamp01(half - cx + 0.5f);
            if (y >= 56 && y < 58) a *= 0.25f;                             // trait entre l'étui et l'ogive
            px[y * W + x] = new Color32(255, 255, 255, (byte)(a * 255));
        }
        t.SetPixels32(px); t.Apply();
        return bulletSprite = UnityEngine.Sprite.Create(t, new Rect(0, 0, W, H), new Vector2(0f, 0.5f), W);
    }
}
