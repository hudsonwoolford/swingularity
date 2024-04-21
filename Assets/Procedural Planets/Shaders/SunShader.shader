Shader "Custom/SunShader"
{
    Properties
    {
        [HDR]_Color1 ("Color 1", Color) = (1,1,1,1)
        [HDR]_Color2 ("Color 2", Color) = (1,1,1,1)
        _BigNoise("Big Noise", float) = 1
        _Power("Power", float) = 1
        _Speed("Speed", float) = 1

        [Header(Noise)] //Noise

        [IntRange]_Octaves("Octaves", Range(0,5)) = 1
        _Offset("Offset", vector) = (0,0,0)
        _Scale("Scale", Range(0,50)) = 1
        _Persistence("Persistence", Range(0, 5)) = 1
        _Lacunarity("Lacunarity", Range(0,5)) = 1
        _Multiplier("Multiplier", Range(0,5)) = 1
        _Gain("Gain", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert 
        #include "Includes/FractalNoise.cginc"
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 vertPos;
        };
        fixed4 _Color1, _Color2;
        int _Octaves;
        float3 _Offset;
        float _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain;
        float _Power, _Speed;
        float _BigNoise;
        float2 hash(float2 p)
        {
            return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
        }
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float angle = snoise((500+_Time) * 0.1 * (1+snoise(v.normal))); // You can adjust _Frequency to control the speed of oscillation
            float sinValue = sin(angle);

            float3 displacement = v.normal * (0.05 * (sinValue + 1) / 2) ;

            v.vertex = float4(v.vertex.xyz + displacement, 1);

            o.vertPos = normalize(v.vertex);
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = 0;
            float bigPass =  pow(snoise(IN.vertPos + _BigNoise), 1);
            float lerpVal =  pow(SimpleNoise(IN.vertPos, _Octaves, _Scale, _Persistence, _Lacunarity, _Gain, float3(_Time.y + 50, _Time.y, _Time.y - 50) * _Speed)/2, _Power) - bigPass;
            o.Emission = lerp(_Color2, _Color1, saturate(lerpVal));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
