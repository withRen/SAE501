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
    readonly List<Zombie> alive = new List<Zombie>();
    TextMesh banner, status;
    Coroutine bannerRoutine;
    AudioSource sfx;
    AudioClip alarm;

    public TextMesh StatusText => status;

    public int Alive { get { alive.RemoveAll(z => z == null || z.Dead); return alive.Count; } }
    public bool Victory => phase == Phase.Victory;

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
            banner = "NIVEAU 4 · OBJECTIF FINAL\nLe GÉNÉRATEUR est ouvert :\nporte ouest, couloir gauche,\ntout au bout après la cuisine.\nAbaissez son levier, tenez 5 s." },
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
                    SetStatus("Niveau " + Level + " terminé · prochaine vague dans " + statusValue + " s : réparez !");
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
        if (sfx && alarm) sfx.PlayOneShot(alarm, alarmVolume);
        Announce(levels[Level - 1].banner + ZonesOpenedText(Level), 8f);
    }

    // [#106] zones ouvertes à ce niveau et ce qu'on y trouve (ZoneGate.contents)
    static string ZonesOpenedText(int lvl)
    {
        if (lvl >= ZombieLevels.MaxLevel) return "";            // niveau 4 : l'annonce dit déjà où aller
        var sb = new System.Text.StringBuilder();
        foreach (var g in ZoneGate.All)
            if (g.unlockLevel == lvl)
                sb.Append("\nOUVERT : ").Append(g.zoneName).Append(string.IsNullOrEmpty(g.contents) ? "" : "\n· " + g.contents + " ·");
        return sb.ToString();
    }

    void UpdateWave()
    {
        var def = levels[Level - 1];
        bool last = Level == levels.Length;

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
            timer = breakBetweenLevels;
            Announce("VAGUE " + Level + " TERMINÉE\nRéparez les barricades.", 5f);
            return;
        }

        if (StatusChanged(2000 + Level, Alive + Mathf.Max(0, def.zombies - spawned)))
            SetStatus("Niveau " + Level + "/" + levels.Length + " · zombies : " + statusValue + (last ? " · objectif : le générateur" : ""));
    }

    void SpawnOne(LevelDef def)
    {
        var p = def.points[pointIndex++ % def.points.Length];
        var z = spawner.Spawn(p);
        if (def.speed > 0f) z.walkSpeed *= def.speed;      // le spawner a mis sa vitesse (déjà réglée par la difficulté du menu)
        alive.Add(z);
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
