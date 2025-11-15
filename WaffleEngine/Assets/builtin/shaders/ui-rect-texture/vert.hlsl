cbuffer UIElement : register(b0, space1) {
    float3 Position;
    float2 Size;
    float4 Color;
    float4 BorderRadius;
    float4 BorderColor;
    float2 RenderSize;
    float BorderSize;
}

struct VertexOutput {
    float2 UV : TEXCOORD0;
    uint SpriteIndex : TEXCOORD1;
    float4 Position : SV_Position;
};

static const uint triangleIndices[6] = {0, 1, 2, 3, 2, 1};
static const float2 vertexPos[4] = {
    {0.0f, 0.0f},
    {1.0f, 0.0f},
    {0.0f, 1.0f},
    {1.0f, 1.0f}
};

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

VertexOutput main(uint vertexID : SV_VertexID) {
    uint spriteIndex = vertexID / 6;
    uint vert = triangleIndices[vertexID % 6];
    
    VertexOutput output;
    
    int2 pos;
    pos.x = (vertexPos[vert].x) * Size.x + Position.x;
    pos.y = RenderSize.y - (vertexPos[vert].y) * Size.y - Position.y;
    
    float2 clipSpace = pos / RenderSize * 2 - 1;
    
    output.Position = float4(clipSpace, 0, 1);
    output.SpriteIndex = spriteIndex;
    output.UV = vertexPos[vert];
    
    return output;
}