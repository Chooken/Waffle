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
    output.Color = element.Color;
    output.UV = vertexPos[vert];
    
    return output;
}

float roundedBoxSDF(float2 uv, float2 halfSize, float4 corners) {
    // Select radius based on quadrant (r.xy = right, r.zw = left)
    corners.xy = (uv.x > 0.0) ? corners.xy : corners.zw;
    corners.x  = (uv.y > 0.0) ? corners.x  : corners.y;
    
    // Calculate SDF
    float2 position = abs(uv) - halfSize + corners.x;
    return length(max(position, 0.0)) + min(max(position.x, position.y), 0.0) - corners.x;
}

[shader("pixel")]
float4 fsMain(float2 uv : UV, float4 color : Color) : SV_Target {

    float alpha = roundedBoxSDF(uv - 0.5f, float2(0.5, 0.5), float4(0.25f, 0.25f, 0.25f, 0.25f));
    
    alpha = smoothstep(0, -0.005, alpha);

    return float4(color.rgb, alpha);
}