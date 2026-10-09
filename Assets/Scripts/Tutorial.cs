using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Tutoriel contextuel dans le monde (première partie seulement).
//   - Se lance tout seul au chargement d'une scène qui contient un WaveManager (rien à ajouter dans la scène).
//   - Pendant le tutoriel, le WaveManager est désactivé : pas de compte à rebours, pas de zombies.
//     À la fin, il est réactivé et la partie commence normalement (compte à rebours de 15 s).
//   - Un panneau suit doucement le regard ; une flèche montre l'objet à utiliser.
//   - Étapes : planche → poser → marteau → clouer → 2 fenêtres → pistolet → cible → recharger → disjoncteur → caméras.
//   - Passer tout le tutoriel : maintenir A + X pendant 2 s. (Éditeur : N = étape suivante, K = tout passer.)
//   - Déjà fait ? Pendant les 10 premières secondes de la partie, maintenir A + X 2 s pour le revoir.
//   - Un futur bouton du menu « Revoir le tutoriel » peut appeler Tutorial.ForceNext() (ou ResetSaved()).
public class Tutorial : MonoBehaviour
{
    const string SaveKey = "sae501.tutorialDone";
    static bool forceNext;

    /// <summary>Le prochain chargement de la scène de jeu relancera le tutoriel.</summary>
    public static void ForceNext() => forceNext = true;
    /// <summary>Oublie que le tutoriel a été fait.</summary>
    public static void ResetSaved() { PlayerPrefs.DeleteKey(SaveKey); PlayerPrefs.Save(); }
    public static bool Done => PlayerPrefs.GetInt(SaveKey, 0) == 1;
    public static bool Running { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Running = false;

    // appelé après Awake/OnEnable des objets de la scène, avant leurs Start
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        Create();
    }

    static void OnSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
    {
        if (Time.frameCount > 1) Create();     // chargements suivants (rejouer, menu → jeu)
    }

    static void Create()
    {
        if (!WaveManager.Instance || FindFirstObjectByType<Tutorial>()) return;
        var t = new GameObject("Tutoriel").AddComponent<Tutorial>();
        t.replayOnly = Done && !forceNext;
        forceNext = false;
        if (!t.replayOnly)
        {
            WaveManager.Instance.enabled = false;   // son Start (compte à rebours, son) attend la fin du tutoriel
            Running = true;                         // et son Update attend aussi (sécurité si quelqu'un le réactive)
        }
    }

    // ───────────────────────── réglages ─────────────────────────
    public float panelDistance = 1.15f;
    public float panelDrop = 0.28f;            // sous le niveau des yeux
    public float holdToSkip = 2f;
    public float replayWindow = 10f;
    public int targetHits = 3;

    bool replayOnly, skipStep, skipAll;
    float holdTimer;
    Transform head, panel, marker, markerArrow;
    TextMesh header, body, footer, markerLabel;
    Transform holdBar;
    AudioSource audioSrc;
    AudioClip beep;
    Material arrowMat;
    readonly HashSet<Plank> startPlanks = new HashSet<Plank>();
    GameObject spareMag;

    void Start()
    {
        head = Camera.main ? Camera.main.transform : null;
        if (!head) { Finish(false); return; }
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false; audioSrc.spatialBlend = 0f; audioSrc.volume = 0.6f;
        beep = MakeBeep();
        PrepareSpareMagazine();
        StartCoroutine(replayOnly ? OfferReplay() : Run());
    }

    void OnDestroy()
    {
        Running = false;
        if (panel) Destroy(panel.gameObject);
        if (marker) Destroy(marker.gameObject);
    }

