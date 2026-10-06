using UnityEngine;

// Vieille lampe défaillante, façon jeu d'horreur : léger souffle continu et, rarement,
// une brève baisse de tension douce (pas de coupure franche).
public class FlickerLamp : MonoBehaviour
{
    public Light lamp;
    public Renderer bulb;
    public int bulbMaterialIndex;
    public float baseIntensity = 1f;

    [Header("Souffle continu")]
    public float hum = 0.05f;           // amplitude du souffle (0.05 = ±5 %)
    public float humSpeed = 1.6f;

    [Header("Baisses de tension")]
    public float burstMinGap = 8f;      // délai mini entre deux baisses (s)
    public float burstMaxGap = 20f;
    public float burstMinTime = 0.25f;
    public float burstMaxTime = 0.7f;
    [Range(0f, 1f)] public float dipDepth = 0.5f;   // intensité la plus basse atteinte (1 = aucune baisse)
    public float smoothing = 12f;       // plus petit = transitions plus lentes

    float burstEnd, nextBurst, nextStep, target = 1f, k = 1f;
    Material bulbMat;

    void Start()
    {
        nextBurst = Time.time + Random.Range(burstMinGap * 0.5f, burstMaxGap);
        if (bulb)
        {
            var mats = bulb.materials;   // instances, pour ne pas modifier l'asset
            bulb.materials = mats;
            bulbMat = mats[Mathf.Clamp(bulbMaterialIndex, 0, mats.Length - 1)];
        }
    }

    void Update()
    {
        float noise = 1f + (Mathf.PerlinNoise(Time.time * humSpeed, 0.3f) - 0.5f) * 2f * hum;

        if (Time.time >= nextBurst)
        {
            burstEnd = Time.time + Random.Range(burstMinTime, burstMaxTime);
            nextBurst = burstEnd + Random.Range(burstMinGap, burstMaxGap);
        }

        if (Time.time < burstEnd)
        {
            if (Time.time >= nextStep)
            {
                target = Random.Range(dipDepth, 1f);
                nextStep = Time.time + Random.Range(0.07f, 0.16f);
            }
        }
        else target = 1f;

        k = Mathf.Lerp(k, target, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
        float v = k * noise;

        lamp.intensity = baseIntensity * v;
        if (bulbMat) bulbMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.5f) * v * 1.6f);
    }
}
