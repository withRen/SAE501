using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Radio de ravitaillement (construite par RoomBuilder.BuildSupplyRadio) :
//   1. le joueur appuie sur le bouton rouge de la radio (grip sur le bouton) ;
//   2. la radio répond (grésillement) et affiche un compte à rebours ;
//   3. à la fin, une caisse tombe par le trou du toit avec un chargeur plein (12 balles) dedans.
// Un seul appel par vague. Attente selon la difficulté (PARAMÈTRES) : Facile 15 s · Normal 30 s · Difficile 50 s.
// Un seul chargeur par caisse, quelle que soit la difficulté (décision de l'équipe : les munitions restent rares).
// Mode vagues infinies : 2 chargeurs à partir de la vague 5, 3 à partir de la vague 10 (WaveManager.RadioMagazines).
// Le compte à rebours suit le temps du jeu : il s'arrête pendant la pause.
// [Hotfix #164] Au casque, le bouton ne réagissait qu'au grip d'une main vide (sur PC, le clic le sélectionne directement) :
//   - il se déclenche aussi à la gâchette (activate) et au simple toucher d'une main (survol par l'interacteur direct ; pas au PC,
//     où les mains suivent le regard et toucheraient le bouton sans le vouloir) ;
//   - une main qui tient déjà un objet et touche le bouton : son grip est ignoré tant qu'elle est dessus (la prise par clic lâchait l'arme) ;
//   - retour : bouton enfoncé, son, vibration de la main ; anti-rebond de 0,6 s (toucher + gâchette + grip = un seul appel) ;
//     appui alors que la radio n'est pas disponible : petit bip (le bouton marche, la radio est déjà utilisée).
public class SupplyRadio : MonoBehaviour
{
    public XRSimpleInteractable button;
    public Renderer buttonRenderer;
    public TextMesh display;
    public Transform dropFrom;              // point d'où tombe la caisse (au-dessus du plafond s'il y a un trou)
    public GameObject crateTemplate;        // caisse cachée, copiée à chaque livraison
    public GameObject magazineTemplate;     // chargeur caché, copié dans la caisse

    public float[] delayByDifficulty = { 15f, 30f, 50f };     // Facile, Normal, Difficile
    public int[] magazinesByDifficulty = { 1, 1, 1 };

    enum State { Ready, Waiting, Used }
    State state;
    int usedForLevel = -1;
    float arriveAt;
    GameObject lastCrate;
    int shownLevel = -1;
    AudioSource audioSrc;
    AudioClip chirp, whoosh;

    static readonly Color Amber = new Color(1f, 0.75f, 0.27f), Green = new Color(0.35f, 1f, 0.55f), Red = new Color(1f, 0.3f, 0.25f);

    static int Diff => Mathf.Clamp((int)GameSettings.Difficulty, 0, 2);
    static int WaveKey => Mathf.Max(1, WaveManager.Level);     // avant la 1re vague : compte comme la vague 1

    void Start()
    {
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.spatialBlend = 1f; audioSrc.minDistance = 0.5f; audioSrc.maxDistance = 12f; audioSrc.playOnAwake = false;
        chirp = ProcAudio.RadioChirp();
        whoosh = ProcAudio.Whoosh();
        if (button)
        {
            button.selectEntered.AddListener(a => Press(a.interactorObject));
            button.activated.AddListener(a => Press(a.interactorObject));            // gâchette
            button.hoverEntered.AddListener(OnHoverEnter);                          // toucher
            button.hoverExited.AddListener(OnHoverExit);
            buttonRest = button.transform.localPosition;
        }
        if (crateTemplate) crateTemplate.SetActive(false);
        if (magazineTemplate) magazineTemplate.SetActive(false);
        Refresh();
    }

    Vector3 buttonRest;
    float lastPress = -10f, pressedUntil = -1f;
    readonly System.Collections.Generic.Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor, GripGate> gates =
        new System.Collections.Generic.Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor, GripGate>();

