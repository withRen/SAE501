using System.Collections;
using UnityEngine;

// Ambiance animée du menu : éclairs (le décor et la lumière flashent, puis le tonnerre gronde), pluie qui crépite,
// néon vert qui grésille, nappes de brouillard qui dérivent. Sons générés par code (pas d'asset audio).
public class MenuAmbience : MonoBehaviour
{
    public Renderer backdrop;               // grand écran courbé avec l'image de la ville
    public Light flashLight;                // lumière directionnelle des éclairs (intensité 0 au repos)
    public Light neon;                      // néon vert qui grésille
    public Transform[] mist;                // nappes de brouillard
    public Vector2 lightningEvery = new Vector2(7f, 16f);

    AudioSource rain, thunder;
    Color baseColor;
    float neonBase;
    Vector3[] mistStart;

    void Start()
    {
        if (backdrop) baseColor = backdrop.material.GetColor("_BaseColor");
        if (neon) neonBase = neon.intensity;
        if (mist != null) { mistStart = new Vector3[mist.Length]; for (int i = 0; i < mist.Length; i++) mistStart[i] = mist[i].localPosition; }

        rain = gameObject.AddComponent<AudioSource>();
        rain.clip = RainClip(); rain.loop = true; rain.volume = 0.18f; rain.spatialBlend = 0f; rain.Play();
        thunder = gameObject.AddComponent<AudioSource>();
        thunder.spatialBlend = 0f; thunder.volume = 0.8f;
        StartCoroutine(Lightning());
    }

    void Update()
    {
        // néon : grésillement irrégulier, parfois une coupure
        if (neon)
        {
            float n = Mathf.PerlinNoise(Time.time * 7f, 0.3f);
            neon.intensity = neonBase * (n < 0.12f ? 0.1f : 0.75f + 0.35f * n);
        }
        // brouillard : dérive lente
        if (mist != null)
            for (int i = 0; i < mist.Length; i++)
                if (mist[i]) mist[i].localPosition = mistStart[i] + Vector3.right * Mathf.Sin(Time.time * (0.05f + 0.02f * i) + i * 1.7f) * 3f;
    }

    IEnumerator Lightning()
    {
        yield return new WaitForSeconds(3f);
        while (true)
        {
            // deux ou trois flashs rapprochés
            int flashes = Random.Range(2, 4);
            for (int f = 0; f < flashes; f++)
            {
                float k = Random.Range(0.6f, 1f);
                Set(1f + 1.6f * k, 2.5f * k);
                yield return new WaitForSeconds(Random.Range(0.04f, 0.09f));
                Set(1f, 0f);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
            yield return new WaitForSeconds(Random.Range(0.6f, 2f));   // le son arrive après la lumière
            thunder.pitch = Random.Range(0.8f, 1.1f);
            thunder.PlayOneShot(ThunderClip());
            yield return new WaitForSeconds(Random.Range(lightningEvery.x, lightningEvery.y));
        }
    }

    void Set(float backdropGain, float light)
    {
        if (backdrop) backdrop.material.SetColor("_BaseColor", baseColor * backdropGain);
        if (flashLight) flashLight.intensity = light;
    }

    // ---------- sons générés ----------
    static AudioClip rainClip, thunderClip;

    static AudioClip RainClip()
    {
        if (rainClip) return rainClip;
        const int rate = 22050; int n = rate * 4;
        var d = new float[n]; float lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float w = Random.value * 2f - 1f;
            lp += (w - lp) * 0.35f;                                         // bruit adouci = pluie
            float drop = Random.value < 0.0015f ? (Random.value * 2f - 1f) * 0.6f : 0f;   // gouttes
            float fadeLoop = Mathf.Min(1f, Mathf.Min(i, n - i) / (rate * 0.05f));          // boucle sans clic
            d[i] = (lp * 0.5f + drop) * fadeLoop;
        }
        rainClip = AudioClip.Create("pluie", n, 1, rate, false);
        rainClip.SetData(d, 0);
        return rainClip;
    }

    static AudioClip ThunderClip()
    {
        if (thunderClip) return thunderClip;
        const int rate = 22050; int n = (int)(rate * 3.5f);
        var d = new float[n]; float lp = 0f, lp2 = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float w = Random.value * 2f - 1f;
            lp += (w - lp) * 0.04f; lp2 += (lp - lp2) * 0.08f;             // grondement grave
            float env = Mathf.Min(1f, t * 8f) * Mathf.Exp(-t * 1.1f) * (0.7f + 0.3f * Mathf.Sin(t * 9f));
            d[i] = Mathf.Clamp(lp2 * 9f * env, -1f, 1f);
        }
        thunderClip = AudioClip.Create("tonnerre", n, 1, rate, false);
        thunderClip.SetData(d, 0);
        return thunderClip;
    }
}
