using System.Collections;
using UnityEngine;

// [Évasion] Portail de sortie du jardin (battants Garden_ExitGate_Left / _Right, cadenas Garden_ExitGate_Padlock, lampe
// Light_Garden_ExitGate : objets de la carte). Fermé pendant toute la partie. Quand le générateur démarre
// (WaveManager.EscapeOpened), il se déverrouille :
//   1. le cadenas tombe au pied du portail, la chaîne et la grille cliquettent (son de grille secouée, CC0) ;
//   2. les deux battants pivotent vers le jardin (95°, comme dans la cinématique de fin) et cognent leurs butées ;
//   3. la lampe au-dessus du portail passe au vert et pulse : on la voit depuis le jardin.
// Le joueur local qui franchit le portail (tête au-delà de la grille, côté route) déclenche la victoire
// (WaveManager.PlayerEscaped : bilan précédé de la cinématique). Marche en VR comme au PC : on suit la caméra du joueur.
// Le composant s'ajoute tout seul au chargement d'une scène qui contient le portail : rien à reconstruire dans MainScene.
// Les battants ne doivent pas être « Batching Static » (sinon ils ne bougent pas à l'écran) : PortailFer retire ce drapeau.
public class EscapeGate : MonoBehaviour
{
    const string LeftName = "Garden_ExitGate_Left", RightName = "Garden_ExitGate_Right";
    const string PadlockName = "Garden_ExitGate_Padlock", LightName = "Light_Garden_ExitGate";

    public Transform leftLeaf, rightLeaf, padlock;
    public Light lamp;
    public float openAngle = 95f;           // degrés, vers le jardin
    public float openDelay = 0.7f;          // le cadenas tombe d'abord
    public float openDuration = 1.6f;
    public float crossDepth = 0.9f;         // m au-delà de la grille (côté route) pour compter le passage
    public float halfWidth = 4f;            // m de part et d'autre du milieu du portail (le long de la grille)
    public float maxDepth = 12f;            // au-delà : ailleurs sur la carte, pas un passage du portail

    Vector3 axisPoint;                      // milieu du portail, au sol (repère du parent des battants)
    Quaternion leftClosed, rightClosed;
    AudioSource src;
    bool open, escaped;
    Color lampColor; float lampIntensity;

