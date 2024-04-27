Shader "Unlit/GravityMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HeatColor("Heat Color", color) = (0,1,1,1)
        _MaxForce("Max Force", float) = 10
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _Planets[512]; // Specify the size of the array, adjust as needed
            uint _PlanetCount;
            float4 _HeatColor;
            float _MaxForce;

            float GravityForceAtPoint(float3 worldPos, float objMass)
            {
                float totalForce = 0;
                for (uint u = 0; u < min(_PlanetCount, 512); u++)
                {
                    float3 direction = float3(_Planets[u].xy, 0) - worldPos;
                    float distance = length(direction);
                    if (distance == 0) continue; // Avoid division by zero

                    float forceMagnitude = 1 * (_Planets[u].z * objMass) / (distance * distance); // Magnitude of the force

                    totalForce += forceMagnitude;
                }
                return totalForce;
            }
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 vert = float4(o.vertex.xy, 0, 1);
                float3 objW = mul(unity_ObjectToWorld, vert).xyz;
                //o.worldPos = (float3(objW.x * (8 / 2), objW.y * -(8 / 2), 0));
                o.worldPos.z = 0;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float force = GravityForceAtPoint(i.worldPos, 1);
                float4 lerpCol = lerp(0, _HeatColor, force/_MaxForce);
                return lerpCol;
            }
            ENDCG
        }
    }
}
