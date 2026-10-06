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
| Bunker | Salle du joueur en boîtes grises : établi, pupitre, 4 ouvertures (2 portes, 2 fenêtres) | x ±2,6 · z ±3,1 |
| Couloir gauche | Les zombies arrivent par le haut (z = 13) | x −5,5 à −3,9 · z −2 à 14 |
| Cuisine | **La salle complète du fichier Blender** (murs, fenêtres, mobilier de cuisine, établi), tournée de 90° ; son petit couloir d'origine s'ouvre sur le couloir gauche | centre x −10,2 · z 3,4 |
| Générateur | Au bout du couloir gauche : lumières, piège électrique | x −10 à −5,7 · z 8,4 à 12,4 |
| Couloir droit | Petit couloir depuis la porte droite du bunker | x 3,9 à 5,5 · z −0,6 à 14 |
| Stockage | Planches, piles, chargeurs | x 5,7 à 9,7 · z 5 à 9 |

**Ouvertures du bunker** : porte gauche (vers le couloir gauche), porte droite (vers le couloir droit), 2 fenêtres (devant et derrière le joueur). 3 emplacements de planches par ouverture.

**Caméras** (3 écrans du pupitre) : couloir gauche, couloir droit, générateur. Elles regardent depuis le bunker vers le haut des couloirs.

**Points d'apparition des zombies** : `Spawn_Gauche` et `Spawn_Droit` (objets vides en haut des couloirs, dans la hiérarchie sous `Map`).

## Bunker et Cuisine

La salle du fichier Blender (`Assets/Room`) est une cuisine complète : elle est placée **telle quelle** comme pièce Cuisine (objet `Cuisine (salle Blender)`).
Le bunker est une salle neuve en boîtes grises (`Bunker`, construite par `BuildBunkerShell`). Il reprend de la salle d'origine l'établi, la lampe suspendue, la lampe à huile, la radio, les chaises et les cadres de fenêtres/portes (objet `Bunker_Decor`), plus des casiers et des caisses (`Bunker_Materiel`). Les planches et le marteau interactifs sont dans le bunker.

## État

Blockout en boîtes grises (sans décor) dans `MainScene`, objet `Map`. Reconstruit par **SAE501 → Build VR Scene** (`RoomBuilder.BuildMap`). À faire : habillage des pièces, NavMesh, ouverture de la porte au bout du couloir droit.
