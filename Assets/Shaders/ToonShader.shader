Shader "URPCustom/ToonShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Main Texture", 2D) = "white" {}
        [HDR]_AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        [HDR]_SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
        _Glossiness("Glossiness", Float) = 32
        [HDR]_RimColor("Rim Color", Color) = (1,1,1,1)
        _RimAmount("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;
            float4 _AmbientColor;
            float4 _SpecularColor;
            float _Glossiness;
            float4 _RimColor;
            float _RimAmount;
            float _RimThreshold;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - positionWS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(IN.viewDirWS);

                // Get main directional light
                Light mainLight = GetMainLight();

                float NdotL = max(0, dot(normalWS, mainLight.direction));
                float shadowAttenuation = mainLight.shadowAttenuation;

                // Step lighting for toon look
                float lightIntensity = smoothstep(0, 0.01, NdotL * shadowAttenuation);
                float3 light = lightIntensity * mainLight.color.rgb;

                // Specular highlight (Blinn-Phong)
                float3 halfVector = normalize(mainLight.direction + viewDirWS);
                float NdotH = max(0, dot(normalWS, halfVector));
                float specIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
                float specular = smoothstep(0.005, 0.01, specIntensity);

                // Rim lighting
                float rimDot = 1 - dot(viewDirWS, normalWS);
                float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
                rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);

                float4 texColor = tex2D(_MainTex, IN.uv);
                float3 finalColor = (light + _AmbientColor.rgb + specular * _SpecularColor.rgb + rimIntensity * _RimColor.rgb) * _Color.rgb * texColor.rgb;

                return float4(finalColor, 1);
            }

            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
