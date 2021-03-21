using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer
{
    /// <summary>
    ///     An object that does not draw Gizmos.
    /// </summary>
    class NoGizmosRenderer : IGizmosRenderer
    {
        /// <inheritdoc />
        public void UpdateMatrices(Matrix view, Matrix projection) { }

        /// <inheritdoc />
        public void SetColor(Color color) { }

        /// <inheritdoc />
        public void DrawCube(Vector3 origin, Vector3 size) { }

        /// <inheritdoc />
        public void DrawCube(Vector3 origin, Vector3 size, Color color) { }

        /// <inheritdoc />
        public void DrawFrustum(Matrix viewProjection) { }

        /// <inheritdoc />
        public void DrawFrustum(Matrix viewProjection, Color color) { }

        /// <inheritdoc />
        public void DrawLine(Vector3 origin, Vector3 direction) { }

        /// <inheritdoc />
        public void DrawLine(Vector3 origin, Vector3 direction, Color color) { }

        /// <inheritdoc />
        public void DrawPolyLine(Vector3[] points) { }

        /// <inheritdoc />
        public void DrawPolyLine(Vector3[] points, Color color) { }

        /// <inheritdoc />
        public void DrawSphere(Vector3 origin, Vector3 size) { }

        /// <inheritdoc />
        public void DrawSphere(Vector3 origin, Vector3 size, Color color) { }

        /// <inheritdoc />
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius) { }

        /// <inheritdoc />
        public void DrawDisk(Vector3 origin, Vector3 normal, float radius, Color color) { }

        /// <inheritdoc />
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size) { }

        /// <inheritdoc />
        public void DrawCylinder(Vector3 origin, Matrix rotation, Vector3 size, Color color) { }

        /// <inheritdoc />
        void IGizmosRenderer.Draw() { }
    }
}
