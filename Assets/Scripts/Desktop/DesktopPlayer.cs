using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Version PC : pilote le XR Origin du jeu au clavier et à la souris (ajouté par DesktopMode, jamais au casque).
// On garde le même rig et les mêmes mains XRI qu'au casque : les objets, leviers, portes, boutons et armes reçoivent les
// mêmes événements (prise, utilisation) qu'avec les manettes, donc toutes les règles du jeu restent celles de la version VR.
// Seule la façon de placer les mains change :
//   - la caméra suit la souris (verrouillée au centre), le corps se déplace avec ZQSD (touches physiques W A S D) ;
//   - l'objet tenu est placé « en vue FPS » (bas droite pour l'arme, bas gauche pour la lampe) ; la main se pose dessus
//     (on calcule la position de la manette pour que l'objet soit à sa place et que la main le serre) ;
//   - le tir part du centre de l'écran (réticule) : Pistol utilise le rayon de la caméra en version PC ;
//   - E = interagir avec ce que vise le réticule (prendre, poser une planche, levier, porte, bouton, écran) ;
//   - marteau et armes de mêlée : le clic lance un vrai coup (la tête de l'outil va jusqu'au point visé et revient),
//     les clous et les zombies le détectent comme un coup donné au casque.
// Écran d'aide : DesktopHud (réticule, « E - Prendre… », munitions). Menu d'accueil : caméra fixe et souris libre.
[DefaultExecutionOrder(-200)]           // avant le gestionnaire d'interactions XRI : les objets tenus suivent la main dans la même image
public class DesktopPlayer : MonoBehaviour
{
    public float eyeHeight = 1.65f;
    public float reach = 2.2f;              // distance maximale pour prendre un objet ou appuyer sur un bouton
    public float swingDuration = 0.34f;     // coup de marteau / d'arme : aller-retour

    public enum Kind { None, Pistol, Hammer, Melee, Lamp, Plank, Magazine, Battery, Other }

    // ───────────────────────── une main ─────────────────────────
    class Hand
    {
        public bool right;
        public Transform ctrl, attach;
        public XRDirectInteractor it;
        public Animator anim;
        public Renderer[] renderers;
        public bool activating;
        public int useFrame;
        public float grip, trigger;
        // objet tenu : pose de départ (repère caméra) pour l'animation de prise
        public Vector3 fromPos; public Quaternion fromRot; public float pickup = 1f;
        // main sans objet qui va vers un point (levier, porte, bouton) ; null : main au repos (cachée)
        public Vector3? target;

        public IXRSelectInteractable Held => it && it.hasSelection ? it.firstInteractableSelected : null;
        public XRGrabInteractable Grab => Held as XRGrabInteractable;
        public T Item<T>() where T : Component { var g = Grab; var c = g ? g.GetComponent<T>() : null; return c ? c : null; }
    }

    XROrigin origin;
    CharacterController cc;
    Camera cam;
    Transform head;
    Hand rightH, leftH;
    float yaw, pitch, baseFov;
    bool menuScene, wasPaused;
    float pausePitch, pauseYaw;
    PauseMenu pauseMenu;
    MainMenu mainMenu;
    float moveSpeed = 2.5f;

    // visée
    Vector3 aimPoint;
    float aimDistance;
    IXRSelectInteractable hover;            // ce que E ferait maintenant
    string hoverText;
    BarricadeSlot aimedSlot;                // planche en main : emplacement visé
    bool aiming;                            // clic droit (pistolet)
    float aimBlend;
    bool inspecting;

    // actions en cours
    float swing = -1f;                      // coup (0..1), < 0 : pas de coup
    Vector3 swingTarget; bool swingOnNail;
    float recoil;
    Lever lever; float leverGoal;
    HingedDoor door; float doorGoal; float doorTime;
    IXRSelectInteractable press; float pressT = -1f; Vector3 pressPoint;
    float reload = -1f; MagWell reloadWell; bool lampWasInLeft;
    float bob, bobAmount;
    Vector2 sway;

    // poche (version PC) : marteau et arme de mêlée rangés hors de la main (pas de holster pour eux au casque)
    readonly List<XRGrabInteractable> pocket = new List<XRGrabInteractable>();
    BeltSocket[] sockets = new BeltSocket[0];

    public static DesktopPlayer Instance { get; private set; }
    public bool MenuScene => menuScene;
    public Vector3 AimPoint => aimPoint;
    public string HoverText => hoverText;
    public bool Hovering => hover != null || aimedSlot != null;
    public Kind RightKind => KindOf(rightH?.Grab);
    public Kind LeftKind => KindOf(leftH?.Grab);
    public Pistol HeldPistol => rightH != null ? rightH.Item<Pistol>() : null;
    public Flashlight HeldLamp { get { var l = leftH?.Item<Flashlight>(); return l ? l : rightH?.Item<Flashlight>(); } }
    public bool Aiming => aiming && HeldPistol;
    public bool Inspecting => inspecting;

    void Awake()
    {
        Instance = this;
        origin = GetComponent<XROrigin>();
        cc = GetComponent<CharacterController>();
        cam = origin.Camera ? origin.Camera : GetComponentInChildren<Camera>(true);
        head = cam.transform;
        mainMenu = FindFirstObjectByType<MainMenu>();
        menuScene = mainMenu != null;

        origin.CameraYOffset = eyeHeight;       // lu par Crouch au démarrage (hauteur debout)
        var offset = origin.CameraFloorOffsetObject ? origin.CameraFloorOffsetObject.transform : null;
        if (offset) { var lp = offset.localPosition; lp.y = eyeHeight; offset.localPosition = lp; }

        // plus de suivi du casque ni des manettes : c'est ce script qui place la tête et les mains
        foreach (var tpd in GetComponentsInChildren<UnityEngine.InputSystem.XR.TrackedPoseDriver>(true)) tpd.enabled = false;
        head.localPosition = Vector3.zero;
        yaw = head.eulerAngles.y - transform.eulerAngles.y;
        baseFov = GameSettings.FieldOfView;
        cam.fieldOfView = baseFov;
        cam.nearClipPlane = Mathf.Min(cam.nearClipPlane, 0.03f);

        // sans manettes suivies, XRInputModalityManager éteint les deux mains : on le coupe
        var modality = GetComponent<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>();
        if (modality) modality.enabled = false;
        var move = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.ContinuousMoveProvider>(true);
        if (move) moveSpeed = move.moveSpeed;

        if (menuScene) return;                  // menu d'accueil : pas de mains, caméra fixe (voir MenuUpdate)
        rightH = MakeHand("Right Controller", true);
        leftH = MakeHand("Left Controller", false);
        DesktopHud.Create(this);
    }

    Hand MakeHand(string name, bool right)
    {
        Transform ctrl = null;
        foreach (var t in GetComponentsInChildren<Transform>(true)) if (t.name == name) { ctrl = t; break; }
        if (!ctrl) return null;
        ctrl.gameObject.SetActive(true);
        var h = new Hand { right = right, ctrl = ctrl, it = ctrl.GetComponentInChildren<XRDirectInteractor>(true) };
        if (!h.it) return null;
        h.attach = h.it.attachTransform ? h.it.attachTransform : h.it.transform;
        // les boutons « prendre » et « utiliser » sont envoyés par ce script, plus par les manettes
        h.it.selectInput.inputSourceMode = XRInputButtonReader.InputSourceMode.ManualValue;
        h.it.activateInput.inputSourceMode = XRInputButtonReader.InputSourceMode.ManualValue;
        h.it.selectActionTrigger = UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor.InputTriggerType.Toggle;
        // la main est animée ici (poing fermé quand elle tient un objet), plus par les gâchettes des manettes
        foreach (var a in ctrl.GetComponentsInChildren<AnimateHandOnInput>(true)) a.enabled = false;
        h.anim = ctrl.GetComponentInChildren<Animator>(true);
        var model = ctrl.GetComponentInChildren<AnimateHandOnInput>(true);
        h.renderers = model ? model.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
        return h;
    }

