
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace LcLSoftRender
{
    /// <summary>
    /// 变换工具类
    /// </summary>
    public class TransformTool
    {
        // 将裁剪空间中的坐标转换到屏幕空间中的坐标
        public static float4 ClipPositionToScreenPosition(float4 clipPos, out float3 ndcPos)
        {
            // 将裁剪空间中的坐标转换为NDC空间中的坐标
            ndcPos = clipPos.xyz / clipPos.w;
            // 将NDC空间中的坐标转换为屏幕空间中的坐标
            float4 screenPos = new float4(
                (ndcPos.x + 1.0f) * 0.5f * Global.screenSize.x,
                (ndcPos.y + 1.0f) * 0.5f * Global.screenSize.y,
                // z深度
                ndcPos.z,
                // w透视矫正系数
                clipPos.w
            );
            return screenPos;
        }
        /// <summary>
        /// 将坐标从模型空间转换到其次裁剪空间
        /// </summary>
        /// <param name="modelPos"></param>
        /// <param name="matrixMVP"></param>
        /// <returns></returns>
        public static float4 TransformObjectToHClip(float3 modelPos, float4x4 matrixMVP)
        {
            // float4 clipPos = mul(matrixMVP, float4(modelPos.xyz, 1));
            return mul(matrixMVP, float4(modelPos.xyz, 1.0f));
        }

        /// <summary>
        /// 透视投影矩阵VP
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static float4x4 CreateMatrixVP(Camera camera)
        {
            float3 position = camera.transform.position;
            float3 forward = camera.transform.forward;
            float3 up = camera.transform.up;
            // 创建一个视图变换矩阵
            var viewMatrix = CreateViewMatrix(position, forward, up);
            // float4x4 viewMatrix = float4x4.TRS(camera.transform.position, camera.transform.rotation, float3.one).inverse;

            // 创建一个透视投影矩阵
            float4x4 projectionMatrix = Perspective(camera.nearClipPlane, camera.farClipPlane, camera.fieldOfView, camera.aspect);

            // 将视图矩阵和投影矩阵相乘，得到最终的视图投影矩阵
            return mul(projectionMatrix, viewMatrix);
        }

        public static float4x4 CreateOrthographicMatrixVP(Camera camera)
        {
            float3 position = camera.transform.position;
            float3 forward = camera.transform.forward;
            float3 up = camera.transform.up;
            // 创建一个视图变换矩阵
            var viewMatrix = CreateViewMatrix(position, forward, up);

            // 创建一个正交投影矩阵
            float4x4 projectionMatrix = Orthographic(camera.nearClipPlane, camera.farClipPlane, camera.orthographicSize * 2, camera.aspect);

            // 将视图矩阵和投影矩阵相乘，得到最终的视图投影矩阵
            return mul(projectionMatrix, viewMatrix);
        }

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


        /// <summary>
        ///  正交投影矩阵
        /// </summary>
        /// <param name="near"></param>
        /// <param name="far"></param>
        /// <param name="height"></param>
        /// <param name="aspect"></param>
        /// <returns></returns>

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


        /// <summary>
        ///  透视投影矩阵
        /// </summary>
        /// <param name="near"></param>
        /// <param name="far"></param>
        /// <param name="fov"></param>
        /// <param name="aspect"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 高效的重心坐标算法
        /// (https://github.com/ssloy/tinyrenderer/wiki/Lesson-2:-Triangle-rasterization-and-back-face-culling)
        /// </summary>
        /// <param name="P"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float3 BarycentricCoordinate(float2 P, float2 v0, float2 v1, float2 v2)
        {
            var v2v0 = v2 - v0;
            var v1v0 = v1 - v0;
            var v0P = v0 - P;
            float3 u = cross(float3(v2v0.x, v1v0.x, v0P.x), float3(v2v0.y, v1v0.y, v0P.y));
            // float3 u = cross(float3(v2.x - v0.x, v1.x - v0.x, v0.x - P.x), float3(v2.y - v0.y, v1.y - v0.y, v0.y - P.y));
            if (abs(u.z) < 1) return float3(-1, 1, 1);
            return float3(1 - (u.x + u.y) / u.z, u.y / u.z, u.x / u.z);
        }

        /// <summary>
        ///  重心坐标(https://zhuanlan.zhihu.com/p/538468807)
        /// </summary>
        /// <param name="p"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float3 BarycentricCoordinate2(float2 p, float3 v0, float3 v1, float3 v2)
        {
            // 计算三角形三个边的向量(左手坐标系，顺时针为正，逆时针为负)
            float3 v0v1 = new float3(v1.xy - v0.xy, 0);
            float3 v1v2 = new float3(v2.xy - v1.xy, 0);
            float3 v2v0 = new float3(v0.xy - v2.xy, 0);
            // 计算点p到三角形三个顶点的向量
            float3 v0p = new float3(p - v0.xy, 0);
            float3 v1p = new float3(p - v1.xy, 0);
            float3 v2p = new float3(p - v2.xy, 0);

            // 计算三角形的法向量，用来判断三角形的正反面
            var normal = cross(v2v0, v0v1);
            // 大三角形面积，这里没有除以2，因为后面计算的时候会相互抵消
            float area = abs(normal.z);
            // 方向向量
            normal = normalize(normal);

            // 计算三角形的面积：
            // 叉乘可以用来计算两个向量所在平行四边形的面积，因为叉乘的结果是一个向量，
            // 将这个向量与单位法向量进行点乘，可以得到一个有向的面积。
            // 小三角形面积
            // float area0 = dot(cross(v1v2, v1p), normal);
            // float area1 = dot(cross(v2v0, v2p), normal);
            // float area2 = dot(cross(v0v1, v0p), normal);

            // 又因为所有的点z都为0，所以z就是向量的模长，也就是面积，所以可以进一步简化为：
            float area0 = cross(v1v2, v1p).z * normal.z;
            float area1 = cross(v2v0, v2p).z * normal.z;
            float area2 = cross(v0v1, v0p).z * normal.z;


            return new float3(area0 / area, area1 / area, area2 / area);
        }

    }
}