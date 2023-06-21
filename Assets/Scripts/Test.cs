using System.Collections;
using System.Collections.Generic;
using LcLSoftRender;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    private void OnEnable()
    {
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        // GeometryUtility.CalculateFrustumPlanes(m_Camera);
    }

    void Update()
    {

    }
}
