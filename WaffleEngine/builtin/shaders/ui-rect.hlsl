cbuffer UIElement : register(b0, space1) {
    float3 Position;
    float2 Size;
    float2 VertexOffset;
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

VertexOutput vsMain(uint vertexID : SV_VertexID) {
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

float roundedBoxSDF(float2 uv, float2 halfSize, float4 corners) {

    // Select radius based on quadrant (r.xy = right, r.zw = left)
    corners.xy = (uv.x < 0.0) ? corners.xy : corners.zw;
    corners.x  = (uv.y > 0.0) ? corners.x  : corners.y;
    
    float radius = min(corners.x, min(halfSize.x, halfSize.y));
    
    // Calculate SDF
    float2 position = abs(uv) - halfSize + radius;
    return length(max(position, 0.0)) + min(max(position.x, position.y), 0.0) - radius;
}

float4 fsMain(VertexOutput input) : SV_Target {

    float alpha = roundedBoxSDF((input.UV - 0.5f) * Size, Size * 0.5f, BorderRadius);
    
    float4 color = lerp(Color, BorderColor, saturate(alpha + BorderSize) * saturate(BorderSize));

    return float4(color.rgb, color.a * -alpha);
}