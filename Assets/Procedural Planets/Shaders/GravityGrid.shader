Shader "Custom/GravityGrid"
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

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 vertexPos;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
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

        void vert (inout appdata_full v, out Input o)
        {
            
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertexPos = v.vertex;
            o.worldPos = mul(unity_ObjectToWorld, o.vertexPos).xyz;

            float2 displacement =  GravityForceAtPoint(o.worldPos, 1);
            v.vertex.xy += displacement;
            o.vertexPos = v.vertex;


        }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
