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

        private readonly GraphicsDevice device;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        public AxisLines(GraphicsDevice device)
        {
            this.device = device;
            CreateVertexBuffer();
        }

        /// <summary>
        ///     Array of vertex positions and colors.
        /// </summary>
        private VertexPositionColor[] AxisLinesVertices { get; set; }

        /// <summary>
        ///     Represents a list of 3D vertices to be streamed to the graphics device.
        /// </summary>
        private VertexBuffer Vertices { get; set; }

        /// <summary>
        ///     Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
        /// </summary>
        private BasicEffect Effect { get; set; }

        /// <summary>
        ///     Create a vertex buffer for the figure with the given information.
        /// </summary>
        private void CreateVertexBuffer()
        {
            AxisLinesVertices = new VertexPositionColor[NumberOfVertices];
            // Red = +x Axis
            AxisLinesVertices[0] = new VertexPositionColor(Vector3.Zero, Color.Red);
            AxisLinesVertices[1] = new VertexPositionColor(Vector3.UnitX * 60, Color.Red);
            // Green = +y Axis
            AxisLinesVertices[2] = new VertexPositionColor(Vector3.Zero, Color.Green);
            AxisLinesVertices[3] = new VertexPositionColor(Vector3.UnitY * 60, Color.Green);
            // Blue = +z Axis
            AxisLinesVertices[4] = new VertexPositionColor(Vector3.UnitZ * 60, Color.Blue);
            AxisLinesVertices[5] = new VertexPositionColor(Vector3.Zero, Color.Blue);

            Effect = new BasicEffect(device);

            Vertices = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, NumberOfVertices,
                BufferUsage.WriteOnly);
            Vertices.SetData(AxisLinesVertices);
        }

        /// <summary>
        ///     Draw de axes.
        /// </summary>
        /// <param name="view">The view matrix, normally from the camera.</param>
        /// <param name="projection">The projection matrix, normally from the application.</param>
        public void Draw(Matrix view, Matrix projection)
        {
            //Obtener World coordinate de la esquina inferior de la pantalla
            var width = device.Viewport.Width;
            var height = device.Viewport.Height;
            var sx = AxisPosOffset;
            var sy = height - AxisPosOffset;
            var v = new Vector3(sx, sy, 1.0f);

            //Transform the screen space into 3D space
            var worldCoordPos = device.Viewport.Unproject(v, projection, view, Matrix.Identity);

            Effect.World = Matrix.CreateTranslation(worldCoordPos);
            Effect.View = view;
            Effect.Projection = projection;
            Effect.VertexColorEnabled = true;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.LineList, AxisLinesVertices, 0, 3);
            }
        }
    }
}