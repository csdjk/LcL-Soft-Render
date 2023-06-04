using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace LcLSoftRender
{
    struct LcLMesh
    {
        public int vertexBufferHandle;
        public int indexBufferHandle;
        public MeshFilter mesh;
        // public ModelProperty modelProperty;
        public LcLMesh(int vertexBufferHandle, int indexBufferHandle, MeshFilter mesh)
        {
            this.vertexBufferHandle = vertexBufferHandle;
            this.indexBufferHandle = indexBufferHandle;
            this.mesh = mesh;
            // this.modelProperty = modelProperty;
        }
    }

    public class SoftRender : MonoBehaviour
    {
        public RasterizerType RasterizerType = RasterizerType.CPU;
        public Color clearColor = Color.black;
        public PrimitiveType primitiveType = PrimitiveType.Triangle;
        Camera m_Camera;
        CPURasterizer m_Rasterizer;
        List<RenderObject> m_RenderObjects = new List<RenderObject>();
        List<MeshFilter> m_Meshes = new List<MeshFilter>();
        BlinnPhongShader m_BlinnPhongShader;
        List<LcLMesh> m_Models = new List<LcLMesh>();

        private void Awake()
        {
            Init();
        }

        void Init()
        {
            var width = Screen.width;
            var height = Screen.height;
            m_Camera = GetComponent<Camera>();

            m_Models.Clear();


            m_Rasterizer = new CPURasterizer(width, height);
            CollectRenderObjects();
            m_BlinnPhongShader = new BlinnPhongShader();

        }

        // 收集所有的渲染对象
        private void CollectRenderObjects()
        {
            m_RenderObjects = FindObjectsOfType<RenderObject>().ToList();

            foreach (var obj in m_RenderObjects)
            {
                obj.Init();
                // var mesh = obj.GetComponent<MeshFilter>()?.mesh;

                // Vector3[] vertices = mesh.vertices;
                // int[] indices = mesh.triangles;
                // Vector2[] uvs = mesh.uv;
                // Vector3[] normals = mesh.normals;
                // Vector4[] tangents = mesh.tangents;
                // Color[] colors = mesh.colors;

                // Vertex[] mVertices = new Vertex[vertices.Length];
                // if (colors.Length > 0)
                // {
                //     for (int i = 0; i < mVertices.Length; i++)
                //     {
                //         mVertices[i] = new Vertex(vertices[i], uvs[i], normals[i], tangents[i], colors[i]);
                //     }
                // }
                // else
                // {
                //     for (int i = 0; i < mVertices.Length; i++)
                //     {
                //         mVertices[i] = new Vertex(vertices[i], uvs[i], normals[i], tangents[i], Color.black);
                //     }
                // }
                // VBO
                // m_Rasterizer.GenVertexBuffer(mVertices);
                // IBO
                // m_Rasterizer.GenIndexBuffer(indices);
                // m_Models.Add(new LcLMesh(VBO, IBO, meshFilter));
            }
        }

        private void Update()
        {
            Profiler.BeginSample("LcLSoftRender.Clear");
            m_Rasterizer?.Clear(ClearMask.COLOR | ClearMask.DEPTH, clearColor);
            Profiler.EndSample();


            Matrix4x4 matrixVP = Matrix4x4.identity;
            if (m_Camera.orthographic)
            {
                matrixVP = TransformTool.CreateOrthographicMatrixVP(m_Camera);
            }
            else
            {
                matrixVP = TransformTool.CreateMatrixVP(m_Camera);
            }
            m_Rasterizer?.SetMatrixVP(matrixVP);


            Global.ambientColor = RenderSettings.ambientLight;

            m_Rasterizer.SetShader(m_BlinnPhongShader);
            m_BlinnPhongShader.viewPos = m_Camera.transform.position;

            Profiler.BeginSample("LcLSoftRender.CameraRenderer.Render");
            foreach (var model in m_RenderObjects)
            {
                // m_Rasterizer.BindVertexBuffer(model.vertexBufferHandle);
                // m_Rasterizer.BindIndexBuffer(model.indexBufferHandle);
                m_Rasterizer.CalculateMatriaxMVP(model.transform.localToWorldMatrix);
                m_Rasterizer.DrawElements(model, primitiveType);
            }
            // m_Rasterizer?.Render();


            Profiler.EndSample();
        }

        private void OnPostRender()
        {
            
            // // 对三角形的三个顶点按照 y 坐标从小到大排序
            // if (v0.position.y > v1.position.y)
            // {
            //     Vertex temp = v0;
            //     v0 = v1;
            //     v1 = temp;
            // }
            // if (v0.position.y > v2.position.y)
            // {
            //     Vertex temp = v0;
            //     v0 = v2;
            //     v2 = temp;
            // }
            // if (v1.position.y > v2.position.y)
            // {
            //     Vertex temp = v1;
            //     v1 = v2;
            //     v2 = temp;
            // }

            // // 计算三条边的斜率 k0、k1、k2，以及截距 b0、b1、b2
            // float k0 = (v1.position.x - v0.position.x) / (v1.position.y - v0.position.y);
            // float k1 = (v2.position.x - v0.position.x) / (v2.position.y - v0.position.y);
            // float k2 = (v2.position.x - v1.position.x) / (v2.position.y - v1.position.y);
            // float b0 = v0.position.x - k0 * v0.position.y;
            // float b1 = v0.position.x - k1 * v0.position.y;
            // float b2 = v1.position.x - k2 * v1.position.y;

            // // 从 v0 开始，按照 y 坐标从小到大的顺序，扫描每一行像素
            // for (int y = Mathf.RoundToInt(v0.position.y); y <= Mathf.RoundToInt(v2.position.y); y++)
            // {
            //     // 计算出该行与三条边的交点 x0、x1、x2
            //     float x0 = k0 * y + b0;
            //     float x1 = k1 * y + b1;
            //     float x2 = k2 * y + b2;

            //     // 将 x0、x1、x2 按照 x 坐标从小到大排序，得到两个区间 [x0, x1] 和 [x1, x2]
            //     float xStart = Mathf.Min(x0, Mathf.Min(x1, x2));
            //     float xEnd = Mathf.Max(x0, Mathf.Max(x1, x2));
            //     float xMiddle = x0 + x1 + x2 - xStart - xEnd;

            //     // 对于每个区间，从左到右遍历每个像素，计算出该像素的颜色值，并将其写入帧缓冲区
            //     for (int x = Mathf.RoundToInt(xStart); x <= Mathf.RoundToInt(xEnd); x++)
            //     {
            //         float t = (x - xStart) / (xEnd - xStart);
            //         if (x < Mathf.RoundToInt(xMiddle))
            //         {
            //             t = (x - xStart) / (xMiddle - xStart);
            //             Color color = Color.Lerp(v0.color, v1.color, t);
            //             m_FrameBuffer.SetColor(x, y, color);
            //         }
            //         else
            //         {
            //             t = (x - xMiddle) / (xEnd - xMiddle);
            //             Color color = Color.Lerp(v1.color, v2.color, t);
            //             m_FrameBuffer.SetColor(x, y, color);
            //         }
            //     }
            // }
        }



        private void OnGUI()
        {
            // draw full screen texture
            var texture = m_Rasterizer?.ColorTexture;
            if (texture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
            }
        }

    }
}
