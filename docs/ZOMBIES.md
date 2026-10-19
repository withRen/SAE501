# Zombies — hitbox, niveaux, dégâts, NavMesh

Toutes les valeurs d'équilibrage sont dans **`Assets/Scripts/ZombieLevels.cs`** (un seul endroit à modifier). Les valeurs « niveau » 2 à 4 pour les PV des zombies (×1,3 / ×1,6 / ×2) sont **proposées par l'IA** : à valider ou à changer par le groupe.

## 1. Modèle, rig et hitbox

**Modèle :** *Free Shirtless Zombie* (Asset Store, dossier `Assets/NewPunch/ShirtlessZombieFree`). Prefab utilisé : `ShirtlessZombie_FREE_URP`. Il fait 1,76 m de haut, a 3 niveaux de détail (LOD) et un rig **humanoïde** (squelette de type Unreal : `pelvis`, `spine_01` à `spine_03`, `clavicle`, `upperarm`, `lowerarm`, `hand` + doigts, `neck_01`, `head`, `thigh`, `calf`, `foot`, `ball`).

**Animations : clips Mixamo** (PR withRen/SAE501#68 d'alamine01, fusionnée le 7 octobre 2026). Le pack gratuit n'a aucune animation ; 3 clips Mixamo (`Zombie Walk.fbx`, `Zombie Attack.fbx`, `Zombie Death.fbx`, dans **`Assets/Animations/Zombie`**) sont réglés en Humanoid par `ZombieMixamo.cs` (sur place : c'est le NavMeshAgent qui déplace le zombie) et rangés dans `Assets/Generated/Zombie_Mixamo.controller` (paramètres `Speed`, `Attack`, `Die`, pilotés par `ZombieAnimationBridge`).

| Clip | Durée | Réglage |
|---|---|---|
| Marche | 4,03 s, en boucle | Vitesse du clip = vitesse réelle du zombie / 0,55 m/s (`WalkClipSpeed`) |
| Attaque | 2,63 s | Jouée ×1,5 : la main frappe à 1,05 s du clip, donc **0,7 s** après le début ; les dégâts au joueur et l'arrachage de planche tombent à ce moment (`attackHitDelay` = 0,7 s) |
| Mort | 2,97 s | Le corps descend jusqu'au sol |

Les clips Mixamo **ne sont pas sur place** (la marche avance de 1,29 m en 4 s) : leur déplacement horizontal est extrait en *root motion* puis ignoré (`lockRootPositionXZ = false`, `applyRootMotion = false`), sinon le corps s'éloignait pendant le cycle puis revenait d'un coup à chaque boucle (« la marche se recharge »). Mesuré après correction : le bassin reste à moins de 7 cm du zombie, aucun saut d'une image à l'autre.

Si les 3 fichiers sont absents, `ZombieBuilder` revient à l'animation par script (`ZombieBoneAnim`). Testé dans l'éditeur sur la nouvelle carte : marche, passage de fenêtre, attaque (le joueur perd 20 %), mort.

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

Le passage d'un niveau à l'autre est géré par le `WaveManager` (voir wiki « Niveaux et vagues ») : il règle `ZombieSpawner.level`, ce qui fixe PV et dégâts des zombies créés, et remet à zéro le quota de régénération du joueur.

## 3. Santé du joueur

La santé du joueur passe de « 4 PV » à **100 PV (= 100 %)**, pour pouvoir retirer un pourcentage. Anciens réglages équivalents conservés : régénération de **25 PV** après 10 s sans dégât, **2 fois par vague** au maximum. Le vignettage rouge réagit à la part de vie perdue (et le « cœur qui bat » démarre à 25 % de vie ou moins).

> Conséquence : au niveau 1, le joueur survit à **5** coups (et non plus 4), puisque chaque coup retire 20 %.

## 4. Points de départ et NavMesh

Le prefab **`Assets/Prefabs/ZombieSpawnPoints.prefab`** regroupe les points de départ ; **`Assets/Prefabs/Zombie.prefab`** contient le zombie (NavMeshAgent **rayon 0,2 m** / hauteur 1,8 m). Détail de la carte et du NavMesh : [MAP.md](MAP.md).

| Point | Position (Blender) | Chemin vers… | Longueur (mesurée) |
|---|---|---|---|
| `Spawn_Couloir_Gauche` | (−21,5 ; −6,8), fond de la cuisine | porte ouest du bunker | 17,6 m |
| `Spawn_Couloir_Droit` | (7,1 ; 7) | porte est du bunker | 18 m |
| `Spawn_Exterieur_Sud` | (−1,85 ; −17,6) | fenêtres sud-ouest et sud-est | 3,7 m |
| `Spawn_Toit_Bunker` | (−1,5 ; −10), sur le toit | trous du toit | niveaux 3 et 4 seulement |

- Les fenêtres sont franchies par des **liens NavMesh** (extérieur → intérieur).
- Chaque zombie choisit l'ouverture la plus proche **par le chemin réel**.
- `Spawn_Couloir_Gauche` était au bout nord du couloir (−12 ; 3,7), à moins de 5 m de l'interrupteur du générateur : déplacé au fond de la cuisine, à **11,1 m** de l'interrupteur et hors de sa vue (issue #93).
- Le départ est posé sur le NavMesh le plus proche **dans un rayon de 1 m d'abord** : avec 4 m d'emblée, Unity choisissait le toit du couloir droit (2,8 m au-dessus) et ces zombies arrivaient par le toit.

## 5. Poursuite hors du bunker et zombie coincé

- **Poursuite** (issue #92) : un zombie qui marche vers son ouverture (ou casse ses planches) poursuit le joueur s'il le **voit à moins de 3,5 m** et que le joueur est **hors du bunker**. Il le frappe comme dans le bunker. Il le lâche si le joueur rentre dans le bunker, s'éloigne à plus de 7 m ou reste caché 2 s, et repart vers une ouverture. Pendant la poursuite il ne prend ni les fenêtres ni les trous du toit.
- **Toit, plus de chute surprise** (issue #104, niveaux 3 et 4) : sur le toit, les pas du zombie résonnent (coup sourd toutes les ~0,55 s, entendu jusqu'à 16 m). Arrivé au bord de son trou, il attend **3 à 4 s** (état `Lurking`) : de la poussière tombe du trou dans le bunker avec un bruit de gravats (`Opening.WarnFromAbove`), puis il saute. Les trous ne se barricadent toujours pas.
- **Zombie coincé** (issue #95) : un zombie qui devrait se déplacer (marche, entrée, poursuite) mais n'a pas parcouru 0,5 m en **20 s** meurt seul, pour que la vague se termine toujours. Casser des planches ou frapper ne compte pas comme « coincé ».

Test dans l'éditeur (nouvelle carte) : zombie de l'extérieur → fenêtre sud-est, 3 planches arrachées, fenêtre enjambée, attaque du joueur ; zombie du couloir droit → porte est (18 m).
