struct Instance
{
    float2 Min;
    float2 Max;
    int Offset;
    int Length;
};

struct Point
{
    float2 Position;
};

StructuredBuffer<Instance> v_InstanceBuffer : register(t0, space0);
StructuredBuffer<Point> v_PointBuffer : register(t1, space0);

struct VertexOutput {
    nointerpolation uint Instance : TEXCOORD0;
    float2 UV : TEXCOORD1;
    float4 Position : SV_Position;
};

static const uint triangleIndices[6] = {0, 1, 2, 3, 2, 1};
static const float2 vertexPos[4] = {
    {0.0f, 0.0f},
    {1.0f, 0.0f},
    {0.0f, 1.0f},
    {1.0f, 1.0f}
};

VertexOutput vsMain(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID) {
    
    uint vert = triangleIndices[vertexID % 6];

    Instance instance = v_InstanceBuffer[instanceID];
    
    VertexOutput output;
    
    float2 pos;
    pos.x = lerp(instance.Min.x, instance.Max.x, vertexPos[vert].x);
    pos.y = lerp(instance.Min.y, instance.Max.y, vertexPos[vert].y);
    
    output.Position = float4(pos, 0, 1);
    output.Instance = instanceID;
    output.UV = vertexPos[vert];
    
    return output;
}

StructuredBuffer<Instance> f_InstanceBuffer : register(t0, space2);
StructuredBuffer<Point> f_PointBuffer : register(t1, space2);

float4 fsMain(VertexOutput input) : SV_Target {
    
    Instance instance = f_InstanceBuffer[input.Instance];
    
    if (instance.Length < 2)
    {
        return float4(0, 0, 0, 1);
    }
    
    float2 pos;
    pos.x = lerp(instance.Min.x, instance.Max.x, input.UV.x);
    pos.y = lerp(instance.Min.y, instance.Max.y, input.UV.y);
    
    Point point_1 = f_PointBuffer[instance.Offset];
    Point point_2 = f_PointBuffer[instance.Offset + 1];
    
    float2 point_1_pos = point_1.Position;
    float2 point_2_pos = point_2.Position;
    
    point_1_pos.y -= pos.y;
    point_2_pos.y -= pos.y;
    
    float ray_start = pos.x;
    
    float intersection = (point_1_pos.x * point_2_pos.y - point_2_pos.x * point_1_pos.y) / (point_2_pos.y - point_1_pos.y);
    
    if (ray_start < intersection)
    {
        return float4(1, 0, 0, 1);
    }
    
    return float4(0, 0, 1, 1);
}