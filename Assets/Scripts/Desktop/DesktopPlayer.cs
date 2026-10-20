using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Version PC : pilote le XR Origin du jeu au clavier et à la souris (ajouté par DesktopMode, jamais en VR).
// On garde le même rig et les mêmes mains XRI que dans le casque : les objets, leviers, portes, boutons et armes
// reçoivent les mêmes événements (prise, utilisation) qu'avec les manettes. Seule la façon de bouger les mains change :
//   - la caméra suit la souris, le corps se déplace avec ZQSD (touches physiques W A S D, donc ZQSD sur un clavier AZERTY) ;
//   - les mains flottent devant la caméra et visent le point au centre de l'écran (réticule) ;
//   - « prendre » = l'objet visé (à moins de `reach`) est attrapé par la main ; un levier ou une poignée de porte reste
//     tenu et suit le regard (regarder vers le bas abaisse le levier, reculer ou tourner ouvre la porte) ;
//   - marteau et armes de mêlée : le clic lance un coup vers le point visé (la tête de l'outil va jusqu'à ce point et revient).
[DefaultExecutionOrder(-200)]           // avant le gestionnaire d'interactions XRI : les objets tenus suivent la main dans la même image
public class DesktopPlayer : MonoBehaviour
{
    public float eyeHeight = 1.65f;
    public float moveSpeed = 2f;
    public float mouseSensitivity = 0.1f;
    public float reach = 2.5f;              // distance maximale pour prendre un objet ou appuyer sur un bouton
    public float swingDuration = 0.25f;

    class Hand
    {
        public Transform ctrl, attach;
        public XRDirectInteractor it;
        public Vector3 rest;                // position au repos, dans le repère de la caméra
        public float holdDistance = -1f;    // > 0 : tient un levier / une poignée, qui suit le regard à cette distance
        public float swing = -1f;           // avancement du coup de marteau (0..1), < 0 : pas de coup
        public Vector3 swingDelta;
        public bool activating;
        public int useFrame;                // image de l'appui : la gâchette reste enfoncée au moins 2 images (sinon XRI ne la voit pas)

        public IXRSelectInteractable Held => it && it.hasSelection ? it.firstInteractableSelected : null;
        public XRGrabInteractable HeldGrab => Held as XRGrabInteractable;
        public T HeldItem<T>() where T : Component { var g = HeldGrab; var c = g ? g.GetComponent<T>() : null; return c ? c : null; }
    }

    XROrigin origin;
    CharacterController cc;
    Transform head;
    Hand right, left;
    float yaw, pitch;
    Vector3 aimPoint;
    string hoverName;
    bool help = true;
    BeltSocket[] sockets = new BeltSocket[0];

    void Awake()
    {
        origin = GetComponent<XROrigin>();
        cc = GetComponent<CharacterController>();
        head = origin.Camera ? origin.Camera.transform : GetComponentInChildren<Camera>(true).transform;
        origin.CameraYOffset = eyeHeight;       // lu par Crouch au démarrage (hauteur debout)

        // plus de suivi du casque ni des manettes : c'est ce script qui place la tête et les mains
        foreach (var tpd in GetComponentsInChildren<UnityEngine.InputSystem.XR.TrackedPoseDriver>(true)) tpd.enabled = false;
        head.localPosition = Vector3.zero;
        yaw = head.eulerAngles.y - transform.eulerAngles.y;

        // sans manettes suivies, XRInputModalityManager éteint les deux mains : on le coupe et on les rallume
        var modality = GetComponent<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>();
        if (modality) modality.enabled = false;

        right = MakeHand("Right Controller", new Vector3(0.18f, -0.2f, 0.42f));
        left = MakeHand("Left Controller", new Vector3(-0.2f, -0.24f, 0.38f));
    }

