cbuffer VertUniforms : register(b0, space1) {
    float3 v_Position;
    float2 v_Size;
    float4 v_Color;
    float4 v_BorderRadius;
    float4 v_BorderColor;
    float2 v_RenderSize;
    float v_BorderSize;
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
    pos.x = (vertexPos[vert].x) * v_Size.x + v_Position.x;
    pos.y = v_RenderSize.y - (vertexPos[vert].y) * v_Size.y - v_Position.y;
    
    float2 clipSpace = pos / v_RenderSize * 2 - 1;
    
    output.Position = float4(clipSpace, 0, 1);
    output.SpriteIndex = spriteIndex;
    output.UV = vertexPos[vert];
    
    return output;
}

cbuffer FragUniforms : register(b0, space3) {
    float3 f_Position;
    float2 f_Size;
    float4 f_Color;
    float4 f_BorderRadius;
    float4 f_BorderColor;
    float2 f_RenderSize;
    float f_BorderSize;
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

    float alpha = roundedBoxSDF((input.UV - 0.5f) * f_Size, f_Size * 0.5f, f_BorderRadius);
    
    float4 color = lerp(f_Color, f_BorderColor, saturate(alpha + f_BorderSize) * saturate(f_BorderSize));

    return float4(color.rgb, color.a * -alpha);
}