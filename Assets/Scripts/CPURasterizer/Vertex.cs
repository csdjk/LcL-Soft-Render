using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace LcLSoftRender
{
    public class Vertex
    {
        private float3 m_Position;
        public float3 position
        {
            get => m_Position;
            set => m_Position = value;
        }
        private float2 m_UV;
        public float2 uv
        {
            get => m_UV;
            set => m_UV = value;
        }

        private float3 m_Normal;
        public float3 normal
        {
            get => m_Normal;
            set => m_Normal = value;
        }
        private float4 m_Tangent;
        public float4 tangent
        {
            get => m_Tangent;
            set => m_Tangent = value;
        }
        private float4 m_Color;
        public float4 color
        {
            get => m_Color;
            set => m_Color = value;
        }

        public Vertex(float3 position)
        {
            m_Position = position;
        }

        public Vertex(float3 position, float2 uv)
        {
            m_Position = position;
            m_UV = uv;
        }

        public Vertex(float3 position, float2 uv, float3 normal)
        {
            m_Position = position;
            m_UV = uv;
            m_Normal = normal;
        }

        public Vertex(float3 position, float2 uv, float3 normal, float4 tangent, Color color)
        {

            m_Position = position;
            m_UV = uv;
            m_Normal = normal;
            m_Tangent = tangent;
            m_Color = new float4(color.r, color.g, color.b, color.a);
        }
    }
}
