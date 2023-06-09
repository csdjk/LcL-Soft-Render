// using System.Numerics;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace LcLSoftRender
{
    class CPURasterizer : IRasterizer
    {
        PrimitiveType m_PrimitiveType = PrimitiveType.Triangle;
        private int m_Width;
        private int m_Height;
        private int2 m_ScreenSize;
        private float4x4 m_Model;
        private float4x4 m_MatrixVP;
        private float4x4 m_MatrixMVP;
        private FrameBuffer m_FrameBuffer;
        // private List<VertexBuffer> m_VertexBuffers = new List<VertexBuffer>();
        // private List<IndexBuffer> m_IndexBuffers = new List<IndexBuffer>();
        private LcLShader m_OverrideShader;


        public CPURasterizer(int width, int height)
        {
            this.m_Width = width;
            this.m_Height = height;
            m_ScreenSize = new int2(width, height);
            m_FrameBuffer = new FrameBuffer(width, height);
        }

        public Texture ColorTexture
        {
            get => m_FrameBuffer.GetOutputTexture();
        }

        public float4x4 CalculateMatrixMVP(float4x4 model)
        {
            m_Model = model;
            // m_MatrixMVP = m_MatrixVP * m_Model;
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
            foreach (var model in renderObjects)
            {
                model.shader.MatrixM = model.GetMatrixM();
                model.shader.MatrixVP = m_MatrixVP;
                model.shader.MatrixMVP = CalculateMatrixMVP(model.GetMatrixM());
                DrawElements(model);
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
        /// 绘制
        /// </summary>
        /// <param name="model"></param>
        public void DrawElements(RenderObject model)
        {
            switch (m_PrimitiveType)
            {
                case PrimitiveType.Line:
                    DrawWireFrame(model);
                    break;
                case PrimitiveType.Triangle:
                    DrawTriangles(model);
                    break;
            }

            m_FrameBuffer.Apply();
        }

        #region DrawWireFrame

        /// <summary>
        /// 绘制线框
        /// </summary>
        /// <param name="model"></param>
        private void DrawWireFrame(RenderObject model)
        {
            if (model == null) return;

            var vertexBuffer = model.vertexBuffer;
            var indexBuffer = model.indexBuffer;
            var shader = model.shader;
            for (int i = 0; i < indexBuffer.Count(); i += 3)
            {
                Vertex v0 = vertexBuffer[indexBuffer[i + 0]];
                Vertex v1 = vertexBuffer[indexBuffer[i + 1]];
                Vertex v2 = vertexBuffer[indexBuffer[i + 2]];

                WireFrameTriangle(v0, v1, v2, shader.baseColor);
            }
        }

        /// <summary>
        /// 绘制线框三角形
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="color"></param>
        private void WireFrameTriangle(Vertex v0, Vertex v1, Vertex v2, Color color)
        {
            var screenPos0 = TransformTool.ModelPositionToScreenPosition(v0.position, m_MatrixMVP, m_ScreenSize).xyz;
            var screenPos1 = TransformTool.ModelPositionToScreenPosition(v1.position, m_MatrixMVP, m_ScreenSize).xyz;
            var screenPos2 = TransformTool.ModelPositionToScreenPosition(v2.position, m_MatrixMVP, m_ScreenSize).xyz;

            DrawLine(screenPos0, screenPos1, color);
            DrawLine(screenPos1, screenPos2, color);
            DrawLine(screenPos2, screenPos0, color);
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


        /// <summary>
        /// 绘制三角形
        /// </summary>
        private void DrawTriangles(RenderObject model)
        {
            if (model == null) return;

            var vertexBuffer = model.vertexBuffer;
            var indexBuffer = model.indexBuffer;
            var shader = model.shader;
            for (int i = 0; i < indexBuffer.Count(); i += 3)
            {
                Vertex v0 = vertexBuffer[indexBuffer[i + 0]];
                Vertex v1 = vertexBuffer[indexBuffer[i + 1]];
                Vertex v2 = vertexBuffer[indexBuffer[i + 2]];
                // RasterizeTriangle(v0, v1, v2, shader);
                RasterizeTriangle(v0, v1, v2, shader);
            }
        }

        /// <summary>
        /// 三角形光栅化(重心坐标法)
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        private void RasterizeTriangle(Vertex v0, Vertex v1, Vertex v2, LcLShader shader)
        {
            // if (IsBackFace(v0, v1, v2))
            // {
            //     return;
            // }
            var vertex0 = shader.Vertex(v0);
            var vertex1 = shader.Vertex(v1);
            var vertex2 = shader.Vertex(v2);

            var position0 = vertex0.positionCS;
            var position1 = vertex1.positionCS;
            var position2 = vertex2.positionCS;

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
                        // 除以w分量(摄像机空间的Z)，以进行透视矫正
                        barycentric = barycentric / float3(vertex0.positionCS.w, vertex1.positionCS.w, vertex2.positionCS.w);
                        // 除以重心坐标的和进行归一化，以确保它们的和为1
                        barycentric = barycentric / (barycentric.x + barycentric.y + barycentric.z);
                        // barycentric = 1 / (barycentric.x + barycentric.y + barycentric.z);

                        // 计算像素的深度值
                        float depth = barycentric.x * position0.z + barycentric.y * position1.z + barycentric.z * position2.z;
                        var depthBuffer = m_FrameBuffer.GetDepth(x, y);
                        // 如果像素的深度值小于深度缓冲区中的值，则更新深度缓冲区并绘制该像素
                        if (depth < depthBuffer)
                        {
                            var lerpVertex = InterpolateVertexOutputs(vertex0, vertex1, vertex2, barycentric);

                            var color = shader.Fragment(lerpVertex, out bool isDiscard);
                            if (!isDiscard)
                            {
                                m_FrameBuffer.SetColor(x, y, color);
                                m_FrameBuffer.SetDepth(x, y, depth);
                            }
                        }
                    }
                }
            }
        }
        private bool IsClockwise(Vertex v0, Vertex v1, Vertex v2)
        {
            float2 e1 = v1.position.xy - v0.position.xy;
            float2 e2 = v2.position.xy - v0.position.xy;
            return Cross(e1, e2) < 0;
        }
        private float Cross(float2 a, float2 b)
        {
            return a.x * b.y - a.y * b.x;
        }


        /// <summary>
        /// 判断三角形是否为背面三角形
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private bool IsBackFace(Vertex v0, Vertex v1, Vertex v2)
        {
            float3 e1 = v1.position - v0.position;
            float3 e2 = v2.position - v0.position;
            float3 normal = cross(e1, e2);
            float3 viewDir = normalize(v0.position - Global.cameraPosition);
            return dot(normal, viewDir) > 0;
        }

        private VertexOutput InterpolateVertexOutputs(VertexOutput v0, VertexOutput v1, VertexOutput v2, float3 barycentric)
        {

            var result = (VertexOutput)Activator.CreateInstance(v0.GetType());
            // result.positionCS = barycentric.x * v0.positionCS + barycentric.y * v1.positionCS + barycentric.z * v2.positionCS;
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
    }
}