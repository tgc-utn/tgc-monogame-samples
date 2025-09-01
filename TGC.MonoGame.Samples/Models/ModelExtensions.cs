using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Models;

public static class ModelExtensions
{
    public static ModelInfo Get(Model model)
    {
        int geometryCount = 0;
        foreach (var mesh in model.Meshes)
        {
            geometryCount += mesh.MeshParts.Count;
        }
        
        var geometryData = new GeometryData[geometryCount];

        int geometryIndex = 0;
        var absoluteMatrices = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(absoluteMatrices);

        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                var mainTexture = ((BasicEffect)part.Effect).Texture;
                Texture[] textures = mainTexture != null ? [mainTexture] : [];

                geometryData[geometryIndex] =
                    new GeometryData(Geometry.FromMeshPart(part), absoluteMatrices[mesh.ParentBone.Index],
                        textures);

                geometryIndex++;
            }
        }

        return new ModelInfo(geometryData);
    }
    
    public static ModelInfo GetMerged(Microsoft.Xna.Framework.Graphics.Model model)
    {
        var absoluteMatrices = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(absoluteMatrices);

        int vertexCount = 0;
        int indexCount = 0;
        
        var device = model.Meshes.First().Effects.First().GraphicsDevice;
            
        var textures = new List<Texture>();
        
        // Extract textures, vertices and indices
        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                vertexCount += part.NumVertices;
                indexCount += part.PrimitiveCount * 3;
                
                var mainTexture = ((BasicEffect)part.Effect).Texture;

                if (mainTexture != null)
                    textures.Add(mainTexture);
            }
        }

        VertexBuffer vertexBuffer;
        {
            // Extract vertices
            var vertices = new VertexPositionColorNormalTexture[vertexCount];

            Dictionary<VertexBuffer, byte[]> vertexData = new();

            foreach (var mesh in model.Meshes)
            {
                var transform = absoluteMatrices[mesh.ParentBone.Index];

                foreach (var part in mesh.MeshParts)
                {
                    var partVertexBuffer = part.VertexBuffer;

                    if (!vertexData.TryGetValue(partVertexBuffer, out var bufferData))
                    {
                        var declaration = partVertexBuffer.VertexDeclaration;
                        var vertexSize = declaration.VertexStride;
                        bufferData = new byte[vertexSize * partVertexBuffer.VertexCount];
                        partVertexBuffer.GetData(bufferData);
                        vertexData.Add(partVertexBuffer, bufferData);
                    }

                    int offsetByStride = part.VertexOffset *
                                      part.VertexBuffer.VertexDeclaration.VertexStride;
                    
                    int numVerticesByStride = part.NumVertices *
                                      part.VertexBuffer.VertexDeclaration.VertexStride;

                    CopyTo(bufferData.AsSpan().Slice(offsetByStride, numVerticesByStride),
                        vertices.AsSpan().Slice(part.VertexOffset, part.NumVertices),
                        transform, partVertexBuffer.VertexDeclaration);
                }
            }
            
            vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorNormalTexture), vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);
        }


        IndexBuffer indexBuffer = new IndexBuffer(device, 
            vertexCount > ushort.MaxValue ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits, 
            indexCount, BufferUsage.None);
        
        {
            // Extract indices
            
            Dictionary<IndexBuffer, byte[]> indexData = new();
            int currentIndexCount = 0;

            if (vertexCount > ushort.MaxValue)
            {
                var indices = new uint[indexCount];
                
                foreach (var mesh in model.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        var partIndexBuffer = part.IndexBuffer;

                        int stride = partIndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4;
                        
                        if (!indexData.TryGetValue(partIndexBuffer, out var bufferData))
                        {
                            var data = new byte[partIndexBuffer.IndexCount * stride];
                            partIndexBuffer.GetData(data);
                            bufferData = data;
                            indexData.Add(partIndexBuffer, data);
                        }

                        if (partIndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
                        {
                            var shortIndexData = MemoryMarshal.Cast<byte, ushort>(bufferData.AsSpan()
                                .Slice(part.StartIndex * stride, part.PrimitiveCount * 3 * stride));
                            
                            for (int i = 0; i < shortIndexData.Length; i++)
                            {
                                indices[i + currentIndexCount] = shortIndexData[i];
                            }

                            currentIndexCount += shortIndexData.Length;
                        }
                        else
                        {
                            var integerIndexData = MemoryMarshal.Cast<byte, uint>(bufferData.AsSpan()
                                .Slice(part.StartIndex * stride, part.PrimitiveCount * 3 * stride));
                            
                            integerIndexData.CopyTo(indices.AsSpan().Slice(currentIndexCount));
                            currentIndexCount += integerIndexData.Length;
                        }
                    }
                }
                
                indexBuffer.SetData(indices);
            }
            else
            {
                var indices = new ushort[indexCount];
                
                foreach (var mesh in model.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        var partIndexBuffer = part.IndexBuffer;

                        int stride = partIndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4;
                        
                        if (!indexData.TryGetValue(partIndexBuffer, out var bufferData))
                        {
                            var data = new byte[partIndexBuffer.IndexCount * stride];
                            partIndexBuffer.GetData(data);
                            bufferData = data;
                            indexData.Add(partIndexBuffer, data);
                        }

                        if (partIndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
                        {
                            var shortIndexData = MemoryMarshal.Cast<byte, ushort>(bufferData.AsSpan()
                                .Slice(part.StartIndex * stride, part.PrimitiveCount * 3 * stride));
                            
                            shortIndexData.CopyTo(indices.AsSpan().Slice(currentIndexCount));
                            
                            currentIndexCount += shortIndexData.Length;
                        }
                        else
                        {
                            var integerIndexData = MemoryMarshal.Cast<byte, uint>(bufferData.AsSpan()
                                .Slice(part.StartIndex * stride, part.PrimitiveCount * 3 * stride));
                            
                            for (int i = 0; i < integerIndexData.Length; i++)
                            {
                                indices[i + currentIndexCount] = (ushort)integerIndexData[i];
                            }

                            currentIndexCount += integerIndexData.Length;
                        }
                    }
                }
                
                indexBuffer.SetData(indices);
            }
        }
        
        return new ModelInfo(
        [
            new GeometryData(new Geometry(vertexBuffer, indexBuffer), Matrix.Identity, textures.ToArray())
        ]);
    }

    public static ModelInfo GetCentered(Model model)
    {
        int geometryCount = 0;
        foreach (var mesh in model.Meshes)
        {
            geometryCount += mesh.MeshParts.Count;
        }
        
        var geometryData = new GeometryData[geometryCount];

        var absoluteMatrices = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(absoluteMatrices);
        
        int vertexCount = 0;
        
        var device = model.Meshes.First().Effects.First().GraphicsDevice;

        List<VertexBuffer> vertexBuffers = new();
        
        Dictionary<VertexBuffer, (int Index, byte[] Data, VertexPositionColorNormalTexture[] Vertices)> vertexData = new();

        {
            Vector3 sum = Vector3.Zero;
            
            // Extract vertices

            int vertexBufferIndex = 0;
            
            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    vertexCount += part.NumVertices;
                    
                    var partVertexBuffer = part.VertexBuffer;

                    if (!vertexData.TryGetValue(partVertexBuffer, out var bufferData))
                    {
                        var declaration = partVertexBuffer.VertexDeclaration;
                        var vertexSize = declaration.VertexStride;
                        var data = new byte[vertexSize * partVertexBuffer.VertexCount];
                        partVertexBuffer.GetData(data);

                        bufferData.Index = vertexBufferIndex;
                        bufferData.Data = data;
                        bufferData.Vertices = new VertexPositionColorNormalTexture[partVertexBuffer.VertexCount];
                        vertexData.Add(partVertexBuffer, bufferData);
                        
                        vertexBufferIndex++;
                    }

                    int offsetByStride = part.VertexOffset *
                                         part.VertexBuffer.VertexDeclaration.VertexStride;
                    
                    int numVerticesByStride = part.NumVertices *
                                              part.VertexBuffer.VertexDeclaration.VertexStride;

                    Sum(bufferData.Data.AsSpan().Slice(offsetByStride, numVerticesByStride), partVertexBuffer.VertexDeclaration, ref sum);
                }
            }

            sum /= vertexCount;

            var transform = Matrix.CreateTranslation(-sum);

            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    var partVertexBuffer = part.VertexBuffer;

                    var vertexBufferData = vertexData[partVertexBuffer];

                    int offsetByStride = part.VertexOffset *
                                         part.VertexBuffer.VertexDeclaration.VertexStride;
                    
                    int numVerticesByStride = part.NumVertices *
                                              part.VertexBuffer.VertexDeclaration.VertexStride;

                    CopyTo(vertexBufferData.Data.AsSpan().Slice(offsetByStride, numVerticesByStride),
                        vertexBufferData.Vertices.AsSpan().Slice(part.VertexOffset, part.NumVertices),
                        transform, partVertexBuffer.VertexDeclaration);
                }
            }
            
            foreach (var data in vertexData.Values)
            {
                var vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorNormalTexture), data.Vertices.Length, BufferUsage.None);
                vertexBuffer.SetData(data.Vertices);
                vertexBuffers.Add(vertexBuffer);
            }
        }
        
        HashSet<VertexBuffer> assignedVertexBuffers = new();

        int geometryIndex = 0;
        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                var mainTexture = ((BasicEffect)part.Effect).Texture;

                Texture[] textures;
                
                if (mainTexture != null)
                    textures = [mainTexture];
                else
                    textures = [];
                
                geometryData[geometryIndex] = new GeometryData(
                    new Geometry()
                    {
                        VertexBuffer = vertexBuffers[vertexData[part.VertexBuffer].Index],
                        PrimitiveCount = part.PrimitiveCount,
                        VertexCount = part.NumVertices,
                        IndexBuffer = part.IndexBuffer,
                        StartIndex = part.StartIndex,
                        VertexOffset = part.VertexOffset,
                        OwnsVertexBuffer = assignedVertexBuffers.Add(part.VertexBuffer),
                        OwnsIndexBuffer = false
                    },
                    absoluteMatrices[mesh.ParentBone.Index],
                    textures
                );
                
                geometryIndex++;
            }
        }
        
        return new ModelInfo(geometryData);
    }
    
    public static ModelInfo GetCenteredXZ(Model model)
    {
        int geometryCount = 0;
        foreach (var mesh in model.Meshes)
        {
            geometryCount += mesh.MeshParts.Count;
        }
        
        var geometryData = new GeometryData[geometryCount];

        var absoluteMatrices = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(absoluteMatrices);
        
        int vertexCount = 0;
        
        var device = model.Meshes.First().Effects.First().GraphicsDevice;

        List<VertexBuffer> vertexBuffers = new();
        
        Dictionary<VertexBuffer, (int Index, byte[] Data, VertexPositionColorNormalTexture[] Vertices)> vertexData = new();

        {
            Vector3 sum = Vector3.Zero;
            
            // Extract vertices

            int vertexBufferIndex = 0;
            
            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    vertexCount += part.NumVertices;
                    
                    var partVertexBuffer = part.VertexBuffer;

                    if (!vertexData.TryGetValue(partVertexBuffer, out var bufferData))
                    {
                        var declaration = partVertexBuffer.VertexDeclaration;
                        var vertexSize = declaration.VertexStride;
                        var data = new byte[vertexSize * partVertexBuffer.VertexCount];
                        partVertexBuffer.GetData(data);

                        bufferData.Index = vertexBufferIndex;
                        bufferData.Data = data;
                        bufferData.Vertices = new VertexPositionColorNormalTexture[partVertexBuffer.VertexCount];
                        vertexData.Add(partVertexBuffer, bufferData);
                        
                        vertexBufferIndex++;
                    }

                    int offsetByStride = part.VertexOffset *
                                         part.VertexBuffer.VertexDeclaration.VertexStride;
                    
                    int numVerticesByStride = part.NumVertices *
                                              part.VertexBuffer.VertexDeclaration.VertexStride;

                    Sum(bufferData.Data.AsSpan().Slice(offsetByStride, numVerticesByStride), partVertexBuffer.VertexDeclaration, ref sum);
                }
            }

            sum /= vertexCount;

            var transform = Matrix.CreateTranslation(-sum.X, 0f, -sum.Z);

            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    var partVertexBuffer = part.VertexBuffer;

                    var vertexBufferData = vertexData[partVertexBuffer];

                    int offsetByStride = part.VertexOffset *
                                         part.VertexBuffer.VertexDeclaration.VertexStride;
                    
                    int numVerticesByStride = part.NumVertices *
                                              part.VertexBuffer.VertexDeclaration.VertexStride;

                    CopyTo(vertexBufferData.Data.AsSpan().Slice(offsetByStride, numVerticesByStride),
                        vertexBufferData.Vertices.AsSpan().Slice(part.VertexOffset, part.NumVertices),
                        transform, partVertexBuffer.VertexDeclaration);
                }
            }
            
            foreach (var data in vertexData.Values)
            {
                var vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorNormalTexture), data.Vertices.Length, BufferUsage.None);
                vertexBuffer.SetData(data.Vertices);
                vertexBuffers.Add(vertexBuffer);
            }
        }
        
        HashSet<VertexBuffer> assignedVertexBuffers = new();

        int geometryIndex = 0;
        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                var mainTexture = ((BasicEffect)part.Effect).Texture;

                Texture[] textures;
                
                if (mainTexture != null)
                    textures = [mainTexture];
                else
                    textures = [];
                
                geometryData[geometryIndex] = new GeometryData(
                    new Geometry()
                    {
                        VertexBuffer = vertexBuffers[vertexData[part.VertexBuffer].Index],
                        PrimitiveCount = part.PrimitiveCount,
                        VertexCount = part.NumVertices,
                        IndexBuffer = part.IndexBuffer,
                        StartIndex = part.StartIndex,
                        VertexOffset = part.VertexOffset,
                        OwnsVertexBuffer = assignedVertexBuffers.Add(part.VertexBuffer),
                        OwnsIndexBuffer = false
                    },
                    absoluteMatrices[mesh.ParentBone.Index],
                    textures
                );
                
                geometryIndex++;
            }
        }
        
        return new ModelInfo(geometryData);
    }

    public static ModelInfo GetMergedCentered(Microsoft.Xna.Framework.Graphics.Model model)
    {
        var absoluteMatrices = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(absoluteMatrices);

        int vertexCount = 0;
        int indexCount = 0;
        
        var device = model.Meshes.First().Effects.First().GraphicsDevice;
            
        var textures = new List<Texture>();
        
        // Extract textures, vertices and indices
        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                vertexCount += part.NumVertices;
                indexCount += part.PrimitiveCount * 3;
                
                var mainTexture = ((BasicEffect)part.Effect).Texture;

                if (mainTexture != null)
                    textures.Add(mainTexture);
            }
        }

        Vector3 sum = Vector3.Zero;
            
        Dictionary<VertexBuffer, byte[]> vertexData = new();
        
        // Extract vertices
            
        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                vertexCount += part.NumVertices;
                    
                var partVertexBuffer = part.VertexBuffer;

                if (!vertexData.TryGetValue(partVertexBuffer, out var bufferData))
                {
                    var declaration = partVertexBuffer.VertexDeclaration;
                    var vertexSize = declaration.VertexStride;
                    bufferData = new byte[vertexSize * partVertexBuffer.VertexCount];
                    partVertexBuffer.GetData(bufferData);
                    vertexData.Add(partVertexBuffer, bufferData);
                }

                int offsetByStride = part.VertexOffset *
                                     part.VertexBuffer.VertexDeclaration.VertexStride;
                    
                int numVerticesByStride = part.NumVertices *
                                          part.VertexBuffer.VertexDeclaration.VertexStride;

                Sum(bufferData.AsSpan().Slice(offsetByStride, numVerticesByStride), partVertexBuffer.VertexDeclaration, ref sum);
            }
        }

        sum /= vertexCount;

        var centeringTransform = Matrix.CreateTranslation(-sum);
        
        VertexBuffer vertexBuffer;
        {
            // Extract vertices
            var vertices = new VertexPositionColorNormalTexture[vertexCount];


            foreach (var mesh in model.Meshes)
            {
                var transform = absoluteMatrices[mesh.ParentBone.Index] * centeringTransform;

                foreach (var part in mesh.MeshParts)
                {
                    var partVertexBuffer = part.VertexBuffer;

                    var data = vertexData[partVertexBuffer];

                    int offsetByStride = part.VertexOffset *
                                      part.VertexBuffer.VertexDeclaration.VertexStride;
                    
                    int numVerticesByStride = part.NumVertices *
                                      part.VertexBuffer.VertexDeclaration.VertexStride;

                    CopyTo(data.AsSpan().Slice(offsetByStride, numVerticesByStride),
                        vertices.AsSpan().Slice(part.VertexOffset, part.NumVertices),
                        transform, partVertexBuffer.VertexDeclaration);
                }
            }
            
            vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorNormalTexture), vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);
        }

        IndexBuffer indexBuffer = new IndexBuffer(device, 
            vertexCount > ushort.MaxValue ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits, 
            indexCount, BufferUsage.None);
        
        {
            // Extract indices
            
            Dictionary<IndexBuffer, byte[]> indexData = new();
            int currentIndexCount = 0;

            if (vertexCount > ushort.MaxValue)
            {
                var indices = new uint[indexCount];
                
                foreach (var mesh in model.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        var partIndexBuffer = part.IndexBuffer;

                        int stride = partIndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4;
                        
                        if (!indexData.TryGetValue(partIndexBuffer, out var bufferData))
                        {
                            var data = new byte[partIndexBuffer.IndexCount * stride];
                            partIndexBuffer.GetData(data);
                            bufferData = data;
                            indexData.Add(partIndexBuffer, data);
                        }

                        if (partIndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
                        {
                            var shortIndexData = MemoryMarshal.Cast<byte, ushort>(bufferData.AsSpan()
                                .Slice(part.StartIndex * stride, part.PrimitiveCount * 3 * stride));
                            
                            for (int i = 0; i < shortIndexData.Length; i++)
                            {
                                indices[i + currentIndexCount] = shortIndexData[i];
                            }

                            currentIndexCount += shortIndexData.Length;
                        }
                        else
                        {
                            var integerIndexData = MemoryMarshal.Cast<byte, uint>(bufferData.AsSpan()
                                .Slice(part.StartIndex * stride, part.PrimitiveCount * 3 * stride));
                            
                            integerIndexData.CopyTo(indices.AsSpan().Slice(currentIndexCount));
                            currentIndexCount += integerIndexData.Length;
                        }
                    }
                }
                
                indexBuffer.SetData(indices);
            }
            else
            {
                var indices = new ushort[indexCount];
                
                foreach (var mesh in model.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        var partIndexBuffer = part.IndexBuffer;

                        int stride = partIndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4;
                        
                        if (!indexData.TryGetValue(partIndexBuffer, out var bufferData))
                        {
                            var data = new byte[partIndexBuffer.IndexCount * stride];
                            partIndexBuffer.GetData(data);
                            bufferData = data;
                            indexData.Add(partIndexBuffer, data);
                        }

                        if (partIndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
                        {
                            var shortIndexData = MemoryMarshal.Cast<byte, ushort>(bufferData.AsSpan()
                                .Slice(part.StartIndex * stride, part.PrimitiveCount * 3 * stride));
                            
                            shortIndexData.CopyTo(indices.AsSpan().Slice(currentIndexCount));
                            
                            currentIndexCount += shortIndexData.Length;
                        }
                        else
                        {
                            var integerIndexData = MemoryMarshal.Cast<byte, uint>(bufferData.AsSpan()
                                .Slice(part.StartIndex * stride, part.PrimitiveCount * 3 * stride));
                            
                            for (int i = 0; i < integerIndexData.Length; i++)
                            {
                                indices[i + currentIndexCount] = (ushort)integerIndexData[i];
                            }

                            currentIndexCount += integerIndexData.Length;
                        }
                    }
                }
                
                indexBuffer.SetData(indices);
            }
        }
        
        return new ModelInfo(
        [
            new GeometryData(new Geometry(vertexBuffer, indexBuffer), Matrix.Identity, textures.ToArray())
        ]);
    }
    
    private static uint GetMask(VertexElement[] elements)
    {
        uint mask = 0;

        for (int i = 0; i < elements.Length; i++)
        {
            mask |= (uint)((1 << (int)elements[i].VertexElementUsage));
        }

        return mask;
    }

    private static void Sum(Span<byte> data, VertexDeclaration declaration, ref Vector3 sum)
    {
        var positionElement = 
            declaration.GetVertexElements().First(e => e.VertexElementUsage == VertexElementUsage.Position);

        int dataOffset = 0;
        for (int i = 0; i < data.Length; i += declaration.VertexStride)
        {
            sum += MemoryMarshal.AsRef<Vector3>(data.Slice(dataOffset + positionElement.Offset));
        }
    }
    
    private static void CopyTo(Span<byte> from, Span<VertexPositionColorNormalTexture> destination, in Matrix matrix, VertexDeclaration declaration)
    {
        var elements = declaration.GetVertexElements().AsSpan();
        elements.Sort((a, b) 
            => a.VertexElementUsage.CompareTo(b.VertexElementUsage));

        uint mask = GetMask(declaration.GetVertexElements());

        int dataOffset = 0;
        int elementIndex = 0;
        for (int i = 0; i < destination.Length; i++)
        {
            if ((mask & (1 << (int)VertexElementUsage.Position)) != 0)
            {
                destination[i].Position = Vector3.Transform(
                    MemoryMarshal.AsRef<Vector3>(from.Slice(dataOffset + elements[elementIndex].Offset)), matrix);
                elementIndex++;
            }
            
            if ((mask & (1 << (int)VertexElementUsage.Color)) != 0)
            {
                destination[i].Color = MemoryMarshal.AsRef<Color>(from.Slice(dataOffset + elements[elementIndex].Offset));
                elementIndex++;
            }
            
            if ((mask & (1 << (int)VertexElementUsage.TextureCoordinate)) != 0)
            {
                destination[i].TextureCoordinate = MemoryMarshal.AsRef<Vector2>(from.Slice(dataOffset + elements[elementIndex].Offset));
                elementIndex++;
            }
            
            if ((mask & (1 << (int)VertexElementUsage.Normal)) != 0)
            {
                destination[i].Normal = Vector3.TransformNormal(
                    MemoryMarshal.AsRef<Vector3>(from.Slice(dataOffset + elements[elementIndex].Offset)), matrix);
            }
            
            dataOffset += declaration.VertexStride;
            elementIndex = 0;
        }
    }
}