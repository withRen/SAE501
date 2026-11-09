# Audio Design Document — Combat Zombies (`zombies-combat`)

**Projet** : The Last Escapor (*The Last Survivor* — SAÉ 5.D.01)
**Moteur & Cibles** : Unity `6000.6.0f1` URP — Meta Quest 3 (VR Standalone) & PC (`DesktopMode.On`)
**Pipeline** : `/team-audio zombies-combat` (`direction.md`, `sfx-spec.md`, `accessibility.md`, `integration-plan.md`, `engine-notes.md`)
**Statut** : Approuvé & Implémenté

---

## 0. Note d'Écart Documentaire
Aucune bible sonore globale n'existait à `design/audio/sound-bible.md` au lancement de ce pipeline. Ce document constitue la référence d'architecture sonore et d'accessibilité multimodale pour le combat contre les infectés dans le bunker.

---

## 1. Identité Sonore & Séparation Spectrale 360°

### 1.1. Piliers Acoustiques
1. **L'Audio 360° comme radar de survie primaire (3C VR & PC)** : Sans HUD flottant intrusif en VR, la spatialisation 3D permet d'identifier les yeux fermés l'azimut et l'élévation parmi les 6 accès du bunker (Fenêtre Sud-Ouest, Fenêtre Sud-Est, Porte Ouest, Porte Est, 2 Trous du toit), l'archétype exact de la menace, et la létalité des impacts.
2. **Contraste Mécanique vs Organique** : Les armes du joueur (M1911 9mm, Hache, Pied-de-biche) sont franches, métalliques et déterministes ; les infectés sont gutturaux, asymétriques et organiques.
3. **Découpage Fréquentiel par Archétype** :

| Archétype | Échelle / Vitesse | Bande Fréquentielle | Signature Vocale & Locomotrice |
| :--- | :--- | :--- | :--- |
| **Le Cogneur** (Renforcé) | `1.93 m`, vitesse `x0.8-0.9` | **Basses & Bas-Médiums** (`60 – 350 Hz`) | Pitch vocal grave (`0.72..0.82`), pas lourds sub-basses (`ZombieStep_Bruiser`), râle caverneux et chute massive. |
| **Le Rôdeur** (Standard) | `1.76 m`, vitesse `x1.0` | **Médiums Centraux** (`350 Hz – 1.8 kHz`) | Pitch vocal naturel (`0.94..1.06`), pas traînants sur béton poussiéreux (`ZombieStep_Walker`). |
| **Le Coureur** (Rapide) | `1.69 m`, vitesse `x1.45-1.6` | **Haut-Médiums & Aigus** (`1.8 – 6 kHz`) | Pitch vocal tendu (`1.18..1.32`), cadence nerveuse (`1.2..3.2s`), foulées rapides et sèches (`ZombieStep_Runner`). |

---

## 2. Spécifications Vocales, Synthèse Procédurale & Armes

### 2.1. Profilage Vocal & Préemption (`ZombieVoice.cs`)
- Plafond global porté à `maxVoicesAtOnce = 4` avec préemption prioritaire : un **Coureur** ou un **Cogneur** à moins de `10 m` coupe en fondu rapide (`0.15s`) la voix du Rôdeur le plus éloigné si les 4 canaux sont occupés.
- Pondération des banques `Zombie_Voix_1.mp3` (cri long) / `Zombie_Voix_2.mp3` (cri court) :
  - **Rôdeur** : `50 %` / `50 %`, pitch `0.94..1.06`, pause `3.5..6.5s` (`2.2..4.2s` sous `6 m`), priorité `110`.
  - **Cogneur** : `75 %` / `25 %`, pitch `0.72..0.82`, pause `4.5..7.5s` (`3.0..5.0s` sous `6 m`), priorité `80`.
  - **Coureur** : `20 %` / `80 %`, pitch `1.18..1.32`, pause `1.8..3.2s` (`1.2..2.2s` sous `6 m`), priorité `64`.

