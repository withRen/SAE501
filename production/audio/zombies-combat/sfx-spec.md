# Spécifications SFX — Combat Zombies (`zombies-combat`)

**Projet** : The Last Escapor (Unity 6 — Meta Quest 3 VR & PC)
**Étape du Pipeline** : Étape 2 — Sound Design Specification (`/team-audio zombies-combat`)
**Direction Retenue** : Option B (Profilage DSP Vocal par Archétype + Synthèse Procédurale Enrichie dans `ProcAudio.cs` + Musique Adaptative à 4 États)

---

## 1. Spécifications Vocales par Archétype (`ZombieVoice.cs`)

Les trois archétypes utilisent les banques existantes `Assets/Audio/Zombie/Zombie_Voix_1.mp3` (cri long guttural) et `Assets/Audio/Zombie/Zombie_Voix_2.mp3` (cri court agressif), différenciées en temps réel par leur enveloppe de pitch, leur cadence d'émission, leur pondération de clip et leur priorité d'allocation.

### 1.1. Paramètres Acoustiques par Archétype

| Paramètre | Le Rôdeur (Standard) | Le Cogneur (Lourd / Blindé) | Le Coureur (Rapide / Agile) |
| :--- | :--- | :--- | :--- |
| **Échelle / Vitesse GDD** | `1.76 m` (`1.0x`), vitesse `x1.0` | `1.93 m` (`1.12x`), vitesse `x0.8-0.9` | `1.69 m` (`0.92x`), vitesse `x1.45-1.6` |
| **Plage de Pitch (`AudioSource.pitch`)** | `0.94 .. 1.06` | `0.72 .. 0.82` | `1.18 .. 1.32` |
| **Volume Nominal (`maxVolume`)** | `0.80` | `1.00` | `0.90` |
| **Pondération des Clips** | `50 %` Voix_1 / `50 %` Voix_2 | `75 %` Voix_1 / `25 %` Voix_2 | `20 %` Voix_1 / `80 %` Voix_2 |
| **Intervalle Vocal Distant (`> 6 m`)** | `3.5s .. 6.5s` | `4.5s .. 7.5s` | `1.8s .. 3.2s` |
| **Intervalle Vocal Proche (`<= 6 m`)** | `2.2s .. 4.2s` | `3.0s .. 5.0s` | `1.2s .. 2.2s` |
| ** Spatialisation (`spatialBlend`)** | `1.0f` (3D pur) | `1.0f` (3D pur) | `1.0f` (3D pur) |
| **Distances (`minDistance` / `maxDistance`)** | `1.8 m` / `24.0 m` | `2.5 m` / `26.0 m` | `2.0 m` / `24.0 m` |
| **Priorité Unity (`AudioSource.priority`)** | `110` (`<= 8 m`) / `140` (`> 8 m`) | `80` | `64` |

### 1.2. Règle d'Allocation de Polyphonie (`maxVoicesAtOnce = 4`)
* Le plafond global de voix zombies simultanées passe de `3` à `4`.
* Si `4` voix sont déjà actives lorsqu'un **Coureur** ou un **Cogneur** situé à moins de `10 m` du joueur tente de vocaliser, il **préempte (coupe en fondu rapide `0.15s`)** la voix du Rôdeur le plus éloigné.

---

## 2. Nouveaux Sons Procéduraux à Synthétiser (`Assets/Scripts/ProcAudio.cs`)

### Règle d'Architecture & Performance VR (Meta Quest 3)
* Fréquence d'échantillonnage : `SampleRate = 44100 Hz`, Mono (`1 canal`).
* **Zéro Allocation GC en Combat** : Chaque nouveau son procédural doit être stocké dans un champ `private static AudioClip` initialisé une seule fois (au premier appel ou via une méthode `ProcAudio.Warmup()` appelée au démarrage de la scène). Aucun `AudioClip.Create` ni `new float[]` ne doit s'exécuter à chaque pas ou impact.
* **Protection Anti-Clipping** : Appliquer une saturation douce `Mathf.Clamp(sample, -0.95f, 0.95f)` en sortie de chaque boucle de synthèse.

