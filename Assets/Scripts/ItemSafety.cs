using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Filet de sécurité des objets attrapables (ajouté par RoomBuilder.MakeGrab) : un objet ne doit jamais finir sous le sol.
// En VR, la main passe facilement sous le sol virtuel quand on pose un objet par terre (le vrai sol n'est pas forcément
// à la même hauteur) : l'objet, collé à la main, se retrouve sous le sol et tombe sans fin une fois lâché.
//   1. Au lâcher : vu d'en haut (depuis la hauteur de la tête), si la première surface de la carte rencontrée à la verticale
//      de l'objet (sol, terrain : collider de structure) est au-dessus de lui, il est remis sur cette surface.
//   2. En continu : un objet libre qui tombe sous la carte (y < limite) revient à sa dernière position sûre.
[RequireComponent(typeof(Rigidbody))]
public class ItemSafety : MonoBehaviour
{
    public float killY = -3f;               // en dessous : l'objet est tombé de la carte
    public float lift = 0.08f;              // marge au-dessus de la surface quand on le remet en place

    // [hotfix #164] objet hors de portée : posé et immobile plus de UnreachableDelay s à un endroit qu'on ne peut pas atteindre à pied
    // (dehors par une fenêtre, derrière une porte de zone fermée), il revient à sa dernière position atteignable. Contrôle toutes les
    // 2 s environ, seulement si l'objet a bougé ; rien si la scène n'a pas de NavMesh (menu) ni pour un objet jamais atteignable
    // (chargeur posé dans une zone encore fermée en début de partie).
    const float UnreachableDelay = 8f, CheckEvery = 2f;
    Vector3 lastReachable, lastChecked = new Vector3(float.MaxValue, 0f, 0f);
    Quaternion lastReachableRot;
    bool hasReachable;
    float unreachableSince = -1f, nextCheck;
    static UnityEngine.AI.NavMeshPath path;

    XRGrabInteractable grab;
    Rigidbody rb;
    Vector3 lastSafe;
    Quaternion lastSafeRot;
    int fixAtFrame = -1;
    static readonly RaycastHit[] hits = new RaycastHit[16];

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        lastSafe = transform.position; lastSafeRot = transform.rotation;
        if (grab) grab.selectExited.AddListener(OnReleased);
    }

    void OnDestroy() { if (grab) grab.selectExited.RemoveListener(OnReleased); }

    // le lâcher est traité à l'image suivante (la physique a repris la main sur l'objet)
    void OnReleased(SelectExitEventArgs a) => fixAtFrame = Time.frameCount + 1;

    void Update()
    {
        if (grab && grab.isSelected) return;
        if (fixAtFrame >= 0 && Time.frameCount >= fixAtFrame) { fixAtFrame = -1; LiftIfUnderFloor(); }

        var p = transform.position;
        if (p.y < killY) { Place(lastSafe, lastSafeRot); return; }
        // position sûre : posé et immobile, ou rangé (cinématique)
        if (rb.isKinematic || rb.linearVelocity.sqrMagnitude < 0.0025f) { lastSafe = p; lastSafeRot = transform.rotation; }
        CheckReachable(p);
    }

    void CheckReachable(Vector3 p)
    {
        if (rb.isKinematic || rb.linearVelocity.sqrMagnitude > 0.0025f || Time.time < nextCheck) return;   // rangé, en mouvement
        nextCheck = Time.time + CheckEvery * Random.Range(0.8f, 1.2f);
        bool moved = (p - lastChecked).sqrMagnitude > 0.01f;
        if (!moved && unreachableSince < 0f) return;                // déjà vérifié à cet endroit, atteignable
        lastChecked = p;
        var player = PlayerHealth.Instance;
        if (!player) return;
        var feet = player.transform.position;
        if (!UnityEngine.AI.NavMesh.SamplePosition(feet, out var from, 1.5f, UnityEngine.AI.NavMesh.AllAreas)) return;   // pas de NavMesh ici
        bool reachable = UnityEngine.AI.NavMesh.SamplePosition(p, out var to, 2f, UnityEngine.AI.NavMesh.AllAreas);
        if (reachable)
        {
            if (path == null) path = new UnityEngine.AI.NavMeshPath();
            // à pied seulement : sans les passages par les fenêtres et les trous du toit (zones réservées aux zombies)
            reachable = UnityEngine.AI.NavMesh.CalculatePath(from.position, to.position, Zombie.WalkMask, path)
                        && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
        }
        if (reachable) { lastReachable = p; lastReachableRot = transform.rotation; hasReachable = true; unreachableSince = -1f; return; }
        if (!hasReachable) return;                                  // jamais atteignable : on ne le déplace pas
        if (unreachableSince < 0f) { unreachableSince = Time.time; return; }
        if (Time.time - unreachableSince >= UnreachableDelay)
        {
            unreachableSince = -1f;
            Place(lastReachable, lastReachableRot);
            lastChecked = lastReachable;
        }
    }

    void LiftIfUnderFloor()
    {
        var p = transform.position;
        float top = Camera.main ? Mathf.Max(Camera.main.transform.position.y, p.y + 0.5f) : p.y + 2f;
        var from = new Vector3(p.x, top, p.z);
        int n = Physics.RaycastNonAlloc(from, Vector3.down, hits, top - p.y + 0.01f, ~0, QueryTriggerInteraction.Ignore);
        float best = float.MinValue;
        for (int i = 0; i < n; i++)
        {
            var c = hits[i].collider;
            if (c.attachedRigidbody != null) continue;                    // objets mobiles, joueur : ignorés
            if (!(c is MeshCollider) && !(c is TerrainCollider)) continue; // seulement sols et terrain (meubles : boîtes)
            if (hits[i].normal.y < 0.5f) continue;                         // une surface sur laquelle on peut poser
            if (hits[i].point.y > p.y + 0.02f) best = Mathf.Max(best, hits[i].point.y);
        }
        if (best > float.MinValue) Place(new Vector3(p.x, best + lift, p.z), transform.rotation);
    }

    void Place(Vector3 pos, Quaternion rot)
    {
        if (!rb.isKinematic) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        rb.position = pos; rb.rotation = rot;
        transform.SetPositionAndRotation(pos, rot);
    }
}
