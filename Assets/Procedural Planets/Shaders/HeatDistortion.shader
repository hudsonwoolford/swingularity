Shader "Unlit/HeatDistortion"
{
Properties
    {
        _Noise("Noise", 2D) = "white" {}
        _StrengthFilter("Strength Filter", 2D) = "white" {}
        _Strength("Distort Strength", float) = 1.0
        _Speed("Distort Speed", float) = 1.0
    }

    SubShader
    {
        Tags 
        {
            "Queue" = "Transparent"
            "DisableBatching" = "True"
        }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

        // Render the object with the texture generated above, and invert the colors
        Pass
        {

            CGPROGRAM
            #pragma vertex vert 
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Includes/SimplexNoise.cginc"
            #include "Includes/ShaderGraphNodes.cginc"


            // Properties
            sampler2D _Noise;
            sampler2D _StrengthFilter;
            sampler2D _BackgroundTexture;
            float     _Strength;
            float     _Speed;
            float     _Scale;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 texCoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float3 uv : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texCoord;
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }
 
            half4 frag (v2f i) : SV_Target
            {
 
                float2 grabUV = i.grabPos.xy / i.grabPos.w;

                float filt = tex2D(_StrengthFilter, i.uv).r;
                grabUV = grabUV;
 
                half4 col = tex2D(_BackgroundTexture, grabUV);
                return col;
            }

            ENDCG
        }

    }
}
