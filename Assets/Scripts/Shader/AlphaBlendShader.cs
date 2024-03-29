﻿using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace LcLSoftRenderer
{

    public class AlphaBlendShader : LcLShader
    {
        public AlphaBlendShader()
        {
            RenderQueue = RenderQueue.Transparent;
            BlendMode = BlendMode.AlphaBlend;
        }

        /// ================================ Shader 属性 ================================

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
            output.normalWS = mul(MatrixM, float4(vertex.normal, 0));
            output.uv = vertex.uv;
            // output.color = vertex.color;
            return output;
        }

        /// <summary>
        /// 片元着色器
        /// </summary>
        /// <returns></returns>
        public override bool Fragment(VertexOutput vertexOutput, out float4 colorOutput)
        {
            colorOutput = 0;
            vertexOutput = vertexOutput as Attribute;

            var uv = vertexOutput.uv;
            var normalWS = normalize(vertexOutput.normalWS);

            var tex = Utility.SampleTexture2D(mainTexture, uv);

            colorOutput.xyz = tex.xyz * baseColor.ToFloat4().xyz;
            colorOutput.w = tex.w * baseColor.a;

            return false;
        }
    }
}
