cbuffer Uniforms : register(b0, space1) {
    float4 Position;    // xy = top-left in pixels
    float4 Size;        // xy = size in pixels
    float4 RenderSize;  // xy = render target size in pixels
    float4 Clip;        // xy = clip min, zw = clip max (pixels)
};

struct VertexOutput {
    float2 UV : TEXCOORD0;
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
    
    uint vert = triangleIndices[vertexID % 6];
    
    float2 pos = vertexPos[vert] * Size.xy + Position.xy;
    float2 clipped = min(max(pos, Clip.xy), Clip.zw);
    
    float2 clipSpace = clipped / RenderSize.xy * 2 - 1;
    clipSpace.y = -clipSpace.y;
    
    VertexOutput output;
    
    output.Position = float4(clipSpace.xy, Position.z, 1);
    // UV follows the clipped position so edges show the right texels
    // instead of squashing the whole texture into the visible part.
    output.UV = (clipped - Position.xy) / Size.xy;
    
    return output;
}

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 fsMain(float2 uv : TEXCOORD0) : SV_Target {
    return Texture.Sample(Sampler, uv);
}
