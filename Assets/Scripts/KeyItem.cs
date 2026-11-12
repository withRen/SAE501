using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// [Clé du générateur] Clé que le joueur ramasse (cabane du jardin) : elle ouvre une porte à serrure (KeyLock).
// Étiquette : « Clé de la salle du générateur » flotte au-dessus de la clé quand on la vise (survol) ou qu'on la tient,
// face aux yeux (VR). Sur PC, le texte du réticule reprend displayName (DesktopPlayer.Label).
// HeldByHand : la clé est dans une main du joueur de CE casque (pas posée, pas dans un emplacement) : la serrure l'accepte.
[RequireComponent(typeof(XRGrabInteractable))]
public class KeyItem : MonoBehaviour
{
    public string displayName = "Clé de la salle du générateur";
    public float above = 0.07f;

    XRGrabInteractable grab;
    TextMesh label;
    Transform head;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        var go = new GameObject("Etiquette_Cle");
        // pas enfant de la clé : son échelle et sa rotation ne déforment pas le texte
        label = go.AddComponent<TextMesh>();
        label.text = displayName;
        label.anchor = TextAnchor.LowerCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 48;
        label.characterSize = 0.0025f;
        label.color = new Color(1f, 0.85f, 0.45f);
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = label.font.material;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        go.SetActive(false);
    }

    void OnDestroy() { if (label) Destroy(label.gameObject); }

    /// <summary>Tenue dans une main (VR ou PC), pas dans un emplacement de ceinture.</summary>
    public bool HeldByHand
    {
        get
        {
            if (!grab || !grab.isSelected) return false;
            foreach (var i in grab.interactorsSelecting)
                if (!(i is XRSocketInteractor)) return true;
            return false;
        }
    }

    void LateUpdate()
    {
        bool show = grab && (grab.isHovered || HeldByHand);
        if (label.gameObject.activeSelf != show) label.gameObject.SetActive(show);
        if (!show) return;
        if (!head) { var cam = Camera.main; if (cam) head = cam.transform; else return; }
        var t = label.transform;
        t.position = transform.position + Vector3.up * above;
        t.rotation = head.rotation;                       // à plat face aux yeux, sans inclinaison
    }
}
