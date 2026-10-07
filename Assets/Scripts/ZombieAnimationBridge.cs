using UnityEngine;

// Fait le lien entre le comportement (Zombie) et l'animation.
// - Avec un Animator (modèle réel) : paramètres "Speed" (float), "Attack" (trigger), "Die" (trigger).
//   Les paramètres absents sont ignorés, pour s'adapter au contrôleur du modèle.
// - Sans Animator (zombie provisoire en boîtes) : animation procédurale.
public class ZombieAnimationBridge : MonoBehaviour
{
    public Animator animator;
    public ZombieProcAnim procedural;
    public ZombieBoneAnim boneAnim;            // modèle réel sans clips : os animés par script

    public string speedParam = "Speed";
    public string attackTrigger = "Attack";
    public string dieTrigger = "Die";

    public float LastSpeed { get; private set; }

    bool Has(string p)
    {
        if (animator == null || string.IsNullOrEmpty(p)) return false;
        foreach (var x in animator.parameters) if (x.name == p) return true;
        return false;
    }

    public void SetMoveSpeed(float s)
    {
        LastSpeed = s;
        if (animator != null && Has(speedParam)) animator.SetFloat(speedParam, s);
        if (procedural != null) procedural.moveSpeed = s;
        if (boneAnim != null) boneAnim.moveSpeed = s;
    }

    public void Attack()
    {
        if (animator != null && Has(attackTrigger)) animator.SetTrigger(attackTrigger);
        if (procedural != null) procedural.PlayAttack();
        if (boneAnim != null) boneAnim.PlayAttack();
    }

    public void Die()
    {
        if (animator != null && Has(dieTrigger)) animator.SetTrigger(dieTrigger);
        if (procedural != null) procedural.PlayDie();
        if (boneAnim != null) boneAnim.PlayDie();
    }
}
