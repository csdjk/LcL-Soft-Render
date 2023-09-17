// using System.Numerics;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;

namespace LcLSoftRenderer
{
    public class GPURasterizer : IRasterizer
    {
        PrimitiveType m_PrimitiveType = PrimitiveType.Triangle;
        CameraClearFlags m_ClearFlags = CameraClearFlags.Color;

        MSAAMode m_MSAAMode = MSAAMode.MSAA4x;
        public MSAAMode MSAAMode
        {
            get => m_MSAAMode;
            set
            {
                m_MSAAMode = value;
            }
        }
        public int SampleCount => (int)m_MSAAMode;
        public bool IsMSAA => m_MSAAMode != MSAAMode.None;
        public float4x4 MatrixVP => m_MatrixVP;
        int2 m_ViewportSize;
        int m_ScreenZoom = 1;
        Camera m_Camera;
        float4x4 m_Model;
        float4x4 m_MatrixVP;
        float4x4 m_MatrixMVP;
        public float4x4 MatrixMVP => m_MatrixMVP;

        ComputeShader m_ComputeShader;
        ComputeShader m_CommonShader;
        RenderTexture m_ColorTexture;
        RenderTexture m_ColorMSAATexture;
        RenderTexture m_DepthTexture;
        ComputeBuffer m_VertexOutputBuffer;
        readonly string m_VertexProp = "VertexBuffer";
        readonly string m_VertexOutputProp = "VertexOutputBuffer";
        readonly string m_TriangleProp = "TriangleBuffer";
        readonly string m_ColorProp = "ColorTexture";
        readonly string m_DepthProp = "DepthTexture";
        readonly string m_ClearColorProp = "ClearColor";
        readonly string m_ViewportSizeProp = "ViewportSize";
        readonly string m_SampleCountProp = "_SampleCount";
        readonly string m_ScreenZoomProp = "_ScreenZoom";
        readonly string m_ColorMSAAProp = "ColorTextureMSAA";


        int m_ClearKernelIndex = -1;
        int m_ClearMSAAKernelIndex = -1;
        int m_VertexKernelIndex = -1;
        int m_RasterizeTriangleKernelIndex = -1;
        int m_RasterizeTriangleMSAAKernelIndex = -1;
        int m_WireFrameKernelIndex = -1;
        int m_ResolveKernelIndex = -1;

        int2 textureThreadGroup => (int2)ceil((float2)m_ViewportSize / (float2)8);

        public Texture ColorTexture => m_ColorTexture;

        public GPURasterizer(Camera camera, ComputeShader computeShader, MSAAMode msaaMode = MSAAMode.None)
        {
            m_MSAAMode = msaaMode;
            m_Camera = camera;
            m_ViewportSize = new int2(camera.pixelWidth, camera.pixelHeight);
            m_CommonShader = computeShader;

            m_ClearKernelIndex = m_CommonShader.FindKernel("Clear");
            m_ClearMSAAKernelIndex = m_CommonShader.FindKernel("ClearMSAA");
            m_ResolveKernelIndex = m_CommonShader.FindKernel("Resolve");

            m_ScreenZoom = Mathf.CeilToInt(sqrt((int)m_MSAAMode));
            var msaaRtSize = m_ViewportSize * m_ScreenZoom;
            m_ColorTexture = new RenderTexture(m_ViewportSize.x, m_ViewportSize.y, 0, RenderTextureFormat.ARGBFloat);
            m_ColorTexture.enableRandomWrite = true;
            m_ColorTexture.Create();
            m_CommonShader.SetTexture(m_ClearKernelIndex, m_ColorProp, m_ColorTexture);
            m_CommonShader.SetTexture(m_ClearMSAAKernelIndex, m_ColorProp, m_ColorTexture);

            // Depth Buffer
            // m_DepthTexture = new RenderTexture(msaaRtSize.x, msaaRtSize.y, 1, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            m_DepthTexture = new RenderTexture(msaaRtSize.x, msaaRtSize.y, 0, RenderTextureFormat.RInt, RenderTextureReadWrite.Linear);
            m_DepthTexture.enableRandomWrite = true;
            m_DepthTexture.filterMode = FilterMode.Point;
            m_DepthTexture.Create();
            m_CommonShader.SetTexture(m_ClearKernelIndex, m_DepthProp, m_DepthTexture);
            m_CommonShader.SetTexture(m_ClearMSAAKernelIndex, m_DepthProp, m_DepthTexture);

            if (IsMSAA)
            {
                m_ColorMSAATexture = new RenderTexture(msaaRtSize.x, msaaRtSize.y, 0, RenderTextureFormat.ARGBFloat);
                m_ColorMSAATexture.enableRandomWrite = true;
                m_ColorMSAATexture.Create();

                m_CommonShader.SetTexture(m_ClearMSAAKernelIndex, m_ColorMSAAProp, m_ColorMSAATexture);
                m_CommonShader.SetTexture(m_ResolveKernelIndex, m_ColorProp, m_ColorTexture);
                m_CommonShader.SetTexture(m_ResolveKernelIndex, m_ColorMSAAProp, m_ColorMSAATexture);
            }
        }

