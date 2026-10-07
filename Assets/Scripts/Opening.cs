using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Une ouverture du bunker (porte ou fenêtre) : ses 3 emplacements de planches et les points d'approche des zombies.
public class Opening : MonoBehaviour
{
    public static readonly List<Opening> All = new List<Opening>();

    public string openingName;
    public BarricadeSlot[] slots;
    public Transform approachPoint;     // devant l'ouverture, côté extérieur : le zombie s'y arrête pour arracher les planches
    public Transform insidePoint;       // juste derrière l'ouverture, côté bunker
    public bool zombiesCanUse = true;   // faux tant que l'extérieur n'est pas accessible (fenêtres)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => All.Clear();

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public int PlankCount => slots.Count(s => s.Plank != null && s.Plank.InSlot);      // posées (clouées ou non)
    public bool IsBarricaded => PlankCount > 0;

    // planche à arracher : la plus haute
    public Plank NextPlankToBreak()
    {
        return slots.Where(s => s.Plank != null && s.Plank.InSlot)
                    .OrderByDescending(s => s.transform.position.y)
                    .Select(s => s.Plank)
                    .FirstOrDefault();
    }
}
