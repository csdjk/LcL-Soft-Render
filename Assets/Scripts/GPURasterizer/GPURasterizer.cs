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


        // ColorComputeHandler m_ColorComputeHandler;
        ComputeShader m_ComputeShader;
        RenderTexture m_ColorTexture;
        ComputeBuffer m_VertexOutputBuffer;
        int m_VertexKernelIndex = -1;
        // public ComputeBuffer computeBuffer => m_VertexOutputBuffer;
        public GPURasterizer(Camera camera, ComputeShader computeShader, MSAAMode msaaMode = MSAAMode.None)
        {
            m_MSAAMode = msaaMode;
            m_Camera = camera;
            m_ViewportSize = new int2(camera.pixelWidth, camera.pixelHeight);
            m_ComputeShader = computeShader;

            m_VertexKernelIndex = m_ComputeShader.FindKernel("VertexTransform");

            // m_ColorComputeHandler = new ColorComputeHandler(computeShader, m_ViewportSize);
            // m_ColorComputeHandler.Initialize();
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
                // model.shader.MatrixM = model.matrixM;
                // model.shader.MatrixVP = m_MatrixVP;
                // model.shader.MatrixMVP = CalculateMatrixMVP(model.matrixM);

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

            m_ComputeShader.SetBuffer(m_VertexKernelIndex, "VertexOutputBuffer", m_VertexOutputBuffer);
            return m_VertexOutputBuffer;
        }

        public void DrawElements(RenderObject model)
        {
            if (model == null || m_ComputeShader == null) return;
            // m_ColorComputeHandler.SetComputeBuffer("VertexBuffer", model.vertexBuffer.computeBuffer);
            m_ComputeShader.SetMatrix("MATRIX_M", model.matrixM);
            m_ComputeShader.SetMatrix("MATRIX_VP", m_MatrixVP);
            m_ComputeShader.SetBuffer(m_VertexKernelIndex, "VertexBuffer", model.vertexBuffer.computeBuffer);

            SetVertexOutputBuffer(model.vertexBuffer.computeBuffer.count);


            var threadGroupX = Mathf.CeilToInt(m_ViewportSize.x / 128);
            m_ComputeShader.Dispatch(m_VertexKernelIndex, threadGroupX, 1, 1);


            var m_VertexOutputData = new VertexOutputData[m_VertexOutputBuffer.count];
            m_VertexOutputBuffer.GetData(m_VertexOutputData);
            foreach (var item in m_VertexOutputData)
            {
                Debug.Log(item.positionCS);
            }
        }
        public void Clear(CameraClearFlags clearFlags, Color? clearColor = null, float depth = float.PositiveInfinity)
        {
            // m_ClearFlags = clearFlags;
            // switch (clearFlags)
            // {
            //     case CameraClearFlags.Nothing:
            //         break;
            //     case CameraClearFlags.Skybox:
            //         Clear(ClearMask.DEPTH);
            //         break;
            //     default:
            //         Clear(ClearMask.COLOR | ClearMask.DEPTH, clearColor);
            //         break;
            // }
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