using System.IO;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;

// [Multijoueur] Construit les prefabs réseau dans Assets/Resources/Net (chargés par NetSession, NetLobby, NetGame) :
//   NetworkManager (NetworkManager + Unity Transport + liste des prefabs réseau), NetLobby, NetGame, NetPlayer.
// Menu SAE501 → Build Net Prefabs. À relancer seulement si on ajoute un prefab réseau : les identifiants (GlobalObjectIdHash)
// des prefabs existants ne changent pas tant qu'on ne les supprime pas, et TOUS les casques doivent avoir la même version.
public static class NetPrefabsBuilder
{
    const string Dir = "Assets/Resources/Net";

    [MenuItem("SAE501/Build Net Prefabs")]
    public static void Build()
    {
        Directory.CreateDirectory(Dir);
        var lobby = Prefab<NetLobby>("NetLobby");
        var game = Prefab<NetGame>("NetGame");
        var player = Prefab<NetPlayer>("NetPlayer");
        var duel = Prefab<NetDuel>("NetDuel");

        var listPath = Dir + "/NetPrefabs.asset";
        var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(listPath);
        if (!list) { list = ScriptableObject.CreateInstance<NetworkPrefabsList>(); AssetDatabase.CreateAsset(list, listPath); }
        foreach (var go in new[] { lobby, game, player, duel })
            if (!list.Contains(go)) list.Add(new NetworkPrefab { Prefab = go });
        EditorUtility.SetDirty(list);

        var nmPath = Dir + "/NetworkManager.prefab";
        var root = new GameObject("NetworkManager");
        var nm = root.AddComponent<NetworkManager>();
        var utp = root.AddComponent<UnityTransport>();
        nm.NetworkConfig = new NetworkConfig
        {
            NetworkTransport = utp,
            ConnectionApproval = true,
            EnableSceneManagement = true,
            TickRate = 30,
            ClientConnectionBufferTimeout = 10,
            LoadSceneTimeOut = 60,          // le Quest met du temps à charger MainScene
        };
        nm.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(list);
        utp.ConnectionData.Port = NetSession.GamePort;
        PrefabUtility.SaveAsPrefabAsset(root, nmPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        Debug.Log("[Net] prefabs réseau construits dans " + Dir);
    }

    // prefab vide (NetworkObject + le script) ; s'il existe déjà, on le garde tel quel (même identifiant réseau)
    static GameObject Prefab<T>(string name) where T : Component
    {
        var path = Dir + "/" + name + ".prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing && existing.GetComponent<T>()) return existing;
        var go = new GameObject(name);
        go.AddComponent<NetworkObject>();
        go.AddComponent<T>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }
}
