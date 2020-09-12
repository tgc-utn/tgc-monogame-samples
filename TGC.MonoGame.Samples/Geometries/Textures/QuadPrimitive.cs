using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Geometries.Textures
{
    /// <summary>
    ///     The quad is like a plane but its made by two triangle and the surface is oriented in the XY plane of the local
    ///     coordinate space.
    /// </summary>
    public class QuadPrimitive
    {
        /// <summary>
        ///     Create a textured quad.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="origin">The center.</param>
        /// <param name="normal">Normal vector.</param>
        /// <param name="up">Up vector.</param>
        /// <param name="width">The Width.</param>
        /// <param name="height">The High.</param>
        /// <param name="texture">The texture to use.</param>
        /// <param name="textureRepeats">Times to repeat the given texture.</param>
        public QuadPrimitive(GraphicsDevice graphicsDevice, Vector3 origin, Vector3 normal, Vector3 up, float width,
            float height, Texture2D texture, float textureRepeats)
        {
            Effect = new BasicEffect(graphicsDevice);
            Effect.TextureEnabled = true;
            Effect.Texture = texture;
            Effect.EnableDefaultLighting();

            Origin = origin;
            Normal = normal;
            Up = up;

            CreateVertexBuffer(graphicsDevice, width, height, textureRepeats);
            CreateIndexBuffer(graphicsDevice);
        }

        /// <summary>
        ///     Represents a list of 3D vertices to be streamed to the graphics device.
        /// </summary>
        private VertexBuffer Vertices { get; set; }

        /// <summary>
        ///     Describes the rendering order of the vertices in a vertex buffer, using clockwise winding.
        /// </summary>
        private IndexBuffer Indices { get; set; }

        /// <summary>
        ///     The left direction.
        /// </summary>
        private Vector3 Left { get; set; }

        /// <summary>
        ///     The lower left corner.
        /// </summary>
        private Vector3 LowerLeft { get; set; }

        /// <summary>
        ///     The lower right corner.
        /// </summary>
        private Vector3 LowerRight { get; set; }

        /// <summary>
        ///     The normal direction.
        /// </summary>
        private Vector3 Normal { get; }

        /// <summary>
        ///     The center.
        /// </summary>
        private Vector3 Origin { get; }

        /// <summary>
        ///     The up direction.
        /// </summary>
        private Vector3 Up { get; }

        /// <summary>
        ///     The up left corner.
        /// </summary>
        private Vector3 UpperLeft { get; set; }

        /// <summary>
        ///     The up right corner.
        /// </summary>
        private Vector3 UpperRight { get; set; }

        /// <summary>
        ///     Used to set and query effects and choose techniques.
        /// </summary>
        public BasicEffect Effect { get; }

        /// <summary>
        ///     Create a vertex buffer for the figure with the given information.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="width">The Width.</param>
        /// <param name="height">The High.</param>
        /// <param name="textureRepeats">Times to repeat the given texture.</param>
        private void CreateVertexBuffer(GraphicsDevice graphicsDevice, float width, float height, float textureRepeats)
        {
            // Calculate the quad corners
            Left = Vector3.Cross(Normal, Up);
            var upperCenter = Up * height / 2 + Origin;
            UpperLeft = upperCenter + Left * width / 2;
            UpperRight = upperCenter - Left * width / 2;
            LowerLeft = UpperLeft - Up * height;
            LowerRight = UpperRight - Up * height;

            // Fill in texture coordinates to display full texture on quad
            var textureUpperLeft = Vector2.Zero;
            var textureUpperRight = Vector2.UnitX;
            var textureLowerLeft = Vector2.UnitY;
            var textureLowerRight = Vector2.One;

            // Set the position and texture coordinate for each vertex
            var vertices = new[]
            {
                new VertexPositionNormalTexture(LowerLeft, Normal, textureLowerLeft * textureRepeats),
                new VertexPositionNormalTexture(UpperLeft, Normal, textureUpperLeft * textureRepeats),
                new VertexPositionNormalTexture(LowerRight, Normal, textureLowerRight * textureRepeats),
                new VertexPositionNormalTexture(UpperRight, Normal, textureUpperRight * textureRepeats)
            };

            Vertices = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
                BufferUsage.WriteOnly);
            Vertices.SetData(vertices);
        }

        private void CreateIndexBuffer(GraphicsDevice graphicsDevice)
        {
            // Set the index buffer for each vertex, using clockwise winding
            var indices = new ushort[]
            {
                0, 1, 2, 2, 1, 3
            };

            Indices = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length,
                BufferUsage.WriteOnly);
            Indices.SetData(indices);
        }

        /// <summary>
        ///     Draw the Quad.
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