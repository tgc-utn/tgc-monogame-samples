using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Gizmo for drawing lines.
    /// </summary>
    class LineSegmentGizmoGeometry : GizmoGeometry
    {
        /// <summary>
        ///     Creates a wire Line Segment.
        /// </summary>
        /// <param name="graphicsDevice">Graphics Device to bind the geometry to.</param>
        public LineSegmentGizmoGeometry(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            var vertices = new VertexPosition[2]
            {
                new VertexPosition(Vector3.Zero),
                new VertexPosition(Vector3.One),
            };
            var indices = new ushort[2] { 0, 1 };
            InitializeVertices(vertices);
            InitializeIndices(indices);
        }

        /// <summary>
        ///     Calculates the World matrix for the Line.
        /// </summary>
        /// <param name="origin">The origin point of the Line in World space.</param>
        /// <param name="destination">The destination point of the Line in World space.</param>
        /// <returns>The calculated World matrix</returns>
        public static Matrix CalculateWorld(Vector3 origin, Vector3 destination)
        {
            var scale = destination - origin;
            return Matrix.CreateScale(scale) * Matrix.CreateTranslation(origin);
        }


    }
}
