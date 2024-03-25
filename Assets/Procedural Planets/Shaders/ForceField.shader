Shader "Custom/ForceField"
{
    Properties
    {
        _MainTex("UV Map", 2D) = "white" {}
        [HDR]_FrontColor ("Front Color", Color) = (1,1,1,1)
        [HDR]_BackColor ("Back Color", Color) = (1,1,1,1)

        [Header(Shield Variables)]
        _PanelSpread ("Panel Spread", Range(0, 0.25)) = 1
        _Frequency ("Frequency", Range(0, 0.3)) = 0.25
        _BaseOpacity ("Base Opacity", Range(-50, 50)) = 0.25

        [Header(Interactivity Variables)]
        _InteractiveDistance ("Interactive Distance", float) = 1
        _InteractiveMultiplier ("Interactive Multiplier", Range(0, 100)) = 50
        _FalloffThreshold ("Falloff Threshold", float) = 1
        _FalloffIntensity ("Falloff Intensity", float) = 2

        [Header(Fresnel)] //Fresnel
        [Toggle(_UseFresnel)]_UseFresnel("UseFresnel", float) = 0
        [HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
        _FresnelStrengthNear("Fresnel Strength Near", float) = 2
        _FresnelStrengthFar("Fresnel Strength Far", float) = 5
        _FresnelPower("Fresnel Power", float) = 2

        [Header(Metal Smooth)]
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend One One
        Cull Back
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert alpha:blend
        #pragma target 4.0
        #include "Includes/SimplexNoise.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
            float3 normal;
            float3 vertPos;
            float fresnelValue;
            float displacementRatio;
            bool objNear;
            float objDistRatio;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _FrontColor; //front surface color
        fixed4 _BackColor; //back surface color
        float _PanelSpread;
        float _Frequency;
        float _BaseOpacity;

        float _UseFresnel;
        fixed4 _FresnelColor;
        float _FresnelPower;

        float3 _ObjPos;
        float _InteractiveDistance;
        float _InteractiveMultiplier;
        float _FalloffThreshold;
        float _FalloffIntensity;
        float _Scale;
        float3 _TransformWorldPos;


        void vert (inout appdata_full v, out Input o)
        {
            
            UNITY_INITIALIZE_OUTPUT(Input, o);
            //fresnel
            float3 viewDir = float3(0,0,1);
            float3 sphereNormal = normalize(v.vertex);
            o.fresnelValue = 1-saturate(0.05 * pow(abs(dot(sphereNormal, viewDir)), _FresnelPower));

            o.vertPos = v.vertex;

            //interactivity
            float3 faceCentre = _TransformWorldPos + (v.normal * _Scale);
            if(length(_ObjPos.xyz - faceCentre) <= _InteractiveDistance)
            {
                o.objNear = true;
                o.objDistRatio = length(_ObjPos.xyz - faceCentre) / _FalloffThreshold;
                return;
            }
            else
            {
                o.objNear = false;
            }

            //displacement
            float angle = snoise((500+_Time) * _Frequency * (1+snoise(v.normal))); // You can adjust _Frequency to control the speed of oscillation
            float sinValue = sin(angle);
            float3 displacement = v.normal * _PanelSpread * ((sinValue + 1) / 2);
            float4 modifiedVertex = float4(v.vertex.xyz + displacement, 1);
            o.displacementRatio = saturate(length(displacement) * 1.1 / _PanelSpread); //how displaced is the tile

            v.vertex = modifiedVertex;
            o.vertPos = v.vertex;
        }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float d = dot(IN.viewDir, IN.worldNormal);
            bool front = d > 0 ? true : false;

            fixed4 mc = front ? _FrontColor : _BackColor; //determine which color to multiply
            float4 compoundColor = lerp(_FresnelColor, mc, IN.fresnelValue * _UseFresnel);

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * compoundColor;
            
            if(IN.objNear)
            {
                o.Albedo = lerp(c*_InteractiveMultiplier, c, pow(saturate(IN.objDistRatio), _FalloffIntensity));
                o.Alpha = c.a;
                return;
            }

            o.Albedo = c;

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = saturate(c.a * pow(IN.displacementRatio,-1.7) + (c.a * _BaseOpacity));

        }
        ENDCG
    }
}
