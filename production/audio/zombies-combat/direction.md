# Direction Audio — Combat Zombies (`zombies-combat`)

**Projet** : The Last Escapor (Unity 6 — Meta Quest 3 VR & PC)
**Étape du Pipeline** : Étape 1 — Audio Direction (`/team-audio zombies-combat`)
**Statut** : Approuvé pour Spécification & Implémentation

---

## 0. Note d'Écart Documentaire (Absence de Sound Bible)

Aucune bible sonore globale n'existe actuellement à l'emplacement `design/audio/sound-bible.md`. La présente direction audio pour le module `zombies-combat` est donc établie directement à partir du Game Design Document (GDD) et de l'audit technique de l'existant dans le dépôt (`Assets/Scripts/ProcAudio.cs`, `ZombieVoice.cs`, `MusicPlayer.cs`, `MeleeWeapon.cs`, `HealthVignette.cs` et les banques `Assets/Audio/`). Ce document fait office de référence fondatrice pour l'identité sonore, le mixage et le comportement acoustique du combat.

---

## 1. Identité Sonore (Sonic Identity) & Palette Émotionnelle

### 1.1. Piliers Acoustiques du Bunker
* **Huis clos brutaliste et industriel** : Le combat se déroule dans un bunker souterrain soviétique délabré. L'acoustique doit évoquer le béton froid, la poussière, le bois humide qui cède et l'acier rouillé, tout en conservant des transitoires sèches et franches pour ne jamais noyer la localisation spatiale dans une réverbération confuse.
* **L'Audio 360° comme capteur de survie primaire (3C VR & PC)** : En l'absence volontaire de HUD flottant intrusif en VR, l'ouïe remplace l'interface visuelle. Le joueur doit pouvoir identifier les yeux fermés :
  1. **L'origine azimutale et verticale** parmi les 6 points d'infiltration : Fenêtre Sud-Ouest, Fenêtre Sud-Est, Porte Ouest, Porte Est, et les 2 Trous du toit.
  2. **L'archétype exact de la menace** (Rôdeur, Cogneur ou Coureur) avant même de tourner la tête.
  3. **L'état de létalité de ses actions** (tir à sec, impact chair, rupture crânienne en headshot, mort effective de la cible).
* **Contraste Mécanique vs Organique** : Les armes du joueur (M1911 9mm, Hache, Pied-de-biche) sonnent métalliques, franches et déterministes ; les infectés sonnent gutturaux, asymétriques, charnus et instables.

### 1.2. Séparation Spectrale par Archétype
Pour éviter la bouillie fréquentielle lors des vagues denses (Niveaux 3 et 4), chaque archétype occupe une bande passante prioritaire :

| Archétype | Taille / Vitesse | Créneau Fréquentiel | Intention Émotionnelle & Signature |
| :--- | :--- | :--- | :--- |
| **Le Cogneur** (Lourd) | `1.93 m` (`1.12x`), vitesse `x0.8-0.9` | **Basses & Bas-Médiums** (`60 Hz – 350 Hz`) | Oppression physique, masse blindée inexorable. Pas sourds qui font vibrer la dalle, râles caverneux graves, impacts de poing massifs. |
| **Le Rôdeur** (Standard) | `1.76 m` (`1.0x`), vitesse `x1.0` (`0.9 m/s`) | **Médiums Centraux** (`350 Hz – 1.8 kHz`) | Menace persistante et familière. Gémissements rauques naturels, pas traînants réguliers, griffures franches. |
| **Le Coureur** (Rapide) | `1.69 m` (`0.92x`), vitesse `x1.45-1.6` | **Haut-Médiums & Aigus** (`1.8 kHz – 6 kHz`) | Panique immédiate, urgence vitale. Cris courts haletants et tendus, foulées rapides et sèches au sol, attaques incisives. |

---

## 2. Options de Direction Audio & Différenciation des Archétypes

