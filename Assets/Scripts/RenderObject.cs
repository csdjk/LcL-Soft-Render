using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace LcLSoftRender
{
    public class RenderObject : MonoBehaviour
    {

        [SerializeReference]
        public LcLShader shader;

        VertexBuffer m_VertexBuffer;
        public VertexBuffer vertexBuffer
        {
            get
            {
                return m_VertexBuffer;
            }
        }
        IndexBuffer m_IndexBuffer;
        public IndexBuffer indexBuffer
        {
            get
            {
                return m_IndexBuffer;
            }
        }
        public void Init()
        {
            var mesh = GetComponent<MeshFilter>()?.mesh;
            var vertices = mesh.vertices;
            var indices = mesh.triangles;
            var uvs = mesh.uv;
            var normals = mesh.normals;
            var tangents = mesh.tangents;
            var colors = mesh.colors;

            Vertex[] mVertices = new Vertex[vertices.Length];
            if (colors.Length > 0)
            {
                for (int i = 0; i < mVertices.Length; i++)
                {
                    mVertices[i] = new Vertex(vertices[i], uvs[i], normals[i], tangents[i], colors[i]);
                }
            }
            else
            {
                for (int i = 0; i < mVertices.Length; i++)
                {
                    mVertices[i] = new Vertex(vertices[i], uvs[i], normals[i], tangents[i], Color.black);
                }
            }
            m_VertexBuffer = new VertexBuffer(mVertices);
            m_IndexBuffer = new IndexBuffer(indices);
        }

        public float4x4 GetMatrixM()
        {
            return (float4x4)transform.localToWorldMatrix;
        }

        public void SetShader(LcLShader shader)
        {
            this.shader = shader;
        }
    }
}
