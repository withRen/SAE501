using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public enum BeltItemType { Pistol, Hammer, Flashlight, Magazine }

public class BeltItem : MonoBehaviour
{
    public BeltItemType type;

    // vrai si l'objet est tenu par une main (et pas rangé dans un holster)
    public static bool InHand(XRGrabInteractable g) =>
        g != null && g.isSelected && !(g.firstInteractorSelecting is XRSocketInteractor);
}

// Ceinture : suit la tête (position + rotation en Y lissée) à hauteur de hanche.
[DefaultExecutionOrder(-50)]
public class BeltRig : MonoBehaviour
{
    public Transform head;
    public Transform rigRoot;
    public float heightRatio = 0.55f;
    public float yawSpeed = 3f;

    float yaw;

    void Start()
    {
        yaw = head.eulerAngles.y;
        Application.onBeforeRender += Apply;
    }

    void OnDestroy() => Application.onBeforeRender -= Apply;

    void Update() => Apply();

    void Apply()
    {
        yaw = Mathf.LerpAngle(yaw, head.eulerAngles.y, 1f - Mathf.Exp(-yawSpeed * Time.deltaTime));
        float h = Mathf.Max(0.6f, head.position.y - rigRoot.position.y);
        transform.SetPositionAndRotation(
            new Vector3(head.position.x, rigRoot.position.y + h * heightRatio, head.position.z),
            Quaternion.Euler(0f, yaw, 0f));
    }
}

// Holster : n'accepte qu'un type d'objet
public class BeltSocket : XRSocketInteractor
{
    public BeltItemType accepts;

    bool Match(object o)
    {
        var c = o as Component;
        var item = c ? c.GetComponent<BeltItem>() : null;
        return item && item.type == accepts;
    }

    public override bool CanHover(IXRHoverInteractable interactable) => base.CanHover(interactable) && Match(interactable);
    public override bool CanSelect(IXRSelectInteractable interactable) => base.CanSelect(interactable) && Match(interactable);
}
