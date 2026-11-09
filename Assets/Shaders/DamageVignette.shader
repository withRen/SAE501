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
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 sp = i.pos.xy / _ScreenParams.xy;          // 0..1 dans l'image de l'œil
                float2 p = (sp - 0.5) * 2.0;
                float r = length(p * float2(0.95, 1.0));
                // bords irréguliers, comme du sang sur l'écran
                float ang = atan2(p.y, p.x);
                float n = vnoise(float2(ang * 2.2, _Time.y * 0.25)) * 0.7 + vnoise(float2(ang * 7.0, 3.7 - _Time.y * 0.4)) * 0.3;
                float edge = smoothstep(_Inner + (n - 0.5) * 0.18, _Inner + _Width, r);
                float a = saturate(edge * _Intensity * (0.8 + 0.4 * n));
                float3 col = lerp(_Color.rgb, _Color.rgb * 0.3, edge);

                // indicateurs de direction : _Hits[k].w == 0 -> dégât rouge, _Hits[k].w == 1 -> alerte acoustique ambre (brèche / angle mort)
                float2 q = p * float2(_ScreenParams.x / _ScreenParams.y, 1.0);
                float rq = length(q);
                float2 nq = q / max(rq, 1e-4);
                float hRed = 0;
                float hWarn = 0;
                [unroll] for (int k = 0; k < 4; k++)
                {
                    float w = _Hits[k].z;
                    float isWarn = _Hits[k].w;
                    float c = dot(nq, _Hits[k].xy);                                      // 1 = pile dans la direction du coup / de l'alerte
                    float arc = smoothstep(lerp(0.80, 0.85, isWarn), 0.96, c);           // arc plus fin pour une pré-alerte ambre
                    float ring = smoothstep(lerp(0.42, 0.50, isWarn), lerp(0.46, 0.53, isWarn), rq)
                               * (1 - smoothstep(lerp(0.52, 0.56, isWarn), lerp(0.57, 0.60, isWarn), rq));
                    float side = smoothstep(lerp(0.75, 0.82, isWarn), 1.3, r) * pow(saturate(c), 4);
                    float val = w * max(arc * ring * 0.9, side * 0.55);
                    if (isWarn > 0.5) hWarn = max(hWarn, val);
                    else              hRed  = max(hRed,  val);
                }
                float h = max(hRed, hWarn);
                float3 hc = lerp(float3(0.8, 0.02, 0.02), float3(1.0, 0.62, 0.08), saturate(hWarn / max(hRed + hWarn, 1e-4)));
                float aOut = 1 - (1 - a) * (1 - h);
                col = (col * a * (1 - h) + hc * h) / max(aOut, 1e-4);
                return fixed4(col, aOut);
            }
            ENDCG
        }
    }
}
