using Unity.Mathematics;
namespace LcLSoftRenderer
{
    using System.Runtime.InteropServices;

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
}