using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// [Multijoueur] Un joueur dans la partie (prefab Resources/Net/NetPlayer, créé par l'hôte pour chaque casque, NetGame).
//   - Sur son propre casque (propriétaire) : il recopie 20 fois par seconde la tête et les mains du XR Origin local, et sa vie.
//     Il n'affiche rien (on ne voit pas son propre avatar) et décale son XR Origin pour que les joueurs n'apparaissent pas l'un dans l'autre.
//   - Chez les autres : son personnage (tenue choisie dans le lobby, PlayerSkin), animé par PlayerAvatar à partir de la tête et des mains,
//     avec l'objet qu'il tient dans chaque main (copie visuelle) et une étiquette « J2 » de sa couleur ; gris s'il est à terre.
//     Si les prefabs de personnage manquent : avatar simple (tête, visière, deux mains).
//   - Chez l'hôte, pour un joueur distant : un « double » PlayerHealth sur la tête de l'avatar, que les zombies visent ;
//     un coup reçu par ce double est envoyé au casque du joueur (HitRpc), qui l'applique à sa vraie vie.
public class NetPlayer : NetworkBehaviour
{
    public static readonly List<NetPlayer> All = new List<NetPlayer>();
    public static NetPlayer Local { get; private set; }

    public static readonly Color[] Colors =
    {
        new Color(1f, 0.75f, 0.27f),    // J1 ambre
        new Color(0.3f, 0.75f, 1f),     // J2 bleu
        new Color(0.45f, 1f, 0.5f),     // J3 vert
        new Color(1f, 0.4f, 0.7f),      // J4 rose
    };

    public struct Pose : INetworkSerializeByMemcpy
    {
        public Vector3 head, left, right;
        public Quaternion headRot, leftRot, rightRot;
        public float floorY;                        // sol sous le joueur (y du XR Origin) : les pieds du personnage ne passent pas dessous
        // objet tenu dans chaque main : nom de l'objet (haché, 0 = rien) et sa pose dans le repère de la main
        public int leftItem, rightItem;
        public Vector3 leftItemPos, rightItemPos;
        public Quaternion leftItemRot, rightItemRot;
        public bool desktop;                        // joueur en version PC : ses mains sont placées « vue FPS », l'avatar tient l'arme à la 3e personne
    }

    readonly NetworkVariable<Pose> pose = new NetworkVariable<Pose>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public readonly NetworkVariable<float> Health = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public readonly NetworkVariable<int> Number = new NetworkVariable<int>(1);
    /// <summary>Tenue choisie par le joueur (PlayerSkin), envoyée par son casque.</summary>
    public readonly NetworkVariable<int> Skin = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public bool Dead => Health.Value <= 0f;

    // propriétaire : sources (rig local)
    Transform srcHead, srcLeft, srcRight;
    UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor itLeft, itRight;
    float nextSend;
    // autres : avatar
    Transform head, left, right;
    Renderer[] tinted;
    TextMesh label;
    bool shownDead;
    PlayerHealth proxy;
    PlayerAvatar body;
    readonly HeldProp propLeft = new HeldProp(), propRight = new HeldProp();
    class HeldProp { public int id; public GameObject go; public bool gun; public Vector3 grip; public Quaternion muzzle; }

    public override void OnNetworkSpawn()
    {
        All.Add(this);
        name = "Joueur_" + OwnerClientId;
        if (IsOwner)
        {
            Local = this;
            srcHead = Camera.main ? Camera.main.transform : null;
            var l = GameObject.Find("Left Controller"); var r = GameObject.Find("Right Controller");
            srcLeft = l ? l.transform : null; srcRight = r ? r.transform : null;
            itLeft = l ? l.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>(true) : null;
            itRight = r ? r.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>(true) : null;
            Skin.Value = PlayerSkin.Selected;
            var h = PlayerHealth.Instance;
            if (h)
            {
                Health.Value = h.Health;
                h.Changed += OnLocalHealth;
                h.Died += OnLocalDied;
            }
            SpreadRig();
            return;
        }
        BuildAvatar();
        Number.OnValueChanged += (_, n) => Tint();
        Skin.OnValueChanged += (_, n) => BuildBody();
        if (IsServer)
        {
            proxy = PlayerHealth.CreateProxy(head.gameObject, head);
            proxy.RemoteDamaged += (amount, from) => HitRpc(amount, from);
        }
    }

    public override void OnNetworkDespawn()
    {
        All.Remove(this);
        if (Local == this) Local = null;
        var h = PlayerHealth.Instance;
        if (IsOwner && h) { h.Changed -= OnLocalHealth; h.Died -= OnLocalDied; }
    }

    void OnLocalHealth(float hp, float max) => Health.Value = hp;

