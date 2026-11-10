using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

// Découverte des parties sur le Wi-Fi local, sans taper d'adresse IP :
//   - l'hôte diffuse toutes les secondes (UDP 47777, adresse de diffusion 255.255.255.255) « ESCAPOR|version|nom|joueurs » ;
//   - le menu REJOINDRE écoute ce port et liste les parties entendues depuis moins de 4 s.
// Sur le Quest (Android), le Wi-Fi jette les paquets de diffusion tant qu'on ne tient pas un « verrou multicast » :
// il est pris pendant l'écoute (MulticastLock). Les réseaux qui isolent les appareils (eduroam, Wi-Fi de l'IUT) bloquent tout :
// utiliser le partage de connexion d'un téléphone.
public class LanDiscovery : MonoBehaviour
{
    public const int Port = 47777;
    const string Tag = "ESCAPOR";

    public struct Game { public string ip, name, version; public int players; public float seen; }

    static LanDiscovery instance;
    UdpClient sender, listener;
    float nextSend;
    bool advertising, listening;
    readonly Dictionary<string, Game> games = new Dictionary<string, Game>();
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject multicastLock;
#endif

    static LanDiscovery Get()
    {
        if (instance) return instance;
        var go = new GameObject("LanDiscovery");
        DontDestroyOnLoad(go);
        return instance = go.AddComponent<LanDiscovery>();
    }

    public static void StartAdvertising() { var d = Get(); d.advertising = true; d.nextSend = 0f; }
    public static void StopAdvertising() { if (instance) instance.advertising = false; }

    public static void StartListening()
    {
        var d = Get();
        if (d.listening) return;
        d.games.Clear();
        try
        {
            d.listener = new UdpClient();
            d.listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            d.listener.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
            d.listening = true;
            d.AcquireMulticastLock();
        }
        catch (Exception e) { Debug.LogWarning("[Net] écoute de la découverte impossible : " + e.Message); }
    }

    public static void StopListening()
    {
        if (!instance) return;
        instance.listening = false;
        instance.listener?.Close();
        instance.listener = null;
        instance.ReleaseMulticastLock();
    }

    /// <summary>Parties entendues récemment (les plus récentes d'abord).</summary>
    public static List<Game> Games()
    {
        var list = new List<Game>();
        if (!instance) return list;
        foreach (var g in instance.games.Values) if (Time.unscaledTime - g.seen < 4f) list.Add(g);
        list.Sort((a, b) => string.CompareOrdinal(a.ip, b.ip));
        return list;
    }

    /// <summary>Adresse IPv4 de ce casque sur le réseau local (affichée par l'hôte, pour la saisie manuelle).</summary>
    public static string LocalIp()
    {
        try
        {
            // pas d'envoi réel : connecter une socket UDP suffit à choisir l'interface de sortie
            using (var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                s.Connect("10.255.255.255", 1);
                return ((IPEndPoint)s.LocalEndPoint).Address.ToString();
            }
        }
        catch { return "?"; }
    }

    void Update()
    {
        if (advertising && Time.unscaledTime >= nextSend)
        {
            nextSend = Time.unscaledTime + 1f;
            Send();
        }
        if (listening && listener != null)
        {
            try
            {
                while (listener.Available > 0)
                {
                    var from = new IPEndPoint(IPAddress.Any, 0);
                    var data = listener.Receive(ref from);
                    Parse(Encoding.UTF8.GetString(data), from.Address.ToString());
                }
            }
            catch (Exception e) { Debug.LogWarning("[Net] découverte : " + e.Message); }
        }
    }

    // 255.255.255.255 ne part pas sur toutes les machines (macOS : « No route to host ») : on envoie aussi à l'adresse
    // de diffusion de chaque réseau local (masque de l'interface, ou /24 si Android ne le donne pas)
    void Send()
    {
        if (sender == null) { sender = new UdpClient(); sender.EnableBroadcast = true; }
        int n = NetSession.Net ? NetSession.Net.ConnectedClientsIds.Count : 1;
        var msg = Encoding.UTF8.GetBytes(Tag + "|" + Application.version + "|" + HostName() + "|" + n);
        if (targets == null || Time.unscaledTime > targetsAt + 10f) { targets = BroadcastTargets(); targetsAt = Time.unscaledTime; }
        int sent = 0;
        foreach (var t in targets)
        {
            try { sender.Send(msg, msg.Length, new IPEndPoint(t, Port)); sent++; }
            catch { }
        }
        if (sent == 0 && !warned) { warned = true; Debug.LogWarning("[Net] diffusion impossible sur ce réseau : les autres doivent saisir l'adresse " + LocalIp()); }
    }

    List<IPAddress> targets;
    float targetsAt;
    bool warned;

    static List<IPAddress> BroadcastTargets()
    {
        var list = new List<IPAddress> { IPAddress.Broadcast };
        try
        {
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(ua.Address)) continue;
                    byte[] ip = ua.Address.GetAddressBytes(), mask;
                    try { mask = ua.IPv4Mask != null ? ua.IPv4Mask.GetAddressBytes() : null; } catch { mask = null; }
                    if (mask == null || mask.Length != 4 || (mask[0] | mask[1] | mask[2] | mask[3]) == 0) mask = new byte[] { 255, 255, 255, 0 };
                    var b = new byte[4];
                    for (int i = 0; i < 4; i++) b[i] = (byte)(ip[i] | ~mask[i]);
                    var addr = new IPAddress(b);
                    if (!list.Contains(addr)) list.Add(addr);
                }
            }
        }
        catch { }
        // dernier recours : /24 de l'adresse locale
        var local = LocalIp();
        if (IPAddress.TryParse(local, out var la))
        {
            var b = la.GetAddressBytes(); b[3] = 255;
            var addr = new IPAddress(b);
            if (!list.Contains(addr)) list.Add(addr);
        }
        return list;
    }

    void Parse(string msg, string ip)
    {
        var parts = msg.Split('|');
        if (parts.Length < 4 || parts[0] != Tag) return;
        int.TryParse(parts[3], out int n);
        games[ip] = new Game { ip = ip, version = parts[1], name = parts[2], players = n, seen = Time.unscaledTime };
    }

    static string HostName()
    {
        var n = SystemInfo.deviceName;
        if (string.IsNullOrEmpty(n) || n == SystemInfo.unsupportedIdentifier) n = SystemInfo.deviceModel;
        return n.Replace("|", " ");
    }

    void AcquireMulticastLock()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var wifi = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            {
                multicastLock = wifi.Call<AndroidJavaObject>("createMulticastLock", "escapor-discovery");
                multicastLock.Call("acquire");
            }
        }
        catch (Exception e) { Debug.LogWarning("[Net] verrou multicast : " + e.Message); }
#endif
    }

    void ReleaseMulticastLock()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try { multicastLock?.Call("release"); } catch { }
        multicastLock = null;
#endif
    }

    void OnDestroy()
    {
        sender?.Close();
        listener?.Close();
        ReleaseMulticastLock();
    }
}
