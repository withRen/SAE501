using UnityEngine;

public enum HitZone { Head, Torso, Legs }

// Zone touchable d'un zombie (tête, torse, jambes). Le pistolet envoie "OnShot" au collider touché.
public class ZombieHitbox : MonoBehaviour
{
    public Zombie zombie;
    public HitZone zone = HitZone.Torso;

    void OnShot(RaycastHit hit)
    {
        if (zombie != null) zombie.OnHit(zone);
    }
}
