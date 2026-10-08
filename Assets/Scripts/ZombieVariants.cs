using UnityEngine;

// Liste des autres types de zombie, posée sur le prefab principal (Assets/Prefabs/Zombie.prefab) par ZombieBuilder.
// ZombieSpawner tire au hasard entre le prefab principal et ces prefabs (réglages de chaque type : Zombie.minLevel, spawnWeight...).
// Comme ça la scène n'a qu'un seul prefab à connaître : rien à changer dans RoomBuilder.
public class ZombieVariants : MonoBehaviour
{
    public GameObject[] others;
}
