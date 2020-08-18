using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;

namespace TGC.MonoGame.Samples.Geometries
{
    /// <summary>
    /// Triangle in a 3D world.
    /// </summary>
    public class TrianglePrimitive
    {
        private const int NumberOfVertices = 3;

        /// <summary>
        /// Create a triangle based on the vertices and colored white.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="vertex1">Vertex of the triangle.</param>
        /// <param name="vertex2">Vertex of the triangle.</param>
        /// <param name="vertex3">Vertex of the triangle.</param>
        public TrianglePrimitive(GraphicsDevice device, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) : this(device,
            vertex1, vertex2, vertex3, Color.White)
        {
        }

        /// <summary>
        /// Create a triangle based on vertices and color.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="vertex1">Vertex of the triangle.</param>
        /// <param name="vertex2">Vertex of the triangle.</param>
        /// <param name="vertex3">Vertex of the triangle.</param>
        /// <param name="vertexColor">The color of the triangle.</param>
        public TrianglePrimitive(GraphicsDevice device, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Color vertexColor) :
            this(device, vertex1, vertex2, vertex3, vertexColor, vertexColor, vertexColor)
        {
        }

        /// <summary>
        /// Create a triangle based on the vertices and a color for each one.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="vertex1">Vertex of the triangle.</param>
        /// <param name="vertex2">Vertex of the triangle.</param>
        /// <param name="vertex3">Vertex of the triangle.</param>
        /// <param name="vertexColor1">The color of the vertex.</param>
        /// <param name="vertexColor2">The color of the vertex.</param>
        /// <param name="vertexColor3">The color of the vertex.</param>
        public TrianglePrimitive(GraphicsDevice device, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Color vertexColor1,
            Color vertexColor2, Color vertexColor3)
        {
            Effect = new BasicEffect(device);
            CreateVertexBuffer(device, vertex1, vertex2, vertex3, vertexColor1, vertexColor2, vertexColor3);
        }

        /// <summary>
        /// Array of vertex positions and colors.
        /// </summary>
        private VertexPositionColor[] TriangleVertices { get; set; }

        /// <summary>
        /// Represents a list of 3D vertices to be streamed to the graphics device.
        /// </summary>
        private VertexBuffer Vertices { get; set; }

        /// <summary>
        /// Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
        /// </summary>
        private BasicEffect Effect { get; set; }

        /// <summary>
        /// Create a vertex buffer for the figure with the given information.
        /// </summary>
        /// <param name="device">The graphics device to associate with this vertex buffer.</param>
        /// <param name="vertex1">Vertex of the triangle.</param>
        /// <param name="vertex2">Vertex of the triangle.</param>
        /// <param name="vertex3">Vertex of the triangle.</param>
        /// <param name="vertexColor1">The color of the vertex.</param>
        /// <param name="vertexColor2">The color of the vertex.</param>
        /// <param name="vertexColor3">The color of the vertex.</param>
        private void CreateVertexBuffer(GraphicsDevice device, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
            Color vertexColor1, Color vertexColor2, Color vertexColor3)
        {
            TriangleVertices = new VertexPositionColor[NumberOfVertices];
            TriangleVertices[0].Position = vertex1;
            TriangleVertices[0].Color = vertexColor1;
            TriangleVertices[1].Position = vertex2;
            TriangleVertices[1].Color = vertexColor2;
            TriangleVertices[2].Position = vertex3;
            TriangleVertices[2].Color = vertexColor3;

            Effect = new BasicEffect(device);

            Vertices = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, NumberOfVertices,
                BufferUsage.WriteOnly);
            Vertices.SetData(TriangleVertices);
        }

        /// <summary>
        /// Draw the triangle.
        /// </summary>
        /// <param name="graphicsDevice">The device where to draw.</param>
        /// <param name="camera">The camera contains the necessary matrices.</param>
        public void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            Effect.World = camera.WorldMatrix;
            Effect.View = camera.ViewMatrix;
            Effect.Projection = camera.ProjectionMatrix;
            Effect.VertexColorEnabled = true;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives(
                    // We’ll be rendering one triangles
                    PrimitiveType.TriangleList,
                    // The array of verts that we want to render
                    TriangleVertices,
                    // The offset, which is 0 since we want to start at the beginning of the floorVerts array
                    0,
                    // The number of triangles to draw
                    1);
            }
        }
    }
}