# Spécification de Design Visuel : Menu de Fin de Partie — Terminal BUNKER-OS (`game-over-menu`)

> **Phase `/team-ui`** : Phase 2 — Visual Design (`art-director`)
> **Projet** : The Last Escapor (`Unity 6000.6.0f1` URP — VR Meta Quest 3 + PC Desktop `feature/portage-pc`)
> **Spécification UX source** : `design/ux/game-over-menu.md` (Statut : `APPROVED`)
> **Références Code & Palette** : `project.yaml`, `Assets/Scripts/UiKit.cs`, `Assets/Scripts/AmberMenu.cs`, `Assets/Scripts/PauseMenu.cs`, `Assets/Scripts/MenuButton.cs`, `Assets/Scripts/MenuPointer.cs`, `Assets/Scripts/GameOverUI.cs`
> **Note Art Bible** : `design/art-bible.md` absent — alignement strict sur la charte procédurale CRT « BUNKER-OS » établie dans `UiKit.cs`, `AmberMenu.cs` et la section `palette.ui` de `project.yaml`.

---

## 1. Identité Visuelle BUNKER-OS & Bichromie Thématique

### 1.1 Philosophie esthétique : Terminal Rétro-Industriel Procédural (Zéro Asset Externe)
Toute l'interface de **The Last Escapor** repose sur l'esthétique diégétique d'un système d'exploitation de bunker souterrain (**BUNKER-OS**) généré intégralement par code via `UiKit` (quads primitifs, cadres fins, coins en équerre, texture de balayage CRT `2x256` en `FilterMode.Point`).

Contrairement à l'ancien écran de fin qui projetait du texte flottant sur un fond noir collé aux yeux, le nouveau `GameOverUI` matérialise un **moniteur de diagnostic tactique en World Space** :
- **Dimensions locales** : `PanelSize = new Vector2(1.52f, 1.24f)` (`HW = 0.68f` de demi-largeur utile).
- **Échelle dans le monde** : `scale = 0.8f` (dimensions réelles dans l'espace 3D : `1.216 m` de large par `0.992 m` de haut).
- **Placement spatial** : posé à `distance = 1.3f` mètre devant `head` (`Camera.main.transform`), abaissé de `0.10 m` sous la hauteur des yeux (`head.position + fwd * 1.3f - Vector3.up * 0.1f`), orienté exclusivement sur l'axe horizontal (`fwd.y = 0f`, `Quaternion.LookRotation(fwd)`).

---

### 1.2 Palette Chromatique Exacte (Thème Défaite vs Thème Victoire)

La bichromie thématique permet au joueur d'identifier l'issue de la partie en moins de `200 ms`, tout en conservant la grammaire visuelle de `MainMenu` et `PauseMenu`.

#### A. Palette Commune (Structure, Fonds & Textes Neutres)

| Token / Constante Code | Valeur `Color(r, g, b, a)` | Hexadécimal | Rôle visuel |
|---|---|---|---|
| `GameOverFade` (`black`) | `new Color(0f, 0f, 0f, 0.78f)` | `#000000` (78 %) | Voile d'assombrissement `HeadOverlay` derrière le terminal (isole la scène sans supprimer la perception d'espace). |
| `Fond_Opaque` | `new Color(0f, 0f, 0f, 0.97f)` | `#000000` (97 %) | Plaque arrière occultante du terminal (bloque toute lumière ou géométrie derrière le panneau). |
| `AmberMenu.Ink` | `new Color(0.06f, 0.04f, 0.015f, 1f)` | `#0F0A04` | Texte sombre inversé sur le bandeau supérieur plein (`Bande_G`, `Bande_D`). |
| `UiKit.White` | `new Color(0.88f, 0.92f, 0.90f, 1f)` | `#E0EBE6` | Sous-titre narratif, valeurs numériques du bilan et libellés des boutons au repos. |
| `UiKit.Dim` | `new Color(0.47f, 0.59f, 0.51f, 1f)` | `#789682` | Libellés de gauche du `BILAN DE MISSION` et ligne d'aide contextuelle en pied de page. |
| `UiKit.Amber` | `new Color(1.00f, 0.75f, 0.27f, 1f)` | `#FFBF45` | Accent d'interactivité des boutons (`MenuButton.accent`), flèche `▶`, et badge `"NOUVEAU RECORD !"`. |
| `Cadre_Bilan_Fond` | `new Color(0f, 0f, 0f, 0.38f)` | `#000000` (38 %) | Fond assombri du sous-cadre central `BILAN DE MISSION` pour détacher les 5 lignes de statistiques. |

