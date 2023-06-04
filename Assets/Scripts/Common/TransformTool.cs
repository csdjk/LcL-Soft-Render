
using UnityEngine;

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
        public static Vector3 ModelPositionToScreenPosition(Vector3 modelPos, Matrix4x4 matrixMVP, Vector2Int screenSize)
        {
            // 将模型空间中的顶点坐标转换为裁剪空间中的坐标
            Vector4 clipPos = matrixMVP * new Vector4(modelPos.x, modelPos.y, modelPos.z, 1);
            // 将裁剪空间中的坐标转换为NDC空间中的坐标
            Vector3 ndcPos = clipPos / clipPos.w;
            // 将NDC空间中的坐标转换为屏幕空间中的坐标
            Vector2 screenPos = new Vector2(
                (ndcPos.x + 1.0f) * 0.5f * screenSize.x,
                (ndcPos.y + 1.0f) * 0.5f * screenSize.y
            );

            // 将屏幕空间中的坐标转换为像素坐标
            return new Vector3(
                Mathf.RoundToInt(screenPos.x),
                Mathf.RoundToInt(screenPos.y),
                ndcPos.z
            );
        }
        /// <summary>
        /// 透视投影矩阵VP
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static Matrix4x4 CreateMatrixVP(Camera camera)
        {
            Vector3 position = camera.transform.position;
            Vector3 forward = camera.transform.forward;
            Vector3 up = camera.transform.up;
            // 创建一个视图变换矩阵
            var viewMatrix = CreateViewMatrix(position, forward, up);
            // Matrix4x4 viewMatrix = Matrix4x4.TRS(camera.transform.position, camera.transform.rotation, Vector3.one).inverse;

            // 创建一个透视投影矩阵
            Matrix4x4 projectionMatrix = Perspective(camera.nearClipPlane, camera.farClipPlane, camera.fieldOfView, camera.aspect);

            // 将视图矩阵和投影矩阵相乘，得到最终的视图投影矩阵
            return projectionMatrix * viewMatrix;
        }

        public static Matrix4x4 CreateOrthographicMatrixVP(Camera camera)
        {
            Vector3 position = camera.transform.position;
            Vector3 forward = camera.transform.forward;
            Vector3 up = camera.transform.up;
            // 创建一个视图变换矩阵
            var viewMatrix = CreateViewMatrix(position, forward, up);

            // 创建一个正交投影矩阵
            Matrix4x4 projectionMatrix = Orthographic(camera.nearClipPlane, camera.farClipPlane, camera.orthographicSize * 2, camera.aspect);

            // 将视图矩阵和投影矩阵相乘，得到最终的视图投影矩阵
            return projectionMatrix * viewMatrix;
        }

        public static Matrix4x4 CreateViewMatrix(Vector3 position, Vector3 forward, Vector3 up)
        {
            // 计算相机的右向量
            Vector3 right = Vector3.Cross(up, forward).normalized;

            // 计算相机的上向量
            up = Vector3.Cross(forward, right).normalized;

            // 创建一个变换矩阵，将相机的位置和方向转换为一个矩阵
            var viewMatrix = new Matrix4x4(
               new Vector4(right.x, up.x, forward.x, 0),
               new Vector4(right.y, up.y, forward.y, 0),
               new Vector4(right.z, up.z, forward.z, 0),
               new Vector4(-Vector3.Dot(right, position), -Vector3.Dot(up, position), -Vector3.Dot(forward, position), 1)
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

        public static Matrix4x4 Orthographic(float near, float far, float height, float aspect)
        {
            float width = height * aspect;
            Matrix4x4 orthographicMatrix = new Matrix4x4(new Vector4(2f / width, 0, 0, 0),
                                                         new Vector4(0, 2f / height, 0, 0),
                                                         new Vector4(0, 0, 2f / (far - near), 0),
                                                         new Vector4(0, 0, -(far + near) / (far - near), 1));
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
        public static Matrix4x4 Perspective(float near, float far, float fov, float aspect)
        {
            float rad = fov * Mathf.Deg2Rad;
            float tanHalfFov = Mathf.Tan(rad / 2);
            float fovY = 1 / tanHalfFov;
            float fovX = fovY / aspect;
            Matrix4x4 perspectiveMatrix = new Matrix4x4(new Vector4(fovX, 0, 0, 0),
                                                        new Vector4(0, fovY, 0, 0),
                                                        new Vector4(0, 0, (far + near) / (far - near), 1),
                                                        new Vector4(0, 0, -(2 * far * near) / (far - near), 0));
            return perspectiveMatrix;
        }

        // public static Matrix4x4 Perspective3(float near, float far, float fov, float aspect)
        // {
        //     float height = 2 * near * Mathf.Tan(Mathf.Deg2Rad * (fov / 2));
        //     float width = aspect * height;

        //     Matrix4x4 perspectiveMatrix = new Matrix4x4(new Vector4(2 * near / width, 0, 0, 0),
        //                                                 new Vector4(0, 2 * near / height, 0, 0),
        //                                                 new Vector4(0, 0, (near + far) / (far - near), 1),
        //                                                 new Vector4(0, 0, -(2 * near * far) / (far - near), 0));

        //     return perspectiveMatrix;
        // }

        // public static Vector3 ScreenPositionToBarycentric(Vector2 point, Vector3 v0, Vector3 v1, Vector3 v2)
        // {
        //     v0.z = 0;
        //     v1.z = 0;
        //     v2.z = 0;
        //     // 计算三角形的面积
        //     float area = Vector3.Cross(v1 - v0, v2 - v0).magnitude;

        //     // 计算像素点到三角形三个顶点的距离
        //     float d0 = Vector2.Distance(point, new Vector2(v0.x, v0.y));
        //     float d1 = Vector2.Distance(point, new Vector2(v1.x, v1.y));
        //     float d2 = Vector2.Distance(point, new Vector2(v2.x, v2.y));

        //     // 将 screenPos 向量转换为 Vector3 类型的向量
        //     Vector3 screenPos3 = new Vector3(point.x, point.y, 0);

        //     // 计算像素点的重心坐标
        //     float w0 = Vector3.Cross(v1 - v2, screenPos3 - v2).magnitude / area;
        //     float w1 = Vector3.Cross(v2 - v0, screenPos3 - v0).magnitude / area;
        //     float w2 = Vector3.Cross(v0 - v1, screenPos3 - v1).magnitude / area;

        //     return new Vector3(w0, w1, w2);
        // }
        public static Vector3 ComputeBarycentricCoordinates(Vector2 p, Vector2 v0, Vector2 v1, Vector2 v2)
        {
            // Compute vectors
            Vector2 v01 = v1 - v0;
            Vector2 v02 = v2 - v0;
            Vector2 vp0 = p - v0;

            // Compute dot products
            float dot00 = Vector2.Dot(v01, v01);
            float dot01 = Vector2.Dot(v01, v02);
            float dot02 = Vector2.Dot(v01, vp0);
            float dot11 = Vector2.Dot(v02, v02);
            float dot12 = Vector2.Dot(v02, vp0);

            // Compute barycentric coordinates
            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            float w = 1 - u - v;

            return new Vector3(u, v, w);
        }
        public static Vector3 ScreenPositionToBarycentric(Vector3 point, Vector3 A, Vector3 B, Vector3 C)
        {
            var weightA =
                    ((A.y - B.y) * point.x + (B.x - A.x) * point.y + A.x * B.y - B.x * A.y)
                    / ((A.y - B.y) * C.x + (B.x - A.x) * C.y + A.x * B.y - B.x * A.y);

            var weightB =
                    ((A.y - C.y) * point.x + (C.x - A.x) * point.y + A.x * C.y - C.x * A.y)
                    / ((A.y - C.y) * B.x + (C.x - A.x) * B.y + A.x * C.y - C.x * A.y);

            var weightC = 1 - weightA - weightB;
            return new Vector3(weightA, weightB, weightC);
        }
    }
}