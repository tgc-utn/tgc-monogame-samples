using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using TGC.MonoGame.Samples.Models.Drawers;
using TGC.MonoGame.Samples.Utils;

namespace TGC.MonoGame.Samples.Collisions
{
    /// <summary>
    ///     Class that extends BoundingVolumes classes
    /// </summary>
    public static class BoundingVolumesExtensions
    {
        /// <summary>
        ///     Get an extents vector that contains the half-size on each axis of the box.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate its extents</param>
        /// <returns>The extents on each axis</returns>
        public static Vector3 GetExtents(BoundingBox box)
        {
            var max = box.Max;
            var min = box.Min;

            return (max - min) * 0.5f;            
        }

        /// <summary>
        ///     Gets the total volume of the box in units.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate its volume</param>
        /// <returns>The volume of the box in units</returns>
        public static float GetVolume(BoundingBox box)
        {
            var difference = box.Max - box.Min;
            return difference.X * difference.Y * difference.Z;
        }

        /// <summary>
        ///     Gets the center position of the box.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate its center</param>
        /// <returns>The position of the center of the box.</returns>
        public static Vector3 GetCenter(BoundingBox box)
        {
            return (box.Max + box.Min) * 0.5f;
        }


        /// <summary>
        ///     Scales the box by a given scalar.
        /// </summary>
        /// <param name="scale">The scale for every axis</param>
        /// <param name="box">The <see cref="BoundingBox"/> to scale</param>
        /// <returns>A new box with its extents scaled</returns>
        public static BoundingBox Scale(BoundingBox box, float scale)
        {
            var center = GetCenter(box);
            var extents = GetExtents(box);
            var scaledExtents = extents * scale;

            return new BoundingBox(center - scaledExtents, center + scaledExtents);
        }

        /// <summary>
        ///     Scales the box by a given scalar per axis.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to calculate its scale</param>
        /// <param name="scale">The scale for each axis</param>
        /// <returns>A new <see cref="BoundingBox">BoundingBox</see> with its extents scaled</returns>
        public static BoundingBox Scale(BoundingBox box, Vector3 scale)
        {
            var center = GetCenter(box);
            var extents = GetExtents(box);
            var scaledExtents = extents * scale;

            return new BoundingBox(center - scaledExtents, center + scaledExtents);
        }

        /// <summary>
        ///     Gets the closest point to the box.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate the closest point</param>
        /// <param name="point">The point to find the closest point from</param>
        /// <returns>The position inside the box that is closer to the given point</returns>
        public static Vector3 ClosestPoint(BoundingBox box, Vector3 point)
        {
            var min = box.Min;
            var max = box.Max;
            point.X = MathHelper.Clamp(point.X, min.X, max.X);
            point.Y = MathHelper.Clamp(point.Y, min.Y, max.Y);
            point.Z = MathHelper.Clamp(point.Z, min.Z, max.Z);
            return point;
        }

        /// <summary>
        ///     Gets the normal vector from a point in the box surface.
        /// </summary>
        /// <param name="box">A <see cref="BoundingBox"/> to calculate the normal</param>
        /// <param name="point">The point in the surface of the box</param>
        /// <returns>The normal vector of the surface in which the point is in</returns>
        public static Vector3 GetNormalFromPoint(BoundingBox box, Vector3 point)
        {
            var normal = Vector3.Zero;
            var min = float.MaxValue;

            point -= GetCenter(box);
            var extents = GetExtents(box);

            var distance = MathF.Abs(extents.X - Math.Abs(point.X));
            if (distance < min)
            {
                min = distance;
                normal = Math.Sign(point.X) * Vector3.UnitX;
                // Cardinal axis for X            
            }
            distance = Math.Abs(extents.Y - Math.Abs(point.Y));
            if (distance < min)
            {
                min = distance;
                normal = Math.Sign(point.Y) * Vector3.UnitY;
                // Cardinal axis for Y            
            }
            distance = Math.Abs(extents.Z - Math.Abs(point.Z));
            if (distance < min)
            {
                normal = Math.Sign(point.Z) * Vector3.UnitZ;
                // Cardinal axis for Z            
            }
            return normal;
        }

