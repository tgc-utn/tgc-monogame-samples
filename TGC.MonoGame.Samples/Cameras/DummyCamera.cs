using Microsoft.Xna.Framework;

namespace TGC.MonoGame.Samples.Cameras
{
    /// <summary>
    /// Static camera looking at a particular point.
    /// Frustum 
    /// </summary>
    public class DummyCamera : Camera
    {
        /// <summary>
        /// The direction that is "up" from the camera's point of view.
        /// </summary>
        public readonly Vector3 DefaultWorldUpVector = Vector3.Up;

        public new const float DefaultNearPlaneDistance = 50f;
        public new const float DefaultFarPlaneDistance = 300f;

        /// <summary>
        /// Static camera looking at a particular direction, which has the up vector (0,1,0).
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="position">The position of the camera.</param>
        /// <param name="lookAt">The target towards which the camera is pointing.</param>
        public DummyCamera(float aspectRatio, Vector3 position, Vector3 lookAt) : base(aspectRatio,DefaultFieldOfViewDegrees,DefaultNearPlaneDistance,DefaultFarPlaneDistance)
        {
            Position = position;
            FrontDirection = Vector3.Normalize(position - lookAt);
            RightDirection = Vector3.Normalize(Vector3.Cross(DefaultWorldUpVector, FrontDirection));
            UpDirection = Vector3.Cross(FrontDirection, RightDirection);
            View = Matrix.CreateLookAt(Position, FrontDirection, UpDirection);
        }

        ///<inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            // This camera has no movement, once initialized with position and lookAt it is no longer updated.
        }
    }
}