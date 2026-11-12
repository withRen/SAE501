using System.Collections.Generic;
using UnityEngine;

// [Hotfix #160] Bandeau des scores du duel 1v1, inspiré de CS:GO, propre à CE casque (créé par NetDuel sur chaque casque).
//   En haut de la vue : [ nom + score + pastilles du joueur côté 1 ] [ MANCHE n · chrono · PREMIER À k ] [ côté 2 ]
//   - ton côté en bleu, l'adversaire en rouge, fond sombre semi-transparent ;
//   - pastilles : une par manche à gagner, pleines pour les manches gagnées ;
//   - dessous, selon l'état du duel (NetDuel.Phase) : décompte 3-2-1 puis « COMBATTEZ ! » (gel), « MANCHE GAGNÉE / PERDUE »
//     (fin de manche), « VICTOIRE / DÉFAITE » (fin de match).
// Au casque : accroché à 1,3 m devant les yeux, 10° au-dessus du centre, il suit la tête avec un léger retard (comme un HUD
// de casque, sans être collé à l'écran). Sur PC : fixe dans la vue. Dessiné par-dessus le décor (pas caché par un mur).
[DefaultExecutionOrder(1001)]       // après DesktopPlayer et NetKillFx : suit la caméra dans sa pose finale
public class NetDuelHud : MonoBehaviour
{
    static readonly Color Mine = new Color(0.32f, 0.68f, 1f), Theirs = new Color(1f, 0.32f, 0.28f);
    static readonly Color Panel = new Color(0.04f, 0.05f, 0.07f, 0.78f), Dim = new Color(1f, 1f, 1f, 0.55f);
    const float Dist = 1.3f;
    // taille et hauteur : plus grand au casque (champ de vision plus large, écran moins net) que sur PC
    static float Scale => DesktopMode.On ? 1.35f : 1.6f;
    static float Up => DesktopMode.On ? 0.25f : 0.30f;      // ≈ 11-13° au-dessus du centre de la vue
    int lastSec;

    NetDuel duel;
    Transform cam;
    TextMesh name1, name2, score1, score2, roundText, clock, target, banner;
    Renderer bar1, bar2, bannerBg;
    readonly List<Renderer> pips1 = new List<Renderer>(), pips2 = new List<Renderer>();
    int lastPips = -1;
    Material quadMat, textMat;

    public static NetDuelHud Create(NetDuel duel)
    {
        var go = new GameObject("HUD_Duel");
        var h = go.AddComponent<NetDuelHud>();
        h.duel = duel;
        return h;
    }

    void Start()
    {
        quadMat = new Material(Shader.Find("UI/Default"));
        quadMat.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
        quadMat.renderQueue = 4100;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textMat = new Material(font.material) { renderQueue = 4110 };
        textMat.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);

        // fond : trois blocs comme le bandeau de CS:GO (équipe, centre, équipe)
        bar1 = Quad("Bloc_1", new Vector2(-0.2f, 0f), new Vector2(0.235f, 0.105f), Panel);
        Quad("Bloc_Centre", new Vector2(0f, 0f), new Vector2(0.155f, 0.105f), Panel);
        bar2 = Quad("Bloc_2", new Vector2(0.2f, 0f), new Vector2(0.235f, 0.105f), Panel);
        Quad("Liseré_1", new Vector2(-0.2f, -0.0505f), new Vector2(0.235f, 0.004f), Color.white).name = "Lisere_1";
        Quad("Liseré_2", new Vector2(0.2f, -0.0505f), new Vector2(0.235f, 0.004f), Color.white).name = "Lisere_2";

        name1 = Text(new Vector2(-0.2f, 0.033f), 0.0021f, TextAnchor.MiddleCenter);
        name2 = Text(new Vector2(0.2f, 0.033f), 0.0021f, TextAnchor.MiddleCenter);
        score1 = Text(new Vector2(-0.2f, 0.0f), 0.0052f, TextAnchor.MiddleCenter);
        score2 = Text(new Vector2(0.2f, 0.0f), 0.0052f, TextAnchor.MiddleCenter);
        roundText = Text(new Vector2(0f, 0.032f), 0.0023f, TextAnchor.MiddleCenter);
        clock = Text(new Vector2(0f, 0.004f), 0.0038f, TextAnchor.MiddleCenter);
        target = Text(new Vector2(0f, -0.032f), 0.0021f, TextAnchor.MiddleCenter);

