using UnityEngine;

// Animation procédurale des os du zombie « Shirtless Zombie FREE » (le modèle gratuit n'a AUCUN clip d'animation).
// Marche traînante, bras tendus devant, buste penché ; attaque = les deux bras s'abattent ; mort = chute en arrière.
// Les os sont tournés autour d'axes du MONDE (droite / haut du zombie) : ça marche quelle que soit l'orientation des os du rig.
// À remplacer par de vrais clips dans un Animator Controller si on en trouve (le pont ZombieAnimationBridge sait déjà piloter un Animator).
public class ZombieBoneAnim : MonoBehaviour
{
    public Transform visual;                                    // parent du modèle (tourné pour la chute)
    public Transform spine, neck, head, thighL, thighR, calfL, calfR, armL, armR, foreL, foreR;

    public float moveSpeed;
    public float stepsPerMeter = 0.9f;                          // cycles de marche par mètre parcouru
    public float leanDegrees = 14f;

    float phase, attackT = -1f, dieT = -1f;
    Quaternion rSpine, rNeck, rHead, rThighL, rThighR, rCalfL, rCalfR, rArmL, rArmR;
    Vector3 dirArmL, dirArmR;
    Vector3 visualPos0;
    bool ready;

    void Awake()
    {
        if (!spine) return;
        rSpine = spine.rotation; rNeck = neck ? neck.rotation : Quaternion.identity; rHead = head ? head.rotation : Quaternion.identity;
        rThighL = thighL.rotation; rThighR = thighR.rotation; rCalfL = calfL.rotation; rCalfR = calfR.rotation;
        rArmL = armL.rotation; rArmR = armR.rotation;
        dirArmL = (foreL.position - armL.position).normalized;
        dirArmR = (foreR.position - armR.position).normalized;
        if (visual) visualPos0 = visual.localPosition;
        phase = Random.value * 6.28f;
        ready = true;
    }

    public void PlayAttack() { if (dieT < 0f) attackT = 0f; }
    public void PlayDie() { if (dieT < 0f) { dieT = 0f; attackT = -1f; } }

    // tourne un os (rotation de repos `rest`, en monde) de `deg` degrés autour de l'axe monde `axis`
    static void Rot(Transform t, Quaternion rest, Vector3 axis, float deg) => t.rotation = Quaternion.AngleAxis(deg, axis) * rest;

    void LateUpdate()
    {
        if (!ready) return;
        var right = transform.right; var up = transform.up; var fwd = transform.forward;

        if (dieT >= 0f)
        {
            dieT += Time.deltaTime;
            if (visual)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(dieT / 0.9f));
                visual.localRotation = Quaternion.Euler(-88f * k, 0f, 0f);
                visual.localPosition = visualPos0 + new Vector3(0f, 0.12f * k, 0f);
            }
            return;                                              // le reste du corps garde sa dernière pose
        }

        phase += moveSpeed * stepsPerMeter * 6.2832f * Time.deltaTime;
        float amp = Mathf.Clamp01(moveSpeed / 0.9f);             // 0 à l'arrêt, 1 à pleine marche
        float s = Mathf.Sin(phase), sOpp = Mathf.Sin(phase + Mathf.PI);

        float atk = 0f;                                          // 0..1..0 pendant l'attaque (0,7 s)
        if (attackT >= 0f)
        {
            attackT += Time.deltaTime;
            float u = attackT / 0.7f;
            atk = u < 1f ? Mathf.Sin(u * Mathf.PI) : 0f;
            if (u >= 1f) attackT = -1f;
        }

        // jambes : cuisse qui avance/recule, genou qui plie quand la jambe se lève
        Rot(thighL, rThighL, right, -28f * amp * s);
        Rot(thighR, rThighR, right, -28f * amp * sOpp);
        Rot(calfL, rCalfL, right, 40f * amp * Mathf.Max(0f, Mathf.Cos(phase)));
        Rot(calfR, rCalfR, right, 40f * amp * Mathf.Max(0f, Mathf.Cos(phase + Mathf.PI)));

        // buste penché en avant (plus à l'attaque), léger balancement
        Rot(spine, rSpine, right, leanDegrees + 18f * atk);
        if (neck) Rot(neck, rNeck, right, 6f);
        if (head) head.rotation = Quaternion.AngleAxis(4f * Mathf.Sin(phase * 0.5f), up) * rHead;

        // bras tendus devant (légèrement vers le haut) ; à l'attaque ils s'abattent vers le bas
        var armDir = Vector3.Slerp((fwd + up * 0.15f).normalized, (fwd - up * 0.9f).normalized, atk);
        var sway = right * (0.08f * Mathf.Sin(phase * 0.5f)) * amp;
        armL.rotation = Quaternion.FromToRotation(dirArmL, (armDir + sway).normalized) * rArmL;
        armR.rotation = Quaternion.FromToRotation(dirArmR, (armDir - sway).normalized) * rArmR;
    }
}
