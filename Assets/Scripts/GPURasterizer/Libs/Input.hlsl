#ifndef INPUT_INCLUDED
#define INPUT_INCLUDED

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

#endif
