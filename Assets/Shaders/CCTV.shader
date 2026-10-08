// [7.2] Effet « caméra de surveillance » pour les écrans du pupitre (et la vue plein écran) :
// image en vert (vision nocturne), grain qui bouge, lignes de balayage, bande qui défile, bords sombres, léger scintillement.
// Le rouge vif (le point « REC ») garde sa couleur. Léger pour le Quest : quelques calculs par pixel, pas de texture en plus.
Shader "SAE501/CCTV"
{
    Properties
    {
        _BaseMap ("Image de la caméra", 2D) = "black" {}
        _Tint ("Teinte", Color) = (0.35, 1.0, 0.5, 1)
        _Gain ("Luminosité (vision nocturne)", Range(0.5, 12)) = 5
        _Noise ("Grain", Range(0, 0.6)) = 0.18
        _Lines ("Nombre de lignes", Float) = 180
        _LineStrength ("Force des lignes", Range(0, 1)) = 0.3
        _Vignette ("Bords sombres", Range(0, 3)) = 1.3
        _Roll ("Bande qui défile", Range(0, 0.5)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            fixed4 _Tint;
            float _Gain, _Noise, _Lines, _LineStrength, _Vignette, _Roll;
            float _CctvPower;      // global, réglé par le disjoncteur (Breaker) : 0 = caméras coupées

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
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float t = _Time.y;

                // bande horizontale qui défile lentement de bas en haut (léger décalage de l'image)
                float band = smoothstep(0.0, 0.04, 0.04 - abs(frac(uv.y - t * 0.08) - 0.5));
                uv.x += band * 0.004;

                fixed3 src = tex2D(_BaseMap, uv).rgb;
                // vision nocturne : les zones sombres sont fortement relevées, les zones claires ne saturent pas (carte très sombre)
                float lum0 = dot(src, float3(0.299, 0.587, 0.114));
                float lum = 1.0 - exp(-_Gain * sqrt(lum0));

                // grain : change ~24 fois par seconde
                float n = hash(floor(uv * float2(320, 180)) + floor(t * 24.0)) - 0.5;
                lum += n * _Noise;

                // lignes de balayage
                float scan = 1.0 - _LineStrength * step(0.5, frac(uv.y * _Lines));
                lum *= scan;

                // bande plus claire + scintillement général
                lum += band * _Roll;
                lum *= 0.94 + 0.06 * sin(t * 37.0);

                // bords sombres
                float2 d = uv - 0.5;
                lum *= saturate(1.0 - dot(d, d) * _Vignette);

                fixed3 col = saturate(lum) * _Tint.rgb;
                // disjoncteur coupé : écran éteint (juste un léger grain)
                if (_CctvPower < 0.5) return fixed4(saturate(n * 0.05 + 0.01) * float3(1, 1, 1), 1);

                // le rouge vif (point « REC ») reste rouge
                float red = saturate((src.r - max(src.g, src.b)) * 3.0);
                col = lerp(col, src * 1.3, red);
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}
