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
        // Debug.Log(transform.position);
        // var matrix = transform.localToWorldMatrix;
        // var position = new float3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
        // Debug.Log("Transform position from matrix is: " + position);
        // var position2 = new float3(matrix.m03, matrix.m13, matrix.m23);
        // Debug.Log("Transform position from matrix is: " + position2);
        // Matrix4x4

        // var matrix = new float4x4(new float4(1, 2, 3, 4),
        //                             new float4(5, 6, 7, 8),
        //                             new float4(9, 10, 11, 12),
        //                             new float4(13, 14, 15, 16));
        // Debug.Log(matrix);
        // float4x4 matrix3 = (float4x4)matrix;
        // float4x4 matrix2 = new float4x4(new float4(1, 2, 3, 4),
        //                             new float4(5, 6, 7, 8),
        //                             new float4(9, 10, 11, 12),
        //                             new float4(13, 14, 15, 16));
        // Debug.Log(matrix2);


        // var matrix = new float4x4(new float4(1, 0, 0, 1),
        //                            new float4(0, 1, 0, 2),
        //                            new float4(0, 0, 1, 3),
        //                            new float4(0, 0, 15, 1));

        // var newPoint = matrix * new float4(0, 0, 0, 1);
        // Debug.Log(newPoint);

        float4x4 matrix2 = new float4x4(new float4(1, 0, 0, 1),
                                        new float4(0, 1, 0, 2),
                                        new float4(0, 0, 1, 3),
                                        new float4(0, 0, 15, 1));
        // matrix2 = transpose(matrix2);
        var newPoint2 = mul(matrix2, new float4(0, 0, 0, 1));
        Debug.Log(newPoint2);


        RenderQueue renderQueue = RenderQueue.Geometry;
        renderQueue = (RenderQueue)3001;
        Debug.Log(renderQueue);
    }


    
    // Update is called once per frame
    void Update()
    {

    }
}
