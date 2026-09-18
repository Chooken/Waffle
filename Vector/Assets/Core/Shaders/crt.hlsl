cbuffer VertUniforms : register(b0, space1) {
    float3 v_Position;
    float2 v_Size;
    float4 v_Color;
    float4 v_BorderRadius;
    float4 v_BorderColor;
    float2 v_RenderSize;
    float2 v_RefRes;
    float v_ChromaticAberration;
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
    float2 f_RefRes;
    float f_ChromaticAberration;
    float f_BorderSize;
}

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

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

    float2 uv = float2(input.UV.x, (floor(input.UV.y * f_RefRes.y) + 0.5f) / f_RefRes.y);
    float2 uv2 = (float2(input.UV.x, 1 - input.UV.y) * f_RefRes % 1 - 0.5f) * 2;

    float3 color;

    color.r = Texture.Sample(Sampler, saturate(uv - float2(f_ChromaticAberration / f_RefRes.x, 0))).r;
    color.gb = Texture.Sample(Sampler, uv).gb;

    float3 outputColor;

    float minHeight = max(0.2f, 1 / (f_Size.y / f_RefRes.y) * 2);
    
    float height = abs(uv2.y);
    float3 rayHeight = max(color, minHeight);
    
    rayHeight.r = 1 - pow(1 - rayHeight.r, 3);
    rayHeight.g = 1 - pow(1 - rayHeight.g, 3);
    rayHeight.b = 1 - pow(1 - rayHeight.b, 3);
    
    outputColor.r = smoothstep(height, height + minHeight, rayHeight.r);
    outputColor.g = smoothstep(height, height + minHeight, rayHeight.g);
    outputColor.b = smoothstep(height, height + minHeight, rayHeight.b);

    outputColor = lerp(outputColor * color.rgb, f_BorderColor.rgb, saturate(alpha + f_BorderSize) * saturate(f_BorderSize));

    return float4(lerp(max(outputColor, color.rgb * 0.0f), f_Color.rgb, f_Color.a), -alpha);
}