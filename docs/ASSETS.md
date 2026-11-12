# Assets du projet — THE LAST ESCAPOR

> Liste des assets trouvés sur l'Asset Store, rangés par usage et reliés aux tâches de [PLANNING.md](PLANNING.md).
> Statut : ⬜ à importer · ✅ importé · ❌ écarté. Prix et licence : **à remplir par la personne qui télécharge** (je ne les ai pas vérifiés).

---

## Version finale : ce qui est dans le jeu (12 novembre 2026)

État de `main` pour la release v1.0.0. Les sections 0 à 7 plus bas sont la **liste de départ** (octobre) : beaucoup d'assets prévus n'ont finalement pas été importés (planches, établi, marteau, textures de l'Asset Store), remplacés par des modèles de l'équipe, de Poly Haven ou faits par script.

### Packs et modèles extérieurs

| Asset | Auteur / source | Licence | Dossier | Taille | Utilisé pour |
|---|---|---|---|---|---|
| Modern Guns : Handgun | Nokobot, Asset Store | Asset Store (EULA) | `Assets/Nokobot` | 9 Mo | Pistolet M1911, chargeur, douille, flash |
| Free Shirtless Zombie | NewPunch, Asset Store | Asset Store (EULA) | `Assets/NewPunch` | 92 Mo | Modèle de base des zombies |
| Zombie Male (AAB) | Asset Store (importé par l'équipe) | Asset Store (EULA), à confirmer | `Assets/ZombieMale_AAB` | 107 Mo | Deuxième modèle de zombie (prefab utilisé par `ZombieBuilder`) |
| Pack « Zombie » (FBX, matériaux, prefabs) | importé par l'équipe | à confirmer | `Assets/Zombie` | 31 Mo | Troisième modèle de zombie (types Cogneur / Coureur) |
| Animations Mixamo | Adobe Mixamo | licence Mixamo (usage dans un jeu permis) | `Assets/Animations` | 20 Mo | Marche, attaque, mort, escalade des fenêtres, chute (zombies) |
| Survivalist (allégé : 4 tenues, textures 1024) | Slayver, Asset Store | Asset Store (EULA) | `Assets/Survivalist` | 55 Mo | Personnages des joueurs en multijoueur |
| Cold War Weapons : AK74M | Asset Store | Asset Store (EULA), à confirmer | `Assets/Cold War Weapons` | 4 Mo | AK74 du duel 1v1 |
| VCR AWM (sniper) | VCR, importé par l'équipe | à confirmer | `Assets/VCR01` | 155 Mo | Sniper AWM et lunette du duel 1v1 |
| Mains animées | exemples du cours / XR Interaction Toolkit | Unity | `Assets/Animated Hands` | 1,5 Mo | Mains du joueur en VR |
| Décor extérieur et ciel | Alban (équipe) | créé par l'équipe | `Assets/Environment` | 2 Mo | Sphère de ciel et décor autour de la carte |

Les packs de l'Asset Store sont commités dans le dépôt (Git LFS) pour que tout le groupe ouvre le même projet. **Le dépôt reste privé** : ils ne sont publiés nulle part en clair (la release ne contient que les exécutables).

### Modèles et textures libres (CC0)

| Asset | Auteur / source | Dossier | Utilisé pour |
|---|---|---|---|
| Textures et modèles de la carte (`dark_wooden_planks`, `distressed_painted_planks`, `decrepit_wallpaper`, `grey_plaster`, `old_wood_floor`, `metal_plate`, `wine_barrel_01`, `wooden_crate_01`, `vintage_oil_lamp`…) | [Poly Haven](https://polyhaven.com) | `Assets/Room/Textures` | Murs, sols, portes, décor |
| Small Plastic Torch | Abiyyu Panggalih, Poly Haven | `Assets/Lampe` | Lampe torche (allégée à 6 000 triangles) |
| Bleach Bottle, Multi Cleaner Bottle | James Ray Cock, Poly Haven | `Assets/Decor` | Bouteilles du décor |
| Key + rusty_metal_04 | Quaternius (Poly Pizza) + Poly Haven | `Assets/Cle` | Clé de la cabane et clé du générateur |
| Arbre et sols du jardin | Poly Haven, refaits dans Blender (`arbre_quest.py`) | `Assets/Nature` | Arbres (LOD + imposteurs) et sols du jardin |
| Textures des arènes du duel | Poly Haven | `Assets/Arenas/Textures` | Alvéole 17 et Quai 9 |

Liste détaillée (auteur, lien, poids, fichiers) : page wiki [Textures et modèles](https://github.com/withRen/SAE501/wiki/Textures-et-modeles).

### Fait pour le projet (équipe ou script)

| Asset | Comment | Dossier |
|---|---|---|
| Carte (bunker, couloirs, cuisine, jardin, cabane) | Fichier Blender de l'équipe, importé par `RoomImport` | `Assets/Room` |
| Pile BA-30, poste de surveillance, textures d'usure | Scripts Python / Blender (faits par l'IA) | `Assets/Pile`, `Assets/Decor/Surveillance`, `Assets/Room/Textures/Usure` |
| Planches, clous, marteau, matériaux et maillages générés | Construits par les scripts de l'éditeur | `Assets/Generated` |
| Icône et logo du jeu | Équipe (`docs/images/branding`) | `Assets/Branding` |

### Sons et musiques

| Son | Source | Licence | Dossier |
|---|---|---|---|
| Musiques (menu, partie 1 et 2), tir et rechargement du pistolet, lancement de partie, voix des zombies | fournis par l'équipe | source et licence à préciser | `Assets/Audio` |
| Sons de LeCapet (marteau et clou, souffle de la hache, bois touché et cassé, coup sur un zombie, morsure, chute du corps) | équipe (PR #151) | équipe, source à préciser | `Assets/Resources/Sounds` |
| Son de début de vague | SOUND BAG (YouTube) | usage non commercial autorisé par l'auteur | `Assets/Resources/Audio` |
| Impacts de balle, pas des zombies, grille et cadenas du portail | Freesound (voir tableau en bas de page) | CC0 1.0 | `Assets/Resources/Sounds`, `Assets/Resources/Audio` |

### Police

| Police | Auteur | Licence | Dossier |
|---|---|---|---|
| Nosifer (titre de vague) | Typomondo, Google Fonts | SIL OFL 1.1 | `Assets/Resources/Fonts` |

---

## Liste de départ (octobre 2026)

## 0. Pièce principale (fournie par l'équipe)

| Asset | Usage | Statut | Fichier |
|---|---|---|---|
| LastBoardStanding_Kitchen.blend | Salle principale : murs, fenêtres, portes, établi, mobilier, couloir sud, emplacements de planches (`PlankSlot_*`) | ✅ importé | `Assets/Room/` |

Réimport : exporter depuis Blender en FBX (axes Y haut / -Z avant), puis menu **SAE501 → Salle → 1 - Importer la salle**, puis **SAE501 → Build VR Scene**.
Textures issues de Poly Haven (CC0) et modèles Poly Haven (CC0).
Clé de la cabane : modèle « Key » de Quaternius ([Poly Pizza](https://poly.pizza/m/y3bSVdIjTh), CC0) et texture rusty_metal_04 de Poly Haven ([lien](https://polyhaven.com/a/rusty_metal_04), CC0), dans `Assets/Cle/`. Liste complète des sources : page wiki [Textures et modèles](https://github.com/withRen/SAE501/wiki/Textures-et-modeles).
Textures d'usure (`Assets/Room/Textures/Usure/`, métal peint, acier, plastique, papier) : générées par `Tools/textures/generer_usure.py` à partir de rust_coarse_01 et dirty_concrete de Poly Haven (CC0) ; albedo des pierres du jardin : stone_01 de Poly Haven (CC0). Appliquées par **SAE501 → Salle → Appliquer les textures d'usure (PBR)** (aussi à l'import de la carte et à la reconstruction de la scène).
Poste de surveillance (`Assets/Decor/Surveillance/`) : étagère, enregistreur, clavier et radio modélisés par `Tools/blender/poste_surveillance.py` (fait par l'IA), placés par `Assets/Editor/PosteSurveillance.cs` (appelé par RoomBuilder).

## 1. Armes et outils (le kit du joueur)

| Asset | Usage dans le jeu | Tâches liées | Priorité | Statut | Lien |
|---|---|---|---|---|---|
| Guns Pack – Low Poly Guns Collection | Pistolet (choix n°1) | 3.1 à 3.6 | DOIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/3d/props/guns/guns-pack-low-poly-guns-collection-192553) |
| Modern Guns – Handgun | Pistolet **choisi** (M1911 animé, chargeur, balle, flash) ; matériaux URP du chargeur et de la balle sans texture, refaits dans `Assets/Generated` | 3.1 à 3.6 | DOIT | ✅ importé, utilisé | [Asset Store](https://assetstore.unity.com/packages/3d/props/guns/modern-guns-handgun-129821) |
| Marteau | Marteau pour clouer les planches | 4.3, 4.6 | DOIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/package/20496) |
| Electric Torch | Lampe torche (hors GDD, à trancher) | voir PLANNING §8 | POURRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/3d/props/furniture/electric-torch-224071) |

## 2. Barricades et décor de la salle

| Asset | Usage dans le jeu | Tâches liées | Priorité | Statut | Lien |
|---|---|---|---|---|---|
| Wooden Planks – Various | Planches des barricades | 4.1, 4.2, 4.4, 4.5 | DOIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/3d/props/industrial/wooden-planks-various-308365) |
| Établi d'outils de survie | Établi central avec les planches et le marteau | 2.4 | DOIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/package/82886) |
| Metal Plates | Plaques de métal : décor et renforts autour des ouvertures | 2.2, 2.6, 2.7 | DEVRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/3d/props/exterior/metal-plates-64240) |
| Low Poly Sci-Fi Monitor Pack (retro CRT / slimline) | Écrans du pupitre de contrôle | 2.5, 7.1, 7.2 | DEVRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/3d/props/electronics/low-poly-sci-fi-monitor-pack-2-sets-retro-crt-slimline-assets-326721) |

## 3. Ennemis

| Asset | Usage dans le jeu | Tâches liées | Priorité | Statut | Lien |
|---|---|---|---|---|---|
| Free Shirtless Zombie | Modèle du zombie (rig humanoïde, **sans aucune animation**) | 5.1, 5.4 à 5.8 | DOIT | ✅ importé, utilisé | [Asset Store](https://assetstore.unity.com/packages/3d/characters/humanoids/free-shirtless-zombie-276762) |

## 4. Textures (salle, béton, métal rouillé)

| Asset | Usage | Tâches liées | Priorité | Statut | Lien |
|---|---|---|---|---|---|
| Hand Painted Seamless Wood Texture Vol. 6 | Bois : planches, établi | 2.7 | DEVRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/2d/textures-materials/wood/hand-painted-seamless-wood-texture-vol-6-162145) |
| Brick Textures Lite (Free) | Brique : murs | 2.7 | DEVRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/2d/textures-materials/brick-textures-lite-free-289917) |
| Basic Metal Texture Pack | Métal 1 | 2.7 | DEVRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/2d/textures-materials/metals/basic-metal-texture-pack-37402) |
| Realistic Metal Texture | Métal 2 | 2.7 | DEVRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/2d/textures-materials/metals/realistic-metal-texture-305877) |

