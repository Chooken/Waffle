cbuffer Uniforms : register(b0, space1) {
    float4x4 ViewMatrix;
    float4x4 ProjectionMatrix;
    int PlayerHeight;
}

struct Tile {
    float4 Position;
    int TileIndex;
    int PaletteIndex;
    int Height;
    int Padding;
};

struct VertexOutput {
    float2 UV : TEXCOORD0;
    int TileIndex : TEXCOORD1;
    int PaletteColor : TEXCOORD2;
    int Height : TEXCOORD3;
    float4 Position : SV_Position;
};

static const uint triangleIndices[6] = {0, 1, 2, 3, 2, 1};
static const float2 vertexPos[4] = {
    {0.0f, 0.0f},
    {1.0f, 0.0f},
    {0.0f, 1.0f},
    {1.0f, 1.0f}
};

StructuredBuffer<Tile> TileBuffer : register(t0, space0);

VertexOutput main(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID) {
    
    uint vert = triangleIndices[vertexID % 6];

    Tile tile = TileBuffer[instanceID];
    
    VertexOutput output;
    
    float3 pos;
    pos.x = vertexPos[vert].x + tile.Position.x;
    pos.y = vertexPos[vert].y + tile.Position.y;
    pos.z = tile.Position.z;
    
    output.Position = mul(mul(ProjectionMatrix, ViewMatrix), float4(pos, 1));
    output.TileIndex = tile.TileIndex;
    output.PaletteColor = tile.PaletteIndex;
    output.UV = vertexPos[vert];
    output.Height = abs(tile.Height - PlayerHeight);
    
    return output;
}