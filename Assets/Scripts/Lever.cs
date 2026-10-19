using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// [#111] Levier physique (générateur, disjoncteur) : sur l'objet pivot, la poignée en enfant.
// On attrape la boule (grip, prise par clic : ClickGrab la laisse tenue) et on ABAISSE la main : le levier suit la main
// autour de son axe (x local du pivot). Haut = upAngle, bas = downAngle ; la poignée part vers l'avant (z local, côté joueur).
// - springBack : levier à ressort (générateur) : lâché, il remonte tout seul ; il faut le tenir en bas.
// - sinon (disjoncteur) : lâché, il bascule vers la butée la plus proche et y reste.
// La main trop loin de la boule (> releaseDistance) lâche le levier, comme la poignée des portes (HingedDoor).
public class Lever : MonoBehaviour
{
    public XRSimpleInteractable handle;     // boule au bout de la poignée
    public float upAngle = -50f;            // angle autour de x local : négatif = poignée levée
    public float downAngle = 50f;           // positif = poignée abaissée
    public bool springBack;
    public float returnSpeed = 160f;        // °/s : retour à la butée quand on lâche
    public float releaseDistance = 0.5f;
    public bool locked;                     // bloqué où il est (générateur démarré)

    public float Angle { get; private set; }
    public float Down01 => Mathf.InverseLerp(upAngle, downAngle, Angle);   // 0 = en haut, 1 = en bas
    public bool IsDown => Down01 >= 0.9f;
    public bool IsUp => Down01 <= 0.1f;
    public bool Held => handle != null && handle.isSelected;

    Quaternion rest;
    AudioSource src;
    static AudioClip clack;
    bool wasDown, wasUp = true;

    void Awake()
    {
        rest = transform.localRotation;
        Angle = upAngle;
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f; src.playOnAwake = false;
        if (!clack) clack = ProcAudio.DryFire();
        Apply();
    }

    // place le levier sans animation (état de départ)
    public void Snap(bool down) { Angle = down ? downAngle : upAngle; wasDown = down; wasUp = !down; Apply(); }

    void Update()
    {
        if (handle) handle.enabled = !locked;
        if (Held && !locked)
        {
            var hand = handle.firstInteractorSelecting.GetAttachTransform(handle).position;
            if (Vector3.Distance(hand, handle.transform.position) > releaseDistance)
            {
                handle.interactionManager.SelectExit(handle.firstInteractorSelecting, handle);
            }
            else
            {
                // angle de la main autour de l'axe, mesuré depuis la position de repos (poignée horizontale, vers l'avant)
                var restWorld = (transform.parent ? transform.parent.rotation : Quaternion.identity) * rest;
                var axis = restWorld * Vector3.right;
                var v = Vector3.ProjectOnPlane(hand - transform.position, axis);
                if (v.sqrMagnitude > 0.0004f)
                    Angle = Mathf.Clamp(Vector3.SignedAngle(restWorld * Vector3.forward, v, axis), Mathf.Min(upAngle, downAngle), Mathf.Max(upAngle, downAngle));
            }
        }
        else if (!locked)
        {
            float target = springBack || Down01 < 0.5f ? upAngle : downAngle;
            Angle = Mathf.MoveTowards(Angle, target, returnSpeed * Time.deltaTime);
        }
        Apply();

        // clac aux butées
        if (IsDown && !wasDown) Clack(0.7f);
        if (IsUp && !wasUp) Clack(1f);
        wasDown = IsDown; wasUp = IsUp;
    }

    void Clack(float pitch)
    {
        if (!src || Time.timeSinceLevelLoad < 0.5f) return;
        src.pitch = pitch;
        src.PlayOneShot(clack, 1f);
    }

    void Apply() => transform.localRotation = rest * Quaternion.Euler(Angle, 0f, 0f);
}
