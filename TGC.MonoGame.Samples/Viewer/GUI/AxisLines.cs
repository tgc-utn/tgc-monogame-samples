using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer.GUI
{
    /// <summary>
    ///     Tool to draw the Cartesian axes.
    /// </summary>
    public class AxisLines
    {
        private const float AxisPosOffset = 40;
        private const float AxisPosDistance = 25;
        private const int NumberOfVertices = 6;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        public AxisLines(GraphicsDevice graphicsDevice)
        {
            Effect = new BasicEffect(graphicsDevice);
            Effect.VertexColorEnabled = true;

            CreateVertexBuffer(graphicsDevice);
        }

        /// <summary>
        ///     Represents a list of 3D vertices to be streamed to the graphics device.
        /// </summary>
        private VertexBuffer Vertices { get; set; }

        /// <summary>
        ///     Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
        /// </summary>
        private BasicEffect Effect { get; }

        /// <summary>
        ///     Create a vertex buffer for the figure with the given information.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        private void CreateVertexBuffer(GraphicsDevice graphicsDevice)
        {
            var linesVertices = new VertexPositionColor[NumberOfVertices];
            // Red = +x Axis
            linesVertices[0] = new VertexPositionColor(Vector3.Zero, Color.Red);
            linesVertices[1] = new VertexPositionColor(Vector3.UnitX * 60, Color.Red);
            // Green = +y Axis
            linesVertices[2] = new VertexPositionColor(Vector3.Zero, Color.Green);
            linesVertices[3] = new VertexPositionColor(Vector3.UnitY * 60, Color.Green);
            // Blue = +z Axis
            linesVertices[4] = new VertexPositionColor(Vector3.UnitZ * 60, Color.Blue);
            linesVertices[5] = new VertexPositionColor(Vector3.Zero, Color.Blue);

            Vertices = new VertexBuffer(graphicsDevice, VertexPositionColor.VertexDeclaration, linesVertices.Length,
                BufferUsage.WriteOnly);
            Vertices.SetData(linesVertices);
        }

        /// <summary>
        ///     Draw de axes.
        /// </summary>
        /// <param name="view">The view matrix, normally from the camera.</param>
        /// <param name="projection">The projection matrix, normally from the application.</param>
        public void Draw(Matrix view, Matrix projection)
        {
            var graphicsDevice = Effect.GraphicsDevice;

            // Set our vertex buffer.
            graphicsDevice.SetVertexBuffer(Vertices);

            // Get World coordinate from the bottom corner of the screen.
            var width = graphicsDevice.Viewport.Width;
            var height = graphicsDevice.Viewport.Height;
            var sx = width - AxisPosOffset;
            var sy = height - AxisPosOffset;
            var screenPosition = new Vector3(sx, sy, 1.0f);

            //Transform the screen space into 3D space
            var worldCoordPos = graphicsDevice.Viewport.Unproject(screenPosition, projection, view, Matrix.Identity);

            Effect.World = Matrix.CreateTranslation(worldCoordPos);
            Effect.View = view;
            Effect.Projection = projection;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, NumberOfVertices / 2);
            }
        }
    }
}