using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lobby d'attente (scène Menu) : la liste des joueurs connectés, leur état PRÊT, et le lancement.
// Objet réseau créé par l'hôte (prefab Resources/Net/NetLobby) dès qu'il héberge ; les clients le reçoivent en se connectant.
// Quand TOUS les joueurs (au moins MinPlayers) sont prêts : compte à rebours de CountdownTime s, puis l'hôte charge
// la scène de jeu pour tout le monde (NetworkSceneManager), dans le mode choisi par l'hôte (Vagues infinies ou Évasion).
// Un joueur qui repasse « pas prêt » ou qui part annule le compte à rebours.
public class NetLobby : NetworkBehaviour
{
    public static NetLobby Instance { get; private set; }

    public const float CountdownTime = 5f;
    [Tooltip("Joueurs minimum pour lancer (1 : test seul dans l'éditeur).")]
    public int minPlayers = 2;
    public string gameScene = "MainScene";

    public struct Entry : INetworkSerializable, IEquatable<Entry>
    {
        public ulong clientId;
        public int number;          // Joueur 1, 2, 3, 4 (couleur de l'avatar, ligne du score)
        public bool ready;
        public int skin;            // tenue choisie (PlayerSkin), affichée dans le lobby
        public FixedString32Bytes device;
        public FixedString64Bytes name;     // [hotfix #155] nom du compte Meta / du casque (PlayerName), vide = « Joueur N »

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref clientId); s.SerializeValue(ref number); s.SerializeValue(ref ready); s.SerializeValue(ref skin); s.SerializeValue(ref device);
            s.SerializeValue(ref name);
        }
        public bool Equals(Entry o) => clientId == o.clientId && number == o.number && ready == o.ready && skin == o.skin && device.Equals(o.device) && name.Equals(o.name);
    }

    public NetworkList<Entry> Players;
    public readonly NetworkVariable<float> Countdown = new NetworkVariable<float>(-1f);   // < 0 : pas de compte à rebours
    /// <summary>Mode choisi par l'hôte : vrai = Vagues infinies, faux = Évasion (4 niveaux, puis le générateur).</summary>
    public readonly NetworkVariable<bool> EndlessMode = new NetworkVariable<bool>(true);
    /// <summary>Index du mode de jeu et de la carte : 0 = Coop Vagues infinies (Bunker), 1 = Coop Évasion (Bunker), 2 = Duel 1v1 (Alvéole 17).</summary>
    public readonly NetworkVariable<int> ModeIndex = new NetworkVariable<int>(0);

    public bool IsDuel1v1 => ModeIndex.Value == 2;

    /// <summary>Duel : nombre de manches (1, 3, 5, 7, 9 ; 5 par défaut, premier à 3) et arme principale (0 aucune, 1 AK74, 2 sniper). Choix de l'hôte.</summary>
    public readonly NetworkVariable<int> DuelRounds = new NetworkVariable<int>(NetDuel.DefaultRounds);
    public readonly NetworkVariable<int> DuelPrimary = new NetworkVariable<int>(0);

    public void SetDuelRounds(int rounds) { if (IsServer && IsDuel1v1 && DuelRounds.Value != rounds) { DuelRounds.Value = Mathf.Clamp(rounds, 1, 9); ResetReady(); } }
    public void SetDuelPrimary(int primary) { if (IsServer && IsDuel1v1 && DuelPrimary.Value != primary) { DuelPrimary.Value = Mathf.Clamp(primary, 0, 2); ResetReady(); } }

    /// <summary>Duel : carte jouée (index dans ArenaMaps.All : 0 Alvéole 17, 1 Quai 9). Choix de l'hôte, vu par le client.</summary>
    public readonly NetworkVariable<int> DuelMap = new NetworkVariable<int>(0);

    public void SetDuelMap(int map)
    {
        map = Mathf.Clamp(map, 0, ArenaMaps.Count - 1);
        if (!IsServer || !IsDuel1v1 || DuelMap.Value == map) return;
        DuelMap.Value = map;
        gameScene = ArenaMaps.Get(map).scene;
        ResetReady();
    }

    // un réglage a changé : chacun doit se remettre PRÊT, le compte à rebours repart de zéro
    void ResetReady()
    {
        Countdown.Value = -1f;
        for (int i = 0; i < Players.Count; i++) { var e = Players[i]; e.ready = false; Players[i] = e; }
    }

    /// <summary>Duel : l'hôte force le lancement même seul (test, adversaire absent) : plus besoin d'être prêt ni d'être deux.</summary>
    public readonly NetworkVariable<bool> ForceStart = new NetworkVariable<bool>(false);

    /// <summary>Bouton « FORCER LE LANCEMENT » du lobby (hôte, mode duel seulement) ; un second appui annule.</summary>
    public void ToggleForceStart()
    {
        if (!IsServer || !IsDuel1v1 || launched) return;
        ForceStart.Value = !ForceStart.Value;
        if (!ForceStart.Value) Countdown.Value = -1f;
    }

    public static string ModeName(int index)
    {
        switch (index)
        {
            case 1: return "COOP : ÉVASION (BUNKER)";
            case 2: return "DUEL 1V1 : ALVÉOLE 17";
            default: return "COOP : VAGUES INFINIES (BUNKER)";
        }
    }

    /// <summary>Hôte : passe au mode suivant (annule un compte à rebours en cours, tout le monde doit se remettre PRÊT).</summary>
    public void CycleMode()
    {
        if (!IsServer) return;
        int next = (ModeIndex.Value + 1) % 3;
        SetMode(next);
    }

    /// <summary>Hôte : définit le mode par son index.</summary>
    public void SetMode(int mode)
    {
        if (!IsServer || ModeIndex.Value == mode) return;
        ModeIndex.Value = mode;
        ForceStart.Value = false;
        EndlessMode.Value = (mode == 0);
        gameScene = (mode == 2) ? ArenaMaps.Get(DuelMap.Value).scene : "MainScene";
        minPlayers = (Array.IndexOf(Environment.GetCommandLineArgs(), "-netsolo") >= 0 || SoloTest) ? 1 : 2;

        Countdown.Value = -1f;
        for (int i = 0; i < Players.Count; i++) { var e = Players[i]; e.ready = false; Players[i] = e; }
    }

    public void SetMode(bool endless)
    {
        SetMode(endless ? 0 : 1);
    }

    /// <summary>Numéro (1..4) attribué à un joueur dans le lobby, gardé pour la partie (avatar, scores).</summary>
    public static readonly System.Collections.Generic.Dictionary<ulong, int> Numbers = new System.Collections.Generic.Dictionary<ulong, int>();

    /// <summary>Test : un seul joueur suffit pour lancer (éditeur, mode Play sans second casque).</summary>
    public static bool SoloTest;

    bool launched;

    void Awake() => Players = new NetworkList<Entry>();

    public override void OnNetworkSpawn()
    {
        Instance = this;
        StartCoroutine(SendSkinWhenListed());
        PlayerName.Changed += SendName;
        if (!IsServer) return;
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-netsolo") >= 0 || SoloTest) minPlayers = 1;     // test seul
        foreach (var id in NetworkManager.ConnectedClientsIds) Add(id, "");
        NetworkManager.OnClientConnectedCallback += OnConnected;
        NetworkManager.OnClientDisconnectCallback += OnDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
        PlayerName.Changed -= SendName;
        if (NetworkManager)
        {
            NetworkManager.OnClientConnectedCallback -= OnConnected;
            NetworkManager.OnClientDisconnectCallback -= OnDisconnected;
        }
    }

    void OnConnected(ulong id) => Add(id, "");

    void OnDisconnected(ulong id)
    {
        for (int i = Players.Count - 1; i >= 0; i--) if (Players[i].clientId == id) Players.RemoveAt(i);
        Numbers.Remove(id);
        Countdown.Value = -1f;
    }

    void Add(ulong id, string device)
    {
        foreach (var e in Players) if (e.clientId == id) return;
        int n = 1;
        while (Numbers.ContainsValue(n)) n++;
        Numbers[id] = n;
        Players.Add(new Entry { clientId = id, number = n, ready = false, device = device });
    }

    /// <summary>Bouton PRÊT du lobby (chaque joueur pour lui-même).</summary>
    public void SetReady(bool ready) => SetReadyRpc(ready, SystemInfo.deviceModel.Length > 30 ? SystemInfo.deviceModel.Substring(0, 30) : SystemInfo.deviceModel);

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetReadyRpc(bool ready, string device, RpcParams p = default)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].clientId != p.Receive.SenderClientId) continue;
            var e = Players[i]; e.ready = ready; e.device = device; Players[i] = e;
        }
    }

    /// <summary>Choix du personnage dans le lobby (chaque joueur pour lui-même) ; gardé pour les parties suivantes.</summary>
    public void SetSkin(int skin)
    {
        PlayerSkin.Selected = skin;
        SetSkinRpc(PlayerSkin.Selected);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetSkinRpc(int skin, RpcParams p = default)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].clientId != p.Receive.SenderClientId) continue;
            var e = Players[i]; e.skin = Mathf.Clamp(skin, 0, PlayerSkin.Count - 1); Players[i] = e;
        }
    }

    // à l'arrivée dans le lobby : on annonce sa tenue dès que l'hôte nous a inscrits
    System.Collections.IEnumerator SendSkinWhenListed()
    {
        float until = Time.unscaledTime + 10f;
        while (Time.unscaledTime < until)
        {
            foreach (var e in Players) if (e.clientId == NetworkManager.LocalClientId) { SetSkinRpc(PlayerSkin.Selected); SendName(); yield break; }
            yield return null;
        }
    }

    // [hotfix #155] nom du joueur (compte Meta / casque), envoyé à l'arrivée et s'il arrive plus tard (réponse du compte Meta)
    void SendName() => SetNameRpc(new FixedString64Bytes(PlayerName.Local));

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetNameRpc(FixedString64Bytes n, RpcParams p = default)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].clientId != p.Receive.SenderClientId) continue;
            var e = Players[i]; e.name = n; Players[i] = e;
        }
    }

    /// <summary>Nom d'un joueur du lobby (« Joueur N » s'il n'en a pas).</summary>
    public string NameOf(Entry e) => PlayerName.Display(e.name.ToString(), e.number);

    public bool IsReady(ulong id) { foreach (var e in Players) if (e.clientId == id) return e.ready; return false; }

    public bool AllReady
    {
        get
        {
            if (ForceStart.Value && IsDuel1v1 && Players.Count >= 1) return true;      // lancement forcé par l'hôte
            if (Players.Count < minPlayers) return false;
            foreach (var e in Players) if (!e.ready) return false;
            return true;
        }
    }

    void Update()
    {
        if (!IsServer || launched) return;
        if (!AllReady) { if (Countdown.Value >= 0f) Countdown.Value = -1f; return; }
        if (Countdown.Value < 0f) Countdown.Value = CountdownTime;
        Countdown.Value = Mathf.Max(0f, Countdown.Value - Time.unscaledDeltaTime);
        if (Countdown.Value > 0f) return;

        launched = true;
        NetSession.InGame = true;
        LanDiscovery.StopAdvertising();
        StartGameRpc(EndlessMode.Value, DuelRounds.Value, DuelPrimary.Value);
        if (IsDuel1v1) gameScene = ArenaMaps.Get(DuelMap.Value).scene;          // la carte choisie dans le lobby
        NetworkManager.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

    // juste avant le chargement : tout le monde passe dans le mode choisi (la difficulté est celle de l'hôte : c'est lui qui crée les zombies)
    [Rpc(SendTo.Everyone)]
    void StartGameRpc(bool endless, int duelRounds, int duelPrimary)
    {
        NetDuel.Rounds = duelRounds;
        NetDuel.Primary = duelPrimary;
        NetSession.InGame = true;
        WaveManager.Endless = endless;
        WaveManager.ResumeLevel = 0;
        foreach (var e in Players) Numbers[e.clientId] = e.number;
    }
}
