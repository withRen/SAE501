using System.Collections;
using UnityEngine;

// Voix d'un zombie : il pousse un cri dès son arrivée, puis de temps en temps tant qu'il est en vie.
// Chaque cri choisit au hasard parmi les variantes (Zombie_Voix_1 : long cri, Zombie_Voix_2 : cri court).
// Le son est en 3D : on l'entend venir du couloir ou de la fenêtre. À sa mort le zombie se tait (fondu rapide).
// Au plus `maxVoicesAtOnce` zombies crient en même temps (sinon tout se mélange et le casque a trop de sons à traiter).
// [SONS] Les bruitages du zombie passent aussi par ici, sans toucher à Zombie.cs : le script observe le zombie.
//   - touché (ses PV baissent : balle, hache, pied de biche)  -> flesh_zombie_hit (remplace le bruit de chair procédural) ;
//   - il attaque le joueur (état Attacking, même cadence que Zombie)  -> zombie_bite, calé sur le moment du coup ;
//   - il meurt  -> zombie_impact_fall_body_die.
// Sons réglables ici, sinon pris dans Assets/Resources/Sounds/<nom du fichier>. Le réglage « Zombies » des paramètres les règle aussi.
[RequireComponent(typeof(Zombie))]
public class ZombieVoice : MonoBehaviour
{
    public AudioClip[] variants;
    [Range(0f, 1f)] public float volume = 0.8f;
    public Vector2 firstCryDelay = new Vector2(0.5f, 2.5f);      // 1er cri après l'apparition (s)
    public Vector2 pauseBetweenCries = new Vector2(5f, 12f);      // silence entre la fin d'un cri et le suivant (s)
    public float maxDistance = 25f;
    public int maxVoicesAtOnce = 3;

    [Header("Bruitages (sinon Assets/Resources/Sounds/…)")]
    public AudioClip hitSound;        // flesh_zombie_hit
    public AudioClip biteSound;       // zombie_bite
    public AudioClip deathSound;      // zombie_impact_fall_body_die
    [Range(0f, 1f)] public float hitVolume = 0.9f;
    [Range(0f, 1f)] public float biteVolume = 1f;
    [Range(0f, 1f)] public float deathVolume = 1f;
    public float biteLead = 0.2f;     // le pic du son de morsure arrive ~0,2 s après son début : on le lance un peu avant le coup

    static int voicesNow;
    AudioSource src, sfx;
    Zombie zombie;
    bool counted;
    float lastHealth = -1f, nextBite;
    bool wasAttacking, deathPlayed;
    AudioClip hitClip, biteClip, deathClip;

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
        sfx.minDistance = 1.5f; sfx.maxDistance = maxDistance; sfx.dopplerLevel = 0f;
        hitClip = AudioSlices.Clip(hitSound, "flesh_zombie_hit");
        biteClip = AudioSlices.Clip(biteSound, "zombie_bite");
        deathClip = AudioSlices.Clip(deathSound, "zombie_impact_fall_body_die");
        if (hitClip) zombie.fleshVolume = 0f;        // le vrai son remplace le bruit de chair procédural de Zombie.HitFeedback
    }

    // volume des bruitages : même réglage « Zombies » que la voix (curseur à 6/10 = volume d'origine)
    float Gain => Mathf.Clamp(GameSettings.Zombies / 0.6f, 0f, 1.5f);

    void TrackEvents()
    {
        if (zombie == null) return;

        // touché : les PV baissent (balle, hache, pied de biche)
        float hp = zombie.Health;
        if (lastHealth < 0f) lastHealth = hp;
        if (hp < lastHealth - 0.01f && hitClip)
        {
            sfx.pitch = Random.Range(0.92f, 1.08f);
            sfx.PlayOneShot(hitClip, hitVolume * Gain);
        }
        lastHealth = hp;

        // mort : bruit de la chute (une seule fois)
        if (zombie.Dead)
        {
            if (!deathPlayed && deathClip)
            {
                deathPlayed = true;
                sfx.pitch = Random.Range(0.95f, 1.05f);
                sfx.PlayOneShot(deathClip, deathVolume * Gain);
            }
            return;
        }

        // attaque du joueur : même cadence que Zombie.UpdateAttacking (1er coup dès l'entrée dans l'état, puis toutes les attackCooldown s)
        bool attacking = zombie.state == Zombie.State.Attacking;
        if (attacking)
        {
            if (!wasAttacking) nextBite = Time.time;
            var pl = PlayerHealth.Instance;
            if (biteClip && Time.time >= nextBite && (pl == null || !pl.Dead))
            {
                nextBite = Time.time + zombie.attackCooldown;
                Invoke(nameof(Bite), Mathf.Max(0f, zombie.attackHitDelay - biteLead));
            }
        }
        wasAttacking = attacking;
    }

    void Bite()
    {
        if (zombie == null || zombie.Dead || biteClip == null) return;
        sfx.pitch = Random.Range(0.95f, 1.05f);
        sfx.PlayOneShot(biteClip, biteVolume * Gain);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => voicesNow = 0;

    IEnumerator Start()
    {
        if (variants == null || variants.Length == 0) yield break;
        yield return new WaitForSeconds(Random.Range(firstCryDelay.x, firstCryDelay.y));
        while (zombie != null && !zombie.Dead)
        {
            if (voicesNow < maxVoicesAtOnce)
            {
                var clip = variants[Random.Range(0, variants.Length)];
                if (clip)
                {
                    src.clip = clip;
                    src.pitch = Random.Range(0.92f, 1.08f);     // chaque zombie a une voix un peu différente
                    src.Play();
                    voicesNow++; counted = true;
                    float end = Time.time + clip.length / src.pitch;
                    while (Time.time < end && zombie != null && !zombie.Dead) yield return null;
                    if (counted) { voicesNow--; counted = false; }
                }
            }
            yield return new WaitForSeconds(Random.Range(pauseBetweenCries.x, pauseBetweenCries.y));
        }
    }

    void Update()
    {
        TrackEvents();
        // réglage « Zombies » des paramètres : volume et portée de la voix (on les entend de plus loin quand on monte le curseur)
        if (zombie == null || !zombie.Dead)
        {
            float z = GameSettings.Zombies;
            src.volume = Mathf.Clamp01(volume * z * 1.6f);       // curseur à 6/10 : volume d'origine (0,77)
            src.minDistance = Mathf.Lerp(1f, 6f, z);
            src.maxDistance = Mathf.Max(maxDistance, src.minDistance + 5f);
            return;
        }
        // mort : fondu rapide puis silence
        if (src.isPlaying)
        {
            src.volume = Mathf.MoveTowards(src.volume, 0f, Time.deltaTime * 4f);
            if (src.volume <= 0.01f) src.Stop();
        }
    }

    void OnDestroy()
    {
        if (counted) { voicesNow--; counted = false; }
    }
}
