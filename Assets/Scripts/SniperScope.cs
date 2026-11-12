using UnityEngine;
using UnityEngine.Rendering.Universal;

// Lunette grossissante du sniper (AWM du duel 1v1), sur le prefab Resources/Weapons/Sniper.
//
// Au casque : une vraie lunette. Une caméra fixée à l'avant de la lunette (champ étroit : grossissement × `magnification`) rend
// dans une petite RenderTexture (`resolution`²), affichée sur un disque posé sur l'oculaire (shader SAE501/ScopeLens : réticule
// mil-dot, point rouge, vignettage, ombre quand l'œil n'est pas dans l'axe). Cette caméra ne tourne que quand un œil est près de
// l'oculaire (moins de `activationDistance`) et que la tête regarde à peu près dans l'axe (moins de `activationAngle`) ; sinon elle est
// éteinte et la lentille montre un verre sombre avec un reflet. Pas de post-traitement, pas d'ombres, pas d'anticrénelage.
// Seulement pour l'arme du joueur local : chaque casque a son propre sniper dans sa copie de l'arène (DuelLoadout) ; l'adversaire n'en
// voit qu'une copie visuelle (NetPlayer.VisualCopy, qui ignore la lentille) : aucune caméra de lunette chez lui.
//
// Version PC : rien ne s'active ici. DesktopPlayer zoome au clic droit (champ de vision ÷ `magnification`, sensibilité × `pcSensitivity`)
// et DesktopHud affiche la lunette plein écran ; l'arme et les mains sont cachées pendant le zoom (PcScoped).
//
// Géométrie (repère du prefab : Z = canon, Y = haut, origine = poignée) : mesurée sur la pièce « Scope » du modèle AWM (pack VCR01).
[DisallowMultipleComponent]
public class SniperScope : MonoBehaviour
{
    [Header("Optique")]
    [Range(1.5f, 12f)] public float magnification = 5f;        // grossissement (casque et PC)
    public int resolution = 512;                                // taille de l'image de la lunette (casque)
    public float farClip = 250f;
    public bool shadows = false;                                // ombres dans la lunette (coûteux sur Quest)

    [Header("Activation au casque")]
    public float activationDistance = 0.14f;                    // œil ↔ oculaire (m)
    public float activationAngle = 30f;                         // tête ↔ axe de la lunette (°)
    public float eyeRelief = 0.075f;                            // distance idéale œil ↔ oculaire (image entière)
    public float eyeTolerance = 0.012f;                         // décalage de l'œil hors de l'axe avant que l'ombre n'apparaisse (m)
    public float halfIpd = 0.032f;                              // demi-écart des yeux (on vise d'un œil)

    [Header("Géométrie (repère du prefab)")]
    public Vector3 eyepiece = new Vector3(-0.0077f, 0.2018f, -0.0205f);     // centre de l'oculaire (arrière du tube)
    public Vector3 objective = new Vector3(-0.0077f, 0.2002f, 0.3900f);     // devant l'objectif : position de la caméra
    public float lensRadius = 0.021f;

    [Header("Version PC")]
    public float pcSensitivity = 0.3f;                          // sensibilité de la souris pendant le zoom

    /// <summary>Visée en cours au casque (0..1, fondu).</summary>
    public float Blend => blend;
    public bool Scoping => scoping;

    Camera scopeCam;
    RenderTexture rt;
    Material lensMat;
    Renderer lens;
    Renderer[] weaponRenderers;
    bool scoping, pcHidden;
    float blend;
    Pistol pistol;

    const string LensName = "Lunette_Lentille", CameraName = "Lunette_Camera";
    static readonly int ActiveId = Shader.PropertyToID("_Active");
    static readonly int EyeId = Shader.PropertyToID("_Eye");
    static readonly int TexId = Shader.PropertyToID("_MainTex");