        /// <summary>
        ///     Creates a <see cref="BoundingBox">BoundingBox</see> from a Matrix.
        /// </summary>
        /// <param name="matrix">The Matrix that describes a transformation to apply to each point of a box</param>
        /// <returns>The <see cref="BoundingBox">BoundingBox</see> created from the Matrix</returns>
        public static BoundingBox FromMatrix(Matrix matrix)
        {
            return new BoundingBox(Vector3.Transform(-Vector3.One * 0.5f, matrix), Vector3.Transform(Vector3.One * 0.5f, matrix));
        }

        /// <summary>
        ///     Creates a <see cref="BoundingBox">BoundingBox</see> from a Model, using all its sub-meshes.
        /// </summary>
        /// <param name="model">The model to create the box</param>
        /// <returns>The <see cref="BoundingBox">BoundingBox</see> that encloses the vertices from the model</returns>
        public static BoundingBox CreateAABBFrom(Model model)
        {
            var minPoint = Vector3.One * float.MaxValue;
            var maxPoint = Vector3.One * float.MinValue;

            var transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            var meshes = model.Meshes;
            for (int index = 0; index < meshes.Count; index++)
            {
                var meshParts = meshes[index].MeshParts;
                for (int subIndex = 0; subIndex < meshParts.Count; subIndex++)
                {
                    var vertexBuffer = meshParts[subIndex].VertexBuffer;
                    var declaration = vertexBuffer.VertexDeclaration;
                    var vertexSize = declaration.VertexStride / sizeof(float);

                    var rawVertexBuffer = new float[vertexBuffer.VertexCount * vertexSize];
                    vertexBuffer.GetData(rawVertexBuffer);

                    for (var vertexIndex = 0; vertexIndex < rawVertexBuffer.Length; vertexIndex += vertexSize)
                    {
                        var transform = transforms[meshes[index].ParentBone.Index];
                        var vertex = new Vector3(rawVertexBuffer[vertexIndex], rawVertexBuffer[vertexIndex + 1], rawVertexBuffer[vertexIndex + 2]);
                        vertex = Vector3.Transform(vertex, transform);
                        minPoint = Vector3.Min(minPoint, vertex);
                        maxPoint = Vector3.Max(maxPoint, vertex);
                    }
                }
            }
            return new BoundingBox(minPoint, maxPoint);
        }



        public static BoundingBox CreateAABBFrom(ModelDrawer modelDrawer)
        {
            if (modelDrawer.SingleWorldMatrix)
                return CreateAABBFrom(modelDrawer.GeometryDrawers.FirstOrDefault(), modelDrawer.World);
            else
                return CreateAABBFrom(modelDrawer.GeometryDrawers, 
                    modelDrawer.GeometryData.Select(data => data.Matrices.FirstOrDefault()).ToList());
        }


        /// <summary>
        ///     Creates a <see cref="BoundingBox">BoundingBox</see> from a GeometryDrawer, using its vertices.
        /// </summary>
        /// <param name="geometries">The geometries from which to create the box</param>
        /// <returns>The <see cref="BoundingBox">BoundingBox</see> that encloses the vertices from the model</returns>
        public static BoundingBox CreateAABBFrom(List<GeometryDrawer> geometries)
        {
            var minPoint = Vector3.One * float.MaxValue;
            var maxPoint = Vector3.One * float.MinValue;

            Vector3 localMinPoint;
            Vector3 localMaxPoint;

            for (var index = 0; index < geometries.Count; index++)
            {
                FindMinMaxPositions(geometries[index].VertexBuffer, out localMinPoint, out localMaxPoint);
                minPoint = Vector3.Min(minPoint, localMinPoint);
                maxPoint = Vector3.Max(maxPoint, localMaxPoint);
            }

            return new BoundingBox(minPoint, maxPoint);
        }

