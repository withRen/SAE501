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

    // [4.4] boucles simples au lieu de LINQ : aucune allocation (appelé souvent par les zombies)
    static bool Holds(BarricadeSlot s) => s && s.Plank != null && (s.Plank.state == Plank.State.Nailed || s.Plank.state == Plank.State.Fixed);

    public int PlankCount
    {
        get
        {
            int n = 0;
            for (int i = 0; i < slots.Length; i++) if (Holds(slots[i])) n++;
            return n;
        }
    }

    public bool IsBarricaded
    {
        get
        {
            for (int i = 0; i < slots.Length; i++) if (Holds(slots[i])) return true;
            return false;
        }
    }

    // planche à attaquer : d'abord celle qui est déjà abîmée (on finit ce qu'on a commencé), sinon la plus haute
    public Plank NextPlankToBreak()
    {
        Plank best = null;
        float bestY = float.MinValue;
        int bestDamage = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (!Holds(s)) continue;
            int d = s.Plank.Damage;
            float y = s.transform.position.y;
            if (d > bestDamage || (d == bestDamage && y > bestY)) { best = s.Plank; bestDamage = d; bestY = y; }
        }
        return best;
    }
}
