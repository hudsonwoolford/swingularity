Shader "Custom/Dissolve"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [IntRange]_Octaves("Octaves", Range(0,5)) = 1
        _Offset("Offset", vector) = (0,0,0)
        _Scale("Scale", float) = 1
        _Persistence("Persistence", Range(0, 5)) = 1
        _Lacunarity("Lacunarity", Range(0,5)) = 1
        _Gain("Gain", Range(0, 5)) = 1
        _Cutoff("Alpha Cutoff", Range(0,1)) = 1
        _EdgeAdd("Edge Size", float) = 1
        [hdr]_EdgeColor ("Edge Color", Color) = (1,1,1,1)

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard vertex:vert fullforwardshadows alpha:blend

        #pragma target 3.0
        #include "Includes/FractalNoise.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 vertPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Cutoff;
        float _EdgeAdd;
        fixed4 _EdgeColor;

        int _Octaves;
        float3 _Offset;
        float _Scale, _Persistence, _Lacunarity, _Gain;
        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertPos = v.vertex;
        }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            float noiseVal = sin(SimpleNoise(IN.vertPos, _Octaves, _Scale, _Persistence, _Lacunarity, _Gain, _Offset));
            o.Alpha = step(c.a * noiseVal, _Cutoff);
            float stepProgress = 1 - step(noiseVal + _EdgeAdd, _Cutoff);
            o.Emission = stepProgress * _EdgeColor;


            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
