using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace LcLSoftRender
{

    public class UnlitShader : LcLShader
    {
        public UnlitShader()
        {
            RenderQueue = RenderQueue.Geometry;
        }



        /// ================================ Shader 属性 ================================
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
            return output;
        }

        /// <summary>
        /// 片元着色器
        /// </summary>
        /// <returns></returns>
        public override bool Fragment(VertexOutput vertexOutput, out float4 colorOutput)
        {
            vertexOutput = vertexOutput as UnlitVertexOutput;

            colorOutput = baseColor.ToFloat4();
            return false;
        }
    }
}
