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

        [Header(WarpNoise)]
        [IntRange]_WarpOctaves("Warp Octaves", Range(0,5)) = 1
        _WarpOffset("Warp Offset", vector) = (0,0,0)
        _WarpScale("Warp Scale", Range(0,5)) = 1
        _WarpPersistence("Warp Persistence", Range(0, 5)) = 1
        _WarpLacunarity("Warp Lacunarity", Range(0,5)) = 1
        _WarpMultiplier("Warp Multiplier", Range(0,5)) = 1
        _WarpGain("Warp Gain", Range(0, 5)) = 1

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
        float4 _Color1, _Color2;
        int _Octaves;
        float3 _Offset;
        float _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain;
        int _WarpOctaves;
        float3 _WarpOffset;
        float _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain;
        
        float _Power, _Speed;
        float _BigNoise;
        float2 hash(float2 p)
        {
            return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
        }
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float displacement = SimpleNoise(v.normal, 3, 1, 2, 0.5, 0.005, 1, float3(_Time.y,_Time.y,_Time.y)) ;

            v.vertex.xyz = v.vertex.xyz + v.normal * displacement;

            o.vertPos = normalize(v.vertex);
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = 0;
            float bigPass =  pow(snoise(IN.vertPos + _BigNoise), 1);
            float warp = WarpedNoise(IN.vertPos, _WarpOctaves, _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain, _WarpOffset);
            float lerpVal =  pow(SimpleNoise(IN.vertPos + warp, _Octaves, _Scale, _Persistence, _Lacunarity, _Gain, float3(_Time.y + 50, _Time.y, _Time.y - 50) * _Speed)/2, _Power) - bigPass;
            o.Emission = lerp(_Color2, _Color1, saturate(lerpVal));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
