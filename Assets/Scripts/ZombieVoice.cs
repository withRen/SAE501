using System.Collections;
using UnityEngine;

// Voix d'un zombie : il pousse un cri dès son arrivée, puis de temps en temps tant qu'il est en vie.
// Chaque cri choisit au hasard parmi les variantes (Zombie_Voix_1 : long cri, Zombie_Voix_2 : cri court).
// Le son est en 3D : on l'entend venir du couloir ou de la fenêtre. À sa mort le zombie se tait (fondu rapide).
// Au plus `maxVoicesAtOnce` zombies crient en même temps (sinon tout se mélange et le casque a trop de sons à traiter).
[RequireComponent(typeof(Zombie))]
public class ZombieVoice : MonoBehaviour
{
    public AudioClip[] variants;
    [Range(0f, 1f)] public float volume = 0.8f;
    public Vector2 firstCryDelay = new Vector2(0.5f, 2.5f);      // 1er cri après l'apparition (s)
    public Vector2 pauseBetweenCries = new Vector2(5f, 12f);      // silence entre la fin d'un cri et le suivant (s)
    public float maxDistance = 25f;
    public int maxVoicesAtOnce = 3;

    static int voicesNow;
    AudioSource src;
    Zombie zombie;
    bool counted;

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
        // mort : fondu rapide puis silence
        if (zombie != null && zombie.Dead && src.isPlaying)
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
