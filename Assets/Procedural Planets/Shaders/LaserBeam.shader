Shader "Unlit/LaserBeam"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Mask("Mask", 2D) = "white" {}
        [HDR]_Color("Color", Color) = (1,1,1,1)
        _Speed("Speed", vector) = (1,1,0,0)
        _LineSpeedMult("Line Speed Mult", float) = 1
        _AlphaClip("Alpha Clip", Range(0,1)) = 0
        [Header(Distortion)]
        _DistortionAmount("Distortion Amount", Range(0,1)) = 0.5
        _DistortionScale("Distortion Scale", float) = 30
        _DistortionSpeed("Distortion Speed", vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="true" }
        LOD 100
	    Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Includes/ShaderGraphNodes.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 stillUV : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _Mask;
            sampler2D _Mask_ST;
            float4 _MainTex_ST;
            fixed4 _Color;
            float2 _Speed;
            float _AlphaClip;
            float2 _DistortionSpeed;
            float _DistortionAmount;
            float _DistortionScale;
            float _LineSpeedMult;

            v2f vert (appdata v)
            {
                v2f o;
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.stillUV = v.uv;
                _MainTex_ST = float4(_MainTex_ST.x, _MainTex_ST.y, _MainTex_ST.z * _LineSpeedMult, _MainTex_ST.w);

                float3 worldScale = float3(
                length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
                length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
                length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
                );

                float2 noiseScroll = float2(_Time.y * _DistortionSpeed.x, _Time.y * _DistortionSpeed.y);
                float noiseOffset = UnitySimpleNoise(noiseScroll, _DistortionScale);
                float2 distortion = lerp(v.uv, noiseOffset, _DistortionAmount);

                float2 uvOffset = float2(_Time.y * _Speed.x, _Time.y * _Speed.y);
                o.uv = TRANSFORM_TEX(distortion, _MainTex) + uvOffset;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float filter = tex2D(_Mask, i.stillUV).r;

                col*=filter;
                clip(col.a - _AlphaClip);

                return col * _Color * i.color;
            }
            ENDCG
        }
    }
}
