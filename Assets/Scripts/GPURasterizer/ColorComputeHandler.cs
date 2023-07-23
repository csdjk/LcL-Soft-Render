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

            // m_Buffer = new ComputeBuffer((m_Resolution.x * m_Resolution.y), sizeof(float4));
            m_ColorTexture = CreateRenderTexture(m_Resolution.x, m_Resolution.y, true, RenderTextureFormat.ARGBFloat);
            m_KernelIndex = m_ComputeShader.FindKernel("CSMain");
            // m_ComputeShader.SetBuffer(m_KernelIndex, "Result", m_Buffer);
            m_ComputeShader.SetTexture(m_KernelIndex, "ResultTexture", m_ColorTexture);


        }

        public override RenderTexture Run()
        {
            if (m_ComputeShader == null) return null;

            m_ComputeShader.Dispatch(m_KernelIndex, m_Resolution.x / 8, m_Resolution.y / 8, 1);
            return m_ColorTexture;
        }

        public override void Dispose()
        {
            m_Buffer?.Dispose();
        }
    }
}
