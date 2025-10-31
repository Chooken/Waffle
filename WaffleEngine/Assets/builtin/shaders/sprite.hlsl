struct Sprite {
    float4x4 TranslationMatrix;
    float2 Size;
};

struct VertexOutput {
    float2 UV : TEXCOORD0;
    float4 Position : SV_Position;
};

static const uint triangleIndices[6] = {0, 1, 2, 3, 2, 1};
static const float2 vertexPos[4] = {
    {0.0f, 0.0f},
    {1.0f, 0.0f},
    {0.0f, 1.0f},
    {1.0f, 1.0f}
};

Texture2D<float4> Texture : register(t0, space0);
SamplerState Sampler : register(s0, space0);
StructuredBuffer<Sprite> SpriteBuffer : register(t1, space0);

VertexOutput vsMain(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID) {
    
    uint vert = triangleIndices[vertexID % 6];

    Sprite sprite = SpriteBuffer[instanceID];
    
    VertexOutput output;
    
    float2 pos;
    pos.x = vertexPos[vert].x * sprite.Size.x - sprite.Size.x * 0.5f;
    pos.y = (1 - vertexPos[vert].y) * sprite.Size.y  - sprite.Size.y * 0.5f;
    
    output.Position = mul(sprite.TranslationMatrix, float4(pos, 0, 1));
    output.UV = vertexPos[vert];
    
    return output;
}

float4 fsMain(float2 uv : TEXCOORD0) : SV_Target {
    return Texture.Sample(Sampler, uv);
}