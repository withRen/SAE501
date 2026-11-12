// Lentille de la lunette du sniper (SniperScope) : disque posé sur l'oculaire, au bout du tube.
//   - visée (_Active = 1) : l'image de la caméra de la lunette (_MainTex, RenderTexture), réticule fin type mil-dot dessiné ici
//     (net quelle que soit la résolution de l'image), point central rouge, vignettage noir sur le bord, et ombre de la « pupille de sortie » :
//     un cercle noir qui mange l'image quand l'œil n'est pas dans l'axe ou trop loin (_Eye : xy = décalage de l'œil, z = rayon de la pupille) ;
//   - hors visée (_Active = 0) : verre sombre avec un reflet (la caméra de la lunette est éteinte).
// Disque découpé dans un quad (uv 0..1). Compatible XR (stéréo instanciée) : la même image est vue par les deux yeux.
Shader "SAE501/ScopeLens"
{
    Properties
    {
        _MainTex ("Image de la lunette", 2D) = "black" {}
        _Active ("Visée (0 = verre, 1 = image)", Range(0, 1)) = 0
        _Eye ("Œil (xy décalage, z rayon de la pupille)", Vector) = (0, 0, 1, 0)
        _Glass ("Verre", Color) = (0.03, 0.05, 0.07, 1)
        _Tint ("Reflet", Color) = (0.35, 0.55, 0.75, 1)
        _Dot ("Point du réticule", Color) = (1, 0.12, 0.08, 1)
        _Line ("Épaisseur des traits", Range(0.001, 0.02)) = 0.006
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+10" "RenderType" = "Opaque" "IgnoreProjector" = "True" }
        Pass
        {
            Cull Off
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Active, _Line;
            float4 _Eye;
            fixed4 _Glass, _Tint, _Dot;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // trait d'épaisseur w (en rayons de lentille), adouci sur un pixel
            float band(float d, float w, float aa) { return 1 - smoothstep(w - aa, w + aa, d); }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 p = (i.uv - 0.5) * 2.0;            // -1..1, 0 au centre
                float r = length(p);
                float aa = max(fwidth(r), 1e-4);
                clip(1.0 - r);

                // ----- image de la lunette -----
                float3 img = tex2D(_MainTex, i.uv).rgb;
                // pupille de sortie : l'image n'est entière que l'œil dans l'axe, à la bonne distance
                float pupil = 1 - smoothstep(_Eye.z - 0.18, _Eye.z, length(p - _Eye.xy));
                float edge = 1 - smoothstep(0.80, 0.99, r);                   // vignettage du tube
                float3 view = img * pupil * edge;

                // réticule duplex : traits fins au centre, montants épais vers le bord, points mil-dot tous les 0,1
                float w = _Line;
                float thin = max(band(abs(p.x), w * 0.5, aa) * step(abs(p.y), 0.55), band(abs(p.y), w * 0.5, aa) * step(abs(p.x), 0.55));
                float thick = max(band(abs(p.x), w * 1.6, aa) * step(0.55, abs(p.y)), band(abs(p.y), w * 1.6, aa) * step(0.55, abs(p.x)));
                float dots = 0;
                [unroll] for (int k = 1; k <= 4; k++)
                {
                    float s = k * 0.11;
                    dots = max(dots, band(length(float2(abs(p.x) - s, p.y)), w * 1.3, aa));
                    dots = max(dots, band(length(float2(p.x, abs(p.y) - s)), w * 1.3, aa));
                }
                float ret = saturate(max(max(thin, thick), dots)) * pupil;
                view = lerp(view, float3(0, 0, 0), ret);
                float centre = band(r, w * 1.5, aa);
                view = lerp(view, _Dot.rgb * 1.4, centre * pupil);            // point rouge éclairé

                // ----- verre sombre (hors visée) -----
                float refl = smoothstep(0.35, 0.0, abs(p.x * 0.7 + p.y * 0.7 - 0.35)) * smoothstep(1.0, 0.2, r);
                float rim = smoothstep(0.7, 1.0, r);
                float3 glass = _Glass.rgb + _Tint.rgb * (refl * 0.35 + rim * 0.12);

                float3 col = lerp(glass, view, _Active);
                col *= 1 - smoothstep(0.93, 1.0, r) * 0.85;                    // bord noir du tube
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