    void OnLocalDied()
    {
        // à terre : les autres continuent ; relevé à la pause suivante (NetGame)
        if (WaveManager.Instance) WaveManager.Instance.RemoteAnnounce("VOUS ÊTES À TERRE\nVos coéquipiers tiennent encore.\nRelevé à la prochaine pause.", 7f);
    }

    // chaque joueur arrive à sa place dans le bunker ou l'arène 1v1
    void SpreadRig()
    {
        var h = PlayerHealth.Instance;
        if (!h) return;
        int n = NetLobby.Numbers.TryGetValue(OwnerClientId, out var k) ? k : 1;
        var rig = h.transform;
        var cc = rig.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (ArenaMaps.IsArena(sceneName))
        {
            // En 1v1 : J1 et J2 aux 2 apparitions de la carte jouée (ArenaMaps), face au centre
            ArenaMaps.PlaceRig(rig, n);
        }
        else if (n > 1)
        {
            rig.position += new Vector3(((n % 2 == 0) ? 1f : -1f) * 0.5f * ((n + 1) / 2), 0f, 0f);
        }
        if (cc) cc.enabled = true;
    }

    /// <summary>Appelé par PlayerHitbox quand une balle touche l'avatar distant.</summary>
    public void TakeShotDamage(float amount, Vector3 from)
    {
        HitRpc(amount, from);
    }

    // le zombie ou une balle a frappé ce joueur : on applique le coup à la vraie vie, sur son casque
    [Rpc(SendTo.Owner)]
    void HitRpc(float amount, Vector3 from)
    {
        if (PlayerHealth.Instance) PlayerHealth.Instance.TakeDamage(amount, from);
    }

    void Update()
    {
        if (!IsSpawned) return;
        if (IsOwner)
        {
            if (Time.unscaledTime < nextSend) return;
            nextSend = Time.unscaledTime + 0.05f;
            if (!srcHead && Camera.main) srcHead = Camera.main.transform;
            var p = new Pose();
            if (srcHead) { p.head = srcHead.position; p.headRot = srcHead.rotation; }
            if (srcLeft) { p.left = srcLeft.position; p.leftRot = srcLeft.rotation; } else { p.left = p.head + Vector3.down * 0.5f; p.leftRot = p.headRot; }
            if (srcRight) { p.right = srcRight.position; p.rightRot = srcRight.rotation; } else { p.right = p.head + Vector3.down * 0.5f; p.rightRot = p.headRot; }
            p.floorY = PlayerHealth.Instance ? PlayerHealth.Instance.transform.position.y : p.head.y - 1.65f;
            Held(itLeft, srcLeft, out p.leftItem, out p.leftItemPos, out p.leftItemRot);
            Held(itRight, srcRight, out p.rightItem, out p.rightItemPos, out p.rightItemRot);
            p.desktop = DesktopMode.On;
            pose.Value = p;
            return;
        }

        var v = pose.Value;
        float k = 1f - Mathf.Exp(-Time.deltaTime * 15f);
        Follow(head, v.head, v.headRot, k);
        Follow(left, v.left, v.leftRot, k);
        Follow(right, v.right, v.rightRot, k);
        if (body)
        {
            body.floorY = v.floorY;
            body.leftHolding = v.leftItem != 0; body.rightHolding = v.rightItem != 0;
        }
        ShowProp(propLeft, left, v.leftItem, v.leftItemPos, v.leftItemRot);
        ShowProp(propRight, right, v.rightItem, v.rightItemPos, v.rightItemRot);
        // version PC, arme à feu dans la main droite : l'avatar la tient à la 3e personne (PlayerAvatar.HoldGun)
        if (body)
        {
            bool tps = v.desktop && propRight.go && propRight.gun;
            body.HoldGun(tps ? propRight.go.transform : null, propRight.grip, propRight.muzzle, tps && propLeft.go ? propLeft.go.transform : null);
        }
        if (proxy) proxy.SetProxyHealth(Health.Value);
        if (shownDead != Dead) { shownDead = Dead; Tint(); }
        if (label && Camera.main) label.transform.rotation = Quaternion.LookRotation(label.transform.position - Camera.main.transform.position);
    }

    static void Follow(Transform t, Vector3 p, Quaternion r, float k)
    {
        if (!t || r == default) return;
        if ((t.position - p).sqrMagnitude > 4f) t.SetPositionAndRotation(p, r);
        else t.SetPositionAndRotation(Vector3.Lerp(t.position, p, k), Quaternion.Slerp(t.rotation, r, k));
    }

