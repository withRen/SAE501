using UnityEngine;

// Équilibrage des zombies par niveau (un niveau = une vague). Toutes les valeurs sont ici, à un seul endroit.
// Voir docs/ZOMBIES.md pour le tableau complet.
public static class ZombieLevels
{
    public struct Stats
    {
        public float hpMultiplier;      // PV du zombie = ZombieLevels.BaseHealth × ce facteur
        public float playerDamage;      // part de la vie MAX du joueur retirée à chaque coup (0..1)
    }

    public const float BaseHealth = 100f;          // PV d'un zombie au niveau 1

    // Part des PV de base retirée par un tir selon la zone touchée (la tête tue toujours d'un coup)
    public const float HeadShot = 1f;              // 100 % : one-shot
    public const float TorsoShot = 1f / 3f;        //  33 % : 3 tirs au niveau 1
    public const float LegsShot = 1f / 5f;         //  20 % : 5 tirs au niveau 1

    public const int MaxLevel = 4;

    static readonly Stats[] table =
    {
        new Stats { hpMultiplier = 1.0f, playerDamage = 0.20f },       // niveau 1
        new Stats { hpMultiplier = 1.3f, playerDamage = 0.20f },       // niveau 2
        new Stats { hpMultiplier = 1.6f, playerDamage = 1f / 3f },     // niveau 3
        new Stats { hpMultiplier = 2.0f, playerDamage = 0.40f },       // niveau 4 : 3 coups pour mourir (50 % = 2 coups faisait une marche trop brutale)
    };

    // Au-delà du niveau 4, on garde les valeurs du niveau 4.
    public static Stats For(int level) => table[Mathf.Clamp(level, 1, MaxLevel) - 1];

    public static float ShotFraction(HitZone zone)
    {
        switch (zone)
        {
            case HitZone.Head: return HeadShot;
            case HitZone.Torso: return TorsoShot;
            default: return LegsShot;
        }
    }

    // nombre de tirs nécessaires dans une zone donnée à un niveau donné
    public static int ShotsToKill(int level, HitZone zone)
    {
        if (zone == HitZone.Head) return 1;
        float hp = BaseHealth * For(level).hpMultiplier;
        return Mathf.CeilToInt(hp / (BaseHealth * ShotFraction(zone)) - 0.001f);
    }
}
