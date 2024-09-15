
cbuffer MatrixBuffer : register(b0)
{
    matrix projectionMatrix;
    matrix viewMatrix;
    matrix modelMatrix;
};

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

float3 getNormal(float3 normal)
{
    return normalize(mul(normal, (float3x3)modelMatrix));
}

PixelData Main(VertexData input)
{
    PixelData result;
    
    float4 worldPosition = float4(input.position, 1.0f);
    
    worldPosition = mul(worldPosition, modelMatrix);
    worldPosition = mul(worldPosition, viewMatrix);
    worldPosition = mul(worldPosition, projectionMatrix);
    
    result.position = worldPosition;
    result.normal = getNormal(input.normal);
    result.color = float4(input.color, 1.0f);
    result.uv = input.uv;
    
    return result;
}