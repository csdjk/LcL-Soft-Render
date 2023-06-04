// using System.Numerics;
using UnityEngine;
using System.Collections.Generic;
// using Unity.Mathematics;

namespace LcLSoftRender
{
    class CPURasterizer : IRasterizer
    {
        private int m_Width;
        private int m_Height;
        private Vector2Int m_ScreenSize;
        private Matrix4x4 m_Model;
        private Matrix4x4 m_MatrixVP;
        private Matrix4x4 m_MatrixMVP;
        private FrameBuffer m_FrameBuffer;
        // private List<VertexBuffer> m_VertexBuffers = new List<VertexBuffer>();
        // private List<IndexBuffer> m_IndexBuffers = new List<IndexBuffer>();
        private LcLShader m_OverrideShader;


        public CPURasterizer(int width, int height)
        {
            this.m_Width = width;
            this.m_Height = height;
            m_ScreenSize = new Vector2Int(width, height);
            m_FrameBuffer = new FrameBuffer(width, height);
        }

        public Texture ColorTexture
        {
            get => m_FrameBuffer.GetOutputTexture();
        }

        public void CalculateMatriaxMVP(Matrix4x4 model)
        {
            m_Model = model;
            m_MatrixMVP = m_MatrixVP * m_Model;
        }
        public void SetShader(LcLShader shader)
        {
            m_OverrideShader = shader;
        }
        public void SetMatrixVP(Matrix4x4 matrixVP)
        {
            m_MatrixVP = matrixVP;
        }


        /// <summary>
        ///  生成顶点缓冲区
        /// VOB：Vertex Object Buffer
        /// </summary>
        /// <param name="vertices"></param>
        // public void GenVertexBuffer(IEnumerable<Vertex> vertices)
        // {
        //     m_VertexBuffers.Add(new VertexBuffer(vertices));
        // }
        /// <summary>
        /// 生成索引缓冲区
        /// IBO：Index Object Buffer
        /// </summary>
        /// <param name="indices"></param>
        // public void GenIndexBuffer(IEnumerable<int> indices)
        // {
        //     m_IndexBuffers.Add(new IndexBuffer(indices));
        // }


        public void Render()
        {
            // Clear(ClearMask.COLOR | ClearMask.DEPTH);
            // SetView(m_worldToLocalMatrix);

            // m_FrameBuffer.SetColor(0, 0, Color.red);
            // m_FrameBuffer.Apply();
        }


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

