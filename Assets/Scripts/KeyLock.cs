using System;
using UnityEngine;

// [Clé du générateur] Serrure d'une porte sur charnière (HingedDoor) : la porte de la salle du générateur.
// En plus du verrou de niveau (ZoneGate : salle ouverte après la vague 3), la porte reste fermée tant qu'on n'a pas la clé :
//   - déverrouiller : tenir la clé (KeyItem) dans une main et saisir la poignée avec l'autre (VR), ou approcher la clé tenue
//     de la poignée (à moins de unlockDistance) ; sur PC, viser la porte avec la clé en main et appuyer sur E ;
//   - sans la clé : la poignée se lâche, petit bruit de serrure, message « Verrouillée — il faut la clé de la salle du
//     générateur » sur le panneau de la porte (texte dans le monde, lisible au casque) et en bas de l'écran sur PC ;
//   - une fois déverrouillée, elle le reste (la clé n'est plus nécessaire, la porte s'ouvre ensuite normalement).
// [Indication au casque] une serrure (plaque et trou) sous la poignée, côté joueur, avec une faible lueur fixe ; quand le joueur tient la clé à moins de
//   hintDistance de la porte encore verrouillée : la serrure luit en ambre et pulse (plus fort en approchant), le panneau dit
//   « CLÉ EN MAIN — Approchez-la de la serrure » et l'étiquette de la clé ajoute « → approchez-la de la serrure ».
// Multijoueur coop : le déverrouillage passe par NetGame (LocalUnlocked → l'hôte → NetworkVariable GeneratorDoorUnlocked) : la
// porte est déverrouillée sur tous les casques ; le message « verrouillée » reste propre au joueur qui essaie.
public class KeyLock : MonoBehaviour
{
    public HingedDoor door;
    public ZoneGate gate;                       // panneau de la porte (gate.sign) et verrou de niveau
    public KeyItem key;                         // la clé de CE casque (chaque casque a sa propre scène)
    public string lockedText = "VERROUILLÉE\nIl faut la clé de la salle\ndu générateur";
    public string keyHeldText = "CLÉ EN MAIN\nApprochez-la de la serrure\n(sous la poignée)";
    public string keyHeldTextPC = "CLÉ EN MAIN\nVisez la porte\net appuyez sur E";
    public float unlockDistance = 0.25f;        // clé → serrure ou poignée
    public float hintDistance = 4f;             // clé tenue à cette distance de la porte : indication visible
    public float idleGlow = 0.6f;               // lueur fixe de la serrure sans la clé (salle ouverte, porte verrouillée)
    public float messageSeconds = 3f;

    public static KeyLock Instance { get; private set; }
    /// <summary>Ce casque vient de déverrouiller la porte (NetGame le relaie à l'hôte).</summary>
    public static event Action LocalUnlocked;

    public bool Unlocked { get; private set; }

    AudioSource src;
    AudioClip rattle, unlockClip;
    float messageUntil = -1f;
    string signDefault;
    Color signColor;
    Transform keyhole;                          // centre de la serrure (enfant de la charnière : tourne avec la porte)
    Renderer glow;
    MaterialPropertyBlock mpb;
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    static readonly Color Amber = new Color(1f, 0.7f, 0.25f, 1f);

    void Awake()
    {
        Instance = this;
        if (door) door.keyLock = this;
        if (!key) { Unlocked = true; Debug.LogWarning("[KeyLock] pas de clé dans la scène : porte sans serrure"); }   // jamais de porte impossible à ouvrir
        src = (door ? door.gameObject : gameObject).AddComponent<AudioSource>();     // le son vient de la porte
        src.spatialBlend = 1f;
        src.playOnAwake = false;
        rattle = ProcAudio.Clack();
        unlockClip = Resources.Load<AudioClip>("Audio/Porte_Deverrouillee");
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        if (gate && gate.sign) { signDefault = gate.sign.text; signColor = gate.sign.color; }
        if (door && door.handle && door.leaf && key) BuildKeyhole();
    }

    // serrure faite au lancement (pas d'objet dans la scène) : plaque de la matière de la poignée, trou noir, lueur additive
    void BuildKeyhole()
    {
        var h = door.handle.transform;
        var normal = door.leaf.right; normal.y = 0f; normal.Normalize();             // face du battant côté poignée (joueur)
        var side = h.position - door.leaf.position; side.y = 0f;
        if (Vector3.Dot(normal, side) < 0f) normal = -normal;
        keyhole = new GameObject("Serrure").transform;
        keyhole.SetParent(door.transform, true);
        keyhole.SetPositionAndRotation(h.position + Vector3.down * 0.15f, Quaternion.LookRotation(normal, Vector3.up));
        var hr = h.GetComponent<Renderer>();
        Part(PrimitiveType.Cube, "Plaque", new Vector3(0f, 0f, -0.004f), new Vector3(0.055f, 0.1f, 0.008f), hr ? hr.sharedMaterial : null, Color.clear);
        Part(PrimitiveType.Cube, "Trou", new Vector3(0f, 0.008f, 0f), new Vector3(0.012f, 0.012f, 0.004f), null, Color.black);
        Part(PrimitiveType.Cube, "Fente", new Vector3(0f, -0.006f, 0f), new Vector3(0.005f, 0.02f, 0.004f), null, Color.black);
        var mat = Resources.Load<Material>("Fx_Lueur");
        if (mat)
        {
            glow = Part(PrimitiveType.Quad, "Lueur", new Vector3(0f, 0f, 0.01f), Vector3.one * 0.16f, mat, Color.clear);
            glow.enabled = false;
            mpb = new MaterialPropertyBlock();
        }
    }

