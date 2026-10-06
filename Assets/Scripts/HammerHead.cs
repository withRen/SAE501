using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HammerHead : MonoBehaviour
{
    public float Speed { get; private set; }
    Vector3 last;

    void Start() => last = transform.position;

    void Update()
    {
        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        float inst = (transform.position - last).magnitude / dt;
        last = transform.position;
        Speed = Mathf.Max(inst, Speed * 0.85f);
    }
}
