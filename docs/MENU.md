# Menu d'accueil

Scène **`Assets/Scenes/Menu.unity`**, 1re scène du jeu. Construite par **SAE501 → Build Menu Scene** (`Assets/Editor/MenuBuilder.cs`).
« Build VR Scene » garde le menu en 1re position dans les Build Settings.

## Décor (ambiance animée)

- Le joueur est debout sur un quai mouillé. L'image `Assets/Menu/MenuBackground.png` (ville en ruine sous la pluie) est posée sur un **grand écran courbé** au loin (14 m, 110°), comme un horizon.
- Autour : **pluie** (particules), **brouillard** qui dérive, **néon vert** « HORS SERVICE » qui grésille et se reflète sur le sol, **éclairs** (le décor flashe, puis le tonnerre gronde), son de pluie. Sons générés par code (`MenuAmbience.cs`).

## Menu (style « terminal de bunker », ambre)

| Bouton | Action |
|---|---|
| SOLO | fondu au noir, charge `MainScene` |
| MULTIJOUEUR | grisé « BIENTÔT » (à brancher si le groupe a le temps) |
| PARAMÈTRES | son (musique, effets), confort VR (rotation par à-coups / fluide, bords sombres, recentrer la vue), difficulté (facile / normal / difficile) |
| QUITTER | ferme le jeu |

- Pointage : un **rayon** part de chaque manette (`MenuPointer.cs`), le bouton visé s'allume avec une petite vibration, **la gâchette** valide. Sans manettes (éditeur) : le regard sert de rayon, **barre espace** = gâchette.
- Les réglages sont enregistrés (`PlayerPrefs`) et appliqués à chaque scène (`GameSettings.cs`) :
  - Musique / Effets : `MusicPlayer.volume` et `AudioListener.volume` ;
  - Rotation : `ControllerInputActionManager.smoothTurnEnabled` ;
  - Difficulté : nombre de zombies par vague (×0,75 / ×1 / ×1,3), temps entre deux apparitions (×1,25 / ×1 / ×0,8), vitesse des zombies (×0,85 / ×1 / ×1,15).

Fichiers : `MainMenu.cs`, `MenuButton.cs`, `MenuPointer.cs`, `MenuAmbience.cs`, `GameSettings.cs`, `UiKit.cs`, `VRInput.cs`, `Editor/MenuBuilder.cs`.
