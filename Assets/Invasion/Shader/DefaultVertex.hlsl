
struct VertexData
{
    float3 position : POSITION;
    float3 normal : NORMAL;
    float3 color : COLOR;
    float2 uv : TEXCOORD;
};

struct PixelData
{
    float4 position : SV_POSITION;
    float3 normal : NORMAL;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
};

PixelData Main(VertexData input)
{
    PixelData result;
    
    result.position = float4(input.position, 1.0f);
    result.normal = input.normal;
    result.color = float4(input.color, 1.0f);
    result.uv = input.uv;
    
    return result;
}