## 5. Bonus et hors GDD actuel

Ces assets ne correspondent à aucune mécanique du GDD pour l'instant. Ils sont gardés si l'équipe décide d'ajouter des armes ou des pièges.

| Asset | Usage possible | Tâches liées | Priorité | Statut | Lien |
|---|---|---|---|---|---|
| Dungeon Floor Traps | Pièges dans les couloirs (piques). Le GDD parle d'une décharge électrique : à adapter | 10.1, 10.2 | POURRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/3d/props/interior/dungeon-floor-traps-77765) |
| M26 Grenade | Grenade (aucune mécanique prévue) | — | POURRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/3d/props/weapons/m26-grenade-207012) |
| Low Poly Weapons Vol. 1 | Armes supplémentaires (boîte magique) | 10.3 | POURRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/3d/props/guns/low-poly-weapons-vol-1-151980) |
| Flash Grenade | Grenade flash (aucune mécanique prévue) | — | POURRAIT | ⬜ | [Asset Store](https://assetstore.unity.com/packages/3d/props/weapons/flash-grenade-355982) |

---

## 6. Il manque encore

| Besoin | Pourquoi | Tâches liées |
|---|---|---|
| Sons : tir, culasse, chargeur, pas, grognements, bois qui se fend, marteau | Aucun asset audio dans la liste | 9.1 à 9.4 |
| Animations du zombie (marche, frappe, mort) | Absentes du pack : animées par script (`ZombieBoneAnim`) en attendant de vrais clips (Mixamo : licence à contrôler) | 5.7, 5.8 |
| Fenêtre / cadre de fenêtre | Pour les 2 fenêtres du GDD | 2.2, 2.3 |
| Effets : sang, étincelles, vignettage rouge | UI de santé | 8.2 |

---

## 7. Comment importer sans se marcher dessus

1. **Une seule personne** télécharge et importe chaque asset (Package Manager → My Assets), puis commite. Les autres font un `git pull`.
   Si chacun importe de son côté, les fichiers `.meta` (GUID) diffèrent et les scènes se cassent.
2. Mettre chaque asset dans son dossier (`Assets/ThirdParty/<Nom>/`), ne pas déplacer les fichiers après l'import.
3. **Shaders URP** : beaucoup d'assets sont faits pour le pipeline standard (matériaux roses). Convertir avec *Edit → Rendering → Materials → Convert Selected Built-in Materials to URP*.
4. **Budget Quest** : vérifier le nombre de polygones et la taille des textures (≤ 2K). Réduire ou retirer ce qui est trop lourd.
5. Les gros fichiers (`.fbx`, `.png`, `.tga`, `.psd`) passent déjà par Git LFS (voir `.gitattributes`).
6. **Licence** : le dépôt doit rester **privé**. Les assets de l'Asset Store ne doivent pas être publiés en clair. Chaque membre doit aussi pouvoir télécharger l'asset avec son compte Unity (les assets gratuits sont à ajouter à chaque compte).
7. Cocher le statut ✅ dans ce fichier à chaque import, avec le prix et la licence.

## Packs dans le dépôt (état au 7 octobre 2026)

Les packs utilisés sont **commités dans le dépôt** pour que tout le groupe ouvre le même projet sans rien importer. Les textures et modèles sont en **Git LFS**.

| Pack | Dossier | Taille | Utilisé pour |
|---|---|---|---|
| Modern Guns : Handgun (Nokobot) | `Assets/Nokobot` | 9 Mo | Pistolet, chargeur, douille, flash (matériaux URP du pack inclus) |
| Free Shirtless Zombie (NewPunch) | `Assets/NewPunch` | 92 Mo | Modèle des zombies |

⚠️ Ce sont des assets de l'Asset Store : le dépôt doit rester **privé**, et ils ne sont à publier nulle part en clair (README, release, démo en ligne).

## Sons téléchargés (CC0)

Détail, liens et extraits utilisés : `Tools/sons_cc0/CREDITS.md`.

| Son | Auteur | Source | Licence | Utilisé pour |
|---|---|---|---|---|
| Rattling at an iron gate | launemax | [Freesound 274766](https://freesound.org/people/launemax/sounds/274766/) | CC0 1.0 | Zombies qui secouent la grille (cinématique de fin), cadenas qui tombe (portail du jardin) |
| Gate Close and rattle | Alex_hears_things | [Freesound 494823](https://freesound.org/people/Alex_hears_things/sounds/494823/) | CC0 1.0 | Grille qui claque (cinématique), battants contre leurs butées (portail du jardin) |
| VisceralBulletImpacts | u1769092 | [Freesound 423301](https://freesound.org/people/u1769092/sounds/423301/) | CC0 1.0 | Balle de pistolet dans un zombie (5 variantes) — `Tools/sons_cc0/zombies/CREDITS.md` |
| Meaty Damage Impact | modusmogulus | [Freesound 779805](https://freesound.org/people/modusmogulus/sounds/779805/) | CC0 1.0 | Balle de pistolet dans un zombie (6e variante) |
| pas zombie route | indieground | [Freesound 223815](https://freesound.org/people/indieground/sounds/223815/) | CC0 1.0 | Pas traînants du Rôdeur |
| Heavy Footsteps \| Crunchy Thuds | GammaGool | [Freesound 854190](https://freesound.org/people/GammaGool/sounds/854190/) | CC0 1.0 | Pas du Cogneur et du Coureur |
