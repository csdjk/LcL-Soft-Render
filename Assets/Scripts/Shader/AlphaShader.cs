using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace LcLSoftRender
{

    public class AlphaShader : LcLShader
    {
        /// ================================ Shader 属性 ================================

        public override RenderQueue RenderQueue { get => RenderQueue.Transparent; set => base.RenderQueue = value; }
        /// <summary>
        /// 顶点着色器输出
        /// </summary>
        internal class Attribute : VertexOutput
        {
        }

        /// <summary>
        /// 顶点着色器
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public override VertexOutput Vertex(Vertex vertex)
        {
            VertexOutput output = new Attribute();
            output.positionCS = TransformTool.ModelPositionToScreenPosition(vertex.position, MatrixMVP, Global.screenSize);
            // output.normal = MatrixM * vertex.normal;
            // output.uv = vertex.uv;
            // output.color = vertex.color;
            return output;
        }

        /// <summary>
        /// 片元着色器
        /// </summary>
        /// <returns></returns>
        public override float4 Fragment(VertexOutput vertexOutput, out bool discard)
        {
            discard = false;
            vertexOutput = vertexOutput as Attribute;
            var color = vertexOutput.color;
            var normalWS = normalize(vertexOutput.normal);
            color.x = normalWS.x;
            color.y = normalWS.y;
            color.z = normalWS.z;
            color.w = 0.5f;

            return color;
        }
    }
}
