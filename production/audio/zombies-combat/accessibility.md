# Spécifications d'Accessibilité Audio & Confort Sensoriel (`zombies-combat`)

**Projet** : The Last Escapor (Unity 6 — Meta Quest 3 VR & PC)
**Étape du Pipeline** : Étape 2 — Audio Accessibility (`/team-audio zombies-combat`)
**Rôle** : `accessibility-specialist` (routé via `sound-designer`)

---

## 1. Principe Directeur : Redondance Multimodale en Huis Clos 360°

Dans **The Last Escapor**, l'absence de HUD flottant intrusif en VR fait reposer la détection des menaces à 360° (Fenêtre Sud-Ouest, Fenêtre Sud-Est, Porte Ouest, Porte Est, et les 2 Trous du toit) sur la spatialisation sonore.

**Règle d'Accessibilité Critique** : Aucun événement sonore porteur d'une information de survie immédiate ne doit être exclusivement auditif. Chaque alerte vitale doit posséder un équivalent **visuel périphérique/diégétique** et, en VR, un équivalent **haptique directionnel** afin de garantir la jouabilité pour les joueurs sourds, malentendants ou jouant à volume réduit.

---

## 2. Événements Critiques Déjà Couverts dans le Code (`CONFORME`)

L'audit du code existant confirme que 3 signaux de survie majeurs disposent déjà d'une redondance multimodale complète :

| Événement Audio Critique | Canal Visuel Existant | Canal Haptique Existant (VR) | Statut |
| :--- | :--- | :--- | :--- |
| **Dégât subi & Direction de l'agresseur** (`PlayerHurt()` + `ClawSwipe()`) | `HealthVignette.cs` affiche un arc rouge directionnel à 360° autour du centre du champ de vision pointant vers la position de l'attaquant, accompagné d'un flash rouge périphérique. | `PlayerHealth.cs` déclenche une impulsion haptique asymétrique (manette gauche ou droite selon l'angle d'attaque). | **PASS** |
| **Santé Critique `< 25 % PV`** (`Heartbeat()` à `1.3 Hz`) | `HealthVignette.cs` fait pulser l'opacité du vignettage rouge périphérique exactement à la fréquence cardiaque de `1.3 Hz`. | Pulsation de basse amplitude possible en soutien du vignettage. | **PASS** |
| **Infiltration Verticale par les 2 Trous du Toit** (`RoofStep()` + `DustFall()` pendant `3.5s`) | `Opening.WarnFromAbove` / `MakeDust` dans `RoomBuilder.cs` génère un cône dense de poussière et de gravats tombant du plafond jusqu'au sol pendant `3.5s` avant la chute du zombie. | Non requis à distance (indice visuel 3D traversant toute la hauteur de la pièce). | **PASS** |

---

## 3. Écarts d'Accessibilité Identifiés & Solutions Requises (`BLOCKING` & `ADVISORY`)

Tout écart privant un joueur sourd ou malentendant d'une information indispensable pour survivre à une vague est classé **BLOCKING** et doit être résolu lors de l'implémentation C# (Étape 3/4).

