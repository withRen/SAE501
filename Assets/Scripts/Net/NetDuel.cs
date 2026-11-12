using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// [Multijoueur 1v1] Gestionnaire du duel (arènes Alvéole 17 et Quai 9). Premier à TargetWins manches gagne le match.
// [Hotfix #160] Machine à états, décidée par l'hôte et synchronisée (Phase) :
//   Gel (3-2-1, personne ne tire) → MancheActive (on tire) → FinDeManche (4 s, personne ne tire ni ne marque) → Gel… → FinDeMatch.
//   - un seul point par manche : le premier joueur tué en MancheActive fait passer en FinDeManche, les morts suivantes sont ignorées
//     (deux morts presque simultanées : seule la première arrivée à l'hôte compte) ;
//   - hors MancheActive, l'arme ne tire pas (Pistol.Fire → CanShoot) et un coup qui arriverait quand même est refusé par la victime
//     (NetPlayer.HitRpc) : un joueur mort ne peut plus marquer ;
//   - nouvelle manche : vie pleine, chacun à son côté de la carte, arme rendue, gel de 3 s puis MancheActive ;
//   - le déplacement reste libre pendant le gel et la fin de manche (pas de blocage imposé au casque).
// Affichage : NetDuelHud (bandeau des scores façon CS:GO, en haut de la vue), à la place de l'ancien tableau suspendu.
public class NetDuel : NetworkBehaviour
{
    public static NetDuel Instance { get; private set; }

    /// <summary>Réglages du duel choisis par l'hôte dans le lobby (envoyés à tous au lancement, NetLobby.StartGameRpc).</summary>
    public const int DefaultRounds = 5;
    public static int Rounds = DefaultRounds;           // nombre de manches (impair) : 1, 3, 5, 7 ou 9
    public static int Primary;                          // arme principale : 0 aucune (pistolet seul), 1 AK74, 2 sniper ; le pistolet est toujours l'arme secondaire
    public static int TargetWins => Mathf.Max(1, (Rounds + 1) / 2);     // 5 manches : premier à 3

