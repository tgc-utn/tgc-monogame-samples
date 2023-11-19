namespace TGC.MonoGame.Samples.Collisions;

/// <summary>
///     Describes the type of intersection a <see cref="BoundingCylinder" /> and a <see cref="BoundingBox" /> had.
/// </summary>
public enum BoxCylinderIntersection
{
    ///<summary>The box touches the cylinder at an edge. Penetration is zero.</summary>
    Edge,

    ///<summary>The box touches the cylinder. Penetration is more than zero.</summary>
    Intersecting,

    ///<summary>The box and the cylinder do not intersect.</summary>
    None
}
