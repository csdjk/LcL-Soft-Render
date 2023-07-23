
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace LcLSoftRenderer
{
    public interface IRasterizer
    {
        public abstract Texture ColorTexture { get; }
        public MSAAMode MSAAMode { get; set; }
        public float4x4 MatrixVP { get; }

        public abstract void Render(List<RenderObject> renderObjects);
        public virtual void SetDebugIndex(int debugIndex) { }
        public void SetPrimitiveType(PrimitiveType primitiveType);
        public void SetMatrixVP(float4x4 matrixVP);
        public abstract void Clear(CameraClearFlags clearFlags, Color? clearColor = null, float depth = float.PositiveInfinity);
    }

}
