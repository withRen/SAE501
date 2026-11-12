using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Armes de départ du duel 1v1 (kit choisi par l'hôte dans le lobby : NetDuel.Primary) :
//   - arme principale AK74 ou sniper : dans la main droite ; le pistolet, arme secondaire, au holster de la ceinture ;
//   - « aucune » : le pistolet seul, dans la main droite.
// Au début de chaque manche (NetDuel), les armes reviennent à ces places, même si elles avaient été lâchées.
// Le pistolet de l'Ouest est celui du joueur 1, celui de l'Est celui du joueur 2. Les armes ne sont pas partagées entre les casques :
// chacun a ses propres armes dans sa copie de l'arène (les autres joueurs voient celle qu'il tient grâce à NetPlayer).
// Munitions : Pistol.InfiniteAmmo (duel).
public static class DuelLoadout
{
    static Pistol primaryWeapon;
    static int primaryKind = -1;

    // L'arène lancée seule (mode Play directement dans la scène, sans passer par le lobby) : armes en main et munitions infinies,
    // pour pouvoir la tester sans réseau. En partie réseau, c'est NetDuel qui s'en charge.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BootSolo()
    {
        primaryWeapon = null; primaryKind = -1;
        if (!ArenaMaps.InArena) return;      // toutes les cartes du duel (ArenaMaps)
        var go = new GameObject("DuelSoloBoot");
        go.AddComponent<SoloBoot>();
    }

    class SoloBoot : MonoBehaviour
    {
        System.Collections.IEnumerator Start()
        {
            yield return new WaitForSeconds(1.2f);
            if (NetSession.Active) { Destroy(gameObject); yield break; }      // partie réseau : NetDuel donne les armes
            Pistol.InfiniteAmmo = true;
            Give();
            Destroy(gameObject);
        }
    }

    public static void Give()
    {
        var health = PlayerHealth.Instance;
        if (!health) return;
        int number = NetLobby.Side(NetSession.LocalId);  // [hotfix #158] côté du duel (1 ou 2)
        var pistol = FindPistol(number == 1 ? "Pistol_West" : "Pistol_East");
        var hand = GameObject.Find("Right Controller");
        var it = hand ? hand.GetComponentInChildren<XRDirectInteractor>(true) : null;
        if (!pistol || !it || !it.interactionManager) { Debug.LogWarning("[Duel] armes de départ impossibles (pistolet ou main introuvable)"); return; }

        var primary = NetDuel.Primary != 0 ? Primary(NetDuel.Primary, it) : null;
        pistol.ammo = pistol.maxAmmo;
        pistol.pvpHeadDamage = 50f; pistol.pvpBodyDamage = 20f; pistol.pvpLegDamage = -1f;      // CS : 5 balles au corps, 2 à la tête (PV 100)
        if (primary) primary.ammo = primary.maxAmmo;

        it.interactionManager.CancelInteractorSelection((IXRSelectInteractor)it);          // la main est libre
        if (primary)
        {
            ToHolster(pistol);
            ToHand(primary, it);
        }
        else ToHand(pistol, it);
    }

    // arme principale du kit (créée une fois, gardée d'une manche à l'autre)
    static Pistol Primary(int kind, XRDirectInteractor it)
    {
        if (primaryWeapon && primaryKind == kind) return primaryWeapon;
        if (primaryWeapon) Object.Destroy(primaryWeapon.gameObject);
        string name = kind == 1 ? "AK74" : "Sniper";
        var prefab = Resources.Load<GameObject>("Weapons/" + name);
        if (!prefab) { Debug.LogWarning("[Duel] Resources/Weapons/" + name + " introuvable : menu SAE501 → Arène 1v1 → Construire AK74 et sniper"); return null; }
        var go = Object.Instantiate(prefab);
        go.name = name;
        if (kind != 1 && !go.GetComponent<SniperScope>()) go.AddComponent<SniperScope>();      // ancien prefab sans lunette
        var attach = it.attachTransform ? it.attachTransform : it.transform;
        go.transform.SetPositionAndRotation(attach.position, attach.rotation);
        primaryKind = kind;
        return primaryWeapon = go.GetComponent<Pistol>();
    }

    static void ToHand(Pistol weapon, XRDirectInteractor it)
    {
        var grab = weapon.GetComponent<XRGrabInteractable>();
        if (!grab) return;
        if (grab.isSelected) it.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);
        // l'arme va dans la paume (GrabGuard n'accepte qu'une prise à portée de main), puis la main la prend
        var attach = it.attachTransform ? it.attachTransform : it.transform;
        Reset(weapon, attach.position, attach.rotation);
        it.interactionManager.SelectEnter((IXRSelectInteractor)it, (IXRSelectInteractable)grab);
    }

    static void ToHolster(Pistol weapon)
    {
        var grab = weapon.GetComponent<XRGrabInteractable>();
        BeltSocket socket = null;
        foreach (var s in Object.FindObjectsByType<BeltSocket>(FindObjectsSortMode.None)) if (s.accepts == BeltItemType.Pistol) socket = s;
        if (!grab || !socket || !socket.interactionManager) return;
        if (grab.isSelected) socket.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);
        if (socket.hasSelection) socket.interactionManager.CancelInteractorSelection((IXRSelectInteractor)socket);
        var attach = socket.attachTransform ? socket.attachTransform : socket.transform;
        Reset(weapon, attach.position, attach.rotation);
        socket.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)grab);
    }

    static void Reset(Pistol weapon, Vector3 pos, Quaternion rot)
    {
        var rb = weapon.GetComponent<Rigidbody>();
        if (rb) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        weapon.transform.SetPositionAndRotation(pos, rot);
        Physics.SyncTransforms();
    }

    static Pistol FindPistol(string name)
    {
        foreach (var p in Object.FindObjectsByType<Pistol>(FindObjectsSortMode.None))
            if (p.name == name) return p;
        return null;
    }
}
