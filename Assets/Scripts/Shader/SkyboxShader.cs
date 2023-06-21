using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace LcLSoftRender
{


    public class SkyboxShader : LcLShader
    {
        public SkyboxShader()
        {
            RenderQueue = RenderQueue.Background;
        }

        /// ================================ Shader 属性 ================================
        // 定义一个cubemap 用于存储天空盒的六张图片
        public SkyboxImages skybox;


        class Attribute : VertexOutput
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
            output.positionCS = TransformTool.TransformObjectToHClip(vertex.position.xyz, MatrixMVP);
            output.uv = vertex.uv;
            output.positionOS = float4(vertex.position, 1);
            return output;
        }

        /// <summary>
        /// 片元着色器
        /// </summary>
        /// <returns></returns>
        public override bool Fragment(VertexOutput input, out float4 colorOutput)
        {
            input = input as Attribute;
            colorOutput = 1;
            var dir = input.positionOS.xyz;
            var color = Utility.SampleCubemap(skybox, dir);
            colorOutput = color;
            return false;
        }
    }
}
