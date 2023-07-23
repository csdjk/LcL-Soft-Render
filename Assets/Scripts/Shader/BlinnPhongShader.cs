using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace LcLSoftRenderer
{

    public class BlinnPhongShader : LcLShader
    {
        /// ================================ Shader 属性 ================================
        public float intensity = 1.0f;
        public Color specularColor = Color.white;
        public float power = 1.0f;

        /// <summary>
        /// 顶点着色器输出
        /// </summary>
        internal class BlinnPhongVertexOutput : VertexOutput
        {
        }

        /// <summary>
        /// 顶点着色器
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public override VertexOutput Vertex(Vertex vertex)
        {
            var output = new BlinnPhongVertexOutput();
            output.positionCS = TransformTool.TransformObjectToHClip(vertex.position, MatrixMVP);
            output.uv = vertex.uv;
            output.color = vertex.color;
            output.normalWS = mul(MatrixM, float4(vertex.normal, 0));
            var positionWS = mul(MatrixM, float4(vertex.position, 1)).xyz;
            output.viewDir = normalize(Global.cameraPosition - positionWS);
            return output;
        }

        /// <summary>
        /// 片元着色器
        /// </summary>
        /// <returns></returns>
        public override bool Fragment(VertexOutput vertexOutput, out float4 colorOutput)
        {
            var input = vertexOutput as BlinnPhongVertexOutput;

            var uv = input.uv;
            var tex = Utility.SampleTexture2D(mainTexture, uv);

            float3 normal = normalize(input.normalWS).xyz;
            float3 viewDir = normalize(input.viewDir);

            float3 lightDir = Global.light.direction;
            float3 lightColor = Global.light.color;

            float3 diffuse = max(dot(normal, lightDir) * 0.5f + 0.5f, 0) * baseColor.ToFloat3() * tex.xyz;
            float3 halfDir = normalize(lightDir + viewDir);
            float3 specular = pow(max(0, dot(normal, halfDir)), power) * specularColor.ToFloat3();
            float3 finalColor = lightColor * (diffuse + Global.ambientColor.xyz + specular);

            colorOutput = float4(finalColor, 1);
            return false;
        }
    }
}
