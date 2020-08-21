using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Cameras
{
    /// <summary>
    /// Static camera looking at a particular point.
    /// </summary>
    public class StaticCamera : Camera
    {
        /// <summary>
        /// The direction that is "up" from the camera's point of view.
        /// </summary>
        public readonly Vector3 DefaultWorldUpVector = Vector3.Up;

        /// <summary>
        /// Static camera looking at a particular direction, which has the up vector (0,1,0).
        /// </summary>
        /// <param name="position">The position of the camera.</param>
        /// <param name="lookAt">The target towards which the camera is pointing.</param>
        public StaticCamera(Vector3 position, Vector3 lookAt)
        {
            Position = position;
            FrontDirection = Vector3.Normalize(position - lookAt);
            RightDirection = Vector3.Normalize(Vector3.Cross(DefaultWorldUpVector, FrontDirection));
            UpDirection = Vector3.Cross(FrontDirection, RightDirection);
            ViewMatrix = Matrix.CreateLookAt(Position, FrontDirection, UpDirection);
        }
    }
}