    // ---------- objet tenu ----------
    // Les objets (pistolet, marteau, planches, chargeurs…) ne sont pas partagés : chaque casque a les siens. Pour que les autres
    // voient ce qu'un joueur tient, son casque envoie le NOM de l'objet et sa pose dans la main ; ici on affiche une copie
    // visuelle (maillages seulement) de l'objet du même nom trouvé dans notre propre scène.
    static void Held(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor it, Transform hand, out int id, out Vector3 pos, out Quaternion rot)
    {
        id = 0; pos = Vector3.zero; rot = Quaternion.identity;
        if (!it || !hand || !it.hasSelection) return;
        var t = it.firstInteractableSelected != null ? it.firstInteractableSelected.transform : null;
        if (!t) return;
        id = ItemId(t.name);
        pos = hand.InverseTransformPoint(t.position);
        rot = Quaternion.Inverse(hand.rotation) * t.rotation;
    }

    static string BaseName(string n) { int i = n.IndexOf(" ("); if (i < 0) i = n.IndexOf("(Clone)"); return i > 0 ? n.Substring(0, i) : n; }
    static int ItemId(string name) { int h = Animator.StringToHash(BaseName(name)); return h == 0 ? 1 : h; }

    static Dictionary<int, GameObject> templates;
    static string templatesScene;

