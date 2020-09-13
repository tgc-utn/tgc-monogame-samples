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
    /// </summary>dd
    public class SimpleHeightmap : TGCSample
    {
        public SimpleHeightmap(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.Heightmaps;
            Name = "Simple Heightmap";
            Description = "Shows how to create a terrain based on a HeightMap texture manually.";
        }

        private BasicEffect Effect { get; set; }
        private Texture2D TerrainTexture { get; set; }
        private VertexBuffer VertexBufferTerrain { get; set; }
        private Camera Camera { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
            Camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(1500f, 450f, 3000f), 200, 0.5f,
                1, 5000);

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            // Heightmap texture of the terrain.
            var currentHeightmap = Game.Content.Load<Texture2D>(ContentFolderTextures + "heightmaps/heightmap-3");

            var currentScaleXZ = 50f;
            var currentScaleY = 1.5f;
            CreateHeightMapMesh(currentHeightmap, currentScaleXZ, currentScaleY);

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
            GraphicsDevice.SetVertexBuffer(VertexBufferTerrain);

            // Render terrain.
            Effect.View = Camera.View;
            Effect.Projection = Camera.Projection;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, VertexBufferTerrain.VertexCount / 3);
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
            var heightmap = LoadHeightMap(texture);

            // Gets the number of vertices the terrain has.
            var totalVertices = 2 * 3 * (heightmap.GetLength(0) - 1) * (heightmap.GetLength(1) - 1);

            // Create temporary array of vertices.
            var dataIdx = 0;
            var data = new VertexPositionTexture[totalVertices];

            // Iterate over the entire Heightmap matrix and create the necessary triangles for the terrain.
            for (var i = 0; i < heightmap.GetLength(0) - 1; i++)
            for (var j = 0; j < heightmap.GetLength(1) - 1; j++)
            {
                // Create the four vertices that make up this quadrant, applying the corresponding scale.
                var v1 = new Vector3(i * scaleXZ, heightmap[i, j] * scaleY, j * scaleXZ);
                var v2 = new Vector3(i * scaleXZ, heightmap[i, j + 1] * scaleY, (j + 1) * scaleXZ);
                var v3 = new Vector3((i + 1) * scaleXZ, heightmap[i + 1, j] * scaleY, j * scaleXZ);
                var v4 = new Vector3((i + 1) * scaleXZ, heightmap[i + 1, j + 1] * scaleY, (j + 1) * scaleXZ);

                // Create the texture coordinates for the four vertices of the quadrant.
                var t1 = new Vector2(i / (float) heightmap.GetLength(0), j / (float) heightmap.GetLength(1));
                var t2 = new Vector2(i / (float) heightmap.GetLength(0), (j + 1) / (float) heightmap.GetLength(1));
                var t3 = new Vector2((i + 1) / (float) heightmap.GetLength(0), j / (float) heightmap.GetLength(1));
                var t4 = new Vector2((i + 1) / (float) heightmap.GetLength(0),
                    (j + 1) / (float) heightmap.GetLength(1));

                // Load first triangle.
                data[dataIdx] = new VertexPositionTexture(v1, t1);
                data[dataIdx + 1] = new VertexPositionTexture(v2, t2);
                data[dataIdx + 2] = new VertexPositionTexture(v4, t4);

                // Load second triangle.
                data[dataIdx + 3] = new VertexPositionTexture(v1, t1);
                data[dataIdx + 4] = new VertexPositionTexture(v4, t4);
                data[dataIdx + 5] = new VertexPositionTexture(v3, t3);

                dataIdx += 6;
            }

            // Fill the entire VertexBuffer with the temporary array.
            VertexBufferTerrain = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), totalVertices,
                BufferUsage.WriteOnly);
            VertexBufferTerrain.SetData(data);
        }

        /// <summary>
        ///     Load Bitmap and get the grayscale value of Y for each coordinate (x, z).
        /// </summary>
        /// <param name="texture">The Heightmap texture.</param>
        /// <returns>The height of each vertex.</returns>
        private int[,] LoadHeightMap(Texture2D texture)
        {
            var rawData = new Color[texture.Width * texture.Height];
            texture.GetData(rawData);
            var heightmap = new int[texture.Width, texture.Height];
            for (var i = 0; i < texture.Width; i++)
            for (var j = 0; j < texture.Height; j++)
            {
                // Get the color.
                // (j, i) inverted to sweep rows first and then columns.
                var pixel = rawData[j * texture.Width + i];
                // Calculate intensity in grayscale.
                var intensity = pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f;
                heightmap[i, j] = (int) intensity;
            }

            return heightmap;
        }

        /// <inheritdoc />
        protected override void UnloadContent()
        {
            VertexBufferTerrain.Dispose();

            base.UnloadContent();
        }
    }
}