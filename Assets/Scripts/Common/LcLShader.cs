using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LcLShader
{
    public Matrix4x4 modelMatrix { get; set; }
    public Vector3 normal { get; set; }
    public Vector3 tangent { get; set; }
    public Vector2 uv { get; set; }
    public Color vertexColor { get; set; }

    public abstract Color FragmentShade();
}
