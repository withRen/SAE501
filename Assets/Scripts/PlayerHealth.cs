using System;
using System.Collections.Generic;
using UnityEngine;

// Logique de santé du joueur : 100 PV (= 100 %). Les zombies retirent un pourcentage de la vie max (voir ZombieLevels).
// Régénération (GDD) : +25 PV (l'ancien « 1 PV sur 4 ») après 10 s sans dégât, 2 fois maximum par vague.
// Rééquilibrage : +50 PV au début de chaque pause entre deux vagues (Heal, appelé par WaveManager) ; dégâts comptés pour le bilan.
// Sur le XR Origin. Les zombies appellent PlayerHealth.Instance.TakeDamage().
// (UI : HealthVignette, GameOverUI.)
// [#115] Chaque coup reçu fait vibrer les deux manettes : on le sent même sans voir le zombie (plus fort si le coup tue).
// Coup venant d'un zombie (TakeDamage avec sa position) : la manette du côté du zombie vibre plus fort que l'autre,
// et DamagedFrom donne la position de l'attaquant (indicateur de direction de HealthVignette).
// [Multijoueur] Sur l'hôte, chaque joueur distant a un « double » (IsProxy, créé par NetPlayer sur son avatar) : les zombies
// le visent comme le joueur local (All, NearestAlive). Un coup reçu par un double n'enlève rien ici : il part au casque
// de ce joueur (RemoteDamaged), qui l'applique à sa vraie santé ; la vie du double suit celle du joueur (SetProxyHealth).
public class PlayerHealth : MonoBehaviour
{
    /// <summary>Le joueur de CE casque (jamais un double).</summary>
    public static PlayerHealth Instance { get; private set; }
    /// <summary>Tous les joueurs visables par les zombies : le joueur local et, sur l'hôte, les doubles des joueurs distants.</summary>
    public static readonly List<PlayerHealth> All = new List<PlayerHealth>();

    public bool IsProxy { get; private set; }
    public event Action<float, Vector3> RemoteDamaged;  // double : coup à transmettre au casque du joueur
    static bool nextIsProxy;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { All.Clear(); nextIsProxy = false; }

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
    public event Action<float, Vector3> DamagedFrom;    // dégâts subis + position de l'attaquant (seulement si elle est connue)
    public event Action Died;

    float sinceLastHit;
    int regenThisWave;
    Transform head;

    void Awake()
    {
        IsProxy = nextIsProxy;
        if (!IsProxy) Instance = this;
        All.Add(this);
        Health = maxHealth;
    }

    /// <summary>Double d'un joueur distant (hôte uniquement), posé sur la tête de son avatar.</summary>
    public static PlayerHealth CreateProxy(GameObject on, Transform head)
    {
        nextIsProxy = true;
        var h = on.AddComponent<PlayerHealth>();
        nextIsProxy = false;
        h.head = head;
        return h;
    }

    public void SetProxyHealth(float hp)
    {
        if (!IsProxy || Mathf.Approximately(hp, Health)) return;
        bool wasDead = IsDead;
        Health = hp;
        Changed?.Invoke(Health, maxHealth);
        if (!wasDead && IsDead) Died?.Invoke();
    }

    /// <summary>[Hotfix #161] Coop Évasion : ce joueur a franchi le portail ; les zombies ne le visent plus (il attend son équipe).</summary>
    public bool Escaped { get; set; }

    /// <summary>Joueur vivant (et pas encore sorti par le portail) le plus proche d'un point (à plat) ; null s'il n'y en a pas.</summary>
    public static PlayerHealth NearestAlive(Vector3 from)
    {
        PlayerHealth best = null; float bd = float.MaxValue;
        foreach (var p in All)
        {
            if (p == null || p.IsDead || p.Escaped) continue;
            var d = p.HeadPosition - from; d.y = 0f;
            if (d.sqrMagnitude < bd) { bd = d.sqrMagnitude; best = p; }
        }
        return best;
    }

    /// <summary>Au moins un joueur vivant (sorti par le portail ou non).</summary>
    public static bool AnyAlive() { foreach (var p in All) if (p != null && !p.IsDead) return true; return false; }

    void Start() => Changed?.Invoke(Health, maxHealth);

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        All.Remove(this);
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
        if (IsDead || IsProxy) return;

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
    public void TakeDamageFraction(float fractionOfMax, Vector3 from) => TakeDamage(fractionOfMax * maxHealth, from);

    public void TakeDamage(float amount) => TakeDamage(amount, null);

    public void TakeDamage(float amount, Vector3? from)
    {
        if (IsDead) return;
        if (IsProxy) { RemoteDamaged?.Invoke(amount, from ?? HeadPosition); return; }

        GameStats.Damage(Mathf.Min(amount, Health));
        Health = Mathf.Max(0f, Health - amount);
        if (Health < 0.01f) Health = 0f;      // évite qu'il reste 0,00001 PV après 3 coups d'un tiers
        sinceLastHit = 0f;
        HitHaptics(from);
        Damaged?.Invoke(amount);
        if (from.HasValue) DamagedFrom?.Invoke(amount, from.Value);
        Changed?.Invoke(Health, maxHealth);

        if (Health <= 0f) Died?.Invoke();
    }

    // Vibration au coup : les deux manettes, la manette du côté de l'attaquant au maximum, l'autre à moitié environ.
    void HitHaptics(Vector3? from)
    {
        float amp = Health <= 0f ? 1f : hitHaptic, dur = Health <= 0f ? hitHapticDuration * 2f : hitHapticDuration;
        float side = 0f;                                                   // -1 = à gauche, +1 = à droite
        if (from.HasValue && Camera.main)
        {
            var cam = Camera.main.transform;
            var d = Vector3.ProjectOnPlane(from.Value - cam.position, Vector3.up);
            if (d.sqrMagnitude > 1e-4f) side = Vector3.Dot(d.normalized, Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized);
        }
        float near = Mathf.Min(1f, amp + 0.2f * Mathf.Abs(side)), far = amp * Mathf.Lerp(1f, 0.45f, Mathf.Abs(side));
        VRInput.Haptic(UnityEngine.XR.XRNode.LeftHand, side < 0f ? near : far, dur);
        VRInput.Haptic(UnityEngine.XR.XRNode.RightHand, side < 0f ? far : near, dur);
    }

    // tir dans la tête (Pistol) : mort immédiate
    public void Kill()
    {
        if (IsDead) return;
        TakeDamage(Health);
    }

    // Soin (WaveManager : début de la pause entre deux vagues). Rien si le joueur est mort.
    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f || Health >= maxHealth) return;
        Health = Mathf.Min(maxHealth, Health + amount);
        Changed?.Invoke(Health, maxHealth);
    }

    // [Multijoueur] joueur tombé pendant la vague : relevé à la pause suivante avec `amount` PV (NetGame)
    public void Revive(float amount)
    {
        if (!IsDead || IsProxy) return;
        Health = Mathf.Clamp(amount, 1f, maxHealth);
        sinceLastHit = 0f;
        Changed?.Invoke(Health, maxHealth);
        Revived?.Invoke();
    }

    public event Action Revived;

    // À appeler au début de chaque vague (WaveManager) : remet le quota de régénération à zéro.
    public void NewWave()
    {
        regenThisWave = 0;
    }
}
