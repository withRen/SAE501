using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Disjoncteur des caméras de surveillance (boîtier électrique du bunker) : un appui (grip) bascule le levier.
// Coupé : les caméras ne sont pas rendues, les écrans du pupitre sont noirs (CCTV.shader, _CctvPower) ; voyant rouge.
// Enclenché : caméras allumées, voyant vert, petit bourdonnement électrique.
public class Breaker : MonoBehaviour
{
    public static bool On { get; private set; }

    public Transform lever;
    public Renderer led;
    public Vector3 front;               // face avant du boîtier (le levier bascule autour de cet axe)

    XRSimpleInteractable it;
    AudioSource src;
    static AudioClip clack;
    Quaternion leverRest;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => On = false;

    void Awake()
    {
        it = GetComponent<XRSimpleInteractable>();
        it.selectEntered.AddListener(_ => Set(!On));
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        if (!clack) clack = ProcAudio.DryFire();
        if (lever) leverRest = lever.rotation;
        Set(false);
    }

    public void Set(bool on)
    {
        On = on;
        Shader.SetGlobalFloat("_CctvPower", on ? 1f : 0f);
        if (lever) lever.rotation = Quaternion.AngleAxis(on ? -35f : 35f, Vector3.Cross(Vector3.up, front)) * leverRest;
        if (led) led.material.color = on ? new Color(0.2f, 1f, 0.3f) : new Color(1f, 0.15f, 0.1f);
        if (led) { led.material.EnableKeyword("_EMISSION"); led.material.SetColor("_EmissionColor", led.material.color * 2f); }
        if (src && Time.timeSinceLevelLoad > 0.5f) src.PlayOneShot(clack, 1f);
    }
}
