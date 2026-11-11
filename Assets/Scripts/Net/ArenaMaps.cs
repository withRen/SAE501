using UnityEngine;
using UnityEngine.SceneManagement;

// Les cartes du duel 1v1 (choix de l'hôte dans le lobby, NetLobby.DuelMap) : scène, nom, description, apparitions des 2 joueurs
// et aperçu (Resources/ArenaPreviews/<scène>, capture faite par le menu SAE501 → Arènes 1v1 → Captures).
// Toutes les règles propres au duel (saut de 0,75 m, armes du kit, gestionnaire NetDuel) valent pour chaque carte de cette liste.
// Les scènes sont construites par code : Arena1v1Builder (Alvéole 17), ArenaMapsBuilder (Quai 9).
public static class ArenaMaps
{
    public class Map
    {
        public string scene, name, kind, desc;
        public Vector3 spawn1, spawn2;          // J1 et J2 (pieds du joueur)
        public float yaw1, yaw2;                // regard au départ (vers le centre)
        public Vector3 banner;                  // tableau des scores du duel (NetDuel)
    }

    public static readonly Map[] All =
    {
        new Map
        {
            scene = "Arena1v1_Bunker", name = "ALVÉOLE 17", kind = "BUNKER",
            desc = "Hall de béton, brèche centrale\net passerelles en hauteur",
            spawn1 = new Vector3(-13f, 0.1f, 0f), yaw1 = 90f,
            spawn2 = new Vector3(13f, 0.1f, 0f), yaw2 = -90f,
            banner = new Vector3(0f, 3.2f, 0f),
        },
        new Map
        {
            scene = "Arena1v1_Quai", name = "QUAI 9", kind = "EXTÉRIEUR",
            desc = "Cour de quarantaine de nuit :\nconteneurs, sacs de sable, longues vues",
            spawn1 = new Vector3(-18f, 0.05f, 0f), yaw1 = 90f,
            spawn2 = new Vector3(18f, 0.05f, 0f), yaw2 = -90f,
            banner = new Vector3(0f, 6.2f, 0f),
        },
    };

    public static int Count => All.Length;

    public static Map Get(int index) => All[Mathf.Clamp(index, 0, All.Length - 1)];

    public static int IndexOf(string scene)
    {
        for (int i = 0; i < All.Length; i++) if (All[i].scene == scene) return i;
        return -1;
    }

    public static bool IsArena(string scene) => IndexOf(scene) >= 0;

    /// <summary>La scène active est-elle une arène de duel ?</summary>
    public static bool InArena => IsArena(SceneManager.GetActiveScene().name);

    /// <summary>Carte de la scène active (Alvéole 17 si la scène n'est pas une arène).</summary>
    public static Map Current { get { int i = IndexOf(SceneManager.GetActiveScene().name); return All[Mathf.Max(0, i)]; } }

    /// <summary>Aperçu de la carte pour le lobby (null si la capture n'a pas encore été faite).</summary>
    public static Texture2D Preview(int index) => Resources.Load<Texture2D>("ArenaPreviews/" + Get(index).scene);

    /// <summary>Place le joueur (rig) à son apparition : J1 d'un côté, J2 de l'autre, regard vers le centre.</summary>
    public static void PlaceRig(Transform rig, int playerNumber)
    {
        var m = Current;
        var cc = rig.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        bool first = playerNumber == 1;
        rig.position = first ? m.spawn1 : m.spawn2;
        rig.rotation = Quaternion.Euler(0f, first ? m.yaw1 : m.yaw2, 0f);
        if (cc) cc.enabled = true;
    }
}
