using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Gizmo for drawing Wire Spheres.
    /// </summary>
    class SphereGizmoGeometry : RadialGizmoGeometry
    {
        /// <summary>
        ///     Constructs a Wire Sphere.
        /// </summary>
        /// <param name="graphicsDevice">Graphics Device to bind the geometry to.</param>
        /// <param name="subdivisions">The amount of the subdivisions each sphere ring will have.</param>
        public SphereGizmoGeometry(GraphicsDevice graphicsDevice, int subdivisions) : base(graphicsDevice)
        {
            var positions = GeneratePolygonPositions(subdivisions);
            var originalIndices = GeneratePolygonIndices(subdivisions);

            var subdivisionsTimesTwo = subdivisions * 2;
            var indices = new ushort[subdivisions * 6];

            Array.Copy(originalIndices, 0, indices, 0, subdivisionsTimesTwo);
            Array.Copy(originalIndices.Select(index => (ushort)(index + subdivisions)).ToArray(), 0, indices, subdivisionsTimesTwo, subdivisionsTimesTwo);
            Array.Copy(originalIndices.Select(index => (ushort)(index + subdivisionsTimesTwo)).ToArray(), 0, indices, subdivisionsTimesTwo * 2, subdivisionsTimesTwo);


            var vertices = new VertexPosition[subdivisions * 3];
            positions
                .Select(position => new VertexPosition(new Vector3(position, 0f)))
                .ToArray()
                .CopyTo(vertices, 0);

            Array.Copy(positions
                .Select(position => new VertexPosition(new Vector3(position.Y, 0f, position.X)))
                .ToArray(), 0, vertices, subdivisions, subdivisions);

            Array.Copy(positions
                .Select(position => new VertexPosition(new Vector3(0f, position.Y, position.X)))
                .ToArray(), 0, vertices, subdivisionsTimesTwo, subdivisions);

            InitializeVertices(vertices);
            InitializeIndices(indices);
        }

        /// <summary>
        ///     Calculates the World matrix for the Sphere.
        /// </summary>
        /// <param name="origin">The position in world space.</param>
        /// <param name="size">The scale of the Sphere.</param>
        /// <returns>The calculated World matrix</returns>
        public static Matrix CalculateWorld(Vector3 origin, Vector3 size)
        {
            return Matrix.CreateScale(size) * Matrix.CreateTranslation(origin);
        }
    }
}
