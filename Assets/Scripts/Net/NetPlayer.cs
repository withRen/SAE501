using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// [Multijoueur] Un joueur dans la partie (prefab Resources/Net/NetPlayer, créé par l'hôte pour chaque casque, NetGame).
//   - Sur son propre casque (propriétaire) : il recopie 20 fois par seconde la tête et les mains du XR Origin local, et sa vie.
//     Il n'affiche rien (on ne voit pas son propre avatar) et décale son XR Origin pour que les joueurs n'apparaissent pas l'un dans l'autre.
//   - Chez les autres : un avatar simple (tête, visière, deux mains, étiquette « J2 ») de la couleur du joueur, gris s'il est à terre.
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
    }

    readonly NetworkVariable<Pose> pose = new NetworkVariable<Pose>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public readonly NetworkVariable<float> Health = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public readonly NetworkVariable<int> Number = new NetworkVariable<int>(1);

    public bool Dead => Health.Value <= 0f;

    // propriétaire : sources (rig local)
    Transform srcHead, srcLeft, srcRight;
    float nextSend;
    // autres : avatar
    Transform head, left, right;
    Renderer[] tinted;
    TextMesh label;
    bool shownDead;
    PlayerHealth proxy;

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

    // chaque joueur arrive à sa place dans le bunker (50 cm d'écart par numéro) au lieu d'apparaître dans l'autre
    void SpreadRig()
    {
        var h = PlayerHealth.Instance;
        int n = NetLobby.Numbers.TryGetValue(OwnerClientId, out var k) ? k : 1;
        if (!h || n <= 1) return;
        var rig = h.transform;
        var cc = rig.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        rig.position += new Vector3(((n % 2 == 0) ? 1f : -1f) * 0.5f * ((n + 1) / 2), 0f, 0f);
        if (cc) cc.enabled = true;
    }

    // le zombie (chez l'hôte) a frappé le double de ce joueur : on applique le coup à la vraie vie, sur son casque
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
            pose.Value = p;
            return;
        }

        var v = pose.Value;
        float k = 1f - Mathf.Exp(-Time.deltaTime * 15f);
        Follow(head, v.head, v.headRot, k);
        Follow(left, v.left, v.leftRot, k);
        Follow(right, v.right, v.rightRot, k);
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

    // ---------- avatar (primitives sans collider : ni les balles ni les zombies ne s'y arrêtent) ----------
    void BuildAvatar()
    {
        var parts = new List<Renderer>();
        head = Part("Tete", PrimitiveType.Sphere, new Vector3(0.2f, 0.24f, 0.22f), parts).transform;
        var visor = Part("Visiere", PrimitiveType.Cube, new Vector3(0.17f, 0.06f, 0.05f), null);
        visor.transform.SetParent(head, false);
        visor.transform.localPosition = new Vector3(0f, 0.02f, 0.105f);
        visor.GetComponent<Renderer>().material.color = new Color(0.05f, 0.05f, 0.06f);
        var body = Part("Torse", PrimitiveType.Capsule, new Vector3(0.32f, 0.3f, 0.2f), parts);
        body.transform.SetParent(head, false);
        body.transform.localPosition = new Vector3(0f, -0.48f, -0.05f);
        left = Part("Main_G", PrimitiveType.Cube, new Vector3(0.07f, 0.09f, 0.14f), parts).transform;
        right = Part("Main_D", PrimitiveType.Cube, new Vector3(0.07f, 0.09f, 0.14f), parts).transform;
        tinted = parts.ToArray();

        var lg = new GameObject("Etiquette");
        lg.transform.SetParent(head, false);
        lg.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        label = lg.AddComponent<TextMesh>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lg.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
        label.fontSize = 48; label.characterSize = 0.01f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
        Tint();
    }

    GameObject Part(string n, PrimitiveType t, Vector3 scale, List<Renderer> list)
    {
        var g = GameObject.CreatePrimitive(t);
        g.name = n;
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(transform, false);
        g.transform.localScale = scale;
        var r = g.GetComponent<Renderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        list?.Add(r);
        return g;
    }

    void Tint()
    {
        if (tinted == null) return;
        int n = Mathf.Clamp(Number.Value, 1, Colors.Length);
        var c = Dead ? new Color(0.35f, 0.35f, 0.35f) : Colors[n - 1];
        foreach (var r in tinted) r.material.color = c;
        if (label) { label.text = "J" + n + (Dead ? "  À TERRE" : ""); label.color = c; }
    }
}
