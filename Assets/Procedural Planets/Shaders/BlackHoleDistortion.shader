Shader "Unlit/BlackHoleDistortionShader"
{
Properties
    {
        _Noise("Noise", 2D) = "white" {}
        _StrengthFilter("Strength Filter", 2D) = "white" {}
        _Strength("Distort Strength", float) = 1.0
        _Speed("Speed", float) = 1.0
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
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "DisableBatching" = "True"
        }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }
        Cull Off
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
            float _CameraOrthoSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texCoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
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
                float2 rotateduvs = Unity_Rotate_Radians_float(i.uv, float2(0.5, 0.5), _Time.y * _Speed); //spinning uvs
                float4 distortionPos = float4(Twirl(rotateduvs, float2(0.5, 0.5), _TwirlStrength, float2(0, 0)).xy - 0.5, 0.0, 1.0); //twirling uvs
                float noise = UnitySimpleNoise(distortionPos, _Scale) / pow((_CameraOrthoSize),0.25); //noise with twirled uvs
                float filt = tex2D(_StrengthFilter, i.uv).r * noise; //twirled noised passed through filter
                filt = pow(filt, _Power); //powering noise
                float4 clipSpaceDistortion = UnityObjectToClipPos(distortionPos); 
                float4 grabDistortion = ComputeGrabScreenPos(clipSpaceDistortion) + filt;
                grabDistortion.xy /= grabDistortion.w;
                
                float2 grabUV = i.grabPos.xy / i.grabPos.w;


                grabUV = lerp(grabUV, grabDistortion, filt);
 
                half4 col = tex2D(_BackgroundTexture, grabUV);
                return col;
            }
            ENDCG
        }

    }
}

// float noise = SimpleNoise(float3(twirledPos.x, twirledPos.y, 0), _Octaves, _Scale, _Persistence, _Lacunarity, _Gain, _Offset);
// float poweredNoise = pow(noise, _Power);

