using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Geometries
{
    public class FullScreenQuad
    {
        private readonly GraphicsDevice device;
        private IndexBuffer indexBuffer;
        private VertexBuffer vertexBuffer;

        /// <summary>
        ///     Create a quad used in clip space
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        public FullScreenQuad(GraphicsDevice device)
        {
            this.device = device;
            CreateVertexBuffer();
            CreateIndexBuffer();
        }

        private void CreateVertexBuffer()
        {
            var vertices = new VertexPositionTexture[4];
            vertices[0].Position = new Vector3(-1f, -1f, 0f);
            vertices[0].TextureCoordinate = new Vector2(0f, 1f);
            vertices[1].Position = new Vector3(-1f, 1f, 0f);
            vertices[1].TextureCoordinate = new Vector2(0f, 0f);
            vertices[2].Position = new Vector3(1f, -1f, 0f);
            vertices[2].TextureCoordinate = new Vector2(1f, 1f);
            vertices[3].Position = new Vector3(1f, 1f, 0f);
            vertices[3].TextureCoordinate = new Vector2(1f, 0f);

            vertexBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, 4,
                BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
        }

        private void CreateIndexBuffer()
        {
            var indices = new ushort[6];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;
            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 2;

            indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }


        public void Draw(Effect effect)
        {
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }

        public void Dispose()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }
    }
}