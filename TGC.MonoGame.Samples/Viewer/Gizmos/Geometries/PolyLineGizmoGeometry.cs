using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Gizmo for drawing Poly-Lines.
    /// </summary>
    class PolyLineGizmoGeometry
    {
        private GraphicsDevice GraphicsDevice;
        private short[] Indices;

        /// <summary>
        ///     Creates a wire PolyLine.
        /// </summary>
        /// <param name="graphicsDevice">Graphics Device to bind the geometry to.</param>
        public PolyLineGizmoGeometry(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            Indices = new short[1000];
            Indices[0] = 0;
            for (short index = 1; index < 999; index += 2)
            {
                Indices[index] = index;
                Indices[index + 1] = index;
            }
        }

        /// <summary>
        ///     Draws the Poly-Line using a set of points in World space. The continuous line will travel through these.
        /// </summary>
        /// <param name="points">Points in World space.</param>
        public void Draw(Vector3[] points)
        {
            var vertices = points.Select(point => new VertexPosition(point)).ToArray();

            var primitiveCount = vertices.Length - 1;
            var indexCount = primitiveCount * 2;
            var indices = Indices.Take(indexCount).ToArray();

            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, vertices.Length, indices, 0, primitiveCount);
        }
    }
}
