using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEditor;

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
            var haveUV = uvs.Length > 0;

            Vertex[] mVertices = new Vertex[vertices.Length];
            if (colors.Length > 0)
            {
                for (int i = 0; i < mVertices.Length; i++)
                {
                    mVertices[i] = new Vertex(vertices[i], haveUV ? uvs[i] : 0, normals[i], tangents[i], colors[i]);
                }
            }
            else
            {
                for (int i = 0; i < mVertices.Length; i++)
                {
                    mVertices[i] = new Vertex(vertices[i], haveUV ? uvs[i] : 0, normals[i], tangents[i], Color.black);
                }
            }
            m_VertexBuffer = new VertexBuffer(mVertices);
            m_IndexBuffer = new IndexBuffer(indices);
        }

        void Update()
        {
            // 变化时重新计算矩阵
            // https://blog.csdn.net/silangquan/article/details/50984641
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


        #region Debug
        public bool debug = false;
        public bool showPositionOS = true;
        public bool showPositionWS = true;
        public bool showUV = true;
        GUIStyle style = new GUIStyle();
        private void OnDrawGizmos()
        {
            if (!debug) return;

            style.normal.textColor = Color.green;
            style.fontSize = 20;
            // 在每个顶点处画一个label, 显示顶点的索引以及坐标
            if (vertexBuffer != null)
            {
                for (int i = 0; i < vertexBuffer.Length; i++)
                {
                    var positionOS = vertexBuffer[i].position;
                    var positionWS = mul(matrixM, float4(positionOS, 1)).xyz;
                    // 四舍五入positionWS, 保留两位小数
                    positionWS = round(positionWS * 100) / 100;

                    var uv = vertexBuffer[i].uv;
                    uv = round(uv * 100) / 100;

                    var debugStr = i.ToString();
                    // if (showPositionOS)
                    {
                        debugStr += $"\nOS({positionOS.x},{positionOS.y},{positionOS.z})";
                    }
                    // if (showPositionWS)
                    {
                        debugStr += $"\nWS({positionWS.x},{positionWS.y},{positionWS.z})";
                    }
                    // if (showUV)
                    {
                        debugStr += $"\nuv({uv.x},{uv.y})";
                    }
                    Handles.Label(positionWS, debugStr, style);
                }
            }
        }
        #endregion
    }
}
