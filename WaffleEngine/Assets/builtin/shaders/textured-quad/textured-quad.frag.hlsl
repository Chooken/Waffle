struct VertexOutput {
    float2 UV : TEXCOORD0;
    float4 Position : SV_Position;
};

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 main(float2 uv : TEXCOORD0) : SV_Target {
    return Texture.Sample(Sampler, uv);
}