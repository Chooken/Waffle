// Transparency checkerboard locked to canvas texels: squares stay glued to
// asset pixels at any zoom. Grid.x = canvas texel count across the quad,
// Grid.y = texels per square. Uniforms are float4-only so the C# side
// (7 x Vector4) matches byte-for-byte.
cbuffer VertUniforms : register(b0, space1) {
    float4 v_Position;    // xy = top-left in pixels
    float4 v_Size;        // xy = size in pixels
    float4 v_RenderSize;  // xy = render target size in pixels
    float4 v_Clip;        // xy = clip min, zw = clip max (pixels)
    float4 v_Grid;        // x = canvas texels, y = texels per square
    float4 v_ColorA;
    float4 v_ColorB;
}

struct VertexOutput {
    float2 T : TEXCOORD0;
    float4 Position : SV_Position;
};

static const uint triangleIndices[6] = {0, 1, 2, 3, 2, 1};
static const float2 vertexPos[4] = {
    {0.0f, 0.0f},
    {1.0f, 0.0f},
    {0.0f, 1.0f},
    {1.0f, 1.0f}
};

VertexOutput vsMain(uint vertexID : SV_VertexID) {
    uint vert = triangleIndices[vertexID % 6];

    float2 pos = vertexPos[vert] * v_Size.xy + v_Position.xy;
    float2 clipped = min(max(pos, v_Clip.xy), v_Clip.zw);

    float2 clipSpace = clipped / v_RenderSize.xy * 2 - 1;
    clipSpace.y = -clipSpace.y;

    VertexOutput output;
    output.Position = float4(clipSpace.xy, v_Position.z, 1);
    output.T = (clipped - v_Position.xy) / v_Size.xy;
    return output;
}

cbuffer FragUniforms : register(b0, space3) {
    float4 f_Position;
    float4 f_Size;
    float4 f_RenderSize;
    float4 f_Clip;
    float4 f_Grid;
    float4 f_ColorA;
    float4 f_ColorB;
}

float4 fsMain(VertexOutput input) : SV_Target {
    float2 cells = floor(input.T * f_Grid.x / max(f_Grid.y, 1.0));
    float checker = fmod(cells.x + cells.y, 2.0);
    return checker < 0.5 ? f_ColorA : f_ColorB;
}
