using System;
using UnityEngine;

// Santé du joueur. L'effet visuel (vignettage rouge) viendra plus tard (tâche 8.2).
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    public float maxHealth = 100f;
    public float Health { get; private set; }
    public bool Dead => Health <= 0f;
    public event Action<float> Damaged;       // dégâts reçus
    public event Action Died;

    Transform head;

    void Awake()
    {
        Instance = this;
        Health = maxHealth;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // position de la tête du joueur (là où les zombies se dirigent)
    public Vector3 HeadPosition
    {
        get
        {
            if (head == null && Camera.main != null) head = Camera.main.transform;
            return head != null ? head.position : transform.position + Vector3.up * 1.6f;
        }
    }

    public void TakeDamage(float amount)
    {
        if (Dead) return;
        Health = Mathf.Max(0f, Health - amount);
        Damaged?.Invoke(amount);
        if (Dead) Died?.Invoke();
    }
}