### Diagnostic des lacunes actuelles
1. **Uniformité vocale** : `ZombieVoice.cs` applique le même pitch (`0.92..1.08`) sur `Zombie_Voix_1.mp3` et `Zombie_Voix_2.mp3` pour tous les zombies.
2. **Silence locomoteur au sol** : Seul le plafond génère des pas (`ProcAudio.RoofStep()`). Un Coureur arrivant dans le dos du joueur au sol est totalement silencieux s'il ne crie pas.
3. **Armes de mêlée muettes au swing et uniformes à l'impact** : `MeleeWeapon.cs` (Hache et Pied-de-biche) ne joue aucun son de mouvement (`Whoosh`) et partage uniquement `Flesh()` / `HeadCrack()` avec le pistolet, sans timbre tranchant ni résonance métallique.
4. **Absence de confirmation de mort** : À la mort d'un zombie, `ZombieVoice.cs` baisse simplement le volume vers zéro (`MoveTowards(0, dt * 4f)`), privant le joueur d'un retour sonore de neutralisation dans le noir ou hors champ.

### Comparatif des 3 Options Techniques & Artistiques

#### Option A — Différenciation 100 % DSP / Pitch sur l'existant (Zéro nouveau son procédural ni fichier)
* **Principe** : Modifier uniquement les plages de pitch et d'intervalle de `ZombieVoice.cs` selon l'archétype, et réutiliser `ProcAudio.RoofStep()` avec un pitch modifié pour simuler les pas au sol et les coups de mêlée.
* **Avantages** : Implémentation en moins d'une heure ; aucun impact mémoire.
* **Inconvénients** : Réutiliser `RoofStep()` partout crée une confusion cognitive grave entre un zombie sur le toit et un zombie dans le dos du joueur. Les armes de mêlée restent dépourvues de souffle (`Whoosh`) et de timbre métallique, et la mort des zombies reste muette.

