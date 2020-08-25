using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.Samples.Cameras
{   
    /// <summary>
    /// Camera looking at a particular point.
    /// </summary>
    public class TargetCamera : Camera
    {
        /// <summary>
        /// The direction that is "up" from the camera's point of view.
        /// </summary>
        public readonly Vector3 DefaultWorldUpVector = Vector3.Up;

        public Vector3 TargetPosition { get; set; }

        /// <summary>
        /// Camera looking at a particular direction, which has the up vector (0,1,0).
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="position">The position of the camera.</param>
        /// <param name="targetPosition">The target towards which the camera is pointing.</param>
        public TargetCamera(float aspectRatio, Vector3 position, Vector3 targetPosition) : base(aspectRatio)
        {
            Position = position;
            TargetPosition = targetPosition;
            UpdateView();
        }

        /// <summary>
        /// Update the internal directions and View matrix.
        /// </summary>
        public void UpdateView()
        {
            FrontDirection = Vector3.Normalize(TargetPosition - Position);
            RightDirection = Vector3.Normalize(Vector3.Cross(DefaultWorldUpVector, FrontDirection));
            UpDirection = Vector3.Cross(FrontDirection, RightDirection);
            View = Matrix.CreateLookAt(Position, Position + FrontDirection, UpDirection);
        }

        ///<inheritdoc/>
        public override void Update(GameTime gameTime)
        {

        }
    }
}
