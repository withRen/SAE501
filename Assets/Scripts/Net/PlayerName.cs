using UnityEngine;

// [Multijoueur, hotfix #155] Nom du joueur affiché aux autres (lobby, étiquette au-dessus de l'avatar, fil des éliminations,
// « ÉLIMINÉ … », « Éliminé par … », spectateur). Demande de Rahim : le nom du compte Meta / du profil du casque.
// Ordre d'essai, une seule fois au lancement du jeu :
//   1. compte Meta (Meta Platform SDK, paquet com.meta.xr.sdk.platform déjà installé) : Core.AsyncInitialize, puis
//      Entitlements.IsUserEntitledToApplication, puis Users.GetLoggedInUser → DisplayName (sinon OculusID).
//      Il faut un identifiant d'application Meta (App ID du tableau de bord développeur, Oculus → Platform → Edit Settings,
//      champ « Meta Quest/2/Pro/3 » : Resources/OculusPlatformSettings). Sans App ID, cette étape est sautée. L'APK doit aussi
//      être reconnu par Meta (installé depuis une chaîne de diffusion de l'appli, ou compte membre de l'organisation), sinon
//      la vérification d'achat échoue : on passe à l'étape 2 ;
//   2. nom du casque (réglage Android « device_name », celui que l'on donne au casque dans l'appli Meta Horizon),
//      sauf s'il est générique (« Quest 3 », « Meta Quest 3 »…) ;
//   3. version PC, éditeur ou échec : « Joueur N » (numéro du lobby, comme avant).
// Le nom est envoyé à l'hôte dans le lobby (NetLobby.Entry.name) et par chaque joueur en partie (NetPlayer.Name).
public static class PlayerName
{
    /// <summary>Nom de CE joueur ; vide tant qu'il n'est pas connu (on affiche alors « Joueur N »).</summary>
    public static string Local { get; private set; } = "";
    public static event System.Action Changed;

    const int MaxLength = 20;
    static bool started;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Local = ""; started = false; Changed = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (started) return;
        started = true;
#if UNITY_ANDROID && !UNITY_EDITOR
        Set(DeviceName());          // tout de suite (repli), remplacé par le nom du compte s'il arrive
        TryMetaAccount();
#endif
    }

    /// <summary>Nom à afficher pour un joueur : son nom s'il est connu, sinon « Joueur N ».</summary>
    public static string Display(string name, int number) => string.IsNullOrWhiteSpace(name) ? "Joueur " + number : name;

    /// <summary>Version courte pour les étiquettes : le nom, sinon « J2 ».</summary>
    public static string Short(string name, int number) => string.IsNullOrWhiteSpace(name) ? "J" + number : name;

    static void Set(string n)
    {
        n = (n ?? "").Trim();
        if (n.Length > MaxLength) n = n.Substring(0, MaxLength);
        if (n.Length == 0 || n == Local) return;
        Local = n;
        Debug.Log("[PlayerName] nom du joueur : " + n);
        Changed?.Invoke();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static void TryMetaAccount()
    {
        string appId = Oculus.Platform.PlatformSettings.MobileAppID;
        if (string.IsNullOrEmpty(appId)) { Debug.Log("[PlayerName] pas d'App ID Meta : nom du casque ou « Joueur N »"); return; }
        try
        {
            Oculus.Platform.Core.AsyncInitialize(appId).OnComplete(init =>
            {
                if (init.IsError) { Debug.LogWarning("[PlayerName] Meta Platform : échec de l'initialisation"); return; }
                Oculus.Platform.Entitlements.IsUserEntitledToApplication().OnComplete(ent =>
                {
                    if (ent.IsError) { Debug.LogWarning("[PlayerName] Meta Platform : appli non reconnue (entitlement)"); return; }
                    Oculus.Platform.Users.GetLoggedInUser().OnComplete(u =>
                    {
                        if (u.IsError || u.Data == null) { Debug.LogWarning("[PlayerName] Meta Platform : utilisateur inconnu"); return; }
                        Set(!string.IsNullOrWhiteSpace(u.Data.DisplayName) ? u.Data.DisplayName : u.Data.OculusID);
                    });
                });
            });
        }
        catch (System.Exception e) { Debug.LogWarning("[PlayerName] Meta Platform indisponible : " + e.Message); }
    }

    // nom donné au casque (Settings.Global « device_name »), ignoré s'il n'est que le modèle
    static string DeviceName()
    {
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            using (var global = new AndroidJavaClass("android.provider.Settings$Global"))
            {
                string n = global.CallStatic<string>("getString", resolver, "device_name");
                if (string.IsNullOrWhiteSpace(n)) return "";
                string low = n.ToLowerInvariant().Replace("meta", "").Replace("oculus", "").Trim();
                if (low.StartsWith("quest") && low.Length <= 9) return "";        // « Quest 3 », « Quest 3S », « Quest Pro »
                return n;
            }
        }
        catch { return ""; }
    }
#endif
}
