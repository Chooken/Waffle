cbuffer WaffleUniforms : register(b0, space1) {
    float4x4 MVP;
}

struct VertexInput {
    float4 Color : COLOR0;
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexOutput {
    float4 Color : TEXCOORD0;
    float2 UV : TEXCOORD1;
    float4 Position : SV_Position;
};

VertexOutput vsMain(VertexInput input) {
    VertexOutput output;
    
    output.Position = mul(MVP, input.Position);
    
    output.UV = input.UV;
    output.Color = input.Color;
    return output;
}

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 fsMain(VertexOutput input) : SV_Target {
    return input.Color * Texture.Sample(Sampler, input.UV);
}