        public static BoundingBox CreateAABBFrom(List<GeometryDrawer> geometries, List<Matrix> matrices)
        {
            var minPoint = Vector3.One * float.MaxValue;
            var maxPoint = Vector3.One * float.MinValue;

            Vector3 localMinPoint;
            Vector3 localMaxPoint;

            for (var index = 0; index < geometries.Count; index++)
            {
                FindMinMaxPositions(geometries[index].VertexBuffer, matrices[index], out localMinPoint, out localMaxPoint);
                minPoint = Vector3.Min(minPoint, localMinPoint);
                maxPoint = Vector3.Max(maxPoint, localMaxPoint);
            }

            return new BoundingBox(minPoint, maxPoint);
        }




        /// <summary>
        ///     Creates a <see cref="BoundingBox">BoundingBox</see> from a GeometryDrawer, using its vertices.
        /// </summary>
        /// <param name="geometry">The geometry from which to create the box</param>
        /// <returns>The <see cref="BoundingBox">BoundingBox</see> that encloses the vertices from the model</returns>
        public static BoundingBox CreateAABBFrom(GeometryDrawer geometry)
        {
            Vector3 min, max;
            FindMinMaxPositions(geometry.VertexBuffer, out min, out max);
            return new BoundingBox(min, max);
        }

        /// <summary>
        ///     Creates a <see cref="BoundingBox">BoundingBox</see> from a GeometryDrawer, using its vertices.
        /// </summary>
        /// <param name="geometry">The geometry from which to create the box</param>
        /// <returns>The <see cref="BoundingBox">BoundingBox</see> that encloses the vertices from the model</returns>
        public static BoundingBox CreateAABBFrom(GeometryDrawer geometry, Matrix matrix)
        {
            Vector3 min, max;
            FindMinMaxPositions(geometry.VertexBuffer, matrix, out min, out max);
            return new BoundingBox(min, max);
        }

        /// <summary>
        ///     Creates a <see cref="BoundingSphere">BoundingSphere</see> from a Model, using all its sub-meshes.
        /// </summary>
        /// <param name="model">The model to create the sphere</param>
        /// <returns>The <see cref="BoundingSphere">BoundingSphere</see> which radius encloses the vertices from the model</returns>
        public static BoundingSphere CreateSphereFrom(Model model)
        {
            var minPoint = Vector3.One * float.MaxValue;
            var maxPoint = Vector3.One * float.MinValue;

            Vector3 localMinPoint;
            Vector3 localMaxPoint;

            var transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            var meshes = model.Meshes;
            for (var index = 0; index < meshes.Count; index++)
            {
                var meshParts = meshes[index].MeshParts;
                for (var subIndex = 0; subIndex < meshParts.Count; subIndex++)
                {
                    var vertexBuffer = meshParts[subIndex].VertexBuffer;
                    var transform = transforms[meshes[index].ParentBone.Index];

                    FindMinMaxPositions(vertexBuffer, transform, out localMinPoint, out localMaxPoint);

                    minPoint = Vector3.Min(localMinPoint, minPoint);
                    maxPoint = Vector3.Min(localMaxPoint, maxPoint);
                }
            }
            return SphereFromMinMax(minPoint, maxPoint);
        }

        /// <summary>
        ///     Creates a <see cref="BoundingSphere">BoundingBox</see> from a GeometryDrawer, using its vertices.
        /// </summary>
        /// <param name="geometry">The geometry from which to create the sphere</param>
        /// <returns>The <see cref="BoundingSphere">BoundingSphere</see> that encloses the vertices from the model</returns>
        public static BoundingSphere CreateSphereFrom(GeometryDrawer geometry)
        {
            Vector3 min, max;
            FindMinMaxPositions(geometry.VertexBuffer, out min, out max);
            return SphereFromMinMax(min, max);
        }

