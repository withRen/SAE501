using UnityEngine;

// Petits sons générés par code (mis en cache statique pour éviter toute allocation GC en VR sur Meta Quest 3)
public static class ProcAudio
{
    const int Rate = 44100;

    static AudioClip _stepWalker, _stepBruiser, _stepRunner, _attackWindup, _zombieDeath, _bodyThud, _meleeSwing, _axeImpact, _crowbarImpact;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _stepWalker = _stepBruiser = _stepRunner = _attackWindup = _zombieDeath = _bodyThud = _meleeSwing = _axeImpact = _crowbarImpact = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Warmup()
    {
        ZombieStep_Walker();
        ZombieStep_Bruiser();
        ZombieStep_Runner();
        AttackWindup();
        ZombieDeath();
        BodyThud();
        MeleeSwing();
        AxeImpact();
        CrowbarImpact();
    }

    static AudioClip Make(string name, float dur, System.Func<float, float> f)
    {
        int n = (int)(dur * Rate);
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(f(i / (float)Rate), -0.95f, 0.95f);
        var c = AudioClip.Create(name, n, 1, Rate, false);
        c.hideFlags = HideFlags.DontSave;
        c.SetData(d, 0);
        return c;
    }

    public static AudioClip Shot() => Make("shot", 0.25f, t => (Random.value * 2 - 1) * Mathf.Exp(-t * 22f) * 0.9f + Mathf.Sin(t * 120f * 6.28f) * Mathf.Exp(-t * 18f) * 0.5f);
    public static AudioClip Click() => Make("click", 0.04f, t => (Random.value * 2 - 1) * Mathf.Exp(-t * 120f) * 0.5f);
    // clic de culasse vide : un « tic » métallique sec (corps grave + résonance aiguë qui s'éteint vite), suivi d'un rebond plus faible
    public static AudioClip DryFire() => Make("dryfire", 0.09f, t =>
    {
        float tick = (Mathf.Sin(t * 1900f * 6.28f) * 0.55f + Mathf.Sin(t * 3300f * 6.28f) * 0.3f) * Mathf.Exp(-t * 95f);
        float click = (Random.value * 2 - 1) * Mathf.Exp(-t * 400f) * 0.5f;
        float t2 = t - 0.045f;
        float rebound = t2 > 0f ? Mathf.Sin(t2 * 2400f * 6.28f) * Mathf.Exp(-t2 * 130f) * 0.25f : 0f;
        return tick + click + rebound;
    });
    // [#102] alarme de début de vague : sirène à deux tons (3 cycles), non spatialisée (WaveManager)
    public static AudioClip Alarm() => Make("alarme", 2.4f, t =>
    {
        float f = (t % 0.8f) < 0.4f ? 660f : 880f;
        float env = Mathf.Clamp01(t / 0.03f) * Mathf.Clamp01((2.4f - t) / 0.2f);
        float sq = Mathf.Sign(Mathf.Sin(t * f * 6.2832f)) * 0.25f + Mathf.Sin(t * f * 6.2832f) * 0.25f;
        return sq * env * 0.8f;
    });
    // [#104] pas d'un zombie sur le toit du bunker : coup sourd et grave, avec un peu de gravier
    public static AudioClip RoofStep() => Make("pas_toit", 0.22f, t =>
        Mathf.Sin(t * 70f * 6.2832f) * Mathf.Exp(-t * 25f) * 0.8f + (Random.value * 2 - 1) * Mathf.Exp(-t * 40f) * 0.25f);
    // [#104] poussière et gravats qui tombent du trou du toit : ruissellement (bruit filtré) avec des petits chocs
    public static AudioClip DustFall()
    {
        float lp = 0f;
        return Make("poussiere", 3.5f, t =>
        {
            lp += ((Random.value * 2 - 1) - lp) * 0.08f;                     // bruit adouci (passe-bas)
            float env = Mathf.Clamp01(t / 0.4f) * Mathf.Clamp01((3.5f - t) / 0.8f);
            float grit = Random.value < 0.0006f ? (Random.value * 2 - 1) * 0.9f : 0f;
            return (lp * 1.6f + grit) * env * 0.7f;
        });
    }
    // [#113] balle dans la chair : impact mat et grave, un peu « mouillé » (bruit adouci), sans résonance métallique
    public static AudioClip Flesh()
    {
        float lp = 0f;
        return Make("chair", 0.18f, t =>
        {
            lp += ((Random.value * 2 - 1) - lp) * 0.25f;
            return lp * Mathf.Exp(-t * 30f) * 1.4f + Mathf.Sin(t * 95f * 6.2832f) * Mathf.Exp(-t * 35f) * 0.7f;
        });
    }
    // [#113] tir à la tête : craquement sec (os) par-dessus l'impact de chair
    public static AudioClip HeadCrack() => Make("tete", 0.22f, t =>
    {
        float crack = (Random.value * 2 - 1) * Mathf.Exp(-t * 70f) * 0.8f;
        float t2 = t - 0.018f;
        float crack2 = t2 > 0f ? (Random.value * 2 - 1) * Mathf.Exp(-t2 * 90f) * 0.5f : 0f;
        return crack + crack2 + Mathf.Sin(t * 70f * 6.2832f) * Mathf.Exp(-t * 18f) * 0.8f;
    });
    // [#114] planche qui se cale sur l'ouverture : « clac » de bois sec (deux tons courts)
    public static AudioClip Clack() => Make("clac", 0.09f, t =>
        (Mathf.Sin(t * 520f * 6.2832f) * 0.6f + Mathf.Sin(t * 1240f * 6.2832f) * 0.3f) * Mathf.Exp(-t * 75f)
        + (Random.value * 2 - 1) * Mathf.Exp(-t * 300f) * 0.5f);
    // [#116] planche arrachée : craquement de bois (série de petits claquements serrés) puis le choc grave
    public static AudioClip WoodCrack() => Make("craquement", 0.6f, t =>
    {
        float snaps = t < 0.25f && Random.value < 0.02f * (1f - t * 4f) ? (Random.value * 2 - 1) : 0f;
        float noise = (Random.value * 2 - 1) * Mathf.Exp(-t * 14f) * 0.35f;
        float body = Mathf.Sin(t * 140f * 6.2832f) * Mathf.Exp(-t * 12f) * 0.5f;
        return snaps * 0.9f + noise + body;
    });
    // radio de ravitaillement : deux bips montants sur un fond de grésillement
    public static AudioClip RadioChirp() => Make("radio_chirp", 0.7f, t =>
    {
        float noise = (Random.value * 2 - 1) * 0.18f * (0.6f + 0.4f * Mathf.Sin(t * 37f));
        float beep = 0f;
        if (t > 0.15f && t < 0.27f) beep = Mathf.Sin(t * 1200f * 6.28f) * 0.45f;
        if (t > 0.35f && t < 0.47f) beep = Mathf.Sin(t * 1600f * 6.28f) * 0.45f;
        return noise * Mathf.Clamp01(1.4f - t * 1.8f) + beep;
    });
    // souffle qui monte puis retombe : la caisse arrive par le toit
    public static AudioClip Whoosh() => Make("whoosh", 1.2f, t =>
        (Random.value * 2 - 1) * 0.6f * Mathf.Sin(Mathf.Clamp01(t / 1.2f) * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(t * 18f)));
    // caisse qui tombe sur le sol : coup grave et sourd
    public static AudioClip Thud() => Make("caisse", 0.5f, t =>
        Mathf.Sin(t * 70f * 6.28f) * Mathf.Exp(-t * 9f) * 0.9f + (Random.value * 2 - 1) * Mathf.Exp(-t * 35f) * 0.5f);
    // coup reçu par le joueur : choc sourd dans le corps (grave) + souffle bref ; non spatialisé (HealthVignette)
    public static AudioClip PlayerHurt()
    {
        float lp = 0f;
        return Make("coup_recu", 0.35f, t =>
        {
            lp += ((Random.value * 2 - 1) - lp) * 0.12f;
            return Mathf.Sin(t * 55f * 6.2832f) * Mathf.Exp(-t * 12f) * 0.9f + lp * Mathf.Exp(-t * 18f) * 1.6f;
        });
    }
    // griffure du zombie : bruit aigu filtré (< 3.5 kHz pour confort auditif VR) qui monte puis retombe ; joué du côté du zombie (HealthVignette)
    public static AudioClip ClawSwipe()
    {
        float lp = 0f, topLp = 0f;
        return Make("griffure", 0.2f, t =>
        {
            float n = Random.value * 2 - 1;
            lp += (n - lp) * 0.28f;
            float hp = n - lp;
            topLp += (hp - topLp) * 0.32f;
            return topLp * Mathf.Sin(Mathf.PI * t / 0.2f) * 0.85f;
        });
    }
    // battement de cœur « poum-poum » quand il reste peu de PV (HealthVignette, au rythme de la pulsation rouge)
    public static AudioClip Heartbeat(float gap = 0.23f) => Make("coeur", 0.5f, t =>
    {
        float b1 = Mathf.Sin(t * 48f * 6.2832f) * Mathf.Exp(-t * 22f);
        float t2 = t - gap;
        float b2 = t2 > 0f ? Mathf.Sin(t2 * 42f * 6.2832f) * Mathf.Exp(-t2 * 22f) * 0.7f : 0f;
        return (b1 + b2) * 0.9f;
    });
    public static AudioClip Hit() => Make("hit", 0.12f, t => Mathf.Sin(t * 900f * 6.28f) * Mathf.Exp(-t * 45f) * 0.6f + (Random.value * 2 - 1) * Mathf.Exp(-t * 80f) * 0.4f);

