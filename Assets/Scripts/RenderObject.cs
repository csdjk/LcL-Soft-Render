using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEditor;

namespace LcLSoftRenderer
{
    [ExecuteAlways]
    public class RenderObject : MonoBehaviour
    {

        [SerializeReference]
        public LcLShader shader = new UnlitShader();
        public RenderQueue renderQueue => shader.RenderQueue;
        public bool isTransparent => shader.RenderQueue >= RenderQueue.AlphaTest;
        public bool isSkyBox => shader.RenderQueue == RenderQueue.Background;
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
            CalculateMatrix();

            var mesh = GetComponent<MeshFilter>()?.sharedMesh;
            if(mesh == null)
            {
                Debug.LogError("MeshFilter is null");
                return;
            }
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
            if (transform.hasChanged)
            {
                CalculateMatrix();
                transform.hasChanged = false;
            }
        }

        /// <summary>
        /// 计算M矩阵
        /// https://blog.csdn.net/silangquan/article/details/50984641
        /// </summary>
        void CalculateMatrix()
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
            if (!debug || !SoftRenderer.instance) return;

            style.normal.textColor = Color.green * 0.8f;
            style.fontSize = 15;
            // 在每个顶点处画一个label, 显示顶点的索引以及坐标
            if (vertexBuffer != null)
            {
                for (int i = 0; i < vertexBuffer.Length; i++)
                {
                    var debugStr = i.ToString();
                    var positionOS = vertexBuffer[i].position;
                    var positionWS = mul(matrixM, float4(positionOS, 1));
                    var uv = vertexBuffer[i].uv;

                    // if (showPositionOS)
                    {
                        // 四舍五入positionWS, 保留两位小数
                        var posOS = round(positionOS * 100) / 100;
                        debugStr += $"\nOS({posOS.x},{posOS.y},{posOS.z})";
                    }
                    // if (showPositionWS)
                    {
                        var posWS = round(positionWS * 100) / 100;
                        debugStr += $"\nWS({posWS.x},{posWS.y},{posWS.z})";
                    }

                    if (SoftRenderer.instance.rasterizer != null)
                    {
                        var rasterizer = SoftRenderer.instance.rasterizer;
                        var positionCS = mul(rasterizer.MatrixVP, positionWS);
                        var ndc = positionCS.xyz / positionCS.w;

                        positionCS = round(positionCS * 100) / 100;
                        debugStr += $"\nCS({positionCS.x},{positionCS.y},{positionCS.z},{positionCS.w})";

                        ndc = round(ndc * 100) / 100;
                        debugStr += $"\nNDC({ndc.x},{ndc.y},{ndc.z})";
                    }

                    // if (showUV)
                    {
                        uv = round(uv * 100) / 100;
                        debugStr += $"\nuv({uv.x},{uv.y})";
                    }
                    Handles.Label(positionWS.xyz, debugStr, style);
                }
            }
        }
        #endregion
    }
}
