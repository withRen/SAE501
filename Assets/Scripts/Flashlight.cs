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
    float barLength, barZ;
    XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.activated.AddListener(_ => { if (BeltItem.InHand(grab) && level > 0f) on = !on; });
        if (bar) { barLength = bar.localScale.z; barZ = bar.localPosition.z; }
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
            // batterie presque vide (< 10 %) : la lumière faiblit et ondule doucement (bruit lissé, pas de saut à chaque image)
            float k = 1f;
            if (level < 0.1f)
            {
                float low = Mathf.InverseLerp(0f, 0.1f, level);                       // 1 → 0 quand la batterie se vide
                float wave = Mathf.PerlinNoise(Time.time * 1.5f, 0.37f);              // variation lente
                k = Mathf.Lerp(0.35f, 1f, low) * Mathf.Lerp(0.75f, 1f, wave);
            }
            spot.intensity = Mathf.Lerp(spot.intensity, baseIntensity * k, 1f - Mathf.Exp(-8f * Time.deltaTime));
        }

        if (bar)
        {
            var s = bar.localScale;
            s.z = Mathf.Max(0.001f, barLength * level);
            bar.localScale = s;
            var p = bar.localPosition;
            p.z = barZ - barLength * 0.5f * (1f - level);          // la barre se vide vers l'avant, depuis sa place sur le manche
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
