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

    [Header("HUD munitions")]
    public Color hudNormal = Color.white;
    public Color hudLow = new Color(1f, 0.6f, 0.1f);
    public Color hudEmpty = new Color(1f, 0.15f, 0.1f);
    public int lowThreshold = 3;

    MeshRenderer hudRenderer;

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
        if (hud) hudRenderer = hud.GetComponent<MeshRenderer>();
        UpdateHud();
    }

    // Le compteur n'est visible que quand le pistolet est en main (pas dans le holster).
    void Update()
    {
        if (!hud) return;
        bool held = BeltItem.InHand(grab);
        if (hudRenderer) hudRenderer.enabled = held;
        if (!held) return;

        // chargeur vide : clignote en rouge
        if (ammo <= 0) hud.color = Mathf.Repeat(Time.time, 0.6f) < 0.3f ? hudEmpty : hudEmpty * 0.35f;
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
        if (!hud) return;
        hud.text = ammo.ToString("00") + "/" + maxAmmo.ToString("00");
        hud.color = ammo <= 0 ? hudEmpty : (ammo <= lowThreshold ? hudLow : hudNormal);
    }
}
