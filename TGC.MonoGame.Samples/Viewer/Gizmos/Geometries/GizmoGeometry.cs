using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Gizmo that is drawn using non-user indexed line lists.
    /// </summary>
    abstract class GizmoGeometry
    {
        private readonly GraphicsDevice _graphicsDevice;

        private VertexBuffer _vertexBuffer;

        private IndexBuffer _indexBuffer;

        private int _primitiveCount;

        /// <summary>
        ///     Creates a Gizmo Geometry.
        /// </summary>
        /// <param name="device">Graphics Device to bind the geometry to,</param>
        public GizmoGeometry(GraphicsDevice device)
        {
            _graphicsDevice = device;
        }

        /// <summary>
        ///     Initializes the Vertex Buffer using an array of vertices in local space.
        /// </summary>
        /// <param name="positions">An array of vertices in local space.</param>
        protected void InitializeVertices(VertexPosition[] positions)
        {
            _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPosition), positions.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(positions);
        }

        /// <summary>
        ///     Initializes the Index Buffer.
        /// </summary>
        /// <param name="indices">The indices that points to the line vertices.</param>
        protected void InitializeIndices(ushort[] indices)
        {
            _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);

            _primitiveCount = indices.Length / 2;
        }

        /// <summary>
        ///     Binds the geometry to the Graphics Device.
        /// </summary>
        public virtual void Bind()
        {
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;
        }

        /// <summary>
        ///     Draws the geometry. Bind must be called first.
        /// </summary>
        public virtual void Draw()
        {
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, _primitiveCount);
        }

        /// <summary>
        ///     Disposes the created geometry.
        /// </summary>
        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
        }
    }
}
