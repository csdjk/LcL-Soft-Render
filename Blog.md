# 基于 Unity 的软光栅化

最近闲暇时基于 Unity 实现了一个软光栅化，之前一直都没有亲自实现过。这次也正好练习一下，顺带熟悉下渲染流程和相关算法。

至于为什么要选在 Unity 中实现呢？因为我对 C++不是很熟悉，懒得折腾了，并且也正好熟悉一下 ComputeShader，而且还有一个优点就是可以直接和 Unity 的渲染结果做对比。

分别实现 CPU Rasterizer（纯 C#）和 GPU Rasterizer（Compute Shader）。

在本文中，我将重点介绍 CPU Rasterizer 的实现，至于 GPU Rasterizer 用的算法都是一样的，这里就不赘述了。源码放在我的[github](https://github.com/csdjk/LcL-Soft-Render)上了，有兴趣的可以看看。

由于篇幅原因以下大部分的算法都不会过于详细的解释(不会，抄就完事*^▽^*)，但我会提供相关的参考链接和公式推导链接。
并且后面提供的代码也不是完整的，只截取关键部分，完整的代码可以去 github 上查看。

## 准备工作

1. 为了更好的符合图形编程的习惯，这里引入了 Mathematics 库，方便后面的运算中使用 float4、float4x4 等数据类型和“.xyz”等语法。![1693648263522](https://file+.vscode-resource.vscode-cdn.net/e%3A/LiChangLong/LcL-Soft-Render/image/%E7%9F%A5%E4%B9%8E/1693648263522.png)
2. 由于需要读取模型和贴图数据，所以需要开启模型和贴图的读写，这里就简单编写了一个资源导入脚本，导入资源的时候会自动开启读写功能。

   ```csharp
   using UnityEditor;
   using UnityEngine;
   public class LcLAssetPostprocessor : AssetPostprocessor
   {
       void OnPreprocessModel()
       {
           var importer = assetImporter as ModelImporter;
           importer.isReadable = true;
       }

       void OnPreprocessTexture()
       {
           var importer = assetImporter as TextureImporter;
           importer.isReadable = true;
       }
   }
   ```

## 实现

### 渲染流程

在实现之前，我们肯定要熟悉整个渲染流水线，这里贴个简单的流程图，具体细节这里就不赘述了，可以网上查查，或者参考[这篇文章](https://www.lfzxb.top/shader-render-pipeline/)。

![1693706868874](image/Blog/1693706868874.png)

### 框架搭建

在这里为了让软光栅化的渲染结果能够显示出来，我使用了 Texture2D 作为渲染目标，并将 ColorBuffer 通过 SetPixels 方法设置到 Texture2D 中。最后通过 GUI.DrawTexture 方法将 Texture2D 绘制到屏幕上。

首先先创建创建一个 IRasterizer 接口，用于定义 CPU 和 GPU Rasterizer 的公共方法。

```csharp
public interface IRasterizer
{
    public abstract Texture ColorTexture { get; }
    public MSAAMode MSAAMode { get; set; }
    public float4x4 MatrixVP { get; }

    public abstract void Render(List<RenderObject> renderObjects);
    public void SetPrimitiveType(PrimitiveType primitiveType);
    public void SetMatrixVP(float4x4 matrixVP);
    public abstract void Clear(CameraClearFlags clearFlags, Color? clearColor = null, float depth = float.PositiveInfinity);
    public abstract void Dispose();
}
```

然后创建一个 CPURasterizer 类,实现 IRasterizer 接口。

创建一个 SoftRenderer 类，用于管理光栅化的相关操作。

```csharp
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
    [Range(0.01f, 10)]
    public float frameInterval = 0.1f;
    int m_FrameCount = 0;
    Camera m_Camera;
    IRasterizer m_Rasterizer;
    public IRasterizer rasterizer => m_Rasterizer;
    List<RenderObject> m_RenderObjects = new List<RenderObject>();
    public List<RenderObject> renderObjects => m_RenderObjects;

    public ComputeShader colorComputeShader;
    public static SoftRenderer instance;
    void Awake()
    {
        instance = this;
        Init();
        Render();
    }
    public void Init()
    {
        m_Camera = GetComponent<Camera>();
        if (rasterizerType == RasterizerType.GPUDriven)
            m_Rasterizer = new GPURasterizer(m_Camera, colorComputeShader, msaaMode);
        else
            m_Rasterizer = new CPURasterizer(m_Camera, msaaMode);
        CollectRenderObjects();
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

```

SoftRenderer 脚本挂载在摄像机上，读取 Camera 的相关参数，设置一些渲染参数。

- Init 方法用于初始化光栅化器，收集渲染对象，设置渲染参数等。
- CollectRenderObjects 方法用于收集所有的渲染对象（RenderObject）。
- Render 方法用于渲染，这里主要是设置一些全局参数，然后调用光栅化器的相关方法进行渲染。

在 OnGUI 方法中，通过 GUI.DrawTexture 方法将渲染结果绘制到屏幕上。这里还添加了两个按钮，用于切换渲染方式。一个是使用软光栅化渲染，一个是使用 Unity 的渲染。方便对比。

![1693714611400](image/Blog/1693714611400.png)

### 计算 MVP 矩阵

创建一个 RenderObject 类，用于存储渲染对象的相关数据。挂载在场景中的物体上。

```csharp
// RenderObject
public void Init()
{
    CalculateMatrix();

    var mesh = GetComponent<MeshFilter>()?.sharedMesh;
    if (mesh == null)
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
```

- Init 方法: 用于读取模型数据，创建 Vertex Buffer 和 Index Buffer，其中 Vertex 类也是自定义的，用于存储顶点数据,例如顶点坐标、UV、法线、切线、颜色等。
- CalculateMatrix 方法: 用于计算模型矩阵，公式推导可以参考[这篇文章](https://blog.csdn.net/silangquan/article/details/50984641)。

其中 TransformTool 工具类的 QuaternionToMatrix 方法用于将四元数转换为旋转矩阵。
公式推导可以参考[彻底搞懂四元数](https://blog.csdn.net/silangquan/article/details/39008903)。

实现如下:

```csharp
public static float4x4 QuaternionToMatrix(Quaternion rotation)
{
    float x = rotation.x;
    float y = rotation.y;
    float z = rotation.z;
    float w = rotation.w;
    // 模长
    float n = 1.0f / sqrt(x * x + y * y + z * z + w * w);
    // 归一化,将四元数的四个分量除以它们的模长
    x *= n;
    y *= n;
    z *= n;
    w *= n;

    // R = | 1 - 2y^2 - 2z^2   2xy - 2zw       2xz + 2yw       0 |
    //     | 2xy + 2zw         1 - 2x^2 - 2z^2   2yz - 2xw       0 |
    //     | 2xz - 2yw         2yz + 2xw        1 - 2x^2 - 2y^2  0 |
    //     | 0                 0                0               1 |
    float4x4 matrix = float4x4(
        1.0f - 2.0f * y * y - 2.0f * z * z, 2.0f * x * y - 2.0f * z * w, 2.0f * x * z + 2.0f * y * w, 0.0f,
        2.0f * x * y + 2.0f * z * w, 1.0f - 2.0f * x * x - 2.0f * z * z, 2.0f * y * z - 2.0f * x * w, 0.0f,
        2.0f * x * z - 2.0f * y * w, 2.0f * y * z + 2.0f * x * w, 1.0f - 2.0f * x * x - 2.0f * y * y, 0.0f,
        0.0f, 0.0f, 0.0f, 1.0f
        );
    return matrix;
}
```

计算 View 矩阵和 Projection 矩阵的方法可以参考
[详解 MVP 矩阵之 ViewMatrix](https://blog.csdn.net/silangquan/article/details/50987196?ops_request_misc=%257B%2522request%255Fid%2522%253A%2522169492856116800192269602%2522%252C%2522scm%2522%253A%252220140713.130102334.pc%255Fblog.%2522%257D&request_id=169492856116800192269602&biz_id=0&utm_medium=distribute.pc_search_result.none-task-blog-2~blog~first_rank_ecpm_v1~rank_v31_ecpm-1-50987196-null-null.268^v1^koosearch&utm_term=MVP)。
[详解 MVP 矩阵之 ProjectionMatrix](https://blog.csdn.net/silangquan/article/details/52705150?ops_request_misc=%257B%2522request%255Fid%2522%253A%2522169492856116800192269602%2522%252C%2522scm%2522%253A%252220140713.130102334.pc%255Fblog.%2522%257D&request_id=169492856116800192269602&biz_id=0&utm_medium=distribute.pc_search_result.none-task-blog-2~blog~first_rank_ecpm_v1~rank_v31_ecpm-3-52705150-null-null.268^v1^koosearch&utm_term=MVP)

```csharp
public static float4x4 CreateViewMatrix(float3 position, float3 forward, float3 up)
{
    // 计算相机的右向量
    float3 right = normalize(cross(up, forward));

    // 计算相机的上向量
    up = normalize(cross(forward, right));

    // 创建一个变换矩阵，将相机的位置和方向转换为一个矩阵
    var viewMatrix = new float4x4(
        float4(right.x, up.x, forward.x, 0),
        float4(right.y, up.y, forward.y, 0),
        float4(right.z, up.z, forward.z, 0),
        float4(-dot(right, position), -dot(up, position), -dot(forward, position), 1)
    );
    return viewMatrix;
}
///  透视投影矩阵
public static float4x4 Perspective(float near, float far, float fov, float aspect)
{
    float rad = fov * Mathf.Deg2Rad;
    float tanHalfFov = Mathf.Tan(rad / 2);
    float fovY = 1 / tanHalfFov;
    float fovX = fovY / aspect;
    float4x4 perspectiveMatrix = new float4x4(
                                        float4(fovX, 0, 0, 0),
                                        float4(0, fovY, 0, 0),
                                        float4(0, 0, (far + near) / (far - near), 1),
                                        float4(0, 0, -(2 * far * near) / (far - near), 0)
                                            );
    return perspectiveMatrix;
}
///  正交投影矩阵
public static float4x4 Orthographic(float near, float far, float height, float aspect)
{
    float width = height * aspect;
    float4x4 orthographicMatrix = new float4x4(float4(2f / width, 0, 0, 0),
                                            float4(0, 2f / height, 0, 0),
                                            float4(0, 0, 2f / (far - near), 0),
                                            float4(0, 0, -(far + near) / (far - near), 1)
                                                );
    return orthographicMatrix;
}
```

有了 MVP 矩阵，我们就可以将模型空间的坐标转换到裁剪空间了。
有了 clip pos 还需要把裁剪空间的坐标转换到屏幕空间的坐标，算法如下：

```csharp
// 将裁剪空间中的坐标转换到屏幕空间中的坐标
public static float4 ClipPositionToScreenPosition(float4 clipPos, Camera camera, out float3 ndcPos)
{
    // 将裁剪空间中的坐标转换为NDC空间中的坐标
    ndcPos = clipPos.xyz / clipPos.w;
    // 将NDC空间中的坐标转换为屏幕空间中的坐标
    float4 screenPos = new float4(
        (ndcPos.x + 1.0f) * 0.5f * (camera.pixelWidth - 1),
        (ndcPos.y + 1.0f) * 0.5f * (camera.pixelHeight - 1),
        // ndcPos.z * (f - n) / 2 + (f + n) / 2,
        ndcPos.z * 0.5f + 0.5f,
        // w透视矫正系数
        clipPos.w
    );
    return screenPos;
}
```

### 绘制线框

有了 Vertex Buffer 和 Index Buffer，以及 MVP 矩阵，我们就可以开始绘制了。这里先绘制线框，看看是否能够正确的显示出来。
绘制算法用的是 Bresenham 算法，原理可以Google一下，网上很多，或者也可以参考[这篇文章](https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm)。

```csharp
/// Bresenham's 画线算法
private void DrawLine(float3 v0, float3 v1, Color color)
{
    int x0 = (int)v0.x;
    int y0 = (int)v0.y;
    int x1 = (int)v1.x;
    int y1 = (int)v1.y;

    int dx = abs(x1 - x0);
    int dy = abs(y1 - y0);
    int sx = x0 < x1 ? 1 : -1;
    int sy = y0 < y1 ? 1 : -1;
    int err = dx - dy;

    while (true)
    {
        m_FrameBuffer.SetColor(x0, y0, color);

        if (x0 == x1 && y0 == y1)
        {
            break;
        }

        int e2 = 2 * err;

        if (e2 > -dy)
        {
            err -= dy;
            x0 += sx;
        }

        if (e2 < dx)
        {
            err += dx;
            y0 += sy;
        }
    }
}

```

效果如下:

