using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// Comportement d'un zombie (GDD : 4 états, très prévisibles) :
//   Marche  : va vers l'ouverture accessible la plus proche (chemin NavMesh)
//             joueur hors du bunker, visible à moins de 3,5 m : il le poursuit (et le frappe) au lieu de l'ignorer
//   Casse   : frappe les planches (un coup toutes les 2 s) : planche posée sans clou = 1 coup, clouée = 3 coups
//   Attaque : ouverture libre -> entre dans le bunker et frappe le joueur
//   [#104] Toit (niveau 3+) : plus de chute surprise. Ses pas résonnent sur le toit, puis il attend 3 à 4 s au bord du trou
//             (état Lurking) pendant que de la poussière tombe dans le bunker (Opening.WarnFromAbove), et seulement ensuite il saute.
//   Mort    : tête = mort immédiate ; torse (33 %) et jambes (20 %) retirent une part des PV (voir ZombieLevels)
//             armes de corps à corps (MeleeWeapon) : dégâts propres à chaque arme
//   [#113] Touché : gerbe de particules au point d'impact (Fx.Blood), son de chair (craquement à la tête), petit recul.
//   [#117] Yeux lumineux visibles au-delà de 8 m (ZombieEyes, ajouté ici s'il manque au prefab).
//   Rééquilibrage : à la barricade, il frappe le joueur trop proche à travers l'ouverture (bras passés entre les planches) ;
//             au niveau 4, dès que le générateur est prêt (HuntPlayer), il traque le joueur hors du bunker sans le lâcher.
[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    public enum State { Walking, Breaking, Entering, Attacking, Chasing, Lurking, Dead }

    [Header("Déplacement")]
    public float walkSpeed = 0.9f;          // vague 1 : très lent
    public float slowFactor = 0.5f;         // vitesse pendant le ralentissement
    public float slowDuration = 1.8f;

    [Header("Barricades")]
    public float breakInterval = 2f;        // un coup toutes les 2 s : planche posée sans clou = 1 coup, planche clouée = 3 coups
    [Tooltip("Joueur à moins de cette distance (à plat) pendant qu'il casse : il le frappe à travers l'ouverture au lieu de taper la planche.")]
    public float barricadeReach = 1.6f;     // il se tient ~1 m devant l'ouverture : le joueur doit rester à ~60 cm en retrait
    [Range(0f, 1f)] public float barricadeDamageFactor = 0.5f;   // coup à travers les planches : moitié des dégâts du niveau

    [Header("Attaque")]
    public float attackRange = 1.15f;
    public float attackCooldown = 1.5f;
    [Tooltip("Part de la vie max du joueur retirée à chaque coup (0..1). Fixée par le niveau, voir ZombieLevels.")]
    public float attackDamage = 0.2f;
    public float attackHitDelay = 0.45f;    // moment du coup dans l'animation

    [Header("Poursuite hors du bunker")]
    public float chaseRange = 3.5f;         // joueur hors du bunker, visible à cette distance : le zombie le poursuit
    public float chaseLoseRange = 7f;       // au-delà (ou joueur rentré dans le bunker, ou caché 2 s) : il reprend sa marche vers son ouverture
    // Niveau 4, générateur prêt (WaveManager) : tous les zombies traquent le joueur dès qu'il quitte le bunker, sans distance ni vue
    public static bool HuntPlayer;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => HuntPlayer = false;

    [Header("Toit : avertissement avant la chute [#104]")]
    public Vector2 dropWarning = new Vector2(3f, 4f);    // secondes de poussière avant de sauter par le trou
    public float roofHeight = 2.2f;                      // au-dessus : il marche sur le toit (pas audibles)
    public float roofStepInterval = 0.55f;

    [Header("Impact [#113]")]
    public float flinchDistance = 0.12f;    // recul (m) à chaque balle ; double à la tête
    [Range(0f, 1f)] public float fleshVolume = 0.9f;

    [Header("Sécurité : zombie coincé")]
    [Tooltip("Immobile (moins de 0,5 m parcouru) depuis ce temps alors qu'il devrait marcher : il meurt, pour que la vague se termine toujours.")]
    public float stuckTimeout = 20f;

    [Header("Type de zombie (voir ZombieVariants)")]
    public string typeName = "Normal";
    public float speedMultiplier = 1f;      // x la vitesse donnée par le spawner
    public float healthMultiplier = 1f;     // x les PV du niveau (la tête tue toujours d'un coup)
    public int minLevel = 1;                // vague à partir de laquelle ce type peut apparaître
    public float spawnWeight = 1f;          // chance d'apparaître par rapport aux autres types

    [Header("Vie")]
    public int level = 1;
    public float Health { get; private set; } = ZombieLevels.BaseHealth;
    public float MaxHealth { get; private set; } = ZombieLevels.BaseHealth;

    public State state { get; private set; }
    public Opening target { get; private set; }
    public bool Dead => state == State.Dead;

    NavMeshAgent agent;
    ZombieAnimationBridge anim;
    float slowUntil, nextBreak, nextAttack, hitAt = -1f, repath, nextChoose, nextSight, lastSeen;
    Plank pendingPlank;
    PlayerHealth player;
    bool chasing;                           // l'attaque en cours vient d'une poursuite hors du bunker (pas d'une entrée)
    bool pendingPlayerHit;                  // coup en cours à travers l'ouverture (barricade)
    bool lastHitWasPlayer;                  // dernier coup à la barricade porté au joueur : le suivant va à une planche (alternance)
    bool lastHitMelee;                      // dernier coup reçu : arme de mêlée (pour le bilan)
    Vector3 stuckAnchor;
    float stuckSince, lurkUntil, nextRoofStep;
    AudioSource roofSrc, hitSrc;
    static AudioClip roofStep, flesh, headCrack;
    Vector3 flinchDir; float flinchSpeed;
    static readonly RaycastHit[] sightHits = new RaycastHit[8];

    // intérieur du bunker (repère Unity ; MAP.md : Blender x -7,5..3,8 · y -14,1..-6,1) : là, le joueur est protégé par ses barricades
    public static readonly Bounds BunkerArea = new Bounds(new Vector3(1.85f, 1.5f, 10.1f), new Vector3(11.3f, 4f, 8f));

    // règle PV et dégâts selon le niveau (à appeler juste après la création, avant Start)
    public void ApplyLevel(int lvl)
    {
        level = Mathf.Clamp(lvl, 1, ZombieLevels.MaxLevel);
        var st = ZombieLevels.For(level);
        MaxHealth = Health = ZombieLevels.BaseHealth * st.hpMultiplier * Mathf.Max(0.1f, healthMultiplier);
        attackDamage = st.playerDamage * GameSettings.DamageMultiplier;    // difficulté du menu : Facile x0,75, Difficile x1,25
    }

    // [Survie] au-delà du dernier niveau défini, ZombieLevels n'augmente plus rien : WaveManager ajoute ici des PV et des dégâts en plus
    // (à appeler juste après la création, après ApplyLevel)
    public void ApplyEndlessScaling(float hpFactor, float damageFactor)
    {
        MaxHealth *= Mathf.Max(0.1f, hpFactor);
        Health = MaxHealth;
        attackDamage = Mathf.Min(1f, attackDamage * Mathf.Max(0.1f, damageFactor));
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<ZombieAnimationBridge>();
        if (!GetComponent<ZombieEyes>()) gameObject.AddComponent<ZombieEyes>();
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
        stuckAnchor = transform.position; stuckSince = Time.time;
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
            case State.Chasing: UpdateChasing(); break;
            case State.Lurking: UpdateLurking(); break;
        }
        RoofSteps();
        Flinch();
        if (!Dead) CheckStuck();
    }

    // Sécurité : un zombie qui devrait se déplacer (marche, entrée, poursuite) mais n'a pas bougé de 0,5 m depuis stuckTimeout
    // est coincé (NavMesh, décor, ouverture inaccessible) : il meurt, sinon la vague ne se termine jamais.
    // Casser des planches et frapper le joueur ne comptent pas : il est immobile mais il agit.
    void CheckStuck()
    {
        bool busy = state == State.Breaking || state == State.Attacking || state == State.Lurking;
        if (busy || (transform.position - stuckAnchor).sqrMagnitude > 0.25f)
        {
            stuckAnchor = transform.position; stuckSince = Time.time;
            return;
        }
        if (Time.time - stuckSince < stuckTimeout) return;
        Debug.Log($"[Zombie] {name} immobile depuis {stuckTimeout:0} s ({state}) : éliminé pour que la vague se termine.");
        Die();                              // pas compté dans le bilan : ce n'est pas le joueur qui l'a tué
    }

    bool Arrived() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.15f;

    void UpdateWalking()
    {
        if (TryStartChase()) return;
        if (target == null)
        {
            // aucune ouverture accessible : on réessaie une fois par seconde (pas à chaque image)
            if (Time.time >= nextChoose) { nextChoose = Time.time + 1f; ChooseOpening(); }
            return;
        }
        if (!Arrived()) return;
        agent.isStopped = true;
        if (target.IsBarricaded || target.DoorClosed) { state = State.Breaking; nextBreak = Time.time + 0.5f; }
        else if (target.fromAbove) StartLurking();
        else StartEntering();
    }

    // ---------- toit : il prévient avant de tomber [#104] ----------
    void StartLurking()
    {
        state = State.Lurking;
        float wait = Random.Range(dropWarning.x, dropWarning.y);
        lurkUntil = Time.time + wait;
        target.WarnFromAbove(wait);
    }

    void UpdateLurking()
    {
        FaceToward(target.insidePoint ? target.insidePoint.position : target.transform.position);
        if (Time.time >= lurkUntil) StartEntering();
    }

    // pas lourds sur le toit du bunker (en marche vers son trou et en attendant au bord) : on les entend de l'intérieur
    void RoofSteps()
    {
        if (Dead || !agent.enabled || transform.position.y < roofHeight || Time.time < nextRoofStep) return;
        bool moving = !agent.isStopped && agent.velocity.sqrMagnitude > 0.04f;
        if (!moving && state != State.Lurking) return;
        nextRoofStep = Time.time + roofStepInterval * Random.Range(0.85f, 1.15f) * (state == State.Lurking ? 1.4f : 1f);
        if (!roofSrc)
        {
            roofSrc = gameObject.AddComponent<AudioSource>();
            roofSrc.spatialBlend = 1f; roofSrc.minDistance = 2f; roofSrc.maxDistance = 16f; roofSrc.playOnAwake = false;
        }
        if (!roofStep) roofStep = ProcAudio.RoofStep();
        roofSrc.pitch = Random.Range(0.85f, 1.1f);
        roofSrc.PlayOneShot(roofStep, 1f);
    }

    void UpdateBreaking()
    {
        if (TryStartChase()) return;
        FaceToward(target.transform.position);
        // joueur collé à l'ouverture : il passe les bras entre les planches et le frappe (moitié des dégâts) au lieu de taper la planche.
        // Sans ça, tuer un zombie à la barricade (hache, pistolet à bout portant) ne coûtait rien.
        // Un coup sur deux seulement : le joueur ne protège pas ses planches avec son corps (le coup suivant va à une planche).
        if (pendingPlayerHit)
        {
            if (Time.time < hitAt) return;
            hitAt = -1f; pendingPlayerHit = false; lastHitWasPlayer = true;
            if (player != null && !player.Dead && FlatDistanceToPlayer() <= barricadeReach * 1.15f)
                player.TakeDamageFraction(attackDamage * barricadeDamageFactor);
            return;
        }
        if (!target.DoorClosed && pendingPlank == null && hitAt < 0f && !lastHitWasPlayer && Time.time >= nextBreak
            && player != null && !player.Dead && FlatDistanceToPlayer() <= barricadeReach)
        {
            pendingPlayerHit = true;
            FaceToward(player.HeadPosition);
            anim?.Attack();
            hitAt = Time.time + attackHitDelay;
            nextBreak = Time.time + breakInterval;
            return;
        }
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
            pendingPlank = null; hitAt = -1f;      // coup suivant : de nouveau possible sur le joueur
            lastHitWasPlayer = false;
            if (!target.IsBarricaded) StartEntering();
        }
    }

    // ---------- poursuite hors du bunker ----------
    // Hors du bunker, le joueur n'est protégé par rien : un zombie qui le voit à moins de chaseRange le poursuit.
    // Il reste sur les zones de marche (WalkMask) : pas de passage par les fenêtres ni les trous du toit, et il lâche
    // le joueur dès que celui-ci rentre dans le bunker (c'est alors aux barricades de jouer leur rôle).
    bool PlayerOutsideBunker() => player != null && !player.Dead && !BunkerArea.Contains(player.HeadPosition);

    bool CanSeePlayer(float range)
    {
        if (FlatDistanceToPlayer() > range) return false;
        var eye = transform.position + Vector3.up * 1.5f;
        var to = player.HeadPosition - eye;
        int n = Physics.RaycastNonAlloc(eye, to.normalized, sightHits, to.magnitude, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            var t = sightHits[i].transform;
            if (t.IsChildOf(transform) || t.GetComponentInParent<PlayerHealth>() != null) continue;    // lui-même, le joueur
            if (t.GetComponentInParent<Zombie>() != null) continue;                                   // un autre zombie ne cache pas le joueur
            return false;                                                                             // mur, porte, planche…
        }
        return true;
    }

    bool TryStartChase()
    {
        if (Time.time < nextSight) return false;
        nextSight = Time.time + 0.25f;                      // pas à chaque image
        if (!PlayerOutsideBunker() || (!HuntPlayer && !CanSeePlayer(chaseRange))) return false;
        pendingPlank = null; pendingPlayerHit = false; hitAt = -1f;
        StartChasing();
        return true;
    }

    void StartChasing()
    {
        state = State.Chasing;
        chasing = true;
        agent.areaMask = WalkMask;
        agent.isStopped = false;
        repath = 0f;
        lastSeen = Time.time;
    }

    void UpdateChasing()
    {
        if (!PlayerOutsideBunker()) { StopChasing(); return; }
        // générateur prêt : la traque ne s'arrête que si le joueur rentre dans le bunker
        if (!HuntPlayer && FlatDistanceToPlayer() > chaseLoseRange) { StopChasing(); return; }
        if (!HuntPlayer && Time.time >= nextSight)
        {
            nextSight = Time.time + 0.25f;
            if (CanSeePlayer(chaseLoseRange)) lastSeen = Time.time;
            else if (Time.time - lastSeen > 2f) { StopChasing(); return; }
        }
        if (Time.time >= repath)
        {
            repath = Time.time + 0.25f;
            var p = player.HeadPosition;
            if (NavMesh.SamplePosition(new Vector3(p.x, transform.position.y, p.z), out var hit, 2f, WalkMask)) agent.SetDestination(hit.position);
        }
        if (FlatDistanceToPlayer() <= attackRange) { agent.isStopped = true; state = State.Attacking; nextAttack = Time.time; }
    }

    // le joueur s'est échappé : retour vers une ouverture du bunker
    void StopChasing()
    {
        chasing = false;
        state = State.Walking;
        agent.isStopped = false;
        ChooseOpening();                    // aucune ouverture accessible : UpdateWalking réessaie chaque seconde
    }

    void StartEntering()
    {
        chasing = false;
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
        if (d > attackRange * 1.4f) { if (chasing) StartChasing(); else StartEntering(); return; }   // le joueur a reculé : on le poursuit
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
        lastHitMelee = false;
        if (zone == HitZone.Head) { Health = 0f; Die(true); return; }

        Health -= ZombieLevels.BaseHealth * ZombieLevels.ShotFraction(zone);
        if (Health <= 0.01f) { Health = 0f; Die(true); return; }
        slowUntil = Time.time + (zone == HitZone.Legs ? slowDuration * 1.5f : slowDuration);     // touché : ralenti (plus longtemps aux jambes)
    }

    // appelé par une arme de corps à corps (MeleeWeapon) : retire une part des PV de base, ralentit et fait reculer un peu
    public void OnMeleeHit(HitZone zone, float fractionOfBase)
    {
        if (Dead) return;
        lastHitMelee = true;
        Health -= ZombieLevels.BaseHealth * fractionOfBase;
        if (Health <= 0.01f) { Health = 0f; Die(true); return; }
        slowUntil = Time.time + slowDuration;
    }

    // [#113] retour d'impact (balle ou arme) : particules au point touché, son de chair, petit recul dans le sens du coup.
    // Appelé avant OnHit / OnMeleeHit : le zombie qui meurt du coup a lui aussi sa gerbe et son son.
    public void HitFeedback(Vector3 point, Vector3 direction, bool head)
    {
        if (Dead) return;
        Fx.Blood(point, -direction, head);
        if (!hitSrc)
        {
            hitSrc = gameObject.AddComponent<AudioSource>();
            hitSrc.spatialBlend = 1f; hitSrc.minDistance = 1.5f; hitSrc.maxDistance = 25f; hitSrc.playOnAwake = false;
        }
        if (!flesh) { flesh = ProcAudio.Flesh(); headCrack = ProcAudio.HeadCrack(); }
        hitSrc.pitch = Random.Range(0.85f, 1.1f);
        hitSrc.PlayOneShot(head ? headCrack : flesh, fleshVolume);
        direction.y = 0f;
        if (direction.sqrMagnitude > 1e-4f)
        {
            flinchDir = direction.normalized;
            flinchSpeed = (head ? 2f : 1f) * flinchDistance / 0.12f;      // le recul s'éteint en ~0,12 s
        }
    }

    void Flinch()
    {
        if (flinchSpeed <= 0.01f) { flinchSpeed = 0f; return; }
        if (agent.enabled && agent.isOnNavMesh && !agent.isOnOffMeshLink) agent.Move(flinchDir * flinchSpeed * Time.deltaTime);
        flinchSpeed *= Mathf.Exp(-Time.deltaTime / 0.06f);
    }

    // byPlayer : tué par le joueur (compté dans le bilan) ; faux pour la sécurité « coincé » et la fin de partie
    public void Die(bool byPlayer = false)
    {
        if (Dead) return;
        if (byPlayer) GameStats.Kill(lastHitMelee);
        state = State.Dead;
        agent.isStopped = true;
        agent.enabled = false;
        anim?.Die();
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        Destroy(gameObject, 6f);
    }
}
