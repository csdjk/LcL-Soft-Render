#include "Input.hlsl"
#include "Common.hlsl"


// 插值顶点属性(裁剪三角形的时候用)
VertexOutput InterpolateVertexOutputs(VertexOutput start, VertexOutput end, float t)
{
    VertexOutput result = (VertexOutput)0;
    result.positionCS = lerp(start.positionCS, end.positionCS, t);
    result.positionOS = lerp(start.positionOS, end.positionOS, t);
    result.normalWS = lerp(start.normalWS, end.normalWS, t);
    result.tangent = lerp(start.tangent, end.tangent, t);
    result.color = lerp(start.color, end.color, t);
    result.uv = lerp(start.uv, end.uv, t);
    result.viewDir = lerp(start.viewDir, end.viewDir, t);
    return result;
}

// 插值顶点属性(重心坐标插值)
VertexOutput InterpolateVertexOutputs(VertexOutput v0, VertexOutput v1, VertexOutput v2, float3 barycentric)
{
    VertexOutput result = (VertexOutput)0;
    // result.positionCS = barycentric.x * v0.positionCS + barycentric.y * v1.positionCS + barycentric.z * v2.positionCS;
    result.positionOS = barycentric.x * v0.positionOS + barycentric.y * v1.positionOS + barycentric.z * v2.positionOS;
    result.normalWS = barycentric.x * v0.normalWS + barycentric.y * v1.normalWS + barycentric.z * v2.normalWS;
    result.tangent = barycentric.x * v0.tangent + barycentric.y * v1.tangent + barycentric.z * v2.tangent;
    result.color = barycentric.x * v0.color + barycentric.y * v1.color + barycentric.z * v2.color;
    result.uv = barycentric.x * v0.uv + barycentric.y * v1.uv + barycentric.z * v2.uv;
    result.viewDir = barycentric.x * v0.viewDir + barycentric.y * v1.viewDir + barycentric.z * v2.viewDir;
    return result;
}


bool ClipTriangle(VertexOutput vertex0, VertexOutput vertex1, VertexOutput vertex2, out VertexOutput vertices[6], out int numClippedVertices)
{
    // 定义三角形的顶点列表和裁剪后的顶点列表
    vertices[0] = vertex0;
    vertices[1] = vertex1;
    vertices[2] = vertex2;
    
    VertexOutput clippedVertices[MAX_CLIP_VERTEX_COUNT];
    numClippedVertices = 3;

    // 对三角形进行六次裁剪，分别对应于六个裁剪平面
    for (int i = 0; i < 6; i++)
    {
        // 裁剪后的顶点列表
        int numClippedVerticesThisPlane = 0;

        // 对顶点列表进行裁剪
        for (int j = 0; j < numClippedVertices; j++)
        {
            // 获取当前边的起点和终点
            VertexOutput vj = vertices[j];
            VertexOutput vk = vertices[(j + 1) % numClippedVertices];

            // 判断当前边的起点和终点是否在裁剪平面的内侧
            bool vjInside = IsInsidePlane(Planes[i], vj.positionCS);
            bool vkInside = IsInsidePlane(Planes[i], vk.positionCS);
            // 根据起点和终点的位置关系进行裁剪
            if (vjInside && vkInside)
            {
                // 如果起点和终点都在内侧，则将起点添加到裁剪后的顶点列表中
                clippedVertices[numClippedVerticesThisPlane++] = vj;
            }
            else if (vjInside && !vkInside)
            {
                // 如果起点在内侧，终点在外侧，则计算交点并将起点和交点添加到裁剪后的顶点列表中
                float t = dot(Planes[i], vj.positionCS) / dot(Planes[i], vj.positionCS - vk.positionCS);
                clippedVertices[numClippedVerticesThisPlane++] = vj;
                clippedVertices[numClippedVerticesThisPlane++] = InterpolateVertexOutputs(vj, vk, t);
            }
            else if (!vjInside && vkInside)
            {
                // 如果起点在外侧，终点在内侧，则计算交点并将交点添加到裁剪后的顶点列表中
                float t = dot(Planes[i], vj.positionCS) / dot(Planes[i], vj.positionCS - vk.positionCS);
                clippedVertices[numClippedVerticesThisPlane++] = InterpolateVertexOutputs(vj, vk, t);
            }
        }

        // 更新裁剪后的顶点列表和顶点计数器
        numClippedVertices = numClippedVerticesThisPlane;
        for (int j = 0; j < numClippedVertices; j++)
        {
            vertices[j] = clippedVertices[j];
        }

        // 如果裁剪后的顶点列表为空，则表示三角形被完全裁剪，返回 false
        if (numClippedVertices == 0)
        {
            return false;
        }
    }

    // 如果裁剪后的顶点列表为空，则表示三角形被完全裁剪，返回 false
    if (numClippedVertices == 0)
    {
        return false;
    }
    return true;
}

