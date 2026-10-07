using UnityEngine;
using UnityEngine.XR;

// Pointeur du menu : un rayon part de chaque manette ; le bouton visé s'allume (petite vibration),
// la gâchette de CETTE main le valide. Sans manettes (éditeur) : le regard sert de rayon, barre espace = gâchette.
public class MenuPointer : MonoBehaviour
{
    public Transform left, right, head;
    public float maxDistance = 6f;
    public Color color = UiKit.Amber;

    LineRenderer lineL, lineR;
    MenuButton hovL, hovR, hovHead;
    bool downL = true, downR = true, downHead = true;

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;
        foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (!left && t.name == "Left Controller") left = t;
            if (!right && t.name == "Right Controller") right = t;
        }
        if (left) lineL = MakeLine("Rayon_Gauche");
        if (right) lineR = MakeLine("Rayon_Droit");
    }

    LineRenderer MakeLine(string name)
    {
        var g = new GameObject(name);
        g.transform.SetParent(transform, false);
        var lr = g.AddComponent<LineRenderer>();
        lr.material = UiKit.Mat(Color.white);
        lr.widthCurve = new AnimationCurve(new Keyframe(0, 0.006f), new Keyframe(1, 0.002f));
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.startColor = new Color(color.r, color.g, color.b, 0.9f);
        lr.endColor = new Color(color.r, color.g, color.b, 0.1f);
        return lr;
    }

    void Update()
    {
        bool hasHands = (left && left.gameObject.activeInHierarchy) || (right && right.gameObject.activeInHierarchy);
        var newL = Hand(left, lineL, XRNode.LeftHand, hovL, ref downL);
        var newR = Hand(right, lineR, XRNode.RightHand, hovR, ref downR);
        MenuButton newH = null;
        if (!hasHands && head)
        {
            newH = Cast(new Ray(head.position, head.forward), out _);
            bool d = VRInput.AnyTrigger();
            if (d && !downHead && newH) newH.Click();
            downHead = d;
        }
        Refresh(hovL, hovR, hovHead, false);
        hovL = newL; hovR = newR; hovHead = newH;
        Refresh(hovL, hovR, hovHead, true);
    }

    static void Refresh(MenuButton a, MenuButton b, MenuButton c, bool on)
    {
        if (a) a.SetHover(on); if (b) b.SetHover(on); if (c) c.SetHover(on);
    }

    MenuButton Hand(Transform hand, LineRenderer line, XRNode node, MenuButton previous, ref bool wasDown)
    {
        if (!hand || !line) return null;
        if (!hand.gameObject.activeInHierarchy) { line.enabled = false; return null; }
        line.enabled = true;
        var ray = new Ray(hand.position, hand.forward);
        var b = Cast(ray, out float dist);
        line.SetPosition(0, ray.origin);
        line.SetPosition(1, ray.origin + ray.direction * (b ? dist : 2.5f));
        if (b && b != previous && b.interactable) VRInput.Haptic(node, 0.2f, 0.03f);

        bool down = VRInput.Trigger(node) || VRInput.KeyboardSpace();
        if (down && !wasDown && b) { b.Click(); VRInput.Haptic(node, 0.5f, 0.06f); }
        wasDown = down;
        return b;
    }

    MenuButton Cast(Ray ray, out float dist)
    {
        // premier bouton sur le rayon : on ignore ce qui n'est pas un bouton (objet tenu en main, mur proche en jeu)
        dist = 0f;
        MenuButton best = null;
        float bestD = float.MaxValue;
        foreach (var hit in Physics.RaycastAll(ray, maxDistance, ~0, QueryTriggerInteraction.Collide))
        {
            if (hit.distance >= bestD) continue;
            var b = hit.collider.GetComponentInParent<MenuButton>();
            if (b) { best = b; bestD = hit.distance; }
        }
        if (best) dist = bestD;
        return best;
    }
}
