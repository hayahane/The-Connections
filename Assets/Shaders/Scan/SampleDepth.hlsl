//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

SAMPLER(Sampler_BlitTexture);

void SampleColor_float(float2 uv, out half3 color)
{
    color = SAMPLE_TEXTURE2D(_BlitTexture, Sampler_BlitTexture, uv);
}
