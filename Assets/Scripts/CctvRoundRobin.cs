using UnityEngine;

// Caméras de surveillance : au lieu de rendre les 4 caméras à chaque image (trop lourd pour le Quest),
// on en rend une seule par image, à tour de rôle : chaque écran est rafraîchi ~18 fois par seconde à 72 Hz.
// Rien n'est rendu quand aucun écran du pupitre n'est visible et que la vue plein écran est fermée.
public class CctvRoundRobin : MonoBehaviour
{
    public Camera[] cameras;
    public Renderer[] screens;          // écrans du pupitre
    public GameObject overlay;          // vue plein écran
    public int camerasPerFrame = 1;
    int next;

    void Start()
    {
        foreach (var c in cameras) if (c) c.enabled = false;   // rendues à la main
    }

    bool AnyoneWatching()
    {
        if (overlay && overlay.activeInHierarchy) return true;
        if (screens == null || screens.Length == 0) return true;     // scène sans la liste des écrans : on rend toujours
        foreach (var s in screens) if (s && s.isVisible) return true;
        return false;
    }

    void LateUpdate()
    {
        if (cameras == null || cameras.Length == 0 || !AnyoneWatching()) return;
        for (int k = 0; k < camerasPerFrame; k++)
        {
            var c = cameras[next];
            next = (next + 1) % cameras.Length;
            if (c && c.targetTexture) c.Render();
        }
    }
}
