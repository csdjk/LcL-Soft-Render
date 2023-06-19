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
        public bool active
        {
            get
            {
                return m_Camera.cullingMask == 0;
            }
            set
            {
                m_Camera.cullingMask = value ? 0 : 1;
            }
        }
        public RasterizerType RasterizerType = RasterizerType.CPU;
        public Color clearColor = Color.black;
        public PrimitiveType primitiveType = PrimitiveType.Triangle;
        private int m_FrameCount = 0;
        public int m_FrameInterval = 2;

        Camera m_Camera;
        CPURasterizer m_Rasterizer;
        public CPURasterizer rasterizer => m_Rasterizer;
        List<RenderObject> m_RenderObjects = new List<RenderObject>();
        public List<RenderObject> renderObjects => m_RenderObjects;
        List<MeshFilter> m_Meshes = new List<MeshFilter>();
        // instance
        public static SoftRender instance;

        void Awake()
        {
            Init();
            Render();
        }
        void OnEnable()
        {
            instance = this;
        }

        public void Init()
        {
            m_Camera = GetComponent<Camera>();
            m_Rasterizer = new CPURasterizer(m_Camera);
            CollectRenderObjects();
            DisableUnityCamera();
        }

        private void OnDisable()
        {
            m_Camera.cullingMask = 1;
            instance = null;
        }

        private void DisableUnityCamera()
        {
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
            // 每隔m_FrameInterval帧执行一次渲染
            if (Time.frameCount - m_FrameCount < m_FrameInterval) return;
            m_FrameCount = Time.frameCount;
            Render();
        }
        public void Render()
        {
            Global.ambientColor = RenderSettings.ambientLight;

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

        public void DebugIndex(int debugIndex)
        {
            m_Rasterizer?.SetDebugIndex(debugIndex);
        }

        private void OnGUI()
        {
            var texture = m_Rasterizer?.ColorTexture;
            if (texture != null && active)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
            }

            if (GUILayout.Button("LcL Render"))
            {
                active = true;
                Init();
                Render();
            }
            if (GUILayout.Button("Unity Render"))
            {
                active = false;
            }
        }

    }
}
