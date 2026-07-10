using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Models;

/// <summary>
/// Data associated to a geometry, generally extracted from a MonoGame <see cref="Model"/>.
/// Represents an instance to be drawn with special custom data that a mesh contained.
/// </summary>
/// <param name="relativeMatrix">A matrix that. </param>
/// <param name="textures"></param>
public struct GeometryData(Geometry geometry, Matrix relativeMatrix, Texture[] textures)
{
    /// <summary>
    /// The geometry extracted from a model. Contains methods to be drawn with an <see cref="Effect"/>.
    /// </summary>
    public Geometry Geometry { get; private set; } = geometry;

    /// <summary>
    /// A matrix that was used to place the geometry in the center of the model.
    /// </summary>
    public Matrix RelativeMatrix { get; private set; } = relativeMatrix;

    /// <summary>
    /// A collection of textures associated to the geometry inside the model.
    /// </summary>
    public Texture[] Textures { get; private set; } = textures;
}
