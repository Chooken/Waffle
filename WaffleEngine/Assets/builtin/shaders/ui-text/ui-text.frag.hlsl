struct VertexOutput {
    float4 Color: TEXCOORD0;
    float2 UV : TEXCOORD1;
    float4 Position : SV_Position;
};

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 main(VertexOutput input) : SV_Target {
    return input.Color * Texture.Sample(Sampler, input.UV);
}