#### B. Thème 1 — DÉFAITE (`BUNKER-OS  //  ALERTE CRITIQUE` — Mode Évasion & Mode Survie)

| Élément Visuel | Source / Constante | Valeur `Color(r, g, b, a)` | Hexadécimal | Détails d'application |
|---|---|---|---|---|
| **Fond de page (`Fond_Page`)** | `UiKit.PanelRed` | `new Color(0.05f, 0.024f, 0.024f, 0.90f)` | `#0D0606` (90 %) | Teinte bordeaux très sombre typique d'un moniteur CRT en état d'alerte. |
| **Bandeau supérieur (`Bande_Haut`)** | `UiKit.Red` (alpha `0.92f`) | `new Color(1.00f, 0.27f, 0.24f, 0.92f)` | `#FF453D` (92 %) | Bandeau plein en haut du terminal (`1.52 m x 0.084 m` à `y = +0.578`). |
| **Cadre externe (`Cadre_Externe`)** | `UiKit.Red` (alpha `0.45f`) | `new Color(1.00f, 0.27f, 0.24f, 0.45f)` | `#FF453D` (45 %) | Filet périmétrique `t = 0.004f` autour de `PanelSize`. |
| **Équerres de coin (`Brackets`)** | `UiKit.Red` (alpha `1.00f`) | `new Color(1.00f, 0.27f, 0.24f, 1.00f)` | `#FF453D` | 4 coins en équerre `len = 0.07f`, `t = 0.008f` sur `PanelSize + (0.03, 0.03)`. |
| **Sur-titre (`Sur_Titre`)** | `UiKit.Amber` | `new Color(1.00f, 0.75f, 0.27f, 1.00f)` | `#FFBF45` | `UiKit.Spaced("SYSTÈME DÉFAILLANT")` (`S Y S T È M E   D É F A I L L A N T`). |
| **Titre principal (`Titre_Principal`)** | `UiKit.Red` | `new Color(1.00f, 0.27f, 0.24f, 1.00f)` | `#FF453D` | `"VOUS ÊTES MORT"` en gras (`charSize = 0.025f`). |
| **Cadre & En-tête Bilan** | `UiKit.Amber` | `new Color(1.00f, 0.75f, 0.27f, 1.00f)` | `#FFBF45` | Titre `"BILAN DE MISSION"` (`100 %`), filet séparateur et cadre `Cadre_Bilan` (`alpha = 0.40f`). |
| **Boutons (`MenuButton`)** | `UiKit.Amber` | `new Color(1.00f, 0.75f, 0.27f, 1.00f)` | `#FFBF45` | Cadre `alpha = 0.60f`, fond `alpha = 0.10f` (repos) / `0.30f` (survol), flèche `▶` en ambre. |

#### C. Thème 2 — VICTOIRE (`BUNKER-OS  //  EXTRACTION` — Mode Évasion Niveau 4)

