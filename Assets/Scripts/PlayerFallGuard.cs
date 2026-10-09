using UnityEngine;

// Filet de sécurité du joueur (sur le XR Origin, ajouté par SetupVRScene) : il ne doit jamais passer sous la carte.
// On mémorise la dernière position où il avait les pieds au sol ; s'il descend nettement sous le sol (capsule poussée à travers
// un sol fin, contre un coin de mur ou une caisse), il y est ramené aussitôt.
[RequireComponent(typeof(CharacterController))]
public class PlayerFallGuard : MonoBehaviour
{
    public float fallLimit = -0.35f;        // y du rig en dessous duquel on considère qu'il est passé sous le sol
    CharacterController cc;
    Vector3 lastSafe;
    float safeTimer;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        lastSafe = transform.position;
    }

    void LateUpdate()
    {
        var p = transform.position;
        if (p.y < fallLimit)
        {
            bool was = cc.enabled; cc.enabled = false;           // le CharacterController empêche de déplacer le transform
            transform.position = lastSafe;
            cc.enabled = was;
            Physics.SyncTransforms();
            return;
        }
        // au sol depuis un instant, à une hauteur normale : position sûre
        if (cc.isGrounded && p.y > -0.1f && p.y < 1f) { safeTimer += Time.deltaTime; if (safeTimer > 0.25f) lastSafe = p; }
        else safeTimer = 0f;
    }
}
