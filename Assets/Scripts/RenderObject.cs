using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace LcLSoftRender
{
    [ExecuteAlways]
    public class RenderObject : MonoBehaviour
    {

        [SerializeReference]
        public LcLShader shader = new UnlitShader();
        public RenderQueue renderQueue => shader.RenderQueue;
        public bool isTransparent => shader.RenderQueue >= RenderQueue.AlphaTest;

        private float4x4 m_MatrixM;
        public float4x4 matrixM => m_MatrixM;


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

        private void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            var mesh = GetComponent<MeshFilter>()?.sharedMesh;
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

        void Update()
        {
            // 变化时重新计算矩阵
            if (transform.hasChanged)
            {
                float4x4 translateMatrix = float4x4(1, 0, 0, transform.position.x,
                                                               0, 1, 0, transform.position.y,
                                                               0, 0, 1, transform.position.z,
                                                               0, 0, 0, 1);

                float4x4 scaleMatrix = float4x4(transform.lossyScale.x, 0, 0, 0,
                                                0, transform.lossyScale.y, 0, 0,
                                                0, 0, transform.lossyScale.z, 0,
                                                0, 0, 0, 1);


                // float4x4 rotationMatrix = (float4x4)Matrix4x4.Rotate(transform.rotation);
                float4x4 rotationMatrix = TransformTool.QuaternionToMatrix(transform.rotation);

                m_MatrixM = mul(translateMatrix, mul(rotationMatrix, scaleMatrix));
                // m_MatrixM = (float4x4)transform.localToWorldMatrix;

                transform.hasChanged = false;
            }
        }

        public void SetShader(LcLShader shader)
        {
            this.shader = shader;
        }
    }
}
