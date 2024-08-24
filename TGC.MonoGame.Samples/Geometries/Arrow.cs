using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Geometries
{
    public class Arrow
    {
        private readonly Vector3 ORIGINAL_DIR = Vector3.Up;

        private VertexBuffer VertexBuffer { get; set; }

        public Color BodyColor { get; set; }

        public Color HeadColor { get; set; }

        public float Thickness { get; set; }

        public Vector2 HeadSize { get; set; }

        private BasicEffect Effect { get; set; }

        public Vector3 FromPosition { get; set; }

        public Vector3 ToPosition { get; set; }

        public Arrow(GraphicsDevice device)
        {
            VertexBuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, 54, BufferUsage.None);

            Thickness = 0.06f;
            HeadSize = new Vector2(0.3f, 0.6f);
            BodyColor = Color.Blue;
            HeadColor = Color.LightBlue;

            Effect = new BasicEffect(device);
            Effect.VertexColorEnabled = true;
            Effect.TextureEnabled = false;
        }


        /// <summary>
        ///     Actualizar parámetros de la flecha en base a los valores configurados
        /// </summary>
        public void UpdateValues()
        {
            var vertices = new VertexPositionColor[54];

            //Crear caja en vertical en Y con longitud igual al módulo de la recta.
            var lineVec = Vector3.Subtract(FromPosition, ToPosition);
            var lineLength = lineVec.Length();
            var min = new Vector3(-Thickness, 0, -Thickness);
            var max = new Vector3(Thickness, lineLength, Thickness);

            // Front face
            vertices[0] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), BodyColor);
            vertices[1] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), BodyColor);
            vertices[2] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), BodyColor);
            vertices[3] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), BodyColor);
            vertices[4] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), BodyColor);
            vertices[5] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), BodyColor);

            // Back face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[6] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), BodyColor);
            vertices[7] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), BodyColor);
            vertices[8] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), BodyColor);
            vertices[9] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), BodyColor);
            vertices[10] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), BodyColor);
            vertices[11] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), BodyColor);

            // Top face
            vertices[12] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), BodyColor);
            vertices[13] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), BodyColor);
            vertices[14] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), BodyColor);
            vertices[15] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), BodyColor);
            vertices[16] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), BodyColor);
            vertices[17] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), BodyColor);

            // Bottom face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[18] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), BodyColor);
            vertices[19] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), BodyColor);
            vertices[20] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), BodyColor);
            vertices[21] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), BodyColor);
            vertices[22] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), BodyColor);
            vertices[23] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), BodyColor);

            // Left face
            vertices[24] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), BodyColor);
            vertices[25] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), BodyColor);
            vertices[26] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), BodyColor);
            vertices[27] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), BodyColor);
            vertices[28] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), BodyColor);
            vertices[29] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), BodyColor);

            // Right face (remember this is facing *away* from the camera, so vertices should be clockwise order)
            vertices[30] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), BodyColor);
            vertices[31] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), BodyColor);
            vertices[32] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), BodyColor);
            vertices[33] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), BodyColor);
            vertices[34] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), BodyColor);
            vertices[35] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), BodyColor);

            //Vertices del cuerpo de la flecha
            var hMin = new Vector3(-HeadSize.X, lineLength, -HeadSize.X);
            var hMax = new Vector3(HeadSize.X, lineLength + HeadSize.Y, HeadSize.X);

            //Bottom face
            vertices[36] = new VertexPositionColor(new Vector3(hMin.X, hMin.Y, hMax.Z), HeadColor);
            vertices[37] = new VertexPositionColor(new Vector3(hMin.X, hMin.Y, hMin.Z), HeadColor);
            vertices[38] = new VertexPositionColor(new Vector3(hMax.X, hMin.Y, hMin.Z), HeadColor);
            vertices[39] = new VertexPositionColor(new Vector3(hMin.X, hMin.Y, hMax.Z), HeadColor);
            vertices[40] = new VertexPositionColor(new Vector3(hMax.X, hMin.Y, hMin.Z), HeadColor);
            vertices[41] = new VertexPositionColor(new Vector3(hMax.X, hMin.Y, hMax.Z), HeadColor);

            //Left face
            vertices[42] = new VertexPositionColor(new Vector3(hMin.X, hMin.Y, hMin.Z), HeadColor);
            vertices[43] = new VertexPositionColor(new Vector3(0, hMax.Y, 0), HeadColor);
            vertices[44] = new VertexPositionColor(new Vector3(hMin.X, hMin.Y, hMax.Z), HeadColor);

            //Right face
            vertices[45] = new VertexPositionColor(new Vector3(hMax.X, hMin.Y, hMin.Z), HeadColor);
            vertices[46] = new VertexPositionColor(new Vector3(0, hMax.Y, 0), HeadColor);
            vertices[47] = new VertexPositionColor(new Vector3(hMax.X, hMin.Y, hMax.Z), HeadColor);

            //Back face
            vertices[48] = new VertexPositionColor(new Vector3(hMin.X, hMin.Y, hMin.Z), HeadColor);
            vertices[49] = new VertexPositionColor(new Vector3(0, hMax.Y, 0), HeadColor);
            vertices[50] = new VertexPositionColor(new Vector3(hMax.X, hMin.Y, hMin.Z), HeadColor);

            //Front face
            vertices[51] = new VertexPositionColor(new Vector3(hMin.X, hMin.Y, hMax.Z), HeadColor);
            vertices[52] = new VertexPositionColor(new Vector3(0, hMax.Y, 0), HeadColor);
            vertices[53] = new VertexPositionColor(new Vector3(hMax.X, hMin.Y, hMax.Z), HeadColor);

            //Obtener matriz de rotacion respecto del vector de la linea
            lineVec.Normalize();
            var angle = MathF.Acos(Vector3.Dot(ORIGINAL_DIR, lineVec));
            var axisRotation = Vector3.Cross(ORIGINAL_DIR, lineVec);
            axisRotation.Normalize();
            var t = Matrix.CreateFromAxisAngle(axisRotation, angle) * Matrix.CreateTranslation(FromPosition);

            //Transformar todos los puntos
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position = Vector3.Transform(vertices[i].Position, t);
            }

            //Cargar vertexBuffer
            VertexBuffer.SetData(vertices);
        }


        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Effect.World = world;
            Effect.View = view;
            Effect.Projection = projection;

            var graphicsDevice = Effect.GraphicsDevice;
            graphicsDevice.SetVertexBuffer(VertexBuffer);

            foreach (var effectPass in Effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 18);
            }
        }

        public void Dispose()
        {
            Effect.Dispose();
            VertexBuffer.Dispose();
        }
    }
}
