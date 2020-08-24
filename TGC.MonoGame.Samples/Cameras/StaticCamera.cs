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

        public Vector3 LookAtPosition { get; private set; }

        public void SetPosition(Vector3 position)
        {
            Position = position;
            Update();
        }


        /// <summary>
        /// Static camera looking at a particular direction, which has the up vector (0,1,0).
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="position">The position of the camera.</param>
        /// <param name="lookAt">The target towards which the camera is pointing.</param>
        public StaticCamera(float aspectRatio, Vector3 position, Vector3 lookAt) : base(aspectRatio)
        {
            LookAtPosition = lookAt;
            Position = position;
            Update();
        }

        private void Update()
        {
            FrontDirection = Vector3.Normalize(LookAtPosition - Position);
            RightDirection = Vector3.Normalize(Vector3.Cross(DefaultWorldUpVector, FrontDirection));
            UpDirection = Vector3.Cross(FrontDirection, RightDirection);
            View = Matrix.CreateLookAt(Position, Position + FrontDirection, UpDirection);
        }

        ///<inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            // This camera has no movement, once initialized with position and lookAt it is no longer updated.
        }
    }
}