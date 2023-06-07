using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Mathematics;

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
            }
        }

        private void Update()
        {
            Global.ambientColor = RenderSettings.ambientLight;
            Global.screenSize = new Vector2Int(Screen.width, Screen.height);


            Profiler.BeginSample("LcLSoftRender");
            {
                m_Rasterizer?.Clear(ClearMask.COLOR | ClearMask.DEPTH, clearColor);


                float4x4 matrixVP;
                if (m_Camera.orthographic)
                {
                    matrixVP = TransformTool.CreateOrthographicMatrixVP(m_Camera);
                }
                else
                {
                    matrixVP = TransformTool.CreateMatrixVP(m_Camera);
                }
                m_Rasterizer?.SetMatrixVP(matrixVP);
                m_Rasterizer?.SetPrimitiveType(primitiveType);
                m_Rasterizer?.Render(m_RenderObjects);

            }
            Profiler.EndSample();
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
