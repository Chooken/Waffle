cbuffer Uniforms : register(b0, space1) {
    float4 SelectedColor;
    float2 CursorPosition;
    float2 TextureSize;
}

struct VertexToFragment {
    float2 UV : TEXCOORD0;
    float4 Position : SV_Position;
};

VertexToFragment main(uint vertexID : SV_VertexID) {

    VertexToFragment output;
    
    output.UV = float2((vertexID << 1) & 2, vertexID & 2);
    output.Position = float4(output.UV * float2(2, -2) + float2(-1, 1), 0, 1);
    
    return output;
}