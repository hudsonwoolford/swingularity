Shader "Custom/TreeLeafShader"
{
    Properties
    {
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #include "Includes/FractalNoise.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float3 vertPos;
            float normal;
        };

        int _Octaves;
        float3 _Offset;
        float _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain;

        int _WarpOctaves;
        float3 _WarpOffset;
        float _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain;

        float _BiomeA;
        float _BiomeB;
        float _BiomeAEnd;
        float _BiomeBStart;
        float _SwapBiome;

        float3 _TreeColA;
        float3 _TreeColB;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        void vert(inout appdata_full v, out Input o){
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertPos = v.vertex;
            o.normal = v.normal;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float warpNoiseTex = SimpleNoise(IN.vertPos, _WarpOctaves, _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain, _WarpOffset);
            float noiseTex = SimpleNoise(IN.vertPos + warpNoiseTex, _Octaves, _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain, _Offset);

            float blendValue = 0;
            if(noiseTex<_BiomeAEnd) { blendValue = 0; }
            else if(noiseTex>_BiomeBStart){ blendValue = 1; }
            else { 
                float difference = _BiomeBStart - _BiomeAEnd; blendValue = (noiseTex - _BiomeAEnd) / difference; 
            }
            blendValue = (_BiomeA == 1 && _BiomeB == 0) ? 0 : (_BiomeA == 0 && _BiomeB == 1) ? 1 : blendValue;
            if(_BiomeA == 0 && _BiomeB == 0) {o.Albedo = 0.1; return;}
            blendValue = (_SwapBiome == 1) ? 1 - blendValue : blendValue;

            o.Albedo = lerp(_TreeColA, _TreeColB, blendValue);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
