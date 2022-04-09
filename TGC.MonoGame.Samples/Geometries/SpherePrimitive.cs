#region File Description

//-----------------------------------------------------------------------------
// SpherePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion File Description

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Models;
using TGC.MonoGame.Samples.Models.Drawers;

#endregion Using Statements

namespace TGC.MonoGame.Samples.Geometries
{
    /// <summary>
    ///     Geometric primitive class for drawing spheres.
    /// </summary>
    public class SpherePrimitive : ModelDrawer
    {
        /// <summary>
        ///     Constructs a new sphere primitive, with the specified size, tessellation level and white color.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="diameter">Diameter of the sphere.</param>
        /// <param name="tessellation">The number of times the surface triangles are subdivided.</param>
        public SpherePrimitive(GraphicsDevice graphicsDevice, float diameter = 1, int tessellation = 16) : this(
            graphicsDevice, diameter, tessellation, Color.White)
        {
        }

        /// <summary>
        ///     Constructs a new sphere primitive, with the specified size, tessellation level and color.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="diameter">Diameter of the sphere.</param>
        /// <param name="tessellation">The number of times the surface triangles are subdivided.</param>
        /// <param name="color">Color of the sphere.</param>
        public SpherePrimitive(GraphicsDevice graphicsDevice, float diameter, int tessellation, Color color)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            var verticalSegments = tessellation;
            var horizontalSegments = tessellation * 2;

            var radius = diameter / 2;

            var builder = new GeometryBuilder<VertexPositionNormalColorTexture>();

            // Start with a single vertex at the bottom of the sphere.
            builder.AddVertex(new VertexPositionNormalColorTexture(Vector3.Down * radius, Vector3.Down, color, Vector2.Zero));

            // Create rings of vertices at progressively higher latitudes.
            for (var i = 0; i < verticalSegments - 1; i++)
            {
                var latitude = (i + 1) * MathHelper.Pi /
                    verticalSegments - MathHelper.PiOver2;

                var dy = (float) Math.Sin(latitude);
                var dxz = (float) Math.Cos(latitude);


                var yCoordinate = i / (verticalSegments - 1f);

                // Create a single ring of vertices at this latitude.
                for (var j = 0; j < horizontalSegments; j++)
                {
                    var longitude = j * MathHelper.TwoPi / horizontalSegments;

                    var dx = (float) Math.Cos(longitude) * dxz;
                    var dz = (float) Math.Sin(longitude) * dxz;

                    var normal = new Vector3(dx, dy, dz);

                    var xCoordinate = j / horizontalSegments;

                    builder.AddVertex(new VertexPositionNormalColorTexture(normal * radius, normal, color, new Vector2(xCoordinate, yCoordinate)));
                }
            }

            // Finish with a single vertex at the top of the sphere.
            builder.AddVertex(new VertexPositionNormalColorTexture(Vector3.Up * radius, Vector3.Up, color, Vector2.UnitY));

            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (var i = 0; i < horizontalSegments; i++)
            {
                builder.AddIndex(0);
                builder.AddIndex(1 + (i + 1) % horizontalSegments);
                builder.AddIndex(1 + i);
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (var i = 0; i < verticalSegments - 2; i++)
                for (var j = 0; j < horizontalSegments; j++)
                {
                    var nextI = i + 1;
                    var nextJ = (j + 1) % horizontalSegments;

                    builder.AddIndex(1 + i * horizontalSegments + j);
                    builder.AddIndex(1 + i * horizontalSegments + nextJ);
                    builder.AddIndex(1 + nextI * horizontalSegments + j);

                    builder.AddIndex(1 + i * horizontalSegments + nextJ);
                    builder.AddIndex(1 + nextI * horizontalSegments + nextJ);
                    builder.AddIndex(1 + nextI * horizontalSegments + j);
                }

            // Create a fan connecting the top vertex to the top latitude ring.
            for (var i = 0; i < horizontalSegments; i++)
            {
                builder.AddCurrentIndex(-1);
                builder.AddCurrentIndex(- 2 - (i + 1) % horizontalSegments);
                builder.AddCurrentIndex(- 2 - i);
            }

            GeometryDrawers.Add(builder.Build(graphicsDevice));
        }
    }
}