    // ───────────────────────── déroulé ─────────────────────────
    IEnumerator Run()
    {
        Running = true;
        // compatibilité avec un écran de début : on attend qu'il soit fermé
        while (GameObject.Find("EcranDebut")) yield return null;

        BuildPanel();
        BuildMarker();
        foreach (var p in Plank.All) if (p && p.InSlot) startPlanks.Add(p);

        var pistol = FindFirstObjectByType<Pistol>();
        var pistolGrab = pistol ? pistol.GetComponent<XRGrabInteractable>() : null;
        var hammer = FindFirstObjectByType<Hammer>();
        var station = FindFirstObjectByType<MonitorStation>();
        var sw = FindOpening("SW");
        var se = FindOpening("SE");
        var breaker = FindFirstObjectByType<Breaker>();
        const int total = 10;

        yield return Info("BIENVENUE DANS LE BUNKER",
            "Les zombies vont attaquer par les fenêtres.\nApprenons d'abord à les barricader\net à nous défendre.", 5f);

        yield return Step(1, total, "Prenez une planche :\napprochez la main et appuyez une fois\nsur le bouton latéral (grip).\nUn 2e appui la lâche.",
            () => Plank.Held != null, () => NearestFreePlank(), "PLANCHE");

        yield return Step(2, total, "Approchez la planche d'une fenêtre\n(planche fantôme) et lâchez-la :\nelle se pose toute seule.",
            () => NewPlank(p => p.InSlot), () => NearestFreeSlot(sw, se), "ICI");

        if (hammer)
            yield return Step(3, total, "Gardez la planche en place.\nAvec l'autre main, prenez le marteau\naccroché à votre ceinture (à gauche).",
                () => Hammer.Held || NewPlank(p => p.state == Plank.State.Nailed),
                () => hammer ? hammer.transform.position : (Vector3?)null, "MARTEAU");

        yield return Step(4, total, "Clouez la planche :\nfrappez chaque clou 3 fois.",
            () => NewPlank(p => p.state == Plank.State.Nailed),
            () => NewPlankPosition(), "CLOUS");

        if (sw && se)
            yield return Step(5, total, "Barricadez les deux fenêtres\n(au moins une planche clouée sur chacune).",
                () => sw.IsBarricaded && se.IsBarricaded,
                () => !sw.IsBarricaded ? NearestFreeSlot(sw, null) : !se.IsBarricaded ? NearestFreeSlot(se, null) : null, "FENÊTRE",
                () => "Fenêtres barricadées : " + ((sw.IsBarricaded ? 1 : 0) + (se.IsBarricaded ? 1 : 0)) + " / 2");

        if (pistolGrab)
        {
            yield return Step(6, total, "Prenez le pistolet à votre ceinture (à droite).",
                () => BeltItem.InHand(pistolGrab), () => pistol.transform.position, "PISTOLET");

            var target = SpawnTarget();
            yield return Step(7, total, "Tirez sur la cible avec la gâchette.\nVisez la tête des zombies : un tir suffit.",
                () => target == null || target.Hits >= targetHits,
                () => target ? target.transform.position + Vector3.up * 0.35f : (Vector3?)null, "CIBLE",
                () => target ? "Touches : " + target.Hits + " / " + targetHits : "");
            if (target) target.Pop();

            EnsureMagazineAvailable();
            int ammoBefore = pistol.ammo;
            yield return Step(8, total, "Rechargez : prenez le chargeur\nà votre ceinture (devant)\net glissez-le sous la poignée du pistolet.",
                () => pistol == null || pistol.ammo > ammoBefore,
                () => MagazinePosition(), "CHARGEUR");
            RestoreMagazine();
        }

        // les écrans restent noirs tant que le disjoncteur est coupé (Breaker)
        if (breaker && !Breaker.On)
            yield return Step(9, total, "Allumez les caméras :\nabaissez le levier du disjoncteur\n(main dessus + grip).",
                () => Breaker.On, () => breaker.transform.position + Vector3.up * 0.3f, "DISJONCTEUR");

        if (station)
        {
            yield return Step(10, total, "Les caméras montrent l'extérieur.\nApprochez la main d'un écran\net appuyez sur le grip.",
                () => station.overlay && station.overlay.gameObject.activeSelf,
                () => NearestScreen(station), "CAMÉRAS");
            if (!skipAll && station.overlay && station.overlay.gameObject.activeSelf)
            {
                if (panel) panel.gameObject.SetActive(true);
                SetPanel("CAMÉRAS", "Appuyez à nouveau sur le grip\npour fermer l'image.", false);
                float t0 = Time.time;
                while (station.overlay.gameObject.activeSelf && Time.time - t0 < 8f && !skipAll) yield return null;
            }
        }

        skipAll = false;
        HideMarker();
        yield return Info("PRÊT ?", "Survivez aux vagues de zombies.\nRéparez les barricades entre les vagues.\n\nLa première vague arrive…", 4f);
        Finish(true);
    }

