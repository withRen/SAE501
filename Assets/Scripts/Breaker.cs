using UnityEngine;

// Disjoncteur des caméras de surveillance (boîtier électrique du bunker).
// [#111] Levier physique (Lever) : on l'attrape (grip) et on l'ABAISSE jusqu'en bas pour enclencher ; on le remonte pour couper.
// Coupé : les caméras ne sont pas rendues, les écrans du pupitre sont noirs (CCTV.shader, _CctvPower) ; voyant rouge.
// Enclenché : caméras allumées, voyant vert.
public class Breaker : MonoBehaviour
{
    public static bool On { get; private set; }

    public Lever lever;
    public Renderer led;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => On = false;

    void Start() => Set(false);

    void Update()
    {
        if (!lever) return;
        // levier en bas : enclenché ; remonté en haut : coupé (entre les deux : l'état ne change pas)
        if (!On && lever.IsDown) Set(true);
        else if (On && lever.IsUp) Set(false);
    }

    public void Set(bool on)
    {
        On = on;
        Shader.SetGlobalFloat("_CctvPower", on ? 1f : 0f);
        if (lever && !lever.Held) lever.Snap(on);
        if (led)
        {
            led.material.color = on ? new Color(0.2f, 1f, 0.3f) : new Color(1f, 0.15f, 0.1f);
            led.material.EnableKeyword("_EMISSION");
            led.material.SetColor("_EmissionColor", led.material.color * 2f);
        }
    }
}