// Bresenham's 画线算法
void DrawLine(float3 v0, float3 v1, float4 color)
{
    int x0 = (int)v0.x;
    int y0 = (int)v0.y;
    int x1 = (int)v1.x;
    int y1 = (int)v1.y;

    int dx = abs(x1 - x0);
    int dy = abs(y1 - y0);
    int sx = x0 < x1 ? 1 : - 1;
    int sy = y0 < y1 ? 1 : - 1;
    int err = dx - dy;

    while (true)
    {
        ColorTexture[int2(x0, y0)] = color;
        if (x0 == x1 && y0 == y1)
        {
            break;
        }

        int e2 = 2 * err;

        if (e2 > - dy)
        {
            err -= dy;
            x0 += sx;
        }

        if (e2 < dx)
        {
            err += dx;
            y0 += sy;
        }
    }
}

void WireFrameTriangle(VertexOutput vertex0, VertexOutput vertex1, VertexOutput vertex2)
{
    float3 ndcPos0;
    float3 ndcPos1;
    float3 ndcPos2;
    float4 screenPos0 = ClipPositionToScreenPosition(vertex0.positionCS, ndcPos0);
    float4 screenPos1 = ClipPositionToScreenPosition(vertex1.positionCS, ndcPos1);
    float4 screenPos2 = ClipPositionToScreenPosition(vertex2.positionCS, ndcPos2);

    DrawLine(screenPos0.xyz, screenPos1.xyz, _BaseColor);
    DrawLine(screenPos1.xyz, screenPos2.xyz, _BaseColor);
    DrawLine(screenPos2.xyz, screenPos0.xyz, _BaseColor);
}

[numthreads(128, 1, 1)]
void WireFrameTriangle(uint3 id : SV_DispatchThreadID)
{
    int3 tri = TriangleBuffer[id.x];
    VertexOutput vertex0 = VertexOutputBuffer[tri.x];
    VertexOutput vertex1 = VertexOutputBuffer[tri.y];
    VertexOutput vertex2 = VertexOutputBuffer[tri.z];

    // 裁剪三角形
    VertexOutput clippedVertices[MAX_CLIP_VERTEX_COUNT];
    int numClippedVertices;
    if (!ClipTriangle(vertex0, vertex1, vertex2, clippedVertices, numClippedVertices))
    {
        return;
    }
    for (int j = 1; j < numClippedVertices - 1; j++)
    {
        WireFrameTriangle(clippedVertices[0], clippedVertices[j], clippedVertices[j + 1]);
    }
}


