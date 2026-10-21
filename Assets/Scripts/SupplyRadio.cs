using System.Collections;
using UnityEngine;

// Radio de ravitaillement (construite par RoomBuilder.BuildSupplyRadio) :
//   1. le joueur décroche le combiné (grip) et le porte à son oreille (RadioHandset) ;
//   2. la radio répond dans l'écouteur (grésillement + vibration de la manette) et affiche un compte à rebours ;
//   3. à la fin, une caisse tombe par le trou du toit avec un chargeur plein (12 balles) dedans.
// On raccroche en lâchant le combiné : il revient sur son socle. Hors service : tonalité « occupé » dans l'écouteur.
// Un seul appel par vague. Attente selon la difficulté (PARAMÈTRES) : Facile 15 s · Normal 30 s · Difficile 50 s.
// Un seul chargeur par caisse, quelle que soit la difficulté (décision de l'équipe : les munitions restent rares).
// Le compte à rebours suit le temps du jeu : il s'arrête pendant la pause.
public class SupplyRadio : MonoBehaviour
{
    public RadioHandset handset;
    public Renderer lamp;                   // voyant qui clignote quand on peut appeler
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
    AudioSource audioSrc, earSrc;           // haut-parleur de la radio, écouteur du combiné
    AudioClip chirp, whoosh, beep;

    static readonly Color Amber = new Color(1f, 0.75f, 0.27f), Green = new Color(0.35f, 1f, 0.55f), Red = new Color(1f, 0.3f, 0.25f);

    static int Diff => Mathf.Clamp((int)GameSettings.Difficulty, 0, 2);
    static int WaveKey => Mathf.Max(1, WaveManager.Level);     // avant la 1re vague : compte comme la vague 1

    void Start()
    {
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.spatialBlend = 1f; audioSrc.minDistance = 0.5f; audioSrc.maxDistance = 12f; audioSrc.playOnAwake = false;
        chirp = ProcAudio.RadioChirp();
        whoosh = ProcAudio.Whoosh();
        beep = ProcAudio.Click();
        if (handset)
        {
            earSrc = handset.gameObject.AddComponent<AudioSource>();
            earSrc.spatialBlend = 1f; earSrc.minDistance = 0.05f; earSrc.maxDistance = 2f; earSrc.playOnAwake = false;
            handset.PutToEar += OnEar;
            handset.Lifted += Refresh;
            handset.HungUp += Refresh;
        }
        if (crateTemplate) crateTemplate.SetActive(false);
        if (magazineTemplate) magazineTemplate.SetActive(false);
        Refresh();
    }

    void OnDestroy()
    {
        if (!handset) return;
        handset.PutToEar -= OnEar;
        handset.Lifted -= Refresh;
        handset.HungUp -= Refresh;
    }

    // combiné porté à l'oreille
    void OnEar()
    {
        if (PauseMenu.Paused) return;
        switch (state)
        {
            case State.Ready: Call(); break;
            case State.Waiting: EarSound().PlayOneShot(chirp, 0.35f); break;     // déjà commandé : simple accusé
            default: StartCoroutine(Busy()); break;                               // plus de caisse cette vague
        }
    }

    public void Call()
    {
        if (state != State.Ready || PauseMenu.Paused) return;
        state = State.Waiting;
        usedForLevel = WaveKey;
        arriveAt = Time.time + delayByDifficulty[Diff];
        EarSound().PlayOneShot(chirp, 0.8f);
        if (handset && handset.Holder) VRInput.Haptic(VRInput.HandOf(handset.Holder), 0.35f, 0.25f);
        Refresh();
        StartCoroutine(Deliver());
    }

    AudioSource EarSound() => handset && handset.IsHeld && earSrc ? earSrc : audioSrc;

    IEnumerator Busy()
    {
        for (int i = 0; i < 3 && handset && handset.AtEar; i++)
        {
            EarSound().PlayOneShot(beep, 0.7f);
            yield return new WaitForSeconds(0.25f);
        }
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
        // nouvelle vague : la radio redevient disponible
        if (state == State.Used && WaveKey > usedForLevel) { state = State.Ready; Refresh(); }
        if (state == State.Ready && lamp)
        {
            float a = 0.5f + 0.5f * Mathf.Sin(Time.time * 4f);       // le voyant clignote quand on peut appeler
            lamp.material.color = Color.Lerp(new Color(0.35f, 0.05f, 0.05f), Red, a);
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
                t = handset && handset.IsHeld ? "À L'OREILLE\nPOUR\nCOMMANDER" : "RAVITAILLEMENT\n1 CHARGEUR\n▶ DÉCROCHEZ"; c = Amber; break;
        }
        if (display.text != t) display.text = t;
        display.color = c;
        if (state != State.Ready && lamp) lamp.material.color = new Color(0.25f, 0.05f, 0.05f);
    }
}
