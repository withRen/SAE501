using UnityEngine;

// Caisse de ravitaillement qui tombe du plafond (SupplyRadio) : un « boum » quand elle touche le sol.
public class SupplyCrate : MonoBehaviour
{
    public bool Landed { get; private set; }
    AudioClip thud;

    void Awake() => thud = ProcAudio.Thud();

    void OnCollisionEnter(Collision c)
    {
        if (Landed || c.relativeVelocity.magnitude < 1f) return;
        Landed = true;
        AudioSource.PlayClipAtPoint(thud, transform.position, 1f);
    }
}
