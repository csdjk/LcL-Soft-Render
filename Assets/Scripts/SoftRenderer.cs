using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Mathematics;
namespace LcLSoftRenderer
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
    public class SoftRenderer : MonoBehaviour
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
        public RasterizerType rasterizerType = RasterizerType.CPU;
        public CameraClearFlags clearFlags = CameraClearFlags.Color;
        public Color clearColor = Color.black;

        public PrimitiveType primitiveType = PrimitiveType.Triangle;
        public MSAAMode msaaMode = MSAAMode.None;
        public int frameInterval = 2;
        int m_FrameCount = 0;

        Camera m_Camera;
        IRasterizer m_Rasterizer;
        public IRasterizer rasterizer => m_Rasterizer;
        List<RenderObject> m_RenderObjects = new List<RenderObject>();
        public List<RenderObject> renderObjects => m_RenderObjects;

        public ComputeShader colorComputeShader;
        // instance
        public static SoftRenderer instance;

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
            if (rasterizerType == RasterizerType.GPUDriven)
                m_Rasterizer = new GPURasterizer(m_Camera, colorComputeShader,msaaMode);
            else
                m_Rasterizer = new CPURasterizer(m_Camera, msaaMode);
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
            if (Time.frameCount - m_FrameCount < frameInterval) return;
            m_FrameCount = Time.frameCount;
            Render();
        }
        public void Render()
        {
            Global.ambientColor = RenderSettings.ambientLight.ToFloat4();
            Global.cameraPosition = m_Camera.transform.position;
            Global.cameraDirection = m_Camera.transform.forward;


            Profiler.BeginSample("LcLSoftRender");
            {
                m_Rasterizer.MSAAMode = msaaMode;
                m_Rasterizer?.Clear(clearFlags, clearColor);

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

        public void Dispose()
        {
            m_Rasterizer?.Dispose();
        }

        public void DebugIndex(int debugIndex)
        {
            m_Rasterizer?.SetDebugIndex(debugIndex);
        }

        private void OnGUI()
        {
            var texture = m_Rasterizer?.ColorTexture;
            var screenSize = new Vector2(Screen.width, Screen.height);
            if (texture != null && active)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture, ScaleMode.ScaleToFit, false);
            }

            if (GUILayout.Button("LcL Render", GUILayout.Width(screenSize.x / 10), GUILayout.Height(screenSize.x / 20)))
            {
                active = true;
                Init();
                Render();
            }
            if (GUILayout.Button("Unity Render", GUILayout.Width(screenSize.x / 10), GUILayout.Height(screenSize.x / 20)))
            {
                active = false;
            }
        }

    }
}
