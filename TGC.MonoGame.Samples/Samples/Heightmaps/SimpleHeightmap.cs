using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Heightmaps
{
    /// <summary>
    ///     Create Basic Heightmap:
    ///     Creates a terrain based on a Heightmap texture.
    ///     Apply a texture to color (DiffuseMap) on the ground.
    ///     The texture is parsed and a VertexBuffer is created based on the different heights of the image.
    ///     Author: Matias Leone, Leandro Barbagallo.
    /// </summary>
    public class SimpleHeightMap : TGCSample
    {
        public SimpleHeightMap(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Heightmaps;
            Name = "Simple Heightmap";
            Description = "Shows how to create a terrain based on a HeightMap texture manually.";
        }

        private BasicEffect Effect { get; set; }
        private Texture2D TerrainTexture { get; set; }
        private VertexBuffer TerrainVertexBuffer { get; set; }
        private IndexBuffer TerrainIndexBuffer { get; set; }
        private Camera Camera { get; set; }

        // Triangle count in this case
        private int PrimitiveCount { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-400f, 1000f, 2000f), 400, 1.0f, 1,
                6000);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Heightmap texture of the terrain.
            var currentHeightmap = Game.Content.Load<Texture2D>(ContentFolderTextures + "heightmaps/heightmap-3");

            var scaleXZ = 50f;
            var scaleY = 4f;
            CreateHeightMapMesh(currentHeightmap, scaleXZ, scaleY);

            // Terrain texture.
            TerrainTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "heightmaps/terrain-texture-3");

            Effect = new BasicEffect(GraphicsDevice)
            {
                World = Matrix.Identity,
                TextureEnabled = true,
                Texture = TerrainTexture
            };
            Effect.EnableDefaultLighting();

            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            Camera.Update(gameTime);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Color.Black;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SetVertexBuffer(TerrainVertexBuffer);
            GraphicsDevice.Indices = TerrainIndexBuffer;

            // Render terrain.
            Effect.View = Camera.View;
            Effect.Projection = Camera.Projection;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, PrimitiveCount);
            }

            AxisLines.Draw(Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Create and load the VertexBuffer based on a Heightmap texture.
        /// </summary>
        /// <param name="texture">The Heightmap texture.</param>
        /// <param name="scaleXZ">ScaleXZ is the distance between the vertices in the XZ plane, where the terrain does not rise.</param>
        /// <param name="scaleY">ScaleY is the distance by variability of gray in the heightmap.</param>
        private void CreateHeightMapMesh(Texture2D texture, float scaleXZ, float scaleY)
        {
            // Parse bitmap and load height matrix.
            var heightMap = LoadHeightMap(texture);

            CreateVertexBuffer(heightMap, scaleXZ, scaleY);

            var heightMapWidthMinusOne = heightMap.GetLength(0) - 1;
            var heightMapLengthMinusOne = heightMap.GetLength(1) - 1;

            PrimitiveCount = 2 * heightMapWidthMinusOne * heightMapLengthMinusOne;

            CreateIndexBuffer(heightMapWidthMinusOne, heightMapLengthMinusOne);
        }

        /// <summary>
        ///     Load Bitmap and get the grayscale value of Y for each coordinate (x, z).
        /// </summary>
        /// <param name="texture">The Heightmap texture.</param>
        /// <returns>The height of each vertex from zero to one.</returns>
        private float[,] LoadHeightMap(Texture2D texture)
        {
            var texels = new Color[texture.Width * texture.Height];

            // Obtains each texel color from the texture, note that this is an expensive operation
            texture.GetData(texels);

            var heightmap = new float[texture.Width, texture.Height];

            for (var x = 0; x < texture.Width; x++)
            for (var y = 0; y < texture.Height; y++)
            {
                // Get the color.
                // (j, i) inverted to sweep rows first and then columns.
                var texel = texels[y * texture.Width + x];
                heightmap[x, y] = texel.R;
            }

            return heightmap;
        }

        /// <summary>
        ///     Create a Vertex Buffer from a HeightMap
        /// </summary>
        /// <param name="heightMap">The Heightmap which specifies height for each vertex</param>
        /// <param name="scaleXZ">The distance between the vertices in both the X and Z axis</param>
        /// <param name="scaleY">The scale in the Y axis for the vertices of the HeightMap</param>
        private void CreateVertexBuffer(float[,] heightMap, float scaleXZ, float scaleY)
        {
            var heightMapWidth = heightMap.GetLength(0);
            var heightMapLength = heightMap.GetLength(1);

            var offsetX = heightMapWidth * scaleXZ * 0.5f;
            var offsetZ = heightMapLength * scaleXZ * 0.5f;

            // Amount of subdivisions in X times amount of subdivisions in Z.
            var vertexCount = heightMapWidth * heightMapLength;

            // Create temporary array of vertices.
            var vertices = new VertexPositionTexture[vertexCount];

            var index = 0;
            Vector3 position;
            Vector2 textureCoordinates;

            for (var x = 0; x < heightMapWidth; x++)
            for (var z = 0; z < heightMapLength; z++)
            {
                position = new Vector3(x * scaleXZ - offsetX, heightMap[x, z] * scaleY, z * scaleXZ - offsetZ);
                textureCoordinates = new Vector2((float) x / heightMapWidth, (float) z / heightMapLength);
                vertices[index] = new VertexPositionTexture(position, textureCoordinates);
                index++;
            }

            // Create the actual vertex buffer
            TerrainVertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration, vertexCount,
                BufferUsage.None);
            TerrainVertexBuffer.SetData(vertices);
        }


        /// <summary>
        ///     Create an Index Buffer for a tesselated plane
        /// </summary>
        /// <param name="quadsInX">The amount of quads in the X axis</param>
        /// <param name="quadsInZ">The amount of quads in the Z axis</param>
        private void CreateIndexBuffer(int quadsInX, int quadsInZ)
        {
            var indexCount = 3 * 2 * quadsInX * quadsInZ;

            var indices = new ushort[indexCount];
            var index = 0;

            int right;
            int top;
            int bottom;

            var vertexCountX = quadsInX + 1;
            for (var x = 0; x < quadsInX; x++)
            for (var z = 0; z < quadsInZ; z++)
            {
                right = x + 1;
                bottom = z * vertexCountX;
                top = (z + 1) * vertexCountX;

                //  d __ c  
                //   | /|
                //   |/_|
                //  a    b

                var a = (ushort) (x + bottom);
                var b = (ushort) (right + bottom);
                var c = (ushort) (right + top);
                var d = (ushort) (x + top);

                // ACB
                indices[index] = a;
                index++;
                indices[index] = c;
                index++;
                indices[index] = b;
                index++;

                // ADC
                indices[index] = a;
                index++;
                indices[index] = d;
                index++;
                indices[index] = c;
                index++;
            }

            TerrainIndexBuffer =
                new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.None);
            TerrainIndexBuffer.SetData(indices);
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            TerrainVertexBuffer.Dispose();

            base.UnloadContent();
        }
    }
}