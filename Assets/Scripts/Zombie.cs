using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// Comportement d'un zombie (GDD : 4 états, très prévisibles) :
//   Marche  : va vers l'ouverture accessible la plus proche (chemin NavMesh)
//   Casse   : frappe les planches (un coup toutes les 2 s) : planche posée sans clou = 1 coup, clouée = 3 coups
//   Attaque : ouverture libre -> entre dans le bunker et frappe le joueur
//   Mort    : tête = mort immédiate ; torse (33 %) et jambes (20 %) retirent une part des PV (voir ZombieLevels)
//             armes de corps à corps (MeleeWeapon) : dégâts propres à chaque arme
[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    public enum State { Walking, Breaking, Entering, Attacking, Dead }

    [Header("Déplacement")]
    public float walkSpeed = 0.9f;          // vague 1 : très lent
    public float slowFactor = 0.5f;         // vitesse pendant le ralentissement
    public float slowDuration = 1.8f;

    [Header("Barricades")]
    public float breakInterval = 2f;        // un coup toutes les 2 s : planche posée sans clou = 1 coup, planche clouée = 3 coups

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
    float slowUntil, nextBreak, nextAttack, hitAt = -1f, repath, nextChoose;
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

    // Liens de passage des fenêtres et des trous du toit (zone NavMesh 3) : réservés aux zombies qui ENTRENT.
    // En marche vers leur ouverture, ils ne peuvent pas les emprunter : sinon un zombie qui visait une porte côté couloir
    // passait par une fenêtre sans casser ses planches et traversait le bunker sans s'occuper du joueur.
    public const int PassageArea = 3;
    public static int WalkMask => NavMesh.AllAreas & ~(1 << PassageArea);

    void Start()
    {
        player = PlayerHealth.Instance;
        agent.speed = walkSpeed;
        agent.areaMask = WalkMask;
        ChooseOpening();
    }

    // ouverture accessible (longueur du chemin réel) : tirée au sort parmi celles à peine plus loin que la plus proche,
    // pour que les zombies se répartissent (les deux fenêtres, les deux trous du toit) au lieu de tous prendre la même
    public float openingSpread = 6f;     // m de plus que la plus proche encore acceptés

    void ChooseOpening()
    {
        target = null;
        var path = new NavMeshPath();
        var found = new System.Collections.Generic.List<(Opening o, float len)>();
        foreach (var o in Opening.All.Where(x => x.zombiesCanUse))
        {
            if (!NavMesh.CalculatePath(transform.position, o.approachPoint.position, WalkMask, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            float len = 0f;
            for (int i = 1; i < path.corners.Length; i++) len += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            found.Add((o, len));
        }
        if (found.Count == 0) return;
        float best = found.Min(f => f.len);
        var near = found.Where(f => f.len <= best + openingSpread).ToList();
        target = near[Random.Range(0, near.Count)].o;
        state = State.Walking;
        agent.SetDestination(target.approachPoint.position);
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
        if (target == null)
        {
            // aucune ouverture accessible : on réessaie une fois par seconde (pas à chaque image)
            if (Time.time >= nextChoose) { nextChoose = Time.time + 1f; ChooseOpening(); }
            return;
        }
        if (!Arrived()) return;
        agent.isStopped = true;
        if (target.IsBarricaded || target.DoorClosed) { state = State.Breaking; nextBreak = Time.time + 0.5f; }
        else StartEntering();
    }

    void UpdateBreaking()
    {
        FaceToward(target.transform.position);
        // porte fermée devant les planches : il l'enfonce d'abord (HingedDoor : 3 coups)
        if (target.DoorClosed)
        {
            if (Time.time >= nextBreak && hitAt < 0f) { anim?.Attack(); hitAt = Time.time + attackHitDelay; nextBreak = Time.time + breakInterval; }
            if (hitAt > 0f && Time.time >= hitAt) { hitAt = -1f; target.door.ZombieHit(); }
            return;
        }
        if (pendingPlank == null && Time.time >= nextBreak)
        {
            pendingPlank = target.NextPlankToBreak();
            if (pendingPlank == null) { StartEntering(); return; }
            anim?.Attack();
            hitAt = Time.time + attackHitDelay;
            nextBreak = Time.time + breakInterval;
        }
        if (pendingPlank != null && Time.time >= hitAt)
        {
            pendingPlank.TakeHit((target.approachPoint.position - target.transform.position).normalized * -1f);   // si elle cède, elle part vers l'intérieur
            pendingPlank = null;
            if (!target.IsBarricaded) StartEntering();
        }
    }

    void StartEntering()
    {
        state = State.Entering;
        agent.areaMask = NavMesh.AllAreas;           // il peut maintenant passer par sa fenêtre / le trou du toit
        if (target && target.door) target.door.ForceOpen();
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

    // appelé par une arme de corps à corps (MeleeWeapon) : retire une part des PV de base, ralentit et fait reculer un peu
    public void OnMeleeHit(HitZone zone, float fractionOfBase)
    {
        if (Dead) return;
        Health -= ZombieLevels.BaseHealth * fractionOfBase;
        if (Health <= 0.01f) { Health = 0f; Die(); return; }
        slowUntil = Time.time + slowDuration;
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
