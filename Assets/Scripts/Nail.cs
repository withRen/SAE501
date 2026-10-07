using UnityEngine;

// Un clou : apparaît quand on a le marteau en main, s'enfonce à chaque coup (3 coups).
// Détection des coups sans physique : on teste le trajet de la tête du marteau (segment) contre la tête du clou.
public class Nail : MonoBehaviour
{
    public Plank plank;
    public GameObject visual;
    public Collider col;               // inutilisé (la détection est géométrique)
    public int hitsNeeded = 3;
    public float restZ = 0.05f;        // clou sorti
    public float drivenZ = -0.025f;    // clou enfoncé
    public float minSpeed = 0.9f;      // m/s : en dessous, ce n'est pas un vrai coup
    public float sinkSpeed = 0.5f;     // m/s : vitesse à laquelle le clou s'enfonce visuellement

    int hits;
    bool popped;                       // [4.4] arraché par un zombie : il dépasse de la planche et reste visible
    float lastHit, z;
    AudioSource src;
    static AudioClip hitClip;

    public bool Driven => hits >= hitsNeeded;
    public int Hits => hits;

    void Awake()
    {
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        if (!hitClip) hitClip = ProcAudio.Hit();
        z = restZ;
        Apply();
    }

    public void Refresh(bool show)
    {
        bool vis = show || hits > 0 || popped;
        if (visual.activeSelf != vis) visual.SetActive(vis);
    }

    // clou déjà enfoncé (planche pré-posée au début de la partie)
    public void SetDriven()
    {
        hits = hitsNeeded;
        z = drivenZ;
        visual.SetActive(true);
        Apply();
    }

    public void ResetHits()
    {
        hits = 0;
        popped = false;
    }

    // [4.4] un zombie fait sauter ce clou : il ressort (animation vers restZ) et devra être renfoncé de 3 coups
    public void Pop()
    {
        hits = 0;
        popped = true;
        visual.SetActive(true);
    }

    void Apply()
    {
        var p = visual.transform.localPosition;
        p.z = z;
        visual.transform.localPosition = p;
    }

    void Update()
    {
        // animation d'enfoncement
        float target = Mathf.Lerp(restZ, drivenZ, hits / (float)hitsNeeded);
        if (!Mathf.Approximately(z, target)) { z = Mathf.MoveTowards(z, target, sinkSpeed * Time.deltaTime); Apply(); }

        if (!visual.activeSelf || Driven || Time.time - lastHit < 0.25f) return;
        var head = HammerHead.Instance;
        if (head == null || !plank.IsHeldByHand || head.Speed < minSpeed) return;

        // tête du clou, en monde
        Vector3 tip = transform.TransformPoint(0f, 0f, z + 0.04f);
        if (DistancePointSegment(tip, head.PreviousPosition, head.Position) > head.Radius + 0.03f) return;
        // le marteau doit aller vers la planche (pas de coup en reculant)
        if (Vector3.Dot(head.Velocity.normalized, -transform.forward) < 0.25f) return;

        hits++;
        if (Driven) popped = false;
        lastHit = Time.time;
        src.pitch = 0.9f + 0.1f * hits;
        src.PlayOneShot(hitClip);
    }

    static float DistancePointSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        float l = ab.sqrMagnitude;
        float t = l < 1e-8f ? 0f : Mathf.Clamp01(Vector3.Dot(p - a, ab) / l);
        return Vector3.Distance(p, a + ab * t);
    }
}
