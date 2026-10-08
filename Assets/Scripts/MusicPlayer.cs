using UnityEngine;

// Musique de fond : les pistes s'enchaînent l'une après l'autre et recommencent au début, sans fin (1, 2, 1, 2...).
// Une seule source : quand une piste se termine, la suivante démarre. (L'ancienne version programmait les pistes à l'avance
// sur l'horloge audio ; après une pause du jeu cette horloge continuait et la musique se décalait ou s'arrêtait.)
// La musique ignore le volume général des effets (AudioListener) : seul le curseur Musique la règle (GameSettings).
public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] tracks;
    [Range(0f, 1f)] public float volume = 0.8f;
    public float startDelay = 0.5f;

    AudioSource src;
    int next;
    float startAt;

    void Start()
    {
        if (tracks == null || tracks.Length == 0) { enabled = false; return; }
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;              // musique d'ambiance, pas de position dans l'espace
        src.priority = 0;                   // jamais coupée au profit d'un autre son
        src.ignoreListenerVolume = true;    // indépendante du curseur Effets
        src.loop = tracks.Length == 1;      // une seule piste (menu) : boucle simple
        src.volume = volume;
        startAt = Time.unscaledTime + startDelay;
    }

    void Update()
    {
        if (!src) return;
        src.volume = volume;
        if (AudioListener.pause || Time.unscaledTime < startAt) return;     // menu pause : la musique attend
        if (src.isPlaying) return;
        src.clip = tracks[next];
        src.Play();
        next = (next + 1) % tracks.Length;
    }
}
