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

        public static Vector3 ColorToVector3(Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }

        public static Vector3 UnpackNormal(Vector3 normal)
        {
            return new Vector3(normal.x * 2 - 1, normal.y * 2 - 1, normal.z * 2 - 1);
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
