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


            // Properties
            sampler2D _Noise;
            sampler2D _StrengthFilter;
            sampler2D _BackgroundTexture;
            float     _Strength;
            float     _Speed;
            float     _Scale;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 texCoord : TEXCOORD0;
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float3 uv : TEXCOORD1;
            };

            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;
                
                // billboard to camera
                float4 pos = input.vertex;
                pos = mul(UNITY_MATRIX_P, 
                      mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1))
                          + float4(pos.x, pos.z, 0, 0) * _Scale);
                output.pos = pos;                

                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                output.grabPos = ComputeGrabScreenPos(output.pos);

                // distort based on noise & strength filter
                float noise = snoise(input.vertex * _Time.y * _Speed);
                float3 filt = tex2Dlod(_StrengthFilter, float4(input.texCoord, 0)).rgb;
                output.grabPos.x += sin(noise) * filt * _Strength / 10;
                output.grabPos.y += cos(noise) * filt * _Strength / 10;

                output.uv = input.texCoord;
                return output;
            }

            float4 frag(vertexOutput input) : COLOR
            {
                //return tex2D(_StrengthFilter, input.uv) * tex2Dproj(_BackgroundTexture, input.grabPos);
                return float4(1,1,1,1); // billboard test
                //return tex2Dproj(_BackgroundTexture, input.grabPos);
            }

            ENDCG
        }

    }
}
