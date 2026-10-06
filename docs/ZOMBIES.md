# Zombies — hitbox, niveaux, dégâts, NavMesh

Toutes les valeurs d'équilibrage sont dans **`Assets/Scripts/ZombieLevels.cs`** (un seul endroit à modifier). Les valeurs « niveau » 2 à 4 pour les PV des zombies (×1,3 / ×1,6 / ×2) sont **proposées par l'IA** : à valider ou à changer par le groupe.

## 1. Modèle, rig et hitbox

**Modèle :** *Free Shirtless Zombie* (Asset Store, dossier `Assets/NewPunch/ShirtlessZombieFree`). Prefab utilisé : `ShirtlessZombie_FREE_URP`. Il fait 1,76 m de haut, a 3 niveaux de détail (LOD) et un rig **humanoïde** (squelette de type Unreal : `pelvis`, `spine_01` à `spine_03`, `clavicle`, `upperarm`, `lowerarm`, `hand` + doigts, `neck_01`, `head`, `thigh`, `calf`, `foot`, `ball`).

**Le pack gratuit ne contient aucune animation** (0 clip, pas d'Animator Controller) : les animations sont donc faites **par script** (`ZombieBoneAnim`, os tournés à chaque image) : marche traînante, bras tendus devant, buste penché, bras qui s'abattent à l'attaque, chute en arrière à la mort. Ce n'est pas du vrai mocap : si le groupe trouve de vrais clips humanoïdes (marche, attaque, mort), on les met dans un Animator Controller ; `ZombieAnimationBridge` pilote déjà les paramètres `Speed`, `Attack`, `Die`.

Hitbox : 3 zones, posées **sur les os** (elles suivent l'animation), chacune avec un composant `ZombieHitbox` :

| Zone | Objets (collider) | Os | Part des PV de base retirée par tir | Tirs pour tuer (niveau 1) |
|---|---|---|---|---|
| **Tête** | `Hitbox_Tete` (sphère 0,14 m) | `head` | **100 %** (one-shot, tue à tous les niveaux) | **1** |
| **Torse** | `Hitbox_Torse` (boîte du bassin au cou), `Hitbox_Bras_l/r` (capsules épaule → main) | `spine_02`, `upperarm_l/r` | **33 %** (1/3) | **3** |
| **Jambes** | `Hitbox_Cuisse_l/r`, `Hitbox_Mollet_l/r` (capsules) | `thigh_l/r`, `calf_l/r` | **20 %** (1/5) | **5** |

- Les bras comptent comme du **torse**.
- Un tir qui ne tue pas **ralentit** le zombie (1,8 s au torse, 2,7 s aux jambes).
- Le zombie provisoire en boîtes reste disponible (menu *SAE501 → Zombie → Reconstruire le prefab provisoire*) et sert de secours si le pack n'est pas importé.

## 2. Niveaux (une vague = un niveau)

Les PV d'un zombie valent `100 × facteur du niveau`. Chaque tir au torse ou aux jambes retire toujours la même part des **100 PV de base** : plus le zombie a de PV, plus il faut de tirs. La tête reste un one-shot à tous les niveaux.

| Niveau | PV du zombie | Tirs à la tête | Tirs au torse | Tirs aux jambes | Dégâts au joueur par coup | Coups pour tuer le joueur |
|---|---|---|---|---|---|---|
| **1** | 100 | 1 | 3 | 5 | **20 %** de sa vie max | 5 |
| **2** | 130 | 1 | 4 | 7 | **20 %** | 5 |
| **3** | 160 | 1 | 5 | 8 | **33 %** | 3 |
| **4** | 200 | 1 | 6 | 10 | **50 %** | 2 |

Les niveaux au-delà de 4 gardent les valeurs du niveau 4. Ces nombres de tirs sont ceux **mesurés** avec le vrai code dans l'éditeur.

Passage au niveau suivant : `ZombieSpawner.NextLevel()` (à appeler au début de chaque vague ; elle remet aussi à zéro le quota de régénération du joueur).

## 3. Santé du joueur

La santé du joueur passe de « 4 PV » à **100 PV (= 100 %)**, pour pouvoir retirer un pourcentage. Anciens réglages équivalents conservés : régénération de **25 PV** après 10 s sans dégât, **2 fois par vague** au maximum. Le vignettage rouge réagit à la part de vie perdue (et le « cœur qui bat » démarre à 25 % de vie ou moins).

> Conséquence : au niveau 1, le joueur survit à **5** coups (et non plus 4), puisque chaque coup retire 20 %.

## 4. Points de départ et NavMesh

Le prefab **`Assets/Prefabs/ZombieSpawnPoints.prefab`** (composant `ZombieSpawnPoints`) regroupe les points de départ des chemins NavMesh. Le prefab **`Assets/Prefabs/Zombie.prefab`** contient le zombie complet (NavMeshAgent rayon 0,3 m / hauteur 1,8 m, hitbox, animation). Les deux sont reconstruits par **SAE501 → Zombie** ; les modifier à la main dans les prefabs est possible, mais « Build VR Scene » les regénère.

| Point | Position (repère carte) | Chemin vers… | Longueur du chemin |
|---|---|---|---|
| `Spawn_Couloir_Gauche` | x −4,7 · z 13,3 (bout du couloir gauche) | porte gauche | 14,9 m |
| `Spawn_Couloir_Droit` | x 4,7 · z 13,3 (bout du couloir droit) | porte droite | 13,2 m |
| `Spawn_Fenetre_Nord` | x −0,3 · z 7,0 (cour au nord du bunker) | fenêtre Nord | 2,8 m |

- La **cour Nord** (`Cour_Nord`, sol, plafond et mur nord, entre les deux couloirs) est nouvelle : sans sol à l'extérieur, aucun zombie ne pouvait atteindre une fenêtre. La fenêtre Nord est donc maintenant utilisable par les zombies ; la fenêtre Sud reste fermée (pas d'extérieur de ce côté).
- La cour n'est reliée à aucun couloir : un zombie qui part de la fenêtre ne passe que par la fenêtre. Il n'y a pas de caméra sur la cour pour l'instant.
- Chaque zombie choisit l'ouverture la plus proche **par le chemin réel** à partir de son point de départ.

Test dans l'éditeur : zombie niveau 3 parti de la fenêtre → marche jusqu'à la fenêtre (2,8 m), passe en état « casse » et commence à arracher les planches.