    // toucher le bouton avec la main (au casque) ; si cette main tient un objet, son grip est ignoré tant qu'elle reste dessus
    /// <summary>Tests dans l'éditeur (version PC) : le toucher marche quand même.</summary>
    public static bool ForceTouchForTests;

    // main vide : XRI la fait « survoler » le bouton quand elle le touche
    void OnHoverEnter(UnityEngine.XR.Interaction.Toolkit.HoverEnterEventArgs a)
    {
        if (DesktopMode.On && !ForceTouchForTests) return;
        if (a.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor
            || a.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor) Press(a.interactorObject);
    }

    void OnHoverExit(UnityEngine.XR.Interaction.Toolkit.HoverExitEventArgs a) { }

    // main qui tient un objet : XRI ne la fait survoler rien d'autre ; on regarde nous-mêmes si elle touche le bouton (3 cm).
    // Tant qu'elle est dessus, son grip est gelé (la prise par clic ne lâche pas l'objet) et le toucher appuie sur le bouton.
    UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor[] hands;
    Collider buttonCol;

    void CheckHeldHands()
    {
        if ((DesktopMode.On && !ForceTouchForTests) || !button) return;
        if (hands == null || hands.Length == 0) hands = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>(FindObjectsSortMode.None);
        if (!buttonCol) buttonCol = button.GetComponent<Collider>();
        if (!buttonCol) return;
        foreach (var h in hands)
        {
            if (!h) continue;
            var hp = h.attachTransform ? h.attachTransform.position : h.transform.position;
            bool touching = h.isActiveAndEnabled && h.hasSelection && (buttonCol.ClosestPoint(hp) - hp).sqrMagnitude < 0.03f * 0.03f;
            bool gated = gates.TryGetValue(h, out var g);
            if (touching && !gated)
            {
                g = new GripGate { raw = h.selectInput };
                h.selectInput.bypass = g;
                gates[h] = g;
                Press(h);
            }
            else if (!touching && gated)
            {
                if (h.selectInput.bypass == g) h.selectInput.bypass = null;
                gates.Remove(h);
            }
        }
    }

    void OnDestroy()
    {
        foreach (var kv in gates) if (kv.Key && kv.Key.selectInput.bypass == kv.Value) kv.Key.selectInput.bypass = null;
        gates.Clear();
    }

    // grip « gelé » : ni appui ni relâchement vus (la prise par clic ne bascule pas), l'état maintenu reste le vrai
    class GripGate : UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.IXRInputButtonReader
    {
        public UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputButtonReader raw;
        public bool ReadIsPerformed() => raw.ReadIsPerformed();
        public bool ReadWasPerformedThisFrame() => false;
        public bool ReadWasCompletedThisFrame() => false;
        public float ReadValue() => raw.ReadValue();
        public bool TryReadValue(out float value) => raw.TryReadValue(out value);
    }

    /// <summary>Appui sur le bouton (grip, gâchette, toucher ou clic PC), avec anti-rebond et retour.</summary>
    public void Press(object by = null)
    {
        if (PauseMenu.Paused || Time.unscaledTime - lastPress < 0.6f) return;
        lastPress = Time.unscaledTime;
        pressedUntil = Time.unscaledTime + 0.18f;                  // bouton enfoncé
        if (by is Component c) VRInput.Haptic(VRInput.HandOf(c), 0.5f, 0.08f);
        if (state == State.Ready) Call();
        else if (audioSrc) audioSrc.PlayOneShot(ProcAudioClick(), 0.5f);   // déjà utilisée : petit bip
    }

    static AudioClip click;
    static AudioClip ProcAudioClick() { if (!click) click = ProcAudio.Click(); return click; }

