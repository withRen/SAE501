using System;
using UnityEngine;

// Logique de santé du joueur : 100 PV (= 100 %). Les zombies retirent un pourcentage de la vie max (voir ZombieLevels).
// Régénération (GDD) : +25 PV (l'ancien « 1 PV sur 4 ») après 10 s sans dégât, 2 fois maximum par vague.
// Sur le XR Origin. Les zombies appellent PlayerHealth.Instance.TakeDamage().
// (UI : HealthVignette, GameOverUI.)
// [#115] Chaque coup reçu fait vibrer les deux manettes : on le sent même sans voir le zombie (plus fort si le coup tue).
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Règles du GDD")]
    public float maxHealth = 100f;
    public float regenDelay = 10f;      // secondes sans dégât avant la régénération
    public float regenAmount = 25f;     // PV rendus à chaque régénération
    public int maxRegenPerWave = 2;     // nombre de régénérations par vague

    [Header("Vibration au coup reçu [#115]")]
    [Range(0f, 1f)] public float hitHaptic = 0.8f;
    public float hitHapticDuration = 0.25f;

    public float Health { get; private set; }
    public bool IsDead => Health <= 0f;
    public bool Dead => IsDead;         // alias utilisé par les zombies

    public event Action<float, float> Changed;  // (pv actuels, pv max)
    public event Action<float> Damaged;         // dégâts subis (en PV)
    public event Action Died;

    float sinceLastHit;
    int regenThisWave;
    Transform head;

    void Awake()
    {
        Instance = this;
        Health = maxHealth;
    }

    void Start() => Changed?.Invoke(Health, maxHealth);

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // position de la tête du joueur (là où les zombies se dirigent)
    public Vector3 HeadPosition
    {
        get
        {
            if (head == null && Camera.main != null) head = Camera.main.transform;
            return head != null ? head.position : transform.position + Vector3.up * 1.6f;
        }
    }

    void Update()
    {
        if (IsDead) return;

        sinceLastHit += Time.deltaTime;
        if (Health < maxHealth && regenThisWave < maxRegenPerWave && sinceLastHit >= regenDelay)
        {
            Health = Mathf.Min(maxHealth, Health + regenAmount);
            regenThisWave++;
            sinceLastHit = 0f;
            Changed?.Invoke(Health, maxHealth);
        }
    }

    // Appelé par le zombie à chaque coup de griffe.
    public void TakeDamageFraction(float fractionOfMax) => TakeDamage(fractionOfMax * maxHealth);

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        Health = Mathf.Max(0f, Health - amount);
        if (Health < 0.01f) Health = 0f;      // évite qu'il reste 0,00001 PV après 3 coups d'un tiers
        sinceLastHit = 0f;
        VRInput.HapticBoth(Health <= 0f ? 1f : hitHaptic, Health <= 0f ? hitHapticDuration * 2f : hitHapticDuration);
        Damaged?.Invoke(amount);
        Changed?.Invoke(Health, maxHealth);

        if (Health <= 0f) Died?.Invoke();
    }

    // tir dans la tête (Pistol) : mort immédiate
    public void Kill()
    {
        if (IsDead) return;
        TakeDamage(Health);
    }

    // À appeler au début de chaque vague (WaveManager) : remet le quota de régénération à zéro.
    public void NewWave()
    {
        regenThisWave = 0;
    }
}
