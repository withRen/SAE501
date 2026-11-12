using System;
using UnityEngine;

// [Clé du générateur] Serrure d'une porte sur charnière (HingedDoor) : la porte de la salle du générateur.
// En plus du verrou de niveau (ZoneGate : salle ouverte après la vague 3), la porte reste fermée tant qu'on n'a pas la clé :
//   - déverrouiller : tenir la clé (KeyItem) dans une main et saisir la poignée avec l'autre (VR), ou approcher la clé tenue
//     de la poignée (à moins de unlockDistance) ; sur PC, viser la porte avec la clé en main et appuyer sur E ;
//   - sans la clé : la poignée se lâche, petit bruit de serrure, message « Verrouillée — il faut la clé de la salle du
//     générateur » sur le panneau de la porte (texte dans le monde, lisible au casque) et en bas de l'écran sur PC ;
//   - une fois déverrouillée, elle le reste (la clé n'est plus nécessaire, la porte s'ouvre ensuite normalement).
// Multijoueur coop : le déverrouillage passe par NetGame (LocalUnlocked → l'hôte → NetworkVariable GeneratorDoorUnlocked) : la
// porte est déverrouillée sur tous les casques ; le message « verrouillée » reste propre au joueur qui essaie.
public class KeyLock : MonoBehaviour
{
    public HingedDoor door;
    public ZoneGate gate;                       // panneau de la porte (gate.sign) et verrou de niveau
    public KeyItem key;                         // la clé de CE casque (chaque casque a sa propre scène)
    public string lockedText = "VERROUILLÉE\nIl faut la clé de la salle\ndu générateur";
    public float unlockDistance = 0.25f;
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
    }

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
            && Vector3.Distance(key.transform.position, door.handle.transform.position) < unlockDistance)
            Unlock(true);
    }

    void LateUpdate()
    {
        if (!gate || !gate.sign) return;
        // panneau : après la vague 3 (zone ouverte) et sans clé, il reste affiché et dit qu'il faut la clé ; après un essai, il clignote
        if (gate.IsOpen && !Unlocked)
        {
            if (!gate.sign.gameObject.activeSelf) gate.sign.gameObject.SetActive(true);
            gate.sign.text = lockedText;
            bool flash = Time.time < messageUntil;
            gate.sign.color = flash && Mathf.Repeat(Time.time * 3f, 1f) < 0.5f ? Color.white : signColor;
        }
        else if (gate.IsOpen && Unlocked && gate.sign.gameObject.activeSelf) gate.sign.gameObject.SetActive(false);
        else if (!gate.IsOpen && signDefault != null && gate.sign.text != signDefault) { gate.sign.text = signDefault; gate.sign.color = signColor; }
    }
}