    public void Call()
    {
        if (state != State.Ready || PauseMenu.Paused) return;
        state = State.Waiting;
        usedForLevel = WaveKey;
        arriveAt = Time.time + delayByDifficulty[Diff];
        audioSrc.PlayOneShot(chirp, 0.8f);
        StartCoroutine(Deliver());
    }

    IEnumerator Deliver()
    {
        while (Time.time < arriveAt) { Refresh(); yield return null; }
        // le bruit arrive d'en haut juste avant la caisse
        if (dropFrom) AudioSource.PlayClipAtPoint(whoosh, dropFrom.position, 1f);
        yield return new WaitForSeconds(1f);

        if (lastCrate) Destroy(lastCrate);             // l'ancienne caisse disparaît (ses chargeurs restent)
        var crate = Instantiate(crateTemplate, dropFrom.position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        crate.name = "Caisse_Ravitaillement";
        crate.SetActive(true);
        lastCrate = crate;
        var sc = crate.GetComponent<SupplyCrate>();

        // on attend que la caisse touche le sol (ou 4 s au plus), puis les chargeurs apparaissent dedans
        for (float t = 0f; t < 4f && (sc == null || !sc.Landed); t += Time.deltaTime) yield return null;
        yield return new WaitForSeconds(0.3f);
        int n = magazinesByDifficulty[Diff];
        if (WaveManager.Instance) n = WaveManager.Instance.RadioMagazines(n);     // mode survie : 2 chargeurs dès la vague 5, 3 dès la vague 10
        for (int i = 0; i < n; i++)
        {
            var off = crate.transform.TransformDirection(new Vector3((i - (n - 1) / 2f) * 0.08f, 0.12f, 0f));
            var m = Instantiate(magazineTemplate, crate.transform.position + off, crate.transform.rotation * Quaternion.Euler(0f, 0f, 90f));
            m.name = "Magazine";
            m.SetActive(true);
        }
        audioSrc.PlayOneShot(chirp, 0.5f);
        state = State.Used;
        Refresh();
    }

    void Update()
    {
        CheckHeldHands();
        // bouton enfoncé de 12 mm pendant 0,18 s après un appui (l'avant de la radio est vers +Z local)
        if (button) button.transform.localPosition = buttonRest + (Time.unscaledTime < pressedUntil ? new Vector3(0f, 0f, -0.012f) : Vector3.zero);
        // nouvelle vague : la radio redevient disponible
        if (state == State.Used && WaveKey > usedForLevel) { state = State.Ready; Refresh(); }
        if (shownLevel != WaveKey) { shownLevel = WaveKey; Refresh(); }     // le nombre de chargeurs affiché suit la vague (mode survie)
        if (state == State.Ready && buttonRenderer)
        {
            float a = 0.5f + 0.5f * Mathf.Sin(Time.time * 4f);       // le bouton clignote quand on peut appeler
            buttonRenderer.material.color = Color.Lerp(new Color(0.35f, 0.05f, 0.05f), Red, a);
        }
    }

    void Refresh()
    {
        if (!display) return;
        string t; Color c;
        switch (state)
        {
            case State.Waiting:
                t = "REÇU\nCAISSE DANS\n" + Mathf.CeilToInt(Mathf.Max(0f, arriveAt - Time.time)) + " s"; c = Green; break;
            case State.Used:
                t = "INDISPONIBLE\nPROCHAINE\nVAGUE"; c = Red; break;
            default:
                int n = WaveManager.Instance ? WaveManager.Instance.RadioMagazines(magazinesByDifficulty[Diff]) : magazinesByDifficulty[Diff];
                t = "RAVITAILLEMENT\n" + n + (n > 1 ? " CHARGEURS" : " CHARGEUR") + "\n▶ BOUTON ROUGE"; c = Amber; break;
        }
        if (display.text != t) display.text = t;
        display.color = c;
        if (state != State.Ready && buttonRenderer) buttonRenderer.material.color = new Color(0.25f, 0.05f, 0.05f);
    }
}
