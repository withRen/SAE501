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

