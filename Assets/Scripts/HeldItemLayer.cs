using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// [Hotfix #156, suite] Saut infini en visant au sniper : la détection du sol de XRI (GravityProvider) lance une sphère depuis la TÊTE
// jusqu'aux pieds. En épaulant le sniper (long canon sous le visage), la sphère touchait l'arme tenue : le joueur était « au sol »
// en l'air, pouvait ressauter sans fin et la gravité ne s'appliquait plus (il restait en haut tant que l'arme n'était pas baissée).
// Correction à la source, pour TOUS les objets pris en main (armes, marteau, planches, lampe…) :
//   - tant qu'il est tenu par une main, l'objet passe sur le calque HeldLayer (29), que la détection du sol ignore
//     (masque du GravityProvider : Default, TransparentFX, Water, UI) ; les caméras et lumières, elles, voient tous les calques ;
//   - ses collisionneurs ne touchent plus la capsule du joueur (CharacterController) : on ne peut plus « marcher » sur son arme ;
//   - lâché ou rangé dans un holster : calques d'origine et collisions rétablis.
// Branché sur les interacteurs des mains de chaque scène (pas les holsters de la ceinture), donc aussi les armes créées en jeu.
public static class HeldItemLayer
{
    public const int HeldLayer = 29;

    class Saved { public List<(GameObject go, int layer)> layers = new List<(GameObject, int)>(); public List<Collider> cols = new List<Collider>(); }
    static readonly Dictionary<Transform, Saved> held = new Dictionary<Transform, Saved>();
    static readonly HashSet<XRBaseInteractor> hooked = new HashSet<XRBaseInteractor>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { held.Clear(); hooked.Clear(); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.sceneLoaded += OnLoaded;
        OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void OnLoaded(Scene s, LoadSceneMode m)
    {
        hooked.RemoveWhere(i => !i);
        foreach (var it in Object.FindObjectsByType<XRBaseInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (it is XRSocketInteractor || !hooked.Add(it)) continue;        // holsters de la ceinture : non
            it.selectEntered.AddListener(OnGrab);
            it.selectExited.AddListener(OnRelease);
        }
    }

    static CharacterController Player() => PlayerHealth.Instance ? PlayerHealth.Instance.GetComponent<CharacterController>() : Object.FindFirstObjectByType<CharacterController>();

    static void OnGrab(SelectEnterEventArgs a)
    {
        var t = a.interactableObject != null ? a.interactableObject.transform : null;
        if (!t || held.ContainsKey(t)) return;
        var saved = new Saved();
        foreach (var tr in t.GetComponentsInChildren<Transform>(true))
        {
            saved.layers.Add((tr.gameObject, tr.gameObject.layer));
            if (tr.gameObject.layer < 6) tr.gameObject.layer = HeldLayer;   // calques spéciaux (écran de fin, etc.) laissés tels quels
        }
        var cc = Player();
        foreach (var c in t.GetComponentsInChildren<Collider>(true))
        {
            if (c.isTrigger) continue;
            if (cc) Physics.IgnoreCollision(cc, c, true);
            saved.cols.Add(c);
        }
        held[t] = saved;
    }

    static void OnRelease(SelectExitEventArgs a)
    {
        var t = a.interactableObject != null ? a.interactableObject.transform : null;
        if (!t || !held.TryGetValue(t, out var saved)) return;
        // encore tenu par l'autre main : on garde le calque
        if (a.interactableObject.isSelected && a.interactableObject.interactorsSelecting.Exists(i => !(i is XRSocketInteractor))) return;
        held.Remove(t);
        foreach (var (go, layer) in saved.layers) if (go) go.layer = layer;
        var cc = Player();
        if (cc) foreach (var c in saved.cols) if (c) Physics.IgnoreCollision(cc, c, false);
    }
}