| Élément Visuel | Source / Constante | Valeur `Color(r, g, b, a)` | Hexadécimal | Détails d'application |
|---|---|---|---|---|
| **Fond de page (`Fond_Page`)** | `UiKit.PanelGreen` | `new Color(0.024f, 0.055f, 0.04f, 0.88f)` | `#060E0A` (88 %) | Teinte vert phosphoré sombre d'un terminal de bunker alimenté par le générateur. |
| **Bandeau supérieur (`Bande_Haut`)** | `UiKit.Green` (alpha `0.92f`) | `new Color(0.35f, 1.00f, 0.55f, 0.92f)` | `#59FF8C` (92 %) | Bandeau plein en haut du terminal (`1.52 m x 0.084 m` à `y = +0.578`). |
| **Cadre externe (`Cadre_Externe`)** | `UiKit.Green` (alpha `0.45f`) | `new Color(0.35f, 1.00f, 0.55f, 0.45f)` | `#59FF8C` (45 %) | Filet périmétrique `t = 0.004f` autour de `PanelSize`. |
| **Équerres de coin (`Brackets`)** | `UiKit.Green` (alpha `1.00f`) | `new Color(0.35f, 1.00f, 0.55f, 1.00f)` | `#59FF8C` | 4 coins en équerre `len = 0.07f`, `t = 0.008f` sur `PanelSize + (0.03, 0.03)`. |
| **Sur-titre (`Sur_Titre`)** | `UiKit.Green` | `new Color(0.35f, 1.00f, 0.55f, 1.00f)` | `#59FF8C` | `UiKit.Spaced("SIGNAL TRANSMIS")` (`S I G N A L   T R A N S M I S`). |
| **Titre principal (`Titre_Principal`)** | `GameOverUI.victoryColor` | `new Color(1.00f, 0.80f, 0.35f, 1.00f)` | `#FFCC59` | `"VOUS AVEZ ÉCHAPPÉ"` en or lumineux gras (`charSize = 0.025f`). |
| **Cadre & En-tête Bilan** | `UiKit.Green` | `new Color(0.35f, 1.00f, 0.55f, 1.00f)` | `#59FF8C` | Titre `"BILAN DE MISSION"` (`100 %`), filet séparateur et cadre `Cadre_Bilan` (`alpha = 0.40f`). |
| **Boutons (`MenuButton`)** | `UiKit.Amber` | `new Color(1.00f, 0.75f, 0.27f, 1.00f)` | `#FFBF45` | Conserve `UiKit.Amber` comme couleur universelle d'action cliquable BUNKER-OS. |

---

## 2. Spécification Typographique & Grille de Coordonnées

Tous les textes sont construits via `UiKit.Text(...)` qui utilise la police système intégrée `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` avec `fontSize = 64` pour une netteté maximale en VR, l'échelle réelle étant pilotée par `characterSize` (en mètres dans le repère local du panneau).

