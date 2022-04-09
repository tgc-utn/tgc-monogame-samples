using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.Samples.Models.Drawers;

namespace TGC.MonoGame.Samples.Models
{
    public class GeometryBuilder<T> where T : struct
    {
        private List<T> _vertices;
        private List<ushort> _indices;

        public GeometryBuilder()
        {
            _vertices = new List<T>();
            _indices = new List<ushort>();
        }

        public void AddVertex(T vertex)
        {
            _vertices.Add(vertex);
        }

        public void AddIndex(ushort index)
        {
            _indices.Add(index);
        }
        public void AddIndex(int index)
        {
            _indices.Add((ushort)index);
        }
        public void AddCurrentIndex(ushort offset)
        {
            AddIndex((ushort)(offset + _vertices.Count));
        }
        public void AddCurrentIndex(int offset)
        {
            AddIndex((ushort)(offset + _vertices.Count));
        }


        public void AddTriangle(ushort indexA, ushort indexB, ushort indexC)
        {
            _indices.Add(indexA);
            _indices.Add(indexB);
            _indices.Add(indexC);
        }

        public GeometryDrawer Build(GraphicsDevice device)
        {
            // Create a vertex buffer, and copy our vertex data into it.
            var vertexBuffer = new VertexBuffer(device, typeof(T), _vertices.Count, BufferUsage.None);
            vertexBuffer.SetData(_vertices.ToArray());

            // Create an index buffer, and copy our index data into it.
            var indexBuffer = new IndexBuffer(device, typeof(ushort), _indices.Count, BufferUsage.None);
            indexBuffer.SetData(_indices.ToArray());

            return new GeometryDrawer(vertexBuffer, indexBuffer, device);
        }
    }
}
