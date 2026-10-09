using UnityEngine;
using Unity.XR.CoreUtils;

// S'accroupir : tant que le JOYSTICK GAUCHE est enfoncé (clic), la vue descend (pour passer sous un obstacle, ramasser au sol,
// viser sous une planche) ; relâché, on se relève. Sur le XR Origin, ajouté par SetupVRScene. Éditeur sans casque : touche C.
[RequireComponent(typeof(XROrigin))]
public class Crouch : MonoBehaviour
{
    public float crouchDrop = 0.55f;        // la vue descend de 55 cm
    public float speed = 6f;                // vitesse de transition (exponentielle)

    XROrigin origin;
    float standY, current;

    public static bool Crouching { get; private set; }

    void Start()
    {
        origin = GetComponent<XROrigin>();
        standY = origin.CameraYOffset;
        current = standY;
    }

    void Update()
    {
        bool want = VRInput.LeftStickClick() || VRInput.KeyboardKey(UnityEngine.InputSystem.Key.C);
        Crouching = want;
        float goal = want ? standY - crouchDrop : standY;
        current = Mathf.Lerp(current, goal, 1f - Mathf.Exp(-speed * Time.deltaTime));
        if (Mathf.Abs(origin.CameraYOffset - current) > 0.0005f) origin.CameraYOffset = current;
        // l'XR Origin n'applique ce décalage que dans certains modes de suivi : on déplace aussi directement l'objet « Camera Offset »
        var off = origin.CameraFloorOffsetObject ? origin.CameraFloorOffsetObject.transform : null;
        if (off && Mathf.Abs(off.localPosition.y - current) > 0.0005f) { var lp = off.localPosition; lp.y = current; off.localPosition = lp; }
    }
}
