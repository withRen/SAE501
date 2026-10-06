using UnityEngine;

// Crée un quad collé à la caméra (nécessaire en VR : un Canvas "Screen Space Overlay" n'est pas visible dans le casque).
public static class HeadOverlay
{
    public static Renderer Create(Transform cam, string name, Texture2D tex, Color color, float distance, int queue)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name;
        Object.Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(cam, false);
        go.transform.localPosition = new Vector3(0f, 0f, distance);
        go.transform.localRotation = Quaternion.identity;

        var shader = Shader.Find("Sprites/Default");
        var mat = new Material(shader);
        if (tex) mat.mainTexture = tex;
        mat.color = color;
        mat.renderQueue = queue;

        var r = go.GetComponent<Renderer>();
        r.material = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return r;
    }

    // Dégradé radial : centre transparent, bords opaques.
    public static Texture2D RadialTexture(int size = 256, float inner = 0.45f)
    {
        var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x / (size - 1f)) * 2f - 1f;
            float dy = (y / (size - 1f)) * 2f - 1f;
            float r = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
            float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(inner, 1f, r));
            t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        t.Apply();
        return t;
    }
}
