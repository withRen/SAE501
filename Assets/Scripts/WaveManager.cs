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
// [Survie] Mode « vagues infinies » (choisi dans le menu SOLO, voir Endless) : plus de niveau final ni de victoire, le but est de tenir le plus longtemps.
//   Les 4 vagues définies dans `levels` servent de base (mêmes zombies, même rythme) ; au-delà, BuildEndlessWave() prolonge chaque
//   réglage par une formule (suite arithmétique pour le nombre de zombies, approche d'un plafond pour la vitesse, le nombre de zombies
//   vivants en même temps et le délai entre deux apparitions). Aucun tableau de vagues à remplir à la main.
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public static int Level { get; private set; }          // 0 = partie pas commencée (en mode survie : numéro de la vague, sans limite)

    /// <summary>Mode vagues infinies. Fixé par le menu (MainMenu) avant de charger la scène de jeu ; reste vrai pour « rejouer ».</summary>
    public static bool Endless { get; set; }

    [Serializable]
    public class LevelDef
    {
        public int zombies = 4;                                         // zombies de la vague
        public float spawnInterval = 12f;                               // secondes entre deux apparitions
        public ZombieSpawner.Point[] points = { ZombieSpawner.Point.Exterieur };
        public float speed = 1f;                                        // vitesse de marche (x la vitesse du spawner)
        public int maxAlive = 4;                                        // zombies vivants en même temps, au maximum
        [TextArea] public string banner;                                // annonce au début du niveau
    }

    public ZombieSpawner spawner;
    public LevelDef[] levels = DefaultLevels();
    public int startLevel = 1;                 // test : commencer à un autre niveau
    public float firstDelay = 15f;             // temps pour se préparer avant la 1re vague
    public float breakBetweenLevels = 35f;     // pause entre deux vagues (réparer les barricades, aller chercher des planches)
    public float lastLevelReinforcementEvery = 15f;   // niveau 4 : un renfort tant que le générateur n'est pas actif
    public int lastLevelMaxAlive = 9;
    public float victoryRestartDelay = 12f;

    [Header("Mode survie : vagues infinies")]
    [Tooltip("Test : lance la scène de jeu directement en mode survie (sans passer par le menu).")]
    public bool forceEndless = false;
    [Tooltip("Plafond de la vitesse des zombies (multiplicateur, comme LevelDef.speed). La vitesse s'en approche sans jamais le dépasser.")]
    public float endlessSpeedCap = 2f;
    [Tooltip("Plafond de zombies vivants en même temps (performances du casque).")]
    public int endlessMaxAliveCap = 20;
    [Tooltip("Délai minimal entre deux apparitions (secondes) : l'intervalle s'en approche sans jamais l'atteindre.")]
    public float endlessMinInterval = 1.5f;
    [Tooltip("Au-delà de la dernière vague définie : PV en plus par vague (0,1 = +10 %), et plafond du multiplicateur.")]
    public float endlessHpPerWave = 0.10f, endlessHpCap = 3f;
    [Tooltip("Idem pour les dégâts infligés au joueur.")]
    public float endlessDamagePerWave = 0.04f, endlessDamageCap = 1.5f;
    [Tooltip("La pause entre deux vagues raccourcit de ce nombre de secondes par vague (au-delà de la dernière définie), jusqu'au minimum.")]
    public float endlessBreakShrink = 1.5f, endlessBreakMin = 20f;
    [Tooltip("Ajuste le nombre de zombies et de zombies simultanés selon la difficulté du menu : FACILE, NORMAL, DIFFICILE. NORMAL = vagues définies telles quelles.")]
    public float[] endlessDifficultyScale = { 0.85f, 1f, 1.2f };

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
    LevelDef cur;                              // vague en cours (celle de `levels`, ou celle calculée en mode survie)
    float timer, spawnTimer, reinforceTimer;
    int spawned, pointIndex;
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
    public bool GeneratorReady => !Endless && Level >= levels.Length && (LevelKills >= generatorKills || LevelTime >= generatorDelay);
    public string GeneratorCondition => "Éliminez " + Mathf.Max(0, generatorKills - LevelKills) + " zombie" + (generatorKills - LevelKills > 1 ? "s" : "")
                                      + "\nou tenez " + Mathf.CeilToInt(Mathf.Max(0f, generatorDelay - LevelTime)) + " s";

    static LevelDef[] DefaultLevels() => new[]
    {
        new LevelDef { zombies = 6, speed = 0.8f, maxAlive = 3, spawnInterval = 10f, points = new[] { ZombieSpawner.Point.Exterieur },
            banner = "NIVEAU 1\nRestez dans le bunker :\nles couloirs sont verrouillés." },
        new LevelDef { zombies = 9, speed = 0.95f, maxAlive = 4, spawnInterval = 8f, points = new[] { ZombieSpawner.Point.Exterieur, ZombieSpawner.Point.CouloirGauche, ZombieSpawner.Point.CouloirDroit },
            banner = "NIVEAU 2" },
        new LevelDef { zombies = 12, speed = 1.05f, maxAlive = 5, spawnInterval = 6.5f, points = new[] { ZombieSpawner.Point.Exterieur, ZombieSpawner.Point.CouloirGauche, ZombieSpawner.Point.CouloirDroit, ZombieSpawner.Point.Toit },
            banner = "NIVEAU 3" },
        new LevelDef { zombies = 15, speed = 1.2f, maxAlive = 7, spawnInterval = 5.5f, points = new[] { ZombieSpawner.Point.Exterieur, ZombieSpawner.Point.CouloirGauche, ZombieSpawner.Point.CouloirDroit, ZombieSpawner.Point.Toit },
            // [#107] où aller, quoi faire
            banner = "NIVEAU 4 · OBJECTIF FINAL\nLe GÉNÉRATEUR est ouvert :\nporte ouest, couloir gauche,\ntout au bout après la cuisine.\nIl démarre après 6 zombies abattus\nou 60 s. Puis abaissez son levier 5 s." },
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Level = 0; Instance = null; }

    void Awake()
    {
        Instance = this;
        Level = 0;
        if (forceEndless) Endless = true;
        timer = firstDelay;
        GameStats.Reset();
        EndlessRecord.BeginRun();              // record du mode survie : on mémorise celui d'avant la partie                     // [#122] nouvelle partie (aussi après « rejouer ») : bilan remis à zéro
    }

    // quitter la partie (menu pause, rejouer…) : la progression du mode survie compte pour le record
    void OnDestroy()
    {
        if (Endless && Level >= 1) EndlessRecord.Submit(Level, GameStats.Time);
    }

    void Start()
    {
        ZoneGate.UpdateAll(0);
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
                    SetStatus("Préparez-vous… " + statusValue + " s");
                if (timer <= 0f) StartLevel(startLevel);
                break;

            case Phase.Wave: UpdateWave(); break;

            case Phase.Break:
                timer -= Time.deltaTime;
                if (StatusChanged(1000 + Level, Mathf.CeilToInt(Mathf.Max(0f, timer))))
                    SetStatus((Endless ? "Vague " : "Niveau ") + Level + " terminé · prochaine vague dans " + statusValue + " s : réparez !");
                if (timer <= 0f) StartLevel(Level + 1);
                break;
        }
    }

    void StartLevel(int lvl)
    {
        Level = Endless ? Mathf.Max(1, lvl) : Mathf.Clamp(lvl, 1, levels.Length);
        cur = Endless ? BuildEndlessWave(Level) : levels[Level - 1];
        spawner.level = Mathf.Min(Level, levels.Length);   // le spawner et ZombieLevels s'arrêtent au dernier niveau défini ; la suite est gérée ici (ApplyEndlessScaling)
        spawned = 0; pointIndex = 0; spawnTimer = 1f; reinforceTimer = lastLevelReinforcementEvery;
        killsAtLevelStart = GameStats.Kills; LevelKills = 0; LevelTime = 0f; generatorAnnounced = false;
        phase = Phase.Wave;
        ZoneGate.UpdateAll(Level);
        if (PlayerHealth.Instance) PlayerHealth.Instance.NewWave();
        if (sfx && alarm) sfx.PlayOneShot(alarm, alarmVolume);
        if (Endless) EndlessRecord.Submit(Level, GameStats.Time);
        Announce((Endless ? EndlessBanner(Level) : cur.banner) + ZonesOpenedText(Level), 8f);
    }

    static string EndlessBanner(int n)
    {
        if (n == 1) return "VAGUE 1 · MODE SURVIE\nTenez le plus longtemps possible.\nLes couloirs sont verrouillés.";
        return "VAGUE " + n;
    }

    // [#106] zones ouvertes à ce niveau et ce qu'on y trouve (ZoneGate.contents)
    static string ZonesOpenedText(int lvl)
    {
        if (!Endless && lvl >= ZombieLevels.MaxLevel) return "";   // niveau 4 : l'annonce dit déjà où aller (en mode survie, la zone est annoncée comme les autres)
        var sb = new System.Text.StringBuilder();
        foreach (var g in ZoneGate.All)
            if (g.unlockLevel == lvl)
                sb.Append("\nOUVERT : ").Append(g.zoneName).Append(string.IsNullOrEmpty(g.contents) ? "" : "\n· " + g.contents + " ·");
        return sb.ToString();
    }

    void UpdateWave()
    {
        var def = cur;
        bool last = !Endless && Level == levels.Length;      // mode survie : jamais de dernier niveau
        LevelTime += Time.deltaTime;
        LevelKills = GameStats.Kills - killsAtLevelStart;
        if (last && !generatorAnnounced && GeneratorReady)
        {
            generatorAnnounced = true;
            if (sfx && alarm) sfx.PlayOneShot(alarm, alarmVolume * 0.6f);
            Announce("LE GÉNÉRATEUR EST PRÊT\nAbaissez son levier\net tenez-le 5 s.", 7f);
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
            timer = Endless ? EndlessBreak(Level) : breakBetweenLevels;
            Announce("VAGUE " + Level + " TERMINÉE\nRéparez les barricades.", 5f);
            return;
        }

        if (last && !GeneratorReady)
        {
            // [#120] tant que le générateur est bloqué : ce qu'il reste à faire
            int left = Mathf.Max(0, generatorKills - LevelKills), secs = Mathf.CeilToInt(Mathf.Max(0f, generatorDelay - LevelTime));
            if (StatusChanged(3000 + left, secs))
                SetStatus("Générateur bloqué · " + left + " zombie" + (left > 1 ? "s" : "") + " à abattre ou " + secs + " s");
        }
        else if (Endless)
        {
            // survie : numéro de la vague, zombies restants et temps tenu (la ligne change chaque seconde)
            if (StatusChanged(2000 + Level, Alive + Mathf.Max(0, def.zombies - spawned), Mathf.FloorToInt(GameStats.Time)))
                SetStatus("Vague " + Level + " · zombies : " + statusValue + " · " + ShortTime(GameStats.Time));
        }
        else if (StatusChanged(2000 + Level, Alive + Mathf.Max(0, def.zombies - spawned)))
            SetStatus("Niveau " + Level + "/" + levels.Length + " · zombies : " + statusValue + (last ? " · objectif : le générateur" : ""));
    }

    // ---------- mode survie : vagues générées par formule ----------
    // Les vagues 1 à L (L = levels.Length, 4 aujourd'hui) sont celles de `levels`, avec le réglage de difficulté du menu.
    // Au-delà, chaque réglage prolonge la tendance de ces L vagues (k = numéro de la vague - L) :
    //   zombies de la vague        suite arithmétique : même progression que les vagues définies (+3 par vague aujourd'hui)
    //   vitesse / zombies vivants  s'approchent d'un plafond : au début la même pente que les vagues définies, puis ça ralentit
    //   délai entre apparitions    s'approche d'un plancher, de la même façon
    // Si on modifie `levels` (nombre de zombies, vitesse…), la suite infinie s'adapte toute seule.
    LevelDef BuildEndlessWave(int n)
    {
        int L = levels.Length;
        var first = levels[0];
        var last = levels[L - 1];
        float diff = EndlessDifficulty();

        var d = new LevelDef();
        if (n <= L)
        {
            var src = levels[n - 1];
            d.zombies = src.zombies; d.speed = src.speed; d.maxAlive = src.maxAlive; d.spawnInterval = src.spawnInterval;
            d.points = src.points;
        }
        else
        {
            int k = n - L;
            d.zombies = Mathf.RoundToInt(last.zombies + (L > 1 ? (last.zombies - first.zombies) / (float)(L - 1) : 3f) * k);
            d.speed = Approach(first.speed, last.speed, L, endlessSpeedCap, k);
            d.maxAlive = Mathf.RoundToInt(Approach(first.maxAlive, last.maxAlive, L, endlessMaxAliveCap, k));
            d.spawnInterval = Approach(first.spawnInterval, last.spawnInterval, L, endlessMinInterval, k);
            d.points = last.points;                       // toutes les zones de spawn, toit compris
        }
        // difficulté du menu : plus ou moins de zombies, et de zombies en même temps (NORMAL = inchangé)
        d.zombies = Mathf.Max(1, Mathf.RoundToInt(d.zombies * diff));
        d.maxAlive = Mathf.Max(1, Mathf.RoundToInt(d.maxAlive * diff));
        d.banner = EndlessBanner(n);
        return d;
    }

    // Prolonge un réglage : même pente moyenne que les vagues définies (first -> last sur L vagues) pour la 1re vague de plus,
    // puis la pente diminue (suite géométrique de raison r) et la valeur tend vers `cap` sans jamais l'atteindre.
    //   valeur(k) = cap + (last - cap) * r^k        avec r = 1 - pente / (cap - last)
    static float Approach(float first, float last, int L, float cap, int k)
    {
        float slope = L > 1 ? (last - first) / (L - 1) : 0f;
        float gap = cap - last;
        if (Mathf.Abs(gap) < 1e-4f || Mathf.Abs(slope) < 1e-6f || slope * gap <= 0f) return last;   // déjà au plafond, ou pas de tendance
        float r = Mathf.Clamp(1f - slope / gap, 0.5f, 0.98f);
        return cap + (last - cap) * Mathf.Pow(r, k);
    }

    float EndlessDifficulty()
    {
        if (endlessDifficultyScale == null || endlessDifficultyScale.Length == 0) return 1f;
        int i = Mathf.Clamp((int)GameSettings.Difficulty, 0, endlessDifficultyScale.Length - 1);     // 0 facile, 1 normal, 2 difficile
        return Mathf.Max(0.1f, endlessDifficultyScale[i]);
    }

    float EndlessBreak(int n) => Mathf.Max(endlessBreakMin, breakBetweenLevels - endlessBreakShrink * Mathf.Max(0, n - levels.Length));

    void SpawnOne(LevelDef def)
    {
        var p = def.points[pointIndex++ % def.points.Length];
        var z = spawner.Spawn(p);
        if (def.speed > 0f) z.walkSpeed *= def.speed;      // le spawner a mis sa vitesse (déjà réglée par la difficulté du menu)
        if (Endless && Level > levels.Length)
        {
            // au-delà du dernier niveau défini, ZombieLevels n'augmente plus rien : PV et dégâts continuent ici
            int k = Level - levels.Length;
            z.ApplyEndlessScaling(Mathf.Min(endlessHpCap, 1f + endlessHpPerWave * k), Mathf.Min(endlessDamageCap, 1f + endlessDamagePerWave * k));
        }
        alive.Add(z);
        spawned++;
    }

    // le joueur a activé le générateur
    public void GeneratorActivated()
    {
        if (Endless || phase == Phase.Victory) return;      // mode survie : pas de victoire, le générateur est hors service
        phase = Phase.Victory;
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

    // pour les tests / le debug : saute directement à un niveau
    public void SkipToLevel(int lvl) => StartLevel(lvl);

    void SetStatus(string s) { if (status && status.text != s) status.text = s; }

    // le texte de statut n'est reconstruit que si ce qu'il affiche a changé (évite une chaîne neuve à chaque image)
    int statusKey = -1, statusValue = -1, statusExtra = -1;
    bool StatusChanged(int key, int value, int extra = 0)
    {
        if (key == statusKey && value == statusValue && extra == statusExtra) return false;
        statusKey = key; statusValue = value; statusExtra = extra;
        return true;
    }

    static string ShortTime(float s)
    {
        int t = Mathf.FloorToInt(s);
        return (t / 60) + ":" + (t % 60).ToString("00");
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
