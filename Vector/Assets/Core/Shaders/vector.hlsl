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

int solve(float2 pixelPos, float2 p0, float2 p1, float2 p2)
{
    float a = p0.y - 2.0f * p1.y + p2.y;
    float b = 2.0f * (p1.y - p0.y);
    float c = p0.y - pixelPos.y;

    float disc = b * b - 4.0f * a * c;
    
    float signB = (b >= 0.0f) ? 1.0f : -1.0f;
    float q = -0.5f * (b + signB * sqrt(max(disc, 0.0f)));

    float safeA = (a == 0.0f) ? 1e-20f : a;
    float safeQ = (q == 0.0f) ? 1e-20f : q;

    float t0 = q / safeA;
    float t1 = c / safeQ;

    bool validDisc = disc >= 0.0f;
    bool validT0 = validDisc && (t0 >= 0.0f && t0 < 1.0f);
    bool validT1 = validDisc && (t1 >= 0.0f && t1 < 1.0f);
    
    float t = validT0 ? t0 : (validT1 ? t1 : 0.0f);

    float x01 = lerp(p0.x, p1.x, t);
    float x12 = lerp(p1.x, p2.x, t);
    float x = lerp(x01, x12, t);

    bool isToRight = x >= pixelPos.x;
    int isValid = ((validT0 || validT1) && isToRight) ? 1 : 0;

    float dY = 2.0f * a * t + b;
    int winding = sign(dY) * isValid;
    
    return winding;
}

float4 fsMain(VertexOutput input) : SV_Target {
    
    Instance instance = f_InstanceBuffer[input.Instance];
    
    if (instance.Length < 2)
    {
        return float4(0, 0, 0, 1);
    }
    
    float2 pos;
    pos.x = lerp(instance.Min.x, instance.Max.x, input.UV.x);
    pos.y = lerp(instance.Min.y, instance.Max.y, input.UV.y);
    
    int winding = 0;
    
    for (int i = 0; i < instance.Length; i += 2)
    {
        Point start = f_PointBuffer[instance.Offset + i];
        Point control = f_PointBuffer[instance.Offset + i + 1];
        
        int end_index = (i + 2 < instance.Length) ? i + 2 : 0;
        
        Point end = f_PointBuffer[instance.Offset + end_index];

        float max_x = max(max(start.Position.x, control.Position.x), end.Position.x);

        bool y_hit = (start.Position.y <= pos.y && pos.y < start.Position.y) ||
            (start.Position.y >= pos.y && pos.y > start.Position.y);
        
        // Early out if the whole curve is to the left or above or below y band.
        if (max_x < pos.x || y_hit) continue;
        
        winding += solve(pos, start.Position, control.Position, end.Position);
    }
    
    if (winding == 0)
    {
        discard;
    }
    
    return float4(0, 1, 0, 1);
}