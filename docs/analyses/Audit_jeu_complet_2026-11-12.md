# Audit du reste du jeu (solo, VR, PC, build) — 12 novembre 2026

Demande de Rahim : « Puis recherche si il y a d'autres bugs important également à corriger » (après l'audit multijoueur).
Audit en **lecture seule** fait par l'IA (Claude Code) sur `main` (`aca05d2`), hors multijoueur (déjà audité : `Audit_multijoueur_2026-11-12.md`). Aucun code modifié.

Légende : **BLOQUANT** / **MAJEUR** / **MINEUR**. « Lu dans le code » = chemin vérifié en lisant l'appelant et l'appelé. « Vu dans les logs » = trace réelle dans `Logs/Editor.log` (56 Mo, session du 12/11).

## Résumé

| # | Sévérité | Zone | Problème | Statut |
|---|---|---|---|---|
| 1 | MAJEUR | Radio de ravitaillement (VR) | Le bouton ne réagit qu'au grip, main vide : la gâchette, le simple toucher et une main qui tient déjà un objet ne font rien | Lu dans le code |
| 2 | MAJEUR (BLOQUANT depuis un clone) | Build Quest | Réglages de build seulement en local : permission micro retirée (sinon « Launch app » grisé dans MQDH), OpenGLES3 retiré (build 2 à 3 fois plus long) | Lu (`git diff`) |
| 3 | MINEUR | Objets | Un objet lancé hors de portée (dehors par une fenêtre, derrière une porte de zone fermée) n'est jamais rendu ; seuls les objets tombés sous le sol reviennent | Lu dans le code |
| 4 | MINEUR | Évasion | Ouvrir le portail avant la 1re vague (outil de test / passage direct au niveau 4) provoque une erreur à chaque image dans `WaveManager.UpdateWave` | Vu dans les logs (test) |
| 5 | MINEUR | Version PC | `DesktopHud.Update` plante à chaque image si le joueur PC est détruit (aucun contrôle) | Vu dans les logs (test) |
| 6 | MINEUR | Ceinture (XRI) | « routine is null » dans `XRSocketInteractor.OnEnable/OnDisable` (18 fois), venant du paquet XRI, sans effet visible | Vu dans les logs |
| 7 | INFO | Pause en multijoueur | Le temps ne s'arrête pas (voulu), mais on peut mourir menu ouvert | Lu dans le code |

**Rien de bloquant pour un build fait depuis `~/SAE501`.** Le point 2 devient bloquant pour un build fait par quelqu'un d'autre depuis GitHub.

## Détails

