using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// [Confort] Garde-fou de la téléportation, posé sur le rig du joueur par RoomBuilder.
//   - Si le joueur se retrouve dans une zone encore fermée (WaveManager) ou dehors (au sud des fenêtres),
//     il est ramené à sa dernière position valide : la téléportation ne permet pas de « sauter » les portes de niveau
//     ni de sortir par une fenêtre.
//   - Les mains (et les holsters) ne doivent pas pouvoir « attraper » le sol : on leur retire la couche d'interaction
//     « Teleport », réservée au rayon de téléportation.
public class TeleportGuard : MonoBehaviour
{
    Vector3 lastValid;
    bool hasValid;
    float stripAt;

    void Start()
    {
        StripTeleportLayer();
        stripAt = Time.unscaledTime + 1f;      // une 2e fois : certains interactors sont créés après le démarrage
    }

    void LateUpdate()
    {
        if (stripAt > 0f && Time.unscaledTime >= stripAt) { stripAt = 0f; StripTeleportLayer(); }

        var p = transform.position;
        if (MapZones.Allowed(p)) { lastValid = p; hasValid = true; return; }
        if (!hasValid) return;
        // zone fermée ou extérieur : retour à la dernière position valide
        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        transform.position = lastValid;
        if (cc) cc.enabled = true;
    }

    static void StripTeleportLayer()
    {
        int tp = InteractionLayerMask.GetMask("Teleport");
        if (tp == 0) return;
        foreach (var it in FindObjectsByType<XRBaseInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (it is XRRayInteractor && it.name.Contains("Teleport")) continue;   // le rayon de téléportation garde sa couche
            int m = it.interactionLayers;
            if ((m & tp) != 0) it.interactionLayers = m & ~tp;
        }
    }
}

// Zones de la carte (docs/MAP.md, repère Blender : X droite, Y nord ; Unity = (-x, z, -y)) et niveau qui les ouvre.
public static class MapZones
{
    struct Zone { public float x0, x1, y0, y1; public int level; }

    static readonly Zone[] Zones =
    {
        new Zone { x0 = -7.5f,   x1 = 3.8f,   y0 = -14.1f, y1 = -6.1f,  level = 1 },   // bunker
        new Zone { x0 = -14.5f,  x1 = -7.35f, y0 = -11.2f, y1 = 4.2f,   level = 2 },   // couloir gauche
        new Zone { x0 = -22f,    x1 = -14.5f, y0 = -8f,    y1 = -1.65f, level = 2 },   // cuisine (+ son passage)
        new Zone { x0 = -19.7f,  x1 = -14.5f, y0 = 0.35f,  y1 = 4.65f,  level = 4 },   // générateur
        new Zone { x0 = 3.65f,   x1 = 10.5f,  y0 = -11.2f, y1 = 7.6f,   level = 2 },   // couloir droit
        new Zone { x0 = 10.5f,   x1 = 16.8f,  y0 = 3.85f,  y1 = 9.15f,  level = 3 },   // stockage
    };

    /// <summary>Niveau qui ouvre la zone de ce point (1..4), 0 si hors des zones connues, -1 dehors (au sud du bunker).</summary>
    public static int LevelAt(Vector3 unityPos)
    {
        float bx = -unityPos.x, by = -unityPos.z;
        foreach (var z in Zones)
            if (bx >= z.x0 && bx <= z.x1 && by >= z.y0 && by <= z.y1) return z.level;
        if (by < -14.2f) return -1;
        return 0;
    }

    public static int CurrentLevel => Mathf.Max(1, WaveManager.Level);

    public static bool IsOpen(int level) => level <= 0 || level <= CurrentLevel;

    /// <summary>Le joueur a-t-il le droit d'être ici ? (dehors : non ; zone fermée : non ; trou entre deux zones : oui)</summary>
    public static bool Allowed(Vector3 unityPos)
    {
        int lv = LevelAt(unityPos);
        if (lv < 0) return false;
        return lv == 0 || lv <= CurrentLevel;
    }
}
