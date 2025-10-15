using System;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Models;

public class Geometry : IDisposable
{
    internal VertexBuffer VertexBuffer;
    
    internal IndexBuffer IndexBuffer;

    internal int VertexOffset;

    internal int StartIndex;
    
    internal int PrimitiveCount;
    
    internal bool OwnsVertexBuffer;
    
    internal bool OwnsIndexBuffer;
    

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