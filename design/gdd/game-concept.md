# Game Design Document — The Last Escapor (The Last Survivor)

- **Projet** : The Last Escapor
- **Titre initial / de travail** : The Last Survivor
- **Cadre** : SAÉ 5.D.01 « Dispositifs interactifs » (MMI 3e année)
- **Date de rendu** : 13 novembre 2026
- **Équipe de développement** :
  - Rahim Tamhaev
  - Al Amine Bah
  - Alban Chabalier
  - Hugo Delpuech
- **Plateforme cible** : Meta Quest 3 (Standalone Android) + Version PC (Clavier + Souris)
- **Moteur & Technologies** : Unity 6000.6.0f1 (URP, OpenXR, Meta XR SDK v205, XRI 3.6, Input System)

---

## 1. Game Proposal

### 1.1 Concept
*The Last Escapor* est un survival shooter en réalité virtuelle où le joueur, enfermé dans un bunker fortifié, doit repousser des hordes de zombies organisées en vagues successives et accomplir une série d'objectifs pour réussir à s'échapper.

### 1.2 Intentions
Proposer une expérience nerveuse avec une forte tension dramatique, exigeant compétence motrice, rapidité de réflexion et prises de décisions logiques sous stress. Le jeu tire parti de l'immersion physique permise par la VR (manipulation d'outils, rechargement gestuel, perception spatiale du danger à 360°) tout en conservant la nervosité des shooters arcade comme *Call of Duty: Zombies*.

### 1.3 Fiche signalétique
- **Genre** : Survival, Shooter, Zombie, Défense de zone.
- **Mode de jeu** : Solo (conception initiale ouverte au multijoueur/coop).
- **Public visé** : Joueurs amateurs de VR, amateurs de survival horror et de jeux de tir sous tension (PEGI 18 / Killer).
- **Profil de joueur (Taxonomie de Bartle)** :
  - **Killer (5/5)** : Domination, élimination précise, gestion de l'agression et tension de combat.
  - **Achiever (3/5)** : Réussite des 5 vagues, optimisation des munitions et déverrouillage des salles.
  - **Socializer (2/5)** : Partage de score et entraide potentielle.
  - **Explorer (1/5)** : Découverte des recoins du bunker pour trouver clés et ravitaillement.

### 1.4 Différenciation (USP & KSP)
- **USP (Unique Selling Proposition)** : Une expérience à la *Call of Duty Zombies* sublimée par une immersion physique totale en réalité virtuelle.
- **KSP (Key Selling Points)** :
  - Combat gestuel viscéral : le corps du joueur fait office de contrôleur (viser, clouer, recharger manuellement).
  - Gestion d'espace à 360° : 4 ouvertures vulnérables autour du joueur à surveiller simultanément.
  - Système de vagues dynamique combiné à un déblocage progressif de l'environnement (bunker, cuisine, stockage, générateur).

---

## 2. Gamefeel & Interaction Physique

### 2.1 Le corps comme manette
- **Aucune prise à distance (No raycast grabbing)** : Tous les objets interactifs sont attrapés à portée de main (échelle 1 unité = 1 mètre). On s'approche, on tend le bras, on saisit.
- **Prise par clic (Toggle Grip)** : Un appui sur le grip prend l'objet, un second appui le lâche. Cela évite la crispation continue de la main et favorise la précision du geste.
- **Ergonomie en main** :
  - Pistolet : crosse calée naturellement dans la paume.
  - Marteau : tête orientée vers le haut, prête à frapper.
  - Lampe torche : faisceau lumineux calé dans l'axe naturel du poignet.
  - Planches : saisies exactement à l'endroit où la main entre en contact.
- **Ceinture physique diégétique** :
  - Côté droit : pistolet dans son holster.
  - Côté gauche : marteau.
  - Devant : chargeur de rechange 9mm.
  - Emplacement utilitaire : lampe torche à piles.
  - L'accès se fait par mémoire musculaire sans avoir besoin de regarder son corps.
- **Portes physiques** : Les portes suivent le mouvement réel de la main autour de leur charnière avec un retour sonore de grincement métallique.

### 2.2 Clouer : Le geste central
- Devant chaque ouverture apparaît une planche fantôme indiquant l'emplacement optimal.
- Une planche maintenue contre le cadre nécessite **3 vrais coups de marteau** par clou.
- États de fixation d'une planche :
  - 0 clou : la planche lâchée tombe par terre sous l'effet de la gravité.
  - 1 clou : la planche pend librement sur son axe, laissant passer les zombies.
  - 2 clous : la planche est solidement fixée.
- Lorsqu'un zombie arrache une planche, un choc sourd retentit et la planche tombe au sol, prête à être récupérée et reclouée.

