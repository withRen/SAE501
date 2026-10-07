# THE LAST ESCAPOR — Plan de production

> Basé sur le GDD. Rendu le **13 novembre 2026** (J14). Estimations en heures de travail (h) pour une personne.
> Légende statut : ✅ fait dans le prototype · 🟡 existe mais à adapter au GDD · ⬜ à faire
> Légende priorité : **DOIT** (obligatoire) · **DEVRAIT** (si le temps) · **POURRAIT** (bonus)

---

## 1. Où on en est (prototype actuel)

| Élément du GDD | État | Commentaire |
|---|---|---|
| Projet Unity 6 + Meta XR + OpenXR + XRI | ✅ | Repo GitHub, Git LFS, mains animées |
| Salle grise low poly | 🟡 | Il y a **2 portes**, le GDD demande **4 ouvertures** (2 fenêtres + 2 portes) à 360° |
| Établi + planches attrapables (1 seule à la fois) | ✅ | |
| Snap de la planche + overlay vert | ✅ | |
| Marteau : 3 coups, vélocité, autre main qui tient la planche | ✅ | Seuil de vélocité à régler |
| Planche qui pend avec un seul clou | ✅ | Bonus par rapport au GDD |
| Pistolet 12 balles, raycast, chargeur de 12 | 🟡 | Rechargement simplifié, pas d'éjection ni de culasse, pas de dégâts tête/corps |
| Ceinture VR avec holsters | ✅ | |
| Pupitre + 3 écrans caméras (Render Textures) | 🟡 | GDD : **2 couloirs**, boutons rouges, écran de score |
| Lampe torche + piles | 🟡 | **Hors GDD** : à garder ou retirer (voir §8) |
| Zombies, vagues, score, santé, pièges | ⬜ | Rien |
| Sons | ⬜ | Seulement des sons générés par code (provisoires) |
| Déplacements | 🟡 | Le rig XRI a encore marche + téléportation : le GDD veut **snap turn uniquement** |

---

## 2. Objectifs

| # | Objectif | Priorité | Critère de réussite |
|---|---|---|---|
| O1 | Fondations VR et confort | DOIT | Build Quest 3 stable à 72 fps, snap turn seulement, pas de marche ni téléportation |
| O2 | Salle de contrôle (4 ouvertures, établi, pupitre) | DOIT | Les 4 ouvertures sont réparties à 360°, tout est à portée de bras depuis le centre |
| O3 | Tir et rechargement | DOIT | Tir raycast, tête = mort, corps = ralentit, rechargement manuel complet |
| O4 | Barricades et marteau | DOIT | 4 ouvertures × 3 planches ; planche arrachable par un zombie ; réparation avec 3 vrais coups |
| O5 | Zombies (IA) | DOIT | 4 états (Spawn, Marche, Casse, Attaque) avec NavMesh |
| O6 | Vagues et score | DOIT (base) / DEVRAIT (5 vagues, points) | Une partie jouable de la vague 1 à la vague 5 avec points |
| O7 | Caméras et pupitre | DEVRAIT | 2 couloirs filmés, score sur écran digital |
| O8 | Santé et interface diégétique | DEVRAIT | Vignettage rouge qui augmente avec les dégâts, game over |
| O9 | Audio | DEVRAIT | Sons 3D : pas, grognements, bois qui se fend, armes |
| O10 | Pièges et bonus | POURRAIT | Boutons rouges + piège électrique, boîte magique |
| O11 | Polish, optimisation, rendu | DOIT | Build Android validé, tests au casque, vidéo / dossier de rendu |

---

## 3. Tâches et sous-tâches par objectif

