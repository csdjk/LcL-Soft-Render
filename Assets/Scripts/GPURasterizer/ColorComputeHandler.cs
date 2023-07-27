using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace LcLSoftRenderer
{
    public class ColorComputeHandler : ComputeShaderHandler
    {
        int2 m_Resolution;
        ComputeBuffer m_Buffer;
        int m_KernelIndex = -1;
        RenderTexture m_ColorTexture;
        public ColorComputeHandler(ComputeShader computeShader, int2 resolution) : base(computeShader)
        {
            m_Resolution = resolution;
        }



        public override void Initialize()
        {
            if (m_ComputeShader == null)
            {
                return;
            }
            m_ColorTexture = CreateRenderTexture(m_Resolution.x, m_Resolution.y, true, RenderTextureFormat.ARGBFloat);
            // m_KernelIndex = m_ComputeShader.FindKernel("CSMain");
            m_KernelIndex = m_ComputeShader.FindKernel("VertexShader");
            // m_ComputeShader.SetTexture(m_KernelIndex, "ResultTexture", m_ColorTexture);
        }

        public void SetComputeBuffer(string name, ComputeBuffer buffer)
        {
            m_Buffer = buffer;
            m_ComputeShader.SetBuffer(m_KernelIndex, name, buffer);
        }

        public override RenderTexture Run()
        {
            if (m_ComputeShader == null) return null;
            var threadGroupX = Mathf.CeilToInt(m_Resolution.x / 128);
            m_ComputeShader.Dispatch(m_KernelIndex, threadGroupX, 1, 1);
            // m_ComputeShader.Dispatch(m_KernelIndex, m_Resolution.x / 8, m_Resolution.y / 8, 1);

            // get buffer
            var m_VertexOutputData = new VertexOutputData[m_Buffer.count];
            m_Buffer.GetData(m_VertexOutputData);
            // log
            foreach (var item in m_VertexOutputData)
            {
                Debug.Log(item.positionCS);
            }
            return m_ColorTexture;
        }

        public override void Dispose()
        {
            m_Buffer?.Dispose();
            m_Buffer = null;
            m_ColorTexture?.Release();
        }
    }
}
