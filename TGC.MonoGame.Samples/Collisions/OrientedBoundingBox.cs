using Microsoft.Xna.Framework;
using System;

namespace TGC.MonoGame.Samples.Collisions;

/// <summary>
///     Represents an Oriented-BoundingBox (OBB).
/// </summary>
public struct OrientedBoundingBox
{
    /// <summary>
    ///     Center.
    /// </summary>
    public Vector3 Center { get; set; }

    /// <summary>
    ///     Orientation 
    /// </summary>
    public Matrix Orientation { get; set; }

    /// <summary>
    ///     Extents
    /// </summary>
    public Vector3 Extents { get; set; }

    /// <summary>
    ///     Builds an empty Bounding Oriented Box.
    /// </summary>
    public OrientedBoundingBox()
    {
        Center = Vector3.Zero;
        Orientation = Matrix.Identity;
        Extents = Vector3.Zero;
    }

    /// <summary>
    ///     Builds a Oriented Bounding-Box with a center and extents.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="extents"></param>
    public OrientedBoundingBox(in Vector3 center, in Vector3 extents)
    {
        Center = center;
        Extents = extents;
        Orientation = Matrix.Identity;
    }

    /// <summary>
    ///     Rotate the OBB with a given Matrix.
    ///     Note that this is a relative rotation.
    /// </summary>
    /// <param name="rotation">Rotation matrix</param>
    public void Rotate(in Matrix rotation)
    {
        Orientation *= rotation;
    }

    /// <summary>
    ///     Rotate the OBB with a given Quaternion.
    ///     Note that this is a relative rotation.
    /// </summary>
    /// <param name="rotation">Rotation quaternion</param>
    public void Rotate(in Quaternion rotation)
    {
        Rotate(Matrix.CreateFromQuaternion(rotation));
    }


    /// <summary>
    ///     Creates an OBB from a given set of points.
    ///     Searches for the best OBB orientation that matches the points.
    ///     Note that it is an expensive operation.
    /// </summary>
    /// <param name="points">An array of points in World Space</param>
    /// <returns>A generated Oriented Bounding Box that contains the set of points</returns>
    public static OrientedBoundingBox ComputeFromPoints(Vector3[] points)
    {
        return ComputeFromPointsRecursive(points, Vector3.Zero, new Vector3(360, 360, 360), 10f);
    }


    /// <summary>
    ///     Calculates an OBB with a given set of points.
    ///     Tests every orientations between initValues and endValues, stepping through angle intervals with a given step size.
    ///     Goes on until it reaches a step less than 0.01
    /// </summary>
    /// <returns>A generated Oriented Bounding Box that contains the set of points</returns>
    private static OrientedBoundingBox ComputeFromPointsRecursive(Vector3[] points, in Vector3 initValues, in Vector3 endValues,
        float step)
    {
        var minObb = new OrientedBoundingBox();
        var minimumVolume = float.MaxValue;
        var minInitValues = Vector3.Zero;
        var minEndValues = Vector3.Zero;
        var transformedPoints = new Vector3[points.Length];

        var x = initValues.X;
        while (x <= endValues.X)
        {
            float y = initValues.Y;
            float rotationX = MathHelper.ToRadians(x);
            while (y <= endValues.Y)
            {
                float z = initValues.Z;
                float rotationY = MathHelper.ToRadians(y);
                while (z <= endValues.Z)
                {
                    // Rotation matrix
                    float rotationZ = MathHelper.ToRadians(z);
                    var rotationMatrix = Matrix.CreateFromYawPitchRoll(rotationY, rotationX, rotationZ);

                    // Transform every point to OBB-Space
                    for (var index = 0; index < transformedPoints.Length; index++)
                    {
                        transformedPoints[index] = Vector3.Transform(points[index], rotationMatrix);
                    }

                    // Obtain an AABB enclosing every transformed point
                    var aabb = BoundingBox.CreateFromPoints(transformedPoints);

                    // Calculate the volume of the AABB
                    var volume = BoundingVolumesExtensions.GetVolume(aabb);

                    // Find lesser volume
                    if (volume < minimumVolume)
                    {
                        minimumVolume = volume;
                        minInitValues = new Vector3(x, y, z);
                        minEndValues = new Vector3(x + step, y + step, z + step);

                        // Restore the AABB center in World-Space
                        var center = BoundingVolumesExtensions.GetCenter(aabb);
                        center = Vector3.Transform(center, rotationMatrix);

                        // Create OBB
                        minObb = new OrientedBoundingBox(center, BoundingVolumesExtensions.GetExtents(aabb));
                        minObb.Orientation = rotationMatrix;
                    }

                    z += step;
                }
                y += step;
            }
            x += step;
        }

        // Loop again if the step is higher than a given acceptance threshold
        if (step > 0.01f)
        {
            minObb = ComputeFromPointsRecursive(points, minInitValues, minEndValues, step / 10f);
        }

        return minObb;
    }

