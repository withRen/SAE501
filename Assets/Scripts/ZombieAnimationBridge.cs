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

    // paramètres présents dans le contrôleur, lus une seule fois (animator.parameters renvoie une copie à chaque appel)
    bool cached, hasSpeed, hasAttack, hasDie;
    int speedId, attackId, dieId;

    void Cache()
    {
        if (animator == null) { cached = true; return; }
        var ps = animator.parameters;
        cached = ps.Length > 0;                 // contrôleur pas encore prêt : on relira au prochain appel
        foreach (var x in ps)
        {
            if (x.name == speedParam) hasSpeed = true;
            else if (x.name == attackTrigger) hasAttack = true;
            else if (x.name == dieTrigger) hasDie = true;
        }
        speedId = Animator.StringToHash(speedParam);
        attackId = Animator.StringToHash(attackTrigger);
        dieId = Animator.StringToHash(dieTrigger);
    }

    public void SetMoveSpeed(float s)
    {
        if (!cached) Cache();
        LastSpeed = s;
        if (animator != null && hasSpeed) animator.SetFloat(speedId, s);
        if (procedural != null) procedural.moveSpeed = s;
        if (boneAnim != null) boneAnim.moveSpeed = s;
    }

    public void Attack()
    {
        if (!cached) Cache();
        if (animator != null && hasAttack) animator.SetTrigger(attackId);
        if (procedural != null) procedural.PlayAttack();
        if (boneAnim != null) boneAnim.PlayAttack();
    }

    public void Die()
    {
        if (!cached) Cache();
        if (animator != null && hasDie) animator.SetTrigger(dieId);
        if (procedural != null) procedural.PlayDie();
        if (boneAnim != null) boneAnim.PlayDie();
    }
}
