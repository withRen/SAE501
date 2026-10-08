using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Objectif du jeu : activer le générateur (salle Générateur, déverrouillée au niveau 4).
// Il faut actionner l'interrupteur (un appui sur grip, prise par clic : ClickGrab) et garder la main dessus pendant `holdSeconds` ;
// si on retire la main ou qu'on rappuie, la jauge redescend.
// À la fin : lumière de la salle rallumée, victoire (WaveManager.GeneratorActivated).
public class Generator : MonoBehaviour
{
    public XRSimpleInteractable button;
    public Renderer buttonRenderer;
    public TextMesh label;
    public Light lamp;                  // lumière de la salle
    public Light[] roomLights;          // autres lumières de la salle, presque éteintes avant le démarrage
    public float[] roomLightLevels;     // leur intensité une fois le générateur démarré
    public int unlockLevel = 4;
    public float holdSeconds = 5f;
    public float releaseDecay = 0.5f;   // la jauge redescend à cette vitesse relative quand on lâche

    float progress;
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
        // prise par clic : la sélection ne s'arrête plus en relâchant grip, donc on la coupe si la main s'éloigne
        if (button != null && button.isSelected && !button.isHovered)
            button.interactionManager.SelectExit(button.firstInteractorSelecting, button);
        if (on) return;
        bool unlocked = WaveManager.Level >= unlockLevel;
        if (!unlocked) { SetLabel("HORS SERVICE"); return; }

        bool held = button != null && button.isSelected;
        progress = Mathf.Clamp(progress + (held ? 1f : -releaseDecay) * Time.deltaTime, 0f, holdSeconds);
        SetLabel(progress <= 0f ? "APPUYEZ SUR L'INTERRUPTEUR\net gardez la main dessus" : "DÉMARRAGE… " + Mathf.RoundToInt(progress / holdSeconds * 100f) + " %");
        SetButtonColor(Color.Lerp(buttonOff, new Color(1f, 0.7f, 0.1f), progress / holdSeconds));

        if (progress >= holdSeconds) Activate();
    }

    public void Activate()
    {
        on = true;
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