    /// <summary>
    ///     Creates an <see cref="OrientedBoundingBox">OrientedBoundingBox</see> from a <see cref="BoundingBox">BoundingBox</see>.
    /// </summary>
    /// <param name="box">A <see cref="BoundingBox">BoundingBox</see> to create the <see cref="OrientedBoundingBox">OrientedBoundingBox</see> from</param>
    /// <returns>The generated <see cref="OrientedBoundingBox">OrientedBoundingBox</see></returns>
    public static OrientedBoundingBox FromAabb(in BoundingBox box)
    {
        var center = BoundingVolumesExtensions.GetCenter(box);
        var extents = BoundingVolumesExtensions.GetExtents(box);
        return new OrientedBoundingBox(center, extents);
    }

    /// <summary>
    ///     Converts a point from World-Space to OBB-Space.
    /// </summary>
    /// <param name="point">Point in World-Space</param>
    /// <returns>The point in OBB-Space</returns>
    public Vector3 ToObbSpace(in Vector3 point)
    {
        var difference = point - Center;
        return Vector3.Transform(difference, Orientation);
    }


    /// <summary>
    ///     A helper method to create an array from a Vector3
    /// </summary>
    /// <param name="vector">The vector to create the array from</param>
    /// <returns>An array of length three with each position matching the Vector3 coordinates</returns>
    private float[] ToArray(in Vector3 vector)
    {
        return new[] { vector.X, vector.Y, vector.Z };
    }



    /// <summary>
    ///     A helper method to create an array from a 3x3 Matrix
    /// </summary>
    /// <param name="matrix">A 3x3 Matrix to create the array from</param>
    /// <returns>An array of length nine with each position matching the matrix elements</returns>
    private float[] ToFloatArray(Matrix matrix)
    {
        return new[]
        {
            matrix.M11, matrix.M21, matrix.M31,
            matrix.M12, matrix.M22, matrix.M32,
            matrix.M13, matrix.M23, matrix.M33,
        };
    }

