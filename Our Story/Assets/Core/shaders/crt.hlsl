cbuffer UIElement : register(b0, space1) {
    float3 Position;
    float2 Size;
    float4 Color;
    float4 BorderRadius;
    float4 BorderColor;
    float2 RenderSize;
    float2 RefRes;
    float ChromaticAberration;
    float BorderSize;
}

struct VertexOutput {
    float2 UV : TEXCOORD0;
    uint SpriteIndex : TEXCOORD1;
    float4 Color : TEXCOORD2;
    float4 Position : SV_Position;
};

static const uint triangleIndices[6] = {0, 1, 2, 3, 2, 1};
static const float2 vertexPos[4] = {
    {0.0f, 0.0f},
    {1.0f, 0.0f},
    {0.0f, 1.0f},
    {1.0f, 1.0f}
};

Texture2D<float4> Texture : register(t0, space0);
SamplerState Sampler : register(s0, space0);

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
    output.Color = Color;
    
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

    float2 uv = float2(input.UV.x, (floor(input.UV.y * RefRes.y) + 0.5f) / RefRes.y);
    float2 uv2 = (float2(input.UV.x, 1 - input.UV.y) * RefRes % 1 - 0.5f) * 2;

    float3 color;

    color.r = Texture.Sample(Sampler, saturate(uv - float2(ChromaticAberration / RefRes.x, 0))).r;
    color.gb = Texture.Sample(Sampler, uv).gb;

    float3 outputColor;

    float minHeight = max(0.2f, 1 / (Size.y / RefRes.y) * 2);
    
    float height = abs(uv2.y);
    float3 rayHeight = max(color, minHeight);
    
    rayHeight.r = 1 - pow(1 - rayHeight.r, 3);
    rayHeight.g = 1 - pow(1 - rayHeight.g, 3);
    rayHeight.b = 1 - pow(1 - rayHeight.b, 3);
    
    outputColor.r = smoothstep(height, height + minHeight, rayHeight.r);
    outputColor.g = smoothstep(height, height + minHeight, rayHeight.g);
    outputColor.b = smoothstep(height, height + minHeight, rayHeight.b);

    outputColor = lerp(outputColor * color.rgb, BorderColor.rgb, saturate(alpha + BorderSize) * saturate(BorderSize));

    return float4(lerp(max(outputColor, color.rgb * 0.0f), input.Color.rgb, input.Color.a), -alpha);
}