    public bool IsOpen => open;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Register()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 1re scène (lancement, mode Play de l'éditeur) : sceneLoaded n'est pas toujours appelé pour elle (comme DesktopMode)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AttachFirstScene() => OnSceneLoaded(default, UnityEngine.SceneManagement.LoadSceneMode.Single);

    static void OnSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
    {
        if (FindFirstObjectByType<EscapeGate>()) return;
        var left = GameObject.Find(LeftName);
        if (left && left.transform.parent) left.transform.parent.gameObject.AddComponent<EscapeGate>();
    }

    void Awake()
    {
        if (!leftLeaf) leftLeaf = Child(LeftName);
        if (!rightLeaf) rightLeaf = Child(RightName);
        if (!padlock) padlock = Child(PadlockName);
        if (!lamp) { var l = GameObject.Find(LightName); if (l) lamp = l.GetComponent<Light>(); }
        if (!leftLeaf || !rightLeaf) { enabled = false; return; }
        leftClosed = leftLeaf.localRotation; rightClosed = rightLeaf.localRotation;
        axisPoint = (leftLeaf.localPosition + rightLeaf.localPosition) * 0.5f;
        if (lamp) { lampColor = lamp.color; lampIntensity = lamp.intensity; }
        foreach (var t in new[] { leftLeaf, rightLeaf })
        {
            var r = t.GetComponent<Renderer>();
            if (r && r.isPartOfStaticBatch) Debug.LogWarning("[EscapeGate] " + t.name + " est en batching statique : il ne pivotera pas à l'écran (PortailFer doit retirer le drapeau).");
        }
        var snd = new GameObject("Son_Portail");               // objet à part : le composant est sur le parent de la carte, qu'il ne faut pas déplacer
        snd.transform.SetParent(transform, false);
        snd.transform.localPosition = axisPoint + Vector3.up * 1.2f;
        src = snd.AddComponent<AudioSource>();
        src.playOnAwake = false; src.spatialBlend = 1f; src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.minDistance = 3f; src.maxDistance = 40f;
        WaveManager.EscapeOpened += Open;
    }

    void OnDestroy() => WaveManager.EscapeOpened -= Open;

    Transform Child(string n)
    {
        foreach (Transform c in transform) if (c.name == n) return c;
        var g = GameObject.Find(n);
        return g ? g.transform : null;
    }

    /// <summary>Déverrouille et ouvre le portail (une fois). Appelé par WaveManager.EscapeOpened.</summary>
    public void Open()
    {
        if (open || !enabled) return;
        open = true;
        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        var rattle = Resources.Load<AudioClip>("Audio/Portail_Grille_Cadenas");
        var bang = Resources.Load<AudioClip>("Audio/Portail_Grille_Claque");
        if (rattle) src.PlayOneShot(rattle, 0.9f);
        if (padlock) StartCoroutine(DropPadlock());
        yield return new WaitForSeconds(openDelay);

        // les battants pivotent vers le jardin : vite au début, freinés en fin de course, puis rebond sur la butée
        float l0 = 0f;
        for (float t = 0f; t < openDuration; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / openDuration), 3f);
            l0 = openAngle * k;
            SetAngle(l0);
            yield return null;
        }
        if (bang) src.PlayOneShot(bang, 0.8f);
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            SetAngle(openAngle - 4f * Mathf.Sin(t / 0.5f * Mathf.PI) * (1f - t / 0.5f));
            yield return null;
        }
        SetAngle(openAngle);
    }

    // gauche : charnière côté +Z, le battant s'étend vers -Z ; +angle autour de Y l'envoie vers -X (le jardin). Droite : l'inverse.
    void SetAngle(float a)
    {
        leftLeaf.localRotation = Quaternion.Euler(0f, a, 0f) * leftClosed;
        rightLeaf.localRotation = Quaternion.Euler(0f, -a, 0f) * rightClosed;
    }

    IEnumerator DropPadlock()
    {
        foreach (var c in padlock.GetComponents<Collider>()) c.enabled = false;
        Vector3 p = padlock.localPosition;
        float ground = axisPoint.y + 0.06f, v = 0f;
        var spin = Quaternion.Euler(Random.Range(-40f, 40f), Random.Range(-90f, 90f), 75f);
        var r0 = padlock.localRotation;
        Vector3 drift = new Vector3(-0.25f, 0f, Random.Range(-0.15f, 0.15f));      // tombe côté jardin
        Vector3 start = p;
        for (float t = 0f; p.y > ground; t += Time.deltaTime)
        {
            v += 9.81f * Time.deltaTime;
            p.y -= v * Time.deltaTime;
            float k = Mathf.Clamp01(t / 0.5f);
            p.x = start.x + drift.x * k; p.z = start.z + drift.z * k;
            padlock.localPosition = new Vector3(p.x, Mathf.Max(p.y, ground), p.z);
            padlock.localRotation = Quaternion.Slerp(r0, spin * r0, k);
            yield return null;
        }
    }

    void Update()
    {
        if (!open) return;
        if (lamp)
        {
            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 3f);
            lamp.color = Color.Lerp(lampColor, new Color(0.45f, 1f, 0.55f), 0.55f);
            lamp.intensity = lampIntensity * 1.3f * pulse;
        }
        if (escaped) return;
        var wm = WaveManager.Instance;
        if (!wm || !wm.Escaping) return;
        if (PlayerHealth.Instance && PlayerHealth.Instance.Dead) return;
        var cam = Camera.main;
        if (!cam) return;
        // côté route = +X du repère du portail (le jardin est en -X)
        var lp = transform.InverseTransformPoint(cam.transform.position) - axisPoint;
        if (lp.x > crossDepth && lp.x < maxDepth && Mathf.Abs(lp.z) < halfWidth)
        {
            escaped = true;
            wm.PlayerEscaped();
        }
    }
}
