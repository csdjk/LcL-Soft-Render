#ifndef INPUT_INCLUDED
#define INPUT_INCLUDED


uint _CullMode;
uint _ZTest;
uint _ZWrite;
uint _BlendMode;
uint _SampleCount;
uint _ScreenZoom;


struct Vertex
{
    float3 position;
    float2 uv;
    float3 normal;
    float4 tangent;
    float4 color;
};
struct VertexOutput
{
    float4 positionCS;
    float4 positionOS;
    float4 normalWS;
    float4 tangent;
    float2 uv;
    float4 color;
    float3 viewDir;
};


// ================================ Debug ================================
struct DebugData
{
    float4 data0;
};
RWStructuredBuffer<DebugData> DebugDataBuffer;
// ================================ Debug ================================

RWTexture2D<float4> ColorTexture;
RWTexture2D<float4> ColorTextureMSAA;
// RWTexture2D<float> DepthTexture;
RWTexture2D<uint> DepthTexture;

StructuredBuffer<Vertex> VertexBuffer;
RWStructuredBuffer<VertexOutput> VertexOutputBuffer;
StructuredBuffer<uint3> TriangleBuffer;

Texture2D<float4> AlbedoTexture;
bool IsMSAA()
{
    return _SampleCount > 1;
}


int2 GetOffset(int2 screenPos, int sampleIndex)
{
    int x = sampleIndex % _ScreenZoom;
    int y = sampleIndex / _ScreenZoom;
    return screenPos * _ScreenZoom + int2(x, y);
}


float GetDepth(int2 screenPos)
{
    return DepthTexture[screenPos];
}
float GetDepth(int2 screenPos, int sampleIndex)
{
    screenPos = GetOffset(screenPos, sampleIndex);
    return GetDepth(screenPos);
}

void SetDepth(int2 screenPos, int depth)
{
    DepthTexture[screenPos] = depth;
}
void SetDepth(int2 screenPos, float depth)
{
    DepthTexture[screenPos] = depth;
}
void SetDepth(int2 screenPos, float depth, int sampleIndex)
{
    screenPos = GetOffset(screenPos, sampleIndex);
    SetDepth(screenPos, depth);
}

float4 GetColor(int2 screenPos)
{
    return ColorTexture[screenPos];
}
float4 GetColor(int2 screenPos, int sampleIndex)
{
    screenPos = GetOffset(screenPos, sampleIndex);
    return ColorTextureMSAA[screenPos];
}
void SetColor(int2 screenPos, float4 color)
{
    ColorTexture[screenPos] = color;
}
void SetColor(int2 screenPos, float4 color, int sampleIndex)
{
    screenPos = GetOffset(screenPos, sampleIndex);
    ColorTextureMSAA[screenPos] = color;
}


float ReverseZ(float depth)
{
    return 1 - depth;
}
#endif
