using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Geometries.Textures
{
    /// <summary>
    ///     Textured 3D box or cube.
    /// </summary>
    public class BoxPrimitive
    {
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

            var positions = new Vector3[]
            {
                // Back face
                new Vector3(x, -y, z),
                new Vector3(-x, -y, z),
                new Vector3(x, y, z),
                new Vector3(-x, y, z),
                
                // Front face
                new Vector3(x, y, -z),
                new Vector3(-x, y, -z),
                new Vector3(x, -y, -z),
                new Vector3(-x, -y, -z),
                
                // Top face
                new Vector3(x, y, z),
                new Vector3(-x, y, z),
                new Vector3(x, y, -z),
                new Vector3(-x, y, -z),
                
                // Bottom face
                new Vector3(x, -y, -z),
                new Vector3(x, -y, z),
                new Vector3(-x, -y, z),
                new Vector3(-x, -y, -z),
                
                // Left face
                new Vector3(-x, -y, z),
                new Vector3(-x, y, z),
                new Vector3(-x, y, -z),
                new Vector3(-x, -y, -z),
                
                // Right face
                new Vector3(x, -y, -z),
                new Vector3(x, y, -z),
                new Vector3(x, y, z),
                new Vector3(x, -y, z),
            };

            var textureCoordinates = new Vector2[]
            {
                // Back face
                Vector2.Zero,
                Vector2.UnitX,
                Vector2.UnitY,
                Vector2.One,
                
                // Front face
                Vector2.Zero,
                Vector2.UnitX,
                Vector2.UnitY,
                Vector2.One,
                
                // Top face
                Vector2.UnitX,
                Vector2.One,
                Vector2.Zero,
                Vector2.UnitY,
                
                // Bottom face
                Vector2.Zero,
                Vector2.UnitX,
                Vector2.One,
                Vector2.UnitY,
                
                // Left face
                Vector2.Zero,
                Vector2.UnitY,
                Vector2.One,
                Vector2.UnitX,
                
                // Right face
                Vector2.Zero,
                Vector2.UnitY,
                Vector2.One,
                Vector2.UnitX,
            };

            var normals = new Vector3[]
            {
                // Back face
                Vector3.Backward,
                Vector3.Backward,
                Vector3.Backward,
                Vector3.Backward,
                
                // Front face
                Vector3.Forward,
                Vector3.Forward,
                Vector3.Forward,
                Vector3.Forward,

                // Top face
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                
                // Bottom face
                Vector3.Down,
                Vector3.Down,
                Vector3.Down,
                Vector3.Down,
                
                // Left face
                Vector3.Left,
                Vector3.Left,
                Vector3.Left,
                Vector3.Left,
                
                // Right face
                Vector3.Right,
                Vector3.Right,
                Vector3.Right,
                Vector3.Right,
            };

            var vertices = new VertexPositionNormalTexture[positions.Length];

            for (int index = 0; index < vertices.Length; index++)
                vertices[index] = new VertexPositionNormalTexture(positions[index], normals[index], textureCoordinates[index]);


            Vertices = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
                    BufferUsage.None);
            Vertices.SetData(vertices);
        }

        /// <summary>
        ///     Create an index buffer for the vertex buffer that the figure has.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        private void CreateIndexBuffer(GraphicsDevice graphicsDevice)
        {
            var indices = new ushort[]
            {
                
                // Back face
                1, 2, 0,
                1, 3, 2,
                
                // Front face
                5, 6, 4,
                5, 7, 6,
                
                // Top face
                9, 10, 8,
                9, 11, 10,
                
                // Bottom face
                12, 15, 13,
                13, 15, 14,
                
                // Left face
                17, 16, 19,
                17, 19, 18,
                
                // Right face
                20, 23, 21,
                21, 23, 22,
            };
            

            Indices = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length,
                BufferUsage.None);
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
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.IndexCount / 3);
            }
        }
    }
}