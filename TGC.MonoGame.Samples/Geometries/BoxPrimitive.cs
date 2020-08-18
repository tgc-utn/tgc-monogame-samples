using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;

namespace TGC.MonoGame.Samples.Geometries
{
    /// <summary>
    /// 3D box or cube.
    /// </summary>
    public class BoxPrimitive
    {
        private const int NumberOfVertices = 8;
        private const int NumberOfIndices = 36;

        /// <summary>
        /// Create a box with center in (0,0,0), size 1 and white color
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        public BoxPrimitive(GraphicsDevice device) : this(device, Vector3.One)
        {
        }

        /// <summary>
        /// Create a box with center at (0,0,0), with a size and white color.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="size">Size of the box.</param>
        public BoxPrimitive(GraphicsDevice device, Vector3 size) : this(device, size, Vector3.Zero)
        {
        }

        /// <summary>
        /// Create a box with center in the given point, with a size and white color.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="size">Size of the box.</param>
        /// <param name="center">Center of the box.</param>
        public BoxPrimitive(GraphicsDevice device, Vector3 size, Vector3 center) : this(device, size, center, Color.White)
        {
        }

        /// <summary>
        /// Create a box with center in the given point, with a size and solid color.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="size">Size of the box.</param>
        /// <param name="center">Center of the box.</param>
        /// <param name="color">Color of the box.</param>
        public BoxPrimitive(GraphicsDevice device, Vector3 size, Vector3 center, Color color) : this(device, size, center, color,
            color, color, color, color, color, color, color)
        {
        }

        /// <summary>
        /// Create a box with a center at the given point, with a size and a color in each vertex.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
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
        public BoxPrimitive(GraphicsDevice device, Vector3 size, Vector3 center, Color color1, Color color2, Color color3,
            Color color4, Color color5, Color color6, Color color7, Color color8)
        {
            Effect = new BasicEffect(device);
            CreateVertexBuffer(device, size, center, color1, color2, color3, color4, color5, color6, color7,
                color8);
            CreateIndexBuffer(device);
        }

        /// <summary>
        /// Represents a list of 3D vertices to be streamed to the graphics device.
        /// </summary>
        private VertexBuffer Vertices { get; set; }

        /// <summary>
        /// Describes the rendering order of the vertices in a vertex buffer.
        /// </summary>
        private IndexBuffer Indices { get; set; }

        /// <summary>
        /// Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
        /// </summary>
        private BasicEffect Effect { get; }

        /// <summary>
        /// Create a vertex buffer for the figure with the given information.
        /// </summary>
        /// <param name="device">The graphics device to associate with this vertex buffer.</param>
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
        private void CreateVertexBuffer(GraphicsDevice device, Vector3 size, Vector3 center, Color color1,
            Color color2, Color color3, Color color4, Color color5, Color color6, Color color7, Color color8)
        {
            var x = size.X / 2;
            var y = size.Y / 2;
            var z = size.Z / 2;

            var cubeVertices = new VertexPositionColor[NumberOfVertices];
            cubeVertices[0].Position = new Vector3(-x + center.X, -y + center.Y, -z + center.Z);
            cubeVertices[0].Color = color1;
            cubeVertices[1].Position = new Vector3(-x + center.X, -y + center.Y, z + center.Z);
            cubeVertices[1].Color = color2;
            cubeVertices[2].Position = new Vector3(x + center.X, -y + center.Y, z + center.Z);
            cubeVertices[2].Color = color3;
            cubeVertices[3].Position = new Vector3(x + center.X, -y + center.Y, -z + center.Z);
            cubeVertices[3].Color = color4;
            cubeVertices[4].Position = new Vector3(-x + center.X, y + center.Y, -z + center.Z);
            cubeVertices[4].Color = color5;
            cubeVertices[5].Position = new Vector3(-x + center.X, y + center.Y, z + center.Z);
            cubeVertices[5].Color = color6;
            cubeVertices[6].Position = new Vector3(x + center.X, y + center.Y, z + center.Z);
            cubeVertices[6].Color = color7;
            cubeVertices[7].Position = new Vector3(x + center.X, y + center.Y, -z + center.Z);
            cubeVertices[7].Color = color8;

            Vertices = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, NumberOfVertices,
                BufferUsage.WriteOnly);
            Vertices.SetData(cubeVertices);
        }

        /// <summary>
        /// Create an index buffer for the vertex buffer that the figure has.
        /// </summary>
        /// <param name="device">The GraphicsDevice object to associate with the index buffer.</param>
        private void CreateIndexBuffer(GraphicsDevice device)
        {
            var cubeIndices = new ushort[NumberOfIndices];

            //Bottom face
            cubeIndices[0] = 0;
            cubeIndices[1] = 2;
            cubeIndices[2] = 3;
            cubeIndices[3] = 0;
            cubeIndices[4] = 1;
            cubeIndices[5] = 2;

            //Top face
            cubeIndices[6] = 4;
            cubeIndices[7] = 6;
            cubeIndices[8] = 5;
            cubeIndices[9] = 4;
            cubeIndices[10] = 7;
            cubeIndices[11] = 6;

            //Front face
            cubeIndices[12] = 5;
            cubeIndices[13] = 2;
            cubeIndices[14] = 1;
            cubeIndices[15] = 5;
            cubeIndices[16] = 6;
            cubeIndices[17] = 2;

            //Back face
            cubeIndices[18] = 0;
            cubeIndices[19] = 7;
            cubeIndices[20] = 4;
            cubeIndices[21] = 0;
            cubeIndices[22] = 3;
            cubeIndices[23] = 7;

            //Left face
            cubeIndices[24] = 0;
            cubeIndices[25] = 4;
            cubeIndices[26] = 1;
            cubeIndices[27] = 1;
            cubeIndices[28] = 4;
            cubeIndices[29] = 5;

            //Right face
            cubeIndices[30] = 2;
            cubeIndices[31] = 6;
            cubeIndices[32] = 3;
            cubeIndices[33] = 3;
            cubeIndices[34] = 6;
            cubeIndices[35] = 7;

            Indices = new IndexBuffer(device, IndexElementSize.SixteenBits, NumberOfIndices, BufferUsage.WriteOnly);
            Indices.SetData(cubeIndices);
        }

        /// <summary>
        /// Draw the box.
        /// </summary>
        /// <param name="graphicsDevice">The device where to draw.</param>
        /// <param name="camera">The camera contains the necessary matrices.</param>
        public void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;

            Effect.World = camera.WorldMatrix;
            Effect.View = camera.ViewMatrix;
            Effect.Projection = camera.ProjectionMatrix;

            Effect.VertexColorEnabled = true;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumberOfIndices / 3);
            }
        }
    }
}