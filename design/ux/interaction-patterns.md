# Bibliothèque de Patterns d'Interaction — Terminal BUNKER-OS (`interaction-patterns.md`)

> **Statut** : Approuvé
> **Auteur** : `ui-programmer` & `ux-designer`
> **Dernière mise à jour** : 2026-11-09
> **Plateformes Cibles** : Meta Quest 3 (VR 6DoF — Pointeur rayon `MenuPointer` + Gâchette) & PC Desktop (`DesktopMode` / `DesktopPlayer` — Souris libre + Clic gauche / Clavier)
> **Scripts de Référence** : `Assets/Scripts/UiKit.cs`, `Assets/Scripts/AmberMenu.cs`, `Assets/Scripts/PauseMenu.cs`, `Assets/Scripts/MainMenu.cs`, `Assets/Scripts/GameOverUI.cs`, `Assets/Scripts/MenuPointer.cs`, `Assets/Scripts/MenuButton.cs`

---

## 1. Architecture Globale des Interfaces BUNKER-OS

Tous les menus diégétiques de **The Last Escapor** (`MainMenu`, `PauseMenu`, `GameOverUI`) reposent sur un système procédural unifié construit en code C# via [`UiKit`](file:///Users/rahimtam/SAE501/Assets/Scripts/UiKit.cs), sans dépendance à des prefabs uGUI ou des fichiers `.uxml`.

### Principes fondamentaux :
1. **Placement World Space (Zéro texte collé à la caméra)** :
   - En jeu (`PauseMenu`, `GameOverUI`), le panneau apparaît dans le monde à `distance = 1.3f` mètre devant `head` (`Camera.main.transform`), abaissé de `0.10 m` sous les yeux (`head.position + fwd * 1.3f - Vector3.up * 0.10f`), orienté uniquement sur l'axe horizontal (`fwd.y = 0f`, `Quaternion.LookRotation(fwd)`), avec `scale = 0.8f`.
2. **Priorité d'affichage garantie (`UiKit.SetOnTop`)** :
   - L'activation de `UiKit.SetOnTop(true)` (`unity_GUIZTestMode = CompareFunction.Always`) combinée aux `renderQueue` `4490` à `4600` garantit que les panneaux World Space restent intégralement visibles et lisibles même si le joueur fait face à un mur ou un obstacle proche.
   - `Physics.SyncTransforms()` est systématiquement appelé à l'activation du panneau pour synchroniser les `BoxCollider` des boutons dans PhysX dès la première frame.

---

## 2. Catalogue des Composants Réutilisables

### 2.1. `TerminalPanel` (Cadre principal de page BUNKER-OS)
- **Implémentation** : `AmberMenu.Panel(...)` / `GameOverUI.BuildPanel(...)`
- **Dimensions locales** : `1.52 m x 1.28 m` (`AmberMenu`) ou `1.52 m x 1.24 m` (`GameOverUI`), demi-largeur utile `HW = 0.68 m`.
- **Structure d'empilement** :
  - `Fond_Opaque` : `UiKit.Quad` noir `rgba(0, 0, 0, 0.97)` à `z = +0.020f`, `renderQueue = 4490`.
  - `Fond_Page` : `UiKit.Quad` teinté (`PanelCol` ambre, `UiKit.PanelRed` en Défaite, `UiKit.PanelGreen` en Victoire) à `z = +0.012f`, `renderQueue = 4495`.
  - `Cadre_Externe` : `UiKit.Frame` (`t = 0.004f`) + `UiKit.Brackets` (`len = 0.07f`, `t = 0.008f` sur `PanelSize + (0.03, 0.03)`).
  - `Bande_Haut` : Bandeau plein en haut du panneau (`hauteur = 0.084 m`) portant le titre système `BUNKER-OS  //  ...` à gauche (`x = -0.73f`, `Ink`, gras) et le contexte/version à droite (`x = +0.73f`, `Ink`, gras).
  - `Balayage` : `UiKit.Scanlines` (`Texture2D` `2x256` en `FilterMode.Point`, lignes paires à `alpha = 0.22f`), `renderQueue = 4520`.

