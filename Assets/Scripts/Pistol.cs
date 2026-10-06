using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pistol : MonoBehaviour
{
    public int ammo = 12;
    public int maxAmmo = 12;
    public Transform muzzle;
    public Light flash;
    public TextMesh hud;
    public float range = 60f;

    XRGrabInteractable grab;
    AudioSource src;
    static AudioClip shot, click;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.activated.AddListener(OnActivate);
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        if (!shot) { shot = ProcAudio.Shot(); click = ProcAudio.Click(); }
        if (flash) flash.enabled = false;
        UpdateHud();
    }

    void OnActivate(ActivateEventArgs a)
    {
        if (!BeltItem.InHand(grab)) return;
        if (ammo <= 0) { src.PlayOneShot(click); return; }

        ammo--;
        UpdateHud();
        src.PlayOneShot(shot);
        StartCoroutine(Flash());

        if (Physics.Raycast(muzzle.position, muzzle.forward, out var hit, range, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.rigidbody) hit.rigidbody.AddForceAtPosition(muzzle.forward * 6f, hit.point, ForceMode.Impulse);
            hit.collider.SendMessage("OnShot", hit, SendMessageOptions.DontRequireReceiver);
        }
    }

    IEnumerator Flash()
    {
        if (!flash) yield break;
        flash.enabled = true;
        yield return new WaitForSeconds(0.04f);
        flash.enabled = false;
    }

    public void Load(int rounds)
    {
        ammo = Mathf.Min(rounds, maxAmmo);
        src.PlayOneShot(click);
        UpdateHud();
    }

    void UpdateHud()
    {
        if (hud) hud.text = ammo.ToString("00");
    }
}

public class Magazine : MonoBehaviour
{
    public int rounds = 12;

    public void Consume()
    {
        var g = GetComponent<XRGrabInteractable>();
        if (g && g.interactionManager) g.interactionManager.CancelInteractableSelection((IXRSelectInteractable)g);
        Destroy(gameObject);
    }
}

// Emplacement du chargeur sur le pistolet
public class MagWell : XRSocketInteractor
{
    public Pistol pistol;

    static Magazine MagOf(object o)
    {
        var c = o as Component;
        return c ? c.GetComponent<Magazine>() : null;
    }

    protected override void Awake()
    {
        base.Awake();
        selectEntered.AddListener(OnIn);
    }

    void OnIn(SelectEnterEventArgs a)
    {
        var mag = MagOf(a.interactableObject);
        if (mag) StartCoroutine(Insert(mag));
    }

    IEnumerator Insert(Magazine mag)
    {
        yield return null;
        pistol.Load(mag.rounds);
        mag.Consume();
    }

    public override bool CanHover(IXRHoverInteractable interactable) => base.CanHover(interactable) && MagOf(interactable);
    public override bool CanSelect(IXRSelectInteractable interactable) => base.CanSelect(interactable) && MagOf(interactable);
}
