Shader "Unlit/MoonShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
            CGPROGRAM
            #pragma surface surf Standard fullforwardshadows vertex:vert
            #pragma target 3.5

            #include "UnityCG.cginc"
            #include "TriplanarMapping.cginc"

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
            float4 _MainTex_ST;
            float2 heightMinMax;

            void vert (inout appdata_full v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT (Input, o);
                o.vertexPos = v.vertex;
                o.terrainData = v.texcoord;
                o.normal = v.normal;
                o.tangent = v.tangent;
            }

            void surf (Input IN, inout SurfaceOutputStandard o){
                float4 terrainData = IN.terrainData;
                float height01 = remap01(length(IN.vertexPos), heightMinMax.x, heightMinMax.y);
            }
            ENDCG
    }
}
