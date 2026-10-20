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
| **4** | 200 | 1 | 6 | 10 | **40 %** (avant : 50 %) | 3 |

Les niveaux au-delà de 4 gardent les valeurs du niveau 4. Ces nombres de tirs sont ceux **mesurés** avec le vrai code dans l'éditeur.

**Difficulté du menu** (`GameSettings.DamageMultiplier`) : les dégâts au joueur du tableau sont multipliés par **×0,75** en Facile, ×1 en Normal, **×1,25** en Difficile (`Zombie.ApplyLevel`).

Le passage d'un niveau à l'autre est géré par le `WaveManager` (voir wiki « Niveaux et vagues ») : il règle `ZombieSpawner.level`, ce qui fixe PV et dégâts des zombies créés, et remet à zéro le quota de régénération du joueur.

## 3. Santé du joueur

La santé du joueur passe de « 4 PV » à **100 PV (= 100 %)**, pour pouvoir retirer un pourcentage. Anciens réglages équivalents conservés : régénération de **25 PV** après 10 s sans dégât, **2 fois par vague** au maximum. **Soin entre deux vagues** : **+50 PV** au début de chaque pause (`WaveManager.breakHeal`, `PlayerHealth.Heal`), sans dépasser 100. Le vignettage rouge réagit à la part de vie perdue (et le « cœur qui bat » démarre à 25 % de vie ou moins).

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
- **Tirage pondéré des points** (issue #123, `WaveManager.SpawnWeights`) : chaque zombie de la vague part d'un point tiré au sort avec un poids (avant : un tour fixe). Ordre : extérieur, couloir gauche, couloir droit, toit.

| Niveau | Extérieur | Couloir gauche | Couloir droit | Toit |
|---|---|---|---|---|
| 1 | 100 % | — | — | — |
| 2 | 40 % | 30 % | 30 % | — |
| 3 | 35 % | 25 % | 25 % | 15 % |
| 4 | 30 % | 22,5 % | 22,5 % | 25 % |

  Si les points d'un niveau sont changés dans la scène (nombre différent), on revient au tour fixe.
- `Spawn_Couloir_Gauche` était au bout nord du couloir (−12 ; 3,7), à moins de 5 m de l'interrupteur du générateur : déplacé au fond de la cuisine, à **11,1 m** de l'interrupteur et hors de sa vue (issue #93).
- Le départ est posé sur le NavMesh le plus proche **dans un rayon de 1 m d'abord** : avec 4 m d'emblée, Unity choisissait le toit du couloir droit (2,8 m au-dessus) et ces zombies arrivaient par le toit.

## 5. Poursuite hors du bunker et zombie coincé

- **Poursuite** (issue #92) : un zombie qui marche vers son ouverture (ou casse ses planches) poursuit le joueur s'il le **voit à moins de 3,5 m** et que le joueur est **hors du bunker**. Il le frappe comme dans le bunker. Il le lâche si le joueur rentre dans le bunker, s'éloigne à plus de 7 m ou reste caché 2 s, et repart vers une ouverture. Pendant la poursuite il ne prend ni les fenêtres ni les trous du toit.
- **Traque au niveau 4** : dès que le générateur est prêt (`WaveManager` met `Zombie.HuntPlayer` à vrai), tous les zombies poursuivent le joueur dès qu'il est hors du bunker, **sans condition de distance ni de vue**, et ne le lâchent que s'il rentre dans le bunker. Remis à faux au début de chaque niveau et à la victoire.
- **Coup à travers la barricade** : un zombie qui casse des planches frappe le joueur placé à moins de **1,6 m** de lui (à plat, `Zombie.barricadeReach` ; le zombie se tient ~1 m devant l'ouverture, donc le joueur doit rester à ~60 cm en retrait). Ce coup fait **50 %** des dégâts du niveau (`barricadeDamageFactor`). Les coups **alternent** : après un coup au joueur, le suivant va à une planche (`lastHitWasPlayer`), le joueur ne peut donc pas protéger ses planches avec son corps. Rien au-delà de 1,6 m. Pas de coup au joueur tant que la porte (ouest / est) est fermée.
- **Toit, plus de chute surprise** (issue #104, niveaux 3 et 4) : sur le toit, les pas du zombie résonnent (coup sourd toutes les ~0,55 s, entendu jusqu'à 16 m). Arrivé au bord de son trou, il attend **3 à 4 s** (état `Lurking`) : de la poussière tombe du trou dans le bunker avec un bruit de gravats (`Opening.WarnFromAbove`), puis il saute. Les trous ne se barricadent toujours pas.
- **Zombie coincé** (issue #95) : un zombie qui devrait se déplacer (marche, entrée, poursuite) mais n'a pas parcouru 0,5 m en **20 s** meurt seul, pour que la vague se termine toujours. Casser des planches ou frapper ne compte pas comme « coincé ».

Test dans l'éditeur (nouvelle carte) : zombie de l'extérieur → fenêtre sud-est, 3 planches arrachées, fenêtre enjambée, attaque du joueur ; zombie du couloir droit → porte est (18 m).

## 6. Retours d'impact, visibilité dans le noir, niveau 4

- **Zombie touché** (issue #113) : à chaque balle (ou coup d'arme), gerbe de gouttes sombres au point d'impact (`Fx.Blood`, plus grosse à la tête), son de chair (`ProcAudio.Flesh`, craquement `ProcAudio.HeadCrack` à la tête) et petit recul de 12 cm dans le sens du tir (24 cm à la tête, `Zombie.flinchDistance`). Tir à la tête : la manette du pistolet vibre une deuxième fois, à fond (`Pistol.headshotHapticAmplitude` = 1, 0,2 s).
- **Yeux lumineux** (issue #117, `ZombieEyes.cs`) : deux petites lueurs rouges sur le visage, invisibles à moins de 7 m, pleines au-delà de 9 m (8 m au milieu du fondu), cachées quand le zombie est de dos ou mort. Leur taille grandit avec la distance pour rester visibles dans le brouillard.
- **Niveau 4** (issue #120) : le levier du générateur reste bloqué jusqu'à **6 zombies abattus** pendant le niveau **ou 60 s** passées dans le niveau (`WaveManager.generatorKills`, `generatorDelay`). Le panneau du générateur et l'écran d'annonces disent ce qu'il reste à faire ; une alarme courte et l'annonce « LE GÉNÉRATEUR EST PRÊT » signalent le déblocage. Les zombies tués par la sécurité « coincé » ne comptent pas.
- **Bilan** (issue #122, `GameStats.cs`, `GameOverUI.cs`) : à la mort et à la victoire, sous le titre : zombies éliminés, % de tirs à la tête (balles dans une tête / balles tirées), planches posées, niveau atteint, temps total (vagues seulement, sans tutoriel ni pause). Gâchette pour rejouer après 3 s, sinon relance au bout de 15 s.

## 7. Rééquilibrage du 20/10/2026 (journal 088)

| Réglage | Avant | Après |
|---|---|---|
| Dégâts au joueur, niveau 4 | 50 % (2 coups) | **40 %** (3 coups) |
| Coup à travers la barricade | aucun | **50 %** des dégâts du niveau, à moins de **1,6 m**, en alternance avec les planches |
| Soin par pause | 0 | **+50 PV** |
| Niveau 4, générateur prêt | poursuite à 3,5 m si vu | **traque** hors du bunker, sans distance ni vue |
| Difficulté : dégâts reçus | ×1 | **×0,75 / ×1 / ×1,25** (Facile / Normal / Difficile) |
| Difficulté : chargeurs dans la carte | 50 % | **75 % / 50 % / 35 %** (`GameSettings.MagazineChanceFactor`) |
| Chargeurs du bunker (2) | 50 % chacun | **toujours présents** |
| Recharger | les balles restantes étaient perdues | l'arme prend 12 au plus, **le chargeur ressort avec le reste** |
| Points d'apparition | tour fixe | **tirage pondéré** (section 4) |

Mesures pour le playtest du 23/10 (issue #124) : une ligne `[Bilan]` dans le log à chaque vague terminée, à la mort et à la victoire (tués, dont mêlée, tirs, têtes, dégâts reçus par niveau, PV, balles chargées, difficulté) ; lecture au casque avec `adb logcat -s Unity`.

Testé dans l'éditeur (mode Play, simulation) : rechargement 5 + 12 → 12 dans l'arme et 5 dans le chargeur éjecté, chargeurs du bunker présents, coup à la barricade (joueur à 1,3 m frappé un coup sur deux, 10 PV, les planches tombent quand même ; rien à 2,2 m), +50 PV et ligne `[Bilan]` en fin de vague, traque au niveau 4 (zombie à 21,7 m qui poursuit), reprise au niveau 3 après une mort. **Pas testé au casque.**
