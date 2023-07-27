using Unity.Mathematics;
namespace LcLSoftRenderer
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        private float3 position;
        private float2 uv;
        private float3 normal;
        private float4 tangent;
        private float4 color;

        public VertexData(float3 position, float2 uv, float3 normal, float4 tangent, float4 color)
        {
            this.position = position;
            this.uv = uv;
            this.normal = normal;
            this.tangent = tangent;
            this.color = color;
        }
        public static int size => Marshal.SizeOf<VertexData>();

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexOutputData
    {
        public float4 positionCS;
        public float4 positionOS;
        public float4 normalWS;
        public float4 tangent;
        public float2 uv;
        public float4 color;
        public float3 viewDir;
        public static int size => Marshal.SizeOf<VertexOutputData>();
    }
}