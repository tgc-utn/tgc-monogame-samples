using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Tutorials
{
    /// <summary>
    ///     Tutorial 1:
    ///     Shows how to create a triangle and display it on the screen.
    ///     The classic hello world in graphics.
    ///     Author: René Juan Rico Mendoza.
    /// </summary>
    public class Tutorial1 : TGCSample
    {
        /// <inheritdoc />
        public Tutorial1(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 1";
            Description =
                "The classic hello world in graphics! Shows how to create triangles with color vertices drawn with primitives.";
        }

        /// <summary>
        ///     Represents a list of 3D vertices to be streamed to the graphics device.
        /// </summary>
        private VertexBuffer Vertices { get; set; }

        /// <summary>
        ///     Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
        /// </summary>
        private BasicEffect Effect { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            // Setup our graphics scene matrices
            var worldMatrix = Matrix.Identity;
            var viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 50), Vector3.Zero, Vector3.Up);
            var projectionMatrix =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 100);

            // Setup our basic effect
            Effect = new BasicEffect(GraphicsDevice)
            {
                World = worldMatrix,
                View = viewMatrix,
                Projection = projectionMatrix,
                VertexColorEnabled = true
            };

            // Array of vertex positions and colors.
            var triangleVertices = new[]
            {
                new VertexPositionColor(new Vector3(-15f, -5f, 0f), Color.Blue),
                new VertexPositionColor(new Vector3(0f, 10f, 0f), Color.Red),
                new VertexPositionColor(new Vector3(15f, -5f, 0f), Color.Green)
            };

            Vertices = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, triangleVertices.Length,
                BufferUsage.WriteOnly);
            Vertices.SetData(triangleVertices);

            base.Initialize();
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;
            AxisLines.Draw(Effect.View, Effect.Projection);

            // Set our vertex buffer.
            GraphicsDevice.SetVertexBuffer(Vertices);

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawPrimitives(
                    // We’ll be rendering one triangles.
                    PrimitiveType.TriangleList,
                    // The offset, which is 0 since we want to start at the beginning of the floorVerts array.
                    0,
                    // The number of triangles to draw.
                    1);
            }

            base.Draw(gameTime);
        }
    }
}