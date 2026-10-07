using System.Collections.Generic;
using UnityEngine;

// [4.5] Approvisionnement en planches (décision du groupe : réserve LIMITÉE qui se renouvelle à chaque vague).
//   - L'établi a des emplacements (spots) : dès qu'un emplacement est vide, une planche de la réserve y réapparaît.
//   - La réserve gagne `perWave` planches au début de chaque pause entre deux vagues (pour réparer), sans dépasser `maxStock`.
//   - Une planche arrachée par un zombie retombe dans le bunker : on la ramasse et on la remet (Plank.Break).
//   - Une planche perdue (tombée hors du bunker, ou sous le sol) disparaît après `lostAfter` s et retourne dans la réserve.
//   - Un panneau au-dessus de l'établi affiche la réserve.
// Construit par RoomBuilder (BuildPlankSupply). Réglages dans l'Inspector de l'objet « ReserveDePlanches ».
public class PlankSupply : MonoBehaviour
{
    public static PlankSupply Instance { get; private set; }

    [Header("Planches")]
    public Plank template;                 // planche modèle, désactivée (copiée pour chaque nouvelle planche)
    public Transform[] spots;              // emplacements sur l'établi

    [Header("Réserve")]
    public int stock = 3;                  // planches en réserve en plus de celles posées sur l'établi
    public int perWave = 4;                // ajoutées à chaque pause entre deux vagues
    public int maxStock = 8;

    [Header("Planches perdues")]
    public Vector2 bunkerHalfSize = new Vector2(2.7f, 3.2f);   // demi-taille du bunker (repère du monde, centré en 0,0)
    public float lostAfter = 8f;           // secondes hors du bunker avant de revenir dans la réserve

    [Header("Affichage")]
    public TextMesh label;

    readonly Dictionary<Plank, float> outsideSince = new Dictionary<Plank, float>();
    readonly List<Plank> toRecycle = new List<Plank>();
    bool wasInBreak;
    float nextCheck, flashUntil;
    string flashText;

    public int Stock => stock;

    void Awake() => Instance = this;
    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        if (template) template.gameObject.SetActive(false);
        UpdateLabel();
    }

    void Update()
    {
        // nouvelle pause entre deux vagues -> livraison de planches
        bool inBreak = WaveManager.Instance && WaveManager.Instance.InBreak;
        if (inBreak && !wasInBreak) AddStock(perWave, "+" + perWave + " planches !");
        wasInBreak = inBreak;

        if (Time.time >= nextCheck)
        {
            nextCheck = Time.time + 0.5f;
            RecycleLostPlanks();
            RefillBench();
        }

        if (label)
        {
            var cam = Camera.main;
            if (cam) label.transform.rotation = Quaternion.LookRotation(label.transform.position - cam.transform.position);
            if (flashText != null && Time.time >= flashUntil) { flashText = null; UpdateLabel(); }
        }
    }

    public void AddStock(int n, string message = null)
    {
        stock = Mathf.Min(maxStock, stock + n);
        if (message != null) { flashText = message; flashUntil = Time.time + 3f; }
        UpdateLabel();
    }

    // il manque des planches sur l'établi + planche en réserve -> une planche réapparaît (la pile se reconstruit par le bas)
    void RefillBench()
    {
        if (template == null || spots == null || spots.Length == 0 || stock <= 0) return;
        if (Plank.Held != null) return;   // une seule planche attrapable à la fois : on attend que le joueur la lâche

        int onBench = 0;
        foreach (var p in Plank.All) if (p && !p.Placed && OnBench(p.transform.position)) onBench++;

        foreach (var s in spots)
        {
            if (stock <= 0 || onBench >= spots.Length) break;
            if (s == null || SpotTaken(s.position)) continue;
            var p = Instantiate(template, s.position, s.rotation, template.transform.parent);
            p.name = "Planche_Reserve";
            p.gameObject.SetActive(true);
            stock--;
            onBench++;
            UpdateLabel();
        }
    }

    // zone de l'établi : autour des emplacements
    bool OnBench(Vector3 pos)
    {
        foreach (var s in spots)
        {
            if (!s) continue;
            var d = pos - s.position;
            if (d.y > -0.25f && d.y < 0.4f && new Vector2(d.x, d.z).magnitude < 0.6f) return true;
        }
        return false;
    }

    // une planche occupe déjà cet emplacement (même hauteur, à peu près au même endroit)
    static bool SpotTaken(Vector3 pos)
    {
        foreach (var p in Plank.All)
        {
            if (!p) continue;
            var d = p.transform.position - pos;
            if (Mathf.Abs(d.y) < 0.025f && new Vector2(d.x, d.z).magnitude < 0.12f) return true;
        }
        return false;
    }

    // planche libre, non tenue, hors du bunker depuis trop longtemps -> retour dans la réserve
    void RecycleLostPlanks()
    {
        toRecycle.Clear();
        foreach (var p in Plank.All)
        {
            if (!p || p.Placed || Plank.Held == p) { if (p) outsideSince.Remove(p); continue; }
            var pos = p.transform.position;
            bool lost = pos.y < -2f || Mathf.Abs(pos.x) > bunkerHalfSize.x || Mathf.Abs(pos.z) > bunkerHalfSize.y;
            if (!lost) { outsideSince.Remove(p); continue; }
            if (!outsideSince.TryGetValue(p, out float since)) { outsideSince[p] = Time.time; continue; }
            if (Time.time - since >= lostAfter || pos.y < -2f) toRecycle.Add(p);
        }
        foreach (var p in toRecycle)
        {
            outsideSince.Remove(p);
            Destroy(p.gameObject);
            AddStock(1);
        }
    }

    void UpdateLabel()
    {
        if (!label) return;
        label.text = flashText ?? ("RÉSERVE : " + stock + (stock > 1 ? " planches" : " planche"));
        label.color = flashText != null ? new Color(0.5f, 1f, 0.5f) : stock > 0 ? new Color(1f, 0.85f, 0.4f) : new Color(1f, 0.35f, 0.3f);
    }

    [ContextMenu("Test : livraison de fin de vague")]
    void TestDelivery() => AddStock(perWave, "+" + perWave + " planches !");
}
