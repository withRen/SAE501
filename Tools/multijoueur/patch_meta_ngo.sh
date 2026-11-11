#!/bin/sh
# Netcode for GameObjects + Meta XR SDK 205 sous Unity 6.6 : le « building block » NGO de Meta (code éditeur,
# activé dès que Netcode est installé) lit `instanceId`, devenu une erreur de compilation dans Unity 6.6.
# On le remplace par `entityId` dans le cache des paquets (2 lignes). Validé par Rahim le 10/11/2026.
# À relancer après chaque réinstallation des paquets (dossier Library effacé, nouveau clone, mise à jour du Meta XR SDK).
cd "$(dirname "$0")/../.." || exit 1
for f in Library/PackageCache/com.meta.xr.sdk.core@*/Editor/BuildingBlocks/BlockData/MultiplayerBlocks/NGO/SceneListenerNGO.cs; do
  [ -f "$f" ] || { echo "fichier introuvable : $f"; exit 1; }
  chmod u+w "$f"
  sed -i '' 's/(createGameObjectHierarchyEvent\.instanceId)/(createGameObjectHierarchyEvent.entityId)/; s/(changeGameObjectStructure\.instanceId)/(changeGameObjectStructure.entityId)/' "$f"
  echo "corrigé : $f"
done
