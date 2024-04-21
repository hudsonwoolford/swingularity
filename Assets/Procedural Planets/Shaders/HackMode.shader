Shader "Hidden/HackMode"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0,5)) = 0
        _EdgeColor("Edge Color", Color) = (1,1,1,1)
    } 
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        
        Pass
        {
            Tags { "LightMode" = "Edge Pass" }
            Name "Edge Pass"
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float2 _MainTex_TexelSize;
            float _Threshold;
            float _Progress;
            float4 _EdgeColor;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                //offsets of pixel
                float offsetStep = _MainTex_TexelSize.y;
                float2 offsets[9] = {
                    float2(-offsetStep, offsetStep),
                    float2(0, offsetStep),
                    float2(offsetStep, offsetStep),
                    float2(-offsetStep, 0),
                    float2(0,0),
                    float2(offsetStep, 0),
                    float2(-offsetStep, -offsetStep),
                    float2(0, -offsetStep),
                    float2(offsetStep, -offsetStep)
                };
                //color sampling, 3x3
                float colorSamples[9];
                //samplers 3x3, converts to grayscale
                for (int j = 0; j < 9; j++)
                {
                    colorSamples[j] = dot(tex2D(_MainTex, i.uv + offsets[j]).rgb, float3(0.299, 0.587, 0.114));
                    if(j == 4) {dot(col.rgb, float3(0.299, 0.587, 0.114)); continue;}
                }
                //sobel algorithm info
                float sobelX[9] = {-1, 0, 1,
                                   -2, 0, 2,
                                   -1, 0, 1};
                float sobelY[9] = {-1,-2,-1,
                                    0, 0, 0,
                                    1, 2, 1};
                float gx = 0, gy = 0;
                //sobel algorthm application
                for(int i = 0; i<9; i++)
                {
                    gx += colorSamples[i] * sobelX[i];
                    gy += colorSamples[i] * sobelY[i];
                }

                //calculating edge magnitude
                float edgeMag = sqrt((gx*gx) + (gy*gy));
                if(edgeMag > _Threshold)
                {
                    return edgeMag * _EdgeColor * col.a;
                }
                return 0;
                
            }
            ENDCG
        }
    }
}
