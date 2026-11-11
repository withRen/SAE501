using System.Collections.Generic;
using UnityEngine;

// Corps d'un joueur vu par les autres (personnage humanoïde du pack Survivalist, voir PlayerSkin).
// Le joueur n'a que 3 points suivis : la tête et les deux mains (casque et manettes en VR ; caméra et mains flottantes en version PC).
// Le reste du corps est reconstruit ici, de la même façon dans les deux modes :
//   - jambes : animation idle / marche / course (Animator, paramètre Speed) selon la vitesse réelle de la tête ;
//   - corps  : placé sous la tête, tourné doucement dans la direction du regard ;
//   - tête   : l'os de la tête prend l'orientation du casque ;
//   - bras   : cinématique inverse à deux os (épaule, coude, poignet) vers chaque main, coude vers le bas et l'extérieur ;
//   - mains  : poignet orienté comme la manette, doigts refermés quand la main tient un objet (arme, marteau, planche, levier…) ;
//   - taille : le personnage est mis à l'échelle de la taille debout du joueur (plus grande hauteur de tête observée, 85 à 112 %) ;
//   - pieds  : jamais sous le sol (joueur accroupi : les genoux plient).
//   - arme à la 3e personne (joueur en version PC) : sa main droite est placée comme une arme « vue FPS », collée à sa caméra ;
//     vue de l'extérieur, ça donnait un bras replié vers le visage et une arme qui flotte. Quand il tient une arme à feu, on ignore
//     donc ses mains : l'arme est posée devant l'épaule droite dans l'axe du regard (fusil épaulé, pistolet bras tendus),
//     les deux mains vont dessus (poignée et garde-main) et le buste se penche avec le regard (HoldGun, appelé par NetPlayer).
// Sans cible (aperçu du menu), le personnage joue simplement son animation d'attente.
// Tout est fait dans LateUpdate, après l'Animator : pas besoin de la passe IK de Mecanim.
[DefaultExecutionOrder(200)]
public class PlayerAvatar : MonoBehaviour
{
    public Transform headTarget, leftTarget, rightTarget;
    [Tooltip("Hauteur du sol sous le joueur (y du XR Origin). NaN : pas de correction des pieds.")]
    public float floorY = float.NaN;
    public bool leftHolding, rightHolding;

    [Header("Réglages")]
    public Vector3 eyeOffset = new Vector3(0f, 0.09f, 0.10f);   // des yeux (caméra) à l'os de la tête, dans le repère de la tête
    public float wristBack = 0.10f;                              // du centre de la manette au poignet, vers l'arrière
    public float turnSpeed = 5f;
    public float curlRelaxed = 18f, curlHolding = 62f;          // degrés par phalange

    [Header("Arme tenue à la 3e personne (joueur en version PC)")]
    [Tooltip("Poignée du fusil depuis l'épaule droite, dans le repère de la visée (x droite, y haut, z avant), à l'échelle 1.")]
    public Vector3 rifleGrip = new Vector3(-0.04f, -0.06f, 0.15f);
    [Tooltip("Poignée du pistolet depuis l'épaule droite (bras tendus, tenue à deux mains).")]
    public Vector3 pistolGrip = new Vector3(-0.16f, -0.06f, 0.46f);
    [Tooltip("Part du tangage du regard prise par la colonne (le reste par la tête).")]
    public float spineShare = 0.6f;
    public float aimTurnSpeed = 18f;
    [Tooltip("Orientation des « manettes » virtuelles sur l'arme (degrés, repère de la visée) : main droite, main gauche au fusil (posée sur le garde-main), main gauche au pistolet (enveloppe la droite).")]
    public Vector3 rightGripRot = new Vector3(12f, 0f, 0f), leftRifleRot = new Vector3(-10f, 0f, -15f), leftPistolRot = new Vector3(20f, 15f, -25f);
    [Tooltip("Main gauche : au fusil, depuis la poignée (z = distance au garde-main, réduite à 50 % du canon) ; au pistolet, depuis la poignée.")]
    public Vector3 leftRifleOffset = new Vector3(0.01f, 0.10f, 0.40f), leftPistolOffset = new Vector3(-0.045f, -0.015f, 0.02f);

    Transform gunProp, sideProp, aimR, aimL;
    Vector3 gunGrip; Quaternion gunMuzzle = Quaternion.identity; float gunBarrel;
    Transform[] spineBones;
    Transform shoulderR;

    Animator anim;
    Transform hips, headBone, neck;
    Arm left, right;
    Leg legL, legR;
    Quaternion headBind;                 // rotation de l'os de la tête quand le modèle regarde vers +Z
    Vector3 lastHead; bool hasLast;
    float speed, yaw;
    float modelEye, standing, baseScale = 1f;   // hauteur des yeux du modèle (échelle 1), taille debout mesurée du joueur
    static readonly int SpeedId = Animator.StringToHash("Speed");

