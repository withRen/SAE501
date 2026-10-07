using System.Collections;
using UnityEngine;

// Écran de début : titre du jeu, règles en 3 lignes, « Appuyez sur la gâchette pour commencer ».
// La partie (compte à rebours du WaveManager) ne démarre qu'après l'appui. Collé à la tête, sur un fond sombre.
public class StartScreen : MonoBehaviour
{
    public Transform head;
    public string gameTitle = "THE LAST ESCAPOR";
    [TextArea] public string rules =
        "Clouez des planches sur les portes et les fenêtres du bunker.\n" +
        "Tirez sur les zombies : un tir à la tête les tue d'un coup.\n" +
        "Survivez aux vagues, puis activez le générateur pour vous échapper.";
    public float inputDelay = 1.5f;

    Renderer dim;
    TextMesh title, body, hint;
    bool waiting = true, triggerWasDown = true;
    float shownAt;

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        dim = HeadOverlay.Create(head, "DebutFond", null, new Color(0, 0, 0, 0.75f), Camera.main.nearClipPlane + 0.03f, 4100);
        dim.transform.localScale = Vector3.one * 6f;
        title = Make("Debut_Titre", new Vector3(0f, 0.30f, 1.5f), 0.014f, new Color(1f, 0.85f, 0.4f), gameTitle);
        body = Make("Debut_Regles", new Vector3(0f, -0.02f, 1.5f), 0.0045f, Color.white, rules);
        hint = Make("Debut_Commencer", new Vector3(0f, -0.32f, 1.5f), 0.0055f, new Color(0.5f, 1f, 0.5f), "Appuyez sur la gâchette pour COMMENCER");
        shownAt = Time.time;
        if (WaveManager.Instance) WaveManager.Instance.waitForStart = true;
    }

    TextMesh Make(string name, Vector3 local, float size, Color c, string text)
    {
        var g = new GameObject(name);
        g.transform.SetParent(head, false);
        g.transform.localPosition = local;
        var tm = g.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64; tm.characterSize = size;
        tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.color = c; tm.text = text;
        var mr = g.GetComponent<MeshRenderer>();
        mr.sharedMaterial = tm.font.material;
        mr.material.renderQueue = 4300;
        return tm;
    }

    void Update()
    {
        if (!waiting) return;
        // le texte « commencer » clignote doucement
        var c = hint.color; c.a = 0.55f + 0.45f * Mathf.Sin(Time.time * 3f); hint.color = c;

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
        var texts = new[] { title, body, hint };
        for (float t = 0; t < 0.8f; t += Time.deltaTime)
        {
            float k = 1f - t / 0.8f;
            dim.material.color = new Color(0, 0, 0, 0.75f * k);
            foreach (var tm in texts) { var c = tm.color; c.a = k; tm.color = c; }
            yield return null;
        }
        Destroy(dim.gameObject);
        foreach (var tm in texts) Destroy(tm.gameObject);
        Destroy(this);
    }
}
