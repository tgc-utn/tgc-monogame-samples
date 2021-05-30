﻿using System;
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
                .Select(position => new VertexPosition(new Vector3(position.Y, 0f, position.X)))
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
            var rotation = Matrix.Identity;
            if (!normal.Equals(Vector3.Up))
            {
                // Rotate our disk!
                // Pretty sure this can be optimized
                var axis = Vector3.Cross(Vector3.Up, normal);
                float amplitude = Vector3.Dot(Vector3.Up, normal);
                rotation = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(axis, amplitude));
            }

            return Matrix.CreateScale(radius) * rotation * Matrix.CreateTranslation(origin);
        }
    }
}
