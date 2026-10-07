using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// [#74] Le zombie ENJAMBE la fenêtre au lieu de glisser à travers le mur.
// Les fenêtres sont reliées par un lien NavMesh (RoomBuilder.MakeOpening). Par défaut Unity fait glisser l'agent en ligne
// droite au niveau du sol ; ici on prend la main pendant ce passage :
//   1. il s'approche du mur ;  2. il se hisse sur le rebord en se penchant en avant (bras tendus : animation d'attaque) ;
//   3. il bascule et retombe à l'intérieur ;  4. il se redresse et repart vers le joueur.
// La hauteur du rebord est déduite de la planche la plus basse de la fenêtre. Ajouté au prefab par ZombieBuilder.
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

    public bool Climbing { get; private set; }

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
        if (best)
        {
            tWall = Vector3.Dot(best.transform.position - start, dir);
            float low = float.MaxValue;
            foreach (var s in best.slots) if (s) low = Mathf.Min(low, s.transform.position.y);
            if (low < float.MaxValue) sill = Mathf.Clamp(low - 0.15f - ground, 0.4f, 1.4f);
        }
        tWall = Mathf.Clamp(tWall, 0.3f, flat.magnitude - 0.3f);

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

        // 2. se hisse sur le rebord (bras tendus, penché en avant)
        anim?.Attack();
        a = Along(tWall - 0.35f, 0f); b = Along(tWall + 0.05f, sill);
        for (float t = 0; t < climbTime; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            float k = Mathf.SmoothStep(0f, 1f, t / climbTime);
            // monte d'abord, avance ensuite (on s'agrippe puis on passe le bassin)
            var p = Vector3.Lerp(a, b, k); p.y = Mathf.Lerp(a.y, b.y, Mathf.Sqrt(k));
            transform.SetPositionAndRotation(p, rot);
            Lean(leanDegrees * k);
            anim?.SetMoveSpeed(0.25f);
            yield return null;
        }

        // 3. bascule et retombe à l'intérieur
        a = Along(tWall + 0.05f, sill); b = Along(tWall + 0.5f, 0f);
        for (float t = 0; t < dropTime; t += Time.deltaTime)
        {
            if (Abort()) yield break;
            float k = t / dropTime;
            var p = Vector3.Lerp(a, b, k); p.y = Mathf.Lerp(a.y, b.y, k * k);   // chute qui accélère
            transform.SetPositionAndRotation(p, rot);
            Lean(Mathf.Lerp(leanDegrees, 20f, k));
            yield return null;
        }

        // 4. se redresse
        for (float t = 0; t < recoverTime; t += Time.deltaTime)
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

    // mort pendant l'escalade : il retombe au sol là où il est
    bool Abort()
    {
        if (!(zombie && zombie.Dead) && agent.enabled) return false;
        var p = transform.position;
        RaycastHit hit;
        if (Physics.Raycast(p + Vector3.up * 0.1f, Vector3.down, out hit, 3f)) p.y = hit.point.y;
        transform.position = p;
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
