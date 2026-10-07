using System.Collections;
using UnityEngine;

// Musique de fond : les pistes s'enchaînent l'une après l'autre, en boucle (1, 2, 1, 2...).
// Deux AudioSource se relaient et chaque piste suivante est programmée à l'échantillon près (PlayScheduled) :
// pas de blanc entre deux pistes, et le décompte suit l'horloge audio (il s'arrête si l'appli est mise en pause).
//
// Piège évité : le projet a « Enable Output Suspension » (Audio Manager). Quand aucun son ne joue, Unity coupe la sortie audio
// ET FIGE l'horloge audio (AudioSettings.dspTime). Un PlayScheduled posé dans cet état n'arrive jamais : la musique ne démarrait pas.
// La première piste est donc lancée par un Play() normal, qui réveille l'audio ; les suivantes sont programmées ensuite,
// quand l'horloge avance (la musique joue en continu, la sortie ne se rendort plus).
public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] tracks;
    [Range(0f, 1f)] public float volume = 0.35f;
    public float startDelay = 0.5f;

    readonly AudioSource[] sources = new AudioSource[2];
    int nextTrack, nextSource;
    double nextStart;
    bool synced;

    IEnumerator Start()
    {
        if (tracks == null || tracks.Length == 0) { enabled = false; yield break; }
        for (int i = 0; i < sources.Length; i++)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = false;
            s.spatialBlend = 0f;        // musique d'ambiance, pas de position dans l'espace
            s.volume = volume;
            s.priority = 0;             // jamais coupée au profit d'un autre son
            sources[i] = s;
        }
        yield return new WaitForSeconds(startDelay);

        var first = sources[0];
        first.clip = tracks[0];
        first.Play();                   // Play() normal : réveille l'audio même si la sortie était suspendue
        nextTrack = 1 % tracks.Length;
        nextSource = 1;
    }

    void Update()
    {
        foreach (var s in sources) if (s) s.volume = volume;
        if (sources[0] == null || sources[0].clip == null) return;

        // 1re image où la piste joue vraiment : on calcule précisément l'instant où elle finira
        if (!synced)
        {
            var s0 = sources[0];
            if (s0.isPlaying && s0.timeSamples > 0)
            {
                nextStart = AudioSettings.dspTime + (double)(s0.clip.samples - s0.timeSamples) / s0.clip.frequency;
                synced = true;
            }
            return;
        }

        // une seconde avant la fin de la piste en cours, on programme la suivante sur l'autre source
        if (AudioSettings.dspTime > nextStart - 1.0) ScheduleNext();
    }

    void ScheduleNext()
    {
        var clip = tracks[nextTrack];
        var src = sources[nextSource];
        src.clip = clip;
        src.PlayScheduled(nextStart);
        nextStart += (double)clip.samples / clip.frequency;
        nextTrack = (nextTrack + 1) % tracks.Length;
        nextSource = 1 - nextSource;
    }
}
