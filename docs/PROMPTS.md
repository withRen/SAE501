# Journal des prompts — BARRICADE : THE LAST STAND

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