    static GameObject Template(int id)
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (templates == null || templatesScene != scene)
        {
            templates = new Dictionary<int, GameObject>(); templatesScene = scene;
            foreach (var g in FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                int k = ItemId(g.name);
                if (!templates.ContainsKey(k)) templates[k] = g.gameObject;
            }
        }
        return templates.TryGetValue(id, out var t) ? t : null;
    }

    void ShowProp(HeldProp prop, Transform hand, int id, Vector3 pos, Quaternion rot)
    {
        if (prop.id != id)
        {
            prop.id = id;
            if (prop.go) Destroy(prop.go);
            var src = id != 0 ? Template(id) : null;
            // leviers, poignées de porte… : rien à copier dans la main (pas de corps physique : ils restent à leur place)
            if (src && src.GetComponent<Rigidbody>()) prop.go = VisualCopy(src, hand);
            // arme à feu : poignée (accroche de la main) et orientation du canon, dans le repère de l'arme
            var pistol = src ? src.GetComponent<Pistol>() : null;
            var grab = src ? src.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() : null;
            prop.gun = prop.go && pistol;
            prop.grip = grab && grab.attachTransform ? src.transform.InverseTransformPoint(grab.attachTransform.position) : Vector3.zero;
            prop.muzzle = pistol && pistol.muzzle ? Quaternion.Inverse(src.transform.rotation) * pistol.muzzle.rotation : Quaternion.identity;
        }
        if (prop.go) prop.go.transform.SetLocalPositionAndRotation(pos, rot);
    }

    static GameObject VisualCopy(GameObject src, Transform parent)
    {
        var root = new GameObject("Tenu_" + src.name);
        root.transform.SetParent(parent, false);
        root.transform.localScale = src.transform.lossyScale;
        foreach (var mf in src.GetComponentsInChildren<MeshFilter>(false))
        {
            var mr = mf.GetComponent<MeshRenderer>();
            if (!mr || !mr.enabled || !mf.sharedMesh) continue;
            var g = new GameObject(mf.name);
            g.transform.SetParent(root.transform, false);
            g.transform.localPosition = src.transform.InverseTransformPoint(mf.transform.position);
            g.transform.localRotation = Quaternion.Inverse(src.transform.rotation) * mf.transform.rotation;
            var ls = src.transform.lossyScale; var ms = mf.transform.lossyScale;
            g.transform.localScale = new Vector3(ms.x / ls.x, ms.y / ls.y, ms.z / ls.z);
            g.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterials = mr.sharedMaterials;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        return root;
    }

    // ---------- avatar ----------
    // head / left / right : trois repères qui suivent la tête et les mains du joueur. Le personnage (PlayerAvatar) s'y accroche ;
    // tête et torse portent un PlayerHitbox pour les tirs entre joueurs (duel).
    void BuildAvatar()
    {
        head = new GameObject("Tete").transform; head.SetParent(transform, false);
        left = new GameObject("Main_G").transform; left.SetParent(transform, false);
        right = new GameObject("Main_D").transform; right.SetParent(transform, false);

        var lg = new GameObject("Etiquette");
        lg.transform.SetParent(head, false);
        lg.transform.localPosition = new Vector3(0f, 0.34f, 0f);
        label = lg.AddComponent<TextMesh>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lg.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
        label.fontSize = 48; label.characterSize = 0.01f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
        BuildBody();
    }

    void BuildBody()
    {
        if (body) Destroy(body.gameObject);
        foreach (var r in simple) if (r) Destroy(r);
        simple.Clear(); tinted = null;

        body = PlayerSkin.Create(Skin.Value, transform);
        if (body)
        {
            body.headTarget = head; body.leftTarget = left; body.rightTarget = right;
            Hitbox(body.Bone(HumanBodyBones.Head), HitZone.Head, new Vector3(0f, 0.06f, 0.02f), 0.13f, 0f);
            Hitbox(body.Bone(HumanBodyBones.Chest) ?? body.Bone(HumanBodyBones.Spine), HitZone.Torso, Vector3.zero, 0.19f, 0.62f);
            LegBox(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg); LegBox(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot);
            LegBox(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg); LegBox(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot);
            body.SetDowned(Dead);
        }
        else BuildSimple();
        Tint();
    }

    // jambe : capsule entre un os et le suivant (le sniper fait moins de dégâts aux jambes)
    void LegBox(HumanBodyBones from, HumanBodyBones to)
    {
        var a = body.Bone(from); var b = body.Bone(to);
        if (!a || !b) return;
        var g = new GameObject("Zone_" + HitZone.Legs);
        g.transform.SetParent(a, false);
        g.transform.position = (a.position + b.position) * 0.5f;
        g.transform.rotation = Quaternion.FromToRotation(Vector3.up, b.position - a.position);
        var c = g.AddComponent<CapsuleCollider>();
        c.radius = 0.09f; c.height = (b.position - a.position).magnitude + 0.08f; c.direction = 1;
        var hb = g.AddComponent<PlayerHitbox>();
        hb.player = this; hb.zone = HitZone.Legs;
    }

    // zone touchable posée sur un os du personnage (sphère pour la tête, capsule verticale pour le torse)
    void Hitbox(Transform bone, HitZone zone, Vector3 offset, float radius, float height)
    {
        if (!bone) return;
        var g = new GameObject("Zone_" + zone);
        g.transform.SetParent(bone, false);
        g.transform.position = bone.position + body.transform.TransformVector(offset);
        g.transform.rotation = body.transform.rotation;
        if (height > 0f) { var c = g.AddComponent<CapsuleCollider>(); c.radius = radius; c.height = height; c.direction = 1; }
        else g.AddComponent<SphereCollider>().radius = radius;
        var hb = g.AddComponent<PlayerHitbox>();
        hb.player = this; hb.zone = zone;
    }

    // secours sans prefab de personnage : tête, visière, torse, deux mains
    readonly List<GameObject> simple = new List<GameObject>();
    void BuildSimple()
    {
        var parts = new List<Renderer>();
        var h = Part("Tete_Simple", PrimitiveType.Sphere, new Vector3(0.2f, 0.24f, 0.22f), parts, true, HitZone.Head);
        h.transform.SetParent(head, false);
        var visor = Part("Visiere", PrimitiveType.Cube, new Vector3(0.17f, 0.06f, 0.05f), null, false);
        visor.transform.SetParent(head, false);
        visor.transform.localPosition = new Vector3(0f, 0.02f, 0.105f);
        visor.GetComponent<Renderer>().material.color = new Color(0.05f, 0.05f, 0.06f);
        var torso = Part("Torse", PrimitiveType.Capsule, new Vector3(0.32f, 0.3f, 0.2f), parts, true, HitZone.Torso);
        torso.transform.SetParent(head, false);
        torso.transform.localPosition = new Vector3(0f, -0.48f, -0.05f);
        var l = Part("Main_G_Simple", PrimitiveType.Cube, new Vector3(0.07f, 0.09f, 0.14f), parts, false); l.transform.SetParent(left, false);
        var r = Part("Main_D_Simple", PrimitiveType.Cube, new Vector3(0.07f, 0.09f, 0.14f), parts, false); r.transform.SetParent(right, false);
        simple.AddRange(new[] { h, visor, torso, l, r });
        tinted = parts.ToArray();
    }

    GameObject Part(string n, PrimitiveType t, Vector3 scale, List<Renderer> list, bool hitbox = false, HitZone zone = HitZone.Torso)
    {
        var g = GameObject.CreatePrimitive(t);
        g.name = n;
        if (!hitbox)
        {
            Destroy(g.GetComponent<Collider>());
        }
        else
        {
            var hb = g.AddComponent<PlayerHitbox>();
            hb.player = this;
            hb.zone = zone;
        }
        g.transform.SetParent(transform, false);
        g.transform.localScale = scale;
        var r = g.GetComponent<Renderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        list?.Add(r);
        return g;
    }

    void Tint()
    {
        int n = Mathf.Clamp(Number.Value, 1, Colors.Length);
        var c = Dead ? new Color(0.35f, 0.35f, 0.35f) : Colors[n - 1];
        if (body) body.SetDowned(Dead);
        if (tinted != null) foreach (var r in tinted) r.material.color = c;
        if (label) { label.text = "J" + n + (Dead ? "  À TERRE" : ""); label.color = c; }
    }
}
