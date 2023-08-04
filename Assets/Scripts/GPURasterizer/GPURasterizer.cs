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
        Camera m_Camera;
        float4x4 m_Model;
        float4x4 m_MatrixVP;
        float4x4 m_MatrixMVP;
        public float4x4 MatrixMVP => m_MatrixMVP;
        LcLShader m_OverrideShader;


        ComputeShader m_ComputeShader;
        RenderTexture m_ColorTexture;
        RenderTexture m_DepthTexture;
        ComputeBuffer m_VertexOutputBuffer;
        string m_VertexProp = "VertexBuffer";
        string m_VertexOutputProp = "VertexOutputBuffer";
        string m_TriangleProp = "TriangleBuffer";
        string m_ColorProp = "ColorTexture";
        string m_DepthProp = "DepthTexture";
        string m_ClearColorProp = "ClearColor";
        string m_ViewportSizeProp = "ViewportSize";

        int m_ClearKernelIndex = -1;
        int m_VertexKernelIndex = -1;
        int m_RasterizeTriangleKernelIndex = -1;
        int m_WireFrameKernelIndex = -1;
        // int vertexThreadGroupX => (int)ceil(m_ViewportSize.x / 128);
        int2 textureThreadGroup => (int2)ceil((float2)m_ViewportSize / (float2)8);

        // public ComputeBuffer computeBuffer => m_VertexOutputBuffer;
        public GPURasterizer(Camera camera, ComputeShader computeShader, MSAAMode msaaMode = MSAAMode.None)
        {
            m_MSAAMode = msaaMode;
            m_Camera = camera;
            m_ViewportSize = new int2(camera.pixelWidth, camera.pixelHeight);
            m_ComputeShader = computeShader;

            m_ClearKernelIndex = m_ComputeShader.FindKernel("Clear");
            m_VertexKernelIndex = m_ComputeShader.FindKernel("VertexTransform");
            m_WireFrameKernelIndex = m_ComputeShader.FindKernel("WireFrameTriangle");
            m_RasterizeTriangleKernelIndex = m_ComputeShader.FindKernel("RasterizeTriangle");

            m_ColorTexture = new RenderTexture(m_ViewportSize.x, m_ViewportSize.y, 0, RenderTextureFormat.ARGBFloat);
            m_ColorTexture.enableRandomWrite = true;
            m_ColorTexture.Create();
            m_ComputeShader.SetTexture(m_WireFrameKernelIndex, m_ColorProp, m_ColorTexture);
            m_ComputeShader.SetTexture(m_RasterizeTriangleKernelIndex, m_ColorProp, m_ColorTexture);
            m_ComputeShader.SetTexture(m_ClearKernelIndex, m_ColorProp, m_ColorTexture);

            m_DepthTexture = new RenderTexture(m_ViewportSize.x, m_ViewportSize.y, 1, RenderTextureFormat.R16);
            m_DepthTexture.enableRandomWrite = true;
            m_DepthTexture.Create();
            m_ComputeShader.SetTexture(m_WireFrameKernelIndex, m_DepthProp, m_DepthTexture);
            m_ComputeShader.SetTexture(m_RasterizeTriangleKernelIndex, m_DepthProp, m_DepthTexture);
            m_ComputeShader.SetTexture(m_ClearKernelIndex, m_DepthProp, m_DepthTexture);
        }

        public Texture ColorTexture
        {
            get => m_ColorTexture;
        }


        public void Render(List<RenderObject> renderObjects)
        {
            int length = renderObjects.Count;
            for (int i = 0; i < length; i++)
            {
                RenderObject model = renderObjects[i];
                // if (m_ClearFlags != CameraClearFlags.Skybox && model.isSkyBox)
                // {
                //     continue;
                // }
                // model.vertexBuffer.computeBuffer;
                DrawElements(model);
                if (IsDebugging() && i == m_DebugIndex)
                {
                    break;
                }
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

        public void DrawElements(RenderObject model)
        {
            if (model == null || m_ComputeShader == null) return;
            var vertexBuffer = model.vertexBuffer.computeBuffer;
            var indexBuffer = model.indexBuffer.computeBuffer;
            var shader = model.shader;

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
                    WireFrameTriangle(indexBuffer, shader);
                    break;
                case PrimitiveType.Triangle:
                    RasterizeTriangle(indexBuffer, shader);
                    break;
                    // if (IsMSAA)
                    //     RasterizeTriangleMSAA(clippedVertices[0], clippedVertices[j], clippedVertices[j + 1], shader, SampleCount);
                    // else
                    // RasterizeTriangle(indexBuffer, shader);
                    // break;
            }

            // var m_VertexOutputData = new VertexOutputData[m_VertexOutputBuffer.count];
            // m_VertexOutputBuffer.GetData(m_VertexOutputData);
            // foreach (var item in m_VertexOutputData)
            // {
            //     Debug.Log(item.positionCS);
            // }
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
        /// <param name="vertex0"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="shader"></param>
        private void WireFrameTriangle(ComputeBuffer indexBuffer, LcLShader shader)
        {
            var threadGroupX = Mathf.CeilToInt(indexBuffer.count / 128f);
            m_ComputeShader.SetBuffer(m_WireFrameKernelIndex, m_VertexOutputProp, m_VertexOutputBuffer);
            m_ComputeShader.SetBuffer(m_WireFrameKernelIndex, m_TriangleProp, indexBuffer);
            m_ComputeShader.SetVector(m_ViewportSizeProp, new Vector4(m_ViewportSize.x, m_ViewportSize.y, 0, 0));
            m_ComputeShader.SetVector("_BaseColor", shader.baseColor);
            // var debugBuffer = GetDebugDataBuffer(3);
            // m_ComputeShader.SetBuffer(m_WireFrameKernelIndex, "DebugDataBuffer", debugBuffer);
            m_ComputeShader.SetInt("_CullMode", (int)shader.CullMode);
            m_ComputeShader.Dispatch(m_WireFrameKernelIndex, threadGroupX, 1, 1);
            // debugBuffer.GetData(m_DebugData);
            // foreach (var item in m_DebugData)
            // {
            //     Debug.Log(item.data0);
            // }
        }

        private void RasterizeTriangle(ComputeBuffer indexBuffer, LcLShader shader)
        {
            var threadGroupX = Mathf.CeilToInt(indexBuffer.count / 128f);
            m_ComputeShader.SetBuffer(m_RasterizeTriangleKernelIndex, m_VertexOutputProp, m_VertexOutputBuffer);
            m_ComputeShader.SetBuffer(m_RasterizeTriangleKernelIndex, m_TriangleProp, indexBuffer);
            m_ComputeShader.SetVector(m_ViewportSizeProp, new Vector4(m_ViewportSize.x, m_ViewportSize.y, 0, 0));
            m_ComputeShader.SetVector("_BaseColor", shader.baseColor);
            m_ComputeShader.SetInt("_CullMode", (int)shader.CullMode);
            m_ComputeShader.SetInt("_ZWrite", (int)shader.ZWrite);
            m_ComputeShader.SetInt("_ZTest", (int)shader.ZTest);
            m_ComputeShader.SetInt("_BlendMode", (int)shader.BlendMode);

            // m_ComputeShader.SetInts("_SampleCount", new int[] { SampleCount, SampleCount });
            m_ComputeShader.Dispatch(m_RasterizeTriangleKernelIndex, threadGroupX, 1, 1);
        }

        public void Clear(CameraClearFlags clearFlags, Color? clearColor = null, float depth = float.PositiveInfinity)
        {
            m_ClearFlags = clearFlags;
            
            var color = clearColor == null ? Color.clear : clearColor.Value;
            m_ComputeShader.SetVector(m_ClearColorProp, color);
            m_ComputeShader.Dispatch(m_ClearKernelIndex, textureThreadGroup.x, textureThreadGroup.y, 1);

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
            // m_ColorComputeHandler.Dispose();
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