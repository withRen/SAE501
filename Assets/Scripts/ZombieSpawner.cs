using UnityEngine;

// Fait apparaître des zombies au bout des couloirs (là où le joueur ne regarde pas).
public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public Transform leftSpawn, rightSpawn;

    [Header("Réglages appliqués à chaque zombie (la valeur du prefab est ignorée)")]
    public int zombieDamage = 1;            // le joueur a 4 PV : 4 coups d'affilée pour le tuer
    public float zombieSpeed = 0.9f;

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
        var z = go.GetComponent<Zombie>();
        z.attackDamage = zombieDamage;
        z.walkSpeed = zombieSpeed;
        return z;
    }
}
