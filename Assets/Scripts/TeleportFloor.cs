using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

// [Confort] Sol de la carte où l'on peut se téléporter (option PARAMÈTRES > Téléportation ; manette droite : joystick vers l'avant).
// Posé par RoomBuilder sur chaque sol de la carte, avec un TeleportationArea (couche d'interaction « Teleport »).
// Actif seulement si l'option est activée ET si la zone de ce sol est déjà ouverte (MapZones / WaveManager).
// Sécurité en plus : TeleportGuard ramène le joueur s'il atterrit malgré tout dans une zone fermée ou dehors.
public class TeleportFloor : MonoBehaviour
{
    public int unlockLevel = 1;          // niveau à partir duquel ce sol est accessible (0 = inconnu : suit seulement l'option)

    TeleportationArea area;
    float nextCheck;

    void Awake() => area = GetComponent<TeleportationArea>();

    void Update()
    {
        if (!area || Time.unscaledTime < nextCheck) return;
        nextCheck = Time.unscaledTime + 0.25f;
        bool on = GameSettings.Teleport && MapZones.IsOpen(unlockLevel);
        if (area.enabled != on) area.enabled = on;
    }
}