    void Start()
    {
        sockets = FindObjectsByType<BeltSocket>(FindObjectsSortMode.None);
        LockCursor(!menuScene);
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (Instance == this) Instance = null;
    }

    static void LockCursor(bool on)
    {
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }

    /// <summary>La souris pilote la caméra (verrouillée) : faux en pause, au menu ou fenêtre pas encore cliquée.</summary>
    public bool MouseCaptured => Cursor.lockState == CursorLockMode.Locked;

    /// <summary>La partie est finie (mort ou victoire) : plus de commandes.</summary>
    public static bool GameOver => PlayerHealth.Instance && PlayerHealth.Instance.IsDead || WaveManager.Instance && WaveManager.Instance.Victory;

    // ───────────────────────── boucle ─────────────────────────
    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;
        if (menuScene) { MenuUpdate(); return; }
        if (rightH == null || leftH == null) return;

        bool paused = PauseMenu.Paused;
        bool gameOverPanel = GameOverUI.PanelOpen;
        bool clickConsumed = false;
        if (paused || gameOverPanel) { if (MouseCaptured) LockCursor(false); }
        else if (wasPaused && !GameOver && Application.isFocused) { LockCursor(true); clickConsumed = true; }   // reprise (bouton ou Échap)
        else if (!MouseCaptured && !GameOver && mouse.leftButton.wasPressedThisFrame && Application.isFocused)
        {
            LockCursor(true);                   // 1er clic dans la fenêtre (ou après la pause) : capture la souris
            clickConsumed = true;
        }
        else if (!MouseCaptured && !GameOver && Time.frameCount < 5) LockCursor(true);

