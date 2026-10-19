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
    // clic de culasse vide : un « tic » métallique sec (corps grave + résonance aiguë qui s'éteint vite), suivi d'un rebond plus faible
    public static AudioClip DryFire() => Make("dryfire", 0.09f, t =>
    {
        float tick = (Mathf.Sin(t * 1900f * 6.28f) * 0.55f + Mathf.Sin(t * 3300f * 6.28f) * 0.3f) * Mathf.Exp(-t * 95f);
        float click = (Random.value * 2 - 1) * Mathf.Exp(-t * 400f) * 0.5f;
        float t2 = t - 0.045f;
        float rebound = t2 > 0f ? Mathf.Sin(t2 * 2400f * 6.28f) * Mathf.Exp(-t2 * 130f) * 0.25f : 0f;
        return tick + click + rebound;
    });
    // [#102] alarme de début de vague : sirène à deux tons (3 cycles), non spatialisée (WaveManager)
    public static AudioClip Alarm() => Make("alarme", 2.4f, t =>
    {
        float f = (t % 0.8f) < 0.4f ? 660f : 880f;
        float env = Mathf.Clamp01(t / 0.03f) * Mathf.Clamp01((2.4f - t) / 0.2f);
        float sq = Mathf.Sign(Mathf.Sin(t * f * 6.2832f)) * 0.25f + Mathf.Sin(t * f * 6.2832f) * 0.25f;
        return sq * env * 0.8f;
    });
    // [#104] pas d'un zombie sur le toit du bunker : coup sourd et grave, avec un peu de gravier
    public static AudioClip RoofStep() => Make("pas_toit", 0.22f, t =>
        Mathf.Sin(t * 70f * 6.2832f) * Mathf.Exp(-t * 25f) * 0.8f + (Random.value * 2 - 1) * Mathf.Exp(-t * 40f) * 0.25f);
    // [#104] poussière et gravats qui tombent du trou du toit : ruissellement (bruit filtré) avec des petits chocs
    public static AudioClip DustFall()
    {
        float lp = 0f;
        return Make("poussiere", 3.5f, t =>
        {
            lp += ((Random.value * 2 - 1) - lp) * 0.08f;                     // bruit adouci (passe-bas)
            float env = Mathf.Clamp01(t / 0.4f) * Mathf.Clamp01((3.5f - t) / 0.8f);
            float grit = Random.value < 0.0006f ? (Random.value * 2 - 1) * 0.9f : 0f;
            return (lp * 1.6f + grit) * env * 0.7f;
        });
    }
    public static AudioClip Hit() => Make("hit", 0.12f, t => Mathf.Sin(t * 900f * 6.28f) * Mathf.Exp(-t * 45f) * 0.6f + (Random.value * 2 - 1) * Mathf.Exp(-t * 80f) * 0.4f);
}
