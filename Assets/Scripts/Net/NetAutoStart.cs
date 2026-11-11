using System;
using System.Collections;
using UnityEngine;

// [Multijoueur] Lancement automatique par arguments, pour tester sans casque (version PC, deux fenêtres sur la même machine) :
//   -nethost              héberge dès le menu            -netjoin 192.168.1.20   rejoint cette adresse (127.0.0.1 : même machine)
//   -netready             se met PRÊT dans le lobby      -netsolo                l'hôte lance seul (1 joueur suffit)
// Exemple macOS : open -n Escapor.app --args -pc -netjoin 127.0.0.1 -netready
public class NetAutoStart : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        var args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "-nethost") < 0 && Array.IndexOf(args, "-netjoin") < 0) return;
        var go = new GameObject("NetAutoStart");
        DontDestroyOnLoad(go);
        go.AddComponent<NetAutoStart>();
    }

    IEnumerator Start()
    {
        var args = Environment.GetCommandLineArgs();
        yield return new WaitForSeconds(1f);
        int j = Array.IndexOf(args, "-netjoin");
        bool ok = j >= 0 && j + 1 < args.Length ? NetSession.Join(args[j + 1]) : NetSession.Host();
        Debug.Log("[Net] lancement automatique : " + (j >= 0 ? "rejoindre " + args[j + 1] : "héberger") + (ok ? "" : " — échec : " + NetSession.LastError));
        if (!ok || Array.IndexOf(args, "-netready") < 0) yield break;
        float until = Time.unscaledTime + 30f;
        while (!NetLobby.Instance && Time.unscaledTime < until) yield return null;
        if (!NetLobby.Instance) { Debug.LogWarning("[Net] pas de lobby reçu"); yield break; }
        yield return new WaitForSeconds(0.5f);
        NetLobby.Instance.SetReady(true);
        Debug.Log("[Net] PRÊT envoyé");
    }
}
