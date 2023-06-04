
using UnityEngine;

namespace LcLSoftRender
{
    public interface IRasterizer
    {
        // void Setup();
        void Render();
        Texture ColorTexture { get; }
    }

}
