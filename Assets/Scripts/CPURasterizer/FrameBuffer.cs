
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Unity.Mathematics;
namespace LcLSoftRender
{
    class FrameBuffer
    {
        public Texture2D m_ColorTexture;
        public Texture2D m_DepthTexture;
        private int m_Width;
        private int m_Height;

        private Color[] m_ColorBuffer;
        private float[] m_DepthBuffer;

        public FrameBuffer(int width, int height , int sampleCount = 1)
        {
            m_Width = width;
            m_Height = height;

            if (sampleCount > 1)
            {
                // 创建多重采样帧缓冲区
                m_ColorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "ColorAttachment0" };
                

                m_ColorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "ColorAttachment0" };
                m_ColorTexture.SetPixel(0, 0, Color.clear);
                m_ColorTexture.Apply();

                m_DepthTexture = new Texture2D(width, height, TextureFormat.RFloat, false);
                m_DepthTexture.SetPixel(0, 0, Color.clear);
                m_DepthTexture.Apply();
            }
            else
            {
                m_ColorBuffer = new Color[width * height];
                m_ColorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "ColorAttachment0" };

                m_DepthBuffer = new float[width * height];
                m_DepthTexture = new Texture2D(width, height, TextureFormat.RFloat, false);
            }

            // m_ColorBuffer = new Color[width * height];
            // m_ColorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "ColorAttachment0" };
            // m_DepthBuffer = new float[width * height];
            // m_DepthTexture = new Texture2D(width, height, TextureFormat.RFloat, false);
        }

        public void Release()
        {
            if (m_ColorTexture != null)
                Object.Destroy(m_ColorTexture);
            if (m_DepthTexture != null)
                Object.Destroy(m_DepthTexture);
        }

        public int GetIndex(int x, int y)
        {
            return y * m_Width + x;
        }

        public void SetColor(int x, int y, Color color)
        {
            var index = GetIndex(x, y);
            if (index >= m_ColorBuffer.Length || index < 0)
            {
                Debug.LogError($"SetColor index out of range x:{x} y:{y} index:{index}");
                return;
            }
            m_ColorBuffer[index] = color;
        }
        public void SetColor(int x, int y, float4 color)
        {
            SetColor(x, y, new Color(color.x, color.y, color.z, color.w));
        }

        public float4 GetColor(int x, int y)
        {
            var color = m_ColorBuffer[GetIndex(x, y)];
            return color.ToFloat4();
        }


        public void Foreach(Action<int, int> action)
        {
            // 遍历m_ColorAttachment0 每个像素
            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Height; y++)
                {
                    action(x, y);
                }
            }
        }


        public void Apply()
        {
            m_ColorTexture.SetPixels(m_ColorBuffer);
            m_ColorTexture.Apply();
            // m_DepthTexture.SetPixels(m_DepthBuffer);
            // m_DepthTexture.Apply();
        }

        public float GetDepth(int x, int y)
        {
            return m_DepthBuffer[GetIndex(x, y)];
        }

        public void SetDepth(int x, int y, float depth)
        {
            m_DepthBuffer[GetIndex(x, y)] = depth;
        }

        public Texture2D GetOutputTexture()
        {
            return m_ColorTexture;

        }
    }
}