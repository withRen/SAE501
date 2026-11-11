# Plan d'Intégration Audio Technique — Combat Zombies (`zombies-combat`)

**Projet** : The Last Escapor (Unity `6000.6.0f1` URP — Meta Quest 3 VR & PC)
**Étape du Pipeline** : Étape 3 — Technical Implementation (`technical-artist`)
**Direction Retenue** : Option B + Résolution complète des écarts d'accessibilité GAP 1, GAP 2 et GAP 3

---

## 1. Architecture Audio Native (Justification Sans Middleware Tiers)

À J-4 de la livraison sur Meta Quest 3 (Android ARM64) et PC, l'architecture repose exclusivement sur le moteur audio natif d'Unity `6000.6.0f1` combiné au synthétiseur procédural `ProcAudio.cs` :

1. **Zéro surcharge CPU/DSP externe** : L'intégration de Wwise ou FMOD introduirait un double graphe de mixage et des bibliothèques natives Android supplémentaires, inutiles alors que le projet génère déjà ses SFX de combat en C# via `ProcAudio.cs`.
2. **Contrôle déterministe des paramètres `GameSettings.cs`** : Le projet sépare déjà le volume général (`AudioListener.volume`) du multiplicateur dédié aux infectés (`GameSettings.Zombies`, plage `0..1`, défaut `0.6`). Tous les sons émis par un zombie (`ZombieVoice`, `ZombieStep_*`, `AttackWindup`, `ZombieDeath`, `BodyThud`) multiplient leur volume nominal par `Mathf.Clamp(GameSettings.Zombies / 0.6f, 0.15f, 1.65f)` (ou directement par le facteur d'échelle `GameSettings.Zombies`) afin de respecter immédiatement les préférences du joueur.
3. **Routage de priorité matériel direct** : Avec `maxRealVoices = 32` sur Quest 3, le tri natif `AudioSource.priority` (`0..256`) garantit que la strate **P0 (Survie 360°)** ne peut jamais être évincée par la strate **P3 (Voix distantes)** ou **P4 (Musique)**.

---

## 2. Structure de Bus Virtuelle & Ducking Centralisé (`MusicPlayer.cs`)

Actuellement, `MusicPlayer.cs` utilise un unique `AudioSource` (`ignoreListenerVolume = true`, `priority = 0`) lisant `tracks[0]` et `tracks[1]` en boucle fixe.

### 2.1. Refonte de `MusicPlayer.cs` (Machine à 4 États + Ducking)
* **Correction de priorité** : Passer `AudioSource.priority` de `0` à `200` (Strate P4) pour empêcher la musique de voler une voix matérielle aux alertes P0.
* **Double source pour Crossfade fluide** : Utiliser deux `AudioSource` (`srcA` sur `Musique_Principale_1.mp3` pour les États 1 et 2 ; `srcB` sur `Musique_Principale_2.mp3` pour les États 3 et 4) ou un fondu enchaîné piloté par l'intensité de combat :
  * *État 1 (Tension Latente)* : Volume cible `0.18`, Pitch `0.96`.
  * *État 2 (Assaut Périmétrique)* : Volume cible `0.30`, Pitch `1.00`.
  * *État 3 (Brèche / Incursion < 6 m)* : Transition vers piste 2, Volume cible `0.40`, Pitch `1.00`.
  * *État 4 (`HuntPlayer` / PV < 25 %)* : Piste 2, Volume cible `0.45`, Pitch `1.05` (plafonné à `0.22` tant que `Heartbeat` est actif).
* **API statique de Ducking (`MusicPlayer.Duck(float attenuationFactor, float duration)`)** :
  * Expose une méthode statique légère appelée par `ProcAudio` / `Pistol` / `PlayerHealth` :
    * `PlayerHurt()` : `MusicPlayer.Duck(0.50f, 0.50f)` (`-6 dB` pendant `0.5s`).
    * `Pistolet_Tir` / `ClawSwipe()` : `MusicPlayer.Duck(0.63f, 0.35f)` (`-4 dB` pendant `0.35s`).
    * `WoodCrack()` / `RoofStep()` / `AttackWindup` proche : `MusicPlayer.Duck(0.70f, 0.35f)` (`-3 dB` pendant `0.35s`).
  * Le multiplicateur de ducking remonte progressivement vers `1.0f` via `Mathf.MoveTowards(duckMult, 1f, Time.deltaTime * 1.8f)`.

---

## 3. Budget Mémoire RAM / CPU & Stratégie de Compression (Meta Quest 3)

### 3.1. Empreinte RAM Exacte du Cache Procédural (`ProcAudio.cs`)
Tous les nouveaux clips procéduraux sont synthétisés en **PCM 32-bit float Mono à `44 100 Hz`** (`4 octets / sample`) une seule fois au chargement et conservés en cache statique :

| Clip Procédural (`static AudioClip`) | Durée | Échantillons (`44.1 kHz`) | Taille RAM Exacte |
| :--- | :--- | :--- | :--- |
| `_stepWalker` (`ZombieStep_Walker`) | `160 ms` | `7 056` | `28.22 Ko` |
| `_stepBruiser` (`ZombieStep_Bruiser`) | `240 ms` | `10 584` | `42.34 Ko` |
| `_stepRunner` (`ZombieStep_Runner`) | `110 ms` | `4 851` | `19.40 Ko` |
| `_attackWindup` (`AttackWindup`) | `250 ms` | `11 025` | `44.10 Ko` |
| `_zombieDeath` (`ZombieDeath`) | `550 ms` | `24 255` | `97.02 Ko` |
| `_bodyThud` (`BodyThud`) | `280 ms` | `12 348` | `49.39 Ko` |
| `_meleeSwing` (`MeleeSwing`) | `190 ms` | `8 379` | `33.52 Ko` |
| `_axeImpact` (`AxeImpact`) | `210 ms` | `9 261` | `37.04 Ko` |
| `_crowbarImpact` (`CrowbarImpact`) | `260 ms` | `11 466` | `45.86 Ko` |
| **TOTAL NOUVEAUX CLIPS `ProcAudio`** | **`2.25 s`** | **`99 225`** | **`396.90 Ko` (`< 0.39 Mo`)** |

* **Coût d'initialisation (`Warmup`)** : `< 4.5 ms` sur le processeur Snapdragon XR2 Gen 2 du Meta Quest 3 au chargement de la scène, puis **`0 octet` d'allocation GC** et **`0 %` de coût CPU de synthèse** pendant toute la partie.

### 3.2. Paramètres d'Import des Fichiers `Assets/Audio/`

| Fichier Asset | Load Type | Compression Format | Quality / Channels | Justification Technique |
| :--- | :--- | :--- | :--- | :--- |
| `Musique_Principale_1.mp3`, `Musique_Principale_2.mp3` | `Streaming` | `Vorbis` | `Quality 70%`, Stéréo | Économise ~18 Mo de RAM vive ; seules 2 pistes sont lues simultanément au maximum. |
| `Zombie_Voix_1.mp3`, `Zombie_Voix_2.mp3` | `CompressedInMemory` | `Vorbis` | `Quality 75%`, Forcer Mono (`Force To Mono = true`) | Source 3D pure (`spatialBlend = 1f`) plafonnée à `maxVoicesAtOnce = 4` ; le passage en Mono divise par 2 la taille mémoire sans aucune perte spatiale. |
| `Pistolet_Tir.mp3`, `Pistolet_Rechargement.mp3` | `DecompressOnLoad` | `PCM` / `ADPCM` | Latence nulle | Déclenchement instantané requis au tir VR sans latence de décodage Vorbis. |

---

## 4. Spécification Technique des Effets Visuels & Haptiques Audio-Réactifs

Cette section détaille le câblage exact permettant de résoudre les 3 écarts d'accessibilité (**GAP 1**, **GAP 2**, **GAP 3**) en réutilisant l'infrastructure existante sans créer de nouveau matériau ni de nouveau Canvas.

### 4.1. Extension de `DamageVignette.shader` & `HealthVignette.cs` (Résolution GAP 1 & GAP 2)
* **Exploitation du canal `_Hits[k].w` libre** :
  * Actuellement, `HealthVignette.cs` envoie `Vector4[] hitShader = new Vector4[4]` au shader `SAE501/DamageVignette`, où `xy` = direction 2D normalisée, `z` = intensité (`0..1`), et `w` vaut toujours `0f`.
  * **Convention d'encodage de `_Hits[k].w`** :
    * `w = 0.0` : **Dégât corporel direct (`OnDamageFrom`)** — Couleur rouge sang `float3(0.80, 0.02, 0.02)`, anneau directionnel large (`r > 0.55`).
    * `w = 1.0` : **Alerte acoustique périphérique (`NotifyWarningFrom`)** pour bris de barricade (`WoodCrack`, GAP 1) ou télégraphe d'attaque hors champ (`AttackWindup`, GAP 2) — Couleur ambre/orange haute visibilité `float3(1.00, 0.62, 0.08)`, anneau périphérique plus fin et externe (`r > 0.68`) afin que le joueur distingue immédiatement une **menace/brèche (`ambre`)** d'une **blessure subie (`rouge`)**.
* **Modification du Fragment Shader (`DamageVignette.shader`)** :
  ```hlsl
  float3 alertCol = float3(1.0, 0.62, 0.08);
  float warnMask = 0;
  for (int k = 0; k < 4; k++)
  {
      float intensity = _Hits[k].z;
      if (intensity <= 0.001) continue;
      float isWarn = _Hits[k].w;
      float ringStart = lerp(0.55, 0.68, isWarn);
      float ring = saturate((r - ringStart) / (1.0 - ringStart));
      float2 dir = _Hits[k].xy;
      float align = saturate(dot(pNorm, dir));
      float sharpness = lerp(5.0, 7.5, isWarn);
      float arc = pow(align, sharpness) * ring * intensity;
      if (isWarn > 0.5) warnMask = max(warnMask, arc);
      else              dirMask  = max(dirMask, arc);
  }
  ```
  Puis combiner `col = lerp(blood, alertCol, saturate(warnMask / max(0.001, dirMask + warnMask)))` et `alpha = saturate(max(baseVignette + dirMask * 0.85, warnMask * 0.72))`.
* **API dans `HealthVignette.cs`** :
  * Ajouter `public static void NotifyWarningFrom(Vector3 worldPos, float intensity = 0.65f, float duration = 0.45f)` qui insère une entrée dans `hits[]` avec `isWarning = true` (`w = 1f`), sans augmenter `damageFlash` plein écran ni jouer `ClawSwipe()`.
  * Appelé par :
    1. `Opening.cs` lors d'un `WoodCrack()` sur une barricade hors champ (**GAP 1**, `intensity = 0.60f`, `duration = 0.45s`).
    2. `Zombie.cs` lors du `AttackWindup` si `distance < 2.2f` et que le zombie est hors du cône de vision frontal `dot < 0.35f` (**GAP 2**, `intensity = 0.75f`, `duration = 0.35s`) + pré-alerte haptique asymétrique (`amplitude = 0.35f`, `duration = 0.09s`).

### 4.2. Forçage d'Inspection Munitions sur `DryFire` (`InspectHud.cs` & `Pistol.cs` — Résolution GAP 3)
* Dans `InspectHud.cs` :
  * Ajouter la méthode publique :
    ```csharp
    public void Flash(float duration = 1.5f)
    {
        lastSeen = Mathf.Max(lastSeen, Time.time + duration - linger);
        Refresh();
    }
    ```
* Dans `Pistol.cs` (lorsque le joueur tente de tirer avec `ammo <= 0`) :
  * Appeler `ProcAudio.DryFire(transform.position);`
  * Appeler `GetComponent<InspectHud>()?.Flash(1.5f);` pour forcer l'apparition immédiate du compteur `0 / 12` en rouge pendant `1.5s` même si le poignet VR n'est pas incliné.
  * Envoyer une impulsion haptique sèche (`SendHapticImpulse(0.35f, 0.04f)`) dans la main tenant l'arme.

### 4.3. Détection de Swing & Timbre d'Arme dans `MeleeWeapon.cs`
* Ajouter un identifiant de profil sur `MeleeWeapon.cs` : `public bool isAxe = false;` (ou déduit automatiquement du nom du GameObject / prefab : `name.IndexOf("Axe", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Hache", StringComparison.OrdinalIgnoreCase) >= 0`).
* **Détection de Swing (`Update`)** :
  * Lorsque `speed >= minSpeed * 1.15f` (`~1.84 m/s`) et `Time.time - lastSwingSoundTime >= 0.24f`, appeler `ProcAudio.MeleeSwing(head.position, isAxe)` et déclencher une légère haptique d'élan `Haptic(0.18f, 0.04f)`.
* **Différenciation d'Impact (`Strike`)** :
  * En plus de `ProcAudio.HeadCrack(hitPos)` ou `ProcAudio.Flesh(hitPos)`, appeler `ProcAudio.AxeImpact(hitPos)` si `isAxe` est vrai, sinon `ProcAudio.CrowbarImpact(hitPos)`.
