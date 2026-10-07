using UnityEngine;

// Caméras de surveillance : au lieu de rendre les 4 caméras à chaque image (trop lourd pour le Quest),
// on en rend une seule par image, à tour de rôle : chaque écran est rafraîchi ~18 fois par seconde à 72 Hz.
public class CctvRoundRobin : MonoBehaviour
{
    public Camera[] cameras;
    public int camerasPerFrame = 1;
    int next;

    void Start()
    {
        foreach (var c in cameras) if (c) c.enabled = false;   // rendues à la main
    }

    void LateUpdate()
    {
        if (cameras == null || cameras.Length == 0) return;
        for (int k = 0; k < camerasPerFrame; k++)
        {
            var c = cameras[next];
            next = (next + 1) % cameras.Length;
            if (c && c.targetTexture) c.Render();
        }
    }
}
