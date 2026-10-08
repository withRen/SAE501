using UnityEngine;

// Fait apparaître des zombies aux points du prefab ZombieSpawnPoints (bouts des couloirs, extérieur sud du bunker).
// Chaque zombie reçoit PV, vitesse et cadence d'attaque de son type (ZombieTypes). Le chemin NavMesh de chaque zombie part de son point d'apparition.
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

    // type : standard / rapide / renforcé (ZombieTypes). speedMultiplier : réglage de la vague (WaveManager).
    public Zombie Spawn(Point where, ZombieType type = ZombieType.Standard, float speedMultiplier = 1f)
    {
        Transform p = where == Point.CouloirGauche ? points.leftCorridorEnd
                    : where == Point.CouloirDroit ? points.rightCorridorEnd
                    : points.exterior;
        var go = Instantiate(zombiePrefab, p.position, p.rotation);
        var z = go.GetComponent<Zombie>();
        z.ApplyType(type, zombieSpeed * speedMultiplier);
        ZombieTypes.ApplyLook(go, type);
        return z;
    }
}
