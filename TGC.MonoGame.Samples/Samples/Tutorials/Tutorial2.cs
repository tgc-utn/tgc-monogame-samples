using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    ///     Tutorial 2:
    ///     Shows how to create a colored 3D box and display it on the screen.
    ///     Author: René Juan Rico Mendoza.
    /// </summary>
    public class Tutorial2 : TGCSample
    {
        /// <inheritdoc />
        public Tutorial2(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 2";
            Description =
                "Shows the Creation of a 3D Box with one color per vertex and can be moved using the keyboard arrows.";
        }

        private Camera Camera { get; set; }

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
        private BasicEffect Effect { get; set; }

        private Matrix BoxWorld { get; set; } = Matrix.Identity;

        /// <inheritdoc />
        public override void Initialize()
        {
            Game.Background = Color.CornflowerBlue;
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);

            Effect = new BasicEffect(GraphicsDevice);
            Effect.VertexColorEnabled = true;

            CreateVertexBuffer(Vector3.One * 25, Vector3.Zero, Color.Cyan, Color.Black, Color.Magenta, Color.Yellow,
                Color.Green, Color.Blue, Color.Red, Color.White);
            CreateIndexBuffer(GraphicsDevice);

            base.Initialize();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            // Press Directional Keys to rotate cube
            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Up)) BoxWorld *= Matrix.CreateRotationX(-0.05f);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Down)) BoxWorld *= Matrix.CreateRotationX(0.05f);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Left)) BoxWorld *= Matrix.CreateRotationY(-0.05f);

            if (Game.CurrentKeyboardState.IsKeyDown(Keys.Right)) BoxWorld *= Matrix.CreateRotationY(0.05f);

            Game.Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.SetVertexBuffer(Vertices);
            GraphicsDevice.Indices = Indices;

            Effect.World = BoxWorld;
            Effect.View = Camera.View;
            Effect.Projection = Camera.Projection;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.IndexCount / 3);
            }

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Create a vertex buffer for the figure with the given information.
        /// </summary>
        /// <param name="size">Size of the box.</param>
        /// <param name="center">Center of the box.</param>
        /// <param name="color1">Color of a vertex.</param>
        /// <param name="color2">Color of a vertex.</param>
        /// <param name="color3">Color of a vertex.</param>
        /// <param name="color4">Color of a vertex.</param>
        /// <param name="color5">Color of a vertex.</param>
        /// <param name="color6">Color of a vertex.</param>
        /// <param name="color7">Color of a vertex.</param>
        /// <param name="color8">Color of a vertex.</param>
        private void CreateVertexBuffer(Vector3 size, Vector3 center, Color color1, Color color2, Color color3,
            Color color4, Color color5, Color color6, Color color7, Color color8)
        {
            var x = size.X / 2;
            var y = size.Y / 2;
            var z = size.Z / 2;

            var cubeVertices = new[]
            {
                // Bottom-Left Front.
                new VertexPositionColor(new Vector3(-x + center.X, -y + center.Y, -z + center.Z), color1),
                // Bottom-Left Back.
                new VertexPositionColor(new Vector3(-x + center.X, -y + center.Y, z + center.Z), color2),
                // Bottom-Right Back.
                new VertexPositionColor(new Vector3(x + center.X, -y + center.Y, z + center.Z), color3),
                // Bottom-Right Front.
                new VertexPositionColor(new Vector3(x + center.X, -y + center.Y, -z + center.Z), color4),
                // Top-Left Front.
                new VertexPositionColor(new Vector3(-x + center.X, y + center.Y, -z + center.Z), color5),
                // Top-Left Back.
                new VertexPositionColor(new Vector3(-x + center.X, y + center.Y, z + center.Z), color6),
                // Top-Right Back.
                new VertexPositionColor(new Vector3(x + center.X, y + center.Y, z + center.Z), color7),
                // Top-Right Front.
                new VertexPositionColor(new Vector3(x + center.X, y + center.Y, -z + center.Z), color8)
            };

            Vertices = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, cubeVertices.Length,
                BufferUsage.WriteOnly);
            Vertices.SetData(cubeVertices);
        }

        /// <summary>
        ///     Create an index buffer for the vertex buffer that the figure has.
        /// </summary>
        /// <param name="device">The GraphicsDevice object to associate with the index buffer.</param>
        private void CreateIndexBuffer(GraphicsDevice device)
        {
            var cubeIndices = new ushort[]
            {
                // Bottom face.
                0, 2, 3, 0, 1, 2,
                // Top face.
                4, 6, 5, 4, 7, 6,
                // Front face.
                5, 2, 1, 5, 6, 2,
                // Back face.
                0, 7, 4, 0, 3, 7,
                // Left face.
                0, 4, 1, 1, 4, 5,
                // Right face.
                2, 6, 3, 3, 6, 7
            };

            Indices = new IndexBuffer(device, IndexElementSize.SixteenBits, cubeIndices.Length, BufferUsage.WriteOnly);
            Indices.SetData(cubeIndices);
        }
    }
}