#### Option B (RECOMMANDÉE) — Hybride DSP Vocal par Archétype + Synthèse Procédurale Enrichie (`ProcAudio.cs`)
* **Principe** :
  1. **Profilage vocal par archétype dans `ZombieVoice.cs`** à partir des 2 MP3 existants (`Zombie_Voix_1/2.mp3`) :
     * *Rôdeur* : Pitch `0.94..1.06`, cadence `3.5s..6.5s`, privilégie l'alternance cri long / cri court.
     * *Cogneur* : Pitch abaissé `0.72..0.82` (timbre guttural lourd), volume `1.0`, cadence `4.5s..7.5s`, privilégie `Zombie_Voix_1` (cri long massif).
     * *Coureur* : Pitch relevé `1.18..1.32` (timbre nerveux et étranglé), cadence resserrée `1.8s..3.2s`, privilégie `Zombie_Voix_2` (cri court agressif).
  2. **Nouveaux générateurs procéduraux dédiés dans `ProcAudio.cs`** (100 % C# natif, zéro fichier externe) :
     * **Pas au sol différenciés (`ZombieStep`)** : Déclenchés selon la vitesse de déplacement réelle, avec 3 signatures acoustiques distinctes de `RoofStep()` (frottement mat pour Rôdeur, impact lourd basse fréquence pour Cogneur, tapement sec et rapide pour Coureur).
     * **Télégraphe d'attaque (`AttackGrunt` / `Windup`)** : Court son d'amorce spatialisé au début de `attackHitDelay` (`0.45s`) pour laisser au joueur VR une fenêtre de réaction (esquive physique ou parade/tir réflexe).
     * **Râle d'agonie & chute (`ZombieDeath` + `BodyThud`)** : Souffle descendant râpeux suivi d'un impact sourd au sol (`0.35s` après la mort) confirmant l'élimination sans avoir à regarder le cadavre.
     * **Identité de mêlée (`MeleeSwing`, `AxeChop`, `CrowbarClang`)** : Bruit de fente d'air au déclenchement du coup dans `MeleeWeapon.cs`, complété à l'impact par une composante tranchante humide (Hache) ou un tintement métallique amorti `450–700 Hz` (Pied-de-biche) superposé à `Flesh()` / `HeadCrack()`.
* **Avantages** : Zéro dépendance d'assets externes à J-4, empreinte RAM/CPU négligeable sur Meta Quest 3, cohérence esthétique parfaite avec le moteur procédural existant, et résolution complète des 4 lacunes critiques de gameplay.
* **Inconvénients** : Nécessite un réglage soigné des courbes d'enveloppe et des filtres dans `ProcAudio.cs`.

#### Option C — Pack d'Assets Audio Externes Dédiés par Archétype + Mixeur Multi-Bus Complexe
* **Principe** : Intégrer 30+ nouveaux fichiers WAV/MP3 (3 banques de voix, 3 banques de pas, impacts hache/pied-de-biche, râles de mort) et restructurer tout le routage via un `AudioMixer` Unity complexe.
* **Avantages** : Variété organique supérieure sur les timbres vocaux en écoute isolée.
* **Inconvénients** : Risque élevé à J-4 (sourcing, découpe, normalisation, licences), augmentation de la taille mémoire sur casque autonome VR, et disparité sonore flagrante entre les nouveaux échantillons et les sons `ProcAudio` existants (`WoodCrack`, `ClawSwipe`, `Flesh`).

### Décision Retenue
**L'Option B est retenue.** Elle offre le meilleur ratio lisibilité tactique / maîtrise technique pour un développeur solo à J-4 sur Unity 6 VR + PC, en capitalisant sur l'architecture `ProcAudio.cs` déjà éprouvée dans le projet.

---

## 3. Direction Musicale Adaptative en Combat (`MusicPlayer.cs`)

Actuellement, `Musique_Principale_1.mp3` et `Musique_Principale_2.mp3` sont lues séquentiellement sans lien avec l'état de la partie. La musique ne doit jamais couvrir les indices spatiaux 360° ; elle doit agir comme un **baromètre de pression psychologique** articulé autour de 4 états dynamiques :

| État de Combat | Condition de Déclenchement | Source & Traitement Temps Réel | Niveau Cible (Volume) | Intention Narrative & Gameplay |
| :--- | :--- | :--- | :--- | :--- |
| **1. Tension Latente (Inter-Vague / Approche)** | Aucune brèche ouverte, `< 2` zombies actifs à distance | `Musique_Principale_1.mp3` — Pitch `0.96` | `0.18` (`~ -15 dBFS`) | Laisser respirer le bunker. Mettre en valeur le vent, les pas sur le toit (`RoofStep`) et les premiers craquements de bois. |
| **2. Assaut Périmétrique (Combat Standard)** | Vague active, attaques sur les 4 barricades (Portes O/E, Fenêtres SO/SE) | `Musique_Principale_1.mp3` — Pitch `1.00` | `0.30` (`~ -10.5 dBFS`) | Soutenir le rythme de défense sans masquer la direction des `WoodCrack()` ni des voix zombies. |
| **3. Brèche & Incursion (Danger Élevé)** | Au moins 1 barricade détruite OU chute depuis les 2 Trous du toit OU présence d'un Cogneur/Coureur à `< 6 m` | Crossfade (`1.5s`) vers `Musique_Principale_2.mp3` — Pitch `1.00` | `0.40` (`~ -8 dBFS`) | Signaler que le périmètre est compromis et que la menace est désormais à l'intérieur de la pièce. |
| **4. Chasse Finale (`HuntPlayer` / Critique)** | Mode `HuntPlayer` actif (Niveau 4) OU PV du joueur `< 25 %` | `Musique_Principale_2.mp3` — Pitch `1.05` + Ducking dynamique (`-5 dB`) si PV `< 25 %` | `0.45` (`0.25` sous `Heartbeat`) | Urgence absolue. Dès que le coeur bat (`Heartbeat` à `1.3 Hz`) ou qu'une attaque survient à `< 3 m`, la musique s'efface partiellement pour laisser une clarté chirurgicale aux réflexes de survie. |

---

## 4. Hiérarchie des Priorités de Mixage & Spatialisation 360°

### 4.1. Pyramide de Priorité Acoustique (Règle : Survie 360° > Armes > Voix > Musique)

En situation de saturation sonore, le mixage obéit à 5 strates strictes :

1. **Priorité P0 — Alertes Vitales & Menaces de Contact (`< 3.5 m`)**
   * *Sons concernés* : `ClawSwipe()` (griffure directionnelle à `0.7 m`), télégraphe d'attaque proche, foulées de `Coureur` à proximité immédiate, `RoofStep()` + `DustFall()` (menace verticale imminente), `PlayerHurt()`, `Heartbeat()` (`< 25 %` PV), `DryFire()` (clic chargeur vide).
   * *Cible de mixage* : `0.85 – 1.00` (`-1.5 à 0 dBFS`), `AudioSource.priority = 0..32`. Aucun masquage toléré.
2. **Priorité P1 — Armes du Joueur & Confirmations Létales**
   * *Sons concernés* : `Pistolet_Tir.mp3`, `Pistolet_Rechargement.mp3`, `HeadCrack()` (confirmation critique de headshot), `Flesh()`, swings et impacts `MeleeWeapon` (Hache / Pied-de-biche), `ZombieDeath()` + `BodyThud()`.
   * *Cible de mixage* : `0.70 – 0.90` (`-3 à -1 dBFS`), `AudioSource.priority = 32..64`.
3. **Priorité P2 — Menaces Périmétriques & Brèches (`3.5 m – 12 m`)**
   * *Sons concernés* : `WoodCrack()` sur les Fenêtres SO/SE et Portes O/E, pas au sol des zombies (`ZombieStep`), voix des Coureurs et Cogneurs en approche.
   * *Cible de mixage* : `0.50 – 0.70` (`-6 à -3 dBFS`), `AudioSource.priority = 64..110`.
4. **Priorité P3 — Choeur Zombie Distant (`12 m – 25 m`)**
   * *Sons concernés* : Voix des Rôdeurs éloignés (`Zombie_Voix_1/2.mp3`).
   * *Cible de mixage* : `0.25 – 0.45` (`-12 à -7 dBFS`), `AudioSource.priority = 128..160`. Plafond de polyphonie vocale maintenu à `maxVoicesAtOnce = 4` (en privilégiant systématiquement l'infecté le plus proche ou le plus dangereux : Coureur > Cogneur > Rôdeur).
5. **Priorité P4 — Musique & Ambiance de Fond**
   * *Sons concernés* : `Musique_Principale_1.mp3`, `Musique_Principale_2.mp3`.
   * *Cible de mixage* : `0.18 – 0.45` (`-15 à -7 dBFS`), `AudioSource.priority = 200`. Soumise à une atténuation momentanée (ducking de `35 %` pendant `0.4s`) lors d'un coup de feu, d'un dégât subi (`PlayerHurt`) ou d'une rupture de planche (`WoodCrack`).

### 4.2. Directives de Spatialisation 3D (VR Meta Quest 3 & PC)
* **Spatial Blend** : `1.0f` (3D pur) obligatoire sur l'intégralité des sons zombies (voix, pas, télégraphes, mort, `ClawSwipe`), des impacts de balles/mêlée (`Flesh`, `HeadCrack`) et des destructions de barricades/toit (`WoodCrack`, `RoofStep`, `DustFall`).
* **Exceptions 2D / Semi-3D** :
  * `PlayerHurt()` et `Heartbeat()` restent en `2D` (`spatialBlend = 0f`) car ils représentent l'état physiologique interne du joueur (la direction de l'agresseur étant déjà fournie par `ClawSwipe()`).
  * Les armes tenues en main (`Pistolet_Tir`, `MeleeSwing`) utilisent un `spatialBlend = 0.65f` en VR (pour suivre la main réelle du joueur sans créer de déséquilibre extrême gauche/droite dans le casque) et `0.25f` sur PC.
* **Distances d'Atténuation (Rolloff)** :
  * *Voix Zombies* : `minDistance = 1.8m`, `maxDistance = 24m` (permet de sentir la proximité critique dès qu'un zombie franchit une fenêtre ou tombe du toit).
  * *Pas Zombies (`ZombieStep`)* : `minDistance = 1.2m`, `maxDistance = 14m` pour le Rôdeur, `18m` pour le Cogneur (pas lourds portants) et `16m` pour le Coureur (transitoires aiguës audibles avant le contact).
  * *Brèches (`WoodCrack`) & Toit (`RoofStep`)* : `minDistance = 2.5m`, `maxDistance = 22m` afin de couvrir toute la diagonale du bunker souterrain.
* **Budget Performance Meta Quest 3** : Aucune allocation mémoire (`new float[]`) en plein combat pour les sons fréquents — tous les nouveaux clips procéduraux de `ProcAudio.cs` doivent être pré-générés et mis en cache au chargement (comme le fait déjà `ProcAudio` avec ses variables statiques `_flesh`, `_headCrack`, etc.) afin de garantir zéro pic de Garbage Collector en VR.