| Nom GameObject | Coordonnées locales `(x, y)` | `charSize` | `FontStyle` | `TextAnchor` / `TextAlignment` | Couleur (Défaite / Victoire) | Texte exact |
|---|---|---|---|---|---|---|
| `Bande_G` | `(-0.730f, +0.578f)` | `0.0085f` | `Bold` | `MiddleLeft` / `Left` | `Ink` (`#0F0A04`) | Défaite : `"BUNKER-OS  //  ALERTE CRITIQUE"`<br>Victoire : `"BUNKER-OS  //  EXTRACTION"` |
| `Bande_D` | `(+0.730f, +0.578f)` | `0.0085f` | `Bold` | `MiddleRight` / `Right` | `Ink` (`#0F0A04`) | Survie : `"MODE SURVIE"`<br>Évasion : `"MODE ÉVASION"` |
| `Sur_Titre` | `(0.000f, +0.485f)` | `0.0095f` | `Bold` | `MiddleCenter` / `Center` | `UiKit.Amber` / `UiKit.Green` | Défaite : `UiKit.Spaced("SYSTÈME DÉFAILLANT")`<br>Victoire : `UiKit.Spaced("SIGNAL TRANSMIS")` |
| `Titre_Principal` | `(0.000f, +0.395f)` | `0.0250f` | `Bold` | `MiddleCenter` / `Center` | `UiKit.Red` / `victoryColor` (`#FFCC59`) | Défaite : `"VOUS ÊTES MORT"`<br>Victoire : `"VOUS AVEZ ÉCHAPPÉ"` |
| `Sous_Titre` | `(0.000f, +0.310f)` | `0.0085f` | `Normal` | `MiddleCenter` / `Center` | `UiKit.White` (`#E0EBE6`) | Évasion Mort : `"Le signal n'a pas été envoyé..."`<br>Survie Mort : `"Vous avez succombé à la horde."`<br>Victoire : `"Le générateur est en marche."` |
| `Titre_Bilan` | `(-0.640f, +0.215f)` | `0.0075f` | `Bold` | `MiddleLeft` / `Left` | `UiKit.Amber` / `UiKit.Green` | `"BILAN DE MISSION"` |
| `Stat_Lbl_1` | `(-0.640f, +0.140f)` | `0.0082f` | `Normal` | `MiddleLeft` / `Left` | `UiKit.Dim` (`#789682`) | `"Zombies éliminés"` |
| `Stat_Val_1` | `(+0.640f, +0.140f)` | `0.0085f` | `Bold` | `MiddleRight` / `Right` | `UiKit.White` (`#E0EBE6`) | `"{Kills}"` + (`MeleeKills > 0 ? "  (dont " + MeleeKills + " à la mêlée)" : ""`) |
| `Stat_Lbl_2` | `(-0.640f, +0.075f)` | `0.0082f` | `Normal` | `MiddleLeft` / `Left` | `UiKit.Dim` (`#789682`) | `"Tirs à la tête"` |
| `Stat_Val_2` | `(+0.640f, +0.075f)` | `0.0085f` | `Bold` | `MiddleRight` / `Right` | `UiKit.White` (`#E0EBE6`) | `"{Mathf.RoundToInt(HeadShotPercent)} %  ({HeadShots} sur {Shots} tirs)"` |
| `Stat_Lbl_3` | `(-0.640f, +0.010f)` | `0.0082f` | `Normal` | `MiddleLeft` / `Left` | `UiKit.Dim` (`#789682`) | `"Planches posées"` |
| `Stat_Val_3` | `(+0.640f, +0.010f)` | `0.0085f` | `Bold` | `MiddleRight` / `Right` | `UiKit.White` (`#E0EBE6`) | `"{PlanksPlaced}"` |
| `Stat_Lbl_4` | `(-0.640f, -0.055f)` | `0.0082f` | `Normal` | `MiddleLeft` / `Left` | `UiKit.Dim` (`#789682`) | Évasion : `"Niveau atteint"`<br>Survie : `"Vague atteinte"` |
| `Stat_Val_4` | `(+0.640f, -0.055f)` | `0.0085f` | `Bold` | `MiddleRight` / `Right` | `UiKit.White` (`#E0EBE6`) | Évasion : `"{Mathf.Max(1, level)} / {maxLevel}"`<br>Survie : `"{wave}"` + (`!NewRecord && BestWave > 0 ? "  (record : vague " + BestWave + ")" : ""`) |
| `Stat_Lbl_5` | `(-0.640f, -0.120f)` | `0.0082f` | `Normal` | `MiddleLeft` / `Left` | `UiKit.Dim` (`#789682`) | Évasion : `"Temps total"`<br>Survie : `"Temps de survie"` |
| `Stat_Val_5` | `(+0.640f, -0.120f)` | `0.0085f` | `Bold` | `MiddleRight` / `Right` | `EndlessRecord.NewRecord ? UiKit.Amber : UiKit.White` | `"{GameStats.FormatTime(GameStats.Time)}"` + (`EndlessRecord.NewRecord ? "  ·  NOUVEAU RECORD !" : ""`) |
| `Bouton.Texte` | `(-0.548f, 0.000f)` (relatif au bouton) | `0.0105f` (3 btn)<br>`0.0113f` (2 btn) | `Bold` | `MiddleLeft` / `Left` | Repos : `UiKit.White`<br>Survol : `UiKit.Amber` | Voir section 4 (libellés des boutons) |
| `Bouton.Fleche` | `(-0.640f, 0.000f)` (relatif au bouton) | `0.0090f` | `Bold` | `MiddleCenter` / `Center` | `UiKit.Amber` (`#FFBF45`) | `"▶"` (actif uniquement au survol) |
| `Aide_Bas` | `(0.000f, -0.568f)` | `0.0068f` | `Normal` | `MiddleCenter` / `Center` | `UiKit.Dim` (`#789682`) | VR : `"Visez un bouton  ·  gâchette pour valider"`<br>PC : `"Souris : visez un bouton  ·  clic gauche pour valider"` |

---

## 3. Architecture des Couches, Profondeur Locale `Z` & `renderQueue` (URP)

Pour éviter tout problème de tri de transparence (Z-fighting ou texte masqué par les quads de fond ou par `GameOverFade`) dans Universal Render Pipeline (`Shader.Find("UI/Default")`), l'ordre de rendu suit exactement la convention de `PauseMenu.cs` avec `UiKit.SetOnTop(true)` :

