using UnityEngine;

// Matériau des textes du décor (Resources/WorldText, shader SAE501/WorldText : caché par les murs, fondu par distance).
// La police intégrée est dynamique : sa texture peut être reconstruite (nouveaux caractères), on la recopie alors
// dans le matériau. Utilisé par RoomBuilder (panneaux des portes, générateur, radio).
public static class WorldTextMaterial
{
    public const string ResourceName = "WorldText";
    static Material mat;
    static Font font;

    public static Font Font => font ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        mat = Resources.Load<Material>(ResourceName);
        if (!mat || !Font) return;
        Sync(Font);
        Font.textureRebuilt -= Sync;
        Font.textureRebuilt += Sync;
    }

    static void Sync(Font f)
    {
        if (mat && f == Font && f.material) mat.mainTexture = f.material.mainTexture;
    }
}
