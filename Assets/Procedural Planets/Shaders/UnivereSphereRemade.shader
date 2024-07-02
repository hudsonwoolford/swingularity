Shader "Custom/UnivereSphereRemade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cube ("Skybox Texture", Cube) = "" {}
        _CubeOffset ("Cube Offset", vector) = (0,0,0)
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
            float3 localNormal : TEXCOORD2;
        };
        
        sampler2D _MainTex;
        samplerCUBE _Cube;
        float3 _CubeOffset;
        float _DepthScale;
        float _EmissionPower;
        float4 _Emission;
        
        half _Glossiness;
        half _Metallic;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
            o.viewDir = mul((float3x3)unity_WorldToObject, viewDir);
            o.localNormal = (v.normal + v.vertex * 2)/3;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 parallaxOffset = IN.viewDir * _DepthScale;
            float3 worldDir = normalize(mul((float3x3)unity_ObjectToWorld, IN.localNormal + parallaxOffset));
            half4 col = texCUBE(_Cube, worldDir + _CubeOffset);

            float luminance = dot(float3(0.2126729, 0.7151522, 0.0721750), col.rgb);
            o.Albedo = pow(col.rgb, _EmissionPower) * _Emission;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
