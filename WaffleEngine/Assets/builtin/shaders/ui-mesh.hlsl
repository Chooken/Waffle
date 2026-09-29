// Generalised UI mesh shader.
//
// textured-quad and ui-rect are procedural (SV_VertexID) and ignore bound
// vertex buffers, so anything that builds real geometry — AtlasedText glyph
// quads today, icons or vector overlays later — needs this instead: it
// consumes the standard Vertex layout (Color, Position, UV, in that order,
// matching mesh.hlsl so pipeline offsets line up) in y-down pixel space.
//
// Uniforms are float4-only so the C# side (UiMeshData = 4 x Vector4) matches
// byte-for-byte with no HLSL packing surprises.
cbuffer VertUniforms : register(b0, space1) {
    float4 v_Offset;      // xy = screen-space pixel offset
    float4 v_RenderSize;  // xy = render target size in pixels
    float4 v_Tint;
    float4 v_Clip;        // xy = clip min, zw = clip max (pixels)
}

struct VertexInput {
    float4 Color : COLOR0;
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexOutput {
    float4 Color : TEXCOORD0;
    float2 UV : TEXCOORD1;
    float4 Position : SV_Position;
};

VertexOutput vsMain(VertexInput input) {
    VertexOutput output;

    float2 pos = input.Position.xy + v_Offset.xy;
    pos = min(max(pos, v_Clip.xy), v_Clip.zw);

    float2 clipSpace = pos / v_RenderSize.xy * 2 - 1;
    clipSpace.y = -clipSpace.y;

    output.Position = float4(clipSpace, input.Position.z, 1);
    output.Color = input.Color;
    output.UV = input.UV;
    return output;
}

cbuffer FragUniforms : register(b0, space3) {
    float4 f_Offset;
    float4 f_RenderSize;
    float4 f_Tint;
    float4 f_Clip;
}

Texture2D<float4> Texture : register(t0, space2);
SamplerState Sampler : register(s0, space2);

float4 fsMain(VertexOutput input) : SV_Target {
    float4 tex = Texture.Sample(Sampler, input.UV);
    return tex * input.Color * f_Tint;
}