---

### 2.1. Pas au Sol par Archétype (`ZombieStep_Walker`, `ZombieStep_Bruiser`, `ZombieStep_Runner`)

Déclenchés dans `Zombie.cs` via un accumulateur de distance parcourue au sol (`stepAccumulator += planarVelocity * Time.deltaTime`), uniquement lorsque le zombie est au sol (hors phase d'approche sur le toit déjà couverte par `RoofStep`).

* **Seuils de distance de foulée (`stepStride`)** :
  * **Rôdeur** : `0.68 m`
  * **Cogneur** : `0.78 m`
  * **Coureur** : `0.55 m`

#### A. `ZombieStep_Walker` (Pas traînant standard — Durée : `160 ms` / `7056 samples`)
* **Formule Acoustique** :
  * *Composante 1 (Impact talon sourd)* : Sinusoïde à fréquence glissante `f(t) = 95 - 35 * (t / 0.16)` Hz, enveloppe `exp(-t * 32) * 0.45`.
  * *Composante 2 (Frottement semelle sur béton poussiéreux)* : Bruit blanc filtré par passe-bas unipolaire (`alpha = 0.14`), enveloppe `sin(pi * t / 0.16) * exp(-t * 14) * 0.35`.
* **Paramètres d'Exécution** : Volume `0.42`, Pitch aléatoire `0.92 .. 1.08`, `spatialBlend = 1.0f`, `minDistance = 1.2 m`, `maxDistance = 14.0 m`, `priority = 96`.

#### B. `ZombieStep_Bruiser` (Impact lourd basse fréquence — Durée : `240 ms` / `10584 samples`)
* **Formule Acoustique** :
  * *Composante 1 (Onde de choc sub-basse)* : Phase intégrée sur fréquence glissante `f(t) = 110 * exp(-t * 12) + 42` Hz, enveloppe `exp(-t * 16) * 0.75`.
  * *Composante 2 (Écrasement de gravats)* : Bruit blanc filtré (`alpha = 0.22`), enveloppe `exp(-t * 24) * 0.40`.
  * *Traitement non-linéaire* : `sample = (float)System.Math.Tanh(sample * 1.4f)` pour renforcer les harmoniques bas-médiums sur les haut-parleurs du Meta Quest 3.
* **Paramètres d'Exécution** : Volume `0.68`, Pitch aléatoire `0.88 .. 0.98`, `spatialBlend = 1.0f`, `minDistance = 1.8 m`, `maxDistance = 18.0 m`, `priority = 72`.

#### C. `ZombieStep_Runner` (Foulée sèche et incisive — Durée : `110 ms` / `4851 samples`)
* **Formule Acoustique** :
  * *Composante 1 (Claquement plante de pied)* : Sinusoïde `f(t) = 175 - 60 * (t / 0.11)` Hz, enveloppe rapide `exp(-t * 48) * 0.50`.
  * *Composante 2 (Transitoire haute-médium sur béton)* : Différence de bruit filtré passe-bande (`alphaHigh = 0.45`, `alphaLow = 0.15`), enveloppe `exp(-t * 42) * 0.55`.
* **Paramètres d'Exécution** : Volume `0.58`, Pitch aléatoire `1.04 .. 1.18`, `spatialBlend = 1.0f`, `minDistance = 1.5 m`, `maxDistance = 16.0 m`, `priority = 56`.

---

### 2.2. Télégraphe d'Attaque (`AttackWindup`)

Joué dans `Zombie.cs` à l'instant exact où le zombie arme son attaque (`t = 0` du délai `attackHitDelay = 0.45s`), offrant au joueur `450 ms` de préavis spatialisé avant le coup (`ClawSwipe` + `PlayerHurt`).

* **Durée** : `250 ms` (`11025 samples`).
* **Formule Acoustique** :
  * *Porteuse laryngée montante (FM)* : Fréquence porteuse `fc(t) = 135 + 125 * (t / 0.25)` Hz modulée en phase par `fm = 36 Hz` (indice de modulation `2.2`), amplitude `0.55`.
  * *Souffle d'expiration agressif* : Bruit blanc filtré (`alpha = 0.28`), amplitude `0.35`.
  * *Enveloppe dynamique d'élan* : Montée nerveuse sur `180 ms` puis chute rapide : `Mathf.Pow(t / 0.25f, 0.55f) * Mathf.Exp(-Mathf.Max(0f, t - 0.17f) * 38f)`.
* **Paramètres d'Exécution** :
  * Volume : `0.78`
  * Pitch selon archétype : `0.78` (Cogneur), `1.00` (Rôdeur), `1.22` (Coureur).
  * Spatialisation : `spatialBlend = 1.0f`, `minDistance = 1.5 m`, `maxDistance = 10.0 m`, `priority = 24` (Strate P0 Survie).

---

### 2.3. Neutralisation & Chute du Corps (`ZombieDeath` & `BodyThud`)

Déclenchés dans `Zombie.cs` lors de la mort (`Die()`). Coupe immédiatement le `ZombieVoice` actif, joue `ZombieDeath` à `t = 0s`, puis déclenche `BodyThud` après un délai de `0.35s`.

#### A. `ZombieDeath` (Râle d'agonie descendant — Durée : `550 ms` / `24255 samples`)
* **Formule Acoustique** :
  * *Gorge descendante* : Fréquence fondamentale `f0(t) = 210 * exp(-t * 2.2) + 55` Hz avec rugosité d'amplitude `(1.0 + 0.35 * sin(2 * pi * 21 * t))`.
  * *Harmonique rauque* : `0.60 * sin(phase) + 0.25 * sin(phase * 2.0f) + 0.15 * sin(phase * 3.0f)`.
  * *Souffle final* : Bruit filtré (`alpha = 0.16`) modulé par `exp(-t * 5.5) * 0.35`.
  * *Enveloppe globale* : `(1f - exp(-t * 45f)) * exp(-t * 4.5f)`.
* **Paramètres d'Exécution** :
  * Volume : `0.82`
  * Pitch selon archétype : `0.74..0.80` (Cogneur), `0.95..1.05` (Rôdeur), `1.15..1.25` (Coureur).
  * Spatialisation : `spatialBlend = 1.0f`, `minDistance = 2.0 m`, `maxDistance = 20.0 m`, `priority = 48`.

#### B. `BodyThud` (Chute du cadavre au sol — Durée : `280 ms` / `12348 samples`)
* **Formule Acoustique** :
  * *Impact 1 (Genoux / Membres à `t = 0.00s`)* : `sin(2 * pi * (90 -> 45 Hz) * t) * exp(-t * 26) * 0.45`.
  * *Impact 2 (Torse massif à `t = 0.085s`)* : Pour `t >= 0.085s` (`t2 = t - 0.085s`), `sin(2 * pi * (78 -> 34 Hz) * t2) * exp(-t2 * 15) * 0.80` + frottement de vêtements/poussière `noise_filtered * exp(-t2 * 14) * 0.25`.
* **Paramètres d'Exécution** :
  * Volume & Pitch selon archétype : Cogneur (`volume = 0.85`, `pitch = 0.80`), Rôdeur (`volume = 0.65`, `pitch = 1.00`), Coureur (`volume = 0.55`, `pitch = 1.12`).
  * Spatialisation : `spatialBlend = 1.0f`, `minDistance = 1.5 m`, `maxDistance = 15.0 m`, `priority = 52`.

---

### 2.4. Armes de Mêlée (`MeleeSwing`, `AxeImpact`, `CrowbarImpact`)

Intégrés dans `MeleeWeapon.cs` pour donner un retour kininesthésique au swing VR/PC et différencier nettement la **Hache** (tranchante) du **Pied-de-biche** (acier contondant).

#### A. `MeleeSwing` (Souffle de frappe — Durée : `190 ms` / `8379 samples`)
* **Formule Acoustique** :
  * Bruit blanc traversant un filtre passe-bas à ouverture dynamique `alpha(t) = 0.05 + 0.26 * sin(pi * t / 0.19)` + onde d'air grave `sin(2 * pi * (130 - 45 * (t / 0.19)) * t) * 0.30`.
  * Enveloppe en cloche asymétrique : `sin(pi * Mathf.Pow(t / 0.19f, 0.75f)) * 0.75f`.
* **Paramètres d'Exécution** :
  * Hache : `pitch = 0.88 .. 0.96`, `volume = 0.60`.
  * Pied-de-biche : `pitch = 1.06 .. 1.16`, `volume = 0.52`.
  * Spatialisation : `spatialBlend = 0.65f` (VR) / `0.25f` (PC), `minDistance = 0.8 m`, `maxDistance = 8.0 m`, `priority = 40`, cooldown `0.22s`.

#### B. `AxeImpact` (Entaille tranchante — Durée : `210 ms` / `9261 samples`)
* **Formule Acoustique** : Superposé à `Flesh()` ou `HeadCrack()` lors d'un impact à la hache.
  * *Morsure de lame* : Impulsion glissante rapide `f(t) = 1850 * exp(-t * 45) + 320` Hz, enveloppe `exp(-t * 50) * 0.55`.
  * *Déchirement fibreux* : Bruit filtré (`alpha = 0.32`) modulé à `70 Hz`, enveloppe `exp(-t * 19) * 0.55`.
* **Paramètres d'Exécution** : Volume `0.80`, Pitch `0.94 .. 1.06`, `spatialBlend = 1.0f`, `minDistance = 1.5 m`, `maxDistance = 14.0 m`, `priority = 36`.

#### C. `CrowbarImpact` (Résonance d'acier forgé — Durée : `260 ms` / `11466 samples`)
* **Formule Acoustique** : Superposé à `Flesh()` ou `HeadCrack()` lors d'un impact au pied-de-biche.
  * *Choc contondant* : `sin(2 * pi * (135 -> 52 Hz) * t) * exp(-t * 28) * 0.55`.
  * *Résonance modale de barre d'acier (3 partiels inharmoniques)* :
    * Mode 1 : `sin(2 * pi * 540 * t) * exp(-t * 18) * 0.22`
    * Mode 2 : `sin(2 * pi * 1210 * t) * exp(-t * 26) * 0.15`
    * Mode 3 : `sin(2 * pi * 2380 * t) * exp(-t * 42) * 0.08`
* **Paramètres d'Exécution** : Volume `0.82`, Pitch `0.96 .. 1.04`, `spatialBlend = 1.0f`, `minDistance = 1.5 m`, `maxDistance = 16.0 m`, `priority = 36`.

---

## 3. Calibrage des Sons Existants (`Assets/Audio/` & `ProcAudio.cs`)

* **`Pistolet_Tir.mp3`** (`Pistol.cs`) : Volume `0.90`, Pitch `0.97 .. 1.03`, `spatialBlend = 0.65f` (VR) / `0.20f` (PC), `priority = 32`. Déclenche un ducking musical de `-4 dB` pendant `0.30s`.
* **`Pistolet_Rechargement.mp3`** (`Pistol.cs`) : Volume `0.78`, Pitch `1.00`, `spatialBlend = 0.65f` (VR) / `0.20f` (PC), `priority = 44`.
* **`DryFire()`** (`ProcAudio.cs`, `90 ms`) : Volume relevé de `0.65` à `0.82` pour percer le mixage en cas de chargeur vide, `priority = 20` (P0).
* **`Flesh()`** (`ProcAudio.cs`, `180 ms`) : Volume `0.75`, Pitch `0.92 .. 1.08`, `spatialBlend = 1.0f`, `minDistance = 1.5 m`, `maxDistance = 15.0 m`, `priority = 40`.
* **`HeadCrack()`** (`ProcAudio.cs`, `220 ms`) : Volume `0.92`, Pitch `0.95 .. 1.05`, `spatialBlend = 1.0f`, `minDistance = 2.0 m`, `maxDistance = 18.0 m`, `priority = 32`.
* **`WoodCrack()`** (`ProcAudio.cs`, `600 ms`) : Volume `0.85`, Pitch `0.90 .. 1.08`, `spatialBlend = 1.0f`, `minDistance = 2.5 m`, `maxDistance = 22.0 m`, `priority = 64`.
* **`RoofStep()` & `DustFall()`** (`ProcAudio.cs`, `220 ms` & `3.5s`) : Volume `0.85` / `0.75`, `spatialBlend = 1.0f`, `minDistance = 2.5 m`, `maxDistance = 22.0 m`, `priority = 28` (P0 Alerte verticale).
* **`ClawSwipe()`** (`ProcAudio.cs`, `200 ms`) : Joué à `0.7 m` du joueur dans la direction de l'attaquant (`HealthVignette.cs`). Volume `0.90`, filtre passe-bas plafonné à `3.5 kHz` (confort auditif VR), `spatialBlend = 1.0f`, `priority = 16` (P0).
* **`PlayerHurt()`** (`ProcAudio.cs`, `350 ms`) : Volume `0.88`, `spatialBlend = 0.0f` (2D interne), `priority = 12` (P0). Déclenche un ducking musical de `-6 dB` pendant `0.50s`.
* **`Heartbeat()`** (`ProcAudio.cs`, cycle `1.3 Hz` sous `25 %` PV) : Volume `0.80`, `spatialBlend = 0.0f` (2D interne), `priority = 10` (P0). Maintient la musique plafonnée à `-5 dB` tant que les PV sont `< 25 %`.

---

## 4. Matrice Complète des Événements Audio (`zombies-combat`)

| ID Événement | Script & Point d'Appel C# | Source Audio | Strate & Priorité Unity | SpatialBlend & Rolloff (`min` / `max`) | Polyphonie Max & Cooldown | Règle de Ducking Musical |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `SFX_HEARTBEAT` | `HealthVignette.Update()` (`hpRatio < 0.25f`) | `ProcAudio.Heartbeat()` | **P0** (`10`) | `0.0f` (2D) | `1` instance / `0.77s` (`1.3 Hz`) | Plafonne Musique à `-5 dB` en continu |
| `SFX_PLAYER_HURT` | `PlayerHealth.TakeDamage()` | `ProcAudio.PlayerHurt()` | **P0** (`12`) | `0.0f` (2D) | `1` instance / `0.20s` | Ducking `-6 dB` pendant `0.50s` |
| `SFX_CLAW_SWIPE` | `HealthVignette.OnDamageFrom()` | `ProcAudio.ClawSwipe()` | **P0** (`16`) | `1.0f` (`0.5m` / `6.0m` à `0.7m`) | `2` instances / `0.15s` | Ducking `-4 dB` pendant `0.35s` |
| `SFX_DRY_FIRE` | `Pistol.TryShoot()` (`ammo == 0`) | `ProcAudio.DryFire()` | **P0** (`20`) | `0.5f` (`0.5m` / `5.0m`) | `1` instance / `0.12s` | Aucun |
| `SFX_ATTACK_WINDUP` | `Zombie.StartAttack()` (`t = 0` de `attackHitDelay`) | `ProcAudio.AttackWindup()` | **P0** (`24`) | `1.0f` (`1.5m` / `10.0m`) | `3` instances / `0.25s` par zombie | Ducking `-3 dB` si distance `< 3.0m` |
| `SFX_ROOF_WARN` | `Opening.WarnFromAbove()` | `ProcAudio.RoofStep()` + `DustFall()` | **P0** (`28`) | `1.0f` (`2.5m` / `22.0m`) | `2` instances / `0.30s` | Ducking `-3 dB` pendant `1.0s` |
| `SFX_PISTOL_SHOT` | `Pistol.Fire()` | `Pistolet_Tir.mp3` | **P1** (`32`) | `0.65f` VR / `0.2f` PC (`1.5m` / `25m`) | `3` instances / `0.08s` | Ducking `-4 dB` pendant `0.30s` |
| `SFX_HEAD_CRACK` | `Zombie.TakeDamage(isHead: true)` | `ProcAudio.HeadCrack()` | **P1** (`32`) | `1.0f` (`2.0m` / `18.0m`) | `3` instances / `0.05s` | Aucun |
| `SFX_MELEE_IMPACT` | `MeleeWeapon.OnHitZombie()` | `ProcAudio.AxeImpact()` ou `CrowbarImpact()` | **P1** (`36`) | `1.0f` (`1.5m` / `16.0m`) | `2` instances / `0.15s` | Aucun |
| `SFX_FLESH_HIT` | `Zombie.TakeDamage(isHead: false)` | `ProcAudio.Flesh()` | **P1** (`40`) | `1.0f` (`1.5m` / `15.0m`) | `4` instances / `0.06s` | Aucun |
| `SFX_MELEE_SWING` | `MeleeWeapon.BeginSwing()` | `ProcAudio.MeleeSwing()` | **P1** (`40`) | `0.65f` VR / `0.25f` PC (`0.8m` / `8m`) | `1` instance / `0.22s` | Aucun |
| `SFX_PISTOL_RELOAD` | `Pistol.Reload()` | `Pistolet_Rechargement.mp3` | **P1** (`44`) | `0.65f` VR / `0.2f` PC (`1.0m` / `8m`) | `1` instance / `1.00s` | Aucun |
| `SFX_ZOMBIE_DEATH` | `Zombie.Die()` (`t = 0s`) | `ProcAudio.ZombieDeath()` | **P1** (`48`) | `1.0f` (`2.0m` / `20.0m`) | `3` instances / `0.10s` | Aucun |
| `SFX_BODY_THUD` | `Zombie.Die()` (`t = +0.35s`) | `ProcAudio.BodyThud()` | **P1** (`52`) | `1.0f` (`1.5m` / `15.0m`) | `3` instances / `0.10s` | Aucun |
| `SFX_STEP_RUNNER` | `Zombie.UpdateStep()` (Coureur) | `ProcAudio.ZombieStep_Runner()` | **P2** (`56`) | `1.0f` (`1.5m` / `16.0m`) | `4` instances / `0.18s` | Aucun |
| `SFX_VOICE_RUNNER` | `ZombieVoice.Update()` (Coureur) | `Zombie_Voix_2.mp3` (80 %) / `Voix_1` (20 %) | **P2** (`64`) | `1.0f` (`2.0m` / `24.0m`) | Partagé dans pool `maxVoices = 4` | Aucun |
| `SFX_WOOD_CRACK` | `Opening.DamagePlank()` | `ProcAudio.WoodCrack()` | **P2** (`64`) | `1.0f` (`2.5m` / `22.0m`) | `3` instances / `0.08s` | Ducking `-3 dB` pendant `0.35s` |
| `SFX_STEP_BRUISER` | `Zombie.UpdateStep()` (Cogneur) | `ProcAudio.ZombieStep_Bruiser()` | **P2** (`72`) | `1.0f` (`1.8m` / `18.0m`) | `3` instances / `0.35s` | Aucun |
| `SFX_VOICE_BRUISER` | `ZombieVoice.Update()` (Cogneur) | `Zombie_Voix_1.mp3` (75 %) / `Voix_2` (25 %) | **P2** (`80`) | `1.0f` (`2.5m` / `26.0m`) | Partagé dans pool `maxVoices = 4` | Aucun |
| `SFX_STEP_WALKER` | `Zombie.UpdateStep()` (Rôdeur) | `ProcAudio.ZombieStep_Walker()` | **P2** (`96`) | `1.0f` (`1.2m` / `14.0m`) | `4` instances / `0.30s` | Aucun |
| `SFX_VOICE_WALKER` | `ZombieVoice.Update()` (Rôdeur) | `Zombie_Voix_1.mp3` (50 %) / `Voix_2` (50 %) | **P2/P3** (`110/140`) | `1.0f` (`1.8m` / `24.0m`) | Partagé dans pool `maxVoices = 4` | Aucun |
| `MUS_COMBAT_LOOP` | `MusicPlayer.Update()` (4 états) | `Musique_Principale_1/2.mp3` | **P4** (`200`) | `0.0f` (2D Stéréo) | `2` sources en crossfade | Cible de tous les duckings P0/P1/P2 |
