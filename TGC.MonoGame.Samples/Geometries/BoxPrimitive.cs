using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Geometries
{
    /// <summary>
    ///     3D box or cube.
    /// </summary>
    public class BoxPrimitive : GeometricPrimitive
    {
        /// <summary>
        ///     Create a box with center in (0,0,0), size 1 and white color
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        public BoxPrimitive(GraphicsDevice graphicsDevice) : this(graphicsDevice, Vector3.One)
        {
        }

        /// <summary>
        ///     Create a box with center at (0,0,0), with a size and white color.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="size">Size of the box.</param>
        public BoxPrimitive(GraphicsDevice graphicsDevice, Vector3 size) : this(graphicsDevice, size, Vector3.Zero)
        {
        }

        /// <summary>
        ///     Create a box with center in the given point, with a size and white color.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="size">Size of the box.</param>
        /// <param name="center">Center of the box.</param>
        public BoxPrimitive(GraphicsDevice graphicsDevice, Vector3 size, Vector3 center) : this(graphicsDevice, size,
            center, Color.White)
        {
        }

        /// <summary>
        ///     Create a box with center in the given point, with a size and solid color.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        /// <param name="size">Size of the box.</param>
        /// <param name="center">Center of the box.</param>
        /// <param name="color">Color of the box.</param>
        public BoxPrimitive(GraphicsDevice graphicsDevice, Vector3 size, Vector3 center, Color color) : this(
            graphicsDevice, size, center, color, color, color, color, color, color, color, color)
        {
        }

        /// <summary>
        ///     Create a box with a center at the given point, with a size and a color in each vertex.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
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
        public BoxPrimitive(GraphicsDevice graphicsDevice, Vector3 size, Vector3 center, Color color1, Color color2,
            Color color3, Color color4, Color color5, Color color6, Color color7, Color color8)
        {
            var x = size.X / 2;
            var y = size.Y / 2;
            var z = size.Z / 2;

            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 1);
            AddIndex(CurrentVertex + 2);

            AddVertex(new Vector3(-x + center.X, -y + center.Y, -z + center.Z), color1);
            AddVertex(new Vector3(-x + center.X, -y + center.Y, z + center.Z), color2);
            AddVertex(new Vector3(x + center.X, -y + center.Y, z + center.Z), color3);
            AddVertex(new Vector3(x + center.X, -y + center.Y, -z + center.Z), color4);
            AddVertex(new Vector3(-x + center.X, y + center.Y, -z + center.Z), color5);
            AddVertex(new Vector3(-x + center.X, y + center.Y, z + center.Z), color6);
            AddVertex(new Vector3(x + center.X, y + center.Y, z + center.Z), color7);
            AddVertex(new Vector3(x + center.X, y + center.Y, -z + center.Z), color8);

            //Bottom face
            AddIndex(0);
            AddIndex(2);
            AddIndex(3);
            AddIndex(0);
            AddIndex(1);
            AddIndex(2);

            //Top face
            AddIndex(4);
            AddIndex(6);
            AddIndex(5);
            AddIndex(4);
            AddIndex(7);
            AddIndex(6);

            //Front face
            AddIndex(5);
            AddIndex(2);
            AddIndex(1);
            AddIndex(5);
            AddIndex(6);
            AddIndex(2);

            //Back face
            AddIndex(0);
            AddIndex(7);
            AddIndex(4);
            AddIndex(0);
            AddIndex(3);
            AddIndex(7);

            //Left face
            AddIndex(0);
            AddIndex(4);
            AddIndex(1);
            AddIndex(1);
            AddIndex(4);
            AddIndex(5);

            //Right face
            AddIndex(2);
            AddIndex(6);
            AddIndex(3);
            AddIndex(3);
            AddIndex(6);
            AddIndex(7);

            InitializePrimitive(graphicsDevice);
        }
    }
}
