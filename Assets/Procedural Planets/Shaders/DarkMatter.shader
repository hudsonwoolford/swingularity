Shader "Custom/DarkMatter"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _AlphaClip ("Alpha Clip", Range(0, 1)) = 0.5
        _NoiseScale ("Noise Scale", float) = 1
        _NoiseSpeed ("Noise Speed", float) = 1
        _NoisePower ("Noise Power", float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Off
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
                float2 uv_MainTex : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv_MainTex : TEXCOORD0;
                float4 startColor : COLOR;
                float4 vertPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            half _Glossiness;
            half _Metallic;
            fixed4 _Color;
            float _AlphaClip;
            float _NoiseScale;
            float _NoiseSpeed;
            float _NoisePower;

            v2f vert(appdata_full v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv_MainTex = v.uv_MainTex;
                o.startColor = v.color;
                o.vertPos = v.vertex;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 texColor = tex2D(_MainTex, i.uv_MainTex) * i.startColor * _Color;
                // Apply noise effect
                float noise = UnitySimpleNoise(i.vertPos + i.vertPos *(_Time.y+500) * _NoiseSpeed, _NoiseScale);
                texColor.a *= pow(noise, _NoisePower);

                // Apply alpha clipping
                clip(texColor.a - _AlphaClip);

                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