| Couche | Élément | Position `z` locale | `renderQueue` | `ZTest` (`unity_GUIZTestMode`) | Rôle technique |
|---|---|---|---|---|---|
| **0. Voile Caméra** | `GameOverFade` (`HeadOverlay` sur `head`) | `nearClipPlane + 0.03f` (sur `head`) | `4100` | `Always` (via `HeadOverlay`) | Assombrit la scène 3D derrière le panneau (`alpha = 0.78f`), dessiné **avant** le terminal. |
| **1. Plaque Arrière** | `Fond_Opaque` (`1.52m x 1.24m`) | `+0.020f` | `4490` | `Always` (`UiKit.SetOnTop(true)`) | Noir `97 %` garantissant que le décor ne transparaît pas derrière le panneau. |
| **2. Fond Thématique** | `Fond_Page` (`1.52m x 1.24m`) | `+0.012f` | `4495` | `Always` | `UiKit.PanelRed` (Défaite) ou `UiKit.PanelGreen` (Victoire). |
| **3. Fond Encadré Bilan** | `Cadre_Bilan_Fond` (`1.36m x 0.41m`) | `+0.010f` | `4498` | `Always` | Noir `38 %` sous le tableau des statistiques (`y = +0.045f`). |
| **4. Fonds de Boutons** | `Bouton_Fond` (`MenuButton.background`) | `+0.008f` | `4500` | `Always` | Fond interactif de chaque bouton (`1.20m x 0.092m` ou `0.110m`). |
| **5. Bandeau & Cadres** | `Bande_Haut`, `Cadre_Externe`, `Cadre_Bilan`, `Trait_Bilan`, `Bouton_Cadre`, `Coins` | `+0.004f` à `+0.006f` | `4505` | `Always` | Toutes les lignes géométriques, bandeaux et équerres de coin. |
| **6. Balayage CRT** | `Balayage` (`UiKit.Scanlines`) | `0.000f` | `4520` | `Always` | Trame horizontale rétro-CRT appliquée sur toute la surface `1.52m x 1.24m`. |
| **7. Textes** | Tous les `TextMesh` (`Bande_G/D`, titres, stats, labels boutons, `▶`, aide) | `0.000f` | `4600` | `Always` | Dessinés en dernier pour une lisibilité parfaite sans aucun écrêtage par les fonds. |
| **8. Rayons VR** | `Rayon_Gauche`, `Rayon_Droit` (`MenuPointer` `LineRenderer`) | Monde | `4550` | `Always` | Faisceaux ambre (`width: 6mm -> 2mm`, `alpha: 0.9 -> 0.1`) partant des manettes VR jusqu'au bouton visé. |

---

## 4. États Visuels & Géométrie des Boutons (`MenuButton`)

### 4.1 Dimensions et Positions selon la Configuration
- **Configuration 3 Boutons** (Mode Évasion, Défaite au niveau `>= 2`) :
  - Taille unitaire : `size = new Vector2(1.20f, 0.092f)` ; `BoxCollider.size = new Vector3(1.20f, 0.092f, 0.03f)` ; pas vertical : `0.110 m` (espace libre inter-bouton : `18 mm`).
  - `Btn_Reprendre` : `pos = (0f, -0.235f)` — Label : `"REPRENDRE AU NIVEAU " + resumeLevel`
  - `Btn_Recommencer` : `pos = (0f, -0.345f)` — Label : `"RECOMMENCER (NIVEAU 1)"`
  - `Btn_Menu` : `pos = (0f, -0.455f)` — Label : `"MENU PRINCIPAL"`
- **Configuration 2 Boutons** (Mode Évasion Niveau 1, Mode Évasion Victoire, ou Mode Survie) :
  - Taille unitaire : `size = new Vector2(1.20f, 0.110f)` ; `BoxCollider.size = new Vector3(1.20f, 0.110f, 0.03f)` ; pas vertical : `0.140 m` (espace libre inter-bouton : `30 mm`).
  - `Btn_Recommencer` : `pos = (0f, -0.275f)` — Label : `"REJOUER"` (Défaite Niv. 1 ou Survie) / `"NOUVELLE PARTIE"` (Victoire)
  - `Btn_Menu` : `pos = (0f, -0.415f)` — Label : `"MENU PRINCIPAL"`

### 4.2 Table des États Visuels de `MenuButton`

