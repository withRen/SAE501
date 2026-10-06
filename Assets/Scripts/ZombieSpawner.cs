using UnityEngine;

// Fait apparaître des zombies au bout des couloirs (là où le joueur ne regarde pas).
public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public Transform leftSpawn, rightSpawn;

    [Header("Test : un zombie au lancement")]
    public bool spawnOnStart = true;
    public float startDelay = 4f;
    public bool startLeft = true;

    void Start()
    {
        if (spawnOnStart) Invoke(nameof(SpawnTest), startDelay);
    }

    void SpawnTest() => Spawn(startLeft);

    public Zombie Spawn(bool left)
    {
        var p = left ? leftSpawn : rightSpawn;
        var go = Instantiate(zombiePrefab, p.position, Quaternion.LookRotation(Vector3.back));
        return go.GetComponent<Zombie>();
    }
}
