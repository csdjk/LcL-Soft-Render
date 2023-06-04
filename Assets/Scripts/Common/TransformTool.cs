
using UnityEngine;

namespace LcLSoftRender
{
    /// <summary>
    /// 变换工具类
    /// </summary>
    public class TransformTool
    {
        public static Vector2Int ModelPositionToScreenPosition(Vector3 modelPos, Matrix4x4 matrixMVP, Vector2Int screenSize)
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
            return new Vector2Int(
                Mathf.RoundToInt(screenPos.x),
                Mathf.RoundToInt(screenPos.y)
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

    }
}