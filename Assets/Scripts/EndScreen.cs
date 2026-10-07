using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// [6.5] Écran de fin : victoire (générateur activé) ou défaite (joueur mort), avec le score, et « rejouer ».
//   - Défaite : GameOverUI fait le fondu au noir et le texte « VOUS ÊTES MORT » ; cet écran ajoute le bilan en dessous.
//   - Victoire : fondu au noir partiel, titre « VOUS AVEZ ÉCHAPPÉ ! » et bilan.
//   - Rejouer : appuyer sur la gâchette d'une des deux manettes (ou attendre `autoRestartAfter` secondes).
// Tout est collé à la tête (une interface 2D classique n'est pas visible dans le casque).
public class EndScreen : MonoBehaviour
{
    public static EndScreen Instance { get; private set; }

    public Transform head;
    public float showDelay = 2.5f;          // après la mort / la victoire
    public float inputDelay = 2f;           // la gâchette n'est prise en compte qu'après ce délai (évite un clic par erreur)
    public float autoRestartAfter = 45f;

    TextMesh title, details, hint;
    bool shown, ended, triggerWasDown = true;
    float shownAt;

    void Awake() => Instance = this;
    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        title = Make("Fin_Titre", new Vector3(0f, 0.32f, 1.5f), 0.012f, new Color(1f, 0.85f, 0.4f));
        details = Make("Fin_Bilan", new Vector3(0f, -0.22f, 1.5f), 0.0055f, Color.white);
        hint = Make("Fin_Rejouer", new Vector3(0f, -0.5f, 1.5f), 0.005f, new Color(0.5f, 1f, 0.5f));
        if (PlayerHealth.Instance) PlayerHealth.Instance.Died += OnDied;
    }

    TextMesh Make(string name, Vector3 local, float size, Color c)
    {
        var g = new GameObject(name);
        g.transform.SetParent(head, false);
        g.transform.localPosition = local;
        var tm = g.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64; tm.characterSize = size;
        tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.color = c;
        var mr = g.GetComponent<MeshRenderer>();
        mr.sharedMaterial = tm.font.material;
        mr.material.renderQueue = 4300;                 // au-dessus du fondu au noir (4100) et du texte de mort (4200)
        mr.enabled = false;
        return tm;
    }

    void OnDied()
    {
        if (ended) return;
        ended = true;
        if (ScoreManager.Instance) ScoreManager.Instance.Stop();
        StartCoroutine(Show(false));
    }

    void Update()
    {
        var wm = WaveManager.Instance;
        if (!ended && wm && wm.Victory) { ended = true; StartCoroutine(Show(true)); }

        if (!shown) return;
        bool down = VRInput.AnyTrigger();
        bool pressed = down && !triggerWasDown;
        triggerWasDown = down;
        if (Time.time - shownAt < inputDelay) return;
        if (pressed || Time.time - shownAt > autoRestartAfter) Restart();
    }

    IEnumerator Show(bool victory)
    {
        yield return new WaitForSeconds(showDelay);
        if (victory)
        {
            var black = HeadOverlay.Create(head, "VictoireFondu", null, new Color(0, 0, 0, 0), Camera.main.nearClipPlane + 0.03f, 4100);
            black.transform.localScale = Vector3.one * 6f;
            for (float t = 0; t < 1f; t += Time.deltaTime) { black.material.color = new Color(0, 0, 0, 0.85f * t); yield return null; }
            title.text = "VOUS AVEZ ÉCHAPPÉ !";
            title.color = new Color(0.5f, 1f, 0.5f);
            title.GetComponent<MeshRenderer>().enabled = true;
        }
        // défaite : le titre « VOUS ÊTES MORT » est déjà affiché par GameOverUI au centre

        var sm = ScoreManager.Instance;
        var wm = WaveManager.Instance;
        int total = wm ? wm.levels.Length : 4;
        int t2 = sm ? Mathf.FloorToInt(sm.Duration) : 0;
        details.text =
            "SCORE : " + (sm ? sm.Score : 0) + "\n" +
            "Vague atteinte : " + Mathf.Max(1, WaveManager.Level) + "/" + total + "\n" +
            "Zombies tués : " + (sm ? sm.Kills : 0) + "  (dont " + (sm ? sm.Headshots : 0) + " à la tête)\n" +
            "Durée : " + (t2 / 60) + " min " + (t2 % 60).ToString("00") + " s";
        hint.text = "Appuyez sur la gâchette pour REJOUER";
        details.GetComponent<MeshRenderer>().enabled = true;
        hint.GetComponent<MeshRenderer>().enabled = true;
        shown = true;
        shownAt = Time.time;
        triggerWasDown = true;   // il faut relâcher puis appuyer
    }

    void Restart()
    {
        shown = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    [ContextMenu("Test : écran de victoire")] void TestWin() { if (!ended) { ended = true; StartCoroutine(Show(true)); } }
}
