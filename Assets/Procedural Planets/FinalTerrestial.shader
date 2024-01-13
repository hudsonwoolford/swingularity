Shader "Custom/FinalTerrestial"
{
    Properties
    {
     [Header(Parameters)] //Parameters
     [Toggle()]_BiomeA("BiomeA", float) = 1
     [Toggle()]_BiomeB("BiomeB", float) = 1
     _BiomeAEnd("Biome A End", Range(0,1)) = 0.4
     _BiomeBStart("Biome B  Start", Range(0,1)) = 0.6
     [Toggle()]_SwapBiome("SwapBiome",float) = 0
     _SteepMultiplier("Steep Multiplier", Range(0,5)) = 1

     [Header(Biome A)] //Biome A

     _FlatColorA ("Flat Color A", Color) = (1,1,1)
     _SteepColorA ("Steep Color A", Color) = (1,1,1)
     _ShorelineColorA ("Shoreline Color A", Color) = (1,1,1)
     _HighElevationColorA ("High Elevation Color A", Color) = (1,1,1) 
     _HighElevationSteepColorA ("High Elevation Steep Color A", Color) = (1,1,1) 

     _MetallicA ("Metallic A", Range(0, 1)) = 0
     _SmoothnessA ("Smoothness A", Range(0,1)) = 0

     _ShorelineA("Shoreline Elevation A", Range(0, 0.2)) = 0.01
     _VegetationLineA ("Vegetation Line A", Range(0, 0.5)) = 0.1
     [PowerSlider(3.0)]_VegetationLineHarshnessA ("Vegetation Line Harshness A", Range(0, 10)) = 2

     [Header(Biome B)] //Biome B

     _FlatColorB ("Flat Color B", Color) = (0,0,0)
     _SteepColorB ("Steep Color B", Color) = (0,0,0)
     _ShorelineColorB ("Shoreline Color B", Color) = (0,0,0)
     _HighElevationColorB ("High Elevation Color B", Color) = (0,0,0)
     _HighElevationSteepColorB ("High Elevation Steep Color B", Color) = (0,0,0)

     _MetallicB ("Metallic B", Range(0, 1)) = 0
     _SmoothnessB ("Smoothness B", Range(0,1)) = 0
     
     _ShorelineB("Shoreline Elevation A", Range(0, 0.2)) = 0.01
     _VegetationLineB ("Vegetation Line B", Range(0, 0.5)) = 0.1
     [PowerSlider(3.0)]_VegetationLineHarshnessB ("Vegetation Line Harshness B", Range(0, 10)) = 2

     [Header(Constants)] //Constants

     _SeaLevel ("Sea Level", Range(0, 2)) = 1
     _OceanBlendEnd("Ocean Blend End", Range(0, 2)) = 0.5
     [PowerSlider(3.0)]_OceanBlendIntensity("Ocean Blend Intensity", Range(0, 10)) = 1
     _SeabedColor ("Sea Bed Color", Color) = (1,1,1)

     [Header(Fresnel)] //Fresnel
     [Toggle()]_UseFresnel("UseFresnel", float) = 0
     _FresnelColor("Fresnel Color", Color) = (1,1,1)
     _FresnelStrengthNear("Fresnel Strength Near", float) = 2
     _FresnelStrengthFar("Fresnel Strength Far", float) = 5
     _FresnelPower("Fresnel Power", float) = 2

     [Header(Noise)] //Noise

     [IntRange]_Octaves("Octaves", Range(0,5)) = 1
     _Offset("Offset", vector) = (0,0,0)
     _Scale("Scale", Range(0,5)) = 1
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
        Tags
        {
            "RenderType" = "Opaque"
        }

        CGPROGRAM
        #pragma surface surf Standard vertex:vert
        #include "FractalNoise.cginc"

        float bodyScale;
        //Parameters
        float _BiomeA;
        float _BiomeB;
        float _BiomeBStart;
        float _BiomeAEnd;
        float _SwapBiome;
        float _SteepMultiplier;
        //Biome A
        fixed4 _FlatColorA;
        fixed4 _SteepColorA;
        fixed4 _ShorelineColorA;
        fixed4 _HighElevationColorA;
        fixed4 _HighElevationSteepColorA;
        float _ShorelineA;
        float _VegetationLineA;
        float _VegetationLineHarshnessA;
        //Biome B
        fixed4 _FlatColorB;
        fixed4 _SteepColorB;
        fixed4 _ShorelineColorB;
        fixed4 _HighElevationColorB;
        fixed4 _HighElevationSteepColorB;
        float _ShorelineB;
        float _VegetationLineB;
        float _VegetationLineHarshnessB;
        //Constant through biomes
        float _SeaLevel;
        float _OceanBlendEnd;
        float _OceanBlendIntensity;
        fixed4 _SeabedColor;

        float _MetallicA;
        float _MetallicB;
        float _SmoothnessA;
        float _SmoothnessB;

        float _UseFresnel;
        fixed4 _FresnelColor;
        float _FresnelStrengthNear;
        float _FresnelStrengthFar;
        float _FresnelPower;

        int _Octaves;
        float3 _Offset;
        float _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain;

        int _WarpOctaves;
        float3 _WarpOffset;
        float _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain;

        struct Input
        {
            float3 worldPos;
            float3 vertPos;
            float3 normal;
            float fresnel;
        };

        void vert (inout appdata_full v, out Input o)
        {
            
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertPos = v.vertex;
            o.normal = v.normal;


            //Fresnel (fade out when close to body)
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 relCameraPos = float3(worldPos.xy, _WorldSpaceCameraPos.z);
            float3 bodyWorldCenter = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
            float camRadiiFromSurface = (length(bodyWorldCenter - relCameraPos) - bodyScale) / bodyScale;
            float fresnelT = smoothstep(0,1,camRadiiFromSurface);
            float3 viewDir = normalize(worldPos - relCameraPos);
            float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal,0)));
            float fresStrength = lerp(_FresnelStrengthNear, _FresnelStrengthFar, fresnelT);
            o.fresnel = saturate(fresStrength * pow(1 + dot(viewDir, normWorld), _FresnelPower));
        }


        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 sphereNormal = normalize(IN.vertPos); //a perfect sphere normal
            float seaLevelHeight = length(_SeaLevel*sphereNormal); //height of sealevel from middle of the sphere
            float elevation = length(IN.vertPos) - seaLevelHeight; //elevation of vert above sea level

            float warpNoiseTex = SimpleNoise(IN.vertPos, _WarpOctaves, _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain, _WarpOffset);
            float noiseTex = SimpleNoise(IN.vertPos + warpNoiseTex, _Octaves, _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain, _Offset);

            //Define blendValue
            float blendValue = 0;
            if(noiseTex<_BiomeAEnd) { blendValue = 0; }
            else if(noiseTex>_BiomeBStart){ blendValue = 1; }
            else { 
                float difference = _BiomeBStart - _BiomeAEnd; blendValue = (noiseTex - _BiomeAEnd) / difference; 
            }
            blendValue = (_BiomeA == 1 && _BiomeB == 0) ? 0 : (_BiomeA == 0 && _BiomeB == 1) ? 1 : blendValue;
            if(_BiomeA == 0 && _BiomeB == 0) {o.Albedo = 0.1; return;}
            blendValue = (_SwapBiome == 1) ? 1 - blendValue : blendValue;

            //Define new values
            float3 flatColor = lerp(_FlatColorA, _FlatColorB, blendValue);
            float3 steepColor = lerp(_SteepColorA, _SteepColorB, blendValue);
            float3 highElevationColor = lerp(_HighElevationColorA, _HighElevationColorB, blendValue);
            float3 highElevationSteepColor = lerp(_HighElevationSteepColorA, _HighElevationSteepColorB, blendValue);
            float3 shorelineColor = lerp(_ShorelineColorA, _ShorelineColorB, blendValue);
            float shoreline = lerp(_ShorelineA, _ShorelineB, blendValue);
            float vegetationLine = lerp(_VegetationLineA, _VegetationLineB, blendValue);
            float vegetationLineHarshness = lerp(_VegetationLineHarshnessA, _VegetationLineHarshnessB, blendValue);
            float metallic = lerp(_MetallicA, _MetallicB, blendValue);
            float smoothness = lerp(_SmoothnessA, _SmoothnessB, blendValue);

            //Terrain steepness
            float steepness = 1 - dot(sphereNormal, IN.normal); //dot product / how close is the sphere normal to the vertex normal
            float elevationFloat = clamp(0, 1, pow(abs(elevation) / vegetationLine, vegetationLineHarshness));
            float3 correctSteepColor = lerp(steepColor, highElevationSteepColor, elevationFloat);
            float3 correctFlatColor = lerp(flatColor, highElevationColor, elevationFloat);
            float3 groundColor = lerp(correctFlatColor, correctSteepColor, steepness * _SteepMultiplier); //lerps the flat and steep color depending on steepness

            //Terrain elevation
            if(elevation < shoreline) 
            { 
                float groundLerp = clamp(0, 1, pow(abs(elevation) / _OceanBlendEnd, _OceanBlendIntensity));
                float3 groundColor = lerp(shorelineColor, _SeabedColor, groundLerp);
                o.Albedo = groundColor;
                return;
            }
            
            
            //Compound colors
            float3 compoundColor = lerp(groundColor, _FresnelColor, IN.fresnel * _UseFresnel);
            o.Albedo = compoundColor;
            o.Metallic = metallic;
            o.Smoothness = smoothness;
            
        }

        ENDCG
    }
    FallBack "Diffuse"
}
