// using System.Numerics;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;

namespace LcLSoftRender
{
    public class CPURasterizer : IRasterizer
    {
        PrimitiveType m_PrimitiveType = PrimitiveType.Triangle;
        MSAAMode m_MSAAMode = MSAAMode.MSAA4x;
        private int2 m_ViewportSize;
        private Camera m_Camera;
        private float4x4 m_Model;
        private float4x4 m_MatrixVP;
        public float4x4 MatrixVP => m_MatrixVP;
        private float4x4 m_MatrixMVP;
        public float4x4 MatrixMVP => m_MatrixMVP;
        private FrameBuffer m_FrameBuffer;
        private LcLShader m_OverrideShader;

        public CPURasterizer(Camera camera)
        {

            m_FrameBuffer = new FrameBuffer(camera.pixelWidth, camera.pixelHeight);
            m_Camera = camera;
            m_ViewportSize = new int2(camera.pixelWidth, camera.pixelHeight);
        }

        public Texture ColorTexture
        {
            get => m_FrameBuffer.GetOutputTexture();
        }

        public float4x4 CalculateMatrixMVP(float4x4 model)
        {
            m_Model = model;
            m_MatrixMVP = mul(m_MatrixVP, m_Model);
            return m_MatrixMVP;
        }
        public void SetShader(LcLShader shader)
        {
            m_OverrideShader = shader;
        }
        public void SetMatrixVP(float4x4 matrixVP)
        {
            m_MatrixVP = matrixVP;
        }

        public void SetPrimitiveType(PrimitiveType primitiveType)
        {
            m_PrimitiveType = primitiveType;
        }



