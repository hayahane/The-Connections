Shader "FullScreen/Sobel"
{
    Properties
    {
        _Delta("Thickness", Float) = 0.001
        _Threshold("Threshold", Float) = 0.2
    }
    
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Sobel.hlsl"

    float _Delta;
    float _Threshold;
    
    half4 colwhite = half4(1,1,1,1);
    half4 colblack = half4(0,0,0,1);

    half4 frag(Varyings input) : SV_TARGET
    {
        float2 uv = GetNormalizedScreenSpaceUV(input.positionCS);
        float sobel =  SobelDepth(_Delta, uv);
        return pow(sobel,_Threshold);
        return Luminance(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv));
        return SampleSceneDepth(uv);
    }
    
    ENDHLSL
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}