using System;
using UnityEngine;

// [6.4] Points (GDD) : +10 par zombie tué (+5 de bonus pour un tir à la tête), +50 par vague terminée, +200 si le générateur est activé.
// Garde aussi les statistiques de la partie pour l'écran de fin (zombies tués, tirs à la tête, durée).
// Un seul par scène (construit par RoomBuilder). Remis à zéro quand la scène est rechargée (bouton « rejouer »).
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public static event Action<int> Changed;             // nouveau score
    public static event Action<int, string> Gained;      // points gagnés + raison (affichée sur l'écran de score)

    public int perKill = 10;
    public int headshotBonus = 5;
    public int perWave = 50;
    public int victoryBonus = 200;

    public int Score { get; private set; }
    public int Kills { get; private set; }
    public int Headshots { get; private set; }
    public float Duration => (endTime > 0f ? endTime : Time.time) - startTime;

    float startTime, endTime = -1f;
    bool wasInBreak, wasVictory;

    void Awake() { Instance = this; startTime = Time.time; }
    void OnEnable() => Zombie.Killed += OnKilled;
    void OnDisable() => Zombie.Killed -= OnKilled;
    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnKilled(Zombie z, HitZone zone)
    {
        Kills++;
        int pts = perKill;
        if (zone == HitZone.Head) { Headshots++; pts += headshotBonus; }
        Add(pts, zone == HitZone.Head ? "TIR À LA TÊTE" : "ZOMBIE ABATTU");
    }

    void Update()
    {
        var wm = WaveManager.Instance;
        if (!wm) return;
        if (wm.InBreak && !wasInBreak) Add(perWave, "VAGUE TERMINÉE");      // vague terminée
        wasInBreak = wm.InBreak;
        if (wm.Victory && !wasVictory) { Add(victoryBonus, "GÉNÉRATEUR ACTIVÉ"); Stop(); }
        wasVictory = wm.Victory;
    }

    public void Add(int pts, string reason = null)
    {
        if (endTime > 0f) return;                          // partie finie : le score ne bouge plus
        Score += pts;
        Changed?.Invoke(Score);
        Gained?.Invoke(pts, reason);
    }

    public void Stop() { if (endTime < 0f) endTime = Time.time; }
    public void ResetClock() => startTime = Time.time;     // la durée compte à partir de l'écran de début

    [ContextMenu("Test : +10 points")] void Test() => Add(10, "TEST");
}
