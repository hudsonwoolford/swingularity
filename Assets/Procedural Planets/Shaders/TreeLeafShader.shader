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

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert
        #include "Includes/FractalNoise.cginc"
        


        sampler2D _MainTex;

        struct Input
        {
            float3 vertPos;
            float3 normal;
            float3 worldPos;
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
        float _UseTreesA;
        float _UseTreesB;
        float _SeaLevel;

        float _Glossiness;
        float _Metallic;


        void vert(inout appdata_full v, out Input o){
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertPos = v.vertex;
            o.normal = v.normal;
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; 
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 normalizedVertexPos = normalize(IN.worldPos-IN.vertPos) * _SeaLevel;
            float warpNoiseTex = SimpleNoise(normalizedVertexPos, _WarpOctaves, _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain, _WarpOffset); //Sample warp noise
            float noiseTex = SimpleNoise(normalizedVertexPos + warpNoiseTex, _Octaves, _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain, _Offset); //Sample simple noise

            //Define blendValue / biome
            float blendValue = 0;
            if(noiseTex<_BiomeAEnd) { blendValue = 0; }
            else if(noiseTex>_BiomeBStart){ blendValue = 1; }
            else { 
                float difference = _BiomeBStart - _BiomeAEnd; blendValue = (noiseTex - _BiomeAEnd) / difference; 
            }
            blendValue = (_BiomeA == 1 && _BiomeB == 0) ? 0 : (_BiomeA == 0 && _BiomeB == 1) ? 1 : blendValue;
            if(_BiomeA == 0 && _BiomeB == 0) {return;}
            blendValue = (_SwapBiome == 1) ? 1 - blendValue : blendValue;

            if (_UseTreesA == 1 && _UseTreesB == 0){
                o.Albedo = _TreeColA;
                return;
            }
            if (_UseTreesA == 0 && _UseTreesB == 1){
                o.Albedo = _TreeColB;
                return;
            }
            else{
                float3 biomeColor;
                if(blendValue < 0.5)
                {
                    biomeColor = _TreeColA;
                }
                else if(blendValue >= 0.5)
                {
                    biomeColor = _TreeColB;
                }
    
                
                o.Albedo = biomeColor;
            }

            o.Metallic = 0.3;
            o.Smoothness = 0.3;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
