using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

// [Multijoueur] La partie en réseau (prefab Resources/Net/NetGame, créé par l'hôte quand tous les casques ont chargé MainScene).
// L'hôte fait tourner le vrai jeu : vagues (WaveManager), zombies (IA, NavMesh), dégâts. Ce script relaie aux autres casques :
//   - les zombies : création (type, position), positions 15 fois par seconde, coups, morts → des « marionnettes » chez les clients ;
//   - les tirs et coups de mêlée des clients sur ces marionnettes → appliqués par l'hôte, qui crédite le tireur ;
//   - les barricades : planche posée, clouée, retirée par n'importe qui, arrachée par un zombie (repérées par leur numéro) ;
//   - l'écran du bunker (annonces, statut), début et fin de vague (soin, portes de zone), poussière avant une chute par le toit ;
//   - les scores (par joueur et d'équipe), les joueurs à terre relevés à la pause, la fin de partie quand tous sont tombés.
// Ce qui reste propre à chaque casque : les objets en main (pistolet, chargeurs, marteau, planches libres), la radio, les portes.
public class NetGame : NetworkBehaviour
{
    public static NetGame Instance { get; private set; }

    public const int KillPoints = 100, HeadPoints = 50, MeleePoints = 25, PlankPoints = 20, WaveBonus = 500;
    const float StateRate = 1f / 15f;

