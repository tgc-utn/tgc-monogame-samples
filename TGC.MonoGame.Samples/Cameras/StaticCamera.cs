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
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="fieldOfViewDegrees">Field of view in the y direction, in radians.</param>
        /// <param name="nearPlane">Distance to the near view plane.</param>
        /// <param name="farPlane">Distance to the far view plane.</param>
        /// <param name="position">The position of the camera.</param>
        /// <param name="lookAt">The target towards which the camera is pointing.</param>
        public StaticCamera(float aspectRatio, float fieldOfViewDegrees, float nearPlane, float farPlane,
            Vector3 position, Vector3 lookAt)
        {
            AspectRatio = aspectRatio;
            FieldOfView = fieldOfViewDegrees;
            NearPlane = nearPlane;
            FarPlane = farPlane;
            Position = position;
            LookAtDirection = Vector3.Normalize(position - lookAt);
            RightDirection = Vector3.Normalize(Vector3.Cross(DefaultWorldUpVector, LookAtDirection));
            UpDirection = Vector3.Cross(LookAtDirection, RightDirection);
            ViewMatrix = Matrix.CreateLookAt(Position, LookAtDirection, UpDirection);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
            WorldMatrix = Matrix.Identity;
        }
    }
}