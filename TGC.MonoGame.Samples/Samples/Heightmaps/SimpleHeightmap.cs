using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.Heightmaps;

/// <summary>
///     Create Basic Heightmap:
///     Creates a terrain based on a Heightmap texture.
///     Apply a texture to color (DiffuseMap) on the ground.
///     The texture is parsed and a VertexBuffer is created based on the different heights of the image.
///     Author: Matias Leone, Leandro Barbagallo.
/// </summary>
public class SimpleHeightMap : TGCSample
{
    private Camera _camera;
    private BasicEffect _effect;
    // Triangle count in this case.
    private int _primitiveCount;
    private IndexBuffer _terrainIndexBuffer;
    private Texture2D _terrainTexture;
    private VertexBuffer _terrainVertexBuffer;

    public SimpleHeightMap(TGCViewer game) : base(game)
    {
        Category = TGCSampleCategory.Heightmaps;
        Name = "Simple Heightmap";
        Description = "Shows how to create a terrain based on a HeightMap texture manually.";
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        _camera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-500f, 1000f, 2000f), 400, 1.0f, 1,
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
        _terrainTexture = Game.Content.Load<Texture2D>(ContentFolderTextures + "heightmaps/terrain-texture-3");

        _effect = new BasicEffect(GraphicsDevice)
        {
            World = Matrix.Identity,
            TextureEnabled = true,
            Texture = _terrainTexture
        };
        _effect.EnableDefaultLighting();

        base.LoadContent();
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        _camera.Update(gameTime);

        Game.Gizmos.UpdateViewProjection(_camera.View, _camera.Projection);

