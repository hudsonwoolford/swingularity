Shader "Unlit/Portal"
{
    Properties
    {
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_MaskTex ("Mask Texture", 2D) = "white" {}
		_Scale ("Scale", float) = 1
		_Strength ("Strength", float) = 1
		_Power ("Power", float) = 1
		_Speed ("Speed", float) = 1
		_AngleOffset ("Angle Offset", float) = 1
		_Centre ("Centre", vector) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #include "Includes/ShaderGraphNodes.cginc"

            sampler2D _MainTex;

            struct appdata_full
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            half _Glossiness;
            half _Metallic;
            float4 _Color;
            float3 _Centre;
            float _Strength;
            float _Power;
            float _Speed;
            float _Scale;
            float _AngleOffset;
            sampler2D _MaskTex;
            float4 _MaskTex_ST;

            v2f vert(appdata_full v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                float timeVal = _Time.y * _Speed;
                float2 voronoiUV = Twirl(i.uv, _Centre.xy, _Strength, float2(timeVal, timeVal));
                float voronoi;
                Unity_Voronoi_float(voronoiUV, _AngleOffset, _Scale, voronoi);
                float poweredVoronoi = pow(voronoi, _Power);
            
                return poweredVoronoi * tex2D(_MaskTex, i.uv) * _Color;
            }
            ENDCG
        }
    }
}