    class Arm
    {
        public Transform upper, lower, hand;
        public Quaternion bindFrameInv;      // repère (doigts, dos de la main) de la main en pose de repos, inversé, x la rotation de l'os
        public Transform[][] fingers;
        public float curl;
    }

    class Leg { public Transform upper, lower, foot; public float ankle; }

    void Awake() => Init();

    /// <summary>Mesure le squelette en pose de repos (appelé par Awake ; public pour l'aperçu de l'éditeur).</summary>
    public void Init()
    {
        if (left != null) return;
        anim = GetComponent<Animator>();
        if (!anim || !anim.isHuman) { enabled = false; return; }
        anim.applyRootMotion = false;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        hips = anim.GetBoneTransform(HumanBodyBones.Hips);
        headBone = anim.GetBoneTransform(HumanBodyBones.Head);
        neck = anim.GetBoneTransform(HumanBodyBones.Neck);
        headBind = Quaternion.Inverse(transform.rotation) * headBone.rotation;
        left = MakeArm(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, true);
        right = MakeArm(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, false);
        legL = MakeLeg(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot);
        legR = MakeLeg(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot);
        var sp = new List<Transform>();
        foreach (var b in new[] { HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest }) { var t = anim.GetBoneTransform(b); if (t) sp.Add(t); }
        spineBones = sp.ToArray();
        shoulderR = right.upper;
        aimR = new GameObject("Visee_MainD").transform; aimR.SetParent(transform, false);
        aimL = new GameObject("Visee_MainG").transform; aimL.SetParent(transform, false);
        yaw = transform.eulerAngles.y;
        baseScale = transform.localScale.x;
        modelEye = (headBone.position.y - transform.position.y) / Mathf.Max(0.01f, transform.lossyScale.y) + eyeOffset.y;
    }

    Arm MakeArm(HumanBodyBones u, HumanBodyBones l, HumanBodyBones h, bool isLeft)
    {
        var a = new Arm { upper = anim.GetBoneTransform(u), lower = anim.GetBoneTransform(l), hand = anim.GetBoneTransform(h) };
        // doigts : index, majeur, annulaire, auriculaire (3 phalanges chacun) ; le pouce reste tel quel
        var f = new List<Transform[]>();
        var first = isLeft ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal;
        for (int i = 0; i < 4; i++)
        {
            var chain = new List<Transform>();
            for (int j = 0; j < 3; j++) { var b = anim.GetBoneTransform(first + i * 3 + j); if (b) chain.Add(b); }
            if (chain.Count > 0) f.Add(chain.ToArray());
        }
        a.fingers = f.ToArray();
        // repère de la main au repos : « avant » = vers les doigts, « haut » = dos de la main
        Vector3 fingersDir, back;
        HandFrame(a, out fingersDir, out back);
        a.bindFrameInv = Quaternion.Inverse(Quaternion.LookRotation(fingersDir, back)) * a.hand.rotation;
        return a;
    }

    // direction des doigts et dos de la main, mesurés sur les os (ne dépend pas de la pose de repos du modèle, en T ou en A)
    static void HandFrame(Arm a, out Vector3 fingersDir, out Vector3 back)
    {
        var h = a.hand.position;
        Transform index = a.fingers.Length > 0 ? a.fingers[0][0] : null, pinky = a.fingers.Length > 3 ? a.fingers[3][0] : null,
                  middle = a.fingers.Length > 1 ? a.fingers[1][0] : index;
        fingersDir = middle ? (middle.position - h).normalized : (h - a.lower.position).normalized;
        back = Vector3.up;
        if (index && pinky)
        {
            var n = Vector3.Cross(index.position - h, pinky.position - h).normalized;
            back = n.y >= 0f ? n : -n;          // au repos (bras en T ou en A), le dos de la main regarde vers le haut
        }
        back = Vector3.ProjectOnPlane(back, fingersDir).normalized;
    }

    Leg MakeLeg(HumanBodyBones u, HumanBodyBones l, HumanBodyBones f)
    {
        var g = new Leg { upper = anim.GetBoneTransform(u), lower = anim.GetBoneTransform(l), foot = anim.GetBoneTransform(f) };
        g.ankle = (g.foot.position.y - transform.position.y) / Mathf.Max(0.01f, transform.lossyScale.y);     // hauteur de la cheville au repos (échelle 1)
        return g;
    }

    void LateUpdate() => Solve(Time.deltaTime);

