using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.Gizmos.Geometries
{
    /// <summary>
    ///     Abstract class for any geometry that contains a circle.
    /// </summary>
    internal abstract class RadialGizmoGeometry : GizmoGeometry
    {
        /// <summary>
        ///     Creates a Radial Geometry, which contains a radial wire shape.
        /// </summary>
        /// <param name="device">Graphics Device to bind the geometry to.</param>
        protected RadialGizmoGeometry(GraphicsDevice device) : base(device)
        {

        }

        /// <summary>
        ///     Generates a set of positions in a radial circle, given a number of subdivisions.
        /// </summary>
        /// <param name="subdivisions">The number of subdivisions that the circle will contain.</param>
        /// <returns>An array of positions in two dimensions describing a circle with the given subdivisions.</returns>
        protected Vector2[] GeneratePolygonPositions(int subdivisions)
        {
            var positions = new Vector2[subdivisions];

            var offset = 0f;

            // Odd? Then start at 90 degrees
            if (subdivisions % 2 == 1)
                offset = MathHelper.PiOver2;

            var increment = MathHelper.TwoPi / subdivisions;
            for (ushort index = 0; index < subdivisions; index++)
            {
                positions[index] = new Vector2(MathF.Cos(offset), MathF.Sin(offset));
                offset += increment;
            }

            return positions;
        }

        /// <summary>
        ///     Generates the indices that relates the positions of a circle, creating the lines between them.
        /// </summary>
        /// <param name="subdivisions">The number of subdivisions that the circle contains.</param>
        /// <returns>An array of indices, in pairs, joining points to form lines of a circle.</returns>
        protected ushort[] GeneratePolygonIndices(int subdivisions)
        {
            var subdivisionsTimesTwo = subdivisions * 2;
            var indices = new ushort[subdivisionsTimesTwo];

            for (ushort index = 0; index < subdivisions; index++)
            {
                indices[index * 2] = index;
                indices[index * 2 + 1] = (ushort)(index + 1);
            }

            // Override the last index, close the loop
            indices[subdivisionsTimesTwo - 1] = 0;
            return indices;
        }
    }
}
