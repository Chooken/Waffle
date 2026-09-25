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
    output.UV = pos;
    
    return output;
}

StructuredBuffer<Instance> f_InstanceBuffer : register(t0, space2);
StructuredBuffer<Point> f_PointBuffer : register(t1, space2);

uint CalcRootCode(float y1, float y2, float y3)
{
    // Calculate the root eligibility code for a sample-relative quadratic Bézier curve.
    // Extract the signs of the y coordinates of the three control points.

    uint i1 = asuint(y1) >> 31U;
    uint i2 = asuint(y2) >> 30U;
    uint i3 = asuint(y3) >> 29U;

    uint shift = (i2 & 2U) | (i1 & ~2U);
    shift = (i3 & 4U) | (shift & ~4U);

    // Eligibility is returned in bits 0 and 8.

    return ((0x2E74U >> shift) & 0x0101U);
}

float2 SolveHorizPoly(float2 p1, float2 p2, float2 p3)
{
    // Solve for the values of t where the curve crosses y = 0.
    // The quadratic polynomial in t is given by
    //
    //     a t^2 - 2b t + c,
    //
    // where a = p1.y - 2 p2.y + p3.y, b = p1.y - p2.y, and c = p1.y.
    // The discriminant b^2 - ac is clamped to zero, and imaginary
    // roots are treated as a double root at the global minimum
    // where t = b / a.

    float2 a = p1 - p2 * 2.0 + p3;
    float2 b = p1 - p2;
    float ra = 1.0 / a.y;
    float rb = 0.5 / b.y;

    float d = sqrt(max(b.y * b.y - a.y * p1.y, 0.0));
    float t1 = (b.y - d) * ra;
    float t2 = (b.y + d) * ra;

    // If the polynomial is nearly linear, then solve -2b t + c = 0.

    if (abs(a.y) < 1.0 / 65536.0) t1 = t2 = p1.y * rb;

    // Return the x coordinates where C(t) = 0.

    return (float2((a.x * t1 - b.x * 2.0) * t1 + p1.x, (a.x * t2 - b.x * 2.0) * t2 + p1.x));
}

float2 SolveVertPoly(float2 p1, float2 p2, float2 p3)
{
    // Solve for the values of t where the curve crosses x = 0.

    float2 a = p1 - p2 * 2.0 + p3;
    float2 b = p1 - p2;
    float ra = 1.0 / a.x;
    float rb = 0.5 / b.x;

    float d = sqrt(max(b.x * b.x - a.x * p1.x, 0.0));
    float t1 = (b.x - d) * ra;
    float t2 = (b.x + d) * ra;

    // If the polynomial is nearly linear, then solve -2b t + c = 0.

    if (abs(a.x) < 1.0 / 65536.0) t1 = t2 = p1.x * rb;

    // Return the y coordinates where C(t) = 0.

    return (float2((a.y * t1 - b.y * 2.0) * t1 + p1.y, (a.y * t2 - b.y * 2.0) * t2 + p1.y));
}

float CalcCoverage(float xcov, float ycov, float xwgt, float ywgt)
{
    // Combine coverages from the horizontal and vertical rays using their weights.
    // Absolute values ensure that either winding direction convention works.

    float coverage = max(abs(xcov * xwgt + ycov * ywgt) / max(xwgt + ywgt, 1.0 / 65536.0), min(abs(xcov), abs(ycov)));
    
    coverage = saturate(coverage);

    return (coverage);
}

float4 fsMain(VertexOutput input) : SV_Target {
    
    Instance instance = f_InstanceBuffer[input.Instance];
    
    if (instance.Length < 2)
    {
        return float4(0, 0, 0, 1);
    }
    
    float2 emsPerPixel = fwidth(input.UV);
    float2 pixelsPerEm = 1.0 / emsPerPixel;
    
    float2 pos = input.UV;
    
    float xcov = 0.0;
    float xwgt = 0.0;
    
    for (int i = 0; i < instance.Length; i += 2)
    {
        float2 start = f_PointBuffer[instance.Offset + i].Position - pos;
        float2 control = f_PointBuffer[instance.Offset + i + 1].Position - pos;
        
        int end_index = (i + 2 < instance.Length) ? i + 2 : 0;
        
        float2 end = f_PointBuffer[instance.Offset + end_index].Position - pos;
        
        uint code = CalcRootCode(start.y, control.y, end.y);
        if (code != 0U)
        {
            float2 r = SolveHorizPoly(start, control, end) * pixelsPerEm.x;
            
            if ((code & 1U) != 0U)
            {
                xcov += saturate(r.x + 0.5);
                xwgt = max(xwgt, saturate(1.0 - abs(r.x) * 2.0));
            }

            if (code > 1U)
            {
                xcov -= saturate(r.y + 0.5);
                xwgt = max(xwgt, saturate(1.0 - abs(r.y) * 2.0));
            }
        }
    }
    
    float ycov = 0.0;
    float ywgt = 0.0;
    
    for (int i = 0; i < instance.Length; i += 2)
    {
        float2 start = f_PointBuffer[instance.Offset + i].Position - pos;
        float2 control = f_PointBuffer[instance.Offset + i + 1].Position - pos;
        
        int end_index = (i + 2 < instance.Length) ? i + 2 : 0;
        
        float2 end = f_PointBuffer[instance.Offset + end_index].Position - pos;
        
        uint code = CalcRootCode(start.x, control.x, end.x);
        if (code != 0U)
        {
            float2 r = SolveVertPoly(start, control, end) * pixelsPerEm.x;
            
            if ((code & 1U) != 0U)
            {
                ycov += saturate(r.x + 0.5);
                ywgt = max(xwgt, saturate(1.0 - abs(r.x) * 2.0));
            }

            if (code > 1U)
            {
                ycov -= saturate(r.y + 0.5);
                ywgt = max(xwgt, saturate(1.0 - abs(r.y) * 2.0));
            }
        }
    }
    
    return float4(1, 0, 0, 1) * step(1, CalcCoverage(xcov, ycov, xwgt, ywgt));
}