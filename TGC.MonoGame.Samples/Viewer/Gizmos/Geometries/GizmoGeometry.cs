using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Gizmo that is drawn using non-user indexed line lists.
    /// </summary>
    abstract class GizmoGeometry
    {
        protected GraphicsDevice GraphicsDevice;

        protected VertexBuffer VertexBuffer;
        
        protected IndexBuffer IndexBuffer;

        private int PrimitiveCount;

        /// <summary>
        ///     Creates a Gizmo Geometry.
        /// </summary>
        /// <param name="device">Graphics Device to bind the geometry to,</param>
        public GizmoGeometry(GraphicsDevice device)
        {
            GraphicsDevice = device;
        }

        /// <summary>
        ///     Initializes the Vertex Buffer using an array of vertices in local space.
        /// </summary>
        /// <param name="positions">An array of vertices in local space.</param>
        protected void InitializeVertices(VertexPosition[] positions)
        {
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPosition), positions.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(positions);
        }

        /// <summary>
        ///     Initializes the Index Buffer.
        /// </summary>
        /// <param name="indices">The indices that points to the line vertices.</param>
        protected void InitializeIndices(ushort[] indices)
        {
            IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);

            PrimitiveCount = indices.Length / 2;
        }

        /// <summary>
        ///     Binds the geometry to the Graphics Device.
        /// </summary>
        public virtual void Bind()
        {
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;
        }

        /// <summary>
        ///     Draws the geometry. Bind must be called first.
        /// </summary>
        public virtual void Draw()
        {
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, PrimitiveCount);
        }

        /// <summary>
        ///     Disposes the created geometry.
        /// </summary>
        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
