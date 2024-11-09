
TextureCube skyboxTexture : register(t0);
SamplerState SampleType : register(s0);

struct PixelData
{
    float4 position : SV_POSITION;
    float3 texCoord : TEXCOORD0;
};

float4 Main(PixelData input) : SV_TARGET
{
    return skyboxTexture.Sample(SampleType, input.texCoord);
}