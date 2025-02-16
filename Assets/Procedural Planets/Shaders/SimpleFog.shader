// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/SimpleFog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Range(0,5)) = 0
        _FalloffPower ("Falloff Power", Range(0,5)) = 0
        _FogColor ("Fog Color", color) = (1,1,1,1)
    } 
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Tags { "LightMode" = "Blur Pass" }
            Name "Blur Pass"
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
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, o.vertex);
                o.worldPos.z = 0;
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float2 _MainTex_TexelSize;
            float _Radius;
            float3 _BallPosition;
            float2 _CamPos;
            float2 _CameraSize;
            float _FalloffPower;
            fixed4 _FogColor;

            
            fixed4 frag (v2f i) : SV_Target
            {

                fixed4 col = tex2D(_MainTex, i.uv);
                float2 worldPos = _WorldSpaceCameraPos.xy + float2((i.uv.x-0.5) * _CameraSize.x, (i.uv.y-0.5) * _CameraSize.y);

                float fogIntensity = clamp(pow(max(length(worldPos - _BallPosition.xy) - _Radius, 0), _FalloffPower), 0 , 1);
                
                return lerp(col, _FogColor, fogIntensity);
            }
            ENDCG
        }
    }
}
