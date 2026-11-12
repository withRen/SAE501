using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump;

// [Hotfix #156] Saut au casque : en appuyant sur A en rafale, on enchaînait les sauts en l'air et on « volait ».
// Le JumpProvider de XRI autorise un saut quand le GravityProvider dit « au sol » ou pendant la fenêtre de tolérance
// (0,25 s après avoir quitté le sol). Or la détection du sol (petite sphère sous les pieds) clignote contre un mur, une caisse,
// une planche ou une arme tenue : chaque clignotement remet le saut à zéro. La version PC avait été corrigée dans DesktopPlayer
// (TuneJump : fenêtre de tolérance réduite), pas la version casque.
// Ici, pour les deux versions : pas de saut en l'air, fenêtre de tolérance courte, et surtout un saut n'est permis qu'après
// être resté AU SOL sans interruption pendant 0,15 s. Le blocage passe par le « bypass » de l'entrée du saut (le bouton est lu
// comme relâché) : le JumpProvider reste allumé, la gravité et l'atterrissage fonctionnent normalement.
// Suite (saut infini en visant au sniper, voir HeldItemLayer pour la cause) : en plus, comme demandé par Rahim,
//   - 0,9 s au moins entre deux sauts (depuis le début du saut précédent) ;
//   - il faut être vraiment redescendu : pas plus de 0,15 m au-dessus du point de départ du saut précédent, ou posé depuis 0,6 s
//     (saut sur une caisse) ; s'il ne redescend pas, il ne peut pas ressauter.
// Ajouté automatiquement au rig de chaque scène (aucune modification de scène).
public class JumpGuard : MonoBehaviour
{
    const float GroundedBeforeJump = 0.15f;
    const float MinInterval = 0.9f;             // entre deux sauts
    const float BackDown = 0.15f;               // redescendu : à moins de 0,15 m du point de départ du saut précédent
    const float LandedLong = 0.6f;              // ... ou posé assez longtemps (arrivé sur une caisse)

    JumpProvider jump;
    GravityProvider gravity;
    float groundedFor;
    float lastJump = -10f, takeoffY;
    bool wasJumping;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.sceneLoaded += OnLoaded;
        OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void OnLoaded(Scene s, LoadSceneMode m)
    {
        foreach (var j in FindObjectsByType<JumpProvider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!j.GetComponent<JumpGuard>()) j.gameObject.AddComponent<JumpGuard>();
    }

    void Awake()
    {
        jump = GetComponent<JumpProvider>();
        gravity = FindFirstObjectByType<GravityProvider>(FindObjectsInactive.Include);
        if (!jump) { enabled = false; return; }
        jump.unlimitedInAirJumps = false;
        jump.inAirJumpCount = 0;
        jump.jumpForgivenessWindow = 0.05f;
        gate = new Gate { raw = jump.jumpInput };
        jump.jumpInput.bypass = gate;
    }

    Gate gate;
    Transform rig;
    float RigY { get { if (!rig) { var o = GetComponentInParent<Unity.XR.CoreUtils.XROrigin>(); rig = o ? o.transform : transform.root; } return rig.position.y; } }

    void Update()
    {
        if (!gravity) { gravity = FindFirstObjectByType<GravityProvider>(); if (!gravity) return; }
        if (jump.isJumping && !wasJumping) { lastJump = Time.time; takeoffY = RigY; }
        wasJumping = jump.isJumping;
        groundedFor = gravity.isGrounded && !jump.isJumping ? groundedFor + Time.deltaTime : 0f;
        bool cameDown = RigY <= takeoffY + BackDown || groundedFor >= LandedLong;
        gate.allow = groundedFor >= GroundedBeforeJump && Time.time - lastJump >= MinInterval && cameDown;
    }

    void OnDestroy() { if (jump && jump.jumpInput != null && jump.jumpInput.bypass == gate) jump.jumpInput.bypass = null; }

    // le vrai bouton, sauf « appuyé » quand le saut n'est pas permis (le relâchement passe toujours : un appui = un saut)
    class Gate : UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.IXRInputButtonReader
    {
        public UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader raw;
        public bool allow;
        public bool ReadIsPerformed() => allow && raw.ReadIsPerformed();
        public bool ReadWasPerformedThisFrame() => allow && raw.ReadWasPerformedThisFrame();
        public bool ReadWasCompletedThisFrame() => raw.ReadWasCompletedThisFrame();
        public float ReadValue() => allow ? raw.ReadValue() : 0f;
        public bool TryReadValue(out float value) { if (allow) return raw.TryReadValue(out value); value = 0f; return false; }
    }
}
