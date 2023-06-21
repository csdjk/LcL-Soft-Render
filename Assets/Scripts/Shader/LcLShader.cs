using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace LcLSoftRender
{
    [System.Serializable]
    public class SkyboxImages
    {
        public Texture2D front;
        public Texture2D back;
        public Texture2D left;
        public Texture2D right;
        public Texture2D up;
        public Texture2D down;
    }


    public abstract class VertexOutput
    {
        public float4 positionCS;
        public float4 positionOS;
        public float4 normal;
        public float4 tangent;
        public float2 uv;
        public float4 color;
    }

    public abstract class LcLShader
    {
        [SerializeField, HideInInspector]
        protected RenderQueue m_RenderQueue = RenderQueue.Geometry;
        public virtual RenderQueue RenderQueue { get => m_RenderQueue; set => m_RenderQueue = value; }

        [SerializeField, HideInInspector]
        protected ZWrite m_ZWrite = ZWrite.On;
        public virtual ZWrite ZWrite { get => m_ZWrite; set => m_ZWrite = value; }

        [SerializeField, HideInInspector]
        protected ZTest m_ZTest = ZTest.LessEqual;
        public virtual ZTest ZTest { get => m_ZTest; set => m_ZTest = value; }

        [SerializeField, HideInInspector]
        protected CullMode m_CullMode = CullMode.Back;
        public virtual CullMode CullMode { get => m_CullMode; set => m_CullMode = value; }

        [SerializeField, HideInInspector]
        protected BlendMode m_BlendMode = BlendMode.None;
        public virtual BlendMode BlendMode { get => m_BlendMode; set => m_BlendMode = value; }

        public Color baseColor = Color.white;

        public float4x4 MatrixM { get; set; }
        public float4x4 MatrixVP { get; set; }
        public float4x4 MatrixMVP { get; set; }

        public abstract bool Fragment(VertexOutput vertexOutput, out float4 color);
        public abstract VertexOutput Vertex(Vertex vertex);
    }

}
