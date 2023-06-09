using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace LcLSoftRender
{

    public static class Utility
    {
        public static float4 tex2D(Texture2D texture, float2 uv)
        {
            if (texture == null)
            {
                return 1;
            }
            return texture.GetPixel((int)(uv.x * texture.width), (int)(uv.y * texture.height)).ToFloat4();
        }

        public static Vector3 ColorToVector3(Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }

        public static Vector3 UnpackNormal(Vector3 normal)
        {
            return new Vector3(normal.x * 2 - 1, normal.y * 2 - 1, normal.z * 2 - 1);
        }
    }
}
