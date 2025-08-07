uniform Texture2D source;
uniform SamplerState sampler;

struct VertexToFragment {
    float2 UV : UV;
    float4 Position : SV_Position;
}

[shader("vertex")]
VertexToFragment vsMain(uint vertexID : SV_VertexID) {

    VertexToFragment out;
    
    out.UV = float2((vertexID << 1) & 2, vertexID & 2);
    out.Position = float4(out.UV * float2(2, -2) + float2(-1, 1), 0, 1);
    
    return out;
}

[shader("pixel")]
float4 fsMain(float2 uv : UV) : SV_Target {
    float4 color = source.Sample(sampler, uv);

    return float4(color.rgb / color.a, color.a);
}