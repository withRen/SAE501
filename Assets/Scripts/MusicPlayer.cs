using UnityEngine;

// Musique de fond : les pistes s'enchaînent l'une après l'autre et recommencent au début, sans fin (1, 2, 1, 2...).
// Une seule source : quand une piste se termine, la suivante démarre. (L'ancienne version programmait les pistes à l'avance
// sur l'horloge audio ; après une pause du jeu cette horloge continuait et la musique se décalait ou s'arrêtait.)
// La musique ignore le volume général des effets (AudioListener) : seul le curseur Musique la règle (GameSettings).
public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] tracks;
    [Range(0f, 1f)] public float volume = 0.8f;
    public float startDelay = 0.5f;

    static float duckTarget = 1f;
    static float duckUntil = -1f;
    static float currentDuck = 1f;
    /// <summary>Coupe la musique en fondu (cinématique de fin : EscapeVideo a sa propre bande-son).</summary>
    public static bool Silenced;
    float silence = 1f;

    AudioSource src;
    int next;
    float startAt;
    float combatIntensity = 0.8f;
    float targetPitch = 1f;
    float nextStateCheck;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        duckTarget = 1f;
        duckUntil = -1f;
        currentDuck = 1f;
        Silenced = false;
    }

    /// <summary>Atténue momentanément la musique sous une alerte critique P0/P1 (tir, dégât, bris de planche).</summary>
    public static void Duck(float factor = 0.65f, float duration = 0.35f)
    {
        duckTarget = Mathf.Min(duckTarget, Mathf.Clamp(factor, 0.25f, 1f));
        duckUntil = Mathf.Max(duckUntil, Time.unscaledTime + duration);
    }

    void Start()
    {
        Silenced = false;                   // nouvelle partie : la cinématique de fin a pu la laisser coupée
        if (tracks == null || tracks.Length == 0) { enabled = false; return; }
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;              // musique d'ambiance, pas de position dans l'espace
        src.priority = 0;                   // jamais coupée : avec 4 sources par zombie, les 32 voix réelles peuvent être toutes prises
        src.ignoreListenerVolume = true;    // indépendante du curseur Effets
        src.loop = tracks.Length == 1;      // une seule piste (menu) : boucle simple
        src.volume = volume;
        startAt = Time.unscaledTime + startDelay;
    }

    void Update()
    {
        if (!src) return;
        UpdateCombatState();
        if (Time.unscaledTime >= duckUntil) duckTarget = 1f;
        currentDuck = Mathf.MoveTowards(currentDuck, duckTarget, Time.unscaledDeltaTime * (currentDuck > duckTarget ? 8f : 1.8f));
        silence = Mathf.MoveTowards(silence, Silenced ? 0f : 1f, Time.unscaledDeltaTime / 1.2f);
        src.volume = volume * combatIntensity * currentDuck * silence;
        src.pitch = Mathf.MoveTowards(src.pitch, targetPitch, Time.unscaledDeltaTime * 0.5f);
        if (AudioListener.pause || Time.unscaledTime < startAt) return;     // menu pause : la musique attend
        if (src.isPlaying) return;
        src.clip = tracks[next];
        src.Play();
        next = (next + 1) % tracks.Length;
    }

    void UpdateCombatState()
    {
        var ph = PlayerHealth.Instance;
        if (!ph || tracks == null || tracks.Length < 2)
        {
            combatIntensity = 1f;
            targetPitch = 1f;
            return;
        }
        if (Time.unscaledTime < nextStateCheck) return;
        nextStateCheck = Time.unscaledTime + 0.4f;

        bool lowHp = !ph.Dead && ph.Health <= ph.maxHealth * 0.25f;
        if (lowHp)
        {
            // État 4 sous battement de cœur : pitch tendu mais volume abaissé pour laisser percer Heartbeat
            combatIntensity = 0.52f;
            targetPitch = 1.04f;
            return;
        }
        if (Zombie.HuntPlayer)
        {
            // État 4 : Chasse finale (Générateur prêt au Niveau 4)
            combatIntensity = 1.0f;
            targetPitch = 1.04f;
            return;
        }

        // brèche : un zombie est vraiment entré dans le bunker (les joueurs posent rarement des planches,
        // une fenêtre sans planche ne suffit donc pas, sinon cet état serait actif presque tout le temps)
        bool breach = WaveManager.Instance && WaveManager.Instance.AnyZombieInBunker();

        if (breach)
        {
            // État 3 : un zombie est dans le bunker
            combatIntensity = 0.92f;
            targetPitch = 1.00f;
        }
        else if (WaveManager.Instance && WaveManager.Instance.Alive < 2)
        {
            // État 1 : Tension latente (inter-vague ou approche initiale < 2 zombies)
            combatIntensity = 0.55f;
            targetPitch = 0.96f;
        }
        else
        {
            // État 2 : Assaut périmétrique standard
            combatIntensity = 0.78f;
            targetPitch = 1.00f;
        }
    }
}
