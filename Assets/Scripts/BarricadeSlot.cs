using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BarricadeSlot : MonoBehaviour
{
    public static readonly List<BarricadeSlot> All = new List<BarricadeSlot>();
    public bool Occupied;
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

    public bool Accepts(Transform plank) =>
        !Occupied
        && Vector3.Distance(plank.position, transform.position) < 0.22f
        && Mathf.Abs(Vector3.Dot(plank.right, transform.right)) > 0.8f;
}