        base.Update(gameTime);
    }

    /// <inheritdoc />
    public override void Draw(GameTime gameTime)
    {
        Game.Background = Color.Black;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.SetVertexBuffer(_terrainVertexBuffer);
        GraphicsDevice.Indices = _terrainIndexBuffer;

        // Render terrain.
        _effect.View = _camera.View;
        _effect.Projection = _camera.Projection;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitiveCount);
        }

        base.Draw(gameTime);
    }

    /// <summary>
    ///     Create and load the VertexBuffer based on a Heightmap texture.
    /// </summary>
    /// <param name="texture">The Heightmap texture</param>
    /// <param name="scaleXZ">ScaleXZ is the distance between the vertices in the XZ plane, where the terrain does not rise</param>
    /// <param name="scaleY">ScaleY is the distance by variability of gray in the heightmap</param>
    private void CreateHeightMapMesh(Texture2D texture, float scaleXZ, float scaleY)
    {
        var heightMap = new HeightMap(texture.Width, texture.Height);

        // Parse bitmap and load height matrix.
        LoadHeightMap(texture, ref heightMap);

        CreateVertexBuffer(heightMap, scaleXZ, scaleY);

        var heightMapWidthMinusOne = heightMap.Width - 1;
        var heightMapLengthMinusOne = heightMap.Length - 1;

        _primitiveCount = 2 * heightMapWidthMinusOne * heightMapLengthMinusOne;

        CreateIndexBuffer(heightMapWidthMinusOne, heightMapLengthMinusOne);
    }

    /// <summary>
    ///     Load Bitmap and get the grayscale value of Y for each coordinate (x, z).
    /// </summary>
    /// <param name="texture">The Heightmap texture</param>
    /// <param name="heightMap">The filled heightmap data</param>
    /// <returns>The height of each vertex from zero to one</returns>
    private void LoadHeightMap(Texture2D texture, ref HeightMap heightMap)
    {
        var texels = new Color[texture.Width * texture.Height];

        // Obtains each texel color from the texture, note that this is an expensive operation.
        texture.GetData(texels);

        for (var x = 0; x < texture.Width; x++)
        for (var y = 0; y < texture.Height; y++)
        {
            // Get the color.
            // (j, i) inverted to sweep rows first and then columns.
            var texel = texels[y * texture.Width + x];
            heightMap[x, y] = texel.R;
        }
    }

    /// <summary>
    ///     Create a Vertex Buffer from a HeightMap.
    /// </summary>
    /// <param name="heightMap">The Heightmap which specifies height for each vertex</param>
    /// <param name="scaleXZ">The distance between the vertices in both the X and Z axis</param>
    /// <param name="scaleY">The scale in the Y axis for the vertices of the HeightMap</param>
    private void CreateVertexBuffer(in HeightMap heightMap, float scaleXZ, float scaleY)
    {
        var offsetX = heightMap.Width * scaleXZ * 0.5f;
        var offsetZ = heightMap.Length * scaleXZ * 0.5f;

        // Amount of subdivisions in X times amount of subdivisions in Z.
        var vertexCount = heightMap.Width * heightMap.Length;

        // Create temporary array of vertices.
        var vertices = new VertexPositionNormalTexture[vertexCount];

        var index = 0;

        for (var x = 0; x < heightMap.Width; x++)
        for (var z = 0; z < heightMap.Length; z++)
        {
            var position = new Vector3(x * scaleXZ - offsetX, heightMap[x, z] * scaleY, z * scaleXZ - offsetZ);
            var textureCoordinates = new Vector2((float)x / heightMap.Width, (float)z / heightMap.Length);
            var normal = CalculateNormal(heightMap, x, z, scaleXZ, scaleY);
            vertices[index] = new VertexPositionNormalTexture(position, normal, textureCoordinates);
            index++;
        }

        // Create the actual vertex buffer.
        _terrainVertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration,
            vertexCount, BufferUsage.None);
        _terrainVertexBuffer.SetData(vertices);
    }

    /// <summary>
    ///     Create an Index Buffer for a tessellated plane.
    /// </summary>
    /// <param name="quadsInX">The amount of quads in the X axis</param>
    /// <param name="quadsInZ">The amount of quads in the Z axis</param>
    private void CreateIndexBuffer(int quadsInX, int quadsInZ)
    {
        var indexCount = 3 * 2 * quadsInX * quadsInZ;

        var indices = new ushort[indexCount];
        var index = 0;

        var vertexCountX = quadsInX + 1;
        for (var x = 0; x < quadsInX; x++)
        for (var z = 0; z < quadsInZ; z++)
        {
            var right = x + 1;
            var bottom = z * vertexCountX;
            var top = (z + 1) * vertexCountX;

            //  d __ c  
            //   | /|
            //   |/_|
            //  a    b

            var a = (ushort)(x + bottom);
            var b = (ushort)(right + bottom);
            var c = (ushort)(right + top);
            var d = (ushort)(x + top);

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

        _terrainIndexBuffer =
            new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.None);
        _terrainIndexBuffer.SetData(indices);
    }

    /// <summary>
    ///     Calculate the normal vector for a vertex at the specified position in the heightmap.
    /// </summary>
    /// <param name="heightMap">The heightmap data</param>
    /// <param name="x">X coordinate in the heightmap</param>
    /// <param name="z">Z coordinate in the heightmap</param>
    /// <param name="scaleXZ">Scale factor for XZ plane</param>
    /// <param name="scaleY">Scale factor for Y axis</param>
    /// <returns>Normalized normal vector</returns>
    private Vector3 CalculateNormal(in HeightMap heightMap, int x, int z, float scaleXZ, float scaleY)
    {
        // Get neighboring heights (clamp at edges).
        var hL = heightMap[Math.Max(0, x - 1), z] * scaleY; // Left
        var hR = heightMap[Math.Min(heightMap.Width - 1, x + 1), z] * scaleY; // Right
        var hD = heightMap[x, Math.Max(0, z - 1)] * scaleY; // Down
        var hU = heightMap[x, Math.Min(heightMap.Length - 1, z + 1)] * scaleY; // Up

        // Calculate tangent vectors.
        var tangentX = new Vector3(2.0f * scaleXZ, hR - hL, 0);
        var tangentZ = new Vector3(0, hU - hD, 2.0f * scaleXZ);

        // Normal = normalized cross product.
        var normal = Vector3.Cross(tangentZ, tangentX);
        normal.Normalize();

        return normal;
    }

    /// <inheritdoc />
    protected override void UnloadContent()
    {
        _terrainVertexBuffer?.Dispose();
        _terrainIndexBuffer?.Dispose();
        _effect?.Dispose();
        base.UnloadContent();
    }

    /// <summary>
    ///     Struct holding heightmap data on a 2D grid (Width x Length).
    /// </summary>
    /// <param name="width">Sample count in X</param>
    /// <param name="length">Sample count in Z</param>
    private readonly struct HeightMap(int width, int length)
    {
        public readonly int Width = width;
        public readonly int Length = length;

        private readonly float[] _samples = new float[width * length];

        /// <summary>Gets or sets the height at (x, z)</summary>
        /// <param name="x">X in range [0, Width)</param>
        /// <param name="z">Z in range [0, Length)</param>
        public float this[int x, int z]
        {
            get => _samples[x + z * Width];
            set => _samples[x + z * Width] = value;
        }
    }
}