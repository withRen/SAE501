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
        if (WaveManager.Endless) return EndlessSummary(level);
        return "Zombies éliminés : " + Kills
             + "\nTirs à la tête : " + Mathf.RoundToInt(HeadShotPercent) + " %  (" + HeadShots + " sur " + Shots + " tirs)"
             + "\nPlanches posées : " + PlanksPlaced
             + "\nNiveau atteint : " + Mathf.Max(1, level) + " / " + maxLevel
             + "\nTemps total : " + FormatTime(Time);
    }

    // mode survie : pas de « niveau / maximum » (les vagues n'ont pas de fin) ; le record est mis à jour avant d'être affiché
    // (même nombre de lignes que le bilan normal, pour tenir dans l'écran de fin)
    static string EndlessSummary(int wave)
    {
        wave = Mathf.Max(1, wave);
        EndlessRecord.Submit(wave, Time);
        string waveLine = "Vague atteinte : " + wave
                        + (!EndlessRecord.NewRecord && EndlessRecord.BestWave > 0 ? "  (record : vague " + EndlessRecord.BestWave + ")" : "");
        string timeLine = "Temps de survie : " + FormatTime(Time) + (EndlessRecord.NewRecord ? "  · NOUVEAU RECORD !" : "");
        return "Zombies éliminés : " + Kills
             + "\nTirs à la tête : " + Mathf.RoundToInt(HeadShotPercent) + " %  (" + HeadShots + " sur " + Shots + " tirs)"
             + "\nPlanches posées : " + PlanksPlaced
             + "\n" + waveLine
             + "\n" + timeLine;
    }
}

// Record du mode survie (vagues infinies) : la meilleure partie, au temps de survie. Gardé entre les parties (PlayerPrefs).
public static class EndlessRecord
{
    const string KeyTime = "endless_best_time", KeyWave = "endless_best_wave";
    static float runStartBest;

    public static float BestTime => PlayerPrefs.GetFloat(KeyTime, 0f);
    public static int BestWave => PlayerPrefs.GetInt(KeyWave, 0);
    public static bool HasRecord => BestTime > 0f;
    /// <summary>Le record d'avant la partie a été battu pendant cette partie.</summary>
    public static bool NewRecord { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { runStartBest = 0f; NewRecord = false; }

    /// <summary>Début d'une partie : on retient le record actuel pour savoir s'il est battu.</summary>
    public static void BeginRun() { runStartBest = BestTime; NewRecord = false; }

    /// <summary>Peut être appelé autant de fois que voulu : ne garde que la meilleure partie.</summary>
    public static void Submit(int wave, float time)
    {
        if (time <= BestTime) return;
        PlayerPrefs.SetFloat(KeyTime, time);
        PlayerPrefs.SetInt(KeyWave, wave);
        PlayerPrefs.Save();
        NewRecord = runStartBest > 0f;          // le tout premier score n'est pas annoncé comme un record
    }

    public static string Describe() => "vague " + BestWave + "  ·  " + GameStats.FormatTime(BestTime);
}
