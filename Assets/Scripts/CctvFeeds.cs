using System.Collections.Generic;
using UnityEngine;

// Les caméras de surveillance ne se rendent pas à chaque image : une seule à la fois, à tour de rôle.
// Effet "vieille caméra" et gros gain de performance (3 rendus de scène en moins par image).
public class CctvFeeds : MonoBehaviour
{
    public List<Camera> cameras = new List<Camera>();
    public int period = 12;            // 72 fps / 12 = 6 images par seconde et par caméra
    public int fastPeriod = 4;         // caméra affichée en plein écran : 18 images par seconde

    public int FullscreenIndex = -1;   // renseigné par MonitorStation

    void Start()
    {
        foreach (var c in cameras) c.enabled = false;
    }

    void Update()
    {
        int n = cameras.Count;
        if (n == 0) return;
        int frame = Time.frameCount;
        for (int i = 0; i < n; i++)
        {
            int p = (i == FullscreenIndex) ? fastPeriod : period;
            int offset = (i * p) / n;
            cameras[i].enabled = (frame % p) == offset;
        }
    }
}
