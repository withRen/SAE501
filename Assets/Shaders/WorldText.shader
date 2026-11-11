// Texte du décor (panneaux des portes, état du générateur, radio) : TextMesh avec la police intégrée.
// Le matériau par défaut des polices (GUI/Text Shader) se dessine sans test de profondeur dans URP : le texte se voyait
// à travers les murs. Ici : test de profondeur normal (les murs le cachent) et fondu selon la distance à la caméra,
// calculé dans le shader (aucun script par image, aucun raycast). Compatible XR (stéréo instanciée).
Shader "SAE501/WorldText"
{
    Properties
    {
        _MainTex ("Texture de la police", 2D) = "white" {}
        _FadeNear ("Fondu : début (m)", Float) = 3.5
        _FadeFar ("Fondu : invisible au-delà (m)", Float) = 5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Pass
        {
            ZTest LEqual
            ZWrite Off
            Cull Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _FadeNear, _FadeFar;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityWorldToClipPos(wp);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                // 1 près du joueur, 0 au-delà de _FadeFar
                o.color.a *= 1 - smoothstep(_FadeNear, _FadeFar, distance(wp, _WorldSpaceCameraPos));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = i.color;
                c.a *= tex2D(_MainTex, i.uv).a;     // la police intégrée stocke les glyphes dans l'alpha
                clip(c.a - 0.004);                  // rien à dessiner : on évite le mélange (Quest : bande passante)
                return c;
            }
            ENDCG
        }
    }
}
