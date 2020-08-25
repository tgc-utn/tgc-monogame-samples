using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;

namespace TGC.MonoGame.Samples.Geometries
{
    public class FrustumPrimitive: BoundingFrustum
    {
        private GraphicsDevice Device;

        public BasicEffect Effect { get; }

        private IndexBuffer Indices;
        private DynamicVertexBuffer Vertices;
        private Camera Camera;
        /// <summary>
        /// Create a debug frustum with the specified camera
        /// </summary>
        /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
        public FrustumPrimitive(GraphicsDevice device, Camera camera):base(camera.View * camera.Projection)
        {
            Device = device;
            Effect = new BasicEffect(device);
            Camera = camera;
            CreateIndexBuffer();
            CreateVertexBuffer();
        }

        private void CreateVertexBuffer()
        {
            Vertices = new DynamicVertexBuffer(Device,VertexPosition.VertexDeclaration, 8,
                BufferUsage.WriteOnly);
            Vertices.SetData(GetVerticesFromCorners(GetCorners()));
        }

        private VertexPosition[] GetVerticesFromCorners(Vector3[] corners)
        {
            return new VertexPosition[] {
                new VertexPosition(corners[0]),
                new VertexPosition(corners[1]),
                new VertexPosition(corners[2]),
                new VertexPosition(corners[3]),
                new VertexPosition(corners[4]),
                new VertexPosition(corners[5]),
                new VertexPosition(corners[6]),
                new VertexPosition(corners[7])
            };
        }

        private void CreateIndexBuffer() {
            ushort[] indices = {
                                0, 1, 5, //top
                                0, 4, 5,
                                0, 1, 2, //front
                                0, 3, 2,
                                4, 5, 6, //back
                                4, 7, 6,
                                2, 3, 6, //bottom
                                3, 7, 6,
                                0, 4, 7, //left
                                0, 3, 7,
                                1, 5, 6, //right
                                1, 2, 6
                            };
            Indices = new IndexBuffer(Device,IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            Indices.SetData(indices);
        }

        /// <summary>
        /// Draw the box.
        /// </summary>
        /// <param name="world">The world matrix for this box.</param>
        /// <param name="view">The view matrix, normally from the camera.</param>
        /// <param name="projection">The projection matrix, normally from the application.</param>
        public void Draw(Matrix view, Matrix projection, Matrix transform)
        {
            RasterizerState originalState = Device.RasterizerState;
 
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.FillMode = FillMode.WireFrame;
            rasterizerState.CullMode = CullMode.None;
            Device.RasterizerState = rasterizerState;
            
            Effect.World = transform;
            Effect.View = view;
            Effect.Projection = projection;
            Effect.LightingEnabled = true;
            Effect.AmbientLightColor = Color.LightSeaGreen.ToVector3();
            
            Device.SetVertexBuffer(Vertices);

            Device.Indices = Indices;
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
            }
            
            Device.RasterizerState = originalState;
            Device.SetVertexBuffer(Vertices);
            Device.Indices = Indices;            
        }
    }
}