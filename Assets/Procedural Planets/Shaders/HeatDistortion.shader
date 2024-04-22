Shader "Unlit/HeatDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZTest Always
        GrabPass
        {
            "_BackgroundTexture"
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _BackgroundTexture;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = mul(UNITY_MATRIX_MV, float4(v.vertex.x, v.vertex.y, 0, 1));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_BackgroundTexture, i.grabPos.xy / i.grabPos.w);
            }
            ENDCG
        }
    }
}
