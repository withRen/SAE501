using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Voix d'un zombie : il pousse un cri dès son arrivée, puis de temps en temps tant qu'il est en vie.
// Profilage vocal par archétype (Rôdeur, Cogneur, Coureur) : pitch, cadence, pondération Voix_1/Voix_2 et priorité.
// Au plus `maxVoicesAtOnce` zombies crient en même temps ; un Coureur ou Cogneur proche préempte un Rôdeur éloigné.
[RequireComponent(typeof(Zombie))]
public class ZombieVoice : MonoBehaviour
{
    public AudioClip[] variants;
    [Range(0f, 1f)] public float volume = 0.8f;
    public Vector2 firstCryDelay = new Vector2(0.5f, 2.5f);      // 1er cri après l'apparition (s)
    public Vector2 pauseBetweenCries = new Vector2(5f, 12f);      // silence entre la fin d'un cri et le suivant (s)
    public float maxDistance = 25f;
    public int maxVoicesAtOnce = 4;

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
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => activeVoices.Clear();

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