void RasterizeTriangle(VertexOutput vertex0, VertexOutput vertex1, VertexOutput vertex2)
{
    float3 ndcPos0;
    float3 ndcPos1;
    float3 ndcPos2;
    float4 position0 = ClipPositionToScreenPosition(vertex0.positionCS, ndcPos0);
    float4 position1 = ClipPositionToScreenPosition(vertex1.positionCS, ndcPos1);
    float4 position2 = ClipPositionToScreenPosition(vertex2.positionCS, ndcPos2);

    if (IsCull(ndcPos0, ndcPos1, ndcPos2, _CullMode)) return;

    // 计算三角形的边界框
    int2 bboxMin = (int2)min(position0.xy, min(position1.xy, position2.xy));
    int2 bboxMax = (int2)max(position0.xy, max(position1.xy, position2.xy));

    // 遍历边界框内的每个像素
    for (int y = bboxMin.y; y <= bboxMax.y; y++)
    {
        for (int x = bboxMin.x; x <= bboxMax.x; x++)
        {
            // 计算像素的重心坐标
            float3 barycentric = BarycentricCoordinate(float2(x, y), position0.xy, position1.xy, position2.xy);
            // 如果像素在三角形内，则绘制该像素(NegativeInfinity避免误差)
            if (barycentric.x >= NegativeInfinity && barycentric.y >= NegativeInfinity && barycentric.z >= NegativeInfinity)
            {
                /// ================================ 透视矫正 ================================
                // 推导公式:https://blog.csdn.net/Motarookie/article/details/124284471
                // z是当前像素在摄像机空间中的深度值。
                // 插值校正系数
                float z = 1 / (barycentric.x / position0.w + barycentric.y / position1.w + barycentric.z / position2.w);

                int2 screenPos = int2(x, y);
                /// ================================ 当前像素的深度插值 ================================
                float depth = barycentric.x * position0.z + barycentric.y * position1.z + barycentric.z * position2.z;

                float depthBuffer = GetDepth(screenPos);
                // 深度测试
                if (DepthTest(depth, depthBuffer, _ZTest))
                {
                    // 进行透视矫正
                    barycentric = barycentric / float3(position0.w, position1.w, position2.w) * z;
                    // 插值顶点属性
                    VertexOutput lerpVertex = InterpolateVertexOutputs(vertex0, vertex1, vertex2, barycentric);
                    // InitClip();
                    bool isDiscard = false;
                    float4 color = fragment(lerpVertex, isDiscard);
                    
                    if (!isDiscard)
                    {
                        color = BlendColors(color, GetColor(screenPos), _BlendMode);
                        SetColor(screenPos, color);
                        if (_ZWrite == ZWrite_On)
                        {
                            SetDepth(screenPos, depth);
                        }
                    }
                }
            }
        }
    }
}

/// MSAA
/// https://mynameismjp.wordpress.com/2012/10/24/msaa-overview/
/// https://zhuanlan.zhihu.com/p/554603218
void RasterizeTriangleMSAA(VertexOutput vertex0, VertexOutput vertex1, VertexOutput vertex2, int sampleCount)
{
    float3 ndcPos0;
    float3 ndcPos1;
    float3 ndcPos2;
    float4 position0 = ClipPositionToScreenPosition(vertex0.positionCS, ndcPos0);
    float4 position1 = ClipPositionToScreenPosition(vertex1.positionCS, ndcPos1);
    float4 position2 = ClipPositionToScreenPosition(vertex2.positionCS, ndcPos2);

    if (IsCull(ndcPos0, ndcPos1, ndcPos2, _CullMode)) return;

    // 计算三角形的边界框
    int2 bboxMin = (int2)min(position0.xy, min(position1.xy, position2.xy));
    int2 bboxMax = (int2)max(position0.xy, max(position1.xy, position2.xy));

    // 遍历边界框内的每个像素
    for (int y = bboxMin.y; y <= bboxMax.y; y++)
    {
        for (int x = bboxMin.x; x <= bboxMax.x; x++)
        {
            float4 color = 0;
            bool isShaded = false;
            bool isDiscard = false;
            // 对每个采样点进行采样
            for (int i = 0; i < sampleCount; i++)
            {
                int2 screenPos = int2(x, y);
                // 计算采样点的位置
                float2 samplePos = float2(x, y) + GetSampleOffset(i, sampleCount);
                // 计算像素的重心坐标
                float3 barycentric = BarycentricCoordinate(samplePos, position0.xy, position1.xy, position2.xy);
                // 如果像素在三角形内，则进行采样
                if (barycentric.x >= NegativeInfinity && barycentric.y >= NegativeInfinity && barycentric.z >= NegativeInfinity)
                {
                    // 透视矫正
                    float z = 1 / (barycentric.x / position0.w + barycentric.y / position1.w + barycentric.z / position2.w);
                    float depth = barycentric.x * position0.z + barycentric.y * position1.z + barycentric.z * position2.z;
                    float depthBuffer = GetDepth(screenPos, i);
                    // 深度测试
                    if (DepthTest(depth, depthBuffer, _ZTest))
                    {
                        barycentric = barycentric / float3(position0.w, position1.w, position2.w) * z;
                        // 插值顶点属性
                        VertexOutput lerpVertex = InterpolateVertexOutputs(vertex0, vertex1, vertex2, barycentric);
                        // 每个像素只进行一次片段着色
                        if (!isShaded)
                        {
                            // InitClip();
                            isDiscard = false;
                            float4 fragmentColor = fragment(lerpVertex, isDiscard);
                            float4 blendColor = BlendColors(fragmentColor, GetColor(screenPos, i), _BlendMode);

                            isShaded = true;
                            color = blendColor;
                        }
                        if (!isDiscard)
                        {
                            SetColor(screenPos, color, i);
                            if (_ZWrite == ZWrite_On)
                                SetDepth(screenPos, depth, i);
                        }
                    }
                }
            }
        }
    }
}