### 1. MAJEUR — La radio « ne marche pas » au casque
- **Où** : `Assets/Scripts/SupplyRadio.cs:46` (`button.selectEntered` seulement) ; `Assets/Editor/RoomBuilder.cs:1692-1694` (bouton = `XRSimpleInteractable`, zone de 11 × 11 × 7,5 cm) ; `Assets/Scripts/ClickGrab.cs` (prise par clic : un appui de grip sur une main qui tient déjà un objet le **lâche** au lieu d'appuyer sur le bouton).
- **Pourquoi pas sur PC** : `DesktopPlayer` sélectionne directement l'objet visé au clic, quelle que soit la main ; le chemin VR (grip de la manette, main libre, toucher la zone) n'existe pas.
- **Scénario** : au casque, pistolet dans la main droite, le joueur appuie sur le bouton rouge avec la gâchette, ou en le touchant du doigt, ou avec le grip de la main qui tient le pistolet : rien ne se passe (ou il lâche son arme).
- **Correctif** : appeler aussi `Call()` sur `button.activated` (gâchette) et sur `hoverEntered` d'une main (toucher le bouton) ; l'écran peut dire « TOUCHE LE BOUTON ». À tester au casque.
- Rappel : un seul appel par vague ; appelée avant la 1re vague, la radio reste « INDISPONIBLE » pendant la vague 1 (voulu, mais peut passer pour une panne).

### 2. MAJEUR — Réglages de build non commités
- **Où** : modifications locales de Rahim : `Assets/Plugins/Android/AndroidManifest.xml` (retrait de `RECORD_AUDIO`, journal 197), `Assets/Settings/Build Profiles/Meta Quest.asset` et `ProjectSettings/ProjectSettings.asset` (Vulkan seul, journal 196).
- **Scénario** : un membre du groupe clone `main` et construit l'APK : la permission micro revient (« Launch app » grisé dans MQDH, triangle orange) et le build recompile les shaders GLES3 (30-40 min au lieu de ~15).
- **Correctif** : commiter ces deux changements (sans les réglages URP ni `DevAgentSettings`, que Rahim garde en local), si Rahim est d'accord.

### 3. MINEUR — Objet perdu hors de portée
- **Où** : `Assets/Scripts/ItemSafety.cs:43` (retour seulement si l'objet passe sous `killY`).
- **Scénario** : le marteau lancé dehors par une fenêtre ou derrière une porte de zone encore fermée : il reste là, impossible à récupérer avant l'ouverture de la zone (le pistolet et le chargeur, eux, peuvent aussi y finir).
- **Correctif** : si un objet de la ceinture (pistolet) ou le marteau reste hors du NavMesh joignable plus de 10 s, le renvoyer à son holster / établi.

### 4. MINEUR — Évasion ouverte sans vague en cours
- **Où** : `Assets/Scripts/WaveManager.cs:297-311` (`cur` nul si aucune vague n'a démarré).
- **Scénario** : seulement si le générateur est activé avant la 1re vague (outil de test, `GeneratorActivated` forcé). En jeu normal le générateur n'est utilisable qu'à la vague 4.
- **Correctif** : `if (cur == null) return;` en tête de `UpdateWave`.

### 5. MINEUR — HUD PC sans joueur
- **Où** : `Assets/Scripts/Desktop/DesktopHud.cs:166-171` (`player` utilisé sans contrôle).
- **Scénario** : le rig PC est détruit (changement de scène raté, test) : 1 erreur par image (≈ 7 900 dans le log de la session de tests).
- **Correctif** : `if (!player) return;` en tête du bloc réticule.

### 6. MINEUR — Erreurs du paquet XRI sur la ceinture
- **Où** : `XRSocketInteractor.OnEnable/OnDisable` (paquet XRI 3.6), « routine is null », 18 fois.
- **Effet** : aucun visible (les holsters marchent). À ignorer, ou mettre à jour XRI après le rendu.

### 7. INFO — Pause en réseau
- **Où** : `Assets/Scripts/PauseMenu.cs:97-101` : en multijoueur, la pause n'arrête pas le temps (voulu, pour les autres joueurs). Le joueur peut être tué menu ouvert. À dire au jury plutôt qu'à corriger.

## Vérifié sans problème

- Vagues : un zombie bloqué meurt seul après 20 s (une vague finit toujours) ; les zombies apparaissent sur le NavMesh (`ZombieSpawner` : recherche à 1 m puis 4 m).
- Sons générés : mis en cache (champs statiques), pas d'allocation à chaque zombie ou tir.
- Aucun `Find…` dans les `Update` / `LateUpdate` du jeu.
- Pause : `Time.timeScale` et `AudioListener.pause` toujours remis (reprise, retour au menu, destruction du menu).
- Shaders des HUD inclus d'office dans le build.
- **Logs** : les ≈ 39 700 `NullReferenceException` du log viennent d'un test de l'IA qui avait détruit le rig du joueur exprès (spectateur, 12/11) : `AffordanceStateReceiver` (XRI), `DesktopHud:171`, `JumpGuard:69`. Aucune erreur du jeu en dehors de ces tests.

## Non vérifié

- Rien n'a été testé au casque ; le point 1 est déduit du code (le bouton marche sur PC).
- Performances non mesurées (pas de profilage au casque dans cette analyse).
