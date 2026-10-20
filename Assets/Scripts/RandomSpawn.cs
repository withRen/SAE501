using UnityEngine;

// Objet qui n'apparaît pas à chaque partie : au chargement de la scène, il reste avec la probabilité `chance`,
// sinon il est retiré. Utilisé pour les chargeurs posés dans la carte (RoomBuilder.BuildMagazines).
public class RandomSpawn : MonoBehaviour
{
    [Range(0f, 1f)] public float chance = 0.5f;
    public bool scaledByDifficulty;       // chargeurs : chance x GameSettings.MagazineChanceFactor (Facile 75 %, Normal 50 %, Difficile 35 %)

    void Awake()
    {
        float c = scaledByDifficulty ? Mathf.Clamp01(chance * GameSettings.MagazineChanceFactor) : chance;
        if (Random.value <= c) return;
        gameObject.SetActive(false);      // tout de suite (pas de contour, pas d'interaction)
        Destroy(gameObject);
    }
}
