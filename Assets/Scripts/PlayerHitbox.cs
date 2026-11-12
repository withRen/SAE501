using UnityEngine;

// Zone touchable d'un joueur en multijoueur (tête, torse).
// L'arme (Pistol, y compris fusil et sniper) appelle Hit avec SES dégâts ; OnShot garde ceux du pistolet pour les autres usages.
public class PlayerHitbox : MonoBehaviour
{
    public NetPlayer player;
    public HitZone zone = HitZone.Torso;

    void OnShot(RaycastHit hit) => Hit(50f, 20f, hit.point);        // pistolet : tête 50 PV (2 tirs), corps 20 PV (5 tirs)

    /// <summary>Dégâts de l'arme selon la zone touchée (jambes : legDamage, négatif = comme le corps).</summary>
    public void Hit(float headDamage, float bodyDamage, Vector3 point, float legDamage = -1f)
    {
        if (player == null || player.Dead) return;
        if (!ArenaMaps.InArena) return;         // [hotfix #163] coop : pas de tir ami (coups entre joueurs seulement en duel)
        float amount = zone == HitZone.Head ? headDamage : zone == HitZone.Legs && legDamage >= 0f ? legDamage : bodyDamage;
        player.TakeShotDamage(amount, point);
    }
}
