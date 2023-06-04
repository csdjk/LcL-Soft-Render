using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log(transform.position);
        var matrix = transform.localToWorldMatrix;
        var position = new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
        Debug.Log("Transform position from matrix is: " + position);
        var position2 = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        Debug.Log("Transform position from matrix is: " + position2);

    }
    // Update is called once per frame
    void Update()
    {

    }
}
