# The Last Escapor

![The Last Escapor : des zombies aux yeux rouges bloqués derrière la grille en fer forgé du jardin, de nuit](docs/images/cover.png)

Logo et icônes du jeu (desktop, mobile, Quest) : [docs/images/branding/](docs/images/branding/)

> Seul dans un bunker sombre, tu barricades les ouvertures à coups de marteau et tu tires sur les zombies qui arrivent par les couloirs, en surveillant les caméras.

Jeu vidéo en **réalité virtuelle** (Meta Quest 3) développé avec **Unity 6** pour la SAÉ 5.D.01 « Dispositifs interactifs » du BUT MMI 3e année. Rendu le **vendredi 13 novembre 2026**.

**Statut : prototype jouable en boîtes grises.** Ce que contient la version actuelle est décrit plus bas, avec ce qui reste à faire.

---

## Le jeu

Le joueur est enfermé dans un bunker avec **4 ouvertures** (2 portes, 2 fenêtres). Des zombies arrivent par deux longs couloirs. Il doit faire deux choses à la fois :

- **réparer** les barricades en clouant des planches avec un vrai marteau (un geste, pas un bouton) ;
- **se défendre** au pistolet, avec un rechargement manuel, tout en surveillant les **caméras** du pupitre pour voir arriver les zombies.

Références : *Call of Duty Zombies* (réparer les fenêtres avec des planches), *Job Simulator* (les objets se prennent vraiment avec les mains), *Five Nights at Freddy's* (surveiller des caméras sous pression).

**Pourquoi la VR ?** Il faut tourner la tête pour voir quelle barricade cède derrière soi, les mains font tout le jeu (saisir une planche, tenir, viser le clou, frapper) et l'échelle 1:1 donne la tension d'un zombie à 50 cm du visage.

## Ce qui est jouable aujourd'hui

| Élément | État |
|---|---|
| Bunker en boîtes grises, 4 ouvertures, couloirs gauche et droit, 3 pièces (Cuisine, Générateur, Stockage) | ✅ Voir [docs/MAP.md](docs/MAP.md) |
| Planches : on en tient une seule à la fois, elle se cale contre l'ouverture, **il faut la tenir** | ✅ |
| Marteau : les clous apparaissent, chaque coup les **enfonce** (3 coups) | ✅ |
| Une planche lâchée sans clou tombe ; avec 1 clou elle pend ; avec 2 clous elle est fixée | ✅ |
| Ceinture : pistolet (12 balles), 1 chargeur, marteau, lampe torche à piles | ✅ |
| Pupitre avec 3 écrans de caméras (sélection = plein écran) | ✅ |
| Lampe grésillante, ambiance sombre | ✅ |
| Zombies, vagues, score, santé, pièges | ⬜ |
| Rechargement complet (éjecter, insérer, armer), sons réels, tutoriel intégré | ⬜ |
| Déplacement limité à la rotation par crans (confort) | ⬜ |

Le détail et l'ordre de travail sont dans [docs/PLANNING.md](docs/PLANNING.md).

## Contrôles (Meta Quest)

| Action | Manette |
|---|---|
| Attraper une planche, un objet, un outil | Grip |
| Tirer / allumer la lampe torche | Gâchette |
| Ranger un objet | Le lâcher près de son emplacement sur la ceinture |
| Tenir une planche calée | La garder en main contre l'ouverture, ou la saisir avec l'autre main |
| Clouer | Marteau dans une main, planche tenue, **frapper le clou** avec un vrai geste |
| Regarder une caméra | Viser un écran et le sélectionner ; sélectionner l'image pour la fermer |

> Le déplacement n'est pas encore limité au confort prévu (rotation par crans uniquement). Voir la tâche 1.2.

---

## Technique

| | |
|---|---|
| Moteur | Unity **6000.6.0f1** (Unity 6.6), rendu URP |
| Plateforme | Android, **Meta Quest 3** (OpenXR + Meta XR SDK) |
| XR | XR Interaction Toolkit 3.6, OpenXR 1.18, Meta XR SDK All 205, Input System 1.20 |
| Langage | C# |
| Dépôt | Git + **Git LFS** (modèles, textures, sons) |

### Ouvrir le projet

```bash
git lfs install
git clone https://github.com/withRen/SAE501.git
```

1. Installer **Unity 6000.6.0f1** avec le module **Android Build Support** (via Unity Hub).
2. Dans Unity Hub : *Add → Add project from disk*, choisir le dossier cloné. Le premier import est long (beaucoup de textures).
3. Ouvrir la scène `Assets/Scenes/MainScene.unity`.

### Construire et installer sur un Quest

Casque en mode développeur, branché en USB. Menus **SAE501 → Build** :

