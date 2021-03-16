using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Viewer
{
    /// <summary>
    ///     Gizmo for drawing the Axis Lines of an application.
    /// </summary>
    class AxisLines
    {
        private const float AxisScreenOffset = 20f;
        private const float AxisScreenDistance = 40f;

        private Vector3 NearVector;
        private Vector3 FarVector;

        private GraphicsDevice GraphicsDevice;
        private Model Model;
        private BasicEffect Effect;

        private Matrix CloserProjection;

        private Matrix BaseRotation;

        /// <summary>
        ///     Constructs an AxisLines drawable object.
        /// </summary>
        /// <param name="device">The GraphicsDevice to bind the resources.</param>
        /// <param name="model">The Model of the AxisLines, loaded from content.</param>
        public AxisLines(GraphicsDevice device, Model model)
        {
            GraphicsDevice = device;
            Model = model;

            foreach (var mesh in Model.Meshes)
                foreach (var part in mesh.MeshParts)
                    Effect = (BasicEffect)part.Effect;

            Effect.Projection = Matrix.Identity;
            Effect.View = Matrix.Identity;
            
            var screenPosition = new Vector3(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 1f);
            NearVector = screenPosition;
            NearVector.X -= AxisScreenOffset;
            NearVector.Y -= AxisScreenOffset;

            FarVector = screenPosition;
            FarVector.X -= AxisScreenDistance;
            FarVector.Y -= AxisScreenDistance;

            BaseRotation = Matrix.CreateRotationX(MathHelper.PiOver2);

            Effect.EnableDefaultLighting();
        }

        /// <summary>
        ///     Sets the matrices needed to draw the Axis Lines in the screen.
        /// </summary>
        /// <param name="view">The view matrix, generally from a camera.</param>
        /// <param name="projection">The projection matrix, either from a camera or from the application.</param>
        public void SetMatrices(Matrix view, Matrix projection)
        {
            bool viewChanged = Effect.View != view;
            bool projectionChanged = ProjectionChanged(projection);

            // We use a closer projection, a matrix that has a negative offset
            // in the far plane to prevent far culling our axis
            if (projectionChanged)
            {
                CloserProjection = SubtractFromFarPlane(projection, 100f);
                Effect.Projection = projection;
            }

            if (projectionChanged || viewChanged)
            {
                //Transform the screen space into 3D space
                var worldCoordPos = GraphicsDevice.Viewport.Unproject(FarVector, CloserProjection, view, Matrix.CreateTranslation(0f, 0f, 2f));
                var worldCoordPosEnd = GraphicsDevice.Viewport.Unproject(NearVector, CloserProjection, view, Matrix.CreateTranslation(0f, 0f, 2f));
                var scale = (worldCoordPosEnd - worldCoordPos).Length();
                Effect.World = BaseRotation * Matrix.CreateScale(scale) * Matrix.CreateTranslation(worldCoordPos);
            }
            Effect.View = view;
        }
        
        /// <summary>
        ///     Fast method to check if projection matrices match.
        /// </summary>
        /// <param name="newProjection">The projection matrix of this tick.</param>
        /// <returns>true if the projection matrix changed, false otherwise.</returns>
        private bool ProjectionChanged(Matrix newProjection)
        {
            var projection = Effect.Projection;
            return newProjection.M43 != projection.M43 ||
                newProjection.M11 != projection.M11 ||
                newProjection.M22 != projection.M22;
        }

        /// <summary>
        ///     Subtracts a value from the far plane of a projection matrix.
        /// </summary>
        /// <param name="projection">The projection matrix to subtract the far plane distance value.</param>
        /// <param name="subtraction">The distance in units to subtract from the far plane.</param>
        /// <returns>The new projection matrix with the value subtracted.</returns>
        private Matrix SubtractFromFarPlane(Matrix projection, float subtraction)
        {
            float near = projection.M43 / (projection.M33 - 1f);
            float far = projection.M43 / (projection.M33 + 1f);
            far -= subtraction;

            float difference = far - near;
            projection.M33 = -(far + near) / difference;
            projection.M43 = -2f * far * near / difference;

            return projection;
        }

        /// <summary>
        ///     Draws the AxisLines
        /// </summary>
        public virtual void Draw()
        {
            foreach (var mesh in Model.Meshes)
                mesh.Draw();
        }
    }
}
