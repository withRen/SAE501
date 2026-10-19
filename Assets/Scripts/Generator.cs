using UnityEngine;

// Objectif du jeu : activer le générateur (salle Générateur, déverrouillée au niveau 4).
// [#111] Levier physique à ressort (Lever) : on l'attrape (grip), on l'ABAISSE jusqu'en bas et on le tient `holdSeconds` ;
// lâché, il remonte tout seul et la jauge redescend.
// À la fin : lumière de la salle rallumée, victoire (WaveManager.GeneratorActivated).
// [#120] Au niveau 4, le levier reste bloqué tant que WaveManager.GeneratorReady est faux (X zombies abattus ou 60 s) ;
// le panneau affiche ce qu'il reste à faire.
public class Generator : MonoBehaviour
{
    public Lever lever;
    public Renderer buttonRenderer;     // voyant (boule du levier) : rouge -> orange pendant le démarrage -> vert
    public TextMesh label;
    public Light lamp;                  // lumière de la salle
    public Light[] roomLights;          // autres lumières de la salle, presque éteintes avant le démarrage
    public float[] roomLightLevels;     // leur intensité une fois le générateur démarré
    public int unlockLevel = 4;
    public float holdSeconds = 5f;
    public float releaseDecay = 0.5f;   // la jauge redescend à cette vitesse relative quand on lâche

    float progress, nextLabel;
    bool on;
    AudioSource hum;
    Color buttonOff = new Color(0.6f, 0.1f, 0.1f), buttonOn = new Color(0.2f, 1f, 0.3f);

    public bool IsOn => on;

    void Start()
    {
        hum = gameObject.AddComponent<AudioSource>();
        hum.spatialBlend = 1f; hum.loop = true; hum.volume = 0.5f; hum.maxDistance = 12f;
        var n = 22050;
        var clip = AudioClip.Create("generator_hum", n, 1, 22050, false);
        var d = new float[n];
        for (int i = 0; i < n; i++) { float t = i / 22050f; d[i] = (Mathf.Sin(t * 55f * 6.2832f) * 0.5f + Mathf.Sin(t * 110f * 6.2832f) * 0.3f + Mathf.Sin(t * 165f * 6.2832f) * 0.15f) * 0.6f; }
        clip.SetData(d, 0);
        hum.clip = clip;
        SetButtonColor(buttonOff);
    }

    void SetButtonColor(Color c)
    {
        if (buttonRenderer) buttonRenderer.material.SetColor("_BaseColor", c);
    }

    void Update()
    {
        if (on) return;
        bool unlocked = WaveManager.Level >= unlockLevel;
        var wm = WaveManager.Instance;
        bool ready = unlocked && (wm == null || wm.GeneratorReady);
        if (lever) lever.locked = !ready;                    // avant le niveau 4, puis tant que la condition n'est pas remplie : levier bloqué en haut
        if (!unlocked) { SetLabel("HORS SERVICE"); return; }
        if (!ready)
        {
            if (Time.time >= nextLabel) { nextLabel = Time.time + 0.25f; SetLabel("DÉMARRAGE BLOQUÉ\n" + wm.GeneratorCondition); }
            return;
        }

        bool held = lever != null && lever.IsDown;           // levier à ressort : en bas seulement tant qu'on le tient
        progress = Mathf.Clamp(progress + (held ? 1f : -releaseDecay) * Time.deltaTime, 0f, holdSeconds);
        SetLabel(progress <= 0f ? "ABAISSEZ LE LEVIER\net tenez-le en bas " + holdSeconds.ToString("0") + " s" : "DÉMARRAGE… " + Mathf.RoundToInt(progress / holdSeconds * 100f) + " %");
        SetButtonColor(Color.Lerp(buttonOff, new Color(1f, 0.7f, 0.1f), progress / holdSeconds));

        if (progress >= holdSeconds) Activate();
    }

    public void Activate()
    {
        on = true;
        if (lever) { lever.Snap(true); lever.locked = true; }      // le levier reste en bas
        SetLabel("GÉNÉRATEUR ACTIVÉ");
        SetButtonColor(buttonOn);
        if (lamp) { lamp.color = new Color(1f, 0.8f, 0.5f); lamp.intensity = 3f; lamp.range = 9f; }
        if (roomLights != null)
            for (int i = 0; i < roomLights.Length; i++)
                if (roomLights[i]) roomLights[i].intensity = roomLightLevels != null && i < roomLightLevels.Length ? roomLightLevels[i] : 1f;
        if (hum) hum.Play();
        if (WaveManager.Instance) WaveManager.Instance.GeneratorActivated();
    }

    void SetLabel(string s) { if (label && label.text != s) label.text = s; }
}
