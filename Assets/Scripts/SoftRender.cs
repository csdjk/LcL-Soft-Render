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

    [ExecuteAlways]
    public class SoftRender : MonoBehaviour
    {
        public RasterizerType RasterizerType = RasterizerType.CPU;
        public Color clearColor = Color.black;
        public PrimitiveType primitiveType = PrimitiveType.Triangle;
        Camera m_Camera;
        CPURasterizer m_Rasterizer;
        List<RenderObject> m_RenderObjects = new List<RenderObject>();
        public List<RenderObject> renderObjects => m_RenderObjects;
        List<MeshFilter> m_Meshes = new List<MeshFilter>();

        private void Awake()
        {
            Init();
        }

        void Init()
        {
            var width = Screen.width;
            var height = Screen.height;
            m_Camera = GetComponent<Camera>();
            m_Rasterizer = new CPURasterizer(width, height, m_Camera);
            CollectRenderObjects();
            DisableUnityCamera();
        }

        private void DisableUnityCamera()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            m_Camera.cullingMask = 0;
        }

        // 收集所有的渲染对象
        public void CollectRenderObjects()
        {
            m_RenderObjects = FindObjectsOfType<RenderObject>().ToList();
            foreach (var obj in m_RenderObjects)
            {
                obj.Init();
            }
            SortRenderObjects();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Global.ambientColor = RenderSettings.ambientLight;
            Global.screenSize = new int2(Screen.width, Screen.height);
            Global.cameraPosition = m_Camera.transform.position;


            Profiler.BeginSample("LcLSoftRender");
            {
                m_Rasterizer?.Clear(ClearMask.COLOR | ClearMask.DEPTH, clearColor);


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
                m_Rasterizer?.SetPrimitiveType(primitiveType);

                SortRenderObjects();
                m_Rasterizer?.Render(m_RenderObjects);

            }
            Profiler.EndSample();
        }

        void SortRenderObjects()
        {
            m_RenderObjects.Sort((a, b) =>
           {
               if (a.renderQueue == b.renderQueue)
               {
                   if (a.isTransparent)
                   {
                       float distanceA = Vector3.Distance(a.transform.position, m_Camera.transform.position);
                       float distanceB = Vector3.Distance(b.transform.position, m_Camera.transform.position);
                       return distanceB.CompareTo(distanceA);
                   }
                   else
                   {
                       // 由近到远排序
                       float distanceA = Vector3.Distance(a.transform.position, m_Camera.transform.position);
                       float distanceB = Vector3.Distance(b.transform.position, m_Camera.transform.position);
                       return distanceA.CompareTo(distanceB);
                   }
               }
               return a.renderQueue.CompareTo(b.renderQueue);
           });
        }

        public void DebugIndex(int debugIndex){
            m_Rasterizer?.SetDebugIndex(debugIndex);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            // draw full screen texture
            var texture = m_Rasterizer?.ColorTexture;
            if (texture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
            }
        }

    }
}
