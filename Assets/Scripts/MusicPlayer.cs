using UnityEngine;

// Musique de fond : les pistes s'enchaînent l'une après l'autre et recommencent au début, sans fin (1, 2, 3, 1, 2, 3...).
// Une seule source : quand une piste se termine, la suivante démarre. (L'ancienne version programmait les pistes à l'avance
// sur l'horloge audio ; après une pause du jeu cette horloge continuait et la musique se décalait ou s'arrêtait.)
// La musique ignore le volume général des effets (AudioListener) : seul le curseur Musique la règle (GameSettings).
// Ajouter une piste : déposer « <playlist>_N.mp3 » dans Assets/Audio/Music puis SAE501 > Mettre à jour les musiques
// (MusicLibrary) ; la liste « tracks » se remplit toute seule dans l'ordre des numéros. On peut aussi la remplir à la main.
public class MusicPlayer : MonoBehaviour
{
    [Tooltip("Début du nom des fichiers de la playlist dans Assets/Audio/Music (ex. Musique_Principale), utilisé par l'éditeur")]
    public string playlist;
    [Tooltip("Pistes jouées dans l'ordre, puis on recommence au début")]
    public AudioClip[] tracks;
    [Range(0f, 1f)] public float volume = 0.8f;
    public float startDelay = 0.5f;

    AudioSource src;
    int next;
    float startAt;

    void Start()
    {
        int count = 0;
        if (tracks != null) foreach (var t in tracks) if (t) count++;
        if (count == 0) { enabled = false; return; }
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;              // musique d'ambiance, pas de position dans l'espace
        src.priority = 0;                   // jamais coupée au profit d'un autre son
        src.ignoreListenerVolume = true;    // indépendante du curseur Effets
        src.loop = count == 1;              // une seule piste (menu) : boucle simple
        src.volume = volume;
        startAt = Time.unscaledTime + startDelay;
    }

    void Update()
    {
        if (!src) return;
        src.volume = volume;
        if (AudioListener.pause || Time.unscaledTime < startAt) return;     // menu pause : la musique attend
        if (src.isPlaying) return;
        for (int i = 0; i < tracks.Length; i++)                            // case vide dans la liste : on la saute
        {
            var clip = tracks[next];
            next = (next + 1) % tracks.Length;
            if (!clip) continue;
            src.clip = clip;
            src.Play();
            return;
        }
    }
}
