using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LcLSoftRender
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

        public Vertex this[int i]
        {
            get { return m_Vertex[i]; }
        }

        public int Count()
        {
            return m_Vertex.Count;
        }
    }
}