        // pause ou écran de fin : la vue se tourne vers le panneau (la souris sert de pointeur), puis revient où l'on visait
        if (paused && !wasPaused) { pausePitch = pitch; pauseYaw = yaw; pauseMenu = FindAnyObjectByType<PauseMenu>(); }
        if (paused && pauseMenu)
        {
            var d = Quaternion.Inverse(transform.rotation) * (pauseMenu.transform.position - head.position);
            float k = 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime);
            pitch = Mathf.LerpAngle(pitch, -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg, k);
            yaw = Mathf.LerpAngle(yaw, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, k);
        }
        else if (gameOverPanel && GameOverUI.Instance && GameOverUI.Instance.PanelTransform)
        {
            var d = Quaternion.Inverse(transform.rotation) * (GameOverUI.Instance.PanelTransform.position - head.position);
            float k = 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime);
            pitch = Mathf.LerpAngle(pitch, -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg, k);
            yaw = Mathf.LerpAngle(yaw, Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, k);
        }
        if (!paused && wasPaused) { pitch = pausePitch; yaw = pauseYaw; }
        wasPaused = paused;

        bool control = !paused && !GameOver && enabled;
        if (control && MouseCaptured) Look(mouse);
        head.localPosition = Vector3.zero;
        head.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        if (control) Move(kb);
        Aim();

        if (control && MouseCaptured)
        {
            if (kb.eKey.wasPressedThisFrame) Interact();
            if (!clickConsumed && mouse.leftButton.wasPressedThisFrame) Primary();
            if (kb.rKey.wasPressedThisFrame) Reload();
            if (kb.fKey.wasPressedThisFrame) ToggleLamp();
            if (kb.gKey.wasPressedThisFrame) Drop(rightH);
            if (kb.digit1Key.wasPressedThisFrame) Select(1);
            if (kb.digit2Key.wasPressedThisFrame) Select(2);
            if (kb.digit3Key.wasPressedThisFrame) Select(3);
            if (kb.digit4Key.wasPressedThisFrame) Select(4);
            float wheel = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(wheel) > 0.1f) Cycle(wheel < 0f ? 1 : -1);
            aiming = mouse.rightButton.isPressed;
            inspecting = kb.vKey.isPressed;
        }
        else { aiming = false; inspecting = false; }
        if (!kb.eKey.isPressed || !control) { if (lever) ReleaseLever(); }

        if (rightH.activating && Time.frameCount > rightH.useFrame + 1) StopUse(rightH);
        if (leftH.activating && Time.frameCount > leftH.useFrame + 1) StopUse(leftH);

        Tick();
        PlaceHand(rightH);
        PlaceHand(leftH);
        UpdatePocket();
        // champ de vision : un peu resserré quand on vise au pistolet (clic droit)
        aimBlend = Mathf.MoveTowards(aimBlend, Aiming ? 1f : 0f, Time.deltaTime * 6f);
        cam.fieldOfView = Mathf.Lerp(baseFov, baseFov * 0.8f, Smooth(aimBlend));
    }

    void OnApplicationFocus(bool focus)
    {
        if (!focus && !menuScene) LockCursor(false);     // Alt+Tab : la souris est rendue au système
    }

    /// <summary>Applique le champ de vision des PARAMÈTRES (GameSettings.Apply).</summary>
    public void ApplySettings()
    {
        baseFov = GameSettings.FieldOfView;
        if (cam && !menuScene) cam.fieldOfView = baseFov;
    }

    // ───────────────────────── menu d'accueil ─────────────────────────
    // caméra fixe, tournée vers le panneau ; la souris est libre et sert de pointeur (MenuPointer)
    void MenuUpdate()
    {
        if (MouseCaptured) LockCursor(false);
        var target = mainMenu ? mainMenu.transform.position : head.position + Vector3.forward;
        if (mainMenu && mainMenu.transform.position == Vector3.zero) target = mainMenu.position;
        var d = Quaternion.Inverse(transform.rotation) * (target - head.position);
        // légère parallaxe à la souris : le décor bouge un peu, le panneau reste lisible
        var m = Mouse.current.position.ReadValue();
        var c = new Vector2(Screen.width, Screen.height) * 0.5f;
        var p = c.sqrMagnitude > 0f ? new Vector2((m.x - c.x) / c.x, (m.y - c.y) / c.y) : Vector2.zero;
        float ty = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg + Mathf.Clamp(p.x, -1f, 1f) * 1.5f;
        float tp = -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg - Mathf.Clamp(p.y, -1f, 1f) * 1f;
        float k = Time.frameCount < 3 ? 1f : 1f - Mathf.Exp(-4f * Time.deltaTime);
        yaw = Mathf.LerpAngle(yaw, ty, k);
        pitch = Mathf.LerpAngle(pitch, tp, k);
        head.localPosition = Vector3.zero;
        head.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // ───────────────────────── tête et corps ─────────────────────────
    void Look(Mouse mouse)
    {
        var d = mouse.delta.ReadValue() * (0.06f * GameSettings.MouseSensitivity);
        if (GameSettings.InvertMouseY) d.y = -d.y;
        yaw += d.x;
        pitch = Mathf.Clamp(pitch - d.y, -85f, 85f);
        sway = Vector2.Lerp(sway, Vector2.ClampMagnitude(d, 3f), 1f - Mathf.Exp(-10f * Time.deltaTime));
    }

    /// <summary>Tourne la tête vers un point (tests en mode Play, sans souris).</summary>
    public void LookAt(Vector3 point)
    {
        var d = Quaternion.Inverse(transform.rotation) * (point - head.position);
        yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(-Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg, -85f, 85f);
        head.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void Move(Keyboard kb)
    {
        var input = Vector3.zero;
        if (kb.wKey.isPressed) input.z += 1f;           // Z sur AZERTY (touches physiques)
        if (kb.sKey.isPressed) input.z -= 1f;
        if (kb.aKey.isPressed) input.x -= 1f;           // Q sur AZERTY
        if (kb.dKey.isPressed) input.x += 1f;
        bool moving = input != Vector3.zero && cc && cc.enabled;
        bobAmount = Mathf.MoveTowards(bobAmount, moving ? 1f : 0f, Time.deltaTime * 4f);
        if (!moving) return;
        float speed = moveSpeed * (Crouch.Crouching ? 0.5f : 1f);
        bob += Time.deltaTime * speed * 4.2f;
        var dir = Quaternion.Euler(0f, head.eulerAngles.y, 0f) * input.normalized;
        cc.Move(dir * speed * Time.deltaTime);          // la gravité et le saut (Espace) restent ceux du rig (XRI)
    }

    // ───────────────────────── visée (réticule) ─────────────────────────
    void Aim()
    {
        var ray = new Ray(head.position, head.forward);
        aimPoint = ray.GetPoint(30f);
        aimDistance = 30f;
        foreach (var hit in Sorted(Physics.RaycastAll(ray, 60f, ~0, QueryTriggerInteraction.Ignore)))
        {
            if (IsMine(hit.collider)) continue;
            aimPoint = hit.point; aimDistance = hit.distance;
            break;
        }
        hover = null; hoverText = null; aimedSlot = null;
        if (PauseMenu.Paused || GameOver) { InteractableOutline.PointerHover = null; return; }

        var station = OpenStation();
        if (station) { hoverText = "Fermer l'écran"; InteractableOutline.PointerHover = null; return; }

        if (Plank.Held && rightH.Item<Plank>() == Plank.Held)
        {
            aimedSlot = SlotAt(ray);
            if (aimedSlot || Plank.Held.state == Plank.State.Snapped) hoverText = "Poser la planche";
        }
        if (aimedSlot == null && Pick(out var target, out _))
        {
            hover = target;
            hoverText = Label(target);
        }
        InteractableOutline.PointerHover = hover as UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable;
    }

    // emplacement de planche libre visé (à portée), le plus proche du rayon
    BarricadeSlot SlotAt(Ray ray)
    {
        BarricadeSlot best = null; float bd = 0.55f;
        foreach (var s in BarricadeSlot.All)
        {
            if (!s || s.Occupied || !s.isActiveAndEnabled) continue;
            var to = s.transform.position - ray.origin;
            float along = Vector3.Dot(to, ray.direction);
            if (along < 0.3f || along > reach + 0.4f) continue;
            float off = Vector3.Distance(s.transform.position, ray.GetPoint(along));
            if (off < bd) { bd = off; best = s; }
        }
        return best;
    }

    // ───────────────────────── actions (publiques : testables en mode Play sans clavier) ─────────────────────────
    /// <summary>E : interagir avec ce que vise le réticule.</summary>
    public void Interact()
    {
        var station = OpenStation();
        if (station) { station.Hide(); return; }
        if (door) { door = null; ReleaseRight(); return; }                       // 2e appui : on lâche la porte

        // planche en main : la poser sur l'emplacement visé (ou celui où elle est déjà calée)
        var plank = rightH.Item<Plank>();
        if (plank && (aimedSlot || plank.state == Plank.State.Snapped))
        {
            if (aimedSlot && plank.state != Plank.State.Snapped) plank.transform.SetPositionAndRotation(aimedSlot.transform.position, aimedSlot.transform.rotation);
            ReleaseRight();
            return;
        }

        if (!Pick(out var target, out var point)) return;
        if (target is XRGrabInteractable g) { Take(g, point); return; }

        var t = target.transform;
        var lv = t.GetComponentInParent<Lever>();
        if (lv) { GrabLever(lv, target); return; }
        var hd = t.GetComponentInParent<HingedDoor>();
        if (hd) { GrabDoor(hd, target); return; }
        // bouton, écran, radio : la main va appuyer dessus
        if (pressT < 0f) { press = target; pressT = 0f; pressPoint = point; }
    }

    /// <summary>Clic gauche : tirer, frapper, utiliser l'objet en main.</summary>
    public void Primary()
    {
        if (reload >= 0f) return;
        var pistol = rightH.Item<Pistol>();
        if (pistol)
        {
            pistol.Fire(rightH.it);
            if (pistol.ammo >= 0) recoil = 1f;
            rightH.trigger = 1f;
            return;
        }
        if (rightH.Item<Hammer>() || rightH.Item<MeleeWeapon>()) { StartSwing(); return; }
        if (rightH.Item<Battery>()) { RechargeLamp(); return; }
        if (rightH.Item<Flashlight>()) { Use(rightH); return; }
    }

    /// <summary>R : recharger le pistolet (chargeur de la ceinture), ou la lampe avec une pile tenue.</summary>
    public void Reload()
    {
        if (rightH.Item<Battery>() || leftH.Item<Battery>()) { RechargeLamp(); return; }
        var pistol = rightH.Item<Pistol>();
        if (!pistol || reload >= 0f) return;
        if (pistol.ammo >= pistol.maxAmmo) { DesktopHud.Message("Chargeur déjà plein"); return; }
        reloadWell = pistol.GetComponentInChildren<MagWell>();
        // chargeur : main gauche, ceinture, ou posé à portée
        var mag = leftH.Item<Magazine>();
        XRGrabInteractable magGrab = mag ? mag.GetComponent<XRGrabInteractable>() : null;
        if (!magGrab)
        {
            var s = Socket(BeltItemType.Magazine);
            magGrab = s && s.hasSelection ? s.firstInteractableSelected as XRGrabInteractable : null;
            if (!magGrab) { var m = Nearest<Magazine>(reach); if (m) magGrab = m.GetComponent<XRGrabInteractable>(); }
        }
        if (!magGrab) { DesktopHud.Message("Aucun chargeur à la ceinture"); return; }
        if (!reloadWell)
        {
            // pas de puits (ancien modèle) : chargement direct, comme MagWell
            var m = magGrab.GetComponent<Magazine>();
            int left = pistol.Load(m.rounds); if (left > 0) m.rounds = left; else m.Consume();
            return;
        }
        // la lampe de la main gauche retourne à la ceinture le temps de recharger
        lampWasInLeft = leftH.Item<Flashlight>() != null;
        if (leftH.Held != null && !leftH.Item<Magazine>()) Release(leftH);
        if (leftH.Held == null) Grab(leftH, magGrab, Center(magGrab));
        if (!leftH.Item<Magazine>()) { DesktopHud.Message("Impossible de prendre le chargeur"); return; }
        reload = 0f;
    }

    /// <summary>F : allumer / éteindre la lampe tenue.</summary>
    public void ToggleLamp()
    {
        if (leftH.Item<Flashlight>()) Use(leftH);
        else if (rightH.Item<Flashlight>()) Use(rightH);
        else Select(3);
    }

    /// <summary>1 pistolet, 2 marteau, 3 lampe (main gauche), 4 arme de mêlée : sortir l'objet, ou le ranger s'il est en main.</summary>
    public void Select(int slot)
    {
        if (reload >= 0f || swing >= 0f) return;
        if (slot == 3) { LampToggleHand(); return; }
        var cur = rightH.Grab;
        var want = SlotItem(slot);
        if (cur && want == cur) { Stow(rightH); return; }
        if (!want) { DesktopHud.Message(slot == 1 ? "Pas de pistolet" : slot == 2 ? "Pas de marteau (il est sur l'établi)" : "Pas d'arme de mêlée"); return; }
        if (cur) Stow(rightH);
        if (rightH.Held != null) return;
        TakeFrom(rightH, want);
    }

    // molette : objet suivant / précédent parmi pistolet, marteau, arme de mêlée et main vide
    void Cycle(int dir)
    {
        var list = new List<XRGrabInteractable> { null };
        for (int s = 1; s <= 4; s++) { if (s == 3) continue; var g = SlotItem(s); if (g && !list.Contains(g)) list.Add(g); }
        var cur = rightH.Grab;
        if (cur && !list.Contains(cur)) list.Add(cur);
        if (list.Count < 2) return;
        int i = Mathf.Max(0, list.IndexOf(cur));
        var next = list[(i + dir + list.Count) % list.Count];
        if (cur) Stow(rightH);
        if (rightH.Held != null || !next) return;
        TakeFrom(rightH, next);
    }

    XRGrabInteractable SlotItem(int slot)
    {
        switch (slot)
        {
            case 1:
            {
                var r = rightH.Item<Pistol>(); if (r) return rightH.Grab;
                var s = Socket(BeltItemType.Pistol);
                if (s && s.hasSelection) return s.firstInteractableSelected as XRGrabInteractable;
                var p = Nearest<Pistol>(reach); return p ? p.GetComponent<XRGrabInteractable>() : null;
            }
            case 2:
            {
                if (rightH.Item<Hammer>()) return rightH.Grab;
                foreach (var g in pocket) if (g && g.GetComponent<Hammer>()) return g;
                var h = Nearest<Hammer>(reach + 0.8f); return h ? h.GetComponent<XRGrabInteractable>() : null;
            }
            case 4:
            {
                if (rightH.Item<MeleeWeapon>()) return rightH.Grab;
                foreach (var g in pocket) if (g && g.GetComponent<MeleeWeapon>()) return g;
                var m = Nearest<MeleeWeapon>(reach); return m ? m.GetComponent<XRGrabInteractable>() : null;
            }
        }
        return null;
    }

    // la lampe va dans la main gauche (ou retourne à la ceinture)
    void LampToggleHand()
    {
        if (leftH.Item<Flashlight>()) { Stow(leftH); return; }
        if (rightH.Item<Flashlight>()) { Stow(rightH); return; }
        var s = Socket(BeltItemType.Flashlight);
        var g = s && s.hasSelection ? s.firstInteractableSelected as XRGrabInteractable : null;
        if (!g) { var f = Nearest<Flashlight>(reach); if (f) g = f.GetComponent<XRGrabInteractable>(); }
        if (!g) { DesktopHud.Message("Pas de lampe"); return; }
        if (leftH.Held != null) Stow(leftH);
        if (leftH.Held == null) TakeFrom(leftH, g);
    }

    /// <summary>G : lâcher l'objet de la main droite devant soi (les objets de la ceinture y retournent).</summary>
    public void Drop(bool rightHand = true) => Drop(rightHand ? rightH : leftH);

    void Drop(Hand h)
    {
        var g = h.Grab;
        if (!g || reload >= 0f) return;
        if (g.GetComponent<BeltItem>() && FreeSocket(g.GetComponent<BeltItem>().type)) { Release(h); return; }
        // posé devant soi : sur la surface visée si elle est proche, sinon il tombe d'un peu devant
        var at = aimDistance < 1.6f ? aimPoint - head.forward * 0.12f + Vector3.up * 0.08f : head.position + head.forward * 0.6f;
        g.throwOnDetach = false;
        h.it.interactionManager.SelectExit((IXRSelectInteractor)h.it, (IXRSelectInteractable)g);
        g.transform.position = at;
        var rb = g.GetComponent<Rigidbody>();
        if (rb && !rb.isKinematic) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
    }

    // ───────────────────────── prise ─────────────────────────
    // E sur un objet : il va dans la bonne main (lampe à gauche, chargeur ou pile vers la ceinture si on a déjà une arme)
    void Take(XRGrabInteractable g, Vector3 point)
    {
        if (reload >= 0f) return;
        var kind = KindOf(g);
        if (kind == Kind.Lamp) { if (leftH.Held != null) Stow(leftH); if (leftH.Held == null) Grab(leftH, g, point); return; }
        if (kind == Kind.Magazine && rightH.Item<Pistol>())
        {
            var s = FreeSocket(BeltItemType.Magazine);
            if (s) { s.interactionManager.SelectEnter((IXRSelectInteractor)s, (IXRSelectInteractable)g); DesktopHud.Message("Chargeur rangé à la ceinture"); return; }
        }
        if (rightH.Held != null) Stow(rightH);
        if (rightH.Held != null) return;
        Grab(rightH, g, point);
    }

    void TakeFrom(Hand h, XRGrabInteractable g)
    {
        if (pocket.Contains(g)) Unpocket(g);
        Grab(h, g, Center(g));
    }

    // la paume va d'abord sur l'objet (GrabGuard n'accepte qu'une prise à portée de main), puis l'objet vient en vue
    void Grab(Hand h, XRGrabInteractable g, Vector3 point)
    {
        if (g.isSelected && g.firstInteractorSelecting is XRSocketInteractor sock && !(sock is BeltSocket)) return;   // déjà dans le pistolet
        var fromPos = head.InverseTransformPoint(g.transform.position);
        var fromRot = Quaternion.Inverse(head.rotation) * g.transform.rotation;
        Reach(h, point);
        h.it.interactionManager.SelectEnter((IXRSelectInteractor)h.it, (IXRSelectInteractable)g);
        if (h.Grab != g) return;
        h.fromPos = fromPos; h.fromRot = fromRot; h.pickup = 0f;
    }

    void Reach(Hand h, Vector3 point) => h.ctrl.position += point - h.attach.position;

    // ranger l'objet tenu : holster de la ceinture, poche (marteau, arme de mêlée), sinon il est lâché devant soi
    void Stow(Hand h)
    {
        var g = h.Grab;
        if (!g) { if (h.Held != null) Release(h); return; }
        var item = g.GetComponent<BeltItem>();
        if (item && FreeSocket(item.type)) { Release(h); return; }
        if (g.GetComponent<Hammer>() || g.GetComponent<MeleeWeapon>()) { Pocket(h, g); return; }
        Drop(h);
    }

    void Release(Hand h)
    {
        var held = h.Held;
        if (held == null) return;
        StopUse(h);
        var g = held as XRGrabInteractable;
        if (g) g.throwOnDetach = false;
        var item = g ? g.GetComponent<BeltItem>() : null;
        var socket = item ? FreeSocket(item.type) : null;
        h.it.interactionManager.SelectExit((IXRSelectInteractor)h.it, held);
        if (socket) socket.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)g);   // retourne à son holster
    }

    void ReleaseRight() => Release(rightH);

    // poche : l'objet est caché et suit la ceinture (version PC seulement)
    void Pocket(Hand h, XRGrabInteractable g)
    {
        Release(h);
        SetPocketed(g, true);
        if (!pocket.Contains(g)) pocket.Add(g);
    }

    void Unpocket(XRGrabInteractable g)
    {
        pocket.Remove(g);
        g.transform.position = head.position + head.rotation * new Vector3(0.2f, -0.35f, 0.3f);
        SetPocketed(g, false);
        Physics.SyncTransforms();                                  // colliders rallumés à la bonne place avant la prise (GrabGuard)
    }

    static void SetPocketed(XRGrabInteractable g, bool on)
    {
        foreach (var r in g.GetComponentsInChildren<Renderer>(true)) if (!r.name.StartsWith("Contour")) r.enabled = !on;
        foreach (var c in g.GetComponentsInChildren<Collider>(true)) c.enabled = !on;
        var rb = g.GetComponent<Rigidbody>();
        if (rb) { if (on && !rb.isKinematic) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; } rb.isKinematic = on; }
    }

    void UpdatePocket()
    {
        for (int i = pocket.Count - 1; i >= 0; i--)
        {
            var g = pocket[i];
            if (!g || g.isSelected) { if (g) SetPocketed(g, false); pocket.RemoveAt(i); continue; }
            g.transform.position = transform.position + Vector3.up * 0.9f;    // suit le joueur (hanche), invisible
        }
    }

    // ───────────────────────── utiliser ─────────────────────────
    void Use(Hand h)
    {
        if (!h.Grab) return;
        h.it.activateInput.QueueManualState(true, 1f);                      // gâchette (lampe) : tenue 2 images (XRI la voit)
        h.activating = true;
        h.useFrame = Time.frameCount;
    }

    void StopUse(Hand h)
    {
        if (!h.activating) return;
        h.it.activateInput.QueueManualState(false, 0f);
        h.activating = false;
    }

    void RechargeLamp()
    {
        var battery = rightH.Item<Battery>(); if (!battery) battery = leftH.Item<Battery>();
        if (!battery) return;
        var lamp = HeldLamp;
        if (!lamp) { var s = Socket(BeltItemType.Flashlight); if (s && s.hasSelection) lamp = (s.firstInteractableSelected as Component)?.GetComponent<Flashlight>(); }
        if (!lamp) lamp = Nearest<Flashlight>(reach);
        if (!lamp) { DesktopHud.Message("Pas de lampe à recharger"); return; }
        lamp.Recharge();
        battery.Consume();                                                   // comme le port de la lampe (Flashlight)
        DesktopHud.Message("Lampe rechargée");
    }

    // ───────────────────────── coup (marteau, mêlée) ─────────────────────────
    void StartSwing()
    {
        if (swing >= 0f) return;
        swingOnNail = false;
        // aide à la visée : clou visé (à moins de 12 cm du réticule) → la tête du marteau va droit dessus
        if (rightH.Item<Hammer>())
        {
            Nail best = null; float bd = 0.12f;
            var ray = new Ray(head.position, head.forward);
            foreach (var p in Plank.All)
            {
                if (!p || !p.CanBeNailed) continue;
                foreach (var n in p.nails)
                {
                    if (!n || n.Driven || !n.visual.activeSelf) continue;
                    var tip = NailHead(n);
                    float along = Vector3.Dot(tip - ray.origin, ray.direction);
                    if (along < 0.2f || along > 1.9f) continue;
                    float off = Vector3.Distance(tip, ray.GetPoint(along));
                    if (off < bd) { bd = off; best = n; }
                }
            }
            // la tête du marteau passe PAR la tête du clou (Nail : trajet de la tête à moins de son rayon + 3 cm), un peu au-delà
            if (best) { var tip = NailHead(best); swingTarget = tip + (tip - head.position).normalized * 0.05f; swingOnNail = true; }
        }
        if (!swingOnNail)
            swingTarget = aimDistance < 1.7f ? aimPoint - head.forward * 0.02f : head.position + head.forward * 1.15f;
        swing = 0f;
    }

    // la planche calée est encore dans le regard (moins de 80 cm du rayon, à portée)
    bool SnappedSlotAimed(Plank plank)
    {
        foreach (var s in BarricadeSlot.All)
        {
            if (!s || s.Plank != plank) continue;
            var to = s.transform.position - head.position;
            float along = Vector3.Dot(to, head.forward);
            return along > 0.2f && along < reach + 0.8f && Vector3.Distance(s.transform.position, head.position + head.forward * along) < 0.8f;
        }
        return false;
    }

    // tête du clou, là où Nail teste le passage du marteau
    static Vector3 NailHead(Nail n) => n.transform.TransformPoint(0f, 0f, n.visual.transform.localPosition.z + 0.04f);

    // ───────────────────────── leviers et portes ─────────────────────────
    void GrabLever(Lever lv, IXRSelectInteractable handle)
    {
        if (lv.locked) { DesktopHud.Message("Bloqué"); return; }
        if (rightH.Held != null) Stow(rightH);
        if (rightH.Held != null) return;
        Reach(rightH, handle.transform.position);
        rightH.it.interactionManager.SelectEnter((IXRSelectInteractor)rightH.it, handle);
        if (!rightH.it.IsSelecting(handle)) return;
        lever = lv;
        // on le pousse vers l'autre butée : abaissé s'il est en haut, remonté s'il est en bas
        leverGoal = lv.Down01 < 0.5f ? lv.downAngle : lv.upAngle;
    }

    void ReleaseLever()
    {
        if (lever && lever.handle && rightH.it.IsSelecting(lever.handle)) Release(rightH);
        lever = null;
    }

    void GrabDoor(HingedDoor hd, IXRSelectInteractable handle)
    {
        if (hd.locked) { DesktopHud.Message("Verrouillée"); return; }
        if (rightH.Held != null) Stow(rightH);
        if (rightH.Held != null) return;
        Reach(rightH, handle.transform.position);
        rightH.it.interactionManager.SelectEnter((IXRSelectInteractor)rightH.it, handle);
        if (!rightH.it.IsSelecting(handle)) return;
        door = hd;
        doorGoal = hd.Closed ? hd.maxAngle * 0.92f : 0f;
        doorTime = 0f;
    }

    // ───────────────────────── animations en cours ─────────────────────────
    void Tick()
    {
        float dt = Time.deltaTime;
        if (PauseMenu.Paused) return;
        recoil = Mathf.MoveTowards(recoil, 0f, dt * 7f);
        rightH.trigger = Mathf.MoveTowards(rightH.trigger, 0f, dt * 6f);
        if (rightH.pickup < 1f) rightH.pickup = Mathf.Min(1f, rightH.pickup + dt / 0.22f);
        if (leftH.pickup < 1f) leftH.pickup = Mathf.Min(1f, leftH.pickup + dt / 0.22f);

        if (swing >= 0f)
        {
            swing += Mathf.Min(dt, 1f / 30f) / swingDuration;               // au moins ~10 images par coup, même si une image traîne
            if (swing >= 1f || !(rightH.Item<Hammer>() || rightH.Item<MeleeWeapon>())) swing = -1f;
        }

        // levier : la main pousse la boule vers la butée visée tant que E est tenu
        rightH.target = null;
        if (lever)
        {
            if (!lever.handle || !rightH.it.IsSelecting(lever.handle) || lever.locked) lever = null;
            else
            {
                var pivot = lever.transform;
                var axis = pivot.right;
                var dirNow = lever.handle.transform.position - pivot.position;
                float r = dirNow.magnitude;
                float next = Mathf.MoveTowards(lever.Angle, leverGoal, 160f * dt);
                var dir = Quaternion.AngleAxis(next - lever.Angle, axis) * dirNow.normalized;
                rightH.target = pivot.position + dir * r;
            }
        }

        // porte : la main tire la poignée autour de la charnière
        if (door)
        {
            doorTime += dt;
            if (!door.handle || !rightH.it.IsSelecting(door.handle) || doorTime > 2.5f) { door = null; if (rightH.Held is XRSimpleInteractable) Release(rightH); }
            else
            {
                var hinge = door.transform.position;
                var hp = door.handle.transform.position;
                var v = hp - hinge; float y = v.y; v.y = 0f;
                float next = Mathf.MoveTowards(door.Angle, doorGoal, 95f * dt);
                var dir = Quaternion.AngleAxis((next - door.Angle) * door.openSign, Vector3.up) * v;
                rightH.target = hinge + dir + Vector3.up * y;
                if (Mathf.Abs(door.Angle - doorGoal) < 1.5f) { door = null; Release(rightH); }
            }
        }

        // bouton / écran / radio : la main va appuyer (aller 0,12 s, appui, retour)
        if (pressT >= 0f)
        {
            float prev = pressT;
            pressT += dt / 0.3f;
            var pb = press as Behaviour;
            if (prev < 0.45f && pressT >= 0.45f && pb && pb.isActiveAndEnabled)
            {
                if (rightH.Held == null || rightH.Held is XRSimpleInteractable)
                    rightH.it.interactionManager.SelectEnter((IXRSelectInteractor)rightH.it, press);
                else
                    leftH.it.interactionManager.SelectEnter((IXRSelectInteractor)leftH.it, press);   // main droite occupée : la gauche appuie
            }
            if (pressT >= 1f) { pressT = -1f; press = null; }
        }

        // rechargement : le chargeur monte de la ceinture sous la crosse, MagWell l'insère (comme au casque)
        if (reload >= 0f)
        {
            reload += dt / 0.55f;
            var pistol = rightH.Item<Pistol>();
            if (!pistol || reload >= 1f || !leftH.Item<Magazine>())
            {
                if (leftH.Item<Magazine>()) Release(leftH);                   // pas inséré (temps écoulé) : retour à la ceinture
                reload = -1f;
                if (lampWasInLeft) { lampWasInLeft = false; LampToggleHand(); }
            }
        }
    }

    // ───────────────────────── placement des mains et des objets ─────────────────────────
    void PlaceHand(Hand h)
    {
        var g = h.Grab;
        bool visible = g != null || (h.right && (h.target.HasValue || pressT >= 0f && (rightH.Held == null || rightH.Held is XRSimpleInteractable)))
                       || (!h.right && pressT >= 0f && rightH.Held != null && !(rightH.Held is XRSimpleInteractable));
        foreach (var r in h.renderers) if (r) r.enabled = visible;

        // poing fermé quand la main tient quelque chose, index sur la détente pour le pistolet
        float gripGoal = g != null || h.Held != null ? 1f : (visible ? 0.35f : 0f);
        h.grip = Mathf.MoveTowards(h.grip, gripGoal, Time.deltaTime * 8f);
        if (h.anim) { h.anim.SetFloat("Grip", h.grip); h.anim.SetFloat("Trigger", Mathf.Max(h.trigger, g && g.GetComponent<Pistol>() ? 0.15f : g != null ? 1f : 0f)); }

        if (g)
        {
            ItemPose(h, g, out var pos, out var rot);
            if (h.pickup < 1f)
            {
                float k = Smooth(h.pickup);
                var from = head.TransformPoint(h.fromPos);
                pos = Vector3.Lerp(from, pos, k);
                rot = Quaternion.Slerp(head.rotation * h.fromRot, rot, k);
            }
            PutItem(h, g, pos, rot);
            return;
        }

        if (h.Held != null && h.target.HasValue) { PutAttach(h, h.target.Value, Quaternion.LookRotation(h.target.Value - head.position, head.up)); return; }
        if (h.Held != null && !h.target.HasValue && h.Held is XRSimpleInteractable si && si) { PutAttach(h, si.transform.position, Quaternion.LookRotation(si.transform.position - head.position, head.up)); return; }

        // appui sur un bouton : la main part du bas de l'écran, touche le point puis revient
        if (pressT >= 0f && visible)
        {
            var rest = head.TransformPoint(new Vector3(h.right ? 0.18f : -0.18f, -0.28f, 0.3f));
            float s = pressT < 0.45f ? pressT / 0.45f : 1f - (pressT - 0.45f) / 0.55f;
            var p = Vector3.Lerp(rest, pressPoint - head.forward * 0.02f, Smooth(s));
            PutAttach(h, p, Quaternion.LookRotation(pressPoint - head.position, head.up));
            return;
        }

        // main vide : repos, sous le champ de vision (cachée)
        h.ctrl.SetPositionAndRotation(head.TransformPoint(new Vector3(h.right ? 0.25f : -0.25f, -0.55f, 0.05f)), head.rotation);
    }

    // pose voulue de l'objet tenu, en monde (vue FPS)
    void ItemPose(Hand h, XRGrabInteractable g, out Vector3 pos, out Quaternion rot)
    {
        var kind = KindOf(g);
        Vector3 local;
        var up = head.up;
        // balancement de la marche et léger retard sur la souris : l'arme vit un peu
        var bobOff = new Vector3(Mathf.Cos(bob) * 0.006f, -Mathf.Abs(Mathf.Sin(bob)) * 0.009f, 0f) * bobAmount;
        var swayOff = new Vector3(-sway.x * 0.0025f, -sway.y * 0.0025f, 0f);
        switch (kind)
        {
            case Kind.Pistol:
            {
                local = Vector3.Lerp(new Vector3(0.15f, -0.16f, 0.36f), new Vector3(0f, -0.118f, 0.34f), Smooth(aimBlend));
                local += (bobOff + swayOff) * (1f - aimBlend * 0.8f);
                local += new Vector3(0f, 0.012f, -0.045f) * recoil;
                if (reload >= 0f) local += new Vector3(-0.05f, 0.02f, -0.03f) * Mathf.Sin(Mathf.Clamp01(reload) * Mathf.PI);
                if (inspecting) local = new Vector3(0.06f, -0.08f, 0.3f);
                pos = head.TransformPoint(local);
                var pistol = g.GetComponent<Pistol>();
                var aimDir = (aimPoint - pos).normalized;
                if (Vector3.Dot(aimDir, head.forward) < 0.5f || aimDistance < 0.5f) aimDir = head.forward;
                rot = Quaternion.LookRotation(aimDir, up);
                if (pistol && pistol.muzzle)
                {
                    // le canon vise le réticule : on tourne l'arme pour que muzzle.forward pointe vers le point visé
                    var rel = Quaternion.Inverse(g.transform.rotation) * pistol.muzzle.rotation;
                    rot = rot * Quaternion.Inverse(rel);
                }
                rot = rot * Quaternion.Euler(-14f * recoil, 0f, 0f);
                if (reload >= 0f) rot = rot * Quaternion.Euler(0f, 0f, 28f * Mathf.Sin(Mathf.Clamp01(reload) * Mathf.PI));
                if (inspecting) rot = head.rotation * Quaternion.Euler(-10f, -60f, 25f);
                return;
            }
            case Kind.Lamp:
            {
                local = h.right ? new Vector3(0.17f, -0.17f, 0.4f) : new Vector3(-0.17f, -0.18f, 0.38f);
                local += bobOff + swayOff;
                if (inspecting) local = new Vector3(h.right ? 0.05f : -0.05f, -0.06f, 0.3f);
                pos = head.TransformPoint(local);
                var fl = g.GetComponent<Flashlight>();
                var dir = (aimPoint - pos).normalized;
                if (Vector3.Dot(dir, head.forward) < 0.5f) dir = head.forward;
                rot = Quaternion.LookRotation(dir, up);
                if (fl && fl.spot) rot = rot * Quaternion.Inverse(Quaternion.Inverse(g.transform.rotation) * fl.spot.transform.rotation);
                if (inspecting) rot = head.rotation * Quaternion.Euler(-25f, h.right ? -50f : 50f, 0f);
                return;
            }
            case Kind.Hammer:
            case Kind.Melee:
            {
                ToolAxes(g, kind, out var tip, out var faceLocal);
                // repos : la main en bas à droite, le manche levé vers l'avant, tête en haut, face de frappe vers l'avant
                var restUp = head.rotation * new Vector3(-0.22f, 0.62f, 0.75f).normalized;
                var restRot = Map(faceLocal, Vector3.up, Vector3.ProjectOnPlane(head.forward, restUp), restUp);
                var grip = head.TransformPoint(new Vector3(0.2f, -0.2f, 0.4f) + bobOff + swayOff);
                var restPos = grip - restRot * GripLocal(h, g);
                pos = restPos; rot = restRot;
                if (swing >= 0f && h.right)
                {
                    // armer (0 → 0,25) : l'outil remonte ; frapper (0,25 → 0,5) : la tête va jusqu'à la cible ; revenir (0,5 → 1)
                    var tipRest = restPos + restRot * tip;
                    var raised = tipRest + head.rotation * new Vector3(0.04f, 0.12f, -0.08f);
                    var hitUp = head.rotation * new Vector3(-0.1f, 0.25f, -0.95f).normalized;      // manche couché vers soi au moment du coup
                    var toTarget = (swingTarget - head.position).normalized;
                    var hitRot = Map(faceLocal, Vector3.up, toTarget, Vector3.ProjectOnPlane(hitUp, toTarget));
                    Vector3 tipPos; Quaternion r;
                    if (swing < 0.25f) { float s = Smooth(swing / 0.25f); tipPos = Vector3.Lerp(tipRest, raised, s); r = Quaternion.Slerp(restRot, restRot * Quaternion.Euler(-25f, 0f, 0f), s); }
                    else if (swing < 0.5f) { float s = (swing - 0.25f) / 0.25f; s = s * s; tipPos = Vector3.Lerp(raised, swingTarget, s); r = Quaternion.Slerp(restRot * Quaternion.Euler(-25f, 0f, 0f), hitRot, s); }
                    else { float s = Smooth((swing - 0.5f) / 0.5f); tipPos = Vector3.Lerp(swingTarget, tipRest, s); r = Quaternion.Slerp(hitRot, restRot, s); }
                    rot = r;
                    pos = tipPos - r * tip;
                }
                if (inspecting) { pos = head.TransformPoint(new Vector3(0.05f, -0.12f, 0.45f)); rot = head.rotation * Quaternion.Euler(0f, -70f, 20f) * Quaternion.Inverse(Quaternion.LookRotation(faceLocal, Vector3.up)); }
                return;
            }
            case Kind.Plank:
            {
                var plank = g.GetComponent<Plank>();
                if (plank && plank.state == Plank.State.Snapped && SnappedSlotAimed(plank))
                {
                    // déjà calée sur l'ouverture : la main reste dessus tant qu'on la regarde (sinon elle se décroche, Plank)
                    pos = g.transform.position; rot = g.transform.rotation;
                    return;
                }
                if (aimedSlot && plank && plank.state == Plank.State.Free)
                {
                    // emplacement visé : la planche y va (elle se cale toute seule, Plank) ; E la pose
                    pos = aimedSlot.transform.position; rot = aimedSlot.transform.rotation;
                    return;
                }
                // portée en diagonale en bas de l'écran, face visible (on voit encore où l'on va)
                pos = head.TransformPoint(new Vector3(0.12f, -0.4f, 0.8f) + bobOff);
                rot = head.rotation * Quaternion.Euler(-52f, 0f, -16f);
                return;
            }
            default:
            {
                local = (h.right ? new Vector3(0.17f, -0.17f, 0.38f) : new Vector3(-0.17f, -0.17f, 0.38f)) + bobOff + swayOff;
                if (inspecting) local = new Vector3(h.right ? 0.04f : -0.04f, -0.07f, 0.28f);
                if (reload >= 0f && !h.right && kind == Kind.Magazine && reloadWell)
                {
                    // le chargeur monte sous la crosse : son centre va sur le puits (MagWell l'insère à 9 cm)
                    var center = Center(g);
                    var off = center - g.transform.position;
                    var start = head.TransformPoint(new Vector3(-0.12f, -0.3f, 0.3f));
                    float s = Smooth(Mathf.Clamp01(reload / 0.7f));
                    var wellRot = reloadWell.transform.rotation;
                    rot = wellRot;
                    pos = Vector3.Lerp(start, reloadWell.transform.position + Vector3.down * 0.01f, s) - (wellRot * Quaternion.Inverse(g.transform.rotation)) * off;
                    return;
                }
                pos = head.TransformPoint(local);
                // garde l'orientation qu'il avait par rapport à la caméra quand on l'a pris
                rot = head.rotation * h.fromRot;
                return;
            }
        }
    }

    // axes d'un outil (marteau, arme de mêlée) dans son repère : bout qui frappe et direction de la face de frappe
    static readonly Dictionary<XRGrabInteractable, (Vector3 tip, Vector3 face)> toolCache = new Dictionary<XRGrabInteractable, (Vector3, Vector3)>();
    static void ToolAxes(XRGrabInteractable g, Kind kind, out Vector3 tip, out Vector3 face)
    {
        if (toolCache.TryGetValue(g, out var c)) { tip = c.tip; face = c.face; return; }
        Transform striker = kind == Kind.Hammer ? g.GetComponentInChildren<HammerHead>(true)?.transform : g.GetComponent<MeleeWeapon>()?.striker;
        tip = striker ? g.transform.InverseTransformPoint(striker.position) : Vector3.up * 0.3f;
        face = Vector3.forward;
        var box = striker ? striker.GetComponent<BoxCollider>() : null;
        if (box) face = box.size.x > box.size.z ? Vector3.right : Vector3.forward;   // la tête est allongée dans le sens du coup
        toolCache[g] = (tip, face);
    }

    // point de prise (accroche de la main) dans le repère de l'objet
    static Vector3 GripLocal(Hand h, XRGrabInteractable g) => g.transform.InverseTransformPoint(g.GetAttachTransform(h.it).position);

    // rotation qui envoie les axes locaux (fwd, up) de l'objet sur les directions monde (fwd, up)
    static Quaternion Map(Vector3 localFwd, Vector3 localUp, Vector3 worldFwd, Vector3 worldUp)
    {
        if (worldFwd.sqrMagnitude < 1e-6f) worldFwd = Vector3.forward;
        return Quaternion.LookRotation(worldFwd, worldUp) * Quaternion.Inverse(Quaternion.LookRotation(localFwd, localUp));
    }

    // place la manette pour que l'objet tenu soit exactement à (pos, rot) : l'accroche de la main va sur celle de l'objet
    void PutItem(Hand h, XRGrabInteractable g, Vector3 pos, Quaternion rot)
    {
        // l'arme ne traverse pas les murs : mur juste devant → on la recule un peu (vers soi) et on la baisse
        if (KindOf(g) != Kind.Plank && swing < 0f)
        {
            var to = pos - head.position;
            if (Physics.Raycast(head.position, to.normalized, out var wall, to.magnitude + 0.08f, ~0, QueryTriggerInteraction.Ignore) && !IsMine(wall.collider))
            {
                float over = Mathf.Clamp(to.magnitude + 0.08f - wall.distance, 0f, 0.14f);
                pos += -head.forward * over - head.up * over * 0.5f;
            }
        }

        var ga = g.GetAttachTransform(h.it);
        var lPos = Quaternion.Inverse(g.transform.rotation) * (ga.position - g.transform.position);
        var lRot = Quaternion.Inverse(g.transform.rotation) * ga.rotation;
        // XRI : l'accroche de l'objet suit l'accroche de la main (même position, même rotation)
        PutAttach(h, pos + rot * lPos, rot * lRot);
    }

    void PutAttach(Hand h, Vector3 attachPos, Quaternion attachRot)
    {
        var aRot = Quaternion.Inverse(h.ctrl.rotation) * h.attach.rotation;
        var aPos = Quaternion.Inverse(h.ctrl.rotation) * (h.attach.position - h.ctrl.position);
        var cRot = attachRot * Quaternion.Inverse(aRot);
        h.ctrl.SetPositionAndRotation(attachPos - cRot * aPos, cRot);
    }

    // ───────────────────────── recherche ─────────────────────────
    bool Pick(out IXRSelectInteractable target, out Vector3 point)
    {
        target = null; point = default;
        var manager = rightH.it.interactionManager;
        if (!manager) return false;
        var ray = new Ray(head.position, head.forward);
        foreach (var hit in Sorted(Physics.RaycastAll(ray, reach, ~0, QueryTriggerInteraction.Collide)))
        {
            if (IsMine(hit.collider)) continue;
            if (Usable(manager, hit.collider, out var s)) { target = s; point = hit.point; return true; }
            if (!hit.collider.isTrigger) break;                                     // un mur, une caisse : rien derrière
        }
        // petits objets (clé, chargeur, pile) : un peu de tolérance autour du réticule
        foreach (var hit in Sorted(Physics.SphereCastAll(ray, 0.06f, reach, ~0, QueryTriggerInteraction.Ignore)))
        {
            if (IsMine(hit.collider) || hit.distance > aimDistance + 0.25f) continue;
            if (Usable(manager, hit.collider, out var s) && s is XRGrabInteractable) { target = s; point = hit.point == Vector3.zero ? hit.collider.bounds.center : hit.point; return true; }
        }
        return false;
    }

    bool Usable(UnityEngine.XR.Interaction.Toolkit.XRInteractionManager manager, Collider c, out IXRSelectInteractable s)
    {
        s = null;
        if (!manager.TryGetInteractableForCollider(c, out var i) || !(i is IXRSelectInteractable sel) || !(sel is Behaviour b) || !b.isActiveAndEnabled) return false;
        if (sel.isSelected && !(sel.firstInteractorSelecting is XRSocketInteractor)) return false;     // déjà dans une main
        if (sel.isSelected && sel.firstInteractorSelecting is XRSocketInteractor sock && !(sock is BeltSocket)) return false;   // chargeur dans le pistolet
        if (b.GetComponent<MonitorOverlayClose>()) return false;
        s = sel;
        return true;
    }

    bool IsMine(Collider c)
    {
        if (!c) return false;
        foreach (var h in new[] { rightH, leftH })
        {
            var g = h?.Grab;
            if (g && c.transform.IsChildOf(g.transform)) return true;
            if (h != null && c.transform.IsChildOf(h.ctrl)) return true;
        }
        return c.transform.IsChildOf(transform);
    }

    static RaycastHit[] Sorted(RaycastHit[] hits)
    {
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        return hits;
    }

    BeltSocket Socket(BeltItemType type)
    {
        foreach (var s in sockets) if (s && s.accepts == type) return s;
        sockets = FindObjectsByType<BeltSocket>(FindObjectsSortMode.None);
        foreach (var s in sockets) if (s && s.accepts == type) return s;
        return null;
    }

    BeltSocket FreeSocket(BeltItemType type)
    {
        var s = Socket(type);
        return s && !s.hasSelection && s.isActiveAndEnabled ? s : null;
    }

    T Nearest<T>(float within) where T : Component
    {
        T best = null;
        float bd = within;
        foreach (var c in FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            var g = c.GetComponent<XRGrabInteractable>();
            if (!g || g.isSelected || pocket.Contains(g)) continue;
            float d = Vector3.Distance(head.position, c.transform.position);
            if (d < bd) { bd = d; best = c; }
        }
        return best;
    }

    static Vector3 Center(XRGrabInteractable g)
    {
        var col = g.colliders.Count > 0 && g.colliders[0] ? g.colliders[0] : g.GetComponent<Collider>();
        return col ? col.bounds.center : g.transform.position;
    }

    static MonitorStation OpenStation()
    {
        foreach (var st in FindObjectsByType<MonitorStation>(FindObjectsSortMode.None))
            if (st.overlay && st.overlay.gameObject.activeSelf) return st;
        return null;
    }

    public static Kind KindOf(XRGrabInteractable g)
    {
        if (!g) return Kind.None;
        if (g.GetComponent<Pistol>()) return Kind.Pistol;
        if (g.GetComponent<Hammer>()) return Kind.Hammer;
        if (g.GetComponent<MeleeWeapon>()) return Kind.Melee;
        if (g.GetComponent<Flashlight>()) return Kind.Lamp;
        if (g.GetComponent<Plank>()) return Kind.Plank;
        if (g.GetComponent<Magazine>()) return Kind.Magazine;
        if (g.GetComponent<Battery>()) return Kind.Battery;
        return Kind.Other;
    }

    static string Label(IXRSelectInteractable s)
    {
        var t = s.transform;
        var lv = t.GetComponentInParent<Lever>();
        if (lv) return lv.locked ? "Levier (bloqué)" : (lv.GetComponentInParent<Generator>() || lv.springBack ? "Maintenir : abaisser le levier" : lv.Down01 < 0.5f ? "Maintenir : abaisser le levier" : "Maintenir : remonter le levier");
        var hd = t.GetComponentInParent<HingedDoor>();
        if (hd) return hd.locked ? "Porte verrouillée" : hd.Closed ? "Ouvrir la porte" : "Fermer la porte";
        if (t.GetComponentInParent<SupplyRadio>()) return "Appeler un ravitaillement";
        if (t.GetComponent<MonitorScreen>()) return "Regarder la caméra";
        if (t.GetComponent<Pistol>()) return "Prendre le pistolet";
        if (t.GetComponent<Hammer>()) return "Prendre le marteau";
        if (t.GetComponent<Plank>()) return "Prendre la planche";
        if (t.GetComponent<Magazine>()) return "Prendre le chargeur";
        if (t.GetComponent<Flashlight>()) return "Prendre la lampe";
        if (t.GetComponent<Battery>()) return "Prendre la pile";
        if (t.GetComponent<MeleeWeapon>()) return "Prendre " + (t.name.ToLowerInvariant().Contains("hache") || t.name.ToLowerInvariant().Contains("axe") ? "la hache" : "l'arme");
        if (t.name.StartsWith("Cle")) return "Prendre la clé";
        return s is XRGrabInteractable ? "Prendre" : "Utiliser";
    }

    static float Smooth(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }
}
