using UnityEngine;

// Suit la tête du marteau (position, vitesse) pour détecter les coups sur les clous.
// Ordre d'exécution : après XRI (qui déplace le marteau), avant les clous.
[DefaultExecutionOrder(-10)]
public class HammerHead : MonoBehaviour
{
    public static HammerHead Instance { get; private set; }

    public Vector3 Position { get; private set; }
    public Vector3 PreviousPosition { get; private set; }
    public Vector3 Velocity { get; private set; }
    public float Speed { get; private set; }     // vitesse lissée : reste "chaude" une fraction de seconde après le coup
    public float Radius { get; private set; } = 0.05f;

    void Awake()
    {
        Instance = this;
        var c = GetComponent<Collider>();
        if (c) Radius = Mathf.Max(c.bounds.extents.x, c.bounds.extents.y, c.bounds.extents.z);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start() => PreviousPosition = Position = transform.position;

    void Update()
    {
        if (PauseMenu.Paused) { PreviousPosition = Position = transform.position; Velocity = Vector3.zero; Speed = 0f; return; }   // pause : pas de clou enfoncé
        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        PreviousPosition = Position;
        Position = transform.position;
        Velocity = (Position - PreviousPosition) / dt;
        Speed = Mathf.Max(Velocity.magnitude, Speed * 0.85f);
    }
}
