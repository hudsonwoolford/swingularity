Shader "Unlit/GolfBallFresnel"
{
    Properties
    {
        [HDR]_MainColor("Main Color", Color) = (1,1,1)
        [HDR]_FresnelColor("Fresnel Color", Color) = (0,0,0)
        _FresnelStrength("Fresnel Strength", float) = 1
        _FresnelMax("Fresnel Maximum", float) = 1
        _FresnellAlphaFalloff("Fresnel Alpha Falloff", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
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
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 normal : TEXCOORD1;
            };

            fixed4 _MainColor;
            fixed4 _FresnelColor;
            float _FresnelStrength;
            float _FresnelMax;
            float _FresnellAlphaFalloff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //float fresnel = 1-abs(dot(i.normal, float3(0,0,1));
                float fresnelVal = 1 - saturate(abs(dot(i.normal, float3(0,0,-1))));
                float fresnel = pow(fresnelVal, _FresnelStrength);
                
                float4 col = lerp(_MainColor, _FresnelColor, pow(fresnel, _FresnelStrength));
                col.a = saturate(1 - pow(fresnel, _FresnelMax) * _FresnellAlphaFalloff);
                return col;
            }
            ENDCG
            Blend SrcAlpha OneMinusSrcAlpha
        }
    }
}

