using UnityEngine;

// [7.4] Écran de score digital au mur du bunker, style afficheur LED « terminal de bunker » :
//   à gauche : VAGUE n/4 + 4 voyants (allumés = vagues commencées) ;  à droite : SCORE 00000 + dernier gain (« +10 ZOMBIE ABATTU »).
// L'objet est posé et orienté par RoomBuilder (le joueur regarde vers +Z de l'objet) ; l'affichage est construit au lancement.
public class ScoreBoard : MonoBehaviour
{
    public float countSpeed = 120f;        // points affichés par seconde quand le score monte
    public float gainShowTime = 3f;

    TextMesh waveText, scoreText, gainText, waveLabel;
    Renderer[] lamps;
    float shown, gainUntil;
    static readonly Color LampOff = new Color(0.24f, 0.16f, 0.04f);

    void OnEnable() => ScoreManager.Gained += OnGain;
    void OnDisable() => ScoreManager.Gained -= OnGain;

    void Start()
    {
        var t = transform;
        // fond noir de l'afficheur (1,8 m x 0,62 m), juste devant le cadre (vers le joueur = -Z local)
        UiKit.Quad(t, "Ecran", Vector2.zero, new Vector2(1.8f, 0.62f), new Color(0.015f, 0.03f, 0.02f, 1f), -1, 0.003f);
        var A = UiKit.Amber; var G = UiKit.Green;
        waveLabel = UiKit.Text(t, "Etiquette_Vague", new Vector2(-0.82f, 0.24f), 0.0106f, new Color(A.r, A.g, A.b, 0.7f), "VAGUE", TextAnchor.UpperLeft, true);
        waveText = UiKit.Text(t, "Vague", new Vector2(-0.82f, 0.15f), 0.042f, A, "-/-", TextAnchor.UpperLeft, true);
        lamps = new Renderer[4];
        for (int i = 0; i < lamps.Length; i++)
            lamps[i] = UiKit.Quad(t, "Voyant", new Vector2(-0.77f + i * 0.11f, -0.19f), new Vector2(0.08f, 0.06f), LampOff, -1, 0.002f);
        UiKit.Quad(t, "Separation", new Vector2(-0.28f, 0f), new Vector2(0.008f, 0.46f), new Color(0.16f, 0.27f, 0.2f, 1f), -1, 0.002f);
        UiKit.Text(t, "Etiquette_Score", new Vector2(-0.20f, 0.24f), 0.0106f, new Color(G.r, G.g, G.b, 0.7f), "SCORE", TextAnchor.UpperLeft, true);
        scoreText = UiKit.Text(t, "Score", new Vector2(-0.20f, 0.15f), 0.042f, G, "00000", TextAnchor.UpperLeft, true);
        gainText = UiKit.Text(t, "Gain", new Vector2(-0.20f, -0.16f), 0.0106f, G, "", TextAnchor.UpperLeft, true);
    }

    void OnGain(int pts, string reason)
    {
        if (!gainText) return;
        gainText.text = "+" + pts + (string.IsNullOrEmpty(reason) ? "" : "  " + reason);
        gainUntil = Time.time + gainShowTime;
    }

    void Update()
    {
        if (!scoreText) return;
        var wm = WaveManager.Instance;
        int total = wm ? wm.levels.Length : 4;
        int lvl = WaveManager.Level;
        string w = lvl == 0 ? "-/" + total : lvl + "/" + total;
        if (waveText.text != w) waveText.text = w;
        string lab = wm && wm.Victory ? "ÉVACUATION" : wm && wm.InBreak ? "PAUSE" : "VAGUE";
        if (waveLabel.text != lab) waveLabel.text = lab;
        for (int i = 0; i < lamps.Length; i++)
        {
            bool on = i < lvl;
            bool blink = wm && wm.InBreak && i == lvl;            // pendant la pause, le voyant de la vague suivante clignote
            var c = on ? UiKit.Amber : LampOff;
            if (blink) c = Color.Lerp(LampOff, UiKit.Amber, 0.5f + 0.5f * Mathf.Sin(Time.time * 6f));
            lamps[i].material.color = c;
        }

        int target = ScoreManager.Instance ? ScoreManager.Instance.Score : 0;
        shown = shown < target ? Mathf.Min(target, shown + countSpeed * Time.deltaTime) : target;
        string s = Mathf.FloorToInt(shown).ToString("00000");
        if (scoreText.text != s) scoreText.text = s;

        float k = Mathf.Clamp01((gainUntil - Time.time) / 0.6f);    // le dernier gain s'efface
        UiKit.SetAlpha(gainText, k);
    }
}
