cbuffer Uniforms : register(b0, space1) {
    float4 SelectedColor;
    float2 CursorPosition;
    float2 TextureSize;
}

struct VertexToFragment {
    float2 UV : TEXCOORD0;
    float4 Position : SV_Position;
};

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

VertexToFragment vsMain(uint vertexID : SV_VertexID) {

    VertexToFragment output;
    
    output.UV = float2((vertexID << 1) & 2, vertexID & 2);
    output.Position = float4(output.UV * float2(2, -2) + float2(-1, 1), 0, 1);
    
    return output;
}

float4 fsMain(float2 uv : TEXCOORD0) : SV_Target0 {
    float4 color = Texture.Sample(Sampler, uv);
    float2 pixelPos = floor(uv * TextureSize);

    if (pixelPos.x == CursorPosition.x && pixelPos.y == CursorPosition.y)
    {
        color = SelectedColor;
    }
    
    return float4(color.rgb / color.a, color.a);
}