    // ───────────────────────── zombies-combat : pas au sol par archétype ─────────────────────────
    public static AudioClip ZombieStep_Walker()
    {
        if (_stepWalker) return _stepWalker;
        float lp = 0f;
        return _stepWalker = Make("pas_rodeur", 0.16f, t =>
        {
            float freq = 95f - 35f * (t / 0.16f);
            float heel = Mathf.Sin(t * freq * 6.2832f) * Mathf.Exp(-t * 32f) * 0.45f;
            lp += ((Random.value * 2f - 1f) - lp) * 0.14f;
            float scuff = lp * Mathf.Sin(Mathf.PI * t / 0.16f) * Mathf.Exp(-t * 14f) * 0.35f;
            return heel + scuff;
        });
    }

    public static AudioClip ZombieStep_Bruiser()
    {
        if (_stepBruiser) return _stepBruiser;
        float lp = 0f, phase = 0f;
        return _stepBruiser = Make("pas_cogneur", 0.24f, t =>
        {
            float freq = 110f * Mathf.Exp(-t * 12f) + 42f;
            phase += freq * 6.2832f / Rate;
            float sub = Mathf.Sin(phase) * Mathf.Exp(-t * 16f) * 0.75f;
            lp += ((Random.value * 2f - 1f) - lp) * 0.22f;
            float grit = lp * Mathf.Exp(-t * 24f) * 0.40f;
            return (float)System.Math.Tanh((sub + grit) * 1.4f);
        });
    }