// ================================ Clear ================================
[numthreads(8, 8, 1)]
void Clear(uint3 id : SV_DispatchThreadID)
{
    SetColor(id.xy, ClearColor);
    SetDepth(id.xy, 1);
}

[numthreads(8, 8, 1)]
void ClearMSAA(uint3 id : SV_DispatchThreadID)
{
    for (int i = 0; i < _SampleCount; i++)
    {
        SetColor(id.xy, ClearColor, i);
        SetDepth(id.xy, 1, i);
    }
}

// ================================ 顶点着色器 ================================
[numthreads(128, 1, 1)]
void VertexTransform(uint3 id : SV_DispatchThreadID)
{
    VertexOutputBuffer[id.x] = vertex(VertexBuffer[id.x]);
}

// ================================ 光栅化 ================================
[numthreads(128, 1, 1)]
void RasterizeTriangle(uint3 id : SV_DispatchThreadID)
{
    int3 tri = TriangleBuffer[id.x];
    VertexOutput vertex0 = VertexOutputBuffer[tri.x];
    VertexOutput vertex1 = VertexOutputBuffer[tri.y];
    VertexOutput vertex2 = VertexOutputBuffer[tri.z];

    // 裁剪三角形
    VertexOutput clippedVertices[MAX_CLIP_VERTEX_COUNT];
    int numClippedVertices;
    if (!ClipTriangle(vertex0, vertex1, vertex2, clippedVertices, numClippedVertices))
    {
        return;
    }
    for (int j = 1; j < numClippedVertices - 1; j++)
    {
        RasterizeTriangle(clippedVertices[0], clippedVertices[j], clippedVertices[j + 1]);
    }
}


[numthreads(128, 1, 1)]
void RasterizeTriangleMSAA(uint3 id : SV_DispatchThreadID)
{
    int3 tri = TriangleBuffer[id.x];
    VertexOutput vertex0 = VertexOutputBuffer[tri.x];
    VertexOutput vertex1 = VertexOutputBuffer[tri.y];
    VertexOutput vertex2 = VertexOutputBuffer[tri.z];

    // 裁剪三角形
    VertexOutput clippedVertices[MAX_CLIP_VERTEX_COUNT];
    int numClippedVertices;
    if (!ClipTriangle(vertex0, vertex1, vertex2, clippedVertices, numClippedVertices))
    {
        return;
    }
    for (int j = 1; j < numClippedVertices - 1; j++)
    {
        RasterizeTriangleMSAA(clippedVertices[0], clippedVertices[j], clippedVertices[j + 1], _SampleCount);
    }
}

// ================================ Resolve MSAA ================================
[numthreads(8, 8, 1)]
void Resolve(uint3 id : SV_DispatchThreadID)
{
    int2 screenPos = int2(id.xy);
    float4 color = 0;
    for (int i = 0; i < _SampleCount; i++)
    {
        color += GetColor(screenPos, i);
    }
    color /= _SampleCount;
    ColorTexture[screenPos] = color;
}

