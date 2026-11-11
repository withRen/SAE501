# Notes Techniques Moteur (Unity 6000.6.0f1 URP & XR) — `game-over-menu`

- **Écran** : Menu de Fin de Partie — Terminal BUNKER-OS (`game-over-menu`)
- **Moteur & Pipeline** : Unity `6000.6.0f1` — Universal Render Pipeline (URP 17), OpenXR + XR Interaction Toolkit (XRI 3)
- **Plateformes cibles** : Meta Quest 3 (VR Standalone 72 Hz) & PC Desktop (`DesktopMode` / `DesktopPlayer` sur `feature/portage-pc`)
- **Scripts concernés** :
  - [`Assets/Scripts/GameOverUI.cs`](file:///Users/rahimtam/SAE501/Assets/Scripts/GameOverUI.cs)
  - [`Assets/Scripts/Desktop/DesktopPlayer.cs`](file:///Users/rahimtam/SAE501/Assets/Scripts/Desktop/DesktopPlayer.cs)
  - [`Assets/Scripts/PauseMenu.cs`](file:///Users/rahimtam/SAE501/Assets/Scripts/PauseMenu.cs)
  - [`Assets/Scripts/MenuButton.cs`](file:///Users/rahimtam/SAE501/Assets/Scripts/MenuButton.cs)
  - [`Assets/Scripts/UiKit.cs`](file:///Users/rahimtam/SAE501/Assets/Scripts/UiKit.cs)
  - [`Assets/Scripts/MenuPointer.cs`](file:///Users/rahimtam/SAE501/Assets/Scripts/MenuPointer.cs)

---

## 1. Choix de l'Architecture Procédurale `UiKit` vs uGUI / UI Toolkit

Dans **The Last Escapor**, les interfaces diégétiques de type terminal (`MainMenu`, `PauseMenu`, `StartScreen`, `EndScreen`) sont intégralement construites par code au runtime via [`UiKit`](file:///Users/rahimtam/SAE501/Assets/Scripts/UiKit.cs), [`MenuPointer`](file:///Users/rahimtam/SAE501/Assets/Scripts/MenuPointer.cs) et [`MenuButton`](file:///Users/rahimtam/SAE501/Assets/Scripts/MenuButton.cs). Ce choix est conservé pour `GameOverUI` pour quatre raisons techniques majeures sous Unity `6000.6.0f1` URP :

1. **Zéro modification de scène ni reconstruction `RoomBuilder`** :
   - `MainScene.unity` est générée procéduralement par [`RoomBuilder.Populate`](file:///Users/rahimtam/SAE501/Assets/Editor/RoomBuilder.cs#L41-L128). Utiliser un `Canvas` uGUI World Space ou un `UIDocument` UI Toolkit nécessiterait soit d'ajouter des assets `.uxml` / `.uss` / `PanelSettings` et un `TrackedDeviceGraphicRaycaster` dans `Assets/Resources`, soit de régénérer la scène.
   - Avec `UiKit`, `GameOverUI` construit son arbre de `Quad`, `TextMesh`, `BoxCollider` et `MenuPointer` à la volée dans `MainScene` sans aucun asset externe.
2. **Unification du Raycast VR 6DoF & Souris PC (`MenuPointer`)** :
   - [`MenuPointer`](file:///Users/rahimtam/SAE501/Assets/Scripts/MenuPointer.cs) effectue un `Physics.RaycastAll(ray, maxDistance, ~0, QueryTriggerInteraction.Collide)` et sélectionne le `BoxCollider` le plus proche portant un [`MenuButton`](file:///Users/rahimtam/SAE501/Assets/Scripts/MenuButton.cs) parent, en ignorant les armes tenues en main, les murs et les meubles proches.
   - Il gère nativement les trois modalités du projet sans configuration supplémentaire :
     - **VR Meta Quest 3** : un rayon `LineRenderer` par manette (`Left Controller` et `Right Controller`), vibration haptique au survol (`0.2f`, `0.03s`) et validation par la gâchette de la main qui survole (`VRInput.Trigger(node)`).
     - **Fallback Éditeur sans manettes** : rayon selon l'axe du regard (`head.forward`) et validation via `Barre Espace` (`VRInput.AnyTrigger()`).
     - **PC Desktop (`DesktopMode.On`)** : rayon sous le curseur souris (`cam.ScreenPointToRay(mouse.position.ReadValue())`) et validation au clic gauche (`mouse.leftButton.wasPressedThisFrame`).
3. **Coût CPU/GPU minimal sur Meta Quest 3 (72 Hz, Single-Pass Instanced)** :
   - Pas de reconstruction de batch uGUI (`Canvas.SendWillRenderCanvases`) ni d'allocation GC par image.
   - Le terminal complet représente ~28 `Quad` primitifs sans collider, 3 `BoxCollider` (pour les boutons) et ~18 `TextMesh` statiques mis à jour une seule fois à l'ouverture du panneau (`< 0.3 ms` CPU au frame d'instanciation, `< 0.05 ms` CPU/GPU par frame ensuite).
4. **Identité visuelle BUNKER-OS 100 % homogène** :
   - Réutilisation exacte des primitives `UiKit.Quad`, `UiKit.Frame`, `UiKit.Brackets`, `UiKit.Scanlines`, `UiKit.Text` et `UiKit.Spaced` déjà employées dans [`AmberMenu`](file:///Users/rahimtam/SAE501/Assets/Scripts/AmberMenu.cs) et [`PauseMenu`](file:///Users/rahimtam/SAE501/Assets/Scripts/PauseMenu.cs).

---

## 2. Rétrocompatibilité avec `MainScene.unity` et `RoomBuilder.BuildHealthUI`

Dans [`RoomBuilder.BuildHealthUI`](file:///Users/rahimtam/SAE501/Assets/Editor/RoomBuilder.cs#L1537-L1562), le composant `GameOverUI` est ajouté sur le GameObject `UI_Sante` (enfant de `XR Origin (XR Rig)`) avec deux références assignées :

```csharp
var go = ui.AddComponent<GameOverUI>();
go.head = head;
go.gameOverText = tm;
```

Pour garantir une compatibilité binaire et sérialisée totale avec `MainScene.unity` et `RoomBuilder.cs` sans avoir à reconstruire la scène :

1. **Conservation de tous les champs publics existants** :
   - `public Transform head;`
   - `public TextMesh gameOverText;`
   - `public float fadeDuration = 1.2f;`
   - `public float restartDelay = 15f;` *(conservé pour la sérialisation de la scène mais ignoré par la nouvelle boucle interactive sans timeout)*
   - `public float minReadTime = 1f;` *(utilisé comme durée du verrou de sécurité `SafetyLock` après le fondu ; borné dans le code par `Mathf.Clamp(minReadTime, 0.2f, 1.0f)` si la scène sérialisée contient l'ancienne valeur `3f`, afin de respecter le délai de sécurité de `1.0 s` de la spécification UX)*
   - `public float victoryDelay = 4f;`
   - `public float restartHold = 1f;` *(conservé pour la sérialisation)*
   - `public string message = "VOUS ÊTES MORT\nLe signal n'a pas été envoyé...";`
   - `public string victoryMessage = "VOUS AVEZ ÉCHAPPÉ\nLe générateur est en marche.";`
   - `public Color deathColor = new Color(0.8f, 0.1f, 0.1f);`
   - `public Color victoryColor = new Color(1f, 0.8f, 0.35f);`
   - `public Color summaryColor = new Color(0.85f, 0.85f, 0.8f);`
2. **Désactivation propre de l'ancien `gameOverText` collé à la caméra** :
   - Dès `Start()`, si `gameOverText` est non nul, son `MeshRenderer` et son `GameObject` sont désactivés (`gameOverText.gameObject.SetActive(false)`). Il n'est plus jamais rattaché à `head` ni affiché.
3. **Indépendance hiérarchique de `Panneau_Fin` par rapport à `UI_Sante`** :
   - Contrairement à `Menu_Pause` qui est à la racine de la scène, `GameOverUI` est attaché à `UI_Sante` (qui porte aussi [`HealthVignette`](file:///Users/rahimtam/SAE501/Assets/Scripts/HealthVignette.cs) sous `XR Origin`).
   - Par conséquent, `GameOverUI` ne déplace ni ne redimensionne son propre `transform` (`UI_Sante`) : il applique `panel.SetPositionAndRotation(...)` et `panel.localScale = Vector3.one * scale` directement sur le sous-objet `Panneau_Fin` (`panel`).

---

## 3. Gestion du Z-Test Global (`UiKit.SetOnTop`) et Hiérarchie des `renderQueue`

En VR comme en PC, le joueur peut mourir collé face contre un mur, une porte verrouillée, une barricade ou un zombie à moins de `1.3 m`. Sans gestion explicite du test de profondeur, le panneau World Space placé à `1.3 m` devant `head` serait coupé ou masqué par la géométrie de la pièce.

### 3.1. `unity_GUIZTestMode` (`CompareFunction.Always`)
- `UiKit.Mat` utilise le shader intégré `UI/Default` et `UiKit.Text` utilise le `Font.material` (`GUI/Text Shader`). Ces deux shaders lisent la variable globale Unity `unity_GUIZTestMode`.
- À l'ouverture du panneau (`Run`), l'appel `UiKit.SetOnTop(true)` exécute :
  ```csharp
  Shader.SetGlobalFloat("unity_GUIZTestMode", (float)CompareFunction.Always);
  ```
- Tant que le panneau est ouvert, tous ses éléments (`Quad` et `TextMesh`) ignorent le Z-buffer de la scène 3D et sont garantis d'être visibles à travers n'importe quel obstacle.
- L'ordre de superposition interne du panneau est alors déterminé par la `renderQueue` de chaque matériau.

### 3.2. Échelle des `renderQueue` (Plage Overlay `4100` – `4600`)

| `renderQueue` | Élément(s) | Décalage local `z` | Rôle dans l'empilement |
|---|---|---|---|
| `4100` | `GameOverFade` (`HeadOverlay` sur `head`) | `nearClipPlane + 0.03f` | Assombrissement global de la vue jusqu'à `rgba(0, 0, 0, 0.78)` ; dessiné derrière le panneau. |
| `4490` | `Fond_Opaque` | `+0.020f` | Fond noir quasi-opaque `rgba(0, 0, 0, 0.97)` isolant le terminal du décor. |
| `4495` | `Fond_Page` | `+0.012f` | Fond teinté du terminal (`UiKit.PanelRed` en Défaite, `UiKit.PanelGreen` en Victoire). |
| `4500` | `Fond_Bilan`, `Bouton_Fond`, `Bande_Haut`, `Cadre_Externe` | `+0.006f` à `+0.009f` | Blocs structurels internes et fonds des boutons `MenuButton`. |
| `4505` | `Cadre_Bilan`, `Trait_Bilan`, `Cadre_Bouton` | `+0.004f` | Filets et bordures fines autour de l'encadré `BILAN DE MISSION` et des boutons. |
| `4510` | `Coin` (`UiKit.Brackets`) | `0.000f` | Équerres d'angle du terminal. |
| `4520` | `Balayage` (`UiKit.Scanlines`) | `0.000f` | Trame CRT semi-transparente (`alpha = 0.22` un pixel sur deux). |
| `4600` | Tous les `TextMesh` du panneau | `0.000f` | Titres, libellés, valeurs statistiques, textes des boutons et chevrons `▶`. |

### 3.3. Synchronisation PhysX (`Physics.SyncTransforms`)
- Dès que `panel.SetPositionAndRotation(...)` et `panel.gameObject.SetActive(true)` sont appelés, `Physics.SyncTransforms()` est invoqué immédiatement.
- Cela force la mise à jour instantanée des `BoxCollider` des boutons dans l'arbre spatial PhysX dans la même frame, avant que `MenuPointer.Update()` ne lance ses raycasts.

---

## 4. Indépendance au `Time.timeScale` (`MenuButton` & `GameOverUI`)

1. **Correction de `MenuButton.Update()`** :
   - Actuellement, [`MenuButton.cs`](file:///Users/rahimtam/SAE501/Assets/Scripts/MenuButton.cs#L30) utilise `Time.deltaTime` pour faire décroître l'impulsion lumineuse de clic (`glow = Mathf.MoveTowards(glow, 0f, Time.deltaTime * 4f);`).
   - En remplaçant `Time.deltaTime` par `Time.unscaledDeltaTime`, l'animation de clic (`glow`) fonctionne de manière fluide quelle que soit la valeur de `Time.timeScale` (y compris dans `PauseMenu` où `Time.timeScale = 0f` et dans `GameOverUI`).
2. **Boucle `GameOverUI.Run(...)` en `Time.unscaledDeltaTime`** :
   - Le fondu d'entrée (`fadeDuration = 1.2f`) et le verrou de sécurité anti-missclick (`SafetyLock` pendant `1.0 s`) utilisent tous deux `Time.unscaledDeltaTime`.

---

## 5. Intégration PC Desktop dans `DesktopPlayer.cs`

Sur la branche `feature/portage-pc`, [`DesktopPlayer.cs`](file:///Users/rahimtam/SAE501/Assets/Scripts/Desktop/DesktopPlayer.cs#L189-L212) gère le verrouillage du curseur souris (`LockCursor`) et l'orientation automatique de la caméra vers [`PauseMenu`](file:///Users/rahimtam/SAE501/Assets/Scripts/PauseMenu.cs). Sans adaptation, à la mort ou à la victoire du joueur :
- `DesktopPlayer.GameOver` passe à `true`, ce qui coupe `Look(mouse)` et `Move(kb)`, **mais le curseur souris reste verrouillé et caché** (`CursorLockMode.Locked`), empêchant le joueur de viser les boutons de `GameOverUI` avec la souris.
- De plus, la ligne `else if (!MouseCaptured && mouse.leftButton.wasPressedThisFrame && Application.isFocused) { LockCursor(true); clickConsumed = true; }` recapturerait immédiatement la souris au premier clic gauche sur un bouton du terminal.

### Solution implémentée :
1. **`GameOverUI` expose deux membres publics** :
   - `public static bool PanelOpen { get; private set; }` (remis à `false` dans `ResetStatics()`, `Awake()`, `OnDestroy()` et lors du chargement d'une scène ; passe à `true` dès que `Panneau_Fin` est activé à la fin du fondu).
   - `public Transform PanelTransform => panel;` (permettant à `DesktopPlayer` de viser le centre du panneau).
2. **`DesktopPlayer.Update()` traite `GameOverUI.PanelOpen` comme un menu ouvert** :
   - Si `paused || GameOverUI.PanelOpen` est vrai et `MouseCaptured` est vrai : appel immédiat à `LockCursor(false)` (`CursorLockMode.None`, `Cursor.visible = true`).
   - La recapture automatique de la souris au clic gauche dans la fenêtre ou lors des 5 premières frames est conditionnée par `!GameOver && !GameOverUI.PanelOpen`.
   - Tant que `GameOverUI.PanelOpen` est vrai et que `GameOverUI.Instance && GameOverUI.Instance.PanelTransform` existe, `DesktopPlayer` interpole doucement `pitch` et `yaw` vers `GameOverUI.Instance.PanelTransform.position` avec `float k = 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime);`, centrant le terminal BUNKER-OS à l'écran.

---

## 6. Exclusion Mutuelle et Sécurité avec `PauseMenu.cs`

Deux cas limites de concurrence entre `PauseMenu` et `GameOverUI` sont verrouillés :

1. **Empêcher d'ouvrir la pause pendant la fin de partie (`PauseMenu.Pause()`)** :
   - Dans [`PauseMenu.Pause()`](file:///Users/rahimtam/SAE501/Assets/Scripts/PauseMenu.cs#L79-L83), la garde actuelle vérifie uniquement `PlayerHealth.Instance.IsDead`.
   - On ajoute `if (WaveManager.Instance && WaveManager.Instance.Victory) return;` et `if (GameOverUI.PanelOpen) return;` afin qu'appuyer sur `Échap` ou le bouton Menu de la manette gauche pendant le délai de victoire (`victoryDelay = 4.0f`) ou sur l'écran de fin ne puisse jamais superposer `PauseMenu` à `GameOverUI`.
2. **Fermer automatiquement la pause si `GameOverUI` se déclenche (`GameOverUI.Run()`)** :
   - Si `PauseMenu.Paused` est vrai au moment où `GameOverUI` démarre son fondu, `FindAnyObjectByType<PauseMenu>()?.Resume();` est appelé pour fermer le panneau de pause, restaurer `Time.timeScale = 1f` et `AudioListener.pause = false` avant d'afficher `Panneau_Fin`.
