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
        if (button) button.selectEntered.AddListener(_ => Call());
        if (crateTemplate) crateTemplate.SetActive(false);
        if (magazineTemplate) magazineTemplate.SetActive(false);
        Refresh();
    }

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
