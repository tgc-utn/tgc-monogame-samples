using System;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Models;

/// <summary>
/// A representation of a simple triangle-based geometry that can be drawn. 
/// </summary>
public class Geometry : IDisposable
{
    protected VertexBuffer VertexBuffer;
    
    protected IndexBuffer IndexBuffer;

    protected int VertexOffset;

    protected int StartIndex;
    
    protected int PrimitiveCount;
    
    protected bool OwnsVertexBuffer;
    
    protected bool OwnsIndexBuffer;

    internal static Geometry FromMeshPart(ModelMeshPart part)
    {
        return new Geometry(part);
    }

    internal Geometry()
    { }

    internal Geometry(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
    {
        VertexBuffer = vertexBuffer;
        IndexBuffer = indexBuffer;
        VertexOffset = 0;
        StartIndex = 0;
        PrimitiveCount = indexBuffer.IndexCount / 3;
        OwnsVertexBuffer = true;
        OwnsIndexBuffer = true;
    }
    
    private Geometry(ModelMeshPart part)
    {
        VertexBuffer = part.VertexBuffer;
        IndexBuffer = part.IndexBuffer;
        VertexOffset = part.VertexOffset;
        StartIndex = part.StartIndex;
        PrimitiveCount = part.PrimitiveCount;
        OwnsVertexBuffer = false;
        OwnsIndexBuffer = false;
    }
    
    internal Geometry(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, 
        int vertexOffset, int startIndex, int primitiveCount,
        bool ownsVertexBuffer, bool ownsIndexBuffer)
    {
        VertexBuffer = vertexBuffer;
        IndexBuffer = indexBuffer;
        VertexOffset = vertexOffset;
        StartIndex = startIndex;
        PrimitiveCount = primitiveCount;
        OwnsVertexBuffer = ownsVertexBuffer;
        OwnsIndexBuffer = ownsIndexBuffer;
    }
    
    public void Draw(Effect effect)
    {
        var graphicsDevice = effect.GraphicsDevice;
        
        graphicsDevice.SetVertexBuffer(VertexBuffer);
        graphicsDevice.Indices = IndexBuffer;
        
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 
                VertexOffset, StartIndex, PrimitiveCount);
        }
    }

    public void Dispose()
    {
        if (OwnsVertexBuffer)
        {
            VertexBuffer.Dispose();
        }

        if (OwnsIndexBuffer)
        {
            IndexBuffer.Dispose();
        }
    }
}