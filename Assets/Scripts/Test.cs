using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    private void OnEnable()
    {
        // Debug.Log(transform.position);
        // var matrix = transform.localToWorldMatrix;
        // var position = new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
        // Debug.Log("Transform position from matrix is: " + position);
        // var position2 = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        // Debug.Log("Transform position from matrix is: " + position2);


        var matrix = new Matrix4x4(new Vector4(1, 2, 3, 4),
                                    new Vector4(5, 6, 7, 8),
                                    new Vector4(9, 10, 11, 12),
                                    new Vector4(13, 14, 15, 16));
        Debug.Log(matrix);
        float4x4 matrix3 = (float4x4)matrix;
        float4x4 matrix2 = new float4x4(new Vector4(1, 2, 3, 4),
                                    new Vector4(5, 6, 7, 8),
                                    new Vector4(9, 10, 11, 12),
                                    new Vector4(13, 14, 15, 16));
        Debug.Log(matrix2);

    }
    // Update is called once per frame
    void Update()
    {

    }
}
