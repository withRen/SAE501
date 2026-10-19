using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Table de surveillance : toucher un petit écran affiche son flux en grand sur l'écran fixé au mur au-dessus du pupitre.
// Toucher à nouveau le même petit écran (ou le grand écran) le referme ; toucher un autre écran change de flux.
// Avant : la vue était collée au visage à 0,5 m (inconfortable en VR). Maintenant plus rien n'est collé à la tête.
public class MonitorStation : MonoBehaviour
{
    public Transform overlay;           // grand écran du mur
    public Renderer overlayRenderer;
    public Material[] feeds;

    int current = -1;

    public void Show(int i)
    {
        if (overlay.gameObject.activeSelf && current == i) { Hide(); return; }
        current = i;
        overlayRenderer.sharedMaterial = feeds[i];
        overlay.gameObject.SetActive(true);
    }

    public void Hide()
    {
        current = -1;
        overlay.gameObject.SetActive(false);
    }
}