        public ComputeBuffer SetVertexOutputBuffer(int count)
        {
            if (m_VertexOutputBuffer != null)
            {
                m_VertexOutputBuffer.Release();
                m_VertexOutputBuffer = null;
            }
            m_VertexOutputBuffer = new ComputeBuffer(count, VertexOutputData.size);

            m_ComputeShader.SetBuffer(m_VertexKernelIndex, m_VertexOutputProp, m_VertexOutputBuffer);
            return m_VertexOutputBuffer;
        }

         RenderTexture   test ;

        public void SetPass(LcLShader shader)
        {
            if (shader == null) return;

            m_VertexKernelIndex = m_ComputeShader.FindKernel("VertexTransform");
            m_WireFrameKernelIndex = m_ComputeShader.FindKernel("WireFrameTriangle");
            m_RasterizeTriangleKernelIndex = m_ComputeShader.FindKernel("RasterizeTriangle");
            m_RasterizeTriangleMSAAKernelIndex = m_ComputeShader.FindKernel("RasterizeTriangleMSAA");

            m_ComputeShader.SetTexture(m_WireFrameKernelIndex, m_ColorProp, m_ColorTexture);
            m_ComputeShader.SetTexture(m_RasterizeTriangleKernelIndex, m_ColorProp, m_ColorTexture);
            m_ComputeShader.SetTexture(m_RasterizeTriangleMSAAKernelIndex, m_ColorProp, m_ColorTexture);

            m_ComputeShader.SetTexture(m_WireFrameKernelIndex, m_DepthProp, m_DepthTexture);
            m_ComputeShader.SetTexture(m_RasterizeTriangleKernelIndex, m_DepthProp, m_DepthTexture);
            m_ComputeShader.SetTexture(m_RasterizeTriangleMSAAKernelIndex, m_DepthProp, m_DepthTexture);

            if (IsMSAA)
            {
                m_ComputeShader.SetTexture(m_RasterizeTriangleMSAAKernelIndex, m_ColorMSAAProp, m_ColorMSAATexture);
            }

            m_ComputeShader.SetVector(m_ViewportSizeProp, new Vector4(m_ViewportSize.x, m_ViewportSize.y, 0, 0));
            m_ComputeShader.SetVector("_BaseColor", shader.baseColor);
            m_ComputeShader.SetInt("_CullMode", (int)shader.CullMode);
            m_ComputeShader.SetInt("_ZWrite", (int)shader.ZWrite);
            m_ComputeShader.SetInt("_ZTest", (int)shader.ZTest);
            m_ComputeShader.SetInt("_BlendMode", (int)shader.BlendMode);


            if (shader.mainTexture)
            {
                if(test == null)
                {
                    test = new RenderTexture(shader.mainTexture.width, shader.mainTexture.height, 0, RenderTextureFormat.ARGBFloat);
                    test.enableRandomWrite = true;
                    test.Create();
                }
                Graphics.Blit(shader.mainTexture, test);
                m_CommonShader.SetTexture(m_RasterizeTriangleKernelIndex, "AlbedoTexture",test);
                // m_CommonShader.SetTexture(m_RasterizeTriangleMSAAKernelIndex, "_MainTexture", shader.mainTexture);
            }
        }


        public void Render(List<RenderObject> renderObjects)
        {
            int length = renderObjects.Count;
            for (int i = 0; i < length; i++)
            {
                RenderObject model = renderObjects[i];
                DrawElements(model);
                if (IsDebugging() && i == m_DebugIndex)
                {
                    break;
                }
            }

            Resolve();
        }

        public LcLShader shader;
        public void DrawElements(RenderObject model)
        {
            if (model == null) return;
            var vertexBuffer = model.vertexBuffer.computeBuffer;
            var indexBuffer = model.indexBuffer.computeBuffer;
            m_ComputeShader = model.computeShader;
            SetPass(model.shader);
            shader = model.shader;
            // 顶点变换
            m_ComputeShader.SetMatrix("MATRIX_M", model.matrixM);
            m_ComputeShader.SetMatrix("MATRIX_VP", m_MatrixVP);
            m_ComputeShader.SetBuffer(m_VertexKernelIndex, m_VertexProp, vertexBuffer);
            SetVertexOutputBuffer(vertexBuffer.count);
            var threadGroupX = Mathf.CeilToInt(vertexBuffer.count / 128f);
            m_ComputeShader.Dispatch(m_VertexKernelIndex, threadGroupX, 1, 1);

            // 光栅化三角形
            switch (m_PrimitiveType)
            {
                case PrimitiveType.Line:
                    WireFrameTriangle(indexBuffer);
                    break;
                case PrimitiveType.Triangle:
                    RasterizeTriangle(indexBuffer);
                    break;
            }

        }


