using UnityEngine;

// [4.4] Sons des barricades attaquées, générés par code (pas d'asset audio) :
//   Crack : coup sourd + craquement du bois ;  Pop : un clou qui saute (grincement aigu) ;  Break : la planche cède.
public static class BarricadeFx
{
    const int Rate = 44100;
    static AudioClip crack, pop, brk;

    static AudioClip Make(string name, float dur, System.Func<float, float> f)
    {
        int n = (int)(dur * Rate);
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(f(i / (float)Rate), -1f, 1f);
        var c = AudioClip.Create(name, n, 1, Rate, false);
        c.SetData(d, 0);
        return c;
    }

    public static AudioClip Crack => crack ? crack : crack = Make("barricade_crack", 0.25f, t =>
        Mathf.Sin(t * 95f * 6.28f) * Mathf.Exp(-t * 30f) * 0.8f                       // coup sourd
        + (Random.value * 2 - 1) * Mathf.Exp(-t * 35f) * 0.45f * (Mathf.Sin(t * 260f) > 0 ? 1f : 0.4f));  // craquement

    public static AudioClip Pop => pop ? pop : pop = Make("barricade_pop", 0.3f, t =>
        Mathf.Sin(t * (1400f - 2600f * t) * 6.28f) * Mathf.Exp(-t * 14f) * 0.35f       // grincement du clou qui sort
        + (Random.value * 2 - 1) * Mathf.Exp(-t * 60f) * 0.4f);

    public static AudioClip Break => brk ? brk : brk = Make("barricade_break", 0.5f, t =>
        Mathf.Sin(t * 70f * 6.28f) * Mathf.Exp(-t * 12f) * 0.7f
        + (Random.value * 2 - 1) * Mathf.Exp(-t * 9f) * 0.55f);

    public static void Play(AudioClip clip, Vector3 pos, float volume = 0.9f) => AudioSource.PlayClipAtPoint(clip, pos, volume);
}
