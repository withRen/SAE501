# Carte — BARRICADE : THE LAST STAND

Le joueur reste dans le **bunker**. Les couloirs et les 3 pièces sont des **décors vus par les caméras** (le joueur n'y va pas) ; les zombies en sortent et arrivent vers le bunker.

```
              (haut de la carte = +Z)

   ┌──────────────┐ ║        ║                    ║        ║
   │ GÉNÉRATEUR   ├─╢        ║                    ║        ╟─┌────────────┐
   └──────────────┘ ║ couloir║                    ║couloir ║ │ STOCKAGE   │
   ┌──────────────┐ ║ gauche ║                    ║ droit  ╟─└────────────┘
   │ CUISINE      ├─╢        ║                    ║        ║
   └──────────────┘ ║        ║                    ║        ║
              Spawn ▲        ║        BUNKER       ║        ▲ Spawn
                    ║        ╠═╗   (6 x 5 m)    ╔═╣        ║
                    ╚════════╝ ║                ║ ╚════════╝
                       porte   ╚════════════════╝  porte (couloir
                       gauche                      existant du fichier Blender)
```

| Zone | Rôle prévu | Position (repère final, X = droite, Z = haut) |
|---|---|---|
| Bunker | Salle du joueur : établi, pupitre, 4 ouvertures | x ±2,5 · z ±3 |
| Couloir gauche | Les zombies arrivent par le haut (z = 13) | x −5,4 à −3,8 · z −2 à 14 |
| Cuisine | Mobilier de cuisine du fichier Blender (plan de travail, évier, four, frigo, bouilloire), même disposition que l'original | x −11 à −5,6 · z 0,6 à 6,2 |
| Générateur | Au bout du couloir gauche : lumières, piège électrique | x −10 à −5,6 · z 8,4 à 12,4 |
| Couloir droit | Relié au petit couloir du fichier Blender | x 4,8 à 6,4 · z −0,6 à 14 |
| Stockage | Planches, piles, chargeurs | x 6,6 à 10,6 · z 5 à 9 |

**Ouvertures du bunker** : porte gauche (vers le couloir gauche), porte droite (vers le couloir droit), 2 fenêtres (devant et derrière le joueur). 3 emplacements de planches par ouverture.

**Caméras** (3 écrans du pupitre) : couloir gauche, couloir droit, générateur. Elles regardent depuis le bunker vers le haut des couloirs.

**Points d'apparition des zombies** : `Spawn_Gauche` et `Spawn_Droit` (objets vides en haut des couloirs, dans la hiérarchie sous `Map`).

## Bunker

La salle du fichier Blender était une cuisine complète. Le mobilier de cuisine a été déplacé dans la pièce **Cuisine** de la carte (objet `Cuisine_Mobilier`). Le bunker garde l'établi, les planches, le marteau, la radio, la lampe à huile et les fenêtres, et reçoit des casiers et des caisses (`Bunker_Materiel`) : plus de cuisine là où le joueur apparaît.

## État

Blockout en boîtes grises (sans décor) dans `MainScene`, objet `Map`. Reconstruit par **SAE501 → Build VR Scene** (`RoomBuilder.BuildMap`). À faire : habillage des pièces, NavMesh, ouverture de la porte au bout du couloir droit.
