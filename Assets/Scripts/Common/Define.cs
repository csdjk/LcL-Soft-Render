namespace LcLSoftRender
{

    public enum RasterizerType
    {
        CPU,
        CPUJobs,
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

    // ZWrite
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
        None,
        AlphaBlend,
        Additive,
        PremultipliedAlpha,
        Multiply,
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