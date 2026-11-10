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
        public FixedString32Bytes device;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref clientId); s.SerializeValue(ref number); s.SerializeValue(ref ready); s.SerializeValue(ref device);
        }
        public bool Equals(Entry o) => clientId == o.clientId && number == o.number && ready == o.ready && device.Equals(o.device);
    }

    public NetworkList<Entry> Players;
    public readonly NetworkVariable<float> Countdown = new NetworkVariable<float>(-1f);   // < 0 : pas de compte à rebours
    /// <summary>Mode choisi par l'hôte : vrai = Vagues infinies, faux = Évasion (4 niveaux, puis le générateur).</summary>
    public readonly NetworkVariable<bool> EndlessMode = new NetworkVariable<bool>(true);

    /// <summary>Hôte : change de mode (annule un compte à rebours en cours, tout le monde doit se remettre PRÊT).</summary>
    public void SetMode(bool endless)
    {
        if (!IsServer || EndlessMode.Value == endless) return;
        EndlessMode.Value = endless;
        Countdown.Value = -1f;
        for (int i = 0; i < Players.Count; i++) { var e = Players[i]; e.ready = false; Players[i] = e; }
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
        if (!IsServer) return;
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-netsolo") >= 0 || SoloTest) minPlayers = 1;     // test seul
        foreach (var id in NetworkManager.ConnectedClientsIds) Add(id, "");
        NetworkManager.OnClientConnectedCallback += OnConnected;
        NetworkManager.OnClientDisconnectCallback += OnDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
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

    public bool IsReady(ulong id) { foreach (var e in Players) if (e.clientId == id) return e.ready; return false; }

    public bool AllReady
    {
        get
        {
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
        StartGameRpc(EndlessMode.Value);
        NetworkManager.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

    // juste avant le chargement : tout le monde passe dans le mode choisi (la difficulté est celle de l'hôte : c'est lui qui crée les zombies)
    [Rpc(SendTo.Everyone)]
    void StartGameRpc(bool endless)
    {
        NetSession.InGame = true;
        WaveManager.Endless = endless;
        WaveManager.ResumeLevel = 0;
        foreach (var e in Players) Numbers[e.clientId] = e.number;
    }
}
