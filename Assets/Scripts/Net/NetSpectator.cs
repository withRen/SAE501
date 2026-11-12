using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// [Multijoueur, hotfix #155] Mode spectateur de la coop zombie : un joueur mort pendant la vague regarde un coéquipier vivant
// au lieu d'attendre devant un écran de fin. Il réapparaît à la fin de la vague (NetGame.OnBreakStarted → PlayerHealth.Revive).
//   - une caméra suit le coéquipier « par-dessus l'épaule » (derrière et au-dessus de sa tête, sans traverser les murs) ;
//   - au casque : l'image s'affiche sur un grand écran flottant devant le joueur, qui suit doucement son regard. La vue du
//     joueur, elle, ne bouge pas : aucun mouvement de caméra imposé (règle de confort de la SAÉ) ;
//   - en version PC : la caméra du spectateur remplace la vue, plein écran ;
//   - A / X (casque), clic gauche ou Espace (PC) : coéquipier suivant ; le spectateur passe tout seul à un autre joueur vivant
//     si celui qu'il regarde tombe ;
//   - pendant ce temps, le déplacement (marche, téléportation, saut) du joueur mort est coupé.
// Pas utilisé en duel 1v1 (la manche suivante démarre 4 s après la mort) ni en solo (écran de fin, GameOverUI).
public class NetSpectator : MonoBehaviour
{
    static NetSpectator inst;
    public static bool Active => inst && inst.isActiveAndEnabled;

    const int TexW = 960, TexH = 540;
    const float PanelDist = 1.5f, PanelW = 1.5f;

    Camera view;
    RenderTexture rt;
    Transform panel;
    TextMesh caption, panelLabel;
    NetPlayer target;
    Vector3 camPos; bool camInit;
    bool prevButton;
    readonly List<Behaviour> frozen = new List<Behaviour>();
    readonly RaycastHit[] hits = new RaycastHit[16];

    /// <summary>Commence à regarder un coéquipier vivant (rien s'il n'y en a pas).</summary>
    public static void Begin()
    {
        if (Active || !AnyAlive()) return;
        var go = new GameObject("Spectateur");
        inst = go.AddComponent<NetSpectator>();
    }

    /// <summary>Fin du mode spectateur (réapparition, fin de partie).</summary>
    public static void End()
    {
        if (inst) Destroy(inst.gameObject);
        inst = null;
    }

    static bool AnyAlive()
    {
        foreach (var p in NetPlayer.All) if (p && !p.IsOwner && !p.Dead) return true;
        return false;
    }

    void Start()
    {
        bool desktop = DesktopMode.On;
        var camGo = new GameObject("Camera_Spectateur");
        camGo.transform.SetParent(transform, false);
        view = camGo.AddComponent<Camera>();
        view.nearClipPlane = 0.05f; view.farClipPlane = 80f; view.fieldOfView = 70f;
        var data = camGo.AddComponent<UniversalAdditionalCameraData>();
        data.renderShadows = false;
        data.antialiasing = AntialiasingMode.None;

        var main = Camera.main;
        if (desktop)
        {
            view.depth = (main ? main.depth : 0f) + 10f;                     // plein écran, par-dessus la vue du joueur
            caption = Label(view.transform, new Vector3(0f, -0.3f, 1f), 0.0045f);
        }
        else
        {
            rt = new RenderTexture(TexW, TexH, 16) { name = "Spectateur_RT" };
            view.targetTexture = rt;
            BuildPanel(main ? main.transform : null);
        }
        Next(+1);
        Freeze(true);
    }

    void OnDestroy()
    {
        Freeze(false);
        if (panel) Destroy(panel.gameObject);
        if (rt) { rt.Release(); Destroy(rt); }
        if (inst == this) inst = null;
    }

    // ---------- écran flottant (casque) ----------
    void BuildPanel(Transform head)
    {
        panel = new GameObject("Ecran_Spectateur").transform;
        var frame = Quad(panel, "Cadre", new Vector3(PanelW + 0.05f, PanelW * TexH / TexW + 0.05f, 1f), new Color(0.05f, 0.05f, 0.06f, 0.95f), null, 3000);
        frame.transform.localPosition = new Vector3(0f, 0f, 0.005f);
        Quad(panel, "Image", new Vector3(PanelW, PanelW * TexH / TexW, 1f), Color.white, rt, 3001);
        caption = Label(panel, new Vector3(0f, -PanelW * TexH / TexW / 2f - 0.12f, 0f), 0.0075f);
        panelLabel = Label(panel, new Vector3(0f, PanelW * TexH / TexW / 2f + 0.09f, 0f), 0.01f);
        panelLabel.color = new Color(1f, 0.3f, 0.25f);
        panelLabel.text = "TU ES MORT  ·  SPECTATEUR";
        if (head) PlacePanel(head, 1f);
    }

