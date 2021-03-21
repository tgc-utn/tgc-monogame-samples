using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer
{
    /// <summary>
    ///     An object that draws Gizmos
    /// </summary>
    interface IGizmosRenderer
    {

        /// <summary>
        ///     Updates the View and Projection matrices. Should be called whenever the camera is updated.
        /// </summary>
        /// <param name="camera">The View matrix of a camera.</param>
        /// <param name="projection">The Projection matrix of a camera or a viewport.</param>
        public void UpdateMatrices(Matrix view, Matrix projection);

        /// <summary>
        ///     Sets the Gizmos color. All Gizmos drawn after are going to use this color if they do not specify one.
        /// </summary>
        /// <param name="color">The Gizmos color to set.</param>
        public void SetColor(Color color);
        
        /// <summary>
        ///     Draws a line between the points origin and destination using the Gizmos color.
        /// </summary>
        /// <param name="origin">The origin of the line.</param>
        /// <param name="destination">The final point of the line.</param>
        public void DrawLine(Vector3 origin, Vector3 direction);

        /// <summary>
        ///     Draws a line between the points origin and destination using the specified color.
        /// </summary>
        /// <param name="origin">The origin of the line.</param>
        /// <param name="destination">The final point of the line.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawLine(Vector3 origin, Vector3 destination, Color color);

        /// <summary>
        ///     Draws a wire cube with an origin and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the cube.</param>
        /// <param name="size">The size of the cube.</param>
        public void DrawCube(Vector3 origin, Vector3 size);

        /// <summary>
        ///     Draws a wire cube with an origin and size using the specified color.
        /// </summary>
        /// <param name="origin">The position of the cube.</param>
        /// <param name="size">The size of the cube.</param>
        /// <param name="color">The color of the cube.</param>
        public void DrawCube(Vector3 origin, Vector3 size, Color color);

        /// <summary>
        ///     Draws a wire sphere with an origin and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the sphere.</param>
        /// <param name="size">The size of the sphere.</param>
        public void DrawSphere(Vector3 origin, Vector3 size);

        /// <summary>
        ///     Draws a wire sphere with an origin and size using the specified color.
        /// </summary>
        /// <param name="origin">The position of the sphere.</param>
        /// <param name="size">The size of the sphere.</param>
        /// <param name="color">The color of the sphere.</param>
        public void DrawSphere(Vector3 origin, Vector3 size, Color color);

        /// <summary>
        ///     Draws a contiguous line joining the given points and using the Gizmos color.
        /// </summary>
        /// <param name="points">The positions of the poly-line points in world space.</param>
        public void DrawPolyLine(Vector3[] points);

        /// <summary>
        ///     Draws a contiguous line joining the given points and using the specified color.
        /// </summary>
        /// <param name="points">The positions of the poly-line points in world space.</param>
        /// <param name="color">The color of the poly-line.</param>
        public void DrawPolyLine(Vector3[] points, Color color);

        /// <summary>
        ///     Draws a wire frustum -ViewProjection matrix- using the Gizmos color.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix of a virtual camera to draw its frustum.</param>
        public void DrawFrustum(Matrix viewProjection);

        /// <summary>
        ///     Draws a wire frustum -ViewProjection matrix- using the specified color.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix of a virtual camera to draw its frustum.</param>
        /// <param name="color">The color of the frustum.</param>
        public void DrawFrustum(Matrix viewProjection, Color color);

        /// <summary>
        ///     Draws a wire circle with an origin and normal direction using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the disk.</param>
        /// <param name="normal">The normal direction of the disk, assumed normalized. It will face this vector.</param>
        /// <param name="radius">The radius of the disk in units.</param>
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius);

        /// <summary>
        ///     Draws a wire disk (a circle) with an origin and normal direction using the specified color.
        /// </summary>
        /// <param name="origin">The position of the disk.</param>
        /// <param name="normal">The normal direction of the disk, assumed normalized. It will face this vector.</param>
        /// <param name="radius">The radius of the disk in units.</param>
        /// <param name="color">The color of the disk.</param>
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius, Color color);


        /// <summary>
        ///     Draws a wire cylinder with an origin, rotation and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the cylinder.</param>
        /// <param name="rotation">A rotation matrix to set the orientation of the cylinder. The cylinder is by default XZ aligned.</param>
        /// <param name="size">The size of the cylinder.</param>
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size);

        /// <summary>
        ///     Draws a wire cylinder with an origin, rotation and size using the specified color.
        /// </summary>
        /// <param name="origin">The position of the cylinder.</param>
        /// <param name="rotation">A rotation matrix to set the orientation of the cylinder. The cylinder is by default XZ aligned.</param>
        /// <param name="size">The size of the cylinder.</param>
        /// <param name="color">The color of the cylinder.</param>
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size, Color color);

        /// <summary>
        ///     Effectively draws the geometry using the parameters from past draw calls. Should be used after calling the other draw methods.
        /// </summary>
        internal void Draw();
    }
}
