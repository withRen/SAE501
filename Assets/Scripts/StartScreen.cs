using System.Collections;
using UnityEngine;

// Écran de début, style « terminal de bunker » (ambre) : titre, accroche, 3 règles numérotées,
// bouton « GÂCHETTE POUR COMMENCER » qui clignote. La partie (compte à rebours du WaveManager) ne démarre qu'après l'appui.
// Collé à la tête, devant un fond assombri.
public class StartScreen : MonoBehaviour
{
    public Transform head;
    public string titleTop = "THE LAST";
    public string titleMain = "ESCAPOR";
    public string tagline = "SURVIVRE  ·  CLOUER  ·  S'ÉCHAPPER";
    public string headerLeft = "BUNKER-OS  //  ALERTE";
    public string headerRight = "SECTEUR 7";
    public string[] rules =
    {
        "Clouez des planches sur les portes et les fenêtres.",
        "Tirez sur les zombies : la tête les tue d'un coup.",
        "Survivez aux vagues, puis activez le générateur.",
    };
    public float inputDelay = 1.5f;

    Renderer dim, buttonBg;
    TextMesh buttonText;
    Transform root;
    bool waiting = true, triggerWasDown = true;
    float shownAt;

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        dim = HeadOverlay.Create(head, "DebutFond", null, new Color(0, 0, 0, 0.6f), Camera.main.nearClipPlane + 0.03f, 4100);
        dim.transform.localScale = Vector3.one * 6f;
        Build();
        UiKit.SetOnTop(true);
        shownAt = Time.time;
        if (WaveManager.Instance) WaveManager.Instance.waitForStart = true;
    }

    void OnDestroy() => UiKit.SetOnTop(false);

    void Build()
    {
        root = UiKit.HeadRoot(head, "EcranDebut", 1.2f, 0.8f);
        var A = UiKit.Amber;
        var size = new Vector2(1.17f, 0.81f);
        UiKit.Quad(root, "Panneau", Vector2.zero, size, UiKit.PanelGreen, UiKit.QPanel);
        UiKit.Scanlines(root, Vector2.zero, size, UiKit.QPanel + 1);
        UiKit.Frame(root, Vector2.zero, size, 0.003f, new Color(A.r, A.g, A.b, 0.4f), UiKit.QLine);
        UiKit.Brackets(root, Vector2.zero, size + new Vector2(0.02f, 0.02f), 0.05f, 0.006f, A, UiKit.QLine);

        // bande de titre
        UiKit.Quad(root, "Bande", new Vector2(0f, 0.376f), new Vector2(size.x, 0.059f), new Color(A.r, A.g, A.b, 0.9f), UiKit.QLine);
        var ink = new Color(0.02f, 0.05f, 0.03f);
        UiKit.Text(root, "Bande_G", new Vector2(-0.56f, 0.376f), 0.0062f, ink, headerLeft, TextAnchor.MiddleLeft, true, UiKit.QText);
        UiKit.Text(root, "Bande_D", new Vector2(0.56f, 0.376f), 0.0062f, ink, headerRight, TextAnchor.MiddleRight, true, UiKit.QText);

        // titre
        UiKit.Text(root, "Titre_Haut", new Vector2(0f, 0.255f), 0.0105f, A, UiKit.Spaced(titleTop), TextAnchor.MiddleCenter, true, UiKit.QText);
        UiKit.Text(root, "Titre", new Vector2(0f, 0.15f), 0.025f, A, titleMain, TextAnchor.MiddleCenter, true, UiKit.QText);
        UiKit.Text(root, "Accroche", new Vector2(0f, 0.04f), 0.0062f, UiKit.Dim, tagline, TextAnchor.MiddleCenter, false, UiKit.QText);
        UiKit.Quad(root, "Separateur", new Vector2(0f, -0.013f), new Vector2(1.01f, 0.003f), new Color(A.r, A.g, A.b, 0.5f), UiKit.QLine);

        // règles numérotées
        for (int i = 0; i < rules.Length; i++)
        {
            float y = -0.075f - i * 0.069f;
            UiKit.Frame(root, new Vector2(-0.46f, y), new Vector2(0.069f, 0.048f), 0.003f, A, UiKit.QLine);
            UiKit.Text(root, "Num", new Vector2(-0.46f, y), 0.0062f, A, (i + 1).ToString("00"), TextAnchor.MiddleCenter, true, UiKit.QText);
            UiKit.Text(root, "Regle", new Vector2(-0.395f, y), 0.0068f, UiKit.White, rules[i], TextAnchor.MiddleLeft, false, UiKit.QText);
        }

        // bouton
        buttonBg = UiKit.Button(root, new Vector2(0f, -0.325f), new Vector2(0.76f, 0.075f), "GÂCHETTE  POUR  COMMENCER", 0.0068f, UiKit.Green, out buttonText)[0];
    }

    void Update()
    {
        if (!waiting) return;
        float a = 0.55f + 0.45f * Mathf.Sin(Time.time * 3.5f);      // le bouton clignote doucement
        UiKit.SetAlpha(buttonText, a);
        UiKit.SetAlpha(buttonBg, 0.1f + 0.12f * a);

        bool down = VRInput.AnyTrigger();
        bool pressed = down && !triggerWasDown;
        triggerWasDown = down;
        if (pressed && Time.time - shownAt > inputDelay) StartCoroutine(Begin());
    }

    IEnumerator Begin()
    {
        waiting = false;
        if (WaveManager.Instance) WaveManager.Instance.BeginGame();
        if (ScoreManager.Instance) ScoreManager.Instance.ResetClock();
        // l'écran se replie vers le haut en s'effaçant
        var r0 = root.localScale;
        for (float t = 0; t < 0.6f; t += Time.deltaTime)
        {
            float k = 1f - t / 0.6f;
            dim.material.color = new Color(0, 0, 0, 0.6f * k);
            root.localScale = new Vector3(r0.x, r0.y * Mathf.Max(0.02f, k * k), r0.z);
            yield return null;
        }
        Destroy(dim.gameObject);
        Destroy(root.gameObject);
        UiKit.SetOnTop(false);
        Destroy(this);
    }
}
