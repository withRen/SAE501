using UnityEngine;

// Points de départ des zombies (prefab ZombieSpawnPoints) : les deux bouts de couloirs et l'extérieur sud du bunker (devant ses fenêtres).
// Chaque point est un objet vide dont la position est le point de départ du chemin NavMesh du zombie ;
// son orientation (Z bleu) est celle du zombie à l'apparition.
public class ZombieSpawnPoints : MonoBehaviour
{
    public Transform leftCorridorEnd;
    public Transform rightCorridorEnd;
    [UnityEngine.Serialization.FormerlySerializedAs("northWindow")] public Transform exterior;
    public Transform roof;              // sur le toit du bunker, près des deux trous

    public Transform[] All => new[] { leftCorridorEnd, rightCorridorEnd, exterior, roof };

    void OnDrawGizmos()
    {
        foreach (var t in All)
        {
            if (!t) continue;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(t.position + Vector3.up * 0.1f, 0.3f);
            Gizmos.DrawRay(t.position + Vector3.up * 0.1f, t.forward * 0.8f);
        }
    }
}
