Shader "Unlit/TerrestialSurface"
{
    Properties
    {
    
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        CGPROGRAM
        #pragma surface surf Lambert

        struct Input
        {
            float3 worldpos;
        };


        void surf(Input IN, inout SurfaceOutput o )
        {
            o.Albedo = fixed3(0,1,0);
        
        }
        ENDCG
    }
}
