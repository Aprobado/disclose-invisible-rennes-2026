// Punches a hole in the eye buffer so the underlay passthrough shows through.
// Multiplies the already rendered color and alpha by (1 - mask): mask = _Strength * vertex alpha.
// Vertex alpha carries the feathered edge of the rug mesh (inner ring = 1, outer ring = 0).
// Meshes without vertex colors read as white, so the whole surface is punched.
Shader "Aprobado/XR/PassthroughPunch"
{
    Properties
    {
        // 0 keeps the virtual scene, 1 shows the passthrough fully
        _Strength ("Strength", Range(0, 1)) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        // On for an opaque core material (with an earlier queue), Off for the feather ring
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Float) = 0
    }

    SubShader
    {
        // Drawn after the Passthrough Occluder (set its material queue to 2998) so it can punch through it,
        // and before the other transparents (queue 3000+) so they blend over the hole instead of being erased.
        // Opaque objects in front are protected by the depth test.
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent-1"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "PassthroughPunch"
            Tags { "LightMode" = "UniversalForward" }

            Blend Zero OneMinusSrcAlpha
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _Strength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half alpha : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.alpha = input.color.a;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half mask = saturate(_Strength * input.alpha);
                return half4(0, 0, 0, mask);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