    static GameObject Quad(Transform parent, string n, Vector3 scale, Color c, Texture tex, int queue)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Quad);
        g.name = n;
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localScale = scale;
        var m = new Material(Shader.Find("Sprites/Default")) { color = c, renderQueue = queue };
        if (tex) m.mainTexture = tex;
        var r = g.GetComponent<Renderer>();
        r.sharedMaterial = m;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return g;
    }

    static TextMesh Label(Transform parent, Vector3 pos, float size)
    {
        var go = new GameObject("Texte");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        var t = go.AddComponent<TextMesh>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var mat = new Material(t.font.material) { renderQueue = 3010 };
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        t.fontSize = 64; t.characterSize = size; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center;
        t.color = Color.white;
        return t;
    }

    // l'écran suit le regard (en lacet seulement) quand on s'en écarte de plus de 35° ; k = 1 : placé d'un coup
    void PlacePanel(Transform head, float k)
    {
        var fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (fwd.sqrMagnitude < 1e-4f) fwd = panel.forward;
        fwd.Normalize();
        var cur = Vector3.ProjectOnPlane(panel.position - head.position, Vector3.up);
        bool far = k >= 1f || cur.sqrMagnitude < 1e-4f || Vector3.Angle(cur, fwd) > 35f || Mathf.Abs(cur.magnitude - PanelDist) > 0.5f;
        if (!far && !following) return;
        following = Vector3.Angle(cur, fwd) > 3f;
        var dir = cur.sqrMagnitude > 1e-4f ? Vector3.Slerp(cur.normalized, fwd, k) : fwd;
        var pos = head.position + dir * PanelDist + Vector3.down * 0.1f;
        panel.position = k >= 1f ? pos : Vector3.Lerp(panel.position, pos, k);
        panel.rotation = Quaternion.LookRotation(panel.position - head.position, Vector3.up);
    }
    bool following;

    // ---------- cible ----------
    void Next(int step)
    {
        var alive = new List<NetPlayer>();
        foreach (var p in NetPlayer.All) if (p && !p.IsOwner && !p.Dead) alive.Add(p);
        alive.Sort((a, b) => a.Number.Value.CompareTo(b.Number.Value));
        if (alive.Count == 0) { target = null; return; }
        int i = target ? alive.IndexOf(target) : -1;
        target = alive[((i < 0 ? 0 : i + step) % alive.Count + alive.Count) % alive.Count];
        camInit = false;
        string more = alive.Count > 1 ? (DesktopMode.On ? "   ·   CLIC / ESPACE : joueur suivant" : "   ·   A / X : joueur suivant") : "";
        caption.text = "Tu regardes " + target.DisplayName + more + "\nRéapparition à la fin de la vague";
    }

    bool SwitchPressed()
    {
        bool b = false;
        foreach (var node in new[] { UnityEngine.XR.XRNode.LeftHand, UnityEngine.XR.XRNode.RightHand })
        {
            var d = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(node);
            if (d.isValid && d.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool v) && v) b = true;
        }
        var m = UnityEngine.InputSystem.Mouse.current; var k = UnityEngine.InputSystem.Keyboard.current;
        if (DesktopMode.On && m != null && m.leftButton.isPressed) b = true;
        if (k != null && (k.spaceKey.isPressed || k.rightArrowKey.isPressed)) b = true;
        bool down = b && !prevButton;
        prevButton = b;
        return down;
    }

    void LateUpdate()
    {
        if (PlayerHealth.Instance && !PlayerHealth.Instance.IsDead || GameOverUI.PanelOpen) { End(); return; }
        if (SwitchPressed() || !target || target.Dead) Next(+1);
        if (!target || target.HeadPoint == null) { view.enabled = false; return; }
        view.enabled = true;

        // derrière et au-dessus de la tête du coéquipier, un peu sur la droite ; on se rapproche si un mur est entre les deux
        var head = target.HeadPoint;
        var fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (fwd.sqrMagnitude < 1e-4f) fwd = Vector3.forward;
        fwd.Normalize();
        var right = Vector3.Cross(Vector3.up, fwd);
        var want = head.position - fwd * 1.7f + Vector3.up * 0.45f + right * 0.35f;
        var from = head.position + Vector3.up * 0.15f;
        var dir = want - from; float dist = dir.magnitude;
        int n = Physics.SphereCastNonAlloc(from, 0.12f, dir / dist, hits, dist, ~0, QueryTriggerInteraction.Ignore);
        float best = dist;
        for (int i = 0; i < n; i++)
        {
            var h = hits[i];
            if (h.distance <= 0f || h.collider.attachedRigidbody || h.collider.GetComponentInParent<NetPlayer>()) continue;
            best = Mathf.Min(best, Mathf.Max(0.3f, h.distance - 0.05f));
        }
        want = from + dir / dist * best;
        float k = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 8f);
        camPos = camInit ? Vector3.Lerp(camPos, want, k) : want;
        camInit = true;
        view.transform.SetPositionAndRotation(camPos, Quaternion.LookRotation(head.position + fwd * 3f + Vector3.down * 0.3f - camPos, Vector3.up));

        if (panel && Camera.main) PlacePanel(Camera.main.transform, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 3f));
    }

    // déplacement du joueur mort coupé (marche, téléportation, saut, version PC : DesktopPlayer s'arrête déjà, joueur à terre)
    void Freeze(bool on)
    {
        if (on)
        {
            var rig = PlayerHealth.Instance ? PlayerHealth.Instance.gameObject : null;
            if (!rig) return;
            foreach (var b in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>(false))
                if (b.enabled) { b.enabled = false; frozen.Add(b); }
        }
        else
        {
            foreach (var b in frozen) if (b) b.enabled = true;
            frozen.Clear();
        }
    }
}
