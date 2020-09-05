#region File Description

//-----------------------------------------------------------------------------
// VertexPositionNormal.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion File Description

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion Using Statements

namespace TGC.MonoGame.Samples.Geometries
{
    /// <summary>
    ///     Custom vertex type for vertices that have just a position a color and a normal,
    ///     without any texture coordinates.
    /// </summary>
    public struct VertexPositionColorNormal : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        /// <summary>
        ///     Initializes a new instance of the VertexPositionColorNormal class.
        /// </summary>
        /// <param name="position">The vertex position.</param>
        /// <param name="color">The vertex color.</param>
        /// <param name="normal">The vertex normal.</param>
        public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal)
        {
            Position = position;
            Color = color;
            Normal = normal;
        }

        /// <summary>
        ///     A VertexDeclaration object, which contains information about the vertex elements contained within this struct.
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}