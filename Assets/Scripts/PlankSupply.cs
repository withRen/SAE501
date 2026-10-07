using UnityEngine;

// [4.5] Approvisionnement en planches (décision du groupe : réserve limitée, renouvelée à chaque vague).
//   - Au départ : une pile de 8 planches dans le bunker (RoomBuilder.BuildPlanks). L'établi ne se remplit pas tout seul.
//   - Les autres planches sont dans les pièces de la carte (cuisine, stockage). À la FIN d'une vague, la zone du niveau
//     suivant s'ouvre tout de suite (WaveManager) : la pause sert à aller y chercher des planches, puis à réparer.
//   - Une planche arrachée par un zombie retombe dans le bunker : on la ramasse et on la recloue (Plank.Break).
//   - Une planche tombée sous le sol revient sur la pile de réserve.
//   - Un panneau au-dessus de l'établi affiche les planches libres présentes dans le bunker.
// Construit par RoomBuilder (BuildPlankSupply).
public class PlankSupply : MonoBehaviour
{
    public Transform rescuePoint;          // pile de réserve du bunker
    public TextMesh label;

    float nextCheck;

    void Update()
    {
        if (Time.time >= nextCheck)
        {
            nextCheck = Time.time + 0.5f;
            RescueFallenPlanks();
            UpdateLabel();
        }
        if (label && Camera.main)
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - Camera.main.transform.position);
    }

    void RescueFallenPlanks()
    {
        if (!rescuePoint) return;
        foreach (var p in Plank.All)
        {
            if (!p || p.Placed || Plank.Held == p || p.transform.position.y > -2f) continue;
            var rb = p.GetComponent<Rigidbody>();
            if (rb) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            p.transform.position = rescuePoint.position;
        }
    }

    // étendue du bunker (docs/MAP.md : Blender x -7,5..3,8 · y -14,1..-6,1 ; Unity = (-x, z, -y))
    static bool InBunker(Vector3 p) => p.x > -3.8f && p.x < 7.5f && p.z > 6.1f && p.z < 14.1f && p.y > -1f;

    void UpdateLabel()
    {
        if (!label) return;
        int n = 0;
        foreach (var p in Plank.All) if (p && !p.Placed && InBunker(p.transform.position)) n++;
        string t = "Planches dans le bunker : " + n;
        if (label.text != t) label.text = t;
        label.color = n > 0 ? new Color(1f, 0.85f, 0.4f) : new Color(1f, 0.35f, 0.3f);
    }
}
