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
| [025](#025--revue-des-pull-requests-69-79-77-rien-poussé-dans-le-main) | 2026-10-07 | Revue des PR #69, #79 et #77 : fonctionnement et conflits, sans rien pousser dans le main | Revue | 2 bugs bloquants (#69 entrée, #77 shader), #79 OK, #67 en conflit |
| [026](#026--revue-bugs-et-optimisation-rien-poussé) | 2026-10-07 | Vérifier bugs et optimisation, sans rien pousser | Revue | Quest rendu avec le pipeline PC, 26 lumières temps réel, ombre de point light, textures 4K du zombie, petites allocations par image |
| [027](#027--corrections-doptimisation-rien-poussé) | 2026-10-07 | Appliquer les corrections d'optimisation proposées | Optimisation | Niveau Quest, textures 1024 px, CCTV rendues seulement si vues, allocations par image supprimées ; occlusion culling essayé puis retiré |
| [028](#028--image-noire-au-casque) | 2026-10-07 | Image noire au casque après le niveau Quest | Bug | Quest_RPAsset : copie du pipeline PC sans ombres coûteuses |
| [029](#029--écran-noir-son-seul) | 2026-10-07 | Toujours écran noir au casque, le son marche | Bug | GPU Resident Drawer qui plantait le pipeline à chaque image ; désactivé pour le Quest |
| [030](#030--écran-noir-avec-les-textes) | 2026-10-07 | Écran noir mais textes visibles | Bug | Build profile Meta Quest qui citait des niveaux de qualité absents ; rendu fovéal fixe (72 images/s) |
| [031](#031--planches-visibles-et-pose-sans-tenir) | 2026-10-07 | Planches plus visibles ; poser sans tenir ; sans clou 1 coup, clouée 3 coups | Gameplay | Râtelier au mur sud ; nouvelles règles de planche |
| [032](#032--zombies-morts-au-dessus-du-sol) | 2026-10-07 | Les zombies meurent 1 m au-dessus du sol | Bug | Hauteur du clip de mort gardée dans la pose |
| [033](#033--intégration-du-menu-daccueil-pr-81) | 2026-10-07 | Intégrer la PR #81 (menu d'accueil), vérifier conflits et qualité | Revue | Fusion locale ; 2 bugs corrigés (compilation, chute du joueur) |
| [034](#034--brouillard-du-menu-menu-plus-large-prise-par-clic) | 2026-10-07 | Brouillard du menu trop fort, menu trop étroit ; prise d'objet par clic au lieu de maintenir grip | Menu, interactions | Brouillard allégé, panneau élargi, `ClickGrab` (un clic prend, un clic lâche) |
| [035](#035--pluie-et-brouillard-réduits-musique-du-menu) | 2026-10-07 | Réduire pluie et brouillard du menu ; musique fournie en musique du menu | Menu, audio | Pluie 450 gouttes/s, brouillard très léger, `Musique_Menu.mp3` en boucle |
| [036](#036--musique-du-menu-en-boucle) | 2026-10-07 | Menu bugué ; musique du menu en boucle tant que la scène est active | Menu, audio | Musique à une piste : boucle simple sur une seule source ; bug du menu non identifié (question restée sans réponse) |
| [037](#037--contour-des-objets-interactifs) | 2026-10-07 | Contour autour des objets avec lesquels on peut interagir | Interactions, rendu | `InteractableOutline` + `Outline.shader` : liseré discret au repos, épais au survol de la main, masqué en main |
| [038](#038--test-du-décor-extérieur-dalban-branche-features_exterieur) | 2026-10-07 | Ajouter pour test le décor extérieur de la branche `features_exterieur` | Décor, revue | Branche locale `test/exterieur` : décor extrait en prefab, placé par le générateur, ciel en skybox, brouillard 0,03 |
| [039](#039--sphère-de-ciel-dalban-seule) | 2026-10-07 | Retirer la forêt, garder seulement la sphère de ciel d'Alban | Décor | Forêt, montagnes, pins et lune retirés ; sphère de ciel sans brouillard qui suit la tête ; brouillard d'origine (0,12) |
| [040](#040--nouvelle-carte-liaisons-ambiance-plus-sombre) | 2026-10-07 | Remplacer la carte par nouvellemap.blend, vérifier les liaisons, assombrir l'ambiance | Carte, lumière | Fichier identique à la carte déjà importée ; liaisons vérifiées ; lumières ×0,6, ciel ×0,4 |
| [041](#041--chargeurs-tir-dans-la-tête-pied-de-biche-et-hache) | 2026-10-07 | 12 chargeurs dans la carte, se tirer une balle dans la tête, pied de biche et hache utilisables comme armes | Gameplay | `BuildMagazines`, `Pistol.AimedAtOwnHead` + `PlayerHealth.Kill`, `MeleeWeapon` |
| [042](#042--menu-pause) | 2026-10-08 | Menu pause sur le bouton menu de la manette gauche, style du menu d'accueil : Reprendre, Paramètres, Menu | Menu | `PauseMenu` + base commune `AmberMenu` ; temps figé, son en pause |
| [043](#043--chargeurs-aléatoires-hache-du-bunker) | 2026-10-08 | Apparition aléatoire de chaque chargeur ; hache sous la table du bunker utilisable comme arme | Gameplay | `RandomSpawn` (50 % par emplacement), `Hache_Bunker` |
| [044](#044--carte-finale-jardin) | 2026-10-08 | Importer et utiliser map_finale.blend en respectant les lumières et l'ambiance sombre | Carte, lumière | Jardin, salle du générateur agrandie ; exposition de Blender appliquée aux lumières ; collisions, chargeurs et matériaux corrigés |
| [045](#045--disjoncteur-caméras-zombies-portes-toit) | 2026-10-08 | Disjoncteurs, caméras trop sombres, zombies (2 fenêtres, plus nombreux, vitesse aléatoire, bug de focus), portes à tirer barricadables, plus sombre, plateforme du menu invisible, zombies par le toit | Gameplay, rendu | `Breaker`, CCTV vision nocturne, `HingedDoor`, passages réservés (zone NavMesh 3), trous du toit, lumières ×0,32 |
| [046](#046--rendu-proche-de-cycles-éclairage-précalculé) | 2026-10-08 | Graphismes aussi développés que le rendu Blender (captures Cycles), ambiance très sombre | Rendu | Éclairage précalculé (lightmaps + sondes), textures procédurales cuites, conversion des lumières corrigée, calage sur des rendus Cycles |
| [047](#047--volume-des-zombies-musique-plus-forte-en-boucle) | 2026-10-08 | Réglage du volume des zombies ; musique en jeu plus forte et en boucle | Son | Curseur « Zombies », `MusicPlayer` simplifié (boucle sûre), musique indépendante du volume des effets |
| [048](#048--carte-map_final_final) | 2026-10-08 | Remplacer la carte par `map_final_final.blend` en respectant ses éléments | Carte | Export + import, collisions et points d'apparition adaptés au terrain, éclairage recuit |
| [049](#049--fin-de-limport-de-la-nouvelle-carte) | 2026-10-08 | Finir proprement l'import de la nouvelle carte | Carte | Vérification en jeu : zombies (2 fenêtres + toit), portes, disjoncteur, armes, chargeurs |
| [050](#050--push-de-la-branche-carte) | 2026-10-08 | Pousser le travail | Git | Branche `feature/map-finale` poussée (5 commits) |
| [051](#051--analyse-de-la-pr-86-tutoriel) | 2026-10-08 | Importer et analyser la PR #86 d'Al Amine (tutoriel, prise en main, vagues) | Revue | Ne compile pas en l'état ; tutoriel fonctionnel après 4 corrections ; conflit avec la prise par clic |
| [052](#052--schéma-des-commandes-quest-3) | 2026-10-09 | Schéma visuel des commandes des manettes Quest 3 | Doc | PNG sur le bureau, commandes relevées dans le code |
| [053](#053--plans-de-la-carte-vue-de-dessus-et-34) | 2026-10-09 | Plan de la carte vu de dessus et en 3/4 (style terminal) | Doc | 2 PNG : plan schématique et rendu Blender isométrique légendé |
| [054](#054--planche-set-dress) | 2026-10-09 | Image représentant le set dress | Doc | Planche de 6 pièces rendues dans Blender avec l'inventaire du décor |
| [055](#055--tutoriel-intégré-et-contour-géant-des-écrans) | 2026-10-09 | Tutoriel absent au casque ; forme jaune devant les écrans CCTV | Correctif | PR #86 fusionnée et corrigée ; contours des objets regroupés (static batching) corrigés |

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

### 025 — Revue des pull requests #69, #79, #77 (rien poussé dans le main)

**Prompt**

> Regarde les pull request : [6.4][6.5][7.4] Points, écran de score au mur, écrans de début et de fin / [#74] Le zombie enjambe la fenêtre au lieu de traverser le mur / Feature/7.2 effet cctv- #77. Vérifie le fonctionnement de chacune l'une après l'autre, et vérifie qu'il n'y ait pas de conflicts (ne push rien dans le main pour l'instant)

**Résultat**

- Chaque PR a été testée seule, sur une branche locale jetable fusionnée avec le main (branches supprimées, **rien poussé**), dans l'éditeur uniquement : **rien n'a été testé au casque**.
- **#69 (score, écrans)** : **bloquante**. `VRInput.AnyTrigger()` appelle `Input.GetKey` (ancien système) alors que le projet est en « Input System » seul : exception à chaque image, l'écran de début ne se ferme jamais, le jeu ne démarre pas. Avec `Keyboard.current.spaceKey.isPressed` à la place, plus d'erreur, `BeginGame` et le score fonctionnent. Les `.meta` des nouveaux scripts ne sont pas dans la PR.
- **#79 (enjambe la fenêtre)** : OK. Le zombie arrive de l'extérieur, se hisse (hauteur max 0,96 m), retombe à l'intérieur et repart ; aucune erreur. Traversée ≈ 4,5 s.
- **#77 (CCTV)** : **bloquante**. `CCTV.shader` nomme une variable `line` (mot réservé HLSL) : erreur de compilation, écrans magenta. Renommée `scan` : l'effet s'affiche (vert, grain, lignes, « CAM 01 COULOIR GAUCHE », date et heure).
- **Conflits** : #69, #79, #77 et #78 fusionnent proprement ensemble. **#67 (planches)** est en conflit avec le main, #69 et #78 sur `RoomBuilder.cs` et `WaveManager.cs` ; `PLANNING.md` est aussi touché par toutes les PR.
- Les correctifs de test (2 lignes) ne sont pas appliqués aux PR : à faire par leurs auteurs.

### 026 — Revue bugs et optimisation (rien poussé)

**Prompt**

> Vérifie qu'il n'y a pas de bugs, pas de problèmes d'optimisation. Ne push toujours rien sur github, pour l'instant l'idée est de vérifier que la structure est bonne et optimisée

**Résultat** (revue du code + mesures dans l'éditeur ; **rien mesuré sur le Quest**)

- Aucune erreur de compilation ni en jeu (vagues, 6 zombies, mort du joueur et rechargement de la scène). Listes statiques bien vidées au rechargement ; textes CCTV invisibles pour les autres caméras ; calque 31 libre ; rendu stéréo « single pass instanced ».
- **Réglages Quest** : un seul niveau de qualité (« PC ») exclu d'Android ; le Quest utilise donc `PC_RPAsset` (échelle de rendu 1, ombres des lumières additionnelles) et `Mobile_RPAsset` ne sert pas.
- **Lumières** : 26 lumières temps réel, rien de précalculé, 4 lumières par objet au maximum ; `Light_Bunker_Workbench` (point) a des ombres douces = 6 rendus d'ombre par image ; les 357 objets projettent des ombres.
- **Textures** : 4 textures du zombie en 4096 px (1024 suffit au casque).
- **Rendu** : une caméra de surveillance rendue à chaque image même quand personne ne regarde les écrans ; caméra du joueur à 1000 m de portée ; occlusion culling non précalculé.
- **Code** : `ZombieAnimationBridge` lit `animator.parameters` (copie de tableau) à chaque appel ; texte de statut du `WaveManager` et horloge CCTV reconstruits à chaque image ; `Zombie.ChooseOpening` relancé à chaque image si aucune ouverture n'est accessible.
- Rien n'a été corrigé : liste et ordre proposés dans la réponse.

### 027 — Corrections d'optimisation (rien poussé)

**Prompt**

> Oui

**Résultat** (commits locaux, rien poussé ; vérifié dans l'éditeur, **pas au casque**)

- **Niveau de qualité « Quest »** (`Mobile_RPAsset`), par défaut sur Android : plus d'ombres des lumières secondaires (l'ombre douce de `Light_Bunker_Workbench` coûtait 6 rendus par image), échelle de rendu 0,8. Le niveau PC ne change pas.
- **Textures** : 15 textures limitées à 1024 px sur Android (dont les 4 du zombie en 4096 px).
- **Caméras de surveillance** : rendues seulement quand un écran du pupitre est visible ou que la vue plein écran est ouverte (mesuré : 60 rendus sur 60 images vue ouverte, 0 vue fermée).
- **Caméra du joueur** : portée 60 m au lieu de 1000 m.
- **Code** : paramètres d'animation lus une fois, texte de statut et horloge CCTV reconstruits seulement quand ils changent, recherche d'ouverture limitée à une fois par seconde.
- **`.meta` ajoutés** pour les fichiers des PR #79 et #77 (absents des PR).
- **Occlusion culling essayé puis retiré** : il coupait aussi les lumières de lune placées dehors, les murs du bunker devenaient sombres.
- **Non fait, choix de l'équipe** : précalculer l'éclairage (26 lumières temps réel, 4 par objet au maximum sur le Quest).
- Page de wiki non mise à jour : le wiki est sur GitHub, et rien ne doit y être poussé pour l'instant.

### 028 — Image noire au casque

**Prompt**

> (capture Meta Casting, image presque noire) Sans les lights le jeu est actuellement noir, il y a pas de render

**Résultat**

- Cause probable : le niveau « Quest » ajouté en 027 utilisait `Mobile_RPAsset`, jamais utilisé dans le projet (renderer en Forward au lieu de Forward+, ancienne version de fichier, Native Render Pass). Le noir ne se reproduit pas dans l'éditeur : la cause exacte au casque n'est pas confirmée.
- Correction : nouveau `Quest_RPAsset`, copie exacte du pipeline PC (celui qui rendait bien au casque) avec seulement 4 réglages d'ombre en moins (ombres des lumières secondaires, ombres douces, 1 cascade, carte d'ombre 1024).
- Commit local, rien poussé. **À vérifier au casque.**

### 029 — Écran noir, son seul

**Prompt**

> (capture Meta Casting, image noire) J'ai encore un écran noir, mais les sons etc fonctionnent

**Résultat**

- Logs du casque (`adb logcat`) : `NullReferenceException` dans `GPUResidentDrawer` à la création du pipeline URP, puis « Blitter is already initialized » à chaque image. Le pipeline ne se crée jamais : rien n'est dessiné, le reste du jeu (son) tourne.
- Le **GPU Resident Drawer** est activé dans `PC_RPAsset` et donc dans sa copie `Quest_RPAsset`. Désactivé dans `Quest_RPAsset`.
- Vérifié sur le casque (builds installés par adb) : avec la correction, plus aucune erreur, 72 images/s, le texte de statut s'affiche. Un build avec l'ancienne configuration de `origin/main` plante de la même façon : le problème existait avant les changements de 027.
- Reste à confirmer casque sur la tête : le décor éclairé était noir sur la capture faite casque posé.

### 030 — Écran noir avec les textes

**Prompt**

> (2 captures Meta Casting : noir, seuls les textes) Encore un écran noir mais avec les textes

**Résultat**

- Diagnostic temporaire au casque : niveau de qualité **« High »** et pipeline **PC_RPAsset**, 0 objet dessiné. Le **Build Profile « Meta Quest »** (actif) remplaçait la qualité du projet par « Mobile » et « Meta Quest (Build Profile) », niveaux qui n'existent pas ; le casque retombait sur le pipeline PC, dont le GPU Resident Drawer plante (le profil retire aussi les variantes « BRG » au build). C'était aussi la cause en 028 et 029.
- Correction : le profil utilise le niveau « Quest » (`Quest_RPAsset`). Vérifié au casque : bunker, mains, lumières et textes affichés, aucune erreur.
- GPU alors à 98 % (54 à 64 images/s) : **rendu fovéal fixe** activé au lancement (`QuestFoveation.cs`). Mesuré : 72 images/s, GPU à environ 60 % (vue moins chargée que la mesure précédente : comparaison approximative).
- Commits locaux, rien poussé. Diagnostic retiré.

### 031 — Planches visibles et pose sans tenir

**Prompt**

> met les planches à un endroit plus visible, et retire le fait que le joueur doit tenir la planche et en même temps taper avec le marteau. Il pose la planche, si les clous ne sont pas posé alors quand un zombi tape la planche elle tombe directement, si elle a les clous alors il lui faut 3 coups pour casser la planche

**Résultat**

- **Râtelier** : les 6 planches de réserve (avant à plat au sol sous les fenêtres) sont sur le mur sud, entre les deux fenêtres, 3 rangées de 2 à 1,0 / 1,32 / 1,64 m, avec des supports métalliques. Elles restent accrochées jusqu'à ce qu'on les prenne.
- **Pose** : lâchée sur l'emplacement, la planche reste en place (plus de planche qui tombe ou qui pend par un clou, plus de zone « tenir avec l'autre main »). Tant qu'aucun clou n'est enfoncé, on peut la reprendre.
- **Clous** : se plantent au marteau sans tenir la planche.
- **Zombie** : planche sans clou = tombe au 1er coup ; planche avec au moins un clou = 3 coups. **Choix de l'IA** : un seul clou suffit pour compter comme clouée ; un coup toutes les 2 s (avant : une planche arrachée toutes les 3 s) ; la planche recule à chaque coup.
- Vérifié dans l'éditeur : pose sans tenir, 1 coup / 3 coups, zombie qui casse 1 planche clouée + 1 non clouée puis entre ; râtelier immobile. **Pas testé au casque.**
- **Conflits à prévoir** avec les PR #78 (dégradation des barricades : `Plank.cs`, `Opening.cs`, `Nail.cs`, `Zombie.cs`) et #67 (approvisionnement en planches : `Plank.cs`, `RoomBuilder.cs`).

### 032 — Zombies morts au-dessus du sol

**Prompt**

> Les zombis meurent 1m au desus du sol, patch ça ils sont senser tomber au sol

**Résultat**

- Mesuré avant : corps couché mais bassin à 0,92 m, tête à 0,84 m après l'animation de mort.
- Cause : le clip Mixamo de mort était importé avec la hauteur extraite en *root motion* (`lockRootHeightY = false`), que l'Animator ignore (`applyRootMotion = false`) : la descente vers le sol était perdue. Corrigé dans `ZombieMixamo.cs` : hauteur gardée dans la pose pour les 3 clips.
- Mesuré après : bassin à 0,17 m, tête à 0,08 m (tué en marchant et tué pendant l'escalade de la fenêtre). Dans l'éditeur seulement.

### 033 — Intégration du menu d'accueil (PR #81)

**Prompt**

> On va intégrer la toute dernière pull request : Menu d'accueil : décor animé (pluie, brouillard, éclairs, néon), Solo. De Al Amine. Vérifie les conflicts, vérifie si tout est bien fait
> (puis) Dès mon apparition dans le menu home je suis tombé, c'est bien genre une scène à part de la map avec les zombis etc ?

**Résultat**

- **Conflits** : aucun avec le `main` local ni avec #69 et #78 (`UiKit.cs` et `VRInput.cs` identiques à ceux de #69, `VRInput` déjà corrigé pour l'Input System).
- **Bug 1, bloquant** : `GameSettings.cs` ne compilait pas sous Unity 6000.6 (`Scene.handle` n'est plus convertible en `int`) : tout le projet ne compilait plus. Corrigé (`HashSet<Scene>`).
- **Bug 2** : le joueur tombait dès l'arrivée dans le menu (collider du sol supprimé, gravité du rig XR active). Corrigé : sol avec collision, gravité et déplacements coupés dans le menu (rotation gardée). Vérifié : le rig reste à y = 0.
- **`.meta`** des 8 nouveaux fichiers absents de la PR : ajoutés.
- Le menu est une **scène séparée** (`Menu.unity`, 1re du build) ; SOLO charge `MainScene`.
- Vérifié dans l'éditeur : décor, 4 boutons, page Paramètres, difficulté « Difficile » appliquée dans le jeu (zombies 5/8/10/13, intervalle 9,6 s, vitesse ×1,15), volume de la musique inchangé, aucune erreur. Build Quest réussi ; **pas encore vu au casque** (performances de la pluie et du brouillard à vérifier).
- Fusion dans le `main` local, rien poussé.

### 034 — Brouillard du menu, menu plus large, prise par clic

**Prompt**

> Actuellement le brouillard est trop fort au niveau du menu d'accueil et le menu n'est pas assez large pour bien pouvoir lire / selectionner les paramètres
> Retire le fait qu'il faut forcément gardé enfoncé la touche sur le côté du joystick pour tenir un objet : Une fois on click pour tenir l'objet, une autre fois pour lacher l'objet

**Résultat** (code écrit par l'IA, Claude Code)

- **Brouillard** (`MenuBuilder.cs`) : densité du brouillard de 0,03 à 0,012 ; nappes 2,5 fois plus transparentes (0,22 → 0,09), plus basses et plus loin (6 / 9 / 12 m au lieu de 5 / 7,5 / 10 m).
- **Menu** (`MainMenu.cs`) : panneau élargi de 1,04 m à 1,52 m (et agrandi ×1,1) ; libellés, curseurs (segments plus grands) et boutons de choix plus grands, boutons de choix assez larges pour leur texte. Vérifié par capture dans l'éditeur.
- **Prise par clic** : nouveau `ClickGrab.cs` sur les deux mains (ajouté par `SetupVRScene`) : grip en mode « bascule » (XRI `InputTriggerType.Toggle`), un appui prend l'objet, un 2e appui le lâche. Les écrans de surveillance (simples boutons) sont relâchés aussitôt pour ne pas rester « tenus ». Interrupteur du générateur : un appui le lance, il faut garder la main dessus (`Generator.cs` coupe la sélection si la main s'éloigne).
- Testé dans l'éditeur en simulant le bouton grip : marteau pris au 1er clic, lâché au 2e ; écran CCTV ouvert puis relâché ; générateur actionné, relâché quand la main s'éloigne. **Pas testé au casque.**
- Rien poussé.

### 035 — Pluie et brouillard réduits, musique du menu

**Prompt**

> (fichier joint : menu_principale.mp3) Dans le menu d'accueil, réduit l'intensité de la pluie et du brouillard. Et met la musique je te donne en musique de menu d'accueil

**Résultat** (code écrit par l'IA, Claude Code ; musique fournie par l'utilisateur)

- **Pluie** (`MenuBuilder.cs`) : 450 gouttes/s au lieu de 1100 (1000 particules max au lieu de 2500), gouttes plus transparentes (0,35 → 0,22) ; son de la pluie baissé (0,35 → 0,18, `MenuAmbience.cs`) pour laisser la musique devant.
- **Brouillard** : densité 0,012 → 0,006, nappes 0,09 → 0,05 d'opacité.
- **Musique** : `Assets/Audio/Music/Musique_Menu.mp3` (1 min 36, via Git LFS), jouée en boucle sans blanc par `MusicPlayer` (le même que dans le jeu), en streaming Vorbis ; son volume suit le curseur Musique des paramètres. Seulement dans le menu : la scène de jeu garde ses deux musiques.
- Vérifié dans l'éditeur : la musique joue au lancement du menu, ~400 gouttes à l'écran, capture du décor plus dégagée. **Pas testé au casque.**
- Rien poussé.

### 036 — Musique du menu en boucle

**Prompt**

> Le menu d'accueil est bugué actuellement (et fait la musique d'accueil loop tant que la scène est active)

**Résultat** (code écrit par l'IA, Claude Code)

- **Musique** (`MusicPlayer.cs`) : avec une seule piste (le menu), la source joue en boucle simple (`loop`) au lieu d'enchaîner la piste sur deux sources ; un clip lu en streaming ne doit pas être joué par deux sources à la fois. La musique tourne tant que la scène du menu est active et s'arrête au chargement du jeu.
- **« Menu bugué »** : logs du casque relus (aucune exception, 72 fps) ; le bug n'a pas pu être identifié, la question posée à l'utilisateur est restée sans réponse. À préciser.

### 037 — Contour des objets interactifs

**Prompt**

> Fait en sorte que si ce sont des objets avec lequels on peut inerragir il y a un outline qui forme le contour de l'objet

**Résultat** (code écrit par l'IA, Claude Code)

- `Assets/Scripts/InteractableOutline.cs` : ajouté automatiquement à chaque chargement de scène sur tous les objets interactifs (`XRGrabInteractable`, `XRSimpleInteractable`) : planches, marteau, pistolet, chargeur, lampe, piles, écrans de surveillance, interrupteur du générateur (36 objets dans `MainScene`). Pas sur les clous ni sur la vue plein écran des caméras.
- Au repos : liseré ambre fin (2,5 mm) et sombre ; main à portée : liseré épais (6 mm) et lumineux ; objet tenu en main ou interaction coupée (planche clouée, autre planche en main) : pas de contour. Un objet rangé dans un support (ceinture) garde son contour.
- `Assets/Resources/Outline.shader` : copie du maillage gonflée le long de normales lissées (arêtes des planches fermées), faces arrière seulement, compatible rendu stéréo du Quest.
- Vérifié dans l'éditeur par captures (repos / survol). **Pas testé au casque** (coût : une copie de maillage dessinée en plus par objet, ~50 petits maillages).
- Rien poussé.

### 038 — Test du décor extérieur d'Alban (branche features_exterieur)

**Prompt**

> Il y a une branche qui s'appelle : features_exterieur. Elle comporte notament un décor d'exterieur pour la map jouable, rajoute le qu'on fasse un test

**Résultat** (intégration faite par l'IA, Claude Code ; décor fait par Alban)

- **Branche analysée** : un seul commit utile (a6501cf, Alban) ; aucun script, le décor est posé à la main dans `MainScene.unity`. La branche part d'un `main` ancien et contient aussi un autre historique (TextMesh Pro, rig XR, réglages OpenXR et ProjectSettings modifiés). Fusion directe : conflit sur `MainScene.unity` (binaire, générée par code) et sur 4 matériaux générés ; elle aurait écrasé la scène générée.
- **Intégration de test** sur une branche locale `test/exterieur` (pas dans `main`) :
  - `Assets/Environment/` repris tel quel de la branche (matériaux, textures, maillages) ;
  - objets ajoutés par Alban extraits de sa scène dans `Assets/Environment/Exterieur_Alban.prefab` : terrain en relief, 4 montagnes, pins, lune (lumière directionnelle), cadres/plaques/voyants des 4 portes de niveau ;
  - `RoomBuilder.BuildExteriorDecor` place le prefab dans la scène générée et rattache la déco des portes au battant de chaque porte.
- **Changements nécessaires pour le voir** : avec notre brouillard (densité 0,12) tout disparaissait au-delà de ~15 m et la sphère de ciel (240 m) restait noire. Ciel remplacé par son matériau skybox (panorama de forêt de nuit), brouillard 0,12 → 0,03 (l'intérieur du bunker est donc moins « noyé »).
- Vérifié dans l'éditeur par captures : extérieur (ciel étoilé, relief, pins, montagnes), intérieur du bunker, portes. **Pas testé au casque.**
- **Points à surveiller** : 395 maillages et 44 000 triangles en plus (marqués statiques pour le regroupement), lune avec ombres douces sur tout le décor : coût GPU à mesurer au Quest ; pins faits de cylindres empilés ; la plaque « SECTEUR CONFINÉ » est derrière le panneau rouge du niveau (chevauchement).
- Rien poussé.

### 039 — Sphère de ciel d'Alban seule

**Prompt**

> retire l'espèce de foret que tu as généré, juste garde la sphère de skybox de Alban

**Résultat** (code écrit par l'IA, Claude Code ; sphère et panorama faits par Alban)

- Précision : la forêt (terrain en relief, montagnes, pins) venait de la branche d'Alban, elle n'avait pas été générée par l'IA ; elle est retirée du prefab, ainsi que sa lumière de lune (qui servait à éclairer la forêt). Assets inutilisés supprimés (sol, roche, pins, maillage du terrain, skybox, texture de porte métallique).
- **Sphère de ciel gardée** : `Assets/Environment/Shaders/SkySphere.shader` (non éclairé, sans brouillard, dessiné derrière tout le reste) et `Assets/Scripts/SkyFollow.cs` (la sphère suit la tête du joueur, rayon ramené à 50 m pour rester sous la distance d'affichage de 60 m). Sans ça, notre brouillard dense la rendait noire.
- **Brouillard d'origine** remis (0,12), pas de skybox : l'ambiance de l'intérieur redevient celle d'avant.
- Gardé aussi : cadres, plaques et voyants des portes de niveau (pas liés à la forêt).
- Vérifié dans l'éditeur : ciel visible dehors, par la fenêtre et par le trou du toit ; la sphère suit la tête en mode Play ; aucune erreur. **Pas testé au casque.**
- Toujours sur la branche locale `test/exterieur`, rien poussé.

### 040 — Nouvelle carte, liaisons, ambiance plus sombre

**Prompt**

> (fichier joint : nouvellemap.blend) Remplace la carte actuelle par celle-ci. Veille bien a ce qu'il n'y ai pas de bug de liaisons etc. Rassombri un peu encore l'ambiance globale, c'était encore trop claire

**Résultat** (vérifications et code par l'IA, Claude Code)

- **Carte** : `nouvellemap.blend` est identique octet pour octet (même MD5) à `LastBoardStanding_Kitchen (1).blend`, déjà importé le 7 octobre au matin (commit e69f594). Vérifié aussi par Blender : mêmes 26 lumières, 4 caméras, mêmes positions. Réexporter n'aurait rien changé : rien n'a été réimporté.
- **Liaisons vérifiées** (noms que le générateur utilise) : `Generator`, `Generator_Switch`, `Bunker_Hammer_Workbench`, `Plank_01`, `Light_Bunker_Workbench`, `Light_GeneratorBulb`, objets retirés (`Corridor_Door`, `Door_Boarded_Planks`, `Hammer`, `Stockage_Hammer`), 6 `Bunker_PlankSlot_*`, 17 planches, 4 caméras CCTV ayant chacune leur écran. Reconstruction de la scène sans aucun avertissement.
- **Ambiance** (`RoomBuilder.cs`) : `LightLevel = 0.6` (toutes les lumières de la carte à 60 %) et `SkyLevel = 0.4` (ciel vu par les fenêtres et le toit). Mesuré à 0,7 : image ~12 % plus sombre, d'où le passage à 0,6.
- La connexion MCP avec Unity s'est coupée avant la reconstruction à 0,6 : **scène à reconstruire** (SAE501 → Build VR Scene). **Pas testé au casque.**
- Sur la branche locale `test/exterieur`, rien poussé.

### 041 — Chargeurs, tir dans la tête, pied de biche et hache

**Prompt**

> Rajoute 2 chargeurs posés dans le bunker (la salle principale) un à côté des écrans des caméras et l'autre au niveau de la table. Rajoute également 2 chargeurs dans la cuisine, 3 dans la salle du générateur et 5 dans la salle de stockage. Rend également possible au joueur de se suicider (se tirer une balle dans la tête). Rend la crowbar (pied de biche) qui est posé dans le bunker utilisable comme une arme pour taper les zombis. Pareil pour la hache qui se trouve dans la salle de stockage

**Résultat** (code écrit par l'IA, Claude Code)

- **Chargeurs** (`RoomBuilder.BuildMagazines`) : 12 chargeurs pleins (12 balles) couchés sur des meubles de la carte : bunker (pupitre des caméras, établi), cuisine (plan de travail ×2), générateur (établi ×2, caisse), stockage (établi ×2, 3 caisses). Vérifié en mode Play : les 12 restent sur leur surface (les 2 de la cuisine tombaient au début : plan de travail orienté autrement, positions corrigées).
- **Tir dans la tête** (`Pistol.AimedAtOwnHead`, `PlayerHealth.Kill`) : si le canon est à moins de 35 cm de la tête et pointé vers elle (angle < 35°) au moment du tir, le joueur meurt (écran de fin habituel). Testé : tir devant soi et arme près du visage pointée devant = pas de mort ; canon contre la tempe = mort (PV 0).
- **Armes de corps à corps** (`MeleeWeapon.cs`, `RoomBuilder.BuildMelee`) : le pied de biche de l'établi du bunker et la hache du stockage deviennent des objets à prendre (contour, prise par clic). Un coup compte si l'extrémité (crochet, fer) touche une zone du zombie à plus de 1,6 m/s ; une fois par coup et par zombie (0,45 s). Dégâts en part des PV de base : pied de biche tête 50 % / torse 25 % / jambes 15 % ; hache tête 100 % / torse 50 % / jambes 33 %. Son de choc et vibration de la manette. Testé : pied de biche au torse 100 → 75 PV ; hache à la tête = mort ; arme poussée lentement contre le zombie = pas de dégât.
- Scène reconstruite (avec l'ambiance à 60 % de l'entrée 040), aucun avertissement. **Pas testé au casque** (force réelle d'un coup, prise en main des armes).
- Sur la branche locale `test/exterieur`, rien poussé.

### 042 — Menu pause

**Prompt**

> Créer un menu pause réglé sur le bouton menu de la manette gauche, pour le style reprend ce qui est fait pour le menu d'accueil du jeu, et les boutons c'est : Reprendre, Paramètre, Menu

**Résultat** (code écrit par l'IA, Claude Code)

- `Assets/Scripts/PauseMenu.cs` (ajouté à la scène de jeu par `RoomBuilder`) : bouton menu (☰) de la manette gauche (Échap dans l'éditeur) pour ouvrir / fermer. Panneau ambre « BUNKER-OS // PAUSE » : REPRENDRE, PARAMÈTRES (même page que l'accueil : son, confort VR, difficulté), MENU (retour à la scène d'accueil). Rayon de la manette + gâchette, comme l'accueil.
- Pendant la pause : temps figé (`Time.timeScale = 0` : zombies, vagues, physique), son en pause, tir, coups d'arme et clous bloqués. Le panneau apparaît devant le joueur, dans la direction regardée, et passe devant le décor et les zombies.
- `Assets/Scripts/AmberMenu.cs` : base commune extraite de `MainMenu` (panneau, gros boutons, page Paramètres, curseurs, choix) pour que les deux menus aient exactement le même style. `MenuPointer` : le rayon ignore ce qui n'est pas un bouton (arme tenue, mur proche).
- Corrigé au passage : le titre de la bande du haut du menu d'accueil (« BUNKER-OS // MENU », « v0.9 ») était caché par la bande (ordre d'affichage) ; il s'affiche.
- Testé dans l'éditeur : pause (temps 0, son en pause, zombie immobile sur 120 images), rayon qui atteint REPRENDRE, PARAMÈTRES puis RETOUR, REPRENDRE (le zombie repart), MENU (scène d'accueil chargée, temps et son rétablis) ; captures. **Pas testé au casque** (bouton ☰ réel de la manette gauche).
- Limite : changer la difficulté en pleine partie ne s'applique qu'à la partie suivante.
- Sur la branche locale `test/exterieur`, rien poussé.

### 043 — Chargeurs aléatoires, hache du bunker

**Prompt**

> Je t'ai déjà décrit les spawn points des chargeurs, randomize maintenant la chance d'apparition de chacun (Y a une ache dans le spawn sous la table, rend la utilisable pour tapper les zombis également)

**Résultat** (code écrit par l'IA, Claude Code)

- `Assets/Scripts/RandomSpawn.cs` : à chaque partie, chacun des 12 emplacements de chargeur garde son chargeur avec une probabilité de 50 % (`RoomBuilder.MagazineChance`), sinon il est retiré. Le chargeur de la ceinture est toujours là. Testé : 5, 5 puis 9 chargeurs présents sur 3 parties, à des emplacements différents.
- La hache posée au sol sous le bord de l'établi du bunker (`Bunker_Axe_Stove`) devient une arme comme celle du stockage (`Hache_Bunker` : tête = mort, torse 50 %, jambes 33 %), avec contour et prise par clic. Vérifié : elle reste au sol, saisissable.
- **Pas testé au casque.** Rien poussé (en attente de l'accord).

### 044 — Carte finale (jardin)

**Prompt**

> (capture de Blender + fichier map_finale.blend) Importe et utilise cette map, respect les lumière et l'ambiance sombre (exemple sur l'image)

**Résultat** (import et code par l'IA, Claude Code ; carte faite dans Blender par le groupe)

- **Ce qui change dans la carte** (comparé objet par objet à l'ancienne) : 88 objets ajoutés (jardin à l'ouest derrière la cuisine : terrain vallonné, haies, 8 arbres, cabane, table de pique-nique, portail de sortie avec cadenas, clé cachée ; salle du générateur agrandie à 8,6 × 7,5 m avec racks, fûts, tuyaux), 3 retirés, 14 déplacés (établi et caisse du générateur). 31 lumières. Le jardin s'ouvre par la porte arrachée du mur ouest de la cuisine (zone du niveau 2).
- **Lumières respectées** : la scène Blender rend avec Cycles, un monde noir et une exposition de +2,5 ; les énergies des lumières ont été baissées en conséquence (lampes ×0,22, lunes ×0,08). L'export transmet maintenant l'exposition (`map_meta.json`) et `RoomBuilder` l'applique (×2^2,5) : lampes proches d'avant, lunes nettement plus sombres, comme dans Blender. Niveau général gardé à 60 %. Portée des spots selon leur hauteur (lune du jardin à 14 m). Les deux lumières de la salle du générateur restent presque éteintes jusqu'à son démarrage (`Generator.roomLights`).
- **Couleurs du jardin** : ses matériaux sont procéduraux (bruit + rampe de couleur), l'export les sortait en blanc ; il prend maintenant la couleur moyenne de la rampe (sol vert-brun, feuillage vert sombre, allée grise).
- **Collisions** : terrain, allée, haies, rochers, piliers du portail, troncs et tuyaux du générateur ont leur forme exacte (une boîte englobante aurait bloqué le jardin entier, fait flotter le joueur au-dessus du terrain et rempli la salle du générateur) ; feuillage, toit de la cabane, flaques et lianes sans collision.
- **Chargeurs** : les 3 emplacements de la salle du générateur suivent le nouvel établi et la caisse déplacée. **Armes** : le pied de biche de la salle du générateur et la hache de la cabane du jardin sont aussi utilisables.
- **Bug corrigé** : après un réimport de la carte, toutes les planches de jeu étaient roses (matériau gardé en mémoire d'une reconstruction à l'autre) ; remis à zéro à chaque reconstruction.
- Export : 1,5 million de triangles dans Blender, ~700 000 après la simplification automatique des objets de décor (90 objets). Vérifié dans l'éditeur : aucun avertissement, aucun matériau manquant, captures du jardin comparées à l'image, sol du jardin, chargeurs posés, générateur qui rallume sa salle. **Pas testé au casque** : performances à mesurer (carte plus lourde).
- Non fait (pas demandé) : la clé cachée et le portail de sortie cadenassé du jardin ne sont pas encore jouables.
- Branche locale `feature/map-finale`, rien poussé.

### 045 — Disjoncteur, caméras, zombies, portes, toit

**Prompt**

> Interraction avec les disjoncteurs (alumer les caméras). Règler les caméras qui sont trop sombres (CCTV). Zombis qui puissent passer par les deux fenêtres. Plus de zombis par vagues. Vitesse aléatoires par zombis. Par moments les zombis vont dans la salle de bunker sans focus le joueur (alors qu'ils devraient). Faire des portes ouvrable en tirant et baricadable. Rassombri encore le jeu, c'est encore trop claire. Rend invisible la plateforme dans le menu (mais garde la pour que le joueur ne tombe pas dans le vide). Dans le bunker y a un trou dans le toit, il faudrait que les zombis puisse arriver par là aussi (rend solide le toit de la carte car actuellement on peu glitch dessus et sortir de la map)

**Résultat** (code écrit par l'IA, Claude Code)

- **Disjoncteur** (`Breaker.cs`) : le boîtier électrique du mur est du bunker se manipule (grip) ; levier et voyant rouge/vert. Coupé au départ : caméras non rendues, écrans noirs (global `_CctvPower`), plein écran impossible. Testé : actionné → caméras allumées.
- **Caméras trop sombres** : `CCTV.shader` en vision nocturne (`1 - exp(-gain·√luminance)`, gain 5) : les zones sombres sont relevées sans saturer. Vérifié par capture : couloirs et portes lisibles en vert.
- **Bug « zombies dans le bunker sans viser le joueur »** : la carte de navigation permettait à un zombie qui visait une porte côté couloir de passer par une fenêtre (sans casser ses planches) et de traverser le bunker. Les liens des fenêtres et du toit sont maintenant en zone NavMesh 3, réservée aux zombies qui entrent (`Zombie.WalkMask`).
- **Les deux fenêtres** : chaque zombie tire au sort parmi les ouvertures à moins de 6 m de plus que la plus proche. Testé avec 12 zombies dehors : 6 sur chaque fenêtre.
- **Plus de zombies** : vagues de 6 / 9 / 12 / 15 (au lieu de 4 / 6 / 8 / 10), apparitions plus rapprochées, 9 zombies max en renfort au dernier niveau. **Vitesse aléatoire** : ×0,75 à ×1,4 par zombie.
- **Portes** (`HingedDoor.cs`) : les 4 portes de niveau sont sur charnière. Verrouillées : la poignée ne répond pas. Déverrouillées : on prend la poignée (grip) et on tire, la porte suit la main ; lâchée, elle reste ouverte. Portes ouest / est du bunker barricadables : planche posée = porte bloquée ; un zombie doit d'abord enfoncer la porte fermée (3 coups) puis casser les planches. Testé : verrouillée 0°, tirée à 84°, barricadée 0°, enfoncée par un zombie en 3 coups puis grande ouverte.
- **Toit** : plafonds et toits ont une collision exacte (on ne passe plus au travers). Deux trous repérés dans le plafond du bunker (au-dessus des lits et de l'établi) : point d'apparition sur le toit (niveaux 1 à 4), le zombie marche jusqu'à un trou et se laisse tomber (chute réaliste), puis attaque. Testé : chute de 2,8 m, attaque du joueur (100 → 80 PV).
- **Plus sombre** : lumières de la carte à 32 % (au lieu de 60 %), ciel à 30 %. Mesuré : image du bunker ~18 % plus sombre.
- **Menu** : la plateforme est invisible, son collider reste (le joueur ne tombe pas).
- Corrigé au passage : avertissement des planches du râtelier posées par code (vitesse d'un corps cinématique).
- **Pas testé au casque.** Limites : seul le disjoncteur du bunker est utilisé (ceux de la salle du générateur restent du décor) ; les portes du stockage et du générateur ne sont pas barricadables (aucun zombie n'y passe).
- Branche locale `feature/map-finale`, rien poussé.

### 046 — Rendu proche de Cycles (éclairage précalculé)

**Prompt**

> (2 captures du rendu Blender Cycles : jardin et pupitre des caméras) Les graphismes sont sensé être autant développé que l'image, et un truc bien sombre comme ça

**Résultat** (code et réglages par l'IA, Claude Code)

- **Constat** : les images viennent de Cycles (calcul de la lumière rebondie, ombres douces), impossible à reproduire en temps réel sur un Quest. Écart mesuré avec Unity : bunker 2,5 fois trop sombre et deux fois trop orange (AgX désature les lumières chaudes), jardin sans texture (matériaux procéduraux sans UV).
- **Éclairage précalculé** (`Assets/Editor/LightBaker.cs`, menu **SAE501 > Cuire l'éclairage**) : lightmaps (rebonds ×3, ombres douces, occlusion ambiante), 25 lampes fixes en « Baked », 8 restent en temps réel (lampes qui vacillent, générateur, lampe de poche). Sondes de lumière (245) pour les zombies, armes et mains. Ciel d'ambiance noir comme dans Blender. Résultat : 2 lightmaps (~3,5 Mo), cuisson ~1 min sur le Mac. **Étapes** : *Build VR Scene* puis *Cuire l'éclairage*.
- **Textures du jardin** (`Tools/blender/export_map.py`) : les 20 objets à matériau procédural (terrain, haies, arbres, rochers, allée) sont dépliés et leur couleur et leur relief sont cuits par Cycles dans des images (39 textures). Gestion des couleurs neutre pendant l'export (vérifié : les textures existantes n'avaient pas été altérées).
- **Conversion des lumières corrigée** : les spots étaient ~11 fois trop faibles (diviseur 275 contre 24 pour les ampoules, alors que Blender les traite pareil) ; couleurs désaturées de 45 % (AgX) ; portée des spots hauts doublée. Niveau général 0,62.
- **Calage sur Cycles** : la même vue rendue par Cycles (avec AgX et l'exposition de la carte) et par Unity, luminosité linéaire mesurée : bunker 0,0117 (Cycles) / 0,0132 (Unity) ; jardin médiane 0,0012 / 0,0015. Comparaison visuelle côte à côte : même pénombre, même flaque de lumière de l'établi, mêmes ombres douces. Zombie en jeu correctement éclairé par les sondes.
- Brouillard allégé (0,12 → 0,06). Différences volontaires : pas de planches aux fenêtres au départ (gameplay), ciel d'Alban au lieu du ciel noir.
- **Pas testé au casque** : à vérifier (rendu, images/s ; l'éclairage précalculé allège le GPU : 8 lumières temps réel au lieu de 33).
- Branche locale `feature/map-finale`, rien poussé.

### 047 — Volume des zombies, musique plus forte et en boucle

**Prompt**

> Rajoute dans les paramètre les sons augmentatble (les zombis). Fait en sorte que la musique ingame soit plus forte et en boucle

**Résultat** (code écrit par l'IA, Claude Code)

- **Curseur « Zombies »** dans PARAMÈTRES > SON (menu d'accueil et menu pause, `AmberMenu`), enregistré (`GameSettings.Zombies`). Il règle le volume des cris et leur portée (`ZombieVoice`) : à 6/10 (défaut) le volume d'avant (0,77), à 10/10 volume maximal et voix entendue de plus loin. Testé : 0,77 → 1,0, portée 4 → 6 m.
- **Musique plus forte** : elle ne passe plus par le volume général des effets (`ignoreListenerVolume`), seul le curseur Musique la règle : 0,96 au réglage par défaut contre 0,35 effectifs avant (≈ ×2,7).
- **Musique en boucle** : `MusicPlayer` réécrit avec une seule source : la piste suivante démarre quand la précédente se termine (1, 2, 1, 2…, sans fin) ; elle attend pendant le menu pause. L'ancienne version programmait les pistes sur l'horloge audio, qui continuait pendant la pause : la musique se décalait ou s'arrêtait. Testé : fin de la piste 1 → piste 2 enchaînée.
- Au passage : fond du menu pause rendu opaque (le bandeau de niveau se lisait au travers) ; GPU Resident Drawer coupé aussi dans le pipeline PC (erreurs en boucle dans l'éditeur avec les sondes de lumière ; il l'était déjà pour le Quest).
- **Pas testé au casque.** Branche locale `feature/map-finale`, rien poussé.

### 048 — Carte map_final_final

**Prompt**

> @map_final_final.blend Utilise cette map là, respect eactement les éléments qu'elle a

**Résultat** (code et réglages par l'IA, Claude Code)

- Carte exportée telle quelle depuis Blender (`export_map.py`) : 492 objets, 31 lumières, 23 objets à matériau procédural cuits en textures (terrain, forêt, route de fuite en pierres). Rien n'a été ajouté ni retiré dans la carte.
- Adaptations du constructeur (`RoomBuilder`) aux nouveaux noms : feuillage de la forêt (« Canopies ») sans collision, route de fuite (« EscapeRoad ») solide, sol plat de secours supprimé quand la carte a son propre terrain, hache de la cabane retirée (absente de cette carte), points d'approche des fenêtres posés sur le terrain en relief.
- `ZombieSpawner` : le zombie apparaît sur le point du NavMesh le plus proche (le terrain est en relief, sinon l'agent pouvait naître sous le sol).
- Éclairage recuit (1 lightmap 1024, 2 729 sondes). Calage sur Cycles : bunker 0,0127 (Cycles) / 0,0138 (Unity), extérieur 0,0015 / 0,0013.

### 049 — Fin de l'import de la nouvelle carte

**Prompt**

> @map_final_final.blend Fini l'import propre de la nouvelle map

**Résultat** (vérifications par l'IA, Claude Code)

- Les zombies qui « disparaissaient » devant les fenêtres : reste d'un script de test de la session précédente, pas un bug du jeu. En partie relancée proprement : 4 zombies (3 dehors, 1 sur le toit) entrent par la fenêtre SE, la fenêtre SW et le trou du toit, puis attaquent le joueur.
- Vérifié en jeu : 4 portes à charnière (dont Ouest/Est barricadables), disjoncteur des caméras, générateur, 4 armes de mêlée (2 pieds-de-biche, 2 haches), chargeurs tirés au sort (6 sur 12 dans cette partie, aucun tombé), aucune erreur dans la console.
- Le dossier `Assets/Protection_suite` (pack importé le 7 octobre) n'est pas inclus dans le commit.
- **Pas testé au casque.** Branche locale `feature/map-finale`, rien poussé.

### 050 — Push de la branche carte

**Prompt**

> Push tout ça proprement

**Résultat** (par l'IA, Claude Code)

- Branche `feature/map-finale` poussée sur GitHub (5 commits, depuis `feature/aex_usable`) après vérification : aucun fichier de plus de 3 Mo, aucun co-auteur. `main` inchangé ; `Assets/Protection_suite` non inclus.

### 051 — Analyse de la PR #86 (tutoriel)

**Prompt**

> Pull request de Al Amine « Tutoriel + prise en main + difficulté des vagues (6.3) » : importe la et analyse la et dit moi ce qu'il en est

**Résultat** (analyse par l'IA, Claude Code ; rien n'a été fusionné)

- Fusion d'essai dans une branche temporaire, puis abandonnée.
- **Ne compile pas**, même sur `main` (sa base) : `Plank.Placed` et `Plank.State.Fixed` n'existent dans aucune branche, `IsSelectedBy` n'existe pas dans XRI 3.6, `CommonUsages` ambigu. 9 erreurs, 4 corrections simples.
- **Tutoriel** (après corrections) : se lance seul, bloque les vagues, panneau lisible, se termine proprement (vagues relancées, mémorisé).
- **GrabGuard** : la « prise ferme » du pistolet remet la main en mode « garder enfoncé » après le lâcher, ce qui annule la prise par clic demandée (prompt 034). Le reste (pas de prise de loin, pas de vol par l'autre main) est utile.
- **Vagues** : conflit avec `feature/map-finale` (6/9/12/15 zombies et le toit contre 4/6/8/12 sans le toit) ; les nouveaux réglages ne s'appliquent qu'après *Build VR Scene*.

### 052 — Schéma des commandes Quest 3

**Prompt**

> (2 images de référence de manettes) Fait moi le schéma de controle visuelle (un PNG que tu vas déposer dans mes documents ou dans mon desktop). Représente les manettes du question 3 et tu fais un trait pour chacun des bouton avec une mini explication / juste 2/3 mots

**Résultat** (par l'IA, Claude Code)

- `Commandes_Quest3_TheLastEscapor.png` (2800×1560) sur le bureau. Commandes relevées dans le code : gâchette = tirer / utiliser, joystick gauche = se déplacer, joystick droit = tourner, grip = attraper / lâcher (prise par clic), bouton menu = menu pause ; A, B, X, Y non utilisés.

### 053 — Plans de la carte (vue de dessus et 3/4)

**Prompt**

> (plan Hadley's Hope et vue isométrique en référence) Génére moi un schématic vue de haut de la map, et une vue en 3/4 (regarde les images de référence)

**Résultat** (par l'IA, Claude Code)

- `Plan_VueDessus_TheLastEscapor.png` : plan vert façon terminal, à l'échelle (dimensions lues dans `map_final_final.blend`), pièces, niveaux d'ouverture, portes, fenêtres, trous du toit, apparitions des zombies, objets (positions de la scène Unity), légende.
- `Plan_Vue3-4_TheLastEscapor.png` : rendu Blender isométrique en traits (Freestyle), plafonds retirés, légendes à traits et cartouche.

### 054 — Planche set dress

**Prompt**

> Génère moi une image afin de représenter le setdress

**Résultat** (par l'IA, Claude Code)

- `SetDress_TheLastEscapor.png` (3600×2440) : 6 pièces (bunker, cuisine, générateur, stockage, couloir ouest, jardin et cabane) rendues dans Blender en coupe 3/4 avec leurs matériaux, nombre d'objets et inventaire du décor par pièce.

### 055 — Tutoriel intégré et contour géant des écrans

**Prompt**

> (2 captures du casque) Le tutoriel n'est acuellement pas fonctionnel. Il y a également un bug d'affichage devant les télévisions (Camera CCTV)

**Résultat** (code par l'IA, Claude Code ; tutoriel et GrabGuard écrits par Al Amine, PR #86)

- **Forme jaune devant les écrans** : régression du prompt 046. Les objets de la carte sont marqués « regroupement statique » pour l'éclairage ; en jeu, le MeshFilter des écrans CCTV et du disjoncteur renvoie alors le maillage fusionné de la carte, et le contour (`InteractableOutline`) l'entourait en entier (≈ 60 m). Correctif : les objets interactifs ne sont plus regroupés (`RoomBuilder.UnbatchInteractables`, éclairage précalculé conservé) et le contour ignore tout maillage regroupé.
- **Tutoriel** : il n'était pas dans la build (PR #86 non fusionnée). Fusionné dans `feature/map-finale` avec : les 4 corrections de compilation, la « prise ferme » du pistolet retirée (elle annulait la prise par clic), textes adaptés à la prise par clic, nouvelle étape « disjoncteur » avant les caméras (écrans noirs sinon), 10 étapes.
- **Vagues** : nos 6/9/12/15 zombies avec le toit, plus la vitesse progressive d'Al Amine (×0,8 → ×1,2) et un maximum de zombies en même temps (3/4/5/7).
- Scène reconstruite et éclairage recuit.
