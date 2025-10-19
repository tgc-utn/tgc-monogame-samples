using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TGC.MonoGame.Samples.Models;

/// <summary>
/// Provides utilities to get <see cref="ModelInfo"/> from MonoGame <see cref="Model"/>s.
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Gets <see cref="ModelInfo"/> for a MonoGame <see cref="Model"/>.
    /// Lists simplified matrices, textures and geometry for a group of meshes that live inside the model.
    /// </summary>
    /// <param name="model">The model to get the info from</param>
    /// <returns>A collection of associated information to each mesh of the model</returns>
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

        IterateMeshAndParts(model, (mesh, part) => 
        {
            var mainTexture = ((BasicEffect)part.Effect).Texture;
            Texture[] textures = mainTexture != null ? [mainTexture] : [];

            geometryData[geometryIndex] =
                new GeometryData(Geometry.FromMeshPart(part), absoluteMatrices[mesh.ParentBone.Index],
                    textures);

            geometryIndex++;
        });

        return new ModelInfo(geometryData);
    }
    
    /// <summary>
    /// Gets a <see cref="ModelInfo"/> with a single entry for a MonoGame <see cref="Model"/>.
    /// Merges meshes and parts that live inside the model according to their matrices.
    /// </summary>
    /// <param name="model">The model to get the merged model info from</param>
    /// <returns>A model info with a single instance of merged geometry</returns>
    public static ModelInfo GetMerged(Model model)
    {
        var absoluteMatrices = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(absoluteMatrices);

        int vertexCount = 0;
        int indexCount = 0;
        
        var textures = new List<Texture>();
        
        IterateMeshAndParts(model, (_, part) =>
        {
            vertexCount += part.NumVertices;
            indexCount += part.PrimitiveCount * 3;
                
            var mainTexture = ((BasicEffect)part.Effect).Texture;

            if (mainTexture != null)
            {
                textures.Add(mainTexture);
            }
        });

        Dictionary<VertexBuffer, byte[]> vertexData = new();
        
        GetMergedBuffers(model, vertexCount, indexCount, Matrix.Identity,
            vertexData, out var vertexBuffer, out var indexBuffer);
        
        return new ModelInfo(
        [
            new GeometryData(new Geometry(vertexBuffer, indexBuffer), Matrix.Identity, textures.ToArray())
        ]);
    }
    
    /// <summary>
    /// Gets <see cref="ModelInfo"/> for a MonoGame <see cref="Model"/>.
    /// Lists simplified matrices, textures and geometry for a group of meshes that live inside the model.
    /// Centers all geometry based on the averaged centered position of all meshes inside the model provided.
    /// <remarks>This method modifies the geometry to center all meshes</remarks>
    /// </summary>
    /// <param name="model">The model to get the info from</param>
    /// <returns>A collection of associated information to each mesh of the model with geometry centered</returns>
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

        ExtractVertexDataAndSum(model, vertexCount, out var vertexData, out Vector3 sum);
        sum /= vertexCount;

        var transform = Matrix.CreateTranslation(-sum);

        CopyVertexBuffersIntoSingleBuffer(model, vertexData, transform);
        
        foreach (var data in vertexData.Values)
        {
            var vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorNormalTexture), data.Vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(data.Vertices);
            vertexBuffers.Add(vertexBuffer);
        }
        
        HashSet<VertexBuffer> assignedVertexBuffers = new();

        int geometryIndex = 0;

        IterateMeshAndParts(model, (mesh, part) =>
        {
            var mainTexture = ((BasicEffect)part.Effect).Texture;

            Texture[] textures = mainTexture != null ? [mainTexture] : [];

            geometryData[geometryIndex] = new GeometryData(
                new Geometry
                (
                    vertexBuffers[vertexData[part.VertexBuffer].Index],
                    part.IndexBuffer,
                    part.VertexOffset,
                    part.StartIndex,
                    part.PrimitiveCount,
                    assignedVertexBuffers.Add(part.VertexBuffer),
                    false
                ),
                absoluteMatrices[mesh.ParentBone.Index],
                textures
            );
                
            geometryIndex++;
        });
        
        return new ModelInfo(geometryData);
    }

    private static void CopyVertexBuffersIntoSingleBuffer(Model model, Dictionary<VertexBuffer, (int Index, byte[] Data, VertexPositionColorNormalTexture[] Vertices)> vertexData, Matrix transform)
    {
        IterateMeshAndParts(model, (_, part) =>
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
        });
    }

    /// <summary>
    /// Gets <see cref="ModelInfo"/> for a MonoGame <see cref="Model"/>.
    /// Lists simplified matrices, textures and geometry for a group of meshes that live inside the model.
    /// Centers all geometry based on the averaged centered position across the XZ plane of all meshes inside the model provided.
    /// </summary>
    /// <param name="model">The model to get the info from</param>
    /// <returns>A collection of associated information to each mesh of the model with geometry centered across the XZ plane</returns>
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
        
        ExtractVertexDataAndSum(model, vertexCount, out var vertexData, out Vector3 sum);

        var transform = Matrix.CreateTranslation(-sum.X, 0f, -sum.Z);

        CopyVertexBuffersIntoSingleBuffer(model, vertexData, transform);
        
        foreach (var data in vertexData.Values)
        {
            var vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorNormalTexture), data.Vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(data.Vertices);
            vertexBuffers.Add(vertexBuffer);
        }
        
        HashSet<VertexBuffer> assignedVertexBuffers = new();

        int geometryIndex = 0;
        
        IterateMeshAndParts(model, (mesh, part) =>
        {
            var mainTexture = ((BasicEffect)part.Effect).Texture;

            Texture[] textures = mainTexture != null ? [mainTexture] : [];
                
            geometryData[geometryIndex] = new GeometryData(
                new Geometry
                (
                    vertexBuffers[vertexData[part.VertexBuffer].Index],
                    part.IndexBuffer,
                    part.VertexOffset,
                    part.StartIndex,
                    part.PrimitiveCount,
                    assignedVertexBuffers.Add(part.VertexBuffer),
                    false
                ),
                absoluteMatrices[mesh.ParentBone.Index],
                textures
            );
                
            geometryIndex++;
        });

        return new ModelInfo(geometryData);
    }

    private static void ExtractVertexDataAndSum(Model model, int vertexCount, out Dictionary<VertexBuffer, 
        (int Index, byte[] Data, VertexPositionColorNormalTexture[] Vertices)> vertexBufferData, out Vector3 sum)
    {
        var generatedVertexData = new Dictionary<VertexBuffer, 
            (int Index, byte[] Data, VertexPositionColorNormalTexture[] Vertices)>();
        
        Vector3 calculatedSum = Vector3.Zero;
        
        int vertexBufferIndex = 0;

        IterateMeshAndParts(model, (_, part) =>
        {
            vertexCount += part.NumVertices;
                
            var partVertexBuffer = part.VertexBuffer;

            if (!generatedVertexData.TryGetValue(partVertexBuffer, out var bufferData))
            {
                var declaration = partVertexBuffer.VertexDeclaration;
                var vertexSize = declaration.VertexStride;
                var data = new byte[vertexSize * partVertexBuffer.VertexCount];
                partVertexBuffer.GetData(data);

                bufferData.Index = vertexBufferIndex;
                bufferData.Data = data;
                bufferData.Vertices = new VertexPositionColorNormalTexture[partVertexBuffer.VertexCount];
                generatedVertexData.Add(partVertexBuffer, bufferData);
                    
                vertexBufferIndex++;
            }

            int offsetByStride = part.VertexOffset *
                                 part.VertexBuffer.VertexDeclaration.VertexStride;
                
            int numVerticesByStride = part.NumVertices *
                                      part.VertexBuffer.VertexDeclaration.VertexStride;

            Sum(bufferData.Data.AsSpan().Slice(offsetByStride, numVerticesByStride),
                partVertexBuffer.VertexDeclaration, ref calculatedSum);
        });

        sum = calculatedSum;
        sum /= vertexCount;
        vertexBufferData = generatedVertexData;
    }

    /// <summary>
    /// Gets <see cref="ModelInfo"/> for a MonoGame <see cref="Model"/>.
    /// Lists simplified matrices, textures and geometry for a group of meshes that live inside the model.
    /// Centers all geometry based on the averaged centered position of all meshes inside the model provided.
    /// Merges meshes and parts that live inside the model according to their matrices.
    /// <remarks>This method modifies the geometry to center all meshes</remarks>
    /// </summary>
    /// <param name="model">The model to get the merged info from</param>
    /// <returns>A model info with a single instance of merged geometry</returns>
    public static ModelInfo GetMergedCentered(Model model)
    {
        int vertexCount = 0;
        int indexCount = 0;
        
        var textures = new List<Texture>();
        
        Vector3 sum = Vector3.Zero;
            
        var vertexData = new Dictionary<VertexBuffer, byte[]>();
        
        IterateMeshAndParts(model, (_, part) =>
        {
            vertexCount += part.NumVertices;
            indexCount += part.PrimitiveCount * 3;
                
            var mainTexture = ((BasicEffect)part.Effect).Texture;

            if (mainTexture != null)
            {
                textures.Add(mainTexture);
            }
                    
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

            // Done this way so its captured
            Sum(bufferData.AsSpan().Slice(offsetByStride, numVerticesByStride), partVertexBuffer.VertexDeclaration, ref sum);
        });

        sum /= vertexCount;

        var centeringTransform = Matrix.CreateTranslation(-sum);
        
        GetMergedBuffers(model, vertexCount, indexCount, centeringTransform,
            vertexData, out var vertexBuffer, out var indexBuffer);

        return new ModelInfo(
        [
            new GeometryData(new Geometry(vertexBuffer, indexBuffer), Matrix.Identity, textures.ToArray())
        ]);
    }

    private static void GetMergedBuffers(Model model, int vertexCount, int indexCount, Matrix absoluteTransform,
        Dictionary<VertexBuffer, byte[]> vertexData, out VertexBuffer vertexBuffer, out IndexBuffer indexBuffer)
    {
        var absoluteMatrices = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(absoluteMatrices);

        var device = model.Meshes.First().Effects.First().GraphicsDevice;

        var vertices = new VertexPositionColorNormalTexture[vertexCount];

        Dictionary<IndexBuffer, byte[]> indexData = new();
        
        bool largeIndices = vertexCount > ushort.MaxValue;
        
        byte[] indices = new byte[indexCount * (largeIndices ? sizeof(uint) : sizeof(ushort))];

        int currentIndex = 0;
        int vertexOffset = 0;
        
        IterateMeshAndParts(model, (mesh, part) =>
        {
            var transform = absoluteMatrices[mesh.ParentBone.Index];
            var partVertexBuffer = part.VertexBuffer;

            if (!vertexData.TryGetValue(partVertexBuffer, out var bufferData))
            {
                var declaration = partVertexBuffer.VertexDeclaration;
                var vertexSize = declaration.VertexStride;
                bufferData = new byte[vertexSize * partVertexBuffer.VertexCount];
                partVertexBuffer.GetData(bufferData);
                vertexData.Add(partVertexBuffer, bufferData);
            }

            int indexStride = part.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4; 
            if (!indexData.TryGetValue(part.IndexBuffer, out var indexBufferData))
            {
                indexBufferData = new byte[part.IndexBuffer.IndexCount * indexStride];
                part.IndexBuffer.GetData(indexBufferData);
                indexData.Add(part.IndexBuffer, indexBufferData);
            }

            int offsetByStride = part.VertexOffset *
                                 part.VertexBuffer.VertexDeclaration.VertexStride;

            int numVerticesByStride = part.NumVertices *
                                      part.VertexBuffer.VertexDeclaration.VertexStride;

            CopyTo(bufferData.AsSpan().Slice(offsetByStride, numVerticesByStride),
                vertices.AsSpan().Slice(vertexOffset, part.NumVertices),
                transform * absoluteTransform, partVertexBuffer.VertexDeclaration);

            int currentIndexCount = part.PrimitiveCount * 3;

            if (largeIndices)
            {
                CopyIndexBuffer(part, indexBufferData.AsSpan(), 
                    MemoryMarshal.Cast<byte, uint>(indices.AsSpan())
                        .Slice(currentIndex, currentIndexCount),
                    vertexOffset);
            }
            else
            {
                CopyIndexBuffer(part, indexBufferData.AsSpan(), 
                    MemoryMarshal.Cast<byte, ushort>(indices.AsSpan())
                        .Slice(currentIndex, currentIndexCount),
                    vertexOffset);
            }

            currentIndex += currentIndexCount;
            vertexOffset += part.NumVertices;
        });
        
        vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorNormalTexture), vertices.Length, BufferUsage.None);
        vertexBuffer.SetData(vertices);
        
        indexBuffer = new IndexBuffer(device, 
            largeIndices ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits, 
            indexCount, BufferUsage.None);

        indexBuffer.SetData(indices);
    }

    private static uint GetMask(VertexElement[] elements)
    {
        uint mask = 0;

        for (int i = 0; i < elements.Length; i++)
        {
            mask |= (uint)(1 << (int)elements[i].VertexElementUsage);
        }

        return mask;
    }

    private static void Sum(Span<byte> data, VertexDeclaration declaration, ref Vector3 addedSum)
    {
        var positionElement = 
            declaration.GetVertexElements().First(e => e.VertexElementUsage == VertexElementUsage.Position);

        int dataOffset = 0;
        for (int i = 0; i < data.Length; i += declaration.VertexStride)
        {
            addedSum += MemoryMarshal.AsRef<Vector3>(data.Slice(dataOffset + positionElement.Offset));
        }
    }

    private static void CopyIndexBuffer<TDestinationType>(ModelMeshPart part, 
        ReadOnlySpan<byte> indexBufferData, Span<TDestinationType> destination, int vertexOffset)
        where TDestinationType : unmanaged, INumber<TDestinationType>
    {
        int indexStride = part.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4; 
        int currentIndexCount = part.PrimitiveCount * 3;

        int startIndexStride = part.StartIndex * indexStride;
        int countStride = currentIndexCount * indexStride;

        if (part.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
        {
            AddAndCopy(
                MemoryMarshal.Cast<byte, ushort>(indexBufferData.Slice(startIndexStride, countStride)), 
                destination, vertexOffset);    
        }
        else
        {
            AddAndCopy(
                MemoryMarshal.Cast<byte, uint>(indexBufferData.Slice(startIndexStride, countStride)), 
                destination, vertexOffset);    
        }
    }

    private static void AddAndCopy<TFrom, TTo>(ReadOnlySpan<TFrom> source,
        Span<TTo> destination, int vertexOffset)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, INumber<TTo>
    {
        if (vertexOffset == 0 && typeof(TFrom) == typeof(TTo))
        {
            MemoryMarshal.Cast<TFrom, TTo>(source).CopyTo(destination);
            return;
        }
        
        var offsetConverted = TTo.CreateChecked(vertexOffset);
        
        for (int i = 0; i < source.Length; i++)
        {
            var converted = TTo.CreateChecked(source[i]);
            destination[i] = converted + offsetConverted;
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
    
    private static void IterateMeshAndParts(Model model, Action<ModelMesh, ModelMeshPart> action)
    {
        foreach(var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                action(mesh, part);
            }
        }
    }
}