    // proposition de revoir le tutoriel (partie déjà commencée, tutoriel déjà fait)
    IEnumerator OfferReplay()
    {
        while (GameObject.Find("EcranDebut")) yield return null;
        yield return null;   // laisse le WaveManager créer ses textes
        BuildPanel();
        panel.localScale *= 0.75f;
        float t0 = Time.time;
        while (Time.time - t0 < replayWindow && WaveManager.Level == 0)
        {
            SetPanel("TUTORIEL", "Maintenez A + X pour revoir le tutoriel.", false, "", false);
            UpdateHold(out bool held);
            if (held || Pressed(Key.T))
            {
                Destroy(panel.gameObject);
                var wm = WaveManager.Instance;
                if (wm)
                {
                    wm.enabled = false;
                    ResetWaveTimer(wm);
                    ShowWaveTexts(false);
                }
                replayOnly = false;
                yield return Run();
                yield break;
            }
            yield return null;
        }
        Destroy(gameObject);
    }

    void Finish(bool save)
    {
        if (save) { PlayerPrefs.SetInt(SaveKey, 1); PlayerPrefs.Save(); }
        var wm = WaveManager.Instance;
        if (wm) { ResetWaveTimer(wm); wm.enabled = true; ShowWaveTexts(true); }
        var pistol = FindFirstObjectByType<Pistol>();
        if (pistol && pistol.ammo < pistol.maxAmmo) pistol.Load(pistol.maxAmmo);
        if (head) RestoreMagazine();   // toujours un chargeur à la ceinture pour la partie
        if (spareMag) Destroy(spareMag.transform.parent ? spareMag.transform.parent.gameObject : spareMag);
        foreach (var b in FindObjectsByType<TutorialBoard>(FindObjectsSortMode.None)) Destroy(b.gameObject);
        if (panel) Destroy(panel.gameObject);       // panneau et flèche ne sont pas enfants du tutoriel
        if (marker) Destroy(marker.gameObject);
        Running = false;
        enabled = false;
        Destroy(gameObject, 0.5f);                  // laisse le temps de remettre le chargeur à la ceinture
    }

    // ───────────────────────── étapes ─────────────────────────
    IEnumerator Info(string title, string text, float seconds)
    {
        if (panel) panel.gameObject.SetActive(true);
        SetPanel(title, text, true);
        float t0 = Time.time;
        while (Time.time - t0 < seconds && !skipStep && !skipAll) yield return null;
        skipStep = false;
    }

    // une étape : l'explication est sur un panneau posé dans la carte, à côté de l'objet concerné (TutorialBoard)
    IEnumerator Step(int n, int total, string text, Func<bool> done, Func<Vector3?> where, string label, Func<string> progress = null)
    {
        if (skipAll) yield break;
        if (panel) panel.gameObject.SetActive(false);              // le panneau devant les yeux laisse la place au panneau de la carte
        var board = TutorialBoard.Create(head, "ÉTAPE " + n + " / " + total, label, text, where != null ? where() : null);
        while (!done() && !skipStep && !skipAll)
        {
            string extra = progress != null ? progress() : "";
            board.SetText(string.IsNullOrEmpty(extra) ? text : text + "\n\n" + extra);
            var p = where != null ? where() : null;
            board.Point(p);
            if (p.HasValue) ShowMarker(p.Value, label); else HideMarker();
            yield return null;
        }
        skipStep = false;
        HideMarker();
        if (skipAll) { if (board) Destroy(board.gameObject); yield break; }
        // réussite : bip + vibration, panneau vert un instant
        audioSrc.PlayOneShot(beep);
        Haptic(XRNode.LeftHand); Haptic(XRNode.RightHand);
        board.Success();
        yield return new WaitForSeconds(1.2f);
    }

    void Update()
    {
        if (replayOnly || !panel) return;
        UpdateHold(out bool held);
        if (held || Pressed(Key.K)) skipAll = true;
        if (Pressed(Key.N)) skipStep = true;
    }

