using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Gizmo for drawing Wire Cylinders.
    /// </summary>
    class CylinderGizmoGeometry : RadialGizmoGeometry
    {
        /// <summary>
        ///     Creates a Wire Cylinder with a number of subdivisions for the top and bottom.
        /// </summary>
        /// <param name="device">The device to bind geometry to.</param>
        /// <param name="subdivisions">The number of subdivisions to </param>
        public CylinderGizmoGeometry(GraphicsDevice device, int subdivisions) : base(device)
        {
            var positions = GeneratePolygonPositions(subdivisions);
            var originalIndices = GeneratePolygonIndices(subdivisions);

            var subdivisionsTimesTwo = subdivisions * 2;
            
            // Lines for each circle, and four lines joining them
            var indices = new ushort[subdivisions * 6 + 8];

            Array.Copy(originalIndices, 0, indices, 0, subdivisionsTimesTwo);
            Array.Copy(originalIndices.Select(index => (ushort)(index + subdivisions)).ToArray(), 0, indices, subdivisionsTimesTwo, subdivisionsTimesTwo);
            Array.Copy(originalIndices.Select(index => (ushort)(index + subdivisionsTimesTwo)).ToArray(), 0, indices, subdivisionsTimesTwo * 2, subdivisionsTimesTwo);

            var index = subdivisions * 6;
            var firstQuadrant = (ushort)(subdivisions / 4);
            var firstQuadrantAdded = (ushort)(firstQuadrant + subdivisions);
            var secondQuadrant = (ushort)(subdivisions / 2);
            var secondQuadrantAdded = (ushort)(secondQuadrant + subdivisions);
            var thirdQuadrant = (ushort)(firstQuadrant + secondQuadrant);
            var thirdQuadrantAdded = (ushort)(thirdQuadrant + subdivisions);

            // Joining lines
            indices[index] = 0;
            index++;
            indices[index] = (ushort)subdivisions;
            index++;
            indices[index] = firstQuadrant;
            index++;
            indices[index] = firstQuadrantAdded;
            index++;
            indices[index] = secondQuadrant;
            index++;
            indices[index] = secondQuadrantAdded;
            index++;
            indices[index] = thirdQuadrant;
            index++;
            indices[index] = thirdQuadrantAdded;
            index++;

            var vertices = new VertexPosition[subdivisions * 3];
            positions
                .Select(position => new VertexPosition(new Vector3(position.X, 1f, position.Y)))
                .ToArray()
                .CopyTo(vertices, 0);

            Array.Copy(positions
                .Select(position => new VertexPosition(new Vector3(position.X, -1f, position.Y)))
                .ToArray(), 0, vertices, subdivisions, subdivisions);

            Array.Copy(positions
                .Select(position => new VertexPosition(new Vector3(position.X, 0f, position.Y)))
                .ToArray(), 0, vertices, subdivisionsTimesTwo, subdivisions);

            InitializeVertices(vertices);
            InitializeIndices(indices);
        }

        /// <summary>
        ///     Calculates the World matrix for the Cylinder. Note that is initially XZ oriented.
        /// </summary>
        /// <param name="origin">The position in world space.</param>
        /// <param name="rotation">The rotation of the Cylinder.</param>
        /// <param name="scale">The scale of the Cylinder.</param>
        /// <returns>The calculated World matrix</returns>
        public static Matrix CalculateWorld(Vector3 origin, Matrix rotation, Vector3 scale)
        {
            return Matrix.CreateScale(scale) * rotation * Matrix.CreateTranslation(origin);
        }
    }
}
