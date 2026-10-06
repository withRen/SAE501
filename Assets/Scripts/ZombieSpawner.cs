using UnityEngine;

// Fait apparaître des zombies aux points du prefab ZombieSpawnPoints (bouts des couloirs, fenêtre Nord).
// Chaque zombie reçoit PV et dégâts de son niveau (ZombieLevels). Le chemin NavMesh de chaque zombie part de son point d'apparition.
public class ZombieSpawner : MonoBehaviour
{
    public enum Point { CouloirGauche, CouloirDroit, FenetreNord }

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
                    : points.northWindow;
        var go = Instantiate(zombiePrefab, p.position, p.rotation);
        var z = go.GetComponent<Zombie>();
        z.ApplyLevel(level);
        z.walkSpeed = zombieSpeed;
        return z;
    }
}
