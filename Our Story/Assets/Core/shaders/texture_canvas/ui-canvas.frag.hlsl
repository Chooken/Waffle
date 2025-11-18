cbuffer Uniforms : register(b0, space3) {
    float4 SelectedColor;
    float2 CursorPosition;
    float2 TextureSize;
}

struct VertexToFragment {
    float2 UV : TEXCOORD0;
    float4 Position : SV_Position;
};

float4 main(float2 uv : TEXCOORD0) : SV_Target0 {
    float4 color;
    float2 pixelPos = floor(uv * TextureSize);

    if (pixelPos.x == CursorPosition.x && pixelPos.y == CursorPosition.y)
    {
        color = SelectedColor;
    }
    
    return float4(color.rgb / color.a, color.a);
}