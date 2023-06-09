using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;


namespace LcLSoftRender
{
    public class AlphaTestShader : LcLShader
    {
        /// ================================ Shader 属性 ================================
        public override RenderQueue RenderQueue { get => RenderQueue.AlphaTest; }
        public Texture2D mainTexture;
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
            output.normal = mul(MatrixM, float4(vertex.normal, 0));
            output.uv = vertex.uv;
            output.color = vertex.color;
            return output;
        }

        /// <summary>
        /// 片元着色器
        /// </summary>
        /// <returns></returns>
        public override float4 Fragment(VertexOutput input, out bool discard)
        {
            discard = false;
            input = input as Attribute;

            // if (input.uv.x < 0.5f)
            // {
            //     discard = true;
            //     return 0;
            // }

            var uv = input.uv;

            var tex = Utility.tex2D(mainTexture, uv);
            return tex;

            var color = input.color;
            color.x = uv.x;
            color.y = uv.y;
            // return float4(input.positionCS.xy/Global.screenSize, 0, 1);
            return float4(uv.xy, 0, 1);
        }
    }
}
