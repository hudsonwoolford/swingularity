Shader "Custom/RayMarchingUniverse"
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
        _Power("Power", float) = 1

        [IntRange]_WarpOctaves("Warp Octaves", Range(0,5)) = 1
        _WarpOffset("Warp Offset", vector) = (0,0,0)
        _WarpScale("Warp Scale", Range(0,5)) = 1
        _WarpPersistence("Warp Persistence", Range(0, 5)) = 1
        _WarpLacunarity("Warp Lacunarity", Range(0,5)) = 1
        _WarpMultiplier("Warp Multiplier", Range(0,5)) = 1
        _WarpGain("Warp Gain", Range(0, 5)) = 1

        [HDR]_NebulaColorA ("Nebula Color A", Color) = (0.5,0.5,0.5)
        [HDR]_NebulaColorB ("Nebula Color B", Color) = (0.5,0.5,0.5)

        _FresnelStrength("Fresnel Strength", float) = 0
        [HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
        [HDR]_StarColor("Star Color", Color) = (1,1,1,1)
        _StarNoiseScale("Star Scale", float) = 0
        _TriplanarScale("Triplanar Scale", float) = 0
        _AngleOffset("Voronoi Angle Offset", float) = 0
        _VoronoiScale("Voronoi Scale", float) = 0
        [HDR]_VoronoiStarColor("Voronoi Star Color", Color) = (1, 1, 1, 1)
        _Metallic("Metallic", float) = 0
        _Smoothness("Smoothness", float) = 0
        _RidgeBlendHeight("Ridge Blend Height", float) = 0
        _RidgeBlendIntensity("Ridge Blend Intensity", float) = 0

        _VolumetricStarsScale("Volumetric Star Scale", float) = 0
        _VolumetricStarsPower("Volumetric Star Power", float) = 0
        _VolumetricStarsSubtraction("Volumetric Star Subtraction", float) = 0

        [HDR]_VolumetricStarsColor("Volumetric Star Color", Color) = (1,1,1,1)

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

            CGPROGRAM
            #pragma target 3.5
            #pragma surface surf Standard vertex:vert

            #include "UnityCG.cginc"
            #include "Includes/FractalNoise.cginc"
            #include "Includes/ShaderGraphNodes.cginc"
            #include "Includes/TriplanarMapping.cginc"

            struct Input
            {
                float3 viewDir : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 normal;
                INTERNAL_DATA
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _MaxSteps;
            float _MaxDistance;
            float _SurfaceDistance;

            int _Octaves;
            float3 _Offset;
            float _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain;
            float _Subtraction, _Power;

            int _WarpOctaves;
            float3 _WarpOffset;
            float _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain;

            float4 _NebulaColorA;
            float4 _NebulaColorB;

            float4 _FresnelColor;
            float _FresnelStrength;
            float4 _StarColor;
            float _StarNoiseScale;
            float _TriplanarScale;
            float _AngleOffset;
            float _VoronoiScale;
            float4 _VoronoiStarColor;

            float _Metallic;
            float _Smoothness;

            float _RidgeBlendHeight;
            float _RidgeBlendIntensity;

            float _VolumetricStarsScale;
            float _VolumetricStarsPower;
            float _VolumetricStarsSubtraction;
            float4 _VolumetricStarsColor;

            void vert (inout appdata_full v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT(Input, o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
                // o.ro = float3(0, 0, 0);
                o.hitPos = v.vertex;
                o.normal = v.normal;
                o.worldNormal = UnityObjectToWorldNormal(v.normal) * 2;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                o.viewDir = viewDir;
            }

            float GetDist(float3 p){
                float warp = SimpleNoise(p, _WarpOctaves, _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain, _WarpOffset);
                float d = pow(SimpleNoise(p + float3(warp, 0, 0), _Octaves, _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain, _Offset), _Power);

                return clamp(d - _Subtraction, 0, 1000000);
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

            float GetDistStars(float3 p){
                float d, cells;
                voronoi3D_float(p, _VolumetricStarsScale, d, cells);
                d = pow(saturate(d), _VolumetricStarsPower) * 0.1;
                return clamp(d - _VolumetricStarsSubtraction, 0, 1000000);
            }

            float Raymarch(float3 ro, float3 rd, bool stars){
                float dO = 0;
                float dS;

                for (int i = 0; i < _MaxSteps; i++){
                    float3 p = ro + dO * rd;
                    dS = GetDistStars(p);
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

            void surf (Input i, inout SurfaceOutputStandard o)
            {
                float3 ro = i.hitPos;
                float3 rd = normalize(i.ro - ro) * -1;

                float d = Raymarch(ro, rd);

                fixed4 col = 0;
                float3 p = ro + rd * d;
                float3 n = GetNormal(p);

                if (d < _MaxDistance){


                    float grad = d;
                    col.rgb = lerp(_NebulaColorA, _NebulaColorB, grad);

                }
                else{
                    discard;
                }

                float starDist = Raymarch(ro, rd, true);
                float3 starPoint = ro + rd * starDist;
                starPoint = (starPoint - ro)/rd;

                float starPointBW = dot(float3(0.2126729, 0.7151522, 0.0721750), starPoint.xyz);
                starPointBW = (starPointBW + 1)/1.5;

                // starPointBW = pow(starPointBW, Unity_Remap_float4(d, float2(0, 8), float2(30, 10)));
                if(starDist < d){
                    col += saturate(1-starPointBW) * _VolumetricStarsColor;
                }

                col += pow(saturate(TriplanarNoise(ro, n, _TriplanarScale, _StarNoiseScale)), 20) * _StarColor;


                float v,cells;
                voronoi3D_float(i.hitPos, _VoronoiScale, 0.5, v, cells);
                float3 voronoiSquares = float3(cells, cells, cells);
                voronoiSquares = voronoiSquares * 2 - float3(1, 1, 1);
                float voronoiOutput = (dot(voronoiSquares, rd) * .5 + float3(.5, .5, .5), 10) * clamp(1-v, 0, 0.8);

                col += voronoiOutput * _VoronoiStarColor * float4(saturate(voronoiSquares.xyz), 1);

                float fresnelVal = 1 - saturate(abs(dot(i.worldNormal, i.viewDir)));
                float fresnel = pow(fresnelVal, _FresnelStrength);

                // float blendVal = clamp(0,1,pow(length(ro)/_RidgeBlendHeight, _RidgeBlendIntensity));
                // col = lerp(col, _FresnelColor, blendVal);
                col = lerp(col, _FresnelColor, pow(fresnel, _FresnelStrength));

                o.Albedo = col.rgb;
                o.Smoothness = _Smoothness;
                o.Metallic = _Metallic;



            }
            ENDCG
    }
}
