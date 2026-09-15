struct VertexInput {
    float3 Position : TEXCOORD0;
    float2 UV : TEXCOORD1;
};

struct VertexOutput {
    float2 UV : TEXCOORD0;
    float4 Position : SV_Position;
};

VertexOutput vsMain(VertexInput input) {
    VertexOutput output;
    output.Position = float4(input.Position, 1);
    output.UV = input.UV;
    return output;
}

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 fsMain(float2 uv : TEXCOORD0) : SV_Target {
    return Texture.Sample(Sampler, uv);
}