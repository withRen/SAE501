// Contour des objets interactifs (InteractableOutline) : la forme est redessinée un peu gonflée, faces arrière seulement,
// d'une couleur unie. Ce qui dépasse autour de l'objet forme le liseré. Compatible rendu stéréo du Quest (multiview).
Shader "SAE501/Outline"
{
    Properties
    {
        _Color ("Couleur", Color) = (1, 0.75, 0.27, 1)
        _Width ("Épaisseur (m)", Float) = 0.004
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry+10" }
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Width;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 w = TransformObjectToWorld(i.positionOS.xyz);
                float3 n = normalize(TransformObjectToWorldNormal(i.normalOS));
                o.positionCS = TransformWorldToHClip(w + n * _Width);   // épaisseur en mètres, quelle que soit l'échelle de l'objet
                return o;
            }

            half4 frag(Varyings i) : SV_Target { return _Color; }
            ENDHLSL
        }
    }
}
