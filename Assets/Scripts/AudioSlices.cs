using System.Collections.Generic;
using UnityEngine;

// Outils pour les sons du jeu :
//  - Clip(assigné, "nom")  : le clip réglé dans l'inspecteur, sinon celui de la banque GameSounds (posée une fois dans la scène),
//    sinon celui du dossier Assets/Resources/Sounds/<nom> : aucune affectation à faire sur chaque prefab ;
//  - Split(clip)           : une bande son qui contient PLUSIEURS sons (wood_hit, Wood_hammer_nail) est découpée toute seule
//    en un clip par son, d'après les silences : chaque frappe devient une variante tirée au hasard ;
//  - Trim(clip, secondes)  : ne garde que le début d'un clip (wood_break : seules les premières secondes contiennent du son).
// Les découpes se font une seule fois (puis sont mises en cache). Réglage d'import requis pour Split/Trim :
// le clip doit être en « Load Type = Decompress On Load » (réglage par défaut d'Unity pour les sons courts).
// Si la découpe est impossible, les méthodes renvoient null : l'appelant garde alors son ancien son procédural.
public static class AudioSlices
{
    static readonly Dictionary<string, AudioClip> byName = new Dictionary<string, AudioClip>();
    static readonly Dictionary<AudioClip, AudioClip[]> splits = new Dictionary<AudioClip, AudioClip[]>();
    static readonly Dictionary<AudioClip, AudioClip> trims = new Dictionary<AudioClip, AudioClip>();
    static readonly HashSet<AudioClip> failed = new HashSet<AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { byName.Clear(); splits.Clear(); trims.Clear(); failed.Clear(); }

    /// <summary>Le clip de l'inspecteur, sinon celui de la banque GameSounds de la scène, sinon Resources/Sounds/&lt;resourceName&gt;.</summary>
    public static AudioClip Clip(AudioClip assigned, string resourceName)
    {
        if (assigned) return assigned;
        if (GameSounds.Instance)
        {
            var banked = GameSounds.Instance.Get(resourceName);
            if (banked) return banked;
        }
        if (!byName.TryGetValue(resourceName, out var c))
        {
            c = Resources.Load<AudioClip>("Sounds/" + resourceName);
            byName[resourceName] = c;
        }
        return c;
    }

    /// <summary>Indice au hasard, différent du précédent (pour ne jamais répéter deux fois la même variante).</summary>
    public static int Pick(int count, ref int last)
    {
        if (count <= 1) return 0;
        int i;
        do { i = Random.Range(0, count); } while (i == last);
        last = i;
        return i;
    }

    /// <summary>Découpe une bande son en sons séparés. onset/end : seuils relatifs au pic ; minGap : silence minimal entre deux sons.</summary>
    public static AudioClip[] Split(AudioClip src, float onset = 0.08f, float end = 0.02f, float minGap = 0.12f, float maxLength = 1.2f, float peakVolume = 0.9f)
    {
        if (!src || failed.Contains(src)) return null;
        if (splits.TryGetValue(src, out var done)) return done;
        if (!Read(src, out var d)) return null;

        int ch = src.channels, rate = src.frequency, frames = d.Length / ch;
        const float win = 0.005f;
        int w = Mathf.Max(1, (int)(rate * win)), k = frames / w;
        var e = new float[k];
        float peak = 1e-6f;
        for (int i = 0; i < k; i++)
        {
            double sum = 0;
            for (int f = i * w; f < (i + 1) * w; f++)
            {
                float m = 0f;
                for (int c = 0; c < ch; c++) m += d[f * ch + c];
                m /= ch;
                sum += m * m;
            }
            e[i] = Mathf.Sqrt((float)(sum / w));
            if (e[i] > peak) peak = e[i];
        }

        var list = new List<AudioClip>();
        int quietNeeded = Mathf.CeilToInt(0.04f / win), gapW = Mathf.CeilToInt(minGap / win), maxW = Mathf.CeilToInt(maxLength / win);
        int lastEnd = -gapW;
        for (int i = 0; i < k;)
        {
            if (e[i] > peak * onset && i - lastEnd > gapW)
            {
                int s = i, j = i, quiet = 0;
                while (j < k)
                {
                    quiet = e[j] < peak * end ? quiet + 1 : 0;      // la fin du son : 40 ms de quasi-silence
                    if (quiet >= quietNeeded || j - s > maxW) break;
                    j++;
                }
                int a = Mathf.Max(0, (int)((s * win - 0.012f) * rate));
                int b = Mathf.Min(frames, (int)((j * win + 0.03f) * rate));
                list.Add(Cut(src.name + "_" + list.Count, d, ch, rate, a, b, 0.003f, 0.02f, peakVolume));
                lastEnd = j;
                i = j;
            }
            else i++;
        }
        var arr = list.Count > 0 ? list.ToArray() : null;
        splits[src] = arr;
        return arr;
    }

    /// <summary>Ne garde que les `seconds` premières secondes (avec un fondu de sortie).</summary>
    public static AudioClip Trim(AudioClip src, float seconds, float fadeOut = 0.5f, float peakVolume = 0.95f)
    {
        if (!src || failed.Contains(src)) return null;
        if (trims.TryGetValue(src, out var done)) return done;
        if (!Read(src, out var d)) return null;
        int ch = src.channels, rate = src.frequency, frames = d.Length / ch;
        var clip = Cut(src.name + "_debut", d, ch, rate, 0, Mathf.Min(frames, (int)(seconds * rate)), 0f, fadeOut, peakVolume);
        trims[src] = clip;
        return clip;
    }

    static bool Read(AudioClip c, out float[] d)
    {
        d = null;
        if (c.loadType != AudioClipLoadType.DecompressOnLoad)
        {
            if (failed.Add(c)) Debug.LogWarning("AudioSlices : « " + c.name + " » doit être importé en Load Type = Decompress On Load pour être découpé (son procédural utilisé à la place).");
            return false;
        }
        if (c.loadState == AudioDataLoadState.Unloaded) c.LoadAudioData();
        if (c.loadState != AudioDataLoadState.Loaded) return false;       // encore en chargement : on réessaiera au prochain son
        d = new float[c.samples * c.channels];
        if (!c.GetData(d, 0)) { failed.Add(c); d = null; return false; }
        return true;
    }

    // copie [a, b) images du tableau, avec fondus et volume du pic normalisé
    static AudioClip Cut(string name, float[] d, int ch, int rate, int a, int b, float fadeIn, float fadeOut, float peakVolume)
    {
        int n = Mathf.Max(1, b - a);
        var o = new float[n * ch];
        System.Array.Copy(d, a * ch, o, 0, n * ch);
        float max = 1e-6f;
        for (int i = 0; i < o.Length; i++) { float v = Mathf.Abs(o[i]); if (v > max) max = v; }
        float g = peakVolume / max;
        int fi = Mathf.Clamp((int)(fadeIn * rate), 0, n), fo = Mathf.Clamp((int)(fadeOut * rate), 0, n);
        for (int f = 0; f < n; f++)
        {
            float k = g;
            if (fi > 0 && f < fi) k *= f / (float)fi;
            if (fo > 0 && f >= n - fo) k *= (n - 1 - f) / (float)fo;
            for (int c = 0; c < ch; c++) o[f * ch + c] *= k;
        }
        var clip = AudioClip.Create(name, n, ch, rate, false);
        clip.SetData(o, 0);
        return clip;
    }
}
