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
            Vector3[] vertices = mesh.vertices;
            int[] indices = mesh.triangles;
            Vector2[] uvs = mesh.uv;
            Vector3[] normals = mesh.normals;
            Vector4[] tangents = mesh.tangents;
            Color[] colors = mesh.colors;

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
            return transpose((float4x4)transform.localToWorldMatrix);
        }

        public void SetShader(LcLShader shader)
        {
            this.shader = shader;
        }
    }
}
