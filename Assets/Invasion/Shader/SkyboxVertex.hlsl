
cbuffer MatrixBuffer : register(b0)
{
    matrix projectionMatrix;
    matrix viewMatrix;
};

struct VertexData
{
    float3 position : POSITION;
};

struct PixelData
{
    float4 position : SV_POSITION;
    float3 texCoord : TEXCOORD0;
};

PixelData Main(VertexData input)
{
    PixelData output;
    
    matrix view = viewMatrix;
    
    view._41 = 0.0f;
    view._42 = 0.0f;
    view._43 = 0.0f;
    
    float4 pos = mul(float4(input.position, 1.0f), view);
    pos = mul(pos, projectionMatrix);
    output.position = pos;
    
    output.texCoord = input.position;

    return output;
}