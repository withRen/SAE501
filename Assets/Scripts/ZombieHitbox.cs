using UnityEngine;

// Zone touchable d'un zombie. Le pistolet envoie "OnShot" au collider touché.
public class ZombieHitbox : MonoBehaviour
{
    public Zombie zombie;
    public bool head;

    void OnShot(RaycastHit hit)
    {
        if (zombie != null) zombie.OnHit(head);
    }
}
