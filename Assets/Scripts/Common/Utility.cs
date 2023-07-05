using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace LcLSoftRender
{

    public static class Utility
    {
        public static float4 SampleTexture2D(Texture2D texture, float2 uv)
        {
            if (texture == null)
            {
                return 1;
            }
            return texture.GetPixel((int)(uv.x * texture.width), (int)(uv.y * texture.height)).ToFloat4();
        }

        /// <summary>
        /// 采样纹理，超出范围的部分使用重复的方式填充
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="uv"></param>
        /// <returns></returns>
        public static float4 RepeatSampleTexture2D(Texture2D texture, float2 uv)
        {

            float2 repeatCoord = uv - floor(uv);
            return SampleTexture2D(texture, repeatCoord);
        }
        /// <summary>
        /// 采样纹理，超出范围的部分使用边缘像素填充
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="uv"></param>
        /// <returns></returns>
        public static float4 ClampSampleTexture2D(Texture2D texture, float2 uv)
        {
            float2 clampedCoord = saturate(uv);
            return SampleTexture2D(texture, clampedCoord);
        }
        /// <summary>
        /// 计算天空盒的面以及uv坐标
        /// https://en.wikipedia.org/wiki/Cube_mapping
        /// 有关立方体图采样，请参见小节 3.7.5 
        /// https://www.khronos.org/registry/OpenGL/specs/es/2.0/es_full_spec_2.0.pdf
        /// </summary>
        /// <param name="cubeMap"></param>
        /// <param name="direction"></param>
        /// <param name="uv"></param>
        /// <returns></returns>
        public static Texture2D SelectCubeMapFace(SkyboxImages cubeMap, float3 direction, out float2 uv)
        {
            float abs_x = abs(direction.x);
            float abs_y = abs(direction.y);
            float abs_z = abs(direction.z);
            float ma, sc, tc;
            Texture2D faceTexture;

            if (abs_x > abs_y && abs_x > abs_z)
            {
                // major axis -> x
                ma = abs_x;
                if (direction.x > 0)
                {
                    // positive x
                    faceTexture = cubeMap.left;
                    sc = -direction.z;
                    tc = -direction.y;
                }
                else
                {
                    // negative x
                    faceTexture = cubeMap.right;
                    sc = +direction.z;
                    tc = -direction.y;
                }
            }
            else if (abs_y > abs_z)
            {
                // major axis -> y
                ma = abs_y;
                if (direction.y > 0)
                {
                    // positive y
                    faceTexture = cubeMap.up;
                    sc = +direction.x;
                    tc = +direction.z;
                }
                else
                {
                    // negative y
                    faceTexture = cubeMap.down;
                    sc = +direction.x;
                    tc = -direction.z;
                }
            }
            else
            {
                // major axis -> z
                ma = abs_z;
                if (direction.z > 0)
                {
                    // positive z
                    faceTexture = cubeMap.front;
                    sc = +direction.x;
                    tc = -direction.y;
                }
                else
                {
                    // negative z
                    faceTexture = cubeMap.back;
                    sc = -direction.x;
                    tc = -direction.y;
                }
            }

            uv = new float2((sc / ma + 1) / 2, (tc / ma + 1) / 2);
            return faceTexture;
        }




        /// <summary>
        /// 采样CubeMap
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static float4 SampleCubemap(SkyboxImages texture, float3 direction)
        {
            float2 uv;
            Texture2D faceTexture = SelectCubeMapFace(texture, direction, out uv);
            uv.y = 1 - uv.y;
            return RepeatSampleTexture2D(faceTexture, uv);
        }



        public static float3 UnpackNormal(float3 normal)
        {
            return new float3(normal.x * 2 - 1, normal.y * 2 - 1, normal.z * 2 - 1);
        }

        public static bool DepthTest(float depth, float depthBuffer, ZTest zTest)
        {
            switch (zTest)
            {
                case ZTest.Always:
                    return true;
                case ZTest.Equal:
                    return depth == depthBuffer;
                case ZTest.Greater:
                    return depth > depthBuffer;
                case ZTest.GreaterEqual:
                    return depth >= depthBuffer;
                case ZTest.Less:
                    return depth < depthBuffer;
                case ZTest.LessEqual:
                    return depth <= depthBuffer;
                case ZTest.NotEqual:
                    return depth != depthBuffer;
                case ZTest.Never:
                    return false;
                default:
                    return true;
            }
        }


        public static float4 BlendColors(float4 srcColor, float4 dstColor, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.None:
                    return srcColor;
                case BlendMode.AlphaBlend:
                    return srcColor.w * srcColor + (1 - srcColor.w) * dstColor;
                case BlendMode.Additive:
                    return srcColor + dstColor;
                case BlendMode.Subtractive:
                    return srcColor - dstColor;
                case BlendMode.PremultipliedAlpha:
                    return srcColor + (1 - srcColor.w) * dstColor;
                case BlendMode.Multiply:
                    return srcColor * dstColor;
                case BlendMode.Screen:
                    return srcColor + dstColor - srcColor * dstColor;
                case BlendMode.Overlay:
                    return dstColor.w < 0.5f ? 2 * srcColor * dstColor : 1 - 2 * (1 - srcColor) * (1 - dstColor);
                case BlendMode.Darken:
                    return min(srcColor, dstColor);
                case BlendMode.Lighten:
                    return max(srcColor, dstColor);
                case BlendMode.ColorDodge:
                    return dstColor.Equals(float4(0)) ? dstColor : min(float4(1), srcColor / (float4(1) - dstColor));
                case BlendMode.ColorBurn:
                    return dstColor.Equals(float4(1)) ? dstColor : max(float4(0), (float4(1) - srcColor) / dstColor);
                case BlendMode.SoftLight:
                    return dstColor * (2 * srcColor + srcColor * srcColor - 2 * srcColor * dstColor + 2 * dstColor - 2 * dstColor * dstColor);
                case BlendMode.HardLight:
                    return BlendColors(dstColor, srcColor, BlendMode.Overlay);
                case BlendMode.Difference:
                    return abs(srcColor - dstColor);
                case BlendMode.Exclusion:
                    return srcColor + dstColor - 2 * srcColor * dstColor;
                // case BlendMode.HSLHue:
                //     return ColorExtensions.ColorFromHSL(srcColor.GetHue(), dstColor.GetSaturation(), dstColor.GetBrightness());
                // case BlendMode.HSLSaturation:
                //     return ColorExtensions.ColorFromHSL(srcColor.GetHue(), srcColor.GetSaturation() * dstColor.GetSaturation(), dstColor.GetBrightness());
                // case BlendMode.HSLColor:
                //     return ColorExtensions.ColorFromHSL(dstColor.GetHue(), dstColor.GetSaturation(), srcColor.GetBrightness());
                // case BlendMode.HSLLuminosity:
                //     return ColorExtensions.ColorFromHSL(dstColor.GetHue(), dstColor.GetSaturation(), srcColor.GetBrightness());
                default:
                    return srcColor;
            }
        }

    }
}
