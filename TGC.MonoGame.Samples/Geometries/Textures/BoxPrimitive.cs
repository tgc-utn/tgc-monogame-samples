using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Geometries.Textures
{
    /// <summary>
    ///     Textured 3D box or cube.
    /// </summary>
    public class BoxPrimitive
    {
        private const int NumberOfIndices = 36;

        /// <summary>
        ///     Create a box with a center at the given point, with a size and a color in each vertex.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="size">Size of the box.</param>
        /// <param name="texture">The box texture.</param>
        public BoxPrimitive(GraphicsDevice graphicsDevice, Vector3 size, Texture2D texture)
        {
            Effect = new BasicEffect(graphicsDevice);
            Effect.TextureEnabled = true;
            Effect.Texture = texture;
            Effect.EnableDefaultLighting();

            CreateVertexBuffer(graphicsDevice, size);
            CreateIndexBuffer(graphicsDevice);
        }

        /// <summary>
        ///     Represents a list of 3D vertices to be streamed to the graphics device.
        /// </summary>
        private VertexBuffer Vertices { get; set; }

        /// <summary>
        ///     Describes the rendering order of the vertices in a vertex buffer.
        /// </summary>
        private IndexBuffer Indices { get; set; }

        /// <summary>
        ///     Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
        /// </summary>
        private BasicEffect Effect { get; }

        /// <summary>
        ///     Create a vertex buffer for the figure with the given information.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="size">Size of the box.</param>
        private void CreateVertexBuffer(GraphicsDevice graphicsDevice, Vector3 size)
        {
            var x = size.X / 2;
            var y = size.Y / 2;
            var z = size.Z / 2;

            var vectors = new[]
            {
                // Top-Left Front.
                new Vector3(-x, y, -z),
                // Top-Right Front.
                new Vector3(x, y, -z),
                // Top-Left Back.
                new Vector3(-x, y, z),
                // Top-Right Back.
                new Vector3(x, y, z),
                // Bottom-Left Front.
                new Vector3(-x, -y, -z),
                // Bottom-Right Front.
                new Vector3(x, -y, -z),
                // Bottom-Left Back.
                new Vector3(-x, -y, z),
                // Bottom-Right Back.
                new Vector3(x, -y, z)
            };

            // A box has six faces, each one pointing in a different direction.
            var normals = new[]
            {
                // Top.
                Vector3.UnitY,
                // Bottom.
                -Vector3.UnitY,
                // Front.
                Vector3.UnitZ,
                // Back.
                -Vector3.UnitZ,
                // Left.
                -Vector3.UnitX,
                // Right.
                Vector3.UnitX
            };

            var textureCoordinates = new[]
            {
                // Top-Left.
                Vector2.Zero,
                // Top-Right.
                Vector2.UnitX,
                // Bottom-Left.
                Vector2.UnitY,
                // Bottom-Right.
                Vector2.One
            };

            var vertices = new[]
            {
                // Top Face.
                new VertexPositionNormalTexture(vectors[0], normals[0], textureCoordinates[2]),
                new VertexPositionNormalTexture(vectors[1], normals[0], textureCoordinates[3]),
                new VertexPositionNormalTexture(vectors[2], normals[0], textureCoordinates[0]),
                new VertexPositionNormalTexture(vectors[3], normals[0], textureCoordinates[1]),
                // Bottom Face.
                new VertexPositionNormalTexture(vectors[4], normals[1], textureCoordinates[2]),
                new VertexPositionNormalTexture(vectors[5], normals[1], textureCoordinates[3]),
                new VertexPositionNormalTexture(vectors[6], normals[1], textureCoordinates[0]),
                new VertexPositionNormalTexture(vectors[7], normals[1], textureCoordinates[1]),
                // Left Face.
                new VertexPositionNormalTexture(vectors[2], normals[4], textureCoordinates[0]),
                new VertexPositionNormalTexture(vectors[0], normals[4], textureCoordinates[1]),
                new VertexPositionNormalTexture(vectors[6], normals[4], textureCoordinates[2]),
                new VertexPositionNormalTexture(vectors[4], normals[4], textureCoordinates[3]),
                // Right Face.
                new VertexPositionNormalTexture(vectors[3], normals[5], textureCoordinates[0]),
                new VertexPositionNormalTexture(vectors[1], normals[5], textureCoordinates[1]),
                new VertexPositionNormalTexture(vectors[7], normals[5], textureCoordinates[2]),
                new VertexPositionNormalTexture(vectors[5], normals[5], textureCoordinates[3]),
                // Back Face.
                new VertexPositionNormalTexture(vectors[0], normals[3], textureCoordinates[0]),
                new VertexPositionNormalTexture(vectors[1], normals[3], textureCoordinates[1]),
                new VertexPositionNormalTexture(vectors[4], normals[3], textureCoordinates[2]),
                new VertexPositionNormalTexture(vectors[5], normals[3], textureCoordinates[3]),
                // Front Face.
                new VertexPositionNormalTexture(vectors[2], normals[2], textureCoordinates[0]),
                new VertexPositionNormalTexture(vectors[3], normals[2], textureCoordinates[1]),
                new VertexPositionNormalTexture(vectors[6], normals[2], textureCoordinates[2]),
                new VertexPositionNormalTexture(vectors[7], normals[2], textureCoordinates[3])
            };

            Vertices = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
                BufferUsage.WriteOnly);
            Vertices.SetData(vertices);
        }

        /// <summary>
        ///     Create an index buffer for the vertex buffer that the figure has.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        private void CreateIndexBuffer(GraphicsDevice graphicsDevice)
        {
            var indices = new ushort[NumberOfIndices];

            // Top.
            indices[0] = 2;
            indices[1] = 1;
            indices[2] = 0;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 1;
            // Back.
            indices[6] = 18;
            indices[7] = 17;
            indices[8] = 16;
            indices[9] = 18;
            indices[10] = 19;
            indices[11] = 17;
            // Left.
            indices[12] = 10;
            indices[13] = 9;
            indices[14] = 8;
            indices[15] = 10;
            indices[16] = 11;
            indices[17] = 9;
            // Front.
            indices[18] = 22;
            indices[19] = 21;
            indices[20] = 20;
            indices[21] = 22;
            indices[22] = 23;
            indices[23] = 21;
            // Right.
            indices[24] = 14;
            indices[25] = 13;
            indices[26] = 12;
            indices[27] = 14;
            indices[28] = 15;
            indices[29] = 13;
            // Bottom.
            indices[30] = 6;
            indices[31] = 5;
            indices[32] = 4;
            indices[33] = 6;
            indices[34] = 7;
            indices[35] = 5;

            Indices = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length,
                BufferUsage.WriteOnly);
            Indices.SetData(indices);
        }

        /// <summary>
        ///     Draw the box.
        /// </summary>
        /// <param name="world">The world matrix for this box.</param>
        /// <param name="view">The view matrix, normally from the camera.</param>
        /// <param name="projection">The projection matrix, normally from the application.</param>
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            // Set BasicEffect parameters.
            Effect.World = world;
            Effect.View = view;
            Effect.Projection = projection;

            // Draw the model, using BasicEffect.
            Draw(Effect);
        }

        /// <summary>
        ///     Draws the primitive model, using the specified effect. Unlike the other Draw overload where you just specify the
        ///     world/view/projection matrices and color, this method does not set any render states, so you must make sure all
        ///     states are set to sensible values before you call it.
        /// </summary>
        /// <param name="effect">Used to set and query effects, and to choose techniques.</param>
        public void Draw(Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;

            // Set our vertex declaration, vertex buffer, and index buffer.
            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumberOfIndices / 3);
            }
        }
    }
}