Shader "Unlit/GravityGrid"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Divisor ("Divisor", float) = 1
        _MinOffset ("Min Offset", float) = 1
        _MaxOffset ("Max Offset", float) = 1
        _Thickness ("Thickness", float) = 0
        _Strength ("Strength", float) = 0

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
            };

            fixed4 _Color;
            float _Thickness;
            float3 _Planets[512]; // Specify the size of the array, adjust as needed
            uint _PlanetCount;
            float _MinOffset;
            float _MaxOffset;
            float _Divisor;
            float _Strength;

            float2 GravityForceAtPoint(float3 worldPos, float objMass)
            {
                float2 totalForce = float2(0, 0);
                for (uint u = 0; u < min(_PlanetCount, 512); u++)
                {
                    float3 direction = float3(_Planets[u].xy, 0) - worldPos;
                    float distance = length(direction);
                    if (distance == 0) continue; // Avoid division by zero

                    float forceMagnitude = 1 * (_Planets[u].z * objMass) / (distance * distance); // Magnitude of the force
                    forceMagnitude = clamp(forceMagnitude, _MinOffset,_MaxOffset);

                    float2 addForce = (forceMagnitude * normalize(direction).xy) / pow(distance, _Strength);
                    if(length(addForce) > distance)
                    {
                        addForce = (distance * normalize(direction).xy);
                    }
                    totalForce += addForce; // Add force vector to totalForce
                }
                return totalForce;
            }

            v2f vert(appdata_full v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float2 displacement = GravityForceAtPoint(worldPos, 1);
                o.vertex.xy += displacement;

                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {

                return _Color;
            }
            ENDCG
        }
    }
}
