namespace LcLSoftRenderer
{

    public enum RasterizerType
    {
        CPU,
        GPUDriven
    }
    public enum ClearMask
    {
        COLOR = 1 << 0,
        DEPTH = 1 << 1
    }
    public enum PrimitiveType
    {
        Triangle,
        Line
    }
    public enum MSAAMode
    {
        None = 1,
        MSAA2x = 2,
        MSAA4x = 4,
        MSAA8x = 8
    }
    // queue
    public enum RenderQueue
    {
        Background = 1000,
        Geometry = 2000,
        AlphaTest = 2450,
        Transparent = 3000,
        Overlay = 4000
    }

    public enum ZTest
    {
        Off,
        Never,
        Less,
        Equal,
        LessEqual,
        Greater,
        NotEqual,
        GreaterEqual,
        Always
    }

    public enum ZWrite
    {
        Off,
        On
    }

    public enum CullMode
    {
        None,
        Front,
        Back
    }

    public enum BlendMode
    {
        None,                   // 不进行混合
        AlphaBlend,             // 标准的透明混合，使用源颜色的alpha值来控制源颜色和目标颜色的混合比例
        Additive,               // 加法混合，将源颜色和目标颜色相加
        Subtractive,            // 减法混合，将源颜色和目标颜色相减
        PremultipliedAlpha,     // 预乘alpha混合，先将源颜色的RGB值乘以alpha值，再进行标准的透明混合
        Multiply,               // 乘法混合，将源颜色和目标颜色相乘
        Screen,                 // 屏幕混合，将源颜色和目标颜色的补色相乘，再取补色
        Overlay,                // 叠加混合，根据源颜色的亮度值来决定是乘以目标颜色还是相加目标颜色
        Darken,                 // 取暗混合，将源颜色和目标颜色中较暗的那个作为混合结果
        Lighten,                // 取亮混合，将源颜色和目标颜色中较亮的那个作为混合结果
        ColorDodge,             // 颜色减淡混合，将源颜色和目标颜色相除
        ColorBurn,              // 颜色加深混合，将源颜色的补色和目标颜色的补色相除，再取补色
        SoftLight,              // 柔光混合，根据源颜色的亮度值来决定是调暗还是调亮目标颜色，类似于叠加混合
        HardLight,              // 强光混合，根据源颜色的亮度值来决定是调暗还是调亮目标颜色，类似于叠加混合
        Difference,             // 差值混合，将源颜色和目标颜色相减，再取绝对值
        Exclusion,              // 排除混合，将源颜色和目标颜色相加，再减去两者的乘积
        HSLHue,                 // 色相混合
        HSLSaturation,          // 饱和度混合
        HSLColor,               // 颜色混合
        HSLLuminosity,          // 亮度混合
    }


    public enum StencilOp
    {
        Keep,
        Zero,
        Replace,
        IncrSat,
        DecrSat,
        Invert,
        IncrWrap,
        DecrWrap
    }

    public enum CompareFunction
    {
        Disabled,
        Never,
        Less,
        Equal,
        LessEqual,
        Greater,
        NotEqual,
        GreaterEqual,
        Always
    }
}