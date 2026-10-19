using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Contour autour des objets avec lesquels on peut interagir (tout XRGrabInteractable / XRSimpleInteractable de la scène) :
//   - au repos : liseré fin et discret, pour repérer ce qu'on peut prendre ;
//   - main à portée (survol) : liseré plus épais et lumineux ;
//   - objet tenu en main, ou interaction coupée (planche clouée, autre planche déjà en main) : pas de contour.
// Ajouté automatiquement à chaque chargement de scène : rien à régler dans les scènes générées.
// Technique : une copie du maillage, normales lissées (sinon les arêtes d'une boîte s'ouvrent), dessinée gonflée (Outline.shader).
public class InteractableOutline : MonoBehaviour
{
    public static readonly Color Idle = new Color(1f, 0.75f, 0.27f, 1f) * 0.55f;
    public static readonly Color Hover = new Color(1f, 0.82f, 0.4f, 1f);
    const float IdleWidth = 0.0025f, HoverWidth = 0.006f;

    public XRBaseInteractable interactable;
    public Renderer[] targets;                 // renderers à entourer (par défaut : ceux de l'objet et de ses enfants)

    static Material idleMat, hoverMat;
    static readonly Dictionary<Mesh, Mesh> smoothCache = new Dictionary<Mesh, Mesh>();
    readonly List<Renderer> outlines = new List<Renderer>();
    bool shown, hovered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AttachAll();
    }

    static void OnSceneLoaded(Scene s, LoadSceneMode m) => AttachAll();

    static void AttachAll()
    {
        foreach (var it in Object.FindObjectsByType<XRBaseInteractable>(FindObjectsInactive.Include))
        {
            if (it.GetComponent<InteractableOutline>() || it.GetComponent<MonitorOverlayClose>()) continue;   // pas la vue plein écran
            var o = it.gameObject.AddComponent<InteractableOutline>();
            o.interactable = it;
            var lever = it.GetComponentInParent<Lever>();
            if (lever) o.targets = lever.GetComponentsInChildren<Renderer>(true);      // levier (générateur, disjoncteur) : tige et boule
        }
    }

    void Start()
    {
        if (!interactable) interactable = GetComponent<XRBaseInteractable>();
        if (!idleMat)
        {
            var sh = Resources.Load<Shader>("Outline");
            idleMat = new Material(sh) { name = "Contour_Repos" };
            idleMat.SetColor("_Color", Idle); idleMat.SetFloat("_Width", IdleWidth);
            hoverMat = new Material(sh) { name = "Contour_Survol" };
            hoverMat.SetColor("_Color", Hover); hoverMat.SetFloat("_Width", HoverWidth);
        }
        if (targets == null || targets.Length == 0) targets = GetComponentsInChildren<Renderer>(true);
        foreach (var r in targets) Build(r);

        Apply(false, false);
    }

    void Build(Renderer r)
    {
        if (!(r is MeshRenderer) || r.GetComponent<TextMesh>() || r.name.StartsWith("Contour")) return;
        if (r.GetComponentInParent<Nail>()) return;                          // les clous d'une planche : pas de contour
        if (r.isPartOfStaticBatch) return;                                   // maillage fusionné (toute la carte) : contour impossible
        var mf = r.GetComponent<MeshFilter>();
        if (!mf || !mf.sharedMesh) return;
        var g = new GameObject("Contour");
        g.transform.SetParent(r.transform, false);
        g.AddComponent<MeshFilter>().sharedMesh = Smoothed(mf.sharedMesh);
        var mr = g.AddComponent<MeshRenderer>();
        mr.sharedMaterial = idleMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        outlines.Add(mr);
    }

    // copie du maillage (tous les sous-maillages fusionnés) dont les normales sont moyennées par position :
    // gonflé le long de ces normales, le contour reste fermé sur les arêtes vives (planches, caisses)
    static Mesh Smoothed(Mesh src)
    {
        if (smoothCache.TryGetValue(src, out var m)) return m;
        if (!src.isReadable) { smoothCache[src] = src; return src; }        // maillage non lisible : normales d'origine
        var v = src.vertices; var n = src.normals;
        if (n == null || n.Length != v.Length) { smoothCache[src] = src; return src; }
        var sum = new Dictionary<Vector3, Vector3>();
        for (int i = 0; i < v.Length; i++) { sum.TryGetValue(v[i], out var s); sum[v[i]] = s + n[i]; }
        var sn = new Vector3[v.Length];
        for (int i = 0; i < v.Length; i++) sn[i] = sum[v[i]].normalized;
        var tris = new List<int>();
        for (int s = 0; s < src.subMeshCount; s++) tris.AddRange(src.GetTriangles(s));
        m = new Mesh { name = src.name + "_Contour", indexFormat = src.indexFormat };
        m.vertices = v; m.normals = sn; m.SetTriangles(tris, 0);
        m.RecalculateBounds();
        smoothCache[src] = m;
        return m;
    }

    void LateUpdate()
    {
        if (!interactable) return;
        // tenu en main : pas de contour ; rangé dans un support (socket, ceinture) : il reste à prendre, donc contour
        bool inHand = interactable.isSelected && !(interactable.firstInteractorSelecting is XRSocketInteractor);
        bool on = interactable.isActiveAndEnabled && !inHand;
        bool hov = false;                                                    // survolé par une main (pas par son support)
        if (on) foreach (var h in interactable.interactorsHovering) if (!(h is XRSocketInteractor)) { hov = true; break; }
        if (on != shown || hov != hovered) Apply(on, hov);
    }

    void Apply(bool on, bool hov)
    {
        shown = on; hovered = hov;
        foreach (var r in outlines)
        {
            if (!r) continue;
            r.enabled = on && r.transform.parent.GetComponent<Renderer>().enabled;
            r.sharedMaterial = hov ? hoverMat : idleMat;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { smoothCache.Clear(); idleMat = hoverMat = null; }
}
