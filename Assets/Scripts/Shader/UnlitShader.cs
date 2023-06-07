using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace LcLSoftRender
{

    public class UnlitShader : LcLShader
    {
        /// ================================ Shader 属性 ================================
        public Color color = Color.white;
        public float intensity = 1.0f;

        internal class UnlitVertexOutput : VertexOutput
        {
        }
        /// <summary>
        /// 顶点着色器
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public override VertexOutput Vertex(Vertex vertex)
        {
            VertexOutput output = new UnlitVertexOutput();
            output.positionCS = TransformTool.ModelPositionToScreenPosition(vertex.position.xyz, MatrixMVP, Global.screenSize);
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
            vertexOutput = vertexOutput as UnlitVertexOutput;

            return 1;
        }
    }
}
