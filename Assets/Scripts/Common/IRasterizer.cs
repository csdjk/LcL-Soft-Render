
using System.Collections.Generic;
using UnityEngine;

namespace LcLSoftRender
{
    public interface IRasterizer
    {
        // void Setup();
        public abstract void Render(List<RenderObject> renderObjects);
        public virtual void SetDebugIndex(int debugIndex) { }
        Texture ColorTexture { get; }
    }

}
