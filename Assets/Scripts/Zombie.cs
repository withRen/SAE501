using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// Comportement d'un zombie (GDD : 4 états, très prévisibles) :
//   Marche  : va vers l'ouverture accessible la plus proche (chemin NavMesh)
//   Casse   : arrache une planche toutes les 3 secondes tant qu'il en reste
//   Attaque : ouverture libre -> entre dans le bunker et frappe le joueur
//   Mort    : tête = mort immédiate ; torse (33 %) et jambes (20 %) retirent une part des PV (voir ZombieLevels)
[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    public enum State { Walking, Breaking, Entering, Attacking, Dead }

    [Header("Déplacement")]
    public float walkSpeed = 0.9f;          // vague 1 : très lent
    public float slowFactor = 0.5f;         // vitesse pendant le ralentissement
    public float slowDuration = 1.8f;

    [Header("Barricades")]
    public float breakInterval = 3f;        // (ancien réglage : remplacé par hitInterval, voir 4.4)
    public float hitInterval = 1f;          // [4.4] un coup par seconde : 3 coups pour une planche clouée de 2 clous = 3 s

    [Header("Attaque")]
    public float attackRange = 1.15f;
    public float attackCooldown = 1.5f;
    [Tooltip("Part de la vie max du joueur retirée à chaque coup (0..1). Fixée par le niveau, voir ZombieLevels.")]
    public float attackDamage = 0.2f;
    public float attackHitDelay = 0.45f;    // moment du coup dans l'animation

    [Header("Vie")]
    public int level = 1;
    public float Health { get; private set; } = ZombieLevels.BaseHealth;
    public float MaxHealth { get; private set; } = ZombieLevels.BaseHealth;

    public State state { get; private set; }
    public Opening target { get; private set; }
    public bool Dead => state == State.Dead;

    NavMeshAgent agent;
    ZombieAnimationBridge anim;
    float slowUntil, nextBreak, nextAttack, hitAt = -1f, repath;
    Plank pendingPlank;
    PlayerHealth player;

    // règle PV et dégâts selon le niveau (à appeler juste après la création, avant Start)
    public void ApplyLevel(int lvl)
    {
        level = Mathf.Clamp(lvl, 1, ZombieLevels.MaxLevel);
        var st = ZombieLevels.For(level);
        MaxHealth = Health = ZombieLevels.BaseHealth * st.hpMultiplier;
        attackDamage = st.playerDamage;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<ZombieAnimationBridge>();
    }

    void Start()
    {
        player = PlayerHealth.Instance;
        agent.speed = walkSpeed;
        ChooseOpening();
    }

    // ouverture accessible la plus proche (longueur du chemin réel, pas la distance à vol d'oiseau)
    void ChooseOpening()
    {
        float best = float.MaxValue;
        target = null;
        var path = new NavMeshPath();
        foreach (var o in Opening.All.Where(x => x.zombiesCanUse))
        {
            if (!NavMesh.CalculatePath(transform.position, o.approachPoint.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            float len = 0f;
            for (int i = 1; i < path.corners.Length; i++) len += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            if (len < best) { best = len; target = o; }
        }
        if (target != null) { state = State.Walking; agent.SetDestination(target.approachPoint.position); }
    }

    void Update()
    {
        if (Dead) return;
        if (player == null) player = PlayerHealth.Instance;

        agent.speed = walkSpeed * (Time.time < slowUntil ? slowFactor : 1f);
        anim?.SetMoveSpeed(agent.isStopped ? 0f : agent.velocity.magnitude);

        switch (state)
        {
            case State.Walking: UpdateWalking(); break;
            case State.Breaking: UpdateBreaking(); break;
            case State.Entering: UpdateEntering(); break;
            case State.Attacking: UpdateAttacking(); break;
        }
    }

    bool Arrived() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.15f;

    void UpdateWalking()
    {
        if (target == null) { ChooseOpening(); return; }
        if (!Arrived()) return;
        agent.isStopped = true;
        if (target.IsBarricaded) { state = State.Breaking; nextBreak = Time.time + 0.5f; }
        else StartEntering();
    }

    void UpdateBreaking()
    {
        FaceToward(target.transform.position);
        if (pendingPlank == null && Time.time >= nextBreak)
        {
            pendingPlank = target.NextPlankToBreak();
            if (pendingPlank == null) { StartEntering(); return; }
            anim?.Attack();
            hitAt = Time.time + attackHitDelay;
            nextBreak = Time.time + hitInterval;
        }
        if (pendingPlank != null && Time.time >= hitAt)
        {
            pendingPlank.Hit((target.approachPoint.position - target.transform.position).normalized * -1f);   // [4.4] un coup : tremble, clou qui saute, puis chute vers l'intérieur
            pendingPlank = null;
            if (!target.IsBarricaded) StartEntering();
        }
    }

    void StartEntering()
    {
        state = State.Entering;
        agent.isStopped = false;
        repath = 0f;
    }

    void UpdateEntering()
    {
        if (player == null || player.Dead) { agent.isStopped = true; return; }
        if (Time.time >= repath)
        {
            repath = Time.time + 0.25f;
            var p = player.HeadPosition;
            if (NavMesh.SamplePosition(new Vector3(p.x, 0f, p.z), out var hit, 2f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
        }
        if (FlatDistanceToPlayer() <= attackRange) { agent.isStopped = true; state = State.Attacking; nextAttack = Time.time; }
    }

    void UpdateAttacking()
    {
        if (player == null || player.Dead) return;
        FaceToward(player.HeadPosition);
        float d = FlatDistanceToPlayer();
        if (d > attackRange * 1.4f) { StartEntering(); return; }   // le joueur a reculé : on le poursuit
        if (Time.time >= nextAttack)
        {
            nextAttack = Time.time + attackCooldown;
            anim?.Attack();
            hitAt = Time.time + attackHitDelay;
        }
        if (hitAt > 0f && Time.time >= hitAt)
        {
            hitAt = -1f;
            if (FlatDistanceToPlayer() <= attackRange * 1.3f) player.TakeDamageFraction(attackDamage);
        }
    }

    float FlatDistanceToPlayer()
    {
        var p = player.HeadPosition;
        return Vector2.Distance(new Vector2(p.x, p.z), new Vector2(transform.position.x, transform.position.z));
    }

    void FaceToward(Vector3 point)
    {
        var d = point - transform.position; d.y = 0f;
        if (d.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(d), 360f * Time.deltaTime);
    }

    // appelé par le tir (voir ZombieHitbox) : la tête tue d'un coup, le torse et les jambes retirent une part des PV de base
    public void OnHit(HitZone zone)
    {
        if (Dead) return;
        if (zone == HitZone.Head) { Health = 0f; Die(); return; }

        Health -= ZombieLevels.BaseHealth * ZombieLevels.ShotFraction(zone);
        if (Health <= 0.01f) { Health = 0f; Die(); return; }
        slowUntil = Time.time + (zone == HitZone.Legs ? slowDuration * 1.5f : slowDuration);     // touché : ralenti (plus longtemps aux jambes)
    }

    public void Die()
    {
        if (Dead) return;
        state = State.Dead;
        agent.isStopped = true;
        agent.enabled = false;
        anim?.Die();
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        Destroy(gameObject, 6f);
    }
}