        struct DebugData
        {
            public float4 data0;
        }
        ComputeBuffer m_DebugDataBuffer;
        DebugData[] m_DebugData = new DebugData[1];
        ComputeBuffer GetDebugDataBuffer(int count)
        {
            if (m_DebugDataBuffer != null)
            {
                m_DebugDataBuffer.Release();
            }
            m_DebugData = new DebugData[count];
            m_DebugDataBuffer = new ComputeBuffer(count, sizeof(float) * 4);
            return m_DebugDataBuffer;
        }

        /// <summary>
        /// 绘制线框三角形
        /// </summary>
        /// <param name="indexBuffer"></param>
        /// <param name="shader"></param>/ 
        /// </summary>
        private void WireFrameTriangle(ComputeBuffer indexBuffer)
        {
            var threadGroupX = Mathf.CeilToInt(indexBuffer.count / 128f);
            m_ComputeShader.SetBuffer(m_WireFrameKernelIndex, m_VertexOutputProp, m_VertexOutputBuffer);
            m_ComputeShader.SetBuffer(m_WireFrameKernelIndex, m_TriangleProp, indexBuffer);
            m_ComputeShader.Dispatch(m_WireFrameKernelIndex, threadGroupX, 1, 1);
        }

        /// <summary>
        /// 光栅化 
        /// </summary>
        /// <param name="indexBuffer"></param>
        /// <param name="shader"></param>
        private void RasterizeTriangle(ComputeBuffer indexBuffer)
        {
            if (shader.mainTexture == null)
            {
                Debug.LogError("Main texture is null!");
                return;
            }
            var threadGroupX = Mathf.CeilToInt(indexBuffer.count / 128f);
            if (IsMSAA)
            {
                m_ComputeShader.SetInt(m_SampleCountProp, SampleCount);
                m_ComputeShader.SetInt(m_ScreenZoomProp, m_ScreenZoom);
                m_ComputeShader.SetBuffer(m_RasterizeTriangleMSAAKernelIndex, m_VertexOutputProp, m_VertexOutputBuffer);
                m_ComputeShader.SetBuffer(m_RasterizeTriangleMSAAKernelIndex, m_TriangleProp, indexBuffer);
                m_ComputeShader.Dispatch(m_RasterizeTriangleMSAAKernelIndex, threadGroupX, 1, 1);
            }
            else
            {
                m_ComputeShader.SetBuffer(m_RasterizeTriangleKernelIndex, m_VertexOutputProp, m_VertexOutputBuffer);
                m_ComputeShader.SetBuffer(m_RasterizeTriangleKernelIndex, m_TriangleProp, indexBuffer);
                m_ComputeShader.Dispatch(m_RasterizeTriangleKernelIndex, threadGroupX, 1, 1);
            }
        }
        public void Resolve()
        {
            if (IsMSAA)
            {
                m_CommonShader.SetInt(m_SampleCountProp, SampleCount);
                m_CommonShader.SetInt(m_ScreenZoomProp, m_ScreenZoom);
                m_CommonShader.Dispatch(m_ResolveKernelIndex, textureThreadGroup.x, textureThreadGroup.y, 1);
            }
        }

        public void Clear(CameraClearFlags clearFlags, Color? clearColor = null, float depth = float.PositiveInfinity)
        {
            m_ClearFlags = clearFlags;

            var color = clearColor == null ? Color.clear : clearColor.Value;
            m_CommonShader.SetVector(m_ClearColorProp, color);
            if (IsMSAA)
            {
                m_CommonShader.SetInt(m_SampleCountProp, SampleCount);
                m_CommonShader.SetInt(m_ScreenZoomProp, m_ScreenZoom);
                m_CommonShader.Dispatch(m_ClearMSAAKernelIndex, textureThreadGroup.x, textureThreadGroup.y, 1);
            }
            else
            {
                m_CommonShader.Dispatch(m_ClearKernelIndex, textureThreadGroup.x, textureThreadGroup.y, 1);
            }

        }

        public void SetMatrixVP(float4x4 matrixVP)
        {
            m_MatrixVP = matrixVP;
        }

        public void SetPrimitiveType(PrimitiveType primitiveType)
        {
            m_PrimitiveType = primitiveType;
        }

        public void Dispose()
        {
        }

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