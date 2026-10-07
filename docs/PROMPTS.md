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
| [012](#012--wiki-github) | 2026-10-06 | Wiki GitHub : une page par fonctionnalité (zombies, vignettage, planches et clous) | Documentation | 3 pages + accueil + barre latérale publiés |
| [013](#013--niveaux-par-vagues-et-générateur) | 2026-10-06 | Système de niveaux par vagues (4), zones débloquées, objectif générateur | Gameplay | WaveManager, portes verrouillées, générateur |
| [014](#014--pistolet-modern-guns-handgun) | 2026-10-06 | Utiliser le pack Modern Guns : Handgun (analyse, animations, chargeur, balle) | Gameplay | M1911 intégré, tir animé, douille, flash, fumée, haptique |
| [015](#015--nouvelle-carte-blender-et-navmesh) | 2026-10-07 | Remplacer la carte par le nouveau fichier Blender, corriger le NavMesh ; retirer les planches déjà posées | Carte | Carte complète importée, NavMesh corrigé, ouvertures sans barricade |
| [016](#016--animations-mixamo-de-la-pr-68) | 2026-10-07 | Reprendre l'animation des zombies de la pull request (Mixamo) sur la nouvelle carte | Gameplay | PR #68 fusionnée, attaque synchronisée avec les dégâts |
| [017](#017--marche-du-zombie-qui-se-recharge) | 2026-10-07 | La marche du zombie « se recharge » en plein milieu | Gameplay | Déplacement du clip extrait en root motion : plus de saut |
| [018](#018--mains-comme-dans-my-project) | 2026-10-07 | Mains comme dans My project : prise à portée de main, pas de rayon, objets placés dans la main | Interaction | Interacteurs directs, accroches fixes pour l'arme et la lampe |
| [019](#019--retours-casque-planches-lampe-rotation-arme) | 2026-10-07 | Retours casque : planches du bunker, lampe, rotation fluide, arme au-dessus du poing | Interaction | Planches sous les fenêtres, objets qui ne traversent plus le sol, rotation fluide, accroche dans le poing |
| [020](#020--pause-plus-longue-entre-les-vagues) | 2026-10-07 | Plus de temps entre les vagues | Gameplay | Pause 20 → 35 s |
| [021](#021--musiques-de-fond-et-sons-du-pistolet) | 2026-10-07 | Deux musiques de fond en alternance, sons du pistolet (tir, rechargement), clic à vide | Audio | MusicPlayer, sons du pistolet, clic de culasse vide |
| [022](#022--commit-propre-des-packs-dassets) | 2026-10-07 | Commiter proprement dans main | Dépôt | 3 commits : pack pistolet, pack zombie, NavMesh ; bruit de régénération laissé de côté |
| [023](#023--analyse-du-skin-du-joueur-protective-suit-man) | 2026-10-07 | Skin du joueur « Protective suit man » : vérifier le modèle et les animations du pack | Personnage joueur | Pack analysé : humanoïde de 1,86 m, aucune animation |
| [024](#024--sons-de-lancement-et-voix-des-zombies) | 2026-10-07 | Sons à ajouter : lancement de la partie, zombies et une variante | Audio | Son de lancement, cris de zombie en 3D (2 variantes) |

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

---

### 012 — Wiki GitHub

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Documentation |
| **Statut** | Fait |

**Prompt**

> Je veux que pour chacune des modifications / ajout au jeu, tu me fasse une explication dans le Wiki sur github exemple là le ZOMBIES.md dans le wiki avec une page pour le Zombies
> Rajoute également aux rules le fait de faire ça à chaque ajout de fonctionnalités
> Fait de la même façon l'explication du vignettage au moment où le personnage reçoit des degats
> Le placement auto et la detection des planches / clous
> (puis : « Le wiki "Home" est créé »)

**Résultat**

- Pages rédigées par l'IA à partir du code et des tests : **Zombies**, **Planches et clous**, **Vignettage de dégâts**, plus `Home` et `_Sidebar`. Chaque page indique ce qui est testé dans l'éditeur et ce qui ne l'est pas au casque.
- Le wiki est un dépôt git séparé (`SAE501.wiki.git`) ; GitHub ne le crée qu'après la première page faite à la main dans l'interface.
- Règle « une page de wiki par fonctionnalité » ajoutée au fichier de règles de l'assistant.

---

### 013 — Niveaux par vagues et générateur

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Gameplay |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> Fait le système de niveau par vagues, en gros y a 4 vagues donc 4 niveaux
> Limites de la carte par niveau : Niveau 1 on a pas accès aux couloirs · Niveau 2 on a accès à la cuisine et aux couloirs · Niveau 3 stockage · Niveau 4 générateur
> Objectif ça sera d'activer le générateur

**Résultat**

- `WaveManager` : 4 niveaux, apparition des zombies par niveau, pause de réparation entre deux vagues, annonces, victoire.
- `ZoneGate` : 4 portes verrouillées (couloir gauche, couloir droit, stockage, générateur) qui bloquent le joueur et s'ouvrent au bon niveau ; panneau rouge « VERROUILLÉ ».
- `Generator` : bouton à maintenir 5 s dans la salle Générateur (niveau 4) ; victoire.
- **Choix de l'IA à valider** (non précisés) : nombre de zombies par vague (4/6/8/10), intervalles, points d'apparition par niveau (niveau 1 : fenêtre Nord seulement), pause de 20 s, « maintenir 5 s » pour activer le générateur, renforts au niveau 4.
- Test dans l'éditeur : enchaînement des niveaux, portes fermées/ouvertes, joueur arrêté à la porte au niveau 0 puis libre au niveau 2, zombies niveau 4 à 200 PV / 50 %, victoire. Non testé : au casque, équilibrage.
- Page de wiki : Niveaux et vagues.

---

### 014 — Pistolet Modern Guns : Handgun

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Gameplay |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> (suite de l'entrée 009 : fumée à l'arrière, retour haptique, respecter les animations)
> Pour le gun utilise : Modern Guns: Handgun — Déjà importé dans le projet unity, analyse la façon dont il fonctionne, ses animations ainsi que les éléments tels que le chargeur et la balle déjà proposé par le pack

**Résultat**

- **Analyse du pack** documentée dans le wiki (page « Pistolet ») : structure du M1911 (glissière, chien, détente, canon, éjection), animation `M1911@Fire` (0,233 s, tir à 0,117 s, douille à 0,183 s), contrôleur à 2 états, `SimpleShoot`, chargeur « plein » avec 2 cartouches, balle en 2 parties, flash de bouche.
- **Intégration :** M1911 à la place des boîtes grises, chargeur et douille du pack, animation de tir jouée tout de suite (l'animation d'origine tire avec 117 ms de retard), douille éjectée, flash de bouche, glissière qui reste en arrière à vide, fumée à l'arrière et vibration de la manette (demande de l'entrée 009).
- **Problèmes du pack** corrigés sans le modifier : matériaux Standard (paquet URP importé), matériaux URP du chargeur et de la balle **sans texture** (refaits), flash en shaders anciens (refait), `SimpleShoot` inutilisable (retiré), événements d'animation non livrés après un départ décalé (minuteur).
- Test dans l'éditeur : 12 tirs, compteur, douille (apparaît puis disparaît), glissière bloquée puis libérée au rechargement, captures du flash et du chargeur texturé. **Non testé : au casque.**
- Le dossier du pack (`Assets/Nokobot`) n'est pas ajouté au dépôt.

---

### 015 — Nouvelle carte Blender et NavMesh

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Carte |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> @LastBoardStanding_Kitchen (1).blend — Voici le fichier de la map, remplace la carte actuelle unity par celle-ci, corrige le NavMesh
> (pendant le travail) Y a des planches déjà placés sur les portes et fenêtres, retire les

**Résultat**

- **Analyse du fichier** : carte complète (bunker, couloirs gauche et droit en L, cuisine, générateur, stockage, pupitre de 4 écrans), 26 lumières, 4 caméras de surveillance, repères des planches ; 895 000 triangles.
- **Export** (`Tools/blender/export_map.py`, écrit par l'IA, à relancer à chaque nouvelle version du `.blend`) : FBX, matériaux, textures en JPG (25 Mo), lumières, caméras ; les objets de décor de plus de 6 000 triangles sont simplifiés (≈ 444 000 triangles au total).
- **Scène** (`RoomBuilder` réécrit) : l'ancienne carte en boîtes grises est remplacée ; lumières et caméras du fichier ; écrans du pupitre branchés sur les caméras (rendu à tour de rôle) ; ouvertures du bunker (2 portes, 2 fenêtres) ; portes de niveau et générateur replacés ; planches de la cuisine et du stockage interactives ; terrain extérieur ajouté au sud.
- **NavMesh corrigé** : colliders exacts pour murs et sols, décor plat sans collision, rayon des zombies 0,3 → 0,2 m (des objets en chicane bouchaient les deux couloirs), déchets mous ignorés, liens NavMesh pour enjamber les fenêtres. Chemins mesurés : 16,3 m, 18,7 m, 3,7 m.
- **Planches** : plus aucune barricade au départ (portes, fenêtres et porte condamnée du couloir) ; planches de jeu à la taille de celles du fichier (1,84 m × 28 cm).
- Test dans l'éditeur : un zombie casse une fenêtre, l'enjambe et attaque ; un autre traverse le couloir droit jusqu'à la porte est ; captures du bunker, des écrans, du couloir, du générateur. **Non testé : au casque (performances).**
- Page de wiki : Carte (et mises à jour : Zombies, Niveaux et vagues, Planches et clous).

---

### 016 — Animations Mixamo de la PR #68

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Gameplay |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> Il y a une pull request sur github concernant l'animation des zombis, récupère la façon dont sont animés les zombis dans cette pullrequest et utilise l'animation en question dans le projet actuelle avec la nouvelle map le navmesh etc

**Résultat**

- PR withRen/SAE501#68 (alamine01) : 3 clips Mixamo (marche, attaque, mort) + `ZombieMixamo.cs` (réglage Humanoid, Animator Controller `Speed` / `Attack` / `Die`). **Fusionnée** dans `main` (commits de l'auteur conservés), sans conflit avec la nouvelle carte (rayon NavMesh 0,2 m conservé).
- Ajout de l'IA : l'attaque du clip (2,63 s, main qui frappe à 1,05 s) est jouée ×1,5 et les dégâts / l'arrachage de planche sont calés sur ce coup (0,7 s au lieu de 0,45 s) ; mesure faite en échantillonnant le clip.
- Test dans l'éditeur sur la nouvelle carte : marche bras tendus, passage par la fenêtre (lien NavMesh), attaque (joueur 100 → 80 PV), mort au sol. **Non testé : au casque.** Glissement des pieds non vérifié (réglage `WalkClipSpeed`).

---

### 017 — Marche du zombie qui se recharge

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Gameplay |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> Y a quelques soucis d'animations, quand le zombi marche, sont animation de marche se recharge en plein milieu

**Résultat**

- **Cause** (mesurée en échantillonnant le clip) : le clip Mixamo de marche n'est pas « sur place », le bassin avance de 1,29 m en 4,03 s. L'import réglé par la PR #68 gardait ce déplacement dans la pose (*bake into pose* XZ) : le corps s'éloignait du zombie pendant le cycle puis revenait d'un coup au début de chaque boucle.
- **Correction** (`ZombieMixamo.cs`) : déplacement horizontal extrait en *root motion*, ignoré par l'Animator ; c'est le NavMeshAgent qui déplace le zombie. Le script réapplique maintenant les réglages quand ils changent (avant, il s'arrêtait si les clips étaient déjà nommés).
- **Vérifié en jeu** (zombie qui marche 10 s dans le couloir droit, 4 boucles) : bassin à moins de 7 cm du zombie, plus grand écart d'une image à l'autre 7 mm. Non testé : au casque.
- Remarque : le clip avance naturellement à 0,32 m/s ; le zombie marche à 0,9 m/s et le clip est joué ×1,6, donc les pieds glissent un peu (réglage `WalkClipSpeed`, à juger au casque).

---

### 018 — Mains comme dans My project

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Interaction |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> (capture du casque : pistolet tenu de travers, loin de la main)
> Regarde dans My Project comment sont configurés les mains, il faut la même configuration :
> * Les objets doivent être à porté de main pour interagir
> * Le joystick de droit ne sert pas à rotate / bouger les objets à distance
> * Retire les raycast qui attrapent les objets de loins
> * Les objets : Arme, lampe torche; doivent être placé proprement dans la main instantannément

**Résultat**

- **Analyse de My project** (scène lue par script) : chaque main a un `XRDirectInteractor` (sphère de 15 cm, point d'accroche dans la paume), pas de rayon pour attraper ; armes avec point d'accroche fixe.
- **Cause du problème** : le rig des Starter Assets utilise le `Near-Far Interactor`, qui attrape de loin avec un rayon, garde l'objet à distance et laisse le joystick le déplacer / tourner.
- **Changement** (`SetupVRScene.UseDirectInteractor`) : Near-Far désactivé, interacteur direct ajouté sur chaque manette avec les valeurs de My project et les mêmes boutons. Points d'accroche fixes : pistolet, lampe torche (nouveau), marteau, chargeur ; planches et piles prises là où la main les touche. Plein écran des caméras rapproché à 50 cm pour être touchable.
- Test dans l'éditeur (prise simulée) : pistolet et lampe à 0,5 cm du point d'accroche, alignés à 0° avec la manette. **Non testé : au casque.**
- Page de wiki : Mains et prise des objets.

---

### 019 — Retours casque : planches, lampe, rotation, arme

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Interaction |
| **Statut** | Fait (testé dans l'éditeur) |

**Prompt**

> (capture du casque : pistolet au-dessus du poing)
> Dans le bunker il manque les planches pour bloquer la fenêtre
> La lampe est bugué
> Pour tourner fait un traveling smooth au lieu d'un tp de caméra
> Actuellement quand j'attrape l'arme elle flotte encore au dessus de la main au lieu d'être dans le grip fermé

**Résultat**

- **Planches** : la réserve existait (6 planches au sol près de la fenêtre sud-est, peu visibles) ; elle est répartie en **2 piles de 3 sous chaque fenêtre**.
- **Arme au-dessus du poing** : le point d'accroche de My project (7 cm sous la manette) ne correspond pas à notre modèle de main (10 cm plus bas) ; mesuré sur les os de la main, le poing est à ~13,5 cm. Point d'accroche déplacé au **centre du poing** (côté droit / gauche) : la crosse est maintenant entre les doigts et la paume (capture dans l'éditeur).
- **Lampe** : bug trouvé en test — lâchée de haut, elle **traversait le sol** et tombait sans fin (petit objet, détection de collision discrète). Tous les objets attrapables passent en **détection continue** ; vérifié avec lampe, pile, chargeur lâchés de 2,6 m. Allumage / extinction à la gâchette vérifiés. Si le bug vu au casque était autre chose, il reste à le décrire.
- **Rotation** : **fluide** au lieu de crans (`SmoothTurn` dans `SetupVRScene.cs`). ⚠️ Contraire à la recommandation de confort du document de la SAÉ (rotation par crans) : choix de l'équipe, à justifier dans le GDD ou à rendre réglable.
- **Non testé : au casque.**

---

### 020 — Pause plus longue entre les vagues

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Gameplay |
| **Statut** | Fait |

**Prompt**

> Laisse un peu plus de temps entre les vagues

**Résultat**

- Pause entre deux vagues : **20 s → 35 s** (`WaveManager.breakBetweenLevels`), pour réparer les barricades et aller chercher des planches. Le délai avant la 1re vague (15 s) n'a pas changé. Valeur vérifiée dans la scène régénérée ; non testé au casque.

---

### 021 — Musiques de fond et sons du pistolet

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Audio |
| **Statut** | Fait (mesuré dans l'éditeur, pas écouté au casque) |

**Prompt**

> (fichiers `music_jeu_principale_1.mp3` et `music_jeu_principale_2.mp3`) Met les deux musiques dans le jeu, ça alterne l'une après l'autre
> (fichiers `gun_reload.mp3` et `gun_shot.mp3`) Vérifie l'alternance des deux musiques en jeu. Rajoute ces sons : tire et rechargement (du pistolet). Rajoute un petit click si le joueur tente de tirer alors qu'il n'a plus de balles

**Résultat**

- **Musique** (`MusicPlayer`) : les 2 pistes (2 min 12 et 1 min 56) s'enchaînent en boucle, chargées en streaming (Vorbis) pour le Quest, volume 35 %.
- **Sons du pistolet** : `Pistolet_Tir.mp3` au tir, `Pistolet_Rechargement.mp3` quand le chargeur entre dans la crosse ; sons 3D sortant de l'arme.
- **Clic à vide** : le clic existant (40 ms de bruit, presque inaudible) est remplacé par un vrai « tic » métallique de culasse vide (0,09 s) généré par code ; aucun fichier fourni pour celui-ci.
- **Bug trouvé en vérifiant** : la première version démarrait la piste 1 avec `PlayScheduled`. Le projet a « Enable Output Suspension » : sans son, Unity coupe l'audio et fige son horloge, donc rien ne démarrait. Corrigé (premier `Play()` normal, les suivantes programmées). À l'origine de plusieurs « blocages » constatés : l'éditeur gelait aussi pendant un « Build And Run » lancé à 11 h 46, et l'horloge audio de l'éditeur est restée bloquée jusqu'à son redémarrage.
- **Vérifié dans l'éditeur (vraies pistes)** : piste 1 → piste 2 à 124 s, piste 2 → piste 1 à 240 s, sans blanc ; durées de lecture mesurées : tir 2,02 s, rechargement 1,00 s, clic 0,12 s. **Non écouté** à l'oreille ni au casque (volume à régler).
- Pages de wiki : Musique (nouvelle), Pistolet (section Sons).

---

### 022 — Commit propre des packs d'assets

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Dépôt |
| **Statut** | Fait |

**Prompt**

> commit ça dans le main proprement déjà

**Résultat**

- Tout le travail de code était déjà commité ; restaient les **deux packs Asset Store** (`Assets/Nokobot`, `Assets/NewPunch`) et le NavMesh régénéré. Sans les packs, `main` était cassé pour les coéquipiers (pistolet et zombie introuvables).
- **3 commits séparés** dans `main` : pack Modern Guns (9 Mo), pack Free Shirtless Zombie (92 Mo, textures et modèles en Git LFS : 28 fichiers), NavMesh de la scène.
- **Laissés de côté volontairement** (bruit local, pas du travail) : 22 matériaux / render textures de `Assets/Generated` réécrits par Unity (il retire un bloc de version interne à chaque génération), `Zombie_Mixamo.controller` (même cause), `DevAgentSettings.asset` (adresse IP locale du débogueur Meta), suppression de `RuntimeActionBindings.json` (régénéré par Unity).
- Le dépôt reste **privé** : les packs sont sous licence Asset Store.

---

### 023 — Analyse du skin du joueur (Protective suit man)

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Personnage joueur |
| **Statut** | Analyse faite (rien d'intégré au jeu) |

**Prompt**

> Le personnage joueur aura ce skin (texture) : Protective suit man (Asset Store 158254)
> Vérifie le modèle, les animations qui sont déjà présents dans l'asset

**Résultat**

- Pack lu dans le cache de l'Asset Store (`Protective suit man.unitypackage`, 62 Mo) puis **importé dans le projet** (`Assets/Protection_suite`), **non commité**.
- **Modèle** : un seul FBX, humanoïde de 1,86 m (T-pose, 1,38 m d'envergure), 15 617 triangles, 8 068 sommets, 2 matériaux (combinaison + visière en verre), 3 jeux d'UV, 67 os au **même squelette que le zombie** (noms de type Unreal, doigts, os de torsion, os IK `ik_hand_gun` / `ik_foot`). Configuré en Humanoïde dans Unity.
- **Animations : aucune.** Le FBX n'a aucun clip, il n'y a ni `.anim` ni Animator Controller dans le pack ; l'Animator du prefab n'a pas de contrôleur.
- **Problèmes à régler avant de l'utiliser** : shader « Standard (2sided) » du pack écrit pour l'ancien pipeline de rendu (rose sous URP) ; 4 textures TGA de 4096×4096 non compressées (224 Mo sur disque, aucun réglage Android) ; maillage en lecture/écriture.
- **Non vérifié** (outil Unity indisponible à ce moment-là) : validité de l'avatar dans Unity, mémoire réelle des textures, rendu.

---

### 024 — Sons de lancement et voix des zombies

| | |
|---|---|
| **Date** | 2026-10-07 |
| **Domaine** | Audio |
| **Statut** | Fait (mesuré dans l'éditeur, pas écouté au casque) |

**Prompt**

> (fichiers `lancement_partie.mp3`, `zombis.mp3`, `zombis2.mp3`) Voici les sons à rajouter : Lancement de la partie. Zombis, et un autre variante des zombis

**Résultat**

- **Lancement de la partie** (5,8 s) : joué une fois au démarrage de la scène par `WaveManager`, non spatialisé, volume 90 %. Mesuré : 5,7 s. Choix de l'IA à valider : « lancement » interprété comme le démarrage de la scène (aussi rejoué après défaite ou victoire) ; à déplacer si un écran de démarrage est ajouté.
- **Voix des zombies** (8,1 s et 1,4 s) : `ZombieVoice` sur chaque zombie, cris en 3D (premier cri 0,5 à 2,5 s après l'apparition, puis toutes les 5 à 12 s), variante tirée au hasard à chaque cri, hauteur de voix ±8 %, 3 zombies maximum qui crient en même temps, voix coupée en fondu à la mort. **Choix de l'IA** : la répartition des deux variantes (aléatoire) et la fréquence des cris sont à régler à l'oreille.
- **Vérifié dans l'éditeur** : 4 zombies, les 2 variantes jouées en alternance aléatoire, jamais plus de 3 voix ; voix coupée 0,6 s après un tir à la tête en plein cri. **Non écouté** au casque (volumes).
- Le dossier contenait 2 autres fichiers non cités (`dragon-studio-…`, dont une copie identique de `zombis.mp3`) : non utilisés.
- Pages de wiki : Zombies (voix), Niveaux et vagues (son de lancement).
