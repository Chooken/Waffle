struct VertexOutput {
    float2 UV : TEXCOORD0;
    int TileIndex : TEXCOORD1;
    int PaletteColor : TEXCOORD2;
    int FadeIndex : TEXCOORD3;
    float4 Position : SV_Position;
};

Texture2D<float4> TileSheet : register(t0, space2);
SamplerState TileSampler : register(s0, space2);

Texture2D<float4> ColorPalette : register(t1, space2);
SamplerState PaletteSampler : register(s1, space2);

float4 main(VertexOutput input) : SV_Target {

    if (input.TileIndex == 0 || input.FadeIndex > 2)
        discard;

    if (input.TileIndex == 0 || input.FadeIndex > 2)
        return ColorPalette[int2(input.PaletteColor, input.FadeIndex)];

    input.TileIndex -= 1;

    input.UV.y = 1 - input.UV.y;

    float alpha = TileSheet.Sample(TileSampler, input.UV / 8 + float2(
        float(input.TileIndex % 8) * 1.0f / 8.0f,
        floor(float(input.TileIndex / 8)) * 1.0f / 8.0f)).r;
    
    return lerp(ColorPalette[int2(8, 0)], ColorPalette[int2(input.PaletteColor, input.FadeIndex)], alpha);
}