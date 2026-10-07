using System;
using UnityEngine;

// Un bouton du menu (pointé par le rayon d'une manette, validé à la gâchette) : fond, texte, flèche, état « choisi ».
public class MenuButton : MonoBehaviour
{
    public Renderer background;
    public TextMesh label;
    public TextMesh arrow;                  // ▶ affiché au survol (boutons principaux)
    public Color accent = UiKit.Amber;
    public Color textColor = UiKit.White;
    public bool interactable = true;
    public bool selected;                   // option choisie (réglages)
    public Action onClick;

    bool hovered;
    float glow;

    public void SetHover(bool h) => hovered = h && interactable;

    public void Click()
    {
        if (!interactable) return;
        glow = 1f;
        onClick?.Invoke();
    }

    void Update()
    {
        glow = Mathf.MoveTowards(glow, 0f, Time.deltaTime * 4f);
        float a = !interactable ? 0.04f : selected ? 0.42f : hovered ? 0.3f : 0.1f;
        if (background) { var c = accent; c.a = Mathf.Clamp01(a + glow * 0.5f); background.material.color = c; }
        if (label) label.color = !interactable ? new Color(0.45f, 0.42f, 0.36f) : (hovered || selected) ? accent : textColor;
        if (arrow) arrow.gameObject.SetActive(hovered);
    }
}
