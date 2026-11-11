using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// [SAE501] Carte supplémentaire pour le duel 1v1, construite par code (comme MainScene et Alvéole 17) :
//   - QUAI 9 (Arena1v1_Quai) : cour de quarantaine en plein air, de nuit, 42 × 28 m. Conteneurs (couverts hauts), sacs de sable
//     (couverts bas), longues lignes de vue le long des murs pour le sniper, cachées par la cabane et le groupe électrogène.
// Équité : chaque carte est construite pour la moitié du joueur 1 (x < 0), puis copiée en la tournant de 180° autour du centre
// pour le joueur 2 (symétrie centrale : chacun a exactement les mêmes couverts, à l'envers).
// Règles de hauteur (saut de 0,75 m : on escalade jusqu'à ~1,05 m, voir Arena1v1Fixes) :
//   couverts bas 1,2 m (on tire par-dessus debout, on ne monte pas dessus), couverts hauts ≥ 2,2 m, objets bas (barils ≤ 1 m)
//   jamais à moins de 2 m d'un couvert qu'ils permettraient d'escalader.
// Quest : tout le décor statique est fusionné par matériau (≈ 12 appels de dessin), aucune ombre temps réel, ≤ 8 lumières par carte.
// Menu : SAE501 → Arènes 1v1 → Construire Quai 9 (extérieur). (La carte hôpital « Bloc C » a été retirée à la demande de Rahim, journal 184.)
public static class ArenaMapsBuilder
{
    const string Tex = "Assets/Room/Textures/";
    const string ArenaTex = "Assets/Arena1v1/Textures/";

    [MenuItem("SAE501/Arènes 1v1/Construire Quai 9 (extérieur)")]
    public static void BuildQuai() => Build(ArenaMaps.Get(ArenaMaps.IndexOf("Arena1v1_Quai")), Quai, new Color(0.2f, 0.24f, 0.33f), new Color(0.05f, 0.07f, 0.11f), 0.016f);

    public static void BuildAll() => BuildQuai();