    void LateUpdate()
    {
        if (!head) return;
        if (panel)
        {
            var fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            if (fwd.sqrMagnitude < 0.01f) fwd = panel.forward;
            fwd.Normalize();
            var goal = head.position + fwd * panelDistance + Vector3.down * panelDrop;
            float k = 1f - Mathf.Exp(-3f * Time.deltaTime);
            panel.position = Vector3.Lerp(panel.position, goal, k);
            panel.rotation = Quaternion.LookRotation(panel.position - head.position);
        }
        if (marker && marker.gameObject.activeSelf)
        {
            markerArrow.localPosition = Vector3.up * (0.06f * Mathf.Sin(Time.time * 4f));
            markerArrow.localRotation = Quaternion.Euler(0f, Time.time * 90f, 0f);
            markerLabel.transform.rotation = Quaternion.LookRotation(markerLabel.transform.position - head.position);
        }
    }

    // ───────────────────────── entrées ─────────────────────────
    void UpdateHold(out bool completed)
    {
        completed = false;
        bool both = Button(XRNode.LeftHand, UnityEngine.XR.CommonUsages.primaryButton) && Button(XRNode.RightHand, UnityEngine.XR.CommonUsages.primaryButton);
        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.isPressed && kb.leftShiftKey.isPressed) both = true;
        holdTimer = both ? holdTimer + Time.deltaTime : 0f;
        if (holdBar)
        {
            float f = Mathf.Clamp01(holdTimer / holdToSkip);
            holdBar.gameObject.SetActive(f > 0f);
            holdBar.localScale = new Vector3(0.9f * f, 0.012f, 1f);
            holdBar.localPosition = new Vector3(-0.45f + 0.45f * f, -0.255f, -0.002f);
        }
        if (holdTimer >= holdToSkip) { holdTimer = -999f; completed = true; }   // relâcher puis rappuyer pour recommencer
    }

    static bool Button(XRNode node, InputFeatureUsage<bool> usage)
    {
        var d = InputDevices.GetDeviceAtXRNode(node);
        return d.isValid && d.TryGetFeatureValue(usage, out bool v) && v;
    }

    static bool Pressed(Key k)
    {
        var kb = Keyboard.current;
        return kb != null && kb[k].wasPressedThisFrame;
    }

    static void Haptic(XRNode node)
    {
        var d = InputDevices.GetDeviceAtXRNode(node);
        if (d.isValid) d.SendHapticImpulse(0, 0.4f, 0.12f);
    }

    // ───────────────────────── recherche d'objets ─────────────────────────
    static Opening FindOpening(string name)
    {
        foreach (var o in Opening.All) if (o && o.openingName == name) return o;
        return null;
    }

    bool NewPlank(Predicate<Plank> cond)
    {
        foreach (var p in Plank.All) if (p && !startPlanks.Contains(p) && cond(p)) return true;
        return false;
    }

    Vector3? NewPlankPosition()
    {
        foreach (var p in Plank.All) if (p && !startPlanks.Contains(p) && p.InSlot) return p.transform.position + Vector3.up * 0.25f;
        return null;
    }

    Vector3? NearestFreePlank()
    {
        Plank best = null; float bd = float.MaxValue;
        foreach (var p in Plank.All)
        {
            if (!p || p.InSlot || !p.gameObject.activeInHierarchy) continue;
            float d = (p.transform.position - head.position).sqrMagnitude;
            if (d < bd) { bd = d; best = p; }
        }
        return best ? best.transform.position + Vector3.up * 0.3f : (Vector3?)null;
    }

    Vector3? NearestFreeSlot(Opening a, Opening b)
    {
        BarricadeSlot best = null; float bd = float.MaxValue;
        foreach (var o in new[] { a, b })
        {
            if (!o || o.slots == null) continue;
            foreach (var s in o.slots)
            {
                if (!s || s.Occupied) continue;
                float d = (s.transform.position - head.position).sqrMagnitude;
                if (d < bd) { bd = d; best = s; }
            }
        }
        if (best) return best.transform.position + Vector3.up * 0.3f;
        var any = a ? a : b;
        return any ? any.transform.position + Vector3.up * 0.5f : (Vector3?)null;
    }

    Vector3? NearestScreen(MonitorStation st)
    {
        MonitorScreen best = null; float bd = float.MaxValue;
        foreach (var s in FindObjectsByType<MonitorScreen>(FindObjectsSortMode.None))
        {
            float d = (s.transform.position - head.position).sqrMagnitude;
            if (d < bd) { bd = d; best = s; }
        }
        return (best ? best.transform.position : st.transform.position) + Vector3.up * 0.4f;
    }

    // ───────────────────────── chargeur de rechange ─────────────────────────
    // Il n'y a qu'un chargeur dans le jeu, et il est détruit quand on l'insère.
    // On en garde une copie cachée pour le remettre à la ceinture après l'étape « recharger ».
    void PrepareSpareMagazine()
    {
        var mag = FindFirstObjectByType<Magazine>();
        if (!mag) return;
        var holder = new GameObject("Tutoriel_ChargeurRechange");
        holder.SetActive(false);
        spareMag = Instantiate(mag.gameObject, holder.transform);
        spareMag.name = mag.gameObject.name;
    }

    Vector3? MagazinePosition()
    {
        var mag = FindFirstObjectByType<Magazine>();
        return mag ? mag.transform.position + Vector3.up * 0.2f : (Vector3?)null;
    }

    void EnsureMagazineAvailable()
    {
        if (!FindFirstObjectByType<Magazine>()) RestoreMagazine();
    }

    void RestoreMagazine()
    {
        if (!spareMag || FindFirstObjectByType<Magazine>()) return;
        BeltSocket socket = null;
        foreach (var s in FindObjectsByType<BeltSocket>(FindObjectsSortMode.None))
            if (s.accepts == BeltItemType.Magazine) { socket = s; break; }
        var at = socket ? (socket.attachTransform ? socket.attachTransform : socket.transform) : head;
        // copie du modèle caché (le modèle reste disponible pour une prochaine fois)
        var mag = Instantiate(spareMag, at.position, at.rotation);
        mag.name = spareMag.name;
        mag.SetActive(true);
        if (socket) StartCoroutine(PutInSocket(socket, mag.GetComponent<XRGrabInteractable>()));
    }

    IEnumerator PutInSocket(BeltSocket socket, XRGrabInteractable grab)
    {
        yield return null;
        if (!socket || !grab || grab.isSelected) yield break;
        var mgr = socket.interactionManager ? socket.interactionManager : grab.interactionManager;
        if (mgr && !socket.hasSelection) mgr.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)grab);
    }

    // ───────────────────────── cible d'entraînement ─────────────────────────
    TutorialTarget SpawnTarget()
    {
        var fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        fwd.Normalize();
        float dist = 3.5f;
        var eye = new Vector3(head.position.x, head.position.y, head.position.z);
        if (Physics.Raycast(eye, fwd, out var hit, dist + 0.3f, ~0, QueryTriggerInteraction.Ignore))
            dist = Mathf.Max(1.5f, hit.distance - 0.3f);
        var floorY = head.position.y - 1.6f;
        if (Physics.Raycast(head.position, Vector3.down, out var floor, 3f, ~0, QueryTriggerInteraction.Ignore)) floorY = floor.point.y;
        var pos = head.position + fwd * dist;
        pos.y = floorY + 1.4f;
        var go = new GameObject("Tutoriel_Cible");
        go.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(-fwd));
        var t = go.AddComponent<TutorialTarget>();
        t.Build(ArrowMaterial(Color.white).shader);
        return t;
    }

    // ───────────────────────── affichage ─────────────────────────
    void BuildPanel()
    {
        panel = new GameObject("Tutoriel_Panneau").transform;
        var fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        panel.position = head.position + fwd * panelDistance + Vector3.down * panelDrop;
        panel.rotation = Quaternion.LookRotation(panel.position - head.position);

        Quad(panel, "Fond", new Vector2(0f, 0f), new Vector2(1.0f, 0.58f), new Color(0.02f, 0.05f, 0.04f, 0.88f), 4250, 0.004f);
        var amber = new Color(1f, 0.75f, 0.27f);
        Quad(panel, "Bande", new Vector2(0f, 0.262f), new Vector2(1.0f, 0.055f), new Color(amber.r, amber.g, amber.b, 0.9f), 4260, 0.002f);
        Quad(panel, "Ligne", new Vector2(0f, -0.29f), new Vector2(1.0f, 0.004f), amber, 4260, 0.002f);
        header = Text(panel, "Titre", new Vector2(-0.48f, 0.262f), 0.0055f, new Color(0.02f, 0.05f, 0.03f), TextAnchor.MiddleLeft, true);
        body = Text(panel, "Texte", new Vector2(0f, 0.04f), 0.0068f, new Color(0.9f, 0.94f, 0.92f), TextAnchor.MiddleCenter, false);
        footer = Text(panel, "Aide", new Vector2(0f, -0.215f), 0.0042f, new Color(0.47f, 0.59f, 0.51f), TextAnchor.MiddleCenter, false);
        holdBar = Quad(panel, "Maintien", Vector2.zero, Vector2.one, amber, 4270, 0f).transform;
        holdBar.gameObject.SetActive(false);
    }

    void SetPanel(string title, string text, bool showSkipHint, string footerOverride = null, bool success = false)
    {
        if (!panel) return;
        if (header.text != title) header.text = title;
        if (body.text != text) body.text = text;
        body.color = success ? new Color(0.35f, 1f, 0.55f) : new Color(0.9f, 0.94f, 0.92f);
        string f = footerOverride ?? (showSkipHint ? "Maintenez A + X pendant 2 s pour passer le tutoriel" : "");
        if (footer.text != f) footer.text = f;
    }

    void BuildMarker()
    {
        marker = new GameObject("Tutoriel_Fleche").transform;
        markerArrow = new GameObject("Pointe").transform;
        markerArrow.SetParent(marker, false);
        var mf = markerArrow.gameObject.AddComponent<MeshFilter>();
        mf.sharedMesh = ArrowMesh();
        var mr = markerArrow.gameObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = ArrowMaterial(new Color(1f, 0.75f, 0.27f));
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        markerLabel = Text(marker, "Etiquette", new Vector2(0f, 0.2f), 0.004f, new Color(1f, 0.8f, 0.35f), TextAnchor.LowerCenter, true, 3100);
        marker.gameObject.SetActive(false);
    }

    void ShowMarker(Vector3 pos, string label)
    {
        if (!marker) return;
        marker.gameObject.SetActive(true);
        marker.position = pos;
        if (markerLabel.text != label) markerLabel.text = label;
    }

    void HideMarker() { if (marker) marker.gameObject.SetActive(false); }

    void ShowWaveTexts(bool show)
    {
        if (!head) return;
        var s = head.Find("Statut");
        if (s) s.gameObject.SetActive(show);
    }

    static void ResetWaveTimer(WaveManager wm)
    {
        if (WaveManager.Level != 0) return;
        var f = typeof(WaveManager).GetField("timer", BindingFlags.Instance | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(float)) f.SetValue(wm, wm.firstDelay);
    }

    // pyramide à 4 faces, pointe vers le bas (origine = pointe)
    static Mesh ArrowMesh()
    {
        float w = 0.07f, h = 0.14f;
        var tip = Vector3.zero;
        var a = new Vector3(-w, h, -w); var b = new Vector3(w, h, -w); var c = new Vector3(w, h, w); var d = new Vector3(-w, h, w);
        var v = new[] { tip, b, a, tip, c, b, tip, d, c, tip, a, d, a, b, c, a, c, d };
        var tris = new int[v.Length];
        for (int i = 0; i < tris.Length; i++) tris[i] = i;
        var m = new Mesh { name = "FlecheTuto", vertices = v, triangles = tris };
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    Material ArrowMaterial(Color c)
    {
        if (arrowMat && arrowMat.color == c) return arrowMat;
        var sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (!sh) sh = Shader.Find("Sprites/Default");
        var m = new Material(sh);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        m.color = c;
        if (!arrowMat) arrowMat = m;
        return m;
    }

    static Renderer Quad(Transform parent, string name, Vector2 pos, Vector2 size, Color c, int queue, float z)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Quad);
        g.name = name;
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = new Vector3(pos.x, pos.y, z);
        g.transform.localScale = new Vector3(size.x, size.y, 1f);
        var r = g.GetComponent<Renderer>();
        var sh = Shader.Find("UI/Default");
        if (!sh) sh = Shader.Find("Sprites/Default");
        var m = new Material(sh) { color = c, renderQueue = queue };
        m.SetFloat("unity_GUIZTestMode", (float)UnityEngine.Rendering.CompareFunction.Always);   // reste visible même près d'un mur
        r.material = m;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return r;
    }

    static TextMesh Text(Transform parent, string name, Vector2 pos, float charSize, Color c, TextAnchor anchor, bool bold, int queue = 4300)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        var tm = g.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64;
        tm.characterSize = charSize;
        tm.anchor = anchor;
        tm.alignment = anchor == TextAnchor.MiddleLeft ? TextAlignment.Left : TextAlignment.Center;
        tm.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        tm.color = c;
        var mr = g.GetComponent<MeshRenderer>();
        mr.sharedMaterial = tm.font.material;
        mr.material.renderQueue = queue;
        mr.material.SetFloat("unity_GUIZTestMode", (float)UnityEngine.Rendering.CompareFunction.Always);
        return tm;
    }

    static AudioClip MakeBeep()
    {
        const int rate = 44100;
        int n = (int)(rate * 0.18f);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            float f = t < 0.08f ? 880f : 1320f;                 // deux notes montantes
            float env = Mathf.Clamp01(1f - t / 0.18f) * Mathf.Clamp01(t * 200f);
            data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * 0.35f * env;
        }
        var clip = AudioClip.Create("BipTuto", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}

