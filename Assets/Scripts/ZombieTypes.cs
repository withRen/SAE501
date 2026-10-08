using UnityEngine;

public enum ZombieType { Standard, Fast, Reinforced }

// Les 3 types de zombies (GDD). Toutes les valeurs sont ici, à un seul endroit.
//   Standard  : lent,   2 balles, 1 PV / 1,5 s
//   Rapide    : +20 % de vitesse, 2 balles, 1 PV / 1 s
//   Renforcé  : lent,   4 balles, 1 PV / 1,5 s
// « N balles » = N tirs au torse (la tête tue toujours d'un coup, voir ZombieLevels).
public static class ZombieTypes
{
    public const float PlayerDamageFraction = 0.25f;     // 1 PV sur 4 = 25 % de la vie max du joueur

    public struct Def
    {
        public string name;
        public int torsoShotsToKill;
        public float speedMultiplier;       // multiplie la vitesse de base (ZombieSpawner.zombieSpeed)
        public float attackCooldown;        // secondes entre deux coups de griffe
        public float scale;
        public Color tint;                  // teinte multipliée sur la couleur du matériau (blanc = inchangé)

        public float Health => torsoShotsToKill * ZombieLevels.BaseHealth * ZombieLevels.TorsoShot;
    }

    static readonly Def[] table =
    {
        new Def { name = "Standard", torsoShotsToKill = 2, speedMultiplier = 1.0f, attackCooldown = 1.5f, scale = 1.00f, tint = Color.white },
        new Def { name = "Rapide",   torsoShotsToKill = 2, speedMultiplier = 1.2f, attackCooldown = 1.0f, scale = 1.00f, tint = new Color(1f, 0.6f, 0.6f) },
        new Def { name = "Renforcé", torsoShotsToKill = 4, speedMultiplier = 1.0f, attackCooldown = 1.5f, scale = 1.12f, tint = new Color(0.6f, 0.65f, 0.75f) },
    };

    public static Def For(ZombieType t) => table[(int)t];

    // distingue visuellement les types (échelle + teinte), sans prefab supplémentaire
    public static void ApplyLook(GameObject go, ZombieType t)
    {
        var d = For(t);
        if (!Mathf.Approximately(d.scale, 1f)) go.transform.localScale *= d.scale;
        if (d.tint == Color.white) return;

        int baseId = Shader.PropertyToID("_BaseColor");
        var mpb = new MaterialPropertyBlock();
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null || !mats[i].HasProperty(baseId)) continue;
                r.GetPropertyBlock(mpb, i);
                mpb.SetColor(baseId, mats[i].GetColor(baseId) * d.tint);
                r.SetPropertyBlock(mpb, i);
            }
        }
    }
}
