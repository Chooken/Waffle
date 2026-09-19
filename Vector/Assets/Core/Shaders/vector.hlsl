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

int evaluate_ray(float2 pos, float2 start, float2 control, float2 end)
{
    float2 p0 = start - pos;
    float2 p1 = control - pos;
    float2 p2 = end - pos;

    float a = p0.y - 2.0 * p1.y + p2.y;
    float b = 2.0 * (p1.y - p0.y);
    float c = p0.y;

    if (abs(a) < 1e-5) {
        if (abs(b) > 1e-5) {
            
            float t = -c / b;
            
            if (t >= 0.0 && t < 1.0) {
                
                float x = (1.0 - t) * (1.0 - t) * p0.x + 2.0 * (1.0 - t) * t * p1.x + t * t * p2.x;
                
                if (x > 0.0)
                {
                    return (b > 0.0) ? 1 : -1;
                }
            }
        }
        return 0;
    }

    float delta = b * b - 4.0 * a * c;
    if (delta < 0.0) return 0;

    float sqrt_delta = sqrt(delta);
    float r1 = (-b - sqrt_delta) / (2.0 * a);
    float r2 = (-b + sqrt_delta) / (2.0 * a);

    int winding = 0;
    float roots[2] = { r1, r2 };

    for (int i = 0; i < 2; i++) {
        
        float t = roots[i];
        
        if (t >= 0.0 && t < 1.0) {
            
            float x = (1.0 - t) * (1.0 - t) * p0.x + 2.0 * (1.0 - t) * t * p1.x + t * t * p2.x;
            
            if (x > 0.0) {
                float dy = 2.0 * a * t + b;
                winding += (dy > 0.0) ? 1 : -1;
            }
        }
    }
    
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
    
    for (int i = 0; i < instance.Length; i++)
    {
        Point point_1 = f_PointBuffer[instance.Offset + i];
        
        if (distance(pos, point_1.Position) < 0.025)
        {
            if (i % 2 == 0)
            {
                return float4(1,0,0,1);
            }
            return float4(0,0,1,1);
        }
    }
    
    int winding = 0;
    
    for (int i = 0; i < instance.Length; i += 2)
    {
        Point start = f_PointBuffer[instance.Offset + i];
        Point control = f_PointBuffer[instance.Offset + i + 1];
        
        int end_index = (i + 2 < instance.Length) ? i + 2 : 0;
        
        Point end = f_PointBuffer[instance.Offset + end_index];
        
        winding += evaluate_ray(pos, start.Position, control.Position, end.Position);
    }
    
    if (winding == 0)
    {
        discard;
    }
    
    return float4(0, 1, 0, 1);
}