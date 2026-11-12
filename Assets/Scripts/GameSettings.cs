using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

// Réglages du joueur (menu PARAMÈTRES), enregistrés d'une partie à l'autre (PlayerPrefs) et appliqués à chaque scène chargée :
//   musique / effets / cris des zombies, rotation fluide ou par à-coups, déplacement continu ou téléportation, prise par clic ou maintenue,
//   bords sombres en mouvement, difficulté ;
//   version PC seulement (DesktopMode) : sensibilité de la souris, souris inversée, champ de vision, VSync et limite d'images
//   par seconde (au casque, c'est le Quest qui impose le champ de vision des lentilles et la fréquence de l'écran, 72 Hz :
//   ces réglages n'apparaissent pas).
public static class GameSettings
{
    public enum Level { Facile, Normal, Difficile }

    public static float Music { get => PlayerPrefs.GetFloat("sae501.music", 0.6f); set { PlayerPrefs.SetFloat("sae501.music", Mathf.Clamp01(value)); Apply(); } }
    // cris des zombies (0..1) : volume et portée de leurs voix (ZombieVoice) ; gardé en mémoire (lu à chaque image par les zombies)
    public static float Zombies { get { if (zombies < 0f) zombies = PlayerPrefs.GetFloat("sae501.zombies", 0.6f); return zombies; } set { zombies = Mathf.Clamp01(value); PlayerPrefs.SetFloat("sae501.zombies", zombies); } }
    static float zombies = -1f;
    public static float Effects { get => PlayerPrefs.GetFloat("sae501.sfx", 0.8f); set { PlayerPrefs.SetFloat("sae501.sfx", Mathf.Clamp01(value)); Apply(); } }
    public static bool SmoothTurn { get => PlayerPrefs.GetInt("sae501.smooth", 1) == 1; set { PlayerPrefs.SetInt("sae501.smooth", value ? 1 : 0); Apply(); } }
    // téléportation (au casque) : joystick vers l'avant = viser le sol, relâcher = s'y rendre ; sinon déplacement continu
    public static bool Teleport { get => PlayerPrefs.GetInt("sae501.teleport", 0) == 1; set { PlayerPrefs.SetInt("sae501.teleport", value ? 1 : 0); Apply(); } }
    // prise des objets (grip) : un clic pour prendre, un clic pour lâcher (défaut) ou tenir tant que le grip est enfoncé
    public static bool HoldToGrab { get => PlayerPrefs.GetInt("sae501.holdgrab", 0) == 1; set { PlayerPrefs.SetInt("sae501.holdgrab", value ? 1 : 0); Apply(); } }
    public static bool ComfortVignette { get => PlayerPrefs.GetInt("sae501.vignette", 1) == 1; set { PlayerPrefs.SetInt("sae501.vignette", value ? 1 : 0); Apply(); } }
    // version PC : sensibilité de la souris (1 = normale), axe vertical inversé, champ de vision vertical (degrés)
    public static readonly float[] MouseSensitivities = { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f };
    public static float MouseSensitivity { get => PlayerPrefs.GetFloat("sae501.mouse", 1f); set { PlayerPrefs.SetFloat("sae501.mouse", Mathf.Clamp(value, 0.05f, 10f)); Apply(); } }
    public static bool InvertMouseY { get => PlayerPrefs.GetInt("sae501.mouseinvert", 0) == 1; set { PlayerPrefs.SetInt("sae501.mouseinvert", value ? 1 : 0); Apply(); } }
    public static readonly int[] FieldOfViews = { 60, 70, 80, 90 };
    public static int FieldOfView { get => PlayerPrefs.GetInt("sae501.fov", 70); set { PlayerPrefs.SetInt("sae501.fov", Mathf.Clamp(value, 40, 100)); Apply(); } }
    // version PC : synchronisation verticale (QualitySettings.vSyncCount = 1, le jeu suit la fréquence de l'écran) ;
    // sans VSync, limite d'images par seconde (Application.targetFrameRate ; 0 = sans limite). Unity ignore targetFrameRate
    // quand le VSync est actif : le menu grise alors la limite. Au casque (Quest ou PC VR), le runtime XR impose sa propre
    // cadence (72 Hz sur le Quest) et ignore ces deux réglages : on n'y touche pas.
    public static bool VSync { get => PlayerPrefs.GetInt("sae501.vsync", 1) == 1; set { PlayerPrefs.SetInt("sae501.vsync", value ? 1 : 0); ApplyFrameRate(); } }
    public static readonly int[] FrameRateCaps = { 0, 60, 72, 90, 144, 240 };
    public static int FrameRateCap { get => PlayerPrefs.GetInt("sae501.fpscap", 0); set { PlayerPrefs.SetInt("sae501.fpscap", Mathf.Max(0, value)); ApplyFrameRate(); } }
    public static Level Difficulty { get => (Level)PlayerPrefs.GetInt("sae501.difficulty", 1); set => PlayerPrefs.SetInt("sae501.difficulty", (int)value); }
    // Rééquilibrage : la difficulté change aussi les dégâts reçus (Zombie.ApplyLevel) et la chance de trouver un chargeur dans la carte (RandomSpawn)
    public static float DamageMultiplier => Difficulty == Level.Facile ? 0.75f : Difficulty == Level.Difficile ? 1.25f : 1f;
    public static float MagazineChanceFactor => Difficulty == Level.Facile ? 1.5f : Difficulty == Level.Difficile ? 0.7f : 1f;   // 50 % -> 75 % / 35 %

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.sceneLoaded += OnLoaded;
        OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static readonly HashSet<Scene> applied = new HashSet<Scene>();

    static void OnLoaded(Scene s, LoadSceneMode m)
    {
        if (!applied.Add(s)) return;      // une seule fois par scène chargée
        Apply();
        ApplyDifficulty();
    }

