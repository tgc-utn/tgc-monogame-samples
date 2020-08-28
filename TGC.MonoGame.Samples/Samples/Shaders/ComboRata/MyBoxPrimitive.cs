using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Samples.Shaders.ComboRata
{
    public class MyBoxPrimitive
    {
        private const int NumberOfVertices = 8;
        private const int NumberOfIndices = 36;
        public Effect Effect;
        public IndexBuffer Indices;
        public VertexBuffer Vertices;

        public MyBoxPrimitive(GraphicsDevice device)
        {
            CreateVertexBuffer(device);
            CreateIndexBuffer(device);
        }

        private void CreateVertexBuffer(GraphicsDevice device)
        {
            var x = 0.5f;
            var y = 0.5f;
            var z = 0.5f;

            var cubeVertices = new VertexPositionNormalTexture[NumberOfVertices];
            cubeVertices[0].Position = new Vector3(-x, -y, -z);
            cubeVertices[1].Position = new Vector3(-x, -y, z);
            cubeVertices[2].Position = new Vector3(x, -y, z);
            cubeVertices[3].Position = new Vector3(x, -y, -z);
            cubeVertices[4].Position = new Vector3(-x, y, -z);
            cubeVertices[5].Position = new Vector3(-x, y, z);
            cubeVertices[6].Position = new Vector3(x, y, z);
            cubeVertices[7].Position = new Vector3(x, y, -z);

            Vertices = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, NumberOfVertices,
                BufferUsage.WriteOnly);
            Vertices.SetData(cubeVertices);
        }

        /// <summary>
        ///     Create an index buffer for the vertex buffer that the figure has.
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

        public void Draw(GraphicsDevice graphicsDevice, Vector3 position, Vector3 size)
        {
            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;

            var world = Matrix.CreateScale(size) * Matrix.CreateTranslation(position);
            Effect.Parameters["World"].SetValue(world);

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumberOfIndices / 3);
            }
        }
    }
}