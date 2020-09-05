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
        private const int NumberOfVertices = 3;

        /// <inheritdoc />
        public Tutorial1(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Tutorials;
            Name = "Tutorial 1";
            Description =
                "The classic hello world in graphics! Shows how to create triangles with color vertices drawn with primitives.";
        }

        /// <summary>
        ///     Array of vertex positions and colors.
        /// </summary>
        private VertexPositionColor[] TriangleVertices { get; set; }

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
            var viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 40), Vector3.Zero, Vector3.Up);
            var projectionMatrix =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 100);

            // Setup our basic effect
            Effect = new BasicEffect(GraphicsDevice);
            Effect.World = worldMatrix;
            Effect.View = viewMatrix;
            Effect.Projection = projectionMatrix;
            Effect.VertexColorEnabled = true;

            TriangleVertices = new VertexPositionColor[NumberOfVertices];
            TriangleVertices[0].Position = new Vector3(-10f, 0f, 0f);
            TriangleVertices[0].Color = Color.Blue;
            TriangleVertices[1].Position = new Vector3(0f, 10f, 0f);
            TriangleVertices[1].Color = Color.Red;
            TriangleVertices[2].Position = new Vector3(10f, 0f, 0f);
            TriangleVertices[2].Color = Color.Green;

            Vertices = new VertexBuffer(Game.GraphicsDevice, VertexPositionColor.VertexDeclaration, NumberOfVertices,
                BufferUsage.WriteOnly);
            Vertices.SetData(TriangleVertices);

            base.Initialize();
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.CornflowerBlue;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserPrimitives(
                    // We’ll be rendering one triangles
                    PrimitiveType.TriangleList,
                    // The array of verts that we want to render
                    TriangleVertices,
                    // The offset, which is 0 since we want to start at the beginning of the floorVerts array
                    0,
                    // The number of triangles to draw
                    1);
            }

            base.Draw(gameTime);
        }
    }
}