using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Système de niveaux par vagues : 4 niveaux = 4 vagues.
//   Niveau 1 : le bunker seul (couloirs verrouillés)     Niveau 2 : + couloirs et cuisine
//   Niveau 3 : + stockage                                Niveau 4 : + générateur  -> objectif : l'activer (Generator)
// Chaque niveau : les zombies ont plus de PV et frappent plus fort (ZombieLevels), apparaissent aux points autorisés,
// puis une pause pour réparer les barricades. Détails : wiki « Niveaux et vagues ».
// [6.3] Courbe de difficulté : la vague 1 est lente, d'un seul côté et avec peu de zombies à la fois ;
//   à chaque vague les zombies sont plus nombreux, plus rapides, et plus d'entre eux attaquent en même temps.
// Niveaux 1 et 2 : pas de zombie sur le toit (ses trous ne se barricadent pas) : tout arrive par une fenêtre ou une porte.
// Une vague se termine toujours : un zombie coincé meurt seul au bout de 20 s (Zombie.stuckTimeout).
// [#102] Annonces et statut sur un écran fixe du bunker (mur sud, au-dessus du râtelier), plus rien ne suit le regard ;
//   une alarme retentit au début de chaque vague. [#106] L'annonce dit ce que contiennent les zones qui s'ouvrent (ZoneGate).
// [#120] Niveau 4 : le générateur ne démarre qu'après generatorKills zombies éliminés OU generatorDelay secondes dans le niveau
//   (on ne gagne plus en courant directement au levier). L'écran annonce le moment où il devient utilisable.
// [#122] Le temps de jeu est compté pour le bilan (GameStats) ; la victoire affiche le bilan (GameOverUI.ShowVictory).
// Rééquilibrage (docs/PROMPTS.md 085-088) :
//   - pause entre deux vagues : +breakHeal PV (la vie perdue ne s'accumule plus jusqu'au niveau 4) ;
//   - [#123] point d'apparition tiré au sort avec un poids par point (SpawnWeights) au lieu d'un tour fixe ;
//   - niveau 4 : dès que le générateur est prêt, les zombies traquent le joueur hors du bunker (Zombie.HuntPlayer) ;
//   - [#121] après une mort au niveau 2 ou plus, on peut reprendre au début du niveau atteint (ResumeLevel, GameOverUI) ;
//   - une ligne « [Bilan] » dans le log à chaque vague terminée et à la mort (GameStats.LogLine) pour régler après un playtest.
// [MODE INFINI + FORMULE] Plus aucun réglage de vague écrit à la main : tout vient de la fonction DefForWave(vague), une suite qui
//   donne nombre de zombies, cadence, vitesse, zombies simultanés, PV et points d'apparition. Elle part de la difficulté du menu
//   (GameSettings.Difficulty) et des quantités du GDD (4, 8, 12, 18, 24 en Normal). Deux modes, choisis dans le menu (SOLO) :
//   - CAMPAGNE : `campaignWaves` vagues puis le générateur (comme avant) ;
//   - VAGUES INFINIES (WaveManager.Infinite) : les vagues ne s'arrêtent jamais, objectif = survivre le plus longtemps ; records sauvegardés.
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public static int Level { get; private set; }          // 0 = partie pas commencée
    // [#121] niveau où reprendre au prochain chargement de la scène (0 : partie normale) ; posé par GameOverUI
    public static int ResumeLevel;
    public static bool Resumed { get; private set; }       // cette partie est une reprise : pas de tutoriel

    // ---- mode de jeu (choisi dans MainMenu > SOLO), mémorisé d'une partie à l'autre ----
    static int mode = -1;
    public static bool Infinite
    {
        get { if (mode < 0) mode = PlayerPrefs.GetInt("sae501.endless", 0); return mode == 1; }
        set { mode = value ? 1 : 0; PlayerPrefs.SetInt("sae501.endless", mode); }
    }
    // records du mode infini
    public static int BestWave { get => PlayerPrefs.GetInt("sae501.endless.bestWave", 0); private set => PlayerPrefs.SetInt("sae501.endless.bestWave", value); }
    public static float BestTime { get => PlayerPrefs.GetFloat("sae501.endless.bestTime", 0f); private set => PlayerPrefs.SetFloat("sae501.endless.bestTime", value); }
    int previousBestWave;
    float previousBestTime;
    /// <summary>Mode infini : la vague atteinte dépasse le record d'avant cette partie.</summary>
    public bool NewRecord => Infinite && previousBestWave > 0 && Level > previousBestWave;
    /// <summary>Mode infini : le temps survécu dépasse le record d'avant cette partie.</summary>
    public bool NewTimeRecord => Infinite && previousBestTime > 0f && GameStats.Time > previousBestTime;

    // Réglages d'UNE vague, calculés par DefForWave (plus saisis à la main).
    [Serializable]
    public class LevelDef
    {
        public int zombies = 4;                                         // zombies de la vague
        public float spawnInterval = 12f;                               // secondes entre deux apparitions
        public ZombieSpawner.Point[] points = { ZombieSpawner.Point.Exterieur };
        public float speed = 1f;                                        // vitesse de marche (x la vitesse du spawner)
        public int maxAlive = 4;                                        // zombies vivants en même temps, au maximum
        public float hpFactor = 1f;                                     // PV en plus (vagues très avancées du mode infini)
        [TextArea] public string banner;                                // annonce au début de la vague
    }

    // Les coefficients de la suite. Normal, vague w (w0 = w - 1) :
    //   zombies      = base + linéaire·w0 + quadratique·w0²   -> 4, 8, 12, 18, 24, 30, 38, 46, 55, 64…   (puis x facteur de difficulté)
    //   cadence      = max(min, départ · facteur^w0)           -> 12 s, 9,6, 7,7, 6,1, 4,9, 3,9, 3,1, 2,5 s…
    //   simultanés   = min(plafond, base + par_vague · w)      -> 3, 5, 7, 9, 11, 13, 14…
    //   vitesse      = min(plafond, départ + par_vague · w0)   -> 0,8, 0,9, 1,0, 1,1, 1,2…
    //   PV           = x(1 + croissance · (w - après_vague))   -> seulement après la vague « hpGrowthAfterWave »
    [Serializable]
    public class WaveFormula
    {
        [Header("Nombre de zombies")]
        public float zombiesBase = 3.5f;
        public float zombiesLinear = 3.7f;
        public float zombiesQuadratic = 0.34f;
        public int maxZombiesPerWave = 80;

        [Header("Cadence d'apparition (secondes entre deux zombies)")]
        public float intervalStart = 12f;
        public float intervalFactor = 0.8f;      // x à chaque vague
        public float intervalMin = 2.5f;

        [Header("Zombies vivants en même temps (garde les 72 fps du Quest)")]
        public float maxAliveBase = 1f;
        public float maxAlivePerWave = 2f;
        public int maxAliveCap = 14;

        [Header("Vitesse de marche (x la vitesse du spawner)")]
        public float speedStart = 0.8f;
        public float speedPerWave = 0.1f;
        public float speedCap = 1.5f;

        [Header("PV : croissance au-delà du dernier niveau de ZombieLevels (mode infini)")]
        public int hpGrowthAfterWave = 4;
        public float hpGrowthPerWave = 0.08f;
        public float hpGrowthCap = 2.5f;

        [Header("Pause entre deux vagues")]
        public float breakShrinkPerWave = 1.5f;  // la pause raccourcit à chaque vague…
        public float breakMin = 18f;             // …jusqu'à ce minimum (mode infini seulement)
    }

    public ZombieSpawner spawner;
    public WaveFormula formula = new WaveFormula();
    [Tooltip("Nombre de vagues de la CAMPAGNE (le générateur s'ouvre à la dernière). Le mode infini ignore ce nombre.")]
    public int campaignWaves = ZombieLevels.MaxLevel;
    [HideInInspector] public LevelDef[] levels = new LevelDef[0];   // plus utilisé (remplacé par `formula`) ; gardé pour que GameSettings compile toujours
    public int startLevel = 1;                 // test : commencer à une autre vague
    public float firstDelay = 15f;             // temps pour se préparer avant la 1re vague
    public float breakBetweenLevels = 35f;     // pause entre deux vagues (réparer les barricades, aller chercher des planches)
    public float skipHoldTime = 1f;            // maintenir B (manette droite) ce temps pour passer l'attente et lancer la vague
    const string SkipHint = "\nMaintenir B : lancer la vague";
    float skipHeld;
    public float lastLevelReinforcementEvery = 15f;   // niveau 4 : un renfort tant que le générateur n'est pas actif
    public int lastLevelMaxAlive = 9;
    public float victoryRestartDelay = 12f;
    public float breakHeal = 50f;              // PV rendus au début de chaque pause
    public float resumeDelay = 40f;            // [#121] reprise : temps pour se préparer (les barricades sont remises à zéro)

    [Header("Niveau 4 : condition du générateur [#120]")]
    public int generatorKills = 6;             // zombies à éliminer pendant le niveau 4…
    public float generatorDelay = 60f;         // …ou secondes à tenir dans le niveau 4

    [Header("Écran d'annonces (fixe, dans le bunker)")]
    public Transform screen;                   // créé par RoomBuilder ; absent : écran posé au mur sud du bunker
    public static readonly Vector3 DefaultScreenPosition = new Vector3(1.85f, 2.25f, 13.80f);

    [Header("Son")]
    public AudioClip startClip;                // joué au lancement de la partie (début de la scène)
    [Range(0f, 1f)] public float startVolume = 0.9f;
    [Range(0f, 1f)] public float alarmVolume = 0.45f;     // alarme de début de vague

    enum Phase { Waiting, Wave, Break, Victory }
    Phase phase = Phase.Waiting;
    float timer, spawnTimer, reinforceTimer;
    int spawned, pointIndex;
    LevelDef current;                           // réglages de la vague en cours (DefForWave)
    readonly List<Zombie> alive = new List<Zombie>();
    TextMesh banner, status;
    Coroutine bannerRoutine;
    AudioSource sfx;
    AudioClip alarm;

    public TextMesh StatusText => status;

    public int Alive { get { alive.RemoveAll(z => z == null || z.Dead); return alive.Count; } }
    public bool Victory => phase == Phase.Victory;

    // [#120] progression du niveau en cours (remise à zéro à chaque niveau)
    public int LevelKills { get; private set; }
    public float LevelTime { get; private set; }
    bool generatorAnnounced;
    int killsAtLevelStart;
    /// <summary>Le générateur peut démarrer : niveau 4 atteint et (assez de zombies éliminés ou délai écoulé).</summary>
    public bool GeneratorReady => !Infinite && Level >= campaignWaves && (LevelKills >= generatorKills || LevelTime >= generatorDelay);
    public string GeneratorCondition => Infinite ? "MODE INFINI :\ngénérateur désactivé" : "Éliminez " + Mathf.Max(0, generatorKills - LevelKills) + " zombie" + (generatorKills - LevelKills > 1 ? "s" : "")
                                      + "\nou tenez " + Mathf.CeilToInt(Mathf.Max(0f, generatorDelay - LevelTime)) + " s";

    // ---------------------------------------------------------------------------------------------------------
    //  LA SUITE : réglages de la vague n (n = 1, 2, 3… sans limite). Les facteurs de difficulté sont ceux du menu.
    // ---------------------------------------------------------------------------------------------------------
    static readonly ZombieSpawner.Point[] PointsExt = { ZombieSpawner.Point.Exterieur };
    static readonly ZombieSpawner.Point[] PointsCorridors = { ZombieSpawner.Point.Exterieur, ZombieSpawner.Point.CouloirGauche, ZombieSpawner.Point.CouloirDroit };
    static readonly ZombieSpawner.Point[] PointsAll = { ZombieSpawner.Point.Exterieur, ZombieSpawner.Point.CouloirGauche, ZombieSpawner.Point.CouloirDroit, ZombieSpawner.Point.Toit };

    // nombre et cadence : mêmes facteurs que GameSettings (la vitesse est déjà réglée sur le spawner par GameSettings)
    static void DifficultyFactors(out float count, out float interval)
    {
        count = 1f; interval = 1f;
        switch (GameSettings.Difficulty)
        {
            case GameSettings.Level.Facile: count = 0.75f; interval = 1.25f; break;
            case GameSettings.Level.Difficile: count = 1.3f; interval = 0.8f; break;
        }
    }

    public LevelDef DefForWave(int wave)
    {
        wave = Mathf.Max(1, wave);
        int w0 = wave - 1;
        var f = formula;
        DifficultyFactors(out float countMul, out float intervalMul);

        float n = (f.zombiesBase + f.zombiesLinear * w0 + f.zombiesQuadratic * w0 * w0) * countMul;
        return new LevelDef
        {
            zombies = Mathf.Clamp(Mathf.FloorToInt(n + 0.5f), 1, Mathf.Max(1, f.maxZombiesPerWave)),
            spawnInterval = Mathf.Max(f.intervalMin, f.intervalStart * Mathf.Pow(f.intervalFactor, w0) * intervalMul),
            maxAlive = Mathf.Clamp(Mathf.FloorToInt((f.maxAliveBase + f.maxAlivePerWave * wave) * countMul + 0.5f), 2, Mathf.Max(2, f.maxAliveCap)),
            speed = Mathf.Min(f.speedCap, f.speedStart + f.speedPerWave * w0),
            hpFactor = wave > f.hpGrowthAfterWave ? Mathf.Min(f.hpGrowthCap, 1f + f.hpGrowthPerWave * (wave - f.hpGrowthAfterWave)) : 1f,
            // vague 1 : un seul côté · vague 2 : + couloirs · vague 3+ : + toit (pas de zombie sur le toit aux vagues 1 et 2)
            points = wave <= 1 ? PointsExt : wave == 2 ? PointsCorridors : PointsAll,
            banner = BannerFor(wave),
        };
    }

    int LastWave => Infinite ? int.MaxValue : Mathf.Max(1, campaignWaves);
    bool IsLastCampaignWave(int wave) => !Infinite && wave >= Mathf.Max(1, campaignWaves);

    // pause entre deux vagues : constante en campagne, de plus en plus courte en mode infini
    float BreakFor(int wave) => Infinite ? Mathf.Max(formula.breakMin, breakBetweenLevels - formula.breakShrinkPerWave * (wave - 1)) : breakBetweenLevels;

    string BannerFor(int wave)
    {
        if (Infinite)
        {
            var n = DefForWaveCount(wave);
            return wave == 1 ? "MODE INFINI\nSurvivez le plus longtemps possible.\nLes couloirs sont verrouillés.\nBalles rares : la hache\net le pied de biche ne s'usent pas."
                             : "VAGUE " + wave + "\n" + n + " zombies";
        }
        if (IsLastCampaignWave(wave))
            return "NIVEAU " + wave + " · OBJECTIF FINAL\nLe GÉNÉRATEUR est ouvert :\nporte ouest, couloir gauche,\ntout au bout après la cuisine.\nIl démarre après " + generatorKills + " zombies abattus\nou " + Mathf.RoundToInt(generatorDelay) + " s. Puis abaissez son levier 5 s.";
        return wave == 1 ? "NIVEAU 1\nRestez dans le bunker :\nles couloirs sont verrouillés.\nBalles rares : la hache\net le pied de biche ne s'usent pas." : "NIVEAU " + wave;
    }

    // nombre de zombies de la vague (sans reconstruire un LevelDef entier : évite la récursion avec BannerFor)
    int DefForWaveCount(int wave)
    {
        int w0 = Mathf.Max(1, wave) - 1;
        DifficultyFactors(out float countMul, out _);
        float n = (formula.zombiesBase + formula.zombiesLinear * w0 + formula.zombiesQuadratic * w0 * w0) * countMul;
        return Mathf.Clamp(Mathf.FloorToInt(n + 0.5f), 1, Mathf.Max(1, formula.maxZombiesPerWave));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Level = 0; Instance = null; ResumeLevel = 0; Resumed = false; mode = -1; }

    void Awake()
    {
        Instance = this;
        Level = 0;
        Zombie.HuntPlayer = false;
        // [#121] reprise : on commence directement au niveau atteint, avec plus de temps pour se préparer
        Resumed = !Infinite && ResumeLevel > 1;            // mode infini : pas de reprise (on recommence à la vague 1)
        if (Resumed) { startLevel = Mathf.Clamp(ResumeLevel, 1, Mathf.Max(1, campaignWaves)); firstDelay = Mathf.Max(firstDelay, resumeDelay); }
        ResumeLevel = 0;
        startLevel = Mathf.Max(1, startLevel);
        previousBestWave = BestWave;                       // records d'avant cette partie (pour « NOUVEAU RECORD »)
        previousBestTime = BestTime;
        timer = firstDelay;
        GameStats.Reset();                     // [#122] nouvelle partie (aussi après « rejouer ») : bilan remis à zéro
    }

    void Start()
    {
        ZoneGate.UpdateAll(Resumed ? startLevel - 1 : 0);      // reprise : les zones des niveaux déjà passés sont ouvertes
        if (!screen)
        {
            screen = new GameObject("Ecran_Annonces").transform;
            screen.SetPositionAndRotation(DefaultScreenPosition, Quaternion.LookRotation(Vector3.forward));
        }
        // textes posés sur l'écran (repère de l'écran : x à droite, y en haut, -z vers le joueur), lisibles depuis le centre du bunker
        banner = MakeText("Annonce", screen, new Vector3(0f, 0.06f, -0.03f), 0.011f, 64, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));
        status = MakeText("Statut", screen, new Vector3(0f, -0.29f, -0.03f), 0.0085f, 64, TextAnchor.MiddleCenter, new Color(0.75f, 0.95f, 0.8f));
        banner.gameObject.SetActive(false);

        // son de lancement de la partie et alarme (non spatialisés : ils viennent de partout, on les entend hors du bunker)
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false; sfx.spatialBlend = 0f;
        alarm = ProcAudio.Alarm();
        if (startClip) sfx.PlayOneShot(startClip, startVolume);
        if (Resumed) Announce("REPRISE AU NIVEAU " + startLevel + "\nBarricadez-vous :\nla vague arrive dans " + Mathf.CeilToInt(firstDelay) + " s.", 8f);
        if (PlayerHealth.Instance) PlayerHealth.Instance.Died += OnPlayerDied;
    }

    static TextMesh MakeText(string name, Transform parent, Vector3 localPos, float charSize, int fontSize, TextAnchor anchor, Color c)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var tm = go.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = fontSize; tm.characterSize = charSize; tm.anchor = anchor; tm.alignment = TextAlignment.Center; tm.color = c;
        go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        return tm;
    }

    void Update()
    {
        // pendant le tutoriel : rien ne bouge (pas de compte à rebours, pas de zombie), même si le WaveManager a été réactivé
        if (spawner == null || Tutorial.Running) return;
        if (phase != Phase.Victory && !(PlayerHealth.Instance && PlayerHealth.Instance.Dead)) GameStats.Tick(Time.deltaTime);   // [#122] temps de jeu (figé à la fin)
        switch (phase)
        {
            case Phase.Waiting:
                timer -= Time.deltaTime;
                if (StatusChanged(0, Mathf.CeilToInt(Mathf.Max(0f, timer))))
                    SetStatus("Préparez-vous… " + statusValue + " s" + SkipHint);
                if (SkipPressed()) timer = 0f;
                if (timer <= 0f) StartLevel(startLevel);
                break;

            case Phase.Wave: UpdateWave(); break;

            case Phase.Break:
                timer -= Time.deltaTime;
                if (StatusChanged(1000 + Level, Mathf.CeilToInt(Mathf.Max(0f, timer))))
                    SetStatus((Infinite ? "Vague " : "Niveau ") + Level + " terminé · prochaine vague dans " + statusValue + " s : réparez !" + SkipHint);
                if (SkipPressed()) timer = 0f;
                if (timer <= 0f) StartLevel(Level + 1);
                break;
        }
    }

    // Passer l'attente (préparation ou pause entre deux vagues) : maintenir B 1 s, pour ne pas la lancer par erreur.
    // Rien pendant le menu pause. Après un passage, il faut relâcher B avant de pouvoir recommencer.
    bool SkipPressed()
    {
        if (PauseMenu.Paused || !VRInput.RightB()) { skipHeld = 0f; return false; }
        if (skipHeld < 0f) return false;                   // déjà utilisé : attendre que B soit relâché
        skipHeld += Time.unscaledDeltaTime;
        if (skipHeld < skipHoldTime) return false;
        skipHeld = -1f;
        return true;
    }

    void StartLevel(int lvl)
    {
        Level = Mathf.Clamp(lvl, 1, LastWave);
        current = DefForWave(Level);
        spawner.level = Mathf.Min(Level, ZombieLevels.MaxLevel);      // types de zombies et dégâts : plafonnés au dernier niveau de ZombieLevels
        spawned = 0; pointIndex = 0; spawnTimer = 1f; reinforceTimer = lastLevelReinforcementEvery;
        killsAtLevelStart = GameStats.Kills; LevelKills = 0; LevelTime = 0f; generatorAnnounced = false;
        Zombie.HuntPlayer = false;
        phase = Phase.Wave;
        ZoneGate.UpdateAll(Level);
        if (PlayerHealth.Instance) PlayerHealth.Instance.NewWave();
        if (sfx && alarm) sfx.PlayOneShot(alarm, alarmVolume);
        SaveRecords();
        Announce(current.banner + ZonesOpenedText(Level), 8f);
    }

    // [#106] zones ouvertes à ce niveau et ce qu'on y trouve (ZoneGate.contents)
    string ZonesOpenedText(int lvl)
    {
        if (IsLastCampaignWave(lvl)) return "";                  // dernier niveau de la campagne : l'annonce dit déjà où aller
        var sb = new System.Text.StringBuilder();
        foreach (var g in ZoneGate.All)
            if (g.unlockLevel == lvl)
                sb.Append("\nOUVERT : ").Append(g.zoneName).Append(string.IsNullOrEmpty(g.contents) ? "" : "\n· " + g.contents + " ·");
        return sb.ToString();
    }

    void UpdateWave()
    {
        var def = current;
        bool last = IsLastCampaignWave(Level);
        LevelTime += Time.deltaTime;
        LevelKills = GameStats.Kills - killsAtLevelStart;
        if (last && !generatorAnnounced && GeneratorReady)
        {
            generatorAnnounced = true;
            Zombie.HuntPlayer = true;          // la course finale : hors du bunker, tous les zombies vous traquent
            if (sfx && alarm) sfx.PlayOneShot(alarm, alarmVolume * 0.6f);
            Announce("LE GÉNÉRATEUR EST PRÊT\nAbaissez son levier\net tenez-le 5 s.\nDehors, ILS VOUS TRAQUENT.", 7f);
        }

        if (spawned < def.zombies)
        {
            spawnTimer -= Time.deltaTime;
            // trop de zombies déjà présents : le suivant attend qu'un autre soit abattu
            bool room = def.maxAlive <= 0 || Alive < def.maxAlive;
            if (spawnTimer <= 0f && room) { SpawnOne(def); spawnTimer = def.spawnInterval; }
        }
        else if (last)
        {
            reinforceTimer -= Time.deltaTime;
            if (reinforceTimer <= 0f && Alive < Mathf.Max(lastLevelMaxAlive, def.maxAlive)) { SpawnOne(def); reinforceTimer = lastLevelReinforcementEvery; }
        }
        else if (Alive == 0)
        {
            phase = Phase.Break;
            timer = BreakFor(Level);
            Debug.Log(GameStats.LogLine("vague terminée", Level, PlayerHealth.Instance ? PlayerHealth.Instance.Health : 0f, AmmoInGun()));
            if (PlayerHealth.Instance) PlayerHealth.Instance.Heal(breakHeal);
            Announce("VAGUE " + Level + " TERMINÉE\nRéparez les barricades.\n+" + Mathf.RoundToInt(breakHeal) + " PV" + (Infinite ? "\nSuivante : " + DefForWaveCount(Level + 1) + " zombies" : ""), 5f);
            return;
        }

        if (last && !GeneratorReady)
        {
            // [#120] tant que le générateur est bloqué : ce qu'il reste à faire
            int left = Mathf.Max(0, generatorKills - LevelKills), secs = Mathf.CeilToInt(Mathf.Max(0f, generatorDelay - LevelTime));
            if (StatusChanged(3000 + left, secs))
                SetStatus("Générateur bloqué · " + left + " zombie" + (left > 1 ? "s" : "") + " à abattre ou " + secs + " s");
        }
        else if (Infinite)
        {
            // mode infini : vague, zombies restants, temps survécu et record
            int remaining = Alive + Mathf.Max(0, def.zombies - spawned), secs = Mathf.FloorToInt(GameStats.Time);
            if (StatusChanged(2000 + Level, remaining * 100000 + secs))
                SetStatus("Vague " + Level + " · zombies : " + remaining + " · " + GameStats.FormatTime(secs) + (BestWave > 0 ? " · record : vague " + BestWave : ""));
        }
        else if (StatusChanged(2000 + Level, Alive + Mathf.Max(0, def.zombies - spawned)))
            SetStatus("Niveau " + Level + "/" + campaignWaves + " · zombies : " + statusValue + (last ? " · objectif : le générateur" : ""));
    }

    // [#123] poids des points d'apparition par niveau, dans l'ordre de LevelDef.points (Extérieur, couloir gauche, couloir droit, toit).
    // Dans le code et pas dans la scène : un réglage ici s'applique sans reconstruire MainScene.
    // Le toit (non barricadable) pèse peu au niveau 3 et plus au niveau 4.
    static readonly float[][] SpawnWeights =
    {
        new[] { 1f },
        new[] { 0.4f, 0.3f, 0.3f },
        new[] { 0.35f, 0.25f, 0.25f, 0.15f },
        new[] { 0.3f, 0.225f, 0.225f, 0.25f },
    };

    ZombieSpawner.Point PickPoint(LevelDef def)
    {
        var w = Level >= 1 ? SpawnWeights[Mathf.Min(Level, SpawnWeights.Length) - 1] : null;   // mode infini : au-delà du niveau 4, mêmes poids qu'au niveau 4
        if (w == null || w.Length != def.points.Length) return def.points[pointIndex++ % def.points.Length];   // points changés dans la scène : tour fixe
        float total = 0f;
        foreach (var x in w) total += x;
        float r = UnityEngine.Random.value * total;
        for (int i = 0; i < w.Length; i++) { r -= w[i]; if (r <= 0f) return def.points[i]; }
        return def.points[def.points.Length - 1];
    }

    void SpawnOne(LevelDef def)
    {
        var p = PickPoint(def);
        var z = spawner.Spawn(p);
        if (def.speed > 0f) z.walkSpeed *= def.speed;      // le spawner a mis sa vitesse (déjà réglée par la difficulté du menu)
        if (def.hpFactor > 1.001f) { z.healthMultiplier *= def.hpFactor; z.ApplyLevel(z.level); }   // vagues avancées du mode infini : PV recalculés
        alive.Add(z);
        spawned++;
    }

    // le joueur a activé le générateur
    public void GeneratorActivated()
    {
        if (phase == Phase.Victory) return;
        phase = Phase.Victory;
        Zombie.HuntPlayer = false;
        Debug.Log(GameStats.LogLine("victoire", Level, PlayerHealth.Instance ? PlayerHealth.Instance.Health : 0f, AmmoInGun()));
        foreach (var z in alive) if (z != null && !z.Dead) z.Die();
        SetStatus("");
        Announce("GÉNÉRATEUR ACTIVÉ\nVous avez échappé !", victoryRestartDelay);
        // [#122] bilan de fin (GameOverUI relance la partie) ; sans lui, relance simple
        if (GameOverUI.Instance) GameOverUI.Instance.ShowVictory();
        else StartCoroutine(RestartAfter(victoryRestartDelay));
    }

    IEnumerator RestartAfter(float s)
    {
        yield return new WaitForSeconds(s);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDestroy() { if (PlayerHealth.Instance) PlayerHealth.Instance.Died -= OnPlayerDied; }

    void OnPlayerDied()
    {
        Debug.Log(GameStats.LogLine("mort", Level, 0f, AmmoInGun()));
        SaveRecords();
    }

    // mode infini : on garde la meilleure vague et le meilleur temps
    void SaveRecords()
    {
        if (!Infinite) return;
        if (Level > BestWave) BestWave = Level;
        if (GameStats.Time > BestTime) BestTime = GameStats.Time;
        PlayerPrefs.Save();
    }

    static int AmmoInGun()
    {
        var p = FindFirstObjectByType<Pistol>();
        return p ? p.ammo : -1;
    }

    // pour les tests / le debug : saute directement à un niveau
    public void SkipToLevel(int lvl) => StartLevel(lvl);

    void SetStatus(string s) { if (status && status.text != s) status.text = s; }

    // le texte de statut n'est reconstruit que si ce qu'il affiche a changé (évite une chaîne neuve à chaque image)
    int statusKey = -1, statusValue = -1;
    bool StatusChanged(int key, int value)
    {
        if (key == statusKey && value == statusValue) return false;
        statusKey = key; statusValue = value;
        return true;
    }

    void Announce(string text, float seconds)
    {
        if (!banner || string.IsNullOrEmpty(text)) return;
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(ShowBanner(text, seconds));
    }

    IEnumerator ShowBanner(string text, float seconds)
    {
        banner.text = text;
        banner.gameObject.SetActive(true);
        var c = banner.color;
        for (float t = 0; t < seconds; t += Time.deltaTime)
        {
            c.a = Mathf.Clamp01(Mathf.Min(t / 0.4f, (seconds - t) / 1f));
            banner.color = c;
            yield return null;
        }
        banner.gameObject.SetActive(false);
    }
}
