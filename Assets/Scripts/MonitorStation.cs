using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Table de surveillance : sélectionner un écran affiche son flux en "plein écran" devant les yeux.
public class MonitorStation : MonoBehaviour
{
    public Transform overlay;
    public Renderer overlayRenderer;
    public Material[] feeds;

    public void Show(int i)
    {
        overlayRenderer.sharedMaterial = feeds[i];
        overlay.gameObject.SetActive(true);
    }

    public void Hide() => overlay.gameObject.SetActive(false);
}
