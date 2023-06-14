using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;


namespace LcLSoftRender
{
    public class AlphaTestShader : LcLShader
    {

        public AlphaTestShader()
        {
            RenderQueue = RenderQueue.AlphaTest;
        }
        /// ================================ Shader 属性 ================================
        public Texture2D mainTexture;


        /// ================================================================
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
            output.positionCS = TransformTool.TransformObjectToHClip(vertex.position, MatrixMVP);
            output.normal = mul(MatrixM, float4(vertex.normal, 0));
            output.uv = vertex.uv;
            output.color = vertex.color;
            return output;
        }

        /// <summary>
        /// 片元着色器
        /// </summary>
        /// <returns></returns>
        public override bool Fragment(VertexOutput input, out float4 colorOutput)
        {
            colorOutput = 1;
            input = input as Attribute;

            var uv = input.uv;

            var tex = Utility.tex2D(mainTexture, uv);
            if (tex.w < 0.5f)
            {
                return true;
            }

            colorOutput.xyz = baseColor.ToFloat4().xyz;
            return false;
        }
    }
}
