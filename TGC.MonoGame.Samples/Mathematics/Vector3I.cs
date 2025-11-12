using System;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Mathematics;

/// <summary>
/// Describes a 3D-vector on integer space.
/// </summary>
public struct Vector3I : IEquatable<Vector3I>
{
    /// <summary>
    /// The first component of the vector.
    /// </summary>
    private int x;

    /// <summary>
    /// The second component of the vector.
    /// </summary>
    private int y;

    /// <summary>
    /// The third componentn of the vector.
    /// </summary>
    private int z;

    /// <summary>
    /// Gets or sets the first component of the vector.
    /// </summary>
    public int X
    {
        get => x;
        set => x = value;
    }

    /// <summary>
    /// Gets or sets the second component of the vector.
    /// </summary>
    public int Y
    {
        get => y;
        set => y = value;
    }

    /// <summary>
    /// Gets or sets the third component of the vector.
    /// </summary>
    public int Z
    {
        get => z;
        set => z = value;
    }

    /// <summary>
    /// Creates a vector setting each of its components to the value provided.
    /// </summary>
    public Vector3I(int value)
    {
        x = value;
        y = value;
        z = value;
    }

    /// <summary>
    /// Creates a vector setting each component.
    /// </summary>
    public Vector3I(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    /// <summary>
    /// Converter for <see cref="Vector3"/>, casting each component to int.
    /// </summary>
    public static explicit operator Vector3I(Vector3 vector)
    {
        return new Vector3I((int)vector.X, (int)vector.Y, (int)vector.Z);
    }

    public bool Equals(Vector3I other)
    {
        return x == other.x && y == other.y && z == other.z;
    }

    public override bool Equals(object obj)
    {
        return obj is Vector3I other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z);
    }

    public override string ToString()
    {
        return $"{{{X}, {Y}, {Z}}}";
    }
}