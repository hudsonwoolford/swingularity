Shader "Unlit/Fresnel"
{
    Properties
    {
        [HDR]_MainColor("Main Color", Color) = (1,1,1)
        [HDR]_FresnelColor("Fresnel Color", Color) = (0,0,0)
        _DotCutoff("Dot Cutoff", float) = 0
        _FresnelStrength("Fresnel Strength", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            float _DotCutoff;

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
                float dotP = saturate(abs(dot(i.normal, float3(0,0,-1))));
                float fresnel = (pow((1.0 - dotP), _FresnelStrength));
                
                if(dotP >= _DotCutoff)
                {
                    return _MainColor;
                }
                float4 col = lerp(_MainColor, _FresnelColor, pow(fresnel, _FresnelStrength));
                return col;
            }
            ENDCG
        }
    }
}

