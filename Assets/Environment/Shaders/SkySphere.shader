// Ciel d'Alban (sphère retournée avec le panorama de forêt de nuit) : non éclairé, sans brouillard,
// dessiné en premier et sans écrire la profondeur, donc toujours derrière le reste de la scène.
// La sphère suit la tête du joueur (SkyFollow) : le ciel paraît infiniment loin et ne sort jamais du champ de la caméra.
// Brume d'horizon : le bas du panorama se fond dans la couleur du brouillard (unity_FogColor), comme le sol au loin,
// pour qu'on ne voie plus la ligne où le décor s'arrête (fin de la distance d'affichage) contre le ciel.
Shader "SAE501/SkySphere"
{
    Properties
    {
        _BaseMap ("Panorama", 2D) = "white" {}
        _BaseColor ("Teinte", Color) = (1, 1, 1, 1)
        _HazeBottom ("Brume : tout brouillard sous (sinus de la hauteur)", Range(-1, 1)) = -0.05
        _HazeTop ("Brume : plus de brouillard au-dessus de", Range(-1, 1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Background" "Queue" = "Background" "PreviewType" = "Skybox" }
        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _HazeBottom;
                float _HazeTop;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 dir : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                o.dir = TransformObjectToWorld(i.positionOS.xyz) - TransformObjectToWorld(float3(0, 0, 0));   // du centre (la tête) vers ce point
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                float h = normalize(i.dir).y;                                         // sinus de la hauteur au-dessus de l'horizon
                c.rgb = lerp(unity_FogColor.rgb, c.rgb, smoothstep(_HazeBottom, _HazeTop, h));
                return c;
            }
            ENDHLSL
        }
    }
}
