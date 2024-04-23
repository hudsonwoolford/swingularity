Shader "Unlit/BlackHoleDistortionShader"
{
Properties
    {
        _Noise("Noise", 2D) = "white" {}
        _StrengthFilter("Strength Filter", 2D) = "white" {}
        _Strength("Distort Strength", float) = 1.0
        _Speed("Distort Speed", float) = 1.0
        _Power("Noise Power", float) = 1.0
        [IntRange]_Octaves("Noise Octaves", Range(0,5)) = 1
        _Offset("Noise Offset", vector) = (0,0,0)
        _Scale("Noise Scale", float) = 1
        _Persistence("Noise Persistence", Range(0, 5)) = 1
        _Lacunarity("Noise Lacunarity", Range(0,5)) = 1
        _Gain("Noise Gain", Range(0, 5)) = 1
        _TwirlStrength("Twirl Strength", float) = 1
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
            #include "Includes/FractalNoise.cginc"
            #include "Includes/ShaderGraphNodes.cginc"


            // Properties
            sampler2D _Noise;
            sampler2D _StrengthFilter;
            sampler2D _BackgroundTexture;
            float _Strength;
            float _Power;
            float _Speed;
            int _Octaves;
            float3 _Offset;
            float _Scale, _Persistence, _Lacunarity, _Gain;
            float _TwirlStrength;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 texCoord : TEXCOORD0;
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float2 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;
                
                // billboard to camera
                float4 pos = input.vertex;
                output.pos = UnityObjectToClipPos(input.vertex);

                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                output.grabPos = ComputeGrabScreenPos(output.pos);

                // distort based on noise & strength filter
                float2 twirledPos = Twirl(input.texCoord, float2(0.5,0.5), _TwirlStrength, float2(0, 0));

                output.grabPos += twirledPos * _Strength;

                output.uv = input.texCoord;
                return output;
            }

            float4 frag(vertexOutput input) : COLOR
            {
                //return tex2D(_StrengthFilter, input.uv) * tex2Dproj(_BackgroundTexture, input.grabPos);
                // return float4(1,1,1,1); // billboard test
                return tex2D(_BackgroundTexture, input.grabPos);
            }

            ENDCG
        }

    }
}

// float noise = SimpleNoise(float3(twirledPos.x, twirledPos.y, 0), _Octaves, _Scale, _Persistence, _Lacunarity, _Gain, _Offset);
// float poweredNoise = pow(noise, _Power);
// float3 filt = tex2Dlod(_StrengthFilter, input.vertex).rgb;
