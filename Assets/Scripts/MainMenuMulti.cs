using System.Collections.Generic;
using UnityEngine;

// Pages MULTIJOUEUR du menu d'accueil (coop en réseau local, Vagues infinies ou Évasion, au choix de l'hôte) :
//   MULTIJOUEUR → HÉBERGER : ce casque (ou le PC) devient l'hôte, le lobby s'ouvre ;
//              → REJOINDRE : liste des parties trouvées sur le Wi-Fi (LanDiscovery), on vise et on valide ;
//   LOBBY : les joueurs connectés et leur état, bouton PRÊT ; quand tout le monde est prêt, compte à rebours puis la partie.
// Le réseau lui-même : NetSession, NetLobby. Les pages sont construites comme les autres (AmberMenu, UiKit).
public partial class MainMenu
{
    enum MultiPage { None, Choice, Join, Lobby }

    Transform multiChoicePage, joinPage, lobbyPage;
    TextMesh multiInfo, joinInfo, lobbyInfo, lobbyCountdown;
    readonly List<MenuButton> gameButtons = new List<MenuButton>();
    readonly List<string> gameIps = new List<string>();
    readonly List<TextMesh> playerLines = new List<TextMesh>();
    MenuButton readyButton, modeButton;
    MultiPage multiPage;
    bool localReady, lastMode = true;
    float nextRefresh;

    void BuildMultiPages()
    {
        // --- choix ---
        var p = multiChoicePage = Panel("Page_Multi", "BUNKER-OS  //  MULTIJOUEUR", version);
        UiKit.Text(p, "Titre_Haut", new Vector2(0f, 0.43f), 0.0113f, A, UiKit.Spaced("COOP"), TextAnchor.MiddleCenter, true);
        UiKit.Text(p, "Titre", new Vector2(0f, 0.31f), 0.02f, A, "RÉSEAU LOCAL", TextAnchor.MiddleCenter, true);
        BigButton(p, 0.12f, "HÉBERGER", Host);
        UiKit.Text(p, "Desc_Heberger", new Vector2(0f, 0.01f), 0.0068f, UiKit.Dim, "Ce casque crée la partie : les autres le rejoignent", TextAnchor.MiddleCenter);
        BigButton(p, -0.13f, "REJOINDRE", () => ShowMulti(MultiPage.Join));
        UiKit.Text(p, "Desc_Rejoindre", new Vector2(0f, -0.24f), 0.0068f, UiKit.Dim, "Vagues infinies ou Évasion, 2 à 4 joueurs\nTous sur le même Wi-Fi (partage de connexion conseillé)", TextAnchor.MiddleCenter);
        multiInfo = UiKit.Text(p, "Info", new Vector2(0f, -0.38f), 0.0075f, UiKit.Red, "", TextAnchor.MiddleCenter, true);
        Back(p, () => ShowMulti(MultiPage.None));
        p.gameObject.SetActive(false);

        // --- rejoindre : parties trouvées ---
        p = joinPage = Panel("Page_Rejoindre", "BUNKER-OS  //  REJOINDRE", version);
        UiKit.Text(p, "Titre", new Vector2(0f, 0.43f), 0.0113f, A, UiKit.Spaced("PARTIES"), TextAnchor.MiddleCenter, true);
        for (int i = 0; i < 4; i++)
        {
            int k = i;
            var b = BigButton(p, 0.27f - i * 0.15f, "", () => Join(k));
            b.gameObject.SetActive(false);
            gameButtons.Add(b);
            gameIps.Add("");
        }
        joinInfo = UiKit.Text(p, "Info", new Vector2(0f, -0.38f), 0.0075f, UiKit.Dim, "", TextAnchor.MiddleCenter, true);
        Back(p, () => ShowMulti(MultiPage.Choice));
        p.gameObject.SetActive(false);

        // --- lobby ---
        p = lobbyPage = Panel("Page_Lobby", "BUNKER-OS  //  LOBBY", version);
        UiKit.Text(p, "Titre", new Vector2(0f, 0.45f), 0.0113f, A, UiKit.Spaced("LOBBY"), TextAnchor.MiddleCenter, true);
        lobbyInfo = UiKit.Text(p, "Info", new Vector2(0f, 0.36f), 0.0072f, UiKit.Dim, "", TextAnchor.MiddleCenter);
        for (int i = 0; i < 4; i++)
            playerLines.Add(UiKit.Text(p, "Joueur_" + (i + 1), new Vector2(-HW + 0.05f, 0.24f - i * 0.085f), 0.0105f, UiKit.White, "", TextAnchor.MiddleLeft, true));
        readyButton = BigButton(p, -0.2f, "PRÊT", ToggleReady);
        lobbyCountdown = UiKit.Text(p, "Compte", new Vector2(0f, -0.35f), 0.0105f, A, "", TextAnchor.MiddleCenter, true);
        var quit = MakeButton(p, new Vector2(-HW + 0.17f, -0.53f), new Vector2(0.34f, 0.09f), "◂  QUITTER", 0.0095f, LeaveLobby);
        quit.textColor = A;
        // l'hôte choisit le mode (les autres le voient dans la ligne d'information)
        modeButton = MakeButton(p, new Vector2(HW - 0.27f, -0.53f), new Vector2(0.54f, 0.09f), "", 0.0085f,
                                () => { if (NetLobby.Instance) NetLobby.Instance.SetMode(!NetLobby.Instance.EndlessMode.Value); });
        modeButton.textColor = A;
        p.gameObject.SetActive(false);

        // retour au menu après une partie en réseau ou une déconnexion : page multijoueur avec la raison
        if (NetSession.Active && !NetSession.InGame && NetLobby.Instance) ShowMulti(MultiPage.Lobby);
        else if (!string.IsNullOrEmpty(NetSession.LastError)) ShowMulti(MultiPage.Choice);
    }

