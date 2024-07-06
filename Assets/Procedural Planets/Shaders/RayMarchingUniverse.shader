Shader "Unlit/RayMarchingUniverse"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaxSteps ("Max Steps", int) = 100
        _MaxDistance ("Max Distance", float) = 100
        _SurfaceDistance ("Surface Distance", float) = 0.001
        [IntRange]_Octaves("Octaves", Range(0,5)) = 1
        _Offset("Offset", vector) = (0,0,0)
        _Scale("Scale", Range(0,5)) = 1
        _Persistence("Persistence", Range(0, 5)) = 1
        _Lacunarity("Lacunarity", Range(0,5)) = 1
        _Multiplier("Multiplier", Range(0,5)) = 1
        _Gain("Gain", Range(0, 5)) = 1
        _Subtraction("Subtraction", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Includes/FractalNoise.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _MaxSteps;
            float _MaxDistance;
            float _SurfaceDistance;

            int _Octaves;
            float3 _Offset;
            float _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain;
            float _Subtraction;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // o.ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
                o.ro = float3(0, 0, 0);
                o.hitPos = v.vertex;
                return o;
            }
            
            float GetDist(float3 p){
                float d = SimpleNoise(p, _Octaves, _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain, _Offset);

                return saturate(d - _Subtraction);
            }

            float Raymarch(float3 ro, float3 rd){
                float dO = 0;
                float dS;

                for (int i = 0; i < _MaxSteps; i++){
                    float3 p = ro + dO * rd;
                    dS = GetDist(p);
                    dO += dS;

                    if(dS < _SurfaceDistance || dO > _MaxDistance) break;
                }

                return dO;
            }

            float3 GetNormal(float3 p){
                float2 e = float2(1e-2, 0);
                float3 n = GetDist(p) - float3(GetDist(p-e.xyy),GetDist(p-e.yxy),GetDist(p-e.yyx));
                return normalize(n);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;
                float3 ro = i.ro;
                float3 rd = normalize(i.hitPos - ro);

                float d = Raymarch(ro, rd);

                fixed4 col = 0;

                if (d < _MaxDistance){
                    float3 p = ro + rd * d;
                    float3 n = GetNormal(p);
                    col.rgb = n;
                }
                else{
                    discard;
                }
                return col;

            }
            ENDCG
        }
    }
}
