using System.Collections.Generic;
using UnityEngine;

// Lobby, mode duel : choix de la CARTE (ArenaMaps) dans un panneau à droite du lobby, avec l'aperçu de la carte, son nom et sa description.
// L'hôte choisit (NetLobby.SetDuelMap) ; chez le client, la carte choisie reste allumée et les autres sont éteintes (NetworkVariable DuelMap).
// Les aperçus sont des captures faites dans l'éditeur (SAE501 → Arènes 1v1 → Captures), rangées dans Resources/ArenaPreviews.
public partial class MainMenu
{
    Transform mapPanel;
    Renderer mapPreview;
    TextMesh mapName, mapKind, mapDesc, mapNoPreview, mapHint;
    readonly List<MenuButton> mapButtons = new List<MenuButton>();
    int shownMap = -1;

    static readonly Vector2 MapPanelSize = new Vector2(0.78f, 1.0f);

    void BuildMapSelector(Transform lobby)
    {
        var p = mapPanel = new GameObject("Panneau_Carte").transform;
        p.SetParent(lobby, false);
        p.localPosition = new Vector3(PanelSize.x / 2f + 0.06f + MapPanelSize.x / 2f, 0.04f, 0f);

        UiKit.Quad(p, "Fond_Carte", Vector2.zero, MapPanelSize, PanelCol, 2990, 0.012f);
        UiKit.Frame(p, Vector2.zero, MapPanelSize, 0.004f, new Color(A.r, A.g, A.b, 0.4f), -1, 0.006f);
        UiKit.Brackets(p, Vector2.zero, MapPanelSize + new Vector2(0.03f, 0.03f), 0.06f, 0.008f, A);
        float top = MapPanelSize.y / 2f;
        UiKit.Quad(p, "Bande", new Vector2(0f, top - 0.044f), new Vector2(MapPanelSize.x, 0.088f), new Color(A.r, A.g, A.b, 0.92f), -1, 0.006f);
        UiKit.Text(p, "Bande_G", new Vector2(-MapPanelSize.x / 2f + 0.03f, top - 0.044f), 0.0085f, Ink, "CARTE DU DUEL", TextAnchor.MiddleLeft, true);

        // aperçu 16:9
        var size = new Vector2(0.70f, 0.394f);
        float py = top - 0.13f - size.y / 2f;
        mapPreview = UiKit.Quad(p, "Apercu", new Vector2(0f, py), size, Color.white, -1, 0.004f);
        UiKit.Frame(p, new Vector2(0f, py), size + new Vector2(0.01f, 0.01f), 0.003f, new Color(A.r, A.g, A.b, 0.7f), -1, 0.003f);
        mapNoPreview = UiKit.Text(p, "Apercu_Absent", new Vector2(0f, py), 0.0065f, UiKit.Dim, "APERÇU INDISPONIBLE", TextAnchor.MiddleCenter, true);

        float y = py - size.y / 2f - 0.05f;
        mapKind = UiKit.Text(p, "Type", new Vector2(-0.35f, y), 0.0062f, UiKit.Dim, "", TextAnchor.MiddleLeft, true);
        mapName = UiKit.Text(p, "Nom", new Vector2(-0.35f, y - 0.065f), 0.011f, A, "", TextAnchor.MiddleLeft, true);
        mapDesc = UiKit.Text(p, "Desc", new Vector2(-0.35f, y - 0.155f), 0.0058f, UiKit.White, "", TextAnchor.MiddleLeft);

        // une case par carte
        int n = ArenaMaps.Count;
        float w = (MapPanelSize.x - 0.08f - (n - 1) * 0.02f) / n;
        float by = -MapPanelSize.y / 2f + 0.115f;
        for (int i = 0; i < n; i++)
        {
            int k = i;
            float x = -MapPanelSize.x / 2f + 0.04f + w / 2f + i * (w + 0.02f);
            var b = MakeButton(p, new Vector2(x, by), new Vector2(w, 0.07f), ArenaMaps.Get(i).name, 0.005f, () => { if (NetLobby.Instance) NetLobby.Instance.SetDuelMap(k); });
            mapButtons.Add(b);
        }
        mapHint = UiKit.Text(p, "Aide", new Vector2(0f, -MapPanelSize.y / 2f + 0.04f), 0.0052f, UiKit.Dim, "", TextAnchor.MiddleCenter);
        p.gameObject.SetActive(false);
    }

