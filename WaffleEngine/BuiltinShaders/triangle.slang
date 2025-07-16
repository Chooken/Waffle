uniform Texture2D texture;
uniform SamplerState sampler;

struct VertexInput {
    float3 Position;
    float2 UV;
}

struct VertexOutput {
    float2 UV : UV;
    float4 Position : SV_Position;
}

[shader("vertex")]
VertexOutput vsMain(VertexInput input) {
    VertexOutput out;
    out.Position = float4(input.Position, 1);
    out.UV = input.UV;
    return out;
}

[shader("pixel")]
float4 fsMain(float2 uv : UV) : SV_Target {
    return texture.Sample(sampler, uv);
    //return float4(1,0,1,1);
}