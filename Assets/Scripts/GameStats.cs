using UnityEngine;

// [#122] Compteurs de la partie, affichés dans le bilan de fin (GameOverUI) :
// zombies éliminés, tirs, tirs à la tête, planches posées, niveau atteint, temps de jeu.
// Remis à zéro au début de chaque partie (WaveManager.Awake) ; le temps ne compte que pendant les vagues (pas le tutoriel, pas la pause).
public static class GameStats
{
    public static int Kills { get; private set; }          // zombies éliminés par le joueur (pas ceux tués par la sécurité « zombie coincé »)
    public static int Shots { get; private set; }          // balles tirées
    public static int HeadShots { get; private set; }      // balles qui ont touché une tête
    public static int PlanksPlaced { get; private set; }   // planches posées sur une ouverture par le joueur
    public static float Time { get; private set; }         // secondes de jeu depuis le compte à rebours de la 1re vague

    public static float HeadShotPercent => Shots > 0 ? 100f * HeadShots / Shots : 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Reset();

    public static void Reset() { Kills = Shots = HeadShots = PlanksPlaced = 0; Time = 0f; }

    // la partie n'a pas commencé (tutoriel) : rien n'est compté
    static bool Counting => WaveManager.Instance != null && !Tutorial.Running;

    public static void Kill() { if (Counting) Kills++; }
    public static void Shot() { if (Counting) Shots++; }
    public static void HeadShot() { if (Counting) HeadShots++; }
    public static void PlankPlaced() { if (Counting) PlanksPlaced++; }
    public static void Tick(float dt) => Time += dt;

    public static string FormatTime(float s)
    {
        int t = Mathf.FloorToInt(s);
        return (t / 60) + " min " + (t % 60).ToString("00") + " s";
    }

    // texte du bilan (écran de fin)
    public static string Summary(int level, int maxLevel)
    {
        return "Zombies éliminés : " + Kills
             + "\nTirs à la tête : " + Mathf.RoundToInt(HeadShotPercent) + " %  (" + HeadShots + " sur " + Shots + " tirs)"
             + "\nPlanches posées : " + PlanksPlaced
             + "\nNiveau atteint : " + Mathf.Max(1, level) + " / " + maxLevel
             + "\nTemps total : " + FormatTime(Time);
    }
}
