using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public class IndexBuffer
{
    ComputeBuffer m_ComputeBuffer;
    public ComputeBuffer computeBuffer => m_ComputeBuffer;
    List<int> m_Index = new List<int>();

    List<int3> m_Triangles = new List<int3>();
    public IndexBuffer(IEnumerable<int> indices)
    {
        m_Index.AddRange(indices);

        for (int i = 0; i < m_Index.Count; i += 3)
        {
            m_Triangles.Add(int3(m_Index[i], m_Index[i + 1], m_Index[i + 2]));
        }
        m_ComputeBuffer = new ComputeBuffer(m_Triangles.Count, sizeof(int) * 3);
        m_ComputeBuffer.SetData(m_Triangles);
    }
    public void AddIndices(IEnumerable<int> indices)
    {
        m_Index.AddRange(indices);
    }

    public int this[int i]
    {
        get { return m_Index[i]; }
    }

    public int Count()
    {
        return m_Index.Count;
    }
}
