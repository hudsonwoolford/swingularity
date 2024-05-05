Shader "Unlit/DarkMatterOrb"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_FrontColor ("Front Color", color) = (0,0,0,1)
        [HDR]_BackColor ("Back Color", color)  = (1,1,1,1)
        [Toggle()] _FrontPart("Front Part", float) = 1
        _VoronoiPower("Voronoi Power", float) = 2
        _ScrollSpeed("Scroll Speed", vector) = (1,2,0,0)

        _CellDensity ("Cell Density", float) = 1
        _AngleOffset("Angle Offset", float) = 0
        _AlphaClip ("Alpha Clip", float) = 0.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Includes/ShaderGraphNodes.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _FrontColor;
            fixed4 _BackColor;
            float _FrontPart;
            float2 _ScrollSpeed;
            float _VoronoiPower;

            float _AlphaClip;
            float _CellDensity;
            float _AngleOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float2 scrolledUV = i.uv + _ScrollSpeed * (_Time.y + 200);

                float randomNoise = UnitySimpleNoise(i.uv, 1);
                float voronoi;
                Unity_Voronoi_float(scrolledUV, _AngleOffset, _CellDensity, voronoi);
                voronoi = pow(voronoi, _VoronoiPower) * randomNoise;
                clip(voronoi - _AlphaClip);


                //Coloring back and front
                float cameraDot = dot(i.normal, float3(0,1,0));
                float4 finalCol = cameraDot >= 0 ? _BackColor : _FrontColor;

                if(cameraDot >= 0 && _FrontPart == 0)
                {
                    finalCol = _BackColor;
                }
                else if(cameraDot < 0 && _FrontPart == 1)
                {
                    finalCol = _FrontColor;
                }
                else
                {
                    clip(voronoi - 1);
                }

                return voronoi * finalCol;
            }
            ENDCG
        }
    }
}
