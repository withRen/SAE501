using UnityEngine;

// Reçoit les événements de l'animation de tir du pack Modern Guns (M1911@Fire) : « Shoot » (à 0,117 s) et « CasingRelease » (à 0,183 s).
// Dans le pack ils sont reçus par SimpleShoot (entrée clavier/souris, inutilisable en VR).
// Pistol tire tout de suite (sans attendre 0,117 s) et éjecte la douille par minuteur : ces deux fonctions ne servent qu'à absorber les événements.
public class GunAnimationEvents : MonoBehaviour
{
    public Pistol pistol;
    public int shootCalls, casingCalls;   // diagnostic : combien d'événements l'animation a livrés

    void Awake() { if (!pistol) pistol = GetComponentInParent<Pistol>(); }

    public void Shoot() { shootCalls++; }

    // la douille est déclenchée par un minuteur dans Pistol (voir casingDelay) : les événements ne sont livrés que si l'animation part du début
    public void CasingRelease() { casingCalls++; }
}
