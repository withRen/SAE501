using System;
using UnityEngine;

// Logique de santé du joueur (GDD : 4 PV, +1 PV après 10 s sans dégât, max 2 PV régénérés par vague).
// À mettre sur un objet de la scène (ex : XR Origin). Les zombies appellent PlayerHealth.Instance.TakeDamage().
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Règles du GDD")]
    public int maxHealth = 4;
    public float regenDelay = 10f;      // secondes sans dégât avant +1 PV
    public int maxRegenPerWave = 2;     // PV récupérables par vague

    public int Health { get; private set; }
    public bool IsDead => Health <= 0;

    public event Action<int, int> Changed;  // (pv actuels, pv max)
    public event Action<int> Damaged;       // dégâts subis
    public event Action Died;

    float sinceLastHit;
    int regenThisWave;

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

    void Update()
    {
        if (IsDead) return;

        sinceLastHit += Time.deltaTime;
        if (Health < maxHealth && regenThisWave < maxRegenPerWave && sinceLastHit >= regenDelay)
        {
            Health++;
            regenThisWave++;
            sinceLastHit = 0f;
            Changed?.Invoke(Health, maxHealth);
        }
    }

    // Appelé par le zombie à chaque coup de griffe.
    public void TakeDamage(int amount = 1)
    {
        if (IsDead) return;

        Health = Mathf.Max(0, Health - amount);
        sinceLastHit = 0f;
        Damaged?.Invoke(amount);
        Changed?.Invoke(Health, maxHealth);

        if (Health <= 0) Died?.Invoke();
    }

    // À appeler au début de chaque vague (WaveManager) : remet le quota de régénération à zéro.
    public void NewWave()
    {
        regenThisWave = 0;
    }
}
