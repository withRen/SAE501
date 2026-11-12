using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

// Multijoueur en réseau local (coop, mode Vagues infinies) : point d'entrée unique.
// Un joueur « héberge » (hôte = serveur + joueur, sur un casque ou le PC), les autres le « rejoignent » par son adresse IP
// (trouvée par LanDiscovery). Netcode for GameObjects + Unity Transport, port UDP 7777, aucun Internet.
// Le NetworkManager vient du prefab Resources/Net/NetworkManager (construit par NetPrefabsBuilder) et survit aux changements de scène.
// En solo, rien de tout ça n'est créé : Active est faux et le jeu marche exactement comme avant.
public static class NetSession
{
    public const ushort GamePort = 7777;
    public const int MaxPlayers = 4;
    public const string MenuScene = "Menu";

    public static NetworkManager Net => NetworkManager.Singleton;
    /// <summary>Une partie en réseau est en cours (hôte ou client).</summary>
    public static bool Active => Net != null && Net.IsListening;
    public static bool IsServer => Active && Net.IsServer;
    /// <summary>Ce casque est client : les zombies et les vagues sont décidés par l'hôte.</summary>
    public static bool IsClientOnly => Active && !Net.IsServer;
    public static ulong LocalId => Active ? Net.LocalClientId : 0;

    /// <summary>Partie lancée depuis le lobby : les clients refusent les nouveaux venus.</summary>
    public static bool InGame;
    /// <summary>Message à afficher au retour dans le menu (hôte parti, version différente…).</summary>
    public static string LastError;

    /// <summary>[Hotfix #157] Une session est encore ouverte ou en train de se fermer (Netcode ferme à l'image suivante) :
    /// on ne peut ni héberger ni rejoindre tant qu'elle n'est pas fermée (StartHost refuse, « Impossible d'ouvrir le port »).</summary>
    public static bool Busy => Net != null && (Net.IsListening || Net.ShutdownInProgress);
    static bool leaving;            // départ volontaire : pas de message « connexion perdue »

    static string Version => Application.version;
    /// <summary>Vérification de version à la connexion : désactivée pour l'instant (en attendant l'auto-updateur).</summary>
    public const bool CheckVersion = false;

    static NetworkManager Ensure()
    {
        if (Net) return Net;
        var prefab = Resources.Load<GameObject>("Net/NetworkManager");
        if (!prefab) { Debug.LogError("[Net] Resources/Net/NetworkManager introuvable : menu SAE501 → Build Net Prefabs"); return null; }
        var go = Object.Instantiate(prefab);
        go.name = "NetworkManager";
        var nm = go.GetComponent<NetworkManager>();
        nm.NetworkConfig.ConnectionApproval = true;
        nm.ConnectionApprovalCallback = Approve;
        nm.OnClientDisconnectCallback += OnDisconnect;
        return nm;
    }

    // l'hôte n'accepte que la même version du jeu, pas plus de MaxPlayers joueurs (2 en 1v1), et personne une fois la partie lancée
    static void Approve(NetworkManager.ConnectionApprovalRequest req, NetworkManager.ConnectionApprovalResponse res)
    {
        string v = req.Payload != null ? Encoding.UTF8.GetString(req.Payload) : "";
        res.CreatePlayerObject = false;
        if (req.ClientNetworkId == Net.LocalClientId) { res.Approved = true; return; }
        if (CheckVersion && v != Version) { res.Approved = false; res.Reason = "Version différente (hôte " + Version + ", vous " + v + ")"; return; }
        if (InGame) { res.Approved = false; res.Reason = "La partie a déjà commencé"; return; }
        int max = (NetLobby.Instance && NetLobby.Instance.IsDuel1v1) ? 2 : MaxPlayers;
        if (Net.ConnectedClientsIds.Count >= max) { res.Approved = false; res.Reason = "Partie complète"; return; }
        res.Approved = true;
    }

