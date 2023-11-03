Shader "FullScreen/ScanEffect"
{
    Properties
    {
        [HDR] _ScanColor("Scan Color", Color) = (1,0.9,0.27,1)

        _ScanOrigin("Scan Origin", Vector) = (0,0,0)
        _ScanRange("Scan Range", Float) = 0
        _ScanWidth("Scan Width", Float) = 12
        _ScanPower("Scan Power", Float) = 2

        _LineWidthBias("Scan Line Width", Float) = 0.5
        _LineDensity("Scan Line Density", Float) = 1

        _OutlineWidth("Outline Width", Float) = 0.01
        _SobelThreshold("Outline Threshold", Float) = 0.15
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Sobel.hlsl"

    CBUFFER_START(UnityPerMaterial)
        half4 _ScanColor;

        float3 _ScanOrigin;
        float _ScanRange;
        float _ScanWidth;
        float _ScanPower;

        float _LineWidthBias;
        float _LineDensity;

        float _OutlineWidth;
        float _SobelThreshold;
    CBUFFER_END

    float2 uvs[9];

    half4 SampleBlitSource(float2 uv)
    {
        return SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointRepeat, uv);
    }

    half4 SampleBlitClamp(float2 uv)
    {
        return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
    }
    

    half4 ScanFrag(Varyings input) : SV_TARGET
    {
        float2 uv = GetNormalizedScreenSpaceUV(input.positionCS);

        #if UNITY_REVERSED_Z
        float depth = SampleSceneDepth(uv);
        #else
            float depth = lerpUNITY_NEAR_CLIP_VALUE,1,SampleSceneDepth(uv));
        #endif

        const float3 positionWS = ComputeWorldSpacePosition(uv, depth,UNITY_MATRIX_I_VP);

        const float pixelDistance = distance(positionWS, _ScanOrigin);
        half4 sceneColor = SampleBlitSource(uv);
        float pixelRange = _ScanRange - pixelDistance;

        // Return if 
        if (pixelRange < 0 || depth >= 1.0) return sceneColor;

        pixelRange /= _ScanWidth;
        const float rangeClamp = clamp(1 - pixelRange, 0, 1);

        const float scanLine = smoothstep(abs(sin(pixelDistance * _LineDensity) + _LineWidthBias), 1,
                                            1 + _LineWidthBias);
        float lerpTemplate = scanLine * rangeClamp + pow(rangeClamp, _ScanPower);
        float sobel = SobelDepth(_OutlineWidth/100, uv);
        sobel = pow(sobel,0.7);
        sobel = sobel < _SobelThreshold ? 0 : sobel;
        
        lerpTemplate += sobel * rangeClamp;
        lerpTemplate = clamp(lerpTemplate, 0, 1);
        
        return lerp(sceneColor, _ScanColor, lerpTemplate);
    }
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Scan Effect"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment ScanFrag
            ENDHLSL
        }
    }
}