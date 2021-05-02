using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.Samples.Viewer
{
    /// <summary>
    ///     Gizmos drawer
    /// </summary>
    public class Gizmos
    {
        private GizmosRenderer GizmosRenderer;
        private NoGizmosRenderer NoGizmosRenderer;

        private IGizmosRenderer Renderer;

        private bool enabled;

        /// <summary>
        ///     Creates a GizmosRenderer.
        /// </summary>
        public Gizmos()
        {
            GizmosRenderer = new GizmosRenderer();
            NoGizmosRenderer = new NoGizmosRenderer();

            Enabled = true;
        }

        /// <summary>
        ///     Loads all the content necessary for drawing Gizmos.
        /// </summary>
        /// <param name="device">The GraphicsDevice to use when drawing. It is also used to bind buffers.</param>
        /// <param name="content">The ContentManager to manage Gizmos resources.</param>
        public void LoadContent(GraphicsDevice device, ContentManager content)
        {
            GizmosRenderer.LoadContent(device, content);
        }

        /// <summary>
        ///     Enables or disables Gizmos drawing. This should be used before a frame, not during it.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (enabled)
                    Renderer = GizmosRenderer;
                else
                    Renderer = NoGizmosRenderer;
            }
        }


        /// <summary>
        ///     Sets the Gizmos color. All Gizmos drawn after are going to use this color if they do not specify one.
        /// </summary>
        /// <param name="color">The Gizmos color to set.</param>
        public void SetBaseColor(Color color)
        {
            Renderer.SetColor(color);
        }

        /// <summary>
        ///     Updates the View and Projection matrices. Should be called whenever the camera is updated.
        /// </summary>
        /// <param name="camera">The View matrix of a camera.</param>
        /// <param name="projection">The Projection matrix of a camera or a viewport.</param>
        public void UpdateViewProjection(Matrix view, Matrix projection)
        {
            Renderer.UpdateMatrices(view, projection);
        }

        /// <summary>
        ///     Draws a line between the points origin and destination using the Gizmos color.
        /// </summary>
        /// <param name="origin">The origin of the line.</param>
        /// <param name="destination">The final point of the line.</param>
        public void DrawLine(Vector3 origin, Vector3 direction)
        {
            Renderer.DrawLine(origin, direction);
        }

        /// <summary>
        ///     Draws a line between the points origin and destination using the specified color.
        /// </summary>
        /// <param name="origin">The origin of the line.</param>
        /// <param name="destination">The final point of the line.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawLine(Vector3 origin, Vector3 direction, Color color)
        {
            Renderer.DrawLine(origin, direction, color);
        }


        /// <summary>
        ///     Draws a wire cube with an origin and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the cube.</param>
        /// <param name="size">The size of the cube.</param>
        public void DrawCube(Vector3 origin, Vector3 size)
        {
            Renderer.DrawCube(origin, size);
        }


        /// <summary>
        ///     Draws a wire cube with a World matrix using the Gizmos color.
        /// </summary>
        /// <param name="world">The World matrix of the cube.</param>
        public void DrawCube(Matrix world)
        {
            Renderer.DrawCube(world);
        }

        /// <summary>
        ///     Draws a wire cube with a World matrix using the specified color.
        /// </summary>
        /// <param name="world">The World matrix of the cube.</param>
        /// <param name="color">The color of the cube.</param>
        public void DrawCube(Matrix world, Color color)
        {
            Renderer.DrawCube(world, color);
        }


        /// <summary>
        ///     Draws a wire cube with an origin and size using the specified color.
        /// </summary>
        /// <param name="origin">The position of the cube.</param>
        /// <param name="size">The size of the cube.</param>
        /// <param name="color">The color of the cube.</param>
        public void DrawCube(Vector3 origin, Vector3 size, Color color)
        {
            Renderer.DrawCube(origin, size, color);
        }

        /// <summary>
        ///     Draws a wire sphere with an origin and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the sphere.</param>
        /// <param name="size">The size of the sphere.</param>
        public void DrawSphere(Vector3 origin, Vector3 size)
        {
            Renderer.DrawSphere(origin, size);
        }

        /// <summary>
        ///     Draws a wire sphere with an origin and size using the specified color.
        /// </summary>
        /// <param name="origin">The position of the sphere.</param>
        /// <param name="size">The size of the sphere.</param>
        /// <param name="color">The color of the sphere.</param>
        public void DrawSphere(Vector3 origin, Vector3 size, Color color)
        {
            Renderer.DrawSphere(origin, size, color);
        }

        /// <summary>
        ///     Draws a contiguous line joining the given points and using the Gizmos color.
        /// </summary>
        /// <param name="points">The positions of the poly-line points in world space.</param>
        public void DrawPolyLine(Vector3[] points)
        {
            Renderer.DrawPolyLine(points);
        }

        /// <summary>
        ///     Draws a contiguous line joining the given points and using the specified color.
        /// </summary>
        /// <param name="points">The positions of the poly-line points in world space.</param>
        /// <param name="color">The color of the poly-line.</param>
        public void DrawPolyLine(Vector3[] points, Color color)
        {
            Renderer.DrawPolyLine(points, color);
        }

        /// <summary>
        ///     Draws a wire frustum -ViewProjection matrix- using the Gizmos color.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix of a virtual camera to draw its frustum.</param>
        public void DrawFrustum(Matrix viewProjection)
        {
            Renderer.DrawFrustum(viewProjection);
        }

        /// <summary>
        ///     Draws a wire frustum -ViewProjection matrix- using the specified color.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix of a virtual camera to draw its frustum.</param>
        /// <param name="color">The color of the frustum.</param>
        public void DrawFrustum(Matrix viewProjection, Color color)
        {
            Renderer.DrawFrustum(viewProjection, color);
        }

        /// <summary>
        ///     Draws a wire frustum -ViewProjection matrix- using the specified color.
        /// </summary>
        /// <param name="viewProjection">The ViewProjection matrix of a virtual camera to draw its frustum.</param>
        /// <param name="color">The color of the frustum.</param>
        public void DrawView(Matrix viewProjection, Color color)
        {
            Renderer.DrawView(viewProjection, color);
        }

        /// <summary>
        ///     Draws a wire circle with an origin and normal direction using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the disk.</param>
        /// <param name="normal">The normal direction of the disk, assumed normalized. It will face this vector.</param>
        /// <param name="radius">The radius of the disk in units.</param>
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius)
        {
            Renderer.DrawDisk(origin, normal, radius);
        }

        /// <summary>
        ///     Draws a wire disk (a circle) with an origin and normal direction using the specified color.
        /// </summary>
        /// <param name="origin">The position of the disk.</param>
        /// <param name="normal">The normal direction of the disk, assumed normalized. It will face this vector.</param>
        /// <param name="radius">The radius of the disk in units.</param>
        /// <param name="color">The color of the disk.</param>
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius, Color color)
        {
            Renderer.DrawDisk(origin, normal, radius, color);
        }

        /// <summary>
        ///     Draws a wire cylinder with an origin, rotation and size using the Gizmos color.
        /// </summary>
        /// <param name="origin">The position of the cylinder.</param>
        /// <param name="rotation">A rotation matrix to set the orientation of the cylinder. The cylinder is by default XZ aligned.</param>
        /// <param name="size">The size of the cylinder.</param>
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size)
        {
            Renderer.DrawCylinder(origin, rotation, size);
        }

        /// <summary>
        ///     Draws a wire cylinder with an origin, rotation and size using the specified color.
        /// </summary>
        /// <param name="origin">The position of the cylinder.</param>
        /// <param name="rotation">A rotation matrix to set the orientation of the cylinder. The cylinder is by default XZ aligned.</param>
        /// <param name="size">The size of the cylinder.</param>
        /// <param name="color">The color of the cylinder.</param>
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size, Color color)
        {
            Renderer.DrawCylinder(origin, rotation, size, color);
        }

        /// <summary>
        ///     Draws a wire cylinder with a World matrix using the Gizmos color.
        /// </summary>
        /// <param name="world">The World matrix of the cylinder.</param>
        /// <param name="color">The color of the cylinder.</param>
        public void DrawCylinder(Matrix world)
        {
            Renderer.DrawCylinder(world);
        }

        /// <summary>
        ///     Draws a wire cylinder with a World matrix using the specified color.
        /// </summary>
        /// <param name="world">The World matrix of the cylinder.</param>
        /// <param name="color">The color of the cylinder.</param>
        public void DrawCylinder(Matrix world, Color color)
        {
            Renderer.DrawCylinder(world, color);
        }


        /// <summary>
        ///     [WARNING] This method should not be used in any sample, and must be called at the end of a frame.
        /// </summary>
        /// <remarks>
        ///      Effectively draws the geometry using the parameters from past draw calls. Should be used after calling the other draw methods.
        /// </remarks>
        internal void Draw()
        {
            Renderer.Draw();
        }


        /// <summary>
        ///    Disposes the used resources (geometries and content).
        /// </summary>
        public void Dispose()
        {
            GizmosRenderer.Dispose();
        }

    }
}
