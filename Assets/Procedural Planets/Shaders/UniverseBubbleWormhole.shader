Shader "Custom/UniverseBubbleWormhole"
{
    Properties
    {
        _ThroatLength("Throat Length", float) = 1
        _Smoothness("Smoothness", float) = 1
        _IntegrationStep("Integration Step", float) = 1
        _MaxSteps("Max Steps", int) = 1
        _CamDistance("Camera Distance", float) = 1

        _CubeMap1("Cubemap 1", Cube) = "" {}
        _CubeMap2("Cubemap 2", Cube) = "" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            float _ThroatLength;
            float _Smoothness;
            float _IntegrationStep;
            int _MaxSteps;

            float _CamDistance = 5;
            float _CamZoom = 1.5;

            samplerCUBE _CubeMap1;
            samplerCUBE _CubeMap2;
            
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
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (i.uv - 0.5) * 2.0;
                float3 vel = normalize(float3(-_CamZoom, uv));
                float2 beta = normalize(vel.yz);

                //Ray tracing general relativity stuff
                float l = _CamDistance;
                float r = Wormhole(_CamDistance);
                float dl = vel.x;
                float H = r * length(vel.yz);
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
                    return texCUBE(_CubeMap1, cubeVec);
                }
                else{
                    return texCUBE(_CubeMap2, cubeVec);
                }
            }
            ENDCG
        }
    }
}
