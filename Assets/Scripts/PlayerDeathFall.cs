using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

// Mort du joueur : le joueur s'effondre avant l'écran de fin (GameOverUI le joue avant son fondu au noir).
//   - éclair rouge, puis la vue se resserre (bords de plus en plus noirs) ;
//   - la vue tombe jusqu'au sol (≈ 35 cm du sol) en ~0,8 s, petit rebond à l'impact, son sourd ;
//   - trois battements de cœur qui ralentissent ;
//   - déplacements, rotation et accroupissement coupés.
// Confort au casque : la chute est uniquement verticale, sans rotation de la tête (la rotation imposée rend malade en VR).
// En version PC (DesktopMode), la vue bascule aussi sur le côté, comme une caméra qui tombe.
public static class PlayerDeathFall
{
    public const float Duration = 2.6f;     // durée totale avant le fondu au noir de GameOverUI
    const float FallTime = 0.8f;            // chute (accélérée, comme un corps qui s'effondre)
    const float EyesAboveGround = 0.35f;

    public static IEnumerator Play(Transform head)
    {
        var rig = head ? head.GetComponentInParent<XROrigin>() : null;
        if (rig == null) { yield return new WaitForSeconds(Duration); yield break; }

        // plus de déplacement ni d'accroupissement (Crouch remettrait la hauteur de la vue à chaque image)
        foreach (var lp in rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>(true)) lp.enabled = false;
        var crouch = rig.GetComponent<Crouch>(); if (crouch) crouch.enabled = false;
        bool desktop = DesktopMode.On;
        var desk = rig.GetComponent<DesktopPlayer>(); if (desk) desk.enabled = false;

        var offset = rig.CameraFloorOffsetObject ? rig.CameraFloorOffsetObject.transform : head.parent;
        Vector3 start = offset.localPosition;
        Quaternion startRot = offset.localRotation;
        float eyes = head.position.y - rig.transform.position.y;
        float drop = Mathf.Clamp(eyes - EyesAboveGround, 0f, 1.4f);

        // éclair rouge puis rideau sombre : deux quads collés à la vue, dessinés après le vignettage de dégâts (file 4100) pour l'assombrir,
        // sous les textes de GameOverUI (4200) ; ils finissent presque noirs, donc invisibles sur son écran noir
        float near = Camera.main ? Camera.main.nearClipPlane : 0.05f;
        var red = HeadOverlay.Create(head, "MortRouge", null, new Color(0.55f, 0f, 0f, 0.6f), near + 0.05f, 4101);
        red.transform.localScale = Vector3.one * 6f;
        // dégradé radial : centre transparent, bords noirs ; taillé pour que ses coins tombent juste aux coins du champ de vision
        var tunnel = HeadOverlay.Create(head, "MortTunnel", HeadOverlay.RadialTexture(256, 0.12f), new Color(0f, 0f, 0f, 0f), near + 0.04f, 4102);
        var cam = head.GetComponent<Camera>();
        float tanV = Mathf.Tan((cam ? cam.fieldOfView : 90f) * 0.5f * Mathf.Deg2Rad), tanH = tanV * (cam ? cam.aspect : 1f);
        tunnel.transform.localScale = Vector3.one * (2f * (near + 0.04f) * Mathf.Sqrt(tanV * tanV + tanH * tanH) * 1.02f);

        var src = head.gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
        var beat = ProcAudio.Heartbeat();
        var thud = ProcAudio.Thud();
        src.PlayOneShot(beat, 1f);
        float[] beats = { 0.95f, 1.75f };     // battements suivants, de plus en plus espacés
        int nextBeat = 0;
        bool landed = false;
        float tilt = desktop ? 75f : 0f;      // PC seulement : la tête bascule sur le côté

        for (float t = 0f; t < Duration; t += Time.deltaTime)
        {
            float f = Mathf.Clamp01(t / FallTime);
            float y = drop * f * f;                                          // chute accélérée
            if (t > FallTime) y = drop - 0.05f * Mathf.Sin(Mathf.Clamp01((t - FallTime) / 0.2f) * Mathf.PI) * Mathf.Exp(-(t - FallTime) * 6f);   // rebond
            offset.localPosition = start - Vector3.up * y;
            if (tilt > 0f) offset.localRotation = startRot * Quaternion.Euler(-12f * f, 0f, tilt * Mathf.SmoothStep(0f, 1f, f));

            if (!landed && t >= FallTime) { landed = true; src.PlayOneShot(thud, 0.9f); VRInput.HapticBoth(0.6f, 0.15f); }
            if (nextBeat < beats.Length && t >= beats[nextBeat]) { src.PlayOneShot(beat, 0.8f - 0.25f * nextBeat); nextBeat++; }

            // éclair rouge vif, puis voile rouge sombre qui s'épaissit ; les bords noircissent d'abord (tunnel)
            float k = Mathf.Clamp01(t / Duration);
            red.material.color = t < 0.5f ? new Color(0.55f, 0f, 0f, Mathf.Lerp(0.5f, 0.15f, t / 0.5f))
                                          : Color.Lerp(new Color(0.55f, 0f, 0f, 0.15f), new Color(0.06f, 0f, 0f, 0.7f), k);
            tunnel.material.color = new Color(0f, 0f, 0f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / (Duration * 0.7f))));
            yield return null;
        }
    }

    // GameOverUI, une fois l'écran tout noir : on retire les voiles (sinon ils teinteraient le fond du bilan)
    public static void Clear(Transform head)
    {
        if (!head) return;
        foreach (Transform c in head)
            if (c.name == "MortRouge" || c.name == "MortTunnel") Object.Destroy(c.gameObject);
    }
}