    public static string PrimaryName(int p) => p == 1 ? "AK74" : p == 2 ? "SNIPER" : "AUCUNE";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Rounds = DefaultRounds; Primary = 0; }
    public const string MenuScene = "Menu";

    public enum Phase : byte { Gel = 0, MancheActive = 1, FinDeManche = 2, FinDeMatch = 3 }
    public const float FreezeTime = 3f, RoundEndTime = 4f, FirstFreezeDelay = 1f;

    public readonly NetworkVariable<int> Round = new NetworkVariable<int>(1);
    public readonly NetworkVariable<int> ScoreP1 = new NetworkVariable<int>(0);
    public readonly NetworkVariable<int> ScoreP2 = new NetworkVariable<int>(0);
    public readonly NetworkVariable<int> Winner = new NetworkVariable<int>(0); // 0 = en cours, 1 = côté 1 gagne, 2 = côté 2 gagne
    public readonly NetworkVariable<byte> PhaseVar = new NetworkVariable<byte>((byte)Phase.Gel);
    public readonly NetworkVariable<double> PhaseUntil = new NetworkVariable<double>(0);    // fin du gel / de la fin de manche (temps serveur)
    public readonly NetworkVariable<double> RoundStart = new NetworkVariable<double>(0);    // début de la manche active (chrono)
    public readonly NetworkVariable<int> RoundWinner = new NetworkVariable<int>(0);         // côté gagnant de la dernière manche

    public Phase CurrentPhase => (Phase)PhaseVar.Value;
    /// <summary>Peut-on tirer ? Vrai hors duel ; en duel, seulement en MancheActive.</summary>
    public static bool CanShoot => !Instance || !Instance.IsSpawned || Instance.CurrentPhase == Phase.MancheActive;
    public double Now => NetworkManager ? NetworkManager.ServerTime.Time : Time.timeAsDouble;

    NetDuelHud hud;

    // Un objet réseau doit venir d'un prefab enregistré (Resources/Net/NetDuel, construit par NetPrefabsBuilder) :
    // un objet créé à la volée n'a pas d'identifiant réseau et ne peut pas être « spawné » (erreur « GlobalObjectIdHash 0 »).
    public static void EnsureSpawned()
    {
        if (Instance != null) return;
        var prefab = Resources.Load<GameObject>("Net/NetDuel");
        if (!prefab) { Debug.LogError("[Net] Resources/Net/NetDuel introuvable : menu SAE501 → Build Net Prefabs"); return; }
        Object.Instantiate(prefab).GetComponent<NetworkObject>().Spawn(true);
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;
        Pistol.InfiniteAmmo = true;             // duel : chargeurs infinis (chargeur limité, rechargement automatique)
        hud = NetDuelHud.Create(this);

        var localHealth = PlayerHealth.Instance;
        if (localHealth) localHealth.Died += OnLocalDied;
        StartCoroutine(GiveWeaponSoon());

        if (!IsServer) return;
        // gel de départ : le temps que les rigs et les armes soient prêts, puis 3-2-1
        SetPhase(Phase.Gel, FirstFreezeDelay + FreezeTime);
        // un joueur réseau (avatar visible, zones touchables, vie) par casque connecté, comme en coop (NetGame)
        foreach (var id in NetworkManager.ConnectedClientsIds)
        {
            int n = NetLobby.Numbers.TryGetValue(id, out var k) ? k : 1;
            var go = Instantiate(Resources.Load<GameObject>("Net/NetPlayer"));
            go.GetComponent<NetPlayer>().Number.Value = n;
            go.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
        }
    }

    void SetPhase(Phase p, float seconds = 0f)
    {
        PhaseVar.Value = (byte)p;
        PhaseUntil.Value = Now + seconds;
        if (p == Phase.MancheActive) RoundStart.Value = Now;
    }

    // le rig et ses mains sont prêts une seconde après l'arrivée dans l'arène
    IEnumerator GiveWeaponSoon()
    {
        yield return new WaitForSeconds(1f);
        DuelLoadout.Give();
    }

    public override void OnNetworkDespawn()
    {
        Pistol.InfiniteAmmo = false;
        if (Instance == this) Instance = null;
        if (hud) Destroy(hud.gameObject);
        var localHealth = PlayerHealth.Instance;
        if (localHealth) localHealth.Died -= OnLocalDied;
    }

    void Update()
    {
        if (!IsServer || !IsSpawned) return;
        if (CurrentPhase == Phase.Gel && Now >= PhaseUntil.Value) SetPhase(Phase.MancheActive);   // fin du gel : on peut tirer
    }

    void OnLocalDied()
    {
        if (!IsServer) ReportDeathServerRpc(NetSession.LocalId);
        else HandlePlayerDeath(NetSession.LocalId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ReportDeathServerRpc(ulong deadClientId) => HandlePlayerDeath(deadClientId);

    /// <summary>Hôte : une mort annoncée. Ne compte qu'en MancheActive, et une seule fois par manche.</summary>
    public void HandlePlayerDeath(ulong deadClientId)
    {
        if (!IsServer || CurrentPhase != Phase.MancheActive || Winner.Value != 0) return;

        // [hotfix #158] côté du mort (1 ou 2, rang parmi les deux joueurs) : l'autre côté marque
        int deadSide = NetLobby.Side(deadClientId);
        int winningSide = (deadSide == 1) ? 2 : 1;
        if (winningSide == 1) ScoreP1.Value++;
        else ScoreP2.Value++;
        RoundWinner.Value = winningSide;

        if (ScoreP1.Value >= TargetWins || ScoreP2.Value >= TargetWins)
        {
            Winner.Value = winningSide;
            SetPhase(Phase.FinDeMatch, 7f);
            StartCoroutine(EndMatch());
        }
        else
        {
            SetPhase(Phase.FinDeManche, RoundEndTime);
            StartCoroutine(NextRoundSequence());
        }
    }

    IEnumerator NextRoundSequence()
    {
        yield return new WaitForSeconds(RoundEndTime);
        Round.Value++;
        SetPhase(Phase.Gel, FreezeTime);
        ResetRoundRpc();
    }

    [Rpc(SendTo.Everyone)]
    void ResetRoundRpc()
    {
        var h = PlayerHealth.Instance;
        if (h)
        {
            h.Revive(100f);
            h.Heal(100f);
            ArenaMaps.PlaceRig(h.transform, NetLobby.Side(NetSession.LocalId));     // [hotfix #158] côté 1 ou 2, jamais le même
        }
        DuelLoadout.Give();                     // l'arme revient dans la main si elle avait été lâchée
    }

    IEnumerator EndMatch()
    {
        yield return new WaitForSeconds(7f);
        if (IsServer)
        {
            // [Hotfix #157] fin du match : chacun ferme la session réseau et revient au menu de lui-même. Avant, l'hôte rechargeait
            // le menu par Netcode sans fermer la session : elle restait ouverte en arrière-plan (lobby disparu) et le HÉBERGER
            // suivant échouait (« Impossible d'ouvrir le port 7777 »).
            EndMatchRpc();
            yield return new WaitForSeconds(0.5f);          // le message part avant que l'hôte ne ferme la session
            NetSession.LeaveToMenu();
        }
    }

    [Rpc(SendTo.NotServer)]
    void EndMatchRpc() => NetSession.LeaveToMenu();

    /// <summary>Nom affiché d'un côté (1 ou 2) : nom du joueur de ce côté, sinon « Joueur N ».</summary>
    public static string SideName(int side)
    {
        foreach (var p in NetPlayer.All)
            if (p && p.IsSpawned && NetLobby.Side(p.OwnerClientId) == side) return p.DisplayName;
        return "Joueur " + side;
    }
}
