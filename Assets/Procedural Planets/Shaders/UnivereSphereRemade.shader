Shader "Custom/UniverseSphereRemade"
{
    Properties
    {
        _FresnelStrength("Fresnel Strength", float) = 0
        [HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Cube ("Skybox Texture", Cube) = "" {}
        _DepthScale ("Depth Scale", float) = 0.1
        [HDR]_Emission("Emission", Color) = (1,1,1,1)
        _EmissionPower("Emission Power", float) = 2
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0


        struct Input
        {
            float2 uv : TEXCOORD0;
            float3 viewDir : TEXCOORD1;
            float3 worldNormal : TEXCOORD2;
            float3 localNormal : TEXCOORD3;
        };
        
        sampler2D _MainTex;
        samplerCUBE _Cube;
        float _DepthScale;
        float _EmissionPower;
        float4 _Emission;
        float4 _FresnelColor;
        float _FresnelStrength;
        half _Glossiness;
        half _Metallic;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
            o.viewDir = viewDir;
            o.localNormal = (v.normal + v.vertex * 2)/3;
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 parallaxOffset = IN.viewDir * _DepthScale;
            float3 worldDir = normalize(IN.localNormal + parallaxOffset);
            float4 col = texCUBE(_Cube, IN.viewDir);

            float luminance = dot(float3(0.2126729, 0.7151522, 0.0721750), col.rgb);

            float fresnelVal = 1 - saturate(abs(dot(IN.worldNormal, IN.viewDir)));
            float fresnel = pow(fresnelVal, _FresnelStrength);
            
            col = pow(col, _EmissionPower) * _Emission;
            col = lerp(col, _FresnelColor, pow(fresnel, _FresnelStrength));

            o.Albedo = col;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
