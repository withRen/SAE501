using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Voix d'un zombie : il pousse un cri dès son arrivée, puis de temps en temps tant qu'il est en vie.
// Profilage vocal par archétype (Rôdeur, Cogneur, Coureur) : pitch, cadence, pondération Voix_1/Voix_2 et priorité.
// Au plus `maxVoicesAtOnce` zombies crient en même temps ; un Coureur ou Cogneur proche préempte un Rôdeur éloigné.
// [SONS, PR #151] Bruitages enregistrés du zombie, appelés par Zombie (aussi sur les marionnettes du multijoueur) :
//   - touché (balle, hache, pied de biche)  -> flesh_zombie_hit (remplace le bruit de chair procédural) ;
//   - il frappe le joueur                   -> zombie_bite, calé sur le moment du coup ;
//   - il meurt                              -> zombie_impact_fall_body_die (remplace le bruit sourd procédural de la chute).
// [SONS] Balle de pistolet : variantes d'impact de chair enregistrées (Resources/Sounds/Impact_Balle, Freesound CC0, voir ASSETS.md),
//   une différente à chaque balle ; flesh_zombie_hit reste pour les coups de hache et de pied de biche.
// Sons réglables ici, sinon pris dans Assets/Resources/Sounds/<nom du fichier>. Le réglage « Zombies » des paramètres les règle aussi.
[RequireComponent(typeof(Zombie))]
public class ZombieVoice : MonoBehaviour
{
    public AudioClip[] variants;
    [Range(0f, 1f)] public float volume = 0.8f;
    public Vector2 firstCryDelay = new Vector2(0.5f, 2.5f);      // 1er cri après l'apparition (s)
    public Vector2 pauseBetweenCries = new Vector2(5f, 12f);      // silence entre la fin d'un cri et le suivant (s)
    public float maxDistance = 25f;
    public int maxVoicesAtOnce = 4;

    [Header("Bruitages (sinon Assets/Resources/Sounds/…) [SONS]")]
    public AudioClip hitSound;        // flesh_zombie_hit
    public AudioClip biteSound;       // zombie_bite
    public AudioClip deathSound;      // zombie_impact_fall_body_die
    [Range(0f, 1f)] public float hitVolume = 0.9f;
    [Range(0f, 1f)] public float bulletHitVolume = 0.85f;
    [Range(0f, 1f)] public float biteVolume = 1f;
    [Range(0f, 1f)] public float deathVolume = 1f;
    public float biteLead = 0.2f;     // le pic du son de morsure arrive ~0,2 s après son début : on le lance un peu avant le coup

    AudioSource sfx;
    AudioClip hitClip, biteClip, deathClip;
    static AudioClip[] bulletHits;
    static int lastBulletHit = -1;

    static readonly List<ZombieVoice> activeVoices = new List<ZombieVoice>();
    AudioSource src;
    Zombie zombie;
    bool counted, isBruiser, isRunner;
    float baseVolume = 0.8f;

    void Awake()
    {
        zombie = GetComponent<Zombie>();
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 1f;                      // son 3D
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 2f;
        src.maxDistance = maxDistance;
        src.volume = volume;
        src.dopplerLevel = 0f;

        // bruitages : source 3D à part (la voix s'éteint à la mort, pas le bruit de la chute)
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false; sfx.loop = false;
        sfx.spatialBlend = 1f; sfx.rolloffMode = AudioRolloffMode.Linear;
        sfx.minDistance = 1.5f; sfx.maxDistance = maxDistance; sfx.dopplerLevel = 0f; sfx.priority = 40;
        hitClip = AudioSlices.Clip(hitSound, "flesh_zombie_hit");
        biteClip = AudioSlices.Clip(biteSound, "zombie_bite");
        deathClip = AudioSlices.Clip(deathSound, "zombie_impact_fall_body_die");
    }

    // volume des bruitages : même réglage « Zombies » que la voix (curseur à 6/10 = volume d'origine)
    static float Gain => Mathf.Clamp(GameSettings.Zombies / 0.6f, 0f, 1.5f);

    /// <summary>Zombie touché. Renvoie faux si le son manque (Zombie garde alors son bruit de chair procédural).</summary>
    public bool PlayHit()
    {
        if (!hitClip || !sfx) return false;
        sfx.pitch = Random.Range(0.92f, 1.08f);
        sfx.PlayOneShot(hitClip, Mathf.Clamp01(hitVolume * Gain));
        return true;
    }

    /// <summary>Zombie touché par une balle : une variante d'impact de chair, jamais deux fois la même de suite.
    /// Renvoie faux si les sons manquent (on retombe alors sur PlayHit).</summary>
    public bool PlayBulletHit()
    {
        if (bulletHits == null) bulletHits = Resources.LoadAll<AudioClip>("Sounds/Impact_Balle");
        if (bulletHits.Length == 0 || !sfx) return PlayHit();
        sfx.pitch = Random.Range(0.94f, 1.06f);
        sfx.PlayOneShot(bulletHits[AudioSlices.Pick(bulletHits.Length, ref lastBulletHit)], Mathf.Clamp01(bulletHitVolume * Gain));
        return true;
    }

    /// <summary>Le zombie lance un coup sur le joueur : la morsure part un peu avant l'impact (hitDelay = délai du coup).</summary>
    public void ScheduleBite(float hitDelay)
    {
        if (!biteClip) return;
        CancelInvoke(nameof(Bite));
        Invoke(nameof(Bite), Mathf.Max(0f, hitDelay - biteLead));
    }

