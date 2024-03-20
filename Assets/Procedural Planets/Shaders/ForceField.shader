Shader "Custom/ForceField"
{
    Properties
    {
        [HDR]_FrontColor ("Front Color", Color) = (1,1,1,1)
        [HDR]_BackColor ("Back Color", Color) = (1,1,1,1)

        _PanelSpread ("Panel Spread", Range(0, 0.25)) = 1
        _Frequency ("Frequency", Range(0, 50)) = 1
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Header(Fresnel)] //Fresnel
        [Toggle(_UseFresnel)]_UseFresnel("UseFresnel", float) = 0
        [HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1)
        _FresnelStrengthNear("Fresnel Strength Near", float) = 2
        _FresnelStrengthFar("Fresnel Strength Far", float) = 5
        _FresnelPower("Fresnel Power", float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Cull Off
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert alpha
        #pragma target 4.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
            float3 normal;
            float3 vertPos;
            float fresnelValue;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _FrontColor; //front surface color
        fixed4 _BackColor; //back surface color
        float _PanelSpread;
        float _Frequency;

        float _UseFresnel;
        fixed4 _FresnelColor;
        float _FresnelPower;

        void vert (inout appdata_full v, out Input o)
        {
            
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float angle = _Time * _Frequency; // You can adjust _Frequency to control the speed of oscillation
            float sinValue = sin(angle);
            float4 modifiedVertex = float4(v.vertex.xyz + v.normal * _PanelSpread * ((sinValue + 1) / 2), 1);


            v.vertex = modifiedVertex;
            o.vertPos = v.vertex;

            float3 viewDir = ObjSpaceViewDir ( v.vertex );
            o.fresnelValue = saturate(0.05 * pow(abs(dot(v.normal, viewDir)), _FresnelPower));
        }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float d = dot(IN.viewDir, IN.worldNormal);
            bool front = d > 0 ? true : false;

            fixed4 mc = front ? _FrontColor : _BackColor; //determine which color to multiply
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * mc;
            
            float3 compoundColor = lerp(_FresnelColor, c.rgb, IN.fresnelValue * _UseFresnel);

            o.Albedo = compoundColor;

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
}
