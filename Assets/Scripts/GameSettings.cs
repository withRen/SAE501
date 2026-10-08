using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

// Réglages du joueur (menu PARAMÈTRES), enregistrés d'une partie à l'autre (PlayerPrefs) et appliqués à chaque scène chargée :
//   musique / effets / cris des zombies, rotation fluide ou par à-coups, bords sombres en mouvement, difficulté.
public static class GameSettings
{
    public enum Level { Facile, Normal, Difficile }

    public static float Music { get => PlayerPrefs.GetFloat("sae501.music", 0.6f); set { PlayerPrefs.SetFloat("sae501.music", Mathf.Clamp01(value)); Apply(); } }
    // cris des zombies (0..1) : volume et portée de leurs voix (ZombieVoice) ; gardé en mémoire (lu à chaque image par les zombies)
    public static float Zombies { get { if (zombies < 0f) zombies = PlayerPrefs.GetFloat("sae501.zombies", 0.6f); return zombies; } set { zombies = Mathf.Clamp01(value); PlayerPrefs.SetFloat("sae501.zombies", zombies); } }
    static float zombies = -1f;
    public static float Effects { get => PlayerPrefs.GetFloat("sae501.sfx", 0.8f); set { PlayerPrefs.SetFloat("sae501.sfx", Mathf.Clamp01(value)); Apply(); } }
    public static bool SmoothTurn { get => PlayerPrefs.GetInt("sae501.smooth", 1) == 1; set { PlayerPrefs.SetInt("sae501.smooth", value ? 1 : 0); Apply(); } }
    public static bool ComfortVignette { get => PlayerPrefs.GetInt("sae501.vignette", 1) == 1; set { PlayerPrefs.SetInt("sae501.vignette", value ? 1 : 0); Apply(); } }
    public static Level Difficulty { get => (Level)PlayerPrefs.GetInt("sae501.difficulty", 1); set => PlayerPrefs.SetInt("sae501.difficulty", (int)value); }

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

    /// <summary>Volume, rotation et confort : appliqués tout de suite (aussi depuis le menu).</summary>
    public static void Apply()
    {
        AudioListener.volume = Effects;
        // la musique ignore AudioListener.volume (MusicPlayer) : seul le curseur Musique la règle ; plus forte qu'avant
        // (curseur à 6/10 -> volume 0,96, contre 0,35 effectifs auparavant)
        foreach (var mp in Object.FindObjectsByType<MusicPlayer>(FindObjectsSortMode.None))
            mp.volume = Mathf.Clamp01(Music * 1.6f);

        // ControllerInputActionManager (Starter Assets XRI) : propriété smoothTurnEnabled, lue par réflexion
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb.GetType().Name != "ControllerInputActionManager") continue;
            var p = mb.GetType().GetProperty("smoothTurnEnabled", BindingFlags.Public | BindingFlags.Instance);
            if (p != null && p.CanWrite) p.SetValue(mb, SmoothTurn);
        }

        // vignette de confort des Starter Assets (si présente dans le rig)
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name.Contains("TunnelingVignette")) t.gameObject.SetActive(ComfortVignette);
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
