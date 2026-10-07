# Carte — THE LAST ESCAPOR

La carte vient du fichier Blender de l'équipe **`LastBoardStanding_Kitchen.blend`** (version du 7 octobre 2026). Elle remplace l'ancienne carte en boîtes grises.

```
                       (haut = nord = +y Blender = -z Unity)

   ┌────────────┐                                         ┌────────────┐
   │ GÉNÉRATEUR │═╗                                   ╔═══│  STOCKAGE  │
   └────────────┘ ║ couloir                   couloir ║   └────────────┘
   ┌────────────┐ ║ gauche                     droit  ║
   │  CUISINE   │═╣                                   ║
   └────────────┘ ║                                   ║
                  ╚══╗  ┌──────────────────────┐  ╔═══╝
                 porte ═│        BUNKER        │═ porte
                 ouest  │  (11,3 m x 8 m)      │  est
                        └──┬───────────────┬───┘
                      fenêtre SW       fenêtre SE
                         ── extérieur (terrain) ──
```

## Pièces (repère Blender, X droite, Y nord)

| Zone | Contenu | Étendue Blender |
|---|---|---|
| **Bunker** | Pupitre de surveillance (4 écrans), établi avec marteau, poêle-baril, lits, étagères, caisses. 2 portes (ouest, est) et 2 fenêtres au sud | x −7,5..3,8 · y −14,1..−6,1 |
| **Couloir gauche** | En L : part de la porte ouest du bunker, puis monte vers le nord ; s'ouvre sur la cuisine et sur le générateur | x −14,5..−7,35 · y −11,2..4,2 |
| **Cuisine** | La salle d'origine (établi, cuisinière, frigo, planches) | x −22..−16,65 · y −8..−1,65 |
| **Générateur** | Générateur à démarrer (objectif), bidons, établi | x −19,7..−14,5 · y 0,35..4,65 |
| **Couloir droit** | En L : part de la porte est du bunker vers l'est, monte vers le nord, puis tourne vers le stockage | x 3,65..10,5 · y −11,2..7,6 |
| **Stockage** | Étagères, casiers, planches de rechange | x 10,5..16,8 · y 3,85..9,15 |
| **Extérieur sud** | Terrain ajouté par le jeu (le fichier n'en a pas) : les zombies y apparaissent et attaquent les fenêtres | y −14..−19 |

Repère Unity : `(x, y, z)` Blender devient `(−x, z, −y)` dans Unity.

## Ouvertures du bunker (attaquées par les zombies)

Chaque ouverture a **3 emplacements de planches** et démarre **sans barricade** : c'est au joueur de clouer.

| Ouverture | Largeur | Côté zombie | Passage des zombies |
|---|---|---|---|
| Porte ouest | 1,4 m | Couloir gauche | À pied (le NavMesh traverse la porte) |
| Porte est | 1,8 m | Couloir droit | À pied |
| Fenêtre sud-ouest | 1,8 m | Extérieur | **Lien NavMesh** : le zombie enjambe la fenêtre |
| Fenêtre sud-est | 1,8 m | Extérieur | **Lien NavMesh** |

Les emplacements des fenêtres viennent des repères du fichier (`Bunker_PlankSlot_*`) ; ceux des portes sont ajoutés par le jeu.

## Zones par niveau

| Niveau | Accès du joueur | Porte qui s'ouvre (Blender) |
|---|---|---|
| 1 | Bunker seul | — |
| 2 | + couloirs, cuisine | porte ouest (x −7,43) et porte est (x 3,72) du bunker |
| 3 | + stockage | entrée du stockage (x 10,58 · y 6,5) |
| 4 | + générateur | entrée de la salle du générateur (x −14,58 · y 2,5) |

## Points de départ des zombies

| Point | Blender | Chemin mesuré jusqu'au bunker |
|---|---|---|
| Bout du couloir gauche | (−12 ; 3,7) | 16,3 m jusqu'à la porte ouest |
| Bout du couloir droit | (7,1 ; 7) | 18,7 m jusqu'à la porte est |
| Extérieur sud | (−1,85 ; −17,6) | 3,7 m jusqu'à chaque fenêtre |

## NavMesh

- Construit sur les **collisions** de la carte : murs, sols et plinthes ont un collider exact (murs percés, sols en L), les meubles une boîte.
- Sans collision : décor plat ou suspendu (tapis, papiers, débris, cadres, câbles, plantes, tuyaux qui longent les couloirs, plafonds).
- **Rayon des zombies : 0,2 m.** Avec 0,3 m, les couloirs étaient bouchés par des objets posés en chicane (caisses dans la branche du couloir gauche, baril renversé et sac poubelle dans le couloir droit).
- Les **déchets mous** (sacs poubelle, cartons, papiers) ne bloquent pas le NavMesh : les zombies les piétinent.
- Les portes de niveau sont ignorées par le NavMesh : ce sont les points d'apparition autorisés par niveau qui empêchent les zombies d'arriver par une zone fermée.

## Mettre à jour la carte

1. Exporter le `.blend` : `/Applications/Blender.app/Contents/MacOS/Blender -b carte.blend --python Tools/blender/export_map.py`
   (FBX, matériaux, textures en JPG, lumières et caméras ; les objets de décor de plus de 6 000 triangles sont simplifiés à l'export).
2. Dans Unity : **SAE501 → Salle → 1 - Importer la carte**, puis **SAE501 → Build VR Scene**.

Les noms des objets comptent : le jeu cherche notamment `Bunker_PlankSlot_*`, `Plank_*`, `Stockage_Plank_*`, `Bunker_Hammer_Workbench`, `CCTV_Screen_0X_*`, `CCTV_Camera_0X_*`, `Generator_Switch`, `Light_GeneratorBulb`, `Light_Bunker_Workbench`.
