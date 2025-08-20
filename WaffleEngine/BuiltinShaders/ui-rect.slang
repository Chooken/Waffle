cbuffer Uniforms 
{
    float2 ViewSize;
}

struct UIElement {
    float3 Position;
    float2 Size;
    float4 Color;
    float4 BorderRadius;
}

struct VertexOutput {
    float2 UV : UV;
    uint SpriteIndex;
    float4 Position : SV_Position;
}

StructuredBuffer<UIElement> uiElements;

static const uint triangleIndices[6] = {0, 1, 2, 3, 2, 1};
static const float2 vertexPos[4] = {
    {0.0f, 0.0f},
    {1.0f, 0.0f},
    {0.0f, 1.0f},
    {1.0f, 1.0f}
};

[shader("vertex")]
VertexOutput vsMain(uint vertexID : SV_VertexID) {
    uint spriteIndex = vertexID / 6;
    uint vert = triangleIndices[vertexID % 6];
    
    UIElement element = uiElements[spriteIndex];
    
    VertexOutput output;
    
    float2 pos;
    pos.x = vertexPos[vert].x * element.Size.x + element.Position.x;
    pos.y = vertexPos[vert].y * element.Size.y + element.Position.y;
    
    float2 clipSpace = pos / ViewSize * 2 - 1;
    
    output.Position = float4(clipSpace, 0, 1);
    output.SpriteIndex = spriteIndex;
    output.UV = vertexPos[vert];
    
    return output;
}

float roundedBoxSDF(float2 uv, float2 halfSize, float4 corners) {

    // Select radius based on quadrant (r.xy = right, r.zw = left)
    corners.xy = (uv.x < 0.0) ? corners.xy : corners.zw;
    corners.x  = (uv.y > 0.0) ? corners.x  : corners.y;
    
    // Calculate SDF
    float2 position = abs(uv) - halfSize + corners.x;
    return length(max(position, 0.0)) + min(max(position.x, position.y), 0.0) - corners.x;
}

[shader("pixel")]
float4 fsMain(VertexOutput input) : SV_Target {

    UIElement element = uiElements[input.SpriteIndex];

    float alpha = roundedBoxSDF((input.UV - 0.5f) * element.Size, element.Size * 0.5f, element.BorderRadius);

    return float4(element.Color.rgb, -alpha);
}