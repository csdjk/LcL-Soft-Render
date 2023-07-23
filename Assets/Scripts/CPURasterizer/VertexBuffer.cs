using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LcLSoftRenderer
{

    public class VertexBuffer
    {
        private List<Vertex> m_Vertex = new List<Vertex>();
        public VertexBuffer(IEnumerable<Vertex> vertices)
        {
            m_Vertex.AddRange(vertices);
        }
        public void AddVertices(IEnumerable<Vertex> vertices)
        {
            m_Vertex.AddRange(vertices);
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
        public List<VertexData> ToVertexDataList()
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
