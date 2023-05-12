using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace TGC.MonoGame.Samples.Collisions
{
    /// <summary>
    ///     Represents an Oriented-BoundingBox (OBB).
    /// </summary>
    public class OrientedBoundingBox
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
        public OrientedBoundingBox() { }

        /// <summary>
        ///     Builds a Oriented Bounding-Box with a center and extents.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="extents"></param>
        public OrientedBoundingBox(Vector3 center, Vector3 extents)
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
        public void Rotate(Matrix rotation)
        {
            Orientation *= rotation;
        }

        /// <summary>
        ///     Rotate the OBB with a given Quaternion.
        ///     Note that this is a relative rotation.
        /// </summary>
        /// <param name="rotation">Rotation quaternion</param>
        public void Rotate(Quaternion rotation)
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
        private static OrientedBoundingBox ComputeFromPointsRecursive(Vector3[] points, Vector3 initValues, Vector3 endValues,
            float step)
        {
            var minObb = new OrientedBoundingBox();
            var minimumVolume = float.MaxValue;
            var minInitValues = Vector3.Zero;
            var minEndValues = Vector3.Zero;
            var transformedPoints = new Vector3[points.Length];
            float y, z;

            var x = initValues.X;
            while (x <= endValues.X)
            {
                y = initValues.Y;
                var rotationX = MathHelper.ToRadians(x);
                while (y <= endValues.Y)
                {
                    z = initValues.Z;
                    var rotationY = MathHelper.ToRadians(y);
                    while (z <= endValues.Z)
                    {
                        // Rotation matrix
                        var rotationZ = MathHelper.ToRadians(z);
                        var rotationMatrix = Matrix.CreateFromYawPitchRoll(rotationY, rotationX, rotationZ);

                        // Transform every point to OBB-Space
                        for (var index = 0; index < transformedPoints.Length; index++)
                            transformedPoints[index] = Vector3.Transform(points[index], rotationMatrix);

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
                minObb = ComputeFromPointsRecursive(points, minInitValues, minEndValues, step / 10f);

            return minObb;
        }

        /// <summary>
        ///     Creates an <see cref="OrientedBoundingBox">OrientedBoundingBox</see> from a <see cref="BoundingBox">BoundingBox</see>.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox">BoundingBox</see> to create the <see cref="OrientedBoundingBox">OrientedBoundingBox</see> from</param>
        /// <returns>The generated <see cref="OrientedBoundingBox">OrientedBoundingBox</see></returns>
        public static OrientedBoundingBox FromAABB(BoundingBox box)
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
        public Vector3 ToOBBSpace(Vector3 point)
        {
            var difference = point - Center;
            return Vector3.Transform(difference, Orientation);
        }


        /// <summary>
        ///     A helper method to create an array from a Vector3
        /// </summary>
        /// <param name="vector">The vector to create the array from</param>
        /// <returns>An array of length three with each position matching the Vector3 coordinates</returns>
        private float[] ToArray(Vector3 vector)
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
            var R = new float[3, 3];
            var AbsR = new float[3, 3];
            var ae = ToArray(Extents);
            var be = ToArray(box.Extents);

            // Compute rotation matrix expressing the other box in this box coordinate frame

            var result = ToFloatArray(Matrix.Multiply(Orientation, box.Orientation));

            for (var i = 0; i < 3; i++)
                 for (var j = 0; j < 3; j++)
                     R[i, j] = result[i * 3 + j];
            

            // Compute translation vector t
            var tVec = box.Center - Center;

            // Bring translation into this boxs coordinate frame

            var t = ToArray(Vector3.Transform(tVec, Orientation));

            // Compute common subexpressions. Add in an epsilon term to
            // counteract arithmetic errors when two edges are parallel and
            // their cross product is (near) null (see text for details)

            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                    AbsR[i, j] = MathF.Abs(R[i, j]) + float.Epsilon;

            // Test axes L = A0, L = A1, L = A2
            for (var i = 0; i < 3; i++)
            {
                ra = ae[i];
                rb = be[0] * AbsR[i, 0] + be[1] * AbsR[i, 1] + be[2] * AbsR[i, 2];
                if (MathF.Abs(t[i]) > ra + rb) return false;
            }

            // Test axes L = B0, L = B1, L = B2
            for (var i = 0; i < 3; i++)
            {
                ra = ae[0] * AbsR[0, i] + ae[1] * AbsR[1, i] + ae[2] * AbsR[2, i];
                rb = be[i];
                if (MathF.Abs(t[0] * R[0, i] + t[1] * R[1, i] + t[2] * R[2, i]) > ra + rb) return false;
            }

            // Test axis L = A0 x B0
            ra = ae[1] * AbsR[2, 0] + ae[2] * AbsR[1, 0];
            rb = be[1] * AbsR[0, 2] + be[2] * AbsR[0, 1];
            if (MathF.Abs(t[2] * R[1, 0] - t[1] * R[2, 0]) > ra + rb) return false;

            // Test axis L = A0 x B1
            ra = ae[1] * AbsR[2, 1] + ae[2] * AbsR[1, 1];
            rb = be[0] * AbsR[0, 2] + be[2] * AbsR[0, 0];
            if (MathF.Abs(t[2] * R[1, 1] - t[1] * R[2, 1]) > ra + rb) return false;

            // Test axis L = A0 x B2
            ra = ae[1] * AbsR[2, 2] + ae[2] * AbsR[1, 2];
            rb = be[0] * AbsR[0, 1] + be[1] * AbsR[0, 0];
            if (MathF.Abs(t[2] * R[1, 2] - t[1] * R[2, 2]) > ra + rb) return false;

            // Test axis L = A1 x B0
            ra = ae[0] * AbsR[2, 0] + ae[2] * AbsR[0, 0];
            rb = be[1] * AbsR[1, 2] + be[2] * AbsR[1, 1];
            if (MathF.Abs(t[0] * R[2, 0] - t[2] * R[0, 0]) > ra + rb) return false;

            // Test axis L = A1 x B1
            ra = ae[0] * AbsR[2, 1] + ae[2] * AbsR[0, 1];
            rb = be[0] * AbsR[1, 2] + be[2] * AbsR[1, 0];
            if (MathF.Abs(t[0] * R[2, 1] - t[2] * R[0, 1]) > ra + rb) return false;

            // Test axis L = A1 x B2
            ra = ae[0] * AbsR[2, 2] + ae[2] * AbsR[0, 2];
            rb = be[0] * AbsR[1, 1] + be[1] * AbsR[1, 0];
            if (MathF.Abs(t[0] * R[2, 2] - t[2] * R[0, 2]) > ra + rb) return false;

            // Test axis L = A2 x B0
            ra = ae[0] * AbsR[1, 0] + ae[1] * AbsR[0, 0];
            rb = be[1] * AbsR[2, 2] + be[2] * AbsR[2, 1];
            if (MathF.Abs(t[1] * R[0, 0] - t[0] * R[1, 0]) > ra + rb) return false;

            // Test axis L = A2 x B1
            ra = ae[0] * AbsR[1, 1] + ae[1] * AbsR[0, 1];
            rb = be[0] * AbsR[2, 2] + be[2] * AbsR[2, 0];
            if (MathF.Abs(t[1] * R[0, 1] - t[0] * R[1, 1]) > ra + rb) return false;

            // Test axis L = A2 x B2
            ra = ae[0] * AbsR[1, 2] + ae[1] * AbsR[0, 2];
            rb = be[0] * AbsR[2, 1] + be[1] * AbsR[2, 0];
            if (MathF.Abs(t[1] * R[0, 2] - t[0] * R[1, 2]) > ra + rb) return false;

            // Since no separating axis is found, the OBBs must be intersecting
            return true;
        }

        /// <summary>
        ///     Tests if this OBB intersects with another AABB.
        /// </summary>
        /// <param name="aabb">The other AABB to test</param>
        /// <returns>True if the two boxes intersect</returns>
        public bool Intersects(in BoundingBox aabb) 
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

            for (int i = 0; i < 15; ++i)
            {
                if (!OverlapOnAxis(aabb, test[i])) 
                    // Separating axis found
                    return false; 
            }

            return true;
        }

        /// <summary>
        ///     Tests if this OBB intersects with another AABB.
        ///     Informs about the normal (pointing out from the OBB) and penetration distance between the two volumes.
        /// </summary>
        /// <param name="aabb">The other AABB to test</param>
        /// <param name="normal">The normal vector of the intersection coming out from the OBB</param>
        /// <param name="penetration">The penetration distance between two vectors</param>
        /// <returns>True if the two boxes intersect</returns>
        public bool Intersects(in BoundingBox aabb, out Vector3 normal, out float penetration) 
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

            penetration = float.MaxValue;
            normal = Vector3.Zero;
            
            for (int i = 0; i < 15; ++i)
            {
                if (!OverlapOnAxis(aabb, test[i], out bool shouldFlip, out float currentPenetration)) 
                    return false; 

                if (currentPenetration > 0f && currentPenetration < penetration) 
                {
                    penetration = currentPenetration;
                    normal = test[i] * (shouldFlip ? -1f : 1f);
                }
            }

            return true;
        }

        bool OverlapOnAxis(in BoundingBox aabb, Vector3 axis) 
        {
            Interval a = GetInterval(aabb, axis);
            Interval b = GetInterval(axis);
            
            return ((b.Min <= a.Max) && (a.Min <= b.Max));
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
        
        Interval GetInterval(Vector3 axis) 
        {
            var vertex = new Vector3[8];

            Vector3 C = Center;	// OBB Center
            Vector3 E = Extents;		// OBB Extents
            
            Vector3[] A = 
            {			// OBB Axis
                Orientation.Right,
                Orientation.Up,
                Orientation.Forward,
            };

            vertex[0] = C + A[0] * E.X + A[1] * E.Y + A[2] * E.Z;
            vertex[1] = C - A[0] * E.X + A[1] * E.Y + A[2] * E.Z;
            vertex[2] = C + A[0] * E.X - A[1] * E.Y + A[2] * E.Z;
            vertex[3] = C + A[0] * E.X + A[1] * E.Y - A[2] * E.Z;
            vertex[4] = C - A[0] * E.X - A[1] * E.Y - A[2] * E.Z;
            vertex[5] = C + A[0] * E.X - A[1] * E.Y - A[2] * E.Z;
            vertex[6] = C - A[0] * E.X + A[1] * E.Y - A[2] * E.Z;
            vertex[7] = C - A[0] * E.X - A[1] * E.Y + A[2] * E.Z;

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

        Interval GetInterval(in BoundingBox aabb, Vector3 axis)
        {
            Vector3 i = aabb.Min;
            Vector3 a = aabb.Max;

            Vector3[] vertex = 
            {
                new Vector3(i.X, a.Y, a.Z),
                new Vector3(i.X, a.Y, i.Z),
                new Vector3(i.X, i.Y, a.Z),
                new Vector3(i.X, i.Y, i.Z),
                new Vector3(a.X, a.Y, a.Z),
                new Vector3(a.X, a.Y, i.Z),
                new Vector3(a.X, i.Y, a.Z),
                new Vector3(a.X, i.Y, i.Z)
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
        
        struct Interval
        {
            public float Min;
            public float Max;
        } 

        /// <summary>
        ///     Tests if this OBB intersects with a Ray.
        /// </summary>
        /// <param name="ray">The ray to test</param>
        /// <param name="result">The length in the ray direction from the ray origin</param>
        /// <returns>True if the OBB intersects the Ray</returns>
        public bool Intersects(Ray ray, out float? result)
        {
            //Transform Ray to OBB-Space
            var rayOrigin = ray.Position;
            var rayDestination = rayOrigin + ray.Direction;


            var rayOriginInOBBSpace = ToOBBSpace(rayOrigin);
            var rayDestinationInOBBSpace = ToOBBSpace(rayDestination);

            var rayInOBBSpace = new Ray(rayOriginInOBBSpace, Vector3.Normalize(rayDestinationInOBBSpace - rayOriginInOBBSpace));

            // Create an AABB that encloses OBB
            var enclosingBox = new BoundingBox(-Extents, Extents);

            // Perform Ray-AABB intersection
            var testResult = enclosingBox.Intersects(rayInOBBSpace);
            result = testResult;

            return testResult != null;
        }


        /// <summary>
        ///     Tests if this OBB intersects with a Sphere.
        /// </summary>
        /// <param name="sphere">The sphere to test</param>
        /// <returns>True if the OBB intersects the Sphere</returns>
        public bool Intersects(BoundingSphere sphere)
        {
            // Transform sphere to OBB-Space
            var obbSpaceSphere = new BoundingSphere(ToOBBSpace(sphere.Center), sphere.Radius);

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
                return PlaneIntersectionType.Intersecting;
            else if (d < 0.0f)
                return PlaneIntersectionType.Front;
            else
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
                    return false;
            }
            return true;
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

    }
    
}
