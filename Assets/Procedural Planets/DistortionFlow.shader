Shader "Custom/DistortionFlow"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _WaterFogColor ("Water Fog Color", Color) = (1,1,1)
        _WaterFogDensity ("Water Fog Density", Range(0.01, 2)) = 0.1
        _OceanCentre ("Ocean Centre", Vector) = (0,0,0)
        _OceanRadius ("Ocean Radius", Range(0,10)) = 1
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        GrabPass { "_WaterBackground" }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha fullforwardshadows finalcolor:ResetAlpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #include "LookingThroughWater.cginc"

        

        sampler2D _MainTex;

        struct Input
        {
            float3 worldPos;
            float4 screenPos;
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _WaterFogger;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            
            float3 relCameraPos = (0,0,0);
            relCameraPos.xy = IN.worldPos;
            float3 rayVector = IN.worldPos - relCameraPos;
            float3 rayDir = rayVector / length(rayVector); 
            o.Emission = ColorBelowWater(rayDir, IN.screenPos, length(IN.worldPos.z - _WorldSpaceCameraPos.z), relCameraPos) * (1-c.a);
            o.Alpha = 1;
            //o.Albedo = ColorBelowWater(rayDir, IN.screenPos, length(IN.worldPos.z - _WorldSpaceCameraPos.z), relCameraPos);
        }
        void ResetAlpha (Input IN, SurfaceOutputStandard o, inout fixed4 color) 
        {
			color.a = 1;
		}
        ENDCG
    }
}
