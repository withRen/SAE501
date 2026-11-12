using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

// [Hotfix #162] Au casque, le joueur peut avancer physiquement la tête (et le corps) dans sa pièce : la caméra bouge dans le rig
// sans être arrêtée par le décor, et passait à travers les murs et les portes fermées (on sortait de la carte).
// Garde-fou : à chaque image, une petite sphère (rayon 0,08 m) est tracée de la dernière position VALIDE de la tête jusqu'à sa
// position actuelle. Si ce trajet traverse du décor solide, le XR Origin est décalé (à plat) pour que la tête revienne à la
// dernière position valide, du bon côté du mur (le joueur garde son orientation), avec un très court fondu noir (0,2 s).
// Si le joueur continue d'avancer dans la pièce réelle, il est retenu à chaque image : le mur « repousse ».
// Ce qui compte comme décor solide : les collisionneurs immobiles (sans corps physique, ou cinématiques) qui ne sont pas des
// déclencheurs. Ignorés : objets tenus (calque 29) ou posés que le joueur pousse (IgnorePlayer : planches, ceinture, caisses),
// zombies, joueurs (avatars, zones touchables), le joueur lui-même. Une porte ouverte ne gêne pas (son battant n'est plus sur le
// trajet) ; une porte fermée bloque.
// Pas de faux positif : se pencher près d'un mur (le trajet ne le traverse pas), une fenêtre ouverte, un escalier (la tête monte avec
// le corps). Un saut de plus de 0,6 m en une image (téléportation, réapparition, placement en début de manche) remet la position
// valide à zéro sans contrôle. Inactif en version PC (DesktopPlayer gère la caméra et le corps ensemble), quand le joueur est mort
// (spectateur) et pendant la cinématique de fin. Ajouté automatiquement au rig de chaque scène (aucune scène modifiée).
[DefaultExecutionOrder(900)]        // après les déplacements (XRI) et avant les HUD qui suivent la caméra
public class HeadWallGuard : MonoBehaviour
{
    const float Radius = 0.08f;
    const float TeleportJump = 0.6f;      // m en une image : téléportation / réapparition, pas un mouvement de tête
    const float FadeTime = 0.2f;

    XROrigin origin;
    CharacterController cc;
    Transform head;
    Vector3 lastValid;
    bool hasValid;
    float fadeUntil = -1f;
    Renderer fade;
    readonly RaycastHit[] hits = new RaycastHit[16];

    public static int Corrections { get; private set; }     // compteur (tests)
    /// <summary>Tests dans l'éditeur (version PC) : actif quand même.</summary>
    public static bool ForceForTests;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.sceneLoaded += OnLoaded;
        OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void OnLoaded(Scene s, LoadSceneMode m)
    {
        foreach (var o in FindObjectsByType<XROrigin>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!o.GetComponent<HeadWallGuard>()) o.gameObject.AddComponent<HeadWallGuard>();
    }

    void Awake()
    {
        origin = GetComponent<XROrigin>();
        cc = GetComponent<CharacterController>();
    }

    bool Active
    {
        get
        {
            if ((DesktopMode.On && !ForceForTests) || EscapeVideo.Playing) return false;
            var h = PlayerHealth.Instance;
            return !(h && h.IsDead);
        }
    }

    void LateUpdate()
    {
        if (!head) head = origin && origin.Camera ? origin.Camera.transform : (Camera.main ? Camera.main.transform : null);
        if (!head) return;
        UpdateFade();
        var p = head.position;
        if (!Active || !hasValid) { lastValid = p; hasValid = true; return; }

        var move = p - lastValid;
        float dist = move.magnitude;
        if (dist > TeleportJump) { lastValid = p; return; }       // téléportation, réapparition : nouvelle référence
        if (dist < 0.002f) return;

        if (Blocked(lastValid, move / dist, dist))
        {
            // la tête a traversé du décor : on ramène le rig pour que la tête revienne à la dernière position valide (à plat)
            var back = lastValid - p; back.y = 0f;
            Shift(back);
            Corrections++;
            fadeUntil = Time.unscaledTime + FadeTime;
            // la position valide reste la même (du bon côté du mur)
        }
        else lastValid = p;
    }

    void Shift(Vector3 delta)
    {
        bool had = cc && cc.enabled;
        if (had) cc.enabled = false;
        transform.position += delta;
        if (had) cc.enabled = true;
    }

    bool Blocked(Vector3 from, Vector3 dir, float dist)
    {
        int n = Physics.SphereCastNonAlloc(from, Radius, dir, hits, dist, ~(1 << HeldItemLayer.HeldLayer), QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            var h = hits[i];
            if (h.distance <= 0f) continue;                       // déjà au contact au départ (penché contre le mur) : pas une traversée
            if (Solid(h.collider)) return true;
        }
        return false;
    }

    static readonly Dictionary<Collider, bool> solidCache = new Dictionary<Collider, bool>();

    bool Solid(Collider c)
    {
        var rb = c.attachedRigidbody;
        if (rb && !rb.isKinematic) return false;                  // objets mobiles (posés, lancés)
        if (c.transform.IsChildOf(transform)) return false;       // le joueur lui-même (mains, ceinture)
        if (solidCache.TryGetValue(c, out var s)) return s;
        s = !c.GetComponentInParent<IgnorePlayer>() && !c.GetComponentInParent<Zombie>()
            && !c.GetComponentInParent<NetPlayer>() && !c.GetComponent<PlayerHitbox>() && !c.GetComponentInParent<PlayerHealth>();
        solidCache[c] = s;
        return s;
    }

    void UpdateFade()
    {
        bool on = Time.unscaledTime < fadeUntil;
        if (!on && !fade) return;
        if (!fade)
        {
            fade = HeadOverlay.Create(head, "Fondu_Mur", Texture2D.whiteTexture, Color.black, 0.12f, 4200);
            fade.transform.localScale = new Vector3(0.6f, 0.45f, 1f);
        }
        fade.enabled = on;
        if (on) fade.material.color = new Color(0f, 0f, 0f, Mathf.Clamp01((fadeUntil - Time.unscaledTime) / FadeTime));
    }

    void OnDestroy() { solidCache.Clear(); }
}
