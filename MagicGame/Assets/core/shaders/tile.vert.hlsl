cbuffer Uniforms : register(b0, space1) {
    float4x4 ProjectionMatrix;
}

struct Tile {
    float4 Position;
    int TileIndex;
    int PaletteIndex;
    int FadeIndex;
    int Rotation;
};

struct VertexOutput {
    float2 UV : TEXCOORD0;
    int TileIndex : TEXCOORD1;
    int PaletteColor : TEXCOORD2;
    int FadeIndex : TEXCOORD3;
    float4 Position : SV_Position;
};

static const uint triangleIndices[6] = {0, 1, 3, 3, 1, 2};
static const int2 vertexPos[4] = {
    {0, 0},
    {0, 1},
    {1, 1},
    {1, 0}
};
static const float2 vertexUv[4] = {
    {0.01f, 0.01f},
    {0.01f, 0.99f},
    {0.99f, 0.99f},
    {0.99f, 0.01f}
};

StructuredBuffer<Tile> TileBuffer : register(t0, space0);

VertexOutput main(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID) {
    
    Tile tile = TileBuffer[instanceID];

    uint vert = triangleIndices[vertexID % 6];
    
    VertexOutput output;
    
    float3 pos;
    pos.x = vertexPos[vert].x + tile.Position.x;
    pos.y = vertexPos[vert].y + tile.Position.y;
    pos.z = tile.Position.z;
    
    output.Position = mul(ProjectionMatrix, float4(pos, 1));
    output.TileIndex = tile.TileIndex;
    output.PaletteColor = tile.PaletteIndex;
    output.UV = vertexUv[(vert + tile.Rotation) % 4];
    output.FadeIndex = tile.FadeIndex;
    
    return output;
}