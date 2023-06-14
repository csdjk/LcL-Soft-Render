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
        public Texture2D mainTexture;


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

            var tex = Utility.tex2D(mainTexture, uv);
            colorOutput = baseColor.ToFloat4() * tex.xyzw;
            return false;
        }
    }
}
