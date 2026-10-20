using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

// Téléportation (option des PARAMÈTRES, au casque) : une seule zone de téléportation posée sur le sol de toute la carte.
// On n'arrive que sur un point où l'on pourrait aller à pied, sans grand détour :
//   - le point visé est sur le NavMesh (sol plat, pas sur une table ni sur une caisse) ;
//   - on pourrait y marcher par un chemin court (au plus 1,5 fois la distance + 1 m) : pas à travers un mur, une fenêtre
//     ou une porte de zone fermée (pas de chemin), ni dans la pièce d'à côté par un long détour (chemin trop long).
// Sinon le rayon s'affiche « bloqué » (rouge) et rien ne se passe.
public class NavTeleportArea : TeleportationArea
{
    const float MaxSnap = 0.35f;            // écart toléré entre le point visé et le NavMesh
    const float MaxDetour = 1.5f;           // chemin à pied / distance à vol d'oiseau

    static NavTeleportArea instance;

    /// <summary>Crée la zone (une fois par scène) sur les sols de la carte. Appelé par GameSettings quand l'option est active.</summary>
    public static void Ensure(TeleportationProvider provider)
    {
        if (instance) return;
        var cols = new List<Collider>();
        foreach (var c in FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (c.isTrigger || !c.enabled || c.attachedRigidbody) continue;                   // objets qu'on prend, zombies
            if (c.GetComponentInParent<IXRInteractable>() != null || c.GetComponentInParent<XROrigin>()) continue;
            cols.Add(c);                    // murs compris : le rayon s'y arrête (normale pas vers le haut, donc refusé)
        }
        var go = new GameObject("Sol_Teleportation");
        go.SetActive(false);                // les collisionneurs doivent être connus avant l'enregistrement (OnEnable)
        var area = go.AddComponent<NavTeleportArea>();
        area.colliders.Clear();
        area.colliders.AddRange(cols);
        area.teleportationProvider = provider;
        area.interactionLayers = InteractionLayerMask.GetMask("Teleport");
        area.filterSelectionByHitNormal = true;   // sol seulement (30° d'écart avec la verticale au plus)
        go.SetActive(true);
        instance = area;
    }

    protected override bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
    {
        if (!base.GenerateTeleportRequest(interactor, raycastHit, ref teleportRequest)) return false;
        if (!Reachable(raycastHit.point, out var dest)) return false;
        teleportRequest.destinationPosition = dest;
        return true;
    }

    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        if (!base.IsSelectableBy(interactor) || PauseMenu.Paused) return false;
        return interactor is XRRayInteractor ray && ray.TryGetCurrent3DRaycastHit(out var hit) && Reachable(hit.point, out _);
    }

    bool Reachable(Vector3 point, out Vector3 dest)
    {
        dest = point;
        if (!NavMesh.SamplePosition(point, out var to, MaxSnap, NavMesh.AllAreas)) return false;
        var origin = teleportationProvider ? teleportationProvider.mediator.xrOrigin : FindFirstObjectByType<XROrigin>();
        if (!origin || !origin.Camera) return false;
        var feet = origin.Camera.transform.position;
        feet.y = origin.Origin.transform.position.y;
        if (!NavMesh.SamplePosition(feet, out var from, 1f, NavMesh.AllAreas)) return false;
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) return false;
        float walk = 0f;
        for (int i = 1; i < path.corners.Length; i++) walk += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        if (walk > MaxDetour * Vector3.Distance(from.position, to.position) + 1f) return false;   // de l'autre côté d'un mur
        dest = to.position;
        return true;
    }
}