    /// <summary>Volume, rotation, confort et champ de vision : appliqués tout de suite (aussi depuis le menu).</summary>
    public static void Apply()
    {
        AudioListener.volume = Effects;
        // la musique ignore AudioListener.volume (MusicPlayer) : seul le curseur Musique la règle ; plus forte qu'avant
        // (curseur à 6/10 -> volume 0,96, contre 0,35 effectifs auparavant)
        foreach (var mp in Object.FindObjectsByType<MusicPlayer>(FindObjectsSortMode.None))
            mp.volume = Mathf.Clamp01(Music * 1.6f);

        // ControllerInputActionManager (Starter Assets XRI) : propriétés smoothTurnEnabled et smoothMotionEnabled, lues par réflexion.
        // Les manettes sont éteintes tant qu'elles ne sont pas suivies (XRInputModalityManager) : on les règle aussi éteintes.
        // [hotfix #155] Répartition des joysticks : GAUCHE = se déplacer (marche continue, ou téléportation si l'option est choisie),
        // DROIT = tourner, et rien d'autre. smoothMotionEnabled de la manette gauche : vrai = le joystick fait marcher ; faux = vers
        // l'avant il vise un point de téléportation. Les actions « tourner » de la main gauche et « viser la téléportation » de la
        // main droite sont coupées et retirées du gestionnaire (sinon il les rallume à chaque changement d'état) : StickRoles.
        bool teleport = Teleport && !DesktopMode.On;                // version PC : jamais de téléportation
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb.GetType().Name != "ControllerInputActionManager") continue;
            var p = mb.GetType().GetProperty("smoothTurnEnabled", BindingFlags.Public | BindingFlags.Instance);
            if (p != null && p.CanWrite) p.SetValue(mb, SmoothTurn);
            bool left = mb.name.Contains("Left");
            StickRoles(mb, left);
            var m = mb.GetType().GetProperty("smoothMotionEnabled", BindingFlags.Public | BindingFlags.Instance);
            if (m != null && m.CanWrite && left) m.SetValue(mb, !teleport);
        }
        // fournisseur de téléportation du rig (éteint par SetupVRScene) et zone de téléportation sur le sol
        foreach (var tp in Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            tp.gameObject.SetActive(teleport);
            if (teleport) NavTeleportArea.Ensure(tp);
        }

        // prise par clic ou en maintenant le grip (ClickGrab, sur les deux mains)
        foreach (var cg in Object.FindObjectsByType<ClickGrab>(FindObjectsInactive.Include, FindObjectsSortMode.None)) cg.ApplyMode();

        // vignette de confort des Starter Assets (si présente dans le rig)
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name.Contains("TunnelingVignette")) t.gameObject.SetActive(ComfortVignette && !DesktopMode.On);

        // version PC : champ de vision de la caméra du joueur
        if (DesktopMode.On && DesktopPlayer.Instance) DesktopPlayer.Instance.ApplySettings();
        ApplyFrameRate();
    }

    // [hotfix #155] main gauche : plus de rotation (champs m_Turn, m_SnapTurn) ; main droite : plus de téléportation (m_TeleportMode,
    // m_TeleportModeCancel, et son rayon de téléportation éteint). L'action est désactivée puis la référence retirée du gestionnaire.
    static void StickRoles(MonoBehaviour cim, bool left)
    {
        var names = left ? new[] { "m_Turn", "m_SnapTurn" } : new[] { "m_TeleportMode", "m_TeleportModeCancel" };
        foreach (var n in names)
        {
            var f = cim.GetType().GetField(n, BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) continue;
            if (f.GetValue(cim) is UnityEngine.InputSystem.InputActionReference r && r && r.action != null) r.action.Disable();
            f.SetValue(cim, null);
        }
        if (left) return;
        var tf = cim.GetType().GetField("m_TeleportInteractor", BindingFlags.NonPublic | BindingFlags.Instance);
        if (tf != null && tf.GetValue(cim) is Component ti && ti)
        {
            ti.gameObject.SetActive(false);
            tf.SetValue(cim, null);
        }
    }

    /// <summary>Version PC : VSync ou limite d'images par seconde. Au casque, rien (la cadence est celle du runtime XR).</summary>
    public static void ApplyFrameRate()
    {
        if (!DesktopMode.On) return;
        bool vsync = VSync;
        QualitySettings.vSyncCount = vsync ? 1 : 0;
        int cap = FrameRateCap;
        Application.targetFrameRate = vsync || cap <= 0 ? -1 : cap;
    }

    /// <summary>Difficulté : nombre de zombies, temps entre deux apparitions, vitesse.</summary>
    static void ApplyDifficulty()
    {
        float count = 1f, interval = 1f, speed = 1f;
        switch (Difficulty)
        {
            case Level.Facile: count = 0.75f; interval = 1.25f; speed = 0.85f; break;
            case Level.Difficile: count = 1.3f; interval = 0.8f; speed = 1.15f; break;
        }
        if (Mathf.Approximately(count, 1f)) return;
        var wm = Object.FindFirstObjectByType<WaveManager>();
        if (wm && wm.levels != null)
            foreach (var l in wm.levels) { l.zombies = Mathf.Max(1, Mathf.RoundToInt(l.zombies * count)); l.spawnInterval *= interval; }
        var sp = Object.FindFirstObjectByType<ZombieSpawner>();
        if (sp) sp.zombieSpeed *= speed;
    }

    /// <summary>Recentre la vue (bouton « RECENTRER » du menu).</summary>
    public static void Recenter()
    {
        var subs = new List<UnityEngine.XR.XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subs);
        foreach (var s in subs) s.TryRecenter();
    }
}
