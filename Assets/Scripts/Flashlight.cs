using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Flashlight : MonoBehaviour
{
    public Light spot;
    public Transform bar;
    public float drainPerSecond = 0.008f;
    public float level = 1f;
    public float baseIntensity = 12f;

    bool on;
    float barLength;
    XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.activated.AddListener(_ => { if (BeltItem.InHand(grab) && level > 0f) on = !on; });
        if (bar) barLength = bar.localScale.z;
    }

    void Update()
    {
        if (on)
        {
            level = Mathf.Max(0f, level - drainPerSecond * Time.deltaTime);
            if (level <= 0f) on = false;
        }

        spot.enabled = on;
        if (on)
        {
            float k = 1f;
            if (level < 0.15f) k = Random.value < 0.25f ? Random.Range(0.1f, 0.5f) : Random.Range(0.6f, 1f);
            spot.intensity = baseIntensity * k;
        }

        if (bar)
        {
            var s = bar.localScale;
            s.z = Mathf.Max(0.001f, barLength * level);
            bar.localScale = s;
            var p = bar.localPosition;
            p.z = -barLength * 0.5f * (1f - level);
            bar.localPosition = p;
        }
    }

    public void Recharge() => level = 1f;

    // un "BatteryPort" (trigger) est placé à l'arrière de la lampe
    void OnTriggerEnter(Collider other)
    {
        var b = other.GetComponentInParent<Battery>();
        if (!b) return;
        Recharge();
        b.Consume();
    }
}
