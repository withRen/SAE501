using UnityEngine;

// [7.2] Incrustation sur l'image d'une caméra de surveillance : « ● REC » qui clignote, nom de la caméra, date et heure.
// Les textes sont posés juste devant l'objectif de la caméra, sur un calque que la caméra du joueur ne voit pas :
// ils n'apparaissent donc que sur l'écran de cette caméra (puis passent dans l'effet CCTV du shader).
[RequireComponent(typeof(Camera))]
public class CctvOverlay : MonoBehaviour
{
    public const int Layer = 31;               // calque réservé aux incrustations (pas besoin de le nommer)
    public string label = "CAM 01";
    public string date = "07/10/2026";
    public int startHour = 23, startMinute = 47;

    TextMesh rec, clock;
    float nextBlink;
    int shownSecond = -1;

    void Start()
    {
        var cam = GetComponent<Camera>();
        cam.cullingMask |= 1 << Layer;
        if (Camera.main) Camera.main.cullingMask &= ~(1 << Layer);      // le joueur ne voit pas les textes flotter dans le décor

        // taille de l'image à 0,12 m devant l'objectif
        const float dist = 0.12f;
        float h = 2f * dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float w = h * (cam.targetTexture ? (float)cam.targetTexture.width / cam.targetTexture.height : cam.aspect);
        float m = h * 0.07f;                                                // marge
        float size = h * 0.012f;                                            // hauteur de ligne ~ 8 % de l'image (TextMesh : 6,4 x characterSize)

        rec = Make("REC", new Vector3(-w / 2 + m, h / 2 - m, dist), size, new Color(1f, 0.1f, 0.08f), TextAnchor.UpperLeft, "● REC");
        Make("Nom", new Vector3(w / 2 - m, h / 2 - m, dist), size, Color.white, TextAnchor.UpperRight, label);
        clock = Make("Heure", new Vector3(-w / 2 + m, -h / 2 + m, dist), size, Color.white, TextAnchor.LowerLeft, "");
    }

    TextMesh Make(string name, Vector3 local, float size, Color c, TextAnchor anchor, string text)
    {
        var g = new GameObject("CCTV_" + name) { layer = Layer };
        g.transform.SetParent(transform, false);
        g.transform.localPosition = local;
        var tm = g.AddComponent<TextMesh>();
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.fontSize = 64; tm.characterSize = size; tm.anchor = anchor; tm.fontStyle = FontStyle.Bold;
        tm.alignment = anchor == TextAnchor.UpperRight ? TextAlignment.Right : TextAlignment.Left;
        tm.color = c; tm.text = text;
        g.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        return tm;
    }

    void Update()
    {
        if (!clock) return;
        if (Time.time >= nextBlink) { nextBlink = Time.time + 0.6f; rec.gameObject.SetActive(!rec.gameObject.activeSelf); }
        int s = startHour * 3600 + startMinute * 60 + (int)Time.time;
        if (s == shownSecond) return;               // l'heure n'est reconstruite qu'une fois par seconde
        shownSecond = s;
        clock.text = date + "  " + (s / 3600 % 24).ToString("00") + ":" + (s / 60 % 60).ToString("00") + ":" + (s % 60).ToString("00");
    }

    /// <summary>« CCTV_Camera_01_Corridor_Left » -> « CAM 01  COULOIR GAUCHE ».</summary>
    public static string LabelFor(string cameraName)
    {
        var s = cameraName.Replace("CCTV_Camera_", "");
        string num = s.Length >= 2 ? s.Substring(0, 2) : "00";
        string place = s.Length > 3 ? s.Substring(3) : s;
        switch (place.ToLowerInvariant())
        {
            case "corridor_left": place = "COULOIR GAUCHE"; break;
            case "corridor_right": place = "COULOIR DROIT"; break;
            case "kitchen": place = "CUISINE"; break;
            case "storage": case "stockage": place = "STOCKAGE"; break;
            case "generator": place = "GÉNÉRATEUR"; break;
            case "exterieur_sud": place = "EXTÉRIEUR SUD"; break;
            default: place = place.Replace('_', ' ').ToUpperInvariant(); break;
        }
        return "CAM " + num + "  " + place;
    }
}
