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
    public int X;
        
    /// <summary>
    /// The second component of the vector.
    /// </summary>
    public int Y;
        
    /// <summary>
    /// The third componentn of the vector.
    /// </summary>
    public int Z;

    /// <summary>
    /// Creates a vector setting each of its components to the value provided.
    /// </summary>
    public Vector3I(int value)
    {
        X = value;
        Y = value;
        Z = value;
    }
    
    /// <summary>
    /// Creates a vector setting each component.
    /// </summary>
    public Vector3I(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
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
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object obj)
    {
        return obj is Vector3I other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public override string ToString()
    {
        return $"{{{X}, {Y}, {Z}}}";
    }
}