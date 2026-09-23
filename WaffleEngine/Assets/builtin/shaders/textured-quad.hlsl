cbuffer Uniforms : register(b0, space1) {
    float3 Position;
    float2 Size;
    float2 RenderSize;
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
    
    int2 pos = vertexPos[vert] * Size + Position;
    
    float2 clipSpace = pos / RenderSize * 2 - 1;
    clipSpace.y = -clipSpace.y;
    
    VertexOutput output;
    
    output.Position = float4(clipSpace.xy, Position.z, 1);
    output.UV = vertexPos[vert];
    
    return output;
}

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 fsMain(float2 uv : TEXCOORD0) : SV_Target {
    return Texture.Sample(Sampler, uv);
}