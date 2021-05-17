using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Collisions
{

    /// <summary>
    ///     Represents a Bounding Cylinder to test for intersections
    /// </summary>
    class BoundingCylinder
    {
        // The center of the Cylinder in World Space
        private Vector3 _center;

        // The radius of the Cylinder
        private float _radius;

        // The distance from the Cylinder center to either its top or bottom
        private float _halfHeight;

        // A matrix describing the Cylinder rotation
        private Matrix _rotation;


        // Matrices
        // These are used to test for intersections, and serve to transform points to the Cylinder space or vice versa


        // The opposite of the translation matrix, to calculate the InverseTransform
        private Matrix _oppositeTranslation;

        // The inverse of the rotation matrix, to calculate the InverseTransform
        private Matrix _inverseRotation;

        // An internal matrix to transform points from Cylinder space back to local space
        private Matrix _inverseTransform;

        // An internal scale matrix to calculate the final Transform matrix
        private Matrix _scale;

        // An internal translation matrix to calculate the final Transform matrix
        private Matrix _translation;


        /// <summary>
        ///     The center of the Cylinder in World Space
        /// </summary>
        public Vector3 Center
        {
            get => _center;
            set
            {
                _center = value;
                UpdateTranslation();
                UpdateInverseRotation();
                UpdateTransform();
            }
        }

        /// <summary>
        ///     The radius of the Cylinder
        /// </summary>
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                UpdateScale();
                UpdateTransform();
            }
        }

        /// <summary>
        ///     The distance from the Cylinder center to either its top or bottom
        /// </summary>
        public float HalfHeight
        {
            get => _halfHeight;
            set
            {
                _halfHeight = value;
                UpdateScale();
                UpdateTransform();
            }
        }

        /// <summary>
        ///     A matrix describing the Cylinder rotation
        /// </summary>
        public Matrix Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                IsXZAligned = _rotation.Equals(Matrix.Identity);
                UpdateInverseRotation();
                UpdateTransform();
            }
        }


        // An internal matrix to transform points from local space to Cylinder space
        public Matrix Transform { get; private set; }

        /// <summary>
        ///     True if this Cylinder has no rotation, and its circular shape is aligned to the XZ plane
        /// </summary>
        public bool IsXZAligned { get; private set; }



        /// <summary>
        ///     Creates a Bounding Cylinder with a center, radius and half-length. Note that it is XZ aligned.
        /// </summary>
        /// <param name="center">The center of the cylinder.</param>
        /// <param name="radius">The horizontal radius of the cylinder.</param>
        /// <param name="halfLength">Half the height of the cylinder.</param>
        public BoundingCylinder(Vector3 center, float radius, float halfLength)
        {
            _center = center;
            _radius = radius;
            _halfHeight = halfLength;
            _rotation = Matrix.Identity;
            IsXZAligned = true;

            UpdateTranslation();
            UpdateScale();
            UpdateInverseRotation();
            UpdateTransform();
        }

        /// <summary>
        ///     Moves the Cylinder center by a delta offset, and updates the internal values of the Cylinder.
        /// </summary>
        /// <param name="delta">The amount of translation on each axis</param>
        public void Move(Vector3 delta)
        {
            _center += delta;

            UpdateTranslation();
            UpdateInverseRotation();
            UpdateTransform();
        }

        /// <summary>
        ///     Rotates the Cylinder using a rotation matrix. Considers the previous rotation and applies the new one.
        ///     Then it updates the internal values of the Cylinder.
        /// </summary>
        /// <param name="rotation">The rotation matrix to apply to the Cylinder</param>
        public void Rotate(Matrix rotation)
        {
            _rotation *= rotation;
            IsXZAligned = _rotation.Equals(Matrix.Identity);
            UpdateInverseRotation();
            UpdateTransform();
        }

        /// <summary> 
        ///     Rotates the Cylinder using a quaternion. Considers the previous rotation and applies the new one.
        ///     Then it updates the internal values of the Cylinder.
        /// </summary>
        /// <param name="rotation">The rotation quaternion to apply to the Cylinder</param>
        public void Rotate(Quaternion rotation)
        {
            Rotate(Matrix.CreateFromQuaternion(rotation));
        }


        /// <summary>
        ///   Check if this <see cref="BoundingCylinder"/> intersects a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The <see cref="Ray"/> to test for intersection.</param>
        /// <returns>
        ///   <code>true</code> if this <see cref="BoundingCylinder"/> intersects <paramref name="ray"/>,
        ///   <code>false</code> if it does not.
        /// </returns>
        public bool Intersects(Ray ray)
        {
            var origin = Vector3.Transform(ray.Position, _inverseTransform);
            var direction = Vector3.TransformNormal(ray.Direction, _inverseTransform);

            var x0 = origin.X;
            var xt = direction.X;
            var y0 = origin.Y;
            var yt = direction.Y;
            var z0 = origin.Z;
            var zt = direction.Z;

            // Note: This solution is based on these equations
            // x^2 + z^2 = 1, -1 <= y <= 1 | For the cylinder
            // (x, y, z) = (x0, y0, z0) + t * (xt, yt, zt) | For the ray

            float t1, t2;

            if (yt == 0)
            {
                if (y0 > 1) return false;
                if (y0 < -1) return false;
                t1 = float.MinValue;
                t2 = float.MaxValue;
            }
            else
            {
                t1 = (-1 - y0) / yt;
                t2 = (1 - y0) / yt;
            }

            float a = xt * xt + zt * zt,
                b = 2 * x0 * xt + 2 * z0 * zt,
                c = x0 * x0 + z0 * z0 - 1;

            var root = b * b - 4 * a * c;

            if (root < 0) return false;
            if (root == 0)
            {
                var t = -b / (2 * a);
                return t >= t1 && t <= t2;
            }
            var up = -b;
            var down = 2 * a;
            var sqrt = MathF.Sqrt(root);

            float t3, t4;
            t3 = (up - sqrt) / down;
            t4 = (up + sqrt) / down;

            if (t3 <= t1 && t4 >= t2) return true;
            if (t3 >= t1 && t3 <= t2) return true;
            if (t4 >= t1 && t4 <= t2) return true;
            return false;
        }

        /// <summary>
        ///   Check if this <see cref="BoundingCylinder"/> contains a point.
        /// </summary>
        /// <param name="point">The <see cref="Vector3"/> to test.</param>
        /// <returns>
        ///   <see cref="ContainmentType.Contains"/> if this <see cref="BoundingCylinder"/> contains
        ///   <paramref name="point"/> or <see cref="ContainmentType.Disjoint"/> if it does not.
        /// </returns>
        public ContainmentType Contains(Vector3 point)
        {
            var transformedPoint = Vector3.Transform(point, _inverseRotation);

            if (MathF.Abs(_center.Y - transformedPoint.Y) > _halfHeight) 
                return ContainmentType.Disjoint;

            var centerToPoint = transformedPoint - _center;

            var squaredPointX = centerToPoint.X * centerToPoint.X;
            var squaredPointZ = centerToPoint.Z * centerToPoint.Z;
            var squaredRadius = _radius * _radius;

            return ((squaredPointX + squaredPointZ) <= squaredRadius) ? ContainmentType.Contains : ContainmentType.Disjoint;
        }


        /// <summary>
        ///     Gets the closest point in a cylinder from a point.
        /// </summary>
        /// <param name="point">The point from which to find the closest position.</param>
        /// <returns>A position in the cylinder that is the closest to <paramref name="point"/></returns>
        private Vector3 ClosestPoint(Vector3 point)
        {
            // Transform the point to cylindrical UVW coordinates
            var uvwPoint = Vector3.Transform(point, _inverseRotation);

            // Find the closest point in UVW coordinates

            var direction = uvwPoint - _center;
            direction.Y = 0;
            if (direction.LengthSquared() > (_radius * _radius))
            {
                direction.Normalize();
                direction *= _radius;
            }

            var distanceY = uvwPoint.Y - _center.Y;
            if (MathF.Abs(distanceY) > _halfHeight)
                return _center + new Vector3(0, _halfHeight, 0) * Math.Sign(distanceY) + direction;

            var uvwResult = _center + new Vector3(0, distanceY, 0) + direction;




            // Transform that result back to world coordinates
            var translatedRotation = Matrix.Invert(_inverseRotation);
            return Vector3.Transform(uvwResult, translatedRotation);
        }



        /// <summary>
        ///   Check if this <see cref="BoundingCylinder"/> intersects a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to test for intersection.</param>
        /// <returns>
        ///   <code>true</code> if this <see cref="BoundingCylinder"/> intersects <paramref name="sphere"/>,
        ///   <code>false</code> if it does not.
        /// </returns>
        public bool Intersects(BoundingSphere sphere)
        {
            // Transform the sphere center to cylindrical UVW coordinates
            var uvwSphereCenter = Vector3.Transform(sphere.Center, _inverseRotation);
          
            // We check if there is intersection in UVW space

            var sphereRadius = sphere.Radius;
            var distanceY = MathF.Abs(uvwSphereCenter.Y - _center.Y);

            // If the sphere is way too high or too low there is no intersection
            if (distanceY > _halfHeight + sphereRadius)
                return false;

            var centerToCenter = uvwSphereCenter - _center;
            centerToCenter.Y = 0;

            var addedRadius = _radius + sphereRadius;

            // If the sphere is too far in the XZ plane there is no intersection
            if (centerToCenter.LengthSquared() > (addedRadius * addedRadius)) 
                return false;

            // If the sphere's center is inside the Y coordinates of the cylinder, there is an intersection
            if (distanceY < _halfHeight) 
                return true;

            // Check if the closest point to the center of the sphere belongs to the cylinder
            centerToCenter.Normalize();
            centerToCenter *= _radius;
            centerToCenter.Y = _halfHeight * MathF.Sign(uvwSphereCenter.Y - _center.Y);
            centerToCenter += _center;

            return (centerToCenter - uvwSphereCenter).LengthSquared() <= (sphereRadius * sphereRadius);
        }


        /// <summary>
        ///   Check if this <see cref="BoundingCylinder"/> intersects a Line Segment.
        /// </summary>
        /// <param name="pointA">The start point of the Line Segment to test for intersection.</param>
        /// <param name="pointB">The end point of the Line Segment to test for intersection.</param>
        /// <returns>
        ///   <code>true</code> if this <see cref="BoundingCylinder"/> intersects with the Line Segment,
        ///   <code>false</code> if it does not.
        /// </returns>
        public Vector3? Intersects(Vector3 pointA, Vector3 pointB)
        {
            var halfHeight = Vector3.TransformNormal(Vector3.Up, Transform);
            var cylinderInit = _center - halfHeight;
            var cylinderEnd = _center + halfHeight;

            var t = -1f;
            var q = Vector3.Zero;

            Vector3 d = cylinderEnd - cylinderInit, m = pointA - cylinderInit, n = pointB - pointA;
            var md = Vector3.Dot(m, d);
            var nd = Vector3.Dot(n, d);
            var dd = Vector3.Dot(d, d);

            // Test if segment fully outside either endcap of cylinder
            if (md < 0.0f && md + nd < 0.0f) return null; // Segment outside ’p’ side of cylinder
            if (md > dd && md + nd > dd) return null; // Segment outside ’q’ side of cylinder
            var nn = Vector3.Dot(n, n);
            var mn = Vector3.Dot(m, n);
            var a = dd * nn - nd * nd;
            var k = Vector3.Dot(m, m) - _radius * _radius;
            var c = dd * k - md * md;
            if (MathF.Abs(a) < float.Epsilon)
            {
                // Segment runs parallel to cylinder axis
                if (c > 0.0f) 
                    return null; // 'a' and thus the segment lie outside cylinder
                // Now known that segment intersects cylinder; figure out how it intersects
                if (md < 0.0f)
                    t = -mn / nn; // Intersect segment against 'p' endcap
                else if (md > dd) 
                    t = (nd - mn) / nn; // Intersect segment against ’q’ endcap
                else 
                    t = 0.0f; // 'a' lies inside cylinder
                q = pointA + t * n;
                return q;
            }
            var b = dd * mn - nd * md;
            var discr = b * b - a * c;
            if (discr < 0.0f) 
                return null; // No real roots; no intersection
            t = (-b - MathF.Sqrt(discr)) / a;
            if (t < 0.0f || t > 1.0f) 
                return null; // Intersection lies outside segment

            if (md + t * nd < 0.0f)
            {
                // Intersection outside cylinder on 'p' side
                if (nd <= 0.0f) 
                    return null; // Segment pointing away from endcap
                t = -md / nd;
                // Keep intersection if Dot(S(t) - p, S(t) - p) <= r^2
                if (k + t * (2.0f * mn + t * nn) <= 0.0f)
                    return q;
                else
                    return null;
            }
            if (md + t * nd > dd)
            {
                // Intersection outside cylinder on 'q' side
                if (nd >= 0.0f) 
                    return null; // Segment pointing away from endcap
                t = (dd - md) / nd;
                // Keep intersection if Dot(S(t) - q, S(t) - q) <= r^2
                if (k + dd - 2.0f * md + t * (2.0f * (mn - nd) + t * nn) <= 0.0f)
                    return q;
                else
                    return null;
            }

            // Segment intersects cylinder between the endcaps; t is correct
            q = pointA + t * n;
            return q;
        }



        /// <summary>
        ///     Check if this <see cref="BoundingCylinder"/> intersects a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test for intersection.</param>
        /// <returns> A <see cref="BoxCylinderIntersection"/> indicating if the bounding volumes intersect with each other.</returns>
        public BoxCylinderIntersection Intersects(BoundingBox box)
        {
            if (IsXZAligned)
                return IntersectsXZAligned(box);
            else
                // TODO: Implement the method from
                // https://github.com/teikitu/teikitu_release/blob/master/teikitu/src/TgS%20COLLISION/TgS%20Collision%20-%20F%20-%20Cylinder-Box.c_inc
                throw new NotImplementedException();
        }

        /// <summary>
        ///     Check if this <see cref="BoundingCylinder"/>, assumed XZ aligned, intersects a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The box to test for intersection.</param>
        /// <returns> A <see cref="BoxCylinderIntersection"/> indicating if the bounding volumes intersect with each other.</returns>
        private BoxCylinderIntersection IntersectsXZAligned(BoundingBox box)
        {
            // Calculate the closest point
            var closestPoint = BoundingVolumesExtensions.ClosestPoint(box, _center);

            // Is closest point the same as the center?
            // This means that the center is inside the box
            if (closestPoint.Equals(_center))
                return BoxCylinderIntersection.Intersecting;

            // Distance in Y, is it greater, less, or in the center?
            var differenceInY = MathF.Abs(closestPoint.Y - _center.Y);

            // If the absolute of the distance is greater than half the height, we are not intersecting
            if (differenceInY > _halfHeight)
                return BoxCylinderIntersection.None;
            
            var radiusSquared = _radius * _radius;
            var centerDistance = new Vector2(_center.X - closestPoint.X, _center.Z - closestPoint.Z);
            var differenceInRadius = centerDistance.LengthSquared() - radiusSquared;

            // If the distance is equal, this means that we are on the top/bottom faces
            // We are colliding on the top or bottom, so check if we are in the radius. 
            // We are either on the edge or not colliding
            if (differenceInY == _halfHeight)
                return (differenceInRadius <= 0f) ? BoxCylinderIntersection.Edge : BoxCylinderIntersection.None;

            // If we got here, the closest point is not at the top/bottom
            // It depends on our distance to classify the intersection

            if (differenceInRadius == 0f)
                return BoxCylinderIntersection.Edge;
            else if (differenceInRadius < 0f)
                return BoxCylinderIntersection.Intersecting;
            else
                return BoxCylinderIntersection.None;
        }



        /// <summary>
        ///     Updates the Translation matrix and the Opposite Translation matrix as well,
        ///     based on the contents of <see cref="_center"/>.
        /// </summary>
        private void UpdateTranslation()
        {
            _translation = Matrix.CreateTranslation(_center);
            _oppositeTranslation = Matrix.CreateTranslation(-_center);
        }

        /// <summary>
        ///     Updates the Scale matrix based on the contents of <see cref="_radius"/> and <see cref="_halfHeight"/>.
        /// </summary>
        private void UpdateScale()
        {
            _scale = Matrix.CreateScale(_radius, _halfHeight, _radius);
        }

        /// <summary>
        ///     Updates the Inverse Rotation matrix, used in intersection tests, based on the values of <see cref="_rotation"/>, 
        ///     <see cref="_translation"/> and <see cref="_oppositeTranslation"/>.
        /// </summary>
        private void UpdateInverseRotation()
        {
            _inverseRotation =
                    _oppositeTranslation *
                    Matrix.Invert(_rotation) *
                    _translation;
        }

        /// <summary>
        ///     Updates the Transform and Inverse Transform matrices, based on the values of <see cref="_scale"/>, <see cref="_rotation"/> and <see cref="_translation"/>.
        /// </summary>
        private void UpdateTransform()
        {
            Transform = _scale * _rotation * _translation;
            _inverseTransform = Matrix.Invert(Transform);
        }
    }


    /// <summary>
    ///     Describes the type of intersection a <see cref="BoundingCylinder"/> and a <see cref="BoundingBox"/> had.
    /// </summary>
    public enum BoxCylinderIntersection
    {
        ///<summary>The box touches the cylinder at an edge. Penetration is zero.</summary>
        Edge,
        ///<summary>The box touches the cylinder. Penetration is more than zero.</summary>
        Intersecting,
        ///<summary>The box and the cylinder do not intersect.</summary>
        None,
    }

}
