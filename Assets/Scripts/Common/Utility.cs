using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;


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

        /// <summary>
        /// 计算天空盒的面以及uv坐标
        /// https://www.khronos.org/registry/OpenGL/specs/es/2.0/es_full_spec_2.0.pdf
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="texcoord"></param>
        /// <returns></returns>
        public static int SelectCubemapFace(float3 direction, out float2 texcoord)
        {
            float abs_x = abs(direction.x);
            float abs_y = abs(direction.y);
            float abs_z = abs(direction.z);
            float ma, sc, tc;
            int face_index;

            if (abs_x > abs_y && abs_x > abs_z)
            {   // major axis -> x
                ma = abs_x;
                if (direction.x > 0)
                {                  // positive x
                    face_index = 0;
                    sc = -direction.z;
                    tc = -direction.y;
                }
                else
                {                                // negative x
                    face_index = 1;
                    sc = +direction.z;
                    tc = -direction.y;
                }
            }
            else if (abs_y > abs_z)
            {             // major axis -> y
                ma = abs_y;
                if (direction.y > 0)
                {                  // positive y
                    face_index = 2;
                    sc = +direction.x;
                    tc = +direction.z;
                }
                else
                {                                // negative y
                    face_index = 3;
                    sc = +direction.x;
                    tc = -direction.z;
                }
            }
            else
            {                                // major axis -> z
                ma = abs_z;
                if (direction.z > 0)
                {                  // positive z
                    face_index = 4;
                    sc = +direction.x;
                    tc = -direction.y;
                }
                else
                {                                // negative z
                    face_index = 5;
                    sc = -direction.x;
                    tc = -direction.y;
                }
            }

            texcoord = new float2((sc / ma + 1) / 2, (tc / ma + 1) / 2);
            return face_index;
        }


        public static float4 TextureRepeatSample(Texture2D texture, float2 texcoord)
        {
            float u = texcoord.x - floor(texcoord.x);
            float v = texcoord.y - floor(texcoord.y);
            return tex2D(texture, float2(u, v));
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
            int face_index = SelectCubemapFace(direction, out uv);
            uv.y = 1 - uv.y;

            switch (face_index)
            {
                case 0:
                    return TextureRepeatSample(texture.right, uv);
                case 1:
                    return TextureRepeatSample(texture.left, uv);
                case 2:
                    return TextureRepeatSample(texture.up, uv);
                case 3:
                    return TextureRepeatSample(texture.down, uv);
                case 4:
                    return TextureRepeatSample(texture.front, uv);
                case 5:
                    return TextureRepeatSample(texture.back, uv);
                default:
                    return 1;
            }
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
