using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexBuffer
{
    private List<int> m_Index = new List<int>();

    public IndexBuffer(IEnumerable<int> indices)
    {
        m_Index.AddRange(indices);
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
