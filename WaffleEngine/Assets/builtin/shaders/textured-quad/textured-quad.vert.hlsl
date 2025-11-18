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

VertexOutput main(VertexInput input) {
    VertexOutput output;
    output.Position = float4(
        (float)((int)(input.Position.x + Position.x)) / RenderSize.x * 2 - 1, 
        (float)((int)(input.Position.y + (RenderSize.y - Position.y))) / RenderSize.y * 2 - 1, 
        Position.z, 1);
    output.UV = input.UV;
    return output;
}