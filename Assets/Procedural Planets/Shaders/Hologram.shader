// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Hologram"
{
    Properties
    {
        _HoloTexture ("Holo Texture", 2D) = "white" {}
        _FresnelPower ("Fresnel Power", float) = 2
        _OffsetSpeed ("Offset Speed", vector) = (0,-1,0,1)
        _FlickerSpeed ("Flicker Speed", Range(0,0.2)) = 0.2
        _FlickerDampening ("Flicker Dampening", vector) = (0.3, 0.6, 0, 1)
        [HDR]_Color ("Color", color) = (1,1,1,1)
        [HDR]_FresnelColor ("_FresnelColor", color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector" = "True"}
        Blend SrcAlpha OneMinusSrcAlpha
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            sampler2D _HoloTexture;
            float4 _HoloTexture_ST;
            fixed4 _Color;
            fixed4 _FresnelColor;
            float4 _OffsetSpeed;
            float _FresnelPower;
            float _FlickerSpeed;
            float _UnscaledTime;
            float4 _FlickerDampening;


            float random (float2 uv)
            {
                return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
            }
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
                o.uv = TRANSFORM_TEX(o.uv, _HoloTexture) + _OffsetSpeed * _UnscaledTime;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float fresnel = saturate(abs(dot(i.normal, float3(0,0,1))));
                fresnel = 1-pow(fresnel, _FresnelPower);
                
                // sample the texture
                float4 mainCol = tex2D(_HoloTexture, i.uv) * _Color;
                mainCol.a = tex2D(_HoloTexture, i.uv).r;
                float4 finalCol = lerp(mainCol, _FresnelColor, fresnel);

                finalCol.a *= clamp(random(float2(_UnscaledTime, _UnscaledTime) * _FlickerSpeed), _FlickerDampening.x, _FlickerDampening.y);
                
                return finalCol;
            }
            ENDCG
        }
    }
}
