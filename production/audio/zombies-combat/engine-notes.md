# Notes Moteur Unity 6 — Combat Zombies (`zombies-combat`)

**Projet** : The Last Escapor
**Moteur Épinglé** : Unity `6000.6.0f1` (URP, OpenXR 1.18, XR Interaction Toolkit 3.6, Single-Pass Instanced Stereo sur Meta Quest 3 + Mode PC `DesktopMode.On`)
**Étape du Pipeline** : Étape 3 — Engine Validation (`unity-specialist`)

---

## 1. Cycle de Vie du Cache `ProcAudio.cs` & Enter Play Mode Options (Unity `6000.6.0f1`)

En Unity 6 (`6000.6.0f1`), lorsque l'option *Enter Play Mode without Domain Reload* est activée dans l'éditeur, les champs `private static AudioClip` conservant des références natives vers des objets Unity détruits ou non réinitialisés peuvent provoquer des exceptions `MissingReferenceException` ou des fuites mémoire entre deux sessions Play.

### Règles d'Implémentation Idiomatiques dans `ProcAudio.cs`
1. **Réinitialisation statique via `SubsystemRegistration`** :
   ```csharp
   [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
   static void ResetStatics()
   {
       _stepWalker = null;
       _stepBruiser = null;
       _stepRunner = null;
       _attackWindup = null;
       _zombieDeath = null;
       _bodyThud = null;
       _meleeSwing = null;
       _axeImpact = null;
       _crowbarImpact = null;
   }
   ```
2. **Pré-allocation au chargement (`AfterSceneLoad`)** :
   Ajouter une méthode `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] public static void Warmup()` qui force la synthèse des 9 nouveaux clips dès le chargement de la scène. Cela garantit que le premier coup de hache, le premier pas de Coureur ou la première mort de zombie en VR ne provoque **aucune allocation `float[]`** ni appel `AudioClip.Create` sur le thread principal pendant le rendu à 72/90 Hz.
3. **Drapeau `hideFlags`** : Marquer chaque `AudioClip` généré avec `clip.hideFlags = HideFlags.DontSave;` pour éviter toute sérialisation accidentelle dans les scènes de l'éditeur.

---

## 2. Optimisation des `AudioSource` par Entité `Zombie` (Zéro Instanciation par Pas)

Actuellement, `ProcAudio.PlayAt(AudioClip, Vector3, ...)` crée un `new GameObject("ProcSfx")` muni d'un `AudioSource` et planifie `Object.Destroy(go, duration)` à chaque appel.
* Si ce pattern est acceptable pour des événements ponctuels (`WoodCrack`, `HeadCrack`, `AxeImpact`, `BodyThud`), **il est proscrit pour les bruits de pas (`ZombieStep_*`)** : 8 zombies marchant simultanément généreraient ~18 `GameObject` créés et détruits par seconde, causant une fragmentation du tas managé et une surcharge du graphe de transformation d'Unity 6.

### Architecture à 2 `AudioSource` Réutilisés par Zombie
Chaque instance de zombie réutilise exactement **2 composants `AudioSource`** persistants pendant toute sa durée de vie :
1. **`voiceSrc` (géré par `ZombieVoice.cs`)** :
   * Dédié aux boucles et appels vocaux (`Zombie_Voix_1/2.mp3`).
   * `spatialBlend = 1.0f`, `rolloffMode = AudioRolloffMode.Linear`.
   * Paramètres dynamiques assignés à l'initialisation selon l'archétype (`Walker`, `Bruiser`, `Runner`) : `minDistance`, `maxDistance`, `priority` (`64`, `80` ou `110`).
2. **`sfxSrc` (ajouté / caché sur `Zombie.cs`)** :
   * Dédié aux sons locomoteurs et d'action du zombie : `ZombieStep_*`, `AttackWindup` et `ZombieDeath`.
   * Utilise `sfxSrc.PlayOneShot(clip, volumeScale)` après ajustement de `sfxSrc.pitch`.
   * À la mort du zombie (`Die()`), avant que le GameObject du zombie ne soit détruit ou désactivé, `ZombieDeath` et `BodyThud` (+`0.35s`) peuvent utiliser soit `sfxSrc` (si le cadavre reste présent `> 0.9s`), soit un unique appel détaché à `ProcAudio` pour ne jamais être coupés par la destruction du GameObject.

---

## 3. Compatibilité Single-Pass Instanced Stereo (`DamageVignette.shader`)

Sur Meta Quest 3 (Unity 6 URP avec OpenXR), le rendu stéréoscopique utilise **Single-Pass Instanced Rendering** (les deux yeux sont dessinés en un seul appel de rendu via `SV_InstanceID`).

### Vérifications dans `Assets/Shaders/DamageVignette.shader`
* Les macros `UNITY_VERTEX_INPUT_INSTANCE_ID`, `UNITY_VERTEX_OUTPUT_STEREO`, `UNITY_SETUP_INSTANCE_ID(v)` et `UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o)` sont déjà présentes dans le Vertex Shader.
* Lors de l'ajout de la branche conditionnelle sur `_Hits[k].w` (distinction entre `0.0` = dégât rouge et `1.0` = alerte ambre GAP 1 / GAP 2) dans le Fragment Shader (`frag(v2f i)`), ajouter `UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);` en tête de `frag` pour respecter strictement la norme URP Unity 6.
* Le tableau uniforme `float4 _Hits[4]` est partagé entre les deux yeux dans le Constant Buffer global/matériau (`SetVectorArray("_Hits", hitShader)`), ce qui ne pose aucun conflit d'instanciation stéréo puisque le quad `HealthVignette` est enfant direct de la caméra (`localPosition = (0, 0, 0.42)`).

---

## 4. Adaptation Bimodale VR (`OpenXR`) vs PC (`DesktopMode.On`) & `GameSettings`

### 4.1. Adaptation Automatique du `spatialBlend` des Armes
En VR, l'arme (`Pistol.cs` ou `MeleeWeapon.cs`) est tenue au bout du bras réel du joueur : un `spatialBlend` à `1.0f` crée un panoramique trop brutal dans une seule oreille lorsque le joueur tient son arme sur le côté, tandis qu'un `spatialBlend` à `0.0f` détruit la sensation de présence de la main. Sur PC (`DesktopMode.On`), l'arme est attachée à la caméra.
* **Formule standardisée pour les armes tenues en main** (`Pistolet_Tir`, `Pistolet_Rechargement`, `DryFire`, `MeleeSwing`) :
  ```csharp
  float weaponSpatialBlend = DesktopMode.On ? 0.20f : 0.65f;
  ```

### 4.2. Respect Strict du Multiplicateur `GameSettings.Zombies`
`GameSettings.cs` expose `public static float Zombies = 0.6f;` (plage de curseur `0.0 .. 1.0`).
* Dans `ZombieVoice.cs`, le volume est déjà modulé par `GameSettings.Zombies`.
* Tous les nouveaux sons liés aux zombies (`ZombieStep_Walker`, `ZombieStep_Bruiser`, `ZombieStep_Runner`, `AttackWindup`, `ZombieDeath`, `BodyThud`) doivent appliquer un facteur d'échelle cohérent :
  ```csharp
  float zombieVolScale = Mathf.Clamp(GameSettings.Zombies / 0.6f, 0f, 1.65f);
  ```
  Si le joueur met le curseur `Zombies` à `0`, tous les pas, voix, télégraphes et râles s'éteignent proprement (tandis que les indicateurs visuels ambre de `HealthVignette` pour GAP 1 et GAP 2 restent pleinement actifs).
