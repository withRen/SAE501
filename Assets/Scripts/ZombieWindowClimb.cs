using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// [#74] Le zombie ENJAMBE la fenêtre au lieu de glisser à travers le mur.
// Les fenêtres sont reliées par un lien NavMesh (RoomBuilder.MakeOpening). Par défaut Unity fait glisser l'agent en ligne
// droite au niveau du sol ; ici on prend la main pendant ce passage :
//   1. il s'approche du mur ;  2. il se hisse sur le rebord en se penchant en avant (bras tendus : animation d'attaque) ;
//   3. il bascule et retombe à l'intérieur ;  4. il se redresse et repart vers le joueur.
// La hauteur du rebord est déduite de la planche la plus basse de la fenêtre. Ajouté au prefab par ZombieBuilder.
// Avec les vrais clips Mixamo (PR #146 : Climbing, Falling Flat Impact, Zombie Stand Up) : ces clips sont faits pour un mur
// de 2 m (mains à 2,15 m au départ, bassin à 2,8 m à la fin) et la chute commence en l'air (bassin à 1,7 m). Joués tels quels
// sur un rebord de 0,9 m, le zombie passait AU-DESSUS du cadre de la fenêtre puis tombait de haut. On garde donc le zombie
// au sol, centré sur la fenêtre, et on décale le modèle (Visual) après l'Animator (LateUpdate) pour que :
//   - en se hissant : les mains restent sur le rebord, le bassin avance dans l'ouverture ;
//   - en tombant : le corps part de là où il est (sur le rebord) et tombe juste derrière le mur (chute courte) ;
//   - en se relevant : le bassin reste où il est tombé (le clip le place 1 m plus loin au départ : plus de saut).
// À la fin, le décalage est reporté sur le zombie lui-même : pas de téléportation quand l'agent reprend la main.
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieWindowClimb : MonoBehaviour
{
    public Transform visual;                // le modèle (penché pendant l'escalade)
    public float approachSpeed = 0.8f;      // m/s jusqu'au pied du mur
    public float climbTime = 1.1f;          // se hisser sur le rebord
    public float dropTime = 0.5f;           // basculer et retomber
    public float recoverTime = 0.35f;       // se redresser
    public float leanDegrees = 55f;         // penché en avant au sommet
    public float hipHeight = 1.0f;          // point de bascule du corps (hanches)
    public float defaultSill = 0.9f;        // hauteur du rebord si aucune fenêtre n'est trouvée
    // avec les vraies animations (Climbing, Falling Flat Impact, Zombie Stand Up) : montée plus lente, et il se relève du sol
    public float climbTimeAnimated = 1.9f;   // 2e moitié du clip Climbing (ZombieMixamo.ClimbStartOffset, ×1,1)
    public float standUpTime = 2.6f;        // se relever (clip joué x1,5) avant de repartir

    bool Animated => anim != null && anim.HasClimbAnims && visual && hips && handL && handR;

    public bool Climbing { get; private set; }

    // décalage du modèle calculé après l'Animator (vrais clips) : phase en cours et repères
    enum Fit { None, Climb, Fall, Rise }
    Fit fit;
    float fitT, fitDur;
    Vector3 fitOffset;                       // décalage monde du modèle (appliqué à Visual)
    Vector3 hipsFrom, hipsTo;                // trajet voulu du bassin (x, z) pendant la phase
    float sillY, groundY;
    Transform hips, handL, handR;

    NavMeshAgent agent;
    Zombie zombie;
    ZombieAnimationBridge anim;
    Vector3 visualPos0;
    Quaternion visualRot0;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        zombie = GetComponent<Zombie>();
        anim = GetComponent<ZombieAnimationBridge>();
        if (!visual) visual = transform.Find("Visual");
        if (visual) { visualPos0 = visual.localPosition; visualRot0 = visual.localRotation; }
        var animator = GetComponentInChildren<Animator>();
        if (animator && animator.isHuman)
        {
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            handL = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            handR = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }
    }

    void Start() => agent.autoTraverseOffMeshLink = false;

    void Update()
    {
        if (Climbing || (zombie && zombie.Dead) || !agent.enabled || !agent.isOnOffMeshLink) return;
        StartCoroutine(Climb());
    }

    IEnumerator Climb()
    {
        Climbing = true;
        var link = agent.currentOffMeshLinkData;
        Vector3 start = transform.position;
        Vector3 end = link.endPos;
        if (end.y < start.y - 1f) { yield return Drop(start, end); yield break; }   // trou du toit : il se laisse tomber
        Vector3 flat = end - start; flat.y = 0f;
        Vector3 dir = flat.sqrMagnitude > 1e-4f ? flat.normalized : transform.forward;
        float ground = start.y;

        // la fenêtre la plus proche du lien : position du mur et hauteur du rebord
        float tWall = flat.magnitude * 0.45f, sill = defaultSill;
        Opening best = null; float bestD = 3f;
        foreach (var o in Opening.All)
        {
            var d = o.transform.position - (start + end) * 0.5f; d.y = 0f;
            if (d.magnitude < bestD) { bestD = d.magnitude; best = o; }
        }
        Vector3 center = (start + end) * 0.5f;
        if (best)
        {
            tWall = Vector3.Dot(best.transform.position - start, dir);
            float low = float.MaxValue;
            Vector3 sum = Vector3.zero; int n = 0;
            foreach (var s in best.slots) if (s) { low = Mathf.Min(low, s.transform.position.y); sum += s.transform.position; n++; }
            if (low < float.MaxValue) sill = Mathf.Clamp(low - 0.15f - ground, 0.4f, 1.4f);
            center = n > 0 ? sum / n : best.transform.position;
        }
        tWall = Mathf.Clamp(tWall, 0.3f, flat.magnitude - 0.3f);

        if (Animated) { yield return ClimbAnimated(start, end, dir, ground, tWall, sill, center); yield break; }

        System.Func<float, float, Vector3> Along = (t, h) => new Vector3(start.x, ground, start.z) + dir * t + Vector3.up * h;
        var rot = Quaternion.LookRotation(dir);

        // 1. approche jusqu'au pied du mur
        Vector3 a = transform.position, b = Along(tWall - 0.35f, 0f);
        float dur = Mathf.Max(0.05f, Vector3.Distance(a, b) / approachSpeed);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            transform.SetPositionAndRotation(Vector3.Lerp(a, b, t / dur), Quaternion.Slerp(transform.rotation, rot, 8f * Time.deltaTime));
            anim?.SetMoveSpeed(approachSpeed);
            yield return null;
        }

        // 2. se hisse sur le rebord (vrai clip d'escalade, sinon bras tendus de l'attaque + corps penché)
        bool real = Animated;
        float up = real ? climbTimeAnimated : climbTime;
        if (real) anim.ClimbUp(); else anim?.Attack();
        a = Along(tWall - 0.35f, 0f); b = Along(tWall + 0.05f, sill);
        for (float t = 0; t < up; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            float k = Mathf.SmoothStep(0f, 1f, t / up);
            // monte d'abord, avance ensuite (on s'agrippe puis on passe le bassin)
            var p = Vector3.Lerp(a, b, k); p.y = Mathf.Lerp(a.y, b.y, Mathf.Sqrt(k));
            transform.SetPositionAndRotation(p, rot);
            if (!real) Lean(leanDegrees * k);
            anim?.SetMoveSpeed(0.25f);
            yield return null;
        }

        // 3. bascule et retombe à l'intérieur (avec le clip : il tombe à plat ventre)
        if (real) anim.FallFlat();
        a = Along(tWall + 0.05f, sill); b = Along(tWall + 0.5f, 0f);
        for (float t = 0; t < dropTime; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            float k = t / dropTime;
            var p = Vector3.Lerp(a, b, k); p.y = Mathf.Lerp(a.y, b.y, k * k);   // chute qui accélère
            transform.SetPositionAndRotation(p, rot);
            if (!real) Lean(Mathf.Lerp(leanDegrees, 20f, k));
            yield return null;
        }

        // 4. se redresse (avec le clip : se relève du sol, lentement)
        if (real) { yield return Rise(); if (!Climbing) yield break; }
        for (float t = 0; t < (real ? 0f : recoverTime); t += Time.deltaTime)
        {
            if (Abort()) yield break;
            Lean(Mathf.Lerp(20f, 0f, t / recoverTime));
            anim?.SetMoveSpeed(0f);
            yield return null;
        }
        Lean(0f);

        // fin du lien : l'agent reprend la main au point d'arrivée (il marche jusqu'au bout du lien)
        a = transform.position; b = end;
        dur = Mathf.Max(0.05f, Vector3.Distance(a, b) / approachSpeed);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            transform.position = Vector3.Lerp(a, b, t / dur);
            anim?.SetMoveSpeed(approachSpeed);
            yield return null;
        }
        if (agent.enabled && agent.isOnOffMeshLink) agent.CompleteOffMeshLink();
        Climbing = false;
    }

    // ───────── vrais clips : le zombie reste au pied du mur, le modèle est placé par LateUpdate ─────────
    IEnumerator ClimbAnimated(Vector3 start, Vector3 end, Vector3 dir, float ground, float tWall, float sill, Vector3 center)
    {
        // ligne d'escalade recentrée sur la fenêtre (les bouts du lien NavMesh ne sont pas forcément au milieu)
        var lateral = Vector3.Cross(Vector3.up, dir);
        float side = Vector3.Dot(center - start, lateral);
        var origin = new Vector3(start.x, ground, start.z) + lateral * side;
        System.Func<float, Vector3> At = t => origin + dir * t;
        var rot = Quaternion.LookRotation(dir);
        groundY = ground; sillY = ground + sill;

        // 1. approche jusqu'au pied du mur, face à la fenêtre
        Vector3 a = transform.position, b = At(tWall - 0.45f);
        float dur = Mathf.Max(0.05f, Vector3.Distance(a, b) / approachSpeed);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            transform.SetPositionAndRotation(Vector3.Lerp(a, b, t / dur), Quaternion.Slerp(transform.rotation, rot, 8f * Time.deltaTime));
            anim.SetMoveSpeed(approachSpeed);
            yield return null;
        }
        transform.SetPositionAndRotation(b, rot);
        fitOffset = Vector3.zero;

        // 2. se hisse : mains sur le rebord, le bassin passe de devant le mur jusqu'au-dessus du rebord
        anim.ClimbUp();
        StartFit(Fit.Climb, climbTimeAnimated, At(tWall - 0.3f), At(tWall + 0.12f));
        for (float t = 0; t < climbTimeAnimated; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            anim.SetMoveSpeed(0f);
            yield return null;
        }

        // 3. bascule et tombe à plat, juste derrière le mur (le corps part du rebord : chute de moins d'1 m)
        anim.FallFlat();
        StartFit(Fit.Fall, dropTimeAnimated, hips ? hips.position : At(tWall), At(tWall + 0.6f));
        for (float t = 0; t < dropTimeAnimated; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            yield return null;
        }

        // 4. un instant au sol, puis se relève sur place
        for (float t = 0; t < 0.4f; t += Time.deltaTime) { if (Abort()) yield break; anim.SetMoveSpeed(0f); yield return null; }
        anim.StandUp();
        StartFit(Fit.Rise, standUpTime, hips ? hips.position : At(tWall + 0.6f), hips ? hips.position : At(tWall + 0.6f));
        for (float t = 0; t < standUpTime; t += Time.deltaTime) { if (Abort()) yield break; anim.SetMoveSpeed(0f); yield return null; }

        // le décalage horizontal du modèle passe au zombie lui-même (rien ne bouge à l'écran), puis l'agent reprend la main
        EndFit(true);
        a = transform.position; b = end;
        dur = Mathf.Max(0.05f, Vector3.Distance(a, b) / approachSpeed);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            transform.position = Vector3.Lerp(a, b, t / dur);
            anim.SetMoveSpeed(approachSpeed);
            yield return null;
        }
        if (agent.enabled && agent.isOnOffMeshLink) agent.CompleteOffMeshLink();
        Climbing = false;
    }

    public float dropTimeAnimated = 0.9f;   // chute (clip Falling Flat Impact : au sol vers 0,5 s)

    void StartFit(Fit f, float duration, Vector3 from, Vector3 to)
    {
        fit = f; fitT = 0f; fitDur = Mathf.Max(0.05f, duration);
        hipsFrom = from; hipsTo = to;
    }

    // fin : le décalage horizontal est reporté sur la position du zombie, le modèle revient à sa place
    void EndFit(bool keepPlace)
    {
        if (keepPlace)
        {
            var h = fitOffset; h.y = 0f;
            var p = transform.position + h; p.y = groundY;
            transform.position = p;
        }
        fit = Fit.None;
        fitOffset = Vector3.zero;
        if (visual) { visual.localPosition = visualPos0; visual.localRotation = visualRot0; }
    }

    // après l'Animator : place le modèle (Visual) pour que le bassin et les mains soient là où il faut
    void LateUpdate()
    {
        if (fit == Fit.None || !visual || !hips) return;
        fitT += Time.deltaTime;
        float k = Mathf.Clamp01(fitT / fitDur);
        // position des os sans le décalage de l'image précédente
        var hipsRaw = hips.position - fitOffset;
        var handsRaw = (handL.position + handR.position) * 0.5f - fitOffset;
        Vector3 want = fitOffset;

        switch (fit)
        {
            case Fit.Climb:
            {
                // hauteur : les mains sur le rebord pendant les 2/3 du geste, puis le corps finit de passer (décalage figé)
                if (k < 0.65f) want.y = sillY + 0.03f - handsRaw.y;
                // horizontal : le bassin suit la ligne d'escalade, du pied du mur jusqu'au-dessus du rebord
                var goal = Vector3.Lerp(hipsFrom, hipsTo, Mathf.SmoothStep(0f, 1f, k));
                want.x = goal.x - hipsRaw.x; want.z = goal.z - hipsRaw.z;
                break;
            }
            case Fit.Fall:
            {
                // part de la hauteur atteinte sur le rebord (hipsFrom) et finit posé au sol (décalage vertical nul)
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(fitT / 0.55f));
                float startY = hipsFrom.y - hipsRaw.y;
                if (fitT < 0.12f) fallStartY = startY;                  // le temps du fondu vers le clip de chute
                want.y = Mathf.Lerp(fallStartY, 0f, s);
                var goal = Vector3.Lerp(hipsFrom, hipsTo, s);
                want.x = goal.x - hipsRaw.x; want.z = goal.z - hipsRaw.z;
                break;
            }
            case Fit.Rise:
            {
                // le bassin ne bouge pas à l'horizontale pendant qu'il se relève ; hauteur : celle du clip (au sol)
                want.y = 0f;
                want.x = hipsFrom.x - hipsRaw.x; want.z = hipsFrom.z - hipsRaw.z;
                break;
            }
        }
        // lissé (pas de saut d'une image à l'autre quand l'Animator passe d'un clip à l'autre)
        fitOffset = Vector3.Lerp(fitOffset, want, 1f - Mathf.Exp(-25f * Time.deltaTime));
        visual.position = transform.TransformPoint(visualPos0) + fitOffset;
        visual.localRotation = visualRot0;
    }

    float fallStartY;

    // trou du toit du bunker : marche jusqu'au-dessus du trou, chute libre jusqu'au sol, se redresse
    IEnumerator Drop(Vector3 start, Vector3 end)
    {
        var flat = end - start; flat.y = 0f;
        var rot = flat.sqrMagnitude > 1e-4f ? Quaternion.LookRotation(flat.normalized) : transform.rotation;
        Vector3 a = start, b = new Vector3(end.x, start.y, end.z);
        float dur = Mathf.Max(0.05f, flat.magnitude / approachSpeed);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            transform.SetPositionAndRotation(Vector3.Lerp(a, b, t / dur), Quaternion.Slerp(transform.rotation, rot, 8f * Time.deltaTime));
            anim?.SetMoveSpeed(approachSpeed);
            yield return null;
        }
        float h = start.y - end.y, fall = Mathf.Sqrt(2f * h / 9.81f);
        bool real = Animated;
        if (real) anim.FallFlat();
        for (float t = 0; t < fall; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            transform.position = new Vector3(end.x, start.y - 0.5f * 9.81f * t * t, end.z);
            if (!real) Lean(Mathf.Lerp(0f, 25f, t / fall));
            yield return null;
        }
        transform.position = end;
        if (real) { yield return Rise(); if (!Climbing) yield break; }
        for (float t = 0; t < (real ? 0f : recoverTime * 2f); t += Time.deltaTime)
        {
            if (Abort()) yield break;
            Lean(Mathf.Lerp(25f, 0f, t / (recoverTime * 2f)));
            anim?.SetMoveSpeed(0f);
            yield return null;
        }
        Lean(0f);
        if (agent.enabled && agent.isOnOffMeshLink) agent.CompleteOffMeshLink();
        Climbing = false;
    }

    // au sol après la chute : un court instant à plat, puis il se relève (clip Zombie Stand Up)
    IEnumerator Rise()
    {
        for (float t = 0; t < 0.4f; t += Time.deltaTime) { if (Abort()) yield break; anim.SetMoveSpeed(0f); yield return null; }
        anim.StandUp();
        for (float t = 0; t < standUpTime; t += Time.deltaTime) { if (Abort()) yield break; anim.SetMoveSpeed(0f); yield return null; }
    }

    // mort pendant l'escalade : il retombe au sol là où il est
    bool Abort()
    {
        if (!(zombie && zombie.Dead) && agent.enabled) return false;
        var p = transform.position;
        RaycastHit hit;
        if (Physics.Raycast(p + Vector3.up * 0.1f, Vector3.down, out hit, 3f)) p.y = hit.point.y;
        transform.position = p;
        if (fit != Fit.None) EndFit(false);
        Lean(0f);
        Climbing = false;
        return true;
    }

    // penche le modèle en avant autour des hanches
    void Lean(float deg)
    {
        if (!visual) return;
        var q = Quaternion.Euler(deg, 0f, 0f);
        visual.localRotation = q * visualRot0;
        visual.localPosition = visualPos0 + (Vector3.up * hipHeight - q * (Vector3.up * hipHeight));
    }
}
