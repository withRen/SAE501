using System.Collections.Generic;
using UnityEngine;

// Porte verrouillée qui ferme une zone de la carte jusqu'à un certain niveau (WaveManager).
// Verrouillée : battant fermé (le joueur ne passe pas : le rig a un CharacterController) + panneau rouge « VERROUILLÉ ».
// Au niveau voulu : la porte se déverrouille, le joueur l'ouvre en tirant la poignée (HingedDoor).
// Ces portes sont ignorées par le NavMesh (les zombies n'apparaissent dans une zone qu'une fois la porte ouverte).
public class ZoneGate : MonoBehaviour
{
    public static readonly List<ZoneGate> All = new List<ZoneGate>();

    public int unlockLevel = 2;
    public string zoneName;
    public Transform door;
    public Collider doorCollider;
    public TextMesh sign;
    public float openHeight = 2.3f;
    public float openSpeed = 1.8f;

    public bool IsOpen { get; private set; }
    Vector3 closedPos;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => All.Clear();

    void Awake()
    {
        All.Add(this);
        closedPos = door.localPosition;
        if (sign) sign.text = "VERROUILLÉ\n" + zoneName + "\n(niveau " + unlockLevel + ")";
    }

    void OnDestroy() => All.Remove(this);

    public static void UpdateAll(int level)
    {
        foreach (var g in All) g.SetOpen(level >= g.unlockLevel);
    }

    public HingedDoor hinged;           // porte sur charnière : déverrouillée, le joueur l'ouvre en tirant (sinon : battant qui monte)

    public void SetOpen(bool open)
    {
        IsOpen = open;
        if (sign) sign.gameObject.SetActive(!open);
        if (hinged) { hinged.locked = !open && hinged.Closed; return; }   // le battant garde sa collision : c'est une vraie porte
        if (doorCollider) doorCollider.enabled = !open;
    }

    void Update()
    {
        if (hinged) return;
        var target = closedPos + (IsOpen ? Vector3.up * openHeight : Vector3.zero);
        door.localPosition = Vector3.MoveTowards(door.localPosition, target, openSpeed * Time.deltaTime);
    }
}
