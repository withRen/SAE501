using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

// Système de 5 vagues (GDD) :
//   Vague  Zombies  Ouvertures  Zones débloquées            Types de zombies
//     1       4         2       salle principale            standard
//     2       8         4       2ème salle                  standard + rapides (10 %)
//     3      12         4       3ème salle                  standard + rapides (20 %)
//     4      18         4       4ème salle                  + rapides (30 %) + renforcés (10 %)
//     5      24         4       générateur + évacuation     + rapides (40 %) + renforcés (10 %) : tenir jusqu'à l'activation du générateur
// Les zones sont débloquées par les ZoneGate (leur « unlockLevel » = numéro de vague) ; le générateur par Generator.unlockLevel (= 5).
// Chaque vague : apparitions espacées, puis pause pour réparer les barricades. Tout se règle dans l'inspecteur (levels).
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public static int Level { get; private set; }          // 0 = partie pas commencée

    public const int WaveCount = 5;

    [Serializable]
    public class LevelDef
    {
        public int zombies = 4;                                         // zombies de la vague
        public float spawnInterval = 12f;                               // secondes entre deux apparitions
        public int maxAlive = 0;                                        // zombies vivants en même temps (0 = pas de limite) : protège les 72 fps
        public int openings = 0;                                        // ouvertures attaquées (0 = toutes), voir openingOrder
        public float speedMultiplier = 1f;                              // vitesse de tous les zombies de la vague
        [Range(0f, 1f)] public float fastChance = 0f;                   // chance qu'un zombie soit rapide
        [Range(0f, 1f)] public float reinforcedChance = 0f;             // chance qu'un zombie soit renforcé
        public ZombieSpawner.Point[] points = { ZombieSpawner.Point.Exterieur };
        [TextArea] public string banner;                                // annonce au début de la vague (les zones débloquées sont ajoutées toutes seules)
    }

    public ZombieSpawner spawner;
    public LevelDef[] levels = DefaultLevels();
    [Tooltip("Ouvertures par ordre d'attaque : la vague N n'attaque que les N premières (champ « openings »). Vide = toutes les ouvertures de la scène.")]
    public Opening[] openingOrder;
    public int startLevel = 1;                 // test : commencer à une autre vague
    public float firstDelay = 15f;             // temps pour se préparer avant la 1re vague
    public float breakBetweenLevels = 35f;     // pause entre deux vagues (réparer les barricades, aller chercher des planches)
    public float lastLevelReinforcementEvery = 15f;   // vague 5 : un renfort tant que le générateur n'est pas actif
    public int lastLevelMaxAlive = 6;
    public float victoryRestartDelay = 12f;

    [Header("Son")]
    public AudioClip startClip;                // joué au lancement de la partie (début de la scène)
    [Range(0f, 1f)] public float startVolume = 0.9f;

    enum Phase { Waiting, Wave, Break, Victory }
    Phase phase = Phase.Waiting;
    float timer, spawnTimer, reinforceTimer;
    int spawned, pointIndex;
    readonly List<Zombie> alive = new List<Zombie>();
    readonly int[] spawnedByType = new int[3];
    Dictionary<Opening, bool> baseUsable;
    TextMesh banner, status;
    Coroutine bannerRoutine;

    public int Alive { get { alive.RemoveAll(z => z == null || z.Dead); return alive.Count; } }
    public bool Victory => phase == Phase.Victory;

    static LevelDef Def(int zombies, float interval, int maxAlive, int openings, float speed, float fast, float reinforced, ZombieSpawner.Point[] points, string banner) =>
        new LevelDef { zombies = zombies, spawnInterval = interval, maxAlive = maxAlive, openings = openings, speedMultiplier = speed,
                       fastChance = fast, reinforcedChance = reinforced, points = points, banner = banner };

    static LevelDef[] DefaultLevels()
    {
        var ext = new[] { ZombieSpawner.Point.Exterieur };
        var all = new[] { ZombieSpawner.Point.Exterieur, ZombieSpawner.Point.CouloirGauche, ZombieSpawner.Point.CouloirDroit };
        return new[]
        {
            Def(4,  12f,  4, 2, 0.8f, 0.00f, 0.00f, ext, "Approche frontale, très lente.\nRestez dans la salle principale."),
            Def(8,  9f,   6, 4, 1f,   0.10f, 0.00f, all, "Les zombies arrivent par les angles morts.\nLa cadence s'accélère."),
            Def(12, 7f,   8, 4, 1f,   0.20f, 0.00f, all, "Cadence accélérée."),
            Def(18, 5.5f, 10, 4, 1f,  0.30f, 0.10f, all, "Assaut de tous les côtés.\nTournez-vous vite (snap turn) !"),
            Def(24, 4.5f, 12, 4, 1f,  0.40f, 0.10f, all, "Assaut maximal.\nObjectif : ACTIVER le générateur !"),
        };
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Level = 0; Instance = null; }

    void Awake()
    {
        Instance = this;
        Level = 0;
        timer = firstDelay;

        // une scène enregistrée avec l'ancien système (4 niveaux) : on repart des 5 vagues par défaut
        if (levels == null || levels.Length != WaveCount)
        {
            Debug.LogWarning("WaveManager : " + (levels == null ? 0 : levels.Length) + " niveaux trouvés, " + WaveCount + " vagues attendues -> valeurs par défaut rechargées.");
            levels = DefaultLevels();
        }
        startLevel = Mathf.Clamp(startLevel, 1, levels.Length);
    }

    void Start()
    {
        ZoneGate.UpdateAll(0);
        CacheOpenings();

        var head = Camera.main ? Camera.main.transform : null;
        banner = MakeText("Annonce", head, new Vector3(0f, 0.12f, 1.5f), 0.006f, 64, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));
        status = MakeText("Statut", head, new Vector3(0f, -0.30f, 1.5f), 0.0035f, 64, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.55f));
        banner.gameObject.SetActive(false);

        // son de lancement de la partie (non spatialisé : il vient de partout)
        if (startClip)
        {
            var a = gameObject.AddComponent<AudioSource>();
            a.playOnAwake = false; a.spatialBlend = 0f; a.volume = startVolume;
            a.PlayOneShot(startClip);
        }
    }

    static TextMesh MakeText(string name, Transform parent, Vector3 localPos, float charSize, int fontSize, TextAnchor anchor, Color c)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var tm = go.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = fontSize; tm.characterSize = charSize; tm.anchor = anchor; tm.alignment = TextAlignment.Center; tm.color = c;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = tm.font.material;
        mr.material.renderQueue = 4100;
        return tm;
    }

    // ---- ouvertures attaquées -------------------------------------------------------------------------------

    // on mémorise l'état de départ de la scène : une ouverture interdite aux zombies par la scène le reste
    void CacheOpenings()
    {
        baseUsable = new Dictionary<Opening, bool>();
        foreach (var o in Opening.All) baseUsable[o] = o.zombiesCanUse;
        if (openingOrder == null || openingOrder.Length == 0) openingOrder = Opening.All.ToArray();
    }

    // seules les n premières ouvertures de openingOrder sont attaquées (n <= 0 : toutes)
    void ApplyOpenings(int n)
    {
        for (int i = 0; i < openingOrder.Length; i++)
        {
            var o = openingOrder[i];
            if (!o) continue;
            bool allowedByScene = !baseUsable.TryGetValue(o, out var b) || b;
            o.zombiesCanUse = allowedByScene && (n <= 0 || i < n);
        }
    }

    // ---- déroulement ----------------------------------------------------------------------------------------

    void Update()
    {
        if (spawner == null) return;
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
                    SetStatus("Vague " + Level + " terminée · prochaine vague dans " + statusValue + " s : réparez !");
                if (timer <= 0f) StartLevel(Level + 1);
                break;
        }
    }

    void StartLevel(int lvl)
    {
        Level = Mathf.Clamp(lvl, 1, levels.Length);
        var def = levels[Level - 1];
        spawner.level = Level;
        spawned = 0; pointIndex = 0; spawnTimer = 1f; reinforceTimer = lastLevelReinforcementEvery;
        Array.Clear(spawnedByType, 0, spawnedByType.Length);
        phase = Phase.Wave;
        ZoneGate.UpdateAll(Level);
        ApplyOpenings(def.openings);
        if (PlayerHealth.Instance) PlayerHealth.Instance.NewWave();
        Announce(BannerFor(Level), 6f);
    }

    // texte de la vague + zones débloquées (lues sur les ZoneGate de la scène)
    string BannerFor(int lvl)
    {
        var sb = new StringBuilder("VAGUE ").Append(lvl).Append('/').Append(levels.Length);
        var def = levels[lvl - 1];
        if (!string.IsNullOrEmpty(def.banner)) sb.Append('\n').Append(def.banner);
        foreach (var g in ZoneGate.All)
            if (g.unlockLevel == lvl && !string.IsNullOrEmpty(g.zoneName)) sb.Append("\nDéverrouillé : ").Append(g.zoneName);
        return sb.ToString();
    }

    void UpdateWave()
    {
        var def = levels[Level - 1];
        bool last = Level == levels.Length;

        if (spawned < def.zombies)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && (def.maxAlive <= 0 || Alive < def.maxAlive)) { SpawnOne(def); spawnTimer = def.spawnInterval; }
        }
        else if (last)
        {
            // dernière vague : la vague ne se termine pas en tuant tout le monde, il faut activer le générateur
            reinforceTimer -= Time.deltaTime;
            if (reinforceTimer <= 0f && Alive < lastLevelMaxAlive) { SpawnOne(def); reinforceTimer = lastLevelReinforcementEvery; }
        }
        else if (Alive == 0)
        {
            phase = Phase.Break;
            timer = breakBetweenLevels;
            Announce("VAGUE " + Level + " TERMINÉE\nRéparez les barricades.", 5f);
            return;
        }

        if (StatusChanged(2000 + Level, Alive + Mathf.Max(0, def.zombies - spawned)))
            SetStatus("Vague " + Level + "/" + levels.Length + " · zombies : " + statusValue + (last ? " · objectif : le générateur" : ""));
    }

    // un seul tirage : renforcé, puis rapide, sinon standard (les pourcentages du GDD sont donc exacts)
    ZombieType PickType(LevelDef def)
    {
        float r = UnityEngine.Random.value;
        if (r < def.reinforcedChance) return ZombieType.Reinforced;
        if (r < def.reinforcedChance + def.fastChance) return ZombieType.Fast;
        return ZombieType.Standard;
    }

    void SpawnOne(LevelDef def)
    {
        var p = def.points[pointIndex++ % def.points.Length];
        var type = PickType(def);
        alive.Add(spawner.Spawn(p, type, def.speedMultiplier));
        spawnedByType[(int)type]++;
        spawned++;
    }

    // le joueur a activé le générateur
    public void GeneratorActivated()
    {
        if (phase == Phase.Victory) return;
        phase = Phase.Victory;
        foreach (var z in alive) if (z != null && !z.Dead) z.Die();
        SetStatus("");
        Announce("GÉNÉRATEUR ACTIVÉ\nVous avez échappé !", victoryRestartDelay);
        StartCoroutine(RestartAfter(victoryRestartDelay));
    }

    IEnumerator RestartAfter(float s)
    {
        yield return new WaitForSeconds(s);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // pour les tests / le debug : saute directement à une vague
    public void SkipToLevel(int lvl) => StartLevel(lvl);

    // répartition des types dans la vague en cours (debug : standard / rapides / renforcés)
    public string TypeStats() => spawnedByType[0] + " standard, " + spawnedByType[1] + " rapides, " + spawnedByType[2] + " renforcés";

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
