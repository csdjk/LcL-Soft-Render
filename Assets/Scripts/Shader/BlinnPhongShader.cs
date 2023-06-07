using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace LcLSoftRender
{

    public class BlinnPhongShader : LcLShader
    {
        /// ================================ Shader 属性 ================================
        public float intensity = 1.0f;

        // public override RenderQueue renderQueue { get => RenderQueue.Geometry; set => base.renderQueue = value; }
        /// <summary>
        /// 顶点着色器输出
        /// </summary>
        internal class BlinnPhongVertexOutput : VertexOutput
        {
            public Color color2;
        }

        /// <summary>
        /// 顶点着色器
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public override VertexOutput Vertex(Vertex vertex)
        {
            VertexOutput output = new BlinnPhongVertexOutput();
            output.positionCS = TransformTool.ModelPositionToScreenPosition(vertex.position, MatrixMVP, Global.screenSize);
            output.uv = vertex.uv;
            output.color = vertex.color;
            return output;
        }

        /// <summary>
        /// 片元着色器
        /// </summary>
        /// <returns></returns>
        public override float4 Fragment(VertexOutput vertexOutput, out bool discard)
        {
            discard = false;
            vertexOutput = vertexOutput as BlinnPhongVertexOutput;
            var color = vertexOutput.color;
            return color;
        }
    }
}
