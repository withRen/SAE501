using UnityEngine;

// Points de départ des zombies (prefab ZombieSpawnPoints) : les deux bouts de couloirs et la fenêtre Nord.
// Chaque point est un objet vide dont la position est le point de départ du chemin NavMesh du zombie ;
// son orientation (Z bleu) est celle du zombie à l'apparition.
public class ZombieSpawnPoints : MonoBehaviour
{
    public Transform leftCorridorEnd;
    public Transform rightCorridorEnd;
    public Transform northWindow;

    public Transform[] All => new[] { leftCorridorEnd, rightCorridorEnd, northWindow };

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
