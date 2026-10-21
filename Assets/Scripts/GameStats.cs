using UnityEngine;

// [#122] Compteurs de la partie, affichés dans le bilan de fin (GameOverUI) :
// zombies éliminés, tirs, tirs à la tête, planches posées, niveau atteint, temps de jeu.
// Remis à zéro au début de chaque partie (WaveManager.Awake) ; le temps ne compte que pendant les vagues (pas le tutoriel, pas la pause).
// Rééquilibrage : mesures pour régler le jeu après un playtest (éliminations au pistolet / à la mêlée, dégâts reçus par niveau,
// balles chargées). WaveManager écrit une ligne « [Bilan] » par vague et à la mort dans le log (adb logcat -s Unity).
public static class GameStats
{
    public static int Kills { get; private set; }          // zombies éliminés par le joueur (pas ceux tués par la sécurité « zombie coincé »)
    public static int Shots { get; private set; }          // balles tirées
    public static int HeadShots { get; private set; }      // balles qui ont touché une tête
    public static int PlanksPlaced { get; private set; }   // planches posées sur une ouverture par le joueur
    public static float Time { get; private set; }         // secondes de jeu depuis le compte à rebours de la 1re vague
    public static int MeleeKills { get; private set; }     // dont éliminés à la mêlée (hache, pied de biche)
    public static int RoundsLoaded { get; private set; }   // balles mises dans le pistolet par les chargeurs
    public static float DamageTaken { get; private set; }  // PV perdus pendant toute la partie
    static readonly float[] damageByLevel = new float[ZombieLevels.MaxLevel + 1];
    public static float DamageAtLevel(int lvl) => damageByLevel[Mathf.Clamp(lvl, 0, ZombieLevels.MaxLevel)];

    public static float HeadShotPercent => Shots > 0 ? 100f * HeadShots / Shots : 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Reset();

    public static void Reset()
    {
        Kills = Shots = HeadShots = PlanksPlaced = MeleeKills = RoundsLoaded = 0; Time = DamageTaken = 0f;
        System.Array.Clear(damageByLevel, 0, damageByLevel.Length);
    }

    // la partie n'a pas commencé (tutoriel) : rien n'est compté
    static bool Counting => WaveManager.Instance != null && !Tutorial.Running;

    public static void Kill(bool melee = false) { if (!Counting) return; Kills++; if (melee) MeleeKills++; }
    public static void Shot() { if (Counting) Shots++; }
    public static void HeadShot() { if (Counting) HeadShots++; }
    public static void PlankPlaced() { if (Counting) PlanksPlaced++; }
    public static void Loaded(int rounds) { if (Counting) RoundsLoaded += rounds; }
    public static void Damage(float hp)
    {
        if (!Counting || hp <= 0f) return;
        DamageTaken += hp;
        damageByLevel[Mathf.Clamp(WaveManager.Level, 0, ZombieLevels.MaxLevel)] += hp;
    }
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
        return "Zombies éliminés : " + Kills + (MeleeKills > 0 ? "  (dont " + MeleeKills + " à la mêlée)" : "")
             + "\nTirs à la tête : " + Mathf.RoundToInt(HeadShotPercent) + " %  (" + HeadShots + " sur " + Shots + " tirs)"
             + "\nPlanches posées : " + PlanksPlaced
             + "\nNiveau atteint : " + Mathf.Max(1, level) + " / " + maxLevel
             + "\nTemps total : " + FormatTime(Time);
    }

    // ligne de mesure pour le réglage (log) : une par vague terminée et une à la mort
    public static string LogLine(string what, int level, float health, int ammoInGun)
    {
        return "[Bilan] " + what + " · niveau " + level + " · temps " + Mathf.RoundToInt(Time) + " s"
             + " · tués " + Kills + " (mêlée " + MeleeKills + ") · tirs " + Shots + " (tête " + HeadShots + ")"
             + " · dégâts reçus niveau " + Mathf.RoundToInt(DamageAtLevel(level)) + " / total " + Mathf.RoundToInt(DamageTaken)
             + " · PV " + Mathf.RoundToInt(health) + " · balles chargées " + RoundsLoaded + " · dans l'arme " + ammoInGun
             + " · difficulté " + GameSettings.Difficulty;
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
        return "Zombies éliminés : " + Kills + (MeleeKills > 0 ? "  (dont " + MeleeKills + " à la mêlée)" : "")
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
