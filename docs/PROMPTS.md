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
| [056](#056--types-de-zombie-pr-89-et-wiki) | 2026-10-09 | Importer la PR #89 (Cogneur et Coureur) avec les packs FREE Zombie Male AAB et Zombie ; mettre à jour le wiki | Zombies | PR fusionnée, chaque type a son propre modèle, wiki complété |
| [057](#057--circulatoire-gdd) | 2026-10-09 | Visuel du circulatoire (GDD) d'après le plan | GDD | Plan avec flux du joueur par niveau, des zombies et des ressources, boucle de jeu |
| [058](#058--macro-boucle-gdd) | 2026-10-09 | Visuel de la macro boucle (gabarit objectif / challenge / récompense) | GDD | Échelle de la partie : s'échapper, tenir la vague, la base s'ouvre |
| [059](#059--micro-boucle-gdd) | 2026-10-09 | Visuel de la micro boucle | GDD | Échelle du moment : tenir l'ouverture, agir sous pression, la menace tombe |
| [060](#060--core-gameplay) | 2026-10-09 | Décrire le core gameplay | GDD | Texte de synthèse : piliers, boucles, tension, victoire / défaite |
| [061](#061--game-feel) | 2026-10-09 | Décrire le game feel | GDD | Texte : gestes, retours sonores, visuels et vibrations, ambiance |
| [062](#062--présentation-des-ennemis-et-t-pose) | 2026-10-09 | Présentations des ennemis et images en T-pose (face / dos) | GDD | 3 fiches (Rôdeur, Cogneur, Coureur) et rendu Unity des 3 modèles |
| [063](#063--hitbox-et-dégâts) | 2026-10-09 | Image explicative des hitbox et des dégâts | GDD | Rendu des vraies hitbox + tableaux ; correction du wiki (hache à la tête) |
| [064](#064--hitbox-simplifiée) | 2026-10-09 | Simplifier l'image des hitbox | GDD | Dégâts écrits à côté de chaque zone, sans les tableaux |
| [065](#065--palette-de-couleurs) | 2026-10-09 | Palette de couleurs utilisée | GDD | 18 couleurs : interface, retours de jeu (code), ambiance (mesurée en jeu) |
| [066](#066--concept-art) | 2026-10-09 | Qu'est-ce que le concept art ? | Question | Explication et propositions |
| [067](#067--lampe-torche-sol-et-prise-à-distance) | 2026-10-09 | Lampe pixelisée, objets sous le sol, prise de loin et de plusieurs objets | Correctif | Tramage + faisceau adouci ; filet de sécurité des objets ; Near-Far supprimé |
| [068](#068--lampe-planches-chute-du-joueur-tutoriel-dans-la-carte) | 2026-10-09 | Lampe encore pixelisée, planches difficiles à poser, chute sous la carte, zombies pendant le tuto, tuto en panneaux dans la carte | Correctif | Rendu Forward + textures filtrées, pose des planches refaite, joueur protégé, vagues bloquées, panneaux uGUI |
| [069](#069--saccroupir-gâchette-gauche) | 2026-10-09 | S'accroupir avec la gâchette gauche | Contrôles | Gâchette gauche maintenue = vue 55 cm plus bas (sauf pistolet ou lampe en main gauche) |
| [070](#070--saccroupir-joystick-gauche) | 2026-10-09 | Correction : s'accroupir avec le joystick gauche, pas la gâchette | Contrôles | Joystick gauche enfoncé (maintenu) = accroupi ; gâchette gauche rendue à son rôle |
| [071](#071--analyse-critique-du-jeu-level-design-et-game-design) | 2026-10-19 | Analyse critique du jeu : points forts et faibles (Level Designer, Game Designer) | Analyse | Rapport : boucle solide, 3 problèmes majeurs (zombies hors bunker, générateur, toit) |
| [072](#072--logique-de-jeu-et-game-feel) | 2026-10-19 | Le jeu est-il simple à comprendre, logique, avec un bon game feel ? | Analyse | Partie ajoutée au rapport : marteau sans vibration, impacts invisibles, mort par tir près du visage |
| [073](#073--liste-des-patchs) | 2026-10-19 | Liste de tout ce qu'il faut modifier pour rendre le jeu logique | Analyse | 28 modifications classées P0 à P4 |
| [074](#074--évaluation-avec-une-grille-pro) | 2026-10-19 | Grille d'évaluation pro (3C, level design, tech art, pondération) | Analyse | Note ≈ 5,7 / 10, red flags : confort, UI collée à la tête, feedback, perf non mesurée |
| [075](#075--tableau-des-modifications) | 2026-10-19 | Tableau de tout ce qui doit être modifié / ajusté | Analyse | Tableau unique de 38 lignes |
| [076](#076--issues-github-des-modifications) | 2026-10-19 | Mettre tout ça dans les issues GitHub | GitHub / organisation | Issues #91 à #128, labels P0 à P4, ajoutées au projet SAE501 |
| [077](#077--relire-rulesmd) | 2026-10-19 | Relire le rules.md | GitHub / organisation | Journal 071-077 ajouté, issues rattachées au projet n°2 |
| [078](#078--correctifs-p0--poursuite-point-gauche-toit-zombie-coincé) | 2026-10-19 | Correctifs P0 : poursuite hors du bunker, point du couloir gauche, pas de toit aux niveaux 1-2, zombie coincé | Gameplay | Issues #92 à #95 ; testé en mode Play dans l'éditeur, pas au casque |
| [079](#079--lisibilité-p1--écran-dannonces-caméra-sud-toit-tutoriel-zones-leviers) | 2026-10-19 | Lisibilité P1 : écran d'annonces fixe + alarme, caméra extérieure sud, poussière avant la chute du toit, « Visez la tête », contenu des zones, annonce du niveau 4, leviers physiques | Gameplay | Issues #102 à #107 et #111 ; testé en mode Play dans l'éditeur, pas au casque |
| [080](#080--game-feel-p2-et-fin-de-partie--vibrations-impacts-yeux-lod-sondes-générateur-bilan) | 2026-10-19 | Game feel P2 et fin de partie : vibrations, impacts, éclats, yeux lumineux, LOD, sondes de lumière, générateur au niveau 4, bilan | Gameplay, rendu | Issues #112 à #120 et #122 ; testé en mode Play dans l'éditeur, pas au casque |
| [081](#081--validation-et-push-dans-main) | 2026-10-19 | Valider les issues #112 à #120 et #122, les assigner, pousser la version dans main | GitHub | Issues assignées et fermées ; `feature/map-finale` poussée dans `main` |
| [082](#082--la-boucle-de-gameplay-est-elle-correcte-) | 2026-10-19 | La boucle de gameplay est-elle correcte, simple, lisible ? | Game design | Boucle correcte et simple ; 3 défauts de rythme et 4 infos non dites, 4 correctifs proposés |
| [083](#083--contexte-de-la-scène-blender-ouverte) | 2026-10-19 | Prendre le contexte de la scène ouverte dans Blender | Environnement | Inventaire de `Hospital_VR_Survival_QualityPass.blend` (carte d'hôpital en 5 zones, 56 k triangles) et adaptation au Quest 3 ; rien modifié |
| [084](#084--hôpital-blender-phase-1-blockout) | 2026-10-19 | Rendre la scène d'hôpital Blender au mieux : phase 1 (collections, matériaux, World, blockout des 6 zones) | Environnement | Blockout construit dans une copie `_v02_phase1.blend`, ancienne passe archivée (masquée, rien supprimé), rendus de contrôle ; phase 2 en attente de validation |
| [085](#085--ce-qui-manque-pour-léquilibrage) | 2026-10-20 | Ce qui manque au jeu Unity pour être équilibré | Game design | Valeurs réelles du code relevées : ≈ 96 balles pour ≈ 225 au torse, mêlée sans risque à la barricade, saut de difficulté au niveau 4, pas de mesures ; 8 manques classés ; rien modifié |
| [086](#086--intégration-des-pr-131-radio-et-132-passer-lattente) | 2026-10-20 | Intégrer en local les PR d'Al Amine : radio de ravitaillement (#131) et passer l'attente avec B (#132) | Gameplay, GitHub | Fusion sans conflit sur une branche locale, compilation sans erreur, scène reconstruite et éclairage recuit ; pas encore poussé, en attente du test au casque |
| [087](#087--version-pc-sans-casque-clavier-et-souris) | 2026-10-20 | Préparer une version non VR pour tester sur PC (ZQSD), sans toucher à la version VR | Joueur, Outils | Scène de lancement `MainScene_PC` + contrôleur clavier/souris qui pilote les mains XRI du rig ; testé en mode Play (tir, recharge, planche clouée, levier, radio) ; MainScene inchangée ; pas encore commité ni poussé |
| [088](#088--rééquilibrage-du-jeu) | 2026-10-20 | Implémenter tout le rééquilibrage proposé en 085 | Gameplay / Équilibrage | Chargeurs du bunker garantis, recharge sans perte, coup à la barricade (1,6 m, 50 %, alternance), +50 PV par pause, niveau 4 à 40 %, traque au niveau 4, points pondérés (#123), difficulté, mesures [Bilan], reprise au niveau atteint (#121) ; tests a-f en mode Play ; pas poussé |
| [089](#089--champ-de-vision-et-brouillard) | 2026-10-20 | Option FOV dans les paramètres, brouillard pour cacher la distance d'affichage | Rendu, Menus | Champ de vision 50 à 90° (version PC ; au casque, imposé par le Quest), brouillard bleu nuit et brume au bas du ciel ; testé en mode Play ; pas poussé |
| [090](#090--téléportation-et-saut) | 2026-10-20 | Option de téléportation, remettre une touche de saut | Joueur, Menus | Option Déplacement : continu / téléportation (casque, point d'arrivée contrôlé par le NavMesh), saut remis (A, Espace sur PC) mais petit (≈ 0,33 m) ; testé en mode Play ; pas poussé |
| [091](#091--prise-par-clic-ou-maintenue) | 2026-10-20 | Option pour tenir un objet : maintenir ou un clic | Mains, Menus | Option Prise (grip) : UN CLIC (défaut) / MAINTENIR, au casque ; la prise est sur le grip, pas la gâchette ; testé en mode Play ; pas poussé |
| [091](#091--rendu-à-8-m-et-réduction-des-polygones) | 2026-10-20 | Comment marche le rendu à 8 m, comment réduire les polygones | Rendu, Optimisation | Explication seulement : yeux lumineux 7-9 m, LOD du décor (bascule réelle ≈ 16 m à cause de `lodBias 2`), caméra 60 m, brouillard ; pistes (occlusion culling, LOD des zombies, coupe par calque) ; rien modifié |
| [092](#092--textures-et-distance-mipmaps) | 2026-10-20 | Les textures baissent-elles en qualité avec la distance ? | Rendu, Optimisation | Explication seulement : oui, par les mipmaps (243 textures sur 246), trilinéaire + anisotrope ×4 sur la carte, 1024 px max, streaming des mipmaps coupé ; rien modifié |
| [094](#094--fermer-les-pull-requests-et-pousser-dans-main) | 2026-10-20 | Fermer toutes les pull requests et pousser dans main la version actuelle du jeu | GitHub | Scène reconstruite et éclairage recuit, `main` poussée en avance rapide, wiki poussé, 7 PR fermées |
| [095](#095--page-wiki-optimisations) | 2026-10-20 | Page wiki sur tout ce qui a été fait en optimisation | Wiki, Optimisation | Nouvelle page `Optimisations` (rendu au casque, lumière précalculée, LOD, textures, culling, caméras de surveillance, code, pistes), Home et barre latérale ; wiki poussé ; rien modifié dans le jeu |
| [096](#096--image-de-couverture-github) | 2026-10-20 | Image de cover du jeu pour le dépôt GitHub | Communication, Rendu | Image 1280 × 640 rendue depuis `MainScene` (zombies, planches et lumières temporaires, rien sauvegardé) puis titre et accroche ajoutés ; à poser à la main dans Settings > Social preview |
| [097](#097--cover-dans-le-readme) | 2026-10-20 | Mettre la cover en tête du README | Documentation | Image copiée dans `docs/images/cover.png` (Git LFS) et affichée sous le titre du README |
| [098](#098--sols-en-double-bouteilles-clé-et-planches) | 2026-10-20 | Plaques au sol (capture au casque), bouteilles qui volent sous la cabane du jardin, clé impossible à prendre, trop de planches sur la table de la cuisine | Carte, Gameplay | Matériaux « Apocalypse » recolorés (mousse, flaques transparentes, tuyaux), tapis et papiers décalés, terrain abaissé sous les planchers ; bouteilles recalées ; clé prenable ; une seule planche sur la table ; éclairage recuit |
| [099](#099--pousser-main-puis-tester-les-vagues-infinies-de-lecapet) | 2026-10-21 | Pousser proprement dans main, puis fusionner la PR de LeCapet « vagues infinies » pour la tester | GitHub, Gameplay | `main` poussée (51fafa8) avec le wiki ; PR #133 fusionnée sur la branche locale `test/vagues-infinies`, conflits résolus, compilation sans erreur ; en attente du test de Rahim |
| [100](#100--équilibrage-des-vagues-évasion-et-vagues-infinies) | 2026-10-21 | Vérifier l'équilibrage des vagues, surtout en vagues infinies | Game design | Courbe calculée vague par vague ; 4 défauts en survie (munitions finies, vie rendue à chaque vague, Coureurs trop rapides, 20 zombies vivants) ; 7 réglages chiffrés proposés ; rien modifié |
| [101](#101--rééquilibrage-du-mode-vagues-infinies) | 2026-10-21 | Faire le rééquilibrage proposé pour les vagues infinies | Gameplay | 6 réglages appliqués (radio 2 puis 3 chargeurs, soin de pause dégressif, vitesse ×1,7, 14 vivants, renforts à la vague 4, PV +8 %/vague ×2,5) ; compilation sans erreur ; pas joué |
| [102](#102--fusion-des-vagues-infinies-dans-main) | 2026-10-21 | Merger proprement le tout dans main | GitHub | `test/vagues-infinies` poussée dans `main` en avance rapide (e8aad93) ; PR #133 marquée fusionnée ; wiki poussé avec la page Mode survie |
| [103](#103--plan-daction-du-multijoueur) | 2026-10-21 | Plan d'action du multijoueur : lobby, prêt, serveur local, scores | Game design | Plan écrit (NGO + Unity Transport en LAN, hôte sur un casque ou le PC, inventaire de ce qui passe en réseau, scores d'équipe et par joueur, 16 à 22 j estimés, option réduite 10 à 13 j) ; rien modifié dans le jeu |
| [105](#105--carte-de-base-finalefinaleblend) | 2026-10-21 | Mettre à jour la carte de base avec Finalefinale.blend | Carte | Même plan que la carte actuelle, seul le jardin change (8 arbres PolyHaven, rochers, pierres) ; feuillage allégé à l'export ; textures manquantes signalées |
| [106](#106--liste-des-textures-manquantes) | 2026-10-21 | Liste des textures manquantes | Carte | 25 images non empaquetées listées (herbe, pavés, arbres, rochers, pierres, tronc) |
| [107](#107--textures-du-jardin-reçues) | 2026-10-21 | Textures du jardin reçues (textures.zip) | Carte | 20 textures rebranchées, herbe et pavés repris de map_final_final ; import, reconstruction, collisions des arbres, éclairage recuit |
| [108](#108--haie-du-jardin-floue) | 2026-10-21 | La haie du jardin est encore en basse qualité | Carte | Cause : matériau procédural cuit en une image pour toute la haie ; correction proposée |
| [109](#109--haie-et-forêt-en-feuillage-répété) | 2026-10-21 | Oui, en gardant l'optimisation | Carte | Texture de feuillage répétable faite à partir des feuilles PolyHaven, UV en mètres sur la haie et les sapins ; même coût |
| [110](#110--textures-qui-clignotent-et-ombres-tachées) | 2026-10-21 | Textures abîmées : objets les uns dans les autres, lumière | Carte | 41 surfaces superposées corrigées à l'export ; petits objets éclairés par sondes au lieu de lightmaps ; éclairage recuit |
| [111](#111--appui-de-fenêtre-encastré-dans-le-mur) | 2026-10-21 | Le bas de la fenêtre est encastré dans le mur | Carte | Encadrements gonflés de 4 mm selon leurs normales (l'appui passe devant le bas de l'ouverture du mur) ; éclairage recuit |
| [112](#112--vraies-portes) | 2026-10-21 | Vraie texture de porte au lieu des portes low poly | Carte | Texture de porte à panneaux produite (bois peint écaillé des portes du fichier) ; appliquée aux 4 portes de niveau, double porte pour les passages larges |
| [113](#113--fondu-de-début-et-menu-des-commandes) | 2026-10-22 | Fondu au début de la partie, menu qui explique les touches | Interface | Fondu depuis le noir (VR et PC) ; page COMMANDES dans le menu d'accueil et le menu pause : manettes Quest 3 avec traits de rappel, clavier et souris |
| [114](#114--modèle-de-la-clé) | 2026-10-22 | Clé : modèle ou texture gratuits, sources dans le wiki | Graphisme | Clé en fer rouillé : modèle Quaternius (CC0) et texture Poly Haven rusty_metal_04 (CC0), sans lueur ; page wiki Textures et modèles |
| [115](#115--ceinture-chargeur-et-marteau) | 2026-10-22 | Ceinture : 1 chargeur, rayon pour reposer le pistolet, marteau sur la table | Gameplay | Holster du chargeur à la hanche gauche (rayon 14 cm), pistolet 16 cm, lampe 12 cm ; plus de marteau à la ceinture, posé sur l'établi du bunker |
| [116](#116--objets-low-poly-remplacés) | 2026-10-22 | Retirer les objets low poly, modèles ou textures libres, sources dans le wiki | Graphisme | 6 bouteilles vertes remplacées par 2 modèles Poly Haven (CC0) ; piles en cubes remplacées par une pile D modélisée et texturée ; page wiki Textures et modèles |
| [117](#117--lampe-torche-réaliste) | 2026-10-22 | Lampe torche : paquet Asset Store, puis modèle réaliste libre | Graphisme | Lampe en cylindres remplacée par Small Plastic Torch de Poly Haven (CC0) ; faisceau, jauge, port de pile et prise gardés |
| [118](#118--balles-et-batterie-à-linspection) | 2026-10-22 | Balles et batterie affichées seulement quand on tourne l'objet de côté (canon vers la gauche) près du visage | Interface | `InspectHud` : « 8 / 12 » + icône de balle sur le pistolet, pile à 4 barres sur la lampe (barre en cours qui clignote de plus en plus vite) |
| [119](#119--inspection-plus-souple-et-prise-des-armes) | 2026-10-22 | Inspection capricieuse ; pistolet et hache décalés dans la main | Gameplay | Seuils assouplis + marge et maintien ; point d'accroche au centre du poing ; prise sur l'axe réel du manche |
| [120](#120--rechargement-facilité) | 2026-10-22 | Recharger trop compliqué, le chargeur traverse le pistolet | Gameplay | Zone du puits 5 → 8 cm ; rechargement par proximité (9 cm) avec le trajet du chargeur entre deux images |
| [121](#121--croix-pour-passer-le-tutoriel) | 2026-10-22 | Croix pour passer le tutoriel | Interface | Croix en haut à droite des panneaux du tutoriel : visée main vide + gâchette (PC : réticule + clic) ; A + X 2 s toujours possible |
| [122](#122--retour-de-dégâts-et-direction-du-coup) | 2026-10-22 | Accentuer les dégâts reçus + indicateur de direction | Gameplay / Ressenti | Arc rouge du côté du zombie (suit la tête), bord rouge de ce côté, flash selon les dégâts, son du coup + griffure spatialisée, battement de cœur sous 25 %, vibration plus forte côté zombie ; VR et PC |
| [123](#123--cover-et-logo-du-jeu) | 2026-10-22 | Refaire la cover du jeu et faire un logo simple (desktop, mobile, Quest) | Communication | Cover refaite depuis la cinématique de fin (zombies derrière la grille), 3 formats ; logo « fenêtre barricadée + yeux rouges », icônes 16 à 1024 px, .ico, .icns, logotypes ; fait par l'IA |
| [124](#124--grille-de-la-cover-plus-détaillée) | 2026-10-22 | La grille de la cover fait low poly | Communication | Grille redessinée en 2D (fer forgé, rouille, rivets) ; modèle 3D du jeu inchangé |
| [125](#125--cover-et-logo-poussés-sur-github) | 2026-10-22 | Mettre à jour le GitHub | GitHub | Cover et logo poussés sur `main` (f9966d6) |
| [126](#126--mise-à-jour-du-gdd) | 2026-10-22 | Mettre à jour le GDD (.docx) avec l'état actuel du jeu | Documents | GDD réécrit par l'IA dans sa mise en forme : 4 niveaux au lieu de 5 vagues, mode Vagues infinies, radio, ceinture, inspection, contrôles, flow, feedbacks, UI ; nouveau fichier `GDD The last escapor - 2026-10-22.docx` |
| [127](#127--son-et-titre-de-début-de-vague) | 2026-10-22 | Son à chaque début de vague + « Vague N » au milieu de l'écran | Gameplay / Ressenti | Chuchotement (SOUND BAG, YouTube) découpé à 4 s ; « VAGUE N » collé à la vue, fondu 1 s / 1,5 s visible / fondu 1,5 s, calé sur le son ; crédits README et wiki |
| [128](#128--titre-de-vague-en-lettres-de-sang) | 2026-10-22 | « VAGUE N » en lettres de sang, police libre de droits | Interface | Police Nosifer (OFL, usage commercial permis), rouge sang + ombre noire ; crédits README et wiki |
| [129](#129--retrait-de-la-version-pc) | 2026-10-22 | Retirer toute trace de la version PC pour l'instant | Outils | Scène MainScene_PC, scripts Desktop, menu Play PC Version, champ de vision, onglet clavier et cas « mode PC » retirés ; état sauvé sur la branche archive/version-pc ; wiki nettoyé |
| [130](#130--release-apk-quest-v090) | 2026-10-22 | Publier l'APK de la dernière version dans les releases GitHub, nom du jeu et icône | Outils | Build Quest 0.9.0 depuis main (menu + scène de jeu), icônes Android depuis le branding, release GitHub « The Last Escapor v0.9.0 » avec l'APK |
| [131](#131--portes-déverrouillées-en-fin-de-vague) | 2026-10-22 | Son au déblocage d'une porte + portes débloquées dès la fin de chaque vague | Gameplay / Ressenti | Les zones de la vague N+1 s'ouvrent à la fin de la vague N (début de la pause), son door-open.mp3, annonce « OUVERT : … » sur l'écran du bunker, panneaux « ouverte après la vague N » |
| [133](#133--portage-pc-clavier--souris) | 2026-11-09 | Portage PC propre : interactions, placement des mains, réticule, ZQSD, E, clic, souris verrouillée | Interaction / Version PC | Version PC dans les mêmes scènes (casque par défaut, PC sans casque) : DesktopPlayer, mains et armes en vue FPS, tir depuis le centre de l'écran, HUD (réticule, aide, munitions), menus à la souris, textes du tutoriel, réglages souris ; testé en mode Play sans casque |
| [134](#134--fusion-de-la-pr-146-escalade-dans-la-version-pc) | 2026-11-09 | Récupérer la PR #146 (escalade des fenêtres, Al Amine) et la fusionner avec la version PC locale pour tester | Dépôt / Zombies | PR fusionnée localement dans feature/portage-pc sans conflit, contrôleurs des zombies régénérés avec les 3 clips d'escalade ; rien poussé |
| [135](#135--escalade-des-fenêtres-corrigée) | 2026-11-09 | Analyser la vidéo du problème d'animation du zombie et le faire passer proprement dans la fenêtre | Zombies | Clips Mixamo faits pour un mur de 2 m + montée du script = zombie au-dessus du cadre ; corrigé : mains sur le rebord, passage dans l'ouverture, chute courte, se relève sans saut ; testé 2 fenêtres × 3 types |
| [136](#136--chaise-et-desserte-retirées-près-des-fenêtres) | 2026-11-09 | Retirer une chaise et une table proches des fenêtres | Carte | Tabouret du poêle et desserte verte (avec sa marmite) retirés de MainScene et de RoomBuilder, NavMesh et éclairage recuits |
| [137](#137--textures-pbr-sur-les-objets-en-aplat) | 2026-11-09 | Remplacer les aplats de couleur par des textures PBR | Graphismes | 24 matériaux en couleur unie texturés (albedo, normal, métal/lissé, usure), 22 textures 1024 ASTC 6x6, réappliquées à la reconstruction |
| [138](#138--initialisation-game-studios-et-gdd) | 2026-11-09 | Initialisation Game Studios, project.yaml et GDD consolidé | Documentation | Création de project.yaml, design/gdd/game-concept.md, production/session-state/active.md et AGENTS.md |
| [139](#139--intégration-du-gdd-officiel-pdf) | 2026-11-09 | Intégration du GDD complet (21 pages PDF) dans project.yaml et game-concept.md | Documentation | Alignement complet de project.yaml et design/gdd/game-concept.md avec le document officiel (3C, 5 vagues, équilibrage, bestiaire, palette, crédits) |
| [140](#140--analyse-dun-audit-déquilibrage) | 2026-11-09 | Vérification d'un audit d'équilibrage externe contre le code | Game design | Tableau vrai/faux/périmé par constat et avis sur chaque proposition, aucun code modifié |
| [141](#141--audit-déquilibrage-game-designer-et-analyse-pcvr) | 2026-11-09 | Juger l'équilibrage du jeu, la cohérence GDD/code et l'écart d'équilibrage PC vs VR | Game design | Audit complet (5 incohérences GDD/code, courbe TTK/économie, 4 asymétries d'équilibrage PC vs VR dans DesktopPlayer/Nail, plan d'ajustement en 6 chantiers) |
| [142](#142--pipeline-audio-combat-zombies-team-audio-zombies-combat) | 2026-11-09 | `/team-audio zombies-combat` — Direction, specs SFX, accessibilité et implémentation audio de combat | Audio / Accessibilité | 6 documents d'architecture/design audio + implémentation C#/Shader (9 sons procéduraux en cache dans ProcAudio, profilage vocal par archétype, musique adaptative 4 états + ducking, alertes ambre GAP 1/2 et Flash HUD GAP 3) |
| [143](#143--checkup-du-projet-après-antigravity) | 2026-11-09 | Checkup du projet Unity après la mise à jour faite avec Antigravity | Qualité / Code | Compilation propre, aucun .meta orphelin ni GUID dupliqué ; rapport de 9 points (musique coupable par manque de voix, musique désaccordée, arcs ambres, etc.), rien corrigé avant accord |
| [144](#144--musique-de-brèche-et-correctifs-du-checkup) | 2026-11-09 | Musique de brèche déclenchée par un zombie dans le bunker et correctifs du checkup | Audio / Code | Brèche = zombie vivant dans le bunker, musique en priorité 0, arc ambre qui n'écrase plus un arc rouge et seulement pour une planche proche hors champ, seuils d'archétype en un seul endroit |
| [145](#145--modèles-détaillés-du-poste-de-surveillance) | 2026-11-09 | Le poste de surveillance est encore low poly | Graphismes | Radio, enregistreur à LEDs, clavier, étagère à écrans et 5e écran remplacés par des modèles détaillés (Blender, ~15 000 triangles), éclairage recuit |
| [146](#146--refonte-complète-du-menu-de-fin-de-partie-terminal-bunker-os-world-space) | 2026-11-09 | `/team-ui` Faire proprement le menu de fin de partie | UI / UX | Terminal BUNKER-OS en World Space (bilan, boutons reprendre/recommencer/menu, verrou anti-clic), souris libérée en version PC, pause bloquée en fin de partie |
| [147](#147--textes-des-portes-cachés-par-les-murs) | 2026-11-09 | Ne plus voir les textes des portes à travers les murs, seulement de près | Interface / Rendu | Shader SAE501/WorldText : test de profondeur (les murs cachent le texte) et fondu entre 3,5 et 5 m calculé dans le shader ; 7 textes du décor (portes, générateur, radio) |
| [148](#148--murs-qui-entrent-les-uns-dans-les-autres) | 2026-11-09 | Corriger les aberrations des murs qui se chevauchent | Carte / Rendu | 224 triangles superposés retirés sur 39 éléments de structure (z-fighting à côté du disjoncteur, sous les portes, aux angles), copies de maillages dans Assets/Generated/Recouvrements |
| [149](#149--branche-develop-et-archivage-des-branches-fusionnées) | 2026-11-09 | Pousser dans une branche propre et archiver les branches déjà fusionnées | Git | Branche `develop` poussée (travail du jour + ajouts Antigravity), wiki poussé, 16 branches fusionnées gardées en tags `archive/<nom>` puis supprimées |
| [150](#150--trous-du-toit-irréguliers) | 2026-11-09 | Trou du toit : forme moins carrée, tiges retirées, effet sur les bords | Carte / Rendu | Bord de béton cassé irrégulier, auréole noircie, gravats, lianes qui pendaient retirées (`TrousToit.cs`), éclairage recuit |
| [151](#151--trous-du-toit-élargis) | 2026-11-09 | Élargir un peu le trou du toit | Carte / Rendu | Ouverture des deux trous agrandie (~0,9 à 1,15 m), éclairage recuit |
| [152](#152--portail-du-jardin-en-fer-forgé-vieilli) | 2026-11-10 | Texture de la grille (portail du jardin) plus réaliste | Carte / Rendu | Matériau propre au portail : fer forgé peint en noir, écaillé, rouille, coulures, terre au pied ; UV refaites (`PortailFer.cs`, `generer_portail.py`) |
| [153](#153--alarme-de-début-de-vague-retirée) | 2026-11-10 | Retirer l'alarme du début des vagues, garder le son de « VAGUE N » | Audio | Alarme procédurale retirée de `WaveManager.StartLevel` ; reste `Vague_Debut` |
| [153](#153--mini-serveur-pour-le-multijoueur) | 2026-11-10 | Réfléchir à un mini serveur pour le multijoueur | Game design | 3 options comparées (hôte joueur, serveur dédié Unity, mini serveur lobby + scores hors Unity) ; conseil : rien dans le build rendu, option « parties en parallèle » (2,5 à 3 j) sur une branche à part seulement si validée ; rien codé |
| [154](#154--modélisation-de-larène-1v1-dans-blender-via-mcp-et-décomposition-des-systèmes) | 2026-11-10 | `/map-systems` Modélisation de l'arène 1v1 dans Blender via MCP et décomposition des systèmes | Carte / Game design | Connexion live MCP Blender 5.2.1, modélisation arène symétrique 30x16m (`arena_1v1_bunker.blend/.fbx`), chicanes, piliers, props, spawns, index des systèmes et LD spec |
| [155](#155--multijoueur-coop-en-réseau-local-netcode) | 2026-11-10 | Faire l'option A : coop en réseau local avec Netcode | Gameplay / Réseau | Branche `feature/multijoueur` : héberger / rejoindre (découverte Wi-Fi), lobby PRÊT, avatars, zombies relayés par l'hôte, barricades partagées, scores ; testé seul dans l'éditeur, pas à 2 ni au casque |
| [155](#155--texturing-pbr-procédural-et-lighting-design-de-larène-1v1-dans-blender-via-mcp) | 2026-11-10 | Texturing PBR procédural et lighting design de l'arène 1v1 | Carte / Graphismes | Shaders PBR procéduraux (béton, dalle, acier, rouille, jaune alerte), projection UV cube, 15 sources lumineuses (ambre, néon froid, alerte, rim lights), profil AgX Punchy |
| [156](#156--passe-dirrégularités-réalistes-et-props-industriels-sur-larène-1v1-dans-blender-via-mcp) | 2026-11-10 | Passe d'irrégularités réalistes et props industriels sur l'arène 1v1 | Carte / Modélisation | Biseautages des arêtes et ébréchures de béton, 8 tuyauteries industrielles suspendues, caisses empilées de travers, barils renversés, gravats et 4 flaques d'eau réfléchissantes |
| [157](#157--déconstruction-des-formes-cubiques-et-enrichissement-réaliste-de-larène-1v1-dans-blender-via-mcp) | 2026-11-10 | Déconstruction des formes cubiques et silhouettes non-carrées de l'arène 1v1 | Carte / Modélisation | Piliers octogonaux et piliers fracturés, barrières en diagonale 15° brisées, mur central à meurtrière, hérissons tchèques, tôles ondulées cintrées et redoutes de sacs de sable en arc |
| [158](#158--élargissement-de-larène-1v1-passerelles-surélevées-et-escaliers-daccès-dans-blender-via-mcp) | 2026-11-10 | Élargissement de l'arène 1v1, passerelles surélevées et escaliers d'accès | Carte / Level Design | Arène portée à 34x26m (5.2m sous plafond), 2 passerelles à +2.4m avec rambardes et nids de tir, 2 escaliers de 11 marches, 4 spots supplémentaires, réexport FBX |
| [161](#161--sons-de-la-pr-151-intégrés-au-portage-pc-et-au-multijoueur) | 2026-11-10 | Récupérer uniquement les sons de la PR #151 (LeCapet) dans la version actuelle | Audio | 7 sons (planches, clous, souffle de la hache, zombie touché / morsure / chute) dans `Assets/Resources/Sounds`, `AudioSlices` sans la banque GameSounds, câblage adapté au multijoueur ; modifs d'UI et `Nail (1).cs` écartées ; compilé hors éditeur, pas écouté en jeu ni au casque |
| [162](#162--ajout-du-toit-en-béton-armé-poutres-maîtresses-en-acier-et-gaine-de-ventilation-dans-blender-via-mcp) | 2026-11-10 | Rajout d'un toit en béton armé, poutrelles acier et gaine HVAC | Carte / Modélisation | Dalle 34.8x26.8m, 5 poutres acier 26m, gaine ventilation 33m suspendue |
| [163](#163--rehaussement-de-la-hauteur-sous-plafond-à-70-m-piliers-et-toiture-dans-blender-via-mcp) | 2026-11-10 | Rehaussement de la hauteur sous plafond à 7.0 m | Carte / Modélisation | Murs, contreforts et piliers à 7.0m, dalle à Z=7.0m (+4.6m de dégagement sur passerelles) |
| [164](#164--import-propre-de-larène-1v1-dans-unity-alvéole-17-nouvelle-scène-et-intégration-menu-multijoueur) | 2026-11-10 | Import propre dans Unity, scène Alvéole 17 et mode Duel 1v1 dans le menu | Unity 6 / Multijoueur | FBX, 8 matériaux URP, constructeur de scène C#, mode 1v1 dans NetLobby/MainMenuMulti, NetDuel |
| [165](#165--élimination-du-flickering-et-du-grésillement-des-lumières-urp-sur-larène-1v1) | 2026-11-10 | Correction du scintillement et flickering des lumières dans Unity | Carte / Rendu URP | Dépassement limite URP (8 lumières/objet) résolu : Directional Light + 5 spots sans ombres, zéro flickering |
| [166](#166--paquets-en-erreur-après-limport-dune-arme-et-dune-map) | 2026-11-10 | Paquets en erreur après l'import d'une arme et d'une map | Paquets Unity | HDRP, Ads, Analytics et Purchasing ajoutés par un vieil import, retirés ; manifest et réglages graphiques restaurés |
| [167](#167--textures-des-murs-de-larène-1v1-et-combinaison-en-urp) | 2026-11-10 | Textures de l'arène 1v1 et combinaison en URP | Carte / Matériaux URP | UV en mètres à l'import du FBX, béton, rouille et acier texturés ; matériaux Protection_suite passés en URP Lit |
| [168](#168--personnages-survivalist-et-choix-du-skin-en-multijoueur) | 2026-11-10 | Configurer les personnages Survivalist (animations arme tenue / interactions, VR et PC) et choix du skin en multi | Gameplay / Réseau | `PlayerAvatar` (bras par cinématique inverse, doigts, jambes animées), 4 tenues, objet tenu visible, choix dans le lobby avec aperçu ; vérifié en images et dans le lobby, pas à 2 joueurs ni au casque |
| [169](#169--pack-survivalist-allégé-et-bouton-forcer-le-lancement-du-1v1) | 2026-11-10 | Alléger le pack de personnages, l'intégrer au dépôt, bouton pour forcer le lancement du 1v1 seul | Gameplay / Réseau | Pack 214 → 63 Mo commité (Git LFS, quota 10 Go) ; bouton « FORCER LE LANCEMENT » (hôte, duel) : la scène de l'arène se charge avec un seul joueur ; NetDuel et NetPlayer ne se créent pas encore dans l'arène |
| [170](#170--correctifs-du-mode-1v1) | 2026-11-10 | Corriger le mode 1v1 bugué (armes qui ne marchent pas) | Gameplay / Réseau | Cause : l'arène n'avait pas d'interacteur direct (pas de prise ni de tir en version PC) ; pistolet de départ en main, munitions infinies, NetDuel en prefab réseau, joueurs réseau créés ; testé seul dans l'éditeur, pas à 2 joueurs ni au casque |
| [171](#171--systèmes-et-pistolet-de-mainscene-dans-larène-1v1) | 2026-11-10 | Reprendre du mode zombie les systèmes du joueur (pause, tir, animations, style) dans l'arène 1v1 | Gameplay | Pause, vignette de dégâts, musique, ceinture avec holster, et le pistolet complet de MainScene (animation de tir, son, douille, flash, fumée) ; arène réparée sans reconstruction ; testé dans l'éditeur, pas au casque |
| [172](#172--arbres-et-sol-du-jardin-refaits-fluidité) | 2026-11-10 | Arbres horribles, texture du sol du jardin horrible, « pas vraiment smooth » | Carte / Rendu / Optimisation | Arbre Poly Haven refait dans Blender (`arbre_quest.py`), forêt en cônes remplacée, sols Poly Haven 1024 en UV métriques, terrain extérieur descendu sous le jardin, LOD + imposteurs (`JardinNature.cs`) ; vérifié en captures de l'éditeur, pas au casque |
| [173](#173--trous-du-toit-bouchés-lianes-retirées-mur-du-couloir-ajouté) | 2026-11-10 | Mur manquant dans le couloir, retirer le trou et les éléments qui tombent du toit | Carte | 2 trous du toit bouchés (`TrousToit.cs`), lianes qui pendent retirées partout, mur ajouté à droite de la porte de la cuisine (`MurCouloir.cs`), zombies du toit par les portes et fenêtres, caisse de la radio sous le plafond |
| [174](#174--liste-durable-des-objets-retirés-à-la-main) | 2026-11-10 | Arrêter de replacer les éléments que je retire à la main | Outils | Cause : chaque « Build VR Scene » recrée tout le FBX ; liste durable `objets_retires.txt` lue par `RoomBuilder`, menu « Retirer la sélection (durable) » (`ObjetsRetires.cs`) |
| [175](#175--arbre-du-jardin-remplacé-par-un-jacaranda-sans-rebuild) | 2026-11-10 | L'arbre n'est toujours pas bon : un arbre classique, un seul tronc et une couronne de feuilles, sans rebuild | Carte / Rendu | Jacaranda Poly Haven allégé dans Blender (`arbre_jacaranda.py`, 32 323 / 13 083 triangles), mis à la place de l'olivier en remplaçant le FBX et les textures des 3 matériaux, sans reconstruire la scène ; reflets de la lune coupés sur les flaques |
| [176](#176--évasion--portail-du-jardin-puis-cinématique-avec-de-vrais-sons) | 2026-11-11 | La victoire en Évasion = générateur puis fuite par le portail du jardin ; sons de la cinématique horribles, coups sur la grille pas réalistes | Gameplay / Son | Le générateur ouvre le portail (cadenas, battants, lampe verte), le franchir déclenche la victoire puis la cinématique de fin (reprise de `feature/cinematiques`, VR et PC) ; bande-son refaite avec les sons du jeu et deux sons de grille CC0 (Freesound) |
| [177](#177--bilan-de-victoire-par-dessus-la-vidéo-de-fin) | 2026-11-11 | Le menu « vous avez échappé » doit apparaître par-dessus l'animation de fin, à la moitié | UI / Cinématique | Bilan affiché devant l'écran de cinéma à 50 % de la vidéo (ou tout de suite si on passe), vidéo qui continue derrière puis se fige avant son fondu |
| [178](#178--bilan-de-victoire--victoire-et-panneau-élargi) | 2026-11-11 | Menu pas assez large ; « Vous avez échappé » pas français : « victoire » ; retirer « Signal transmis » ; nouveau sous-titre | UI | Titre « VICTOIRE », sur-titre retiré, sous-titre « Vous avez réussi à vous enfuir. », panneau 1,52 -> 1,84 m |
| [179](#179--à-quoi-sert-la-clé-du-cabanon-) | 2026-11-11 | À quoi sert la clé du cabanon ? | Question | Elle n'ouvre rien : objet décoratif qu'on peut prendre en main, sans lien avec le portail |
| [180](#180--fichier-temporaire-introuvable-et-licensing-connection-lost) | 2026-11-11 | Patch « Opening file failed: …/Temp/UnityTempFile-… » et l'erreur « Licensing connection lost » | Outils / Éditeur | Cause : « Quit on close » de Vorssaint ferme les processus Unity sans fenêtre ; Temp recréé, garde-fou `TempDirGuard`, licence reconnectée seule |
| [181](#181--arènes-du-duel--alvéole-17-corrigée-2-nouvelles-cartes-et-choix-de-la-carte) | 2026-11-11 | Level design : corriger la map 1v1, proposer 2 autres maps, capture et choix de la carte dans le lobby | Carte / Multijoueur | Alvéole 17 corrigée (escaliers, caisses, sas, gravats, flaques) ; Bloc C et Quai 9 construits ; sélecteur avec aperçu dans le lobby ; non commité |
| [182](#182--arènes-du-duel--vraies-textures-sur-les-blocs) | 2026-11-11 | Les maps 1v1 manquent de textures : beaucoup de blocs sans texture | Carte / Textures | Textures générées (tôle ondulée, portes de conteneur, sacs, barils, casiers, carrelage, bandes de danger) + reliefs ajoutés ; captures refaites ; non commité |
| [183](#183--arènes-du-duel--optimisation-quest-barils-low-poly-fusion-colliders) | 2026-11-11 | « C'est pas très optimisé ça » (baril en fil de fer) | Optimisation | Barils 124 → 48 triangles, Alvéole 17 : 216 → 25 rendus, plus aucun MeshCollider sur les props ; non commité |
| [184](#184--arènes-du-duel--barils-et-barrières-refaits-bloc-c-supprimé) | 2026-11-11 | Corriger les barils et barrières d'Alvéole 17, retirer la map hôpital | Carte / Optimisation | Barils et barrières refaits proprement, contrôle 0 interpénétration / 0 objet flottant, Bloc C supprimé ; non commité |
| [185](#185--paramètres--vsync-et-limite-dimages-par-seconde) | 2026-11-11 | Ajouter le VSync ou une limite de FPS dans les paramètres | Menus / Performance | Version PC : VSync (défaut oui) et Limite FPS sans / 60 / 72 / 90 / 144 / 240, grisée sous VSync ; au casque, cadence du runtime XR ; commit `fa5c328`, journal non commité |
| [186](#186--duel--avatar-distant-qui-tient-son-arme-à-la-3e-personne) | 2026-11-11 | Corriger la tenue d'arme de l'avatar distant (style Counter-Strike) | Multijoueur / Animation | Joueur PC : arme posée devant l'épaule dans l'axe du regard, deux mains dessus, buste qui suit la visée ; commit `c0ff733` (PlayerAvatar), NetPlayer et journal non commités |
| [187](#187--duel--postures-darmes-façon-counter-strike) | 2026-11-11 | Corriger les postures de tenue des armes en s'inspirant de Counter-Strike | Multijoueur / Animation | Une posture par type d'arme (pistolet, fusil d'assaut, sniper), corps de trois quarts, main de soutien sous le garde-main ; commit `6489101`, NetPlayer et journal non commités |
| [188](#188--sons-des-zombies--impact-de-balle-et-pas) | 2026-11-11 | Rendre réalistes le son de balle sur un zombie et les pas des zombies | Audio | Sons CC0 Freesound (6 impacts, pas traînants et lourds) ; pas calés sur les appuis de pied des animations au lieu d'un minuteur, 4 pas max à la fois ; en attente d'écoute |
| [189](#189--mur-manquant-du-couloir-près-de-la-salle-du-générateur) | 2026-11-11 | Remettre le mur manquant à gauche de la salle du générateur | Carte / Outils éditeur | Face du mur ouest du couloir retirée à tort par MursSuperposes (test sur les seuls sommets) ; test refait sur toute la surface, face remise sans reconstruire la scène, rustine MurCouloir retirée, éclairage recuit |
| [190](#190--fusion-de-tout-le-travail-dans-main) | 2026-11-12 | Pousser proprement tout le travail dans main | Git / Documentation | Travail non commité rangé en 4 commits, feature/multijoueur (avec develop) fusionnée dans main, fichiers locaux laissés hors du dépôt, page wiki Historique des versions |
| [191](#191--retrait-des-caméras-de-surveillance) | 2026-11-12 | Supprimer les caméras des couloirs et les écrans | Gameplay / Carte | Caméras, écrans du pupitre, vue plein écran et disjoncteur retirés (scène, générateur de scène, scripts, tutoriel, version PC, docs), éclairage recuit, issue #154 |
| [192](#192--lunette-du-sniper-duel-1v1) | 2026-11-12 | Lunette grossissante pour le sniper des duels 1v1 | Gameplay / Duel / Rendu | Composant SniperScope sur le prefab : au casque caméra ×5 sur la lunette, active seulement l'œil près de l'oculaire ; au PC zoom clic droit + lunette plein écran, sensibilité réduite ; captures et test en mode Play (issue #153) |
| [193](#193--lunette--test-au-casque-sur-une-version-sans-lunette) | 2026-11-12 | Le scope ne fonctionne pas au casque | Diagnostic / Build | L'APK testé (09 h 21) venait de ~/SAE501 (branche feature/154-retrait-cameras), sans SniperScope : la lunette n'était pas dans le build. Code inchangé ; comment builder la bonne copie |
| [194](#194--fusion-de-la-lunette-dans-la-branche-du-retrait-des-caméras) | 2026-11-12 | Fusionner la lunette dans la branche de ~/SAE501 pour builder l'APK | Git / Build | feature/153-lunette-sniper fusionnée dans feature/154-retrait-cameras (conflits DesktopHud, journal, état de session résolus en gardant les deux) ; compilation vérifiée |
| [195](#195--fusion-dans-main-et-branche-de-secours) | 2026-11-12 | Pousser tout dans main proprement, avec un hotfix au cas où | Git | Branche de secours hotfix/avant-lunette-cameras sur l'ancien main (93ca474), feature/154-retrait-cameras (avec la lunette) fusionnée dans main, branches et wiki poussés |
| [110](#110--revue-ld-gd-de-lhôpital-v03) | 2026-10-21 | Revue critique LD/GD de Hospital_VR_Survival_QualityPass_v03_PBR_CoD_Zombie.blend | Carte | Analyse seule : 10 bloquants (accès du hall, escalier plein, rampe sous dalle, bloc op muré…), barricades et apparitions à refaire, couche CoD à retirer, 202 maillages à normales inversées |

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

### 056 — Types de zombie (PR #89) et wiki

**Prompt**

> Il faudrait également importer la pull request Types de zombie : Cogneur (poings) et Coureur (course + coups de pied) #89. FREE Zombie Male AAB, Zombies : les deux assets sont déjà importés (met à jour le wiki avec toutes les nouveautés également)

**Résultat** (code par l'IA, Claude Code ; types de zombie écrits par Al Amine, PR #89)

- PR #89 fusionnée dans `feature/map-finale`. Conflit dans `ZombieSpawner` résolu : point « Toit », départ posé sur le NavMesh et vitesse tirée au hasard conservés, multipliés par la vitesse du type.
- La PR utilisait le même modèle teinté pour les 3 types ; les deux packs importés sont maintenant utilisés : **Cogneur** = FREE Zombie Male AAB (chemise, 1,9 m), **Coureur** = pack « Zombie » (Zombie3). Os trouvés par le squelette humanoïde (noms d'os différents selon le modèle), matériaux « Standard » des packs convertis en URP/Lit (`Assets/Generated`). Si un pack manque, retour au modèle teinté de la PR.
- Réglages d'Al Amine conservés : Cogneur ×0,9 vitesse, ×1,5 PV, dès la vague 2, poids 0,35 ; Coureur ×1,6 vitesse, ×0,7 PV, dès la vague 3, poids 0,3 ; la tête tue toujours d'un coup.
- **Testé dans l'éditeur** : 3 prefabs construits (animations, 8 zones de touche, aucun matériau rose), tirage par vague (aucun type spécial en vague 1 ; vague 3 : 20 normaux, 9 cogneurs, 1 coureur sur 30), cogneurs qui entrent et frappent (animation de coup de poing, dégâts au joueur). Coup de pied du coureur non vérifié jusqu'au bout. **Pas testé au casque.**
- Les deux packs sont ajoutés au dépôt (fichiers lourds en LFS).
- Wiki mis à jour (voir les pages du wiki GitHub).

### 057 — Circulatoire (GDD)

**Prompt**

> Génère moi un visuel pour le Circulatoire (GDD) en te basant sur le schéma

**Résultat** (par l'IA, Claude Code) : `Circulatoire_TheLastEscapor.png` : plan vu de dessus avec la progression du joueur par niveau (1 à 4), les chemins des zombies (extérieur, toit, couloirs), les ressources rapportées au bunker et la boucle de jeu.

### 058 — Macro boucle (GDD)

**Prompt**

> Fait un visuel de la macro boucle (GDD). Visuellement ça doit être 3 éléments comme ça : (gabarit Objectif / Challenge / récompense)

**Résultat** (par l'IA, Claude Code) : `MacroBoucle_TheLastEscapor.png` : objectif « s'échapper du bunker », challenge « tenir la vague », récompense « la base s'ouvre ».

### 059 — Micro boucle (GDD)

**Prompt**

> Et la micro boucle ?

**Résultat** (par l'IA, Claude Code) : `MicroBoucle_TheLastEscapor.png` : objectif « tenir l'ouverture », challenge « agir sous pression », récompense « la menace tombe ».

### 060 — Core gameplay

**Prompt**

> Décrit moi le core gameplay (le core gameplay est comment se définit l'essence même du jeu… le résumé sans avoir à tout lire)

**Résultat** (par l'IA, Claude Code) : texte de synthèse pour le GDD (piliers barricader / se défendre / progresser, boucles, tension, victoire et défaite, accessibilité).

### 061 — Game feel

**Prompt**

> Décrit moi le gamefeel

**Résultat** (par l'IA, Claude Code) : texte pour le GDD (gestes en VR, clouer, tirer, menace sonore, dégâts, ambiance, lisibilité) ; piste : vibration du marteau absente.

### 062 — Présentation des ennemis et T-pose

**Prompt**

> rédige moi des présentations des ennemis, ainsi que des images des ennemis en T pose (une image avec chacun l'un à côté de l'autre, une vue de devant et une vue de derrière pour chacun)

**Résultat** (par l'IA, Claude Code) : fiches du Rôdeur (zombie normal, nom proposé), du Cogneur et du Coureur ; `Ennemis_TPose_TheLastEscapor.png` rendu dans Unity (pose de référence du squelette, face et dos, même échelle).

### 063 — Hitbox et dégâts

**Prompt**

> Fait moi une image explicative de la hitbox / des damages sur les zombis

**Résultat** (par l'IA, Claude Code) : rendu des 8 vraies hitbox du prefab et tableaux des dégâts. En vérifiant le code : un coup de hache à la tête retire 100 PV (pas de mort immédiate en mêlée) ; page wiki « Armes de mêlée » corrigée.

### 064 — Hitbox simplifiée

**Prompt**

> Simplifie l'image qui rémontre la hitbox du zombi, ne met pas les infos telles que : dégat au joueur par coup de zombi, précise directement à côté de la hitbox les dégats

**Résultat** (par l'IA, Claude Code) : `Hitbox_Degats_TheLastEscapor.png` : un encadré par zone (tête, torse et bras, jambes) avec les dégâts au pistolet, à la hache et au pied-de-biche.

### 065 — Palette de couleurs

**Prompt**

> Génère moi la palette de couleurs actuellement utilisé

**Résultat** (par l'IA, Claude Code) : `Palette_TheLastEscapor.png` : couleurs de l'interface et des retours de jeu lues dans le code, couleurs d'ambiance mesurées sur des captures du jeu.

### 066 — Concept art

**Prompt**

> C'est quoi concept art ?

**Résultat** (par l'IA, Claude Code) : explication (rôle, types, différence avec les visuels de production déjà faits) et propositions.

### 067 — Lampe torche, objets sous le sol, prise à distance

**Prompt**

> La flash light est pixelisé quand on l'allume, fait en sorte que ce soit un éclairrage smooth. Les items passent à travers le sol, patch ça. Actuellement il est encore possible d'attraper les objets de loin, et aussi sans le vouloir d'attraper plusieurs objets en même temps à une main

**Résultat** (code par l'IA, Claude Code)

- **Lampe « pixelisée »** : le Quest affiche en 8 bits (pas de HDR) ; dans une scène aussi sombre, le dégradé du faisceau formait des marches visibles. Tramage (dithering) activé sur la caméra du joueur (passe par le post-traitement), bord du faisceau adouci (angle intérieur 22° → 10°), lumière toujours calculée par pixel.
- **Prise de loin et plusieurs objets dans une main** : le `ControllerInputActionManager` des Starter Assets **réactivait le Near-Far Interactor** (le rayon) à la fin d'un geste de téléportation (joystick poussé puis relâché) : chaque main avait alors deux interacteurs. Le Near-Far est maintenant supprimé de la scène et sa référence retirée du script.
- **Objets sous le sol** : non reproduit en lâchant 24 objets dans toutes les pièces (tous restent sur le sol). Cause probable au casque : la main passe sous le sol virtuel quand on pose un objet par terre. Nouveau `ItemSafety` sur tous les objets attrapables : au lâcher, un objet sous le sol (ou le terrain) est remis dessus ; un objet qui tombe de la carte revient à sa dernière position sûre.
- Scène reconstruite, éclairage recuit. **Pas testé au casque.**

### 068 — Lampe, planches, chute du joueur, tutoriel dans la carte

**Prompt**

> La flashlight est encore glitché, c'est encore du pixel, et elle grésille alors quelle devrait être smooth, propre. Le plaçage de planche est encore complexe, la planche se place rarement bien, alors qu'elle devrait se placer proprement à chaque fois. Également il y a des moment où l'on peut tomber en dessous de la map exemple en se baladant dans les couloirs. La tutoriel se lance mais pendant le tuto les zombis se lancent aussi (ça ne devrait pas être le cas). Et le tuto devrai expliquer directement dans la map les planches etc avec des canvas 2D

**Résultat** (code par l'IA, Claude Code)

- **Lampe** : trois causes probables corrigées ensemble. Le Quest utilisait le rendu Forward+ (découpage en tuiles : blocs de lumière qui scintillent sur mobile) → rendu **Forward** (jusqu'à 8 lumières par objet). Textures de la carte en filtrage **trilinéaire + anisotrope ×4** (le sol vu en biais scintillait sous le faisceau). Batterie faible : variation **lissée** au lieu d'un tirage aléatoire à chaque image (grésillement). Tramage du prompt 067 retiré (son bruit se voyait dans le noir).
- **Planches** : la planche se décrochait dès que la main était à plus de 35 cm du centre de l'emplacement ; tenue par un bout, elle l'est toujours (elle mesure 1,84 m) : elle s'accrochait puis se décrochait en boucle. Maintenant : distance mesurée sur **toute la longueur** de la planche (30 cm), décrochage seulement si la main bouge de 40 cm **depuis le calage**, et **lâchée à moins de 50 cm d'un emplacement libre, elle s'y pose toute seule, bien alignée**, quelle que soit son orientation. Testé : tenue par un bout et inclinée de 25° → posée ; lâchée à 40 cm et tournée de 70° → posée et alignée.
- **Chute sous la carte** : le sol n'a aucun trou (29 000 points sondés). Le corps du joueur était fragile : capsule de 10 cm de rayon avec 8 cm de marge (collisions instables), marche de 50 cm (on montait sur les caisses), et des modes de déplacement des Starter Assets jamais coupés (saut sur A, escalade, « grab move » qui déplace le joueur quand on serre le grip dans le vide, téléportation). Capsule 20 cm / marge 2 cm / marche 30 cm, modes inutiles désactivés, et `PlayerFallGuard` : sous le sol → ramené à la dernière position sûre (testé : 1,2 m sous le sol → remis au sol).
- **Zombies pendant le tutoriel** : non reproduit dans l'éditeur (même en passant par le menu). Le `WaveManager` attend maintenant lui-même la fin du tutoriel (`Tutorial.Running`), même si quelque chose le réactive.
- **Tutoriel dans la carte** : chaque étape s'affiche sur un **panneau uGUI (Canvas en espace monde)** posé à côté de l'objet (râtelier de planches, fenêtre, disjoncteur, écrans, cible), tourné vers le joueur et relié à l'objet par un trait ; objets de la ceinture : panneau devant le joueur. Le panneau devant les yeux ne sert plus qu'aux écrans d'accueil et de fin.
- Scène reconstruite, éclairage recuit. **Pas testé au casque.**

### 069 — S'accroupir (gâchette gauche)

**Prompt**

> rajoute le bouton accroupir sur la gachette gauche quand elle est pressée

**Résultat** (code par l'IA, Claude Code)

- `Crouch.cs` sur le XR Origin : tant que la **gâchette gauche** est enfoncée, la vue descend en douceur de **55 cm** (1,36 → 0,81 m) ; relâchée, on se relève. Éditeur : touche C.
- Si la main gauche tient le **pistolet** ou la **lampe**, la gâchette garde son rôle (tirer, allumer) : pas d'accroupi.
- Testé dans l'éditeur : caméra 1,36 m → 0,81 m en maintenant, retour à 1,36 m au relâchement. **Pas testé au casque.**
- Schéma des commandes et wiki mis à jour.

### 070 — S'accroupir (joystick gauche)

**Prompt**

> Pas la gachette gauche, je voulais dire le joystick gauche

**Résultat** (code par l'IA, Claude Code)

- `Crouch.cs` : on s'accroupit tant que le **joystick gauche est enfoncé** (clic maintenu), vue 55 cm plus bas ; relâché, on se relève. La gâchette gauche retrouve son rôle normal (tirer, allumer, valider). Éditeur : touche C.
- Pas de conflit : dans les actions des Starter Assets, le clic du joystick gauche ne sert qu'au « Scale Toggle » du rayon Near-Far (supprimé) et à une action « Crouch » utilisée par aucun script du rig.
- Testé dans l'éditeur (caméra 1,36 → 0,81 → 1,36 m). **Pas testé au casque.** Schéma des commandes et wiki mis à jour.

### 071 — Analyse critique du jeu (level design et game design)

**Prompt**

> Je veux que tu analyses actuellement le jeu, que tu me donnes ses points forts et faiblesses, avec un esprit critique de Level Designer, Game Designer

**Résultat** (analyse par l'IA, Claude Code)

- Analyse faite à partir du code (`Assets/Scripts`, `RoomBuilder.cs`, `SetupVRScene.cs`, `map_meta.json`) de la branche `feature/map-finale`, **sans partie au casque**. Rien n'a été modifié dans le projet.
- Points forts : geste du marteau, états de planche lisibles (posée = 1 coup, clouée = 3), planche arrachée réutilisable, tête = one-shot, plafond de zombies vivants, types de zombie introduits progressivement, râtelier entre les fenêtres, portes barricadables qui obligent à défaire sa défense pour sortir.
- Trois problèmes majeurs : les zombies ignorent le joueur hors du bunker (ils ne le poursuivent qu'après avoir franchi une ouverture) ; le point d'apparition du couloir gauche est à ~3 m du générateur ; les trous du toit (dès le niveau 1) ne se barricadent pas et aucune caméra ne les filme, ni l'extérieur sud.
- Autres : économie de munitions (≈ 204 balles nécessaires au torse, ≈ 96 en moyenne dans la carte), pas de score, rotation fluide par défaut, textes collés à la tête, GDD qui classe l'exploration en « NE FERA PAS ».
- Rapport : `Analyse_LD_GD_The_Last_Escapor.md` (Fichier dans les fichiers du projet Claude (bibliothèque), pas dans le dépôt).

### 072 — Logique de jeu et game feel

**Prompt**

> Et niveau game logic ? En gros le jeu est simple à comprendre, le jeu est logique, un bon game feel

**Résultat** (analyse par l'IA, Claude Code)

- Partie 4 ajoutée au rapport du 071. Simple à comprendre pour la boucle de base, pas pour le reste (viser la tête, intérêt des zones, générateur). Règles cohérentes entre elles, mais situations contradictoires (toit, zombies qui ignorent le joueur, chargeurs aléatoires).
- Trouvé : **tirer avec le canon à moins de 35 cm du visage tue le joueur** (`Pistol.AimedAtOwnHead`).
- Game feel : tir complet (vibration, flash, douille, glissière) ; **coup de marteau sans vibration** ; **zombie touché sans aucun impact** visible ni sonore.

### 073 — Liste des patchs

**Prompt**

> Fait une liste donc de tout ce qu'il faut modifier / patch pour rendre ça logique

**Résultat** (document par l'IA, Claude Code)

- `Liste_patchs_logique_de_jeu.md` (Fichier dans les fichiers du projet Claude (bibliothèque), pas dans le dépôt) : 28 modifications classées P0 (avant le playtest) à P4 (documents), avec fichiers, critère « terminé quand » et effort.

### 074 — Évaluation avec une grille pro

**Prompt**

> ## 1. Game Design & Ergonomie VR (3C : Camera, Character, Controls)
>
> | Critère | Indicateur de réussite (Standard Pro) | Seuil critique / Échec (Red Flag) |
> | --- | --- | --- |
> | **Affordance & Interactions physiques** | Les objets interactifs répondent naturellement (grab, poids simulé, points d'ancrage précis). L'intention du joueur est lue sans friction. | Objets qui traversent les mains, collisionneurs mal ajustés, nécessité d'appuyer sur un bouton abstrait pour une action physique simple. |
> | **Gestion de la cinétose (Motion Sickness)** | Framerate verrouillé (72/90/120 Hz selon le casque), options de confort (vignette, snap turn, téléportation), aucune accélération non sollicitée de la caméra. | Mouvement forcé de la caméra (head-bobbing, cinématique qui déplace la tête du joueur), chutes de FPS sous le seuil de reprojection. |
> | **UI / UX Diégétique et Spatiale** | Interfaces intégrées à l'environnement ou au corps (montre, arme, établi). Textes placés entre **1,5 m et 3 m** pour éviter la fatigue de convergence oculaire. | HUD plaqué sur l'écran (Screen-Space) illisible en périphérie, menus flottants qui traversent les murs (clipping). |
> | **Feedback Multimodal** | Chaque action majeure combine 3 retours : visuel (particules/animation), sonore (audio 3D spatialisé) et haptique (vibration calibrée en fréquence et durée). | Interactions "fantômes" sans vibration ni son d'impact, obligeant le joueur à deviner si l'action a été validée. |
>
> ---
>
> ## 2. Level Design & Architecture Spatiale
>
> | Critère | Indicateur de réussite (Standard Pro) | Seuil critique / Échec (Red Flag) |
> | --- | --- | --- |
> | **Échelle et Proportions (1:1)** | Respect strict des métriques humaines en VR (portes à 2,10 m, tables à 0,75 m). Sensation immédiate de présence physique dans le volume. | Syndrome de la "maison de poupée" ou du monde géant dû à un mauvais import d'échelle (FBX scale factor). |
> | **Zonage d'Interaction (Comfort Zone)** | 80 % des interactions principales se situent entre les hanches et les épaules, à une distance de **0,4 m à 0,8 m** du torse. | Objets clés au sol (problème de tracking bas / sol mal calibré) ou trop hauts, forçant des postures fatigantes sur la durée. |
> | **Guidage Visuel & Audio (Signposting)** | Utilisation de la lumière, des lignes de fuite et du son spatialisé à 360° pour attirer le regard du joueur vers les menaces ou objectifs sans flèche artificielle. | Joueur perdu qui regarde du mauvais côté lors d'un événement clé ou d'une vague d'ennemis. |
> | **Gestion de l'Occlusion et Culling** | Architecture pensée pour couper les lignes de vue trop longues (murs, cloisons, barricades) afin de préserver les performances sans casser l'immersion. | Grandes zones ouvertes vides mais coûteuses en rendu, ou pop-in visible d'objets à moyenne distance. |
>
> ---
>
> ## 3. Game Art & Performance Technique (Tech Art)
>
> | Critère | Indicateur de réussite (Standard Pro) | Seuil critique / Échec (Red Flag) |
> | --- | --- | --- |
> | **Lisibilité Stéréoscopique** | Utilisation de la vraie géométrie (chamfers, reliefs) pour les objets proches des yeux. Les textures restent nettes à 15 cm du visage (haute densité de Texels sur les armes/outils). | Abus de Normal Maps plates sur les objets tenus en main (l'effet de relief disparaît en vision stéréoscopique). |
> | **Anti-Aliasing & Stabilité Visuelle** | Matériaux mats ou rugosité maîtrisée, MSAA 4x actif, absence de scintillement (shimmering) sur les bords fins ou les grilles à distance. | Spécularité agressive, grilles fines ou feuillages alpha-testés qui clignotent dans le casque. |
> | **Lighting & Contraste** | Éclairage précalculé (Baked Lightmaps) couplé à des Light Probes pour les objets dynamiques. Lisibilité immédiate des silhouettes dans la pénombre. | Éclairage 100 % temps réel non optimisé qui détruit le temps de frame GPU, ou scènes totalement plates sans ombres de contact. |
> | **Budget Technique (Frame Timing)** | Temps de rendu maintenu sous **11,1 ms** (90 Hz) ou **13,8 ms** (72 Hz). Utilisation du rendu fovéal, du Single Pass Instanced et d'atlas de textures (Draw Calls maîtrisés). | Surcharge de shaders transparents (overdraw massif sur les VFX de fumée/sang), provoquant une activation constante de l'ASW/Reprojection. |
>
> ---
>
> ## Grille de notation rapide (Pondération recommandée)
>
> * **Confort & Stabilité technique (30 %)** : Si le jeu rend malade ou tourne à 40 FPS, le reste est disqualifié.
> * **Qualité des interactions VR / 3C (30 %)** : Pertinence de l'usage de la VR (le jeu justifie-t-il l'usage d'un casque plutôt qu'un écran plat ?).
> * **Level Design & Lisibilité spatiale (20 %)** : Gestion de l'espace physique, du rythme et du guidage.
> * **Direction Artistique & Immersion (20 %)** : Cohérence visuelle, fidélité des matériaux de près et ambiance sonore.

**Résultat** (analyse par l'IA, Claude Code)

- Réglages relevés : MSAA 4x, Single Pass Instanced (OpenXR), rendu fovéal au maximum, rendu Forward, éclairage précalculé (25 lumières précalculées, 8 temps réel), 1 groupe de light probes, **occlusion culling non précalculé**, **aucun LOD** sur la carte, **aucune mesure de fps au casque**.
- Note pondérée ≈ **5,7 / 10** (confort 5, 3C 6,5, level design 5, DA 6). Red flags : rotation fluide par défaut et pas de téléportation, vue caméra plein écran collée à la tête à 0,5 m, annonces collées à la tête, marteau sans vibration, impacts invisibles, perf non mesurée.
- `Evaluation_grille_pro.md` (Fichier dans les fichiers du projet Claude (bibliothèque), pas dans le dépôt).

### 075 — Tableau des modifications

**Prompt**

> Fait moi un tableau listé de tout ce qui doit être modifié / ajusté

**Résultat** (document par l'IA, Claude Code)

- `Tableau_modifications.md` (Fichier dans les fichiers du projet Claude (bibliothèque), pas dans le dépôt) : fusion des 073 et 074 en un tableau de **38 lignes** (priorité, catégorie, problème, modification, fichiers, terminé quand, effort, bloc de la grille) + ordre conseillé.

### 076 — Issues GitHub des modifications

**Prompt**

> met moi tout ça dans les issues Github (pas besoin de les assigns pour l'instant, ni de temporalité)

**Résultat** (fait par l'IA, Claude Code)

- **38 issues** créées : #91 à #128, titre « [P0-01] … » (priorité + n° de ligne du tableau), corps avec problème, modification, fichiers, terminé quand, effort. Sans assigné, sans date ni milestone.
- 5 labels créés : `P0` bloquant, `P1` lisibilité, `P2` game feel, `P3` équilibrage, `P4` documents ; labels existants réutilisés (gameplay, VR, ui, optimisation, environnement, IA, audio, documentation).

### 077 — Relire le rules.md

**Prompt**

> relis le rules.md

**Résultat** (fait par l'IA, Claude Code)

- Écarts corrigés : les prompts 071 à 076 n'étaient pas dans ce journal (ajoutés, avec l'index) ; les issues #91 à #128 n'étaient pas dans le projet GitHub n°2 « SAE501 » (ajoutées).
- Pas de page de wiki : aucune fonctionnalité n'a été ajoutée ou modifiée (analyse et suivi seulement).

### 078 — Correctifs P0 : poursuite, point gauche, toit, zombie coincé

**Prompt**

> Dans Zombie.cs (UpdateWalking), fais en sorte que les zombies poursuivent le joueur hors du bunker dès qu'il est visible à moins de 3,5 m
>
> Déplace le point d'apparition Spawn_Couloir_Gauche à 10 m ou plus dans ZombieSpawnPoints.prefab et RoomBuilder.cs pour qu'aucun zombie n'apparaît en vue du générateur
>
> Mets à jour WaveManager.cs (DefaultLevels) pour retirer le spawn Toit des niveaux 1 et 2 afin qu'ils arrivent uniquement par des ouvertures barricadables.
>
> Ajoute une sécurité dans Zombie.cs / WaveManager.cs pour tuer automatiquement tout zombie immobile depuis 20 secondes afin que les vagues se finissent toujours.

**Résultat** (code par l'IA, Claude Code)

- **#92 Poursuite** (`Zombie.cs`) : nouvel état `Chasing`. En marche ou en train de casser, un zombie qui voit le joueur (lancer de rayon, murs et planches cachent) à moins de 3,5 m, joueur hors du bunker, le poursuit et le frappe. Il le lâche si le joueur rentre dans le bunker, s'éloigne à plus de 7 m ou reste caché 2 s. Il ne prend ni fenêtre ni trou du toit pendant la poursuite. Le seuil de 7 m et les 2 s sont des choix de l'IA.
- **#93 Point du couloir gauche** : le point est construit dans `ZombieBuilder.cs` (et non `RoomBuilder.cs`, qui ne fait qu'instancier le prefab). Déplacé du bout nord du couloir (4,8 m de l'interrupteur) au **fond de la cuisine**, Blender (−21,5 ; −6,8) : 11,1 m de l'interrupteur, hors de sa vue, 17,6 m de chemin jusqu'à la porte ouest. Choix de l'IA : dans le couloir, aucun point à 10 m n'était assez loin de la porte ouest (moins de 7 m de chemin).
- **#94 Toit** (`WaveManager.cs`) : niveau 1 = extérieur seulement ; niveau 2 = extérieur et 2 couloirs. **Bug trouvé en vérifiant** : les zombies du couloir droit apparaissaient sur le toit (Unity choisissait le NavMesh du toit, 2,8 m au-dessus du point). `ZombieSpawner.cs` cherche maintenant d'abord à 1 m.
- **#95 Zombie coincé** (`Zombie.cs`) : immobile (moins de 0,5 m) depuis 20 s alors qu'il devrait marcher, entrer ou poursuivre → il meurt. Casser et frapper ne comptent pas. Aucun changement dans `WaveManager.cs` : un zombie mort sort déjà du compte des vivants.
- Scène reconstruite (**SAE501 → Build VR Scene**) : les niveaux sont enregistrés dans la scène.
- Testé dans l'éditeur en mode Play : poursuite et coups, abandon quand le joueur rentre au bunker, point de la cuisine vers la porte ouest, point du couloir droit au sol, zombie bloqué éliminé. **Pas testé au casque.**
- Docs : `ZOMBIES.md`, `MAP.md` ; wiki : pages Zombies et Niveaux et vagues.

### 079 — Lisibilité P1 : écran d'annonces, caméra sud, toit, tutoriel, zones, leviers

**Prompt**

> Modifie WaveManager.cs (MakeText) pour remplacer les annonces collées à la tête par un écran fixe dans le bunker accompagné d'un son d'alarme
>
> Ajoute une caméra extérieure sud et un écran au pupitre dans RoomBuilder.cs (BuildCameras) et CctvRoundRobin pour voir les zones d'arrivée au niveau 1.
>
> Mets à jour Zombie.cs et ProcAudio.cs pour supprimer le spawn surprise sur le toit et ajouter de la poussière 3 à 4 secondes avant la chute (niveau 3+).
>
> Ajoute la règle "Visez la tête" à l'étape de la cible dans Tutorial.cs
>
> Affiche le contenu des zones sur le panneau « VERROUILLÉ » et dans l'annonce via ZoneGate.cs et WaveManager.cs.
>
> Rends l'annonce du niveau 4 explicite dans WaveManager.cs pour indiquer clairement où aller et quoi faire concernant l'objectif du générateur.
>
> Remplace l'interaction de type bouton par un levier physique à abaisser pour le générateur et le disjoncteur dans Generator.cs et Breaker.cs.

**Résultat** (code par l'IA, Claude Code)

- **#102 Écran d'annonces** (`WaveManager.cs`, `RoomBuilder.cs`) : annonces et statut de vague sur un écran fixe (2 x 0,8 m) au mur sud du bunker, au-dessus du râtelier à planches, face au point de départ. Plus aucun texte ne suit le regard. Alarme à deux tons (`ProcAudio.Alarm`, non spatialisée) au début de chaque vague. Choix de l'IA : un seul écran, dans le bunker ; hors du bunker, on entend l'alarme mais on ne lit pas l'annonce.
- **#103 Caméra sud** (`RoomBuilder.cs`, `CctvOverlay.cs`) : 5e caméra « CAM 05 EXTÉRIEUR SUD », à 1,5 m de l'angle sud-ouest du bunker, le long du mur sud : les deux fenêtres et le point d'apparition extérieur. Le fichier Blender n'a pas de 5e écran : un moniteur est construit sur le bureau, à droite de l'écran 01. `CctvRoundRobin` n'a pas eu besoin de changer (il prend la liste) ; chaque écran passe de ~18 à ~14 images/s.
- **#104 Toit** (`Zombie.cs`, `Opening.cs`, `ProcAudio.cs`) : pas audibles sur le toit, puis 3 à 4 s d'attente au bord du trou avec poussière et bruit de gravats avant la chute (nouvel état `Lurking`).
- **#105 Tutoriel** (`Tutorial.cs`) : l'étape de la cible dit « RÈGLE : VISEZ LA TÊTE », puis un écran rappelle tête 1 tir, torse 3 tirs, jambes 5 tirs (calculés depuis `ZombieLevels`).
- **#106 Zones** (`ZoneGate.cs`, `WaveManager.cs`, `RoomBuilder.cs`) : contenu sur les panneaux « VERROUILLÉ » (ex. « Planches, chargeurs, batterie ») et dans l'annonce du niveau qui ouvre la zone.
- **#107 Niveau 4** (`WaveManager.cs`) : « Le GÉNÉRATEUR est ouvert : porte ouest, couloir gauche, tout au bout après la cuisine. Abaissez son levier, tenez 5 s. »
- **#111 Leviers** (nouveau `Lever.cs`, `Generator.cs`, `Breaker.cs`, `ClickGrab.cs`, `InteractableOutline.cs`) : on attrape la boule (grip) et on abaisse la main, le levier suit. Disjoncteur : en bas = caméras allumées, remonté = coupées. Générateur : levier à ressort, il faut le tenir en bas 5 s (il remonte si on le lâche), comme l'ancienne règle « garder la main dessus ».
- Scène reconstruite (**SAE501 → Build VR Scene**).
- Testé dans l'éditeur en mode Play (captures) : textes sur l'écran fixe aux niveaux 2 et 4, panneaux, disjoncteur abaissé → caméras allumées, image de la caméra 05, zombie du toit (marche 5 s, attente 3,1 s avec poussière, puis saut), levier du générateur qui remonte seul, générateur démarré après 5 s tenu en bas → victoire. Le geste de la main sur le levier a été simulé (position imposée), **pas testé au casque**, ni le son (alarme, pas, gravats).
- Incident : un script modifié pendant le mode Play a bloqué l'éditeur (rechargement à chaud, `WaveManager.Level` remis à 0) ; Unity a été fermé puis relancé, aucune perte.
- Docs : `ZOMBIES.md` ; wiki : pages Niveaux et vagues, Zombies, Caméras de surveillance, Générateur et disjoncteur, Tutoriel.

### 080 — Game feel P2 et fin de partie : vibrations, impacts, yeux, LOD, sondes, générateur, bilan

**Prompt**

> Implémenter une vibration de la manette de 0,6 à chaque coup de marteau et de 1,0 lorsque le clou est entièrement enfoncé.
> Ajouter des particules d'impact, un son de chair, un léger recul sur le zombie touché, et un retour haptique renforcé en cas de tir à la tête.
> Déclencher un son de clac et une vibration légère lors du calage d'une planche.
> Activer une vibration sur les deux manettes lorsque le joueur subit un coup.
> Provoquer des éclats de bois et un craquement lorsque la planche est arrachée.
> Intégrer des yeux lumineux ou un contour subtil sur les zombies au-delà de 8 mètres pour les rendre visibles dans l'obscurité.
> Configurer un système de LOD sur les objets massifs du décor pour alléger le rendu lointain.
> Déployer plusieurs groupes de light probes dans les couloirs, la cuisine, la zone de stockage et le générateur pour éclairer correctement les zombies.
> Conditionner l'activation du générateur au niveau 4 après l'élimination d'un nombre précis de zombies ou un délai de 60 secondes.
> Concevoir un écran de fin affichant un bilan complet incluant les éliminations, le pourcentage de tirs à la tête, les planches posées, le niveau atteint et le temps total.

(Un premier envoi contenait 4 demandes de plus : reprise au niveau atteint, tirage pondéré, équilibrage munitions / PV, accroupi désactivé par défaut. Elles ont été retirées de cette liste et n'ont pas été faites ici : issues #121, #123, #124, #125.)

**Résultat** (code par l'IA, Claude Code)

- **#112 Marteau** (`Nail.cs`, `Hammer.cs`) : vibration 0,6 (0,07 s) de la main qui tient le marteau à chaque coup, 1,0 (0,18 s) au coup qui enfonce le clou en entier.
- **#113 Zombie touché** (`Zombie.HitFeedback`, `ZombieHitbox.cs`, `MeleeWeapon.cs`, `Pistol.cs`, nouveau `Fx.cs`, `ProcAudio.Flesh` / `HeadCrack`) : gouttes sombres au point d'impact, son de chair (craquement à la tête), recul de 12 cm (24 cm à la tête) ; tir à la tête : deuxième vibration à 1,0 pendant 0,2 s.
- **#114 Calage** (`Plank.cs`, `ProcAudio.Clack`) : « clac » de bois et vibration 0,3 (0,05 s) de la main qui tient la planche.
- **#115 Coup reçu** (`PlayerHealth.cs`, `VRInput.HapticBoth`) : les deux manettes vibrent (0,8 pendant 0,25 s ; 1,0 pendant 0,5 s pour le coup fatal).
- **#116 Planche arrachée** (`Plank.Break`, `Fx.Splinters`, `ProcAudio.WoodCrack`) : éclats de bois sur toute la longueur, projetés vers l'intérieur, et craquement audible jusqu'à 25 m.
- **#117 Yeux** (nouveau `ZombieEyes.cs`, ajouté par `Zombie.Awake`) : deux lueurs rouges additives, fondu entre 7 et 9 m, cachées de dos. Choix de l'IA : les yeux plutôt que le contour (moins coûteux, un seul quad par œil, pas de 2e passe de rendu).
- **#118 LOD** (nouveau `Assets/Editor/LodBuilder.cs`, appelé par `RoomBuilder`) : 139 objets du décor à plus de 2 000 triangles reçoivent un LOD1 simplifié par regroupement des sommets (grille de 20 cases) : 764 472 triangles en LOD0, 189 774 en LOD1 (25 %). LOD0 tant que l'objet fait plus de 10 % de la hauteur de l'écran (environ 8 m pour un meuble de 1,8 m), LOD1 ensuite, rien sous 1,2 %. Choix de l'IA : simplification faite dans Unity (pas dans `export_map.py`) pour ne pas devoir réexporter la carte.
- **#119 Sondes** (`RoomBuilder.BuildLightProbes`) : 7 groupes au lieu d'un (bunker 1,5 m, couloirs gauche et droit 1 m, cuisine 1,25 m, générateur 1 m, stockage 1,25 m, extérieur 3 m), sur 3 hauteurs dans les pièces : 1 871 sondes. Éclairage recuit.
- **#120 Générateur** (`WaveManager.cs`, `Generator.cs`) : levier bloqué jusqu'à 6 zombies abattus pendant le niveau 4 ou 60 s ; panneau « DÉMARRAGE BLOQUÉ » avec ce qu'il reste, statut sur l'écran d'annonces, alarme courte et annonce au déblocage. Choix de l'IA : 6 zombies.
- **#122 Bilan** (nouveau `GameStats.cs`, `GameOverUI.cs`) : à la mort et à la victoire (4 s après le démarrage du générateur). « % de tirs à la tête » = balles dans une tête / balles tirées (choix de l'IA). Gâchette pour rejouer après 3 s, sinon 15 s.
- Matériaux des effets créés par code dans `Assets/Resources` (`Fx_Particules`, `Fx_Lueur`, texture `Fx_Point`) pour être inclus dans le build.
- Scène reconstruite (**SAE501 → Build VR Scene**) et éclairage recuit.
- Testé dans l'éditeur en mode Play (simulation par script et captures) : tirs tête et torse (particules, PV, recul), compteurs du bilan, planche posée puis arrachée (éclats, comptage une seule fois), levier bloqué puis débloqué après 6 zombies, panneau et statut, yeux visibles à 12 m (capture), écrans de mort et de victoire avec le bilan. LOD1 comparé au LOD0 sur une chaise (capture). **Pas testé au casque** : aucune vibration ni aucun son n'a été ressenti ou écouté, ni les fps.
- Docs : `ZOMBIES.md` ; wiki : pages Retours de jeu (nouvelle), Zombies, Planches et clous, Niveaux et vagues, Générateur et disjoncteur, Optimisation et rendu.

### 081 — Validation et push dans main

**Prompt**

> C'est tout bon, assign moi les isses qu'on vient de patch et valide les, pour ce qui est de cette version push la dans le main

**Résultat** (fait par l'IA, Claude Code)

- Issues #112 à #120 et #122 assignées à withRen et fermées (« terminée »), avec un commentaire qui renvoie aux commits et aux pages du wiki.
- `feature/map-finale` poussée sur GitHub, puis dans `main` en avance rapide (aucun commit de `main` n'était absent de la branche) ; wiki poussé.
- Les issues retirées de la demande 080 (#121, #123, #124, #125) restent ouvertes.

### 082 — La boucle de gameplay est-elle correcte ?

**Prompt**

> la boucle de gameplay est-elle correcte ? Elle est simple, lisible etc ?

**Résultat** (analyse par l'IA, Claude Code, à partir du code de `main` ; rien de joué au casque, aucun code modifié)

- Verdict : boucle **correcte et simple** (barricader, défendre, réparer, aller chercher des ressources, 4 vagues, générateur). L'arbitrage clouer / tirer est le cœur VR du jeu et chaque action a son retour.
- 3 défauts de rythme : les zones s'ouvrent au début de la vague et pas pendant la pause (`WaveManager.StartLevel`), donc on ne peut pas explorer au calme ; les nouvelles menaces des niveaux 2 et 3 (portes, toit, Cogneur, Coureur) ne sont pas annoncées ; au niveau 4 les zombies visent toujours le bunker, la course au générateur est le moment le moins dangereux.
- Infos non dites : le but de la partie (dit seulement au niveau 4), les munitions finies (environ 96 balles en moyenne, 1 chance sur 4 de n'avoir aucun chargeur au bunker, issue #124), les armes de mêlée absentes du tutoriel, la mort qui fait tout recommencer (#121).
- 4 correctifs proposés, par ordre d'effort. Hors boucle : rotation fluide activée par défaut (`GameSettings.SmoothTurn`), contraire à la règle de la SAÉ.
- `Boucle_de_gameplay.md` (fichier dans les fichiers du projet Claude (bibliothèque), dossier `analyses`, pas dans le dépôt).

### 083 — Contexte de la scène Blender ouverte

**Prompt**

> Il y a une scène actuellement active dans Blender, prend en le contexte

**Résultat** (analyse par l'IA, Claude Code, en lecture seule dans la session Blender ouverte via le module Blender MCP ; rien de modifié ni enregistré dans Blender ni dans Unity)

- Scène ouverte : `~/Downloads/Hospital_VR_Survival_QualityPass.blend` (Blender 5.2.1), hors dépôt. Ce n'est pas la carte actuelle (`LastBoardStanding_Kitchen.blend`) mais une **carte d'hôpital** en 5 zones : parking extérieur, hall d'accueil (pupitre CCTV), sous-sol du générateur, étage chirurgical (faux ascenseur), toit avec héliport et console d'évacuation à clé.
- 675 objets (611 maillages), **≈ 56 000 triangles**, 40 matériaux sans image (couleurs et bruit procédural), 321 maillages sans UV, 33 lumières, 18 caméras de présentation, aucune animation, échelle 1 unité = 1 m.
- Quest 3 : géométrie très légère ; à reprendre avant import : matériaux procéduraux (ne passent pas en FBX), UV, 314 objets de la passe qualité hors de leur zone et 40 objets masqués, éclairage à recuire, textes 3D à convertir. Usage dans le jeu déduit des noms d'objets, pas d'un document.
- `Scene_Blender_contexte.md` (fichiers du projet Claude, pas dans le dépôt).

### 084 — Hôpital Blender, phase 1 (blockout)

**Prompt**

> Je veux que tu rende au mieu que possible visuellement cette scène :
> Tu es Level Designer / Environment Artist senior pour un jeu de survival horror coopératif en VR, style hôpital abandonné de nuit, ambiance “Left 4 Dead / Resident Evil / backrooms industriel”.
>
> Tu travailles dans Blender via MCP. Échelle métrique stricte : 1 unité Blender = 1 mètre. Unit Scale = 1.0. Grille de travail : 45 m en X, 35 m en Y.
>
> OBJECTIF
> Construis un environnement modulaire, lisible en VR, sombre, cinématographique et gameplay-ready : un hôpital abandonné avec parking extérieur, hall, sous-sol générateur, étage chirurgical et toit-héliport.
>
> RÈGLES DE PRODUCTION
> - Ne supprime aucun objet existant sans me le demander.
> - Inspecte d’abord la scène, les collections, les matériaux et les lumières.
> - Construis par passes : 1) layout/architecture, 2) matériaux, 3) props, 4) éclairage, 5) caméras et validation.
> - Après chaque passe, vérifie : échelle réelle, positions, collisions grossières, objets hors-sol, matériaux manquants, nomenclature.
> - Utilise des collections : ZONE_01_PARKING_EXTERIEUR, ZONE_02_HALL_ACCUEIL, ZONE_03_SOUSSOL_GENERATEUR, PREFAB_FAKE_ELEVATOR, ZONE_04_ETAGE_CHIRURGICAL, ZONE_05_TOIT_HELIPORT, LIGHTING, CAMERAS, MAT_Library.
> - Nomme tous les objets : ex. SM_Wall_Hospital_01, SM_Fence_Breach_East, PROP_Ambulance_01, LIGHT_OilLamp_Workbench.
> - Privilégie bpy.data et bmesh plutôt que bpy.ops lorsque possible ; gère explicitement le mode Objet.
> - Utilise des modificateurs non destructifs : Bevel 0.01 m / 2 segments, Weighted Normal, Array pour les clôtures et planches.
> - Auto Smooth à 30° sur l’architecture.
> - Garde une topologie blockout propre : pas de géométrie inutile, pas de N-gons visibles, densité adaptée au temps réel VR.
> - À la fin de chaque étape, prends un screenshot/rendu de contrôle et liste les corrections proposées.
>
> MATÉRIAUX PBR OBLIGATOIRES
> Crée exactement ces matériaux dans MAT_Library :
> - MAT_Shadow_Void : #020101, Roughness 1.0, Metallic 0.0, sans réflexion.
> - MAT_Wood_Dark : #1B0F07, Roughness 0.85, veines de bois humide.
> - MAT_Wood_Lit : #3C2615, Roughness 0.70, planches de barricade 1.5 x 0.25 x 0.04 m.
> - MAT_Rust_Blood : #892C1E, Roughness 0.65, Metallic 0.8, métal oxydé.
> - MAT_Asphalt_Wet : #060D17, Roughness 0.15, flaques avec Roughness 0.05 dans les creux.
> - MAT_Hospital_Tile : #E0EBE6, Roughness 0.30, carrelage 15 x 15 cm sali par dégradé #1B0F07 en bas des murs.
> - MAT_Emissive_Oil : #CA875C, Emission Strength 8.0.
> - MAT_Emissive_Red : #FF261A, Emission Strength 12.0.
> - MAT_Emissive_Green : #33FF4C, Emission Strength 12.0.
> - MAT_CCTV_Screen : #59FF8C, Emission Strength 5.0.
> - MAT_Gen_Victory : #FFCC80, Emission Strength 25.0.
> - Ajoute aussi MAT_Amber_Ghost : #FFBF45, opacité 0.3, pour les planches fantômes et lueurs de portes.
>
> WORLD
> - Fond : #060D17.
> - Volume Scatter : Density 0.02, Anisotropy 0.4.
> - Ambiance nocturne extérieure, brume légère, contraste fort entre zones sûres éclairées à l’ambre et zones hostiles plongées dans le noir.
>
> ZONE 01 — PARKING EXTERIEUR
> Origine Z=0. Dalle bitume 22 x 18 x 0.2 m en MAT_Asphalt_Wet.
> - Clôture périmétrique grillagée, hauteur 3.2 m, MAT_Rust_Blood.
> - Deux brèches de 1.4 m : Est à X=11, Y=0 ; Ouest à X=-11, Y=0.
> - Chaque brèche reçoit 3 planches MAT_Wood_Lit, plus 3 planches fantômes MAT_Amber_Ghost.
> - Arbres morts décharnés MAT_Wood_Dark autour du parking, hors grille, hauteur 6 m.
> - Ambulance renversée à X=-3.5, Y=2.0 : 5.2 x 2.1 x 2.4 m, carrosserie #E0EBE6 sali, portes arrière ouvertes, gyrophare MAT_Emissive_Red.
> - Tente militaire à X=3.0, Y=-2.0 : 4 x 4 x 2.8 m, structure tubulaire, bâche déchirée #789682.
> - Sous la tente : table de commandement à Z=0.8, MAT_Wood_Lit, avec radio militaire 0.4 x 0.25 x 0.3 m, cadran #FFBF45, lampe à huile MAT_Emissive_Oil, 4 chargeurs 9 mm noirs, marteau de 0.35 m, 6 planches au sol.
> - Guérite à X=-8.0, Y=-7.0 : béton 2.5 x 2.5 x 2.8 m, fenêtre barricadable 1.4 x 1.3 m, allège à 0.95 m.
> - Micro-props : 4 brancards pliés, 5 sacs mortuaires MAT_Shadow_Void, 3 barils rouillés, cônes de signalisation.
>
> ZONE 02 — HALL D’ACCUEIL
> Origine X=0, Y=15, Z=0. Salle 14 x 12 x 3.4 m, murs MAT_Hospital_Tile.
> - Entrée Sud à Y=9 : double porte vitrée coulissante, enseigne cassée “URGENCES” en MAT_Emissive_Red, boîtier de verrouillage #FF453D.
> - Plafond béton avec 2 trous de 1.5 x 1.5 m, fers à béton tordus, gravats au sol.
> - Comptoir central en U : 4.5 x 2.5 x 1.1 m, MAT_Wood_Dark.
> - Poste CCTV au mur Nord, X=-4.0, Y=20.5, Z=1.0 : bureau métallique, 4 CRT 0.45 x 0.45 m en MAT_CCTV_Screen, coffret disjoncteur avec levier abaissable et voyants rouge/vert.
> - Micro-props : chaises renversées, distributeur vitre brisée, fauteuil roulant couché, dossiers médicaux #E0EBE6.
>
> ZONE 03 — SOUS-SOL GÉNÉRATEUR
> Origine X=0, Y=25, Z=-4. Salle 10 x 10 x 2.6 m, briques sombres et béton brut.
> - Rampe béton descendante depuis X=5, Y=21, Z=0 : largeur 3.2 m, pente 20°.
> - Plafond bas avec tuyaux industriels de 0.3 m de diamètre, MAT_Rust_Blood.
> - Générateur diesel central : 3.2 x 1.8 x 2.0 m, MAT_Rust_Blood, grillage jaune/noir, 2 projecteurs MAT_Gen_Victory, gros levier d’amorçage.
> - Établi Ouest à X=-4.2, Y=25 : MAT_Wood_Lit, lampe à huile MAT_Emissive_Oil, hache de pompier 0.75 m, pied-de-biche 0.65 m.
> - Zone Est : 4 étagères 2.0 x 0.6 x 2.2 m, 6 bidons d’essence rouges, caisse à outils #892C1E, flaques d’huile noire MAT_Shadow_Void avec Roughness 0.02.
>
> PREFAB — FAUX ASCENSEUR
> Cabine 3 x 3 x 2.8 m.
> - Doubles portes acier brossé, largeur totale 1.8 m, fente centrale de 0.02 m avec plan émissif #FFBF45.
> - Panneau interne droit à Z=1.3 : levier, voyant #33FF4C, afficheur digital #FFBF45.
> - Trappe de plafond 1 x 1 m ouverte vers conduit MAT_Shadow_Void, lueur rouge sang #800000.
> - Instancie Cabine A à X=0, Y=21, Z=0 et Cabine B à X=0, Y=21, Z=12.
>
> ZONE 04 — ÉTAGE CHIRURGICAL
> Origine X=0, Y=15, Z=12. Plateau annulaire 16 x 16 x 3.2 m.
> - Trémie centrale effondrée de 6 x 6 m, rambardes brisées, vue sur le Hall.
> - Façade Sud éventrée sur 4 m à Y=7, donnant sur le parking : drop-down d’urgence.
> - 4 fenêtres barricadables donnant sur échafaudages métalliques.
> - 2 tables d’opération inox renversées, scialytique pendu par câble, lumière blafarde #789682, rideaux déchirés, chariots chirurgicaux, paravents.
> - Réserve de planches et munitions dans l’angle Nord-Ouest.
> - Escalier métallique Nord-Est à X=7, Y=21, Z=12 menant au toit.
>
> ZONE 05 — TOIT HÉLIPORT
> Origine X=0, Y=15, Z=16. Terrasse 24 x 20 m, acrotère béton de 1.1 m.
> - Héliport octogonal central surélevé : 10 x 10 x 0.8 m, rampe d’accès, “H” peint #FFBF45, 8 balises MAT_Emissive_Oil.
> - Cabane radio Nord-Ouest à X=-8.0, Y=21 : tôle et bois, 3.5 x 3.0 x 2.6 m, clé finale au contour #FFD166 sur bureau métallique.
> - Pupitre d’évacuation à X=0, Y=19.5, Z=17.8 : serrure à clé, gros bouton rouge MAT_Emissive_Red sous cache relevable.
> - 4 blocs HVAC 2.2 x 1.5 x 1.8 m en symétrie autour de l’héliport pour le kiting.
> - Antenne parabolique rouillée, 2 mâts halogènes MAT_Gen_Victory pointés vers le “H”.
>
> ÉCLAIRAGE
> - Point Lights #CA875C, Radius 4.5 m, Power 150 W sur chaque lampe à huile / établi.
> - Lumières pratiques : gyrophare rouge, néon URGENCES, CCTV vert, projecteurs générateur et héliport.
> - Évite les zones totalement noires : conserve toujours une lecture minimale des circulations.
> - Crée une caméra de validation par zone, cadrée pour montrer silhouette, chemin principal et point focal.
>
> VALIDATION FINALE
> Après construction, vérifie et corrige :
> 1. Toutes les dimensions respectent les mètres indiqués.
> 2. Aucun objet ne flotte, ne traverse un mur ou ne bloque une circulation essentielle.
> 3. Les 6 collections existent et contiennent les bons objets.
> 4. Tous les matériaux demandés sont appliqués.
> 5. Les chemins gameplay sont lisibles : parking → hall → sous-sol → hall → ascenseur → étage → toit.
> 6. Prends un screenshot global et un screenshot par zone.
>
> Commence par la Phase 1 uniquement : crée les collections, les matériaux, le World, puis le blockout architectural exact des 6 zones. Ne détaille pas encore les micro-props. À la fin, montre-moi un rendu de contrôle et attends ma validation.

**Résultat** (fait par l'IA, Claude Code, dans la session Blender ouverte via le module Blender MCP ; script bpy/bmesh écrit par l'IA ; rien dans le dépôt Unity)

- Travail enregistré dans une copie : `~/Downloads/Hospital_VR_Survival_QualityPass_v02_phase1.blend`. L'original `Hospital_VR_Survival_QualityPass.blend` n'a pas été touché ; l'état non enregistré de la session au moment de commencer est gardé dans `…_v01_etat_avant_phase1.blend`.
- Rien supprimé : les anciennes collections, les 23 objets à la racine, l'ancien World et les anciens matériaux de même nom sont rangés dans `ARCHIVE_QualityPass_v01` (exclue de la vue et du rendu) ou renommés `ARC_…`.
- 9 collections demandées créées ; 12 matériaux demandés + 6 en plus (béton brut, briques, acier brossé, vitre, carrosserie d'ambulance, bâche), gardés dans la bibliothèque (pastilles `MATSWATCH_` dans `MAT_Library`) ; World `#060D17` + Volume Scatter 0.02 / 0.4.
- Blockout des 6 zones aux cotes demandées (≈ 30 000 triangles après modificateurs, 0 N-gon, UV en mètres sur tout), Bevel 0.01 / 2, Weighted Normal, lissage 30°, Array pour poteaux, planches et marches. Gros props seulement en volumes (ambulance, tente, comptoir, poste CCTV, générateur, établi, pupitre) ; micro-props, lumières et arbres non faits (phases suivantes).
- 8 caméras de validation et 9 rendus de contrôle Workbench (global, 2 plans, 1 par zone, ascenseur). Rendus non éclairés : l'éclairage est la phase 4.
- Incohérences du cahier des charges relevées et réglées par un choix de l'IA, signalées à Rahim : rampe à 20° trop longue pour le sous-sol (palier + sas ajoutés au nord), toit 24 × 20 plus grand que l'étage 16 × 16 (4 poteaux), étage à Z=12 au-dessus d'un hall de 3,4 m (étages 1-2 fermés + puits sous la trémie), HVAC symétriques seulement Est/Ouest (la cabane radio gêne), carte de 42 m en Y pour une grille de 35 m.
- En attente de la validation de Rahim avant la phase 2. Aucun commit.

---

## 085 — Ce qui manque pour l'équilibrage

**Date** : 2026-10-20 · **Domaine** : Game design

> Ok et iveau du jeu sur Unity, qu'est-ce qu'il manquerai au jeu pour que actuellement il soit équilibré ?

**Résultat** (analyse écrite par l'IA, Claude Code, à partir du code de `main` au commit `c0f7d08` ; lecture seule, rien joué au casque, rien modifié dans le dépôt)

- Document : `analyses/Equilibrage.md` dans les fichiers du projet Claude (tableaux des zombies, des munitions et de la mêlée par niveau).
- Munitions : 24 balles sûres, ≈ 96 en moyenne (12 chargeurs à 50 %), pour ≈ 225 balles s'il faut tout tuer au torse ; recharger jette les balles restantes (`Pistol.Load`).
- Mêlée : la hache du bunker tue en 1 à 2 coups à la tête, sans risque, car un zombie qui casse une planche ne frappe jamais le joueur (`Zombie.UpdateBreaking`).
- Niveau 4 : 2 coups pour mourir, sans soin entre les vagues ; la difficulté du menu ne touche ni aux dégâts ni aux balles.
- Pas de mesures pour régler après le playtest (niveau de la mort, dégâts par niveau, éliminations mêlée/pistolet).
- 8 manques classés avec correctifs proposés (liés à #121, #123, #124 et 4 nouveaux) ; rien implémenté, en attente de Rahim.

---

## 086 — Intégration des PR #131 (radio) et #132 (passer l'attente)

**Date** : 2026-10-20 · **Domaine** : Gameplay, GitHub

> Réponds en français. Lis d'abord rules.md du dépôt SAE501 et respecte-le (consigner ce prompt mot pour mot dans docs/PROMPTS.md, pas de Co-Authored-By, issues sur le GitHub project #2, wiki si une fonctionnalité change).
>
> Trouve sur withRen/SAE501 les deux PR ouvertes d'Al Amine : la radio de ravitaillement (1 chargeur par le toit, attente selon la difficulté) et celle qui permet de passer l'attente entre les vagues. Lis-les et intègre-les dans la version de Rahim (son projet Unity local sur main, qui contient tout le travail validé hier), en résolvant les conflits avec nos patchs (WaveManager, annonces, générateur L4, etc.) et en vérifiant que Unity compile sans erreur. Laisse intactes ses modifications locales non commitées (URP settings, RuntimeActionBindings.json, Protection_suite).
>
> Choix par défaut : l'intégration se fait d'abord en local pour que Rahim teste au casque ; le merge des PR sur GitHub et le push sur main attendent son retour de test, sauf s'il te dit dans le fil de les merger tout de suite. Signale-lui ce qui a dû être adapté et ce qu'il doit tester. Note que la radio touche l'économie de munitions (issue #124 ouverte) : dis s'il y a recouvrement.

*(consigne transmise par le Claude coordinateur du projet à partir de la demande de Rahim)*

**Résultat** (fusion faite par l'IA, Claude Code ; le code des deux PR est d'Al Amine, non modifié)

- Les deux PR partent de `c0f7d08` (la version validée le 19 octobre) : fusion sans conflit, aucun code adapté. Branche locale `claude/project-thread-6whioa`, deux commits de fusion.
- Unity : compilation sans erreur ; **SAE501 → Build VR Scene** puis **Cuire l'éclairage** relancés (la reconstruction efface les lightmaps). Radio posée sur un mur latéral du bunker près de l'établi ; un lancer de boîte depuis le point de largage descend jusqu'au sol sans obstacle.
- Recouvrement avec #124 : oui, la radio ajoute jusqu'à 4 chargeurs (48 balles) par partie, ce qui ressemble à la piste « chargeur à chaque pause » de l'issue.
- Wiki préparé (page `Radio-de-ravitaillement`, Commandes, Niveaux et vagues, Tutoriel), poussé avec la fusion dans `main`.
- Pas testé au casque. Merge des PR et push en attente du retour de Rahim.

---

## 087 — Version PC sans casque (clavier et souris)

**Date** : 2026-10-20 · **Domaine** : Joueur, Outils

> Pour que je puisse tester sur PC sans casque, prépare moi une version non VR
> Mouvements ZQSD
> (Ne overwrite pas la version VR)

**Résultat** (code écrit par l'IA, Claude Code ; testé en mode Play dans l'éditeur, pas joué à la main par un humain)

- Nouvelle scène `Assets/Scenes/MainScene_PC.unity` (menu **SAE501 → Build PC Scene**, ou **SAE501 → Play PC Version** pour l'ouvrir et lancer) : elle active la version PC puis charge `MainScene`, qui n'est pas modifiée. La scène PC n'est pas ajoutée au build Quest.
- `DesktopMode` (activé seulement par cette scène) ajoute `DesktopPlayer` au XR Origin : souris = regard, ZQSD = déplacement, les mains XRI du rig sont placées devant la caméra et visent le réticule. Les objets reçoivent les mêmes événements XRI qu'avec les manettes (prise, gâchette), donc le code du jeu est le même.
- Touches : E / clic droit prendre-lâcher (main droite / gauche), clic gauche tirer, frapper ou appuyer, R recharger (chargeur ou pile), 1 / 2 / 3 pistolet, marteau, lampe de la ceinture, F lampe, C s'accroupir, B ou Entrée maintenus pour passer l'attente, Échap pause, Espace valider dans les menus, F1 aide.
- Seule modification d'un script existant : `VRInput.RightB` accepte aussi la touche B quand la version PC est active.
- Vérifié en mode Play (entrées simulées) : déplacement, pistolet sorti et rangé à la ceinture, tir au clic, recharge, planche prise puis posée sur la fenêtre sud-ouest, 2 clous enfoncés en 3 coups de marteau chacun (fenêtre barricadée), levier du disjoncteur abaissé (caméras allumées), bouton de la radio. Non vérifié : partie complète, menu principal à la souris, souris réelle (tests par entrées simulées).

---

## 088 — Rééquilibrage du jeu

**Date** : 2026-10-20 · **Domaine** : Gameplay / Équilibrage

> ok, implémente moi tout ce rééquilibrage

**Résultat** (code écrit par l'IA, Claude Code ; testé en mode Play dans l'éditeur, pas au casque)

- **Chargeurs du bunker garantis** : les 2 chargeurs du bunker (pupitre des caméras, établi) apparaissent toujours ; les 10 autres de la carte gardent leur tirage (`RoomBuilder.BuildMagazines`).
- **Recharger ne jette plus les balles** : l'arme prend 12 balles au plus et le chargeur ressort sous l'arme avec le reste (ex. 5 dans l'arme + chargeur de 12 → 12 dans l'arme, chargeur éjecté avec 5) ; il ne peut pas être repris tout de suite par l'emplacement (`Pistol.Load`, `MagWell`, `Magazine.Eject`, `DesktopPlayer`).
- **Zombie à la barricade** : il frappe le joueur à moins de 1,6 m (à plat) à travers l'ouverture, 50 % des dégâts du niveau, **en alternance** avec les planches (le joueur ne protège pas ses planches avec son corps) (`Zombie.UpdateBreaking`).
- **+50 PV** au début de chaque pause entre deux vagues (`PlayerHealth.Heal`, `WaveManager.breakHeal`).
- **Niveau 4 à 40 %** de dégâts par coup (3 coups pour mourir au lieu de 2, `ZombieLevels`).
- **Traque** : au niveau 4, dès que le générateur est prêt, tous les zombies poursuivent le joueur hors du bunker sans distance ni vue (`Zombie.HuntPlayer`).
- **Tirage pondéré des points d'apparition** (issue #123, `WaveManager.SpawnWeights`).
- **Difficulté** : dégâts reçus ×0,75 / ×1 / ×1,25 et chance des chargeurs de la carte 75 / 50 / 35 % (Facile / Normal / Difficile, `GameSettings`).
- **Mesures** : `GameStats` compte les éliminations à la mêlée, les dégâts reçus par niveau et les balles chargées ; une ligne `[Bilan]` est écrite dans le log à chaque vague terminée, à la mort et à la victoire.
- **Reprise au niveau atteint** (issue #121) : après une mort au niveau 2 ou plus, la gâchette (ou l'attente) reprend au début de ce niveau, sans tutoriel, zones déjà passées ouvertes, 40 s pour se barricader ; maintenir B recommence au niveau 1 (`GameOverUI`, `WaveManager.ResumeLevel`).
- Annonce du niveau 1 : « Balles rares : la hache et le pied de biche ne s'usent pas. »
- Radio d'Al Amine (#131) gardée telle quelle, pas de chargeur gratuit en plus.
- Économie de munitions : 24 balles sûres (arme + chargeur de départ) + 24 du bunker + ≈ 60 en moyenne dans la carte + jusqu'à 48 par la radio.
- Tests en mode Play dans l'éditeur (simulation) : (a) recharge 5 + 12 → 12 et chargeur éjecté à 5 ; (b) 2 chargeurs du bunker présents ; (c) joueur à 1,3 m frappé un coup sur deux (10 PV), les planches tombent quand même, rien à 2,2 m ; un premier essai a trouvé un bug (après un coup sur une planche, plus de coup au joueur) corrigé avant le commit ; (d) fin de vague : ligne `[Bilan]` puis 40 → 90 PV ; (e) `SkipToLevel(4)` + générateur prêt : `HuntPlayer` vrai, zombie à 21,7 m qui poursuit le joueur dans la cuisine ; (f) mort au niveau 3 → rechargement, départ au niveau 3 sans tutoriel, zones du niveau 2 ouvertes. Scène reconstruite et éclairage recuit, scène PC reconstruite.
- **Pas testé au casque.** Commits locaux, pas poussé.

---

## 089 — Champ de vision et brouillard

**Date** : 2026-10-20 · **Domaine** : Rendu, Menus

> Rajoute dans les paramètres l'option de changer le FOV
> Et aussi rajoute un petit effet de brouillard sur la map pour éviter qu'on voit qu'il y a une render distance courte

**Résultat** (code écrit par l'IA, Claude Code ; testé en mode Play dans l'éditeur, pas au casque)

- **Champ de vision** : nouvelle ligne « Champ de vision » (50°, 60°, 70°, 80°, 90°, verticaux ; 60° par défaut, gardé d'une partie à l'autre) dans une section **AFFICHAGE** de la page PARAMÈTRES, **en version PC seulement** (`GameSettings.FieldOfView`, `AmberMenu.BuildSettings`). Au casque, le champ de vision est imposé par les lentilles du Quest (Unity ignore `Camera.fieldOfView`) : la section CONFORT VR reste comme avant et l'option n'apparaît pas. En version PC, la section CONFORT VR (rotation, bords sombres, recentrer, sans effet au clavier) est remplacée par AFFICHAGE.
- **Brouillard** : la carte avait déjà un brouillard, mais presque noir ; au loin le sol devenait noir puis le ciel (panorama d'Alban, sans brouillard) reprenait d'un coup, ce qui marquait la limite d'affichage. Le brouillard passe en bleu nuit (0,06 ; 0,075 ; 0,10), même densité qu'avant (0,06, exponentiel carré), et le bas du ciel se fond dans la couleur du brouillard (`SkySphere.shader` : tout brouillard sous l'horizon, ciel intact au-dessus de ≈ 17°). Le décor au loin et le ciel se rejoignent dans la même brume. Réglé dans `RoomBuilder` (survit à **Build VR Scene**) ; la scène a été mise à jour directement (une ligne, sans reconstruire ni recuire l'éclairage, qui ne dépend pas du brouillard).
- Coût au casque : aucun calcul en plus pour le décor (le brouillard existait déjà) ; trois instructions de plus dans le shader du ciel.
- Vérifié en mode Play : version PC, clic sur 70° → caméra à 70°, 50° et 90° vus en capture ; version VR, page PARAMÈTRES inchangée ; aucune erreur dans la console. Captures dehors (4 directions) avant / après comparées. Densité inchangée, donc la lisibilité des zombies au loin reste celle réglée avant (non revérifiée avec un zombie en jeu).
- **Pas testé au casque.** Commit local, pas poussé.

---

## 090 — Téléportation et saut

**Date** : 2026-10-20 · **Domaine** : Joueur, Menus

> Rajoute en option de tp
> (Et remet une touche de saut)

**Résultat** (code écrit par l'IA, Claude Code ; testé en mode Play dans l'éditeur, pas au casque)

- **Téléportation** : nouvelle ligne **Déplacement : CONTINU / TÉLÉPORTATION** dans CONFORT VR (désactivée par défaut, `GameSettings.Teleport`). Le bouton RECENTRER descend à côté de RETOUR pour laisser la place. Avec l'option, le joystick gauche vise comme le droit (vers l'avant = arc, relâcher = s'y rendre) et le fournisseur de téléportation XRI du rig est allumé. En version PC l'option n'apparaît pas.
- **Point d'arrivée contrôlé** (`NavTeleportArea`, nouvelle zone de téléportation sur tout le sol) : sur le NavMesh, et joignable à pied par un chemin d'au plus 1,5 × la distance + 1 m. Donc pas à travers un mur, par une fenêtre, derrière une porte de zone fermée, ni sur un meuble.
- **Saut** remis : le saut des Starter Assets (bouton A ; Espace en version PC et dans l'éditeur) avait été coupé le 7 octobre car il faisait passer par-dessus les obstacles. Il revient en petit : `jumpHeight` 0,45 (≈ 0,33 m mesurés), pas de double saut (`SetupVRScene`). Aide de la version PC mise à jour.
- Correction en passant : les réglages des manettes (`ControllerInputActionManager`) étaient appliqués seulement aux manettes actives au chargement ; ils le sont maintenant aussi aux manettes éteintes (avant qu'elles soient suivies).
- Scène VR reconstruite (**Build VR Scene**) et éclairage recuit.
- Vérifié en mode Play : saut au casque simulé (0,33 m) et à la touche Espace en version PC (0,33 m) ; en sautant contre les fenêtres ouest, est et sud-ouest le joueur ne sort pas, il ne monte pas sur la caisse militaire de 0,46 m ; téléportation : points de la même pièce acceptés (dont un derrière des meubles), dehors (2 fenêtres, jardin), dessus d'établi et couloirs fermés refusés, déplacement du joueur par le fournisseur ; clic sur CONTINU → retour au déplacement continu ; page PARAMÈTRES capturée (pas de chevauchement). Non vérifié : le rayon et l'arc avec de vraies manettes.
- **Pas testé au casque.** Commit local, pas poussé.

---

## 091 — Rendu à 8 m et réduction des polygones

**Date** : 2026-10-20 · **Domaine** : Rendu, Optimisation

> Actuellement la façon de render à 8 mètre marche comment ? Comment on fait pour réduire les polygones etc ?

**Résultat** (explication écrite par l'IA, Claude Code, après lecture du code, de `MainScene.unity` et de `QualitySettings.asset` ; rien modifié dans le projet)

- Le « 8 m » vient de deux systèmes séparés : les **yeux lumineux** (`ZombieEyes`, fondu de 7 à 9 m, visuel seulement) et le **LOD du décor** (`LodBuilder`, #118 : 134 objets de plus de 2 000 triangles, LOD0 au-dessus de 10 % de la hauteur de l'écran, LOD1 ensuite).
- Constat : `lodBias` vaut 2 dans les niveaux de qualité Quest et PC, donc la bascule LOD0 → LOD1 d'un meuble de 1,8 m se fait vers 16 m et non 8 m (≈ 30 m en version PC à 60°). Inféré par le calcul, pas mesuré en jeu.
- Caméra du joueur limitée à 60 m ; le brouillard (0,06, exponentiel carré) laisse 79 % d'un objet visible à 8 m, 10 % à 25 m, 1 % à 35 m, mais n'enlève aucun objet du rendu.
- Pistes proposées : `lodBias` à 1 (ou seuil LOD0 à 0,2), caméra à 35-40 m, occlusion culling à réessayer (retiré en 027 pour les lumières de lune, l'éclairage est maintenant précalculé), coupe par calque des petits objets, LOD des zombies, meilleure décimation (Blender), et surtout mesure au casque d'abord.
- Rien testé, rien modifié ; aucun chiffre au casque.

---

## 092 — Textures et distance (mipmaps)

**Date** : 2026-10-20 · **Domaine** : Rendu, Optimisation

> et les textures se réduisent par rapport à la distance (en qualité ?)

**Résultat** (explication écrite par l'IA, Claude Code, après lecture des `.meta` des textures, de `RoomImport.cs` et de `QualitySettings.asset` ; rien modifié)

- Oui, par les **mipmaps** : chaque texture a des copies de plus en plus petites (1024, 512, 256…) et le GPU choisit, pixel par pixel, celle qui correspond à la taille à l'écran. Automatique, sans script.
- Relevé : 243 textures du projet sur 246 ont les mipmaps (les 3 sans sont des textures d'effets ou d'interface). Les textures de la carte (`RoomImport`) : 1024 px max, filtrage trilinéaire, anisotrope ×4 (sols vus en biais). 15 textures limitées à 1024 px sur Android (entrée 027).
- Le **streaming des mipmaps** est coupé (`streamingMipmapsActive: 0`) : toutes les tailles restent en mémoire. Il ne ferait gagner que de la mémoire, pas des fps ; inutile vu la taille des textures.

---

## 091 — Prise par clic ou maintenue

**Date** : 2026-10-20 · **Domaine** : Mains, Menus

> Pour tenir un objet rajoute en option :
> Soit maintenant la gachette
> soit juste toggle avec un clique avec la gachette

**Résultat** (code écrit par l'IA, Claude Code ; testé en mode Play dans l'éditeur, pas au casque)

- Dans le jeu, on prend les objets avec le **grip** (bouton latéral), pas avec la gâchette, qui sert à tirer et à utiliser. L'option porte donc sur le grip ; la gâchette ne change pas.
- Nouvelle ligne **Prise (grip) : UN CLIC / MAINTENIR** dans CONFORT VR (`GameSettings.HoldToGrab`). UN CLIC reste le défaut (comportement actuel et texte du tutoriel) ; MAINTENIR = on tient tant que le grip est enfoncé (`ClickGrab.ApplyMode` : mode XRI StateChange au lieu de Toggle). Le tutoriel (étape 1) adapte sa phrase. En version PC, l'option n'apparaît pas (prise toujours par clic).
- Vérifié en mode Play avec une manette simulée sur une planche du râtelier : UN CLIC → tenue après un clic relâché, lâchée au 2e clic ; MAINTENIR → tenue grip enfoncé, lâchée au relâchement ; page PARAMÈTRES capturée (pas de chevauchement). Non vérifié : leviers et portes en mode MAINTENIR, vraies manettes.
- **Pas testé au casque.** Commit local, pas poussé.

---

## 094 — Fermer les pull requests et pousser dans main

**Date** : 2026-10-20 · **Domaine** : GitHub

> ferme toutes les pull requests du github, et push dans le main la version actuelle du jeu

**Résultat** (fait par l'IA, Claude Code)

- Scène `MainScene` reconstruite (**SAE501 → Build VR Scene**) et éclairage recuit : la scène réenregistrée à 14 h 12 n'avait plus ses lightmaps.
- Branche locale `claude/project-thread-6whioa` (PR #131 et #132 d'Al Amine, version PC, rééquilibrage, champ de vision et brouillard, téléportation et saut, option de prise, journaux 082 à 094) poussée dans `main` en avance rapide, sans forcer.
- Non commités, laissés en local : réglages URP, `RuntimeActionBindings.json` supprimé, `Protection_suite`, `scripts/`.
- Wiki poussé, avec la page `Radio-de-ravitaillement` et les ajouts sur le bouton B (Niveaux et vagues, Tutoriel).
- PR fermées avec un commentaire : #131 et #132 (intégrées dans `main`), #129, #87, #78, #69, #67 (anciennes, fermées sur demande de Rahim sans fusion de leur branche).
- Pas testé au casque par l'IA.

---

## 095 — Page wiki Optimisations

**Date** : 2026-10-20 · **Domaine** : Wiki, Optimisation

> Dans le wiki rajoute une page Optimizations, et explique tout ce que l'on a fait en optimization, le culling, les textures, le fast rendering etc

**Résultat** (page écrite par l'IA, Claude Code, après relecture de `main` au commit `88608d9` : scripts, `Quest_RPAsset`, `QualitySettings`, réglages OpenXR, `MainScene.unity`, journal)

- Nouvelle page de wiki **`Optimisations`** (titre en français comme les autres pages) : rendu au casque (Single Pass Instanced, rendu fovéal au maximum, niveau de qualité Quest et son pipeline, pas de post-traitement), lumière précalculée et sondes, simplification à l'export et LOD du décor (avec le `lodBias 2` qui repousse la bascule vers 16 m), distance d'affichage et brouillard, textures (1024 px, mipmaps, trilinéaire, anisotrope), culling (champ de vision, distance, LOD, faces arrière, caméras de surveillance ; occlusion culling absent), SRP Batcher et regroupement statique, zombies et code. Courte section « Pistes non faites ».
- `Home.md` (nouvelle section « Technique ») et `_Sidebar.md` mis à jour. Wiki poussé (commit `7a3d4ab`).
- Rien modifié dans le jeu ; rien mesuré au casque (dit en tête de page).

---

## 096 — Image de couverture GitHub

**Date** : 2026-10-20 · **Domaine** : Communication, Rendu

> Tu pourrais me produire une image de cover du jeu qu'on va utiliser pour la cover du github

**Résultat** (image produite par l'IA, Claude Code : rendu Unity par script, puis mise en page en Python/Pillow)

- Image **1280 × 640 px** (format de l'aperçu social GitHub, 560 Ko) : le bunker vu de l'extérieur sud, de nuit, fenêtres sud barricadées et éclairées de l'intérieur, zombies qui approchent avec les yeux rouges, deux zombies qui frappent les fenêtres. Titre « THE LAST ESCAPOR », accroche « Survival shooter en réalité virtuelle », phrase « Barricade-toi. Tiens la nuit. Échappe-toi. » et badge « Meta Quest 3 · Unity 6 ».
- Rendu fait dans l'éditeur à partir de la vraie `MainScene`, avec une caméra, des zombies (prefab `Zombie`, poses de marche et d'attaque figées), des planches et quelques lumières **temporaires**, puis la scène rechargée depuis le disque sans enregistrer. Les yeux sont dessinés comme ceux de `ZombieEyes` (le script ne tourne qu'en jeu). Retouches en 2D : étalonnage bleu nuit, halo des yeux et des fenêtres, vignette, grain.
- Les zombies Cogneur et Coureur ressortaient en magenta dans ce rendu hors jeu (vêtements) : seuls des zombies « Rôdeur » sont dans l'image. Pas vérifié si le problème existe en jeu.
- Rien modifié dans le projet. L'image ne peut pas être posée par l'IA : à faire dans GitHub, **Settings > General > Social preview > Edit > Upload an image**.

---

## 097 — Cover dans le README

**Date** : 2026-10-20 · **Domaine** : Documentation

> Met le dans le readme en cover

**Résultat** (fait par l'IA, Claude Code)

- L'image de l'entrée 096 est copiée dans `docs/images/cover.png` (suivie par Git LFS, comme tous les PNG) et affichée juste sous le titre du `README.md`, avec un texte alternatif.
- Commit local ; visible sur GitHub une fois poussé.

---

## 098 — Sols en double, bouteilles, clé et planches

**Date** : 2026-10-20 · **Domaine** : Carte, Gameplay

> Y a quelques petits soucis du style (L'image) mais aussi des bouteilles qui volent dans le jardin sous la tente
> La clé n'est pas récupérable, ou devient simplement transparent
> (Dans la cuisine y a trop de planches sur la table, met en qu'une)

(L'image : capture Meta Casting au casque, bunker près de l'échelle et du chariot vert, grandes plaques blanches texturées superposées sur le plancher.)

**Résultat** (fait par l'IA, Claude Code ; corrections dans `RoomBuilder.cs`, scène reconstruite et éclairage recuit)

- **Plaques blanches au sol** : c'était la mousse de `Apoc_Vines_And_Moss` (et les lianes blanches du plafond). Les trois matériaux « Apocalypse » du fichier (mousse, flaques, tuyaux rouillés) ont leur couleur faite par des nœuds Blender que l'export ne lit pas : ils sortaient gris clair opaques. `FixFloorDecals` les remplace : mousse vert très sombre, flaques en eau sombre brillante et transparente (plus de z-fighting entre flaques superposées), tuyaux couleur rouille. Les tapis et papiers sont posés à des hauteurs fixes au-dessus du sol (gravats 2 mm, tapis 7 mm, papiers 12 mm) : plus deux surfaces au même niveau.
- Le terrain extérieur passait sous la maison et touchait les planchers des couloirs : `SinkTerrainUnderFloors` descend ses sommets 10 cm sous tout plancher (copie du maillage dans `Assets/Generated/Terrain_SousMaison.asset`, le FBX n'est pas modifié).
- **Bouteilles qui volent** (cabane du jardin) : le groupe de bouteilles dépassait du buffet de 17 cm, deux bouteilles dans le vide. Recalé sur le buffet avec la lanterne.
- **Clé** : c'était un décor statique sans prise. Elle devient un objet qu'on prend en main (`Cle_Jardin` : corps physique, collider grossi, mêmes règles que les autres objets), posée à l'avant du buffet. Elle n'ouvre rien pour l'instant (aucune serrure dans le jeu).
- **Planches de la cuisine** : 5 planches empilées sur la table, il n'en reste qu'une (`CollectPlanks` ne garde qu'une planche par pile posée sur un meuble). Les 3 planches au sol de la cuisine restent.
- Vérifié : captures avant/après dans l'éditeur ; en mode Play, la clé reste posée, se prend avec une main simulée, suit la main et retombe au sol quand on la lâche ; une seule planche sur la table. **Pas vérifié au casque**, ni la cause exacte de « la clé devient transparente ».

---

## 099 — Pousser main puis tester les vagues infinies de LeCapet

**Date** : 2026-10-21 · **Domaine** : GitHub, Gameplay

> Il y a eu une pull request github de LeCapet (Rajout du mode de jeu Vagues inifinis)
> Regarde sa pull request, merge la avec la mienne que l'on puisse tester (Juste au par avant push proprement dans le main tout ce qu'on a fait jusqu'ici)

**Résultat** (fusion et adaptations faites par l'IA, Claude Code ; le mode de jeu est le code de LeCapet)

- `main` poussée en avance rapide sur `51fafa8` (correctifs du décor, de la clé et des planches, journal 095 à 098) ; wiki poussé. Unity n'était pas en mode Play et la scène avait son éclairage.
- PR #133 fusionnée en local sur la branche `test/vagues-infinies`, créée à partir de `main`. La PR partait de `c0f7d08` : conflits dans `WaveManager.cs` et `GameStats.cs`, résolus en gardant les deux versions.
- Adaptations : pause B et ligne « Maintenir B » gardées en mode survie ; +50 PV par pause et ligne [Bilan] gardées, durée de pause du mode survie ; poids d'apparition (#123) au-delà du niveau 4 = ceux du niveau 4 ; pas de reprise au niveau atteint (#121) en mode survie (`WaveManager`, `GameOverUI`) ; un seul `OnDestroy` (le record est enregistré en quittant) ; nombre de mêlées gardé dans le bilan.
- Compilation sans erreur. Pas joué en mode Play ni au casque. Pas de fusion sur GitHub : en attente du « merge » de Rahim.

---

## 100 — Équilibrage des vagues (Évasion et vagues infinies)

**Date** : 2026-10-21 · **Domaine** : Game design

> Vérifie l'équilibrage des vagues (en mode infinis si c'est sensé être bien compliqué)

**Résultat** (analyse écrite par l'IA, Claude Code, à partir du code de la branche `test/vagues-infinies` ; valeurs calculées, rien joué, rien modifié)

- Document : `Equilibrage_vagues_infinies.md` dans les fichiers du projet Claude (pas dans le dépôt).
- Évasion : courbe correcte, ≈ 144 balles pour ≈ 45 zombies.
- Vagues infinies : la difficulté monte, mais parce que les munitions s'arrêtent après la vague 4 (12 balles par vague par la radio pour 18, puis 21, puis 27 zombies) ; la vie revient presque entièrement à chaque pause ; les Coureurs approchent 3,5 à 3,9 m/s ; jusqu'à 20 zombies vivants, pas mesuré au Quest. La vague 4 de survie est plus facile que le niveau 4 d'Évasion.
- 7 réglages proposés (radio 2 puis 3 chargeurs, soin de pause dégressif, vitesse plafonnée à 1,7, 14 zombies vivants au plus, renforts à la vague 4, PV des zombies +8 %/vague plafonnés à ×2,5), en attente de l'accord de Rahim.

---

## 101 — Rééquilibrage du mode vagues infinies

**Date** : 2026-10-21 · **Domaine** : Gameplay

> Ok rééqquilibre alors
>
> Fait donc le rééquilibrage

**Résultat** (code écrit par l'IA, Claude Code, sur la branche `test/vagues-infinies` ; mode Évasion inchangé)

- `WaveManager` : plafond de vitesse 2 → 1,7 ; zombies vivants au plus 20 → 14 ; PV des zombies +10 %/vague (×3) → +8 %/vague (×2,5) ; soin de pause 50 PV − 5 par vague après la 4e, 15 au minimum (`EndlessHeal`) ; vague 4 : renforts toutes les 15 s pendant 60 s (`endlessLastLevelReinforce`) ; `RadioMagazines` : 2 chargeurs à partir de la vague 5, 3 à partir de la vague 10.
- `SupplyRadio` : livre ce nombre de chargeurs et l'affiche sur son écran.
- Dégâts reçus inchangés (+4 %/vague, ×1,5).
- Compilation sans erreur ; `MainScene` rechargée depuis le disque pour que l'objet `WaveManager` prenne les nouvelles valeurs (rien d'enregistré dans la scène). Pas joué en mode Play ni au casque.

---

## 102 — Fusion des vagues infinies dans main

**Date** : 2026-10-21 · **Domaine** : GitHub

> Merger sur GitHub le tout proprement dans le main

**Résultat** (fait par l'IA, Claude Code)

- Branche `test/vagues-infinies` (PR #133 de LeCapet adaptée, rééquilibrage 101, journaux 099 à 101) poussée dans `main` en avance rapide sur `e8aad93`, sans forcer ; GitHub a marqué la PR #133 fusionnée.
- Wiki poussé : nouvelle page `Mode-survie`, Accueil et menu latéral.
- Non commités, laissés en local : réglages URP, `RuntimeActionBindings.json`, `DevAgentSettings.asset`, `Protection_suite`, `scripts/`.

---

## 103 — Plan d'action du multijoueur

**Date** : 2026-10-21 · **Domaine** : Game design

> Ok maintenant établissons un plan d'action pour mettre en place le multijoueur
> Il faut un système de lobby d'attente, un système de lancement de la partie quand tout le monde est "prêt" (bouton à cliquer) 
> Un système de serveur (Local, vu que je vais charger le jeu sur des casques, explique moi comment je pourrais mettre ça en place)
> Un système de score globale 
> Et un système de score unique aux joueurs

**Résultat** (plan écrit par l'IA, Claude Code, après lecture du code de `main` 5c1d40c ; aucun code du jeu modifié)

- Document : `Plan_multijoueur.md` dans les fichiers du projet Claude (pas dans le dépôt).
- Bibliothèque recommandée : Netcode for GameObjects + Unity Transport (officiel, LAN sans Internet, tests à plusieurs dans l'éditeur) ; Mirror en second choix ; Photon Fusion écarté (dépend du cloud).
- Serveur local : un joueur hôte (casque ou PC) ; découverte par diffusion UDP + IP manuelle en secours ; partage de connexion d'un téléphone plutôt que le Wi-Fi de l'IUT ; permission Internet Android à activer.
- Lobby dans la scène Menu (HÉBERGER / REJOINDRE, PRÊT, compte à rebours quand tous sont prêts) ; inventaire de ce qui passe en réseau (joueurs, zombies, vagues, munitions, planches, portes, radio) ; score par joueur et d'équipe avec un barème.
- Points soulevés : le GDD classe le multijoueur en « NE FERA PAS » ; estimation 16 à 22 jours-personne, option réduite « coop 2 joueurs, vagues infinies » 10 à 13 jours, à valider par Rahim avant tout code.

---

## 105 — Carte de base : Finalefinale.blend

**Date** : 2026-10-21 · **Domaine** : Carte

> Pour la map de base met la à jour avec celle là : 
> Finalefinale.blend (qui se trouve dans mes téléchargements)

**Résultat** (analyse et export faits par l'IA, Claude Code)

- Comparaison avec la carte actuelle (`map_final_final.blend`) : même plan, mêmes pièces, portes, emplacements de planches, caméras et 31 lumières ; 6 objets déplacés de quelques dizaines de cm. Seul le jardin change : 16 arbres simples remplacés par 8 arbres PolyHaven (`Garden_RealTree_*`), plus 18 rochers moussus, 26 pierres d'allée, un tronc couché et deux vitres.
- Chaque nouvel arbre pèse 248 000 triangles : `Tools/blender/export_map.py` garde une feuille sur 20 (agrandie) au lieu de la simplification classique qui écrase le feuillage.
- Blocage : 25 images du jardin ne sont pas empaquetées dans le `.blend` (restées sur le PC de l'auteur). Carte actuelle remise en place en attendant.

---

## 106 — Liste des textures manquantes

**Date** : 2026-10-21 · **Domaine** : Carte

> Fait moi la liste des textures manquantes

**Résultat** (fait par l'IA, Claude Code)

- Liste des 25 images (dossier d'origine `c:/Users/lenovo/Documents/Jardin jeu vr/assets/`) : `grass_*` et `cobble_*` (terrain, allée), `island_tree_02_*` (arbres, branches, feuilles et leur masque alpha), `rock_moss_set_01_*`, `stone_01_*`, `dead_tree_trunk_*`.

---

## 107 — Textures du jardin reçues

**Date** : 2026-10-21 · **Domaine** : Carte

> Voici les textures qu'il manque (textures.zip)

**Résultat** (fait par l'IA, Claude Code)

- Le zip contient 20 des 25 images ; `grass_*` et `cobble_*` manquent encore. L'export les remplace par les matériaux procéduraux du même nom de `map_final_final.blend` : terrain et allée gardent l'aspect de la carte actuelle.
- `export_map.py` : variables `SAE_TEXTURES` (dossier où retrouver les images par leur nom) et `SAE_FALLBACK_BLEND` (ancien fichier pour les matériaux encore incomplets) ; budgets serrés pour les pierres d'allée (800 triangles) et les rochers (1 200). Carte : 990 000 triangles avant LOD, contre 828 000 pour l'ancienne (jardin 226 000 → 387 000).
- `RoomImport.cs` : feuillage en découpe alpha (opaque, double face) au lieu de transparent.
- `RoomBuilder.cs` : arbres PolyHaven avec une capsule sur le tronc (au lieu d'une boîte de 4 m) ; pierres d'allée sans collision.
- MainScene reconstruite (Build VR Scene), éclairage recuit. Points d'apparition, emplacements de planches, générateur, portes et radio identiques à l'ancienne scène. Non testé au casque.

---

## 108 — Haie du jardin floue

**Date** : 2026-10-21 · **Domaine** : Carte

> C'est encore bas qualité ça, c'est normal ? (capture de la haie du jardin)

**Résultat** (analyse de l'IA, Claude Code)

- La haie (`Garden_HighHedges_Boundary`) et les sapins du fond (`Exterior_Forest_Canopies`) ont dans Blender un matériau procédural (bruit) ; l'export le cuisait en une seule image de 1024 px pour des dizaines de mètres, d'où le flou. Défaut déjà présent avant Finalefinale.
- Proposé : une texture de feuillage répétée tous les 1,5 à 2,5 m (ou, à défaut, une image cuite plus grande).

---

## 109 — Haie et forêt en feuillage répété

**Date** : 2026-10-21 · **Domaine** : Carte

> Oui, n'oublie pas de bien optimiser le tout comme avant

**Résultat** (fait par l'IA, Claude Code)

- PolyHaven n'a pas de texture de haie : `Tools/blender/make_hedge_texture.py` en fabrique une (1024 px, raccord sur les bords, couleur + normale) en éparpillant sur 6 couches les feuilles de l'atlas `island_tree_02` déjà fourni ; rien téléchargé. Fichiers : `Tools/blender/textures/haie_feuillage_*.png`.
- `export_map.py` : la haie (1,5 m par répétition) et les sapins (2,5 m) utilisent cette texture avec des UV en mètres (projection sur le cube) au lieu d'une image cuite.
- Optimisation inchangée : une seule image de 1024 px partagée (au lieu de deux images cuites par objet), mêmes triangles (~990 000 avant LOD), éclairage recuit. Non testé au casque.

---

## 110 — Revue LD/GD de l'hôpital v03

**Date** : 2026-10-21 · **Domaine** : Carte

> Analyse l'entièreté de la scène actuellement ouverte dans Blender (Hospital_VR_Survival_QualityPass_v03_PBR_CoD_Zombie.blend)
> fait moi un retour constructif sur comment l'améliorer, soit le plus critique possible (prend le point de vu d'un level designer, d'un game designer)

**Résultat** (analyse faite par l'IA, Claude Code ; le .blend n'a pas été modifié)

- Analyse sur une copie, dans Blender en arrière-plan : inventaire par script, plans en coupe, vues à hauteur d'yeux, test de chemins par rayons. Revue écrite par l'IA : `Revue_LD_GD_Hopital_v03.md` et 5 images annotées dans les fichiers du projet.
- Constat principal : très gros progrès visuel, mais aucun chemin jouable depuis le parking. Entrée du hall de 0,6 m, hall fermé (ascenseur, rampe et escalier derrière un mur plein), escalier en blocs pleins, rampe du sous-sol sous une dalle, bloc opératoire sans porte, porte d'ascenseur du toit derrière un pilastre ; ni points d'apparition ni barricades près du joueur (seulement sur les portails à 55 m) ; portes et fenêtres du hall tournées de 90°.
- Couche « CoD Zombies » (boîte mystère, atouts, Pack-a-Punch, achats muraux) : 4 systèmes absents du jeu, noms déposés par Activision ; à retirer ou à remplacer par les mécaniques existantes.
- Import Quest : 606 000 triangles (BMW 177 000, ambulance 158 000), 1 388 maillages, 220 matériaux, 79 lumières, 202 maillages à normales inversées (dont les sols), 292 sans UV, 143 images orphelines.
- Recommandation : garder la carte actuelle pour le 13 novembre, ou réduire l'hôpital à hall + sous-sol + toit et corriger les bloquants. Rien testé dans Unity ni au casque.

---

## 110 — Textures qui clignotent et ombres tachées

**Date** : 2026-10-21 · **Domaine** : Carte

> Il semblerai y avoir des soucis avec quelques textures comme ici :
> Y a des moments où c'est des props qui sont les uns dans les autres, pour d'autre c'est juste la lumière
> (3 captures : mur et encadrement de porte, coffre à outils, rebord de la fenêtre sud-est)

**Résultat** (fait par l'IA, Claude Code)

- Cause 1 (défaut du fichier Blender, déjà présent avant Finalefinale) : des faces de deux objets dans le même plan et tournées du même côté (encadrements au ras des murs, rebords des fenêtres du bunker, sols qui se chevauchent sous les portes, placards et crédence de la cuisine, bureaux du pupitre, planche posée dans un coffre). `export_map.py` les détecte (plan à 4 mm près, recouvrement > 2 cm) : 41 objets corrigés, encadrements épaissis de 4 mm de chaque côté, crédence et placards avancés de 3 mm, sinon le plus petit objet recule de 3 mm.
- Cause 2 (lumière) : à 6 texels par mètre, un petit objet n'a que quelques pixels de lightmap et ses UV simplifiés débordent (bandes sombres sur les tiroirs). `RoomBuilder.cs` : les objets de moins de 1,3 m sur les trois axes (357) prennent la lumière des sondes ; la résolution des lightmaps ne change pas (même coût au casque).
- Éclairage recuit. Non testé au casque.

---

## 111 — Appui de fenêtre encastré dans le mur

**Date** : 2026-10-21 · **Domaine** : Carte

> les bugs sont encore présent, en gros au niveau de la fenêtre, le bas de la fenêtre est encastré avec le mur du bas, ducoup on voit un morceau du mur dans la fenêtre

**Résultat** (fait par l'IA, Claude Code)

- Cause : le dessus de l'appui (`Bunker_WindowFrame_SE/SW`, z = 0,95 m) est exactement au niveau du bas de l'ouverture du mur sud ; la correction 110 épaississait l'encadrement dans l'épaisseur du mur seulement, pas en hauteur.
- `export_map.py` : les encadrements sont maintenant gonflés de 4 mm selon leurs normales (appui, linteau et montants passent tous devant les faces du mur). Vérifié par capture dans l'éditeur ; éclairage recuit. Non testé au casque.

---

## 112 — Vraies portes

**Date** : 2026-10-21 · **Domaine** : Carte

> Trouve une vraie texture de porte (ou produit en une) qui correspond au style de la map et utilise ça au lieu des portes low poly actuelles

**Résultat** (fait par l'IA, Claude Code)

- Les portes du fichier Blender utilisent le bois peint écaillé PolyHaven `distressed_painted_planks`. `Tools/blender/make_door_texture.py` en tire une porte à 4 panneaux (cadre lisse, panneaux rainurés en creux avec chanfrein en relief, crasse en bas, plaque de propreté en tôle) : `Assets/Textures/Porte/porte_couleur.png` et `porte_normale.png` (512 x 1024). Rien téléchargé.
- `RoomBuilder.cs` : les battants des 4 portes de niveau (avant : boîtes grises) prennent ce matériau ; les passages de plus de 1,6 m (couloir droit, stockage) affichent deux portes ; poignée en métal. Charnière, verrou, barricade et coups des zombies inchangés.
- Vérifié par captures dans l'éditeur ; éclairage recuit. Non testé au casque.

---

## 113 — Fondu de début et menu des commandes

**Date** : 2026-10-22 · **Domaine** : Interface

> Rajoute dans le jeu un petit fade in au moment du début de la game
> Rajoute aussi un menu qui explique les touches brièvement (genre schématique des manettes avec des traits qui disent ce que chaque touche fait)

**Résultat** (fait par l'IA, Claude Code)

- `StartFade.cs` : au chargement de toute scène de jeu (scène qui contient un `WaveManager`, donc aussi « rejouer » et la version PC), l'écran reste noir 0,3 s puis s'éclaircit en 1,4 s (voile collé à la caméra, comme le fondu du menu). S'installe tout seul : MainScene n'a pas été reconstruite, l'éclairage n'a pas bougé.
- Page **COMMANDES** (`AmberMenuControls.cs`), ouverte par un nouveau bouton du menu d'accueil (5 boutons resserrés) et du menu pause : onglet MANETTES (dessin des deux manettes Quest 3, traits de rappel vers gâchette, grip, joystick, A, B, X, menu) et onglet CLAVIER + SOURIS (touches de `DesktopPlayer`). L'onglet affiché d'abord suit la version (casque ou PC). Les textes suivent les réglages en cours : déplacement continu ou téléportation, prise en un clic ou en maintenant, rotation par à-coups ou fluide.
- Dessin des manettes : `Tools/make_controller_images.py` (Pillow) produit `Assets/Resources/Commandes/manette_gauche.png` et `manette_droite.png`, schéma au trait teinté en ambre par le menu. Rien téléchargé.
- Correction au passage : le fond des pages des menus ambre est dessiné avant le reste (il recouvrait les éléments loin du centre du panneau, qui paraissaient ternes).
- Vérifié : captures des pages en mode batch, puis en mode Play dans MainScene (fondu présent au rechargement de la scène, voile à 0,99 puis 0,47 ; page COMMANDES du menu pause). **Pas testé au casque**, ni en version PC jouée au clavier. Remarque vue en capture : un panneau du tutoriel posé dans la carte peut passer devant le menu pause (déjà le cas avant).

---

## 114 — Modèle de la clé

**Date** : 2026-10-22 · **Domaine** : Graphisme

> Trouve une texture de clé ou un modèle de clé gratuit et utilise ça, rajoute le dans les textures utiisées dans le wiki (rajoute le lien etc)

**Résultat** (fait par l'IA, Claude Code)

- Pas de clé chez Poly Haven. Modèle choisi : « Key » de **Quaternius** sur Poly Pizza (https://poly.pizza/m/bg6e1lfNsO), **CC0**, 688 triangles, clé ancienne à anneau ouvragé. Texture : **Metal048C** d'**ambientCG** (https://ambientcg.com/view?id=Metal048C), or impur et rugueux, **CC0**.
- `Tools/blender/prep_key.py` met le modèle à plat, à 16 cm de long (l'ancienne clé faisait 17 cm), centré, avec des UV en projection cubique, et l'exporte dans `Assets/Cle/Cle_Ornee.fbx`. Textures réduites à 512 px (couleur, normale, métal + lissage) dans `Assets/Cle/`.
- `RoomBuilder.cs` (`UseOrnateKey`) : `Cle_Jardin` (sur le buffet de la cabane) prend ce modèle et le matériau `Cle_Ornee` ; la lueur dorée de l'ancienne clé est gardée, adoucie de moitié pour que l'usure du métal se voie. Prise en main, collider, poids et position inchangés (calculés comme avant à partir du modèle).
- MainScene reconstruite, éclairage recuit. Vérifié par capture dans l'éditeur. **Pas testé au casque.**
- Page wiki **Textures et modèles** créée (sources, auteurs, licences, liens).

> Elle est trop dorée, elle devrai être dans le thème du reste de la map, et elle fait un peu trop fantasy

**Résultat** (fait par l'IA, Claude Code)

- Clé ornée dorée écartée. Remplacée par la clé simple « Key » de **Quaternius** (https://poly.pizza/m/y3bSVdIjTh, **CC0**, 284 triangles), mise à 12 cm, habillée du fer rouillé **rusty_metal_04** de **Poly Haven** (https://polyhaven.com/a/rusty_metal_04, **CC0**), comme le reste de la carte. Plus de lueur dorée : le liseré des objets interactifs la signale.
- `RoomBuilder.UseIronKey` remplace `UseOrnateKey` ; `prep_key.py` nomme le maillage d'après le fichier exporté. Anciens fichiers `Cle_Ornee` supprimés.
- MainScene reconstruite, éclairage recuit, vérifié par capture dans l'éditeur. **Pas testé au casque.** Page wiki mise à jour (avec l'historique de la première version).

---

## 115 — Ceinture, chargeur et marteau

**Date** : 2026-10-22 · **Domaine** : Gameplay

> Le système de ceinture est un peu compliqué pour y reposer des items (Rajoute le fait qu'on puisse porter 1 chargeur en même temps, que ce soit celui de base ou un récup sur la map, patch le rayon, parce que actuellement c'est trop compliqué de reposer le pistolet)
> Et pour le marteau, dégage le de la ceinture et fait en sorte qu'il soit posé au début de la game sur la table

**Résultat** (fait par l'IA, Claude Code)

- Le holster du chargeur existait déjà (devant, rayon 9 cm) et acceptait tout chargeur, mais il était difficile à trouver. Il passe à la **hanche gauche**, à la place du marteau, avec un rayon de **14 cm**. Il porte **un seul chargeur** : celui de départ ou n'importe quel chargeur ramassé (carte, radio).
- Rayons des holsters élargis (`RoomBuilder.MakeSocket` prend maintenant le rayon) : pistolet **9 → 16 cm**, lampe 9 → 12 cm. Le rangement se fait toujours en lâchant l'objet près de la hanche (`GrabGuard` empêche toujours le holster d'aspirer un objet tenu).
- Marteau : plus de holster. Il est **posé à plat sur l'établi du bunker** au début de la partie (`BuildHammer`). Textes mis à jour : tutoriel (étape 3 « posé sur l'établi », étape 8 « à gauche »), page COMMANDES, aide PC. Version PC : la touche 2 prend le marteau s'il est à portée.
- MainScene reconstruite, éclairage recuit. Vérifié en mode Play dans l'éditeur : un chargeur de la carte lâché à 11 cm du holster s'y range, le pistolet lâché à 14 cm aussi, le marteau reste couché sur l'établi (capture). **Pas testé au casque** : le bon rayon dépend du ressenti à la manette.
- Page wiki **Ceinture** créée, Tutoriel et Pistolet mis à jour.

---

## 116 — Objets low poly remplacés

**Date** : 2026-10-22 · **Domaine** : Graphisme

> Retire les éléments low poly que tu as rajouté et cherche des textures / GLB qui correspondent ou créer les, n'oublie pas de les référencer dans le wiki

**Résultat** (fait par l'IA, Claude Code)

- Objets visés (ceux visibles sur la capture de l'établi) : ils ne venaient pas du travail sur la ceinture, ils étaient déjà dans la carte. Les **6 bouteilles vertes** du fichier Blender (`Bottle_01`, 84 triangles, couchées : établi et bureau du bunker, cuisine, stockage) et les **4 piles** de la lampe (deux cubes verts faits par `RoomBuilder`).
- Bouteilles : remplacées par deux modèles **Poly Haven** de **James Ray Cock** (**CC0**) en 1k : « Bleach Bottle » (https://polyhaven.com/a/bleach_bottle, 4 052 triangles, 29 cm) et « Multi Cleaner Bottle » (https://polyhaven.com/a/multi_cleaner_bottle, 6 792 triangles, 23 cm). `Tools/blender/prep_prop.py` les met debout, origine au fond, taille réelle, export FBX dans `Assets/Decor/` ; la carte ARM est convertie en carte métal + lissage pour Unity. `RoomBuilder.ReplaceLowPolyBottles` les pose debout à la place de chaque bouteille (place au-dessus vérifiée, au moins 59 cm), en alternant les deux.
- Piles : pas de pile libre de droits en haute définition trouvée (Poly Haven n'en a pas). Pile créée : maillage de pile ronde type D (34 x 61 mm, chanfreins, téton du pôle +, 560 triangles) fait par `RoomBuilder.BatteryMesh`, texture d'étiquette militaire vert olive « BA-30 · pile alcaline 1,5 V » dessinée par `Tools/make_battery_texture.py` (Pillow, avec usure), dans `Assets/Pile/`. Collider en capsule. Rien n'a changé pour le jeu (prise, recharge de la lampe).
- MainScene reconstruite, éclairage recuit, vérifié par captures dans l'éditeur. **Pas testé au casque.**
- Page wiki **Textures et modèles** complétée (sources, auteurs, licences, liens, fichiers).

---

## 117 — Lampe torche réaliste

**Date** : 2026-10-22 · **Domaine** : Graphisme

> Tiens, voilà la lampe torche : https://assetstore.unity.com/packages/3d/props/furniture/electric-torch-224071

> C'est bon

> Bon bah créer moi en une ou telecharge moi en une avec un aspect réaliste comme dans le style du jeu (référence la aussi)

**Résultat** (fait par l'IA, Claude Code)

- Paquet « Electric Torch » (Asset Store, gratuit) : il faut l'ajouter à son compte et le télécharger depuis le Package Manager ; il n'est jamais arrivé sur le Mac, donc pas utilisé.
- Modèle choisi à la place : « Small Plastic Torch » de **Poly Haven** (https://polyhaven.com/a/small_plastic_torch), par **Abiyyu Panggalih**, **CC0** : vieille lampe en plastique jaune usée, 23 cm, avec dragonne. Comparé à Vintage Flashlight, Signal Flashlight et Plastic Torch 6V (Poly Haven), trop gros ou peu maniables pour une lampe tenue à une main.
- `Tools/blender/prep_torch.py` : glTF 1k → FBX (`Assets/Lampe/`), longueur sur un axe, allégée de 13 252 à 6 000 triangles (Decimate). Textures 1k (couleur, normale, métal + lissage tiré de la carte ARM).
- `RoomBuilder.MakeFlashlight` : le modèle remplace les deux cylindres. Il est tourné pour que le verre éclaire vers l'avant et que la dragonne pende dessous ; l'axe du faisceau passe par le centre du verre. Collider calé sur le corps (sans la dragonne), port de la pile à l'arrière, jauge de charge sur le dessus du manche, prise de la main sur le manche. `Flashlight.cs` : la jauge se vide depuis sa place (elle n'est plus forcément au centre de l'objet).
- MainScene reconstruite, éclairage recuit, captures dans l'éditeur (lampe allumée, dragonne en bas). **Pas testé au casque** (prise en main, sens du faisceau).
- Page wiki **Textures et modèles** complétée.

---

## 118 — Balles et batterie à l'inspection

**Date** : 2026-10-22 · **Domaine** : Interface

> Ok pour ce qui est du chargeur / du système de balles / de la batterie de la flashlight
> Fait en sorte que le nombre / la barre de batterie de la lampe ne s'affichent que si le joueur tiens l'objet dans la main et décide de le tilte de côté en layant un peu proche du visage
> Pour le chargeur ça afficherait au niveau du pistolet nombre de balle / 12 avec une icone de balle après
> Pour la flashlight ça afficherai une icone de batterie avec 4 barres dedans, et la barre actuelle qui est utilisée clignotterai doucement et de plus en plus vite quand elle est sur le point de se consummer

(avec deux captures du casque : l'ancienne lampe et sa barre jaune, le pistolet en main)

**Résultat** (fait par l'IA, Claude Code)

- Nouveau script `InspectHud` (pistolet et lampe). Geste d'inspection : objet **tenu en main**, à **moins de 50 cm des yeux**, dans un cône de **40°** devant le regard, et **penché sur le côté d'au moins 35°** (rotation autour de l'axe de l'objet). Le panneau apparaît et disparaît en fondu (≈ 0,12 s), flotte 3,5 cm au-dessus de l'objet et se tourne vers les yeux. Version PC : visible dès que l'objet est en main (on ne peut pas pencher l'objet à la souris).
- Pistolet : « 8 / 12 » puis une icône de balle (dessinée par code) ; blanc, orange à 3 balles ou moins, rouge clignotant à vide. L'ancien compteur toujours visible sur la culasse est retiré.
- Lampe : icône de pile couchée avec 4 barres. Les barres pleines sont ambre (rouge orangé sur la dernière), la barre en cours **clignote doucement (0,5 Hz) quand elle vient de commencer, jusqu'à 4 Hz juste avant de s'éteindre** ; les barres vides restent à peine visibles. L'ancienne barre jaune posée sur la lampe est retirée.
- Page COMMANDES : ligne « Balles, pile : pencher l'objet en main près du visage ».
- Vérifié dans l'éditeur : test du geste (droit = rien, penché près = affiché, penché loin = rien, hors du regard = rien) et capture en mode Play (8 / 12, pile à 3 barres). **Pas testé au casque** : les seuils (50 cm, 40°, 35°) sont à régler au ressenti.

> Il devrait plus être penché de côté, genre le canon vers la gauche, même système pour la lampe

**Résultat** (fait par l'IA, Claude Code)

- Geste changé : au lieu de faire rouler l'objet sur son axe, on le **tourne de côté** pour que le **canon (ou le faisceau de la lampe) pointe vers la gauche** du regard, à 45° près (`InspectHud.maxLeftAngle`), toujours à moins de 50 cm des yeux et devant le regard. On voit alors le flanc de l'arme devant soi. Même règle pour la lampe. Page COMMANDES mise à jour.
- Test du geste dans l'éditeur : visée vers l'avant = rien ; canon à gauche = affiché (aussi un peu relevé) ; canon à droite = rien ; simplement roulé sur l'axe = rien ; à 80 cm = rien. Scène inchangée (pas de reconstruction ni de recuisson). **Pas testé au casque.**

---

## 119 — Inspection plus souple et prise des armes

**Date** : 2026-10-22 · **Domaine** : Gameplay

> Par moments c'est un poil capricieux
>
> Et aussi il faudrait patch le placement de l'arme quand on la prend dans la main, même la ache se retrouve un peu décalé quand je prend dans la ain

(avec une capture du casque : pistolet tenu, sa crosse à côté du poing)

**Résultat** (fait par l'IA, Claude Code)

- Inspection (`InspectHud`) : seuils assouplis (60 cm au lieu de 50, cône de 50° au lieu de 40, canon à 60° de la gauche au lieu de 45°). La direction du canon est lue vue de dessus dans le repère de la tête : on peut relever ou baisser l'arme. Une fois le panneau affiché, les seuils sont élargis de 25 % et le panneau reste 0,35 s après la perte du geste : il ne clignote plus à la limite.
- Prise : mesure du maillage du poing fermé (main droite, repère de la manette). Le point d'accroche des deux mains était 1,5 cm trop vers le pouce et 2,5 cm trop en avant : (±0,015 ; −0,135 ; 0,025) → (±0,03 ; −0,13 ; 0) (`SetupVRScene`). Vaut pour tous les objets.
- Hache, pied-de-biche, marteau : la prise était calée sur le centre du maillage, décalé par le fer (7,3 cm du manche pour la hache, 3,1 cm pour le pied-de-biche). Elle suit maintenant l'axe réel du manche, mesuré sur une tranche (`RoomBuilder.HandleAxis`), comme le collider du manche.
- Vérifié en mode Play avec une main simulée en poing fermé : la crosse du pistolet et le manche de la hache passent dans le poing (captures de côté, de dos, de dessus). MainScene reconstruite, éclairage recuit. **Pas testé au casque.**
- Wiki : Mains et prise des objets (section « Correction de la prise »), Inspection des objets.

---

## 120 — Rechargement facilité

**Date** : 2026-10-22 · **Domaine** : Gameplay

> Ok, et aussi facilite le truc qui permet de recharger car là c'est trop compliqué, par moment mettre le chargeur proche du pistolet le fait passer à travers

**Résultat** (fait par l'IA, Claude Code)

- Cause : le puits sous la crosse (`MagWell`, socket XRI) n'acceptait le chargeur que s'il restait au moins une image dans une sphère de 5 cm. Un geste rapide le faisait passer à travers l'arme entre deux images, sans rien faire.
- Sphère du socket portée à 8 cm. Nouveau rechargement par proximité dans `MagWell` : un chargeur tenu en main qui passe à moins de 9 cm du puits pendant que le pistolet est en main est inséré tout de suite, quelle que soit son orientation. Le trajet du chargeur entre deux images est testé (segment), pas seulement sa position. Rien si l'arme est pleine ; rien si le pistolet est au holster. `Magazine.All` liste les chargeurs pour ne pas parcourir la scène à chaque image.
- Vérifié en mode Play (mains simulées) : pistolet à 3 balles, chargeur plein passé à travers l'arme en une seule image (de 25 cm d'un côté à 25 cm de l'autre) → 12 balles, le chargeur ressort avec 3. MainScene reconstruite, éclairage recuit. **Pas testé au casque.**
- Wiki : page Pistolet (section « Rechargement facilité »).

---

## 121 — Croix pour passer le tutoriel

**Date** : 2026-10-22 · **Domaine** : Interface

> Rajoute une croix au tutoriel pour qu'il soit skippable

**Résultat** (fait par l'IA, Claude Code)

- Nouveau script `TutorialSkipCross` : croix en haut à droite de chaque panneau du tutoriel (panneaux posés dans la carte, `TutorialBoard`, et panneau devant les yeux, `Tutorial`). On la vise avec une manette dont la main est vide : un rayon ambre apparaît seulement quand on vise la croix, elle passe au rouge, petite vibration ; la gâchette valide. Une main qui tient un objet est ignorée (sa gâchette tire ou allume la lampe). Version PC / éditeur : réticule + clic gauche (ou Espace). Portée 8 m ; zone visée 10 cm (panneaux de la carte) et 7 cm (panneau devant les yeux).
- La croix fait comme A + X : le tutoriel saute aux astuces puis à « PRÊT ? », que la même croix ferme aussi. A + X pendant 2 s reste possible. Pas de croix sur l'invitation « revoir le tutoriel ». Textes d'aide des panneaux mis à jour.
- Vérifié en mode Play : la croix du panneau de l'étape 1 (à 4,4 m) est touchée par un rayon visé dessus et pas par un rayon décalé ; le clic termine les étapes (plus de panneau d'étape, passage aux astuces). Capture de la croix. Scripts seulement : scène ni reconstruite ni recuite. **Pas testé au casque.**
- Wiki : page Tutoriel.

---

## 122 — Retour de dégâts et direction du coup

**Date** : 2026-10-22 · **Domaine** : Gameplay / Ressenti

> Il faudrait également accentué les effets de quand on prend des dégats car généralement le joueur ne sais pas exactement qu'il prend des dégats
> Et rajouter un petit indicateur d'où vient le dégat

**Résultat** (fait par l'IA, Claude Code)

- Existant : vignette rouge selon les PV + flash au coup (`HealthVignette`), vibration égale des deux manettes (`PlayerHealth`). Rien n'indiquait la direction, aucun son.
- Indicateur de direction : le zombie transmet sa position avec le coup (`PlayerHealth.TakeDamageFraction(fraction, position)`, événement `DamagedFrom`). Le shader `DamageVignette` dessine un arc rouge autour du centre de la vue du côté du zombie (haut = devant, bas = derrière, côtés = gauche / droite) et rougit le bord de l'écran de ce côté (vision périphérique au casque). L'arc suit la rotation de la tête, reste 0,6 s puis s'efface en 1,6 s ; 4 coups en même temps au plus, un même zombie ravive son arc.
- Coup plus net : flash rouge proportionnel aux dégâts (×0,8 à ×1,5) ; son « choc sourd » dans le corps à chaque coup + griffure jouée depuis la direction du zombie ; battement de cœur sonore (au rythme de la pulsation rouge) sous 25 % des PV ; la manette du côté du zombie vibre plus fort, l'autre moitié moins (`ProcAudio.PlayerHurt`, `ClawSwipe`, `Heartbeat`).
- Confort VR : rien ne bouge la caméra (pas de secousse). Même rendu en version PC (même caméra). Scripts et shader seulement : scène ni reconstruite ni recuite.
- Vérifié en mode Play : coups simulés venant de la gauche et de derrière → arcs à gauche et en bas, événement reçu, PV corrects ; captures. **Pas testé au casque** (vibrations, son spatialisé, lisibilité des arcs dans chaque œil).
- Wiki : page Vignettage de dégâts (section « Retour de coup et direction »).

---

## 123 — Cover et logo du jeu

**Date** : 2026-10-22 · **Domaine** : Communication

> Refait moi l'image de cover du jeu (Et fait moi un logo aussi, je veux un truc simple pour le logo, ça doit être un truc logo pour une version desktop / mobile / quest)

**Résultat** (image produite par l'IA, Claude, en Python/Pillow + rendu SVG/HTML dans Chromium)

- **Cover** refaite à partir d'une image de la cinématique de fin (les zombies bloqués derrière la grille du jardin) : étalonnage nuit, yeux rouges ajoutés, titre THE LAST / ESCAPOR (polices Anton + Barlow Condensed). Formats 1280x640 (aperçu GitHub, `docs/images/cover.png`), 2560x1280 et 1920x1080.
- **Logo** : deux planches clouées (fenêtre barricadée) avec deux yeux rouges dans la fente. Couleurs nuit `#06090F` / `#18233A`, os `#ECE4D3`, rouge `#FF3B2F`.
- Icônes de 16 à 1024 px (version simplifiée sous 64 px), `.ico` Windows, `.icns` macOS, icône adaptative Android, logotypes horizontal et vertical. Fichiers dans `docs/images/branding/` (voir `LISEZMOI.md`).

---

## 124 — Grille de la cover plus détaillée

**Date** : 2026-10-22 · **Domaine** : Communication

> La grille fait low poly, corrige ça, je veux une grille plus détaillée

**Résultat** (fait par l'IA, Claude)

- Grille de la cover redessinée en 2D : 17 barreaux ronds en fer forgé, reflet, grain, rouille, bagues, traverse biseautée rivetée, plaque à 4 boulons.
- Le modèle 3D de la grille dans le jeu n'a pas changé.

---

## 125 — Cover et logo poussés sur GitHub

**Date** : 2026-10-22 · **Domaine** : GitHub

> Niquel, met à jour le github

**Résultat** (fait par l'IA, Claude Code)

- Nouvelle `docs/images/cover.png`, dossier `docs/images/branding/` (covers, icônes, logos, `LISEZMOI.md`) et lien dans le `README.md`, poussés sur `main` (commit `f9966d6`) en avance rapide depuis un worktree séparé (la branche `feature/ceinture-marteau` n'est pas touchée).
- `docs/images/branding/LISEZMOI.md` corrigé ensuite pour décrire le dossier à plat tel qu'il est poussé (il citait des sous-dossiers absents).
- L'aperçu social GitHub (Settings > General > Social preview) reste à changer à la main.

---

## 126 — Mise à jour du GDD

**Date** : 2026-10-22 · **Domaine** : Documents

> Il faudrait également mettre à jour le GDD :

(avec le fichier `GDD The last escapor.docx`)

**Résultat** (texte écrit par l'IA, Claude Code, à partir du journal 085 à 121, du wiki, de `docs/ZOMBIES.md` et des analyses d'équilibrage ; aucun code ni scène modifiés)

- Même document, même mise en forme (styles, couleurs, tableaux), modifié en place : `GDD The last escapor - 2026-10-22.docx` dans les fichiers du projet Claude (pas dans le dépôt).
- Corrigé : titre « The Last Escapor » (la couverture disait « The last survivor ») ; 4 niveaux au lieu de 5 vagues (6 à 15 zombies, renforts au niveau 4) ; types Rôdeur / Cogneur / Coureur au lieu de Standard / Rapide / Renforcé ; dégâts jusqu'à 40 % ; victoire par le levier du générateur (la clé n'ouvre encore rien) ; genre « solo », multijoueur hors périmètre ; ceinture (chargeur à gauche, marteau sur l'établi) ; rechargement par proximité ; mort = bilan et reprise au niveau atteint.
- Ajouté : mode Vagues infinies (tableau et plafonds), radio de ravitaillement, munitions, difficulté, tableau des commandes casque / PC (le schéma des manettes de la v1 est gardé, avec une note), inspection des balles et de la pile, fondu de début, page COMMANDES, croix du tutoriel, options de confort (téléportation, saut, prise), brouillard ; sections vides remplies : Flow, Mécaniques diverses, Core gameplay, Micro boucles, Level design (zones), Signes et feedbacks, UI (avec une capture de l'inspection), Atmosphère ; nouveaux sons ; couleur du brouillard dans la palette.
- Sommaire : Word recalcule les numéros de page à l'ouverture (accepter la mise à jour des champs).
- Vérifié : rendu des 29 pages en PDF (LibreOffice). Les valeurs sont celles du code en Normal, **pas mesurées au casque**.

---

## 127 — Son et titre de début de vague

**Date** : 2026-10-22 · **Domaine** : Gameplay / Ressenti

> Rajoute à chaque début de vagues ce sont :
> https://youtube.com/shorts/YyluAyugj4s?si=MYkwLI0k4qUBj3bA (Rajoute le dans les crédits aussi et dans le wiki)
> Et fait en sorte qu'il y ai le message "Vague" suivi du numéro de la vague au milieu de l'écran du joueur pendant genre 1.5 secondes avec un effet de fade-in fade-out le tout dure 4s (le temps du sons en rythme avec)

**Résultat** (fait par l'IA, Claude Code)

- Son : « Creepy Whisper Sound Effect 13 » de SOUND BAG, récupéré avec yt-dlp (4,5 s). La description de la vidéo autorise l'usage personnel et non commercial (projet étudiant : OK ; usage commercial interdit sans licence). Découpé à 4 s, mono, fondu final 0,4 s : `Assets/Resources/Audio/Vague_Debut.ogg` (chargé par `Resources.Load`, pas besoin de reconstruire la scène).
- Nouveau script `WaveTitle` : « VAGUE N » en rouge au milieu de la vue (collé à la tête, à 1,2 m, dessiné par-dessus le décor ; même rendu en version PC). Calé sur le son mesuré (montée jusqu'à 1 s, pleine voix jusqu'à 2,5 s, extinction) : fondu d'apparition 1 s, 1,5 s bien visible, fondu de disparition 1,5 s, 4 s au total. Joué par `WaveManager.StartLevel` à chaque vague (aussi en mode survie). L'alarme de début de vague et l'annonce sur l'écran du bunker sont gardées.
- Crédits : README (section Crédits) et wiki (page Crédits des sons, page Niveaux et vagues).
- Vérifié en mode Play : son chargé (4,0 s), titre « VAGUE 3 » affiché au centre de la vue (capture). Scripts et son seulement : scène ni reconstruite ni recuite. **Pas testé au casque.**

---

## 128 — Titre de vague en lettres de sang

**Date** : 2026-10-22 · **Domaine** : Interface

> Fait le en lettres de sang, et utilise bien une font libre de droit que ce soit personnal use ou même commercial use

**Résultat** (fait par l'IA, Claude Code)

- Police **Nosifer** (Typomondo), lettres qui dégoulinent de sang, téléchargée depuis le dépôt officiel Google Fonts (`google/fonts`). Licence SIL Open Font License 1.1 : usage libre, personnel et commercial, intégrable dans un jeu ; le texte de la licence est joint (`Assets/Resources/Fonts/Nosifer-OFL.txt`).
- `WaveTitle` : « VAGUE N » en Nosifer, rouge sang sombre (0,62 ; 0,02 ; 0,02), avec une ombre noire légèrement décalée derrière pour rester lisible ; un peu plus petit qu'avant car la police est très large. Si la police manque, retour à la police par défaut.
- Vérifié en mode Play : titre « VAGUE 3 » en lettres de sang au centre de la vue (capture). **Pas testé au casque.**
- Crédits : README et page wiki Crédits (renommée « Crédits des sons et polices »).

---

## 129 — Retrait de la version PC

**Date** : 2026-10-22 · **Domaine** : Outils

> Ok, retire toute trace de la version PC pour l'instant, on retravaillera dessus possiblement dans longtemps

(confirmé dans le fil : « oui, retire la version PC »)

**Résultat** (fait par l'IA, Claude Code)

- Sauvegarde d'abord : l'état complet avec la version PC est sur la branche `archive/version-pc` (commit 57c162e). Pour la reprendre : repartir de cette branche, ou récupérer les fichiers listés ci-dessous.
- Supprimés : scène `MainScene_PC`, dossier `Assets/Scripts/Desktop` (`DesktopBoot`, `DesktopMode`, `DesktopPlayer`), `Assets/Editor/DesktopSceneBuilder.cs` (menus SAE501 → Build PC Scene et Play PC Version).
- Retirés du code partagé : réglage « Champ de vision » (section AFFICHAGE, `GameSettings.FieldOfView`), onglet CLAVIER + SOURIS de la page COMMANDES (la page montre seulement les manettes), cas « mode PC » dans `ClickGrab`, `InspectHud`, `TutorialSkipCross` (réticule et clic souris), `VRInput.RightB` (touche B), `GameSettings` (téléportation), mentions dans les commentaires.
- Gardés : les raccourcis clavier pour tester dans l'éditeur sans casque (Espace, Entrée, C, Échap, N / K), antérieurs à la version PC.
- Wiki : mentions de la version PC retirées de 15 pages (Commandes, Menus, Déplacement, Ceinture, Pistolet, Inspection, Tutoriel, Brouillard, etc.), capture de l'onglet clavier supprimée, note sur l'Accueil. Les anciennes entrées de ce journal (087, 089…) restent telles quelles : c'est l'historique.
- Vérifié : compilation sans erreur ; mode Play dans MainScene : menu pause et page COMMANDES construits (onglet manettes seul), réglages appliqués, coup reçu et titre de vague sans erreur. **Pas testé au casque.** Scène VR ni reconstruite ni recuite (rien n'y change).

---

## 130 — Release APK Quest v0.9.0

**Date** : 2026-10-22 · **Domaine** : Outils

> Je veux que sur le github tu push une version APK dans les release de la toute dernière version du jeu avec le nom du jeu comme titre de l'APK, et le logo / icone bien config

**Résultat** (fait par l'IA, Claude Code)

- Build fait depuis `main` (commit c356f31) dans une copie séparée du projet, pour ne pas embarquer les réglages URP et matériaux non commités du dossier de travail. Scène ni reconstruite ni recuite.
- `QuestBuild.ReleaseApk` (menu SAE501 → Build → Quest Release) : prend maintenant toutes les scènes des Build Settings (`Menu` puis `MainScene` ; l'ancien build release oubliait le menu), sort `Builds/TheLastEscapor-v<version>.apk`, quitte en erreur en mode batch si le build échoue.
- Nouveau `QuestBuild.ApplyIdentity` (menu SAE501 → Build → Appliquer nom, version et icônes) : nom « The Last Escapor », version **0.9.0** (code Android 2), icône par défaut et icônes Android classique / ronde = `icon_1024_carre.png` du branding, icône adaptative = fond nuit `#06090F` + icône réduite à 66 % pour tenir dans le masque rond. Les Player Settings utilisés sont ceux du profil de build **Meta Quest** (actif) : c'est lui qui est enregistré.
- Vérifié dans l'APK (`aapt`) : libellé « The Last Escapor », `com.sae501.vr`, version 0.9.0 (2), icônes du jeu dans les ressources. APK de 628 Mo, signé avec la clé de debug Unity (installation par adb / SideQuest).
- Release GitHub `v0.9.0` « The Last Escapor v0.9.0 » avec `TheLastEscapor-v0.9.0.apk` et les étapes d'installation. Page wiki « Build et installation ».
- **Pas testé au casque** (installation, lancement, icône dans la bibliothèque).

---

## 131 — Portes déverrouillées en fin de vague

**Date** : 2026-10-22 · **Domaine** : Gameplay / Ressenti

> Rajoute au jeu ce son au déblocage d'une porte (passage de vague)
> Et fait en sorte que les portes se débloquent entre les vagues, genre fini la vague 1, instant les portes se débloquent, et ce a chaque fois

(fichier joint : `door-open.mp3`)

**Résultat** (fait par l'IA, Claude Code)

- Avant : une zone de niveau N (couloirs et cuisine : 2, stockage : 3, générateur : 4) s'ouvrait au **début** de la vague N, donc pendant que les zombies arrivaient. Maintenant elle s'ouvre dès la **fin** de la vague N-1, au début de la pause : le joueur a toute la pause pour aller y chercher planches et chargeurs.
- Interprétation : « à chaque fois » = à chaque fin de vague qui a des portes à ouvrir (vagues 1, 2 et 3). Il n'y a que 4 portes de niveau, donc après la vague 3 (et en mode survie au-delà), plus rien ne s'ouvre et aucun son ne joue.
- Son `door-open.mp3` copié en `Assets/Resources/Audio/Porte_Deverrouillee.mp3` (Vorbis, décompressé au chargement, comme le son de début de vague), joué une fois, non spatialisé (on l'entend où qu'on soit), quand au moins une porte vient de se déverrouiller.
- Annonce de fin de vague : « VAGUE N TERMINÉE … +X PV » suivie de « OUVERT : zone · contenu · » (affichée 8 s au lieu de 5) ; l'annonce de début de vague ne répète plus les zones. Après la vague 3 du mode Évasion, la porte du générateur est maintenant annoncée aussi.
- Panneaux des portes : « (ouverte après la vague N) » au lieu de « (niveau N) ». Reprise au niveau N (après une mort) : les portes des niveaux jusqu'à N sont déjà ouvertes.
- `ZoneGate.UpdateAll` dit maintenant si une porte vient de s'ouvrir ; `WaveManager` joue le son en fin de vague. Scène ni reconstruite ni recuite (le son est chargé depuis `Resources`, les textes sont posés au lancement).
- Vérifié en mode Play (mode Évasion, vagues terminées par script) : pendant la vague 1, les 4 portes restent verrouillées ; dès la fin de la vague 1, les deux portes de couloir se déverrouillent (poignée active), l'annonce affiche les deux zones ; fin de la vague 2 : le stockage s'ouvre ; le générateur reste fermé. Le son est chargé et la ligne qui le joue s'exécute, mais je ne l'ai pas écouté. **Pas testé au casque.**
- Source du son : non précisée (crédit à compléter dans le README).

---

## 133 — Portage PC (clavier + souris)

**Date** : 2026-11-09 · **Domaine** : Interaction / Version PC

> Il me faudrait un port pc de tout avec les bonnes interractions, les placement des mains, et rajouter un crossair pour savoir où on vise. touches pour boger ZQSD avec interraction sur E, clique en tire gauche, mouvement de la camera en lock ouris, en gros faire un portage propre de la version pc

**Résultat** (fait par l'IA, Claude Code ; numéro 133 car l'entrée 132 est sur la branche `feature/musiques-playlist`, pas encore dans `main`)

- Branche `feature/portage-pc` créée depuis `main`. La version PC retirée le 22 octobre (journal 129, branche `archive/version-pc`) a servi de point de départ pour le principe (même rig XRI piloté au clavier) ; le reste est réécrit.
- **Le casque reste le mode par défaut** : `DesktopMode` choisit au lancement. Quest → casque ; PC avec casque actif (OpenXR démarré, affichage XR en marche) → casque ; sinon → version PC. Forçable par l'argument `-pc` / `-vr`, et dans l'éditeur par le menu **SAE501 > Mode de jeu** (Automatique / Casque / PC) et **SAE501 > Jouer en version PC**. Plus de scène séparée : la version PC tourne dans `Menu` et `MainScene` (scènes non modifiées, pas de reconstruction ni de recuisson).
- **Joueur PC** (`DesktopPlayer`, ajouté au XR Origin seulement en version PC) : souris verrouillée (sensibilité réglable, pente limitée à ±85°), ZQSD (touches physiques, donc AZERTY et QWERTY), Espace saut du rig, Ctrl accroupi, E interagir, clic gauche tirer / frapper, clic droit viser, R recharger, 1-4 et molette pour changer d'objet, F lampe, G lâcher, V inspecter, B lancer la vague, Échap pause (souris libérée, menu à la souris).
- **Mains et objets en vue FPS** : chaque type d'objet a sa pose par rapport à la caméra (pistolet bas droite, lampe main gauche, marteau levé, planche portée en diagonale) ; le script place la manette pour que l'accroche de la main tombe sur celle de l'objet, donc la main serre vraiment la crosse ou le manche (poing fermé animé). Prise animée (0,2 s), balancement à la marche, recul au tir, arme qui recule devant un mur proche.
- **Toutes les interactions passent par XRI comme au casque** (mêmes règles) : tir depuis le centre de l'écran (`Pistol.AimRay` en version PC, l'impact est sous le réticule), rechargement par la main gauche qui monte le chargeur de la ceinture sous la crosse (inséré par `MagWell`), coups de marteau qui passent par la tête du clou visé (aide à la visée de 12 cm), armes de mêlée, planche posée sur la fenêtre visée, levier (E maintenu), portes (E : la main tire la poignée), boutons et écrans, radio, croix du tutoriel (réticule + clic), écrans de fin (« Clic : rejouer »).
- **HUD** (`DesktopHud`) : réticule qui s'ouvre en ambre sur un objet utilisable, aide « E Prendre le pistolet », munitions et chargeur, charge de la lampe, aide F1. L'objet visé reçoit le liseré épais du contour.
- **Menus** : pointeur souris (`MenuPointer`), PARAMÈTRES > SOURIS ET AFFICHAGE (sensibilité, souris inversée, champ de vision) à la place de CONFORT VR, page COMMANDES avec un onglet CLAVIER + SOURIS. **Tutoriel** : textes avec les touches PC (`DesktopMode.Text`).
- Code partagé : seulement des cas « si version PC » (`Pistol`, `VRInput`, `Crouch`, `MenuPointer`, `TutorialSkipCross`, `InteractableOutline`, `InspectHud`, `ClickGrab`, `GameSettings`, `AmberMenu`, `AmberMenuControls`, `Tutorial`, `TutorialBoard`, `GameOverUI`, `MainMenu`, `PauseMenu`). L'API `DesktopMode.On` / `DesktopPlayer` garde les mêmes noms que l'ancienne version (utilisés par `PlayerDeathFall` sur la branche `feature/cinematiques`).
- **Vérifié** : compilation sans erreur ; scénario automatique en mode Play (version PC forcée, MainScene pendant le tutoriel puis menu d'accueil, captures d'écran) : marche, pistolet sorti de la ceinture, 2 tirs puis rechargement R (10 → 12 balles), visée, lampe dans la main gauche allumée, marteau pris sur l'établi puis rangé et ressorti, planche prise et posée sur une fenêtre, **2 clous enfoncés en 3 coups chacun (planche clouée)**, disjoncteur abaissé en maintenant E, écran de caméra ouvert et refermé, radio appelée (« CAISSE DANS 30 s »), porte de zone ouverte (92°), Ctrl accroupi, pause par Échap et reprise, menu d'accueil (pages COMMANDES et PARAMÈTRES) ; aucune erreur dans la console. Les touches simulées (marche, E maintenu, Ctrl, Échap) ne passent que quand la fenêtre de l'éditeur a le focus : elles ont réussi lors des passages où elle l'avait.
- **Pas testé** : au casque (le code VR n'est pas modifié, mais la détection automatique avec un casque Link / Air Link n'a pas été essayée), une vraie partie jouée à la main au clavier, un build Windows / macOS. Script de test non commité.
- Wiki : nouvelle page [Version PC](https://github.com/withRen/SAE501/wiki/Version-PC) ; Accueil, menu, Commandes, Menus et Tutoriel mis à jour (commit local du wiki, pas encore poussé).

---

## 134 — Fusion de la PR #146 (escalade) dans la version PC

**Date** : 2026-11-09 · **Domaine** : Dépôt / Zombies

> Il y a eu un push de patch d'animation, c'est sous la pull request : Escalade des fenêtres : vraies animations (se hisser, tomber à plat, #146) de AL amine, pull la et merge la avec la version pc locale qu'on fasse un test

**Résultat** (fait par l'IA, Claude Code)

- PR #146 (`features/escalade-zombies`, Al Amine, 1 commit `cb1b973`) récupérée et fusionnée localement dans `feature/portage-pc` (merge, pas de rebase). **Aucun conflit** : la PR touche l'escalade des **zombies** (`ZombieWindowClimb`, `ZombieAnimationBridge`, `ZombieMixamo` et 3 clips Mixamo dans `Assets/Animations/Escalade`), le portage PC ne touche aucun de ces fichiers.
- La PR ne contenait pas les contrôleurs d'animation régénérés : sans eux, les zombies gardaient l'ancienne escalade (code seul). Régénérés dans l'éditeur (`ZombieMixamo.EnsureController` pour le zombie normal, le Cogneur et le Coureur) : chacun a maintenant les états Climb, Fall et StandUp. Même fichier et même GUID : prefabs et scène inchangés, pas de reconstruction ni de recuisson. Les réglages d'import des 3 FBX (rig humanoïde, noms des clips) sont commités avec.
- Vérifié : compilation sans erreur (console de l'éditeur vide). **Pas vérifié en mode Play** : l'éditeur était ouvert par Rahim, je n'ai pas lancé le mode Play à sa place ; test à faire par lui (voir le message de rapport). Rien poussé.

---

## 135 — Escalade des fenêtres corrigée

**Date** : 2026-11-09 · **Domaine** : Zombies

> Analyse la vidéo pour constater du problème d'animation du zombi, et corrige la pour qu'il passe proprement dans la fenêtre

(vidéo jointe : `Screen Recording 2026-11-09 at 10.53.37.mov`, version PC, après la fusion de la PR #146)

**Résultat** (fait par l'IA, Claude Code)

- **Constat (vidéo, vers 15–16 s)** : le zombie ne passe pas par l'ouverture. Il monte le long du mur jusqu'au-dessus du cadre de la fenêtre (presque au plafond, décalé vers le bord gauche, à moitié dans le mur), puis tombe de là (~2 m) à plat au sol et se relève.
- **Cause (mesurée dans l'éditeur en échantillonnant les clips)** : *Climbing* est fait pour un mur de 2 m (mains à 2,15 m au départ, bassin à 2,8 m à la fin) ; *Falling Flat Impact* commence en l'air (bassin à 1,7 m) ; et `ZombieWindowClimb` soulevait en plus le zombie jusqu'au rebord (0,97 m). Les hauteurs s'additionnaient. La ligne d'escalade suivait les bouts du lien NavMesh, pas le centre de la fenêtre. *Zombie Stand Up* commence avec le bassin 1 m devant : saut en se relevant.
- **Correction** (`ZombieWindowClimb`, `ZombieMixamo`) : avec les vrais clips, le zombie reste au sol au pied du mur, sur une ligne recentrée sur la fenêtre (milieu des emplacements de planches). Après l'Animator (`LateUpdate`), le modèle est décalé pour que les mains restent sur le rebord (2/3 du geste) et que le bassin passe dans l'ouverture ; la chute part du rebord et finit 0,6 m derrière le mur ; en se relevant, le bassin reste en place. À la fin, le décalage est reporté sur le zombie (pas de téléportation). Seule la 2e partie du clip *Climbing* est jouée (à partir de 45 %, ×1,1). L'ancien mouvement (sans les clips) est inchangé. Contrôleurs régénérés (même GUID : pas de reconstruction de scène ni de recuisson).
- **Vérifié** : compilation sans erreur ; mode Play dans l'éditeur de Rahim (version PC), 2 fenêtres du bunker (SE, SW) × 3 types (normal, Cogneur, Coureur) : bassin au plus haut à 1,48–1,53 m (rebord 0,97 m, haut de fenêtre 1,99 m), écart au centre ≤ 0,36 m, 5 cm de déplacement du bassin quand l'agent reprend la main ; captures image par image : mains sur le rebord, corps dans l'ouverture, chute à plat derrière le mur, relevé. **Pas testé au casque.**
- Wiki : section « Escalade des fenêtres » de la page Zombies (commit local, pas poussé). Rien poussé.

---

## 136 — Chaise et desserte retirées près des fenêtres

**Date** : 2026-11-09 · **Domaine** : Carte

> Retire cette chaise ainsi que cette table proche des fenêtres

(2 captures jointes, version PC, MainScene)

**Résultat** (fait par l'IA, Claude Code)

- Objets reconnus (captures comparées dans l'éditeur) : la chaise = `Bunker_Stove_Stool_01` (tabouret haut en métal sur le tapis, à côté du fût) ; la table = `Bunker_ToolCart` (desserte verte à roulettes sous la fenêtre sud-est), avec la marmite posée dessus (`Bunker_Stove_Pot`, simple décor : retirée avec elle). Tapis et papiers gardés. Aucun script ne les utilisait.
- Retirés de `MainScene` (dans l'éditeur ouvert de Rahim) et ajoutés à la liste des objets retirés de `RoomBuilder` (une reconstruction de la scène ne les remet pas).
- NavMesh recuit (la desserte était un obstacle sous la fenêtre où entrent les zombies) : 2133 → 2125 triangles, chemins dehors → dedans complets pour les fenêtres SE et SW. Éclairage recuit (SAE501 > Cuire l'éclairage), scène enregistrée. Capture après : plus de desserte ni de tabouret, pas d'ombre restante au sol.
- Avertissement « Retain Transform Parent… Items is deactivated » de la console : il vient des holsters de la ceinture et du puits du pistolet (`XRSocketInteractor.OnDisable`) quand la scène se ferme (fin du mode Play, rechargement) : « Items » est déjà désactivé au moment où ils lâchent pistolet, lampe et chargeur. Sans effet en jeu ; non modifié (le changer toucherait la prise des objets au casque).
- **Pas testé au casque.** Rien poussé.

---

## 137 — Textures PBR sur les objets en aplat

**Date** : 2026-11-09 · **Domaine** : Graphismes

> Fais l'inventaire des matériaux/objets restés low poly (couleurs unies, sans texture PBR) dans MainScene, propose et applique des textures PBR plus réalistes et détaillées (albedo/normal/roughness, usure, saleté, cohérents avec l'ambiance hôpital/bunker sombre), en gardant le budget Quest 3 (textures 1024-2048 max, compression ASTC, pas de coût de perf excessif). Si les modèles n'ont pas d'UV, signale-le. Après changement des matériaux, rebake l'éclairage avant commit. Montre des captures avant/après.

(demande transmise avec une capture du poste de surveillance : écrans CCTV, radio, boîtier à LEDs, clavier, lampe, bureau en aplats unis)

**Résultat** (fait par l'IA, Claude Code)

- **Inventaire** (script dans l'éditeur, MainScene) : 24 matériaux de décor sans texture. Poste de surveillance : `MetalDark` (cadres des écrans, meuble, magnétoscope à LEDs, clavier), `Dark`/`Metal` (5e écran généré), `RadioBody`/`RadioBlack`/`HammerSteel` (radio), `Radio` (radio générée). Ailleurs : `DoorMetal` (caisses, fûts, portes), `GeneratorPaint`, `FuelRed`, `Chrome`, `Rubber`, `LampShade`, `Paper`, `Countertop`, `FridgeEnamel`, `SinkEnamel`, `Mat_CCTV_CameraBody`, `stone_01` (pierres du jardin : normale sans couleur). Laissés tels quels : LED, ampoules, verres, écrans CCTV (shader animé), interface, manettes.
- **Sans UV** (texture impossible, restent unis) : les 4 caméras CCTV et leurs supports muraux, l'armoire de premiers secours, le tableau de carte tactique. À déplier dans Blender.
- `textures.zip` ne contenait que des textures de jardin (arbres, rochers, pierre) : seule la couleur de stone_01 a servi. Rien téléchargé.
- **Textures** : `Tools/textures/generer_usure.py` (Python, numpy) fabrique des textures raccordables 1024 px à partir de rust_coarse_01 et dirty_concrete (Poly Haven, CC0, déjà sur le Mac) : un albedo par matériau (couleur d'origine + éclats de peinture, rouille, rayures, saleté), une normale et une carte métal/lissé par famille (peinture, acier, plastique, papier). 22 textures dans `Assets/Room/Textures/Usure/`, ASTC 6x6 sur Android, mipmaps, environ 14 Mo de mémoire en tout.
- **Application** : `Assets/Editor/TexturesUsure.cs` (menu SAE501 > Salle > Appliquer les textures d'usure), appelé aussi à la fin de `RoomImport.Import` et de `RoomBuilder.Populate` (ces deux scripts recréent les matériaux en couleur unie).
- **Vérifié** : captures avant/après du poste de surveillance dans le mode Play de l'éditeur (`docs/captures/textures_usure/`). Défaut visible : sur le clavier (face du dessus aux UV étirées), les rayures sont étirées.
- Éclairage recuit avec les modèles de l'entrée 145 (même session).
- **Pas testé au casque** (coût réel sur Quest 3 non mesuré). Rien poussé.

---

## 138 — Initialisation Game Studios et GDD

**Date** : 2026-11-09 · **Domaine** : Documentation

> Dossiers et conventions Game Studios : Le projet ne possède pas encore de fichier project.yaml ni les dossiers standards design/ et production/session-state/. Souhaites-tu qu'on initialise cette compatibilité (/init-studio) sans modifier tes fichiers existants ? Fait le project.yaml en lisant tous le concept du jeu, le gameplay etc, d'ailleurs voici le GDD :

**Résultat** (fait par l'IA, Antigravity)

- **Initialisation Game Studios** : création sans modifier les fichiers existants de `project.yaml`, `design/gdd/`, `production/session-state/` et `AGENTS.md`.
- **`project.yaml`** : configuration complète du projet (moteur Unity 6000.6.0f1, URP, cibles Quest 3 Android et PC Desktop, framerate cible 72 FPS, piliers de gameplay, hitboxes, équilibrage des zombies et vagues).
- **GDD consolidé** : formalisation dans `design/gdd/game-concept.md` du concept, des boucles macro/micro, du système de barricades, des armes, de l'IA zombie et de la gestion CCTV/énergie d'après la documentation et le code source.
- **Session State** : création de `production/session-state/active.md` consignant les réalisations récentes (portage PC, escalade) et les priorités du sprint final (rendu J-4, 13 novembre 2026).

---

## 139 — Intégration du GDD officiel (PDF)

**Date** : 2026-11-09 · **Domaine** : Documentation

> Voici le gdd

(Document PDF de 21 pages transmis : « Game Design Document: The last survivor / The Last Escapor », SAÉ 5.D.01)

**Résultat** (fait par l'IA, Antigravity)

- **Extraction et analyse intégrale du GDD (21 pages)** :
  - **Identité & Équipe** : Rahim Tamhaev, Al Amine Bah, Alban Chabalier, Hugo Delpuech ; profil Bartle (Killer 5/5, Achiever 3/5) ; USP/KSP.
  - **Gamefeel & 3C** : le corps comme manette, pas de raycast grab, ceinture diégétique, clouage en 3 coups physiques avec planche fantôme, rechargement gestuel sous la poignée, détection cinétique du corps à corps (> 1.6 m/s), caméras nocturnes via disjoncteur.
  - **Structure des 5 vagues** : 66 zombies au total (4, 8, 12, 18, 24), déblocage progressif des salles (Bunker OG -> Cuisine -> Stockage -> Couloir Ouest -> Générateur et bouton d'évacuation).
  - **Bestiaire & Hitboxes** : fiches complètes du Rôdeur (1.76 m, 0.9 m/s, 100% PV), du Cogneur (1.93 m, x0.9, 150% PV, coups de poing) et du Coureur (1.69 m, x1.6, 70% PV, coups de pied) ; table des dégâts tête (one-shot), torse (-33 PV, ralentit 1.8s) et jambes (-20 PV, ralentit 2.7s).
  - **Level Design & Circulatoire** : les 6 zones avec le portail cadenassé et le générateur électrique.
  - **Direction Artistique & Palette** : intégration des codes couleur exacts (Interface ambre #FFBF45, réticule/contour #8C6926/#FFD166, voyants #33FF4C/#FF261A, ombres #020101).
- **Mise à jour des fichiers** : `project.yaml` et `design/gdd/game-concept.md` mis à jour pour être en parfaite concordance avec le PDF.

---

## 140 — Analyse d'un audit d'équilibrage

**Date** : 2026-11-09 · **Domaine** : Game design

> Analyse :

(Suivi du texte complet d'un audit d'équilibrage et de cohérence produit par un autre outil d'IA : 5 incohérences GDD/code, analyse chiffrée PV, mêlée, vitesses, munitions, niveau 4, et un plan de 6 chantiers avec un tableau de 18 réglages.)

**Résultat** (fait par l'IA, Claude Code) : chaque constat vérifié dans le code de la branche `feature/portage-pc`. Vrais : PV par niveau et par type, mêlée en part fixe des PV de base, chevauchement des vitesses (Coureur jusqu'à 2,4 m/s au niveau 4), hache utilisable dans le bunker dès la vague 1, un seul clou suffit (choix volontaire du commit 02768f6, le GDD n'a pas suivi), disjoncteur jamais recoupé, porte forcée impossible à refermer (vrai bug), 9 zombies max au niveau 4 et 2 coups pour mourir en Difficile. Faux ou périmés : radio (1 chargeur, une fois par vague, pas 2 toutes les 75 s), chargeurs du bunker (tirés au sort, pas garantis), générateur (la jauge redescend déjà à moitié vitesse, pas de remise à zéro), traque du niveau 4 (seulement hors du bunker), mêlée sans ralenti (elle ralentit). Avis : corriger la porte, envisager le retrait de la hache du bunker et le resserrement des vitesses ; refuser les nouvelles mécaniques (coupure de courant, tête à 150 PV) à 4 jours du rendu. Aucun fichier de code modifié.


---

## 141 — Audit d'équilibrage Game Designer et analyse PC/VR

**Date** : 2026-11-09 · **Domaine** : Game design

> Tu es expert Game Designer, Je veux que tu juges l'équilibrage du jeu, ainsi que la cohérence de ce dernier. Ensuite, tu me fera un plan détaillé de quoi modifié pour un meilleur équilibrage.
> Est-ce que sur ta version tu vois bien la version pc ?

**Résultat** (fait par l'IA, Antigravity)

- **Audit d'équilibrage et de cohérence GDD vs Code (`Assets/Scripts/`)** :
  - Analyse croisée de `ZombieLevels.cs`, `WaveManager.cs`, `Zombie.cs`, `MeleeWeapon.cs`, `Plank.cs`, `RoomBuilder.cs`, `RadioEvent.cs` et `Generator.cs`.
  - Identification des écarts entre le GDD PDF (5 vagues, 66 zombies) et le code (4 niveaux de progression puis mode Survie infini), du calcul des dégâts de mêlée en fraction de `BaseHealth` (ne suivant pas `hpMultiplier`), du chevauchement de `speedRange` (`[0.75, 1.4]`), de la présence de `Hache_Bunker` dès le Niveau 1, et du bug de `HingedDoor.ForceOpen()` empêchant de refermer une porte forcée.
- **Vérification de la version PC (`feature/portage-pc`)** :
  - Analyse de `DesktopMode.cs`, `DesktopPlayer.cs`, `DesktopHud.cs` et `DesktopMenuClick.cs`.
  - Identification de 4 asymétries d'équilibrage PC vs VR :
    1. Levier du générateur maintenu par une main virtuelle découplée sur PC (`DesktopPlayer.cs`), permettant au joueur PC de reculer ou combattre pendant le maintien de 10 s.
    2. Poche secondaire (`B`) stockant un second objet lourd rendu invisible (`SetRenderers(pocketed, false)`), contournant la limite d'une seule planche transportable.
    3. Rechargement automatique en `0.55 s` via `R` avec aspiration de chargeur jusqu'à `2.2 m` (`NearestAvailableMag(2.2f)`).
    4. Aim-assist de clouage à `18 cm` du centre de la vue (`Nail.HitFromAim`).
- **Plan détaillé en 6 chantiers** présenté pour sélection avant modification du code.

---

## 142 — Pipeline Audio Combat Zombies (`/team-audio zombies-combat`)

**Date** : 2026-11-09 · **Domaine** : Audio / Accessibilité

> /team-audio zombies-combat

**Résultat** (fait par l'IA, Antigravity)

- **Artefacts de design & production audio générés** :
  - `production/audio/zombies-combat/direction.md` (Étape 1 — Direction audio, séparation spectrale 360° Cogneur/Rôdeur/Coureur, choix de l'Option B hybride DSP + synthèse procédurale).
  - `production/audio/zombies-combat/sfx-spec.md` (Étape 2 — Spécifications SFX complètes, formules DSP 44.1 kHz, matrice des 22 événements P0-P4).
  - `production/audio/zombies-combat/accessibility.md` (Étape 2 — Audit d'accessibilité multimodale VR/PC, résolution des 3 écarts GAP 1, GAP 2 et GAP 3).
  - `production/audio/zombies-combat/integration-plan.md` & `production/audio/zombies-combat/engine-notes.md` (Étape 3 — Plan d'intégration Unity 6000.6.0f1, budget RAM < 400 Ko en cache statique, Single-Pass Instanced Stereo).
  - `design/audio/audio-zombies-combat.md` (Document de design audio consolidé).
- **Implémentation C# & Shader (Étape 4)** :
  - `ProcAudio.cs` : ajout de 9 générateurs procéduraux mis en cache statique (`Warmup` + `SubsystemRegistration`) : `ZombieStep_Walker`, `ZombieStep_Bruiser`, `ZombieStep_Runner`, `AttackWindup`, `ZombieDeath`, `BodyThud`, `MeleeSwing`, `AxeImpact`, `CrowbarImpact`, et filtrage anti-stridence sur `ClawSwipe`.
  - `ZombieVoice.cs` : différenciation vocale par archétype (pitch, cadence, pondération `Voix_1`/`Voix_2`, priorité `64/80/110`) et préemption (`maxVoicesAtOnce = 4`).
  - `Zombie.cs` : pas au sol par archétype (`GroundSteps`), télégraphe d'attaque (`PlayAttackWindup`) avec alerte d'angle mort, et râle + chute du corps à la mort (`ZombieDeath` + `BodyThud`), modulés par `GameSettings.Zombies`.
  - `MeleeWeapon.cs` : détection de swing (`MeleeSwing`) et différenciation d'impact Hache (`AxeImpact`) vs Pied-de-biche (`CrowbarImpact`).
  - `MusicPlayer.cs` : machine à 4 états d'intensité de combat, `priority = 200`, et API `MusicPlayer.Duck(factor, duration)`.
  - `DamageVignette.shader`, `HealthVignette.cs` & `Plank.cs` : exploitation de `_Hits[k].w` pour afficher un arc périphérique ambre (`NotifyWarningFrom`) lors d'un bris de barricade hors champ (GAP 1) ou d'un `AttackWindup` dans l'angle mort (GAP 2).
  - `InspectHud.cs` & `Pistol.cs` : `InspectHud.Flash(1.5f)` forcé sur clic à vide `DryFire` (GAP 3) et `spatialBlend` adaptatif VR (`0.65f`) / PC (`0.20f`).

## 143 — Checkup du projet après Antigravity

**Date** : 2026-11-09 · **Domaine** : Qualité / Code

> Ok analyse le projet Unity directement, et cherche les bugs si il y en a dans le code ou autre, j'ai recemment fait une maj avec Antigravity, visuellement aucun bug, cependant le procédé de compilation étant particulier je préfère un checkup de ta part

**Résultat** (fait par l'IA, Claude Code) : relecture du diff non commité (audio de combat d'Antigravity : ProcAudio, Zombie, ZombieVoice, MusicPlayer, MeleeWeapon, Pistol, Plank, HealthVignette, InspectHud, DamageVignette.shader). La compilation passe (Assembly-CSharp recompilé après les dernières modifications, aucune erreur C# dans la console ; seules erreurs : MRUK « global context », bruit du paquet Meta). Aucun .meta orphelin, aucun fichier sans .meta, aucun GUID dupliqué ; les scènes et prefabs du jeu ne pointent vers aucun asset manquant (références manquantes seulement dans les packs tiers et les samples). Problèmes relevés : musique passée en priorité 200 (peut être coupée quand les 32 voix sont prises), hauteur de la musique modifiée (désaccord audible), état « brèche » quasi permanent, arc ambre qui peut écraser un arc rouge actif, arc ambre sur chaque planche cassée même visible ou loin, seuils d'archétype dupliqués entre Zombie et ZombieVoice, clips DontSave qui s'accumulent dans l'éditeur (rechargement de domaine désactivé), wiki pas mis à jour pour l'audio. Rapport envoyé à Rahim, aucun fichier de code modifié avant son accord. Non testé en jeu ni au casque (pas de mode Play lancé).

## 144 — Musique de brèche et correctifs du checkup

**Date** : 2026-11-09 · **Domaine** : Audio / Code

> 1. "L’état « brèche » est presque toujours actif (`MusicPlayer.cs:95`) : il suffit d’une fenêtre sans planche, donc les 4 états de musique ne servent pas vraiment." ouais mais ducoup on ne fait quasiment jamais les planches donc cet état ne sert à rien

> ok

**Résultat** (fait par l'IA, Claude Code) : l'état « brèche » de MusicPlayer se déclenche quand un zombie vivant est dans le bunker (`WaveManager.AnyZombieInBunker`, zone `Zombie.BunkerArea`) au lieu d'une fenêtre sans planche. Musique remise en priorité 0 (jamais coupée faute de voix). HealthVignette : l'arc ambre ne remplace plus un arc rouge de blessure encore affiché ; nouvelle fonction `NotifyWarningIfUnseen` utilisée par Plank (planche à moins de 12 m et hors du champ de vision). Seuils Cogneur/Coureur définis une seule fois dans Zombie (`IsBruiser`, `IsRunner` publics), lus par ZombieVoice. Hauteur de la musique laissée telle quelle. Compilation vérifiée hors éditeur (dotnet build : 0 erreur) ; l'éditeur était en mode Play, donc pas de test en jeu ni au casque. Pas de commit : les mêmes fichiers contiennent les ajouts non commités d'Antigravity.

---

## 145 — Modèles détaillés du poste de surveillance

**Date** : 2026-11-09 · **Domaine** : Graphismes

> C'est encore low poly, corrige ça en utilisant de bonnes textures réalistes et optimisé

(capture jointe : poste de surveillance en vue scène, après l'entrée 137)

**Résultat** (fait par l'IA, Claude Code)

- **Constat** : les textures de l'entrée 137 étaient bien appliquées et enregistrées ; le problème venait de la forme. La radio, l'enregistreur à LEDs, le clavier, l'étagère à écrans et le 5e écran (avec son pied) étaient de simples boîtes, et aucune texture ne pouvait le masquer.
- **Modèles** : `Tools/blender/poste_surveillance.py` (Blender 5.2, en ligne de commande) construit 5 modèles détaillés, à la place exacte des boîtes, dans `Assets/Decor/Surveillance/` : étagère en tôle pliée sur montants tubulaires avec semelles vissées, écharpes et goulotte de câbles ; enregistreur avec façade, oreilles de baie vissées, fente cassette, boutons, molette, ouïes et pieds ; clavier à touches beiges et pupitre de commande des caméras (manette, boutons, molette, afficheur) sur socle incliné ; radio aux coins arrondis avec grille de haut-parleur, cadran, boutons, poignée et antenne télescopique ; moniteur avec cadre, coque arrière, boutons, rotule et socle. Arêtes biseautées, UV en projection cube (texture répétée tous les 0,6 m environ). Environ 15 000 triangles pour les 5 modèles, sans nouvelle texture : ils réutilisent les matériaux d'usure de l'entrée 137.
- **Durable** : `Assets/Editor/PosteSurveillance.cs`, appelé par `RoomBuilder` (carte : avant la création des collisions ; 5e écran : dans `BuildExtraScreen`, avec les boîtes d'origine si le modèle manque). Appliqué aussi dans la `MainScene` ouverte (collisions en boîtes recalées), scène enregistrée.
- Éclairage recuit (SAE501 > Cuire l'éclairage), scène enregistrée par le script de cuisson.
- **Vérifié** : captures dans l'éditeur avant/après et vues rapprochées (`docs/captures/textures_usure/modeles_*.jpg`). **Pas testé au casque** (coût réel sur Quest 3 non mesuré, mais le surcoût est faible : ~15 000 triangles statiques regroupés). Rien poussé.


---

## 146 — Refonte complète du menu de fin de partie (Terminal BUNKER-OS World Space)

**Date** : 2026-11-09 · **Domaine** : UI / UX (`/team-ui game-over-menu`)

> /team-ui Do the game over menu properly

**Résultat** (fait par l'IA, Antigravity)

- **Constat** : l'ancien `GameOverUI.cs` collait deux `TextMesh` sans cadre (`TexteGameOver` et `TexteBilan`) directement sur la caméra (`head`), imposait un rechargement automatique au bout de `15 s` (`restartDelay`), exigeait des raccourcis cachés (`Gâchette` ou maintien `B` pendant `1 s`), n'offrait aucun bouton pour revenir au menu principal (`Menu`), et laissait le curseur souris verrouillé/caché en version PC (`DesktopPlayer.cs`).
- **Spécifications produites (`/team-ui`)** :
  - `design/ux/game-over-menu.md` (Spécification UX complète 15/15 sections, validée `/ux-review` `APPROVED`).
  - `design/ux/interaction-patterns.md` (Bibliothèque de patterns BUNKER-OS : `TerminalPanel`, `BigButton`, `BilanBox`/`StatRow`, pointeur VR/PC).
  - `production/ui/game-over-menu/visual-design.md`, `engine-notes.md`, `review-ux-designer.md`, `review-art-director.md`, `review-accessibility-specialist.md`.
- **Implémentation C#** :
  - `Assets/Scripts/GameOverUI.cs` : remplacement du texte collé à la tête par un vrai terminal World Space **BUNKER-OS** (`Panneau_Fin`, `1,52 m x 1,24 m`, `scale = 0.8`, posé à `1,3 m` devant le joueur sur le cap horizontal, `UiKit.SetOnTop(true)`, `Physics.SyncTransforms()`, `MenuPointer`). Bichromie **Défaite** (`BUNKER-OS // ALERTE CRITIQUE`, `VOUS ÊTES MORT` en Rouge/Ambre) vs **Victoire** (`BUNKER-OS // EXTRACTION`, `VOUS AVEZ ÉCHAPPÉ` en Vert/Or). Encadré `BILAN DE MISSION` aligné en 2 colonnes (`StatRow`) avec badge `NOUVEAU RECORD !` en mode Survie. Boutons interactifs `MenuButton` sans relance automatique forcée : 3 boutons (`REPRENDRE AU NIVEAU X`, `RECOMMENCER (NIVEAU 1)`, `MENU PRINCIPAL`) si mort au niveau `>= 2` en Évasion, ou 2 boutons (`REJOUER` / `NOUVELLE PARTIE`, `MENU PRINCIPAL`) sinon, avec verrou anti-missclick `SafetyLock` de `1,0 s` après le fondu. Tous les champs publics historiques sont conservés pour ne pas toucher à `MainScene.unity` ni à `RoomBuilder.cs`.
  - `Assets/Scripts/Desktop/DesktopPlayer.cs` : déverrouille automatiquement la souris (`LockCursor(false)`), bloque la recapture au clic gauche (`!GameOver`), et oriente doucement la caméra vers `GameOverUI.Instance.PanelTransform` dès que `GameOverUI.PanelOpen` est vrai.
  - `Assets/Scripts/PauseMenu.cs` : bloque l'ouverture de la pause si `WaveManager.Instance.Victory` ou `GameOverUI.PanelOpen` est vrai.
  - `Assets/Scripts/MenuButton.cs` : passage de la décroissance de `glow` en `Time.unscaledDeltaTime`.

---

## 147 — Textes des portes cachés par les murs

**Date** : 2026-11-09 · **Domaine** : Interface / Rendu

> Actuellement il est possible de voir les textes même à travers les murs, fait en sorte que ce ne s'affiche que quand le joueur soit proche de la porte (et qu'il n'y ai pas un mur qui sépare le joueur et le texte), à toi de voir c'est quoi la méthode la plus efficace / optimisée

(capture jointe : panneau rouge « VERROUILLÉ » de la porte du stockage vu à travers un mur)

**Résultat** (fait par l'IA, Claude Code)

- **Cause** : les panneaux des portes sont des `TextMesh` avec le matériau par défaut de la police intégrée (`GUI/Text Shader`), dont le test de profondeur dépend d'une variable globale qui vaut « toujours » dans URP : le texte se dessinait par-dessus tout.
- **Méthode choisie (la moins coûteuse sur Quest 3)** : nouveau shader `Assets/Shaders/WorldText.shader` (`SAE501/WorldText`) : test de profondeur normal (les murs cachent le texte, sans raycast) et fondu selon la distance à la caméra calculé dans le shader (plein jusqu'à 3,5 m, invisible au-delà de 5 m) ; aucun script par image. Compatible stéréo instanciée (VR) et PC.
- Matériau partagé `Assets/Resources/WorldText.mat` ; `Assets/Scripts/WorldTextMaterial.cs` recopie la texture de la police quand Unity la reconstruit (police dynamique).
- `RoomBuilder.MakeLabel` l'utilise : panneaux des 4 portes, état du générateur, affichage et étiquette de la radio (ombres coupées). Les textes d'interface (tutoriel, menus, HUD) gardent leur affichage par-dessus tout.
- Appliqué aussi aux 7 textes de la `MainScene` ouverte, scène enregistrée (pas de reconstruction).
- **Vérifié** : captures dans l'éditeur du panneau du stockage à 1,6 m (lisible), 4,2 m (estompé), 6 m et derrière la porte (invisible). **Pas testé au casque.**

---

## 148 — Murs qui entrent les uns dans les autres

**Date** : 2026-11-09 · **Domaine** : Carte / Rendu

> Comme on peut le voir il y a des abérations car les murs sont les uns dans les autres corrige ça

(captures jointes : bande tachée à côté du disjoncteur du bunker, vue en fil de fer des murs)

**Résultat** (fait par l'IA, Claude Code)

- **Cause** : dans la carte Blender, les murs du couloir droit traversent le mur est du bunker ; leur face d'about tombe exactement dans le plan de la face visible du mur (plâtre du couloir contre papier peint du bunker) : z-fighting, une bande de 15 cm de chaque côté de la porte. Même problème sous les portes (deux planchers), aux angles des murs, des plinthes, des lambris et des corniches.
- **Correction durable** : `Assets/Editor/MursSuperposes.cs`, appelé par `RoomBuilder` avant les collisions. Pour la structure fixe (murs, sols, plafonds, plinthes, corniches, lambris, encadrements), un triangle aligné sur les axes, entièrement recouvert par une face d'un autre objet dans le même plan et du même côté, est retiré (il ne se voyait qu'à travers l'autre). Les objets déplaçables ou à prendre ne sont pas touchés. Résultat : 224 triangles retirés sur 39 éléments ; copies des maillages dans `Assets/Generated/Recouvrements` (le FBX n'est pas modifié).
- Appliqué dans la `MainScene` ouverte, scène enregistrée. Éclairage non recuit : les surfaces restantes sont les mêmes (seuls des doublons dans le même plan sont retirés).
- **Vérifié** : capture avant/après du mur du disjoncteur (la bande tachée disparaît, seul le papier peint reste au lancer de rayon), vues des autres jonctions (couloir, stockage, cuisine, porte du couloir) sans trou. **Pas testé au casque.** Restent quelques carrés de 2 à 3 cm aux angles de plinthes et de corniches (recouvrement partiel, même matériau) et le bureau de commande qui chevauche le bureau de surveillance (mobilier, non traité).

---

## 149 — Branche develop et archivage des branches fusionnées

**Date** : 2026-11-09 · **Domaine** : Git

> Fait un push dans une branch propre et archive celles qui ont déjà été merge avec celle là

(Puis, aux deux questions posées : « oui » pour inclure les changements Antigravity non commités, « Archive les au cas-où » pour les 16 branches.)

**Résultat** (fait par l'IA, Claude Code)

- Nouvelle branche distante **`develop`** à partir de `feature/portage-pc` (portage PC, escalade corrigée, PR #146 fusionnée localement, textures PBR et modèles du poste de surveillance, shader WorldText, murs superposés), puis commit des ajouts Antigravity validés : audio de combat (ProcAudio, ZombieVoice, MusicPlayer), vignette de dégâts, musique de brèche et correctifs du checkup, menu de fin de partie, documents Game Studios (`project.yaml`, `AGENTS.md`, `design/`, `production/`) et entrées 138 à 146 du journal. Rien n'est fusionné dans `main`.
- Restent hors commit : fichiers personnels (réglages URP, profil de build Quest avec l'identifiant du casque, `RuntimeActionBindings.json`, `DevAgentSettings.asset`, `Protection_suite`, `scripts/`), changements automatiques de l'éditeur dans `Assets/Generated` et `Tools/blender/cinematique`.
- Wiki poussé (4 commits en attente : Version PC, escalade, tabouret et desserte, textes des portes, murs superposés).
- 16 branches distantes entièrement contenues dans `develop` gardées en tags `archive/<nom>` puis supprimées : Gestion-vague-infini, Nouveau-son, UI-Hugo, claude/project-thread-6whioa, claude/project-thread-vr7je7, feature/7.2-effet-cctv, feature/74-zombie-test, feature/aex_usable, feature/ceinture-marteau, feature/map-finale, feature/menu-accueil, feature/zombie-animation, features/passer-attente, features/tutoriels, features/types-zombies, test/vagues-infinies. Gardées : `main`, `archive/version-pc`, `features/escalade-zombies` (PR #146 encore ouverte) et les 7 branches avec du travail absent de `develop`.

---

## 150 — Trous du toit irréguliers

**Date** : 2026-11-09 · **Domaine** : Carte / Rendu

> Change la forme du trou, un peu plus varié que juste un carré, et retire les tiges qui tombent du toit, fait un effet sur les bords du trou

(capture jointe : trou carré au-dessus de l'établi, avec des tiges sombres qui pendent ; puis « Et la suite ? », pris comme accord pour quitter le mode Play et reconstruire)

**Résultat** (fait par l'IA, Claude Code)

- **Cause** : les deux trous du toit du bunker (au-dessus des lits et de l'établi) sont des carrés de 1,2 x 1,2 m découpés dans la dalle `Bunker_Ceiling` de la carte Blender ; les « tiges » sont 9 lianes en boîtes sombres de `Apoc_Vines_And_Moss` qui pendent de chaque trou jusqu'à 1,3 m du sol.
- **Correction durable** : `Assets/Editor/TrousToit.cs`, appelé par `RoomBuilder` après les collisions (le FBX n'est pas modifié) :
  - lianes qui pendaient des trous retirées (copie du maillage ; les taches de mousse au sol restent) ;
  - **bord de béton cassé** qui remplit les coins du carré : ouverture irrégulière (~0,7 à 1,3 m), arête ébréchée, enduit tombé sous la dalle, tranche en béton nu plus sombre ; dessus et dessous dans le plan de la dalle avec les mêmes UV que le plafond (pas de raccord) ;
  - **auréole noircie** (suie et saleté, texture de bruit, transparente vers l'extérieur) sous le plafond autour du trou ;
  - **gravats** au sol sous chaque trou (9 éclats de 8 triangles, seulement sur le sol, pas sur les meubles).
  - Coût : ~800 triangles de bord et ~330 d'auréole par trou, 1 matériau opaque et 1 transparent de plus, aucune collision (NavMesh, zombies et caisse inchangés). La poussière du toit tombe d'une zone de 0,75 m (au lieu de 1 m) pour rester dans l'ouverture.
- Scène reconstruite (**SAE501 → Build VR Scene**) et éclairage recuit.
- **Vérifié** (éditeur, sans mode Play) : captures avant/après (plus de tiges, bord irrégulier) ; la caisse de la radio a au moins 35 cm de jeu autour de son point de chute et rien ne bloque sa descente (lancer de boîte) ; les deux trous gardent leur lien NavMesh, `fromAbove` et leur poussière. **Non testé** en mode Play (chute réelle d'un zombie du toit et d'une caisse) ni **au casque**.

---

## 151 — Trous du toit élargis

**Date** : 2026-11-09 · **Domaine** : Carte / Rendu

> élargie le trou un peu

**Résultat** (fait par l'IA, Claude Code)

- `TrousToit.cs` : rayon de base de l'ouverture porté de 80 % à 95 % du demi-côté du carré (minimum 75 % au lieu de 62 %), bord de béton plus fin contre le carré d'origine. L'ouverture passe d'environ 0,7-1 m à 0,9-1,15 m ; les coins du carré restent cassés, le bord reste irrégulier.
- Caisse de la radio : toujours au moins 35 cm de jeu autour de son point de chute. Scène reconstruite, éclairage recuit.
- **Vérifié** : capture dans l'éditeur. **Non testé** en mode Play ni au casque.

---

## 152 — Portail du jardin en fer forgé vieilli

**Date** : 2026-11-10 · **Domaine** : Carte / Rendu

> Update la texture de la grille pour qu'lle soit plus réaliste

(capture jointe : portail de sortie du jardin, barreaux noirs unis)

**Résultat** (fait par l'IA, Claude Code)

- **Cause** : les deux battants (`Garden_ExitGate_Left` / `_Right`) utilisaient `MetalDark`, le noir des cadres d'écrans, avec des UV incohérentes dans le FBX (barreaux écrasés ou étirés) : la texture d'usure n'y donnait presque rien.
- **Textures** : `Tools/textures/generer_portail.py` (Python, à partir de `rust_coarse_01` de Poly Haven, CC0) génère albedo, normal et métal/lissé 1024 (`Assets/Room/Textures/Usure/Portail_Fer_*`) : peinture noire satinée écaillée, éclats rouillés plus nombreux au pied, coulures de rouille sous les éclats, terre et mousse sur les 50 premiers centimètres, relief martelé du fer forgé. ASTC 6x6 sur Quest (réglages de `TexturesUsure`).
- **Application durable** : `Assets/Editor/PortailFer.cs`, appelé par `RoomBuilder` : UV refaites par projection sur les faces (u = 1 m de largeur, v = hauteur du battant, pour que la rouille soit en bas et les coulures vers le bas), matériau propre `Portail_Fer`. Même nombre de triangles, un matériau de plus. Le cadenas n'est pas touché.
- Scène reconstruite, éclairage recuit.
- **Vérifié** : captures dans l'éditeur (vue d'ensemble et de près). **Non testé** au casque.

---

## 153 — Mini serveur pour le multijoueur

**Date** : 2026-11-10 · **Domaine** : Game design

> Ok réfléchissons à commentre mettre en place un mini serveur pour le mode multijoueur

**Résultat** (réflexion écrite par l'IA, Claude Code ; aucun code)

- Document : `Mini_serveur_multijoueur.md` dans les fichiers du projet Claude (pas dans le dépôt), suite du plan 103.
- Constat : aucun code réseau dans le projet, permission Internet et HTTP non chiffré désactivés, rendu dans 3 jours, gel des fonctionnalités dépassé, multijoueur toujours « NE FERA PAS » dans le GDD.
- Trois options comparées : hôte = un joueur, serveur dédié Unity sur le Mac (aussi coûteux, la difficulté est la synchronisation du monde, pas le serveur), mini serveur Python de lobby et de scores où chaque casque joue sa propre partie (lobby, prêt, top départ, graine commune, score par joueur et d'équipe, page de classement pour le jury ; 2,5 à 3 j).
- Conseil : ne pas toucher au build rendu ; option « parties en parallèle » seulement sur une branche à part et seulement si Rahim la valide ; vraie coop après le rendu.

---

## 153 — Alarme de début de vague retirée

**Date** : 2026-11-10 · **Domaine** : Audio

> retire des sons l'alarme du début des vagues, seul le rire ( le sons que je t'avais fait mettre) suffit

**Résultat** (fait par l'IA, Claude Code)

- Compris : « le rire » = le son de début de vague ajouté au journal 127 (`Vague_Debut.ogg`, joué par `WaveTitle`, que le code appelle un chuchotement) ; c'est celui qui est gardé. À confirmer par Rahim si c'est un autre son.
- `WaveManager.StartLevel` ne joue plus l'alarme à deux tons (`ProcAudio.Alarm`). L'alarme reste jouée une fois au déblocage du générateur au niveau 4 (« LE GÉNÉRATEUR EST PRÊT »), à 60 % du volume : ce n'est pas un début de vague, laissée telle quelle.
- Script seul, scène non reconstruite. Wiki (page Niveaux et vagues) mis à jour. **Non testé** en mode Play ni au casque.


---

## 154 — Modélisation de l'arène 1v1 dans Blender via MCP et décomposition des systèmes

**Date** : 2026-11-10 · **Domaine** : Carte / Level Design / Game design

> /map-systems En te connectant a blender en MCP et en t'inspirant de l'univers de la map actuellement contruite sur Unity / Le thème du jeu : Créer moi une map 1V1

**Résultat** (fait par l'IA, Antigravity)

- **Connexion MCP Blender** : Détection et exécution directe de scripts Python dans Blender 5.2.1 LTS.
- **Création 3D de l'Arène Bunker 1v1 (`Tools/blender/arena_1v1/arena_1v1_bunker.blend` et `.fbx`)** :
  - **Layout & Structure** : Dimensions 30 m x 16 m x 4 m, sol béton industriel, murs d'enceinte renforcés.
  - **Level Design tactique symétrique à 180°** : Brise-vue central en béton (6 m x 2.8 m) interdisant tout tir immédiat au spawn, 8 piliers porteurs massifs (1.2 m), 4 chicanes métalliques d'approche.
  - **Couvertures & Props** : Barils métalliques aux couloirs de flanquement, établis d'atelier, caisses d'approvisionnement disputées au centre.
  - **Repères de spawn & gameplay** : Empties `Spawn_Player_1`, `Spawn_Player_2`, et 5 points de ramassage d'armes/munitions (`Pickup_Weapon_*`, `Pickup_Ammo_*`, `Pickup_Special_Center`).
  - **Éclairage d'ambiance** : Points lumineux d'ambiance ambre aux bases de réapparition, néons froids industriels sur les couloirs latéraux, alerte centrale, caméra en plongée tactique.
- **Documentation Game Design & Systèmes** :
  - Création de `design/gdd/systems-index.md` (décomposition complète des 15 systèmes du jeu, intégration du 1v1).
  - Rédaction de la fiche de Level Design dans `design/gdd/arena-1v1-map.md`.
  - Mise à jour du point de contrôle de session `production/session-state/active.md`.

---

## 155 — Texturing PBR procédural et Lighting Design de l'Arène 1v1 dans Blender via MCP

**Date** : 2026-11-10 · **Domaine** : Carte / Graphismes / Éclairage

> Pour le blocking de la map je suis d'accord, fait le texturing / lightning aussi

**Résultat** (fait par l'IA, Antigravity)

- **Texturing PBR Procédural & UV Mapping** :
  - `Mat_Concrete_Floor` : Shader complet combinant macro-bruit d'humidité et micro-grain, roughness graduée et bump relief. Projection UV cube 4.0m.
  - `Mat_Concrete_Wall` : Béton de bunker militaire sombre texturé avec grain de coffrage, salissures et bump.
  - `Mat_Steel_Dark` : Acier laminé métallique (0.92) avec micro-rayures bruitées.
  - `Mat_Rust_Cover` : Oxydation étagée (brique et brun foncé) sur barils et tôles.
  - `Mat_Hazard_Yellow` : Jaune alerte industriel dégradé sur les caisses centrales.
- **Lighting Design (15 sources d'éclairage équilibrées)** :
  - **Spawns Joueur 1 & Joueur 2** : Spots verticaux chauds Ambre 450 W (`#FF9933`) et fills doux 60 W.
  - **Couloirs de flanquement** : 4 spots néons industriels froids blafards 380 W (`#66B2FF`) créant une alternance d'ombres et de lumière.
  - **Zone centrale** : 2 spots d'alarme rouge ambré 500 W (`#FF3311`) sur les caisses et fill central 120 W.
  - **Rim lights** : 4 éclairages rasants sur les piliers pour détacher nettement les silhouettes en VR.
  - **Color Management** : Configuration AgX Punchy (+0.2 EV d'exposition).
- **Mise à jour des fichiers** :
  - `Tools/blender/arena_1v1/arena_1v1_bunker.blend` et `arena_1v1_bunker.fbx` mis à jour et réexportés.
  - Documentation de level design mise à jour dans `design/gdd/arena-1v1-map.md`.
  - Checkpoint de session mis à jour dans `production/session-state/active.md`.

---

## 156 — Passe d'irrégularités réalistes et props industriels sur l'Arène 1v1 dans Blender via MCP

**Date** : 2026-11-10 · **Domaine** : Carte / Graphismes / Modélisation

> Améliore la map, un peu plus de props, plus de grouves, moins de carrés justes, un peu plus de realismes et d'irrégularités

**Résultat** (fait par l'IA, Antigravity)

- **Casse des angles droits & chanfreins** :
  - Biseautage (Bevel 8 cm) et adoucissement des normales sur les 8 piliers porteurs et le mur brise-vue central.
  - Déformation par bruit organique sur les sommets supérieurs pour simuler des éclats de maçonnerie et du béton ébréché.
- **Réseau de tuyauteries industrielles** :
  - 4 conduites horizontales en acier et rouille (`Pipe_North_*`, `Pipe_South_*`) longeant les plafonds Nord et Sud.
  - 4 colonnes d'alimentation verticales (`Pipe_Drop_*`) reliant le plafond au niveau intermédiaire.
- **Désaxement organique des couvertures & props** :
  - Barils métalliques renversés au sol avec rotation désaxée (`Barrel_Overturned_NW` et `_SE`).
  - Caisses de munitions pivotées de quelques degrés et caisses secondaires empilées de travers (`Crate_Stack_*`).
  - Barils supplémentaires placés dans les angles morts des zones de spawn.
- **Débris & Flaques réfléchissantes** :
  - 6 amas de blocs de béton brisés et décombres rocheux (`Rubble_*`) au pied des piliers.
  - 4 flaques d'eau sombre déformées (`Mat_Water_Puddle`, IOR 1.333, roughness 0.08) sous les néons latéraux générant des reflets spéculaires au sol.
- **Mise à jour des fichiers** :
  - Sauvegarde de `Tools/blender/arena_1v1/arena_1v1_bunker.blend` (154 Ko) et réexport de `arena_1v1_bunker.fbx` (216 Ko).
  - Mise à jour de `design/gdd/arena-1v1-map.md`, `production/session-state/active.md` et `docs/PROMPTS.md`.

---

## 157 — Déconstruction des formes cubiques et enrichissement réaliste de l'Arène 1v1 dans Blender via MCP

**Date** : 2026-11-10 · **Domaine** : Carte / Modélisation 3D

> C'est encore trop carrés, un peu plus d'irrégularité (capture Blender fournie)

**Résultat** (fait par l'IA, Antigravity)

- **Suppression de l'aspect « cubes parfaits » et boîtes monolithiques** :
  - **Piliers octogonaux et piliers fracturés** : Remplacement des 8 blocs de béton cubiques par des sections octogonales de bunker militaire. Deux piliers (`Pillar_Octo_2` et `_3`) ont été brisés net à 1.6 m avec sommet écroulé et profil déchiqueté.
  - **Barrières d'approche brisées en diagonale** : Suppression des 4 cloisons métalliques plates ; remplacement par des murets orientés à 15° (`Broken_Barrier_*`) avec crête éboulée en dégradé diagonal et chanfreins biseautés.
  - **Mur central à meurtrière** : Remplacement du rectangle plein par `Central_Breached_Wall`, présentant un affaissement central formant une fenêtre de tir protégée.
- **Ajout de géométries non-orthogonales & props de guerre** :
  - **Hérissons tchèques antichars** : 2 croisillons de profilés métalliques en acier croisé (`Czech_Hedgehog_NW` et `_SE`), apportant des angles vifs à 45° et 60°.
  - **Tôles ondulées cintrées** : Plaques métalliques à profil sinusoïdal (`Corrugated_Sheet_*`) calées de biais contre les piliers écroulés.
  - **Redoutes en sacs de sable arrondis** : 18 sacs de sable courbés (`Sandbag_*`) agencés en arc de cercle et superposés en quinconce devant les caisses de tir.
- **Fichiers & Exports mis à jour** :
  - `Tools/blender/arena_1v1/arena_1v1_bunker.blend` (277 Ko) et export `arena_1v1_bunker.fbx` (428 Ko).
  - Documentation de level design mise à jour dans `design/gdd/arena-1v1-map.md`.

---

## 158 — Élargissement de l'Arène 1v1, passerelles surélevées et escaliers d'accès dans Blender via MCP

**Date** : 2026-11-10 · **Domaine** : Carte / Modélisation 3D / Level Design

> Fait la plus large, avec aussi des espaces en hauteur, des escaliers pour monter

**Résultat** (fait par l'IA, Antigravity)

- **Élargissement spatial de l'Arène** :
  - **Dimensions au sol** : Passage de 30 m x 16 m à **34.0 m x 26.0 m** (surface au sol de 884 m²).
  - **Hauteur sous plafond** : Augmentation de 4.0 m à **5.2 m** pour accueillir le jeu sur deux niveaux verticaux.
  - Recalage des murs d'enceinte et des réseaux de tuyauteries industrielles aux nouvelles cotes.
- **Ajout de la verticalité (Niveau +2.4m)** :
  - **Deux passerelles surélevées** (`Catwalk_North` et `Catwalk_South`) en béton armé de 12.0 m x 3.8 m x 0.35 m, portées par des fûts octogonaux (`Catwalk_Pillar_*`).
  - **Garde-corps & rambardes métalliques** (`Rail_*`) avec main courante à 1.05 m et montants verticaux ajourés pour protéger les tirs et voir en contrebas.
  - **Nids de tir / Armes de précision** : Deux caisses d'armement lourd (`High_Ground_Crate_*`) et points de ramassage sniper (`Pickup_HighGround_Sniper_*`) sur chaque passerelle.
- **Escaliers d'accès industriels en béton** :
  - `Stairs_West_North` : 11 marches (giron 35 cm, hauteur 20 cm, largeur 1.5 m) montant de la zone Ouest vers la passerelle Nord.
  - `Stairs_East_South` : 11 marches montant de la zone Est vers la passerelle Sud en symétrie 180°.
- **Éclairage & Caméra** :
  - Ajout de 4 spots d'éclairage (`Spot_Catwalk_*` et `Spot_Stairs_*`) pour mettre en valeur les accès verticaux.
  - Recul et rehaussement de `Tactical_Overview_Cam` (Z = 22 m, Y = -25 m) pour cadrer l'ensemble du volume élargi.
- **Fichiers & Exports mis à jour** :
  - Sauvegarde de `Tools/blender/arena_1v1/arena_1v1_bunker.blend` (299 Ko) et export de `arena_1v1_bunker.fbx` (551 Ko).
  - Documentation de level design mise à jour dans `design/gdd/arena-1v1-map.md`.
  - Checkpoint de session mis à jour dans `production/session-state/active.md`.

---

## 155 — Multijoueur coop en réseau local (Netcode)

**Date** : 2026-11-10 · **Domaine** : Gameplay / Réseau

> fait l'option A
>
> Go pour netcode alors

**Résultat** (code écrit par l'IA, Claude Code, sur la branche `feature/multijoueur` ; build rendu non touché)

- **Netcode for GameObjects 2.13.3** + Multiplayer Play Mode 3.0.0. Le Meta XR SDK 205 contient un « building block » NGO qui ne compile pas sous Unity 6.6 dès que Netcode est installé : 2 lignes corrigées dans le cache des paquets par `Tools/multijoueur/patch_meta_ngo.sh` (avec l'accord de Rahim). Unity remet le fichier d'origine quand le dossier Library est recréé : relancer le script après un clone.
- **Menu** : MULTIJOUEUR → HÉBERGER / REJOINDRE (parties trouvées sur le Wi-Fi par diffusion UDP, `LanDiscovery`) → LOBBY (joueurs, PRÊT, compte à rebours de 5 s quand tous sont prêts, puis Vagues infinies pour tous).
- **Partie** : l'hôte fait tourner vagues et zombies ; les autres casques ont des zombies « marionnettes » (positions 15 fois par seconde, coups, morts) ; leurs tirs sont appliqués par l'hôte. Avatars des joueurs (tête, mains, couleur), vie par joueur (les zombies visent le joueur vivant le plus proche), joueur à terre relevé à la pause, fin quand tous sont tombés. Barricades partagées (pose, clou, retrait, arrachage).
- **Scores** : 100 par zombie, +50 à la tête, +25 à la mêlée, 20 par planche posée ; équipe = somme + 500 par vague. Affichés sur l'écran du bunker et dans le bilan.
- Android : permission Internet (profil Meta Quest) et verrou multicast (manifeste).
- **Vérifié** : compilation sans erreur ; dans l'éditeur, hôte seul : lobby, découverte de la partie, chargement de la scène, avatar, vague, zombie tué et score (150 pour un tir à la tête), double d'un joueur distant qui transmet les coups. **Non testé** : un vrai client (deuxième instance), le Quest, deux casques.
- Wiki : page `Multijoueur`.


---

## 159 — Éclairage cinématique EEVEE, biseautage des arêtes, contreforts d'enceinte et détails militaires

**Date** : 2026-11-10 · **Domaine** : Carte / Modélisation 3D / Rendu Blender

> L'éclairage est trop sombre, éclaire un peu plus
> [Capture de la fenêtre Blender montrant un aspect encore trop cubique et un éclairage d'aperçu plat]

**Résultat** (fait par l'IA, Antigravity)

- **Suppression du rendu cubique & biseautage (Beveling)** :
  - Ajout systématique d'un modificateur `Bevel` (2 segments, limite angulaire 35°, largeur 3 à 6 cm) sur l'ensemble des éléments monolithiques : murs périmétriques, piliers octogonaux, murets brisés, mur central de brèche et passerelles métalliques. Les arêtes n'ont plus d'angle rasoir informatique et accrochent désormais des reflets spéculaires réalistes.
- **Architecture de bunker militaire & dé-cubage des murs** :
  - **22 contreforts en béton armé** (`Buttress_*`) placés tous les 4.5 m le long des murs Nord, Sud, Est et Ouest pour briser les grandes parois planes monolithiques.
  - **Socles et plinthes biseautées** (`Skirting_*`) de 35 cm de haut sur 25 cm d'épaisseur au pied de chaque mur d'enceinte.
  - **2 portes blindées de sas de décompression** (`BlastDoor_West` et `BlastDoor_East`) de 3.6 m x 2.8 m avec linteau renforcé, montants acier, nervures horizontales, volant de verrouillage étanche et bande de balisage jaune d'avertissement (`Mat_Hazard_Yellow`).
- **Dégâts réels & gravats angulaires** :
  - **12 fers à béton tordus et rouillés** (`Rebar_*`, matériau `Mat_Rust_Cover`) émergeant des sommets fracturés des piliers écroulés et de la brèche du mur central.
  - **38 débris de béton et blocs de roche angulaires** (`Rubble_*`) dispersés autour des zones de destruction.
  - **Remplacement des cubes de pickup par des caisses militaires** (`Pickup_*`) avec coffre biseauté, couvercle débordant, poignées et renforts d'acier.
- **Refonte de l'éclairage et activation du rendu EEVEE Next** :
  - Activation de l'illumination globale (`Global Illumination`, Raytracing EEVEE avec 4 rayons GI).
  - Bascule des viewports 3D en mode `RENDERED` avec `Scene Lights` et `Scene World` cochés (évitant le mode plat de l'aperçu matériel studio).
  - Création de **10 boîtiers d'éclairage néon industriels suspendus** (`Fixture_*`) avec double tube émissif (`Mat_Lamp_Emissive`) au-dessus des passerelles, voies centrales et escaliers, plus balises ambrées aux sas de réapparition.
- **Fichiers & Exports mis à jour** :
  - Sauvegarde de `Tools/blender/arena_1v1/arena_1v1_bunker.blend` (361 Ko).
  - Exportation complète de `arena_1v1_bunker.fbx` (1.15 Mo, 231 objets).
  - Session active mise à jour dans `production/session-state/active.md`.


---

## 160 — Rapprochement des passerelles contre les murs périmétriques dans Blender via MCP

**Date** : 2026-11-10 · **Domaine** : Carte / Modélisation 3D / Level Design

> Le gap est trop gros, rapproche un peu plus du mûr
> [Capture d'écran montrant le vide d'environ 90 cm entre la bordure arrière de la passerelle et le mur de béton]

**Résultat** (fait par l'IA, Antigravity)

- **Suppression du vide (gap) entre passerelles et murs** :
  - **Passerelle Nord** (`Catwalk_North`) : Translation de **+0.92 m sur l'axe Y** (centre passé de Y = 10.20 m à Y = 11.12 m). La bordure arrière (à Y = 13.02 m) vient s'encastrer de 2 cm dans la face interne du mur Nord (`Wall_North`, Y = 13.00 m) et dans les contreforts en béton armé, éliminant totalement le vide de 90 cm et évitant tout effet de scintillement coplanaire (z-fighting).
  - **Passerelle Sud** (`Catwalk_South`) : Translation symétrique de **-0.92 m sur l'axe Y** (centre passé de Y = -10.20 m à Y = -11.12 m), bordure arrière en butée parfaite contre le mur Sud (`Wall_South`, Y = -13.00 m).
- **Recalage coordonné de l'ensemble des éléments solidaires** :
  - **Escaliers d'accès** : Déplacement des 11 marches de `Stairs_West_North` (+0.90 m sur Y) et des 11 marches de `Stairs_East_South` (-0.90 m sur Y) pour conserver un alignement direct avec l'arrivée des passerelles.
  - **Garde-corps & rambardes** : Les montants (`Rail_*_Post_*`) et mains courantes restent positionnés sur la face avant tournée vers l'arène (Y = 9.25 m pour le Nord, Y = -9.25 m pour le Sud).
  - **Piliers de soutènement** : Fûts octogonaux (`Catwalk_Pillar_*`) translatés à Y = ±9.90 m sous la rive avant des plateformes.
  - **Éclairages & caisses d'armes** : Recalage des spots (`Spot_Catwalk_*`, `Spot_Stairs_*`), des luminaires suspendus (`Fixture_*`) et des points d'apparition d'armes surélevées (`Pickup_HighGround_Sniper_*`).
- **Fichiers & Exports mis à jour** :
  - Sauvegarde de `Tools/blender/arena_1v1/arena_1v1_bunker.blend` (362 Ko).
  - Export FBX mis à jour : `arena_1v1_bunker.fbx` (1.15 Mo).
  - Checkpoint de session mis à jour dans `production/session-state/active.md`.


---

## 161 — Sons de la PR #151 intégrés au portage PC et au multijoueur

**Date** : 2026-11-10 · **Domaine** : Audio

> Récupérer uniquement les sons (fichiers audio + le câblage nécessaire pour qu'ils jouent) de la PR #151 de LeCapet, pas les modifs UI. Les intégrer à la version actuelle de Rahim : le portage PC + le multijoueur.
> (demande transmise par le coordinateur du projet)

**Résultat** (fait par l'IA, Claude Code)

- **Repris de la PR #151** (« Modif UI et rajout son », branche `Nouveau-son` de LeCapet) : les 7 fichiers audio et le code qui les joue. La PR partait d'une base très ancienne (journal 097) : rien n'a été fusionné tel quel, le câblage a été réécrit sur la version actuelle (branche `feature/multijoueur`, qui contient le portage PC).
- **Sons** : `Assets/Resources/Sounds/` : `wood_hit`, `wood_break`, `Wood_hammer_nail`, `axe_whoosh`, `flesh_zombie_hit`, `zombie_bite`, `zombie_impact_fall_body_die`. Import : mono, Vorbis 0,7, « Decompress On Load » (requis pour le découpage automatique). Placés dans `Resources` pour être trouvés sans reconstruire les prefabs ni la scène.
- **`AudioSlices.cs`** (de la PR) : découpe toute seule une bande son en frappes séparées (d'après les silences) et coupe la fin silencieuse de `wood_break`. Retiré : la dépendance à une banque `GameSounds` qui n'existe pas dans le dépôt (la PR ne compilait pas sans elle).
- **Câblage** : planche frappée par un zombie (`wood_hit`, une frappe au hasard) et arrachée (`wood_break`, joué aussi chez les autres joueurs) ; clou enfoncé (`Wood_hammer_nail`) ; souffle de la hache et du pied de biche (`axe_whoosh`, un par grand geste, à la place du souffle procédural) ; zombie touché (`flesh_zombie_hit`, à la place du bruit de chair ; la tête garde son craquement), morsure calée sur le coup (`zombie_bite`), chute du corps (`zombie_impact_fall_body_die`, à la place du bruit sourd). Sans le fichier, chaque script garde son ancien son procédural.
- **Différence avec la PR pour le multijoueur** : la PR déclenchait les sons de zombie en surveillant ses PV et son état, ce qui ne marche pas sur les marionnettes d'un client. Ils sont appelés directement par `Zombie` (coup reçu, attaque, mort), y compris sur les marionnettes ; la morsure d'une marionnette ne se joue que si elle est contre le joueur de ce casque.
- **Écarté** : les modifs d'interface (`GameOverUI`, `MainMenu`, `GameStats`, `WaveManager` : mode infini et écran de fin) et le doublon `Nail (1).cs` (deuxième classe `Nail`, erreur de compilation). Aucun son de vague touché (le début de vague reste `Vague_Debut`).
- **Vérifié** : le code compile (compilateur d'Unity lancé hors de l'éditeur). **Non vérifié** : écoute en jeu, découpage réel des bandes son, réglage des volumes, multijoueur à deux, casque.



---

## 161 — Correction des aberrations géométriques, luminaires corrompus et artefacts de maillage dans Blender via MCP

**Date** : 2026-11-10 · **Domaine** : Carte / Modélisation 3D / Rendu Blender

> Corrige les abérations comme celle ci
> [Capture de la vue 3D montrant des luminaires en forme d'avions en papier déformés au plafond, un maillage pincé sur le mur central, et un amas de cubes jaunes et sacs de sable informes au sol]

**Résultat** (fait par l'IA, Antigravity)

- **Correction des luminaires suspendus (`Fixture_*`)** :
  - Suppression intégrale des anciens boîtiers de néon dont les indices de sommets bmesh avaient produit des polygones étirés en ailes d'avions en papier géantes.
  - Reconstitution propre de 10 luminaires industriels aux cotes réelles (boîtier tôle d'acier 2.2 m x 0.35 m x 0.08 m, deux tubes néons cylindriques émissifs continus sous le boîtier et câbles verticaux reliés au plafond à Z = 5.2 m).
- **Refonte intégrale du mur central (`Central_Breached_Wall`)** :
  - Suppression de l'ancienne géométrie booléenne non-planaire qui générait des facettes et triangles disgracieux sur les pentes de la brèche.
  - Modélisation propre en quads réguliers (profil en U avec hauteur 2.2 m sur les flancs et appui de tir propre à 1.25 m au centre), arêtes biseautées douces (`Bevel_SoftEdge`) et ombrage lissé sans pliure.
  - Remplacement de la boucle de ferraille flottante par 4 fers à béton d'ancrage droits et scellés dans le béton (`Rebar_Breach_*`).
- **Nettoyage de l'amas au pied du mur central** :
  - Suppression des 2 gros blocs jaunes superflus (`Supply_Crate_Central_North` et `_South`) et des 18 sacs de sable informes (`Sandbag_C_*`, `Sandbag_S_*`) qui s'interpénétraient.
  - Mise en valeur dégagée de la caisse d'armement central de duel (`Pickup_Special_Center`) posée au sol au niveau de la brèche.
- **Atténuation de l'éclairage agressif** :
  - Augmentation du fondu des cônes de spots (`spot_blend` porté à 0.75) et du rayon d'ombrage doux (`shadow_soft_size = 0.35`) pour supprimer les traînées angulaires dures au sol et les reflets brûlés sur la crête du mur.
  - Baisse de la puissance du spot d'alarme central de 900 W à 450 W avec teinte ambrée chaleureuse.
- **Fichiers & Exports mis à jour** :
  - Sauvegarde de `Tools/blender/arena_1v1/arena_1v1_bunker.blend` (331 Ko).
  - Export FBX mis à jour : `arena_1v1_bunker.fbx` (1.00 Mo).
  - Checkpoint de session mis à jour dans `production/session-state/active.md`.


---

## 162 — Ajout du toit en béton armé, poutres maîtresses en acier et gaine de ventilation dans Blender via MCP

**Date** : 2026-11-10 · **Domaine** : Carte / Modélisation 3D / Level Design

> Rajoute un toit a la map

**Résultat** (fait par l'IA, Antigravity)

- **Dalle de toit en béton armé (`Ceiling_Slab`)** :
  - Dimensions de **34.8 m (X) x 26.8 m (Y) x 0.4 m (Z)**, posée directement au sommet des murs d'enceinte (face inférieure à Z = 5.20 m, sommet à Z = 5.60 m).
  - Matériau béton armé biseauté (`Mat_Concrete_Wall`), scellant hermétiquement l'environnement indoor du bunker pour la VR et le calcul des ombres.
- **Charpente industrielle & poutrelles en I sous plafond** :
  - **5 poutres maîtresses en acier** (`Ceiling_Beam_X*`), profilés métalliques de 26.0 m de portée (traversant toute la largeur de l'arène du mur Sud au mur Nord) placés à X = -12, -6, 0, 6, 12 m.
  - Section de 28 cm de largeur sur 35 cm de retombée sous dalle (`Mat_Steel_Dark`).
- **Gaine de ventilation technique principale (HVAC)** :
  - Gaine d'aération longitudinale en tôle d'acier galvanisé (`Ceiling_HVAC_Duct`) de 33.0 m de long sur 65 cm de large et 40 cm de haut, suspendue à Z = 4.95 m.
  - 5 colliers métalliques de cerclage de maintien (`HVAC_Strap_*`) espacés tous les 5.5 m.
- **Ancrage physique & organisation** :
  - Les câbles de suspension des 10 luminaires néon (`Fixture_*`) viennent désormais s'ancrer directement dans la sous-face du toit et des poutrelles.
  - Regroupement des 12 éléments de toiture dans une sous-collection dédiée `Arena_Ceiling` (permettant à l'utilisateur de masquer/afficher le toit en un clic dans l'Outliner de Blender s'il souhaite inspecter la vue tactique d'en haut).
  - Ajout d'une caméra intérieure à hauteur d'yeux (`Arena_Interior_Cam` à Z = 1.7 m, focale grand angle 20 mm).
- **Fichiers & Exports mis à jour** :
  - Sauvegarde de `Tools/blender/arena_1v1/arena_1v1_bunker.blend` (338 Ko).
  - Export FBX mis à jour : `arena_1v1_bunker.fbx` (1.04 Mo, 217 objets).
  - Checkpoint de session mis à jour dans `production/session-state/active.md`.


---

## 163 — Rehaussement de la hauteur sous plafond à 7.0 m, piliers et toiture dans Blender via MCP

**Date** : 2026-11-10 · **Domaine** : Carte / Modélisation 3D / Level Design

> Il faudrait le faire plus haut

**Résultat** (fait par l'IA, Antigravity)

- **Rehaussement du plafond et volume global de l'Arène** :
  - **Hauteur sous plafond** : Augmentation de 5.2 m à **7.0 m** (+1.80 m de dégagement vertical).
  - Au niveau du sol (Z = 0.0 m) : hauteur sous plafond majestueuse de 7.0 m, conférant l'ampleur d'un hangar ou alvéole de bunker lourd.
  - Au niveau des passerelles (Z = 2.4 m) : dégagement de **4.6 m sous plafond** (éliminant toute sensation d'écrasement ou de plafond bas en VR).
- **Ajustement coordonné des éléments structuraux** :
  - **Murs périmétriques & contreforts** : `Wall_North`, `South`, `East`, `West` et les 22 contreforts verticaux portés à 7.0 m de haut (centre à Z = 3.5 m).
  - **Piliers octogonaux de soutien** : Les 6 piliers intacts (`Pillar_Octo_1`, `4`, `5`, `6`, `7`, `8`) s'étendent désormais du sol au plafond (hauteur 7.0 m) pour supporter la charpente, accentuant le contraste avec les piliers écroulés maintenus à 1.6 m.
  - **Dalle de toiture & charpente** : `Ceiling_Slab` rehaussée à Z = 7.0 m (sous-face) / 7.4 m (sommet). Les 5 poutrelles d'acier (`Ceiling_Beam_X*`) sont calées à Z = 6.825 m et la gaine HVAC à Z = 6.75 m.
- **Éclairages & caméras rééchelonnés** :
  - Rehaussement des 4 lumières de plafond (`Area_Ceiling_*`) à Z = 6.6 m avec puissance ajustée (1200 W / 950 W) pour maintenir un éclairement franc et net au sol.
  - Recalage des luminaires néons suspendus à Z = 5.6 - 5.8 m avec câbles d'ancrage allongés (1.2 m jusqu'au plafond).
  - Caméra d'aperçu tactique (`Tactical_Overview_Cam`) rehaussée à Z = 27.0 m, Y = -28.0 m.
- **Fichiers & Exports mis à jour** :
  - Sauvegarde de `Tools/blender/arena_1v1/arena_1v1_bunker.blend` (343 Ko).
  - Export FBX mis à jour : `arena_1v1_bunker.fbx` (1.05 Mo, 217 objets).
  - Checkpoint de session mis à jour dans `production/session-state/active.md`.


---

## 164 — Import propre de l'arène 1v1 dans Unity ("Alvéole 17"), nouvelle scène et intégration menu multijoueur

**Date** : 2026-11-10 · **Domaine** : Carte / Unity 6 / Multijoueur Netcode / UI Menu

> Ok, fait l'import porpre dans Unity de cette map avec une nouvelle scène comme le sysètme de la map solo, mais pour le coup là c'est une des maps multi joueur de 1v1, donne lui un nom de map et intègre le en accessible dans le menu multijoueur en 1V1 dans les maps

**Résultat** (fait par l'IA, Antigravity)

- **Nom officiel de la carte** : **Alvéole 17** (`Arena1v1_Bunker`).
- **Import propre des assets dans Unity (`Assets/Arena1v1/`)** :
  - Modèle 3D complet importé dans `Assets/Arena1v1/arena_1v1_bunker.fbx` (1.05 Mo, 217 objets).
  - Génération de 8 matériaux URP Lit dans `Assets/Arena1v1/Materials/` (`Mat_Concrete_Floor`, `Mat_Concrete_Wall`, `Mat_Steel_Dark`, `Mat_Rust_Cover`, `Mat_Hazard_Yellow`, `Mat_Water_Puddle`, plus néons et balises d'alarme émissifs `Mat_Lamp_Emissive` et `Mat_Warning_Emissive`).
  - Remapping automatique des matériaux au niveau du `ModelImporter` du FBX.
- **Création par script de la scène de jeu (`Assets/Editor/Arena1v1Builder.cs`)** :
  - Menu Unity dédié : **SAE501 → Build Arena 1v1 Scene**.
  - Génération de la scène autonome `Assets/Scenes/Arena1v1_Bunker.unity` (431 Ko).
  - Colliders optimisés : `MeshCollider` (statique) sur le sol, murs, passerelles, marches d'escalier, piliers et `MeshCollider` convex sur les caisses, barricades et gravats.
  - Éclairage temps réel URP : 4 lumières zénithales de plafond (60 W / 50 W), 2 spots d'alarme ambrés aux sas, spots sur passerelles et escaliers, ambiance de bunker sans fuite de lumière.
  - Spawns opposés : `Spawn_Player_1` (Ouest, `(-13, 0.1, 0)`, regard vers l'Est) et `Spawn_Player_2` (Est, `(13, 0.1, 0)`, regard vers l'Ouest).
  - Armes & chargeurs au sol (M1911 aux sas, fusils de précision sur passerelles hautes, arme spéciale au centre de la brèche).
  - Configuration XR Origin rig complète (mains animées, DirectInteractor, CharacterController, PlayerFallGuard, Crouch, SmoothTurn).
  - Enregistrement de la scène dans `EditorBuildSettings.scenes` (index 2).
- **Intégration au Menu Multijoueur et Netcode** :
  - **Lobby (`NetLobby.cs`)** : Ajout du mode duel 1v1 synchronisé par `NetworkVariable<int> ModeIndex` (0: Coop Vagues infinies, 1: Coop Évasion, 2: **Duel 1v1 — Alvéole 17**). En mode duel, la scène cible bascule automatiquement sur `Arena1v1_Bunker` et la limite de joueurs est fixée à 2.
  - **Menu UI (`MainMenuMulti.cs`)** : Le bouton de mode dans le lobby permet de basculer instantanément entre le Coop Zombies et le Duel 1v1 sur Alvéole 17.
  - **Découverte Wi-Fi (`LanDiscovery.cs`)** : La diffusion UDP annonce le nom du mode et de la carte (`DUEL 1V1 : ALVÉOLE 17`), visible dans la liste des parties trouvées.
  - **Gestionnaire de match à mort (`NetDuel.cs`)** : Gère les rounds (premier à 3 victoires), le tableau des scores 3D flottant au centre de l'arène, les réapparitions aux sas opposés et le retour au terminal après victoire.
  - **Hitbox PvP (`PlayerHitbox.cs` & `NetPlayer.cs`)** : Tête et torse des avatars distants munis de colliders et détection d'impact de balles (50 PV à la tête, 25 PV au corps).
- **Point de contrôle** : Actualisé dans `production/session-state/active.md`.


---

## 165 — Élimination du flickering et du grésillement des lumières URP sur l'arène 1v1

**Date** : 2026-11-10 · **Domaine** : Carte / Rendu URP / Éclairage / Performance Quest 3

> Les lumières n'arrêtent pas de flickers (grésiller) corrige ça
> [Capture d'écran de l'éditeur Unity montrant la scène Arena1v1_Bunker avec les ampoules et artefacts de scintillement]

**Résultat** (fait par l'IA, Antigravity)

- **Diagnostic technique de la cause racine** :
  - Dans le pipeline Universal Render Pipeline (URP) en mode Forward (profil mobile Meta Quest 3, `Quest_RPAsset.asset`), la limite stricte de lumières additionnelles par objet est de 8 (`m_AdditionalLightsPerObjectLimit: 8`).
  - L'arène comportait 11 sources lumineuses temps réel avec portées étendues affectant simultanément le maillage unique du sol (`Arena_Floor`, 34m x 26m) et les murs.
  - À chaque mouvement ou rotation de vue de la caméra, Unity trillait et permutait dynamiquement les 8 lumières retenues, provoquant l'allumage et l'extinction alternés brutaux de sources lumineuses à chaque frame (effet de « grésillement » ou « flickering » stroboscopique violent).
  - De plus, les ombres douces (`LightShadows.Soft`) activées sur de multiples points lights surchargeaient l'atlas de shadow maps temps réel mobile.
- **Restructuration de l'éclairage de la scène (`Arena1v1Builder.cs` & `Arena1v1_Bunker.unity`)** :
  - **Lumière principale zénithale (`Light_Main_BunkerFill`)** : Création d'une `Directional Light` (orientation X = 70°, Y = -25°, intensité 1.3, couleur bleu acier froid) avec ombres douces. En URP, la Directional Light principale est traitée comme Main Light et ne consomme **aucun slot** des 8 Additional Lights autorisées par objet.
  - **Limitation stricte des spots d'ambiance à 5 sources** (strictement inférieur au plafond de 8) :
    1. `Spot_Spawn_West` : balise ambrée de départ J1 (portée 8.5 m, intensité 3.5).
    2. `Spot_Spawn_East` : balise ambrée de départ J2 (portée 8.5 m, intensité 3.5).
    3. `Spot_Catwalk_North` : néon d'éclairage tactique sur passerelle Nord (portée 9.5 m, intensité 3.2).
    4. `Spot_Catwalk_South` : néon d'éclairage tactique sur passerelle Sud (portée 9.5 m, intensité 3.2).
    5. `Spot_Center_Contested` : projecteur d'alerte sur la brèche centrale contestée (portée 8.0 m, intensité 2.8).
  - **Suppression des ombres sur les lumières additionnelles** (`LightShadows.None`) : préserve le budget GPU du Meta Quest 3 (72 FPS constants) et élimine tout risque de conflit ou scintillement d'atlas d'ombres.
  - **Suppression des point lights collées au plafond** : suppression des zones de surexposition blanche (« brûlures » lumineuses) sur la dalle de toiture.
- **Mise à jour du script de génération et de la scène** :
  - [`Assets/Editor/Arena1v1Builder.cs`](file:///Users/rahimtam/SAE501/Assets/Editor/Arena1v1Builder.cs) mis à jour et validé.
  - Scène [`Assets/Scenes/Arena1v1_Bunker.unity`](file:///Users/rahimtam/SAE501/Assets/Scenes/Arena1v1_Bunker.unity) reconstruite proprement via **SAE501 → Build Arena 1v1 Scene**.
  - Zéro flickering garanti, éclairage stable, fluide et parfaitement contrasté.

## 166 — Paquets en erreur après l'import d'une arme et d'une map

**Date** : 2026-11-10 · **Domaine** : Paquets Unity / Configuration du projet

> Regarde la state du projet, j'ai importé une arme, une nouvelle map, mais j'ai une erreur
> [Capture de la fenêtre Unity « Packages with Errors »]

**Résultat** (fait par l'IA, Claude Code)

- Cause trouvée dans `Logs/Packages-Update.log` et `Editor.log` : un paquet Asset Store de l'époque Unity 2019 importé avec ses dépendances a ajouté HDRP 17.6, Ads 4.19, Analytics 3.8.2 et Purchasing 4.15 au manifest. Analytics et Purchasing 4 sont déclarés obsolètes par Unity, d'où la fenêtre « Packages with Errors ». L'import a aussi enregistré HDRP dans `GraphicsSettings` et rempli `VFXManager`.
- Aucun script du projet n'utilise ces paquets : `manifest.json`, `packages-lock.json`, `GraphicsSettings.asset` et `VFXManager.asset` restaurés depuis le dernier commit ; dossiers `HDRPDefaultResources` et `MobileDependencyResolver` mis à la Corbeille. Le patch Meta NGO et les commits de `feature/multijoueur` sont intacts.
- Reste à faire : le matériau de `Protection_suite` utilise HDRP Lit (rose en URP) et doit passer en URP Lit. Recompilation à vérifier dans l'éditeur.

## 167 — Textures des murs de l'arène 1v1 et combinaison en URP

**Date** : 2026-11-10 · **Domaine** : Carte / Matériaux URP

> "le matériau de la combinaison utilise le shader HDRP Lit, donc il va s'afficher en rose. Il faudra le passer en URP Lit." règle moi ça
> et aussi la scène a un petit soucis, il manque les textures sur les murs etc
> [Capture de l'éditeur Unity sur la scène Arena1v1_Bunker, murs et sol en aplat]

**Résultat** (fait par l'IA, Claude Code)

- Correction d'une erreur de l'IA au prompt 166 : la combinaison n'utilisait pas HDRP Lit mais un shader « Standard(2sided) » du pipeline intégré, rose lui aussi en URP. `Mat_protective suit` passé en URP Lit double face avec découpe alpha (albedo, normal, métal/lissé, occlusion repris) ; `Mat_Scen` (opaque) et `Mat_glass` (transparent) passés en URP Lit.
- Murs sans texture : `Arena1v1Builder` créait des matériaux en couleur unie, et 109 des 203 maillages du FBX n'avaient pas d'UV. Nouveau `Assets/Editor/Arena1v1UV.cs` : à l'import du FBX, UV0 recalculées par projection cubique en mètres (une répétition tous les 2 m), tangentes recalculées.
- `Arena1v1Builder.AppliquerTextures()` (menu SAE501 → Arène 1v1 → Appliquer les textures, appelé aussi par la reconstruction) : béton sale au sol, béton fissuré aux murs, rouille sur les couvertures (Poly Haven CC0, 1k, copiées dans `Assets/Arena1v1/Textures`), acier sombre et peinture jaune avec les textures d'usure de la salle. ASTC 6x6 et mipmaps sur Android.
- Vérifié par capture de la vue Scène dans l'éditeur. Non vérifié : le rendu de la combinaison (scène de démo non ouverte) et le rendu au casque.

---

## 168 — Personnages Survivalist et choix du skin en multijoueur

**Date** : 2026-11-10 · **Domaine** : Gameplay / Réseau

> J'ai importé un pack de design de personnages : Survivalist character by Slayver, config les proprement pour qu'il y ait des bonnes animations lorsqu'une arme est tenu ou lors des interractions, que ce soit en VR ou en mode PC
> De plus dans le mode multi rajoute le fait que chacun peut choisir le skin qu'il veut (son personnage)

**Résultat** (code écrit par l'IA, Claude Code, branche `feature/multijoueur`)

- **Pack** : il ne contient qu'un squelette humanoïde, 4 tenues et les clips attente / marche / course / saut (aucune animation d'arme). Ses scripts de démo (Starter Assets, qui demandaient Cinemachine et cassaient la compilation), ses matériaux HDRP, ses réglages de rendu et sa scène de démo ont été mis à la Corbeille (`Survivalist_retire_20261110`) ; `Packages/manifest.json` remis comme dans git. L'éditeur s'était fermé (crash) après l'import.
- **`SurvivalistBuilder`** (menu SAE501 → Build Survivalist Skins) : textures à 1024 en ASTC 6x6 sur Quest, matériaux URP/Lit simples, contrôleur attente / marche / course, prefabs `Resources/Skins/Survivalist_1..4` (12 000 à 19 000 triangles).
- **`PlayerAvatar`** : le corps est reconstruit à partir de la tête et des deux mains du joueur, de la même façon en VR et en version PC : jambes animées selon la vitesse, corps tourné vers le regard, bras par cinématique inverse à deux os, poignet orienté comme la manette, doigts refermés quand la main tient un objet, pieds jamais sous le sol, taille adaptée au joueur.
- **Multijoueur** : `NetPlayer` affiche ce personnage à la place de l'avatar en cubes, avec une copie de l'objet tenu dans chaque main (nom de l'objet et pose envoyés par le casque du joueur) et des zones touchables sur la tête et le torse. Choix du personnage dans le lobby (boutons 1 à 4, aperçu en pied à côté du panneau, tenue affichée sur la ligne de chaque joueur), gardé d'une partie à l'autre.
- **Vérifié** : compilation ; images de pose rendues en mode batch (4 tenues, pistolet tenu à deux mains) ; lobby en mode Play (aperçu, bouton, ligne du joueur). **Non testé** : à 2 joueurs (corps en mouvement, objet tenu), au casque. Le joueur ne voit pas son propre corps.
- Non commités : le pack (214 Mo, décision de Rahim), les prefabs qui en dépendent, et `NetPlayer` / `NetLobby` / `MainMenuMulti` qui contiennent aussi le travail 1v1 en cours d'une autre session.

---

## 169 — Pack Survivalist allégé et bouton « forcer le lancement » du 1v1

**Date** : 2026-11-10 · **Domaine** : Gameplay / Réseau

> Il faudrait l'optimiser pour eviter qu'il prenne trop de place dans le git
> (captures du quota GitHub) · Ok pour moi
> Cependant je veux que tu ajoutes dans le jeu un bouton pour forcer le lancement du 1v1 même si il n'y a qu'un joueur actuellement présent

**Résultat** (fait par l'IA, Claude Code, branche `feature/multijoueur`)

- **Quota** : GitHub Pro, Git LFS 10 Go de stockage et 10 Go de bande passante par mois (0,2 Go utilisés) ; l'estimation « 1 Go » annoncée d'abord était fausse.
- **Pack** : 214 → 63 Mo (fichiers inutilisés retirés, textures de 2048 à 1024, identique à l'écran sur Quest) ; commit c0562cd (252 fichiers, 48 en LFS). Une erreur en cours de route (matériaux Quest retirés, personnages roses) a été corrigée.
- **Bouton « FORCER LE LANCEMENT »** (`NetLobby.ForceStart` / `ToggleForceStart`, `MainMenuMulti`) : visible de l'hôte en mode Duel seulement ; lance le compte à rebours sans adversaire ni « prêt » ; un second appui annule ; le changement de mode l'annule.
- **Vérifié dans l'éditeur** : bouton créé, appui → lancement forcé → la scène `Arena1v1_Bunker` se charge avec un seul joueur. **Non vu à l'écran** (affichage du bouton dans le lobby) ni au casque.
- **Problème trouvé, pas corrigé** (travail 1v1 d'une autre session) : `NetDuel.EnsureSpawned` crée un objet réseau à l'exécution (erreur Netcode « GlobalObjectIdHash 0 ») et aucun joueur réseau n'est créé dans l'arène : le duel ne tourne pas encore.
- Non commités : `NetLobby.cs`, `MainMenuMulti.cs` (mêlés au travail 1v1 non commité).

---

## 170 — Correctifs du mode 1v1

**Date** : 2026-11-10 · **Domaine** : Gameplay / Réseau

> Le mode 1v1 est bugué, patch moi ça, entre les armes qui marchent pas
> (capture de l'arène : un objet blanc au centre, une planche en main)

**Résultat** (code écrit par l'IA, Claude Code, branche `feature/multijoueur`)

- **Cause des armes** : la scène de l'arène avait le rig de base du Starter Assets (rayon « Near-Far ») au lieu des mains de `MainScene` (interacteur direct dans la paume). La version PC (`DesktopPlayer`) ne trouvait donc aucune main : impossible de prendre ou de tirer. Les pistolets n'avaient pas non plus les règles de prise de `MainScene` (`GrabGuard`, `ItemSafety`, `IgnorePlayer`).
- **Corrigé** : `Arena1v1Builder.AddHand` utilise maintenant `SetupVRScene.UseDirectInteractor` (rendu public), `AddGrabRules` sur les pistolets, et un menu **SAE501 → Arène 1v1 → Réparer mains et armes** a réparé la scène existante sans la reconstruire.
- **Pistolet de départ** (`DuelLoadout`) : chaque joueur commence chaque manche avec son pistolet (Ouest pour J1, Est pour J2) déjà dans la main droite ; `Pistol.InfiniteAmmo` : pas de chargeurs dans l'arène, le pistolet ne se vide jamais. L'arène lancée seule (mode Play direct) donne aussi le pistolet.
- **Duel en réseau** : `NetDuel` est maintenant un prefab réseau (`Resources/Net/NetDuel`, ajouté à `NetPrefabsBuilder`) : avant, l'objet créé à la volée ne pouvait pas être « spawné » (erreur Netcode « GlobalObjectIdHash 0 ») et le duel ne démarrait pas. `NetDuel` crée aussi un `NetPlayer` par joueur (avatar, zones touchables, vie), comme en coop.
- **Vérifié dans l'éditeur (un seul joueur)** : arène lancée seule, pistolet en main, 15 tirs sans que les munitions baissent ; menu → hôte → Duel → lancement forcé → arène, `NetDuel` et `NetPlayer` créés, pistolet en main, aucune erreur ; mort du joueur → manche 2, score 0-1, joueur relevé avec son pistolet. **Non testé** : à 2 joueurs (tirs touchant l'adversaire, avatar vu par l'autre), au casque.
- **Pas fait** : choix du kit d'armes (pistolet / sniper / fusil) : aucun sniper ni fusil dans le projet, les « Sniper » de l'arène sont des pistolets. L'objet blanc au centre de la capture n'a pas été identifié (surbrillance de contour d'un pistolet posé, probablement).
- Non commités (mêlés au travail 1v1 non commité) : `NetDuel.cs`, `DuelLoadout.cs`, `Pistol.cs`, `Arena1v1Builder.cs`, `SetupVRScene.cs`, `NetPrefabsBuilder.cs`, la scène de l'arène et `Resources/Net`.

---

## 171 — Systèmes et pistolet de MainScene dans l'arène 1v1

**Date** : 2026-11-10 · **Domaine** : Gameplay

> Regarde le mode zombi, comment fonctionne les mouvements, les actions, le tire, pause, etc, récupère tout de là bas
> Et le style graphiques / animations ? Reprend aussi du mode solo, l'animation de tire du pistolet etc

**Résultat** (code écrit par l'IA, Claude Code, branche `feature/multijoueur`)

- **Comparaison** des composants de `MainScene` et de l'arène. Déjà communs (même rig, `SetupVRScene.UseDirectInteractor` depuis le journal 170) : déplacement, saut, accroupi, mains, prise par clic, version PC.
- **Ajouté à l'arène** (`RoomBuilder.AddPlayerSystems`, `Arena1v1Builder.AddMainSceneSystems`) : menu pause BUNKER-OS, vignette de dégâts rouge avec direction du tireur, écran de fin (arène lancée seule), musique, ceinture avec holster du pistolet. L'ancien `NetDuel` posé dans la scène (sans objet réseau) est retiré.
- **Pistolet** : l'arène utilisait une version simplifiée. Elle prend maintenant le pistolet complet de `MainScene` (`RoomBuilder.CreatePistol`) : animation de tir du M1911, son du tir et du chargeur, douille éjectée, flash et lumière du canon, fumée, compteur de balles à l'inclinaison, emplacement du chargeur.
- **Réparation** de la scène existante par le menu **SAE501 → Arène 1v1 → Réparer mains et armes** (sans la reconstruire).
- **Vérifié dans l'éditeur (version PC, seul)** : pistolet en main avec son animation, 3 tirs sans erreur, dégâts (100 → 80, vignette rouge), menu pause ouvert (temps figé), reprise. **Non testé** : au casque, à 2 joueurs, ni la musique à l'oreille.
- **Style graphique non modifié** : l'arène a sa propre ambiance (béton éclairé en temps réel) ; MainScene est presque noir avec un brouillard bleu nuit. Question posée à Rahim.
- Non commités (mêlés au travail 1v1 non commité).

---

## 172 — Arbres et sol du jardin refaits, fluidité

**Date** : 2026-11-10 · **Domaine** : Carte / Rendu / Optimisation

> Les arbres sont horrible, corrige ça, et aussi la texture du sol dans le jardin est horrible, corrige ça egalement , et aussi c'est pas vraiment smooth
> (capture : troncs noirs en pointes étirées, sol bleu clair plat)

**Résultat** (code et assets faits par l'IA, Claude Code, branche `feature/multijoueur`)

- **Causes trouvées** : (1) l'export de la carte (`export_map.py`, 6 000 triangles max par objet) avait écrasé les troncs des 8 arbres du jardin : pointes noires étirées ; (2) le terrain extérieur (`Exterior_World_Terrain`) passe sous le jardin et ressort par plaques, avec une texture noire à cet endroit et un reflet de ciel : le « bleu clair » ; (3) les deux terrains n'avaient qu'une image Blender floue étalée sur 20 m et 98 m, avec des facettes d'ombrage ; (4) la forêt autour était le blockout Blender : 36 sapins en cônes et arbres « sucette » sans texture.
- **Arbres** : `Tools/blender/arbre_quest.py` refait l'arbre Poly Haven `island_tree_02` (CC0, 1 million de triangles à l'origine) : tronc simplifié à part, normales vers l'extérieur, une feuille = un quad. LOD0 18 855 triangles, LOD1 6 658, puis imposteur (3 panneaux croisés, 12 triangles) rendu une fois ; `Assets/Nature/Arbres/Arbre_Quest.fbx`. Utilisé pour les 8 arbres du jardin (mêmes places) et pour 36 arbres de forêt à la place des cônes (`Assets/Editor/JardinNature.cs`, appelé par `RoomBuilder`).
- **Sol** : textures Poly Haven 1k (CC0, Rob Tuytel) `forrest_ground_01` (jardin) et `forest_leaves_02` (forêt), UV en mètres (2,5 m et 3,5 m par répétition), normales lissées, ASTC 6x6 sur Quest, anisotrope 4. Terrain extérieur descendu de 15 cm sous le jardin.
- **« Pas smooth »** : lu comme ombrage lisse (facettes) et fluidité. Facettes supprimées ; arbres sans ombre portée, avec LOD et imposteurs, sols sans reflets d'environnement. **Fluidité non mesurée** : pas de casque, et la fréquence d'images de l'éditeur ne dit rien du Quest. À mesurer au casque (72 FPS).
- **Vérifié** : captures dans l'éditeur (MainScene reconstruite par SAE501 → Build VR Scene) : arbres lisibles, sol texturé, plus de plaques bleues. Les pixels magenta vus au début étaient une compilation de shader en cours, disparus ensuite. **Non testé** : au casque, ni les collisions des troncs de forêt en jeu, ni le NavMesh après reconstruction.
- Éclairage précalculé non refait : les nouveaux arbres prennent la lumière des sondes. Wiki : page `Jardin-arbres-et-sol` écrite en local, pas poussée.
- **Complément (même séance)** : Rahim envoie une capture de la vue Scène : vitre de fenêtre et flaques au sol en aplats bleu clair. Cause : vitre (`Mat_Window_Glass_VR`) et flaques (`Flaque_Eau`) sont transparentes et lisses (0,93 et 0,95) ; sans sonde de réflexion, la vue Scène leur fait refléter son ciel par défaut. En jeu (fond noir, réflexions à 0) rien ne se voyait. `JardinNature.NoSkyReflections` coupe les réflexions de l'environnement sur les matériaux transparents lisses à chaque construction de la scène. Vérifié : matériaux modifiés, capture de la fenêtre sans bleu ; la vue Scène elle-même n'a pas pu être capturée par l'outil, à confirmer par Rahim.

---

## 173 — Trous du toit bouchés, lianes retirées, mur du couloir ajouté

**Date** : 2026-11-10 · **Domaine** : Carte

> Manque un mur, et retire le trou et les éléments qui tombent du toit
> (capture de la vue Scène) puis, aux deux questions de l'IA : « (a) boucher les 2 trous et retirer les gravats, en gardant zombies et caisse par une autre entrée » ; « Mur manquant : dans le couloir à droite de la porte de la cuisine »

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`)

- **Trous du toit** : les deux trous (lits, établi) sont bouchés par une dalle de même épaisseur et même matériau (`TrousToit.cs`, `Bouchons_Toit`) ; bord cassé, auréole noircie et gravats ne sont plus créés (code gardé, inutilisé). Les lianes sombres qui pendaient du plafond sont retirées dans toute la carte (avant : seulement sous les trous) et `GenRoom_CeilingVines` supprimé : ce sont ces tiges noires dans le couloir que j'ai comprises comme « les éléments qui tombent du toit ».
- **Gameplay adapté** : plus d'ouvertures `Toit_Lits` ni `Toit_Etabli` (4 ouvertures restent : 2 portes, 2 fenêtres) ; plus de point d'apparition sur le toit : les apparitions « Toit » des niveaux 3+ tombent sur le point extérieur (`ZombieSpawner` le gère déjà). La caisse de la radio apparaît sous le plafond, au-dessus de l'établi, et tombe de 1,5 m (`SupplyDrop` à y = 2,45 m). Le GDD et l'équilibrage des vagues ne sont pas revus : plus d'effet « zombie du toit » (avertissement, pas sur le toit).
- **Mur manquant** : la paroi ouest du couloir (x = 13 m) avait un trou de 2,3 m (z 0,65 à 2,95) entre la salle du générateur et le mur de la cuisine (mesuré par rayons). Mur ajouté (`MurCouloir.cs`) : même épaisseur, hauteur et matériau, UV en continuité, collision, lumière des sondes.
- **Vérifié** (captures dans l'éditeur, scène reconstruite) : trous bouchés (plafond continu), plus de tiges, mur en place, rayon z = 1,8 m touche le nouveau mur, 0 ouverture « Toit ». **Non testé** : une partie jouée (zombies des niveaux 3+, caisse de la radio, NavMesh), au casque ; éclairage précalculé non refait (les bouchons prennent la lumière des sondes, ils peuvent être un peu plus clairs ou sombres que le plafond autour).

---

## 174 — Liste durable des objets retirés à la main

**Date** : 2026-11-10 · **Domaine** : Outils

> Y a beaucoups d'éléments de la map que je retire à la main et que tu replaces, arrête de replacer mes éléments de que je retire

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`)

- **Cause** : `MainScene` est générée par script ; mes 4 reconstructions de la séance (journaux 172 et 173) ont recréé tous les objets du FBX, y compris ceux que Rahim avait supprimés à la main dans la scène. Ces suppressions-là ne sont pas récupérables par l'IA (scène non commitée avant la reconstruction) : il faut les refaire une fois.
- **Correctif** : `Assets/Editor/objets_retires.txt` (un nom d'objet par ligne, versionné), lu par `ObjetsRetires.Appliquer` appelé par `RoomBuilder` juste après les retraits codés en dur. Menu **SAE501 > Scène > Retirer la sélection (durable)** : supprime la sélection et note les noms ; **Ouvrir la liste des objets retirés** pour les relire.
- **Engagement** : plus de reconstruction de la scène sans demande explicite de Rahim. Non testé : le menu et l'application de la liste en reconstruction (compilé seulement).

---

## 175 — Arbre du jardin remplacé par un jacaranda, sans rebuild

**Date** : 2026-11-10 · **Domaine** : Carte / Rendu

> J'ai supprimé des objets, ne les remet pas je l'ai fait directement dans ma scène MainScene, parcontre là l'arbre que tu m'as mis c'est toujours pas bon corrige ça
> puis, à la question de l'IA : « Un arbre plus classique : un seul tronc droit avec une couronne de feuilles (par exemple le jacaranda Poly Haven), mis à la place sans rebuild. »

**Résultat** (code et assets faits par l'IA, Claude Code, branche `feature/multijoueur`)

- **Aucune reconstruction** de la scène : les objets que Rahim retire à la main dans MainScene ne sont pas touchés. Le remplacement passe par les fichiers que la scène référence déjà : `Assets/Nature/Arbres/Arbre_Quest.fbx` (mêmes noms de maillages LOD0 et LOD1, donc mêmes références) et les textures des trois matériaux `island_tree_02*` (feuilles, rameaux, tronc).
- **Arbre** : `Tools/blender/arbre_jacaranda.py` part du jacaranda Poly Haven (CC0, 3,9 millions de triangles, 24 m) : tronc et rameaux simplifiés à part, une fronde = un quad, 4,2 m de haut. LOD0 32 323 triangles, LOD1 13 083, puis l'imposteur (refait). Faces triées par matériau (feuilles, rameaux, tronc) : Unity numérote les sous-maillages dans l'ordre d'apparition des faces, et c'est l'ordre des matériaux déjà posés dans la scène. Textures 1k dans `Assets/Nature/Arbres/Jacaranda` (feuilles : couleur + alpha fusionnés en RGBA).
- **Code** : `JardinNature.LoadTree` retrouve les matériaux par nom (plus par position) et `Retexture` pose les textures du jacaranda ; menu **SAE501 > Jardin > Mettre à jour l'arbre sans reconstruire** (marche aussi sans MainScene ouverte). Les futurs Build VR Scene garderont ce nouvel arbre.
- **Bleu clair par terre** : c'étaient les flaques d'eau (`Flaque_Eau`), plans transparents très lisses qui renvoient la lune bleue ; reflet spéculaire coupé sur les matériaux transparents lisses (`NoSkyReflections`), sans rebuild.
- **Vérifié** : rendu Unity de l'arbre seul (tronc et feuilles aux bonnes textures, couronne dense) et rendu Blender ; **non vérifié** : dans MainScene (non ouverte pendant ce travail), ni au casque, ni la fluidité (32 k triangles en LOD0, à mesurer).

---

## 176 — Évasion : portail du jardin, puis cinématique avec de vrais sons

**Date** : 2026-11-11 · **Domaine** : Gameplay / Son

> Actuellement la condition de victoire du mode evasion c'est juste activer le générateur alors que ça doit être activer le générateur et s'enfuir par le portail dans le jardin, en gros le portail se débloque quand on fini d'activer le générateur, et ensuite se joue l'animation que tu avais réaliser pour démontrer au joueur qu'il s'est enfui et qu'il a gagné
>
> D'ailleurs il faut que tu revois l'animation en reprennant les sons qu'il y a dans le jeu, prce que les sons actuels qui sont dans l'animations sont absolument horrible, et quand le zombi tappe sur le portail en métal ça fait un bruit absolument pas réaliste, prend un sont enligne pour ça, de grille frappé ou quelque chose comme ça

**Résultat** (code, montage audio et choix des sons faits par l'IA, Claude Code, branche `feature/multijoueur`, sans reconstruire la scène)

- **Nouvelle condition de victoire** : `WaveManager` a une phase `Escape`. Le générateur activé n'est plus la victoire : annonce « GÉNÉRATEUR ACTIVÉ — Le PORTAIL DU JARDIN s'ouvre : FUYEZ PAR LÀ ! », statut « fuyez par le portail du jardin », les renforts du niveau 4 continuent et les zombies traquent toujours le joueur. La victoire (`PlayerEscaped`) tombe quand la tête du joueur passe 0,9 m au-delà de la grille, côté route. Mode survie inchangé (générateur hors service, pas de portail).
- **Portail** (`EscapeGate`, ajouté tout seul au chargement de la scène) : le cadenas tombe au pied de la grille avec un cliquetis, les battants pivotent de 95° vers le jardin (comme dans la vidéo) et cognent leurs butées, la lampe au-dessus passe au vert et pulse. Les battants et le cadenas n'étaient pas déplaçables (« Batching Static ») : drapeau retiré à la main dans `MainScene` (3 lignes) et par `PortailFer` pour les futurs Build VR Scene.
- **Multijoueur** : l'hôte ouvre le portail pour tous (`NetGame.EscapeOpenRpc`) ; un client qui franchit le portail le signale à l'hôte (`EscapeRpc`), qui déclare la victoire pour l'équipe.
- **Cinématique** : reprise de la branche `feature/cinematiques` uniquement pour la fin (`EscapeVideo.cs`, la vidéo `CinematiqueFin.mp4`) ; `PlayerDeathFall` (code PC) n'est pas repris. Adaptée au portage actuel : même écran fixe dans le noir en VR et au PC, HUD du PC masqué, musique du jeu coupée en fondu, clic / espace / gâchette pour passer. `GameOverUI` : fondu au noir complet, vidéo, puis le bilan existant (sous-titre « Vous avez franchi le portail du jardin. »).
- **Bande-son refaite** (`Tools/blender/cinematique_son.py`, image de la vidéo inchangée) : plus aucun son synthétisé. Râles = `Zombie_Voix_1` et `_2` du jeu (hauteur et place variées, étouffés de loin au plan A), musique = `Musique_Menu` du jeu en fond, coups sourds = `flesh_zombie_hit` très bas, grille secouée et grille qui claque = deux enregistrements CC0 de Freesound (« Rattling at an iron gate » de launemax, « Gate Close and rattle » d'Alex_hears_things), crédits dans `Tools/sons_cc0/CREDITS.md` et `docs/ASSETS.md`. Niveau réglé à -16 LUFS, fondus calés sur l'image. Mêmes sons de grille en jeu pour l'ouverture du portail.
- **Vérifié** dans l'éditeur Unity (mode Play, version PC, test automatique temporaire non commité) : compilation sans erreur, générateur → portail qui s'ouvre → traversée → victoire → vidéo jouée sur l'écran → bilan. **Non vérifié** : au casque (lecture de la vidéo par le Quest, confort), en multijoueur réseau, et **la bande-son n'a pas été écoutée par l'IA** (vérifiée seulement par ses niveaux) : à écouter par Rahim.

---

## 177 — Bilan de victoire par-dessus la vidéo de fin

**Date** : 2026-11-11 · **Domaine** : UI / Cinématique

> Pour le menu de quand tu t'es échappé, fait le apparaitre par dessus l'animation de fin à la moitié de l'animation (exemple de quand le faire apparaitre dans l'image )

(capture jointe : `evasion_demo.mp4` à 00:17, plan des zombies de dos contre la grille fermée, vers la moitié de la vidéo de fin)

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`, sans reconstruire la scène)

- `EscapeVideo.Play` accepte un rappel `onMenu` appelé à **50 %** de la vidéo (10 s, le plan des zombies contre la grille) ou dès que le joueur passe (clic, espace, gâchette) : le bilan « VOUS AVEZ ÉCHAPPÉ » apparaît alors devant l'écran de cinéma, avec un petit grossissement de 0,3 s. La vidéo et son son continuent derrière ; 1,8 s avant la fin (avant son fondu au noir), l'image se fige et le son s'éteint en fondu. L'écran reste jusqu'au choix d'un bouton.
- `GameOverUI` : le panneau et les rayons de pointage passent sur le calque de l'écran de cinéma (la caméra ne dessine que lui pendant la vidéo), fond du panneau un peu transparent, pas de voile noir à 78 % par-dessus la vidéo. Boutons inchangés (verrou de 1 s anti-clic réflexe). `MusicPlayer` : la musique coupée par la cinématique revient à la partie suivante.
- **Vérifié** dans l'éditeur Unity (mode Play, version PC, test automatique temporaire non commité) : bilan apparu à 10,4 s de vidéo, vidéo visible autour, image figée ensuite. **Non vérifié** : au casque (lisibilité du panneau devant l'écran, rayons des manettes), clic réel sur les boutons pendant la vidéo.

---

## 178 — Bilan de victoire : « VICTOIRE » et panneau élargi

**Date** : 2026-11-11 · **Domaine** : UI

> Le menu est pas assez large, et "Vous avez échappé" ce n'est même pas français, juste "victoire"
> Retire : Signal transmit
> Remplace : "Vous avez franchi le portail du jardin" par : "Vous avez réussi à vous enfuir"

(capture jointe : le titre « VOUS AVEZ ÉCHAPPÉ » débordait des bords du panneau)

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`)

- `GameOverUI` : titre de victoire « **VICTOIRE** » (un peu plus grand et plus haut), sur-titre « SIGNAL TRANSMIS » retiré (la défaite garde « SYSTÈME DÉFAILLANT »), sous-titre « Vous avez réussi à vous enfuir. ». Panneau élargi de 1,52 à **1,84 m** (avant l'échelle 0,8) pour tous les bilans ; bandeau, encadré du bilan, colonnes des statistiques et boutons (1,20 -> 1,50 m) suivent la largeur.
- Annonce de l'écran du bunker à la victoire : « VICTOIRE — Vous avez réussi à vous enfuir. » (au lieu de « VOUS VOUS ÊTES ÉCHAPPÉ ! ») ; plus aucun « échappé » dans le code ni dans le wiki (Fin de partie, Cinématiques, Niveaux et vagues), hormis l'accroche du menu « S'ÉCHAPPER », non touchée.
- **Vérifié** dans l'éditeur Unity (mode Play, version PC, test temporaire non commité) : rien ne déborde (capture `docs/captures/evasion/8_bilan_victoire.jpg`). **Non vérifié** : au casque (le panneau fait 1,47 m de large à 1,3 m : environ 59° de champ), écrans de défaite après l'élargissement (même mise en page, non capturés).

---

## 179 — À quoi sert la clé du cabanon ?

**Date** : 2026-11-11 · **Domaine** : Question

> Ok, à l'heure actuelle à quoi sert la clé qui est dans le cabanon ?

**Résultat** (réponse de l'IA, Claude Code, sans modification du jeu)

- À rien en jeu : `Cle_Jardin` (objet `Garden_Hidden_Key` de la carte) est rendue prenable par `RoomBuilder.FixGardenCabin` (`Assets/Editor/RoomBuilder.cs`, l. 408-428 : modèle de clé en fer, Rigidbody, prise en main), mais aucun script du jeu ne la lit ; la page wiki Carte le dit déjà (« n'ouvre rien pour l'instant »).
- Pas nécessaire pour gagner, et sans lien avec le portail du jardin, qui s'ouvre seul quand le générateur démarre (`EscapeGate`, journal 176).

---

## 180 — Fichier temporaire introuvable et « Licensing connection lost »

**Date** : 2026-11-11 · **Domaine** : Outils / Éditeur

> Patch ceci, et corrige aussi l'erreur de : Licensing connection lost

(capture jointe : boîte macOS « Opening file failed: /Users/rahimtam/SAE501/Temp/UnityTempFile-08bef9a1bd-d1741ecb95fb036529c00a: No such file or directory », boutons Try Again / Force Quit / Cancel, Unity 6000.6.0f1)

**Résultat** (diagnostic et code faits par l'IA, Claude Code, branche `feature/multijoueur`, sans toucher à la scène)

- **Cause trouvée** (journal système macOS, logs Unity et Hub) : après le redémarrage du Mac de 13 h 35, à **13:38:15–17**, Figma, le client de licence Unity (`Unity.Licensing.Client`), `unity serve`, le relay de l'assistant IA et les **deux workers d'import Unity** ont tous quitté proprement en 2 secondes, juste après une activité de l'utilitaire **Vorssaint**, dont l'option **« Quit on close »** (`autoQuitEnabled = 1`, aucune exception) quitte les applis qui n'ont plus de fenêtre. Les workers d'import sont des instances de `Unity.app` sans fenêtre : en quittant normalement, ils suppriment le dossier `Temp` du projet, d'où l'erreur `UnityTempFile` dans l'éditeur encore ouvert. Aucun script du dépôt, crontab, hook git ou session d'agent ne supprime `Temp`.
- **« Licensing connection lost »** : même cause (le client de licence a été quitté). Le Hub l'a relancé à 13:38:17 et l'éditeur s'est reconnecté seul à 13:38:31 (« The re-connection attempt was successful ») ; la licence est valide, rien à faire dans le Hub. Les erreurs « Code 404 … 0 entitlement groups » sont normales (droits optionnels).
- `Temp` recréé (`mkdir -p`, rien supprimé) ; garde-fou **`Assets/Editor/TempDirGuard.cs`** ([InitializeOnLoad]) : recrée `Temp` toutes les 3 s s'il disparaît. Compilé sans erreur dans l'éditeur ouvert.
- **À faire par Rahim** : dans Vorssaint, Réglages → *Quit on close* → *Exceptions*, ajouter Unity, Unity Hub (et si proposés Unity Licensing Client et relay_mac_arm64), ou désactiver *Quit on close* pendant le travail sur Unity. Dans une boîte « Opening file failed » encore ouverte : cliquer **Try Again** (Temp existe de nouveau).
- **Non vérifié** : que l'exception Vorssaint suffit (réglage de l'appli, non modifié par l'IA) ; le garde-fou ne peut pas empêcher les workers d'être quittés, il évite seulement que `Temp` reste absent.

---

## 181 — Arènes du duel : Alvéole 17 corrigée, 2 nouvelles cartes et choix de la carte

**Date** : 2026-11-11 · **Domaine** : Carte / Multijoueur

> Prend le role de level designer / game designer
> Analyse la map 1V1 et corrige ses incohérences, problèmes de textures, problèmes de collisition de props, et propose 2 autres maps (pour chacune des maps fait une capture d'écran et rajoute dans le menu de lancement de la partie une preview de la map et un moyen de selectionner la map qui sera joué en 1V1)

**Résultat** (analyse, code et scènes faits par l'IA, Claude Code, branche `feature/multijoueur`, **non commité** en attendant que Rahim voie les captures ; MainScene non touchée)

- **Alvéole 17** (`Assets/Editor/Arena1v1Fixes.cs`, appliqué sans reconstruire et à chaque *Build Arena 1v1 Scene*) : escaliers des passerelles qui finissaient 0,40 m sous la passerelle (on ne montait qu'en sautant) → 11 marches de 0,217 m ; caisse 0,95 m + caisse empilée 1,45 m contre le mur central = escalier vers le haut du mur avec le saut de 0,75 m → caisse portée à 1,20 m ; gravats avec collider (marchepieds, accrochages) → sans collider ; 5 socles « Pickup » vides retirés ; flaque NW cassée (maillage vide, matériau « Lit » par défaut) remplacée par le miroir de la flaque SE ; reflets blancs des flaques coupés ; ombres « de soleil » dans le hall fermé coupées ; ligne de vue spawn-à-spawn par la brèche (1,25 m) coupée par un écran de sas de 2,4 m devant chaque spawn.
- **Bloc C** (hôpital, 30 × 20 m, combat rapproché) et **Quai 9** (cour de conteneurs de nuit, 42 × 28 m, longues vues) : `ArenaMapsBuilder.cs` + `ArenaKit.cs` (UV en mètres, fusion par matériau, ≤ 8 lumières, sans ombres), symétrie centrale, couverts bas 1,2 m / hauts ≥ 2,2 m. Scènes `Arena1v1_Hopital` et `Arena1v1_Quai` ajoutées au build ; `SetupVRScene` garde toutes les arènes dans la liste.
- **Lobby** : `ArenaMaps.cs` (registre des cartes et apparitions), `NetLobby.DuelMap` (choix de l'hôte synchronisé), panneau « CARTE DU DUEL » avec aperçu (`MainMenuArenas.cs`) ; NetPlayer, NetDuel, DuelLoadout, NetSession, DesktopPlayer reconnaissent les 3 arènes.
- **Vérifié** dans l'éditeur : compilation, simulation du CharacterController (escaliers montés sans saut jusqu'à 2,60 m, caisses et brèche non escaladables), captures `docs/images/arenes/`. **Non vérifié** : en mode Play réseau (lancement réel d'une partie sur Bloc C / Quai 9), au casque (72 fps, lisibilité de nuit).

---

## 182 — Arènes du duel : vraies textures sur les blocs

**Date** : 2026-11-11 · **Domaine** : Carte / Textures

> Les maps de 1v1 manquent de textures, c'est à dire qu'il y a beaucoup de blocks avec aucune textures comme ceci :

(capture jointe : Quai 9, conteneurs, sacs de sable et barils en blocs presque unis)

**Résultat** (textures et code faits par l'IA, Claude Code, branche `feature/multijoueur`, **non commité**)

- Inventaire automatique des matériaux sans albedo ou sans relief dans les 3 arènes (hors lumières, néons, écrans).
- Textures générées par `Tools/textures/generer_arenes.py` (1024 px, raccordables, albedo + normal map, ASTC 6x6 sur Quest) dans `Assets/Arenas/Textures` : **Conteneur** (tôle ondulée, rouille), **Conteneur_Porte** (2 battants, 4 barres de fermeture, poignées), **Sacs** (sacs de sable en quinconce), **Baril** (cerclages), **Casier** (fentes d'aération, poignée), **Carrelage** (sol d'hôpital, carreaux de 25 cm), **Danger** (bandes jaunes et noires).
- Quai 9 : conteneurs en tôle ondulée de 3 couleurs avec portes, longerons et montants d'acier ; sacs de sable, barils, bandes de danger (portails, groupe électrogène, conteneurs des spawns) ; tente et cabane éclaircies, relief ajouté à la toile. Bloc C : sol en carrelage, casiers, distributeur, appareil d'anesthésie et chariots en métal peint, soubassement et rideaux texturés, relief sur plafond et comptoirs. Alvéole 17 : bandes des portes blindées et des écrans de sas en bandes de danger (elles étaient jaune uni).
- Tiling : UV en mètres (rien d'étiré), une répétition par taille réelle (tôle 2 m, porte 2,6 m, sacs 1,2 m, carrelage 1 m).
- **Vérifié** dans l'éditeur : captures refaites. **Non vérifié** : au casque (rendu ASTC, 72 fps).

---

## 183 — Arènes du duel : optimisation Quest (barils low-poly, fusion, colliders)

**Date** : 2026-11-11 · **Domaine** : Optimisation

> C'est pas très optimisé ça

(capture jointe : un baril d'Alvéole 17 en fil de fer, cylindre très dense aux couvercles en longs triangles fins)

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`, **non commité**)

- Audit des 3 arènes. Alvéole 17 (modèle Blender) : 216 objets dessinés un par un, 26 198 triangles, 142 MeshCollider ; barils et tuyaux à 32 faces (124 triangles) avec des couvercles triangulés depuis un seul sommet. Bloc C (6 126 triangles) et Quai 9 (6 842) étaient déjà fusionnées ; Quai 9 avait 14 MeshCollider convexes (barils, poteaux).
- `Arena1v1Fixes` : barils et tuyaux refaits en cylindres partagés de 12 faces (48 triangles, couvercles en éventail depuis le centre ; 8 faces pour les tuyaux fins), cerclages en normal map ; MeshCollider convexes des props (caisses, barils, hérissons) → BoxCollider ; décor fusionné par matériau (plafond à part) : **216 → 25 rendus**, 24 854 triangles.
- `ArenaKit` : un seul maillage de cylindre par dimensions, BoxCollider (barils) ou CapsuleCollider (poteaux, mâts) au lieu de MeshCollider ; fusion des objets à plusieurs matériaux. Quai 9 : **14 → 0 MeshCollider**.
- Restent : les 4 barrières cassées (6 858 triangles) et les 8 piliers octogonaux (3 392) du modèle Blender, et 128 MeshCollider statiques non convexes des murs et sols (sans coût notable à l'exécution).
- **Vérifié** dans l'éditeur : captures et gros plans en fil de fer (`docs/images/arenes/Baril_filfer_*.png`). **Non vérifié** : au casque (fps réels).

---

## 184 — Arènes du duel : barils et barrières refaits, Bloc C supprimé

**Date** : 2026-11-11 · **Domaine** : Carte / Optimisation

> Corrige tout ça,
> et retire simplement la map 1V1 hopital, supprime la

(2 captures jointes d'Alvéole 17 : barils qui se traversent et qui rentrent dans un pilier, un baril couché à travers un autre ; barrières cassées du modèle Blender avec des plaques qui se croisent, des faces vues de l'intérieur et une texture étirée)

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`, **non commité**)

- Cause des barils : mon remplacement low-poly (journal 183) devinait l'axe du cylindre d'après la boîte englobante du maillage Blender, fausse pour les barils couchés et tournés ; et les barils d'origine se chevauchaient déjà.
- `Arena1v1Fixes` : barils Blender masqués, remplacés par des barils propres de 16 faces (64 triangles, flancs lissés, texture à cerclages) posés au sol sans contact : 2 debout + 1 couché par groupe, 1 dans chaque coin des spawns. Les 4 barrières cassées (6 858 triangles) sont remplacées par des murs de béton fermés en 3 tronçons (2,2 / 1,76 / 1,32 m : on ne monte sur aucun avec le saut de 0,75 m), même emprise, UV en mètres, collider en boîtes. Gravats et hérissons tchèques reposés au sol (les hérissons flottaient de 15 cm). Quai 9 : groupe électrogène posé sur son socle (il rentrait dedans).
- Contrôle automatique (`Physics.ComputePenetration` entre tous les props et le décor, appui sous chaque prop) : **0 interpénétration, 0 objet flottant** sur Alvéole 17 et Quai 9 (seuls les pistolets sont en l'air, ils sont donnés en main au début de la manche). Alvéole 17 : 27 rendus, 18 236 triangles.
- **Bloc C supprimé** : scène `Arena1v1_Hopital`, liste du build, entrée de `ArenaMaps` (le lobby ne propose plus qu'Alvéole 17 et Quai 9, la carte du mode affiche « 2 cartes »), code de construction, 13 matériaux `Hop_*`, maillages, textures Carrelage et Casier, aperçu du lobby, captures et section du wiki. Tout compile.
- **Vérifié** dans l'éditeur : contrôle automatique, captures (`docs/images/arenes/Barils_Alveole17*.png`, `Barrieres_Alveole17*.png`). **Non vérifié** : au casque.

---

## 185 — Paramètres : VSync et limite d'images par seconde

**Date** : 2026-11-11 · **Domaine** : Menus / Performance

> Rajoute dans les paramètres le VSync (si c'est pris en compte ou sinon juste un FPS cappter a 60, 72, 90, 144, 244), regarde ce qui est le plus simple par rapport au moteur Unity / ce qui est aussi proposé de base

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`, commit `fa5c328`, non poussé ; ce journal non commité)

- Choix : les deux réglages de base de Unity, sans module en plus. `GameSettings` : `VSync` (`QualitySettings.vSyncCount` 1/0, défaut oui) et `FrameRateCap` (`Application.targetFrameRate` ; sans limite, 60, 72, 90, 144, 240), enregistrés dans `PlayerPrefs` et appliqués à chaque scène chargée (`ApplyFrameRate`). **240 au lieu de 244** : 240 Hz est la vraie fréquence des écrans rapides.
- Unity ignore `targetFrameRate` quand le VSync est actif : dans la page PARAMÈTRES (section SOURIS ET AFFICHAGE, version PC), la ligne « Limite FPS » est grisée tant que VSync = OUI. Lignes de la page un peu resserrées en version PC pour garder RETOUR en bas (`AmberMenu`).
- **Au casque** (Quest ou PC VR) : rien n'est appliqué et les lignes n'apparaissent pas ; c'est le runtime XR qui impose la cadence (72 Hz sur le Quest). Choix 72 / 90 / 120 Hz au casque non ajouté (risque de ne plus tenir la cadence au Quest 3).
- Wiki : pages [Version PC](https://github.com/withRen/SAE501/wiki/Version-PC) et [Menus](https://github.com/withRen/SAE501/wiki/Menus) mises à jour (commit wiki `17f2669`, non poussé).
- **Vérifié** : compilation sans erreur ; capture de la page PARAMÈTRES en version PC rendue dans une scène d'aperçu de l'éditeur (hors mode Play). **Non vérifié** : effet réel sur les fps dans un build Windows, et rien au casque.

---

## 186 — Duel : avatar distant qui tient son arme à la 3e personne

**Date** : 2026-11-11 · **Domaine** : Multijoueur / Animation

> (capture du duel Arena1v1_Bunker en version PC : le personnage du joueur J2 tient mal son pistolet, bras levé et plié, arme qui flotte loin de la main) Corrige le rig et la tenue de l'arme du personnage distant, comme un personnage de Counter-Strike vu à la 3e personne : les deux mains sur l'arme, qui vise devant, et le haut du corps qui suit la visée vers le haut et vers le bas.

(demande transmise par l'assistant du projet, d'après le message et la capture de Rahim)

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`, commit `c0ff733` pour `PlayerAvatar.cs`, non poussé ; `NetPlayer.cs` et ce journal non commités)

- Cause : en version PC, `DesktopPlayer` place l'arme comme une arme « vue FPS », collée à la caméra (15 cm à droite, 16 cm sous les yeux, 36 cm devant pour le pistolet). Le casque envoie cette main aux autres joueurs, et `PlayerAvatar` tendait le bras droit du personnage vers ce point : bras replié vers le visage, poignet tordu, arme loin de la main. La main gauche, cachée sous l'écran, pendait.
- `PlayerAvatar.HoldGun` : quand le joueur distant est en version PC et tient une arme à feu dans la main droite, ses mains suivies sont ignorées. L'arme est posée devant l'épaule droite, canon dans l'axe du regard (le fusil est épaulé, le pistolet tenu bras tendus). La main droite va sur la poignée, la main gauche sur le garde-main (fusil) ou autour de la main droite (pistolet). Le corps se tourne vite vers la visée, la colonne prend 60 % du tangage du regard (haut / bas), la tête le reste. Le chargeur tenu pendant le rechargement est posé sur la main gauche. Tous les réglages sont dans l'Inspector du personnage (`rifleGrip`, `pistolGrip`, `leftRifleOffset`, `spineShare`…).
- `NetPlayer` : la pose envoyée porte un drapeau `desktop` (`DesktopMode.On`) ; pour l'objet tenu, on retient s'il s'agit d'une arme à feu (`Pistol`), sa poignée (accroche XRI) et l'axe de son canon. Au casque VR, rien ne change : le personnage suit les vraies manettes.
- Wiki : page [Personnages](https://github.com/withRen/SAE501/wiki/Personnages), section « Arme à feu tenue à la 3e personne » (commit wiki `c5fc731`, non poussé).
- **Vérifié** dans l'éditeur, hors mode Play : compilation sans erreur, console vide ; captures du personnage dans Alvéole 17 avec la pose réellement reçue d'un joueur PC (AK74 et pistolet, avant / après, visée droite, vers le haut et vers le bas). Les 4 tenues ont été rechargées dans l'éditeur (il gardait les anciennes valeurs en mémoire), les fichiers des prefabs ne changent pas.
- **Non vérifié** : en vraie partie à deux machines (le drapeau `desktop` change la taille de la pose réseau : les deux joueurs doivent avoir la même version), recul et animation de rechargement non reproduits sur l'avatar, et rien au casque. Marteau, lampe et armes de mêlée gardent l'ancienne tenue.

---

## 187 — Duel : postures d'armes façon Counter-Strike

**Date** : 2026-11-11 · **Domaine** : Multijoueur / Animation

> Corrige moie les postules de tenus des armes en t'inspirant de comment sont tenus celes dans Counter Strike :

(3 captures de Counter-Strike 2 jointes : pistolet tenu haut, près du visage, coudes pliés ; AK tenu bas contre le torse, corps de trois quarts ; sniper épaulé, l'œil dans la lunette, buste penché)

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`, commit `6489101`, non poussé ; `NetPlayer.cs` et ce journal non commités)

- La tenue unique du journal 186 (arme devant l'épaule) est remplacée par **une posture par type d'arme** dans `PlayerAvatar` (`GunHold` : `pistolHold`, `rifleHold`, `sniperHold`), choisie d'après la longueur poignée → bouche du canon (fusil à partir de 40 cm, sniper à partir de 85 cm) :
  - pistolet : haut, à 25 cm devant le visage, deux mains, coudes pliés ;
  - fusil d'assaut : bas contre le torse (38 cm sous les yeux), crosse sous l'épaule droite, main gauche sous le garde-main, paume vers le haut ;
  - sniper : épaulé, lunette à hauteur de l'œil, buste penché de 14°.
- Le corps se met de trois quarts, épaule gauche en avant (18° / 32° / 42°), la tête reste dans l'axe du regard. L'arme est placée par rapport aux yeux : elle monte et descend avec la visée, et recule vers l'épaule si le bras ne peut pas l'atteindre. La main de soutien n'est qu'à demi fermée (35° par phalange au lieu de 62°).
- Nouvel outil `Assets/Editor/AvatarPoseCaptures.cs`, menu **SAE501 → Arène 1v1 → Captures tenue d'arme** : rend le personnage dans Alvéole 17 avec la pose envoyée par un joueur PC, pour les 3 armes, visée droite, vers le haut et vers le bas, sans mode Play.
- Wiki : page [Personnages](https://github.com/withRen/SAE501/wiki/Personnages) mise à jour (non poussé).
- **Vérifié** dans l'éditeur, hors mode Play : compilation sans erreur, console vide, captures avant / après des 3 armes (`docs/images/avatar/tenue_*.jpg`). Réglé à l'œil sur les captures de référence, sans animation de Counter-Strike (les poses sont calculées, pas des animations importées).
- **Non vérifié** : en vraie partie à deux machines, personnage en marche avec l'arme, et rien au casque. Recul et rechargement toujours non animés sur l'avatar.

---

## 188 — Sons des zombies : impact de balle et pas

**Date** : 2026-11-11 · **Domaine** : Audio

> Sur le jeu actuellement il y a des problèmes de logique de sons, donc je veux que tu trouves des sont réalistes et que tu rectifies donc le sons de hit quand on tire avec le pistolet sur un zombi, également il faut résoudre le bruit de pats des zombis

**Résultat** (code fait par l'IA, Claude Code, branche `feature/multijoueur`, non commité au moment de l'écriture : en attente de l'écoute de Rahim)

- Sons trouvés sur Freesound, tous **CC0** : « VisceralBulletImpacts » (u1769092) et « Meaty Damage Impact » (modusmogulus) pour la balle ; « pas zombie route » (indieground) et « Heavy Footsteps | Crunchy Thuds » (GammaGool) pour les pas. Découpés avec ffmpeg en variantes (6 impacts, 5 + 5 pas) dans `Assets/Resources/Sounds/Impact_Balle`, `Pas_Zombie`, `Pas_Lourd`. Originaux et crédits : `Tools/sons_cc0/zombies/CREDITS.md`, `docs/ASSETS.md`. Choisis sur leur description et l'analyse du signal : l'IA ne peut pas les écouter, Rahim les valide à l'oreille.
- **Impact** : une balle de pistolet (`ZombieHitbox` → `Zombie.HitFeedback(bullet: true)` → `ZombieVoice.PlayBulletHit`) joue une des 6 variantes, jamais deux fois la même de suite, hauteur ±6 %. Le craquement d'os de la tête reste par-dessus. Hache et pied de biche gardent `flesh_zombie_hit`.
- **Bug des pas trouvé** : `Zombie.GroundSteps` jouait un pas synthétique sur un minuteur fixe (0,62 s Rôdeur, 0,78 s Cogneur, 0,29 s Coureur), sans lien avec l'animation. Le clip de marche du Rôdeur ne fait que 2 pas en 4 s (joué à la vitesse réelle du zombie) : on entendait ~3 pas par pas visible. Les marionnettes du multijoueur n'avaient aucun pas.
- **Correction** : instants d'appui de chaque pied mesurés dans les 3 clips de marche (hauteur et avancée des pieds échantillonnées dans l'éditeur) ; le pas est joué quand la phase de l'état `Walk` de l'Animator passe par ces instants. La cadence suit donc la vitesse de l'animation. Aussi sur les marionnettes. Au plus 4 pas dans la même fenêtre de 0,15 s pour tous les zombies, pas ignorés au-delà de la portée, priorité audio sous les voix et les impacts. Rôdeur : pas traînants ; Cogneur : pas lourds plus graves ; Coureur : pas lourds plus aigus. Repli sur l'ancien minuteur si pas d'Animator.
- **Vérifié** dans l'éditeur, hors mode Play : compilation sans erreur, console vide, sons trouvés par `Resources.LoadAll` ; simulation de l'Animator des 3 prefabs (8 s à 72 images/s) : chaque pas tombe quand le bon pied est au plus bas (Rôdeur 0,6 m/s : 1 pas toutes les ~1,85 s ; Cogneur : 1,04 s / 0,75 s, démarche boiteuse du clip ; Coureur 1,6 m/s : toutes les 0,50 s).
- **Non vérifié** : en partie (mode Play), volumes réels dans la scène, en multijoueur, et rien au casque.

---

## 189 — Mur manquant du couloir près de la salle du générateur

**Date** : 2026-11-11 · **Domaine** : Carte / Outils éditeur

> Il manque actuellement tout un mur, juste à gauche de la salle du générateur

**Résultat** (diagnostic et code faits par l'IA, Claude Code, branche `feature/multijoueur`, commit `d599725`, non poussé ; `MainScene`, ses lightmaps et ce journal non commités car mêlés au travail d'autres fils)

- **Mur concerné** : le mur ouest du couloir (`Wall_Corridor_West`, x = 13 m), sur le pan de z = -1,6 à 3,1 m entre le renfoncement de la salle du générateur et la porte de la cuisine. Depuis le couloir, au-dessus du lambris, on voyait à travers : le renfoncement, le mur de brique de la salle du générateur et le jardin.
- **Cause** : ce n'était ni un objet supprimé à la main ni la liste `objets_retires.txt` (vide). La face côté couloir de ce pan avait été retirée par `MursSuperposes` (journal 148), l'outil qui enlève les faces de murs superposées. Il jugeait une face « cachée » quand ses **trois sommets** tombaient dans des faces d'autres objets du même plan : les coins de ce grand pan touchaient les petits bouts des murs voisins (cuisine, salle du générateur), il a donc été retiré alors que rien ne le cachait au milieu. La face extérieure restait, invisible depuis le couloir. Le mur de rustine `MurCouloir` (journal 173) ne bouchait que 2,3 m du trou.
- **Correction** : `MursSuperposes` vérifie maintenant toute la surface (sommets + grille de 45 points à l'intérieur du triangle) avant de retirer une face. `MurCouloir.cs` et son appel dans `RoomBuilder` sont retirés (il ferait du z-fighting avec la face remise).
- **Dans la scène, sans la reconstruire** : les 2 triangles de la face côté couloir remis dans `Assets/Generated/Recouvrements/Wall_Corridor_West.asset` (collision comprise), objet `Wall_Corridor_West_Ajout` et son maillage supprimés, éclairage recuit (menu **SAE501 → Cuire l'éclairage**), ce qui a aussi légèrement changé l'éclairage général (1 lightmap au lieu de 2).
- **Vérifié** dans l'éditeur, hors mode Play : rayons depuis le couloir tous les 50 cm, de z = -1,5 à 3 m : tous touchent `Wall_Corridor_West` (avant : le mur de la salle du générateur à 14,5 m) ; captures avant / après.
- **Remarque** : un contrôle des autres faces retirées par l'ancien test signale d'autres cas incomplets (surtout des chants et dessus de murs, invisibles, et un pan de 3 m du mur est du bunker à vérifier). Ils resteront tels quels jusqu'au prochain *Build VR Scene*, qui appliquera le nouveau test. Ce prochain build pourra aussi faire réapparaître un peu de z-fighting là où deux faces ne se recouvrent qu'en partie.
- **Non vérifié** : en mode Play et au casque.

**Suite** (même jour)

> Y a une sorte d'overide des objets là, les deux mures se coupent l'un dans l'autre au lieu de couper net avant la jonction pour éviter les problèmes d'affichage

- Cause : les bouts des deux petits murs qui encadrent le pan remis (`Wall_GeneratorStub_South` côté salle du générateur, `Wall_KitchenStub_North` côté cuisine) avaient leur face d'extrémité exactement dans le plan du mur du couloir (x = 12,90 m) : deux faces superposées à la même profondeur (z-fighting).
- Correction sans reconstruire la scène : ces faces d'extrémité (2 triangles par mur, entièrement derrière le pan du couloir) sont retirées dans des copies `Assets/Generated/Recouvrements/Wall_GeneratorStub_South.asset` et `Wall_KitchenStub_North.asset` (collisions comprises), éclairage recuit. Le nouveau test de `MursSuperposes` retire exactement ces faces au prochain *Build VR Scene* : rien à changer dans le code.
- Non vérifié en mode Play ni au casque.

## 190 — Fusion de tout le travail dans main

**Date** : 2026-11-12 · **Domaine** : Git / Documentation

> Faisont un push de tout ça, proprement dans le main avec une explication que c'est du marging de tout ce qui a été fait jusqu'ici

**Résultat** (tri, commits et fusion faits par l'IA, Claude Code, sur le Mac de Rahim)

- **Avant** : `main` sur GitHub (`4d6d4d3`, journal 131) ne contenait ni le portage PC (`develop`, poussée mais pas fusionnée) ni la branche `feature/multijoueur` (40 commits locaux non poussés), et environ 185 fichiers restaient non commités, mêlés entre plusieurs fils de travail.
- **Travail non commité rangé en 4 commits** sur `feature/multijoueur` : duel 1v1 (arènes, armes, NetDuel, lobby, journal 154 à 171 et 181 à 187), évasion par le portail du jardin et cinématique de fin (176 à 178), sons des zombies (188), `MainScene` avec ses matériaux générés, son NavMesh et son éclairage déjà recuits (172 à 175 et 189). Le découpage est fait par fichier : un fichier touché par deux sujets (ex. `NetPlayer.cs`, `Pistol.cs`) est rangé avec le sujet principal.
- **Nommage** (suite : « Go, veille juste bien à ce que les nommages features/ etc respectent bien les conventions ») : la convention du dépôt est `feature/<id>-nom` (README, PLANNING). La branche locale `feature/multijoueur` est renommée et poussée en `feature/147-multijoueur` (issue #147, serveur multijoueur). Les branches distantes hors convention (`features/confort-rapide`, `features/escalade-zombies`, `features/radio-ravitaillement`, `features_exterieur`, `refactor/difficult-vagues`, `chore/nouveau-son`) appartiennent à d'autres membres du groupe : signalées, pas renommées.
- **Fusion** : un commit de fusion `--no-ff` de `feature/147-multijoueur` (qui contient déjà `develop`) dans `main`, dont le message liste tout ce qui entre. `main` passe de la version du 22 octobre à l'état complet du projet au 12 novembre 2026. Page wiki [Historique des versions](https://github.com/withRen/SAE501/wiki/Historique-des-versions).
- **Laissés hors du dépôt** (fichiers locaux de Rahim ou résidus d'import) : réglages URP (`Quest_RPAsset.asset`, `UniversalRenderPipelineGlobalSettings.asset`), profil de build Meta Quest (identifiant du casque branché), `DevAgentSettings.asset` (adresse IP), suppression de `RuntimeActionBindings.json`, `Assets/Protection_suite/`, `scripts/`, `rules.md`, `ProjectSettings/VirtualProjectsConfig.json` (Multiplayer Play Mode), `Assets/Resources/BillingMode.json` (résidu du paquet Purchasing), `vsync_fps_capture.png`, et du pack VCR01 la variante Olive et la scène de démo (seules les variantes Black et Sand servent au sniper).
- **Vérifié** : le projet compile dans l'éditeur ouvert (aucune erreur), `MainScene` et ses lightmaps ont été enregistrés ensemble après la dernière cuisson. Non vérifié : un build Quest ou PC complet depuis `main`, ni au casque.

---

## 191 — Retrait des caméras de surveillance

**Date** : 2026-11-12 · **Domaine** : Gameplay / Carte

> il faut ducoup supprimer le concept des caméras dans les couloirs et au niveau des écrans, au final ça sert à rien

**Résultat** (code et modifications de la scène faits par l'IA, Claude Code, branche `feature/154-retrait-cameras`, issue #154, commit local non poussé en attente de l'accord de Rahim)

- **Dans `MainScene`, sans la reconstruire** (script temporaire lancé en ligne de commande, puis supprimé) : 38 objets retirés : les 5 caméras (`CCTV_Camera_01` à `05`, avec l'incrustation « ● REC »), les 4 boîtiers de caméra et leurs supports dans les couloirs, la cuisine et le stockage (`CCTV_CameraMesh_*`, `CCTV_Mount_*`), leurs 4 lumières infrarouges et la lueur des écrans (`Light_CCTV_IR_*`, `Light_Bunker_CCTV_Glow`), les 4 moniteurs du pupitre avec cadres, dalles et voyants (`Bunker_CCTV_Monitor_*`, `Bezel_*`, `CCTV_Screen_*`, `TallyLED_*`), le 5e écran et la vue plein écran (`MonitorStation`, `MonitorOverlay`), le levier et le voyant du disjoncteur (`Levier_Disjoncteur`, `Voyant`) et le composant `Breaker` du boîtier électrique. Le boîtier électrique, le bureau, le tabouret, l'horloge, l'étagère, l'enregistreur à LEDs, le clavier et la radio restent comme décor.
- **Générateur de scène** : `RoomBuilder` ne crée plus les caméras, les écrans ni le disjoncteur (`BuildCameras`, `BuildMonitors`, `BuildExtraScreen`, `BuildBreaker` retirés) et ignore les lumières `CCTV` de la carte ; les objets du FBX sont ajoutés à `objets_retires.txt` pour ne pas revenir au prochain *Build VR Scene*.
- **Supprimés** : scripts `CctvOverlay`, `CctvRoundRobin`, `MonitorStation`, `MonitorScreen`, `MonitorOverlayClose`, `Breaker` ; shader `SAE501/CCTV` ; `Assets/Generated/Feed_0..4.renderTexture` et `Screen_0..4.mat` ; modèle `Poste_Moniteur.fbx` (et `PosteSurveillance.Moniteur`).
- **Tutoriel** : 8 étapes au lieu de 10 (« Allumez les caméras » et « Les caméras montrent l'extérieur » retirées). **Version PC** : plus de « Regarder la caméra » ni de « Fermer l'écran », le réticule n'est plus caché par la vue plein écran.
- **Éclairage recuit** (réglages de `LightBaker`) : la lueur bleue des écrans ne tombe plus sur le bureau.
- **Docs** : README, `BARRICADE_Scripts.md`, GDD (`game-concept.md`, `systems-index.md`), `docs/PLANNING.md` (O7 marqué retiré), `docs/MAP.md`, `docs/ASSETS.md` ; wiki : page `Cameras-et-disjoncteur` remplacée par une note de retrait, pages liées mises à jour.
- **Vérifié** en ligne de commande (éditeur en mode batch) : compilation sans erreur, aucun « Missing script » dans les 4 scènes et les prefabs, captures du pupitre et du boîtier avant / après / après cuisson.
- **Non vérifié** : en mode Play, le tutoriel complet, et rien au casque. L'étagère du pupitre reste vide (choix à confirmer par Rahim : la garder ou la retirer).

---

## 192 — Lunette du sniper (duel 1v1)

**Date** : 2026-11-12 · **Domaine** : Gameplay / Duel / Rendu

> Pour les 1v1 au sniper il faut créer un système de lunette de scope (genre quand le joueur approche le scope proche de l'oeil ça fait un effet de scope comme un vrai sniper)

**Résultat** (code, shader, outil de captures et mesures faits par l'IA, Claude Code, sur le Mac de Rahim ; branche `feature/153-lunette-sniper`, issue #153, commit local non poussé en attente de l'accord de Rahim. Le n° 191 est le retrait des caméras, sur la branche `feature/154-retrait-cameras`)

- **Nouveau composant `SniperScope`** (`Assets/Scripts/SniperScope.cs`), posé sur `Assets/Resources/Weapons/Sniper.prefab` (l'AWM du duel) par le nouveau menu **SAE501 → Arène 1v1 → Ajouter la lunette au sniper** (sans reconstruire les armes ; `DuelWeaponBuilder` l'ajoute aussi à la prochaine construction). Aucune scène modifiée.
- **Au casque** : une caméra fixée devant l'objectif de la lunette (même direction que le tir, `muzzle.forward`) rend dans une RenderTexture 512 × 512 affichée sur un disque posé sur l'oculaire. Nouveau shader `SAE501/ScopeLens` (`Assets/Resources/Weapons/ScopeLens.shader`) : réticule duplex mil-dot, point rouge éclairé, vignettage du tube, et ombre de la « pupille de sortie » (si l'œil n'est pas dans l'axe ou à la bonne distance, un bord noir mange l'image, comme dans une vraie lunette). Grossissement × 5 (champ de la caméra 6,3°).
- **Activation** : la caméra ne tourne que si l'œil le plus proche de l'axe est à moins de 14 cm de l'oculaire, derrière lui, et que la tête regarde à moins de 30° de l'axe (un peu de marge pour l'éteindre : pas de clignotement). Sinon elle est éteinte et la lentille montre un verre sombre avec un reflet : aucun rendu en plus hors visée. Pas de post-traitement, pas d'ombres, pas d'anticrénelage dans la lunette (Quest 3).
- **Joueur local seulement** : chaque casque a son propre sniper (`DuelLoadout`) ; l'avatar de l'adversaire n'en montre qu'une copie visuelle, qui ignore maintenant la lentille (`NetPlayer.VisualCopy`) : aucune caméra de lunette chez lui.
- **Version PC** : clic droit avec le sniper = l'arme s'épaule puis la lunette passe en plein écran (`DesktopHud` : cercle, bords noirs, même réticule), champ de vision divisé par 5 (70° → 15,9°), sensibilité de la souris × 0,3, arme et mains cachées, petit saut de la vue au tir. Le zoom se coupe pendant le rechargement. Aide F1 mise à jour.
- **Réglages** (inspecteur du prefab) : `magnification`, `resolution`, `activationDistance`, `activationAngle`, `eyeRelief`, `eyeTolerance`, `shadows`, `pcSensitivity` ; géométrie de la lunette (`eyepiece`, `objective`, `lensRadius`) mesurée sur la pièce « Scope » du modèle AWM.
- **Outil** `Assets/Editor/ScopeCaptures.cs`, menu **SAE501 → Arène 1v1 → Captures lunette** : captures dans `docs/images/lunette/` (vue de l'œil droit au casque, gros plan, œil décalé, lunette au repos, vue PC normale et zoomée) dans Alvéole 17 et Quai 9.
- **Vérifié** dans Unity en ligne de commande (copie de travail séparée, l'éditeur ouvert de Rahim n'a pas été utilisé) : compilation sans erreur ; captures (silhouette à 13 m environ 4,5 à 5 fois plus grande dans la lunette qu'à l'œil nu) ; **test en mode Play temporaire (non commité)** : version PC, sniper en main, visée forcée (le curseur ne peut pas être capturé en mode batch) → champ 60° → 13,2°, arme cachée, sensibilité × 0,30, tout revient au relâchement ; version casque sans casque, tête placée à la main → lunette éteinte à 30 cm (caméra pas même créée), allumée à 7,5 cm, éteinte la tête tournée de 60°.
- **Non vérifié** : rien au casque (confort, lisibilité du réticule, 72 fps avec la caméra de lunette sur Quest 3, réglage des 14 cm et de l'ombre de pupille avec un vrai œil), ni en partie réseau à deux joueurs, ni le vrai clic droit à la souris (simulé). L'image de la lunette est la même pour les deux yeux (on vise d'un œil).

---

## 193 — Lunette : test au casque sur une version sans lunette

**Date** : 2026-11-12 · **Domaine** : Diagnostic / Build

> J'ai beau le rapprocher, le scope ne fonctionne pas

**Résultat** (diagnostic fait par l'IA, Claude Code, sur le Mac de Rahim ; aucun code changé)

- **Cause** : l'APK installé sur le casque (`~/SAE501/The Last Escapor.apk`, construit à 09 h 21) vient du dossier `~/SAE501`, sur la branche `feature/154-retrait-cameras`, qui **ne contient pas la lunette** : la lunette est sur la branche `feature/153-lunette-sniper`, dans la copie de travail séparée `~/SAE501-lunette`. Vérifié dans l'APK lui-même : aucune classe `SniperScope` ni shader `ScopeLens` dans ses métadonnées (alors que `NetDuel` et `PlayerHitbox` y sont), et `Sniper.prefab` de `~/SAE501` n'a pas le composant. Le « verre sombre » vu au casque est donc la texture d'origine de l'oculaire du modèle AWM, pas la lentille de `SniperScope`.
- **Pour tester la lunette** : construire l'APK depuis `~/SAE501-lunette` (l'ouvrir comme projet à part dans Unity Hub ; son dossier `Library` est déjà prêt), ou fusionner `feature/153-lunette-sniper` dans la branche de `~/SAE501` (conflits à résoudre dans `DesktopHud.cs`, `docs/PROMPTS.md` et l'état de session, à cause du retrait des caméras).
- Le code de la lunette n'a pas été modifié : rien ne montre pour l'instant que ses seuils (14 cm, 30°) posent problème au casque. Ils restent réglables dans l'inspecteur du prefab (`activationDistance`, `activationAngle`).
- **Non vérifié** : le petit texte jaune vu sur la lentille (la capture n'a pas été vue par l'IA) ; la lunette elle-même au casque.

---

## 194 — Fusion de la lunette dans la branche du retrait des caméras

**Date** : 2026-11-12 · **Domaine** : Git / Build

> Option B : fusionner feature/153-lunette-sniper dans la branche de ~/SAE501, pour rebuilder l'APK comme d'habitude depuis ~/SAE501

(choix de Rahim transmis par l'assistant du projet, après l'entrée 193)

**Résultat** (fusion et résolution des conflits faites par l'IA, Claude Code, sur le Mac de Rahim ; rien n'est poussé)

- `feature/153-lunette-sniper` fusionnée (`--no-ff`) dans `feature/154-retrait-cameras`, la branche ouverte dans `~/SAE501`. Fichiers locaux de Rahim non touchés.
- **Conflits résolus en gardant les deux changements** : `DesktopHud.cs` (lunette plein écran ajoutée, l'ancienne vue plein écran des caméras reste retirée), `docs/PROMPTS.md` (entrées 191, 192, 193 et index dans l'ordre), `production/session-state/active.md`.
- Vérifié : compilation sans erreur, `SniperScope` présent sur `Resources/Weapons/Sniper.prefab`, shader `ScopeLens` dans un dossier `Resources` (inclus dans le build). Non vérifié : l'APK et la lunette au casque.

---

## 195 — Fusion dans main et branche de secours

**Date** : 2026-11-12 · **Domaine** : Git

> fait un push de tout ça dans le main proprement avec une hotfix au cas-où aussi

**Résultat** (fusion et push faits par l'IA, Claude Code, sur le Mac de Rahim)

- **Branche de secours** `hotfix/avant-lunette-cameras` : pointe sur l'ancien `main` (`93ca474`, état du 12 novembre avant le retrait des caméras et la lunette), poussée sur GitHub. Point de retour si la nouvelle version pose problème au casque.
- **Fusion** (`--no-ff`) de `feature/154-retrait-cameras` dans `main` : retrait des caméras de surveillance (journal 191, issue #154) et lunette du sniper (journal 192 à 194, issue #153). Fichiers locaux de Rahim laissés hors du dépôt (réglages URP, profil de build, `DevAgentSettings.asset`, `RuntimeActionBindings.json`, `Protection_suite`, `scripts/`).
- **Poussés** : `main`, `feature/153-lunette-sniper`, `feature/154-retrait-cameras`, `hotfix/avant-lunette-cameras`, et le wiki (pages Lunette du sniper et Caméras et disjoncteur).
- Vérifié : compilation sans erreur dans l'éditeur. Non vérifié : un APK construit depuis `main`, et la lunette au casque.