    static void Build(ArenaMaps.Map map, System.Action<Transform, Transform> content, Color ambient, Color fog, float fogDensity)
    {
        var scene = ArenaKit.NewScene(ambient, fog, fogDensity);
        var root = new GameObject("Arena1v1_Map").transform;
        var half = new GameObject("Moitie_J1").transform;          // moitié du joueur 1, copiée tournée de 180° pour le joueur 2
        half.SetParent(root, false);
        var center = new GameObject("Centre").transform;           // pièces déjà symétriques (sur le centre) et enveloppe
        center.SetParent(root, false);
        content(half, center);

        var copy = Object.Instantiate(half.gameObject, root).transform;
        copy.name = "Moitie_J2";
        copy.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, 180f, 0f));

        int mats = ArenaKit.MergeByMaterial(root, map.scene);
        ArenaKit.FinishScene(scene, root, map);
        Debug.Log($"[Arènes 1v1] {map.name} : {mats} matériaux fusionnés");
    }

    // ================================================================== QUAI 9 : cour de quarantaine
    const float WallH = 4.5f;

    static void Quai(Transform half, Transform center)
    {
        var sol = ArenaKit.Mat("Quai_Sol", ArenaTex + "dirty_concrete_diff_1k.jpg", ArenaTex + "dirty_concrete_nor_gl_1k.jpg", new Color(0.42f, 0.44f, 0.48f), 0.25f, 0f, 3f);
        var beton = ArenaKit.Mat("Quai_Beton", ArenaTex + "cracked_concrete_wall_diff_1k.jpg", ArenaTex + "cracked_concrete_wall_nor_gl_1k.jpg", new Color(0.55f, 0.57f, 0.6f), 0.15f, 0f, 3f);
        var bleu = ArenaKit.Gen("Quai_Conteneur_Bleu", "Conteneur", new Color(0.3f, 0.45f, 0.68f), 0.35f, 0.2f, 2f);
        var rouge = ArenaKit.Gen("Quai_Conteneur_Rouge", "Conteneur", new Color(0.72f, 0.22f, 0.15f), 0.35f, 0.2f, 2f);
        var vert = ArenaKit.Gen("Quai_Conteneur_Vert", "Conteneur", new Color(0.28f, 0.48f, 0.3f), 0.35f, 0.2f, 2f);
        // bouts des conteneurs : 2 battants et 4 barres de fermeture (une texture par face de 2,6 m), même couleur que la caisse
        doors.Clear();
        doors[bleu] = ArenaKit.Gen("Quai_Conteneur_Bleu_Portes", "Conteneur_Porte", new Color(0.3f, 0.45f, 0.68f), 0.35f, 0.2f, 2.6f);
        doors[rouge] = ArenaKit.Gen("Quai_Conteneur_Rouge_Portes", "Conteneur_Porte", new Color(0.72f, 0.22f, 0.15f), 0.35f, 0.2f, 2.6f);
        doors[vert] = ArenaKit.Gen("Quai_Conteneur_Vert_Portes", "Conteneur_Porte", new Color(0.28f, 0.48f, 0.3f), 0.35f, 0.2f, 2.6f);
        var acier = ArenaKit.Mat("Quai_Acier", ArenaTex + "rust_coarse_01_diff_1k.jpg", ArenaTex + "rust_coarse_01_nor_gl_1k.jpg", new Color(0.36f, 0.37f, 0.4f), 0.35f, 0.2f, 1.5f);
        var sacs = ArenaKit.Gen("Quai_Sacs_Sable", "Sacs", new Color(0.78f, 0.72f, 0.6f), 0.1f, 0f, 1.2f);
        var toile = ArenaKit.Mat("Quai_Toile", Tex + "dirty_carpet_diff_1k.jpg", Tex + "dirty_carpet_nor_gl_1k.jpg", new Color(0.58f, 0.62f, 0.44f), 0.1f, 0f, 1.5f);
        var bois = ArenaKit.Mat("Quai_Bois", Tex + "raw_plank_wall_diff_1k.jpg", Tex + "raw_plank_wall_nor_gl_1k.jpg", new Color(0.8f, 0.72f, 0.62f), 0.15f, 0f, 2f);
        var baril = ArenaKit.Gen("Quai_Baril", "Baril", new Color(0.62f, 0.16f, 0.1f), 0.4f, 0.2f, 1f);
        var jaune = ArenaKit.Mat("Quai_Peinture_Jaune", ArenaTex + "dirty_concrete_diff_1k.jpg", null, new Color(0.95f, 0.75f, 0.2f), 0.2f, 0f, 1f);
        var danger = ArenaKit.Gen("Arene_Bandes_Danger", "Danger", Color.white, 0.3f, 0f, 1f);
        var projecteur = ArenaKit.Emissive("Quai_Projecteur", new Color(1f, 0.85f, 0.6f), new Color(3.5f, 2.6f, 1.4f));
        var balise = ArenaKit.Emissive("Quai_Balise", new Color(1f, 0.3f, 0.1f), new Color(3.0f, 0.6f, 0.1f));

        // ---------- enveloppe et ciel (centre) ----------
        ArenaKit.Slab(center, "Sol", -21.5f, -14.5f, 21.5f, 14.5f, -0.2f, 0.2f, sol);
        ArenaKit.Slab(center, "Mur_Nord", -21.5f, 14f, 21.5f, 14.5f, 0f, WallH, beton);
        ArenaKit.Slab(center, "Mur_Sud", -21.5f, -14.5f, 21.5f, -14f, 0f, WallH, beton);
        ArenaKit.Slab(center, "Mur_Ouest", -21.5f, -14f, -21f, 14f, 0f, WallH, beton);
        ArenaKit.Slab(center, "Mur_Est", 21f, -14f, 21.5f, 14f, 0f, WallH, beton);
        Sky(center);
        var moon = new GameObject("Lune").AddComponent<Light>();
        moon.transform.SetParent(center, false);
        moon.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
        moon.type = LightType.Directional;
        moon.color = new Color(0.5f, 0.62f, 0.95f);
        moon.intensity = 0.95f;
        moon.shadows = LightShadows.None;

        // centre : pile de 2 conteneurs en croix (celui du haut fait auvent, sur 4 poteaux), coupe la vue d'un spawn à l'autre
        Container(center, rouge, acier, new Vector3(0f, 0f, 0f), 90f);
        Container(center, bleu, acier, new Vector3(0f, 2.6f, 0f), 0f);
        foreach (var o in new[] { new Vector3(2.75f, 0f, 1.0f), new Vector3(2.75f, 0f, -1.0f), new Vector3(-2.75f, 0f, 1.0f), new Vector3(-2.75f, 0f, -1.0f) })
            ArenaKit.Cylinder(center, "Poteau_Auvent", o, 0.1f, 2.6f, acier, true, 8);

        // ---------- moitié J1 (x < 0), apparition en (-18, 0) ----------
        // portail fermé derrière le spawn, conteneur-écran devant
        ArenaKit.Slab(half, "Portail", -21f, -2.2f, -20.85f, 2.2f, 0f, 3.6f, acier);
        ArenaKit.Slab(half, "Portail_Bande", -20.86f, -2.2f, -20.83f, 2.2f, 3.1f, 0.35f, danger, false);
        ArenaKit.Box(half, "Balise_Spawn", new Vector3(-20.75f, 3.85f, 0f), new Vector3(0.15f, 0.15f, 0.15f), balise, 0f, false);
        Container(half, vert, acier, new Vector3(-14.5f, 0f, 0f), 90f);
        // conteneurs : nord (le long de x), sud empilé (repère visuel), diagonal près du centre
        Container(half, rouge, acier, new Vector3(-9.5f, 0f, 7.5f), 0f);
        Container(half, vert, acier, new Vector3(-11f, 0f, -8.5f), 0f);
        Container(half, bleu, acier, new Vector3(-11f, 2.6f, -8.5f), 4f);
        Container(half, bleu, acier, new Vector3(-5.5f, 0f, -2.6f), 30f);
        // sacs de sable 1,2 m (on tire par-dessus, on ne monte pas dessus)
        Sandbags(half, sacs, new Vector3(-10.5f, 0f, 2.8f), 2.6f, 0f);
        Sandbags(half, sacs, new Vector3(-6.5f, 0f, 3.9f), 2.4f, 90f);
        Sandbags(half, sacs, new Vector3(-8f, 0f, -4.9f), 2.4f, 0f);
        Sandbags(half, sacs, new Vector3(-15.5f, 0f, 9.5f), 2.4f, 90f);
        // barils (1 m) uniquement contre les conteneurs : on monte dessus, mais rien d'escaladable à côté
        ArenaKit.Cylinder(half, "Baril", new Vector3(-12.9f, 0f, 5.55f), 0.3f, 1.0f, baril);
        ArenaKit.Cylinder(half, "Baril", new Vector3(-12.25f, 0f, 5.75f), 0.3f, 1.0f, baril);
        ArenaKit.LyingCylinder(half, "Baril_Couche", new Vector3(-7.4f, 0.3f, -10.3f), 0.3f, 0.95f, 70f, baril);
        // tente de décontamination (pleine) au nord, cabane du gardien et groupe électrogène le long des murs (cassent les longues vues)
        ArenaKit.Slab(half, "Tente", -6f, 8.2f, -2f, 11.2f, 0f, 2.2f, toile);
        ArenaKit.Box(half, "Tente_Toit", new Vector3(-4f, 2.2f, 9.7f), new Vector3(4.05f, 1.6f, 1.6f), toile, 0f, false).transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        ArenaKit.Slab(half, "Cabane", -9.7f, 11.6f, -7.3f, 14f, 0f, 2.6f, bois);
        ArenaKit.Slab(half, "Cabane_Toit", -9.9f, 11.4f, -7.1f, 14f, 2.6f, 0.12f, acier);
        ArenaKit.Slab(half, "Groupe_Electrogene", -6f, -13.4f, -3f, -11.4f, 0.15f, 2.05f, vert);      // posé sur son socle de 15 cm
        ArenaKit.Slab(half, "Groupe_Socle", -6.2f, -13.6f, -2.8f, -11.2f, 0f, 0.15f, beton);
        ArenaKit.Slab(half, "Groupe_Bande", -6.02f, -11.42f, -2.98f, -11.38f, 0.2f, 0.3f, danger, false);
        ArenaKit.Slab(half, "Ecran_Spawn_Bande", -13.27f, -3.0f, -13.25f, 3.0f, 0.2f, 0.3f, danger, false);
        // palettes au sol (décor, 15 cm), marquage jaune du quai
        ArenaKit.Slab(half, "Palette", -16.8f, -5.6f, -15.6f, -4.6f, 0f, 0.15f, bois);
        ArenaKit.Slab(half, "Palette", -3.4f, 5.4f, -2.2f, 6.4f, 0f, 0.15f, bois);
        ArenaKit.Slab(half, "Marquage", -20f, 4.4f, -1f, 4.55f, 0f, 0.01f, jaune, false);
        ArenaKit.Slab(half, "Marquage", -20f, -4.55f, -12f, -4.4f, 0f, 0.01f, jaune, false);
        // projecteurs aux coins (mât, tête, spot) : 2 par moitié + la balise du spawn = 6 lumières en tout
        Floodlight(half, acier, projecteur, new Vector3(-20.3f, 0f, 13.3f), new Vector3(-8f, 0f, 4f));
        Floodlight(half, acier, projecteur, new Vector3(-20.3f, 0f, -13.3f), new Vector3(-9f, 0f, -5f));
        ArenaKit.Point(half, "Lumiere_Balise", new Vector3(-20.3f, 3.6f, 0f), new Color(1f, 0.5f, 0.2f), 4f, 8f);
    }

    // conteneur maritime de 20 pieds (6,06 × 2,6 × 2,44) posé à pos (base), long selon x (yaw 0) ; cadre d'acier aux deux bouts
    static readonly System.Collections.Generic.Dictionary<Material, Material> doors = new System.Collections.Generic.Dictionary<Material, Material>();

    static void Container(Transform p, Material body, Material frame, Vector3 pos, float yaw)
    {
        var r = Quaternion.Euler(0f, yaw, 0f);
        ArenaKit.Box(p, "Conteneur", pos + new Vector3(0f, 1.3f, 0f), new Vector3(6.06f, 2.6f, 2.44f), body, yaw);
        var door = doors.TryGetValue(body, out var d) ? d : frame;
        foreach (float e in new[] { -3.0f, 3.0f })
            ArenaKit.Box(p, "Conteneur_Portes", pos + r * new Vector3(e, 1.3f, 0f), new Vector3(0.1f, 2.62f, 2.46f), door, yaw, false);
        // longerons haut et bas, montants d'angle (acier sombre) : la silhouette d'un vrai conteneur
        foreach (float y in new[] { 0.07f, 2.53f })
            foreach (float z in new[] { -1.2f, 1.2f })
                ArenaKit.Box(p, "Conteneur_Longeron", pos + r * new Vector3(0f, y, z), new Vector3(6.0f, 0.14f, 0.08f), frame, yaw, false);
        foreach (float x in new[] { -2.97f, 2.97f })
            foreach (float z in new[] { -1.2f, 1.2f })
                ArenaKit.Box(p, "Conteneur_Montant", pos + r * new Vector3(x, 1.3f, z), new Vector3(0.14f, 2.62f, 0.1f), frame, yaw, false);
    }

    // muret de sacs de sable : 1,2 m de haut, 0,6 m d'épaisseur, 3 rangées décalées
    static void Sandbags(Transform p, Material m, Vector3 c, float length, float yaw)
    {
        var r = Quaternion.Euler(0f, yaw, 0f);
        ArenaKit.Box(p, "Sacs_Sable", c + new Vector3(0f, 0.4f, 0f), new Vector3(length, 0.8f, 0.6f), m, yaw);
        ArenaKit.Box(p, "Sacs_Sable_Haut", c + r * new Vector3(0.1f, 1.0f, 0f), new Vector3(length - 0.25f, 0.4f, 0.52f), m, yaw);
    }

    static void Floodlight(Transform p, Material mast, Material lamp, Vector3 basePos, Vector3 target)
    {
        ArenaKit.Cylinder(p, "Mat_Projecteur", basePos, 0.12f, 7f, mast, true, 8);
        var head = basePos + Vector3.up * 7.1f;
        var dir = (target - head).normalized;
        var yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        ArenaKit.Box(p, "Projecteur", head, new Vector3(0.6f, 0.4f, 0.25f), lamp, yaw, false);
        ArenaKit.Spot(p, "Lumiere_Projecteur", head, target, new Color(1f, 0.92f, 0.78f), 40f, 80f, 34f);
    }

    // ciel de nuit du jeu (même panorama que MainScene) : sphère qui suit la tête, sans brouillard, dessinée derrière tout
    static void Sky(Transform parent)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Environment/Materials/M_NightForest_SkySphere_SansBrouillard.mat");
        if (!mat) { Debug.LogWarning("[Quai 9] matériau du ciel introuvable"); return; }
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Ciel";
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localScale = Vector3.one * 50f;
        var r = go.GetComponent<Renderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows = false;
        go.AddComponent<SkyFollow>();
    }
}