// Cible d'entraînement du tutoriel : reçoit OnShot du pistolet (Pistol.Fire → SendMessage).
public class TutorialTarget : MonoBehaviour
{
    public int Hits { get; private set; }
    Renderer center;
    AudioSource src;
    AudioClip ding;

    public void Build(Shader shader)
    {
        // anneaux rouge / blanc / rouge, face au joueur (le disque est dans le plan XY local)
        Disc("Anneau_Ext", 0.5f, 0.00f, new Color(0.85f, 0.12f, 0.1f), shader);
        Disc("Anneau_Mil", 0.34f, -0.01f, new Color(0.95f, 0.95f, 0.92f), shader);
        center = Disc("Centre", 0.17f, -0.02f, new Color(0.85f, 0.12f, 0.1f), shader);
        // pied
        var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leg.name = "Pied";
        Destroy(leg.GetComponent<Collider>());
        leg.transform.SetParent(transform, false);
        leg.transform.localPosition = new Vector3(0f, -0.75f, 0.03f);
        leg.transform.localScale = new Vector3(0.05f, 1.3f, 0.05f);
        leg.GetComponent<Renderer>().material = Mat(new Color(0.35f, 0.25f, 0.15f), shader);

        var box = gameObject.AddComponent<BoxCollider>();     // collider normal (le tir ignore les triggers)
        box.size = new Vector3(0.5f, 0.5f, 0.06f);

        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f; src.playOnAwake = false;
        ding = MakeDing();
    }

