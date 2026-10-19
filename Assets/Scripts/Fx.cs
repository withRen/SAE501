using UnityEngine;

// Petits effets de particules du jeu, sans prefab : un système de particules par effet, partagé, en coordonnées monde.
// Chaque appel émet une gerbe à l'endroit voulu (Emit), donc rien n'est créé ni détruit pendant la partie.
//   [#113] Impact sur un zombie : gouttes sombres au point touché (gerbe plus grosse à la tête).
//   [#116] Planche arrachée : éclats de bois projetés vers l'intérieur du bunker.
// Matériau : Resources/Fx_Particules (créé par RoomBuilder.EnsureFxMaterials, shader URP Particles/Unlit).
public static class Fx
{
    static ParticleSystem blood, splinters;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { blood = null; splinters = null; }

    public static void Blood(Vector3 point, Vector3 normal, bool head)
    {
        if (!blood) blood = Make("FX_Impact_Zombie", false, 1.4f);
        if (!blood) return;
        Burst(blood, point, normal, head ? 26 : 12, new Vector2(0.8f, head ? 2.6f : 1.8f), new Vector2(0.012f, head ? 0.035f : 0.025f),
              new Vector2(0.35f, 0.7f), new Color(0.22f, 0.02f, 0.02f), new Color(0.4f, 0.05f, 0.03f), 0.6f);
    }

    public static void Splinters(Vector3 point, Vector3 direction, Vector3 extent)
    {
        if (!splinters) splinters = Make("FX_Eclats_Bois", true, 2.2f);
        if (!splinters) return;
        // éclats répartis sur toute la longueur de la planche
        for (int i = 0; i < 5; i++)
        {
            var p = point + extent * Random.Range(-1f, 1f);
            Burst(splinters, p, direction + Vector3.up * 0.6f, 7, new Vector2(1.2f, 3.2f), new Vector2(0.015f, 0.04f),
                  new Vector2(0.8f, 1.6f), new Color(0.42f, 0.29f, 0.16f), new Color(0.65f, 0.5f, 0.32f), 1f);
        }
    }

    static void Burst(ParticleSystem ps, Vector3 point, Vector3 dir, int count, Vector2 speed, Vector2 size, Vector2 life, Color a, Color b, float spread)
    {
        dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.up;
        var ep = new ParticleSystem.EmitParams();
        for (int i = 0; i < count; i++)
        {
            ep.position = point;
            ep.velocity = (dir + Random.insideUnitSphere * spread).normalized * Random.Range(speed.x, speed.y);
            ep.startSize = Random.Range(size.x, size.y);
            ep.startLifetime = Random.Range(life.x, life.y);
            ep.startColor = Color.Lerp(a, b, Random.value);
            ep.rotation = Random.Range(0f, 360f);
            ps.Emit(ep, 1);
        }
    }

    static ParticleSystem Make(string name, bool stretched, float gravity)
    {
        var mat = Resources.Load<Material>("Fx_Particules");
        if (!mat) { Debug.LogWarning("[Fx] matériau Resources/Fx_Particules introuvable (relancer Build VR Scene)"); return null; }
        var go = new GameObject(name);
        Object.DontDestroyOnLoad(go);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false; main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = gravity;
        main.maxParticles = 300;
        var em = ps.emission; em.enabled = false;
        var shape = ps.shape; shape.enabled = false;
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        if (stretched)
        {
            // éclats : ils tournent sur eux-mêmes en tombant
            var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-8f, 8f);
        }
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = mat;
        r.renderMode = stretched ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
        if (stretched) { r.lengthScale = 2.5f; r.velocityScale = 0.02f; }
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        ps.Play();
        return ps;
    }
}