    public struct Score : INetworkSerializable, IEquatable<Score>
    {
        public ulong clientId;
        public int number, score, kills, heads, melee, planks;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref clientId); s.SerializeValue(ref number); s.SerializeValue(ref score);
            s.SerializeValue(ref kills); s.SerializeValue(ref heads); s.SerializeValue(ref melee); s.SerializeValue(ref planks);
        }
        public bool Equals(Score o) => clientId == o.clientId && number == o.number && score == o.score && kills == o.kills
                                    && heads == o.heads && melee == o.melee && planks == o.planks;
    }

    public struct ZState : INetworkSerializeByMemcpy
    {
        public ushort id;
        public Vector3 pos;
        public float yaw, speed;
    }

    public NetworkList<Score> Scores;
    public readonly NetworkVariable<int> Wave = new NetworkVariable<int>(0);
    /// <summary>Évasion, niveau 4 : le générateur peut démarrer (décidé par l'hôte).</summary>
    public readonly NetworkVariable<bool> GeneratorReady = new NetworkVariable<bool>(false);

    /// <summary>Somme des scores des joueurs + bonus par vague terminée.</summary>
    public int TeamScore
    {
        get { int t = 0; foreach (var s in Scores) t += s.score; return t + WaveBonus * Mathf.Max(0, Wave.Value - 1); }
    }

    public List<Score> SortedScores() { var l = new List<Score>(); foreach (var s in Scores) l.Add(s); l.Sort((a, b) => b.score.CompareTo(a.score)); return l; }

    readonly Dictionary<ushort, Zombie> zombies = new Dictionary<ushort, Zombie>();
    ushort nextId = 1;
    List<Plank> planks;
    List<BarricadeSlot> slots;
    List<Opening> openings;
    ZombieSpawner spawner;
    float nextState;
    bool gameOver;
    TextMesh scoreText;
    string lastScoreLine;
    readonly List<ZState> buffer = new List<ZState>();

    void Awake() => Scores = new NetworkList<Score>();

    public override void OnNetworkSpawn()
    {
        Instance = this;
        spawner = FindFirstObjectByType<ZombieSpawner>();
        // numéros stables : même ordre sur tous les casques (chemin dans la hiérarchie, la scène est générée par code)
        planks = StableOrder(Plank.All);
        slots = StableOrder(BarricadeSlot.All);
        openings = StableOrder(Opening.All);

        Plank.LocalPlaced += OnLocalPlaced;
        Plank.LocalNailed += OnLocalNailed;
        Plank.LocalFreed += OnLocalFreed;
        Zombie.PuppetHit += OnPuppetHit;
        WaveManager.GeneratorRequested += OnGeneratorRequested;
        WaveManager.EscapeRequested += OnEscapeRequested;
        GeneratorReady.OnValueChanged += (_, v) => WaveManager.RemoteGeneratorReady = v;
        WaveManager.RemoteGeneratorReady = GeneratorReady.Value;
        BuildScoreText();
        Scores.OnListChanged += _ => scoresDirty = true;
        Wave.OnValueChanged += (_, __) => scoresDirty = true;

        if (!IsServer) return;
        Plank.Broken += OnBroken;
        ZombieSpawner.Spawned += OnSpawned;
        Zombie.Killed += OnKilled;
        Zombie.AttackAnimPlayed += OnAttackAnim;
        Opening.Warned += OnWarned;
        WaveManager.Announced += OnAnnounced;
        WaveManager.StatusSet += OnStatus;
        WaveManager.WaveStarted += OnWaveStarted;
        WaveManager.BreakStarted += OnBreakStarted;
        WaveManager.VictoryReached += OnVictory;
        WaveManager.EscapeOpened += OnEscapeOpened;

        foreach (var id in NetworkManager.ConnectedClientsIds)
        {
            int n = NetLobby.Numbers.TryGetValue(id, out var k) ? k : Scores.Count + 1;
            Scores.Add(new Score { clientId = id, number = n });
            var go = Instantiate(Resources.Load<GameObject>("Net/NetPlayer"));
            var np = go.GetComponent<NetPlayer>();
            np.Number.Value = n;
            go.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
        Plank.LocalPlaced -= OnLocalPlaced;
        Plank.LocalNailed -= OnLocalNailed;
        Plank.LocalFreed -= OnLocalFreed;
        Zombie.PuppetHit -= OnPuppetHit;
        Plank.Broken -= OnBroken;
        ZombieSpawner.Spawned -= OnSpawned;
        Zombie.Killed -= OnKilled;
        Zombie.AttackAnimPlayed -= OnAttackAnim;
        Opening.Warned -= OnWarned;
        WaveManager.Announced -= OnAnnounced;
        WaveManager.StatusSet -= OnStatus;
        WaveManager.WaveStarted -= OnWaveStarted;
        WaveManager.BreakStarted -= OnBreakStarted;
        WaveManager.VictoryReached -= OnVictory;
        WaveManager.GeneratorRequested -= OnGeneratorRequested;
        WaveManager.EscapeRequested -= OnEscapeRequested;
        WaveManager.EscapeOpened -= OnEscapeOpened;
    }

    static List<T> StableOrder<T>(IEnumerable<T> items) where T : Component =>
        items.Where(x => x).OrderBy(x => Path(x.transform), StringComparer.Ordinal).ToList();

    static string Path(Transform t)
    {
        var sb = new System.Text.StringBuilder();
        for (; t; t = t.parent) sb.Insert(0, "/" + t.GetSiblingIndex().ToString("000") + t.name);
        return sb.ToString();
    }

    void Update()
    {
        if (!IsSpawned) return;
        UpdateScoreText();
        if (!IsServer) return;

        var wm = WaveManager.Instance;
        bool gen = wm && wm.GeneratorReady;
        if (GeneratorReady.Value != gen) GeneratorReady.Value = gen;

        if (Time.time >= nextState)
        {
            nextState = Time.time + StateRate;
            SendStates();
        }

        // tous les joueurs à terre en même temps : fin de partie pour tout le monde
        if (!gameOver && PlayerHealth.All.Count > 0 && PlayerHealth.NearestAlive(Vector3.zero) == null)
        {
            gameOver = true;
            Debug.Log("[Net] tous les joueurs sont à terre : fin de partie (vague " + WaveManager.Level + ", équipe " + TeamScore + ")");
            GameOverRpc();
        }
    }

    // ---------- zombies ----------
    void OnSpawned(Zombie z, int prefabIndex)
    {
        z.NetId = nextId++;
        if (nextId == 0) nextId = 1;
        zombies[z.NetId] = z;
        SpawnZombieRpc(z.NetId, (byte)prefabIndex, z.transform.position, z.transform.eulerAngles.y);
    }

    [Rpc(SendTo.NotServer)]
    void SpawnZombieRpc(ushort id, byte prefabIndex, Vector3 pos, float yaw)
    {
        if (!spawner) spawner = FindFirstObjectByType<ZombieSpawner>();
        if (!spawner || zombies.ContainsKey(id)) return;
        zombies[id] = spawner.SpawnPuppet(prefabIndex, id, pos, yaw);
    }

    void SendStates()
    {
        buffer.Clear();
        foreach (var kv in zombies)
        {
            var z = kv.Value;
            if (!z || z.Dead) continue;
            buffer.Add(new ZState { id = kv.Key, pos = z.transform.position, yaw = z.transform.eulerAngles.y, speed = z.NetMoveSpeed });
        }
        if (buffer.Count > 0) StatesRpc(buffer.ToArray());
    }

    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    void StatesRpc(ZState[] states)
    {
        foreach (var s in states)
            if (zombies.TryGetValue(s.id, out var z) && z) z.PuppetState(s.pos, s.yaw, s.speed);
    }

    void OnAttackAnim(Zombie z) { if (z.NetId != 0) AttackRpc(z.NetId); }

    [Rpc(SendTo.NotServer)]
    void AttackRpc(ushort id) { if (zombies.TryGetValue(id, out var z) && z) z.PuppetAttack(); }

    // un client a touché une marionnette : l'hôte applique le coup au vrai zombie, au nom de ce joueur
    void OnPuppetHit(Zombie z, HitZone zone, float meleeFraction) => HitRpc(z.NetId, (byte)zone, meleeFraction);

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void HitRpc(ushort id, byte zone, float meleeFraction, RpcParams p = default)
    {
        if (!zombies.TryGetValue(id, out var z) || !z || z.Dead) return;
        z.LastHitBy = p.Receive.SenderClientId;
        if (meleeFraction > 0f) z.OnMeleeHit((HitZone)zone, meleeFraction);
        else z.OnHit((HitZone)zone);
        z.LastHitBy = NetworkManager.ServerClientId;
    }

    void OnKilled(Zombie z, bool byPlayer)
    {
        if (z.NetId == 0) return;
        zombies.Remove(z.NetId);
        if (byPlayer) AddScore(z.LastHitBy, s =>
        {
            s.kills++; s.score += KillPoints;
            if (z.LastHitHead) { s.heads++; s.score += HeadPoints; }
            if (z.LastHitMelee) { s.melee++; s.score += MeleePoints; }
            return s;
        });
        DieRpc(z.NetId, byPlayer);
    }

    [Rpc(SendTo.NotServer)]
    void DieRpc(ushort id, bool byPlayer)
    {
        if (zombies.TryGetValue(id, out var z) && z) z.Die(byPlayer);
        zombies.Remove(id);
    }

    void AddScore(ulong clientId, Func<Score, Score> change)
    {
        for (int i = 0; i < Scores.Count; i++)
            if (Scores[i].clientId == clientId) { Scores[i] = change(Scores[i]); return; }
    }

    // ---------- barricades ----------
    const byte Place = 0, Free = 1, Break = 2;

    void OnLocalPlaced(Plank p, BarricadeSlot s, bool nailed, bool fresh) => SendPlank(Place, p, s, nailed, fresh, Vector3.zero);
    void OnLocalNailed(Plank p) => SendPlank(Place, p, SlotOf(p), true, false, Vector3.zero);
    void OnLocalFreed(Plank p) => SendPlank(Free, p, null, false, false, Vector3.zero);
    void OnBroken(Plank p, Vector3 dir) => SendPlank(Break, p, null, false, false, dir);

    BarricadeSlot SlotOf(Plank p) { foreach (var s in slots) if (s.Plank == p) return s; return null; }

    void SendPlank(byte kind, Plank p, BarricadeSlot s, bool nailed, bool fresh, Vector3 dir)
    {
        int pi = planks.IndexOf(p), si = s ? slots.IndexOf(s) : -1;
        if (pi < 0 || (kind == Place && si < 0)) return;
        if (IsServer)
        {
            if (kind == Place && fresh) AddScore(NetworkManager.LocalClientId, x => { x.planks++; x.score += PlankPoints; return x; });
            PlankRpc(kind, (short)pi, (short)si, nailed, dir, NetworkManager.LocalClientId);
        }
        else PlankToServerRpc(kind, (short)pi, (short)si, nailed, fresh);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void PlankToServerRpc(byte kind, short pi, short si, bool nailed, bool fresh, RpcParams p = default)
    {
        ulong from = p.Receive.SenderClientId;
        ApplyPlank(kind, pi, si, nailed, Vector3.zero);
        if (kind == Place && fresh) AddScore(from, x => { x.planks++; x.score += PlankPoints; return x; });
        PlankRpc(kind, pi, si, nailed, Vector3.zero, from);
    }

    [Rpc(SendTo.NotServer)]
    void PlankRpc(byte kind, short pi, short si, bool nailed, Vector3 dir, ulong origin)
    {
        if (origin == NetworkManager.LocalClientId) return;     // c'est ce casque qui l'a fait
        ApplyPlank(kind, pi, si, nailed, dir);
    }

    void ApplyPlank(byte kind, short pi, short si, bool nailed, Vector3 dir)
    {
        if (pi < 0 || pi >= planks.Count || !planks[pi]) return;
        var p = planks[pi];
        if (kind == Place && si >= 0 && si < slots.Count) p.NetPlace(slots[si], nailed);
        else if (kind == Free) p.NetFree();
        else if (kind == Break) p.NetBreak(dir.sqrMagnitude > 0f ? dir : Vector3.down);
    }

    // ---------- écran du bunker, vagues ----------
    void OnAnnounced(string text, float seconds) => AnnounceRpc(text, seconds);
    [Rpc(SendTo.NotServer)] void AnnounceRpc(string text, float seconds) { if (WaveManager.Instance) WaveManager.Instance.RemoteAnnounce(text, seconds); }

    void OnStatus(string text) => StatusRpc(text);
    [Rpc(SendTo.NotServer)] void StatusRpc(string text) { if (WaveManager.Instance) WaveManager.Instance.RemoteStatus(text); }

    void OnWaveStarted(int lvl) { Wave.Value = lvl; WaveRpc(lvl); }
    [Rpc(SendTo.NotServer)] void WaveRpc(int lvl) { if (WaveManager.Instance) WaveManager.Instance.RemoteWaveStart(lvl); }

    // vague terminée : soin pour tous, et les joueurs à terre réapparaissent (vie pleine, ReviveLocal)
    void OnBreakStarted(int lvl, float heal)
    {
        ReviveLocal(heal);
        BreakRpc(lvl, heal);
    }

    [Rpc(SendTo.NotServer)]
    void BreakRpc(int lvl, float heal)
    {
        if (WaveManager.Instance) WaveManager.Instance.RemoteBreak(lvl, heal);
        ReviveLocal(heal);
    }

    // [hotfix #155] le joueur mort (spectateur) réapparaît vie pleine à son point de départ (sûr : la pause commence, plus de zombie)
    static void ReviveLocal(float heal)
    {
        var h = PlayerHealth.Instance;
        if (!h || !h.IsDead) return;
        if (NetPlayer.Local) NetPlayer.Local.RespawnAtStart();
        h.Revive(h.maxHealth);
    }

    void OnWarned(Opening o, float seconds) { int i = openings.IndexOf(o); if (i >= 0) WarnRpc((short)i, seconds); }

    [Rpc(SendTo.NotServer)]
    void WarnRpc(short i, float seconds)
    {
        if (i < 0 || i >= openings.Count || !openings[i]) return;
        Opening.ApplyingRemote = true;
        try { openings[i].WarnFromAbove(seconds); } finally { Opening.ApplyingRemote = false; }
    }

    // ---------- Évasion : le générateur ----------
    void OnGeneratorRequested() => GeneratorRpc();

    // un client a tenu le levier assez longtemps : l'hôte active SON générateur (victoire pour tout le monde)
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void GeneratorRpc()
    {
        var wm = WaveManager.Instance;
        if (!wm || !wm.GeneratorReady) return;
        var g = FindFirstObjectByType<Generator>();
        if (g) g.Activate(); else wm.GeneratorActivated();
    }

    // [Évasion] générateur activé chez l'hôte : le portail du jardin s'ouvre sur tous les casques
    void OnEscapeOpened() { if (IsServer) EscapeOpenRpc(); }
    [Rpc(SendTo.NotServer)] void EscapeOpenRpc() { if (WaveManager.Instance) WaveManager.Instance.RemoteEscapeOpen(); }

    // un client a franchi le portail : l'hôte déclare la victoire (pour tout le monde)
    void OnEscapeRequested() => EscapeRpc();
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void EscapeRpc() { if (WaveManager.Instance) WaveManager.Instance.PlayerEscaped(); }

    void OnVictory() => VictoryRpc();
    [Rpc(SendTo.NotServer)] void VictoryRpc() { if (WaveManager.Instance) WaveManager.Instance.RemoteVictory(); }

    [Rpc(SendTo.Everyone)]
    void GameOverRpc()
    {
        NetSpectator.End();
        if (GameOverUI.Instance) GameOverUI.Instance.ShowTeamDefeat();
    }

    // ---------- score sur l'écran du bunker (sous le statut) ----------
    void BuildScoreText()
    {
        var wm = WaveManager.Instance;
        if (!wm || !wm.screen) return;
        var go = new GameObject("Scores_Equipe");
        go.transform.SetParent(wm.screen, false);
        go.transform.localPosition = new Vector3(0f, -0.38f, -0.03f);
        scoreText = go.AddComponent<TextMesh>();
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        go.GetComponent<MeshRenderer>().sharedMaterial = scoreText.font.material;
        scoreText.fontSize = 64; scoreText.characterSize = 0.0075f;
        scoreText.anchor = TextAnchor.MiddleCenter; scoreText.alignment = TextAlignment.Center;
        scoreText.color = UiKit.Amber;
    }

    bool scoresDirty = true;

    void UpdateScoreText()
    {
        if (!scoreText || !scoresDirty) return;
        scoresDirty = false;
        var sb = new System.Text.StringBuilder("ÉQUIPE ").Append(TeamScore.ToString("N0"));
        foreach (var s in SortedScores()) sb.Append("  ·  J").Append(s.number).Append(' ').Append(s.score.ToString("N0"));
        var line = sb.ToString();
        if (line != lastScoreLine) { lastScoreLine = line; scoreText.text = line; }
    }
}
