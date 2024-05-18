Shader "Custom/UniverseBubbleWormhole"
{
    Properties
    {
        _ThroatLength("Throat Length", Range(0, 100)) = 1
        _Smoothness("Smoothness", Range(0,5)) = 1
        _IntegrationStep("Integration Step", Range(0,5)) = 1
        _MaxSteps("Max Steps", Range(0,5000)) = 100
        _CamDistance("Camera Distance", Range(0,25)) = 1
        _CamZoom("Camera Zoom", Range(0, 15)) = 1

        _WormholeSky("Wormhole Sky", Cube) = "" {}
        _TestSky("Test Sky", Cube) = "" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD1;
            };

            float _ThroatLength;
            float _Smoothness;
            float _IntegrationStep;
            int _MaxSteps;

            float _CamDistance;
            float _CamZoom;

            samplerCUBE _WormholeSky;
            samplerCUBE _TestSky;
            sampler2D _BackgroundTexture;
            
            static const float PI = 3.14159265f;

            float Wormhole(float l){
                float x = max(0, 2 * (abs(l) - _ThroatLength)/ PI /_Smoothness);
                return 1 + _Smoothness * (x * atan(x) - 0.5 * log(1 + x * x));
            }   

            float WormholeDerivative(float l){
                float x = max(0, 2 * (abs(l) - _ThroatLength) / PI / _Smoothness);
                return 2 * atan(x) * sign(l) / PI;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (i.uv - 0.5) * 2.0;
                float3 vel = normalize(float3(-_CamZoom, uv.xy));
                float2 beta = normalize(vel.yz);
                //Ray tracing general relativity stuff
                float l = _CamDistance;
                float r = Wormhole(_CamDistance);
                float dl = vel.x;
                float H = length(vel);
                float phi = 0;
                float dr;
                int steps = 0;

                //Iterate through the wormhole, only stopping unti the end is reached or the camera length runs out
                while(abs(l) < max(abs(_CamDistance) * 2, _ThroatLength + 2) && steps < _MaxSteps){
                    dr = WormholeDerivative(l);
                    r = Wormhole(l);
                    l += dl * _IntegrationStep;
                    phi += H / r / r * _IntegrationStep;
                    dl += H * H * dr / r / r / r * _IntegrationStep;
                    steps++;
                }

                float dx = dl * dr * cos(phi) - H/r * sin(phi);
                float dy = dl * dr * sin(phi) + H/r * cos(phi);

                float3 vec = normalize(float3(dx, dy * beta));
                float3 cubeVec = normalize(float3(-vec.x, vec.z, -vec.y));

                if(l > 0){
                    //float4 grabDistortion = ComputeGrabScreenPos(float4(vec,3));
                    //grabDistortion.xy /= grabDistortion.w;

                    //return tex2D(_BackgroundTexture, grabDistortion.xy);
                    return texCUBE(_TestSky, cubeVec);
                    //return tex2D(_BackgroundTexture, i.grabPos);
                }
                else{
                    return texCUBE(_WormholeSky, cubeVec);
                }
            }
            ENDCG
        }
    }
}
