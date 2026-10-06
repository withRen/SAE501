# BARRICADE — Documentation des scripts Unity

Projet VR (Unity + XR Interaction Toolkit, Meta Quest 3). Ce document décrit le rôle de chaque script, comment ils s'articulent, et qui a travaillé sur quoi.

## Sommaire

1. [Partie de H — UI (PV, game over, munitions)](#1-partie-de-h--ui)
2. [Équipement : ceinture, holsters, pistolet, chargeurs](#2-équipement)
3. [Barricade : marteau, clous, planches](#3-barricade)
4. [Lampe torche et batteries](#4-lampe-torche-et-batteries)
5. [Ambiance et surveillance](#5-ambiance-et-surveillance)
6. [Utilitaires](#6-utilitaires)
7. [Reste à brancher](#7-reste-à-brancher)

---

## 1. Partie de H — UI

**Auteur : H.** Cinq fichiers, dossier `Scripts/UI/` conseillé.
Objectif (GDD, §2 « Victoire et défaite ») : afficher les PV, la défaite et les munitions sans menu flottant, avec une interface diégétique.

| Fichier | Rôle |
|---|---|
| `PlayerHealth.cs` | Logique des points de vie |
| `HealthVignette.cs` | Vignettage rouge selon les PV |
| `GameOverUI.cs` | Écran noir, texte et relance |
| `HeadOverlay.cs` | Outil qui crée les overlays collés à la tête |
| `Pistol.cs` | Version modifiée : compteur de munitions `02/12` |

### 1.1 `PlayerHealth.cs` — système de santé

Singleton (`PlayerHealth.Instance`) placé sur un objet de la scène. Règles du GDD :

- **4 PV** au départ (`maxHealth`).
- **+1 PV** après **10 s** sans dégât (`regenDelay`).
- **2 PV maximum** récupérés par vague (`maxRegenPerWave`).
- À **0 PV** : événement `Died` → game over.

API :

| Membre | Description |
|---|---|
| `TakeDamage(int amount = 1)` | À appeler à chaque coup de griffe d'un zombie |
| `NewWave()` | À appeler au début de chaque vague (remet le quota de régénération à 0) |
| `Health`, `IsDead` | État actuel |
| `Changed(pv, max)`, `Damaged(n)`, `Died` | Événements auxquels l'UI s'abonne |

Le chronomètre de régénération est remis à zéro à chaque coup reçu.

### 1.2 `HealthVignette.cs` — écran qui se trouble

Vignettage rouge, façon Call of Duty :

- L'intensité dépend des PV perdus (tableau `intensityByHpLost` : 0 → 0,3 → 0,55 → 0,85).
- Le rouge **avance vers le centre** à mesure que les PV baissent (le quad rétrécit, ce qui donne l'effet « le vignettage s'épaissit » du GDD).
- Un **flash rouge** s'ajoute à chaque coup reçu, puis s'estompe.
- Avec **1 PV restant**, le vignettage **pulse**.
- Les transitions sont lissées, y compris à la régénération.

### 1.3 `GameOverUI.cs` — défaite

À la mort (`PlayerHealth.Died`) :

1. Fondu au noir (`fadeDuration`, 1,2 s par défaut).
2. Affichage d'un `TextMesh` optionnel (« VOUS ÊTES MORT… »).
3. Rechargement de la scène après `restartDelay` (6 s).

Aucun menu : tout reste diégétique, comme le demande le GDD.

### 1.4 `HeadOverlay.cs` — outil commun

En VR, un Canvas « Screen Space Overlay » n'est pas visible dans le casque. Ce script crée donc un **quad enfant de la caméra**, dessiné par-dessus la scène (render queue 4000 et plus). Il fournit aussi la texture de dégradé radial utilisée par le vignettage.

> **Build Quest :** ajouter `Sprites/Default` dans *Project Settings → Graphics → Always Included Shaders*, sinon le shader peut être retiré du build.

### 1.5 `Pistol.cs` — compteur de munitions

Le pistolet (tir, raycast, son) existait déjà. H a ajouté le **HUD munitions** :

- Format `02/12` sur le côté de l'arme.
- Visible **uniquement quand le pistolet est en main** (masqué dans le holster).
- Blanc normalement, **orange** à 3 balles ou moins (`lowThreshold`), **rouge clignotant** à 0.

### 1.6 Mise en place

1. Mettre `PlayerHealth`, `HealthVignette` et `GameOverUI` sur un objet vide `UI_Manager`.
2. Assigner `head` (Main Camera du XR Origin) dans `HealthVignette` et `GameOverUI`, ou laisser vide pour utiliser `Camera.main`.
3. Placer le `TextMesh` du compteur sur le côté du pistolet et l'assigner dans le champ `hud` du `Pistol`.
4. Test rapide : appeler `PlayerHealth.Instance.TakeDamage()` depuis un script temporaire.

---

## 2. Équipement

Le joueur porte une ceinture avec des **holsters**. Chaque objet est soit en main, soit rangé dans son holster.

| Script | Rôle |
|---|---|
| `BeltRig.cs` | La ceinture suit la tête : position à hauteur de hanche (`heightRatio` = 55 % de la taille), rotation en Y lissée pour ne pas suivre chaque mouvement de tête |
| `BeltItem.cs` | Type d'objet (`Pistol`, `Hammer`, `Flashlight`, `Magazine`) et fonction statique `InHand(grab)` : vrai si l'objet est tenu par une main et pas rangé dans un socket |
| `BeltSocket.cs` | Holster (`XRSocketInteractor`) qui n'accepte qu'un seul type d'objet |
| `Pistol.cs` | 12 balles. Tir par raycast depuis `muzzle`, flash lumineux, son, impulsion physique sur la cible. Envoie le message `OnShot(hit)` à l'objet touché : **c'est le point d'entrée pour les dégâts sur les zombies**. Clic sec si le chargeur est vide |
| `Magazine.cs` | Chargeur (`rounds` balles). `Consume()` le détruit une fois inséré |
| `MagWell.cs` | Emplacement du chargeur sur le pistolet (`XRSocketInteractor`). N'accepte que les chargeurs, appelle `pistol.Load(rounds)` puis détruit le chargeur |

`InHand()` est la brique centrale : le pistolet, le marteau et la lampe ne fonctionnent que s'ils sont en main.

---

## 3. Barricade

C'est le geste signature du jeu : **placer une planche, puis la clouer en balançant le marteau**.

### 3.1 Marteau et clous

| Script | Rôle |
|---|---|
| `Hammer.cs` | Expose `Hammer.Held` (vrai si le marteau est en main). Sert à faire apparaître les clous |
| `HammerHead.cs` | Suit la tête du marteau : position, position précédente, vitesse (lissée) et rayon. S'exécute juste après XRI |
| `Nail.cs` | Un clou. Apparaît quand le marteau est en main, s'enfonce en **3 coups** (`hitsNeeded`) |

**Détection d'un coup :** elle est **géométrique**, sans physique. On teste le segment parcouru par la tête du marteau pendant la frame contre la tête du clou. Un coup est validé seulement si :

- la vitesse dépasse `minSpeed` (0,9 m/s), donc il faut un vrai balancier ;
- le marteau va **vers** la planche (produit scalaire > 0,25), donc pas de coup en reculant ;
- la planche est **maintenue par une main** ;
- au moins 0,25 s s'est écoulée depuis le coup précédent.

Chaque coup joue un son dont la hauteur monte avec le nombre de coups, et le clou s'enfonce visuellement.

### 3.2 Planches et emplacements

| Script | Rôle |
|---|---|
| `BarricadeSlot.cs` | Emplacement sur une ouverture. Affiche une **planche fantôme pulsante** quand le joueur tient une planche. `Accepts()` : distance < 22 cm et orientation alignée |
| `Plank.cs` | Machine à états de la planche (voir ci-dessous) |

**Cycle de vie d'une planche :**

```
Free ──(approchée d'un slot)──> Snapped ──(lâchée avec ≥ 1 clou)──> Nailed ──(2 clous)──> Fixed
                                   │
                                   └─(lâchée sans clou, ou main trop loin)──> Unsnap : retombe
```

- **Free** : planche normale. Une seule planche attrapable à la fois.
- **Snapped** : calée sur l'ouverture, il faut la **tenir**. Si la main s'éloigne de plus de 35 cm (`unsnapRadius`), elle se décale.
- **Nailed** : un clou enfoncé. Elle **pend par ce clou** (petit effet de pendule), sauf si l'autre main la tient via la `holdZone`.
- **Fixed** : deux clous enfoncés, solide.

---

## 4. Lampe torche et batteries

| Script | Rôle |
|---|---|
| `Flashlight.cs` | Gâchette = allumer/éteindre (seulement en main). Batterie qui se vide (`drainPerSecond`), barre de niveau visuelle, **scintillement sous 15 %**, extinction à 0 |
| `Battery.cs` | Pile ramassable. `Consume()` la détruit une fois utilisée |

La lampe a un trigger `BatteryPort` à l'arrière : une pile qui y entre **recharge** la lampe à 100 % et est consommée.

---

## 5. Ambiance et surveillance

| Script | Rôle |
|---|---|
| `FlickerLamp.cs` | Lampe défaillante façon horreur : léger souffle continu (bruit de Perlin) et, rarement, une **baisse de tension douce** (pas de coupure franche). Fait aussi varier l'émission de l'ampoule |
| `MonitorStation.cs` | Table de surveillance : afficher le flux d'une caméra en plein écran devant les yeux |
| `MonitorScreen.cs` | Un écran de la table. Quand on le sélectionne, il affiche son flux via `station.Show(index)` |
| `MonitorOverlayClose.cs` | Fermer l'overlay des flux |

---

## 6. Utilitaires

| Script | Rôle |
|---|---|
| `ProcAudio.cs` | Sons **générés par code** (tir, clic sec, coup de marteau) en attendant de vrais assets audio. À remplacer par les sons nettoyés sous Audacity (GDD §5) |
| `IgnorePlayer.cs` | Ignore les collisions entre un objet (ceinture, planches…) et le `CharacterController` du joueur, pour ne pas le pousser |

Plusieurs classes (`Plank`, `BarricadeSlot`) réinitialisent leurs variables statiques avec `RuntimeInitializeOnLoadMethod`. Cela évite des états résiduels quand on relance le jeu dans l'éditeur sans recharger le domaine.

---

## 7. Reste à brancher

Éléments du GDD qui ne figurent pas encore dans les scripts fournis :

- **Zombies** (NavMesh) : au contact de la fenêtre, appeler `PlayerHealth.Instance.TakeDamage()` ; pour les blesser, implémenter `OnShot(RaycastHit)` sur leur collider.
- **Gestionnaire de vagues** : appeler `PlayerHealth.Instance.NewWave()` au début de chaque vague.
- **Durabilité des planches** : les zombies arrachent le bois (`Plank` / `BarricadeSlot`).
- **Radio et bouton vert** de victoire.
- **Objets jetables** (briques, canettes), classés « POURRAIT » dans le GDD.