    /// <summary>Pièce de la lunette créée à l'exécution (lentille, caméra) : à ne pas copier pour l'avatar distant.</summary>
    public static bool IsScopePart(Transform t) => t && (t.name == LensName || t.name == CameraName);

    void Awake()
    {
        pistol = GetComponent<Pistol>();
        weaponRenderers = GetComponentsInChildren<Renderer>(true);
        BuildLens();
    }

    void BuildLens()
    {
        var shader = Resources.Load<Shader>("Weapons/ScopeLens");
        if (!shader) { Debug.LogWarning("[Lunette] shader Resources/Weapons/ScopeLens introuvable"); return; }
        lensMat = new Material(shader) { name = "Lunette_Lentille (instance)" };

        var go = new GameObject(LensName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = eyepiece;
        go.transform.localRotation = Quaternion.identity;          // face au tireur (regard vers +Z)
        go.transform.localScale = Vector3.one * (lensRadius * 2f);
        go.AddComponent<MeshFilter>().sharedMesh = QuadMesh();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = lensMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        lens = mr;
    }

    static Mesh quad;
    static Mesh QuadMesh()
    {
        if (quad) return quad;
        quad = new Mesh { name = "Lunette_Quad" };
        quad.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f) };
        quad.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
        quad.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        quad.RecalculateNormals();
        quad.RecalculateBounds();
        return quad;
    }

    void EnsureCamera()
    {
        if (scopeCam) return;
        rt = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32) { name = "Lunette_Image", antiAliasing = 1, useMipMap = false, filterMode = FilterMode.Bilinear };
        rt.Create();
        var go = new GameObject(CameraName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = objective;
        // même direction que le tir (Pistol.AimRay au casque : muzzle.forward) : ce qui est sous le réticule est touché
        go.transform.localRotation = pistol && pistol.muzzle ? Quaternion.Inverse(transform.rotation) * pistol.muzzle.rotation : Quaternion.identity;
        scopeCam = go.AddComponent<Camera>();
        scopeCam.enabled = false;
        scopeCam.targetTexture = rt;
        scopeCam.stereoTargetEye = StereoTargetEyeMask.None;
        scopeCam.nearClipPlane = 0.05f;
        scopeCam.farClipPlane = farClip;
        scopeCam.allowHDR = false; scopeCam.allowMSAA = false;
        scopeCam.depth = -10f;
        var main = Camera.main;
        if (main)
        {
            scopeCam.clearFlags = main.clearFlags;
            scopeCam.backgroundColor = main.backgroundColor;
            scopeCam.cullingMask = main.cullingMask & ~(1 << 5);          // sans l'UI
        }
        var data = scopeCam.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = false;
        data.renderShadows = shadows;
        data.antialiasing = AntialiasingMode.None;
        data.requiresColorTexture = false; data.requiresDepthTexture = false;
        UpdateFov();
        lensMat.SetTexture(TexId, rt);
    }

    // l'image remplit la lentille vue à `eyeRelief` : champ = angle de la lentille vue de l'œil ÷ grossissement
    void UpdateFov()
    {
        if (!scopeCam) return;
        float lensAngle = 2f * Mathf.Atan(lensRadius / Mathf.Max(eyeRelief, 0.02f)) * Mathf.Rad2Deg;
        scopeCam.fieldOfView = lensAngle / Mathf.Max(magnification, 1f);
    }

    void LateUpdate()
    {
        if (!lensMat) return;
        bool want = false;
        Vector3 eyeOff = new Vector3(0f, 0f, 1.05f);
        var main = Camera.main;
        if (!DesktopMode.On && main && !ForcedOff())
            want = EyeAtScope(main.transform.position, main.transform.rotation, ref eyeOff);
        if (forced) { want = true; eyeOff = forcedEye; }

        scoping = want;                    // (hystérésis dans EyeAtScope : la lunette reste active un peu plus loin qu'elle ne s'allume)
        blend = Mathf.MoveTowards(blend, scoping ? 1f : 0f, Time.deltaTime * 8f);
        if (blend > 0f) { EnsureCamera(); UpdateFov(); }
        if (scopeCam) scopeCam.enabled = blend > 0f;          // caméra éteinte hors visée : aucun rendu en plus
        lensMat.SetFloat(ActiveId, blend);
        lensMat.SetVector(EyeId, eyeOff);
    }

    bool ForcedOff() => pistol && pistol.Reloading;

    // œil le plus proche de l'axe de la lunette ; vrai s'il est assez près de l'oculaire, derrière lui et dans l'axe
    public bool EyeAtScope(Vector3 headPos, Quaternion headRot, ref Vector3 eyeOff)
    {
        var head = new Pose(headPos, headRot);
        var ep = transform.TransformPoint(eyepiece);
        var axis = (transform.TransformPoint(objective) - ep).normalized;
        if (Vector3.Angle(head.forward, axis) > activationAngle * (scoping ? 1.25f : 1f)) return false;
        Vector3 best = head.position; float bestLat = float.MaxValue;
        for (int s = -1; s <= 1; s += 2)
        {
            var eye = head.position + head.right * (halfIpd * s);
            float lat = Vector3.ProjectOnPlane(eye - ep, axis).magnitude;
            if (lat < bestLat) { bestLat = lat; best = eye; }
        }
        var d = best - ep;
        float along = -Vector3.Dot(d, axis);                     // > 0 : l'œil est derrière l'oculaire
        float limit = activationDistance + (scoping ? 0.03f : 0f);
        if (along < -0.01f || d.magnitude > limit) return false;
        // ombre de la pupille : décalage latéral (repère de la lentille) et distance hors de l'idéal
        var lateral = Vector3.ProjectOnPlane(d, axis);
        var local = Quaternion.Inverse(transform.rotation) * lateral;
        float scale = 1f / Mathf.Max(eyeTolerance * 2f, 1e-3f);           // œil décalé de 1,2 cm : l'ombre mange la moitié du bord
        var off = -new Vector2(local.x, local.y) * scale;        // comme dans un tube : l'œil à droite, l'ombre mange la droite
        float far = Mathf.Clamp01((Mathf.Abs(along - eyeRelief) - 0.015f) / Mathf.Max(limit - eyeRelief, 0.01f));
        eyeOff = new Vector3(off.x, off.y, Mathf.Lerp(1.05f, 0.45f, far) + 0.18f * (1f - far));
        return true;
    }

    /// <summary>Version PC : cache l'arme (et sa lentille) pendant le zoom plein écran.</summary>
    public void SetPcHidden(bool hidden)
    {
        if (hidden == pcHidden) return;
        pcHidden = hidden;
        foreach (var r in weaponRenderers) if (r) r.enabled = !hidden;
        if (lens) lens.enabled = !hidden;
    }

    // ----- tests (captures de l'éditeur) -----
    bool forced; Vector3 forcedEye;
    /// <summary>Force la visée au casque (captures, tests sans casque) ; eye : décalage de l'œil (xy) et rayon de pupille (z).</summary>
    public void ForceScoped(bool on, Vector3 eye)
    {
        forced = on; forcedEye = eye;
        if (on) { blend = 1f; EnsureCamera(); UpdateFov(); lensMat.SetFloat(ActiveId, 1f); lensMat.SetVector(EyeId, eye); scopeCam.Render(); }
        else { blend = 0f; lensMat.SetFloat(ActiveId, 0f); }
    }

    /// <summary>Lentille et caméra créées hors mode Play (captures de l'éditeur).</summary>
    public void EditorSetup() { pistol = GetComponent<Pistol>(); weaponRenderers = GetComponentsInChildren<Renderer>(true); if (!lensMat) BuildLens(); }

    static void Kill(Object o) { if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }

    void OnDestroy()
    {
        if (rt) { rt.Release(); Kill(rt); }
        if (lensMat) Kill(lensMat);
    }
}
