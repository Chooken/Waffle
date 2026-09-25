cbuffer VertUniforms : register(b0, space1) {
    float3 v_Position;
    float2 v_Size;
    float4 v_Color;
    float4 v_BorderRadius;
    float4 v_BorderColor;
    float2 v_RenderSize;
    float2 v_clipMin;
    float2 v_clipMax;
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
    
    int2 pos = vertexPos[vert] * v_Size + v_Position.xy;
    
    int2 clipped = int2(
        min(max(pos.x, v_clipMin.x), v_clipMax.x), 
        min(max(pos.y, v_clipMin.y), v_clipMax.x)
    );
    
    float2 clipSpace = clipped / v_RenderSize * 2 - 1;
    clipSpace.y = -clipSpace.y;
    
    output.Position = float4(clipSpace, 0, 1);
    output.SpriteIndex = spriteIndex;
    output.UV = float2(
        (clipped.x - v_Position.x) / v_Size.x, 
        (clipped.y - v_Position.y) / v_Size.y
    );
    
    return output;
}

cbuffer FragUniforms : register(b0, space3) {
    float3 f_Position;
    float2 f_Size;
    float4 f_Color;
    float4 f_BorderRadius;
    float4 f_BorderColor;
    float2 f_RenderSize;
    float2 f_clipMin;
    float2 f_clipMax;
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