Shader "Custom/Portal"
{
    Properties
    {
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_MaskTex ("Mask Texture", 2D) = "white" {}
		_Scale ("Scale", float) = 1
		_Strength ("Strength", float) = 1
		_Power ("Power", float) = 1
		_Speed ("Speed", float) = 1
		_Centre ("Centre", vector) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 200
        Blend SrcAlpha One
        CGPROGRAM


        #pragma surface surf Standard fullforwardshadows alpha:blend
        #pragma target 3.0
        #include "Includes/ShaderGraphNodes.cginc"
        struct Input
        {
            float2 uv : TEXCOORD0;
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        float4 _Color;
        float3 _Centre;
        float _Strength;
        float _Power;
        float _Speed;
        float _Scale;
        sampler2D _MaskTex;
        float4 _MaskTex_ST;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {   
            float timeVal = _Time.y * _Speed;
            float2 voronoiUV = Twirl(IN.uv_MainTex, _Centre.xy, _Strength, float2(timeVal, timeVal));
            float voronoi = Voronoi(voronoiUV, 0, _Scale);
            float poweredVoronoi = pow(voronoi, _Power);
            
            o.Albedo = poweredVoronoi * tex2D(_MaskTex, IN.uv_MainTex) * _Color;
            o.Alpha = poweredVoronoi * tex2D(_MaskTex, IN.uv_MainTex).a * _Color;
        }
        ENDCG
    }
}
