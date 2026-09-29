// Gradient track for color sliders. Procedural quad (SV_VertexID) whose
// color is computed per-pixel in Oklch, so hue/lightness/chroma ramps are
// always correct with nothing baked on the CPU. Forward Oklch needs only
// cubes (no cbrt), keeping it cheap.
//
// Modes (x): 0 = hue ring (H = t*360, L/C from params),
//            1 = lightness ramp (L = t, C/H from params),
//            2 = chroma ramp (C = t*0.4, L/H from params).
// Params = current (L, C, H degrees); unused channels ignored per mode.
// Uniforms are float4-only so the C# side (7 x Vector4) matches byte-for-byte.
cbuffer VertUniforms : register(b0, space1) {
    float4 v_Position;    // xy = top-left in pixels
    float4 v_Size;        // xy = size in pixels
    float4 v_RenderSize;  // xy = render target size in pixels
    float4 v_Clip;        // xy = clip min, zw = clip max (pixels)
    float4 v_Mode;        // x = ramp mode
    float4 v_Params;      // xyz = (L, C, H degrees)
    float4 v_Pad;
}

struct VertexOutput {
    float T : TEXCOORD0;
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

VertexOutput vsMain(uint vertexID : SV_VertexID) {
    uint vert = triangleIndices[vertexID % 6];

    float2 pos = vertexPos[vert] * v_Size.xy + v_Position.xy;
    float2 clipped = min(max(pos, v_Clip.xy), v_Clip.zw);

    float2 clipSpace = clipped / v_RenderSize.xy * 2 - 1;
    clipSpace.y = -clipSpace.y;

    VertexOutput output;
    output.Position = float4(clipSpace.xy, v_Position.z, 1);
    output.T = vertexPos[vert].x;
    output.UV = (clipped - v_Position.xy) / v_Size.xy;
    return output;
}

cbuffer FragUniforms : register(b0, space3) {
    float4 f_Position;
    float4 f_Size;
    float4 f_RenderSize;
    float4 f_Clip;
    float4 f_Mode;
    float4 f_Params;
    float4 f_Pad;
}

float3 OklchToLinear(float L, float C, float Hdeg) {
    float h = radians(Hdeg);
    float a = C * cos(h);
    float b = C * sin(h);
    float l_ = L + 0.3963377774 * a + 0.2158037573 * b;
    float m_ = L - 0.1055613458 * a - 0.0638541728 * b;
    float s_ = L - 0.0894841775 * a - 1.2914855480 * b;
    float l = l_ * l_ * l_;
    float m = m_ * m_ * m_;
    float s = s_ * s_ * s_;
    return float3(
        +4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s,
        -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s,
        -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s);
}

float3 LinearToGamma(float3 c) {
    c = max(c, 0.0);
    float3 lo = c * 12.92;
    float3 hi = 1.055 * pow(c, 1.0 / 2.4) - 0.055;
    return lerp(hi, lo, step(c, 0.0031308));
}

float RoundedBox(float2 uv, float2 halfSize, float radius) {
    float2 q = abs(uv) - halfSize + radius;
    return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
}

float4 fsMain(VertexOutput input) : SV_Target {
    float t = saturate(input.T);
    float L = f_Params.x;
    float C = f_Params.y;
    float H = f_Params.z;

    if (f_Mode.x < 0.5) {
        H = t * 360.0;
        C = 0.25;
    } else if (f_Mode.x < 1.5) {
        L = t;
    } else {
        C = t * 0.4;
    }

    float3 lin = OklchToLinear(L, C, H);
    float3 srgb = LinearToGamma(lin);

    float2 halfSize = f_Size.xy * 0.5;
    float sdf = RoundedBox((input.UV - 0.5) * f_Size.xy, halfSize, 2.0);
    float coverage = saturate(-sdf);

    return float4(srgb, coverage);
}