### O1 — Fondations VR et confort (DOIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 1.1 | Setup projet | Repo + Git LFS ; Meta XR / OpenXR ; mains animées | — | — | ✅ |
| 1.2 | Locomotion conforme au GDD | Désactiver marche au joystick ; désactiver téléportation ; garder uniquement le snap turn (rotation par crans) ; supprimer le CharacterController inutile ou le figer | 3 h | 1.1 | 🟡 |
| 1.3 | Build Android | Config OpenXR Meta Quest ; build APK ; installation sur Quest 3 de l'IUT | 3 h | 1.1 | ⬜ |
| 1.4 | Profil de perf de base | Vérifier 72 fps sur une scène vide ; régler URP mobile (pas d'ombres, MSAA 4x) | 2 h | 1.3 | ⬜ |

### O2 — Salle de contrôle (DOIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 2.1 | Salle de base | Murs, sol, plafond, lampe grésillante | — | — | ✅ |
| 2.2 | Passer à 4 ouvertures à 360° | 2 fenêtres + 2 portes ; positions autour du joueur (avant, arrière, gauche, droite) ; ouvertures à portée de bras ou presque | 4 h | 2.1 | 🟡 |
| 2.3 | Slots de planches par ouverture | 3 slots par ouverture (déjà fait pour les 2 portes) ; adapter à la forme des fenêtres | 3 h | 2.2 | 🟡 |
| 2.4 | Établi central | Planches, marteau, rangement ; distance depuis le centre de la salle | 2 h | 2.2 | 🟡 |
| 2.5 | Pupitre central | Table, 2 écrans caméras, écran de score, 2 gros boutons rouges | 3 h | 2.2 | 🟡 |
| 2.6 | Couloirs décor derrière les vitres | Boîtes sombres, éclairage faible ; chemin d'arrivée des zombies | 3 h | 2.2 | 🟡 |
| 2.7 | Textures et assets réalistes (métal rouillé, béton) | Choisir 3-4 textures ; matériaux URP ; assets low poly | 6 h | 2.2 | ⬜ DEVRAIT |

### O3 — Tir et rechargement (DOIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 3.1 | Prise en main à la ceinture | Holster pistolet, chargeur ; retour au holster | — | — | ✅ |
| 3.2 | Tir raycast | Gâchette ; flash ; recul visuel léger ; 12 balles | — | — | ✅ |
| 3.3 | Dégâts tête / corps | Hitbox tête et corps sur le zombie ; tête = mort ; corps = ralenti temporaire | 3 h | 5.1 | ⬜ |
| 3.4 | Rechargement manuel complet | Bouton d'éjection du chargeur (ou geste) ; chargeur vide qui tombe ; insertion du chargeur plein ; **tirer la culasse** (geste de l'autre main) avant de pouvoir tirer | 6 h | 3.1 | 🟡 |
| 3.5 | Nombre de chargeurs | 1 chargeur de réserve (ou plus selon équilibrage) ; chargeur vide ne se récupère pas | 1 h | 3.4 | 🟡 |
| 3.6 | Réglages | Dispersion, portée, force du tir | 2 h | 3.3 | ⬜ |

