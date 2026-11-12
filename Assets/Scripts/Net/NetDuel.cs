using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// [Multijoueur 1v1] Gestionnaire de match à mort en duel dans l'Arène Bunker "Alvéole 17".
// Premier joueur à remporter 3 rounds gagne le match.
// Gère les réapparitions aux sas opposés (Spawn_Player_1 Ouest, Spawn_Player_2 Est), la santé et le score.
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

    public readonly NetworkVariable<int> Round = new NetworkVariable<int>(1);
    public readonly NetworkVariable<int> ScoreP1 = new NetworkVariable<int>(0);
    public readonly NetworkVariable<int> ScoreP2 = new NetworkVariable<int>(0);
    public readonly NetworkVariable<int> Winner = new NetworkVariable<int>(0); // 0 = en cours, 1 = J1 gagne, 2 = J2 gagne

    TextMesh bannerText;
    bool roundResolving;

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
        BuildBanner();
        Round.OnValueChanged += (_, r) => UpdateBanner();
        ScoreP1.OnValueChanged += (_, s) => UpdateBanner();
        ScoreP2.OnValueChanged += (_, s) => UpdateBanner();
        Winner.OnValueChanged += (_, w) => UpdateBanner();

        var localHealth = PlayerHealth.Instance;
        if (localHealth) localHealth.Died += OnLocalDied;

        UpdateBanner();
        StartCoroutine(AnnounceStart());
        StartCoroutine(GiveWeaponSoon());

        if (!IsServer) return;
        // un joueur réseau (avatar visible, zones touchables, vie) par casque connecté, comme en coop (NetGame)
        foreach (var id in NetworkManager.ConnectedClientsIds)
        {
            int n = NetLobby.Numbers.TryGetValue(id, out var k) ? k : 1;
            var go = Instantiate(Resources.Load<GameObject>("Net/NetPlayer"));
            go.GetComponent<NetPlayer>().Number.Value = n;
            go.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
        }
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
        var localHealth = PlayerHealth.Instance;
        if (localHealth) localHealth.Died -= OnLocalDied;
    }

    void OnLocalDied()
    {
        if (!IsServer)
        {
            ReportDeathServerRpc(NetSession.LocalId);
        }
        else
        {
            HandlePlayerDeath(NetSession.LocalId);
        }
    }

    [Rpc(SendTo.Server)]
    void ReportDeathServerRpc(ulong deadClientId)
    {
        HandlePlayerDeath(deadClientId);
    }

    void HandlePlayerDeath(ulong deadClientId)
    {
        if (!IsServer || roundResolving || Winner.Value != 0) return;
        roundResolving = true;

        // [hotfix #158] côté du mort (1 ou 2, rang parmi les deux joueurs) : l'autre côté marque
        int deadSide = NetLobby.Side(deadClientId);
        int winningPlayer = (deadSide == 1) ? 2 : 1;
        if (winningPlayer == 1) ScoreP1.Value++;
        else ScoreP2.Value++;

        if (ScoreP1.Value >= TargetWins)
        {
            Winner.Value = 1;
            StartCoroutine(EndMatch());
        }
        else if (ScoreP2.Value >= TargetWins)
        {
            Winner.Value = 2;
            StartCoroutine(EndMatch());
        }
        else
        {
            StartCoroutine(NextRoundSequence(winningPlayer));
        }
    }

    IEnumerator NextRoundSequence(int roundWinner)
    {
        AnnounceRpc($"ROUND {Round.Value} REMPORTÉ PAR LE JOUEUR {roundWinner} !", 3.5f);
        yield return new WaitForSeconds(4f);

        Round.Value++;
        roundResolving = false;
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
        }

        // Repositionner au sas de départ
        int n = NetLobby.Side(NetSession.LocalId);       // [hotfix #158] côté 1 ou 2, jamais le même que l'adversaire
        if (h)
        {
            ArenaMaps.PlaceRig(h.transform, n);        // apparitions de la carte jouée (ArenaMaps)
        }

        DuelLoadout.Give();                     // l'arme revient dans la main si elle avait été lâchée
        StartCoroutine(AnnounceStart());
    }

    IEnumerator AnnounceStart()
    {
        if (bannerText)
        {
            bannerText.text = $"ROUND {Round.Value}\nCOMBATTEZ !";
            bannerText.color = new Color(1f, 0.75f, 0.25f);
        }
        yield return new WaitForSeconds(2.5f);
        UpdateBanner();
    }

    [Rpc(SendTo.Everyone)]
    void AnnounceRpc(string msg, float duration)
    {
        StartCoroutine(DoAnnounce(msg, duration));
    }

    IEnumerator DoAnnounce(string msg, float duration)
    {
        if (bannerText)
        {
            bannerText.text = msg;
            bannerText.color = Color.white;
        }
        yield return new WaitForSeconds(duration);
        UpdateBanner();
    }

    IEnumerator EndMatch()
    {
        AnnounceRpc($"VICTOIRE FINALE : JOUEUR {Winner.Value} !\nRETOUR AU TERMINAL...", 6f);
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

    void BuildBanner()
    {
        // Panneau d'affichage suspendu au-dessus de la brèche centrale
        var go = new GameObject("Duel_Scoreboard");
        go.transform.position = ArenaMaps.Current.banner;      // au-dessus du centre de la carte jouée
        bannerText = go.AddComponent<TextMesh>();
        bannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        go.GetComponent<MeshRenderer>().sharedMaterial = bannerText.font.material;
        bannerText.fontSize = 44;
        bannerText.characterSize = 0.02f;
        bannerText.alignment = TextAlignment.Center;
        bannerText.anchor = TextAnchor.MiddleCenter;
        bannerText.color = new Color(1f, 0.75f, 0.25f);
    }

    void Update()
    {
        // Faire face à la caméra principale
        if (bannerText && Camera.main)
        {
            var cam = Camera.main.transform;
            var dir = bannerText.transform.position - cam.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                bannerText.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void UpdateBanner()
    {
        if (!bannerText) return;
        if (Winner.Value != 0)
        {
            bannerText.text = $"DUEL TERMINÉ\nVICTOIRE JOUEUR {Winner.Value}";
            bannerText.color = new Color(0.3f, 1f, 0.4f);
        }
        else
        {
            bannerText.text = $"{ArenaMaps.Current.name}  //  DUEL 1V1\nROUND {Round.Value}  (PREMIER À {TargetWins})\nJ1 : {ScoreP1.Value}   —   J2 : {ScoreP2.Value}";
            bannerText.color = new Color(1f, 0.75f, 0.25f);
        }
    }
}
