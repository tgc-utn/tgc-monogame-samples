using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TGC.MonoGame.Samples.Models.Drawers;

namespace TGC.MonoGame.Samples.Geometries.Textures
{
    /// <summary>
    ///     The quad is like a plane but its made by two triangle and the surface is oriented in the XY plane of the local
    ///     coordinate space.
    /// </summary>
    public class QuadPrimitive : ModelDrawer
    {
        /// <summary>
        ///     Create a textured quad.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        public QuadPrimitive(GraphicsDevice graphicsDevice) : base()
        {
            var vertexBuffer = CreateVertexBuffer(graphicsDevice);
            var indexBuffer = CreateIndexBuffer(graphicsDevice);

            GeometryDrawers.Add(new GeometryDrawer(vertexBuffer, indexBuffer, graphicsDevice));
        }

        /// <summary>
        ///     Create a vertex buffer for the figure with the given information.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        private VertexBuffer CreateVertexBuffer(GraphicsDevice graphicsDevice)
        {
            // Set the position and texture coordinate for each vertex
            // Normals point Up as the Quad is originally XZ aligned

            var textureCoordinateLowerLeft = Vector2.Zero;
            var textureCoordinateLowerRight = Vector2.UnitX;
            var textureCoordinateUpperLeft = Vector2.UnitY;
            var textureCoordinateUpperRight = Vector2.One;

            var vertices = new[]
            {
                // Possitive X, Possitive Z
                new VertexPositionNormalTexture(Vector3.UnitX + Vector3.UnitZ, Vector3.Up, textureCoordinateUpperRight),
                // Possitive X, Negative Z
                new VertexPositionNormalTexture(Vector3.UnitX - Vector3.UnitZ, Vector3.Up, textureCoordinateLowerRight),
                // Negative X, Possitive Z
                new VertexPositionNormalTexture(Vector3.UnitZ - Vector3.UnitX, Vector3.Up, textureCoordinateUpperLeft),
                // Negative X, Negative Z
                new VertexPositionNormalTexture(-Vector3.UnitX - Vector3.UnitZ, Vector3.Up, textureCoordinateLowerLeft)
            };

            var vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
                BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            return vertexBuffer;
        }

        private IndexBuffer CreateIndexBuffer(GraphicsDevice graphicsDevice)
        {
            // Set the index buffer for each vertex, using clockwise winding
            var indices = new ushort[]
            {
                3, 1, 0, 
                3, 0, 2,
            };

            var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length,
                BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);

            return indexBuffer;
        }

    }
}