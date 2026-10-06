# Journal des prompts — THE LAST ESCAPOR

Historique des demandes (prompts) faites à l'assistant pendant le développement, avec ce qui a été fait en réponse.
Chaque nouveau prompt est ajouté **en bas de ce fichier** (entrée suivante) et dans l'index.

- Le texte du prompt est repris **tel quel** (avec ses fautes éventuelles).
- Le résultat est un résumé court : ce qui a changé, où, et les liens vers les commits et issues.
- Documents liés : [PLANNING.md](PLANNING.md) · [MAP.md](MAP.md) · [ASSETS.md](ASSETS.md)

---

## Index

| N° | Date | Sujet | Domaine | Résultat |
|---|---|---|---|---|
| [001](#001--journal-des-prompts) | 2026-10-06 | Journal des prompts | Documentation | Fichier créé |
| [006](#006--génération-des-zombies) | 2026-10-06 | Génération des zombies : analyse de la carte, mouvement, animations, attaque | Gameplay | Système testé avec un zombie provisoire |
| [007](#007--intégration-de-la-branche-ui-hugo) | 2026-10-06 | Intégrer la branche UI-Hugo et la tester dans l'environnement | Gameplay | UI de santé, Game Over et HUD intégrés, testés |
| [008](#008--mort-en-1-coup-et-vignettage-invisible) | 2026-10-06 | Joueur mort en 1 coup, vignettage invisible | Gameplay | 4 coups pour mourir, vignettage rouge style Call of Duty |
| [009](#009--pistolet-fumée-et-haptique) | 2026-10-06 | Asset Modern Guns Handgun, fumée au tir, haptique | Gameplay | Fumée et haptique faits ; modèle 3D en attente |
| [010](#010--navmesh-des-zombis-hitbox-et-niveaux) | 2026-10-06 | NavMesh des zombis, hitbox tête/torse/jambes, niveaux et dégâts | Gameplay | Prefab des points de départ, 3 zones de hitbox, table des niveaux |
| [011](#011--modèle-free-shirtless-zombie) | 2026-10-06 | Utiliser le modèle Free Shirtless Zombie (déjà importé) | Gameplay | Zombie remplacé par le modèle, hitbox sur les os, animations par script |

**Domaines :** Documentation · Gameplay · Environnement · Éclairage · Optimisation · Build / Quest · GitHub / organisation · Audio · UI

---

## Gabarit d'une entrée

Copier ce bloc en fin de fichier, incrémenter le numéro, puis ajouter une ligne à l'index.

```markdown
### 00X — Titre court

| | |
|---|---|
| **Date** | AAAA-MM-JJ |
| **Domaine** | Gameplay / Environnement / ... |
| **Statut** | Fait · En cours · À valider |

**Prompt**

> Texte exact de la demande.

**Résultat**

- Ce qui a été fait, en quelques lignes.
- Fichiers ou scènes concernés.

**Liens** : commit `abc1234` · issues #12, #13
```

---

## Entrées

### 001 — Journal des prompts

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Documentation |
| **Statut** | Fait |

**Prompt**

> Au dépo github rajoute un MD proprement structuré avec les prompts que je vais te faire

**Résultat**

- Création de ce fichier `docs/PROMPTS.md` : explication, index, gabarit d'entrée, puis une entrée par prompt.
- À partir de maintenant, chaque demande est ajoutée ici avec son résultat.

**Liens** : ce fichier

---

---

### 006 — Génération des zombies

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Gameplay |
| **Statut** | Partiel : modèle réel à importer |

**Prompt**

> Analyse la forme de la carte, et maintenant on va faire la génération des zombis
> Voici le lien des assets de zombi, place en un juste, et teste le mouvement, les animations, l'attaque
> https://assetstore.unity.com/packages/3d/characters/humanoids/free-shirtless-zombie-276762

**Résultat**

- **Analyse de la carte** (voir [docs/MAP.md](MAP.md)) : forme en U, 2 couloirs non reliés, portes de 1 m (agent NavMesh de 0,3 m de rayon), fenêtres inaccessibles de l'extérieur pour l'instant.
- **NavMesh** construit par `NavBuilder` ; chemins complets du bout de chaque couloir jusqu'à la porte puis jusqu'au joueur. Deux obstacles corrigés (cadres de porte et chaises renversées avaient des collisions pleines).
- **Zombie** (`Zombie.cs`) : marche vers l'ouverture accessible la plus proche, arrache une planche toutes les 3 s, entre, attaque (15 dégâts toutes les 1,5 s). Tir à la tête = mort, tir au corps = ralenti. Santé du joueur (`PlayerHealth`).
- **Barricades de départ** : chaque ouverture démarre avec 3 planches clouées ; les planches arrachées tombent et se ramassent.
- **Prefab** `Assets/Prefabs/Zombie.prefab` : zombie **provisoire** en boîtes avec animation procédurale (marche, attaque, mort), car l'asset du zombie n'est pas dans le projet (il doit être ajouté à ses assets depuis le compte Unity). `ZombieAnimationBridge` pilotera un Animator dès que le modèle réel sera là.
- **Test en mode Play** (zombie provisoire) : marche à 0,9 m/s, 3 planches arrachées en ~9 s, entrée dans le bunker, joueur attaqué (100 → 85 → 70), ralenti au tir au corps, mort au tir à la tête.
- Non testé : les animations du modèle réel, et tout au casque.

**Liens** : issues #26 à #33 · [docs/MAP.md](MAP.md)

---

### 007 — Intégration de la branche UI-Hugo

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Gameplay |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> pull la branche : UI-Hugo
> Met la en place dans l'environnement qu'on procède au test

**Résultat**

- La branche `UI-Hugo` (1 commit de LeCapet : UI pistolet, Game Over, PV) est fusionnée dans `main`.
- **Repris :** `HealthVignette`, `GameOverUI`, `HeadOverlay`, le HUD du pistolet (`Pistol.cs`), `PlayerHealth` (4 PV, régénération, `NewWave`), `BARRICADE_Scripts.md`.
- **`PlayerHealth` fusionné** avec la version des zombies : règles de la branche (4 PV) + `HeadPosition` ; le zombie inflige 1 PV par coup.
- **Non repris :** les modifications de réglages OpenXR de la branche (elles désactivaient *Meta Quest Support* et les profils des manettes Quest, cause du plantage au lancement), un réglage URP, et un binaire Windows de 7 Mo (`XrApiLayer_METAX_operator.dll`).
- L'UI est ajoutée à la scène par `RoomBuilder` (`UI_Sante`).
- **Test dans l'éditeur :** le zombie arrive, tue le joueur (4 coups), le vignettage rouge s'épaissit à chaque PV perdu, fondu noir et texte « VOUS ÊTES MORT », puis la scène redémarre à 4/4 PV. Compteur du pistolet : caché au holster, `12/12` en main, orange à 3 balles, rouge à vide.
- Non testé : au casque.

**Liens** : commit `daf5d2f` · branche `UI-Hugo` · [docs/MAP.md](MAP.md)

---

### 008 — Mort en 1 coup et vignettage invisible

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Gameplay |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> Problème : Actuellement le joueur meurt en 1 coup alors que ça devrai être 4 coups d'affilé
> Le vignettage n'est pas dutout visible, ce dernier devrai être comme celui de call of duty

**Résultat**

- **Mort en 1 coup :** le prefab du zombie gardait `attackDamage: 15` (valeur sérialisée de l'ancienne version en points de vie sur 100). Le prefab est reconstruit avec 1 dégât par coup ; `ZombieSpawner` impose désormais `zombieDamage = 1` et `zombieSpeed` à chaque zombie créé. Test dans l'éditeur : 4/4 → 3 → 2 → 1 PV, un coup à la fois.
- **Vignettage :** l'ancien quad en dégradé était trop faible. Remplacé par un shader plein écran (`Assets/Shaders/DamageVignette.shader`) : bords rouge sang aux contours irréguliers, centre dégagé, qui grossit à chaque PV perdu, flash au moment du coup, battement à 1 PV, retrait en douceur à la régénération. `HealthVignette.cs` (écrit par LeCapet) est réécrit pour l'utiliser ; le matériau est créé par `RoomBuilder`.
- Vérifié sur des captures de la caméra dans l'éditeur à 4, 3, 2 et 1 PV et au moment du coup.
- Non testé : au casque (rendu stéréo, lisibilité et confort du flash).

---

### 009 — Pistolet : fumée et haptique

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Gameplay |
| **Statut** | Partiel (testé dans l'éditeur) |

**Prompt**

> utilise cet asset pour le pistolet (importe le dans ma scène): https://assetstore.unity.com/packages/3d/props/guns/modern-guns-handgun-129821
> Quand on tire :
> * créer une mini fumée au moment du tire qui sort de l'arrière de l'arme
> * fait un retour haptic dans le controler
> * Respecte les animation du rig

**Résultat**

- **Fumée :** `Pistol.cs` crée un système de particules (petite bouffée grise, 14 particules, qui monte doucement) à l'arrière de l'arme et l'émet à chaque tir. Fonctionne avec n'importe quel modèle (champ `smokePoint` pour changer l'origine).
- **Haptique :** impulsion sur la manette de la main qui tient l'arme à chaque tir (0,7 pendant 0,08 s) et un petit tic quand le chargeur est vide. Utilise `InputDevice.SendHapticImpulse`, sans réglage de scène.
- **Asset :** le paquet *Modern Guns – Handgun* n'est pas dans le cache de l'Asset Store de l'utilisateur ; l'IA ne peut pas le télécharger. Il doit être ajouté à *My Assets* puis importé (*Package Manager → My Assets*). Le pistolet reste en boîtes grises en attendant.
- Non testé : la vibration réelle et la lisibilité de la fumée au casque.

---

### 010 — NavMesh des zombis, hitbox et niveaux

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Gameplay |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> Il faudrait définir les NavMesh des zombis, pour ça créer moi un prefab à part pour les zombis
> Regarde la structure de la carte, et choisi le bout des couloirs ainsi que la fenêtre comme points de départ des navmesh des zombis
> Aussi analyse de RIG du zombi, défini les différentes parties de la hitbox : Tête, torse, jambes
> Tête= 100% (One-shot) · Corps = 33% (3 shots) · Jambes = 20% (5 shots)
> Fait un tableau propre pour représenter les valeurs
> À l'augmentation des niveaux (début de nouvelles vagues, les zombis auront plus de PV, ou plus de résistance)
> Dégât dealt by zombis : Niveau 1 : 20% de la vie du joueur · Niveau 2 : 20% · Niveau 3 : 33% · Niveau 4 : 50%

**Résultat**

- **Prefab des points de départ** `ZombieSpawnPoints` : bout du couloir gauche, bout du couloir droit, fenêtre Nord. Le prefab `Zombie` (NavMeshAgent, hitbox) est reconstruit avec les nouvelles zones.
- **Cour Nord** ajoutée à la carte pour que la fenêtre Nord soit atteignable (sans extérieur, aucun zombie ne pouvait l'atteindre).
- **Hitbox** : `Hitbox_Tete` (one-shot), `Hitbox_Torse` (33 %, bras compris), `Hitbox_Jambes` (20 %). Rig analysé : celui du zombie provisoire en boîtes (le modèle réel n'est pas encore importé).
- **Niveaux** : table dans `ZombieLevels.cs` et dans [docs/ZOMBIES.md](ZOMBIES.md). **Choix de l'IA à valider** : facteurs de PV ×1 / ×1,3 / ×1,6 / ×2, la tête reste un one-shot à tous les niveaux.
- **Santé du joueur** passée de 4 PV à 100 PV (100 %) pour retirer des pourcentages ; au niveau 1 il faut donc 5 coups pour le tuer (et non plus 4). Le script `PlayerHealth` et le vignettage (de LeCapet) sont adaptés.
- **Test dans l'éditeur** : tirs nécessaires par zone et par niveau mesurés avec le vrai code ; chemins NavMesh depuis les 3 points ; un zombie niveau 3 part de la fenêtre, marche jusqu'à elle et arrache les planches.
- Non testé : au casque ; zombie avec le vrai modèle.

---

### 011 — Modèle Free Shirtless Zombie

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Gameplay |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> FREE Shirtless Zombie est le modele qu'on va utiliser pour les zombis, il est actuellement déjà importé dans le projet

**Résultat**

- Le prefab `Zombie` est reconstruit avec le modèle (`ShirtlessZombie_FREE_URP`) à la place des boîtes grises ; matériaux URP corrects, orientation correcte.
- **Constat :** le pack gratuit n'a **aucune animation**. L'IA a écrit `ZombieBoneAnim` (animation des os par script : marche, bras tendus, attaque, mort). Rendu correct sur les captures de l'éditeur, mais c'est rudimentaire ; de vrais clips seraient mieux.
- Hitbox tête / torse (+ bras) / jambes posées sur les os, mêmes valeurs que dans l'entrée 010.
- Non testé : au casque, performances (3 LOD, mais le coût sur Quest n'est pas mesuré).
- Le dossier du pack (`Assets/NewPunch`, 92 Mo) n'a pas été ajouté au dépôt dans ce commit.
