using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Croix « passer le tutoriel », en haut à droite des panneaux du tutoriel.
// On la vise avec une manette dont la main est VIDE (un rayon apparaît seulement quand on vise la croix) et on appuie sur
// la gâchette. Une main qui tient un objet est ignorée : la gâchette sert alors à tirer ou à allumer la lampe.
// Sans manettes (éditeur sans casque) : on vise avec le regard et on appuie sur Espace (ou la gâchette).
// La zone visée est un carré de `half` (en unités locales de cet objet) autour de lui, dans son plan.
public class TutorialSkipCross : MonoBehaviour
{
    public float half = 30f;                 // demi-côté de la zone visée, en unités locales
    public float maxDistance = 8f;
    public Action onClick;
    public Action<bool> onHover;

    static Transform left, right, head;
    static XRDirectInteractor leftHand, rightHand;
    static LineRenderer ray;
    static Material rayMat;
    static TutorialSkipCross rayOwner;          // plusieurs croix peuvent exister : le rayon appartient à celle qui est visée

    bool hovered;
    bool downL = true, downR = true, downHead = true;     // un appui commencé avant l'apparition ne compte pas

    void OnEnable() => Find();

    static void Find()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        if (left && right) return;
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!left && t.name == "Left Controller") left = t;
            if (!right && t.name == "Right Controller") right = t;
        }
        if (left) leftHand = left.GetComponentInChildren<XRDirectInteractor>(true);
        if (right) rightHand = right.GetComponentInChildren<XRDirectInteractor>(true);
    }

    void Update()
    {
        if (!head) Find();
        bool hands = (left && left.gameObject.activeInHierarchy) || (right && right.gameObject.activeInHierarchy);
        bool aimed = false;
        Vector3 from = Vector3.zero, to = Vector3.zero;

        if (hands)
        {
            aimed |= Hand(left, leftHand, XRNode.LeftHand, ref downL, ref from, ref to);
            aimed |= Hand(right, rightHand, XRNode.RightHand, ref downR, ref from, ref to);
        }
        else if (head)
        {
            bool hit = Hit(new Ray(head.position, head.forward), out _);
            bool d = VRInput.AnyTrigger() || VRInput.KeyboardSpace();
            if (hit && d && !downHead) { onClick?.Invoke(); return; }
            downHead = d;
            aimed = hit;
        }

        if (aimed != hovered) { hovered = aimed; onHover?.Invoke(aimed); }
        bool show = aimed && hands;
        if (show) rayOwner = this;
        if (rayOwner == this) Line(show, from, to);
    }

    bool Hand(Transform hand, XRDirectInteractor it, XRNode node, ref bool wasDown, ref Vector3 from, ref Vector3 to)
    {
        bool down = VRInput.Trigger(node);
        bool free = hand && hand.gameObject.activeInHierarchy && !(it && it.hasSelection);
        bool hit = false;
        if (free)
        {
            var r = new Ray(hand.position, hand.forward);
            hit = Hit(r, out float dist);
            if (hit)
            {
                from = r.origin; to = r.origin + r.direction * dist;
                if (!hovered) VRInput.Haptic(node, 0.2f, 0.03f);
                if (down && !wasDown) { VRInput.Haptic(node, 0.5f, 0.06f); wasDown = true; onClick?.Invoke(); return true; }
            }
        }
        wasDown = down;
        return hit;
    }

    // le rayon coupe-t-il le carré de la croix ?
    public bool Hit(Ray r, out float dist)
    {
        dist = 0f;
        var plane = new Plane(transform.forward, transform.position);
        if (!plane.Raycast(r, out dist) || dist > maxDistance) return false;
        var local = transform.InverseTransformPoint(r.GetPoint(dist));
        return Mathf.Abs(local.x) <= half && Mathf.Abs(local.y) <= half;
    }

    static void Line(bool on, Vector3 from, Vector3 to)
    {
        if (!ray)
        {
            if (!on) return;
            var g = new GameObject("Tutoriel_Rayon_Croix");
            ray = g.AddComponent<LineRenderer>();
            if (!rayMat) { rayMat = new Material(Shader.Find("Universal Render Pipeline/Unlit")); rayMat.SetColor("_BaseColor", new Color(1f, 0.75f, 0.27f)); }
            ray.sharedMaterial = rayMat;
            ray.widthCurve = new AnimationCurve(new Keyframe(0, 0.004f), new Keyframe(1, 0.002f));
            ray.positionCount = 2;
            ray.useWorldSpace = true;
            ray.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        ray.enabled = on;
        if (on) { ray.SetPosition(0, from); ray.SetPosition(1, to); }
    }

    void OnDisable()
    {
        if (hovered) { hovered = false; onHover?.Invoke(false); }
        if (ray && rayOwner == this) ray.enabled = false;
    }
}