    static void OnSceneLoaded(string scene, UnityEngine.SceneManagement.LoadSceneMode mode, System.Collections.Generic.List<ulong> done, System.Collections.Generic.List<ulong> timedOut)
    {
        if (!Net || !Net.IsServer || scene == MenuScene) return;
        // un casque trop lent à charger est déconnecté (il reviendra au menu avec un message)
        foreach (var id in timedOut) Net.DisconnectClient(id, "Chargement trop long");

        if (ArenaMaps.IsArena(scene))      // toutes les cartes du duel (ArenaMaps)
        {
            NetDuel.EnsureSpawned();
        }
        else if (!NetGame.Instance)
        {
            Object.Instantiate(Resources.Load<GameObject>("Net/NetGame")).GetComponent<NetworkObject>().Spawn(true);
        }
    }

    static void OnDisconnect(ulong id)
    {
        // ce casque a perdu l'hôte (ou a été refusé) : retour au menu avec la raison
        if (Net && !Net.IsServer && id == Net.LocalClientId && !leaving)
        {
            LastError = string.IsNullOrEmpty(Net.DisconnectReason) ? "Connexion à l'hôte perdue" : Net.DisconnectReason;
            Leave();
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != MenuScene)
                UnityEngine.SceneManagement.SceneManager.LoadScene(MenuScene);
        }
    }

    public static bool Host()
    {
        var nm = Ensure();
        if (!nm) return false;
        if (Busy) { LastError = "La partie précédente se ferme encore, réessaie dans une seconde"; Leave(); return false; }
        LastError = null; InGame = false; leaving = false;
        NetLobby.Numbers.Clear();           // [hotfix #158] numéros de la session précédente
        nm.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", GamePort, "0.0.0.0");
        nm.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(Version);
        if (!nm.StartHost()) { LastError = "Impossible d'ouvrir le port " + GamePort; return false; }
        LanDiscovery.StartAdvertising();
        // le lobby (liste des joueurs, PRÊT) ; puis, quand tous les casques ont chargé la scène de jeu, la partie réseau
        Object.Instantiate(Resources.Load<GameObject>("Net/NetLobby")).GetComponent<NetworkObject>().Spawn(true);
        nm.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        nm.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        return true;
    }

    public static bool Join(string ip)
    {
        var nm = Ensure();
        if (!nm) return false;
        if (Busy) { LastError = "La partie précédente se ferme encore, réessaie dans une seconde"; Leave(); return false; }
        LastError = null; InGame = false; leaving = false;
        NetLobby.Numbers.Clear();
        nm.GetComponent<UnityTransport>().SetConnectionData(ip, GamePort);
        nm.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(Version);
        if (!nm.StartClient()) { LastError = "Impossible de joindre " + ip; return false; }
        return true;
    }

    /// <summary>Quitter le lobby ou la partie (l'hôte qui part ferme la partie pour tout le monde).</summary>
    public static void Leave()
    {
        LanDiscovery.StopAdvertising();
        InGame = false;
        if (Net && Net.IsListening) { leaving = true; Net.Shutdown(); }
        WaveManager.Endless = false;
    }

    /// <summary>[Hotfix #157] Fin d'un match : fermer la session et revenir au menu, sans message d'erreur.</summary>
    public static void LeaveToMenu()
    {
        Leave();
        LastError = null;
        var go = new GameObject("Retour_Menu");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<Runner>().StartCoroutine(MenuWhenClosed(go));
    }

    // le menu est chargé une fois la session vraiment fermée (Netcode ferme à l'image suivante), 3 s au plus
    static System.Collections.IEnumerator MenuWhenClosed(GameObject runner)
    {
        float until = Time.unscaledTime + 3f; int frames = 0;
        while (Busy && (Time.unscaledTime < until || frames++ < 30)) yield return null;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != MenuScene)
            UnityEngine.SceneManagement.SceneManager.LoadScene(MenuScene);
        Object.Destroy(runner);
    }

    class Runner : MonoBehaviour { }
}
