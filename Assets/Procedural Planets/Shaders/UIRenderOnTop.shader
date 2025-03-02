Shader "Custom/UI_AlwaysOnTop"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
        
        Pass
        {
            // Disable depth buffer writes and ensure it always renders above other objects
            ZWrite Off      // Prevents writing to depth buffer
            ZTest Always    // Always renders (ignores depth testing)
            
            // Blending settings for transparent UI
            Blend SrcAlpha OneMinusSrcAlpha
            
            // The texture for the UI element
            SetTexture [_MainTex] { combine primary }
        }
    }
    FallBack "Diffuse"
}