### 2.2. Nouveaux Sons Procéduraux en Cache (`ProcAudio.cs`)
Tous les clips sont synthétisés en PCM 32-bit float Mono à `44 100 Hz` (`396.90 Ko` au total en RAM), pré-alloués via `Warmup()` au chargement de la scène et réinitialisés via `SubsystemRegistration` (zéro allocation GC en combat sur Meta Quest 3) :
1. **`ZombieStep_Walker()` (`160 ms`)** : Impact talon sourd (`95 -> 60 Hz`) + frottement de semelle filtré passe-bas (`alpha = 0.14`), foulée tous les `0.68 m`.
2. **`ZombieStep_Bruiser()` (`240 ms`)** : Onde sub-basse (`152 -> 42 Hz`) + écrasement de gravats saturé (`Tanh`), foulée tous les `0.78 m`.
3. **`ZombieStep_Runner()` (`110 ms`)** : Claquement plante de pied (`175 -> 115 Hz`) + transitoire passe-bande sèche, foulée tous les `0.55 m`.
4. **`AttackWindup()` (`250 ms`)** : Porteuse laryngée montante modulée en fréquence (`135 -> 260 Hz`, `fm = 36 Hz`) + souffle agressif joué à `t = 0` de `attackHitDelay = 0.45s` (`450 ms` de préavis avant l'impact).
5. **`ZombieDeath()` (`550 ms`) & `BodyThud()` (`280 ms` à `+0.35s`)** : Râle descendant rugueux (`265 -> 55 Hz`) suivi de la double chute genoux/torse au sol, confirmant la neutralisation hors champ.
6. **`MeleeSwing()` (`190 ms`), `AxeImpact()` (`210 ms`), `CrowbarImpact()` (`260 ms`)** : Souffle d'élan au déclenchement du coup dans `MeleeWeapon.cs` + entaille tranchante (Hache) ou résonance modale d'acier à 3 partiels `540/1210/2380 Hz` (Pied-de-biche).

---

## 3. Musique Adaptative à 4 États & Ducking (`MusicPlayer.cs`)

| État | Déclencheur | Piste & Pitch | Volume Cible | Rôle |
| :--- | :--- | :--- | :--- | :--- |
| **1. Tension Latente** | Inter-vague ou `< 2` zombies éloignés | Piste 1 — Pitch `0.96` | `0.22` | Laisser émerger les pas sur le toit et l'approche lointaine. |
| **2. Assaut Périmétrique** | Vague active sur les barricades | Piste 1 — Pitch `1.00` | `0.34` | Soutenir la défense sans masquer `WoodCrack()` ni les voix. |
| **3. Brèche / Incursion** | Barricade ouverte ou zombie à `< 6 m` dans le bunker | Piste 2 — Pitch `1.00` | `0.42` | Signaler que le périmètre est percé. |
| **4. Chasse Finale / Critique** | `Zombie.HuntPlayer` actif (Niveau 4) ou PV `< 25 %` | Piste 2 — Pitch `1.04` | `0.48` (`0.22` sous `Heartbeat`) | Urgence vitale avec effacement sous le battement de cœur. |

- **Ducking centralisé (`MusicPlayer.Duck(factor, duration)`)** : Atténuation automatique de la musique lors d'un dégât joueur (`0.50x` / `0.5s`), d'un tir ou griffure (`0.63x` / `0.35s`), ou d'un bris de planche / alerte toit (`0.70x` / `0.35s`).

---

## 4. Accessibilité Multimodale & Confort Auditif VR

1. **GAP 1 [Résolu] — Bris de barricade hors champ (`WoodCrack`)** : `Plank.Break()` appelle `HealthVignette.NotifyWarningFrom(transform.position, 0.65f)` qui affiche un arc périphérique **ambre/orange** (`_Hits[k].w = 1.0` dans `DamageVignette.shader`) distinct de l'arc rouge de blessure (`_Hits[k].w = 0.0`), doublé d'une légère impulsion haptique directionnelle en VR.
2. **GAP 2 [Résolu] — Télégraphe d'attaque dans l'angle mort (`AttackWindup`)** : Lorsqu'un zombie arme une attaque à `< 2.2 m` hors du cône de vision frontal (`dot < 0.35f`), `Zombie.cs` déclenche `HealthVignette.NotifyWarningFrom(transform.position, 0.75f)` et une pré-alerte haptique asymétrique `450 ms` avant le coup.
3. **GAP 3 [Résolu] — Tir à sec (`DryFire`) sans incliner le poignet** : `Pistol.Fire()` appelle `InspectHud.Flash(1.5f)` lorsque `ammo <= 0`, affichant immédiatement `0 / 12` en rouge clignotant au-dessus de la culasse pendant `1.5s`.
4. **Protection anti-fatigue auditive** : Plafonnement passe-bas à `3.5 kHz` sur `ClawSwipe()`, limitation des partiels aigus de `CrowbarImpact()`, et bornage strict `[-0.95f, +0.95f]` sur tous les générateurs de `ProcAudio.cs`.
