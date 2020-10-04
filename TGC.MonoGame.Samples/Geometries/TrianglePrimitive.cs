using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Geometries
{
    /// <summary>
    ///     Triangle in a 3D world.
    /// </summary>
    public class TrianglePrimitive : GeometricPrimitive
    {
        /// <summary>
        ///     Create a triangle based on the vertices and colored white.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="vertex1">Vertex of the triangle.</param>
        /// <param name="vertex2">Vertex of the triangle.</param>
        /// <param name="vertex3">Vertex of the triangle.</param>
        public TrianglePrimitive(GraphicsDevice device, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) : this(
            device, vertex1, vertex2, vertex3, Color.White)
        {
        }

        /// <summary>
        ///     Create a triangle based on vertices and color.
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="vertex1">Vertex of the triangle.</param>
        /// <param name="vertex2">Vertex of the triangle.</param>
        /// <param name="vertex3">Vertex of the triangle.</param>
        /// <param name="vertexColor">The color of the triangle.</param>
        public TrianglePrimitive(GraphicsDevice device, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
            Color vertexColor) : this(device, vertex1, vertex2, vertex3, vertexColor, vertexColor, vertexColor)
        {
        }

        /// <summary>
        ///     Create a triangle based on the vertices and a color for each one.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="vertex1">Vertex of the triangle.</param>
        /// <param name="vertex2">Vertex of the triangle.</param>
        /// <param name="vertex3">Vertex of the triangle.</param>
        /// <param name="vertexColor1">The color of the vertex.</param>
        /// <param name="vertexColor2">The color of the vertex.</param>
        /// <param name="vertexColor3">The color of the vertex.</param>
        public TrianglePrimitive(GraphicsDevice graphicsDevice, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
            Color vertexColor1, Color vertexColor2, Color vertexColor3)
        {
            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 1);
            AddIndex(CurrentVertex + 2);

            var normal = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex2);
            normal.Normalize();

            AddVertex(vertex1, vertexColor1, normal);
            AddVertex(vertex2, vertexColor2, normal);
            AddVertex(vertex3, vertexColor3, normal);

            InitializePrimitive(graphicsDevice);
        }
    }
}