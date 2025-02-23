Shader "Custom/ParticleFog"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _FadeRadius ("Fade Radius", Float) = 1.0
        _FadeSmoothness ("Fade Smoothness", Float) = 1.0
        _FalloffPower ("Falloff Power", Float) = 1.0
        //_ObjectPos ("Object Position", Vector) = (0,0,0,0)
        _MinAlpha ("Min Alpha", Range(0,1)) = 0.01
        _NoiseScale("Noise Scale", float) = 1
        _NoiseSpeed("Noise Speed", float) = 1
        _NoiseStrength("Noise Strength", float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Zwrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Includes/ShaderGraphNodes.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1; // Store world position
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _FadeRadius;
            float _FadeSmoothness;
            float _FalloffPower;
            float4 _ObjectPos[2];
            float _MinAlpha;
            float _NoiseScale;
            float _NoiseSpeed;
            float _NoiseStrength;


            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // Convert to world space
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Example: encode world position into color channels (not for actual use)
                float noise = UnitySimpleNoise(i.worldPos.xy + _Time.xy * _NoiseSpeed, _NoiseScale) * _NoiseStrength;

                float dist = 1000;
                for(uint u = 0; u < _ObjectPos.Length; u++)
                {
                    dist = min(distance(i.worldPos.xy, _ObjectPos[u].xy), dist);
                }

                float fade = saturate(pow(max((dist - _FadeRadius * noise)/(_FadeSmoothness), _MinAlpha), _FalloffPower));
                col.a = col.a * fade; // Normalize if needed
                return col;
            }
            ENDCG
        }
    }
}
