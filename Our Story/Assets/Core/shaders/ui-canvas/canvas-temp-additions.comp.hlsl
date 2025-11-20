cbuffer Uniforms : register (b0, space2)
{
    int Length;
}

struct TempPixel 
{
    float4 Color;
    float2 Position;
};

StructuredBuffer<TempPixel> TempPixels : register(t0, space0);
RWTexture2D<float4> Canvas : register(u0, space1);

[numthreads(64, 1, 1)]
void main(uint3 GlobalInvocationID : SV_DispatchThreadID) 
{
    if (GlobalInvocationID.x >= Length)
        return;
    
    TempPixel temp_pixel = TempPixels[GlobalInvocationID.x];
    
    int2 coord = int2(temp_pixel.Position);
    
    Canvas[coord] = temp_pixel.Color;
}