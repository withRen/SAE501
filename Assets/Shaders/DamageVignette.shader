// Vignettage de dégâts plein écran, façon Call of Duty : rouge sang sur les bords, bords irréguliers, pulsation.
// + indicateur de direction : un arc rouge autour du centre de la vue, du côté d'où vient le coup (haut = devant, bas = derrière),
//   et les bords de ce côté-là rougissent davantage (visible en vision périphérique au casque).
// Dessiné en espace écran (un quad collé à la caméra suffit), toujours au-dessus de la scène, compatible XR (stéréo instanciée).
Shader "SAE501/DamageVignette"
{
    Properties
    {
        _Color ("Couleur", Color) = (0.55, 0.0, 0.0, 1)
        _Intensity ("Intensité", Range(0, 1)) = 0
        _Inner ("Rayon intérieur", Range(0, 1)) = 0.6
        _Width ("Largeur du dégradé", Range(0.05, 1)) = 0.45
    }
    SubShader
    {
        Tags { "Queue" = "Overlay+100" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Intensity, _Inner, _Width;
            float4 _Hits[4];        // coups récents : xy = direction (x droite, y devant, normalisée), z = intensité 0..1 (HealthVignette)

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                // plein écran : on place directement le quad en espace de découpage (indépendant de sa position)
                o.pos = float4(v.uv.x * 2 - 1, (v.uv.y * 2 - 1) * _ProjectionParams.x, UNITY_NEAR_CLIP_VALUE, 1);
                return o;
            }

            float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            float vnoise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x),
                            lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 sp = i.pos.xy / _ScreenParams.xy;          // 0..1 dans l'image de l'œil
                float2 p = (sp - 0.5) * 2.0;
                float r = length(p * float2(0.95, 1.0));
                // bords irréguliers, comme du sang sur l'écran
                float ang = atan2(p.y, p.x);
                float n = vnoise(float2(ang * 2.2, _Time.y * 0.25)) * 0.7 + vnoise(float2(ang * 7.0, 3.7 - _Time.y * 0.4)) * 0.3;
                float edge = smoothstep(_Inner + (n - 0.5) * 0.18, _Inner + _Width, r);
                float a = saturate(edge * _Intensity * (0.8 + 0.4 * n));
                float3 col = lerp(_Color.rgb, _Color.rgb * 0.3, edge);

                // indicateurs de direction (distance au centre corrigée du rapport largeur / hauteur : arcs bien ronds)
                float2 q = p * float2(_ScreenParams.x / _ScreenParams.y, 1.0);
                float rq = length(q);
                float2 nq = q / max(rq, 1e-4);
                float h = 0;
                [unroll] for (int k = 0; k < 4; k++)
                {
                    float w = _Hits[k].z;
                    float c = dot(nq, _Hits[k].xy);                                      // 1 = pile dans la direction du coup
                    float arc = smoothstep(0.80, 0.95, c);                               // environ ±30°
                    float ring = smoothstep(0.42, 0.46, rq) * (1 - smoothstep(0.52, 0.57, rq));
                    float side = smoothstep(0.75, 1.3, r) * pow(saturate(c), 4);          // bord rouge du côté du coup
                    h = max(h, w * max(arc * ring * 0.9, side * 0.55));
                }
                float3 hc = float3(0.8, 0.02, 0.02);
                float aOut = 1 - (1 - a) * (1 - h);
                col = (col * a * (1 - h) + hc * h) / max(aOut, 1e-4);
                return fixed4(col, aOut);
            }
            ENDCG
        }
    }
}