    /// <summary>Pose le corps sur les cibles, après l'Animator (public pour l'aperçu de l'éditeur).</summary>
    public void Solve(float deltaTime)
    {
        if (!headTarget || left == null) return;    // aperçu du menu : animation seule
        float dt = Mathf.Max(deltaTime, 1e-4f);

        // vitesse à plat de la tête → animation des jambes
        var hp = headTarget.position;
        if (hasLast)
        {
            var d = hp - lastHead; d.y = 0f;
            float v = d.magnitude / dt;
            if (v > 12f) v = 0f;                     // téléportation
            speed = Mathf.Lerp(speed, v, 1f - Mathf.Exp(-dt * 8f));
        }
        lastHead = hp; hasLast = true;
        anim.SetFloat(SpeedId, speed < 0.15f ? 0f : speed);

        // taille du personnage = taille debout du joueur (la plus grande hauteur de tête vue ; redescend très lentement)
        if (!float.IsNaN(floorY))
        {
            float h = hp.y - floorY;
            standing = standing <= 0f ? h : Mathf.Max(h, standing - dt * 0.01f);
            float k = Mathf.Clamp(standing / modelEye, 0.85f, 1.12f);
            transform.localScale = Vector3.one * (baseScale * k);
        }

        // le corps se tourne vers le regard (lentement à l'arrêt, plus vite en marche) ; arme en main (3e personne) : il suit la visée
        bool gun = gunProp;
        var f = headTarget.forward; f.y = 0f;
        if (f.sqrMagnitude > 0.01f)
        {
            float target = Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
            float delta = Mathf.DeltaAngle(yaw, target);
            if (gun) yaw = Mathf.LerpAngle(yaw, target, 1f - Mathf.Exp(-dt * aimTurnSpeed));
            else if (Mathf.Abs(delta) > 25f || speed > 0.3f) yaw = Mathf.LerpAngle(yaw, target, 1f - Mathf.Exp(-dt * turnSpeed));
        }
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // arme en main : le buste se penche avec le regard (haut / bas), réparti sur les os de la colonne
        if (gun && spineBones.Length > 0)
        {
            float pitch = -Mathf.Asin(Mathf.Clamp(headTarget.forward.y, -1f, 1f)) * Mathf.Rad2Deg;     // + : regard vers le bas
            float each = Mathf.Clamp(pitch, -70f, 70f) * spineShare / spineBones.Length;
            foreach (var b in spineBones) b.rotation = Quaternion.AngleAxis(each, transform.right) * b.rotation;
        }

        // le corps se place pour que l'os de la tête soit là où est la vraie tête
        var wantHeadBone = hp - headTarget.TransformVector(eyeOffset);
        transform.position += wantHeadBone - headBone.position;
        headBone.rotation = headTarget.rotation * headBind;

        if (!float.IsNaN(floorY))
        {
            KeepFootOnFloor(legL);
            KeepFootOnFloor(legR);
        }
        if (gun) { PoseGun(); SolveArm(left, aimL, true, true, dt); SolveArm(right, aimR, false, true, dt); return; }
        SolveArm(left, leftTarget, true, leftHolding, dt);
        SolveArm(right, rightTarget, false, rightHolding, dt);
    }