        public void Render(List<RenderObject> renderObjects)
        {
            int length = renderObjects.Count;
            for (int i = 0; i < length; i++)
            {
                RenderObject model = renderObjects[i];
                model.shader.MatrixM = model.matrixM;
                model.shader.MatrixVP = m_MatrixVP;
                model.shader.MatrixMVP = CalculateMatrixMVP(model.matrixM);
                if (model.isSkyBox)
                {
                    // 将skybox的位置设置为相机的位置
                    // Matrix4x4 matrixMVP = m_MatrixVP;
                    // matrixMVP[0, 3] = 0;
                    // matrixMVP[1, 3] = 0;
                    // matrixMVP[2, 3] = 0;
                    // model.shader.MatrixMVP = matrixMVP;
                }
                DrawElements(model);

                if (IsDebugging() && i == m_DebugIndex)
                {
                    break;
                }
            }

        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="clearColor"></param>
        /// <param name="depth"></param>
        public void Clear(ClearMask mask, Color? clearColor = null, float depth = float.PositiveInfinity)
        {
            Color realClearColor = clearColor == null ? Color.clear : clearColor.Value;

            m_FrameBuffer.Foreach((x, y) =>
            {
                if ((mask & ClearMask.COLOR) != 0)
                {
                    m_FrameBuffer.SetColor(x, y, realClearColor);
                }
                if ((mask & ClearMask.DEPTH) != 0)
                {
                    m_FrameBuffer.SetDepth(x, y, depth);
                }
            });

            m_FrameBuffer.Apply();
        }

        /// <summary>
        /// 绘制图元
        /// </summary>
        /// <param name="model"></param>
        public void DrawElements(RenderObject model)
        {
            if (model == null) return;

            var vertexBuffer = model.vertexBuffer;
            var indexBuffer = model.indexBuffer;
            var shader = model.shader;
            var count = indexBuffer.Count();
            for (int i = 0; i < count; i += 3)
            {
                Vertex v0 = vertexBuffer[indexBuffer[i + 0]];
                Vertex v1 = vertexBuffer[indexBuffer[i + 1]];
                Vertex v2 = vertexBuffer[indexBuffer[i + 2]];

                var vertex0 = shader.Vertex(v0);
                var vertex1 = shader.Vertex(v1);
                var vertex2 = shader.Vertex(v2);

                // 裁剪三角形
                if (!ClipTriangle(vertex0, vertex1, vertex2, out var clippedVertices))
                {
                    continue;
                }

                // 绘制裁剪后的三角形
                for (int j = 1; j < clippedVertices.Count - 1; j++)
                {
                    switch (m_PrimitiveType)
                    {
                        case PrimitiveType.Line:
                            WireFrameTriangle(clippedVertices[0], clippedVertices[j], clippedVertices[j + 1], shader);
                            break;
                        case PrimitiveType.Triangle:
                            // RasterizeTriangle(clippedVertices[0], clippedVertices[j], clippedVertices[j + 1], shader);
                            RasterizeTriangleMSAA(clippedVertices[0], clippedVertices[j], clippedVertices[j + 1], shader, 4);
                            break;
                    }
                }


            }
            m_FrameBuffer.Apply();
        }

        #region DrawWireFrame

        /// <summary>
        /// 绘制线框三角形
        /// </summary>
        /// <param name="vertex0"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="shader"></param>
        private void WireFrameTriangle(VertexOutput vertex0, VertexOutput vertex1, VertexOutput vertex2, LcLShader shader)
        {
            var position0 = TransformTool.ClipPositionToScreenPosition(vertex0.positionCS, m_Camera, out var ndcPos0);
            var position1 = TransformTool.ClipPositionToScreenPosition(vertex1.positionCS, m_Camera, out var ndcPos1);
            var position2 = TransformTool.ClipPositionToScreenPosition(vertex2.positionCS, m_Camera, out var ndcPos2);
            Debug.Log($"position0:{position0},position1:{position1},position2:{position2}");
            Debug.Log($"ndcPos0:{ndcPos0},ndcPos1:{ndcPos1},ndcPos2:{ndcPos2}");
            if (IsCull(ndcPos0, ndcPos1, ndcPos2, shader.CullMode))
            {
                return;
            }

            DrawLine(position0.xyz, position1.xyz, shader.baseColor);
            DrawLine(position1.xyz, position2.xyz, shader.baseColor);
            DrawLine(position2.xyz, position0.xyz, shader.baseColor);
        }

        /// <summary>
        /// Bresenham's 画线算法
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        private void DrawLine(float3 v0, float3 v1, Color color)
        {
            int x0 = (int)v0.x;
            int y0 = (int)v0.y;
            int x1 = (int)v1.x;
            int y1 = (int)v1.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                m_FrameBuffer.SetColor(x0, y0, color);

                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int e2 = 2 * err;

                if (e2 > -dy)
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

        #endregion



        #region DrawTriangles

        // 定义六个裁剪平面，分别对应于裁剪体的左、右、下、上、近、远平面
        // 在Unity中，裁剪平面的法向量正方向通常指向视锥体的内部
        float4[] planes = new float4[]
        {
                new float4(1, 0, 0, 1),   // 左平面
                new float4(-1, 0, 0, 1),  // 右平面
                new float4(0, 1, 0, 1),   // 下平面
                new float4(0, -1, 0, 1),  // 上平面
                new float4(0, 0, 1, 1),   // 近平面
                new float4(0, 0, -1, 1)   // 远平面
        };


        /// <summary>
        /// 裁剪三角形(Sutherland-Hodgman)
        /// https://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman_algorithm
        /// </summary>
        /// <param name="vertex0"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        bool ClipTriangle(VertexOutput vertex0, VertexOutput vertex1, VertexOutput vertex2, out List<VertexOutput> vertices)
        {
            // 定义三角形的顶点列表和裁剪后的顶点列表
            vertices = new List<VertexOutput> { vertex0, vertex1, vertex2 };
            var clippedVertices = new List<VertexOutput>();
            int numClippedVertices = 3;

            // 对三角形进行六次裁剪，分别对应于六个裁剪平面
            for (int i = 0; i < planes.Length; i++)
            {
                // 裁剪后的顶点列表
                clippedVertices.Clear();

                // 对顶点列表进行裁剪
                for (int j = 0; j < numClippedVertices; j++)
                {
                    // 获取当前边的起点和终点
                    var vj = vertices[j];
                    var vk = vertices[(j + 1) % numClippedVertices];

                    // 判断当前边的起点和终点是否在裁剪平面的内侧
                    bool vjInside = IsInsidePlane(planes[i], vj.positionCS);
                    bool vkInside = IsInsidePlane(planes[i], vk.positionCS);

                    // 根据起点和终点的位置关系进行裁剪
                    if (vjInside && vkInside)
                    {
                        // 如果起点和终点都在内侧，则将起点添加到裁剪后的顶点列表中
                        clippedVertices.Add(vj);
                    }
                    else if (vjInside && !vkInside)
                    {
                        // 如果起点在内侧，终点在外侧，则计算交点并将起点和交点添加到裁剪后的顶点列表中
                        float t = dot(planes[i], vj.positionCS) / dot(planes[i], vj.positionCS - vk.positionCS);
                        clippedVertices.Add(vj);
                        clippedVertices.Add(InterpolateVertexOutputs(vj, vk, t));
                    }
                    else if (!vjInside && vkInside)
                    {
                        // 如果起点在外侧，终点在内侧，则计算交点并将交点添加到裁剪后的顶点列表中
                        float t = dot(planes[i], vj.positionCS) / dot(planes[i], vj.positionCS - vk.positionCS);
                        clippedVertices.Add(InterpolateVertexOutputs(vj, vk, t));
                    }
                }

                // 更新裁剪后的顶点列表和顶点计数器
                numClippedVertices = clippedVertices.Count;
                vertices = clippedVertices.ToList();
            }

            // 如果裁剪后的顶点列表为空，则表示三角形被完全裁剪，返回 false
            if (numClippedVertices == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 顶点插值(两个顶点之间进行插值)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        VertexOutput InterpolateVertexOutputs(VertexOutput start, VertexOutput end, float t)
        {
            var result = (VertexOutput)Activator.CreateInstance(start.GetType());
            // 对每个顶点属性进行插值
            result.positionCS = lerp(start.positionCS, end.positionCS, t);
            result.positionOS = lerp(start.positionOS, end.positionOS, t);
            result.normal = lerp(start.normal, end.normal, t);
            result.tangent = lerp(start.tangent, end.tangent, t);
            result.color = lerp(start.color, end.color, t);
            result.uv = lerp(start.uv, end.uv, t);
            return result;
        }

        /// <summary>
        /// 判断顶点是否在裁剪平面的内侧
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        private bool IsInsidePlane(float4 plane, float4 vertex)
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

        // private float GetIntersectRatio(float4 prev, float4 curv, float4 plane)
        // {
        //     float t = dot(plane, prev) / dot(plane, prev - curv);
        //     return t;
        // }

        /// <summary>
        /// 三角形光栅化(重心坐标法)
        /// </summary>
        /// <param name="vertex0"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        private void RasterizeTriangle(VertexOutput vertex0, VertexOutput vertex1, VertexOutput vertex2, LcLShader shader)
        {
            var position0 = TransformTool.ClipPositionToScreenPosition(vertex0.positionCS, m_Camera, out var ndcPos0);
            var position1 = TransformTool.ClipPositionToScreenPosition(vertex1.positionCS, m_Camera, out var ndcPos1);
            var position2 = TransformTool.ClipPositionToScreenPosition(vertex2.positionCS, m_Camera, out var ndcPos2);

            if (IsCull(ndcPos0, ndcPos1, ndcPos2, shader.CullMode)) return;

            // 计算三角形的边界框
            int2 bboxMin = (int2)min(position0.xy, min(position1.xy, position2.xy));
            int2 bboxMax = (int2)max(position0.xy, max(position1.xy, position2.xy));

            // 遍历边界框内的每个像素
            for (int y = bboxMin.y; y <= bboxMax.y; y++)
            {
                for (int x = bboxMin.x; x <= bboxMax.x; x++)
                {
                    // 计算像素的重心坐标
                    float3 barycentric = TransformTool.BarycentricCoordinate(float2(x, y), position0.xy, position1.xy, position2.xy);
                    // float3 barycentric = TransformTool.BarycentricCoordinate2(float2(x, y), position0.xyz, position1.xyz, position2.xyz);

                    // 如果像素在三角形内，则绘制该像素
                    if (barycentric.x >= 0 && barycentric.y >= 0 && barycentric.z >= 0)
                    {
                        /// ================================ 透视矫正 ================================
                        // 推导公式:https://blog.csdn.net/Motarookie/article/details/124284471
                        // z是当前像素在摄像机空间中的深度值。
                        // 插值校正系数
                        float z = 1 / (barycentric.x / position0.w + barycentric.y / position1.w + barycentric.z / position2.w);


                        /// ================================ 当前像素的深度插值 ================================
                        float depth = barycentric.x * position0.z + barycentric.y * position1.z + barycentric.z * position2.z;

                        var depthBuffer = m_FrameBuffer.GetDepth(x, y);
                        // 深度测试
                        if (Utility.DepthTest(depth, depthBuffer, shader.ZTest))
                        {
                            // 进行透视矫正
                            barycentric = barycentric / float3(position0.w, position1.w, position2.w) * z;
                            // 插值顶点属性
                            var lerpVertex = InterpolateVertexOutputs(vertex0, vertex1, vertex2, barycentric);

                            var isDiscard = shader.Fragment(lerpVertex, out float4 color);
                            if (!isDiscard)
                            {
                                color = Utility.BlendColors(color, m_FrameBuffer.GetColor(x, y), shader.BlendMode);
                                m_FrameBuffer.SetColor(x, y, color);
                                if (shader.ZWrite == ZWrite.On)
                                    m_FrameBuffer.SetDepth(x, y, depth);
                            }
                        }
                    }
                }
            }
        }


        public void RasterizeTriangleMSAA(VertexOutput vertex0, VertexOutput vertex1, VertexOutput vertex2, LcLShader shader, int sampleCount)
        {
            var position0 = TransformTool.ClipPositionToScreenPosition(vertex0.positionCS, m_Camera, out var ndcPos0);
            var position1 = TransformTool.ClipPositionToScreenPosition(vertex1.positionCS, m_Camera, out var ndcPos1);
            var position2 = TransformTool.ClipPositionToScreenPosition(vertex2.positionCS, m_Camera, out var ndcPos2);

            if (IsCull(ndcPos0, ndcPos1, ndcPos2, shader.CullMode)) return;

            // 计算三角形的边界框
            int2 bboxMin = (int2)min(position0.xy, min(position1.xy, position2.xy));
            int2 bboxMax = (int2)max(position0.xy, max(position1.xy, position2.xy));

            // 遍历边界框内的每个像素
            for (int y = bboxMin.y; y <= bboxMax.y; y++)
            {
                for (int x = bboxMin.x; x <= bboxMax.x; x++)
                {
                    float4 color = 0;
                    float depth = 0;
                    bool inside = false;
                    // 对每个采样点进行采样
                    for (int i = 0; i < sampleCount; i++)
                    {
                        // 计算采样点的位置
                        float2 samplePos = float2(x, y) + GetSampleOffset(i, sampleCount);

                        // bool useSample = all(sampleDist <= 1.0f);

                        // 计算像素的重心坐标
                        float3 barycentric = TransformTool.BarycentricCoordinate(samplePos, position0.xy, position1.xy, position2.xy);

                        // 如果像素在三角形内，则进行采样
                        if (barycentric.x >= 0 && barycentric.y >= 0 && barycentric.z >= 0)
                        {
                            // 透视矫正
                            float z = 1 / (barycentric.x / position0.w + barycentric.y / position1.w + barycentric.z / position2.w);
                            barycentric = barycentric / float3(position0.w, position1.w, position2.w) * z;

                            // 插值顶点属性
                            var lerpVertex = InterpolateVertexOutputs(vertex0, vertex1, vertex2, barycentric);
                            color += 1;

                            // // 执行片元着色器
                            // var isDiscard = shader.Fragment(lerpVertex, out float4 sampleColor);
                            // if (!isDiscard)
                            // {
                            //     // 深度测试
                            //     float sampleDepth = barycentric.x * position0.z + barycentric.y * position1.z + barycentric.z * position2.z;
                            //     if (Utility.DepthTest(sampleDepth, depth, shader.ZTest))
                            //     {
                            //         color += sampleColor;
                            //         depth = sampleDepth;
                            //     }
                            // }
                            inside = true;
                        }
                    }

                    if (inside)
                    {
                        color /= sampleCount;
                        m_FrameBuffer.SetColor(x, y, color);
                    }

                    // // 对采样结果进行平均
                    // color /= sampleCount;

                    // // 混合颜色
                    // color = Utility.BlendColors(color, m_FrameBuffer.GetColor(x, y), shader.BlendMode);

                    // // 写入颜色和深度
                    // m_FrameBuffer.SetColor(x, y, color);
                    // if (shader.ZWrite == ZWrite.On)
                    //     m_FrameBuffer.SetDepth(x, y, depth);
                }
            }
        }

        float2[] m_SampleOffsets2x = new float2[]
        {
             float2(-0.25f, -0.25f),
             float2(+0.25f, +0.25f)
        };
        // float2[] m_SampleOffsets4x = new float2[]
        // {
        //      float2(+0.125f, +0.375f),
        //      float2(+0.375f, -0.125f),
        //      float2(-0.125f, -0.375f),
        //      float2(-0.375f, +0.125f)
        // };

        float2[] m_SampleOffsets4x = new float2[]
        {
            float2(0.375f, 0.875f),
            float2( 0.875f, 0.625f),
            float2( 0.125f, 0.375f),
            float2( 0.625f, 0.125f),
        };
        float2[] m_SampleOffsets8x = new float2[]
        {
            float2(-0.375f, +0.375f),
            float2(+0.125f, +0.375f),
            float2(-0.125f, +0.125f),
            float2(+0.375f, +0.125f),
            float2(-0.375f, -0.125f),
            float2(+0.125f, -0.125f),
            float2(-0.125f, -0.375f),
            float2(+0.375f, -0.375f)
        };
        private float2 GetSampleOffset(int index, int sampleCount)
        {
            // 根据采样点的数量和索引计算采样点的偏移量
            switch (sampleCount)
            {
                case 2:
                    return m_SampleOffsets2x[index];
                case 4:
                    return m_SampleOffsets4x[index];
                case 8:
                    return m_SampleOffsets8x[index];
                default:
                    return 0;
            }
        }

        private float2 GetSampleOffset2(int index, int sampleCount)
        {
            // 根据采样点的数量和索引计算采样点的偏移量
            switch (sampleCount)
            {
                case 2:
                    return new float2(index % 2, index / 2) / 2;
                case 4:
                    return new float2(index % 2, index / 2) / 2 + new float2(0.25f, 0.25f);
                case 8:
                    return new float2(index % 4, index / 4) / 4 + new float2(0.125f, 0.125f);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 背面剔除
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="cullMode"></param>
        /// <returns></returns>
        private bool IsCull(float3 v0, float3 v1, float3 v2, CullMode cullMode)
        {
            switch (cullMode)
            {
                case CullMode.Back:
                    return IsBackFace(v0, v1, v2);
                case CullMode.Front:
                    return IsFrontFace(v0, v1, v2);
                case CullMode.None:
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        ///  判断三角形是否为正面三角形
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private bool IsFrontFace(float3 v0, float3 v1, float3 v2)
        {
            float3 e1 = v1 - v0;
            float3 e2 = v2 - v0;
            float3 normal = cross(e1, e2);
            return normal.z < 0;
        }
        /// <summary>
        /// 判断三角形是否为背面三角形
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private bool IsBackFace(float3 v0, float3 v1, float3 v2)
        {
            float3 e1 = v1 - v0;
            float3 e2 = v2 - v0;
            float3 normal = cross(e1, e2);
            return normal.z > 0;
        }

        /// <summary>
        /// 插值顶点属性(重心坐标插值)
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="barycentric"></param>
        /// <returns></returns>
        private VertexOutput InterpolateVertexOutputs(VertexOutput v0, VertexOutput v1, VertexOutput v2, float3 barycentric)
        {

            var result = (VertexOutput)Activator.CreateInstance(v0.GetType());
            // result.positionCS = barycentric.x * v0.positionCS + barycentric.y * v1.positionCS + barycentric.z * v2.positionCS;
            result.positionOS = barycentric.x * v0.positionOS + barycentric.y * v1.positionOS + barycentric.z * v2.positionOS;
            result.normal = barycentric.x * v0.normal + barycentric.y * v1.normal + barycentric.z * v2.normal;
            result.color = barycentric.x * v0.color + barycentric.y * v1.color + barycentric.z * v2.color;
            result.uv = barycentric.x * v0.uv + barycentric.y * v1.uv + barycentric.z * v2.uv;
            return result;
        }


        /// <summary>
        /// 插值(速度太慢了...)
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="barycentric"></param>
        /// <returns></returns>
        public VertexOutput InterpolateVertex(VertexOutput v0, VertexOutput v1, VertexOutput v2, float3 barycentric)
        {
            var interpolated = (VertexOutput)Activator.CreateInstance(v0.GetType());
            // 获取VertexOutput的所有字段
            var fields = typeof(VertexOutput).GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                // 获取字段的类型
                var fieldType = field.FieldType;

                // 如果字段是Vector2类型，则进行插值
                if (fieldType == typeof(float2))
                {
                    var value0 = (float2)field.GetValue(v0);
                    var value1 = (float2)field.GetValue(v1);
                    var value2 = (float2)field.GetValue(v2);
                    var interpolatedValue = barycentric.x * value0 + barycentric.y * value1 + barycentric.z * value2;
                    field.SetValue(interpolated, interpolatedValue);
                }
                // 如果字段是Vector3类型，则进行插值
                else if (fieldType == typeof(float3))
                {
                    var value0 = (float3)field.GetValue(v0);
                    var value1 = (float3)field.GetValue(v1);
                    var value2 = (float3)field.GetValue(v2);
                    var interpolatedValue = barycentric.x * value0 + barycentric.y * value1 + barycentric.z * value2;
                    field.SetValue(interpolated, interpolatedValue);
                }
                else if (fieldType == typeof(float4))
                {
                    var value0 = (float4)field.GetValue(v0);
                    var value1 = (float4)field.GetValue(v1);
                    var value2 = (float4)field.GetValue(v2);
                    var interpolatedValue = barycentric.x * value0 + barycentric.y * value1 + barycentric.z * value2;
                    field.SetValue(interpolated, interpolatedValue);
                }
                // 如果字段是Color类型，则进行插值
                else if (fieldType == typeof(Color))
                {
                    var value0 = (Color)field.GetValue(v0);
                    var value1 = (Color)field.GetValue(v1);
                    var value2 = (Color)field.GetValue(v2);
                    var interpolatedValue = barycentric.x * value0 + barycentric.y * value1 + barycentric.z * value2;
                    field.SetValue(interpolated, interpolatedValue);
                }
                // 如果字段是float类型，则进行插值
                else if (fieldType == typeof(float))
                {
                    var value0 = (float)field.GetValue(v0);
                    var value1 = (float)field.GetValue(v1);
                    var value2 = (float)field.GetValue(v2);
                    var interpolatedValue = barycentric.x * value0 + barycentric.y * value1 + barycentric.z * value2;
                    field.SetValue(interpolated, interpolatedValue);
                }
            }

            return interpolated;
        }

        #endregion


        #region Debugger

        int m_DebugIndex = -1;

        /// <summary>
        /// 设置调试索引
        /// </summary>
        /// <param name="debugIndex"></param>
        public void SetDebugIndex(int debugIndex)
        {
            this.m_DebugIndex = debugIndex;
        }
        public void CloseDebugger()
        {
            m_DebugIndex = -1;
        }
        public bool IsDebugging()
        {
            return m_DebugIndex != -1;
        }
        #endregion




    }
}