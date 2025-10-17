cbuffer Uniforms : register(b0, space1) {
    float3 Position;
    float2 RenderSize;
};

struct VertexInput {
    float3 Position : TEXCOORD0;
    float2 UV : TEXCOORD1;
};

struct VertexOutput {
    float2 UV : TEXCOORD0;
    float4 Position : SV_Position;
};

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

VertexOutput vsMain(VertexInput input) {
    VertexOutput output;
    output.Position = float4(
        (float)((int)(input.Position.x + Position.x)) / RenderSize.x * 2 - 1, 
        (float)((int)(input.Position.y + (RenderSize.y - Position.y))) / RenderSize.y * 2 - 1, 
        Position.z, 1);
    output.UV = input.UV;
    return output;
}

float4 fsMain(float2 uv : TEXCOORD0) : SV_Target {
    return Texture.Sample(Sampler, uv);
}