### 2.3 Tir : Précis, sonore et punitif
- **Arme à feu (Pistolet semi-automatique 12 balles)** :
  - Retours sensoriels : détonation percutante, flash au canon, fumée volumétrique, éjection physique de douille, recul animé et vibration haptique.
  - Chargeur vide : clic mécanique sec et vibration d'avertissement.
  - Rechargement manuel sous pression : éjection du chargeur vide, saisie d'un nouveau chargeur à la ceinture, insertion physique sous la poignée avec clic d'enclenchement, puis armement de la culasse.
  - Dégâts localisés : tir à la tête = élimination instantanée (one-shot). Tir au corps ou aux jambes = ralentissement visible de l'ennemi.
- **Armes de corps à corps (Hache et Pied-de-biche)** :
  - Détection cinétique : les coups ne portent que si la vélocité physique dépasse **1.6 m/s**.
  - Impact : bruit sourd d'impact sur la chair ou les os et retour haptique puissant.

### 2.4 Menace sonore 3D & Ambiance
- Sons 3D spatialisés : grognements, bruits de pas, planches qui craquent permettant d'identifier la direction de l'assaut (fenêtres, couloir gauche, couloir droit, toit).
- Éclairage oppressant : obscurité dominante, lampes à pétrole vacillantes, faisceau de la lampe torche perçant la brume.

---

## 3. Game System (3C & Mécaniques)