        bannerBg = Quad("Bandeau", new Vector2(0f, -0.1f), new Vector2(0.4f, 0.058f), Panel);
        banner = Text(new Vector2(0f, -0.1f), 0.0034f, TextAnchor.MiddleCenter);
        foreach (var t in new[] { roundText, target }) t.color = Dim;
        clock.color = Color.white;
    }

    Renderer Quad(string n, Vector2 pos, Vector2 size, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Quad);
        g.name = n;
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(transform, false);
        g.transform.localPosition = new Vector3(pos.x, pos.y, 0.002f);
        g.transform.localScale = new Vector3(size.x, size.y, 1f);
        var r = g.GetComponent<Renderer>();
        r.sharedMaterial = quadMat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
        var b = new MaterialPropertyBlock(); b.SetColor("_Color", c); r.SetPropertyBlock(b);
        return r;
    }

    static void Tint(Renderer r, Color c) { var b = new MaterialPropertyBlock(); b.SetColor("_Color", c); r.SetPropertyBlock(b); }

    TextMesh Text(Vector2 pos, float size, TextAnchor anchor)
    {
        var go = new GameObject("Texte");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        var t = go.AddComponent<TextMesh>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        go.GetComponent<MeshRenderer>().sharedMaterial = textMat;
        t.fontSize = 96; t.characterSize = size; t.anchor = anchor; t.alignment = TextAlignment.Center;
        t.color = Color.white; t.text = "";
        return t;
    }

    void BuildPips(int n)
    {
        foreach (var r in pips1) if (r) Destroy(r.gameObject);
        foreach (var r in pips2) if (r) Destroy(r.gameObject);
        pips1.Clear(); pips2.Clear();
        const float step = 0.019f;
        for (int i = 0; i < n; i++)
        {
            float x = (i - (n - 1) / 2f) * step;
            pips1.Add(Quad("Pastille_1", new Vector2(-0.2f + x, -0.034f), new Vector2(0.012f, 0.008f), Color.white));
            pips2.Add(Quad("Pastille_2", new Vector2(0.2f + x, -0.034f), new Vector2(0.012f, 0.008f), Color.white));
        }
        lastPips = n;
    }

    void LateUpdate()
    {
        if (!duel) { Destroy(gameObject); return; }
        var c = Camera.main;
        if (!c) return;
        bool snap = cam != c.transform;
        cam = c.transform;
        transform.localScale = Vector3.one * Scale;

        // suit la tête : au casque avec un léger retard (lissage), sur PC fixe dans la vue
        var wantRot = cam.rotation;
        var wantPos = cam.position + wantRot * new Vector3(0f, Up, Dist);
        float k = DesktopMode.On || snap ? 1f : 1f - Mathf.Exp(-Time.unscaledDeltaTime * 9f);
        transform.rotation = Quaternion.Slerp(transform.rotation, wantRot, k);
        transform.position = Vector3.Lerp(transform.position, cam.position + transform.rotation * new Vector3(0f, Up, Dist), 1f);
        if (k >= 1f) transform.SetPositionAndRotation(wantPos, wantRot);

        Refresh();
    }

    void Refresh()
    {
        int mySide = NetLobby.Side(NetSession.LocalId);
        var c1 = mySide == 1 ? Mine : Theirs; var c2 = mySide == 2 ? Mine : Theirs;
        int s1 = duel.ScoreP1.Value, s2 = duel.ScoreP2.Value, need = NetDuel.TargetWins;
        if (lastPips != need) BuildPips(need);

        name1.text = Short(NetDuel.SideName(1)).ToUpperInvariant() + (mySide == 1 ? "  (TOI)" : "");
        name2.text = Short(NetDuel.SideName(2)).ToUpperInvariant() + (mySide == 2 ? "  (TOI)" : "");
        name1.color = c1; name2.color = c2;
        score1.text = s1.ToString(); score2.text = s2.ToString();
        score1.color = c1; score2.color = c2;
        Tint(bar1, new Color(c1.r * 0.18f, c1.g * 0.18f, c1.b * 0.18f, 0.8f));
        Tint(bar2, new Color(c2.r * 0.18f, c2.g * 0.18f, c2.b * 0.18f, 0.8f));
        var l1 = transform.Find("Lisere_1"); var l2 = transform.Find("Lisere_2");
        if (l1) Tint(l1.GetComponent<Renderer>(), c1);
        if (l2) Tint(l2.GetComponent<Renderer>(), c2);
        for (int i = 0; i < pips1.Count; i++) Tint(pips1[i], i < s1 ? c1 : new Color(1f, 1f, 1f, 0.18f));
        for (int i = 0; i < pips2.Count; i++) Tint(pips2[i], i < s2 ? c2 : new Color(1f, 1f, 1f, 0.18f));

        roundText.text = "MANCHE " + duel.Round.Value;
        target.text = "PREMIER À " + need;

        double now = duel.Now;
        var phase = duel.CurrentPhase;
        double t = phase == NetDuel.Phase.MancheActive ? now - duel.RoundStart.Value : 0;
        // chrono de la manche : tourne en manche active, reste figé en fin de manche, remis à 0 au gel suivant
        int sec = phase == NetDuel.Phase.MancheActive ? Mathf.Max(0, (int)t) : phase == NetDuel.Phase.Gel ? 0 : lastSec;
        lastSec = sec;
        clock.text = (sec / 60) + ":" + (sec % 60).ToString("00");

        // bandeau d'état
        string msg = ""; Color col = Color.white;
        switch (phase)
        {
            case NetDuel.Phase.Gel:
                int left = Mathf.CeilToInt((float)(duel.PhaseUntil.Value - now));
                msg = left > NetDuel.FreezeTime ? "PRÉPAREZ-VOUS" : left > 0 ? left.ToString() : "COMBATTEZ !";
                col = new Color(1f, 0.8f, 0.3f);
                break;
            case NetDuel.Phase.MancheActive:
                if (t < 1.2) { msg = "COMBATTEZ !"; col = new Color(1f, 0.8f, 0.3f); }
                break;
            case NetDuel.Phase.FinDeManche:
                bool won = duel.RoundWinner.Value == mySide;
                msg = won ? "MANCHE GAGNÉE" : "MANCHE PERDUE"; col = won ? Mine : Theirs;
                break;
            case NetDuel.Phase.FinDeMatch:
                bool victory = duel.Winner.Value == mySide;
                msg = victory ? "VICTOIRE" : "DÉFAITE"; col = victory ? Mine : Theirs;
                break;
        }
        banner.text = msg; banner.color = col;
        bannerBg.enabled = msg.Length > 0;
    }

    static string Short(string n) => n.Length > 14 ? n.Substring(0, 13) + "." : n;
}