    void Bite()
    {
        if (zombie == null || zombie.Dead || !biteClip) return;
        sfx.pitch = Random.Range(0.95f, 1.05f);
        sfx.PlayOneShot(biteClip, Mathf.Clamp01(biteVolume * Gain));
    }

    /// <summary>Chute du corps à la mort. Renvoie faux si le son manque (Zombie garde alors son bruit sourd procédural).</summary>
    public bool PlayFall()
    {
        if (!deathClip || !sfx) return false;
        CancelInvoke(nameof(Bite));
        sfx.pitch = Random.Range(0.95f, 1.05f);
        sfx.PlayOneShot(deathClip, Mathf.Clamp01(deathVolume * Gain));
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { activeVoices.Clear(); bulletHits = null; lastBulletHit = -1; }

    void ConfigureArchetype()
    {
        if (!zombie) return;
        isBruiser = zombie.IsBruiser;
        isRunner = zombie.IsRunner;
        if (isBruiser)
        {
            baseVolume = 1.0f;
            maxDistance = 26f;
            src.priority = 80;
            firstCryDelay = new Vector2(0.6f, 1.8f);
        }
        else if (isRunner)
        {
            baseVolume = 0.9f;
            maxDistance = 24f;
            src.priority = 64;
            firstCryDelay = new Vector2(0.25f, 1.0f);
        }
        else
        {
            baseVolume = volume;
            maxDistance = 24f;
            src.priority = 110;
        }
    }

    float DistanceToPlayer()
    {
        var ph = PlayerHealth.Instance;
        return ph ? Vector3.Distance(transform.position, ph.HeadPosition) : 15f;
    }

    bool AcquireVoiceSlot()
    {
        activeVoices.RemoveAll(v => v == null || !v.counted);
        if (activeVoices.Count < maxVoicesAtOnce) return true;
        if ((!isRunner && !isBruiser) || DistanceToPlayer() > 10f) return false;

        // Préemption : un Coureur ou Cogneur proche coupe le Rôdeur le plus éloigné
        ZombieVoice victim = null;
        float maxD = -1f;
        foreach (var v in activeVoices)
        {
            if (v.isRunner || v.isBruiser) continue;
            float d = v.DistanceToPlayer();
            if (d > maxD) { maxD = d; victim = v; }
        }
        if (!victim) return false;
        victim.Preempt();
        return true;
    }

    void Preempt()
    {
        if (src && src.isPlaying) src.Stop();
        if (counted) { activeVoices.Remove(this); counted = false; }
    }

    AudioClip PickClip()
    {
        if (variants == null || variants.Length == 0) return null;
        if (variants.Length == 1) return variants[0];
        float r = Random.value;
        if (isBruiser) return r < 0.75f ? variants[0] : variants[1];
        if (isRunner) return r < 0.20f ? variants[0] : variants[1];
        return variants[Random.Range(0, variants.Length)];
    }

    IEnumerator Start()
    {
        ConfigureArchetype();
        if (variants == null || variants.Length == 0) yield break;
        yield return new WaitForSeconds(Random.Range(firstCryDelay.x, firstCryDelay.y));
        while (zombie != null && !zombie.Dead)
        {
            if (AcquireVoiceSlot())
            {
                var clip = PickClip();
                if (clip)
                {
                    src.clip = clip;
                    src.pitch = isBruiser ? Random.Range(0.72f, 0.82f)
                              : isRunner  ? Random.Range(1.18f, 1.32f)
                              :             Random.Range(0.94f, 1.06f);
                    src.Play();
                    activeVoices.Add(this);
                    counted = true;
                    float end = Time.time + clip.length / Mathf.Max(0.5f, src.pitch);
                    while (Time.time < end && zombie != null && !zombie.Dead && counted) yield return null;
                    if (counted) { activeVoices.Remove(this); counted = false; }
                }
            }
            bool close = DistanceToPlayer() <= 6f;
            Vector2 pause = isBruiser ? (close ? new Vector2(3.0f, 5.0f) : new Vector2(4.5f, 7.5f))
                          : isRunner  ? (close ? new Vector2(1.2f, 2.2f) : new Vector2(1.8f, 3.2f))
                          :             (close ? new Vector2(2.2f, 4.2f) : new Vector2(3.5f, 6.5f));
            yield return new WaitForSeconds(Random.Range(pause.x, pause.y));
        }
    }

    void Update()
    {
        // réglage « Zombies » des paramètres : volume et portée de la voix (on les entend de plus loin quand on monte le curseur)
        if (zombie == null || !zombie.Dead)
        {
            float z = GameSettings.Zombies;
            src.volume = Mathf.Clamp01(baseVolume * z * 1.6f);   // curseur à 6/10 : volume d'origine
            src.minDistance = Mathf.Lerp(1f, isBruiser ? 6.5f : 5.5f, z);
            src.maxDistance = Mathf.Max(maxDistance, src.minDistance + 5f);
            return;
        }
        // mort : coupure immédiate pour laisser la place au râle d'agonie (ZombieDeath)
        if (src.isPlaying) src.Stop();
        if (counted) { activeVoices.Remove(this); counted = false; }
    }

    void OnDestroy()
    {
        if (counted) { activeVoices.Remove(this); counted = false; }
    }
}
