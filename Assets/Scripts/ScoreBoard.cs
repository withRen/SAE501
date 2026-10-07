using UnityEngine;

// [7.4] Écran de score digital, accroché au mur du bunker : numéro de vague et points.
// Style afficheur à LED (vert sur fond noir). Le score défile quand il augmente.
public class ScoreBoard : MonoBehaviour
{
    public TextMesh waveText;
    public TextMesh scoreText;
    public float countSpeed = 120f;        // points affichés par seconde quand le score monte

    float shown;
    float flashUntil;

    void Update()
    {
        var wm = WaveManager.Instance;
        int total = wm ? wm.levels.Length : 4;
        int lvl = WaveManager.Level;
        string w;
        if (wm && wm.Victory) w = "GÉNÉRATEUR ACTIVÉ";
        else if (lvl == 0) w = "PRÉPAREZ-VOUS";
        else if (wm && wm.InBreak) w = "VAGUE " + lvl + "/" + total + " TERMINÉE";
        else w = "VAGUE " + lvl + "/" + total;
        if (waveText && waveText.text != w) waveText.text = w;

        int target = ScoreManager.Instance ? ScoreManager.Instance.Score : 0;
        if (shown < target)
        {
            shown = Mathf.Min(target, shown + countSpeed * Time.deltaTime);
            flashUntil = Time.time + 0.3f;
        }
        else shown = target;
        if (scoreText)
        {
            string s = "SCORE " + Mathf.FloorToInt(shown).ToString("00000");
            if (scoreText.text != s) scoreText.text = s;
            scoreText.color = Time.time < flashUntil ? new Color(0.7f, 1f, 0.7f) : new Color(0.2f, 1f, 0.35f);
        }
    }
}
