using UnityEngine;

// Petits sons générés par code (pas d'assets audio pour l'instant)
public static class ProcAudio
{
    const int Rate = 44100;

    static AudioClip Make(string name, float dur, System.Func<float, float> f)
    {
        int n = (int)(dur * Rate);
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(f(i / (float)Rate), -1f, 1f);
        var c = AudioClip.Create(name, n, 1, Rate, false);
        c.SetData(d, 0);
        return c;
    }

    public static AudioClip Shot() => Make("shot", 0.25f, t => (Random.value * 2 - 1) * Mathf.Exp(-t * 22f) * 0.9f + Mathf.Sin(t * 120f * 6.28f) * Mathf.Exp(-t * 18f) * 0.5f);
    public static AudioClip Click() => Make("click", 0.04f, t => (Random.value * 2 - 1) * Mathf.Exp(-t * 120f) * 0.5f);
    public static AudioClip Hit() => Make("hit", 0.12f, t => Mathf.Sin(t * 900f * 6.28f) * Mathf.Exp(-t * 45f) * 0.6f + (Random.value * 2 - 1) * Mathf.Exp(-t * 80f) * 0.4f);
}
