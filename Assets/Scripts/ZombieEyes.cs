using UnityEngine;

// [#117] Yeux lumineux des zombies : deux petites lueurs rouges sur le visage, qui n'apparaissent qu'au-delà de 8 m du joueur
// (fondu entre 7 et 9 m). Un zombie au bout d'un couloir sombre reste repérable ; de près, on voit son vrai visage.
// Ajouté par Zombie.Awake s'il manque au prefab. Matériau : Resources/Fx_Lueur (additif, créé par RoomBuilder.EnsureFxMaterials).
// Les lueurs suivent l'os de la tête mais sont placées dans le repère du zombie (devant / haut), pour ne pas dépendre
// de l'orientation des os du modèle ; elles font toujours face au joueur.
public class ZombieEyes : MonoBehaviour
{
    public float showFrom = 7f;             // m : invisibles en dessous
    public float fullFrom = 9f;             // m : pleine intensité au-delà (8 m au milieu du fondu)
    public Color color = new Color(1f, 0.18f, 0.05f);
    public float size = 0.06f;
    public Vector3 offset = new Vector3(0.033f, 0.1f, 0.115f);   // écart d'un œil (x), hauteur (y) et avancée (z) par rapport à l'os de la tête

    Transform head;
    Transform[] eyes;
    Renderer[] rends;
    Zombie zombie;
    MaterialPropertyBlock mpb;
    float shown = -1f;
    static Material mat;
    static Mesh quad;
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        zombie = GetComponent<Zombie>();
        var animator = GetComponentInChildren<Animator>();
        if (animator && animator.isHuman) head = animator.GetBoneTransform(HumanBodyBones.Head);
        if (!head) foreach (var t in GetComponentsInChildren<Transform>()) if (t.name == "head" || t.name == "Tete") { head = t; break; }
        if (!mat) mat = Resources.Load<Material>("Fx_Lueur");
        if (!head || !mat) { enabled = false; return; }
        if (!quad) quad = MakeQuad();

        mpb = new MaterialPropertyBlock();
        eyes = new Transform[2];
        rends = new Renderer[2];
        for (int i = 0; i < 2; i++)
        {
            var go = new GameObject(i == 0 ? "Oeil_G" : "Oeil_D");
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * size;
            go.AddComponent<MeshFilter>().sharedMesh = quad;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            r.enabled = false;
            eyes[i] = go.transform; rends[i] = r;
        }
    }

    void LateUpdate()
    {
        var cam = Camera.main;
        if (!cam) return;
        float d = Vector3.Distance(cam.transform.position, head.position);
        float a = zombie == null || !zombie.Dead ? Mathf.InverseLerp(showFrom, fullFrom, d) : 0f;
        if (a <= 0f)
        {
            if (shown != 0f) { foreach (var r in rends) r.enabled = false; shown = 0f; }
            return;
        }
        var root = transform;
        var center = head.position + root.up * offset.y + root.forward * offset.z;
        var toCam = cam.transform.position - center;
        // de dos (le joueur voit sa nuque) : pas de lueur
        a *= Mathf.Clamp01(Vector3.Dot(root.forward, toCam.normalized) * 3f + 0.5f);
        var look = Quaternion.LookRotation(-toCam);
        var scale = Vector3.one * size * Mathf.Max(1f, d / fullFrom);       // taille apparente constante au loin (brouillard)
        for (int i = 0; i < 2; i++)
        {
            eyes[i].localScale = scale;
            eyes[i].SetPositionAndRotation(center + root.right * (i == 0 ? -offset.x : offset.x), look);
            rends[i].enabled = a > 0.01f;
        }
        if (!Mathf.Approximately(a, shown))
        {
            shown = a;
            mpb.SetColor(BaseColor, color * a);
            foreach (var r in rends) r.SetPropertyBlock(mpb);
        }
    }

    static Mesh MakeQuad()
    {
        var m = new Mesh { name = "Lueur" };
        m.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f), new Vector3(-0.5f, 0.5f), new Vector3(0.5f, 0.5f) };
        m.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        m.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        m.RecalculateBounds();
        return m;
    }
}
