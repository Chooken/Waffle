cbuffer UIElement : register(b0, space3) {
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

float4 main(VertexOutput input) : SV_Target {

    float alpha = roundedBoxSDF((input.UV - 0.5f) * Size, Size * 0.5f, BorderRadius);

    float2 uv = float2(input.UV.x, input.UV.y);

    float4 color = Texture.Sample(Sampler, uv);
    
    color = lerp(color, BorderColor, saturate(alpha + BorderSize) * saturate(BorderSize));
    
    return float4(color.rgb, color.a * -alpha);
}