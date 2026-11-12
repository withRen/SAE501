# Audit du multijoueur (duel 1v1 et coop) — 12 novembre 2026

> **Mise à jour (même jour, journal 212, issue #163, branche `hotfix/163-audit-multijoueur`)** : les points 1 à 5 sont **corrigés**, ainsi que les mineurs 6 et 9. Chaque correction a été testée en mode Play dans l'éditeur (hôte + joueurs simulés), pas encore au casque. Colonne « Statut » ci-dessous.

Demande de Rahim : « Analyse la totalité du mode 1V1 et des modes coop, recherche les bugs possibles ».
Audit en **lecture seule** fait par l'IA (Claude Code) sur `main` (`d2129e5`), plus les fichiers locaux non commités de Rahim. Aucun code modifié.

Légende : **BLOQUANT** (empêche de jouer ou de faire la démo), **MAJEUR** (bug visible en partie, à corriger avant le rendu), **MINEUR** (gênant ou rare).
« Lu dans le code » = chemin vérifié en lisant l'appelant et l'appelé, pas reproduit en jeu. « Vu en test » = observé en mode Play pendant les hotfix #155 à #162.

## Résumé

| # | Sévérité | Mode | Problème | Statut |
|---|---|---|---|---|
| 1 | MAJEUR | Coop | Tir ami : les balles blessent et tuent les coéquipiers | Corrigé |
| 2 | MAJEUR | Duel | Si l'adversaire quitte, l'hôte reste seul dans l'arène, sans fin de match | Corrigé |
| 3 | MAJEUR | Lobby / Duel | Passer le lobby en mode duel avec 3 ou 4 joueurs déjà présents lance un duel à 3 ou 4 | Corrigé |
| 4 | MAJEUR | Coop | Un joueur mort (spectateur) peut encore frapper les zombies, tirer le levier du générateur, clouer des planches | Corrigé |
| 5 | MAJEUR | Tous | Le correctif du délai de 12 s (lobby) n'est pas commité : un build fait depuis GitHub renvoie les clients au menu pendant le chargement | Corrigé (commité) |
| 6 | MINEUR | Coop Évasion | Le joueur mort pendant l'évasion lit « Réapparition à la fin de la vague » alors qu'il n'y en a plus | Corrigé |
| 7 | MINEUR | Coop Évasion | Un joueur sorti par le portail peut rentrer et reste invisible pour les zombies | Ouvert |
| 8 | MINEUR | Duel | Morts simultanées : le point va au premier message reçu, pas d'égalité | Ouvert |
| 9 | MINEUR | Duel | La régénération de vie (+25 PV après 10 s) marche en duel, 2 fois par match seulement | Corrigé |
| 10 | MINEUR | Coop | Fin de partie : « QUITTER » de l'hôte affiche « Connexion à l'hôte perdue » aux clients | Ouvert |
| 11 | MINEUR | Coop | Attribution des kills : un zombie qui achève un joueur touché par un coéquipier crédite le coéquipier | Ouvert |
| 12 | MINEUR | VR | Spectateur : deuxième rendu de la scène au casque (perf. à surveiller) | Ouvert |
| 13 | MINEUR | VR | Tête dans un mur en continu : fondus noirs répétés | Ouvert |
| 14 | MINEUR | Lobby | Noms longs (20 caractères en majuscules) : risque de débordement dans la ligne du lobby | Ouvert |
| 15 | INFO | Tous | Autorité client sur sa propre vie et sa propre mort (triche triviale) | Ouvert |

Aucun BLOQUANT trouvé pour un build fait depuis `~/SAE501` (qui contient le correctif du point 5). **Le point 5 devient BLOQUANT pour un build fait depuis un clone propre de GitHub.**

## Détails

### 1. MAJEUR — Tir ami en coop
- **Où** : `Assets/Scripts/Net/NetPlayer.cs:388-391` (zones touchables `PlayerHitbox` ajoutées à tous les avatars, coop compris) ; `Assets/Scripts/Pistol.cs:197-198` (toute `PlayerHitbox` touchée prend les dégâts PvP) ; `NetPlayer.HitRpc` (applique le coup sans regarder le mode).
- **Scénario** : coop, deux joueurs, un zombie entre eux ; J1 tire et touche J2 → J2 perd 20 PV (corps) ou 50 (tête), 100 au sniper ; il peut mourir, le fil affiche « J1 a éliminé J2 », croix rouge chez J1.
- **Correctif** : dans `PlayerHitbox.Hit` (ou `NetPlayer.HitRpc`), ignorer le coup si `!ArenaMaps.InArena` (coop) ; ou ne pas créer les `PlayerHitbox` hors arène.
- Lu dans le code (chemin direct, aucun contrôle de mode).

### 2. MAJEUR — Duel : l'adversaire se déconnecte
- **Où** : `Assets/Scripts/Net/NetDuel.cs` (aucun abonnement à `OnClientDisconnectCallback`).
- **Scénario** : duel, le client quitte (menu pause, casque éteint, Wi-Fi) → l'hôte reste dans l'arène ; la manche ne finit jamais, pas de victoire par forfait ; il doit passer par le menu pause pour sortir. (Si c'est l'hôte qui part, le client revient bien au menu avec un message.)
- **Correctif** : dans `NetDuel`, côté serveur, sur déconnexion d'un joueur pendant le match : `Winner = côté restant`, `FinDeMatch`, « VICTOIRE PAR FORFAIT », puis `EndMatch()`.
- Lu dans le code.

### 3. MAJEUR — Duel lancé avec plus de 2 joueurs
- **Où** : `Assets/Scripts/Net/NetSession.cs:61` (la limite de 2 n'est vérifiée qu'à la connexion) ; `Assets/Scripts/Net/NetLobby.cs` `SetMode` (ne retire personne).
- **Scénario** : l'hôte ouvre un lobby coop, 3 casques rejoignent, l'hôte choisit « DUEL 1V1 » → les 3 sont prêts → duel à 3 : deux joueurs du même côté (`NetLobby.Side` donne le côté 2 à tous sauf le premier), score faussé.
- **Correctif** : interdire le mode duel tant qu'il y a plus de 2 joueurs (bouton grisé + message), ou ne pas pouvoir lancer (`AllReady` faux) si `IsDuel1v1 && Players.Count > 2`.
- Lu dans le code.

### 4. MAJEUR — Coop : le joueur mort agit encore
- **Où** : `Assets/Scripts/Net/NetSpectator.cs` `Freeze` (coupe seulement le déplacement) ; `Assets/Scripts/MeleeWeapon.cs:111` (pas de contrôle « mort ») ; le levier du générateur, le marteau et les planches non plus. Le pistolet, lui, est bloqué (`Pistol.Fire`, hotfix #160).
- **Scénario** : J2 meurt avec la hache en main ; en spectateur il agite la main : il tue des zombies proches de son corps ; il peut aussi tenir le levier du générateur (évasion déclenchée par un mort) ou clouer des planches.
- **Correctif** : à la mort en réseau, lâcher ce qui est tenu et désactiver les interacteurs des mains (`XRBaseInteractor.enabled = false`) jusqu'à `Revived` ; ou contrôle `PlayerHealth.Instance.IsDead` dans `MeleeWeapon`, `Lever`, `Hammer`.
- Lu dans le code.

### 5. MAJEUR (BLOQUANT depuis un clone propre) — Correctif du délai de 12 s non commité
- **Où** : `Assets/Scripts/MainMenuMulti.cs:303-305`, modification locale de Rahim (thread « Plan multijoueur local »).
- **Scénario** : build fait depuis GitHub (`main`) : au lancement de la partie, le lobby disparaît pendant que le Quest charge la scène (souvent > 12 s) → le client se croit déconnecté et revient au menu.
- **Correctif** : commiter ce changement (3 lignes) sur `main`.
- Lu dans le code (`git diff`).

### 6. MINEUR — Message de réapparition faux pendant l'évasion
- **Où** : `NetPlayer.OnLocalDied` / `DiedRpc` (« Réapparition à la fin de la vague ») ; `NetSpectator` (même texte).
- **Scénario** : coop Évasion, après l'ouverture du portail un joueur meurt : il lit qu'il réapparaîtra, mais il n'y a plus de fin de vague (la victoire viendra des autres).
- **Correctif** : si `WaveManager.Instance.Escaping`, afficher « Tes coéquipiers doivent passer le portail ».

### 7. MINEUR — Joueur sorti qui revient dans la carte
- **Où** : `PlayerHealth.Escaped` (posé une fois, jamais retiré) ; `NetGame.EscapeRpc`.
- **Scénario** : J1 passe le portail, revient dans le jardin ou le bunker : les zombies l'ignorent toujours ; il peut tirer sans risque.
- **Correctif** : retirer `Escaped` si le joueur repasse le portail dans l'autre sens, ou le figer dehors (zone de sortie).

### 8. MINEUR — Duel : morts simultanées
- **Où** : `NetDuel.HandlePlayerDeath` (le premier message gagne).
- **Scénario** : les deux joueurs se tuent dans la même image : le point va à celui dont la mort arrive en second à l'hôte, souvent l'hôte (pas de délai réseau).
- **Correctif** : accepter une seconde mort pendant ~0,2 s après la première et déclarer « ÉGALITÉ » (aucun point, manche rejouée).

### 9. MINEUR — Régénération en duel
- **Où** : `PlayerHealth.Update` (régénération +25 PV après 10 s sans dégât, 2 fois par « vague ») ; `NewWave()` n'est jamais appelé en duel.
- **Scénario** : un joueur blessé se cache 10 s et récupère 25 PV, deux fois par match au total (pas par manche).
- **Correctif** : en duel, désactiver la régénération (`maxRegenPerWave = 0`) ou appeler `NewWave()` à chaque manche, selon la règle voulue.

### 10. MINEUR — « Connexion à l'hôte perdue » après une fin de partie coop
- **Où** : `Assets/Scripts/GameOverUI.cs:443` (`NetSession.Leave()` puis chargement du menu, sans prévenir les clients).
- **Scénario** : fin de partie coop, l'hôte clique QUITTER avant les clients : ils reviennent au menu avec « Connexion à l'hôte perdue ».
- **Correctif** : comme pour le duel (#157), message `LeaveToMenu` envoyé aux clients avant de fermer.

### 11. MINEUR — Kill attribué au mauvais joueur
- **Où** : `NetPlayer.OnLocalDied` (dernier tireur dans les 5 s).
- **Scénario** : coop, J1 touche J2 (tir ami, point 1), un zombie l'achève 3 s après : « J1 a éliminé J2 ». Disparaît avec la correction du point 1.

### 12. MINEUR — Coût du spectateur au casque
- **Où** : `NetSpectator` (caméra 960×540, sans ombres).
- **Risque** : pendant qu'on est mort, une deuxième image de la scène par image ; possible chute sous 72 i/s sur Quest 3 dans les grandes zones. Non mesuré.
- **Correctif si besoin** : 640×360, une image sur deux (`view.enabled` alterné), distance de rendu 40 m.

### 13. MINEUR — Fondus noirs répétés contre un mur
- **Où** : `HeadWallGuard` (fondu de 0,2 s à chaque correction).
- **Scénario** : le joueur garde la tête dans le mur : un fondu par image, l'écran reste noir tant qu'il pousse (voulu), mais avec un léger clignotement. À juger au casque.

### 14. MINEUR — Noms longs dans le lobby
- **Où** : `MainMenuMulti.cs` (`lobby.NameOf(e).ToUpperInvariant()`), noms jusqu'à 20 caractères.
- **Correctif** : couper à ~14 caractères comme le HUD du duel (`NetDuelHud.Short`).

### 15. INFO — Autorité client
- Chaque casque applique les coups reçus à sa propre vie et annonce lui-même sa mort (`NetPlayer.HitRpc`, `NetDuel.ReportDeathServerRpc`). Un client modifié pourrait être invincible. Acceptable pour un jeu en réseau local de SAÉ ; à mentionner si le jury pose la question.

## Vérifié sans problème

- 3e client en duel refusé à la connexion (`NetSession.Approve`, limite 2) ; partie déjà lancée : refus.
- Shaders utilisés par les HUD (`UI/Default`, `Sprites/Default`) inclus d'office dans le build (`GraphicsSettings`).
- Pas de mort possible hors manche active en duel (coups refusés, suicide bloqué par `Pistol.Fire`) : pas de joueur bloqué mort entre deux manches.
- Session réseau, numéros des joueurs, côtés du duel, saut, objets tenus, portail en coop : corrigés et testés dans l'éditeur (hotfix #155 à #162), pas encore au casque.
- Record du mode survie : jamais écrit en coop (`!NetSession.Active`).

## Ce qui n'a pas été testé

Aucune partie réelle à deux casques ; les points ci-dessus sont lus dans le code. À rejouer au casque en priorité : 1 (tir ami), 2 (déconnexion en duel), 4 (actions du mort).

## Corrections (hotfix #163)

| # | Correction | Fichiers | Test (éditeur) |
|---|---|---|---|
| 1 | Coups entre joueurs ignorés hors des arènes du duel (et refusés côté victime) | `PlayerHitbox.Hit`, `NetPlayer.HitRpc` | coop : le coup s'arrête avant le coéquipier ; arène : il passe toujours |
| 2 | Adversaire parti : victoire par forfait (« VICTOIRE · ADVERSAIRE PARTI »), retour au menu ; le client dont l'hôte part lit « L'adversaire (hôte) a quitté le duel » | `NetDuel.PlayerLeft`, `NetDuelHud`, `NetSession` | forfait simulé : fin de match, côté restant gagnant |
| 3 | Duel impossible à lancer avec plus de 2 joueurs (même forcé), bouton « forcer » masqué, message en rouge dans le lobby | `NetLobby.TooManyForDuel`, `MainMenuMulti` | lobby à 3 en mode duel : pas de compte à rebours, message affiché |
| 4 | Mort en réseau : objets tenus lâchés, retour au holster de la ceinture s'il y en a un de libre (pistolet), mains désactivées jusqu'à la réapparition | `NetPlayer.SetHands` | coop : pistolet rangé au holster, mains coupées ; duel : mains coupées puis rendues |
| 5 | Correctif du délai de 12 s du lobby commité | `MainMenuMulti.cs` | relu |
| 6 | Évasion : « Tes coéquipiers doivent passer le portail » au lieu de « Réapparition à la fin de la vague » | `NetPlayer`, `NetSpectator` | relu |
| 9 | Pas de régénération de vie en duel | `NetDuel.OnNetworkSpawn` | `maxRegenPerWave = 0` en arène |

