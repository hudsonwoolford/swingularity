Shader "Unlit/GravityGrid"
{
    Properties
    {
        [HDR]_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Divisor ("Divisor", float) = 1
        _MinOffset ("Min Offset", float) = 1
        _MaxOffset ("Max Offset", float) = 1
        _Strength ("Strength", float) = 0

        _MaxForce ("Max Force", float) = 5
        _ForcePower ("Force Power", float) = 1
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            struct appdata_full
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            fixed4 _Color;
            float3 _Planets[512]; // Specify the size of the array, adjust as needed
            uint _PlanetCount;
            float _MinOffset;
            float _MaxOffset;
            float _Divisor;
            float _Strength;

            float _MaxForce;
            float _ForcePower;

            float _GConstant;

            float2 GravityForceAtPoint(float3 worldPos, float objMass)
            {
                float2 displacement = float2(0,0);
                for(uint u = 0; u<_PlanetCount; u++)
                {
                    float2 relPos = (worldPos.xy - _Planets[u].xy);
                    float dispMag = pow((relPos.x * relPos.x + relPos.y * relPos.y),_Strength) / _Divisor;
                    displacement+=float3((-_Planets[u].z/max(dispMag,_Planets[u].z)) * relPos.xy,0);
                
                }
                return displacement;
            }   

            v2f vert(appdata_full v)
            {
                v2f o;

                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float2 displacement = GravityForceAtPoint(o.worldPos, 1);
                float3 displacedWorldPos = float3(o.worldPos.xy + displacement,0);

                v.vertex.xyz = UnityObjectToClipPos(mul(unity_WorldToObject, displacedWorldPos)).xyz;

                o.vertex = v.vertex;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float force = pow(length(GravityForceAtPoint(i.worldPos, 1)), _ForcePower);
                //for(uint u = 0; u<_PlanetCount; u++)
                //{
                //    if(length(i.worldPos.xy - _Planets[u].xy) < 5)
                //    {
                //        return 1;
                //    }
                //}
                return lerp(0, _Color, saturate(force/_MaxForce));
            }
            ENDCG
        }
    }
}
