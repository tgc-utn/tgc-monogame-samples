#region File Description

//-----------------------------------------------------------------------------
// CubePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion File Description

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TGC.MonoGame.Samples.Models;
using TGC.MonoGame.Samples.Models.Drawers;

#endregion Using Statements

namespace TGC.MonoGame.Samples.Geometries
{
    /// <summary>
    ///     Geometric primitive class for drawing cubes.
    /// </summary>
    public class CubePrimitive : ModelDrawer
    {


        public CubePrimitive(GraphicsDevice graphicsDevice) : this(graphicsDevice, 1f, Color.White)
        {
        }

        public CubePrimitive(GraphicsDevice graphicsDevice, float size, Color color) : this(graphicsDevice, size, 
            color, color, color, color, color, color, color, color)
        {

        }

        /// <summary>
        ///     Constructs a new cube primitive, with the specified size.
        /// </summary>
        public CubePrimitive(GraphicsDevice graphicsDevice, float size, Color color1, Color color2, Color color3,
            Color color4, Color color5, Color color6, Color color7, Color color8) : base()
        {
            var builder = new GeometryBuilder<VertexPositionNormalColorTexture>();

            var vertices = new VertexPositionNormalColorTexture[24]
            {
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 0, 1), color1, new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0, 0, 1), color2, new Vector2(0, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0, 0, 1), color3, new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0, 0, 1), color4, new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), color5, new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0, -1, 0), color4, new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0, -1, 0), color3, new Vector2(0, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), color6, new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1, 0, 0), color6, new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-1, 0, 0), color3, new Vector2(0, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-1, 0, 0), color2, new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-1, 0, 0), color7, new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0, 0, -1), color7, new Vector2(0, 0)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0, 0, -1), color8, new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), color5, new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), color6, new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(1, 0, 0), color8, new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 0, 0), color1, new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f, 0.5f), new Vector3(1, 0, 0), color4, new Vector2(0, 0)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(1, 0, 0), color5, new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0, 1, 0), color7, new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0, 1, 0), color2, new Vector2(0, 0)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0, 1, 0), color1, new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0, 1, 0), color8, new Vector2(1, 1)),
            };
         
            var indices = new ushort[36]
            {
                3, 1, 0,
                3, 2, 1,
                7, 5, 4,
                7, 6, 5,
                11, 9, 8,
                11, 10, 9,
                15, 13, 12,
                15, 14, 13,
                19, 17, 16,
                19, 18, 17,
                23, 21, 20,
                23, 22, 21,
            };

            for (var index = 0; index < vertices.Length; index++)
            {
                vertices[index].Position *= size;
                builder.AddVertex(vertices[index]);
            }

            for (var index = 0; index < indices.Length; index++)
                builder.AddIndex(indices[index]);

            GeometryDrawers.Add(builder.Build(graphicsDevice));
        }
    }
}