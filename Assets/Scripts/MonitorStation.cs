using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Table de surveillance : sélectionner un écran affiche son flux en "plein écran" devant les yeux.
public class MonitorStation : MonoBehaviour
{
    public Transform overlay;
    public Renderer overlayRenderer;
    public Material[] feeds;
    public CctvFeeds cctv;

    public void Show(int i)
    {
        if (cctv) cctv.FullscreenIndex = i;
        overlayRenderer.sharedMaterial = feeds[i];
        overlay.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (cctv) cctv.FullscreenIndex = -1;
        overlay.gameObject.SetActive(false);
    }
}
