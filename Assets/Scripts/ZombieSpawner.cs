using UnityEngine;

// Fait apparaître des zombies aux points du prefab ZombieSpawnPoints (bouts des couloirs, extérieur sud du bunker).
// Chaque zombie reçoit PV et dégâts de son niveau (ZombieLevels). Le chemin NavMesh de chaque zombie part de son point d'apparition.
public class ZombieSpawner : MonoBehaviour
{
    public enum Point { CouloirGauche, CouloirDroit, Exterieur, Toit }

    public GameObject zombiePrefab;
    public ZombieSpawnPoints points;

    [Header("Niveau (= vague) : PV des zombies et dégâts infligés au joueur")]
    [Range(1, ZombieLevels.MaxLevel)] public int level = 1;
    public float zombieSpeed = 0.9f;
    public Vector2 speedRange = new Vector2(0.75f, 1.4f);     // vitesse de chaque zombie : zombieSpeed × tirage dans cet intervalle

    [Header("Test : un zombie au lancement")]
    public bool spawnOnStart = true;
    public float startDelay = 4f;
    public Point startPoint = Point.CouloirGauche;

    void Start()
    {
        if (spawnOnStart) Invoke(nameof(SpawnTest), startDelay);
    }

    void SpawnTest() => Spawn(startPoint);

    // début d'une nouvelle vague : niveau suivant (plafonné) et régénération du joueur remise à zéro
    public void NextLevel()
    {
        level = Mathf.Min(level + 1, ZombieLevels.MaxLevel);
        if (PlayerHealth.Instance) PlayerHealth.Instance.NewWave();
    }

    public Zombie Spawn(Point where)
    {
        Transform p = where == Point.CouloirGauche ? points.leftCorridorEnd
                    : where == Point.CouloirDroit ? points.rightCorridorEnd
                    : where == Point.Toit && points.roof ? points.roof
                    : points.exterior;
        // terrain en relief : départ posé sur le NavMesh le plus proche (sinon l'agent ne se crée pas s'il est sous le sol).
        // D'abord tout près : avec un grand rayon, Unity préfère un NavMesh situé juste au-dessus (le toit du couloir droit,
        // à 2,8 m) au sol situé à côté, et le zombie apparaissait sur le toit.
        var pos = p.position;
        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out var hit, 1f, UnityEngine.AI.NavMesh.AllAreas)
            || UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, 4f, UnityEngine.AI.NavMesh.AllAreas)) pos = hit.position;
        var prefab = PickPrefab();
        var go = Instantiate(prefab, pos, p.rotation);
        var z = go.GetComponent<Zombie>();
        LastPrefabIndex = PrefabIndex(prefab);
        z.ApplyLevel(level);
        // chaque zombie a son allure (tirage), multipliée par celle de son type (Cogneur, Coureur…)
        z.walkSpeed = zombieSpeed * Random.Range(speedRange.x, speedRange.y) * z.speedMultiplier;
        Spawned?.Invoke(z, LastPrefabIndex);
        return z;
    }

    // [Multijoueur] type du dernier zombie créé (0 = prefab principal, 1.. = ZombieVariants.others), envoyé aux autres casques
    public int LastPrefabIndex { get; private set; }
    /// <summary>[Multijoueur] un zombie vient d'apparaître chez l'hôte (zombie, type) : NetGame le crée chez les autres.</summary>
    public static event System.Action<Zombie, int> Spawned;

    int PrefabIndex(GameObject prefab)
    {
        var v = zombiePrefab ? zombiePrefab.GetComponent<ZombieVariants>() : null;
        if (v != null && v.others != null) for (int i = 0; i < v.others.Length; i++) if (v.others[i] == prefab) return i + 1;
        return 0;
    }

    /// <summary>[Multijoueur] casque client : zombie « marionnette » du type donné, piloté par l'hôte (voir Zombie.MakePuppet).</summary>
    public Zombie SpawnPuppet(int prefabIndex, ushort id, Vector3 pos, float yaw)
    {
        var v = zombiePrefab ? zombiePrefab.GetComponent<ZombieVariants>() : null;
        var prefab = prefabIndex > 0 && v != null && v.others != null && prefabIndex <= v.others.Length ? v.others[prefabIndex - 1] : zombiePrefab;
        var z = Instantiate(prefab, pos, Quaternion.Euler(0f, yaw, 0f)).GetComponent<Zombie>();
        z.MakePuppet(id);
        return z;
    }

    // types de zombie : le prefab principal liste les autres (ZombieVariants, rempli par ZombieBuilder).
    // Tirage au hasard selon spawnWeight, parmi les types autorisés à ce niveau (minLevel).
    GameObject PickPrefab()
    {
        var v = zombiePrefab ? zombiePrefab.GetComponent<ZombieVariants>() : null;
        if (v == null || v.others == null || v.others.Length == 0) return zombiePrefab;

        float total = 0f;
        for (int i = -1; i < v.others.Length; i++) total += Weight(i < 0 ? zombiePrefab : v.others[i]);
        float r = Random.value * total;
        for (int i = -1; i < v.others.Length; i++)
        {
            var prefab = i < 0 ? zombiePrefab : v.others[i];
            r -= Weight(prefab);
            if (r <= 0f && Weight(prefab) > 0f) return prefab;
        }
        return zombiePrefab;
    }

    float Weight(GameObject prefab)
    {
        var z = prefab ? prefab.GetComponent<Zombie>() : null;
        return z != null && level >= z.minLevel ? Mathf.Max(0f, z.spawnWeight) : 0f;
    }
}