| Menu | Rôle |
|---|---|
| `Quest Development (install + lancer)` | build de développement installé et lancé sur le casque (pour lire les logs) |
| `Quest Release (APK seulement)` | produit `Builds/SAE501.apk` sans lancer le casque |

Installation manuelle d'un APK :

```bash
adb install -r SAE501.apk
```

Puis lancer le jeu depuis *Bibliothèque → Sources inconnues*. Un APK de travail est disponible dans les [releases](https://github.com/withRen/SAE501/releases).

### La scène est générée par script

`MainScene` est construite par du code (`Assets/Editor/RoomBuilder.cs` et `SetupVRScene.cs`) : menu **SAE501 → Build VR Scene**. Elle assemble la salle importée de Blender, le bunker, la carte, les planches, le marteau, la ceinture, les écrans.

- **Une modification faite à la main dans la scène est perdue au prochain « Build VR Scene ».** Pour changer le jeu, modifier le code ou les prefabs, pas la scène.
- La salle Blender est dans `Assets/Room` (menu **SAE501 → Salle → 1 - Importer la salle** après un ré-export du FBX).

### Organisation du code

```
Assets/
  Scripts/    gameplay : planches, clous, marteau, pistolet, ceinture, écrans, lampe
  Editor/     générateurs de scène, import de la salle, build Quest
  Room/       salle Blender (FBX, textures, matériaux)
  Generated/  matériaux et textures de rendu créés par les scripts
docs/         documents du projet (ci-dessous)
```

---

## Documents

| Fichier | Contenu |
|---|---|
| [docs/PLANNING.md](docs/PLANNING.md) | objectifs, tâches et sous-tâches, planning sur 14 jours, risques |
| [docs/MAP.md](docs/MAP.md) | carte : bunker, couloirs, Cuisine, Générateur, Stockage |
| [docs/ZOMBIES.md](docs/ZOMBIES.md) | hitbox du zombi, niveaux (PV, dégâts), points de départ NavMesh |
| [docs/ASSETS.md](docs/ASSETS.md) | liste des assets (Asset Store, textures) et façon de les importer |
| [docs/PROMPTS.md](docs/PROMPTS.md) | journal des demandes faites à l'assistant IA et ce qui en est sorti |

Le **GDD** sera ajouté dans `docs/GDD.md` (version 1 pour le 9 octobre, version finale le 13 novembre).

## Organisation du groupe

- **Suivi des tâches :** [projet GitHub « SAE501 »](https://github.com/users/withRen/projects/2) (vues Kanban, Planning, Priorités) et les [issues](https://github.com/withRen/SAE501/issues), une par tâche du plan.
- **Équipe :** `withRen`, `LeCapet`, `alamine01`, `albanchab14`.
- **Règles de travail :**
  - une branche par tâche (`feature/<id>-nom`), puis une pull request ;
  - ne jamais modifier la même scène à plusieurs : travailler sur des prefabs et du code ;
  - commits petits, testés avant d'être poussés ;
  - au moins un test **dans le casque** par jour.

## Calendrier

| Date | Jalon |
|---|---|
| Vendredi 9 octobre | GDD v1 et prototype gris jouable |
| Vendredi 23 octobre | un autre groupe teste le jeu |
| Vendredi 13 novembre | rendu et oral (build stable sous casque) |

## Usage de l'IA

L'IA (Claude Code) est utilisée pour produire du code, des documents et des réglages Unity. Chaque demande et son résultat sont notés dans [docs/PROMPTS.md](docs/PROMPTS.md). Tout le code livré doit pouvoir être expliqué par le groupe.

## Crédits

- Salle principale : modèle Blender de l'équipe, avec des modèles et textures [Poly Haven](https://polyhaven.com) (CC0).
- Mains animées et rig XR : exemples du cours et XR Interaction Toolkit (Unity).
- Meta XR SDK : licence Meta (voir le SDK).
- Son de début de vague : « Creepy Whisper Sound Effect 13 » de **SOUND BAG** ([YouTube](https://youtube.com/shorts/YyluAyugj4s)), usage personnel et non commercial autorisé par l'auteur ; découpé à 4 s (`Assets/Resources/Audio/Vague_Debut.ogg`).
- Police « lettres de sang » du titre de vague : **Nosifer** de Typomondo ([Google Fonts](https://fonts.google.com/specimen/Nosifer)), licence SIL Open Font License 1.1 : usage libre, personnel et commercial (`Assets/Resources/Fonts`, licence jointe).
- Assets de l'Asset Store prévus : voir [docs/ASSETS.md](docs/ASSETS.md) (à importer, licences à vérifier).

## Problèmes connus

- Le build APK n'est pas encore validé sous casque (un plantage au lancement a été observé sur une version précédente).
- Ombres de la lampe à huile un peu crénelées ; performances sous casque à optimiser (tâche 11.1).
- Le dépôt est **privé** : les assets de l'Asset Store ne doivent pas être publiés en clair.