    Renderer Part(PrimitiveType type, string name, Vector3 localPos, Vector3 scale, Material mat, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        Destroy(go.GetComponent<Collider>());                  // ne gêne ni la prise de la poignée ni le joueur
        go.transform.SetParent(keyhole, false);
        go.transform.localPosition = localPos; go.transform.localScale = scale;
        var r = go.GetComponent<Renderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (mat) r.sharedMaterial = mat;
        else { var m = new Material(r.sharedMaterial); m.SetColor(BaseColor, color); m.SetFloat("_Smoothness", 0f); r.sharedMaterial = m; }
        return r;
    }

    /// <summary>Le joueur de ce casque tient la clé près de la porte encore verrouillée (zone ouverte) : il faut le guider.</summary>
    public bool Guiding => !Unlocked && gate && gate.IsOpen && KeyInHand && door
                           && Vector3.Distance(key.transform.position, LockPoint) < hintDistance;

    Vector3 LockPoint => keyhole ? keyhole.position : door.handle.transform.position;

    /// <summary>Le joueur de ce casque a sa clé en main.</summary>
    public bool KeyInHand => key && key.HeldByHand;

    /// <summary>Le joueur essaie d'ouvrir (poignée saisie en VR, E sur PC). Vrai si la porte est (ou devient) déverrouillée.</summary>
    public bool TryOpen()
    {
        if (Unlocked) return true;
        if (KeyInHand) { Unlock(true); return true; }
        Refuse();
        return false;
    }

    void Refuse()
    {
        if (Time.time < messageUntil - messageSeconds + 0.4f) return;     // anti-rebond : pas de rafale de bruits
        messageUntil = Time.time + messageSeconds;
        if (rattle) { src.pitch = 0.8f; src.PlayOneShot(rattle, 1f); src.PlayOneShot(rattle, 0.7f); }
        DesktopHud.Message("Verrouillée — il faut la clé de la salle du générateur", messageSeconds);
    }

    // local : ce casque a ouvert avec sa clé (son + relais réseau) ; distant : un autre joueur l'a fait (NetGame)
    public void Unlock(bool local)
    {
        if (Unlocked) return;
        Unlocked = true;
        messageUntil = -1f;
        if (unlockClip) { src.pitch = 1f; src.PlayOneShot(unlockClip, 0.9f); }
        if (local)
        {
            DesktopHud.Message("Porte du générateur déverrouillée", 2f);
            LocalUnlocked?.Invoke();
        }
        Debug.Log("[KeyLock] porte de la salle du générateur déverrouillée (" + (local ? "clé de ce joueur" : "autre joueur") + ")");
    }

    void Update()
    {
        // clé tenue approchée de la poignée (geste de la serrure, VR) : déverrouille
        if (!Unlocked && KeyInHand && door && door.handle && !door.locked
            && (Vector3.Distance(key.transform.position, door.handle.transform.position) < unlockDistance
                || Vector3.Distance(key.transform.position, LockPoint) < unlockDistance))
            Unlock(true);
        UpdateGlow();
    }

    // lueur ambre de la serrure : pulse, de plus en plus forte et rapide quand la clé approche
    void UpdateGlow()
    {
        if (!glow) return;
        // salle ouverte et porte encore verrouillée : faible lueur fixe (on voit qu'il y a une serrure, même dans le noir) ;
        // clé en main près de la porte : elle pulse et grossit
        bool on = !Unlocked && gate && gate.IsOpen;
        if (glow.enabled != on) glow.enabled = on;
        if (!on) return;
        float near = 0f, pulse = 0f, a = idleGlow;
        if (Guiding)
        {
            float d = Vector3.Distance(key.transform.position, LockPoint);
            near = 1f - Mathf.InverseLerp(unlockDistance, hintDistance, d);           // 0 loin → 1 contre la serrure
            pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.Lerp(3f, 9f, near));
            a = Mathf.Lerp(0.35f, 1f, near) * Mathf.Lerp(0.55f, 1f, pulse);
        }
        mpb.SetColor(BaseColor, Amber * a);
        glow.SetPropertyBlock(mpb);
        var cam = Camera.main;
        if (cam) glow.transform.rotation = Quaternion.LookRotation(glow.transform.position - cam.transform.position, Vector3.up);
        glow.transform.localScale = Vector3.one * (Guiding ? Mathf.Lerp(0.16f, 0.26f, near) * Mathf.Lerp(0.9f, 1.1f, pulse) : 0.08f);
    }

    void LateUpdate()
    {
        if (!gate || !gate.sign) return;
        // panneau : après la vague 3 (zone ouverte) et sans clé, il reste affiché et dit qu'il faut la clé ; après un essai, il clignote
        if (gate.IsOpen && !Unlocked)
        {
            if (!gate.sign.gameObject.activeSelf) gate.sign.gameObject.SetActive(true);
            bool guiding = Guiding;
            gate.sign.text = guiding ? (DesktopMode.On ? keyHeldTextPC : keyHeldText) : lockedText;
            bool flash = Time.time < messageUntil;
            gate.sign.color = guiding ? Amber : flash && Mathf.Repeat(Time.time * 3f, 1f) < 0.5f ? Color.white : signColor;
        }
        else if (gate.IsOpen && Unlocked && gate.sign.gameObject.activeSelf) gate.sign.gameObject.SetActive(false);
        else if (!gate.IsOpen && signDefault != null && gate.sign.text != signDefault) { gate.sign.text = signDefault; gate.sign.color = signColor; }
    }
}
