
Shader "Unlit/TerrestialSurface"
{
    Properties
    {
     _SteepMultiplier("Steep Multiplier", Range(0,5)) = 1
     _FlatColor ("Flat Color", Color) = (1,1,1)
     _SteepColor ("Steep Color", Color) = (1,1,1)
     _SeaLevel ("Sea Level", Range(0, 2)) = 1
     _VegetationLine ("Vegetation Line", Range(0, 0.5)) = 0.1
     _VegetationLineHarshness ("Vegetation Line Harshness", Range(0, 40)) = 2
     _HighElevationColor ("High Elevation Color", Color) = (1,1,1) 
     _SeabedColor ("Sea Bed Color", Color) = (1,1,1)
     _DeepSea ("Deep Sea", Color) = (1,1,1)
     _DeepSeaDepth ("Deep Sea Depth", Range(0, 0.5)) = 1
     _ShallowSea ("ShallowSea", Color) = (1,1,1)
     _SeaOpacity ("Sea Opacity", Range(0,1)) = 0.25
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        

        float _SteepMultiplier;
        fixed4 _FlatColor;
        fixed4 _SteepColor;
        float _SeaLevel;
        float _VegetationLine;
        float _VegetationLineHarshness;
        fixed4 _HighElevationColor;
        fixed4 _SeabedColor;
        fixed4 _ShallowSea;
        fixed4 _DeepSea;
        float _DeepSeaDepth;
        float _SeaOpacity;

        struct Input
        {
            float3 worldPos;
            float3 vertPos;
            float3 normal;
        };

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertPos = v.vertex;
            o.normal = v.normal;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            //Terrain steepness
            float3 sphereNormal = normalize(IN.vertPos); //a perfect sphere normal
            float steepness = 1- dot(sphereNormal, IN.normal); //dot product / how close is the sphere normal to the vertex normal
            float3 steepColor = lerp(_FlatColor, _SteepColor, steepness * _SteepMultiplier); //lerps the flat and steep color depending on steepness
            //Terrain elevation
            float seaLevelHeight = length(_SeaLevel*sphereNormal);
            float elevation = length(IN.vertPos) - seaLevelHeight; //elevation of vert above sea level
            if(elevation < 0) { 
            //Ocean
                float3 oceanLeveledPos = sphereNormal * seaLevelHeight;
                float oceanDepth = length(oceanLeveledPos - IN.vertPos);
                IN.vertPos = oceanLeveledPos;
                o.Albedo = _SeabedColor;
                return;
            }
            elevation = clamp(0, _VegetationLine, elevation);
            //float3 elevationColor = steepColor;
            //if(elevation >= _VegetationLine) {elevationColor = _HighElevationColor;} 
            float3 elevationColor = lerp(steepColor, _HighElevationColor, pow(elevation / _VegetationLine, _VegetationLineHarshness));
            

            //Compound colors
            float3 compoundColor = lerp(steepColor, elevationColor, 1);

            o.Albedo = compoundColor;
            
        }

        ENDCG
    }
    FallBack "Diffuse"
}