    /// <summary>
    ///     Tests if this OBB intersects with another OBB.
    /// </summary>
    /// <param name="box">The other OBB to test</param>
    /// <returns>True if the two boxes intersect</returns>
    public bool Intersects(OrientedBoundingBox box)
    {
        float ra;
        float rb;
        var rotation = new float[9];
        var absoluteRotation = new float[9];
        var ae = ToArray(Extents);
        var be = ToArray(box.Extents);

        // Compute rotation matrix expressing the other box in this box coordinate frame

        var result = ToFloatArray(Matrix.Multiply(Orientation, box.Orientation));

        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                GetRotationAt(i, j) = result[i * 3 + j];
            }
        }

        // Compute translation vector t
        var tVec = box.Center - Center;

        // Bring translation into this box coordinate frame
        var translation = ToArray(Vector3.Transform(tVec, Orientation));

        // Compute common subexpressions. Add in an epsilon term to
        // counteract arithmetic errors when two edges are parallel and
        // their cross product is (near) null (see text for details)

        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                GetAbsoluteRotationAt(i, j) = MathF.Abs(GetRotationAt(i, j)) + float.Epsilon;
            }
        }

        // Test axes L = A0, L = A1, L = A2
        for (var i = 0; i < 3; i++)
        {
            ra = ae[i];
            rb = be[0] * GetAbsoluteRotationAt(i, 0) + be[1] * GetAbsoluteRotationAt(i, 1) + be[2] * GetAbsoluteRotationAt(i, 2);
            
            if (MathF.Abs(translation[i]) > ra + rb)
            {
                return false;
            }
        }

        // Test axes L = B0, L = B1, L = B2
        for (var i = 0; i < 3; i++)
        {
            ra = ae[0] * GetAbsoluteRotationAt(0, i) + ae[1] * GetAbsoluteRotationAt(1, i) + ae[2] * GetAbsoluteRotationAt(2, i);
            rb = be[i];
            if (MathF.Abs(translation[0] * GetRotationAt(0, i) + translation[1] * GetRotationAt(1, i) + translation[2] * GetRotationAt(2, i)) > ra + rb)
            {
                return false;
            }
        }

        // Test axis L = A0 x B0
        ra = ae[1] * GetAbsoluteRotationAt(2, 0) + ae[2] * GetAbsoluteRotationAt(1, 0);
        rb = be[1] * GetAbsoluteRotationAt(0, 2) + be[2] * GetAbsoluteRotationAt(0, 1);
        
        if (MathF.Abs(translation[2] * GetRotationAt(1, 0) - translation[1] * GetRotationAt(2, 0)) > ra + rb)
        {
            return false;
        }

        // Test axis L = A0 x B1
        ra = ae[1] * GetAbsoluteRotationAt(2, 1) + ae[2] * GetAbsoluteRotationAt(1, 1);
        rb = be[0] * GetAbsoluteRotationAt(0, 2) + be[2] * GetAbsoluteRotationAt(0, 0);
        
        if (MathF.Abs(translation[2] * GetRotationAt(1, 1) - translation[1] * GetRotationAt(2, 1)) > ra + rb)
        {
            return false;
        }

        // Test axis L = A0 x B2
        ra = ae[1] * GetAbsoluteRotationAt(2, 2) + ae[2] * GetAbsoluteRotationAt(1, 2);
        rb = be[0] * GetAbsoluteRotationAt(0, 1) + be[1] * GetAbsoluteRotationAt(0, 0);
        
        if (MathF.Abs(translation[2] * GetRotationAt(1, 2) - translation[1] * GetRotationAt(2, 2)) > ra + rb)
        {
            return false;
        }

        // Test axis L = A1 x B0
        ra = ae[0] * GetAbsoluteRotationAt(2, 0) + ae[2] * GetAbsoluteRotationAt(0, 0);
        rb = be[1] * GetAbsoluteRotationAt(1, 2) + be[2] * GetAbsoluteRotationAt(1, 1);

        if (MathF.Abs(translation[0] * GetRotationAt(2, 0) - translation[2] * GetRotationAt(0, 0)) > ra + rb)
        {
            return false;
        }

        // Test axis L = A1 x B1
        ra = ae[0] * GetAbsoluteRotationAt(2, 1) + ae[2] * GetAbsoluteRotationAt(0, 1);
        rb = be[0] * GetAbsoluteRotationAt(1, 2) + be[2] * GetAbsoluteRotationAt(1, 0);
        
        if (MathF.Abs(translation[0] * GetRotationAt(2, 1) - translation[2] * GetRotationAt(0, 1)) > ra + rb)
        {
            return false;
        }

        // Test axis L = A1 x B2
        ra = ae[0] * GetAbsoluteRotationAt(2, 2) + ae[2] * GetAbsoluteRotationAt(0, 2);
        rb = be[0] * GetAbsoluteRotationAt(1, 1) + be[1] * GetAbsoluteRotationAt(1, 0);
        
        if (MathF.Abs(translation[0] * GetRotationAt(2, 2) - translation[2] * GetRotationAt(0, 2)) > ra + rb)
        {
            return false;
        }

        // Test axis L = A2 x B0
        ra = ae[0] * GetAbsoluteRotationAt(1, 0) + ae[1] * GetAbsoluteRotationAt(0, 0);
        rb = be[1] * GetAbsoluteRotationAt(2, 2) + be[2] * GetAbsoluteRotationAt(2, 1);
        
        if (MathF.Abs(translation[1] * GetRotationAt(0, 0) - translation[0] * GetRotationAt(1, 0)) > ra + rb)
        {
            return false;
        }

        // Test axis L = A2 x B1
        ra = ae[0] * GetAbsoluteRotationAt(1, 1) + ae[1] * GetAbsoluteRotationAt(0, 1);
        rb = be[0] * GetAbsoluteRotationAt(2, 2) + be[2] * GetAbsoluteRotationAt(2, 0);
        
        if (MathF.Abs(translation[1] * GetRotationAt(0, 1) - translation[0] * GetRotationAt(1, 1)) > ra + rb)
        {
            return false;
        }

        // Test axis L = A2 x B2
        ra = ae[0] * GetAbsoluteRotationAt(1, 2) + ae[1] * GetAbsoluteRotationAt(0, 2);
        rb = be[0] * GetAbsoluteRotationAt(2, 1) + be[1] * GetAbsoluteRotationAt(2, 0);

        if (MathF.Abs(translation[1] * GetRotationAt(0, 2) - translation[0] * GetRotationAt(1, 2)) > ra + rb)
        {
            return false;
        }

        // Since no separating axis is found, the OBBs must be intersecting
        return true;

        ref float GetRotationAt(int x, int y)
        {
            return ref rotation[x * 3 + y];
        }
        
        ref float GetAbsoluteRotationAt(int x, int y)
        {
            return ref absoluteRotation[x * 3 + y];
        }
    }

    /// <summary>
    ///     Tests if this OBB intersects with another AABB.
    /// </summary>
    /// <param name="aabb">The other AABB to test</param>
    /// <returns>True if the two boxes intersect</returns>
    public bool Intersects(in BoundingBox aabb) 
    {
        Vector3[] test = new Vector3[15];

        test[0] = Vector3.UnitX;
        test[1] = Vector3.UnitY;
        test[2] = Vector3.UnitZ;
        test[3] = Orientation.Right;
        test[4] = Orientation.Up;
        test[5] = Orientation.Forward;

        for (int i = 0; i < 3; ++i) 
        { 
            // Fill out rest of axis
            test[6 + i * 3 + 0] = Vector3.Cross(test[i], test[0]);
            test[6 + i * 3 + 1] = Vector3.Cross(test[i], test[1]);
            test[6 + i * 3 + 2] = Vector3.Cross(test[i], test[2]);
        }

        for (int i = 0; i < 15; ++i)
        {
            if (!OverlapOnAxis(aabb, test[i]))
            {
                // Separating axis found
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Tests if this OBB intersects with another AABB.
    ///     Informs about the normal (pointing out from the OBB) and penetration distance between the two volumes.
    ///     Normal and penetration on the result are only valid if there's an intersection.
    /// </summary>
    /// <param name="aabb">The other AABB to test</param>
    /// <param name="result">The result containing information about the intersection. Values are only valid if
    /// <see cref="IntersectionResult.Intersects"/> is true</param>
    public void Intersects(in BoundingBox aabb, ref IntersectionResult result) 
    {
        Vector3[] test = new Vector3[15];

        test[0] = new Vector3(1, 0, 0);
        test[1] = new Vector3(0, 1, 0);
        test[2] = new Vector3(0, 0, 1);
        test[3] = Orientation.Right;
        test[4] = Orientation.Up;
        test[5] = Orientation.Forward;

        for (int i = 0; i < 3; ++i) 
        { 
            // Fill out rest of axis
            test[6 + i * 3 + 0] = Vector3.Cross(test[i], test[0]);
            test[6 + i * 3 + 1] = Vector3.Cross(test[i], test[1]);
            test[6 + i * 3 + 2] = Vector3.Cross(test[i], test[2]);
        }

        result = new IntersectionResult(false, Vector3.Zero, float.MaxValue);
        
        for (int i = 0; i < 15; ++i)
        {
            if (!OverlapOnAxis(aabb, test[i], out bool shouldFlip, out float currentPenetration))
            {
                return;
            } 

            if (currentPenetration > 0f && currentPenetration < result.Penetration) 
            {
                result.Penetration = currentPenetration;
                result.Normal = test[i] * (shouldFlip ? -1f : 1f);
            }
        }

        result.Intersects = true;
    }

    /// <summary>
    ///     Tests if this OBB intersects with a Ray.
    /// </summary>
    /// <param name="ray">The ray to test</param>
    /// <returns>A result containing the OBB intersection with the Ray</returns>
    public RayCastResult Intersects(Ray ray)
    {
        //Transform Ray to OBB-Space
        var rayOrigin = ray.Position;
        var rayDestination = rayOrigin + ray.Direction;

        var rayOriginInObbSpace = ToObbSpace(rayOrigin);
        var rayDestinationInObbSpace = ToObbSpace(rayDestination);

        var rayInObbSpace = new Ray(rayOriginInObbSpace, Vector3.Normalize(rayDestinationInObbSpace - rayOriginInObbSpace));

        // Create an AABB that encloses OBB
        var enclosingBox = new BoundingBox(-Extents, Extents);

        // Perform Ray-AABB intersection
        float? testResult = enclosingBox.Intersects(rayInObbSpace);

        if (testResult == null)
        {
            return new RayCastResult(false, float.MaxValue);
        }

        return new RayCastResult(true, testResult.Value);
    }


    /// <summary>
    ///     Tests if this OBB intersects with a Sphere.
    /// </summary>
    /// <param name="sphere">The sphere to test</param>
    /// <returns>True if the OBB intersects the Sphere</returns>
    public bool Intersects(BoundingSphere sphere)
    {
        // Transform sphere to OBB-Space
        var obbSpaceSphere = new BoundingSphere(ToObbSpace(sphere.Center), sphere.Radius);

        // Create AABB enclosing the OBB
        var aabb = new BoundingBox(-Extents, Extents);

        return aabb.Intersects(obbSpaceSphere);
    }

    /// <summary>
    ///     Tests the intersection between the OBB and a Plane.
    /// </summary>
    /// <param name="plane">The plane to test</param>
    /// <returns>Front if the OBB is in front of the plane, back if it is behind, and intersecting if it intersects with the plane</returns>
    public PlaneIntersectionType Intersects(Plane plane)
    {
        // Maximum extent in direction of plane normal 
        var normal = Vector3.Transform(plane.Normal, Orientation);

        // Maximum extent in direction of plane normal 
        var r = MathF.Abs(Extents.X * normal.X)
            + MathF.Abs(Extents.Y * normal.Y)
            + MathF.Abs(Extents.Z * normal.Z);

        // signed distance between box center and plane
        var d = Vector3.Dot(plane.Normal, Center) + plane.D;


        // Return signed distance
        if (MathF.Abs(d) < r)
        {
            return PlaneIntersectionType.Intersecting;
        }

        if (d < 0.0f)
        {
            return PlaneIntersectionType.Front;
        }

        return PlaneIntersectionType.Back;
    }

    /// <summary>
    ///     Tests the intersection between the OBB and a Frustum.
    /// </summary>
    /// <param name="frustum">The frustum to test</param>
    /// <returns>True if the OBB intersects with the Frustum, false otherwise</returns>
    public bool Intersects(BoundingFrustum frustum)
    {
        var planes = new[]
        {
            frustum.Left,
            frustum.Right,
            frustum.Far,
            frustum.Near,
            frustum.Bottom,
            frustum.Top
        };

        for (var faceIndex = 0; faceIndex < 6; ++faceIndex)
        {
            var side = Intersects(planes[faceIndex]);
            if (side == PlaneIntersectionType.Back)
            {
                return false;
            }
        }
        return true;
    }
    
    bool OverlapOnAxis(in BoundingBox aabb, in Vector3 axis) 
    {
        Interval a = GetInterval(aabb, axis);
        Interval b = GetInterval(axis);
        
        return b.Min <= a.Max && a.Min <= b.Max;
    }

    bool OverlapOnAxis(in BoundingBox aabb, Vector3 axis, out bool outShouldFlip, out float penetration) 
    {
        Interval a = GetInterval(aabb, axis);
        Interval b = GetInterval(axis);

        if (!((b.Min <= a.Max) && (a.Min <= b.Max)))
        {
            outShouldFlip = false;
            penetration = 0f;
            return false;
        }
        
        float len1 = a.Max - a.Min;
        float len2 = b.Max - b.Min;
        float min = MathF.Min(a.Min, b.Min);
        float max = MathF.Max(a.Max, b.Max);
        float length = max - min;

        outShouldFlip = (b.Min < a.Min);
        
        penetration = (len1 + len2) - length;
        
        return true;
    }
    
    Interval GetInterval(in Vector3 axis) 
    {
        var vertex = new Vector3[8];

        Vector3[] a =
        {
            // OBB Axis
            Orientation.Right,
            Orientation.Up,
            Orientation.Forward
        };

        vertex[0] = Center + a[0] * Extents.X + a[1] * Extents.Y + a[2] * Extents.Z;
        vertex[1] = Center - a[0] * Extents.X + a[1] * Extents.Y + a[2] * Extents.Z;
        vertex[2] = Center + a[0] * Extents.X - a[1] * Extents.Y + a[2] * Extents.Z;
        vertex[3] = Center + a[0] * Extents.X + a[1] * Extents.Y - a[2] * Extents.Z;
        vertex[4] = Center - a[0] * Extents.X - a[1] * Extents.Y - a[2] * Extents.Z;
        vertex[5] = Center + a[0] * Extents.X - a[1] * Extents.Y - a[2] * Extents.Z;
        vertex[6] = Center - a[0] * Extents.X + a[1] * Extents.Y - a[2] * Extents.Z;
        vertex[7] = Center - a[0] * Extents.X - a[1] * Extents.Y + a[2] * Extents.Z;

        Interval result;
        result.Min = result.Max = Vector3.Dot(axis, vertex[0]);

        for (int i = 1; i < 8; ++i) 
        {
            float projection = Vector3.Dot(axis, vertex[i]);
            result.Min = (projection < result.Min) ? projection : result.Min;
            result.Max = (projection > result.Max) ? projection : result.Max;
        }

        return result;
    }

    Interval GetInterval(in BoundingBox aabb, in Vector3 axis)
    {
        Vector3 i = aabb.Min;
        Vector3 a = aabb.Max;

        Vector3[] vertex = 
        {
            new (i.X, a.Y, a.Z),
            new (i.X, a.Y, i.Z),
            new (i.X, i.Y, a.Z),
            new (i.X, i.Y, i.Z),
            new (a.X, a.Y, a.Z),
            new (a.X, a.Y, i.Z),
            new (a.X, i.Y, a.Z),
            new (a.X, i.Y, i.Z)
        };

        Interval result;
        result.Min = result.Max = Vector3.Dot(axis, vertex[0]);

        for (int ij = 1; ij < 8; ++ij)
        {
            float projection = Vector3.Dot(axis, vertex[ij]);
            result.Min = (projection < result.Min) ? projection : result.Min;
            result.Max = (projection > result.Max) ? projection : result.Max;
        }

        return result;
    }
    
    /// <summary>
    ///     Converts a point from OBB-Space to World-Space.
    /// </summary>
    /// <param name="point">Point in OBB-Space</param>
    /// <returns>The point in World-Space</returns>
    public Vector3 ToWorldSpace(Vector3 point)
    {
        return Center + Vector3.Transform(point, Orientation);
    }

    public struct IntersectionResult
    {
        private bool _intersects;
        private Vector3 _normal;
        private float _penetration;
        
        public bool Intersects
        {
            get => _intersects;
            internal set => _intersects = value;
        }
        
        public Vector3 Normal
        {
            get => _normal;
            internal set => _normal = value;
        }
        
        public float Penetration
        {
            get => _penetration;
            internal set => _penetration = value;
        }

        public IntersectionResult(bool intersects, Vector3 normal, float penetration)
        {
            _intersects = intersects;
            _normal = normal;
            _penetration = penetration;
        }
    }

    public struct RayCastResult
    {
        private bool _hasHit;
        
        private float _fraction;

        public bool HasHit => _hasHit;
        
        public float Fraction => _fraction;
        
        public RayCastResult(bool hasHit, float fraction)
        {
            _hasHit = hasHit;
            _fraction = fraction;
        }
    }
    
    
    /// <summary>
    /// Represents a (min, max) range.
    /// </summary>
    private struct Interval
    {
        public float Min;
        public float Max;
    } 
}

