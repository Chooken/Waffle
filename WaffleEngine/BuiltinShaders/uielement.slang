struct UIElement {
    float3 Position;
    float2 Size;
    float4 Color;
}

struct VertexOutput {
    float2 UV : UV;
    float4 Color : Color;
    float4 Position : SV_Position;
}

StructuredBuffer<UIElement> uiElements;

static const uint triangleIndices[6] = {0, 1, 2, 3, 2, 1};
static const float2 vertexPos[4] = {
    {0.0f, 0.0f},
    {1.0f, 0.0f},
    {0.0f, 1.0f},
    {1.0f, 1.0f}
};

[shader("vertex")]
VertexOutput vsMain(uint vertexID : SV_VertexID) {
    uint spriteIndex = vertexID / 6;
    uint vert = triangleIndices[vertexID % 6];
    
    UIElement element = uiElements[spriteIndex];
    
    VertexOutput output;
    
    output.Position = float4(vertexPos[vert], 0, 1);
    //output.Color = element.Color;
    output.UV = vertexPos[vert];
    
    return output;
}

[shader("pixel")]
float4 fsMain(float2 uv : UV, float4 color : Color) : SV_Target {
    return float4(1,0,1,1);
}