| État du Bouton | Condition | Couleur `background` (`UiKit.Amber` + alpha) | Couleur `Cadre` | Couleur `label` (`TextMesh`) | Visibilité `arrow` (`"▶"` à `x = -0.64`) |
|---|---|---|---|---|---|
| **`SafetyLock / Disabled`** | Pendant `minReadTime = 1.0f` après la fin du fondu (`interactable == false`) | `rgba(1.00, 0.75, 0.27, 0.04)` | `rgba(1.00, 0.75, 0.27, 0.25)` | `new Color(0.45f, 0.42f, 0.36f, 1f)` (`#736B5C`) | Masqué (`SetActive(false)`) |
| **`Default (Idle)`** | `interactable == true && !hovered` | `rgba(1.00, 0.75, 0.27, 0.10)` | `rgba(1.00, 0.75, 0.27, 0.60)` | `UiKit.White` (`#E0EBE6`) | Masqué (`SetActive(false)`) |
| **`Hovered (Focus)`** | `interactable == true && hovered` (rayon VR ou curseur souris sur le collider) | `rgba(1.00, 0.75, 0.27, 0.30)` | `rgba(1.00, 0.75, 0.27, 0.60)` | `UiKit.Amber` (`#FFBF45`) | **Affiché** (`SetActive(true)`) en `UiKit.Amber` |
| **`Pressed (Click Glow)`** | À l'instant de `Click()` (`glow = 1.0f` décroissant vers `0f` à `4f * Time.unscaledDeltaTime`) | `rgba(1.00, 0.75, 0.27, 0.80)` -> `0.30` | `rgba(1.00, 0.75, 0.27, 0.60)` | `UiKit.Amber` (`#FFBF45`) | **Affiché** (`SetActive(true)`) |

---

## 5. Vérification des Contrastes WCAG & Confort Visuel VR

| Couple Premier-plan / Arrière-plan | Couleur Texte | Couleur Fond Effective | Ratio de Contraste Calculé | Conformité WCAG 2.1 |
|---|---|---|---|---|
| Texte `Bande_G` / `Bande_D` sur `Bande_Haut` (Défaite) | `#0F0A04` (`Ink`) | `#EB4038` (`UiKit.Red` 92 %) | `5.3 : 1` | **AA** (Texte gras majuscule) |
| Texte `Bande_G` / `Bande_D` sur `Bande_Haut` (Victoire) | `#0F0A04` (`Ink`) | `#52EB81` (`UiKit.Green` 92 %) | `12.8 : 1` | **AAA** |
| `Titre_Principal` `"VOUS ÊTES MORT"` sur `PanelRed` | `#FF453D` (`UiKit.Red`) | `#0B0505` (`PanelRed` sur `Fond_Opaque`) | `5.8 : 1` | **AA Large Text** (`charSize = 0.025f`) |
| `Titre_Principal` `"VOUS AVEZ ÉCHAPPÉ"` sur `PanelGreen` | `#FFCC59` (`victoryColor`) | `#050C08` (`PanelGreen` sur `Fond_Opaque`) | `13.1 : 1` | **AAA** |
| Valeurs `UiKit.White` sur `Cadre_Bilan_Fond` | `#E0EBE6` (`UiKit.White`) | `#050505` | `16.2 : 1` | **AAA** |
| Accents `UiKit.Amber` (Boutons survolés, Nouveau Record) | `#FFBF45` (`UiKit.Amber`) | `#0B0505` / `#050C08` | `11.6 : 1` | **AAA** |
| Libellés `UiKit.Dim` (Colonne gauche Bilan, Aide bas) | `#789682` (`UiKit.Dim`) | `#050505` | `5.9 : 1` | **AA** |

### Garanties de Confort VR (Meta Quest 3)
1. **Zéro élément attaché à la tête hors fondu** : Seul le quad uniforme `GameOverFade` est enfant de `head` ; tous les textes, cadres et boutons appartiennent au terminal World Space fixe, éliminant tout conflit vergence-accommodation et toute fatigue oculaire lors des mouvements de tête.
2. **Stabilité d'horizon** : La projection horizontale (`fwd.y = 0f`) garantit un roulis (`roll = 0°`) et un tangage (`pitch = 0°`) strictement nuls.
3. **Absence de scintillement** : Les lignes de balayage (`UiKit.Scanlines`) ont une opacité modérée (`alpha = 0.22f`) et aucun élément n'utilise de clignotement périodique.
