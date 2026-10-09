using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BarricadeSlot : MonoBehaviour
{
    public static readonly List<BarricadeSlot> All = new List<BarricadeSlot>();
    public bool Occupied;
    public Plank Plank;                // planche actuellement posée ici (null si libre)
    public Renderer ghost;

    Material gm;
    Color baseCol;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => All.Clear();

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Awake()
    {
        gm = ghost.material;
        baseCol = gm.GetColor("_BaseColor");
    }

    void Update()
    {
        bool show = Plank.Held != null && !Occupied;
        ghost.enabled = show;
        if (show) gm.SetColor("_BaseColor", new Color(baseCol.r, baseCol.g, baseCol.b, 0.28f + 0.14f * Mathf.Sin(Time.time * 5f)));
    }

    // Distance entre l'emplacement et la planche, mesurée sur TOUTE la longueur de la planche (on la tient souvent par un bout) :
    // point de l'axe de la planche le plus proche du centre de l'emplacement.
    public float DistanceTo(Transform plank, float halfLength)
    {
        var d = transform.position - plank.position;
        float t = Mathf.Clamp(Vector3.Dot(d, plank.right), -halfLength, halfLength);
        return Vector3.Distance(transform.position, plank.position + plank.right * t);
    }

    // emplacement libre le plus proche de la planche (null si aucun à moins de maxDist) ;
    // minAlign : |cos| minimal entre la planche et l'emplacement (0 = orientation indifférente)
    public static BarricadeSlot Best(Transform plank, float halfLength, float maxDist, float minAlign)
    {
        BarricadeSlot best = null; float bd = maxDist;
        foreach (var s in All)
        {
            if (s.Occupied) continue;
            if (Mathf.Abs(Vector3.Dot(plank.right, s.transform.right)) < minAlign) continue;
            float d = s.DistanceTo(plank, halfLength);
            if (d < bd) { bd = d; best = s; }
        }
        return best;
    }

    public bool Accepts(Transform plank) => !Occupied && DistanceTo(plank, 0.6f) < 0.3f && Mathf.Abs(Vector3.Dot(plank.right, transform.right)) > 0.5f;
}
