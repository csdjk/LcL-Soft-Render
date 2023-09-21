using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace LcLSoftRenderer
{

    public class UnlitShader : LcLShader
    {
        public UnlitShader()
        {
            RenderQueue = RenderQueue.Geometry;
        }

        /// ================================ Shader 属性 ================================

        /// 
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
            output.positionCS = TransformTool.TransformObjectToHClip(vertex.position.xyz, MatrixMVP);
            output.uv = vertex.uv;
            return output;
        }

        /// <summary>
        /// 片元着色器
        /// </summary>
        /// <returns></returns>
        public override bool Fragment(VertexOutput input, out float4 colorOutput)
        {
            input = input as UnlitVertexOutput;
            colorOutput = 1;

            var uv = input.uv;

            var tex = Utility.SampleTexture2D(mainTexture, uv, WrapMode.Repeat);
            colorOutput = baseColor.ToFloat4() * tex.xyzw;
            // colorOutput = float4(uv, 0, 1);
            return false;
        }
    }
}
