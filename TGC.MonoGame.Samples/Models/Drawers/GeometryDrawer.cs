using Microsoft.Xna.Framework.Graphics;
using System;


namespace TGC.MonoGame.Samples.Models.Drawers
{
    public class GeometryDrawer : IDisposable
    {

        public VertexBuffer VertexBuffer { get; protected set; }

        public IndexBuffer IndexBuffer { get; protected set; }

        protected int _vertexOffset;

        protected int _primitiveCount;

        protected int _startIndex;

        protected GraphicsDevice _device;

        protected GeometryDrawer(GraphicsDevice device)
        {
            _device = device;
        }

        public GeometryDrawer(ModelMeshPart part, GraphicsDevice device)
        {
            VertexBuffer = part.VertexBuffer;
            IndexBuffer = part.IndexBuffer;
            _vertexOffset = part.VertexOffset;
            _primitiveCount = part.PrimitiveCount;
            _startIndex = part.StartIndex;
            _device = device;
        }

        public GeometryDrawer(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, GraphicsDevice device)
        {
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
            _primitiveCount = IndexBuffer.IndexCount / 3;
            _startIndex = 0;
            _device = device;
        }

        public void Draw(EffectPass[] effectPasses)
        {
            _device.Indices = IndexBuffer;
            _device.SetVertexBuffer(VertexBuffer);
            for (var index = 0; index < effectPasses.Length; index++)
            {
                effectPasses[index].Apply();
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, _vertexOffset, _startIndex, _primitiveCount);
            }
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }

    }
}
