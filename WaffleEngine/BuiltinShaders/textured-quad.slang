uniform Texture2D texture;
uniform SamplerState sampler;

struct Uniforms {
    float3 Position;
    float2 RenderSize;
}

struct VertexInput {
    float3 Position;
    float2 UV;
}

struct VertexOutput {
    float2 UV : UV;
    float4 Position : SV_Position;
}

ConstantBuffer<Uniforms> uniforms;

[shader("vertex")]
VertexOutput vsMain(VertexInput input) {
    VertexOutput out;
    out.Position = float4(
        (float)((int)(input.Position.x + uniforms.Position.x)) / uniforms.RenderSize.x * 2 - 1, 
        (float)((int)(input.Position.y + (uniforms.RenderSize.y - uniforms.Position.y))) / uniforms.RenderSize.y * 2 - 1, 
        uniforms.Position.z, 1);
    out.UV = input.UV;
    return out;
}

[shader("pixel")]
float4 fsMain(float2 uv : UV) : SV_Target {
    return texture.Sample(sampler, uv);
    //return float4(1,0,1,1);
}