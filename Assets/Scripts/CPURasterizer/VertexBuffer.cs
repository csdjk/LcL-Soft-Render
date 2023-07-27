using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LcLSoftRenderer
{

    public class VertexBuffer
    {
        ComputeBuffer m_ComputeBuffer;
        public ComputeBuffer computeBuffer => m_ComputeBuffer;
        List<Vertex> m_Vertex = new List<Vertex>();
        List<VertexData> m_VertexData = new List<VertexData>();

        public VertexBuffer(IEnumerable<Vertex> vertices)
        {
            AddVertices(vertices);
            m_ComputeBuffer = new ComputeBuffer(m_VertexData.Count, VertexData.size);
            m_ComputeBuffer.SetData(m_VertexData);

        }
        public void AddVertices(IEnumerable<Vertex> vertices)
        {
            m_Vertex.AddRange(vertices);
            foreach (var vertex in vertices)
            {
                m_VertexData.Add(vertex);
            }
        }

        public int Length => m_Vertex.Count;
        public Vertex this[int i]
        {
            get { return m_Vertex[i]; }
        }


        public int Count()
        {
            return m_Vertex.Count;
        }


        public void Clear()
        {
            m_Vertex.Clear();
        }

        // convert to VertexData List
        public List<VertexData> GetVertexDataList()
        {
            List<VertexData> vertexDataList = new List<VertexData>();
            foreach (var vertex in m_Vertex)
            {
                vertexDataList.Add(vertex);
            }
            return vertexDataList;
        }


    }
}
