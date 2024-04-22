Shader "Custom/FinalTerrestrial"
{
    Properties
    {

     [Header(Parameters)] //Parameters
     [Toggle()]_Ocean("Ocean", float) = 1
     [Toggle()]_BiomeA("BiomeA", float) = 1
     [Toggle()]_BiomeB("BiomeB", float) = 1
     _BiomeAEnd("Biome A End", Range(0,1)) = 0.4
     _BiomeBStart("Biome B  Start", Range(0,1)) = 0.6
     [Toggle()]_SwapBiome("SwapBiome",float) = 0

     [Header(Biome A)] //Biome A

     _FlatColorA ("Flat Color A", Color) = (0.2,0.2,0.2)
     _FlatColorAVariant ("Flat Color A Variant", Color) = (0.2,0.2,0.2)
     _SteepColorA ("Steep Color A", Color) = (0.2,0.2,0.2)
     _HighElevationColorA ("High Elevation Color A", Color) = (0.2,0.2,0.2)
     _HighElevationSteepColorA ("High Elevation Steep Color A", Color) = (0.2,0.2,0.2)

     _MetallicA ("Metallic A", Range(0, 1)) = 0
     _SmoothnessA ("Smoothness A", Range(0,1)) = 0

     _VegetationLineA ("Vegetation Line A", Range(0, 0.5)) = 0.1
     [PowerSlider(3.0)]_VegetationLineHarshnessA ("Vegetation Line Harshness A", Range(0, 10)) = 2

     _SteepMultiplierA("Steep Multiplier A", Range(0, 5)) = 1

     [Header(Biome B)] //Biome B

     _FlatColorB ("Flat Color B", Color) = (0.5,0.5,0.5)
     _FlatColorBVariant ("Flat Color B Variant", Color) = (0.5,0.5,0.5)
     _SteepColorB ("Steep Color B", Color) = (0.5,0.5,0.5)
     _HighElevationColorB ("High Elevation Color B", Color) = (0.5,0.5,0.5)
     _HighElevationSteepColorB ("High Elevation Steep Color B", Color) = (0.5,0.5,0.5)

     _MetallicB ("Metallic B", Range(0, 1)) = 0
     _SmoothnessB ("Smoothness B", Range(0,1)) = 0
     
     _VegetationLineB ("Vegetation Line B", Range(0, 0.5)) = 0.1
     [PowerSlider(3.0)]_VegetationLineHarshnessB ("Vegetation Line Harshness B", Range(0, 10)) = 2
     _SteepMultiplierB("Steep Multiplier B", Range(0, 5)) = 1

     [Header(Polar Biome)] //Biome P

     _FlatColorP ("Flat Color P", Color) = (0.5,0.5,0.5)
     _FlatColorPVariant ("Flat Color P Variant", Color) = (0.5,0.5,0.5)
     _SteepColorP ("Steep Color P", Color) = (0.5,0.5,0.5)
     _HighElevationColorP ("High Elevation Color P", Color) = (0.5,0.5,0.5)
     _HighElevationSteepColorP ("High Elevation Steep Color P", Color) = (0.5,0.5,0.5)

     _MetallicP ("Metallic P", Range(0, 1)) = 0
     _SmoothnessP ("Smoothness P", Range(0,1)) = 0
     
     _VegetationLineP ("Vegetation Line P", Range(0, 0.5)) = 0.1
     [PowerSlider(3.0)]_VegetationLineHarshnessP ("Vegetation Line Harshness P", Range(0, 10)) = 2
     _SteepMultiplierP("Steep Multiplier P", Range(0, 5)) = 1

     [Header(Polar Variables)]
     [Toggle()]_Polar ("Polar", float) = 1
     _PolarHeight ("Polar Height", Range(0,1.5)) = 1
     [PowerSlider(3.0)]_PolarBlendIntensity ("Polar Blend Intensity", Range(0,100)) = 5

     [Header(Flat Terrain Noise)]
     [IntRange]_ColorOctaves("Color Texture Octaves", Range(0,5)) = 1
     _ColorOffset("Color Texture Offset", vector) = (0,0,0)
     [PowerSlider(10.0)]_ColorTextureScale("Color Texture Scale", Range(0, 100)) = 5
     _ColorPersistence("Color Texture Persistence", Range(0, 5)) = 1
     _ColorLacunarity("Color Texture Lacunarity", Range(0,5)) = 1
     _ColorMultiplier("Color Texture Multiplier", Range(0,5)) = 1
     _ColorGain("Color Texture Gain", Range(0, 5)) = 1


     [Header(Constants)] //Constants

     _SeaLevel ("Sea Level", Range(0, 2)) = 1
     _OceanBlendEnd("Ocean Blend End", Range(0, 2)) = 0.5
     _BeachBlendEnd("Beach Blend End", Range(0, 0.05)) = 0.5
     [PowerSlider(3.0)]_OceanBlendIntensity("Ocean Blend Intensity", Range(0, 10)) = 1
     [PowerSlider(3.0)]_BeachBlendIntensity("Beach Blend Intensity", Range(0, 10)) = 1
     _ShorelineColorA ("Shoreline Color A", Color) = (1,1,1)
     _ShorelineColorB ("Shoreline Color B", Color) = (1,1,1)
     _ShorelineColorP ("Shoreline Color P", Color) = (1,1,1)
     _SeabedColor ("Sea Bed Color", Color) = (0,0,0)
     //_OceanColor ("Ocean Color", Color) = (1,1,1)
     //_OceanMetallic ("Ocean Metallic", Range(0 ,1)) = 0
     //_OceanSmoothness ("Ocean Smoothness", Range(0, 1)) = 0

     [Header(Fresnel)] //Fresnel
     [Toggle(_UseFresnel)]_UseFresnel("UseFresnel", float) = 0
     _FresnelColor("Fresnel Color", Color) = (1,1,1)
     _FresnelStrengthNear("Fresnel Strength Near", float) = 2
     _FresnelStrengthFar("Fresnel Strength Far", float) = 5
     _FresnelPower("Fresnel Power", float) = 2
    
     [Header(Normals)]

     _NormalTextureA("Normal Texture A", 2D) = "white" {}
     _NormalOffsetA("Normal Offset A", vector) = (1,1,1)
     _NormalPowerA("Normal Power A", float) = 1
     _NormalScaleA("Normal Scale A", float) = 1

     _NormalTextureB("Normal Texture B", 2D) = "white" {}
     _NormalOffsetB("Normal Offset B", vector) = (1,1,1)
     _NormalPowerB("Normal Power B", float) = 1
     _NormalScaleB("Normal Scale B", float) = 1

     _NormalTextureP("Normal Texture P", 2D) = "white" {}
     _NormalOffsetP("Normal Offset P", vector) = (1,1,1)
     _NormalPowerP("Normal Power P", float) = 1
     _NormalScaleP("Normal Scale P", float) = 1
     bodyScale("Body Scale", float) = 2.23
     [Header(Noise)] //Noise

     [HideInInspector][IntRange]_Octaves("Octaves", Range(0,5)) = 1
     [HideInInspector]_Offset("Offset", vector) = (0,0,0)
     [HideInInspector]_Scale("Scale", Range(0,5)) = 1
     [HideInInspector]_Persistence("Persistence", Range(0, 5)) = 1
     [HideInInspector]_Lacunarity("Lacunarity", Range(0,5)) = 1
     [HideInInspector]_Multiplier("Multiplier", Range(0,5)) = 1
     [HideInInspector]_Gain("Gain", Range(0, 5)) = 1
     //[HideInInspector][Header(WarpNoise)]
     [HideInInspector][IntRange]_WarpOctaves("Warp Octaves", Range(0,5)) = 1
     [HideInInspector]_WarpOffset("Warp Offset", vector) = (0,0,0)
     [HideInInspector]_WarpScale("Warp Scale", Range(0,5)) = 1
     [HideInInspector]_WarpPersistence("Warp Persistence", Range(0, 5)) = 1
     [HideInInspector]_WarpLacunarity("Warp Lacunarity", Range(0,5)) = 1
     [HideInInspector]_WarpMultiplier("Warp Multiplier", Range(0,5)) = 1
     [HideInInspector]_WarpGain("Warp Gain", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        LOD 200
        ZTest Off
        CGPROGRAM
        #pragma target 3.5
        #pragma surface surf Standard vertex:vert 
        #include "Includes/FractalNoise.cginc"
        #include "Includes/Flow.cginc"
        #include "Includes/TriplanarNormals.cginc"

        sampler2D _NormalTextureA;
        sampler2D _NormalTextureB;
        sampler2D _NormalTextureP;
        float3 _NormalOffsetA;
        float3 _NormalOffsetB;
        float3 _NormalOffsetP;
        float _NormalScaleA;
        float _NormalScaleB;
        float _NormalScaleP;
        float _NormalPowerA;
        float _NormalPowerB;
        float _NormalPowerP;
        float bodyScale;
        //Parameters
        float _Ocean;
        float _BiomeA;
        float _BiomeB;
        float _BiomeBStart;
        float _BiomeAEnd;
        float _SwapBiome;
        float _ColorTextureScale;
        //Biome A
        fixed4 _FlatColorA;
        fixed4 _FlatColorAVariant;
        fixed4 _SteepColorA;
        fixed4 _HighElevationColorA;
        fixed4 _HighElevationSteepColorA;
        float _VegetationLineA;
        float _VegetationLineHarshnessA;
        float _SteepMultiplierA;
        //Biome B
        fixed4 _FlatColorB;
        fixed4 _FlatColorBVariant;
        fixed4 _SteepColorB;
        fixed4 _HighElevationColorB;
        fixed4 _HighElevationSteepColorB;
        float _VegetationLineB;
        float _VegetationLineHarshnessB;
        float _SteepMultiplierB;
        //Biome P
        fixed4 _FlatColorP;
        fixed4 _FlatColorPVariant;
        fixed4 _SteepColorP;
        fixed4 _HighElevationColorP;
        fixed4 _HighElevationSteepColorP;
        float _VegetationLineP;
        float _VegetationLineHarshnessP;
        float _SteepMultiplierP;
        //Polar Variables
        float _Polar;
        float _PolarHeight;
        float _PolarBlendIntensity;
        //Constant through biomes
        float _SeaLevel = 1;
        float _BeachBlendEnd;
        float _OceanBlendEnd;
        float _OceanBlendIntensity;
        float _BeachBlendIntensity;
        fixed4 _ShorelineColorA;
        fixed4 _ShorelineColorB;
        fixed4 _ShorelineColorP;
        fixed4 _SeabedColor;

        float _MetallicA;
        float _MetallicB;
        float _MetallicP;
        float _SmoothnessA;
        float _SmoothnessB;
        float _SmoothnessP;

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

        int _ColorOctaves;
        float3 _ColorOffset;
        float _ColorPersistence, _ColorLacunarity, _ColorMultiplier, _ColorGain;
        
        struct Input
        {
            float2 uv_MainTex;
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
            float3 normalizedVertexPos = sphereNormal * bodyScale;
            float seaLevelHeight = length(_SeaLevel*sphereNormal); //height of sealevel from middle of the sphere
            float elevation = length(IN.vertPos) - seaLevelHeight; //elevation of vert above sea level

            float warpNoiseTex = SimpleNoise(normalizedVertexPos, _WarpOctaves, _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain, _WarpOffset);
            float noiseTex = SimpleNoise(normalizedVertexPos + warpNoiseTex, _Octaves, _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain, _Offset);
            //Define blendValue / biome
            float blendValue = 0;
            if(noiseTex<_BiomeAEnd) { blendValue = 0; }
            else if(noiseTex>_BiomeBStart){ blendValue = 1; }
            else { 
                float difference = _BiomeBStart - _BiomeAEnd; blendValue = (noiseTex - _BiomeAEnd) / difference; 
            }
            blendValue = (_BiomeA == 1 && _BiomeB == 0) ? 0 : (_BiomeA == 0 && _BiomeB == 1) ? 1 : blendValue;
            if(_BiomeA == 0 && _BiomeB == 0) {o.Albedo = 0.1; return;}
            blendValue = (_SwapBiome == 1) ? 1 - blendValue : blendValue;

            //Define new values depending on biome
            float3 flatColor = lerp(_FlatColorA, _FlatColorB, blendValue);
            float3 flatColorVariant = lerp(_FlatColorAVariant, _FlatColorBVariant, blendValue);
            float3 steepColor = lerp(_SteepColorA, _SteepColorB, blendValue);
            float3 highElevationColor = lerp(_HighElevationColorA, _HighElevationColorB, blendValue);
            float3 highElevationSteepColor = lerp(_HighElevationSteepColorA, _HighElevationSteepColorB, blendValue);
            float3 shorelineColor = lerp(_ShorelineColorA, _ShorelineColorB, blendValue);
            float vegetationLine = lerp(_VegetationLineA, _VegetationLineB, blendValue);
            float vegetationLineHarshness = lerp(_VegetationLineHarshnessA, _VegetationLineHarshnessB, blendValue);
            float steepnessMultiplier = lerp(_SteepMultiplierA, _SteepMultiplierB, blendValue);
            float metallic = lerp(_MetallicA, _MetallicB, blendValue);
            float smoothness = lerp(_SmoothnessA, _SmoothnessB, blendValue);
            
            float yDist = abs(IN.vertPos.y); //y value of vertex

            float polarBlendValue = clamp(0,1,pow(yDist/_PolarHeight, _PolarBlendIntensity)) * _Polar; //blend value for lerping to polar biome values
            //Define new values for polar biome
            flatColor = lerp(flatColor, _FlatColorP, polarBlendValue);
            flatColorVariant = lerp(flatColorVariant, _FlatColorPVariant, polarBlendValue);
            steepColor = lerp(steepColor, _SteepColorP, polarBlendValue);
            highElevationColor = lerp(highElevationColor, _HighElevationColorP, polarBlendValue);
            highElevationSteepColor = lerp(highElevationSteepColor, _HighElevationSteepColorP, polarBlendValue);
            shorelineColor = lerp(shorelineColor, _ShorelineColorP, polarBlendValue);
            vegetationLine = lerp(vegetationLine, _VegetationLineP, polarBlendValue);
            vegetationLineHarshness = lerp(vegetationLineHarshness, _VegetationLineHarshnessP, polarBlendValue);
            steepnessMultiplier = lerp(steepnessMultiplier, _SteepMultiplierP, polarBlendValue);
            metallic = lerp(metallic, _MetallicP, polarBlendValue);
            smoothness = lerp(smoothness, _SmoothnessP, polarBlendValue);

            //Flat terrain noise
            noiseTex = saturate(SimpleNoise(IN.vertPos, _ColorOctaves, _ColorTextureScale, _ColorPersistence, _ColorLacunarity, _ColorMultiplier, _ColorGain, _ColorOffset));
            flatColor = lerp(flatColor, flatColorVariant, noiseTex);

            //Terrain steepness
            float steepness = clamp(0,1,(1 - dot(sphereNormal, IN.normal)) * steepnessMultiplier); //dot product / how close is the sphere normal to the vertex normal

            float elevationFloat = clamp(0, 1, pow(abs(elevation) / vegetationLine, vegetationLineHarshness));
            float3 correctSteepColor = lerp(steepColor, highElevationSteepColor, elevationFloat);
            float3 correctFlatColor = lerp(flatColor, highElevationColor, elevationFloat);
            float3 groundColor = lerp(correctFlatColor, correctSteepColor, steepness); //lerps the flat and steep color depending on steepness
            //If lower than ocean
            if(elevation < 0 && _Ocean == 1) 
            {             

                float seabedLerp = clamp(0, 1, pow(abs(elevation) / _OceanBlendEnd, _OceanBlendIntensity));
                float3 seabedColor = lerp(shorelineColor, _SeabedColor, seabedLerp);
                o.Albedo = seabedColor;
                o.Metallic = metallic;
                o.Smoothness = smoothness;

                return;
            }
            
            float beachLerp = clamp(0, 1, pow(abs(elevation) / _BeachBlendEnd, _BeachBlendIntensity));
            
            // Interpolate between shoreline and normal terrain color
            float3 finalColor = lerp(shorelineColor, groundColor, beachLerp);


            //Compound colors
            float3 compoundColor = lerp(finalColor, _FresnelColor, IN.fresnel * _UseFresnel);
            o.Albedo = compoundColor;


            TriplanarUV triUVA = GetTriplanarUV(IN.vertPos, _NormalOffsetA, _NormalScaleA);
            TriplanarUV triUVB = GetTriplanarUV(IN.vertPos, _NormalOffsetB, _NormalScaleB);
            TriplanarUV triUVP = GetTriplanarUV(IN.vertPos, _NormalOffsetP, _NormalScaleP);
            float3 ANormals = GetMappedTriplanarNormal(_NormalTextureA, triUVA, _NormalPowerA, IN.normal);
            float3 BNormals = GetMappedTriplanarNormal(_NormalTextureB, triUVB, _NormalPowerB, IN.normal);
            float3 PNormals = GetMappedTriplanarNormal(_NormalTextureP, triUVP, _NormalPowerP, IN.normal);
            float planetNormal = lerp(ANormals, BNormals, blendValue);
            o.Normal += lerp(planetNormal, PNormals, polarBlendValue);

            
            o.Metallic = metallic;
            o.Smoothness = smoothness;
        }

        ENDCG
    }
    FallBack "Diffuse"
}