    void RefreshMapSelector(NetLobby lobby, bool host)
    {
        if (!mapPanel) return;
        bool duel = lobby.IsDuel1v1;
        mapPanel.gameObject.SetActive(duel);
        if (!duel) { shownMap = -1; return; }
        int map = Mathf.Clamp(lobby.DuelMap.Value, 0, ArenaMaps.Count - 1);
        for (int i = 0; i < mapButtons.Count; i++)
        {
            mapButtons[i].selected = i == map;
            mapButtons[i].interactable = host || i == map;
        }
        mapHint.text = host ? "Vous choisissez la carte : l'adversaire la voit ici" : "Carte choisie par l'hôte";
        ShowMap(map);
    }

    void ShowMap(int map)
    {
        if (shownMap == map) return;
        shownMap = map;
        var m = ArenaMaps.Get(map);
        mapName.text = m.name;
        mapKind.text = m.kind + "  ·  CARTE " + (map + 1) + "/" + ArenaMaps.Count;
        mapDesc.text = m.desc;
        var tex = ArenaMaps.Preview(map);
        mapPreview.material.mainTexture = tex;
        mapPreview.enabled = tex;
        mapNoPreview.gameObject.SetActive(!tex);
    }

#if UNITY_EDITOR
    /// <summary>Éditeur seulement (capture du lobby, ArenaCaptures) : construit le menu hors mode Play et affiche le lobby en mode duel,
    /// vu par l'hôte, carte <paramref name="map"/> choisie, sans réseau.</summary>
    public void EditorPreviewDuelLobby(int map)
    {
        Start();
        mainPage.gameObject.SetActive(false);
        lobbyPage.gameObject.SetActive(true);
        multiPage = MultiPage.Lobby;
        for (int i = 0; i < modeCards.Count; i++) { modeCards[i].selected = i == 2; modeCards[i].interactable = true; }
        foreach (var t in duelTexts) t.gameObject.SetActive(true);
        foreach (var l in duelLines) l.gameObject.SetActive(true);
        for (int i = 0; i < roundButtons.Count; i++) { roundButtons[i].gameObject.SetActive(true); roundButtons[i].selected = RoundChoices[i] == NetDuel.DefaultRounds; }
        for (int i = 0; i < primaryButtons.Count; i++) { primaryButtons[i].gameObject.SetActive(true); primaryButtons[i].selected = i == 2; }
        for (int i = 0; i < playerRows.Count; i++)
        {
            if (i == 0) { SetRow(playerRows[i], NetPlayer.Colors[0], true, "JOUEUR 1", "HÔTE · VOUS", UiKit.Green, UiKit.White); playerRows[i].state.text = "✓ PRÊT"; }
            else if (i == 1) { SetRow(playerRows[i], NetPlayer.Colors[1], true, "JOUEUR 2", "", UiKit.Dim, UiKit.White); playerRows[i].state.text = "EN ATTENTE"; }
            else SetRow(playerRows[i], null, false, "", "", UiKit.Dim, UiKit.Dim);
        }
        lobbyInfo.text = "VOUS HÉBERGEZ  ·  192.168.1.20";
        lobbyCountdown.text = "La partie démarre quand tout le monde est prêt";
        readyButton.label.text = "✓  PRÊT  ·  annuler"; readyButton.selected = true;
        forceButton.gameObject.SetActive(true);
        mapPanel.gameObject.SetActive(true);
        for (int i = 0; i < mapButtons.Count; i++) { mapButtons[i].selected = i == map; mapButtons[i].interactable = true; }
        mapHint.text = "Vous choisissez la carte : l'adversaire la voit ici";
        shownMap = -1;
        ShowMap(map);
    }
#endif
}
