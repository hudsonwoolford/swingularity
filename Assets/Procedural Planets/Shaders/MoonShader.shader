Shader "Unlit/MoonShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexScale ("Main Tex Scale", float) = 10

        _PrimaryColA ("Primary A", Color) = (1,1,1,1)
		_SecondaryColA ("Secondary A", Color) = (1,1,1,1)
		_PrimaryColB ("Primary B", Color) = (1,1,1,1)
		_SecondaryColB ("Secondary B", Color) = (1,1,1,1)
        _HeightMin ("Height Minimum", float) = 1
        _HeightMax ("Height Maximum", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows vertex:vert
            #pragma target 3.5

            #include "UnityCG.cginc"
            #include "Includes/TriplanarMapping.cginc"
            #include "Includes/FractalNoise.cginc"

            struct Input{
                float3 vertexPos;
                float3 normal;
                float4 terrainData;
                float3 worldNormal;
                float4 tangent;
                float4 viewDir;
                INTERNAL_DATA
            };

            sampler2D _MainTex;
            float _MainTexScale;
            float4 _MainTex_ST;
            float _HeightMin;
            float _HeightMax;

            void vert (inout appdata_full v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT (Input, o);
                o.vertexPos = v.vertex;
                o.terrainData = v.texcoord;
                o.normal = v.normal;
                o.tangent = v.tangent;
            }

            void surf (Input IN, inout SurfaceOutputStandard o)
            {
                float4 terrainData = IN.terrainData;
                float height01 = remap01(length(IN.vertexPos), _HeightMin, _HeightMax);

                const float maxSteepnessApprox = 0.3;
                float steepness = 1 - dot(IN.normal, normalize(IN.vertexPos));
                steepness = remap01(steepness, 0, maxSteepnessApprox);

                float warpNoise = Blend(0, 3, IN.terrainData.z);
                float texNoise = triplanar(IN.vertexPos, IN.normal, _MainTexScale, _MainTex);
                
            }
            ENDCG
    }
}
