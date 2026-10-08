using UnityEngine;

// Objet qui n'apparaît pas à chaque partie : au chargement de la scène, il reste avec la probabilité `chance`,
// sinon il est retiré. Utilisé pour les chargeurs posés dans la carte (RoomBuilder.BuildMagazines).
public class RandomSpawn : MonoBehaviour
{
    [Range(0f, 1f)] public float chance = 0.5f;

    void Awake()
    {
        if (Random.value <= chance) return;
        gameObject.SetActive(false);      // tout de suite (pas de contour, pas d'interaction)
        Destroy(gameObject);
    }
}
