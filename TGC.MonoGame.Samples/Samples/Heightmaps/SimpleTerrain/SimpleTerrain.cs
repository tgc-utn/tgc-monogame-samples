using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Samples.Heightmaps.SimpleTerrain
{
    /// <summary>
    ///     Permite crear la malla de un terreno en base a una textura de Heightmap.
    ///     TODO needs a refactor.
    /// </summary>
    public class SimpleTerrain
    {
        private readonly Texture2D colorMapTexture;
        private readonly Effect Effect;
        private float m_scaleXZ = 1;
        private float m_scaleY = 1;
        private readonly Texture2D terrainTexture;
        private readonly Texture2D terrainTexture2;
        private VertexBuffer vbTerrain;

        public SimpleTerrain(GraphicsDevice graphicsDevice, Texture2D heightMap, Texture2D colorMap,
            Texture2D diffuseMap, Texture2D diffuseMap2, Effect effect)
        {
            //Shader
            Effect = effect;

            // cargo el heightmap
            LoadHeightmap(graphicsDevice, heightMap, 100, 4, Vector3.Zero);
            // textura con el color Map
            colorMapTexture = colorMap;
            // diffuse maps auxiliares
            terrainTexture = diffuseMap;
            terrainTexture2 = diffuseMap2;
        }

        /// <summary>
        ///     Valor de Y para cada par (X,Z) del Heightmap
        /// </summary>
        public int[,] HeightmapData { get; private set; }

        /// <summary>
        ///     Centro del terreno
        /// </summary>
        public Vector3 Center { get; private set; }

        /// <summary>
        ///     Renderiza el terreno
        /// </summary>
        public void Draw(Matrix World, Matrix View, Matrix Projection)
        {
            var graphicsDevice = Effect.GraphicsDevice;

            Effect.Parameters["texColorMap"].SetValue(colorMapTexture);
            Effect.Parameters["texDiffuseMap"].SetValue(terrainTexture);
            Effect.Parameters["texDiffuseMap2"].SetValue(terrainTexture2);
            Effect.Parameters["World"].SetValue(World);
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);

            graphicsDevice.SetVertexBuffer(vbTerrain);

            //Render con shader
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vbTerrain.VertexCount / 3);
            }
        }

        /// <summary>
        ///     Crea la malla de un terreno en base a un Heightmap
        /// </summary>
        /// <param name="heightmap">Imagen de Heightmap</param>
        /// <param name="scaleXZ">Escala para los ejes X y Z</param>
        /// <param name="scaleY">Escala para el eje Y</param>
        /// <param name="center">Centro de la malla del terreno</param>
        public void LoadHeightmap(GraphicsDevice graphicsDevice, Texture2D heightmap, float scaleXZ, float scaleY,
            Vector3 center)
        {
            Center = center;

            m_scaleXZ = scaleXZ;
            m_scaleY = scaleY;

            float tx_scale = 1; // 50f;

            //cargar heightmap
            HeightmapData = LoadHeightMap(heightmap);
            var width = HeightmapData.GetLength(0);
            var length = HeightmapData.GetLength(1);

            float min_h = 256;
            float max_h = 0;
            for (var i = 0; i < width; i++)
            for (var j = 0; j < length; j++)
            {
                //HeightmapData[i, j] = 256 - HeightmapData[i, j];
                if (HeightmapData[i, j] > max_h)
                    max_h = HeightmapData[i, j];
                if (HeightmapData[i, j] < min_h)
                    min_h = HeightmapData[i, j];
            }

            //Cargar vertices
            var totalVertices = 2 * 3 * (HeightmapData.GetLength(0) - 1) * (HeightmapData.GetLength(1) - 1);
            var dataIdx = 0;
            var data = new VertexPositionNormalTexture[totalVertices];

            center.X = center.X * scaleXZ - width / 2f * scaleXZ;
            center.Y = center.Y * scaleY;
            center.Z = center.Z * scaleXZ - length / 2f * scaleXZ;

            var N = new Vector3[width, length];
            for (var i = 0; i < width - 1; i++)
            for (var j = 0; j < length - 1; j++)
            {
                var v1 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j] * scaleY,
                    center.Z + j * scaleXZ);
                var v2 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j + 1] * scaleY,
                    center.Z + (j + 1) * scaleXZ);
                var v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j] * scaleY,
                    center.Z + j * scaleXZ);
                N[i, j] = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
            }

            for (var i = 0; i < width - 1; i++)
            for (var j = 0; j < length - 1; j++)
            {
                //Vertices
                var v1 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j] * scaleY,
                    center.Z + j * scaleXZ);
                var v2 = new Vector3(center.X + i * scaleXZ, center.Y + HeightmapData[i, j + 1] * scaleY,
                    center.Z + (j + 1) * scaleXZ);
                var v3 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j] * scaleY,
                    center.Z + j * scaleXZ);
                var v4 = new Vector3(center.X + (i + 1) * scaleXZ, center.Y + HeightmapData[i + 1, j + 1] * scaleY,
                    center.Z + (j + 1) * scaleXZ);

                //Coordendas de textura
                var t1 = new Vector2(i / (float) width, j / (float) length) * tx_scale;
                var t2 = new Vector2(i / (float) width, (j + 1) / (float) length) * tx_scale;
                var t3 = new Vector2((i + 1) / (float) width, j / (float) length) * tx_scale;
                var t4 = new Vector2((i + 1) / (float) width, (j + 1) / (float) length) * tx_scale;

                //Cargar triangulo 1
                data[dataIdx] = new VertexPositionNormalTexture(v1, N[i, j], t1);
                data[dataIdx + 1] = new VertexPositionNormalTexture(v2, N[i, j + 1], t2);
                data[dataIdx + 2] = new VertexPositionNormalTexture(v4, N[i + 1, j + 1], t4);

                //Cargar triangulo 2
                data[dataIdx + 3] = new VertexPositionNormalTexture(v1, N[i, j], t1);
                data[dataIdx + 4] = new VertexPositionNormalTexture(v4, N[i + 1, j + 1], t4);
                data[dataIdx + 5] = new VertexPositionNormalTexture(v3, N[i + 1, j], t3);

                dataIdx += 6;
            }

            //Crear vertexBuffer
            vbTerrain = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, totalVertices,
                BufferUsage.WriteOnly);
            vbTerrain.SetData(data);
        }

        /// <summary>
        ///     Carga los valores del Heightmap en una matriz
        /// </summary>
        protected int[,] LoadHeightMap(Texture2D texture)
        {
            var width = texture.Width;
            var height = texture.Height;
            var rawData = new Color[width * height];
            texture.GetData(rawData);
            var heightmap = new int[width, height];

            for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
            {
                //(j, i) invertido para primero barrer filas y despues columnas
                var pixel = rawData[j * texture.Width + i];
                var intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                heightmap[i, j] = (int) intensity;
            }

            return heightmap;
        }

        public float Height(float x, float z)
        {
            var width = HeightmapData.GetLength(0);
            var length = HeightmapData.GetLength(1);

            var pos_i = x / m_scaleXZ + width / 2.0f;
            var pos_j = z / m_scaleXZ + length / 2.0f;
            var pi = (int) pos_i;
            var fracc_i = pos_i - pi;
            var pj = (int) pos_j;
            var fracc_j = pos_j - pj;

            if (pi < 0)
                pi = 0;
            else if (pi >= width)
                pi = width - 1;

            if (pj < 0)
                pj = 0;
            else if (pj >= length)
                pj = length - 1;

            var pi1 = pi + 1;
            var pj1 = pj + 1;
            if (pi1 >= width)
                pi1 = width - 1;
            if (pj1 >= length)
                pj1 = length - 1;

            // 2x2 percent closest filtering usual:
            var H0 = HeightmapData[pi, pj];
            var H1 = HeightmapData[pi1, pj];
            var H2 = HeightmapData[pi, pj1];
            var H3 = HeightmapData[pi1, pj1];
            var H = (H0 * (1 - fracc_i) + H1 * fracc_i) * (1 - fracc_j) + (H2 * (1 - fracc_i) + H3 * fracc_i) * fracc_j;

            return H * m_scaleY;
        }
    }
}