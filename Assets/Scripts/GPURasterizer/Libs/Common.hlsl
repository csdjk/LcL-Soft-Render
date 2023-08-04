#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED

float4 ClearColor;
float4 ViewportSize;
float4x4 MATRIX_M;
float4x4 MATRIX_VP;
static float4x4 MATRIX_MVP = mul(MATRIX_VP, MATRIX_M);


static const float NegativeInfinity = -0.000001f;

static const int MAX_CLIP_VERTEX_COUNT = 6;
static const float4 Planes[6] = {
    float4(1, 0, 0, 1), // 左平面
    float4(-1, 0, 0, 1), // 右平面
    float4(0, 1, 0, 1), // 下平面
    float4(0, -1, 0, 1), // 上平面
    float4(0, 0, 1, 1), // 近平面
    float4(0, 0, -1, 1)   // 远平面

};
// ================================ CullMode ================================
static const int CullMode_None = 0;
static const int CullMode_Front = 1;
static const int CullMode_Back = 2;

// ================================ ZTest ================================
static const int ZTest_Off = 0;
static const int ZTest_Never = 1;
static const int ZTest_Less = 2;
static const int ZTest_Equal = 3;
static const int ZTest_LessEqual = 4;
static const int ZTest_Greater = 5;
static const int ZTest_NotEqual = 6;
static const int ZTest_GreaterEqual = 7;
static const int ZTest_Always = 8;

static const int ZWrite_Off = 0;
static const int ZWrite_On = 1;

// ================================ BlendMode ================================
static const int BlendMode_None = 0;
static const int BlendMode_Additive = 2;
static const int BlendMode_Subtractive = 3;
static const int BlendMode_PremultipliedAlpha = 4;
static const int BlendMode_Multiply = 5;
static const int BlendMode_Screen = 6;
static const int BlendMode_Overlay = 7;
static const int BlendMode_Darken = 8;
static const int BlendMode_Lighten = 9;
static const int BlendMode_ColorDodge = 10;
static const int BlendMode_ColorBurn = 11;
static const int BlendMode_SoftLight = 12;
static const int BlendMode_HardLight = 13;
static const int BlendMode_Difference = 14;
static const int BlendMode_Exclusion = 15;

float4 ClipPositionToScreenPosition(float4 clipPos, out float3 ndcPos)
{
    // 将裁剪空间中的坐标转换为NDC空间中的坐标
    ndcPos = clipPos.xyz / clipPos.w;
    // 将NDC空间中的坐标转换为屏幕空间中的坐标
    float4 screenPos = float4(
        (ndcPos.x + 1.0f) * 0.5f * (ViewportSize.x - 1),
        (ndcPos.y + 1.0f) * 0.5f * (ViewportSize.y - 1),
        // ndcPos.z * (f - n) / 2 + (f + n) / 2,
        ndcPos.z * 0.5f + 0.5f,
        // w透视矫正系数
        clipPos.w
    );
    return screenPos;
}

float4 TransformObjectToHClip(float3 positionOS)
{
    return mul(MATRIX_MVP, float4(positionOS, 1));
}

float3 TransformObjectToWorldNormal(float3 normal)
{
    return normalize(mul(normal, (float3x3)MATRIX_M));
}



inline bool IsInsidePlane(float4 plane, float4 vertex)
{
    // 推导：
    // 在ndc空间必然满足有：
    // 左裁剪面 (x/w >= -1) => (x >= -w) => (x + w >= 0)
    // 右裁剪面 (x/w <= 1) => (x <= w) => (-x + w >= 0)
    // 下裁剪面 (y/w >= -1) => (y >= -w) => (y + w >= 0)
    // 上裁剪面 (y/w <= 1) => (y <= w) => (-y + w >= 0)
    // 近裁剪面 (z/w >= -1) => (z >= -w) => (z + w >= 0)
    // 远裁剪面 (z/w <= 1) => (z <= w) => (-z + w >= 0)
    // 所以：
    return dot(plane, vertex) >= 0;
}

