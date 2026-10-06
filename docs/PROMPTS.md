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