### 3.1 Caméra & Contrôles (3C)
- **Caméra** : Caméra VR 360° à l'échelle 1:1, centrage naturel sur les yeux du joueur.
- **Contrôles Meta Quest** :
  - **Main gauche** : Gâchette (Tirer / Utiliser), Joystick enfoncé (Bouger / S'accroupir), Grip (Attraper / Lâcher), Bouton Menu (Pause).
  - **Main droite** : Gâchette (Tirer / Utiliser), Joystick (Rotation Snap Turn ou Smooth Turn), Grip (Attraper / Lâcher).
- **Paradigmes d'action** : Tirer, Recharger, Regarder autour, Se déplacer, S'accroupir.
- **Méta-paradigmes d'interaction** : Barricader, Ouvrir/fermer des portes, Récupérer une clé, Actionner le levier du générateur.

### 3.2 Progression des 5 Vagues
Le jeu comprend 5 vagues de difficulté croissante (66 zombies au total), chaque vague débloquant une nouvelle section du complexe :

| Vague | Nombre de Zombies | Ouvertures attaquées | Comportement de l'assaut | Déblocage de zone |
| :--- | :--- | :--- | :--- | :--- |
| **Vague 1** | 4 zombies | 2 ouvertures | Approche frontale très lente, apprentissage du tir et du marteau. | Salle principale (Bunker OG). |
| **Vague 2** | 8 zombies | 4 ouvertures | Apparition dans les angles morts, cadence d'assaut accélérée. | Déblocage de la Cuisine (2e salle). |
| **Vague 3** | 12 zombies | 4 ouvertures | Cadence soutenue, zombies rapides (Coureurs) en renfort. | Déblocage de la Salle de Stockage (3e salle). |
| **Vague 4** | 18 zombies | 4 ouvertures | Assaut simultané tous azimuts, snap turn indispensable. | Déblocage du Couloir Ouest & Générateur (4e salle). |
| **Vague 5** | 24 zombies | 4 ouvertures | Assaut maximal avec Cogneurs et Coureurs vers le joueur. | Accès au Générateur et Bouton d'évacuation final. |

- **Transition inter-vagues** : Pause de **30 à 45 secondes** pour réparer les ouvertures, récupérer des munitions et fouiller la nouvelle zone.
- **Annonce radio** : Un signal radio sonore prévient de l'arrivée imminente de la vague suivante.

### 3.3 Bestiaire & Données d'Équilibrage

| Type d'ennemi | Taille | Vitesse | Résistance (PV) | Attaque | Apparition & Rôle |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Le Rôdeur** (Standard) | 1.76 m | 0.9 m/s (x1.0) | 100% (2 balles torse) | Coup de griffe (20% dégâts joueur / 1.5s) | Vagues 1 à 5. Ennemi de base, marche droit vers les ouvertures, dangereux en meute. |
| **Le Cogneur** (Renforcé) | 1.93 m | x0.9 (Lente) | 150% (4 balles torse) | Coups de poing (33% à 40% dégâts / 1.5s) | Vagues 2 à 5 (1 zombie sur 5). Punit les tirs imprécis, détruit les barricades très vite. |
| **Le Coureur** (Rapide) | 1.69 m | x1.6 (Sprint) | 70% (très peu de PV) | Coups de pied (1 PV par seconde) | Vagues 3 à 5 (10% en V2, 40% en V5). Casse le rythme, sprinte vers les brèches. |

#### Table des Hitboxes (Dégâts par zone)
- **Tête** : Mort instantanée en 1 balle (Hache : -100 PV, Pied-de-biche : -50 PV).
- **Torse + Bras** : -33 PV par balle (Hache : -50 PV, Pied-de-biche : -25 PV). Ralentit le zombie pendant 1.8 s.
- **Jambes** : -20 PV par balle (Hache : -33 PV, Pied-de-biche : -15 PV). Ralentit le zombie pendant 2.7 s.

---

## 4. Conditions de Victoire & Défaite

- **Condition de Victoire (Win)** :
  1. Survivre aux 5 vagues d'assaut.
  2. Trouver la clé de sécurité.
  3. Démarrer le générateur électrique.
  4. Enfoncer physiquement le gros bouton d'évacuation près du générateur pour ouvrir la grille extérieure et s'enfuir.
- **Condition de Défaite (Lose)** :
  - Les PV du joueur tombent à 0. Écran noir immédiat et redémarrage de la partie.

---

## 5. Level Design & Circulatoire

Le complexe fortifié est découpé en 6 zones interconnectées :
1. **Bunker OG (Poste central)** : Point de départ avec l'établi (planches, marteau), la ceinture d'équipement, le bureau de l'ancien poste de surveillance (sans caméras ni écrans) et 4 ouvertures (2 portes battantes, 2 fenêtres à barreaux).
2. **Cuisine** : Débloquée après la vague 1, contient des réserves de planches et des munitions supplémentaires.
3. **Salle de Stockage** : Débloquée après la vague 2, abrite des outils et des caisses de ravitaillement.
4. **Couloir Ouest** : Long boyau d'accès sous haute tension reliant la cuisine au générateur.
5. **Salle du Générateur** : Abrite le groupe électrogène, le panneau électrique principal et le bouton d'évacuation.
6. **Jardin & Cabane extérieure** : Zone extérieure nocturne entourée d'une grille en fer forgé cadenassée, point de fuite final du joueur.

---

## 6. Sound Design & Audio

- **Menu d'accueil** : Musique d'ambiance sourde, sons de pluie battante, feedback de survol (hover) et clic de bouton.
- **En jeu** :
  - Musique adaptative continue générant une tension constante.
  - Voix et cris différenciés pour chaque variante de zombie (Rôdeur, Cogneur, Coureur).
  - Événements d'armes : coup de feu sec, éjection de douille, clic mécanique de chargeur vide, glissement du chargeur.
  - Bruitages physiques : choc sourd du marteau sur le clou, craquement du bois lorsqu'une planche cède, grincement des gonds de porte, cliquetis de l'interrupteur électrique, bourdonnement du générateur.

---

## 7. Direction Artistique & Palette de Couleurs

L'atmosphère visuelle est volontairement sombre, oppressante et contrastée, alternant les teintes d'acier rouillé, de bois décrépit et d'éclairages chauds rasants.

### Palette Interface
- **Ambre principal (`#FFBF45`)** : Titres, boutons principaux, bandeaux, flèche du tutoriel.
- **Ambre clair (`#FFD966`)** : Annonces de vagues et messages temporaires.
- **Blanc cassé (`#E0EBE6`)** : Textes courants et descriptions.
- **Gris-vert (`#789682`)** : Textes secondaires et informations passives.
- **Vert terminal (`#59FF8C`)** : Réussite, voyants actifs.
- **Rouge alerte (`#FF453D`)** : Panneaux d'accès verrouillé et alertes d'intrusion.

### Palette Retours de Jeu
- **Contour au repos (`#8C6926`)** : Liseré fin sur les objets interactifs.
- **Contour survol (`#FFD166`)** : Halo lumineux lorsque la main du joueur entre dans la zone de préhension.
- **Vignette dégâts (`#8C0000`)** : Bords de vision rouges réagissant à la perte de points de vie.
- **Voyant allumé (`#33FF4C`)** : Générateur opérationnel.
- **Voyant éteint (`#FF261A`)** : Panne de courant.
- **Générateur activé (`#FFCC80`)** : Lumière chaude baignant la salle lors de la victoire.

### Palette Ambiance
- **Ombres (`#020101`)** : Teinte dominante des recoins non éclairés du bunker (32% de l'espace visuel).
- **Bois sombre (`#1B0F07`)** : Parois et planchers dans la pénombre.
- **Bois éclairé (`#3C2615`)** : Zones frappées par la lumière directe des lampes.
- **Halo de lampe (`#CA875C`)** : Lueur chaude diffusée par les lampes à pétrole et l'établi.
- **Rouille (`#892C1E`)** : Boîtes à outils, barils et renforts métalliques usés.
- **Nuit extérieure (`#060D17`)** : Ciel nocturne, cimes des arbres et sol extérieur.