    Renderer Disc(string name, float diameter, float z, Color c, Shader shader)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        g.name = name;
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(transform, false);
        g.transform.localPosition = new Vector3(0f, 0f, z);
        g.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        g.transform.localScale = new Vector3(diameter, 0.015f, diameter);
        var r = g.GetComponent<Renderer>();
        r.material = Mat(c, shader);
        return r;
    }

    static Material Mat(Color c, Shader shader)
    {
        var m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        m.color = c;
        return m;
    }

    // appelé par Pistol.Fire via SendMessage
    void OnShot(RaycastHit hit)
    {
        Hits++;
        if (src && ding) src.PlayOneShot(ding);
        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        var m = center.material;
        var c = new Color(0.85f, 0.12f, 0.1f);
        Set(m, new Color(1f, 0.9f, 0.3f));
        transform.localScale = Vector3.one * 1.08f;
        yield return new WaitForSeconds(0.12f);
        Set(m, c);
        transform.localScale = Vector3.one;
    }

    static void Set(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        m.color = c;
    }

    /// <summary>La cible rétrécit puis disparaît.</summary>
    public void Pop() { if (this) StartCoroutine(PopRoutine()); }

    IEnumerator PopRoutine()
    {
        var s0 = transform.localScale;
        for (float t = 0; t < 0.4f; t += Time.deltaTime)
        {
            transform.localScale = s0 * (1f - t / 0.4f);
            yield return null;
        }
        Destroy(gameObject);
    }

    static AudioClip MakeDing()
    {
        const int rate = 44100;
        int n = (int)(rate * 0.25f);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            data[i] = Mathf.Sin(2f * Mathf.PI * 1600f * t) * Mathf.Exp(-t * 18f) * 0.5f;
        }
        var clip = AudioClip.Create("DingCible", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
