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
    // [#113] balle dans la chair : impact mat et grave, un peu « mouillé » (bruit adouci), sans résonance métallique
    public static AudioClip Flesh()
    {
        float lp = 0f;
        return Make("chair", 0.18f, t =>
        {
            lp += ((Random.value * 2 - 1) - lp) * 0.25f;
            return lp * Mathf.Exp(-t * 30f) * 1.4f + Mathf.Sin(t * 95f * 6.2832f) * Mathf.Exp(-t * 35f) * 0.7f;
        });
    }
    // [#113] tir à la tête : craquement sec (os) par-dessus l'impact de chair
    public static AudioClip HeadCrack() => Make("tete", 0.22f, t =>
    {
        float crack = (Random.value * 2 - 1) * Mathf.Exp(-t * 70f) * 0.8f;
        float t2 = t - 0.018f;
        float crack2 = t2 > 0f ? (Random.value * 2 - 1) * Mathf.Exp(-t2 * 90f) * 0.5f : 0f;
        return crack + crack2 + Mathf.Sin(t * 70f * 6.2832f) * Mathf.Exp(-t * 18f) * 0.8f;
    });
    // [#114] planche qui se cale sur l'ouverture : « clac » de bois sec (deux tons courts)
    public static AudioClip Clack() => Make("clac", 0.09f, t =>
        (Mathf.Sin(t * 520f * 6.2832f) * 0.6f + Mathf.Sin(t * 1240f * 6.2832f) * 0.3f) * Mathf.Exp(-t * 75f)
        + (Random.value * 2 - 1) * Mathf.Exp(-t * 300f) * 0.5f);
    // [#116] planche arrachée : craquement de bois (série de petits claquements serrés) puis le choc grave
    public static AudioClip WoodCrack() => Make("craquement", 0.6f, t =>
    {
        float snaps = t < 0.25f && Random.value < 0.02f * (1f - t * 4f) ? (Random.value * 2 - 1) : 0f;
        float noise = (Random.value * 2 - 1) * Mathf.Exp(-t * 14f) * 0.35f;
        float body = Mathf.Sin(t * 140f * 6.2832f) * Mathf.Exp(-t * 12f) * 0.5f;
        return snaps * 0.9f + noise + body;
    });
    // radio de ravitaillement : deux bips montants sur un fond de grésillement
    public static AudioClip RadioChirp() => Make("radio_chirp", 0.7f, t =>
    {
        float noise = (Random.value * 2 - 1) * 0.18f * (0.6f + 0.4f * Mathf.Sin(t * 37f));
        float beep = 0f;
        if (t > 0.15f && t < 0.27f) beep = Mathf.Sin(t * 1200f * 6.28f) * 0.45f;
        if (t > 0.35f && t < 0.47f) beep = Mathf.Sin(t * 1600f * 6.28f) * 0.45f;
        return noise * Mathf.Clamp01(1.4f - t * 1.8f) + beep;
    });
    // souffle qui monte puis retombe : la caisse arrive par le toit
    public static AudioClip Whoosh() => Make("whoosh", 1.2f, t =>
        (Random.value * 2 - 1) * 0.6f * Mathf.Sin(Mathf.Clamp01(t / 1.2f) * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(t * 18f)));
    // caisse qui tombe sur le sol : coup grave et sourd
    public static AudioClip Thud() => Make("caisse", 0.5f, t =>
        Mathf.Sin(t * 70f * 6.28f) * Mathf.Exp(-t * 9f) * 0.9f + (Random.value * 2 - 1) * Mathf.Exp(-t * 35f) * 0.5f);
    public static AudioClip Hit() => Make("hit", 0.12f, t => Mathf.Sin(t * 900f * 6.28f) * Mathf.Exp(-t * 45f) * 0.6f + (Random.value * 2 - 1) * Mathf.Exp(-t * 80f) * 0.4f);
}