inline bool IsFrontFace(float3 v0, float3 v1, float3 v2)
{
    float3 e1 = v1 - v0;
    float3 e2 = v2 - v0;
    float3 normal = cross(e1, e2);
    return normal.z < 0;
}
inline bool IsBackFace(float3 v0, float3 v1, float3 v2)
{
    float3 e1 = v1 - v0;
    float3 e2 = v2 - v0;
    float3 normal = cross(e1, e2);
    return normal.z > 0;
}
inline bool IsCull(float3 v0, float3 v1, float3 v2, uint cullMode)
{
    switch(cullMode)
    {
        case CullMode_Back:
            return IsBackFace(v0, v1, v2);
        case CullMode_Front:
            return IsFrontFace(v0, v1, v2);
        case CullMode_None:
            return false;
        default:
            return false;
    }
}

/// <summary>
/// 高效的重心坐标算法
/// (https://github.com/ssloy/tinyrenderer/wiki/Lesson-2:-Triangle-rasterization-and-back-face-culling)
/// </summary>
/// <param name="P"></param>
/// <param name="v0"></param>
/// <param name="v1"></param>
/// <param name="v2"></param>
/// <returns></returns>
float3 BarycentricCoordinate(float2 P, float2 v0, float2 v1, float2 v2)
{
    float2 v2v0 = v2 - v0;
    float2 v1v0 = v1 - v0;
    float2 v0P = v0 - P;
    float3 u = cross(float3(v2v0.x, v1v0.x, v0P.x), float3(v2v0.y, v1v0.y, v0P.y));
    if (abs(u.z) < 1) return float3(-1, 1, 1);
    return float3(1 - (u.x + u.y) / u.z, u.y / u.z, u.x / u.z);
}

bool DepthTest(float depth, float depthBuffer, uint zTest)
{
    switch(zTest)
    {
        case ZTest_Always:
            return true;
        case ZTest_Equal:
            return depth == depthBuffer;
        case ZTest_Greater:
            return depth > depthBuffer;
        case ZTest_GreaterEqual:
            return depth >= depthBuffer;
        case ZTest_Less:
            return depth < depthBuffer;
        case ZTest_LessEqual:
            return depth <= depthBuffer;
        case ZTest_NotEqual:
            return depth != depthBuffer;
        case ZTest_Never:
            return false;
        default:
            return true;
    }
}


float4 BlendColors(float4 srcColor, float4 dstColor, uint blendMode)
{
    switch(blendMode)
    {
        case BlendMode_None:
            return srcColor;
        case BlendMode_Additive:
            return srcColor + dstColor;
        case BlendMode_Subtractive:
            return srcColor - dstColor;
        case BlendMode_PremultipliedAlpha:
            return srcColor + (1 - srcColor.w) * dstColor;
        case BlendMode_Multiply:
            return srcColor * dstColor;
        case BlendMode_Screen:
            return srcColor + dstColor - srcColor * dstColor;
        case BlendMode_Overlay:
            return dstColor.w < 0.5f ? 2 * srcColor * dstColor : 1 - 2 * (1 - srcColor) * (1 - dstColor);
        case BlendMode_Darken:
            return min(srcColor, dstColor);
        case BlendMode_Lighten:
            return max(srcColor, dstColor);
        case BlendMode_ColorDodge:
            return dstColor == 0 ? dstColor : min(1, srcColor / (1 - dstColor));
        case BlendMode_ColorBurn:
            return dstColor == 1 ? dstColor : max(0, (1 - srcColor) / dstColor);
        case BlendMode_SoftLight:
            return dstColor * (2 * srcColor + srcColor * srcColor - 2 * srcColor * dstColor + 2 * dstColor - 2 * dstColor * dstColor);
        case BlendMode_HardLight:
            return srcColor.w < 0.5f ? 2 * dstColor * srcColor : 1 - 2 * (1 - dstColor) * (1 - srcColor);
        case BlendMode_Difference:
            return abs(srcColor - dstColor);
        case BlendMode_Exclusion:
            return srcColor + dstColor - 2 * srcColor * dstColor;
        default:
            return srcColor;
    }
}

float2 GetSampleOffset(int index, int sampleCount)
{
    return float2(0.5 + index) / sampleCount;
}
#endif
