using UnityEngine;

// Vieille lampe qui grésille : petit bruit continu + rafales de coupures.
public class FlickerLamp : MonoBehaviour
{
    public Light lamp;
    public Renderer bulb;
    public float baseIntensity = 6f;

    float burstEnd, nextBurst;
    Material bulbMat;

    void Start()
    {
        nextBurst = Time.time + Random.Range(1f, 4f);
        if (bulb) bulbMat = bulb.material;
    }

    void Update()
    {
        float k = 1f + (Mathf.PerlinNoise(Time.time * 3f, 0.3f) - 0.5f) * 0.14f;
        if (Time.time >= nextBurst)
        {
            burstEnd = Time.time + Random.Range(0.15f, 0.5f);
            nextBurst = burstEnd + Random.Range(2f, 7f);
        }
        if (Time.time < burstEnd)
            k *= Random.value < 0.5f ? Random.Range(0.03f, 0.4f) : Random.Range(0.8f, 1f);

        lamp.intensity = baseIntensity * k;
        if (bulbMat) bulbMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.5f) * k * 2.5f);
    }
}
