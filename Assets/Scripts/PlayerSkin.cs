using UnityEngine;

// Personnage (« skin ») choisi par le joueur : 4 tenues du pack Survivalist (Slayver), prefabs Resources/Skins/Survivalist_1..4
// construits par SurvivalistBuilder (menu SAE501 → Build Survivalist Skins). Le choix est gardé d'une partie à l'autre (PlayerPrefs).
// En multijoueur, chaque joueur envoie son choix (NetLobby pour le lobby, NetPlayer pour la partie) : les autres le voient avec cette tenue.
public static class PlayerSkin
{
    public const int Count = 4;
    // 1 : pull gris et jean · 2 : kaki, casquette et sac à dos · 3 : gilet vert, pantalon beige · 4 : gilet tactique noir, casquette
    static readonly string[] Names = { "CIVIL", "ÉCLAIREUR", "SOLDAT", "TACTIQUE" };
    public static string Name(int index) => Names[Mathf.Clamp(index, 0, Count - 1)];

    public static int Selected
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt("sae501.skin", 0), 0, Count - 1);
        set { PlayerPrefs.SetInt("sae501.skin", Mathf.Clamp(value, 0, Count - 1)); PlayerPrefs.Save(); }
    }

    public static GameObject Prefab(int index) => Resources.Load<GameObject>("Skins/Survivalist_" + (Mathf.Clamp(index, 0, Count - 1) + 1));

    /// <summary>Crée le personnage (null si les prefabs n'ont pas été construits).</summary>
    public static PlayerAvatar Create(int index, Transform parent)
    {
        var prefab = Prefab(index);
        if (!prefab) return null;
        var go = Object.Instantiate(prefab, parent);
        go.name = "Personnage";
        var a = go.GetComponent<PlayerAvatar>();
        return a ? a : go.AddComponent<PlayerAvatar>();
    }
}
