using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LcLSoftRender
{
    public class Vertex
    {
        private Vector3 m_Position;
        public Vector3 position
        {
            get => m_Position;
            set => m_Position = value;
        }
        private Vector2 m_UV;
        public Vector2 uv
        {
            get => m_UV;
            set => m_UV = value;
        }

        private Vector3 m_Normal;
        public Vector3 normal
        {
            get => m_Normal;
            set => m_Normal = value;
        }
        private Vector4 m_Tangent;
        public Vector4 tangent
        {
            get => m_Tangent;
            set => m_Tangent = value;
        }
        private Color m_Color;
        public Color color
        {
            get => m_Color;
            set => m_Color = value;
        }

        public Vertex(Vector3 position)
        {
            m_Position = position;
        }

        public Vertex(Vector3 position, Vector2 uv)
        {
            m_Position = position;
            m_UV = uv;
        }

        public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
        {
            m_Position = position;
            m_UV = uv;
            m_Normal = normal;
        }

        public Vertex(Vector3 position, Vector2 uv, Vector3 normal, Vector4 tangent, Color color)
        {
            m_Position = position;
            m_UV = uv;
            m_Normal = normal;
            m_Tangent = tangent;
            m_Color = color;
        }
    }
}
