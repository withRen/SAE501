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
    MenuButton readyButton, forceButton;
    readonly List<MenuButton> modeCards = new List<MenuButton>();
    readonly List<PlayerRow> playerRows = new List<PlayerRow>();

    class PlayerRow { public Renderer frame, dot; public TextMesh name, tag, skin, state; }
    readonly List<MenuButton> skinButtons = new List<MenuButton>();
    GameObject skinPreview;
    int previewSkin = -1;

    // ---------- réglages du duel : nombre de manches (1, 3, 5, 7, 9 ; 5 par défaut) et arme principale (+ pistolet en arme secondaire) ----------
    static readonly int[] RoundChoices = { 1, 3, 5, 7, 9 };
    static readonly string[] PrimaryChoices = { "AUCUNE", "AK74", "SNIPER" };
    readonly List<MenuButton> roundButtons = new List<MenuButton>(), primaryButtons = new List<MenuButton>();
    readonly List<TextMesh> duelTexts = new List<TextMesh>();
    float readyClickTime, lobbyEnteredAt;

    readonly List<Renderer> duelLines = new List<Renderer>();

    void BuildDuelOptions(Transform p)
    {
        // sous la liste des 2 joueurs : un titre de section (avec le rappel du pistolet), puis une rangée par réglage
        float ys = -0.075f, y1 = -0.155f, y2 = -0.235f;
        duelTexts.Add(UiKit.Text(p, "Duel_Section", new Vector2(-HW, ys), 0.0075f, UiKit.Amber, "RÉGLAGES DU DUEL", TextAnchor.MiddleLeft, true));
        duelTexts.Add(UiKit.Text(p, "Duel_Section_Aide", new Vector2(HW, ys), 0.0062f, UiKit.Dim, "+ PISTOLET EN ARME SECONDAIRE", TextAnchor.MiddleRight));
        duelLines.Add(UiKit.Quad(p, "Duel_Trait", new Vector2(0f, ys - 0.035f), new Vector2(2f * HW, 0.002f), new Color(A.r, A.g, A.b, 0.4f), -1, 0.006f));

        duelTexts.Add(UiKit.Text(p, "Duel_Manches", new Vector2(-HW + 0.03f, y1), 0.0092f, UiKit.White, "MANCHES", TextAnchor.MiddleLeft, true));
        float right = HW;
        for (int i = RoundChoices.Length - 1; i >= 0; i--)
        {
            int v = RoundChoices[i];
            var b = MakeButton(p, new Vector2(right - 0.055f, y1), new Vector2(0.11f, 0.056f), v.ToString(), 0.0085f, () => { if (NetLobby.Instance) NetLobby.Instance.SetDuelRounds(v); });
            roundButtons.Insert(0, b);
            right -= 0.125f;
        }
        duelTexts.Add(UiKit.Text(p, "Duel_Arme", new Vector2(-HW + 0.03f, y2), 0.0092f, UiKit.White, "ARME PRINCIPALE", TextAnchor.MiddleLeft, true));
        right = HW;
        for (int i = PrimaryChoices.Length - 1; i >= 0; i--)
        {
            int v = i;
            float w = 0.10f + PrimaryChoices[i].Length * 0.022f;
            var b = MakeButton(p, new Vector2(right - w / 2f, y2), new Vector2(w, 0.056f), PrimaryChoices[i], 0.0078f, () => { if (NetLobby.Instance) NetLobby.Instance.SetDuelPrimary(v); });
            primaryButtons.Insert(0, b);
            right -= w + 0.03f;
        }
        foreach (var b in roundButtons) b.gameObject.SetActive(false);
        foreach (var b in primaryButtons) b.gameObject.SetActive(false);
        foreach (var t in duelTexts) t.gameObject.SetActive(false);
        foreach (var l in duelLines) l.gameObject.SetActive(false);
    }

    void RefreshDuelOptions(NetLobby lobby, bool host)
    {
        bool duel = lobby.IsDuel1v1;
        foreach (var t in duelTexts) t.gameObject.SetActive(duel);
        foreach (var l in duelLines) l.gameObject.SetActive(duel);
        for (int i = 0; i < roundButtons.Count; i++)
        {
            roundButtons[i].gameObject.SetActive(duel);
            roundButtons[i].selected = RoundChoices[i] == lobby.DuelRounds.Value;
            roundButtons[i].interactable = host || roundButtons[i].selected;
        }
        for (int i = 0; i < primaryButtons.Count; i++)
        {
            primaryButtons[i].gameObject.SetActive(duel);
            primaryButtons[i].selected = i == lobby.DuelPrimary.Value;
            primaryButtons[i].interactable = host || primaryButtons[i].selected;
        }
    }

    void ChooseSkin(int skin)
    {
        PlayerSkin.Selected = skin;
        if (NetLobby.Instance) NetLobby.Instance.SetSkin(skin);
        ShowSkinPreview();
    }

    // le personnage choisi, en pied à gauche du panneau du lobby, tourné vers le joueur (animation d'attente)
    void ShowSkinPreview()
    {
        int skin = PlayerSkin.Selected;
        for (int i = 0; i < skinButtons.Count; i++) skinButtons[i].selected = i == skin;
        if (previewSkin == skin && skinPreview) return;
        previewSkin = skin;
        if (skinPreview) Destroy(skinPreview);
        var a = PlayerSkin.Create(skin, lobbyPage);
        if (!a) return;
        skinPreview = a.gameObject;
        var t = a.transform;
        float s = 1f / Mathf.Max(0.01f, transform.lossyScale.x);
        t.localScale = Vector3.one * s;
        t.localRotation = Quaternion.Euler(0f, 160f, 0f);
        var world = lobbyPage.TransformPoint(new Vector3(-1.3f, 0f, 0.25f)); world.y = 0f;      // pieds au sol
        t.position = world;
    }

    // cartes de mode, dans l'ordre de NetLobby.ModeIndex (0 survie, 1 évasion, 2 duel)
    static readonly (string kind, string title, string desc)[] ModeCards =
    {
        ("COOP", "VAGUES INFINIES", "Bunker · 2 à 4 joueurs\nTenir le plus longtemps"),
        ("COOP", "ÉVASION", "Bunker · 2 à 4 joueurs\n4 niveaux + générateur"),
        ("DUEL", "1 CONTRE 1", ArenaMaps.Count + " cartes · 2 joueurs\nEn manches"),
    };
    MultiPage multiPage;
    bool localReady;
    int lastModeIndex = 0;
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
        // haut : choix du mode en 3 cartes (l'hôte clique, les autres voient la carte choisie) ;
        // milieu : les 4 places de l'équipe ; bas : état du lancement, QUITTER et le gros bouton PRÊT.
        p = lobbyPage = Panel("Page_Lobby", "BUNKER-OS  //  LOBBY", version);
        lobbyInfo = UiKit.Text(p, "Info", new Vector2(HW, 0.49f), 0.0068f, UiKit.Dim, "", TextAnchor.MiddleRight);
        float y = 0.49f;
        Section(p, ref y, "MODE DE JEU");
        for (int i = 0; i < ModeCards.Length; i++)
        {
            int k = i;
            var c = ModeCards[i];
            float x = (i - 1) * 0.455f;
            var b = MakeButton(p, new Vector2(x, 0.31f), new Vector2(0.43f, 0.215f), "", 0.005f, () => { if (NetLobby.Instance) NetLobby.Instance.SetMode(k); });
            UiKit.Text(b.transform, "Type", new Vector2(0f, 0.072f), 0.0066f, UiKit.Dim, c.kind, TextAnchor.MiddleCenter, true);
            b.label = UiKit.Text(b.transform, "Titre", new Vector2(0f, 0.014f), 0.0066f, UiKit.White, c.title, TextAnchor.MiddleCenter, true);
            UiKit.Text(b.transform, "Desc", new Vector2(0f, -0.058f), 0.0052f, UiKit.Dim, c.desc, TextAnchor.MiddleCenter);
            modeCards.Add(b);
        }

        y = 0.155f;
        Section(p, ref y, "ÉQUIPE");
        // choix du personnage (chacun pour soi), sur la ligne du titre ; l'aperçu en pied est à gauche du panneau
        UiKit.Text(p, "Perso_Titre", new Vector2(HW - 0.33f, 0.155f), 0.0068f, UiKit.Dim, "VOTRE PERSONNAGE", TextAnchor.MiddleRight, true);
        for (int i = 0; i < PlayerSkin.Count; i++)
        {
            int k = i;
            var b = MakeButton(p, new Vector2(HW - 0.033f - (PlayerSkin.Count - 1 - i) * 0.074f, 0.157f), new Vector2(0.066f, 0.052f), (i + 1).ToString(), 0.0075f, () => ChooseSkin(k));
            skinButtons.Add(b);
        }
        for (int i = 0; i < 4; i++)
        {
            float ry = 0.065f - i * 0.07f;
            var row = new PlayerRow();
            row.frame = UiKit.Quad(p, "Ligne_" + (i + 1), new Vector2(0f, ry), new Vector2(2f * HW, 0.058f), new Color(A.r, A.g, A.b, 0.05f), -1, 0.007f);
            row.dot = UiKit.Quad(p, "Couleur_" + (i + 1), new Vector2(-HW + 0.035f, ry), new Vector2(0.03f, 0.03f), UiKit.Dim, -1, 0.005f);
            row.name = UiKit.Text(p, "Nom_" + (i + 1), new Vector2(-HW + 0.075f, ry), 0.0092f, UiKit.White, "", TextAnchor.MiddleLeft, true);
            row.tag = UiKit.Text(p, "Role_" + (i + 1), new Vector2(-0.27f, ry), 0.0068f, UiKit.Dim, "", TextAnchor.MiddleLeft, true);
            row.skin = UiKit.Text(p, "Tenue_" + (i + 1), new Vector2(0.06f, ry), 0.0068f, UiKit.Dim, "", TextAnchor.MiddleLeft);
            row.state = UiKit.Text(p, "Etat_" + (i + 1), new Vector2(HW - 0.03f, ry), 0.0085f, UiKit.Dim, "", TextAnchor.MiddleRight, true);
            playerRows.Add(row);
        }

        // duel : réglages de l'hôte, à la place des 2 places vides (rangées 3 et 4) ; les autres joueurs les voient sans pouvoir les changer
        BuildDuelOptions(p);
        BuildMapSelector(p);        // duel : choix de la carte avec aperçu, à droite du panneau (MainMenuArenas.cs)

        lobbyCountdown = UiKit.Text(p, "Compte", new Vector2(0f, -0.325f), 0.0092f, A, "", TextAnchor.MiddleCenter, true);
        // hôte, duel seulement : lancer même sans adversaire (test)
        forceButton = MakeButton(p, new Vector2(0f, -0.41f), new Vector2(1.30f, 0.062f), "FORCER LE LANCEMENT", 0.0085f, () => { if (NetLobby.Instance) NetLobby.Instance.ToggleForceStart(); });
        forceButton.textColor = A;
        forceButton.gameObject.SetActive(false);
        var quit = MakeButton(p, new Vector2(-HW + 0.17f, -0.54f), new Vector2(0.34f, 0.10f), "◂  QUITTER", 0.0082f, LeaveLobby);
        quit.textColor = A;
        readyButton = MakeButton(p, new Vector2(0.19f, -0.54f), new Vector2(0.98f, 0.10f), "PRÊT", 0.0125f, ToggleReady);
        p.gameObject.SetActive(false);

        // retour au menu après une partie en réseau ou une déconnexion : page multijoueur avec la raison
        if (NetSession.Active && !NetSession.InGame && NetLobby.Instance) ShowMulti(MultiPage.Lobby);
        else if (NetSession.Active && !NetLobby.Instance) NetSession.Leave();     // [hotfix #157] session restée ouverte sans lobby : on la ferme
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
        if (page == MultiPage.Lobby) { localReady = false; lobbyEnteredAt = Time.unscaledTime; ShowSkinPreview(); }
        nextRefresh = 0f;
    }

    void Host()
    {
        if (NetSession.Busy) { StartCoroutine(WhenFree(Host, multiInfo)); return; }      // [hotfix #157] session précédente encore ouverte
        if (!NetSession.Host()) { multiInfo.text = NetSession.LastError ?? "Échec"; return; }
        ShowMulti(MultiPage.Lobby);
    }

    void Join(int i)
    {
        if (i >= gameIps.Count || string.IsNullOrEmpty(gameIps[i])) return;
        if (NetSession.Busy) { StartCoroutine(WhenFree(() => Join(i), joinInfo)); return; }
        if (!NetSession.Join(gameIps[i])) { joinInfo.text = NetSession.LastError ?? "Échec"; return; }
        ShowMulti(MultiPage.Lobby);
    }

    // [hotfix #157] une session réseau est encore ouverte ou se ferme (Netcode ferme à l'image suivante) : on la ferme, on attend
    // qu'elle le soit (3 s au plus) puis on relance HÉBERGER / REJOINDRE, au lieu d'afficher « Impossible d'ouvrir le port 7777 »
    System.Collections.IEnumerator WhenFree(System.Action retry, TextMesh info)
    {
        if (info) info.text = "Fermeture de la partie précédente…";
        NetSession.Leave();
        float until = Time.unscaledTime + 3f; int frames = 0;            // 3 s et 30 images au moins (une image peut être longue)
        while (NetSession.Busy && (Time.unscaledTime < until || frames++ < 30)) yield return null;
        if (NetSession.Busy) { if (info) info.text = "Le réseau est encore occupé, réessaie dans un instant"; yield break; }
        retry();
    }

    void ToggleReady()
    {
        if (!NetLobby.Instance) return;
        localReady = !localReady;
        readyClickTime = Time.unscaledTime;
        NetLobby.Instance.SetReady(localReady);
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
        bool mismatch = false;
        for (int i = 0; i < gameButtons.Count; i++)
        {
            bool on = i < games.Count;
            gameButtons[i].gameObject.SetActive(on);
            if (!on) { gameIps[i] = ""; continue; }
            var g = games[i];
            gameIps[i] = g.ip;
            bool same = !NetSession.CheckVersion || g.version == Application.version;
            gameButtons[i].interactable = same;
            string maxP = (g.mode != null && g.mode.Contains("1V1")) ? "2" : NetSession.MaxPlayers.ToString();
            string modeStr = string.IsNullOrEmpty(g.mode) ? "COOP" : g.mode;
            int colon = modeStr.IndexOf(':');
            if (colon > 0) modeStr = modeStr.Substring(0, colon).Trim();           // « DUEL 1V1 », « COOP » : le texte tient dans le bouton
            string host = g.name.ToUpper(); if (host.Length > 14) host = host.Substring(0, 14);
            gameButtons[i].label.text = same ? "REJOINDRE  " + host + "  " + g.players + "/" + maxP + " · " + modeStr
                                             : host + " · VERSION " + g.version + " ≠ " + Application.version;
            if (!same) mismatch = true;
        }
        joinInfo.text = games.Count == 0 ? "Recherche des parties sur le Wi-Fi…\nL'hôte doit avoir choisi HÉBERGER." : mismatch ? "Version différente de la partie : installe la même version sur les deux casques / PC." : "Vise une partie et valide pour la rejoindre.";
    }

    void RefreshLobby()
    {
        var lobby = NetLobby.Instance;
        if (!NetSession.Active) { ShowMulti(MultiPage.Choice); return; }
        if (!lobby)
        {
            lobbyInfo.text = "";
            lobbyCountdown.text = "Connexion à l'hôte…";
            // seulement si la connexion n'a JAMAIS abouti : au lancement, le lobby disparaît pendant que le casque charge la partie (long sur Quest), ce n'est pas un échec
            var nm = Unity.Netcode.NetworkManager.Singleton;
            if (!NetSession.IsServer && !NetSession.InGame && !(nm && nm.IsConnectedClient) && Time.unscaledTime - lobbyEnteredAt > 12f)
            {
                // l'hôte ne répond pas (paquets UDP bloqués) : on revient au menu avec la piste, au lieu d'attendre sans fin
                NetSession.Leave();
                NetSession.LastError = "L'hôte ne répond pas (UDP 7777). Pare-feu Windows : lancer Tools/multijoueur/autoriser_pare_feu_windows.bat. macOS : Réglages > Confidentialité > Réseau local > autoriser Unity.";
                ShowMulti(MultiPage.Choice);
                return;
            }
            foreach (var r in playerRows) SetRow(r, null, false, "", "", UiKit.Dim, UiKit.Dim);
            return;
        }
        bool host = NetSession.IsServer;
        int mode = lobby.ModeIndex.Value;
        lobbyInfo.text = host ? "VOUS HÉBERGEZ  ·  " + LanDiscovery.LocalIp() : "CONNECTÉ À L'HÔTE";
        for (int i = 0; i < modeCards.Count; i++)
        {
            // seul l'hôte choisit ; chez les autres, la carte choisie reste allumée et les autres sont éteintes
            modeCards[i].selected = i == mode;
            modeCards[i].interactable = host || i == mode;
        }
        // l'hôte a changé de mode : chacun doit se remettre PRÊT (le serveur a déjà remis les états à zéro)
        if (lastModeIndex != mode) { lastModeIndex = mode; localReady = false; }
        // un réglage du duel a changé (l'hôte a remis tout le monde « pas prêt ») : le bouton PRÊT suit l'état du serveur
        if (localReady && Time.unscaledTime > readyClickTime + 1.5f && !lobby.IsReady(NetSession.LocalId) && lobby.Countdown.Value < 0f) localReady = false;
        RefreshDuelOptions(lobby, host);
        RefreshMapSelector(lobby, host);
        readyButton.label.text = localReady ? "✓  PRÊT  ·  annuler" : "JE SUIS PRÊT";
        readyButton.selected = localReady;

        int slots = lobby.IsDuel1v1 ? 2 : NetSession.MaxPlayers;
        for (int i = 0; i < playerRows.Count; i++)
        {
            var r = playerRows[i];
            if (i >= slots) { SetRow(r, null, false, "", "", UiKit.Dim, UiKit.Dim); continue; }
            if (i >= lobby.Players.Count) { SetRow(r, null, true, "Place libre", "", UiKit.Dim, UiKit.Dim); continue; }
            var e = lobby.Players[i];
            bool me = e.clientId == NetSession.LocalId;
            bool isHost = e.clientId == Unity.Netcode.NetworkManager.ServerClientId;
            var col = NetPlayer.Colors[Mathf.Clamp(e.number, 1, NetPlayer.Colors.Length) - 1];
            string tag = isHost && me ? "HÔTE · VOUS" : isHost ? "HÔTE" : me ? "VOUS" : "";
            SetRow(r, col, true, lobby.NameOf(e).ToUpperInvariant(), tag, e.ready ? UiKit.Green : UiKit.Dim, UiKit.White);     // [hotfix #155] nom du compte / du casque
            r.state.text = e.ready ? "✓ PRÊT" : "EN ATTENTE";
            r.skin.text = PlayerSkin.Name(e.skin);
        }

        // FORCER LE LANCEMENT : l'hôte, en duel ; un second appui annule
        bool forced = lobby.ForceStart.Value;
        forceButton.gameObject.SetActive(host && lobby.IsDuel1v1 && !lobby.TooManyForDuel);
        forceButton.label.text = forced ? "LANCEMENT FORCÉ  ·  annuler" : lobby.Players.Count < lobby.minPlayers ? "FORCER LE LANCEMENT  ·  jouer seul" : "FORCER LE LANCEMENT";
        forceButton.selected = forced;
        float c = lobby.Countdown.Value;
        int missing = forced ? 0 : Mathf.Max(0, lobby.minPlayers - lobby.Players.Count);
        lobbyCountdown.color = c >= 0f ? A : lobby.TooManyForDuel ? UiKit.Red : UiKit.Dim;
        lobbyCountdown.text = lobby.TooManyForDuel ? "Le duel est limité à 2 joueurs (" + lobby.Players.Count + " dans le lobby) : choisis un mode coop"
                            : c >= 0f ? "LANCEMENT DANS " + Mathf.CeilToInt(c) + " S"
                            : missing > 0 ? "En attente " + (missing > 1 ? "de " + missing + " joueurs" : "d'un autre joueur") + "…"
                            : "La partie démarre quand tout le monde est prêt";
    }

    // une place de l'équipe : pastille de couleur, nom, rôle, état ; color null = pastille éteinte
    static void SetRow(PlayerRow r, Color? color, bool visible, string name, string tag, Color stateColor, Color nameColor)
    {
        r.frame.enabled = visible;
        r.dot.enabled = visible;
        r.dot.material.color = color ?? new Color(UiKit.Dim.r, UiKit.Dim.g, UiKit.Dim.b, 0.35f);
        r.name.text = name; r.name.color = color.HasValue ? nameColor : UiKit.Dim;
        // place libre : petit texte discret ; joueur : nom en gras
        r.name.fontStyle = color.HasValue ? FontStyle.Bold : FontStyle.Italic;
        r.name.characterSize = color.HasValue ? 0.0092f : 0.0072f;
        r.frame.material.color = new Color(A.r, A.g, A.b, color.HasValue ? 0.07f : 0.025f);
        r.tag.text = tag;
        r.skin.text = "";
        r.state.text = ""; r.state.color = stateColor;
    }
}
