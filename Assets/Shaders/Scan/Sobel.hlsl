#ifndef SOBEL_INCLUDE
#define SOBEL_INCLUDE

#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

float SampleBlitLumniance(float2 uv)
{
    return Luminance(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv));
}

float SobelDepth(float delta, float2 uv)
{
    float edgeX = 0, edgeY = 0;

    edgeX += SampleSceneDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
    edgeX += SampleSceneDepth(uv + float2(1.0, -1.0) * delta) * -1.0;
    edgeX += SampleSceneDepth(uv + float2(-1.0, 0.0) * delta) * 2.0;
    edgeX += SampleSceneDepth(uv + float2(1.0, 0.0) * delta) * -2.0;
    edgeX += SampleSceneDepth(uv + float2(-1.0, 1.0) * delta) * 1.0;
    edgeX += SampleSceneDepth(uv + float2(1.0, 1.0) * delta) * -1.0;
    
    edgeY += SampleSceneDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
    edgeY += SampleSceneDepth(uv + float2(0.0, -1.0) * delta) * 2.0;
    edgeY += SampleSceneDepth(uv + float2(1.0, -1.0) * delta) * 1.0;
    edgeY += SampleSceneDepth(uv + float2(-1.0, 1.0) * delta) * -1.0;
    edgeY += SampleSceneDepth(uv + float2(0.0, 1.0) * delta) * -2.0;
    edgeY += SampleSceneDepth(uv + float2(1.0, 1.0) * delta) * -1.0;
    
    return sqrt(edgeX * edgeX + edgeY * edgeY);
}


float SobelNormal(float delta, float2 uv)
{
    
}

#endif