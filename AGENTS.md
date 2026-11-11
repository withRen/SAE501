# Game Studios — Directives de projet & Coordination des agents

Projet : **The Last Escapor** (VR Meta Quest 3, Unity 6000.6.0f1)
Cadre : SAÉ 5.D.01 « Dispositifs interactifs » (MMI 3e année) — Rendu le 13 novembre 2026.

---

## 1. Protocole de collaboration

- **Collaboration pilotée par l'utilisateur** : Toujours suivre le cycle *Question -> Options -> Décision -> Draft -> Approbation*.
- **Validation avant écriture** : Ne jamais modifier les fichiers clés sans confirmation préalable.
- **Continuité de session** : Le point de contrôle principal est `production/session-state/active.md`. Le mettre à jour après chaque phase de travail.

---

## 2. Règles spécifiques au projet Unity

1. **Scène générée par code** :
   - `MainScene` est construite par script (`Assets/Editor/RoomBuilder.cs` et `SetupVRScene.cs`) via le menu **SAE501 → Build VR Scene**.
   - Ne jamais modifier les objets de la scène à la main dans l'éditeur Unity (les modifications seraient écrasées lors du prochain build de scène). Modifier les scripts et prefabs.
2. **Confort & VR (Meta Quest 3)** :
   - Cible obligatoire : **72 FPS constants**.
   - Échelle 1:1 (1 unité Unity = 1 mètre).
   - Pas de translation de caméra imposée.
3. **Journal des prompts (`docs/PROMPTS.md`)** :
   - Conformément aux consignes de la SAÉ, chaque intervention IA doit être consignée en bas de `docs/PROMPTS.md` avec numéro, date, domaine, prompt exact, et résumé sincère du résultat.
4. **Wiki GitHub** :
   - Chaque nouvelle mécanique ou équilibrage doit faire l'objet d'une page explicative sur le wiki du projet (`withRen/SAE501.wiki`).

---

## 3. Rôles et Spécialistes mobilisables

- `unity-specialist` : Pour l'architecture Unity 6, URP, Addressables, et l'optimisation mobile Quest 3.
- `game-designer` : Pour l'équilibrage des vagues, PV des zombies et sensation de jeu (game feel).
- `lead-programmer` : Pour la robustesse des systèmes C#, interfaces et séparation des responsabilités.
- `qa-tester` : Pour la vérification des cas limites (franchissement des fenêtres, rechargement d'armes, bugs d'interaction VR).
