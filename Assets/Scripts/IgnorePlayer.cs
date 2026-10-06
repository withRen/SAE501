using UnityEngine;

// Évite que les objets (ceinture, planches...) poussent le CharacterController du joueur.
public class IgnorePlayer : MonoBehaviour
{
    void Start()
    {
        var cc = FindAnyObjectByType<CharacterController>();
        if (!cc) return;
        foreach (var c in GetComponentsInChildren<Collider>(true)) Physics.IgnoreCollision(c, cc);
    }
}
