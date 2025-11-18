struct VertexToFragment {
    float2 UV : TEXCOORD0;
    float4 Position : SV_Position;
};

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 main(float2 uv : TEXCOORD0) : SV_Target0 {
    float4 color = Texture.Sample(Sampler, uv);
    return float4(color.rgb / color.a, color.a);
}