### O4 — Barricades et marteau (DOIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 4.1 | Planche attrapable (1 seule à la fois) | | — | — | ✅ |
| 4.2 | Snap + overlay | Aimant au slot ; overlay vert seulement quand on tient une planche | — | — | ✅ |
| 4.3 | Marteau et clous | 3 coups ; détection de vélocité ; l'autre main tient la planche | — | — | ✅ |
| 4.4 | **Dégradation des barricades** | État par planche (intacte, cassée) ; un zombie arrache une planche en **3 s** ; la planche arrachée tombe / disparaît ; slot libre pour une nouvelle planche | 5 h | 4.3, 5.4 | ⬜ |
| 4.5 | Approvisionnement en planches | Réserve **limitée** : pile de **8 planches** au départ dans le bunker, l'établi ne se remplit pas tout seul ; les autres planches sont dans la **Cuisine** et le **Stockage** (carte Blender) ; à la **fin** d'une vague, la zone du niveau suivant s'ouvre tout de suite (sauf le générateur) pour aller y chercher des planches ; pause entre deux vagues portée à **45 s** ; planche arrachée récupérable (elle tombe dans le bunker) ; panneau « Planches dans le bunker » au-dessus de l'établi (`PlankSupply.cs`) ; le joueur peut **arracher lui-même** une planche clouée (l'attraper et tirer fort vers soi, 30 cm) pour ressortir du bunker (`Plank.cs`) | 3 h | 4.4 | 🟡 à tester |
| 4.6 | Réglage du geste | Seuil de vélocité ; test avec plusieurs joueurs ; retour visuel (étincelle) et haptique | 3 h | 4.3 | 🟡 |

### O5 — Zombies (DOIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 5.1 | Prototype zombie | Capsule low poly ou asset ; Rigidbody / NavMeshAgent ; santé | 3 h | 2.2 | ⬜ |
| 5.2 | NavMesh | Bake de la salle et des couloirs ; points d'arrivée devant chaque ouverture | 3 h | 2.6 | ⬜ |
| 5.3 | État **Spawn** | Spawn au bout des couloirs, hors du regard direct du joueur | 3 h | 5.2 | ⬜ |
| 5.4 | États **Marche** et **Casse** | Aller en ligne droite vers l'ouverture la plus proche ; animation de frappe ; retire une planche toutes les 3 s | 5 h | 5.2, 4.4 | ⬜ |
| 5.5 | État **Attaque** | Quand toutes les planches sont tombées : avance vers le joueur, inflige des dégâts à portée | 4 h | 5.4 | ⬜ |
| 5.6 | Mort | Mort sur tir à la tête ; animation ; disparition ; compte de points | 3 h | 3.3 | ⬜ |
| 5.7 | Animation du zombie qui passe la tête par la fenêtre | | 6 h | 5.5 | ⬜ DEVRAIT |
| 5.8 | Modèles et animations | Récupérer ou créer un modèle low poly + animations (marche, frappe, mort) | 6 h | — | ⬜ |

### O6 — Vagues et score (DOIT base / DEVRAIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 6.1 | Spawner de zombies | Un point par couloir ; intervalle ; nombre max à la fois | 3 h | 5.3 | ⬜ |
| 6.2 | Gestionnaire de vagues | 5 vagues : liste de zombies, côté(s), vitesse ; passage à la vague suivante quand tous sont morts | 5 h | 6.1 | ⬜ |
| 6.3 | Courbe de difficulté | V1 lente et d'un seul côté ; V2 à V4 de plusieurs côtés ; V5 plus nombreux et plus rapides | 3 h | 6.2 | ⬜ |
| 6.4 | Points | +10 par zombie tué ; bonus de fin de vague | 2 h | 6.2 | ⬜ DEVRAIT |
| 6.5 | Écran de fin | Victoire après la vague 5 ; défaite à la mort ; rejouer | 3 h | 6.2, 8.1 | ⬜ |

### O7 — Caméras et pupitre (DEVRAIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 7.1 | Caméras Render Texture | 2 couloirs (1 par couloir) ; positions utiles | 2 h | 2.6 | 🟡 |
| 7.2 | Effet CCTV | Bruit, scanlines, teinte verte ; fréquence réduite pour la perf | 3 h | 7.1 | ⬜ |
| 7.3 | Vue plein écran d'un écran | | — | — | ✅ (hors GDD, à garder si utile) |
| 7.4 | Écran de score digital | Texte : points, numéro de vague | 3 h | 6.4 | ⬜ |

### O8 — Santé et interface diégétique (DEVRAIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 8.1 | Système de santé | Points de vie du joueur ; dégâts par attaque de zombie ; régénération lente ou non | 3 h | 5.5 | ⬜ |
| 8.2 | Vignettage rouge | Effet sur les bords, de plus en plus opaque quand la santé baisse ; sans mouvement de caméra | 4 h | 8.1 | ⬜ |
| 8.3 | Mort du joueur | Fondu au noir ; écran de fin | 2 h | 8.1 | ⬜ |

### O9 — Audio (DEVRAIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 9.1 | Récupération des sons | Pas lourds, grognements, bois qui se fend, tir, chargeur, culasse, coups de marteau, ambiance | 4 h | — | ⬜ |
| 9.2 | Nettoyage sur Audacity | Normaliser, couper, mono pour le 3D | 4 h | 9.1 | ⬜ |
| 9.3 | Spatialisation 3D dans Unity | AudioSource 3D, courbes d'atténuation, mixage | 4 h | 9.2 | ⬜ |
| 9.4 | Sons gameplay | Bois qui se fend quand une barricade lâche (alerte derrière le joueur) ; pas et grognements des zombies | 3 h | 5.4, 9.3 | ⬜ |

### O10 — Pièges et bonus (POURRAIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 10.1 | Boutons rouges physiques | Appui avec le doigt (poke) ; coût en points ; feedback | 3 h | 7.1, 6.4 | ⬜ |
| 10.2 | Piège électrique par couloir | Décharge ; dégâts aux zombies dans la zone ; recharge | 5 h | 10.1 | ⬜ |
| 10.3 | Boîte magique | Arme aléatoire contre des points | 6 h | 6.4 | ⬜ |

### O11 — Polish, optimisation, rendu (DOIT)

| ID | Tâche | Sous-tâches | Estim. | Dépend de | Statut |
|---|---|---|---|---|---|
| 11.1 | Optimisation 72 fps | Limiter les lumières temps réel ; batching ; occlusion ; résolution des Render Textures | 5 h | tout | ⬜ |
| 11.2 | Tests au casque | Au moins 3 sessions complètes ; liste de bugs | 6 h | tout | ⬜ |
| 11.3 | Confort | Vérifier le snap turn, la vignette, l'absence de mouvements de caméra | 2 h | 8.2 | ⬜ |
| 11.4 | Corrections de bugs | | 8 h | 11.2 | ⬜ |
| 11.5 | Build final | APK Quest 3 ; test d'installation sur un autre casque | 3 h | 11.4 | ⬜ |
| 11.6 | Dossier de rendu | Vidéo de démo ; documentation ; schéma d'architecture | 6 h | 11.5 | ⬜ |

---

## 4. Ordre logique de production (J1 à J14)

| Jour | Objectif de la journée | Tâches | Jalon |
|---|---|---|---|
| J1 | Nettoyer le prototype | 1.2, 1.3, 2.2 | Build Quest qui tourne ; salle à 4 ouvertures |
| J2 | Salle complète | 2.3, 2.4, 2.5, 2.6, 5.8 | Salle jouable avec toutes les zones |
| J3 | Premier zombie | 5.1, 5.2, 5.3 | Un zombie apparaît et suit le NavMesh |
| J4 | Zombies qui cassent le bois | 5.4, 4.4, 4.5 | **Jalon 1 : boucle barricade complète** (zombie casse, joueur répare) |
| J5 | Combat | 3.3, 5.5, 5.6, 8.1 | Zombie qui attaque ; tir tête / corps |
| J6 | Vagues | 6.1, 6.2, 6.3 | **Jalon 2 : 5 vagues jouables** |
| J7 | Rechargement et réglages | 3.4, 3.5, 3.6, 4.6 | Rechargement complet avec culasse |
| J8 | Score et caméras | 6.4, 7.1, 7.2, 7.4 | Pupitre avec flux et score |
| J9 | Santé et fin de partie | 8.2, 8.3, 6.5 | Boucle complète (début, mort, victoire) |
| J10 | Audio | 9.1, 9.2, 9.3, 9.4 | Sons 3D en place |
| J11 | Visuel | 2.7, 5.7 | Textures et zombie qui passe la tête |
| J12 | Optimisation et bonus | 11.1, 10.1, 10.2 (si temps) | 72 fps |
| J13 | Tests et corrections | 11.2, 11.3, 11.4 | Plus de bug bloquant |
| J14 | Rendu | 11.5, 11.6 | **Build final et dossier rendus le 13/11** |

---

## 5. Périmètre (DOIT / DEVRAIT / POURRAIT / NE FERA PAS)

| Catégorie | Contenu |
|---|---|
| **DOIT** | Salle grise, 4 ouvertures, tir, marteau (3 coups), zombies qui marchent et cassent le bois, vagues de base, snap turn, build Quest |
| **DEVRAIT** | Points, écrans caméras sur le pupitre, textures réalistes, zombie qui passe la tête, santé + vignettage, audio 3D |
| **POURRAIT** | Boutons de pièges, piège électrique, boîte magique |
| **NE FERA PAS** | Recherche d'objets, multijoueur, exploration d'autres salles |

---

## 6. Répartition conseillée pour 4 personnes

| Flux | Responsabilité | Tâches principales |
|---|---|---|
| A — Gameplay VR | Armes, marteau, barricades, ceinture | 3.x, 4.x, 1.2 |
| B — IA et vagues | Zombies, NavMesh, vagues, score | 5.x, 6.x |
| C — Environnement et UI | Salle, pupitre, caméras, santé, vignettage | 2.x, 7.x, 8.x |
| D — Audio et intégration | Sons, build Quest, optimisation, dossier | 9.x, 1.3, 1.4, 11.x |

À chaque personne : 1 branche par tâche (`feature/<id>-nom`), une PR pour fusionner, **ne pas modifier la même scène en même temps**.

---

## 7. Risques

| Risque | Impact | Parade |
|---|---|---|
| Conflits Git sur `MainScene` | Perte de travail | Un seul propriétaire par scène ; scènes ou prefabs séparés par flux |
| Performances Quest (lumières, Render Textures) | Mal de crâne / saccades | Pas d'ombres ; caméras à basse fréquence ; tester tôt |
| Rechargement trop long ou trop frustrant | Gameplay pénible | Prototyper tôt (J7), tester avec d'autres joueurs |
| Geste du marteau trop strict ou trop laxiste | Frustration ou triche | Seuil réglable ; tests multiples |
| Animations de zombie manquantes | Retard | Prendre des assets existants (Mixamo ou autre, vérifier la licence) |
| Accès limité aux casques de l'IUT | Peu de tests | Planifier des créneaux dès maintenant ; simulateur XRI en secours |

---

## 8. Points à trancher en équipe

| Question | Option proposée |
|---|---|
| La lampe torche et les piles (hors GDD) : on garde ? | Garder en bonus, ou retirer pour tenir le périmètre |
| Planches : réserve infinie ou limitée ? | Réserve limitée qui se renouvelle par vague (tension sans frustration) |
| Rechargement : bouton ou geste pour éjecter ? | Bouton sur la poignée (plus fiable au casque) |
| 4 ouvertures « à 360° » : tout à portée sans marcher ? | Oui : salle plus petite, ouvertures à environ 2 m du centre |
| Le zombie peut-il passer par une ouverture partiellement barricadée ? | Non : 1 planche restante bloque, 0 planche = il passe |

---

## 9. Définition de « terminé » (checklist de rendu)

- [ ] Une partie complète se joue de la vague 1 à la vague 5, au casque
- [ ] 72 fps stables sur Quest 3
- [ ] Aucun mouvement de caméra imposé (snap turn uniquement)
- [ ] Les 4 ouvertures sont réparables et cassables
- [ ] Tir, rechargement manuel, marteau fonctionnent en gestes 1:1
- [ ] Sons 3D en place
- [ ] APK installé et testé sur un casque de l'IUT
- [ ] Dossier de rendu et vidéo de démonstration
