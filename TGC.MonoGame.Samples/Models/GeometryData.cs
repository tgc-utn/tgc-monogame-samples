using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Models;

public struct GeometryData(Geometry geometry, Matrix relativeMatrix, Texture[] textures)
{
    public Geometry Geometry { get; private set; } = geometry;

    public Matrix RelativeMatrix { get; private set; } = relativeMatrix;

    public Texture[] Textures { get; private set; } = textures;
}