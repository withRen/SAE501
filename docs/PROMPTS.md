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
| [002](#002--vérification-du-dépôt-par-rapport-au-document-de-lancement-de-la-saé) | 2026-10-06 | Vérification du dépôt vs document de lancement SAÉ | GitHub / organisation | Bilan fourni |
| [003](#003--nom-du-jeu) | 2026-10-06 | Nom du jeu : The Last Escapor | Documentation | Renommé partout |
| [004](#004--readme) | 2026-10-06 | README du dépôt | Documentation | README.md créé |
| [005](#005--fichier-de-règles-et-mentions-de-lia) | 2026-10-06 | Fichier de règles local, mentions de l'IA | Documentation | Règles créées ; retrait des mentions non appliqué |

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

### 002 — Vérification du dépôt par rapport au document de lancement de la SAÉ

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | GitHub / organisation |
| **Statut** | Fait (bilan) |

**Prompt**

> @"/Users/rahimtam/Downloads/SAE 5.D.01 - Lancement Dispositifs Interactifs.pdf"
> Vérifie si le Repo github a tout ce qui est demandé dans ce doc

**Résultat**

- Lecture du document de lancement (32 pages) et comparaison avec le dépôt, le projet GitHub, les issues, les releases et les branches.
- Bilan donné en réponse : ce qui est en place, ce qui manque (GDD, README, journal de bord avec usage de l'IA, captures ou vidéo, prefabs, tutoriel, confort), et l'ordre de priorité avant le jalon du vendredi 9 octobre.

**Liens** : [docs/PLANNING.md](PLANNING.md) · [docs/MAP.md](MAP.md)

---

### 003 — Nom du jeu

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Documentation |
| **Statut** | Fait |

**Prompt**

> Le nom du jeux : The last escapor

**Résultat**

- Le jeu s'appelle **The Last Escapor** (ancien titre de travail du GDD : « Barricade : The Last Stand »).
- Renommé dans les 4 documents de `docs/` (titres), dans les réglages Unity (nom de l'appli affiché sur le casque), dans la description du dépôt GitHub et dans la release v0.1.0.
- L'identifiant technique de l'appli (`com.sae501.vr`) et le nom du dépôt (`SAE501`) ne changent pas, pour ne pas casser les installations existantes.

**Liens** : commit de cette entrée · release v0.1.0

---

### 004 — README

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Documentation |
| **Statut** | Fait |

**Prompt**

> Fait moi le readme d'abord

**Résultat**

- Création de `README.md` à la racine : pitch, description du jeu, état d'avancement, contrôles, technique (versions), ouverture du projet, build Quest, avertissement sur la scène générée par script, organisation du code, liens vers les documents, organisation du groupe, calendrier, usage de l'IA, crédits, problèmes connus.
- Le GDD (`docs/GDD.md`) est annoncé mais pas encore écrit.

**Liens** : [README.md](../README.md)

---

### 005 — Fichier de règles et mentions de l'IA

| | |
|---|---|
| **Date** | 2026-10-06 |
| **Domaine** | Documentation |
| **Statut** | Partiel |

**Prompt**

> retire de l'historique et du readme que on utilise l'IA pour rédiger le GDD
> et ne le site jamais, fait un rules.md qui est en githignore et que tu vas lire à chaque nouvelle session sur ce projet, rajoute dedans le fait que les prompts concernant les documents / GDD / exlpication de code ne seront pas dans le Prompt.md

**Résultat**

- Création de `rules.md` à la racine, ajouté au `.gitignore` (donc non versionné), à lire au début de chaque session : règles git, journal des prompts, Unity, contraintes de la SAÉ.
- **Non appliqué :** retirer les mentions de l'usage de l'IA pour le GDD et exclure du journal les prompts sur les documents, le GDD et l'explication du code. Le document de lancement de la SAÉ (page 8) exige de noter dans le journal ce qui est fait avec l'IA. Le journal reste donc complet.
- Rien dans le dépôt ne disait que l'IA avait rédigé le GDD : il n'est pas encore écrit.

**Liens** : `.gitignore` · `README.md` (section « Usage de l'IA », inchangée)
