
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
        /// <summary>
        /// 将模型空间中的顶点坐标转换为裁剪空间中的坐标
        /// </summary>
        /// <param name="modelPos"></param>
        /// <param name="matrixMVP"></param>
        /// <param name="screenSize"></param>
        /// <returns></returns>
        public static float4 ModelPositionToScreenPosition(float3 modelPos, float4x4 matrixMVP, int2 screenSize)
        {
            matrixMVP = transpose(matrixMVP);
            // 将模型空间中的顶点坐标转换为裁剪空间中的坐标
            float4 clipPos = mul(matrixMVP, float4(modelPos.xyz, 1));
            // 将裁剪空间中的坐标转换为NDC空间中的坐标
            float3 ndcPos = clipPos.xyz / clipPos.w;
            // 将NDC空间中的坐标转换为屏幕空间中的坐标
            float2 screenPos = new float2(
                (ndcPos.x + 1.0f) * 0.5f * screenSize.x,
                (ndcPos.y + 1.0f) * 0.5f * screenSize.y
            );

            // 将屏幕空间中的坐标转换为像素坐标
            return float4(
                Mathf.RoundToInt(screenPos.x),
                Mathf.RoundToInt(screenPos.y),
                ndcPos.z,
                1
            );
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
            // return viewMatrix * projectionMatrix;
            var vp = mul(projectionMatrix, viewMatrix);
            return vp;
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
            return projectionMatrix * viewMatrix;
        }

        public static float4x4 CreateViewMatrix(float3 position, float3 forward, float3 up)
        {
            // 计算相机的右向量
            float3 right = normalize(cross(up, forward));

            // 计算相机的上向量
            up = normalize(cross(forward, right));

            // 创建一个变换矩阵，将相机的位置和方向转换为一个矩阵
            var viewMatrix = new float4x4(
               new float4(right.x, up.x, forward.x, 0),
               new float4(right.y, up.y, forward.y, 0),
               new float4(right.z, up.z, forward.z, 0),
               new float4(-dot(right, position), -dot(up, position), -dot(forward, position), 1)
           );

            return (viewMatrix);
            return transpose(viewMatrix);
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
            float4x4 orthographicMatrix = new float4x4(new float4(2f / width, 0, 0, 0),
                                                         new float4(0, 2f / height, 0, 0),
                                                         new float4(0, 0, 2f / (far - near), 0),
                                                         new float4(0, 0, -(far + near) / (far - near), 1));
            return (orthographicMatrix);
            return transpose(orthographicMatrix);
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
            float4x4 perspectiveMatrix = new float4x4(new float4(fovX, 0, 0, 0),
                                                        new float4(0, fovY, 0, 0),
                                                        new float4(0, 0, (far + near) / (far - near), -(2 * far * near) / (far - near)),
                                                        new float4(0, 0, 1, 0));
            return (perspectiveMatrix);
            return transpose(perspectiveMatrix);
        }

        // public static float4x4 Perspective3(float near, float far, float fov, float aspect)
        // {
        //     float height = 2 * near * Mathf.Tan(Mathf.Deg2Rad * (fov / 2));
        //     float width = aspect * height;

        //     float4x4 perspectiveMatrix = new float4x4(new float4(2 * near / width, 0, 0, 0),
        //                                                 new float4(0, 2 * near / height, 0, 0),
        //                                                 new float4(0, 0, (near + far) / (far - near), 1),
        //                                                 new float4(0, 0, -(2 * near * far) / (far - near), 0));

        //     return perspectiveMatrix;
        // }
        public static float3 ComputeBarycentricCoordinates(float2 p, float2 v0, float2 v1, float2 v2)
        {
            // Compute vectors
            float2 v01 = v1 - v0;
            float2 v02 = v2 - v0;
            float2 vp0 = p - v0;

            // Compute dot products
            float dot00 = dot(v01, v01);
            float dot01 = dot(v01, v02);
            float dot02 = dot(v01, vp0);
            float dot11 = dot(v02, v02);
            float dot12 = dot(v02, vp0);

            // Compute barycentric coordinates
            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            float w = 1 - u - v;

            return new float3(u, v, w);
        }
    }
}