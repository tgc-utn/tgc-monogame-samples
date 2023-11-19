using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Geometries;

/// <summary>
/// A quad covering the entire screen.
/// </summary>
public class FullScreenQuad
{
    private readonly GraphicsDevice _device;
    private IndexBuffer _indexBuffer;
    private VertexBuffer _vertexBuffer;

    /// <summary>
    ///     Create a quad used in clip space.
    /// </summary>
    /// <param name="device">Used to initialize and control the presentation of the graphics device.</param>
    public FullScreenQuad(GraphicsDevice device)
    {
        _device = device;
        CreateVertexBuffer();
        CreateIndexBuffer();
    }

    /// <summary>
    ///     Create a vertex buffer for the full screen quad.
    /// </summary>
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

        _vertexBuffer = new VertexBuffer(_device, VertexPositionTexture.VertexDeclaration, 4,
            BufferUsage.WriteOnly);
        _vertexBuffer.SetData(vertices);
    }

    /// <summary>
    /// Create an index buffer for the full screen quad.
    /// </summary>
    private void CreateIndexBuffer()
    {
        var indices = new ushort[6];

        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 3;
        indices[3] = 0;
        indices[4] = 3;
        indices[5] = 2;

        _indexBuffer = new IndexBuffer(_device, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
        _indexBuffer.SetData(indices);
    }

    /// <summary>
    /// Draw the full screen quad.
    /// </summary>
    /// <param name="effect"> Effect to draw the quad with.</param>
    public void Draw(Effect effect)
    {
        _device.SetVertexBuffer(_vertexBuffer);
        _device.Indices = _indexBuffer;

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
    }

    /// <summary>
    /// Dispose the full screen quad.
    /// </summary>
    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
    }
}