    public static AudioClip ZombieStep_Runner()
    {
        if (_stepRunner) return _stepRunner;
        float lpH = 0f, lpL = 0f;
        return _stepRunner = Make("pas_coureur", 0.11f, t =>
        {
            float freq = 175f - 60f * (t / 0.11f);
            float slap = Mathf.Sin(t * freq * 6.2832f) * Mathf.Exp(-t * 48f) * 0.50f;
            float n = Random.value * 2f - 1f;
            lpH += (n - lpH) * 0.45f;
            lpL += (n - lpL) * 0.15f;
            float tap = (lpH - lpL) * Mathf.Exp(-t * 42f) * 0.55f;
            return slap + tap;
        });
    }

    // ───────────────────────── zombies-combat : télégraphe d'attaque & mort ─────────────────────────
    public static AudioClip AttackWindup()
    {
        if (_attackWindup) return _attackWindup;
        float lp = 0f, phase = 0f;
        return _attackWindup = Make("amorce_attaque", 0.25f, t =>
        {
            float fc = 135f + 125f * (t / 0.25f);
            phase += fc * 6.2832f / Rate;
            float fm = Mathf.Sin(phase + 2.2f * Mathf.Sin(t * 36f * 6.2832f)) * 0.55f;
            lp += ((Random.value * 2f - 1f) - lp) * 0.28f;
            float env = Mathf.Pow(Mathf.Clamp01(t / 0.25f), 0.55f) * Mathf.Exp(-Mathf.Max(0f, t - 0.17f) * 38f);
            return (fm + lp * 0.35f) * env;
        });
    }