        public void DrawElements(RenderObject model, PrimitiveType primitiveType)
        {
            // UnityEngine.Assertions.Assert.IsTrue(m_CurVertexBufferHandle != -1, "No vertex buffer binding");

            switch (primitiveType)
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
            for (int i = 0; i < indexBuffer.Count(); i += 3)
            {
                Vertex v0 = vertexBuffer[indexBuffer[i + 0]];
                Vertex v1 = vertexBuffer[indexBuffer[i + 1]];
                Vertex v2 = vertexBuffer[indexBuffer[i + 2]];

                WireFrameTriangle(v0, v1, v2, model.albedoColor);
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
            var screenPos0 = TransformTool.ModelPositionToScreenPosition(v0.position, m_MatrixMVP, m_ScreenSize);
            var screenPos1 = TransformTool.ModelPositionToScreenPosition(v1.position, m_MatrixMVP, m_ScreenSize);
            var screenPos2 = TransformTool.ModelPositionToScreenPosition(v2.position, m_MatrixMVP, m_ScreenSize);

            DrawLine(screenPos0, screenPos1, color);
            DrawLine(screenPos1, screenPos2, color);
            DrawLine(screenPos2, screenPos0, color);
        }

        /// <summary>
        /// Bresenham's 画线算法
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        private void DrawLine(Vector3 v0, Vector3 v1, Color color)
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
            for (int i = 0; i < indexBuffer.Count(); i += 3)
            {
                Vertex v0 = vertexBuffer[indexBuffer[i + 0]];
                Vertex v1 = vertexBuffer[indexBuffer[i + 1]];
                Vertex v2 = vertexBuffer[indexBuffer[i + 2]];

                RasterizeTriangle(v0, v1, v2);
            }
        }
        /// <summary>
        /// 三角形光栅化(重心坐标法)
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        private void RasterizeTriangle(Vertex v0, Vertex v1, Vertex v2)
        {
            var screenPos0 = TransformTool.ModelPositionToScreenPosition(v0.position, m_MatrixMVP, m_ScreenSize);
            var screenPos1 = TransformTool.ModelPositionToScreenPosition(v1.position, m_MatrixMVP, m_ScreenSize);
            var screenPos2 = TransformTool.ModelPositionToScreenPosition(v2.position, m_MatrixMVP, m_ScreenSize);

            // 计算三角形的边界框
            int minX = (int)Mathf.Min(screenPos0.x, Mathf.Min(screenPos1.x, screenPos2.x));
            int minY = (int)Mathf.Min(screenPos0.y, Mathf.Min(screenPos1.y, screenPos2.y));
            int maxX = (int)Mathf.Max(screenPos0.x, Mathf.Max(screenPos1.x, screenPos2.x));
            int maxY = (int)Mathf.Max(screenPos0.y, Mathf.Max(screenPos1.y, screenPos2.y));

            // 遍历边界框内的每个像素
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    // 计算像素的重心坐标
                    Vector3 barycentric = TransformTool.ScreenPositionToBarycentric(new Vector3(x, y, 0), screenPos0, screenPos1, screenPos2);

                    // 如果像素在三角形内，则绘制该像素
                    if (barycentric.x >= 0 && barycentric.y >= 0 && barycentric.z >= 0)
                    {
                        // 计算像素的深度值
                        float depth = barycentric.x * screenPos0.z + barycentric.y * screenPos1.z + barycentric.z * screenPos2.z;
                        m_FrameBuffer.SetColor(x, y, Color.red);

                        // 如果像素的深度值小于深度缓冲区中的值，则更新深度缓冲区并绘制该像素
                        // if (depth < m_DepthBuffer[x, y])
                        // {
                        //     m_DepthBuffer[x, y] = depth;
                        //     m_FrameBuffer[x, y] = Color.White;
                        // }
                    }
                }
            }
        }

        // private float GetTriangleArea(Vector2Int p1, Vector2Int p2, Vector2Int p3)
        // {
        //     // 计算三角形的面积
        //     return Mathf.Abs(p1 * (p2.y - p3.y) + p2.x * (p3.y - p1.y) + p3.x * (p1.y - p2.y)) / 2;
        // }
        /// <summary>
        /// 光栅化三角形
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        private void RasterTriangle(Vertex v0, Vertex v1, Vertex v2)
        {
            var screenPos0 = TransformTool.ModelPositionToScreenPosition(v0.position, m_MatrixMVP, m_ScreenSize);
            var screenPos1 = TransformTool.ModelPositionToScreenPosition(v1.position, m_MatrixMVP, m_ScreenSize);
            var screenPos2 = TransformTool.ModelPositionToScreenPosition(v2.position, m_MatrixMVP, m_ScreenSize);

            // 对三角形的三个顶点按照 y 坐标从小到大排序
            if (screenPos0.y > screenPos1.y)
            {
                Vertex temp = v0;
                v0 = v1;
                v1 = temp;
                var tempScreenPos = screenPos0;
                screenPos0 = screenPos1;
                screenPos1 = tempScreenPos;
            }
            if (screenPos0.y > screenPos2.y)
            {
                Vertex temp = v0;
                v0 = v2;
                v2 = temp;
                var tempScreenPos = screenPos0;
                screenPos0 = screenPos2;
                screenPos2 = tempScreenPos;
            }
            if (screenPos1.y > screenPos2.y)
            {
                Vertex temp = v1;
                v1 = v2;
                v2 = temp;
                var tempScreenPos = screenPos1;
                screenPos1 = screenPos2;
                screenPos2 = tempScreenPos;
            }

            // 计算三条边的斜率 k0、k1、k2，以及截距 b0、b1、b2
            float k0 = (screenPos1.x - screenPos0.x) / (screenPos1.y - screenPos0.y);
            float k1 = (screenPos2.x - screenPos0.x) / (screenPos2.y - screenPos0.y);
            float k2 = (screenPos2.x - screenPos1.x) / (screenPos2.y - screenPos1.y);
            float b0 = screenPos0.x - k0 * screenPos0.y;
            float b1 = screenPos0.x - k1 * screenPos0.y;
            float b2 = screenPos1.x - k2 * screenPos1.y;

            // 从 v0 开始，按照 y 坐标从小到大的顺序，扫描每一行像素
            for (int y = Mathf.RoundToInt(screenPos0.y); y <= Mathf.RoundToInt(screenPos2.y); y++)
            {
                // 计算出该行与三条边的交点 x0、x1、x2
                float x0 = k0 * y + b0;
                float x1 = k1 * y + b1;
                float x2 = k2 * y + b2;

                // 将 x0、x1、x2 按照 x 坐标从小到大排序，得到两个区间 [x0, x1] 和 [x1, x2]
                float xStart = Mathf.Min(x0, Mathf.Min(x1, x2));
                float xEnd = Mathf.Max(x0, Mathf.Max(x1, x2));
                float xMiddle = x0 + x1 + x2 - xStart - xEnd;

                // 对于每个区间，从左到右遍历每个像素，计算出该像素的颜色值，并将其写入帧缓冲区
                for (int x = Mathf.RoundToInt(xStart); x <= Mathf.RoundToInt(xEnd); x++)
                {
                    float t = (x - xStart) / (xEnd - xStart);
                    if (x < Mathf.RoundToInt(xMiddle))
                    {
                        t = (x - xStart) / (xMiddle - xStart);
                        Color color = Color.Lerp(v0.color, v1.color, t);
                        m_FrameBuffer.SetColor(x, y, color);
                    }
                    else
                    {
                        t = (x - xMiddle) / (xEnd - xMiddle);
                        Color color = Color.Lerp(v1.color, v2.color, t);
                        m_FrameBuffer.SetColor(x, y, color);
                    }
                }
            }
        }

        #endregion
    }
}