        /// <summary>
        ///     Creates a <see cref="BoundingSphere">BoundingBox</see> from a GeometryDrawer, using its vertices.
        /// </summary>
        /// <param name="geometry">The geometry from which to create the sphere</param>
        /// <param name="world">The world matrix that will be used to transform each point</param>
        /// <returns>The <see cref="BoundingSphere">BoundingSphere</see> that encloses the vertices from the model</returns>
        public static BoundingSphere CreateSphereFrom(GeometryDrawer geometry, Matrix world)
        {
            Vector3 min, max;
            FindMinMaxPositions(geometry.VertexBuffer, world, out min, out max);
            return SphereFromMinMax(min, max);
        }



        public static BoundingSphere CreateSphereFrom(List<GeometryDrawer> geometries, List<Matrix> matrices)
        {
            var minPoint = Vector3.One * float.MaxValue;
            var maxPoint = Vector3.One * float.MinValue;

            Vector3 localMinPoint;
            Vector3 localMaxPoint;

            for (var index = 0; index < geometries.Count; index++)
            {
                FindMinMaxPositions(geometries[index].VertexBuffer, matrices[index], out localMinPoint, out localMaxPoint);
                minPoint = Vector3.Min(minPoint, localMinPoint);
                maxPoint = Vector3.Max(maxPoint, localMaxPoint);
            }

            return SphereFromMinMax(minPoint, maxPoint);
        }


        public static BoundingSphere CreateSphereFrom(ModelDrawer modelDrawer)
        {
            if (modelDrawer.SingleWorldMatrix)
                return CreateSphereFrom(modelDrawer.GeometryDrawers.FirstOrDefault(), modelDrawer.World);
            else
                return CreateSphereFrom(modelDrawer.GeometryDrawers,
                    modelDrawer.GeometryData.Select(data => data.Matrices.FirstOrDefault()).ToList());
        }




        private static BoundingSphere SphereFromMinMax(Vector3 min, Vector3 max)
        {
            var halfDifference = (max - min) * 0.5f;
            var center = (max + min) * 0.5f;
            return new BoundingSphere(center, halfDifference.Length());
        }



        private static void FindMinMaxPositions(VertexBuffer vertexBuffer, out Vector3 minPosition, out Vector3 maxPosition)
        {
            minPosition = Vector3.One * float.MaxValue;
            maxPosition = Vector3.One * float.MinValue;

            var declaration = vertexBuffer.VertexDeclaration;
            var vertexSize = declaration.VertexStride / sizeof(float);

            var rawVertexBuffer = new float[vertexBuffer.VertexCount * vertexSize];
            vertexBuffer.GetData(rawVertexBuffer);

            for (var vertexIndex = 0; vertexIndex < rawVertexBuffer.Length; vertexIndex += vertexSize)
            {
                var vertex = new Vector3(rawVertexBuffer[vertexIndex], rawVertexBuffer[vertexIndex + 1], rawVertexBuffer[vertexIndex + 2]);
                minPosition = Vector3.Min(minPosition, vertex);
                maxPosition = Vector3.Max(maxPosition, vertex);
            }
        }
        private static void FindMinMaxPositions(VertexBuffer vertexBuffer, Matrix world, out Vector3 minPosition, out Vector3 maxPosition)
        {
            minPosition = Vector3.One * float.MaxValue;
            maxPosition = Vector3.One * float.MinValue;

            var declaration = vertexBuffer.VertexDeclaration;
            var vertexSize = declaration.VertexStride / sizeof(float);

            var rawVertexBuffer = new float[vertexBuffer.VertexCount * vertexSize];
            vertexBuffer.GetData(rawVertexBuffer);

            for (var vertexIndex = 0; vertexIndex < rawVertexBuffer.Length; vertexIndex += vertexSize)
            {
                var vertex = new Vector3(rawVertexBuffer[vertexIndex], rawVertexBuffer[vertexIndex + 1], rawVertexBuffer[vertexIndex + 2]);
                vertex = Vector3.Transform(vertex, world);
                minPosition = Vector3.Min(minPosition, vertex);
                maxPosition = Vector3.Max(maxPosition, vertex);
            }

        }
    }
}
