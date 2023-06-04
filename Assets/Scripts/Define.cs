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
}