    Hand MakeHand(string name, Vector3 rest)
    {
        Transform ctrl = null;
        foreach (var t in GetComponentsInChildren<Transform>(true)) if (t.name == name) { ctrl = t; break; }
        if (!ctrl) return null;
        ctrl.gameObject.SetActive(true);
        var h = new Hand { ctrl = ctrl, rest = rest, it = ctrl.GetComponentInChildren<XRDirectInteractor>(true) };
        if (!h.it) return null;
        h.attach = h.it.attachTransform ? h.it.attachTransform : h.it.transform;
        // les boutons « prendre » et « utiliser » sont envoyés par ce script, plus par les manettes
        h.it.selectInput.inputSourceMode = XRInputButtonReader.InputSourceMode.ManualValue;
        h.it.activateInput.inputSourceMode = XRInputButtonReader.InputSourceMode.ManualValue;
        return h;
    }

    void Start() => sockets = FindObjectsByType<BeltSocket>(FindObjectsSortMode.None);

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null || right == null || left == null) return;

        bool clickConsumed = false;
        if (Cursor.lockState != CursorLockMode.Locked && mouse.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;       // 1er clic dans la fenêtre : capture la souris
            Cursor.visible = false;
            clickConsumed = true;
        }
        if (kb.f1Key.wasPressedThisFrame) help = !help;

        Look(mouse);
        if (!PauseMenu.Paused) Move(kb);
        Aim();

        if (!PauseMenu.Paused)
        {
            // main droite : E prend / lâche, clic gauche utilise (ou prend si la main est vide)
            if (kb.eKey.wasPressedThisFrame) TakeOrDrop(true);
            if (!clickConsumed && mouse.leftButton.wasPressedThisFrame) UseOrTake(true);
            // main gauche : clic droit prend / lâche, F utilise (lampe)
            if (mouse.rightButton.wasPressedThisFrame) TakeOrDrop(false);
            if (kb.fKey.wasPressedThisFrame) { if (left.HeldGrab) Use(left); else if (right.HeldItem<Flashlight>()) Use(right); }

            if (kb.digit1Key.wasPressedThisFrame) Belt(BeltItemType.Pistol);
            if (kb.digit2Key.wasPressedThisFrame) Belt(BeltItemType.Hammer);
            if (kb.digit3Key.wasPressedThisFrame) Belt(BeltItemType.Flashlight);
            if (kb.rKey.wasPressedThisFrame) Reload();
        }
        if (right.activating && !mouse.leftButton.isPressed && Time.frameCount > right.useFrame + 1) StopUse(right);
        if (left.activating && !kb.fKey.isPressed && Time.frameCount > left.useFrame + 1) StopUse(left);

        PlaceHand(right, true);
        PlaceHand(left, false);
    }

    // ───────────────────────── actions (publiques : testables en mode Play sans clavier) ─────────────────────────
    public void TakeOrDrop(bool rightHand)
    {
        var h = rightHand ? right : left;
        if (h.Held != null) Release(h); else TryGrab(h);
    }

    public void UseOrTake(bool rightHand)
    {
        var h = rightHand ? right : left;
        if (h.HeldGrab) Use(h); else TryGrab(h);
    }

    // pistolet et marteau : main droite ; lampe : main gauche
    public void Belt(BeltItemType type) => FromBelt(type == BeltItemType.Flashlight ? left : right, type);

    public string Holding(bool rightHand)
    {
        var held = (rightHand ? right : left).Held;
        return held != null ? held.transform.name : null;
    }

    // ───────────────────────── tête et corps ─────────────────────────
    void Look(Mouse mouse)
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            var d = mouse.delta.ReadValue() * mouseSensitivity;
            yaw += d.x;
            pitch = Mathf.Clamp(pitch - d.y, -85f, 85f);
        }
        head.localPosition = Vector3.zero;
        head.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    /// <summary>Tourne la tête vers un point (tests en mode Play, sans souris).</summary>
    public void LookAt(Vector3 point)
    {
        var d = Quaternion.Inverse(transform.rotation) * (point - head.position);
        yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(-Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg, -85f, 85f);
    }

    void Move(Keyboard kb)
    {
        var input = Vector3.zero;
        if (kb.wKey.isPressed) input.z += 1f;           // Z sur AZERTY
        if (kb.sKey.isPressed) input.z -= 1f;
        if (kb.aKey.isPressed) input.x -= 1f;           // Q sur AZERTY
        if (kb.dKey.isPressed) input.x += 1f;
        if (input == Vector3.zero || !cc || !cc.enabled) return;
        var dir = Quaternion.Euler(0f, head.eulerAngles.y, 0f) * input.normalized;
        cc.Move(dir * moveSpeed * Time.deltaTime);       // la gravité reste celle du rig (GravityProvider)
    }

    void Aim()
    {
        var ray = new Ray(head.position, head.forward);
        aimPoint = ray.GetPoint(30f);
        foreach (var hit in Sorted(Physics.RaycastAll(ray, 60f, ~0, QueryTriggerInteraction.Ignore)))
        {
            if (IsHeld(hit.collider)) continue;
            aimPoint = hit.point;
            break;
        }
        hoverName = null;
        if (!PauseMenu.Paused && Pick(out var target, out _)) hoverName = Label(target);
    }

    // ───────────────────────── mains ─────────────────────────
    void PlaceHand(Hand h, bool isRight)
    {
        var restPos = head.TransformPoint(h.rest);

        // levier / poignée tenu : la main reste sur le point visé, à la distance de la prise
        if (h.holdDistance > 0f)
        {
            if (!(h.Held is XRSimpleInteractable)) h.holdDistance = -1f;   // lâché par le levier ou la porte (main trop loin)
            else
            {
                h.ctrl.rotation = head.rotation;
                h.ctrl.position += head.position + head.forward * h.holdDistance - h.attach.position;
                return;
            }
        }

        var pos = restPos;
        if (h.swing >= 0f)
        {
            h.swing += Mathf.Min(Time.deltaTime, 1f / 30f) / swingDuration;   // au moins ~8 images par coup, même si une image traîne
            if (h.swing >= 1f) h.swing = -1f;
            else
            {
                float s = h.swing < 0.35f ? h.swing / 0.35f : 1f - (h.swing - 0.35f) / 0.65f;
                pos += h.swingDelta * Mathf.SmoothStep(0f, 1f, s);
            }
        }
        h.ctrl.position = pos;

        var pistol = h.HeldItem<Pistol>();
        if (pistol && pistol.muzzle)
        {
            // le canon vise le réticule : on tourne la main pour que muzzle.forward pointe vers le point visé
            var rel = Quaternion.Inverse(h.ctrl.rotation) * pistol.muzzle.rotation;
            var want = Quaternion.LookRotation(aimPoint - pistol.muzzle.position, head.up);
            h.ctrl.rotation = want * Quaternion.Inverse(rel);
        }
        else if (isRight || h.Held != null)
            h.ctrl.rotation = Quaternion.LookRotation(aimPoint - pos, head.up);   // le rayon des menus part vers le réticule
        else
            h.ctrl.rotation = head.rotation * Quaternion.Euler(70f, 0f, 0f);     // main gauche vide : vers le bas (pas de 2e rayon sur les menus)
    }

    void TryGrab(Hand h)
    {
        if (!Pick(out var target, out var point)) return;
        var manager = h.it.interactionManager;
        if (target is XRGrabInteractable g)
        {
            if (g.isSelected && g.firstInteractorSelecting is XRSocketInteractor sock && !(sock is BeltSocket))
                return;                                                   // déjà engagé dans le pistolet (chargeur)
            Grab(h, g, point);
            return;
        }
        // bouton, écran, levier, poignée : la main va se poser dessus
        Reach(h, point);
        bool keep = target.transform.GetComponentInParent<Lever>() || target.transform.GetComponentInParent<HingedDoor>();
        manager.SelectEnter((IXRSelectInteractor)h.it, target);
        if (keep && h.it.IsSelecting(target)) h.holdDistance = Vector3.Distance(head.position, point);
    }

    void Release(Hand h)
    {
        var held = h.Held;
        if (held == null) return;
        StopUse(h);
        h.holdDistance = -1f;
        var g = held as XRGrabInteractable;
        var item = g ? g.GetComponent<BeltItem>() : null;
        var socket = item ? FreeSocket(item.type) : null;
        h.it.interactionManager.SelectExit((IXRSelectInteractor)h.it, held);
        if (socket) h.it.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)g);        // objet de la ceinture : il retourne à son holster
    }

    void Use(Hand h)
    {
        var g = h.HeldGrab;
        if (!g) return;
        Transform striker = null;
        if (g.GetComponent<Hammer>()) striker = HammerHead.Instance ? HammerHead.Instance.transform : null;
        else { var mw = g.GetComponent<MeleeWeapon>(); if (mw) striker = mw.striker; }
        if (striker)
        {
            if (h.swing >= 0f) return;
            // coup : la tête de l'outil va jusqu'au point visé (ou 1 m devant), un peu au-delà, puis revient
            var target = Vector3.Distance(head.position, aimPoint) < 1.6f ? aimPoint : head.position + head.forward * 1.1f;
            h.swingDelta = Vector3.ClampMagnitude((target - striker.position) * 1.15f, 1.3f);
            h.swing = 0f;
            return;
        }
        h.it.activateInput.QueueManualState(true, 1f);                      // gâchette (pistolet, lampe)
        h.activating = true;
        h.useFrame = Time.frameCount;
    }

    void StopUse(Hand h)
    {
        if (!h.activating) return;
        h.it.activateInput.QueueManualState(false, 0f);
        h.activating = false;
    }

    // touches 1 / 2 / 3 : sortir l'objet de la ceinture (ou l'y ranger s'il est déjà en main)
    void FromBelt(Hand h, BeltItemType type)
    {
        var heldItem = h.HeldItem<BeltItem>();
        if (heldItem && heldItem.type == type) { Release(h); return; }
        var socket = Socket(type);
        var item = socket && socket.hasSelection ? socket.firstInteractableSelected as XRGrabInteractable : null;
        if (!item) return;
        if (h.Held != null) Release(h);
        Grab(h, item, item.colliders.Count > 0 && item.colliders[0] ? item.colliders[0].bounds.center : item.transform.position);
    }

    // la paume va d'abord sur l'objet (GrabGuard n'accepte qu'une prise à portée de main), puis revient au repos avec lui
    void Grab(Hand h, XRGrabInteractable g, Vector3 point)
    {
        Reach(h, point);
        h.it.interactionManager.SelectEnter((IXRSelectInteractor)h.it, (IXRSelectInteractable)g);
    }

    void Reach(Hand h, Vector3 point) => h.ctrl.position += point - h.attach.position;

    // R : recharger le pistolet (chargeur en main gauche, à la ceinture ou posé à portée), ou la lampe avec une pile
    public void Reload()
    {
        var pistol = Either<Pistol>();
        if (pistol)
        {
            var mag = Either<Magazine>();
            if (!mag) { var s = Socket(BeltItemType.Magazine); if (s && s.hasSelection) mag = (s.firstInteractableSelected as Component)?.GetComponent<Magazine>(); }
            if (!mag) mag = Nearest<Magazine>();
            if (mag) { pistol.Load(mag.rounds); mag.Consume(); }           // comme MagWell.Insert
            return;
        }
        var battery = Either<Battery>();
        if (!battery) return;
        var lamp = Either<Flashlight>();
        if (!lamp) { var s = Socket(BeltItemType.Flashlight); if (s && s.hasSelection) lamp = (s.firstInteractableSelected as Component)?.GetComponent<Flashlight>(); }
        if (!lamp) lamp = FindFirstObjectByType<Flashlight>();
        if (lamp) { lamp.Recharge(); battery.Consume(); }                    // comme le port de la lampe (Flashlight)
    }

    // objet tenu par l'une des deux mains (pas de ?? : GetComponent renvoie un faux null dans l'éditeur)
    T Either<T>() where T : Component
    {
        var a = right.HeldItem<T>();
        return a ? a : left.HeldItem<T>();
    }

    // ───────────────────────── recherche ─────────────────────────
    bool Pick(out IXRSelectInteractable target, out Vector3 point)
    {
        target = null; point = default;
        var manager = right.it.interactionManager;
        if (!manager) return false;
        foreach (var hit in Sorted(Physics.RaycastAll(new Ray(head.position, head.forward), reach, ~0, QueryTriggerInteraction.Collide)))
        {
            if (IsHeld(hit.collider)) continue;
            if (manager.TryGetInteractableForCollider(hit.collider, out var i) && i is IXRSelectInteractable s
                && s is Behaviour b && b.isActiveAndEnabled)
            {
                if (s.isSelected && !(s.firstInteractorSelecting is XRSocketInteractor)) continue;   // déjà dans une main
                target = s; point = hit.point;
                return true;
            }
            if (!hit.collider.isTrigger) return false;                         // un mur, une caisse : rien derrière
        }
        return false;
    }

    bool IsHeld(Collider c)
    {
        foreach (var h in new[] { right, left })
        {
            var g = h?.HeldGrab;
            if (g && c.transform.IsChildOf(g.transform)) return true;
        }
        return false;
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

    T Nearest<T>() where T : Component
    {
        T best = null;
        float bd = reach;
        foreach (var c in FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            var g = c.GetComponent<XRGrabInteractable>();
            if (g && g.isSelected) continue;
            float d = Vector3.Distance(head.position, c.transform.position);
            if (d < bd) { bd = d; best = c; }
        }
        return best;
    }

    static string Label(IXRSelectInteractable s)
    {
        var t = s.transform;
        if (t.GetComponentInParent<Lever>()) return "levier";
        if (t.GetComponentInParent<HingedDoor>()) return "porte";
        if (t.GetComponent<Pistol>()) return "pistolet";
        if (t.GetComponent<Hammer>()) return "marteau";
        if (t.GetComponent<Plank>()) return "planche";
        if (t.GetComponent<Magazine>()) return "chargeur";
        if (t.GetComponent<Flashlight>()) return "lampe";
        if (t.GetComponent<Battery>()) return "pile";
        if (t.GetComponent<MeleeWeapon>()) return "arme";
        return s is XRGrabInteractable ? t.name : "appuyer";
    }

    // ───────────────────────── affichage ─────────────────────────
    void OnGUI()
    {
        float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f;
        var old = GUI.color;
        GUI.color = hoverName != null ? new Color(1f, 0.75f, 0.27f) : new Color(1f, 1f, 1f, 0.7f);
        GUI.DrawTexture(new Rect(cx - 2f, cy - 2f, 4f, 4f), Texture2D.whiteTexture);
        if (hoverName != null) GUI.Label(new Rect(cx + 12f, cy - 10f, 300f, 22f), "E / clic : " + hoverName);
        GUI.color = old;

        if (Cursor.lockState != CursorLockMode.Locked)
            GUI.Box(new Rect(cx - 150f, cy + 30f, 300f, 26f), "Cliquez dans la fenêtre pour jouer");
        if (!help) { GUI.Label(new Rect(10f, 10f, 300f, 22f), "F1 : touches"); return; }
        GUI.Box(new Rect(10f, 10f, 330f, 300f),
            "VERSION PC (F1 pour masquer)\n\n" +
            "ZQSD : se déplacer · souris : regarder\n" +
            "C : s'accroupir\n" +
            "E : prendre / lâcher (main droite)\n" +
            "Clic gauche : tirer, frapper (marteau), appuyer\n" +
            "Clic droit : prendre / lâcher (main gauche)\n" +
            "F : utiliser l'objet de la main gauche (lampe)\n" +
            "1 : pistolet · 2 : marteau · 3 : lampe (ceinture)\n" +
            "R : recharger (chargeur / pile)\n" +
            "Levier, porte : E puis regarder / reculer\n" +
            "B ou Entrée (maintenir) : passer l'attente\n" +
            "Échap : pause · Espace : valider (menus)\n" +
            "N : passer une étape du tutoriel");
    }
}
