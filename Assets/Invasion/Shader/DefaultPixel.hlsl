
Texture2D diffuse : register(t0);
SamplerState samplerState : register(s0);

struct PixelData
{
    float4 position : SV_POSITION;
    float3 normal : NORMAL;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
};

float4 Main(PixelData input) : SV_TARGET
{
    return diffuse.Sample(samplerState, input.uv) * input.color;
}