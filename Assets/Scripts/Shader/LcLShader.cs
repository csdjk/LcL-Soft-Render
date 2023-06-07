using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace LcLSoftRender
{
    using Unity.Mathematics;
    public abstract class VertexOutput
    {
        public float4 positionCS;
        public float4 normal;
        public float4 tangent;
        public float2 uv;
        public float4 color;
    }

    public abstract class LcLShader
    {
        public virtual RenderQueue RenderQueue { get; set; } = RenderQueue.Geometry;

        // public bool ZWrite = true;
        public virtual ZWrite ZWrite { get; set; } = ZWrite.On;

        // public ZTest zTest = ZTest.LessEqual;
        public virtual ZTest ZTest { get; set; } = ZTest.LessEqual;

        // public CullMode CullMode = CullMode.Back;
        public virtual CullMode CullMode { get; set; } = CullMode.Back;

        // public BlendMode BlendMode = BlendMode.None;
        public virtual BlendMode BlendMode { get; set; } = BlendMode.None;


        public Color baseColor = Color.white;

        public float4x4 MatrixM { get; set; }
        public float4x4 MatrixVP { get; set; }
        public float4x4 MatrixMVP { get; set; }

        public abstract float4 Fragment(VertexOutput vertexOutput, out bool discard);
        public abstract VertexOutput Vertex(Vertex vertex);



        public void Clip()
        {
            // do nothing
        }

    }


    // [CreateAssetMenu(menuName = "LcLSoftRender/Material")]
    // [Serializable]
    // public class LcLMaterial : ScriptableObject
    // {
    //     public Color albedoColor = Color.white;
    // }

}