### GAP 1 [BLOCKING] — Bris de Barricade Hors Champ (`WoodCrack` sur Fenêtres SO/SE et Portes O/E)
* **Problème actuel** : Lorsqu'un zombie attaque ou détruit une planche sur l'un des 4 accès périmétriques (`WoodCrack()`), le retour est 100 % sonore. Un joueur faisant face à la Porte Est ne dispose d'aucun indicateur visuel ni haptique qu'une barricade cède dans son dos à la Porte Ouest ou aux Fenêtres Sud, jusqu'à ce qu'il soit frappé au corps-à-corps.
* **Spécification de remédiation (Obligatoire)** :
  1. **Indicateur Visuel Directionnel (`HealthVignette.cs`)** : Ajouter une méthode publique `NotifyPerimeterBreach(Vector3 worldPos)` appelée par `Opening.cs` lorsqu'une planche cède (`WoodCrack`). Elle affiche sur l'anneau périphérique un **arc directionnel ambre/orange** (`rgba(255, 152, 0, 0.42)`, durée `0.45s`, largeur angulaire plus fine que l'arc rouge de dégât corporel pour éviter toute confusion entre "barricade attaquée" et "joueur blessé").
  2. **Alerte Diégétique / Moniteur** : Faire clignoter brièvement en ambre le voyant ou le cadre du moniteur CCTV / écran d'annonce correspondant au secteur attaqué.
  3. **Retour Haptique VR** : Impulsion courte de faible intensité (`amplitude = 0.22`, `duration = 0.07s`) sur le contrôleur du côté de la brèche.

### GAP 2 [BLOCKING] — Approche & Télégraphe d'Attaque dans l'Angle Mort (`AttackWindup` à `< 2.0 m`)
* **Problème actuel** : Le nouveau son `AttackWindup` offre `450 ms` (`attackHitDelay = 0.45s`) au joueur entendant pour esquiver un Coureur ou un Cogneur arrivé dans son angle mort. Sans équivalent non-auditif, un joueur malentendant perd totalement cette fenêtre de contre-jeu face aux Coureurs rapides (`vitesse x1.45-1.6`).
* **Spécification de remédiation (Obligatoire)** :
  1. **Pré-indicateur Directionnel d'Angle Mort (`HealthVignette.cs`)** : Lorsque `Zombie.cs` déclenche `AttackWindup` à moins de `2.2 m` du joueur et que l'attaquant se trouve hors du cône de vision frontal (`Vector3.Dot(camForward, toZombie) < 0.35f`), déclencher un **micro-arc jaune/ambre vif** (`rgba(255, 193, 7, 0.38)`, durée `0.30s`) dans la direction du zombie.
  2. **Pré-alerte Haptique VR (`PlayerHealth.cs` / `VRHaptics`)** : Déclencher une vibration d'avertissement distincte (`amplitude = 0.38`, `duration = 0.10s`) sur la manette gauche ou droite selon l'azimut du zombie, `450 ms` avant l'impact potentiel.

### GAP 3 [ADVISORY / CONFORT] — Tir à Sec (`DryFire`) en VR sans Inclinaison du Poignet
* **Problème actuel** : En VR, `InspectHud.cs` n'affiche l'état du chargeur que lorsque le joueur incline volontairement son arme. En plein assaut, le clic aigu de `DryFire()` (`90 ms`) peut passer inaperçu ou être inaudible pour un joueur malentendant qui continue alors d'appuyer sur la détente.
* **Spécification de remédiation (Recommandée)** :
  1. **Forçage Visuel Temporaire (`InspectHud.cs` / `Pistol.cs`)** : Dès que `DryFire()` est appelé, forcer l'affichage de `InspectHud` pendant `1.5s` quel que soit l'angle du poignet, avec l'indicateur `0 / 12` coloré en rouge vif (`#FF3B30`).
  2. **Micro-choc Haptique de Culasse Vide** : Envoyer une impulsion sèche (`amplitude = 0.35`, `duration = 0.04s`) dans la manette tenant le pistolet à chaque pression à vide.

---

## 4. Sensibilité Auditive, Écrêtage Fréquentiel & Prévention de la Fatigue (VR)

L'écoute prolongée au casque sur Meta Quest 3 ou au casque PC fermé expose l'oreille interne à une fatigue rapide si les transitoires synthétiques de `ProcAudio.cs` ne sont pas contrôlées.

### 4.1. Écrêtage des Hautes Fréquences Agressives (Anti-Stridence `> 3.5 kHz`)
* **`ClawSwipe()` (`ProcAudio.cs`)** : Le balayage de griffure à `0.7 m` des oreilles ne doit jamais contenir de sifflement pur au-dessus de `3.5 kHz`. Maintenir le coefficient du filtre passe-bas `alpha <= 0.32` pour conserver le mordant de la griffe sans agressivité tympanique.
* **`DryFire()` & `CrowbarImpact()` (`ProcAudio.cs`)** : Plafonner l'amplitude du partiel supérieur (`2380 Hz` pour le pied-de-biche) à `0.08` maximum et amortir toute résonance métallique au-delà de `260 ms` (`exp(-t * 42)`).
* **`Alarm()` (`ProcAudio.cs`)** : L'alarme de bunker existante dans `ProcAudio.cs` doit rester filtrée dans les médiums (`< 1.8 kHz`) et ne jamais être jouée à `spatialBlend = 0f` à plein volume.

### 4.2. Contrôle de la Plage Dynamique (Zéro Jump-Scare Douloureux)
* **Interdiction des transitoires > 0 dBFS** : Tous les tampons générés dans `ProcAudio.cs` doivent être bornés à `[-0.95f, +0.95f]`.
* **Écart dynamique maximal contrôlé** : Même lors d'une attaque surprise d'un Coureur dans le dos, le niveau crête de `AttackWindup` (`0.78`) et `ClawSwipe` (`0.90`) reste strictement encadré par le plafond nominal des armes du joueur (`Pistolet_Tir` à `0.90`), évitant tout choc acoustique disproportionné si le joueur règle son casque sur le volume de ses propres tirs.
* **Anti-Spam Polyphonique** : Lorsque plusieurs zombies frappent simultanément une même barricade ou le joueur, un cooldown de fusion de `80 ms` sur `WoodCrack()` et de `150 ms` sur `ClawSwipe()` empêche l'addition constructive de phase (qui doublerait le volume perçu à `+6 dB`).
