# Level Design Specification — Arène 1v1 Bunker

- **Document** : Spécification Technique & Level Design
- **Fichier Source Blender** : `Tools/blender/arena_1v1/arena_1v1_bunker.blend`
- **Fichier Export Unity** : `Tools/blender/arena_1v1/arena_1v1_bunker.fbx`
- **Thème & Ambiance** : Bunker fortifié, béton brut, acier vieilli, éclairage d'alerte ambre et néons froids

---

## 1. Vue d'ensemble du Layout

L'arène est conçue selon un principe de **symétrie rotationnelle à 180°** garantissant une stricte égalité des chances entre les deux combattants tout en brisant les lignes de vue frontales immédiates :

- **Dimensions totales élargies** : 34.0 m (Longueur axe X) x 26.0 m (Largeur axe Y) x 7.0 m (Hauteur sous plafond).
- **Surface au sol totale** : 884 m² (surface au sol étendue + 84 m² de passerelles surélevées, volume d'environ 6 188 m³).
- **Échelle** : 1 unité Blender = 1 mètre Unity (échelle 1:1 VR Quest 3).
- **Verticalité** : 2 niveaux de jeu (Niveau 0 au sol + Niveau +2.4m sur passerelles adossées aux murs avec escaliers d'accès ; 4.60 m de dégagement libre sous plafond au-dessus des passerelles).

---

## 2. Structure des Zones & Circulation (avec Passerelles et Escaliers)

- **Passerelles Surélevées (Nord & Sud à Z = +2.4m)** :
  - Dalles en béton armé de 12.0 m x 3.8 m surplombant l'arène.
  - Rambardes de sécurité métalliques avec main courante à 1.05 m (`Rail_*`).
  - Caisses d'armement lourd et fusils de sniper (`High_Ground_Crate_*` & `Pickup_HighGround_Sniper_*`).
- **Escaliers d'Accès Industriels** :
  - `Stairs_West_North` : 11 marches de béton montant de la zone Ouest vers la passerelle Nord.
  - `Stairs_East_South` : 11 marches montant de la zone Est vers la passerelle Sud (symétrie rotationnelle 180°).

```text
[SPAWN JOUEUR 1]  ----->  [Chicane Ouest]  ----->  [Couloir Nord (Néon Froid)]  ----->  [Chicane Est]  ----->  [SPAWN JOUEUR 2]
    (-13m, 0m)                (-9.5m)                   (Y = +6.5m)                     (+9.5m)                 (+13m, 0m)
         |                                                   |                                                     |
         v                                                   v                                                     v
   (Abri Flanc)                                    [ZONE CENTRALE CONTESTÉE]                                 (Abri Flanc)
                                                    Mur brise-vue (X=0)
         ^                                          Caisse de munitions                                    ^
         |                                                   ^                                                     |
         |                                                   |                                                     |
[SPAWN JOUEUR 1]  ----->  [Chicane Ouest]  ----->  [Couloir Sud (Néon Froid)]  ----->  [Chicane Est]  ----->  [SPAWN JOUEUR 2]
                              (-9.5m)                   (Y = -6.5m)                     (+9.5m)
```

### 2.1 Les 3 Lignes d'Engagement
1. **Ligne Centrale (Directe mais brisée)** :
   - Un mur en béton massif (`Central_Cover_Wall` : 6.0 m de long, 2.8 m de haut, 0.4 m d'épaisseur) bloque la vue axiale entre les deux spawns.
   - Deux caisses métalliques centrales (`Supply_Crate_Central_North` et `_South`) offrent un couvert à mi-hauteur pour tirer accroupi ou par-dessus.
2. **Couloir Nord (Flanquement Rapide)** :
   - Éclairé par un néon froid industriel (`Light_Flank_North`).
   - Couvertures constituées d'établis d'atelier (`Workbench_North`) et de barils métalliques empilés (`Barrel_NW_1`, `Barrel_NW_2`).
   - Point de ravitaillement en munitions (`Pickup_Ammo_North`).
3. **Couloir Sud (Flanquement Tactique)** :
   - Symétrique au couloir Nord avec établis et barils (`Barrel_SE_1`, `Barrel_SE_2`).
   - Ligne de repli idéale lors des tirs à moyenne distance.

---

## 3. Repères & Empties d'Intégration (Unity)

Les points d'intérêt sont matérialisés dans le fichier Blender sous la collection `Arena_Spawns` :

| Nom de l'Objet | Type | Coordonnées Blender (X, Y, Z) | Orientation | Rôle |
|---|---|---|---|---|
| `Spawn_Player_1` | Empty (Arrows) | `(-13.0, 0.0, 0.0)` | Regard vers +X (0°) | Point de réapparition Joueur 1 |
| `Spawn_Player_2` | Empty (Arrows) | `(13.0, 0.0, 0.0)` | Regard vers -X (180°) | Point de réapparition Joueur 2 |
| `Pickup_Weapon_West` | Empty (Circle) | `(-8.0, 0.0, 0.9)` | - | Arme secondaire / pistolet amélioré |
| `Pickup_Weapon_East` | Empty (Circle) | `(8.0, 0.0, 0.9)` | - | Arme secondaire / pistolet amélioré |
| `Pickup_Ammo_North` | Empty (Circle) | `(0.0, 7.2, 0.95)` | - | Boîte de munitions couloir Nord |
| `Pickup_Ammo_South` | Empty (Circle) | `(0.0, -7.2, 0.95)` | - | Boîte de munitions couloir Sud |
| `Pickup_Special_Center` | Empty (Circle) | `(0.0, 0.0, 1.6)` | - | Ravitaillement disputé central |

---

## 4. Matériaux & Shaders PBR (Graphes Procéduraux)

Chaque matériau a été configuré avec son graphe PBR complet (Base Color, Metallic, Roughness, Bump normal) :
- `Mat_Concrete_Floor` : Dalle béton avec double bruit procédural (macro-taches d'humidité à l'échelle 2.5 et micro-grain à l'échelle 18.0), projection UV cube 4.0m, bump 0.25.
- `Mat_Concrete_Wall` : Béton de coffrage de bunker militaire (bruit à l'échelle 3.5, grain 8.0, roughness 0.88, bump 0.35).
- `Mat_Steel_Dark` : Acier brut laminé sombre pour piliers et chicanes d'angle (Metallic 0.92, micro-rayures bruitées échelle 14.0, bump 0.15).
- `Mat_Rust_Cover` : Barils, tuyaux industriels et tôles oxydées (Roughness 0.85, Metallic 0.30, dégradé brique oxydée et brun profond).
- `Mat_Hazard_Yellow` : Caisses d'approvisionnement centrales (Jaune alerte bunker sali, Roughness 0.60).
- `Mat_Water_Puddle` : Eau stagnante sombre (Roughness 0.08, IOR 1.333) pour reflets spéculaires sous les éclairages néon.

---

## 5. Détails d'Environnement, Asymétrie & Silhouettes Non-Carrées (Pass Déconstruction)

- **Piliers octogonaux & piliers brisés** : Les 8 piliers massifs cubiques ont été remplacés par des structures octogonales militaires. 2 piliers clés ont été tronqués à 1.6 m avec sommet fracturé déchiqueté pour offrir des couverts mi-hauteur.
- **Barrières en diagonale brisées** : Remplacement des chicanes cubiques plates par des murets orientés à 15° avec crête effondrée en biseau et chanfreins.
- **Mur central à meurtrière** : Le mur brise-vue intègre un affaissement central irrégulier formant une encoche de visée protégée.
- **Défenses en hérissons tchèques** : 2 croisillons métalliques antichars (`Czech_Hedgehog_*`) brisant complètement la géométrie de boîte orthogonale.
- **Plaques de tôle ondulée cintrées** : Tôles métalliques (`Corrugated_Sheet_*`) appuyées de biais contre les piliers brisés.
- **Redoutes en sacs de sable en arc de cercle** : 18 sacs de sable arrondis (`Sandbag_*`) disposés en demi-lune superposée autour des points de tir centraux.
- **Tuyauteries & Flaques spéculaires** : 8 conduites industrielles et 4 surfaces liquides au profil organique.

---

## 5. Schéma d'Éclairage (Lighting Design)

15 sources lumineuses créent un contraste fort entre les zones de sécurité et de contestation :
- **Zones de Spawn Joueur 1 (Ouest) et Joueur 2 (Est)** :
  - `Spot_Spawn_West_1` & `Spot_Spawn_East_1` : Spots verticaux chauds Ambre (`#FF9933`, 450 W, ouverture 1.3 rad).
  - `Fill_Spawn_West` & `Fill_Spawn_East` : Lumières ponctuelles douces pour déboucher les ombres du joueur au spawn (60 W).
- **Couloirs de Flanquement (Nord & Sud)** :
  - `Spot_Flank_North_West` / `_East` & `Spot_Flank_South_West` / `_East` : 4 néons industriels froids blafards (`#66B2FF`, 380 W, ouverture 1.15 rad) rythmant la traversée.
- **Zone Centrale Contestée** :
  - `Spot_Center_Alarm_North` & `_South` : 2 spots d'alarme rouge ambré intense (`#FF3311`, 500 W) focalisés sur les caisses centrales.
  - `Fill_Center_Contested` : Ponctuelle de remplissage (120 W).
- **Éclairages Rasants (Rim Lights)** :
  - 4 ponctuelles rasant les piliers centraux (`Rim_Pillar_NW`, `_NE`, `_SW`, `_SE`) pour découper nettement les silhouettes des joueurs en VR.
- **Color Management** : Profil AgX avec contraste « Punchy » et exposition +0.2 EV.
