
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
        int m_SampleCount;
        bool m_IsMSAA => m_SampleCount > 1;
        public Texture2D m_ColorTexture;
        public Texture2D m_DepthTexture;
        private int m_Width;
        private int m_Height;
        private int m_BufferLength;

        private Color[] m_ColorBuffer;
        private Color[][] m_ColorBufferMSAA;
        private float[] m_DepthBuffer;
        private float[][] m_DepthBufferMSAA;

        /// <summary>
        /// 创建帧缓冲区
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="sampleCount"></param>
        public FrameBuffer(int width, int height, int sampleCount = 1)
        {
            m_SampleCount = sampleCount;
            m_Width = width;
            m_Height = height;
            m_BufferLength = width * height;

            m_ColorBuffer = new Color[m_BufferLength];
            m_DepthBuffer = new float[m_BufferLength];
            m_DepthTexture = new Texture2D(width, height, TextureFormat.RFloat, false);
            m_ColorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "ColorAttachment0" };

            if (m_IsMSAA)
            {
                // 创建多重采样帧缓冲区
                m_ColorBufferMSAA = new Color[m_BufferLength][];
                for (int i = 0; i < m_BufferLength; i++)
                    m_ColorBufferMSAA[i] = new Color[m_SampleCount];

                m_DepthBufferMSAA = new float[m_BufferLength][];
                for (int i = 0; i < m_BufferLength; i++)
                    m_DepthBufferMSAA[i] = new float[m_SampleCount];
            }
        }
        /// <summary>
        /// 释放帧缓冲区
        /// </summary>
        public void Release()
        {
            m_DepthBufferMSAA = null;
            m_ColorBufferMSAA = null;
            m_DepthBuffer = null;
            m_ColorBuffer = null;
            if (m_ColorTexture != null)
                Object.Destroy(m_ColorTexture);
            if (m_DepthTexture != null)
                Object.Destroy(m_DepthTexture);
        }

        public bool GetIndex(int x, int y, out int index)
        {
            index = y * m_Width + x;
            if (index >= m_BufferLength || index < 0)
            {
                Debug.LogError($"SetColor index out of range x:{x} y:{y} index:{index}");
                return false;
            }
            return true;
        }

        public void SetColor(int x, int y, Color color, int sampleIndex = 0, bool writeMultipleSamplingBuffer = false)
        {
            if (GetIndex(x, y, out var index))
            {
                m_ColorBuffer[index] = color;
                if (m_IsMSAA && writeMultipleSamplingBuffer)
                    m_ColorBufferMSAA[index][sampleIndex] = color;
            }
        }
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <param name="sampleIndex"></param>
        /// <param name="writeMultipleSamplingBuffer"></param>
        public void SetColor(int x, int y, float4 color, int sampleIndex = 0, bool writeMultipleSamplingBuffer = false)
        {
            SetColor(x, y, new Color(color.x, color.y, color.z, color.w), sampleIndex, writeMultipleSamplingBuffer);
        }
        public float4 GetColor(int x, int y, int sampleIndex = 0)
        {
            var color = Color.black;
            if (GetIndex(x, y, out var index))
            {
                if (m_IsMSAA)
                {
                    color = m_ColorBufferMSAA[index][sampleIndex];
                }
                else
                {
                    color = m_ColorBuffer[index];
                }
            }
            return color.ToFloat4();
        }

        public float GetDepth(int x, int y, int sampleIndex = 0)
        {
            var depth = 0f;
            if (GetIndex(x, y, out var index))
            {
                if (m_IsMSAA)
                {
                    depth = m_DepthBufferMSAA[index][sampleIndex];
                }
                else
                {
                    depth = m_DepthBuffer[index];
                }
            }
            return depth;
        }

        public void SetDepth(int x, int y, float depth, int sampleIndex = 0, bool writeMultipleSamplingBuffer = false)
        {
            if (GetIndex(x, y, out var index))
            {
                m_DepthBuffer[index] = depth;
                if (m_IsMSAA && writeMultipleSamplingBuffer)
                    m_DepthBufferMSAA[index][sampleIndex] = depth;
            }
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

        /// <summary>
        /// 清除帧缓冲区
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="clearColor"></param>
        /// <param name="depth"></param>
        public void Clear(ClearMask mask, Color? clearColor = null, float depth = float.PositiveInfinity)
        {
            bool isClearDepth = (mask & ClearMask.DEPTH) != 0;
            bool isClearColor = (mask & ClearMask.COLOR) != 0;
            // 遍历m_ColorAttachment0 每个像素
            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Height; y++)
                {
                    for (int i = 0; i < m_SampleCount; i++)
                    {
                        if (isClearColor)
                        {
                            SetColor(x, y, clearColor ?? Color.black, i, true);
                        }
                        if (isClearDepth)
                        {
                            SetDepth(x, y, depth, i, true);
                        }
                    }
                }
            }
            Apply();
        }

        /// <summary>
        /// 应用
        /// </summary>
        public void Apply()
        {
            m_ColorTexture.SetPixels(m_ColorBuffer);
            m_ColorTexture.Apply();
            // m_DepthTexture.SetPixels(m_DepthBuffer);
            // m_DepthTexture.Apply();
        }

        /// <summary>
        /// 解析多重采样 
        /// </summary>
        public void Resolve()
        {
            if (!m_IsMSAA) return;

            // 遍历m_ColorAttachment0 每个像素
            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Height; y++)
                {
                    float4 color = 0;
                    for (int i = 0; i < m_SampleCount; i++)
                    {
                        color += GetColor(x, y, i);
                    }
                    color /= m_SampleCount;
                    SetColor(x, y, color);
                }
            }
        }

        /// <summary>
        /// 获取输出纹理
        /// </summary>
        /// <returns></returns>
        public Texture2D GetOutputTexture()
        {
            return m_ColorTexture;

        }
    }
}