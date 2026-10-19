using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Une ouverture du bunker (porte ou fenêtre) : ses 3 emplacements de planches et les points d'approche des zombies.
public class Opening : MonoBehaviour
{
    public static readonly List<Opening> All = new List<Opening>();

    public string openingName;
    public BarricadeSlot[] slots;
    public Transform approachPoint;     // devant l'ouverture, côté extérieur : le zombie s'y arrête pour arracher les planches
    public Transform insidePoint;       // juste derrière l'ouverture, côté bunker
    public bool zombiesCanUse = true;   // faux tant que l'extérieur n'est pas accessible (fenêtres)
    public HingedDoor door;             // porte du bunker devant les planches (portes ouest / est) : le zombie l'enfonce d'abord

    [Header("Trou du toit [#104]")]
    public bool fromAbove;              // trou du toit : le zombie prévient avant de se laisser tomber (Zombie, état Lurking)
    public ParticleSystem dust;         // poussière qui tombe du trou, dans le bunker
    AudioSource dustSrc;
    static AudioClip dustClip;

    // un zombie va tomber par ce trou dans `seconds` : poussière et gravats qui tombent, bruit de ruissellement
    public void WarnFromAbove(float seconds)
    {
        if (dust)
        {
            var main = dust.main;
            main.duration = Mathf.Max(0.5f, seconds);
            dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dust.Play(true);
        }
        if (!dustSrc)
        {
            dustSrc = (dust ? dust.gameObject : gameObject).AddComponent<AudioSource>();
            dustSrc.spatialBlend = 1f; dustSrc.minDistance = 1.5f; dustSrc.maxDistance = 14f; dustSrc.playOnAwake = false;
        }
        if (!dustClip) dustClip = ProcAudio.DustFall();
        dustSrc.PlayOneShot(dustClip, 1f);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => All.Clear();

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public int PlankCount => slots.Count(s => s.Plank != null && s.Plank.InSlot);      // posées (clouées ou non)
    public bool IsBarricaded => PlankCount > 0;
    public bool DoorClosed => door != null && door.Closed;

    // planche à arracher : la plus haute
    public Plank NextPlankToBreak()
    {
        return slots.Where(s => s.Plank != null && s.Plank.InSlot)
                    .OrderByDescending(s => s.transform.position.y)
                    .Select(s => s.Plank)
                    .FirstOrDefault();
    }
}