### 2.2. `BigButton` / `ActionButton` (`MenuButton` + `BoxCollider`)
- **Implémentation** : `AmberMenu.BigButton(...)` / `GameOverUI.MakeActionButton(...)` / [`MenuButton.cs`](file:///Users/rahimtam/SAE501/Assets/Scripts/MenuButton.cs)
- **Dimensions locales** :
  - Standard (4 boutons) : `1.20 m x 0.124 m` (`charSize = 0.0128f`)
  - Compact 2 boutons (`GameOverUI`) : `1.20 m x 0.110 m` (`charSize = 0.0113f`, pas vertical `0.140 m`)
  - Compact 3 boutons (`GameOverUI`) : `1.20 m x 0.092 m` (`charSize = 0.0105f`, pas vertical `0.110 m`)
- **États visuels & Haptique** :
  - `SafetyLock / Disabled` (`interactable = false`) : fond `alpha = 0.04f`, texte grisé `#736B5C`, flèche `▶` masquée.
  - `Default` (`interactable = true`, hors survol) : fond `alpha = 0.10f`, texte `UiKit.White` (`#E0EBE6`), flèche `▶` masquée.
  - `Hovered` (rayon VR ou curseur souris sur le `BoxCollider`) : fond `alpha = 0.30f`, texte `accent` (`UiKit.Amber` `#FFBF45`), flèche géométrique `▶` affichée à `x = -0.64f`, vibration haptique VR (`0.2f`, `0.03s`).
  - `Clicked` : impulsion `glow = 1.0f` (`alpha = 0.80f` décroissant vers `0f` à `4f * Time.unscaledDeltaTime`), vibration haptique VR (`0.5f`, `0.06s`), exécution de `onClick`.

### 2.3. `BilanBox` & `StatRow` (Tableau récapitulatif à deux colonnes)
- **Implémentation** : `GameOverUI.PopulateStats(...)` & `GameOverUI.StatRow(...)`
- **Structure** :
  - Sous-cadre `1.36 m x 0.410 m` (`Fond_Bilan` noir `alpha = 0.35f` + `UiKit.Frame` `t = 0.003f`).
  - En-tête de section aligné à gauche (`x = -0.64f`, `y = +0.215f`, `charSize = 0.0075f`, gras) souligné par un filet horizontal (`1.28 m x 0.002 m` à `y = +0.185f`).
  - Lignes de statistiques (`StatRow`) espacées de `0.065 m` (`y = +0.140f` à `-0.120f`) :
    - Colonne gauche (`x = -0.64f`, `TextAnchor.MiddleLeft`, `charSize = 0.0082f`, `UiKit.Dim`) : intitulé de la métrique.
    - Colonne droite (`x = +0.64f`, `TextAnchor.MiddleRight`, `charSize = 0.0085f`, gras, `UiKit.White` ou `UiKit.Amber` pour mettre en évidence un record) : valeur formatée.

### 2.4. `SegmentedSlider` & `ChoiceRow` (Contrôles de paramètres)
- **Implémentation** : `AmberMenu.Slider(...)` & `AmberMenu.Choice(...)`
- **Usage** : Réglage du volume (`Musique`, `Effets`), de la difficulté (`FACILE`, `NORMAL`, `DIFFICILE`) et des options de confort VR / souris PC.

---

## 3. Pattern d'Entrée Multi-Plateforme (`MenuPointer` + `DesktopPlayer`)

| Modalité | Mécanisme de visée | Validation | Sécurité anti-missclick |
|---|---|---|---|
| **VR Meta Quest 3** | Rayon `LineRenderer` par manette (`Left Controller`, `Right Controller`) dans [`MenuPointer`](file:///Users/rahimtam/SAE501/Assets/Scripts/MenuPointer.cs) | Gâchette de la main qui survole (`VRInput.Trigger(node)`) sur front montant (`down && !wasDown`) | Délai d'armement `0.5 s` à l'activation de `MenuPointer` + verrou `SafetyLock` (`1.0 s`) sur `GameOverUI`. |
| **PC Desktop (`DesktopMode.On`)** | Déverrouillage automatique du curseur (`LockCursor(false)`) + orientation exponentielle de la caméra vers le panneau dans [`DesktopPlayer`](file:///Users/rahimtam/SAE501/Assets/Scripts/Desktop/DesktopPlayer.cs) + `ScreenPointToRay` sous le curseur | Clic gauche souris (`Mouse.current.leftButton.wasPressedThisFrame`) | Inhibition de `LockCursor(true)` tant que `PauseMenu.Paused` ou `GameOverUI.PanelOpen` ou `DesktopPlayer.GameOver` est actif + verrou `SafetyLock` (`1.0 s`). |
| **Éditeur sans manettes** | Rayon central selon `head.forward` | Touche `Espace` (`VRInput.KeyboardSpace()`) | Délai d'armement `0.5 s` + `SafetyLock` (`1.0 s`). |
