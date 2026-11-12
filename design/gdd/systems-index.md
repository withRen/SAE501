# Systems Index — The Last Escapor (The Last Survivor)

- **Date** : 2026-11-10
- **Projet** : The Last Escapor (`com.sae501.vr`)
- **Moteur** : Unity 6000.6.0f1 (URP, OpenXR, Meta Quest 3 & PC Desktop)
- **Mode d'extension** : Survie Co-op & Duel 1v1 PvP

---

## 1. Vue d'ensemble (Overview)

Ce document décompose les systèmes du projet *The Last Escapor* en intégrant l'extension du mode **1v1 Duel PvP** (Arène de Bunker symétrique modélisée sous Blender) et les fondations multijoueurs réseau basées sur Netcode for GameObjects.

Le cœur du jeu repose sur une physique viscérale à l'échelle 1:1, une gestion spatiale sans raycast grab, et des boucles de tension dramatique à haute intensité. Dans le mode 1v1, les piliers de barricade et d'approvisionnement se transforment en contrôle de zone, gestion de munitions limitées et exploitation des lignes de vue fermées.

---

## 2. Énumération des Systèmes (Systems Enumeration)

| # | Nom du Système | Catégorie | Priorité | Statut | Document de Design | Dépendances |
|---|---|---|---|---|---|---|
| 1 | Mouvement & Locomotion VR/PC | Foundation | MVP | Implémenté | `design/gdd/locomotion.md` | - |
| 2 | Préhension Diégétique (Toggle Grip) | Foundation | MVP | Implémenté | `design/gdd/grab-system.md` | - |
| 3 | Barricades & Clouage Physique | Core | MVP | Implémenté | `design/gdd/barricades.md` | 1, 2 |
| 4 | Tir & Balistique Pistolet 9mm | Core | MVP | Implémenté | `design/gdd/weapons-firearms.md` | 1, 2 |
| 5 | Armes de Mêlée Cinétiques | Core | MVP | Implémenté | `design/gdd/weapons-melee.md` | 1, 2 |
| 6 | Santé Joueur & Retours Somatiques | Core | MVP | Implémenté | `design/gdd/player-health.md` | 4, 5 |
| 7 | IA Zombie & Navigation Spatiale | Core | MVP | Implémenté | `design/gdd/zombie-ai.md` | 1, 3, 6 |
| 8 | Directeur de Vagues & Progression | Feature | MVP | Implémenté | `design/gdd/wave-manager.md` | 7 |
| 9 | ~~Énergie, CCTV & Disjoncteur~~ | Feature | — | Retiré (12/11/2026 : les caméras ne servaient pas au jeu) | — | — |
| 10 | Réseau & Réplication (NGO) | Foundation | Vertical Slice | En cours | `design/gdd/net-replication.md` | 1, 2, 4, 6 |
| 11 | Arène & Spawns 1v1 (Level Design) | Feature | Vertical Slice | Modélisé (Blender) | `design/gdd/arena-1v1-map.md` | 10 |
| 12 | Mode Duel 1v1 (Règles & Victoire) | Feature | Vertical Slice | À concevoir | `design/gdd/duel-1v1-rules.md` | 4, 6, 10, 11 |
| 13 | Ravitaillement & Armurerie Contestée | Feature | Vertical Slice | À concevoir | `design/gdd/supply-spawns.md` | 2, 4, 11 |
| 14 | Audio Spatialisé & Ducking P0-P4 | Presentation | MVP | Implémenté | `design/audio/audio-zombies-combat.md` | 4, 5, 6, 7 |
| 15 | HUD Écran & InspectHud | Presentation | MVP | Implémenté | `design/gdd/inspect-hud.md` | 4 |

---

## 3. Carte des Dépendances (Dependency Map)

### Couche 1 : Foundation (Socle)
- **Mouvement & Locomotion** : Contrôle du joueur VR (Snap/Smooth) et PC (`DesktopPlayer`).
- **Préhension Diégétique** : Prise en main par toggle grip, ceinture physique (holster, marteau, poches).
- **Réseau & Réplication (NGO)** : Transport UDP local (`LanDiscovery`), synchronisation des états et ticks.

### Couche 2 : Core (Mécaniques Centrales)
- **Tir & Balistique** (dépend de 1, 2)
- **Armes de Mêlée** (dépend de 1, 2)
- **Barricades & Clous** (dépend de 1, 2)
- **Santé Joueur** (dépend de 4, 5)
- **IA Zombie** (dépend de 1, 3, 6)

### Couche 3 : Feature (Modes & Contenu)
- **Directeur de Vagues** (dépend de 7)
- ~~Énergie & Caméras CCTV~~ : retiré le 12/11/2026
- **Arène & Spawns 1v1** (dépend de 10) : Géométrie Blender (`arena_1v1_bunker.blend`), 2 spawns opposés, lignes de vue brisées.
- **Mode Duel 1v1** (dépend de 4, 6, 10, 11) : Rounds en manches gagnantes (3 ou 5 rounds), temps limite, mort subite.
- **Ravitaillement Contesté** (dépend de 2, 4, 11) : Apparition des chargeurs et armes secondaires au centre.

### Couche 4 : Presentation (Restitution Sensorielle)
- **Audio Spatialisé & Ducking** (dépend de 4, 5, 6, 7) : Retours binauraux 360°, sons de pas et impacts d'armes.
- **InspectHud & Réticule** (dépend de 4) : Affichage discret des munitions au regard ou sur clic à vide.

---

## 4. Ordre Recommandé de Conception

1. **Système 11 : Arène & Spawns 1v1** — Conçu et exporté en FBX (`Tools/blender/arena_1v1/arena_1v1_bunker.fbx`).
2. **Système 12 : Mode Duel 1v1 (Règles & Arbitrage)** — Définition des conditions de victoire, manches, respawn et temps de manche.
3. **Système 13 : Ravitaillement & Armurerie Contestée** — Emplacements, cadences de réapparition des chargeurs et dynamiques de prise de risque.
4. **Système 10 : Réseau & Synchronisation Duel** — Adaptation des RPC de tir et dégâts joueurs entre casques.