    /// <summary>
    /// Arme à feu tenue à la 3e personne (joueur en version PC) : <paramref name="prop"/> est la copie visuelle de l'arme,
    /// <paramref name="gripLocal"/> l'accroche de la main (poignée) et <paramref name="muzzleLocal"/> l'orientation du canon, dans le repère de l'arme.
    /// null : on revient aux mains suivies. <paramref name="side"/> : objet de la main gauche (chargeur pendant le rechargement), posé sur la main gauche.
    /// </summary>
    public void HoldGun(Transform prop, Vector3 gripLocal, Quaternion muzzleLocal, Transform side = null)
    {
        gunProp = prop; sideProp = prop ? side : null;
        if (!prop) return;
        gunGrip = gripLocal; gunMuzzle = muzzleLocal;
        // longueur poignée → bouche du canon (le fusil se tient au garde-main, le pistolet à deux mains sur la poignée)
        var muzzle = Vector3.zero; float best = 0f;
        foreach (var mf in prop.GetComponentsInChildren<MeshFilter>())
        {
            if (!mf.sharedMesh) continue;
            var b = mf.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                var c = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
                var local = Quaternion.Inverse(muzzleLocal) * (prop.InverseTransformPoint(mf.transform.TransformPoint(c)) - gripLocal);
                if (local.z > best) best = local.z;
            }
        }
        gunBarrel = best * prop.lossyScale.z;
    }

    // l'arme devant l'épaule droite, canon dans l'axe du regard ; repères des deux mains sur la poignée et le garde-main
    void PoseGun()
    {
        float s = transform.lossyScale.y;
        var aim = Quaternion.LookRotation(headTarget.forward, transform.up);
        bool rifle = gunBarrel > 0.4f;
        var grip = shoulderR.position + aim * ((rifle ? rifleGrip : pistolGrip) * s);
        // rotation de l'arme : son canon (muzzle.forward) suit la visée ; sa poignée sur `grip`
        var rot = aim * Quaternion.Inverse(gunMuzzle);
        gunProp.rotation = rot;
        gunProp.position += grip - gunProp.TransformPoint(gunGrip);

        // main droite : « manette » sur la poignée, pointée comme le canon (comme une manette VR tenue en pistolet)
        aimR.SetPositionAndRotation(grip + aim * new Vector3(0f, 0.01f, 0.03f), aim * Quaternion.Euler(rightGripRot));
        // main gauche : fusil → sur le garde-main ; pistolet → enveloppe la main droite par la gauche
        if (rifle)
        {
            var o = leftRifleOffset; o.z = Mathf.Min(o.z, gunBarrel * 0.6f);
            aimL.SetPositionAndRotation(grip + aim * o, aim * Quaternion.Euler(leftRifleRot));
        }
        else aimL.SetPositionAndRotation(grip + aim * leftPistolOffset, aim * Quaternion.Euler(leftPistolRot));
        if (sideProp) sideProp.SetPositionAndRotation(aimL.position - aim * Vector3.up * 0.04f, aim);
    }

    void SolveArm(Arm a, Transform target, bool isLeft, bool holding, float dt)
    {
        if (a == null || !target) return;
        var wrist = target.position - target.forward * wristBack;
        var hint = -transform.up + transform.right * (isLeft ? -0.6f : 0.6f) - transform.forward * 0.35f;
        TwoBone(a.upper, a.lower, a.hand, wrist, hint);
        // manette : avant = direction pointée, la paume regarde vers l'intérieur → dos de la main vers l'extérieur
        a.hand.rotation = Quaternion.LookRotation(target.forward, isLeft ? -target.right : target.right) * a.bindFrameInv;

        // doigts : légèrement repliés au repos, refermés sur l'objet tenu
        a.curl = Mathf.Lerp(a.curl, holding ? curlHolding : curlRelaxed, 1f - Mathf.Exp(-dt * 12f));
        var palm = isLeft ? target.right : -target.right;
        foreach (var chain in a.fingers)
            foreach (var b in chain)
            {
                var child = b.childCount > 0 ? b.GetChild(0) : null;
                var dir = child ? (child.position - b.position).normalized : target.forward;
                var axis = Vector3.Cross(dir, palm);
                if (axis.sqrMagnitude > 1e-5f) b.rotation = Quaternion.AngleAxis(a.curl, axis.normalized) * b.rotation;
            }
    }

    void KeepFootOnFloor(Leg g)
    {
        float min = floorY + g.ankle * transform.lossyScale.y;
        var p = g.foot.position;
        if (p.y >= min - 0.005f) return;
        var footRot = g.foot.rotation;
        TwoBone(g.upper, g.lower, g.foot, new Vector3(p.x, min, p.z), transform.forward + transform.up * 0.2f);
        g.foot.rotation = footRot;
    }

    // Cinématique inverse à deux os : place l'extrémité `end` sur `goal` en pliant l'articulation du milieu vers `hint`.
    static void TwoBone(Transform root, Transform mid, Transform end, Vector3 goal, Vector3 hint)
    {
        float l1 = Vector3.Distance(root.position, mid.position), l2 = Vector3.Distance(mid.position, end.position);
        var toGoal = goal - root.position;
        float d = Mathf.Clamp(toGoal.magnitude, Mathf.Abs(l1 - l2) + 0.01f, l1 + l2 - 0.005f);
        var dir = toGoal.sqrMagnitude > 1e-8f ? toGoal.normalized : Vector3.down;
        // position de l'articulation : sur le cercle d'intersection, du côté du repère `hint`
        float a = (l1 * l1 - l2 * l2 + d * d) / (2f * d);
        float h = Mathf.Sqrt(Mathf.Max(0f, l1 * l1 - a * a));
        var side = Vector3.ProjectOnPlane(hint, dir);
        if (side.sqrMagnitude < 1e-6f) side = Vector3.ProjectOnPlane(Vector3.down, dir);
        var midGoal = root.position + dir * a + side.normalized * h;

        root.rotation = Quaternion.FromToRotation(mid.position - root.position, midGoal - root.position) * root.rotation;
        mid.rotation = Quaternion.FromToRotation(end.position - mid.position, root.position + dir * d - mid.position) * mid.rotation;
    }

    // ---------- outils pour NetPlayer ----------
    public Transform Bone(HumanBodyBones b) => anim ? anim.GetBoneTransform(b) : null;

    /// <summary>Grise le personnage (joueur à terre) ou lui rend ses couleurs.</summary>
    public void SetDowned(bool down)
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            var block = new MaterialPropertyBlock();
            if (down) block.SetColor("_BaseColor", new Color(0.32f, 0.32f, 0.34f));
            r.SetPropertyBlock(down ? block : null);
        }
    }
}
