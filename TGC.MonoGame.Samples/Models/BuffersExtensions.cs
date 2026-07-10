using System;
using System.Numerics;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TGC.MonoGame.Samples.Models;

/// <summary>
/// Provides utilities to copy and transform <see cref="VertexBuffer"/>s and <see cref="IndexBuffer"/>s.
/// </summary>
public static class BuffersExtensions
{
    internal static void CopyIndexBuffer<TDestinationType>(
        ModelMeshPart part,
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

    private static void AddAndCopy<TFrom, TTo>(
        ReadOnlySpan<TFrom> source,
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

    internal static void CopyTo(Span<byte> from, Span<VertexPositionColorNormalTexture> destination, in Matrix matrix, VertexDeclaration declaration)
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

    private static uint GetMask(VertexElement[] elements)
    {
        uint mask = 0;

        foreach (var element in elements)
        {
            mask |= (uint)(1 << (int)element.VertexElementUsage);
        }

        return mask;
    }
}
