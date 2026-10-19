using UnityEngine;

public enum HitZone { Head, Torso, Legs }

// Zone touchable d'un zombie (tête, torse, jambes). Le pistolet envoie "OnShot" au collider touché.
public class ZombieHitbox : MonoBehaviour
{
    public Zombie zombie;
    public HitZone zone = HitZone.Torso;

    // [#113] retour d'impact (particules, son, recul) puis dégâts ; [#122] tirs à la tête comptés pour le bilan
    void OnShot(RaycastHit hit)
    {
        if (zombie == null || zombie.Dead) return;
        var dir = hit.point - (Camera.main ? Camera.main.transform.position : hit.point - transform.forward);
        if (zone == HitZone.Head) GameStats.HeadShot();
        zombie.HitFeedback(hit.point, dir, zone == HitZone.Head);
        zombie.OnHit(zone);
    }
}
