Shader "Custom/ForceField"
{
    Properties
    {
        _MainTex("UV Map", 2D) = "white" {}
        [HDR]_FrontColor ("Front Color", Color) = (1,1,1,1)
        [HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
        [Toggle(_OnlyAffected)]_OnlyAffected("Only Affected", float) = 0

        [Header(Shield Variables)]
        _PanelSpread ("Panel Spread", Range(0, 0.25)) = 1
        _Frequency ("Frequency", Range(0, 0.3)) = 0.25

        [Header(Interactivity Variables)]
        _InteractiveDistance ("Interactive Distance", float) = 1
        _InteractiveMultiplier ("Interactive Multiplier", Range(0, 50)) = 1
        _FalloffIntensity ("Falloff Intensity", float) = 2
        
        [Header(Collision Ripple)]
        _RippleFalloff ("Ripple Falloff", Range(0, 0.2)) = 0.1
        _RippleCutoff ("Ripple Cutoff", float) = 0.1
        _RippleHeight("Ripple Height", Range(0, 0.1)) = 0.1
        _RippleSpeed ("Ripple Speed", Float) = 1.0
        _RippleBlendFallofff ("Ripple Blend Falloff", Float) = 0
        _RippleBandLength("Ripple Band Length", Float) = 0.0
        _RipplePowerScaling("Ripple Power Scaling", Float) = 0.0
        _RippleExponent("Ripple Exponent", Float) = 0.0

        [Header(Fresnel)] //Fresnel
        [Toggle(_UseFresnel)]_UseFresnel("UseFresnel", float) = 0
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
        #define PI 3.14159265359
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
            bool objNear, inRipple;
            float objDistRatio;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _FrontColor; //front surface color
        fixed4 _BackColor; //back surface color
        float _OnlyAffected;
        float _PanelSpread;
        float _Frequency;

        float _UseFresnel;
        fixed4 _FresnelColor;
        float _FresnelPower;
        float _FresnelStrengthNear;
        float _FresnelStrengthFar;

        float3 _ObjPos[10];
        float _InteractiveDistance;
        float _InteractiveMultiplier;
        float _FalloffIntensity;
        float _Scale;
        float3 _TransformWorldPos;

        float3 _HitPos[3];
        float2 _HitInfo[3];

        float _RippleHeight; //z
        float _RippleCutoff;
        float _RippleFalloff; //i
        float _RippleBlendFallofff; //i
        float _RippleSpeed; //a
        float _RippleBandLength; //h
        float _RipplePowerScaling;
        float _RippleExponent;


        float2 CalculateRipple(int i, float3 faceCentre)
        {
            float timeSinceHit = _Time.y - _HitInfo[i].x; //time since hit
            float hitPower = pow(_HitInfo[i].y * _RipplePowerScaling, _RippleExponent);
            if (hitPower < _RippleCutoff) return float2(0, 0);

            float theLine = timeSinceHit * _RippleSpeed; //ripple line distance
            float distToHit = length(_HitPos[i] - faceCentre); //distance to hit
            float distToLine = abs(theLine-distToHit); //distance to ripple line
            
            float yUp = 0; //declaration of total offset
            float ripDistRatio = 0; //declaration of blending value
            if(distToLine <= _RippleBandLength/2) //if within ripple band
            {
                float progress = (1-(distToLine / (_RippleBandLength/2))) * (PI/2); //returns 1 rad if middle of ripple band, 0 if on edges
	            yUp = (sin(progress-(PI/2))+1)/2 * _RippleHeight * hitPower; //calculating total offset by SIN, scaled by height and power
                float ripOpacity = pow(timeSinceHit/(_RippleFalloff*hitPower), 2);
                ripDistRatio = saturate(distToLine / (_RippleBandLength / 2) + ripOpacity); //how close is the tile to the middle of the ripple
            }
            
            return float2(yUp, ripDistRatio);
        }
        void vert (inout appdata_full v, out Input o)
        {
            
            UNITY_INITIALIZE_OUTPUT(Input, o);
            //fresnel
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 relCameraPos = float3(worldPos.xy, _WorldSpaceCameraPos.z);
            float3 bodyWorldCenter = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
            float camRadiiFromSurface = (length(bodyWorldCenter - relCameraPos) - _Scale) / _Scale;
            float fresnelT = smoothstep(0,1,camRadiiFromSurface);
            float3 viewDir = normalize(worldPos - relCameraPos);
            float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal,0)));
            float fresStrength = lerp(_FresnelStrengthNear, _FresnelStrengthFar, fresnelT);
            o.fresnelValue = 1-saturate(fresStrength * pow(1 + dot(viewDir, normWorld), _FresnelPower));


            float3 faceCentre = _TransformWorldPos + (v.normal * _Scale);
            float totalY = 0;
            float ripDistRatio = 0;

            for (uint i = 0; i < 10; i++){
                if(_HitInfo[i].x == 0) {break;}
                float2 rippleInfo = CalculateRipple(i, faceCentre);
                ripDistRatio = rippleInfo.y > ripDistRatio ? rippleInfo.y : ripDistRatio;
                totalY += rippleInfo.x;
            }
            totalY*=1-ripDistRatio; 


            float distToBall = 10000;
            for(uint u = 0; u<_ObjPos.Length; u++)
            {
                if (_ObjPos[u].x == 0 && _ObjPos[u].y == 0 && _ObjPos[u].z == 0)
                {
                    continue;
                }

                float lengthToBall = length(_ObjPos[u] - faceCentre) / _Scale;
                distToBall = distToBall > lengthToBall ? lengthToBall : distToBall;
            }
            if (distToBall <= _InteractiveDistance){ o.objNear = true; }
            if (totalY > 0) { o.inRipple = true; }

            float ballDistRatio = pow(saturate(distToBall / _InteractiveDistance), _FalloffIntensity);
            o.objDistRatio = ripDistRatio != 0 && !o.objNear ? ripDistRatio : ballDistRatio;
            
            //displacement
            float angle = snoise((500+_Time+_TransformWorldPos) * _Frequency * (1+snoise(v.normal))); // You can adjust _Frequency to control the speed of oscillation
            float sinValue = sin(angle);

            float3 rippleDisplacement = v.normal * totalY;
            float3 displacement = rippleDisplacement + v.normal * (_PanelSpread * (sinValue + 1) / 2) * ballDistRatio;

            float4 modifiedVertex = float4(v.vertex.xyz + displacement, 1);
            o.displacementRatio = saturate((length(displacement)-length(rippleDisplacement))/ _PanelSpread); //how displaced is the tile
        
            v.vertex = modifiedVertex;
            o.vertPos = v.vertex;
        }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            fixed4 mc = _FrontColor;
            float4 compoundColor = lerp(_FresnelColor, mc, IN.fresnelValue * _UseFresnel);

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * compoundColor;

            float falloff = IN.objNear ? _FalloffIntensity : IN.inRipple ? _RippleBlendFallofff : _FalloffIntensity;
            bool affected = IN.inRipple || IN.objNear;
            
            if(!affected) {c.a *= compoundColor.a;}
            float normalAlpha = saturate(c.a * pow(IN.displacementRatio, 6));

            if(affected)
            {
                o.Emission = c * _InteractiveMultiplier;
                o.Alpha = lerp(c.a, normalAlpha, saturate(pow(IN.objDistRatio, falloff))) * 0.5;
                return;
            }

            o.Emission = c * 0.5;

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = normalAlpha * (1-_OnlyAffected); //_OnlyAffected means that it will only show the ForceField while in ripple or near an object 

        }
        ENDCG
    }
}
