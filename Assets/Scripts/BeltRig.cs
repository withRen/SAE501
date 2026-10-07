using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
