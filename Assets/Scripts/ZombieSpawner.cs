using UnityEngine;

// Fait apparaître des zombies aux points du prefab ZombieSpawnPoints (bouts des couloirs, extérieur sud du bunker).
// Chaque zombie reçoit PV et dégâts de son niveau (ZombieLevels). Le chemin NavMesh de chaque zombie part de son point d'apparition.
public class ZombieSpawner : MonoBehaviour
{
    public enum Point { CouloirGauche, CouloirDroit, Exterieur }

    public GameObject zombiePrefab;
    public ZombieSpawnPoints points;

    [Header("Niveau (= vague) : PV des zombies et dégâts infligés au joueur")]
    [Range(1, ZombieLevels.MaxLevel)] public int level = 1;
    public float zombieSpeed = 0.9f;

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
                    : points.exterior;
        var go = Instantiate(PickPrefab(), p.position, p.rotation);
        var z = go.GetComponent<Zombie>();
        z.ApplyLevel(level);
        z.walkSpeed = zombieSpeed * z.speedMultiplier;
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
