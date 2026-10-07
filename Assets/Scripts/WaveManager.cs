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
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public static int Level { get; private set; }          // 0 = partie pas commencée

    [Serializable]
    public class LevelDef
    {
        public int zombies = 4;                                         // zombies de la vague
        public float spawnInterval = 12f;                               // secondes entre deux apparitions
        public ZombieSpawner.Point[] points = { ZombieSpawner.Point.Exterieur };
        [TextArea] public string banner;                                // annonce au début du niveau
    }

    public ZombieSpawner spawner;
    public LevelDef[] levels = DefaultLevels();
    public int startLevel = 1;                 // test : commencer à un autre niveau
    public float firstDelay = 15f;             // temps pour se préparer avant la 1re vague
    public float breakBetweenLevels = 20f;     // pause entre deux vagues (réparer les barricades)
    public float lastLevelReinforcementEvery = 15f;   // niveau 4 : un renfort tant que le générateur n'est pas actif
    public int lastLevelMaxAlive = 6;
    public float victoryRestartDelay = 12f;

    enum Phase { Waiting, Wave, Break, Victory }
    Phase phase = Phase.Waiting;
    float timer, spawnTimer, reinforceTimer;
    int spawned, pointIndex;
    readonly List<Zombie> alive = new List<Zombie>();
    TextMesh banner, status;
    Coroutine bannerRoutine;

    public int Alive { get { alive.RemoveAll(z => z == null || z.Dead); return alive.Count; } }
    public bool Victory => phase == Phase.Victory;

    static LevelDef[] DefaultLevels() => new[]
    {
        new LevelDef { zombies = 4, spawnInterval = 12f, points = new[] { ZombieSpawner.Point.Exterieur },
            banner = "NIVEAU 1\nRestez dans le bunker : les couloirs sont verrouillés.\nObjectif : survivre, puis activer le générateur." },
        new LevelDef { zombies = 6, spawnInterval = 10f, points = new[] { ZombieSpawner.Point.Exterieur, ZombieSpawner.Point.CouloirGauche, ZombieSpawner.Point.CouloirDroit },
            banner = "NIVEAU 2\nCouloirs et cuisine déverrouillés." },
        new LevelDef { zombies = 8, spawnInterval = 8f, points = new[] { ZombieSpawner.Point.Exterieur, ZombieSpawner.Point.CouloirGauche, ZombieSpawner.Point.CouloirDroit },
            banner = "NIVEAU 3\nStockage déverrouillé." },
        new LevelDef { zombies = 10, spawnInterval = 7f, points = new[] { ZombieSpawner.Point.Exterieur, ZombieSpawner.Point.CouloirGauche, ZombieSpawner.Point.CouloirDroit },
            banner = "NIVEAU 4\nGénérateur déverrouillé.\nObjectif : l'ACTIVER !" },
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Level = 0; Instance = null; }

    void Awake()
    {
        Instance = this;
        Level = 0;
        timer = firstDelay;
    }

    void Start()
    {
        ZoneGate.UpdateAll(0);
        var head = Camera.main ? Camera.main.transform : null;
        banner = MakeText("Annonce", head, new Vector3(0f, 0.12f, 1.5f), 0.006f, 64, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));
        status = MakeText("Statut", head, new Vector3(0f, -0.30f, 1.5f), 0.0035f, 64, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.55f));
        banner.gameObject.SetActive(false);
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

    void Update()
    {
        if (spawner == null) return;
        switch (phase)
        {
            case Phase.Waiting:
                timer -= Time.deltaTime;
                SetStatus("Préparez-vous… " + Mathf.CeilToInt(Mathf.Max(0f, timer)) + " s");
                if (timer <= 0f) StartLevel(startLevel);
                break;

            case Phase.Wave: UpdateWave(); break;

            case Phase.Break:
                timer -= Time.deltaTime;
                SetStatus("Niveau " + Level + " terminé · prochaine vague dans " + Mathf.CeilToInt(Mathf.Max(0f, timer)) + " s : réparez !");
                if (timer <= 0f) StartLevel(Level + 1);
                break;
        }
    }

    void StartLevel(int lvl)
    {
        Level = Mathf.Clamp(lvl, 1, levels.Length);
        spawner.level = Level;
        spawned = 0; pointIndex = 0; spawnTimer = 1f; reinforceTimer = lastLevelReinforcementEvery;
        phase = Phase.Wave;
        ZoneGate.UpdateAll(Level);
        if (PlayerHealth.Instance) PlayerHealth.Instance.NewWave();
        Announce(levels[Level - 1].banner, 6f);
    }

    void UpdateWave()
    {
        var def = levels[Level - 1];
        bool last = Level == levels.Length;

        if (spawned < def.zombies)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f) { SpawnOne(def); spawnTimer = def.spawnInterval; }
        }
        else if (last)
        {
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

        SetStatus("Niveau " + Level + "/" + levels.Length + " · zombies : " + (Alive + Mathf.Max(0, def.zombies - spawned)) + (last ? " · objectif : le générateur" : ""));
    }

    void SpawnOne(LevelDef def)
    {
        var p = def.points[pointIndex++ % def.points.Length];
        alive.Add(spawner.Spawn(p));
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

    // pour les tests / le debug : saute directement à un niveau
    public void SkipToLevel(int lvl) => StartLevel(lvl);

    void SetStatus(string s) { if (status && status.text != s) status.text = s; }

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
