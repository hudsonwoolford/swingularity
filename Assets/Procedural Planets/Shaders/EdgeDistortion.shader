// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/EdgeDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionRes ("Distortion Res", float) = 0.05
        _DistortionStrength ("Distortion Strength", float) = 0.05
        _DistortionDistance ("Distortion Distance", float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata_t
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _DistortionStrength;
            float2 _CamSize;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.position);
                o.uv = v.uv;
                o.worldPos = float2((o.uv.x-0.5) * _CamSize.x, (o.uv.y-0.5) * _CamSize.y) + _WorldSpaceCameraPos.xy;
                return o;
            }

            float2 _MaxBounds;
            float2 _MinBounds;
            float _DistortionDistance;
            float _DistortionRes;

            float2 EdgeDistortion(float2 pos)
            {
                float edgeFactor = min(min(abs(pos.x - _MinBounds.x), abs(_MaxBounds.x - pos.x)), min(abs(pos.y - _MinBounds.y), abs(_MaxBounds.y - pos.y))); // Distance to nearest edge
                edgeFactor = smoothstep(0.0, _DistortionDistance, edgeFactor); // Soft transition

                float2 midPoint = (_MinBounds + _MaxBounds) / 2;
                float2 relPoint = pos - midPoint;

                float2 offset = sin(relPoint * _DistortionRes) * _DistortionStrength * (1.0 - edgeFactor); // Warping effect
                return offset;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 distortedOffset = EdgeDistortion(i.worldPos);
                //return float4(distortedOffset.xy, 0,0);
                return tex2D(_MainTex, i.uv + distortedOffset);
            }
            ENDCG
        }
    }
}