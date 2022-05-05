using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Gizmo for drawing Wire Disks.
    /// </summary>
    class DiskGizmoGeometry : RadialGizmoGeometry
    {
        /// <summary>
        ///     Creates a Wire Disk geometry.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to bind the geometry.</param>
        /// <param name="subdivisions">The amount of subdivisions the wire disk will take when forming a circle.</param>
        public DiskGizmoGeometry(GraphicsDevice graphicsDevice, int subdivisions) : base(graphicsDevice)
        {
            var positions = GeneratePolygonPositions(subdivisions);
            var originalIndices = GeneratePolygonIndices(subdivisions);

            var subdivisionsTimesTwo = subdivisions * 2;

            var indices = new ushort[subdivisionsTimesTwo];
            var vertices = new VertexPosition[subdivisions];

            Array.Copy(originalIndices, 0, indices, 0, subdivisionsTimesTwo);

            Array.Copy(positions
                .Select(position => new VertexPosition(new Vector3(position.Y, position.X, 0f)))
                .ToArray(), 0, vertices, 0, subdivisions);
            
            InitializeVertices(vertices);
            InitializeIndices(indices);
        }


        /// <summary>
        ///     Calculates the World matrix for the Disk. Note that is initially XZ oriented.
        /// </summary>
        /// <param name="origin">The position in world space.</param>
        /// <param name="normal">The normal of the Disk. The circle will face this vector.</param>
        /// <param name="scale">The radius of the Disk.</param>
        /// <returns>The calculated World matrix</returns>
        public static Matrix CalculateWorld(Vector3 origin, Vector3 normal, float radius)
        {
            Matrix rotationAndTranslation;
            // Check if +Z or -Z. In that case, no need to rotate 
            // (also the view matrix is broken in those cases)
            var distanceToZAxis = -MathF.Abs(normal.Z) + 1.0f;
            if (distanceToZAxis < float.Epsilon)
            {
                rotationAndTranslation = Matrix.CreateTranslation(origin);
            }
            else
            {
                rotationAndTranslation = Matrix.Invert(Matrix.CreateLookAt(origin, origin + normal, Vector3.Backward));
            }

            return Matrix.CreateScale(radius) * rotationAndTranslation;
        }
    }
}
