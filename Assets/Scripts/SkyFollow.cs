using UnityEngine;

// La sphère de ciel suit la tête du joueur (position seulement) : elle reste toujours à la même distance,
// comme un vrai ciel, et à l'intérieur de la distance d'affichage de la caméra.
public class SkyFollow : MonoBehaviour
{
    Transform head;

    void LateUpdate()
    {
        if (!head) { var c = Camera.main; if (!c) return; head = c.transform; }
        transform.position = head.position;
    }
}