    void Back(Transform p, System.Action click)
    {
        var back = MakeButton(p, new Vector2(-HW + 0.17f, -0.53f), new Vector2(0.34f, 0.09f), "◂  RETOUR", 0.0095f, click);
        back.textColor = A;
    }

    void ShowMulti(MultiPage page)
    {
        multiPage = page;
        mainPage.gameObject.SetActive(page == MultiPage.None);
        multiChoicePage.gameObject.SetActive(page == MultiPage.Choice);
        joinPage.gameObject.SetActive(page == MultiPage.Join);
        lobbyPage.gameObject.SetActive(page == MultiPage.Lobby);
        if (page == MultiPage.Join) LanDiscovery.StartListening(); else LanDiscovery.StopListening();
        if (page == MultiPage.Choice) multiInfo.text = NetSession.LastError ?? "";
        if (page == MultiPage.Lobby) { localReady = false; readyButton.label.text = "PRÊT"; }
        nextRefresh = 0f;
    }

    void Host()
    {
        if (!NetSession.Host()) { multiInfo.text = NetSession.LastError ?? "Échec"; return; }
        ShowMulti(MultiPage.Lobby);
    }

    void Join(int i)
    {
        if (i >= gameIps.Count || string.IsNullOrEmpty(gameIps[i])) return;
        if (!NetSession.Join(gameIps[i])) { joinInfo.text = NetSession.LastError ?? "Échec"; return; }
        ShowMulti(MultiPage.Lobby);
    }

    void ToggleReady()
    {
        if (!NetLobby.Instance) return;
        localReady = !localReady;
        NetLobby.Instance.SetReady(localReady);
        readyButton.label.text = localReady ? "PAS PRÊT" : "PRÊT";
    }

    void LeaveLobby()
    {
        NetSession.Leave();
        NetSession.LastError = null;
        ShowMulti(MultiPage.Choice);
    }

    void Update()
    {
        if (multiPage == MultiPage.None || multiPage == MultiPage.Choice || Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.25f;
        if (multiPage == MultiPage.Join) RefreshJoin();
        else if (multiPage == MultiPage.Lobby) RefreshLobby();
    }

    void RefreshJoin()
    {
        var games = LanDiscovery.Games();
        for (int i = 0; i < gameButtons.Count; i++)
        {
            bool on = i < games.Count;
            gameButtons[i].gameObject.SetActive(on);
            if (!on) { gameIps[i] = ""; continue; }
            var g = games[i];
            gameIps[i] = g.ip;
            bool same = g.version == Application.version;
            gameButtons[i].interactable = same;
            gameButtons[i].label.text = "PARTIE DE " + g.name.ToUpper() + "   " + g.players + "/" + NetSession.MaxPlayers + (same ? "" : "   (VERSION " + g.version + ")");
        }
        joinInfo.text = games.Count == 0 ? "Recherche des parties sur le Wi-Fi…\nL'hôte doit avoir choisi HÉBERGER." : "Visez une partie et validez.";
    }

    void RefreshLobby()
    {
        var lobby = NetLobby.Instance;
        if (!NetSession.Active) { ShowMulti(MultiPage.Choice); return; }
        if (!lobby)
        {
            lobbyInfo.text = "Connexion à l'hôte…";
            foreach (var t in playerLines) t.text = "";
            return;
        }
        string mode = lobby.EndlessMode.Value ? "VAGUES INFINIES" : "ÉVASION";
        lobbyInfo.text = NetSession.IsServer ? "Vous hébergez · adresse " + LanDiscovery.LocalIp() + " · mode " + mode
                                             : "Connecté · mode " + mode;
        modeButton.gameObject.SetActive(NetSession.IsServer);
        modeButton.label.text = "MODE : " + mode;
        // l'hôte a changé de mode : chacun doit se remettre PRÊT (le serveur a déjà remis les états à zéro)
        if (lastMode != lobby.EndlessMode.Value) { lastMode = lobby.EndlessMode.Value; localReady = false; readyButton.label.text = "PRÊT"; }
        for (int i = 0; i < playerLines.Count; i++)
        {
            if (i >= lobby.Players.Count) { playerLines[i].text = ""; continue; }
            var e = lobby.Players[i];
            bool me = e.clientId == NetSession.LocalId;
            playerLines[i].text = "JOUEUR " + e.number + (e.clientId == Unity.Netcode.NetworkManager.ServerClientId ? " (HÔTE)" : "") + (me ? "  ◂ VOUS" : "")
                                + "      " + (e.ready ? "✓ PRÊT" : "en attente");
            playerLines[i].color = e.ready ? UiKit.Green : UiKit.White;
        }
        float c = lobby.Countdown.Value;
        lobbyCountdown.text = c >= 0f ? "LANCEMENT DANS " + Mathf.CeilToInt(c) + " S"
                            : lobby.Players.Count < lobby.minPlayers ? "En attente d'un autre joueur…"
                            : "La partie démarre quand tout le monde est prêt";
    }
}