    public static AudioClip ZombieDeath()
    {
        if (_zombieDeath) return _zombieDeath;
        float lp = 0f, phase = 0f;
        return _zombieDeath = Make("mort_zombie", 0.55f, t =>
        {
            float f0 = 210f * Mathf.Exp(-t * 2.2f) + 55f;
            phase += f0 * 6.2832f / Rate;
            float rough = 1f + 0.35f * Mathf.Sin(t * 21f * 6.2832f);
            float voice = (0.60f * Mathf.Sin(phase) + 0.25f * Mathf.Sin(phase * 2f) + 0.15f * Mathf.Sin(phase * 3f)) * rough;
            lp += ((Random.value * 2f - 1f) - lp) * 0.16f;
            float breath = lp * Mathf.Exp(-t * 5.5f) * 0.35f;
            float env = (1f - Mathf.Exp(-t * 45f)) * Mathf.Exp(-t * 4.5f);
            return (voice * 0.65f + breath) * env;
        });
    }

    public static AudioClip BodyThud()
    {
        if (_bodyThud) return _bodyThud;
        float lp = 0f;
        return _bodyThud = Make("chute_corps", 0.28f, t =>
        {
            float f1 = Mathf.Lerp(90f, 45f, Mathf.Clamp01(t / 0.10f));
            float hit1 = Mathf.Sin(t * f1 * 6.2832f) * Mathf.Exp(-t * 26f) * 0.45f;
            float hit2 = 0f;
            float t2 = t - 0.085f;
            if (t2 >= 0f)
            {
                float f2 = Mathf.Lerp(78f, 34f, Mathf.Clamp01(t2 / 0.19f));
                lp += ((Random.value * 2f - 1f) - lp) * 0.18f;
                hit2 = Mathf.Sin(t2 * f2 * 6.2832f) * Mathf.Exp(-t2 * 15f) * 0.80f + lp * Mathf.Exp(-t2 * 14f) * 0.25f;
            }
            return hit1 + hit2;
        });
    }

    // ───────────────────────── zombies-combat : armes de mêlée ─────────────────────────
    public static AudioClip MeleeSwing()
    {
        if (_meleeSwing) return _meleeSwing;
        float lp = 0f;
        return _meleeSwing = Make("souffle_melee", 0.19f, t =>
        {
            float u = Mathf.Clamp01(t / 0.19f);
            float alpha = 0.05f + 0.26f * Mathf.Sin(Mathf.PI * u);
            lp += ((Random.value * 2f - 1f) - lp) * alpha;
            float air = Mathf.Sin(t * (130f - 45f * u) * 6.2832f) * 0.30f;
            float env = Mathf.Sin(Mathf.PI * Mathf.Pow(u, 0.75f)) * 0.75f;
            return (lp + air) * env;
        });
    }

    public static AudioClip AxeImpact()
    {
        if (_axeImpact) return _axeImpact;
        float lp = 0f, phase = 0f;
        return _axeImpact = Make("impact_hache", 0.21f, t =>
        {
            float f = 1850f * Mathf.Exp(-t * 45f) + 320f;
            phase += f * 6.2832f / Rate;
            float bite = Mathf.Sin(phase) * Mathf.Exp(-t * 50f) * 0.55f;
            lp += ((Random.value * 2f - 1f) - lp) * 0.32f;
            float rip = lp * (0.65f + 0.35f * Mathf.Sin(t * 70f * 6.2832f)) * Mathf.Exp(-t * 19f) * 0.55f;
            return bite + rip;
        });
    }

    public static AudioClip CrowbarImpact()
    {
        if (_crowbarImpact) return _crowbarImpact;
        return _crowbarImpact = Make("impact_pied_de_biche", 0.26f, t =>
        {
            float f0 = Mathf.Lerp(135f, 52f, Mathf.Clamp01(t / 0.12f));
            float thud = Mathf.Sin(t * f0 * 6.2832f) * Mathf.Exp(-t * 28f) * 0.55f;
            float m1 = Mathf.Sin(t * 540f * 6.2832f) * Mathf.Exp(-t * 18f) * 0.22f;
            float m2 = Mathf.Sin(t * 1210f * 6.2832f) * Mathf.Exp(-t * 26f) * 0.15f;
            float m3 = Mathf.Sin(t * 2380f * 6.2832f) * Mathf.Exp(-t * 42f) * 0.08f;
            return thud + m1 + m2 + m3;
        });
    }
}
