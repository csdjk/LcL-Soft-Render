using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LcLSoftRenderer
{
    public abstract class ComputeShaderHandler
    {
        protected ComputeShader m_ComputeShader;

        public ComputeShaderHandler(ComputeShader computeShader)
        {
            m_ComputeShader = computeShader;
        }

        protected RenderTexture CreateRenderTexture(int width, int height, bool enableRandomWrite = true, RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32)
        {
            var renderTexture = new RenderTexture(width, height, 1, renderTextureFormat);
            renderTexture.enableRandomWrite = enableRandomWrite;
            renderTexture.Create();
            return renderTexture;
        }
        public abstract RenderTexture Run();
        public abstract void Initialize();

        public abstract void Dispose();
    }
}
