using System.Collections.Generic;
using UnityEngine;

// [4.5] Approvisionnement en planches (décision du groupe : réserve limitée, renouvelée à chaque vague).
//   - Au départ : 8 planches libres dans le bunker (5 sur l'établi, 3 au sol) + les planches déjà clouées.
//   - L'établi ne se remplit PAS tout seul : il faut aller chercher les nouvelles planches.
//   - À la fin d'une vague, des planches apparaissent dans la pièce qui vient d'être débloquée
//     (fin de la vague 1 -> 3 dans la Cuisine ; fin de la vague 2 -> 6 dans le Stockage). Une annonce le dit au joueur.
//   - Une planche arrachée par un zombie retombe dans le bunker : on la ramasse et on la recloue (Plank.Break).
//   - Une planche tombée sous le sol réapparaît sur l'établi.
//   - Un panneau au-dessus de l'établi affiche les planches libres.
// Construit par RoomBuilder (BuildPlankSupply). Les caches se déplacent dans la scène (objets « Planches_Cuisine », « Planches_Stockage »).
public class PlankSupply : MonoBehaviour
{
    public static PlankSupply Instance { get; private set; }

    [System.Serializable]
    public class Cache
    {
        public string roomName = "Cuisine";
        public int afterWave = 1;          // livrées à la fin de cette vague
        public int planks = 3;
        public Transform point;            // centre de la pile (au sol de la pièce)
        [HideInInspector] public bool delivered;
    }

    public Plank template;                 // planche modèle, désactivée (copiée pour chaque nouvelle planche)
    public Transform benchPoint;           // établi du bunker (planches tombées sous le sol)
    public Cache[] caches;
    public TextMesh label;

    bool wasInBreak;
    int pendingWave;                       // livraison en attente (le joueur tenait une planche)
    float nextCheck, flashUntil;
    string flashText;

    void Awake() => Instance = this;
    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        if (template) template.gameObject.SetActive(false);
        UpdateLabel();
    }

    void Update()
    {
        var wm = WaveManager.Instance;
        bool inBreak = wm && wm.InBreak;
        if (inBreak && !wasInBreak) pendingWave = WaveManager.Level;   // la vague qui vient de se terminer
        wasInBreak = inBreak;

        // une seule planche attrapable à la fois : on livre quand le joueur n'en tient pas
        if (pendingWave > 0 && Plank.Held == null) { Deliver(pendingWave); pendingWave = 0; }

        if (Time.time >= nextCheck)
        {
            nextCheck = Time.time + 0.5f;
            RescueFallenPlanks();
            if (flashText != null && Time.time >= flashUntil) flashText = null;
            UpdateLabel();
        }

        if (label && Camera.main)
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - Camera.main.transform.position);
    }

    void Deliver(int wave)
    {
        if (caches == null) return;
        foreach (var c in caches)
        {
            if (c.delivered || c.afterWave != wave || c.point == null) continue;
            c.delivered = true;
            for (int i = 0; i < c.planks; i++) Spawn(c.point, i);
            string msg = "+" + c.planks + " planches dans la " + c.roomName.ToUpper() + " !";
            flashText = msg; flashUntil = Time.time + 10f;
            if (WaveManager.Instance) WaveManager.Instance.Say("VAGUE " + wave + " TERMINÉE\n" + msg + "\nAllez les chercher, puis réparez.", 7f);
        }
    }

    // pile : 3 planches côte à côte par couche, posées à plat, lâchées un peu au-dessus du sol
    Plank Spawn(Transform at, int i)
    {
        if (!template) return null;
        float side = (i % 3 - 1) * 0.2f;
        float up = 0.6f + (i / 3) * 0.08f;
        var pos = at.position + at.forward * side + Vector3.up * up;
        var rot = at.rotation * Quaternion.Euler(-90f, 0f, 0f);   // à plat (même convention que RoomBuilder.BuildPlanks)
        var p = Instantiate(template, pos, rot, template.transform.parent);
        p.name = "Planche_Livree";
        p.gameObject.SetActive(true);
        return p;
    }

    void RescueFallenPlanks()
    {
        if (!benchPoint) return;
        foreach (var p in Plank.All)
        {
            if (!p || p.Placed || Plank.Held == p || p.transform.position.y > -2f) continue;
            var rb = p.GetComponent<Rigidbody>();
            if (rb) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            p.transform.SetPositionAndRotation(benchPoint.position + Vector3.up * 0.3f, benchPoint.rotation);
        }
    }

    int FreePlanks()
    {
        int n = 0;
        foreach (var p in Plank.All) if (p && !p.Placed) n++;
        return n;
    }

    void UpdateLabel()
    {
        if (!label) return;
        int n = FreePlanks();
        string t = flashText ?? ("Planches libres : " + n);
        if (label.text != t) label.text = t;
        label.color = flashText != null ? new Color(0.5f, 1f, 0.5f) : n > 0 ? new Color(1f, 0.85f, 0.4f) : new Color(1f, 0.35f, 0.3f);
    }

    [ContextMenu("Test : livrer la Cuisine (fin de vague 1)")] void TestKitchen() => Deliver(1);
    [ContextMenu("Test : livrer le Stockage (fin de vague 2)")] void TestStorage() => Deliver(2);
}
