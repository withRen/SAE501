using UnityEngine;

// Animation procédurale du zombie provisoire (marche, attaque, mort) : sert à tester le comportement sans modèle animé.
public class ZombieProcAnim : MonoBehaviour
{
    public Transform visual;            // racine visuelle (pour la chute)
    public Transform armL, armR, legL, legR;
    [HideInInspector] public float moveSpeed;

    float attackT = -1f, dieT = -1f, phase;

    public void PlayAttack() { attackT = 0f; }
    public void PlayDie() { dieT = 0f; }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dieT >= 0f)
        {
            dieT += dt;
            float k = Mathf.Clamp01(dieT / 0.8f);
            visual.localRotation = Quaternion.Euler(Mathf.Lerp(0f, -88f, k * k), 0f, 0f);
            visual.localPosition = new Vector3(0f, Mathf.Lerp(0f, 0.18f, k), Mathf.Lerp(0f, -0.4f, k));
            return;
        }

        phase += dt * Mathf.Lerp(0f, 5f, Mathf.Clamp01(moveSpeed / 0.9f));
        float swing = Mathf.Sin(phase) * Mathf.Clamp01(moveSpeed / 0.9f);
        legL.localRotation = Quaternion.Euler(swing * 28f, 0f, 0f);
        legR.localRotation = Quaternion.Euler(-swing * 28f, 0f, 0f);
        visual.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Cos(phase)) * 0.025f * Mathf.Clamp01(moveSpeed / 0.9f), 0f);

        // bras tendus devant (zombie), qui oscillent un peu ; pendant l'attaque ils se lèvent puis s'abattent
        float armBase = 78f + swing * 6f;
        float armAngle = armBase;
        if (attackT >= 0f)
        {
            attackT += dt;
            float u = attackT / 0.9f;
            armAngle = u < 0.45f ? Mathf.Lerp(armBase, 160f, u / 0.45f) : Mathf.Lerp(160f, 30f, Mathf.Clamp01((u - 0.45f) / 0.2f));
            if (u > 0.9f) attackT = -1f;
        }
        armL.localRotation = Quaternion.Euler(-armAngle, 0f, 0f);
        armR.localRotation = Quaternion.Euler(-armAngle, 0f, 0f);
    }
}
