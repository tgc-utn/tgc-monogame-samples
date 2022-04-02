using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Models.Drawers;

namespace TGC.MonoGame.Samples.Geometries
{
    public class FullScreenQuad : GeometryDrawer
    {
        /// <summary>
        ///     Create a quad used in clip space
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        public FullScreenQuad(GraphicsDevice device) : base(device)
        {
            CreateVertexBuffer();
            CreateIndexBuffer();
            _primitiveCount = 2;
            _startIndex = 0;
            _vertexOffset = 0;
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

            VertexBuffer = new VertexBuffer(_device, VertexPositionTexture.VertexDeclaration, 4,
                BufferUsage.None);
            VertexBuffer.SetData(vertices);
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

            IndexBuffer = new IndexBuffer(_device, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            IndexBuffer.SetData(indices);
        }


        public void Draw(Effect effect)
        {
            var passes = EffectInspector.GetDefaultPasses(effect.CurrentTechnique);
            Draw(passes);
        }


    }
}