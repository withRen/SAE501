using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Combiné de la radio de ravitaillement (construit par RoomBuilder.BuildSupplyRadio, utilisé par SupplyRadio) :
//   1. on le décroche du socle (grip) — avec la prise par clic (ClickGrab), il reste en main sans garder le bouton ;
//   2. on le porte à l'oreille : quand l'écouteur reste près d'une oreille un court instant, `PutToEar` est déclenché ;
//   3. on le lâche (2e appui sur le grip) : il retourne tout seul sur son socle.
// Il est relié à la radio par un cordon : tiré trop loin, il échappe de la main et revient se raccrocher.
// Toujours cinématique (jamais de chute) : pas d'ItemSafety ni de place à la ceinture.
[RequireComponent(typeof(XRGrabInteractable))]
public class RadioHandset : MonoBehaviour
{
    public Transform earpiece;              // centre de l'écouteur
    public Transform cordAnchor;            // point de départ du cordon sur la radio
    public Transform cordEnd;               // point d'arrivée du cordon sur le combiné
    public LineRenderer cord;
    public float cordLength = 1.1f;         // au-delà (socle → combiné), le combiné échappe de la main
    public float earRadius = 0.14f;         // distance écouteur ↔ oreille pour « à l'oreille »
    public float earDwell = 0.35f;          // temps à garder à l'oreille avant que ça compte

    public event Action PutToEar;
    public event Action Lifted, HungUp;

    public bool IsHeld => grab && grab.isSelected;
    public bool AtEar { get; private set; }
    public Component Holder => grab ? grab.firstInteractorSelecting as Component : null;

    XRGrabInteractable grab;
    Transform head;
    Vector3 cradlePos;                      // pose de repos, dans le repère du parent (la radio)
    Quaternion cradleRot;
    float earTime;
    bool armed = true;                      // il faut écarter le combiné de l'oreille avant un nouvel appel
    Coroutine returning;
    AudioClip clack;

    const int CordPoints = 20;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        cradlePos = transform.localPosition;
        cradleRot = transform.localRotation;
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
        if (cord) { cord.positionCount = CordPoints; cord.useWorldSpace = true; }
    }

    void Start()
    {
        clack = ProcAudio.Clack();
        UpdateCord();
    }

    void OnDestroy()
    {
        if (!grab) return;
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs a)
    {
        if (returning != null) { StopCoroutine(returning); returning = null; }
        earTime = 0f; armed = true;
        Lifted?.Invoke();
    }

    void OnRelease(SelectExitEventArgs a)
    {
        AtEar = false; earTime = 0f;
        if (isActiveAndEnabled) returning = StartCoroutine(ReturnToCradle());
        HungUp?.Invoke();
    }

    void Update()
    {
        if (!IsHeld) return;

        // cordon trop tendu : le combiné échappe de la main (et revient sur le socle via OnRelease)
        if (transform.parent && Vector3.Distance(transform.parent.TransformPoint(cradlePos), transform.position) > cordLength)
        {
            if (grab.interactionManager) grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);
            return;
        }

        if (!head && Camera.main) head = Camera.main.transform;
        if (!head || !earpiece) return;

        // deux oreilles approximatives : de part et d'autre de la tête, un peu plus bas que les yeux
        var e = earpiece.position;
        var down = -head.up * 0.03f;
        var l = head.position - head.right * 0.08f + down;
        var r = head.position + head.right * 0.08f + down;
        bool near = Mathf.Min((e - l).sqrMagnitude, (e - r).sqrMagnitude) < earRadius * earRadius;

        if (!near) { AtEar = false; earTime = 0f; armed = true; return; }
        earTime += Time.deltaTime;
        if (earTime < earDwell) return;
        AtEar = true;
        if (armed) { armed = false; PutToEar?.Invoke(); }
    }

    void LateUpdate() => UpdateCord();

    IEnumerator ReturnToCradle()
    {
        Vector3 p0 = transform.localPosition;
        Quaternion r0 = transform.localRotation;
        const float dur = 0.3f;
        for (float t = 0f; t < dur; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / dur);
            transform.localPosition = Vector3.Lerp(p0, cradlePos, k);
            transform.localRotation = Quaternion.Slerp(r0, cradleRot, k);
            yield return null;
        }
        transform.localPosition = cradlePos;
        transform.localRotation = cradleRot;
        AudioSource.PlayClipAtPoint(clack, transform.position, 0.6f);     // raccroché
        returning = null;
    }

    // cordon : courbe qui pend entre la radio et le combiné, de moins en moins à mesure qu'il se tend
    void UpdateCord()
    {
        if (!cord || !cordAnchor || !cordEnd) return;
        Vector3 a = cordAnchor.position, b = cordEnd.position;
        float span = Vector3.Distance(a, b);
        float sag = Mathf.Lerp(0.18f, 0.01f, Mathf.Clamp01(span / cordLength)) + 0.04f;
        var mid = (a + b) * 0.5f + Vector3.down * sag;
        for (int i = 0; i < CordPoints; i++)
        {
            float t = i / (CordPoints - 1f);
            var p = Vector3.Lerp(Vector3.Lerp(a, mid, t), Vector3.Lerp(mid, b, t), t);    // courbe de Bézier quadratique
            cord.SetPosition(i, p);
        }
    }
}
